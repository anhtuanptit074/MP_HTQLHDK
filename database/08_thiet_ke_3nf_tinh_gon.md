# THIẾT KẾ CƠ SỞ DỮ LIỆU BCNF TỐI GIẢN

## 1. Kết luận thiết kế

Nguồn yêu cầu duy nhất của mô hình này là `doi_tuong_sd_quy_trinh_hd_tich_hop_zap_tu_dong.md`, nhưng chỉ giữ những nội dung trực tiếp phục vụ:

1. Khai báo tài sản và kiểm soát phạm vi quét.
2. Lập lịch, điều phối OWASP ZAP qua API.
3. Tự động nhận, kiểm tra, lưu và chuẩn hóa kết quả ZAP.
4. Phân loại mức độ và theo dõi khắc phục/xác minh.

OWASP ZAP là công cụ quét bên ngoài duy nhất. Không thiết kế khả năng đa nhà cung cấp.

Mô hình đích có **13 bảng vật lý**. Các báo cáo, thống kê, thông báo, dữ liệu “hiện hành” và dữ liệu tổng hợp được tạo bằng truy vấn/view, không tạo bảng hoặc cột lưu lặp.

## 2. Chức năng giữ lại và lược bỏ

### 2.1. Chức năng bắt buộc giữ

| Nhóm | Nội dung tối thiểu |
|---|---|
| Tài sản | Website, chủ sở hữu, ATTT phụ trách, môi trường, mức quan trọng, trạng thái |
| Phạm vi | Phiên bản phạm vi được duyệt, include/exclude và khung giờ quét |
| Hồ sơ ZAP | Cấu hình có phiên bản, tham chiếu bí mật, giới hạn active scan |
| Kế hoạch quét | Lịch định kỳ hoặc yêu cầu quét xác minh |
| Lần chạy | Trạng thái nghiệp vụ, trạng thái ZAP, mã phiên ngoài, timeout/lỗi |
| Báo cáo nguồn | Tệp JSON/XML gốc, SHA-256, mã hóa và trạng thái xử lý |
| Phát hiện | Dấu vân tay chống trùng, Risk/Confidence nguồn, Final Severity |
| Khắc phục | Người phụ trách, hạn xử lý, tiến độ, biện pháp và bằng chứng tối thiểu |
| Xác minh | Tạo kế hoạch quét xác minh và ghi kết luận trên lần chạy xác minh |
| Bảo mật/truy vết | Kiểm quyền theo quan hệ nghiệp vụ và audit append-only |

### 2.2. Chức năng nâng cao bị loại khỏi mô hình lõi

| Chức năng loại bỏ | Lý do |
|---|---|
| Quản lý team và thành viên team | Là quản lý tổ chức, không phải bốn chức năng chính |
| Danh mục permission/RBAC nhiều tầng | Chỉ giữ `user_role` tối thiểu; trách nhiệm cụ thể vẫn suy ra từ owner, ATTT, operator và assignee |
| Chấp nhận/gia hạn rủi ro | Workflow quản trị rủi ro nâng cao, không bắt buộc để quét và khắc phục |
| Bảng chính sách SLA | Hạn xử lý được nhập trực tiếp trên phiếu; chính sách có thể đặt ở cấu hình ứng dụng |
| Thông báo lưu trong database | Danh sách cần chú ý truy vấn từ trạng thái lỗi, mức độ và hạn xử lý |
| Tệp xuất và link tải có thời hạn | Báo cáo tạo theo yêu cầu, kiểm quyền ở API và ghi audit; không lưu link |
| Nhập một phần báo cáo | Mô hình lõi xử lý nguyên tử: báo cáo sai thì từ chối toàn bộ |
| Nhiều artifact cho một lần chạy | Chỉ một JSON/XML làm nguồn; HTML/PDF tạo khi người dùng yêu cầu |
| Nhiều ZAP connection/vendor | Chỉ có một tích hợp OWASP ZAP theo phạm vi đề tài |
| Reviewer thứ hai tùy biến | Không thuộc luồng tối thiểu của bốn chức năng |
| Phân hệ BI/dashboard | Các số liệu được truy vấn trực tiếp hoặc qua view |

## 3. Nguyên tắc 3NF/BCNF

- Mỗi bảng biểu diễn một thực thể, quan hệ nhiều-nhiều hoặc phiên bản có vòng đời độc lập.
- Mọi thuộc tính không khóa phụ thuộc vào khóa, toàn bộ khóa và không phụ thuộc bắc cầu vào thuộc tính không khóa khác.
- Mỗi phụ thuộc hàm quyết định dữ liệu nghiệp vụ phải có vế trái là siêu khóa; các bảng đạt BCNF theo các khóa ứng viên được liệt kê.
- Không tạo bảng 1:1 nếu hai phía luôn sinh và mất cùng nhau trong phạm vi đề tài.
- Không lưu khóa ngoại có thể đi theo một đường quan hệ duy nhất để truy ra.
- Không lưu bộ đếm, MIN/MAX, “current/latest”, nhãn hiển thị hoặc đường dẫn giao diện.
- Không dùng JSONB/mảng để thay quan hệ nhiều giá trị. Ngoại lệ duy nhất là ảnh audit đã che vì đó là payload sự kiện bất biến, không phải dữ liệu dùng điều khiển nghiệp vụ.
- Dữ liệu lịch sử cần cho an toàn được giữ bằng phiên bản bất biến hoặc audit; không tạo thêm bảng lịch sử chuyên biệt nếu audit đã đáp ứng.

## 4. Danh sách 13 bảng

| STT | Bảng | Vai trò |
|---:|---|---|
| 1 | `app_user` | Người dùng và tài khoản dịch vụ |
| 2 | `user_role` | Vai trò tối thiểu, có thể giới hạn theo tài sản |
| 3 | `asset` | Website/tài sản cần quét |
| 4 | `asset_scope_revision` | Phiên bản phạm vi được xác nhận/phê duyệt |
| 5 | `asset_scope_rule` | Domain/URL include hoặc exclude |
| 6 | `asset_scan_window` | Khung giờ được phép quét |
| 7 | `zap_scan_profile` | Hồ sơ OWASP ZAP có phiên bản |
| 8 | `scan_plan` | Lịch định kỳ hoặc yêu cầu quét xác minh |
| 9 | `scan_run` | Hồ sơ từng lần quét, phiên ZAP và báo cáo nguồn 1:1 |
| 10 | `finding` | Phát hiện chuẩn hóa, mức độ và trạng thái hiện hành |
| 11 | `finding_occurrence` | Phát hiện xuất hiện trong một lần quét |
| 12 | `remediation_ticket` | Phân công và theo dõi khắc phục tối thiểu |
| 13 | `audit_log` | Nhật ký bảo mật và lịch sử thay đổi |

## 5. Định nghĩa quan hệ

### 5.1. `app_user`

Khóa chính: `id`  
Khóa ứng viên: `username`

Thuộc tính:

- `username`, `display_name`
- `account_kind`: `Human`, `Service`
- `password_hash`
- `is_active`
- `created_at`

Ràng buộc:

- `Service` phải có `password_hash IS NULL`.
- Tài khoản `Service` không được đăng nhập giao diện hoặc phê duyệt.
- Không lưu role, team hoặc đơn vị trực tiếp trong `app_user`; vai trò nằm trong `user_role`.

### 5.2. `user_role`

Khóa chính: `id`

Thuộc tính:

- `user_id`
- `role_code`: ADMIN, SECURITY, SCAN_OPERATOR, REMEDIATOR, AUDITOR
- `asset_id` nullable
- `granted_by`, `granted_at`
- `revoked_by`, `revoked_at`

Ràng buộc:

- Một quyền đang hiệu lực là duy nhất theo `(user_id, role_code, asset_id)`.
- ADMIN là quyền toàn hệ thống nên `asset_id IS NULL`.
- SECURITY có thể giới hạn theo `asset_id`; quyền đọc báo cáo gốc phải có scope tài sản rõ ràng.
- SCAN_OPERATOR và REMEDIATOR cho biết người dùng đủ điều kiện nhận nhiệm vụ; người đang chịu trách nhiệm cụ thể vẫn lấy từ `scan_plan.operator_id` và `remediation_ticket.assignee_id`.
- Chủ tài sản lấy từ `asset.owner_id`, không lưu thêm OWNER role trùng lặp.
- Không tạo bảng `role`, `permission` hoặc `role_permission` vì tập role cố định và nhỏ; quyền chi tiết được mã hóa tại API theo yêu cầu đề tài.

Lịch sử cấp/thu hồi nằm ngay trong bản ghi grant và được ghi thêm vào `audit_log`; không xóa cứng quyền đã thu hồi.

### 5.3. `asset`

Khóa chính: `id`  
Khóa ứng viên: `code`

Thuộc tính:

- `code`, `name`, `base_url`, `environment`
- `owner_id`: chủ tài sản
- `security_officer_id`: ATTT phụ trách/phê duyệt
- `criticality`, `data_classification`, `internet_exposed`
- `status`: Draft, Pending Approval, Active, Suspended, Retired
- `created_by`, `created_at`, `updated_at`

Không lưu `current_scope_id`. Phạm vi hiện hành được xác định từ phiên bản Approved còn hiệu lực.

Không lưu `domain`; hostname được phân tích từ `base_url` khi cần. Các domain/subdomain bổ sung thuộc `asset_scope_rule`.

Quyền tối thiểu được suy ra:

- Owner: `asset.owner_id`.
- ATTT: `asset.security_officer_id`.
- Admin: quyền ADMIN đang hiệu lực trong `user_role`, nhưng không mặc nhiên đọc báo cáo gốc.

Quyền đọc báo cáo gốc yêu cầu role SECURITY đúng tài sản. Operator chỉ xem trạng thái kỹ thuật; assignee chỉ xem finding/ticket được giao; owner xem dữ liệu đã che của tài sản. Việc gán `security_officer_id`, `operator_id` và `assignee_id` phải kiểm tra người được gán có role phù hợp.

### 5.4. `asset_scope_revision`

Khóa chính: `id`  
Khóa ứng viên: `(asset_id, version_no)`

Thuộc tính:

- `asset_id`, `version_no`
- `status`: Draft, Pending Approval, Approved, Rejected
- `change_reason`
- `created_by`, `created_at`
- `confirmed_by`, `confirmed_at`
- `decided_by`, `decided_at`, `decision_reason`
- `effective_from`, `effective_until`

Ràng buộc:

- Phiên bản Approved là bất biến.
- Các khoảng hiệu lực Approved của cùng asset không chồng lấn.
- Chỉ chủ tài sản xác nhận và ATTT phụ trách phê duyệt.

### 5.5. `asset_scope_rule`

Khóa chính: `id`  
Khóa ứng viên: `(scope_id, effect, target_type, target_value, match_mode)`

Thuộc tính:

- `scope_id`
- `effect`: Include, Exclude
- `target_type`: Domain, Subdomain, URL
- `target_value`
- `match_mode`: Exact, Prefix

Bảng phải giữ riêng để đáp ứng 1NF/BCNF và kiểm tra từng target trước khi gọi ZAP. Không thay bằng JSONB, mảng hoặc chuỗi phân cách.

### 5.6. `asset_scan_window`

Khóa chính: `id`  
Khóa ứng viên: `(scope_id, weekday_iso, start_time, end_time, ends_next_day, timezone)`

Thuộc tính:

- `scope_id`, `weekday_iso`
- `start_time`, `end_time`, `ends_next_day`
- `timezone`

Bảng phải giữ vì khung giờ là quan hệ 1:N với phiên bản phạm vi và là kiểm soát an toàn bắt buộc.

### 5.7. `zap_scan_profile`

Khóa chính: `id`  
Khóa ứng viên: `(code, version_no)`

Thuộc tính:

- `code`, `name`, `version_no`
- `status`: Draft, Pending Approval, Active, Retired
- `spider_kind`, `passive_scan_enabled`, `active_scan_enabled`
- giới hạn crawl, thời gian, request delay và thread
- `auth_kind`, `auth_header_name`, `auth_secret_ref`
- `report_format`: JSON hoặc XML
- `service_user_id`
- `created_by`, `created_at`, `approved_by`, `approved_at`, `retired_at`

Profile Active/Retired bất biến. Thay đổi tạo version mới.

Không tạo `zap_connection`: endpoint, API key, TLS và network route thuộc cấu hình triển khai; database chỉ giữ tham chiếu bí mật.

### 5.8. `scan_plan`

`scan_plan` thay cho cả lịch định kỳ và bảng yêu cầu xác minh.

Khóa chính: `id`  
Khóa ứng viên: `(code, version_no)`

Thuộc tính chung:

- `code`, `version_no`, `name`
- `plan_kind`: Periodic, Verification
- `scope_id`, `zap_profile_id`
- `operator_id`
- `status`: Draft, Active, Paused, Expired, Cancelled
- `created_by`, `created_at`, `approved_by`, `approved_at`

Thuộc tính cho Periodic:

- `recurrence_rule`, `timezone`
- `effective_from`, `effective_until`

Thuộc tính cho Verification:

- `remediation_ticket_id`
- `requested_for`

Ràng buộc CHECK bảo đảm mỗi loại chỉ dùng đúng nhóm cột. Một plan Verification tạo tối đa một `scan_run`; cần quét lại thì tạo plan mới.

Không lưu:

- `asset_id`: suy ra `scope_id -> asset_scope_revision.asset_id`.
- `next_due_at`: tính từ RRULE và các lần chạy đã có.
- `target_urls`: truy từ `asset_scope_rule`.
- `operations_team_id`: mô hình tối giản giao một `operator_id` cụ thể.

Plan đã sinh `scan_run` không được sửa phạm vi/profile/chu kỳ; thay đổi tạo version mới.

### 5.9. `scan_run`

`scan_run` hợp nhất ba quan hệ 1:1 trong thiết kế cũ:

- lần quét nghiệp vụ (`scan_occurrence`),
- phiên thực thi ZAP (`zap_execution`),
- lô/báo cáo nguồn (`import_batch`, `zap_artifact`).

Đây chính là **bảng hồ sơ các lần quét**. Mỗi dòng đại diện một lần chạy cụ thể, được giữ lại để xem lịch sử và không bị ghi đè bởi lần chạy sau.

Khóa chính: `id`  
Khóa ứng viên: `code`, `(plan_id, scheduled_for)`; plan Verification có UNIQUE `plan_id`.

Thuộc tính lập lịch và thực thi:

- `plan_id`, `code`, `scheduled_for`
- `scan_status`: Planned, Queued, Running, Completed, Failed, Timed Out, Cancelled
- `requested_at`, `started_at`, `heartbeat_at`, `finished_at`
- `external_plan_id`, `external_session_id`
- `cancel_requested_at`
- `runner_name`, `image_reference`, `zap_version`, `exit_code`
- `failure_code`, `failure_message_masked`

Thuộc tính báo cáo nguồn nullable cho tới khi nhận báo cáo:

- `report_status`: Not Received, Validating, Processed, Rejected, Failed
- `report_object_key`, `report_sha256`, `report_size_bytes`
- `report_format`, `report_key_ref`, `report_received_at`

Thuộc tính đánh giá chỉ dùng cho plan Verification:

- `coverage_confirmed`, `fingerprint_present`
- `verification_outcome`: Closed, Reopened
- `verification_reason`
- `evaluated_by`, `evaluated_at`

Không lưu `asset_id`, `scope_id`, `profile_id`, `service_user_id` hoặc operator vì đều truy theo `plan_id`.

Không lưu bộ đếm báo cáo. Mô hình tối giản xử lý nguyên tử: toàn bộ báo cáo hợp lệ thì ghi các occurrence trong một transaction; có lỗi cấu trúc/phạm vi thì rollback và đặt `report_status = Rejected/Failed` cùng lỗi đã che.

### 5.10. `finding`

Khóa chính: `id`  
Khóa ứng viên: `code`, `(asset_id, fingerprint)`

Thuộc tính:

- `asset_id`, `code`, `fingerprint`
- `identity_version`, `identity_key_ref`, `identity_ciphertext`
- `source_alert_ref`
- `affected_url_masked`, `parameter_masked`, `input_vector_masked`, `http_method`
- `title_masked`
- `final_severity`, `severity_reason`, `assessed_by`, `assessed_at`
- `status`: New, Triaged, Assigned, In Progress, Pending Verification, Closed, Reopened, False Positive, Cancelled
- `decision_reason`, `decision_by`, `decision_at`
- `created_at`, `row_version`

Không lưu:

- `first_seen_at`, `last_seen_at`: MIN/MAX thời gian `scan_run` qua `finding_occurrence`.
- `current_review_id`: Final Severity hiện hành nằm trực tiếp trên finding; lịch sử thay đổi nằm trong `audit_log`.
- `verification_result_id`: kết quả xác minh truy qua plan Verification và `scan_run`.

Việc giữ Final Severity hiện hành không vi phạm 3NF: giá trị là thuộc tính của Finding và phụ thuộc vào khóa Finding. Audit là lịch sử tuân thủ, không phải một bảng trạng thái song song.

### 5.11. `finding_occurrence`

Đây là bảng nối M:N giữa Finding và ScanRun, đồng thời giữ dữ liệu nguồn thay đổi theo lần quét.

Khóa chính: `id`  
Khóa ứng viên: `(finding_id, scan_run_id)` và `(scan_run_id, record_locator)`

Thuộc tính:

- `finding_id`, `scan_run_id`, `record_locator`
- `source_severity`, `source_confidence`
- `source_attack_masked`, `source_evidence_masked`
- `description_masked`, `technical_details_masked`
- `remediation_guidance_masked`
- `cwe_id`, `wasc_id`
- `created_at`

Không lưu `asset_id`, `scan_id`, `batch_id` hoặc `observed_at`; các giá trị được truy theo hai FK nguồn.

References và tags không cần cho bốn chức năng chính nên không lưu trong mô hình tối giản.

### 5.12. `remediation_ticket`

Khóa chính: `id`  
Khóa ứng viên: `code`; mô hình tối giản đặt UNIQUE `finding_id` để một finding có một workflow khắc phục hiện hành.

Thuộc tính:

- `finding_id`, `code`
- `assignee_id`, `assigned_by`, `assigned_at`
- `due_at`
- `progress_percent`
- `root_cause_masked`, `remediation_action_masked`
- bằng chứng tối thiểu: `evidence_summary_masked`, `evidence_object_key`, `evidence_sha256`, `evidence_key_ref`
- `remediated_at`
- `created_by`, `created_at`, `updated_by`, `updated_at`

Không tạo `ticket_event`, `evidence`, `sla_rule` hoặc `verification_request/result`:

- phân công, tiến độ, biện pháp và bằng chứng tối thiểu nằm trong ticket;
- trạng thái workflow dùng duy nhất `finding.status`, không lặp lại trên ticket;
- lịch sử trước/sau nằm trong `audit_log`;
- hạn xử lý là quyết định nghiệp vụ nhập trên ticket;
- yêu cầu xác minh là một `scan_plan` loại Verification;
- kết quả xác minh nằm trên `scan_run` của plan đó.

`progress_percent` là dữ liệu người xử lý nhập, không phải giá trị có thể suy ra chắc chắn nên được giữ.

### 5.13. `audit_log`

Khóa chính: `id`

Thuộc tính:

- `actor_id`, `actor_kind`, `occurred_at`
- `category`, `action`
- `object_type`, `object_id`
- `outcome`
- `before_masked`, `after_masked`, `reason_masked`
- `correlation_id`

Audit append-only. Không lặp `asset_id`, `finding_id`, `evidence_id` hoặc các FK chuyên biệt. Đối tượng chính được xác định bằng `object_type/object_id`; chuỗi thao tác liên quan được gom bằng `correlation_id`.

JSONB chỉ được dùng cho ảnh before/after đã che. Dữ liệu này không được dùng làm nguồn FK, phân quyền hoặc điều phối ZAP.

## 6. Quan hệ chính

```text
app_user
  ├── user_role
  ├── asset (owner, security officer)
  ├── zap_scan_profile (service user, creator, approver)
  ├── scan_plan (operator, creator, approver)
  ├── finding (assessor/decision maker)
  ├── remediation_ticket (assignee/updater)
  └── audit_log (actor)

asset
  └── asset_scope_revision
        ├── asset_scope_rule
        ├── asset_scan_window
        └── scan_plan
              └── scan_run
                    └── finding_occurrence ── finding
                                                └── remediation_ticket
                                                      └── scan_plan (Verification)

zap_scan_profile ── scan_plan
```

Không có vòng FK: remediation ticket được tạo từ finding; plan Verification tham chiếu ticket; scan run tham chiếu plan; kết quả xác minh được đọc từ scan run.

## 7. Dữ liệu chỉ truy vấn, không lưu vật lý

| Dữ liệu cần hiển thị | Truy vấn nguồn |
|---|---|
| Phạm vi hiện hành | Scope Approved trong khoảng hiệu lực |
| URL được phép quét | Rule Include trừ Rule Exclude của scope |
| Kỳ quét kế tiếp | RRULE của plan + lần chạy gần nhất |
| Tài sản của plan/run | Plan → Scope → Asset |
| Profile, service user, operator của run | Run → Plan → Profile/User |
| First seen/last seen | MIN/MAX run time qua occurrence |
| Số phát hiện theo run | COUNT occurrence |
| Số lỗi theo mức độ/trạng thái | GROUP BY finding |
| Phiếu sắp/quá hạn | Finding status + ticket due_at so với thời gian hiện tại |
| Lần xác minh của ticket | Ticket → Verification Plan → ScanRun |
| Tỷ lệ quét thành công/thất bại | GROUP BY scan_run.scan_status |
| Thông báo cần chú ý | Truy vấn run lỗi, finding High/Critical và ticket gần hạn |
| Báo cáo tổng hợp | View/query từ asset, run, finding và ticket |

Các view được khuyến nghị nhưng không phải bảng:

- `v_asset_current_scope`
- `v_plan_next_due`
- `v_scan_run_detail`
- `v_finding_summary`
- `v_remediation_worklist`
- `v_verification_result`

## 8. Ánh xạ từ 30 bảng hiện tại xuống 13 bảng

| Bảng hiện tại | Xử lý trong mô hình tối giản |
|---|---|
| `app_user`, `user_role` | Giữ; user lưu định danh, role lưu quyền tối thiểu theo tài sản |
| `team`, `team_member` | Bỏ; không quản lý cơ cấu tổ chức |
| `asset` | Giữ, bỏ con trỏ current scope |
| `asset_scope_revision`, `asset_scope_rule`, `asset_scan_window` | Giữ vì kiểm soát phạm vi/an toàn |
| `zap_scan_profile` | Giữ và hoàn thiện version/phê duyệt |
| `scan_schedule` | Đổi thành `scan_plan`, nhận cả Verification |
| `scan_occurrence`, `zap_execution`, `import_batch`, `zap_artifact` | Hợp nhất thành `scan_run` |
| `import_error` | Bỏ; xử lý báo cáo nguyên tử, lỗi đã che trên run/audit |
| `finding` | Giữ, nhận Final Severity hiện hành |
| `finding_occurrence`, `finding_observation` | Hợp nhất thành `finding_occurrence` |
| `finding_review` | Bỏ; hiện hành ở finding, lịch sử ở audit |
| `sla_rule` | Bỏ khỏi phạm vi lõi |
| `remediation_ticket` | Giữ, nhận trạng thái/tiến độ/bằng chứng hiện hành |
| `ticket_event`, `evidence` | Bỏ; lịch sử audit, bằng chứng tối thiểu ở ticket |
| `verification_request`, `verification_result` | Bỏ; plan Verification + scan run |
| `risk_acceptance` | Bỏ vì là workflow nâng cao |
| `audit_log` | Giữ, lược FK chuyên biệt |
| `report_export`, `report_export_asset` | Bỏ; tạo báo cáo theo yêu cầu và ghi audit |
| `notification` | Bỏ; worklist/alert truy vấn từ dữ liệu nguồn |

## 9. Đánh giá chuẩn hóa

| Bảng | Khóa quyết định dữ liệu | Mức chuẩn mục tiêu |
|---|---|---|
| `app_user` | `id`, `username` | BCNF |
| `user_role` | `id`, grant đang hiệu lực `(user_id, role_code, asset_id)` | BCNF |
| `asset` | `id`, `code` | BCNF |
| `asset_scope_revision` | `id`, `(asset_id, version_no)` | BCNF |
| `asset_scope_rule` | `id`, bộ rule duy nhất | BCNF |
| `asset_scan_window` | `id`, bộ window duy nhất | BCNF |
| `zap_scan_profile` | `id`, `(code, version_no)` | BCNF |
| `scan_plan` | `id`, `(code, version_no)` | BCNF |
| `scan_run` | `id`, `code`, `(plan_id, scheduled_for)` | BCNF |
| `finding` | `id`, `code`, `(asset_id, fingerprint)` | BCNF |
| `finding_occurrence` | `id`, hai khóa ứng viên của quan hệ | BCNF |
| `remediation_ticket` | `id`, `code`, `finding_id` | BCNF trong giả định một ticket/finding |
| `audit_log` | `id` | 3NF; JSONB là payload audit bất biến |

## 10. Các giả định giới hạn để đạt 13 bảng

Mô hình 13 bảng đúng khi chấp nhận các giới hạn sau:

1. Chỉ tích hợp một hệ thống OWASP ZAP.
2. Một scan run có tối đa một báo cáo JSON/XML làm nguồn.
3. Báo cáo được xử lý nguyên tử; không giữ chi tiết nhiều lỗi theo từng record.
4. Một finding có một remediation ticket hiện hành.
5. Một verification plan tạo một scan run; quét lại tạo plan mới.
6. Mỗi asset có một owner và một ATTT phụ trách chính.
7. Mỗi plan có một operator và mỗi ticket có một assignee chính.
8. Không quản lý chấp nhận rủi ro, SLA policy, notification inbox hoặc link chia sẻ báo cáo.
9. Báo cáo thống kê được tạo bằng query/view, không lưu snapshot.
10. Những thay đổi nhạy cảm luôn ghi audit cùng giao dịch nghiệp vụ.

Nếu một trong các giới hạn này trở thành yêu cầu bắt buộc, chỉ bổ sung đúng bảng quan hệ cần thiết thay vì khôi phục toàn bộ thiết kế 30 bảng.
