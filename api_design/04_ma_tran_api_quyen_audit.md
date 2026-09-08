# Ma trận API – vai trò – tài sản – audit

## 1. Ký hiệu và kiểm tra phạm vi

`A` = được phép khi có permission và grant asset hiệu lực; `O` = thêm điều kiện là Owner hiện hành; `T` = thêm điều kiện phiếu/current assignment; `S` = thêm cờ quyền nhạy cảm đúng asset + lý do; `P` = theo policy/thẩm quyền độc lập; `–` = mặc định từ chối. Vai trò không thay kiểm asset. Endpoint đọc ID phải authorize từ tập ID được phép trước khi load chi tiết để chống IDOR.

| Nhóm API/operation | Permission | ADMIN | SECURITY | OWNER | SCAN_OPS | REMEDIATOR | RISK_APPROVER | AUDITOR | Audit bắt buộc |
|---|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|---|
| `getCurrentContext` | authenticated | A | A | A | A | A | A | A | Denied theo policy |
| `createAccessGrant`, `revokeAccessGrant` | `access.manage` | A | – | – | – | – | – | – | Permission; mọi outcome |
| `list/getAsset` | `asset.read` | – | A | O | A | T | A | A | Denied; read nếu policy |
| `createAsset` | `asset.declare` | – | A | O | – | – | – | – | Business mutation |
| `updateAsset`, scope create/update/submit | `asset.update` | – | A | O | – | – | – | – | Business mutation |
| `confirmScopeRevision` | `asset.confirm` | – | – | O | – | – | – | – | Business transition |
| `decideScopeRevision` | `asset.approve` | – | A | – | – | – | – | – | Business transition |
| `transitionAsset` | `asset.update` | – | A | – | – | – | – | – | Business transition |
| schedule list | `schedule.read` | – | A | O | A | T | A | A | Denied/read policy |
| schedule create/update/transition | `schedule.manage` | – | A | – | – | – | – | – | Business mutation |
| `confirmScheduleTime` | `schedule.confirm_time` | – | – | O | – | – | – | – | Business mutation |
| scan create (manual) | `scan.record`/`schedule.manage` | – | A | – | A | – | – | – | Business mutation |
| scan occurrence list | `schedule.read` | – | A | O | A | T | A | A | Denied/read policy |
| `transitionScanOccurrence` | `scan.record` | – | – | – | A | – | – | – | Business transition |
| `createImport` | `import.create` | – | – | – | A | – | – | – | Upload; mọi outcome |
| import status/errors | `import.read` | – | A | O | A | T | A | A | Denied; masking |
| `downloadRawImport` | `import.read` + raw flag | – | S | O+S | S | T+S | S | S | Mọi lượt truy cập |
| finding list/detail/source | `finding.read` | – | A | O | A | T | A | A | Denied; masking |
| review history | `finding.read` + `audit.business.read` | – | A | – | – | – | – | A | Denied/read policy |
| review Severity/Disposition | `finding.assess`/`finding.disposition` | – | A | – | – | – | – | – | Mutation/transition |
| `confirmFindingBusinessImpact` | owner contribution | – | – | O | – | – | – | – | Mutation |
| second review | `review.second` | – | P | – | – | – | P | – | Decision mọi outcome |
| create/assign ticket | `ticket.assign` | – | A | – | – | – | – | – | Mutation/transition |
| ticket list/history | `ticket.read` | – | A | O | A | T | A | A | Denied; masking |
| append ticket event | `ticket.assign`/`ticket.update` | – | A | – | – | T | – | – | Mutation/transition |
| submit verification | `ticket.update` | – | – | – | – | T | – | – | Transition |
| create verification request | `verification.request` | – | A | – | – | – | – | – | Mutation |
| create Verification scan | `scan.record`/`verification.request` | – | A | – | A | – | – | – | Mutation; không điều khiển ZAP |
| get Verification request | permission theo nhiệm vụ | – | A | – | A | T | – | – | Denied; masking |
| decide Closed/Reopened | `verification.decide` | – | A+P | – | – | – | – | – | Decision mọi outcome |
| evidence content | `finding.read`/`ticket.read` + evidence flag | – | S | O+S | S | T+S | S | S | Mọi lượt truy cập |
| create/supplement risk | `risk.request` | – | A | O | – | – | – | – | Mutation/transition |
| get/list risk | `finding.read` + risk permission theo vai trò | – | A | O | – | – | A | A | Denied; masking |
| decide risk | `risk.decide` | – | – | – | – | – | A+P | – | Decision mọi outcome |
| create renewal | `risk.request` | – | A | O | – | – | – | – | Mutation |
| create export | `report.export` | – | A | O | A | T | A | A | Mọi outcome/all assets |
| export metadata/download | `report.download` | – | A | O | A | T | A | A | Download mọi outcome |
| revoke export | `report.revoke` | – | A | O | A | T | A | A | Mutation |
| audit Permission | `audit.permission.read` | A | – | – | – | – | – | A | Denied/read policy |
| audit Business | `audit.business.read` | – | A | – | – | – | – | A | Denied/read policy |
| notification list/read | own recipient | – | A | A | A | A | A | – | IDOR denial |

ADMIN chỉ quản lý grant và đọc permission audit; không là superuser nghiệp vụ. AUDITOR chỉ đọc. Quyền raw/evidence mặc định false và không suy ra từ role.

## 2. Action audit và căn cứ

| Miền | Action chính | Object/FK căn cứ | Before/after/from-to | Nội dung cấm |
|---|---|---|---|---|
| Quyền | `access.grant`, `access.revoke` | user_role, user, asset | before/after đã che | token/claim thô, lý do có bí mật |
| Asset/scope | `asset.*`, `asset.scope.*` | asset, scope | from/to hoặc snapshot | URL chứa secret/query nhạy cảm |
| Lịch/scan | `schedule.*`, `scan_occurrence.*` | schedule, scan, asset | from/to + reason | ZAP credential/config secret |
| Import | `import.upload/process` | batch, scan, asset | status/counts | raw bytes, object key, checksum nếu không cần |
| Finding/review | `finding.review.*`, `finding.transition` | finding, review, evidence | from/to + linked FK | Attack/Evidence payload |
| Ticket | `ticket.*` | ticket/event/finding/evidence | snapshot ID, from/to | evidence body |
| Verification | `verification.*` | request/result/scan/batch/finding | from/to + linked FK | raw report/technical secret |
| Risk | `risk.*` | risk/finding | from/to + masked snapshot | conditions/impact chưa che |
| Report | `report.export/download/revoke` | export + asset list | outcome/reason | bearer token/hash, file/object key |

Mọi audit có actor ID/kind, UTC time, category, action, asset khi xác định an toàn, object type/string ID, outcome, reason masked và correlation ID. Denied tới ID không tồn tại dùng string `object_id`, không tạo FK giả.

## 3. Response và masking

| Dữ liệu | API thông thường | Endpoint riêng |
|---|---|---|
| Raw ZAP report | Chỉ metadata tối thiểu; không object/key/hash | `GET /imports/{id}/raw-content`, raw flag + reason + audit |
| Evidence | ID/loại/tóm tắt đã che | Endpoint evidence stream chỉ được thêm khi policy chi tiết được duyệt; sensitive flag + audit |
| Observation Attack/Evidence | Masked/omitted | Không trả qua list/detail thông thường |
| Export | Metadata/trace/expiry/revocation; không token hash/object key | `/report-exports/{id}/content`, recipient + all-asset reauth + audit |
| Audit before/after | JSON đã che | Không có đường lấy request/raw payload |
| Notification | Asset code, Finding code, status, internal link | Không có nội dung nhạy cảm |

## 4. Bộ negative test quyền bắt buộc

1. Thay `assetId`, `findingId`, `ticketId`, `importId`, `riskAcceptanceId` sang asset không được cấp quyền.
2. OWNER có grant nhưng không phải `asset.owner_id`; REMEDIATOR từng được giao nhưng không còn là current assignment.
3. ADMIN thử đọc Finding/raw report; AUDITOR thử mutation; SCAN_OPS thử quyết định Closed; REMEDIATOR thử risk decision.
4. Role hợp lệ nhưng grant revoked/inactive user; grant `asset_id=NULL` thử dùng như wildcard.
5. Raw flag trên asset A thử tải report asset B; quyền bị thu hồi sau khi mở màn hình nhưng trước stream.
6. Export gồm nhiều asset, actor mất quyền trên một asset trước tải.
7. Importer tự apply Final Severity từ chính nguồn; assessor tự second review; requester tự risk approval; người thực sự khắc phục tự Closed.
8. Denied response không phân biệt ID tồn tại/không tồn tại ngoài phạm vi và audit không lộ dữ liệu.
