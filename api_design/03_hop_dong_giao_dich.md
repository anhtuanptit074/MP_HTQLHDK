# Hợp đồng giao dịch và cạnh workflow

## 1. Hợp đồng chung

Mọi command thực hiện theo thứ tự: xác thực danh tính → resolve actor trong `app_user` bằng `(issuer, subject)` → đọc permission/grant hiện hành → xác định asset/task scope → bắt đầu transaction → khóa theo thứ tự ổn định → kiểm lại quyền/trạng thái/version → ghi nghiệp vụ + `audit_log` Success → commit → tạo thông báo tối thiểu. Denied/Failure được ghi bằng transaction audit độc lập với cùng correlation ID. Nếu không ghi được audit bắt buộc, thao tác nhạy cảm fail closed.

Thứ tự khóa chuẩn là `asset` → `finding` → `remediation_ticket`/`ticket_event` → `verification_request`/`risk_acceptance` → đối tượng phụ. Không giữ khóa trong lúc upload/stream, parse tệp hoặc gọi object storage/KMS. `finding.row_version` và ETag được so khớp trong câu UPDATE; lịch sử bất biến chỉ INSERT. Không retry tự động lỗi nghiệp vụ hoặc stale version.

## 2. Asset và scope

| Cạnh/command | Actor/permission | Guard chính | Khóa và dữ liệu ghi | Audit |
|---|---|---|---|---|
| `null → Asset Draft` | SECURITY/OWNER; `asset.declare` | Mã/base URL chuẩn hóa hợp lệ; OWNER chỉ theo policy sở hữu | INSERT `asset`; unique code phân xử cạnh tranh | `asset.create` |
| Tạo/sửa Scope Draft | SECURITY/OWNER; `asset.update` | Cùng asset; ít nhất một Include; Exclude ưu tiên; timezone/window hợp lệ | Lock asset → scope; INSERT revision/rule/window hoặc replace children của Draft | `asset.scope.create/update` |
| Gửi Owner xác nhận | SECURITY/OWNER; `asset.update` | Snapshot Draft đầy đủ; không có trạng thái DB “Awaiting Confirmation” | Lock asset → scope; giữ Draft, ghi audit submit; notification sau commit | `asset.scope.submit` |
| `Scope Draft → Pending Approval` | OWNER; `asset.confirm` | Snapshot đầy đủ; actor là Owner hiện hành | Lock asset → scope; set `confirmed_by/at`; với tài sản mới đồng thời `Asset Draft → Pending Approval`; scope đổi của Asset Active chưa thay current scope | `asset.scope.confirm` |
| `Scope Pending Approval → Approved` | SECURITY; `asset.approve` | Owner đã xác nhận; decision reason; revision cùng asset | Lock asset → scope; set decision/current scope; với tài sản mới đồng thời `Asset Pending Approval → Active`; không sửa kết luận sau commit | `asset.scope.decision` |
| `Scope Pending Approval → Rejected` | SECURITY | Có lý do; không đổi current scope cũ của tài sản đang hoạt động | Lock asset → scope; set decision; với tài sản mới đồng thời `Asset Pending Approval → Draft` | `asset.scope.decision` |
| `Active ↔ Suspended` | SECURITY | Lý do; khi suspend phải xử lý lịch Active nhất quán | Lock asset → schedules theo id; update asset và pause lịch bị ảnh hưởng | `asset.transition`, từng `schedule.transition` |
| `Active/Suspended → Retired` | SECURITY | Lý do; không có cạnh hồi sinh | Lock asset → schedules; retire và cancel/pause theo policy đã duyệt | `asset.transition` |

Scope Approved/Rejected là bất biến; thay đổi material luôn tạo revision mới. Nếu audit thất bại, rollback mọi update. Hai quyết định cạnh tranh dùng lock + version; bên thua nhận `CON-002`/`STATE-001`.

## 3. Lịch và lần quét

| Cạnh/command | Actor | Guard | Transaction | Audit |
|---|---|---|---|---|
| `null → Schedule Draft` | SECURITY | Asset/scope cùng nhau; target nằm trong Include và không thuộc Exclude; recurrence/timezone hợp lệ | Lock asset/current scope; INSERT schedule | `schedule.create` |
| `Draft/Paused → Active` | SECURITY | Asset Active, scope là current Approved, có target, Owner time confirmation/policy và `approved_by` | Lock asset/scope/schedule; update status/approval | `schedule.transition` |
| `Active → Paused/Expired/Cancelled`, `Paused → Active/Cancelled` | SECURITY | Cạnh được duyệt, lý do khi cần | Lock asset/schedule; update | `schedule.transition` |
| `null → Scan Planned` | worker hoặc SCAN_OPS/SECURITY | Lịch Active, due key duy nhất, snapshot scope/targets/team | Lock asset/schedule; INSERT occurrence; worker dùng unique key/claim idempotent | `scan_occurrence.create` |
| `Planned → Completed` | SCAN_OPS | ZAP chạy bên ngoài; có started/ended/performed_by; đúng team/scope | Lock asset/scan; update times/status | `scan_occurrence.transition` |
| `Planned → Failed/Missed/Cancelled` | SCAN_OPS/worker theo policy | Lý do bắt buộc; worker không chạy ZAP thay thế | Lock scan; update terminal state | `scan_occurrence.transition` |
| `Completed → Result Imported` | Import worker | Có ImportBatch Completed/Partially Completed đúng scan/asset | Lock scan/batch; update scan cùng finalize batch | `scan_occurrence.transition` |

## 4. Import và chuẩn hóa

Upload có hai pha vì PostgreSQL và object storage không có transaction phân tán:

1. Stream vào object tạm/quarantine, đồng thời áp giới hạn cấu hình, tính SHA-256, dò loại nội dung; không tin filename/content-type.
2. Kiểm quyền, Asset Active, scan Completed và cùng asset. Dedupe `(asset_id, scan_id, file_sha256)` trước khi lộ bất kỳ metadata nào.
3. Finalize object private/immutable bằng KMS; mở transaction ngắn, lock asset/scan, INSERT `import_batch` Received + audit. Nếu unique race, bỏ/đánh dấu object tạm để reconciliation và trả lô cũ sau kiểm quyền.
4. Worker claim Received bằng lock/lease idempotent, chuyển Validating rồi Processing. Sai toàn tệp → Rejected; lỗi kỹ thuật → Failed.
5. Với mỗi record: transaction/savepoint riêng; chuẩn hóa chưa che → HMAC fingerprint có version → create/get Finding → create/get Occurrence → INSERT Observation đã che → update first/last seen → nếu Finding Closed và occurrence mới sau đóng, `Closed → Reopened` + audit. Lỗi record rollback record rồi INSERT `import_error` đã che.
6. Đối soát count bằng query bảng con; finalize Completed/Partially Completed và `scan_occurrence` Result Imported trong transaction. Partial không được dùng để kết luận “không còn fingerprint” nếu phần lỗi có thể làm thiếu coverage.

Raw metadata/source bytes bất biến. Không tạo ticket tự động. False Positive tái xuất hiện tạo review Recheck, không tự sinh cạnh trạng thái ngoài nguồn.

## 5. Review, Final Severity và disposition

| Cạnh/command | Actor | Guard | Transaction | Audit |
|---|---|---|---|---|
| `New → Triaged` qua Severity review | SECURITY | Không phải importer tự xác nhận nguồn của mình; có lý do/căn cứ | Lock finding; INSERT review; nếu đủ điều kiện mark Applied, set current review/final severity/status/due; audit | `finding.review.apply`, `finding.transition` |
| Điều chỉnh Final Severity | SECURITY | Review dựa trên current version; hạ High/Critical cần reviewer độc lập và Approved | Lock finding/review/ticket; apply review, update pointer/severity/SLA due atomically | `finding.review.apply` |
| `New/Triaged → False Positive` | SECURITY | Lý do + evidence; High nguồn/final cần second review độc lập | Lock finding/review; apply immutable review and status | `finding.transition` |
| `New/Triaged → Cancelled` | SECURITY | Chỉ bản ghi sai/ngoài phạm vi, có reason/evidence | Như trên; không xóa source | `finding.transition` |

Source Risk/Confidence trong Observation không bị sửa khi áp Final Severity. Nhiều proposal có thể tồn tại, nhưng chỉ proposal còn phù hợp `row_version/current_review_id` được áp.

## 6. Phiếu và khắc phục

| Cạnh/command | Actor | Guard | Transaction | Audit |
|---|---|---|---|---|
| `Triaged/Reopened → Assigned` | SECURITY | Phiếu phù hợp; Assignment đủ team/assignee/due/SLA đã phê duyệt | Lock finding→ticket/current event; INSERT snapshot Assignment; move pointer; update finding | `ticket.assign`, `finding.transition` |
| `Assigned → In Progress` | REMEDIATOR | Current assignee/team; accept/progress reason | Lock finding→ticket/current event; INSERT Progress snapshot; move pointer/status | `ticket.event.append`, `finding.transition` |
| `In Progress → In Progress` | REMEDIATOR | Current assignment; snapshot đầy đủ | Append Progress; server sets changed_by/time; remediation author only on actual fix | `ticket.event.append` |
| Đổi người/hạn | SECURITY | Căn cứ; copy nguyên remediation/progress/evidence/author/time; deadline dùng SLA thật | Append Assignment hoặc Deadline snapshot; không ghi đè event cũ | `ticket.reassign/deadline` |
| `In Progress → Pending Verification` | REMEDIATOR | Có remediation action, evidence, remediated_by/at; 100% progress không đủ | Lock finding→ticket/current event; append snapshot nếu cần; update finding | `ticket.submit_verification`, `finding.transition` |

`previous_event_id` phải bằng `current_event_id`. Guard chặn con trỏ cũ/lùi; API trả `TKT-003`/`CON-002` thay vì merge ngầm.

## 7. Xác minh, Closed và Reopened

Tạo request: SECURITY khóa Finding/ticket/event, yêu cầu Finding Pending Verification và event là bản sửa hiện hành; INSERT `verification_request` Requested. Scan Verification được tạo/ghi nhận qua API scan, báo cáo qua API import; không có đường tắt ghi parser result.

Kết luận dùng một transaction:

1. Kiểm SECURITY + `verification.decide` đúng asset và `If-Match` Finding.
2. Lock asset → Finding → ticket/event → request → scan/import theo id.
3. Kiểm request thuộc đúng cycle; evaluator khác tất cả người thực sự khắc phục; scan Verification sau `remediated_at`, đúng scope/asset.
4. Completed hoặc Partial đủ coverage; ATTT xác nhận `is_valid`, coverage và fingerprint bằng dữ liệu nhập.
5. INSERT `verification_result` bất biến.
6. Nếu hợp lệ + coverage + fingerprint=false: `Pending Verification → Closed`; nếu invalid hoặc fingerprint=true: `Pending Verification → Reopened`.
7. Set `finding.verification_result_id`, tăng row version, set request Evaluated, INSERT audit có result/evidence, commit.

Không có cạnh “giữ Pending Verification” cho một `verification_result` đã kết luận vì outcome DB chỉ Closed/Reopened. Không dùng result của cycle cũ. Import mới phát hiện fingerprint sau Closed thực hiện `Closed → Reopened`; sau đó SECURITY có thể `Reopened → Assigned`.

## 8. Chấp nhận rủi ro

| Cạnh/command | Actor | Guard | Transaction | Audit |
|---|---|---|---|---|
| `Triaged/Assigned/In Progress → Pending Risk Acceptance` | OWNER/SECURITY | Đủ reason/impact/mitigation/responsible/requested_until; lưu exact previous status | Lock finding; INSERT risk Pending; update finding | `risk.request.create`, `finding.transition` |
| Pending ↔ Needs Information | RISK_APPROVER tạo yêu cầu; requester bổ sung | Chưa terminal; nội dung bổ sung đã che | Lock finding/risk; update mutable fields/status; before/after audit | `risk.request.more_info/supplement` |
| `Pending Risk Acceptance → Risk Accepted` | RISK_APPROVER | Approver khác requester; có authority, reason, conditions, valid period; đúng thẩm quyền | Lock asset→finding→risk; set Approved + applied_at và finding status cùng transaction | `risk.decision`, `finding.transition` |
| `Pending Risk Acceptance → previous status` | RISK_APPROVER | Reject; phục hồi đúng `previous_finding_status` của chính record | Lock finding/risk; set Rejected; update finding | `risk.decision`, `finding.transition` |
| `Risk Accepted → Reopened` | worker/SECURITY | Hết hạn, vi phạm điều kiện hoặc rủi ro tăng; reason/căn cứ | Lock finding/latest applied risk; update + audit + notify | `risk.expire/reopen`, `finding.transition` |
| Gia hạn | OWNER/SECURITY tạo; RISK_APPROVER quyết định | Bản mới `renewal_of_id` trỏ Approved+applied cũ; old record bất biến | INSERT renewal Pending; khi duyệt áp record mới theo same approval transaction | `risk.renewal.*` |

Gia hạn đang chờ không đổi `valid_until` cũ. `v_latest_applied_risk.within_time_window` không thay việc kiểm điều kiện nghiệp vụ.

## 9. File, export, quyền và thông báo

- Cấp/thay quyền nhạy cảm: lock user/asset/grant; revoke dòng cũ + insert grant mới khi đổi; audit Permission cùng transaction. ADMIN không nhận quyền đọc asset/raw mặc định.
- Raw/evidence download: authorize exact asset + cờ riêng + reason, ghi audit trước stream, đọc object private, verify checksum; mỗi lần tải kiểm lại grant. Không URL public/presign dài hạn.
- Export: kiểm quyền trên mọi asset và render file đã che ngoài DB transaction; finalize object private rồi INSERT đầy đủ export + asset list + token hash + audit trong transaction ngắn. Schema hiện tại không có trạng thái job nên API chỉ trả thành công sau khi phát hành hoàn chỉnh; object mồ côi do DB fail được reconciliation xử lý. Recipient/content/asset list bất biến. Mỗi download kiểm recipient, expiry/revocation và mọi quyền asset; revoke chỉ set metadata, không hard delete.
- Notification được tạo sau commit hoặc bởi worker idempotent; chỉ chứa asset code, finding code, status và internal link. Failure notification không rollback nghiệp vụ đã commit, nhưng được retry/quan sát.

## 10. Ma trận kiểm thử giao dịch tối thiểu

Mọi command có bốn lớp test: success; permission/asset/task denial; invalid state/input; concurrency/rollback. Các test P0 bổ sung: duplicate import đồng thời; two terminal decisions; audit writer failure; DB commit sau object finalize thất bại và reconciliation; stale `current_event_id`; requester tự duyệt risk; remediator/evidence author tự Closed; revoke quyền ngay trước stream; Partial import thiếu coverage; Closed fingerprint tái xuất hiện.
