# Ma trận truy vết màn hình – API – Use Case

## 1. Mapping màn hình tới API

| UI | Operations sử dụng | UC chính |
|---|---|---|
| UI-01/02/03 | `getCurrentContext`, `listNotifications` | C08, C09 |
| UI-04 | `listAssets`, `createAsset` | A01, A07 |
| UI-05 | `createAsset` | A01 |
| UI-06 | `getAsset`, `updateAsset`, `transitionAsset` | A02, A05, A06, A07 |
| UI-07 | `createScopeRevision`, `updateScopeRevision`, `submitScopeRevision`, `confirmScopeRevision`, `decideScopeRevision` | A01–A04 |
| UI-08/09 | `listAssetSchedules`, `createSchedule`, `updateSchedule`, `transitionSchedule`, `confirmScheduleTime` | S01–S05, S08 |
| UI-10/11 | `createScanOccurrence`, `listScanOccurrences`, `transitionScanOccurrence` | S05–S07 |
| UI-12 | `listAssets`, `listScanOccurrences`, `createImport` | I01–I09 |
| UI-13 | `getImport`, `listImportErrors`, `downloadRawImport` | I11, I12, C04 |
| UI-14 | `listFindings` | T01, R12, RA05, C09 |
| UI-15 | `getFinding`, `listFindingOccurrences`, `listFindingReviews` | I10, T01, T06, R09, C06 |
| UI-16 | `createFindingReview`, `applyFindingReview` | T01–T04 |
| UI-17 | `confirmFindingBusinessImpact` | T05 |
| UI-18 | `secondReviewFindingReview`, `applyFindingReview` | T02, T03 |
| UI-19/20 | `listTickets`, `getTicket`, `listTicketEvents` | R03–R05, R12, C06 |
| UI-20/21 | `createRemediationTicket`, `appendTicketEvent`, `submitTicketForVerification`, `downloadEvidence` | R01–R06, R13, C04 |
| UI-22 | `createVerificationRequest`, `createVerificationScanOccurrence`, `getVerificationRequest` | R07–R09 |
| UI-23 | `getVerificationRequest`, `decideVerification` | R09–R11 |
| UI-24 | `listRiskAcceptances` | RA02, RA05 |
| UI-25 | `createRiskAcceptance` | RA01 |
| UI-26 | `getRiskAcceptance`, `supplementRiskAcceptance`, `decideRiskAcceptance`, `createRiskRenewal` | RA02–RA07 |
| UI-27/28 | `createReportExport`, `getReportExport`, `downloadReportExport`, `revokeReportExport` | C05, C10 |
| UI-29 | `listNotifications`, `markNotificationRead` | C08 |
| UI-30 | `listAuditLogs`, history operations | C03, C06, C07 |
| UI-31 | `createAccessGrant`, `revokeAccessGrant`, `listAuditLogs` | C01–C03 |
| UI-32 | Không gọi detail tiếp; chỉ context/worklist an toàn | C09 |

## 2. Mapping toàn bộ 57 operation

| Nhóm operation | UI sử dụng | Trạng thái UI đặc biệt |
|---|---|---|
| Context (1) | UI-01–03, shell | inactive/revoked → unavailable |
| Access (2) | UI-31 | sensitive grant reason; revoke confirm |
| Asset/scope (10) | UI-04–07 | duplicate warning, current/proposed diff, stale 412 |
| Schedule/scan (8) | UI-08–11, UI-22 | timezone/window, Failed/Missed reason, no ZAP control |
| Import (4) | UI-12/13 | streaming, duplicate, progress, Partial, Rejected/Failed |
| Finding/review (8) | UI-14–18 | three severity concepts, SoD, evidence, second review |
| Ticket (6) | UI-19–21 | current event snapshot, due, task scope, stale pointer |
| Verification (4) | UI-22/23 | scan after remediation, coverage, Closed/Reopened |
| Risk (6) | UI-24–26 | previous status, conditional decision fields, immutable renewal |
| Evidence/report (5) | UI-13/20/27/28 | audited stream, recipient/all-asset reauth, revoke/expiry |
| Audit/notification (3) | UI-29–31 | category separation, own recipient, masked data |

Tổng: 57 operations. Các xử lý nội bộ import/masking/fingerprint/deadline/expiry/notification không có form để người dùng gọi trực tiếp.

## 3. Action gating mẫu

| Action | Hiện khi | Không hiện/disabled khi | API vẫn kiểm |
|---|---|---|---|
| Phê duyệt scope | SECURITY + asset.approve + scope Pending + Owner confirmed | scope Draft/terminal, wrong asset | role/asset/state/version |
| Upload report | SCAN_OPS + import.create + Asset Active + Scan Completed | scan Planned/Failed hoặc wrong team | role/asset/scan/type/size |
| Apply Final Severity | SECURITY + review đủ evidence/second review | importer SoD, old proposal, missing reviewer | SoD/current review/version |
| Cập nhật phiếu | current REMEDIATOR | prior assignee/read-only role | current_event/task scope |
| Closed | SECURITY independent + Pending Verification + valid coverage/no fingerprint | remediator participant, Partial thiếu coverage | all verification guards |
| Risk Approve | RISK_APPROVER + authority + not requester | self-approval, wrong asset/status | authority/SoD/time/state |
| Tải raw/evidence | scope + sensitive flag + content exists | role-only/no flag/revoked | reauth per stream + audit |
| Tải export | recipient + rights trên mọi asset + not expired/revoked | mất một asset right | recipient/token/all assets |

## 4. Data fetching và client state

- Route loader lấy `getCurrentContext` trước module data. Permission context chỉ giữ trong memory; request tiếp theo vẫn chịu API authorization.
- Query key luôn gồm resource ID + filter + cursor + safe permission-context version; khi revoke context thay đổi thì invalidate và xóa data nhạy cảm.
- Form mutation giữ `Idempotency-Key` ổn định đến khi có kết quả xác định; không sinh key mới khi retry network không rõ.
- ETag lưu cùng representation và gửi lại bằng `If-Match`; `412` không tự retry mutation. Query refresh đọc version mới, draft form giữ riêng để compare.
- Poll Import dùng backoff và dừng ở terminal state; tab ẩn giảm polling; thông báo có thể dẫn người dùng quay lại.
- Không prefetch raw/evidence/export. Download chỉ bắt đầu sau explicit confirmation.
- Browser back/forward không resubmit mutation; Post/Redirect/Get theo router state và result route.

## 5. Error mapping

| API | UI |
|---|---|
| AUTH-001 | Đăng nhập lại; không giữ deep link ngoài internal allowlist |
| AUTH-003/RES-001 ngoài scope | UI-32 chung |
| CON-001/CON-002 | Conflict panel, giữ draft, tải representation mới |
| STATE-001 | Refresh detail, action biến mất, message giải thích |
| IMP-004 | Info banner và link lô cũ |
| IMP-006/VER-003 | Warning coverage trong Verification; Closed disabled |
| AUTH-005/VER-004/RSK-002 | SoD explanation, chỉ dẫn chuyển người có thẩm quyền |
| FILE-001/002/003 | Inline file error trước/đúng bước upload; bỏ file không hợp lệ |
| FILE-005 | Export expired/revoked state, action tạo export mới nếu có quyền |
| AUDIT-001/SYS-002 | Không báo success; retry có kiểm soát/cùng idempotency key |

## 6. Trace rule

Mỗi implementation PR của UI phải ghi UI-ID, operationId, UC/BR, trạng thái đã thiết kế, screenshot ở 320/1024px, keyboard/axe evidence và test permission-negative. Thay route/field/action phải cập nhật ma trận này và OpenAPI nếu hợp đồng thay đổi.
