# Truy vết use case, kiểm tra và cổng hoàn thành giai đoạn 8

## 1. Truy vết nhóm API tới F/BR

| Nhóm | F/BR chính | Operation/API hoặc xử lý máy chủ |
|---|---|---|
| Auth/access | BR-12, BR-13 | context, grant/revoke, AuthZ middleware |
| Assets/scope | F-01, BR-01, BR-14 | `/assets`, scopes, decisions, transitions |
| Schedule/scan | F-02, BR-01, BR-02 | schedules, occurrences, transitions, due worker |
| Import | F-03, BR-03..BR-07, BR-12 | upload/status/errors/raw; import worker |
| Finding review | F-04, BR-08, BR-09 | finding detail/reviews/second-review/apply |
| Ticket/verification | F-04, BR-10 | tickets/events/verification request/decision |
| Risk | F-04, BR-11 | risk request/supplement/decision/renewal; expiry worker |
| Audit/report/notification | BR-12..BR-15 | audited stream/export/revoke/audit query/in-app notification |

## 2. Truy vết toàn bộ UC

| UC | Hợp đồng thực thi |
|---|---|
| A01 | `createAsset`, `createScopeRevision`, `submitScopeRevision`, `confirmScopeRevision`, `decideScopeRevision` |
| A02 | `updateAsset`, `create/updateScopeRevision`, `transitionAsset` |
| A03 | `confirmScopeRevision` |
| A04 | `decideScopeRevision` |
| A05 | `transitionAsset` tới Suspended |
| A06 | `transitionAsset` tới Retired |
| A07 | `listAssets`, `getAsset` |
| S01 | `createSchedule` |
| S02 | `updateSchedule` |
| S03 | `transitionSchedule` tới Active |
| S04 | `transitionSchedule` tới Paused |
| S05 | `listAssetSchedules`; due worker tạo/nhắc kỳ, không chạy ZAP |
| S06 | `createScanOccurrence`, `transitionScanOccurrence` |
| S07 | `transitionScanOccurrence` Failed/Missed; missed worker |
| S08 | `confirmScheduleTime` |
| I01 | `createImport` |
| I02 | AuthZ nội bộ trong `createImport` |
| I03 | Guard Asset/scan/cùng scope nội bộ trong `createImport` |
| I04 | Streaming type/schema validation trong import worker |
| I05 | Scope validator, Exclude ưu tiên, trong import worker |
| I06 | Private immutable object finalize + checksum trong import service |
| I07 | Masking policy dùng chung cho worker/query/export |
| I08 | Normalize + versioned HMAC fingerprint trong import worker |
| I09 | Idempotency + unique batch/finding/occurrence/observation trong API/worker/DB |
| I10 | Per-record transaction trong import worker |
| I11 | Per-record rollback + `import_error` masked |
| I12 | `getImport`, `listImportErrors` |
| T01 | `list/getFinding`, `createFindingReview` |
| T02 | `createFindingReview`, `secondReviewFindingReview`, `applyFindingReview` |
| T03 | Review False Positive + second review/apply |
| T04 | Review Cancelled + apply; không hard delete |
| T05 | OWNER dùng `confirmFindingBusinessImpact` trên Severity review hiện có |
| T06 | `listFindingReviews` |
| R01 | `createRemediationTicket` |
| R02 | create ticket/Assignment hoặc `appendTicketEvent` |
| R03 | `appendTicketEvent` Accepted/Progress ánh xạ snapshot Progress |
| R04 | `appendTicketEvent` Progress |
| R05 | `appendTicketEvent` Progress + evidence metadata/reference |
| R06 | `submitTicketForVerification` |
| R07 | `createVerificationRequest` |
| R08 | `createVerificationScanOccurrence` tạo lần quét bên ngoài; kết quả qua `transitionScanOccurrence` + `createImport`, rồi được chọn trong verification decision |
| R09 | Fingerprint/coverage validator nội bộ của `decideVerification` |
| R10 | `decideVerification` outcome Closed |
| R11 | `decideVerification` outcome Reopened; import/expiry worker cho cạnh nguồn tương ứng |
| R12 | `listFindings`, `listTickets`; deadline worker/notifications |
| R13 | `appendTicketEvent` Assignment/Deadline snapshot |
| RA01 | `createRiskAcceptance` |
| RA02 | `decideRiskAcceptance` Needs Information; `supplementRiskAcceptance` |
| RA03 | `decideRiskAcceptance` Approve |
| RA04 | `decideRiskAcceptance` Reject/Needs Information |
| RA05 | worklist filters + expiry worker |
| RA06 | expiry worker `Risk Accepted→Reopened` |
| RA07 | `createRiskRenewal`; quyết định dùng same risk decision contract |
| C01 | `createAccessGrant` |
| C02 | `revokeAccessGrant` |
| C03 | `listAuditLogs` category Permission |
| C04 | `downloadRawImport`, `downloadEvidence`; policy quyết định loại evidence/actor nào được cấp cờ riêng |
| C05 | create/get/download export |
| C06 | review/ticket history + `listAuditLogs` Business |
| C07 | Audit middleware/writer; không có endpoint để client tự ghi audit |
| C08 | notifications list/read + notification worker |
| C09 | Mọi list/detail dùng authorized asset/task query |
| C10 | `revokeReportExport` |

UC nội bộ không được lộ thành API riêng vì client có thể bỏ qua transaction cha. Với C04, endpoint evidence stream mặc định từ chối; policy chi tiết quyết định loại evidence/actor nào có thể được ADMIN cấp cờ riêng trên asset. Metadata/bằng chứng đã che vẫn nằm trong Finding/Ticket response thông thường.

## 3. Kết quả kiểm tra tĩnh hồ sơ

| Mã | Kiểm tra | Kết quả thiết kế |
|---|---|---|
| API-QA-01 | OpenAPI dùng 3.1, `/api/v1`, OIDC, operationId duy nhất | Đạt |
| API-QA-02 | Mỗi operation có purpose/actor/scope/state/transaction/concurrency/audit/tests | Đạt qua `x-contract` |
| API-QA-03 | Mutation dùng If-Match và/hoặc Idempotency-Key phù hợp | Đạt thiết kế |
| API-QA-04 | Mọi UC có API hoặc internal server process | Đạt theo mục 2 |
| API-QA-05 | Cạnh workflow khớp enum/guard 27 bảng; không hard delete | Đạt sau các ghi chú mapping |
| API-QA-06 | Raw/report download tách riêng, reauthorize, no public object URL | Đạt |
| API-QA-07 | Ma trận role + asset + owner/task + sensitive flag rõ | Đạt |
| API-QA-08 | Success audit atomic; denial/failure audit độc lập | Đạt |
| API-QA-09 | Import, Close/Reopen, Risk Approval có sequence | Đạt |
| API-QA-10 | Không có API điều khiển ZAP/đa scanner/BI/email/ticket ngoài | Đạt |

Các dòng “Đạt” là kiểm tra nhất quán ở mức thiết kế, chưa phải bằng chứng implementation/runtime.

## 4. Contract/integration test bắt buộc khi lập trình

- Parse/validate OpenAPI; generated TypeScript client và ASP.NET endpoints không drift.
- Response `application/problem+json`, correlation ID, ETag, idempotent replay và cursor contract.
- Authorization negative theo mục 4 của ma trận; mọi Denied/Failure audit tồn tại và không lộ dữ liệu.
- PostgreSQL integration test thật cho mọi cạnh, rollback khi audit lỗi, lock order và stale version.
- Concurrent tests: duplicate Asset/import, append ticket cùng pointer, review/risk/verification terminal decisions.
- Corpus Traditional JSON/XML hợp lệ/lỗi/độc hại; size limit, zip/XML bomb/XXE theo parser thực tế, checksum và object reconciliation.
- Golden masking cho URL/query/header/cookie/token/Attack/Evidence trong UI API, export, log, audit, notification.
- Raw/export stream reauthorization giữa lúc grant/revoke, no-store/nosniff, checksum mismatch fail closed.

## 5. Cổng hoàn thành giai đoạn 8

- [x] Có OpenAPI 3.1 và danh mục lỗi thống nhất.
- [x] Mọi UC có API hoặc xử lý máy chủ tương ứng.
- [x] Mọi API thay đổi trạng thái có guard, giao dịch, concurrency và audit.
- [x] Có ma trận API–vai trò–tài sản–audit và negative tests.
- [x] Có sequence Import, Close/Reopen và Risk Approval.
- [x] Không trả object key/key reference/raw/evidence nhạy cảm trong API thông thường.
- [ ] Product Owner/Tech Lead review hợp đồng URL/payload và mapping UC.
- [ ] Chuyên gia ATTT review AuthZ, SoD, masking, raw/evidence access và audit.
- [ ] DBA/backend review lock order, transaction và mapping đúng 27 bảng.
- [ ] Chủ sở hữu policy chốt IdP mapping, upload/rate limits, masking, SLA và retention trước implementation tương ứng.

Vì bốn mục ký duyệt cuối cần con người có thẩm quyền, trạng thái hồ sơ là **baseline hoàn chỉnh, sẵn sàng review/phê duyệt**, không tự nhận là đã được phê duyệt.
