# Ma trận truy vết và nghiệm thu giai đoạn 6

## 1. Truy vết quy tắc nghiệp vụ đến thành phần

| Quy tắc | Thành phần chịu trách nhiệm chính | Lớp kiểm soát bổ sung | Bằng chứng dự kiến |
|---|---|---|---|
| BR-01 Asset/scope duyệt mới lập lịch | Asset + Scheduling API/worker | FK/guard PostgreSQL | Integration + transition tests |
| BR-02 ZAP ở ngoài | UI hướng dẫn + Scan workflow | Network/architecture không có ZAP connector | Architecture/API inventory review |
| BR-03 Chỉ ZAP JSON/XML | Import API/worker | Type/size/parser sandbox | Malicious/valid parser corpus |
| BR-04 Truy vết nguồn | Import/Finding modules | Composite FK + transaction | DB/integration trace test |
| BR-05 Raw bất biến/checksum | File service + object policy | Immutable metadata trigger/KMS | Tamper/object-policy test |
| BR-06 Chống trùng | Import worker | Unique keys + idempotency | Concurrent duplicate test |
| BR-07 Tái xuất hiện | Finding worker/domain | State guard + occurrence FK/audit | Closed→Reopened test |
| BR-08 Risk/Confidence/Final Severity riêng | Import + Finding review | Schema types/columns | Mapping/review test |
| BR-09 Severity có lý do/lịch sử | Finding review | Immutable review/current pointer/audit | SoD/concurrency test |
| BR-10 Remediator không tự đóng | Remediation/Verification | Actor/FK/guard/authorization | Negative test |
| BR-11 Risk độc lập/có hạn | Risk module + expiry worker | DB checks/immutable decision/audit | Requester≠approver + expiry test |
| BR-12 Mask bí mật | Masking policy + UI/File/Export | CSP/no-store/log allowlist | Golden masking/log-leak test |
| BR-13 Audit truy cập/chuyển trạng thái | Audit writer | Immutable table + separate TX | Rollback/fault injection test |
| BR-14 Không xóa cứng lịch sử | Domain state transitions | DELETE/TRUNCATE guards + object deny delete | SQL/object negative test |
| BR-15 Không mở rộng phạm vi | Module/API catalog + ADR review | Traceability gate | Review không có ZAP control/BI/email/ticket ngoài |

## 2. Truy vết use case/module

| Nhóm UC | Module API | Worker/job | Kho dữ liệu/tệp |
|---|---|---|---|
| UC-A01–A07 | Asset, Authorization | Không bắt buộc ở F-01 | Asset/scope/window/user_role |
| UC-S01–S08 | Scheduling, Authorization | Sinh kỳ/nhắc Missed | schedule/occurrence/notification |
| UC-I01–I12 | Import, File, Authorization | Validate/parse/reconcile | batch/error/raw/finding source |
| UC-T01–T06 | Finding Review, Masking | Thông báo theo policy | finding/review/evidence/audit |
| UC-R01–R13 | Remediation, Verification | Deadline/verification notification | ticket/event/request/result/evidence |
| UC-RA01–RA07 | Risk | Expiry/notification | risk/finding/audit/notification |
| UC-C01–C10 | Authorization, Audit, File/Export | Export/reconciliation/notification | user_role/audit/export/object |

Chi tiết UC → sequence → bảng giữ nguyên tại [`sequence_diagrams/01_ma_tran_truy_vet.md`](../sequence_diagrams/01_ma_tran_truy_vet.md).

## 3. Kiểm tra tính nhất quán hồ sơ

| ID | Kiểm tra | Kết quả 2026-09-07 |
|---|---|---|
| ARC-QA-01 | Có đủ 5 nhóm sản phẩm bàn giao của mục 10 quy trình | Đạt |
| ARC-QA-02 | Sơ đồ nguồn PlantUML biên dịch được sang SVG/PNG | Đạt |
| ARC-QA-03 | Link Markdown nội bộ trong `architecture/` resolve được | Đạt |
| ARC-QA-04 | Kiến trúc chỉ bao phủ F-01–F-04 và hỗ trợ trực tiếp | Đạt |
| ARC-QA-05 | Không có thành phần điều khiển ZAP | Đạt |
| ARC-QA-06 | Raw/evidence/export private, mã hóa, audit, không URL public dài hạn | Đạt thiết kế |
| ARC-QA-07 | Role DB runtime tách migrator/audit/backup và không có quyền vô hạn | Đạt thiết kế |
| ARC-QA-08 | Audit rollback, object/DB không atomic, job idempotency đã có phương án | Đạt thiết kế |
| ARC-QA-09 | Mọi giá trị chính sách chưa có được ghi “chờ” và có owner | Đạt |
| ARC-QA-10 | BR-01–BR-15 có owner component và evidence dự kiến | Đạt |

“Đạt thiết kế” không có nghĩa code/config production đã được kiểm thử. Bằng chứng triển khai được tạo ở giai đoạn 10–15.

## 4. Cổng hoàn thành giai đoạn 6

### 4.1. Hoàn tất trong hồ sơ

- [x] Kiến trúc logic, container, module và deployment boundary.
- [x] Công nghệ, version major/support line và chiến lược migration.
- [x] Kho tệp private, streaming, checksum, KMS, finalize/reconciliation.
- [x] Authentication/authorization/SoD và tài khoản database tối thiểu.
- [x] Worker/job/idempotency/audit rollback.
- [x] Threat model 32 mối đe dọa + abuse cases + residual risk.
- [x] Cấu hình/secrets/key rotation.
- [x] Môi trường, capacity formula, backup/restore/rollback.
- [x] Truy vết BR và use case đến thành phần/test.

### 4.2. Chữ ký/quyết định bên ngoài còn cần

| Cổng | Người xác nhận | Điều kiện |
|---|---|---|
| Baseline kiến trúc | Tech Lead/Kiến trúc sư | Chấp nhận ADR-001–020 và technology ownership |
| Baseline bảo mật | Chuyên gia ATTT | Chấp nhận trust boundaries, P0 controls, residual risk/RLS decision |
| Baseline vận hành | DevOps/Vận hành | Chọn topology/provider và xác nhận khả năng KMS/object/backup/telemetry |
| Chính sách nghiệp vụ | Đại diện nghiệp vụ/chủ nhiệm | SLA, retention, masking, risk authority, notification horizon |
| NFR | Chủ nhiệm + vận hành + ATTT | SLO, availability, RPO/RTO, maintenance, upload quota |
| Ánh xạ định danh | Database Engineer + Tech Lead + ATTT | Bổ sung `(issuer, subject)` bất biến hoặc phương án tương đương, cập nhật schema/từ điển/ERD/test |

Không đánh dấu các ô này là hoàn thành nếu chưa có biên bản/tên người/ngày/phiên bản. Sau chữ ký, cập nhật trạng thái README từ “sẵn sàng review” thành “Approved” và ghi decision log; không sửa nội dung lịch sử lặng lẽ.

## 5. Đầu vào bắt buộc cho giai đoạn kế tiếp

Giai đoạn 7 database đã có baseline 27 bảng. Để sang giai đoạn 8, đặc tả API F-01 phải dùng:

1. OIDC `(issuer, subject)` → `app_user` sau khi xử lý database gap, permission + asset authorization và error/audit contract chung.
2. `/api/v1`, OpenAPI 3.1, optimistic concurrency (`If-Match`/row version) và idempotency khi phù hợp.
3. Giao dịch và lock order của [`database/04_rang_buoc_va_giao_dich.md`](../database/04_rang_buoc_va_giao_dich.md).
4. Command DTO allowlist; actor/time/status server-set; response masking và safe error.
5. Negative tests role × asset × state và SoD; không chỉ test happy path.
6. Không cấp object key, database ID nhạy cảm hoặc raw file URL ra khỏi endpoint chuyên biệt.

## 6. Tiêu chí đổi kiến trúc

Mọi thay đổi module boundary, database, object flow, auth, audit, worker queue hoặc deployment phải có ADR mới và cập nhật đồng thời threat model, sơ đồ, API, migration, test và runbook. Thay đổi nhà cung cấp mà giữ nguyên hợp đồng adapter/policy có thể là quyết định triển khai; nếu làm yếu control, đó là thay đổi kiến trúc và cần ATTT duyệt.
