# Danh sách quyết định kiến trúc

## 1. Quy ước trạng thái

- **Accepted**: baseline để các giai đoạn 8–15 triển khai; thay đổi phải có ADR thay thế và đánh giá tác động.
- **Pending policy**: khung kỹ thuật đã chốt nhưng giá trị/nhà cung cấp cần đơn vị phê duyệt.
- **Superseded**: chỉ dùng khi có ADR mới thay thế, không xóa lịch sử.

## 2. Bảng quyết định

| Mã | Quyết định | Trạng thái | Lý do chính | Hệ quả/đánh đổi |
|---|---|---|---|---|
| ADR-001 | Modular monolith; API và worker cùng codebase/release | Accepted | Bốn chức năng liên kết chặt bởi transaction/workflow; giảm vận hành so với microservice | Cần ranh giới module và review truy cập chéo; scale API/worker vẫn độc lập |
| ADR-002 | React 19.2 + TypeScript cho Web UI | Accepted | UI workflow phong phú, typed client từ OpenAPI, hệ sinh thái ổn định | Thêm chuỗi supply-chain frontend; cần CSP, lockfile và dependency scanning |
| ADR-003 | ASP.NET Core/.NET 10 LTS cho API và worker | Accepted | LTS, streaming, auth/authorization và background service tích hợp | Nhóm phải chuẩn hóa C#/.NET; patch đều đặn |
| ADR-004 | PostgreSQL 17 là source of truth duy nhất | Accepted | Khớp schema 27 bảng đã kiểm thử; constraint/transaction mạnh | Không thay bằng NoSQL; nâng major cần kế hoạch riêng |
| ADR-005 | EF Core/Npgsql cho CRUD; SQL tường minh cho lock/bulk/critical query | Accepted | Giữ năng suất nhưng không che khuất giao dịch khó | Cần integration test PostgreSQL thật, tránh giả lập in-memory |
| ADR-006 | Migration bundle .NET chứa script SQL bất biến, checksum và advisory lock | Accepted | Tái sử dụng baseline SQL, có artifact triển khai và lịch sử phiên bản | Không sửa migration đã chạy; cần backup/forward-fix trước production |
| ADR-007 | Object storage private + KMS, metadata ở PostgreSQL | Accepted | Tệp lớn/nhạy cảm không phù hợp lưu byte trong DB; tách backup/scale | PostgreSQL và object storage không atomic; bắt buộc staging/finalize/reconciliation |
| ADR-008 | OIDC Authorization Code + PKCE với IdP hiện có; không có password store production | Pending policy | Không xây SSO ngoài phạm vi, tận dụng lifecycle/MFA của đơn vị | Cần chốt issuer/client/claim và bổ sung ánh xạ `(issuer, subject)` vào database; dev identity không dùng prod |
| ADR-009 | Quyền phía server theo permission + asset + task; default deny | Accepted | Thực thi BR-12/13 và ma trận bảy vai trò | Cache quyền tối đa trong một request; grant bị thu hồi có hiệu lực ở request kế tiếp |
| ADR-010 | Worker dùng bảng nghiệp vụ PostgreSQL làm durable work queue | Accepted | Không thêm broker khi tải/HA chưa chứng minh nhu cầu | Cần claim idempotent, `SKIP LOCKED`, retry/backoff; xem lại nếu benchmark không đạt |
| ADR-011 | Audit success cùng transaction; denial/failure ở transaction độc lập | Accepted | Không mất audit do rollback nhưng vẫn giữ atomicity với thay đổi thành công | Audit storage lỗi khiến action nhạy cảm fail closed; cần alert và test fault injection |
| ADR-012 | Không public/presign mặc định; download stream qua API | Accepted | Kiểm quyền/audit được thực hiện ở từng lượt | API chịu băng thông; presigned URL ngắn hạn chỉ là phương án có kiểm soát |
| ADR-013 | Cấu hình không bí mật typed/validated; secret từ Secrets Manager qua workload identity | Accepted | Ngăn secret vào repo/image/log, phát hiện cấu hình thiếu lúc startup | Phụ thuộc nền tảng secrets/KMS do đơn vị chọn |
| ADR-014 | Container OCI sau reverse proxy/WAF; private data plane | Accepted | Di động giữa on-prem/cloud, tách vùng mạng và hardening thống nhất | Nền tảng orchestration cụ thể chờ hạ tầng; không mặc định Kubernetes |
| ADR-015 | OpenTelemetry cho log/metric/trace kỹ thuật đã lọc | Accepted | Chuẩn mở, không khóa vendor, hỗ trợ correlation | Cần allowlist field và kiểm log-leak; không dùng telemetry thay audit |
| ADR-016 | UTC trong DB/log; timezone IANA lưu ở lịch nghiệp vụ | Accepted | Tránh lệch DST/môi trường; giữ đúng ý nghĩa lịch | UI phải chuyển đổi rõ ràng và test DST/timezone |
| ADR-017 | REST JSON `/api/v1`, OpenAPI 3.1, optimistic concurrency | Accepted | Phù hợp giai đoạn 8; dễ kiểm quyền và test | Mutation dùng `If-Match`/row version, lỗi xung đột rõ; upload dùng multipart/stream |
| ADR-018 | Không RLS ở baseline; authorization bắt buộc tại API + FK/guard DB | Accepted có điều kiện | Bộ DB hiện chưa có RLS; tránh tuyên bố bảo vệ chưa tồn tại | Negative test bắt buộc; đánh giá RLS lại trước go-live nếu threat/risk yêu cầu |
| ADR-019 | Provider-neutral cho object/KMS/IdP/telemetry | Pending policy | Chưa có quyết định hạ tầng của đơn vị | Adapter phải hẹp; không dùng tính năng độc quyền nếu chưa có ADR |
| ADR-020 | Không message broker, Redis, full-text engine hoặc service mesh ở release đầu | Accepted | Chưa có nhu cầu đã đo; giảm bề mặt tấn công/vận hành | Có tiêu chí xem lại ở mục 4 |

## 3. Quyết định migration cụ thể

1. Chuyển ba tệp hiện tại thành baseline migration `V001` theo đúng thứ tự schema → guards/views → reference data; lưu checksum artifact.
2. Migration bundle là executable/release artifact riêng, chạy bằng `db_migrator` và advisory lock; API/worker không auto-migrate khi startup.
3. Mỗi migration mới chỉ tiến tới, có review SQL và test trên bản sao schema. Không sửa file đã áp dụng.
4. Thay đổi tương thích dùng expand/contract. Dữ liệu backfill dài chạy job có checkpoint, không giữ transaction/lock dài.
5. Rollback production ưu tiên restore/forward-fix theo runbook, không cung cấp down migration phá dữ liệu giả tạo.
6. Lịch sử migration kỹ thuật ghi version, checksum, thời điểm, release và kết quả; không chứa secret.

## 4. Ngưỡng xem lại kiến trúc

Các quyết định không được thay đổi chỉ vì “có thể cần”. Mở ADR đánh giá broker/cache/search/microservice khi có bằng chứng ít nhất một trong các tình huống:

- Worker polling hoặc lock contention không đạt SLO đã duyệt qua load test.
- Một module cần scale/deploy/availability khác biệt và không thể đạt bằng tách workload hiện tại.
- Khối lượng export/search đã đo không đạt bằng index/query PostgreSQL sau tối ưu.
- Yêu cầu tích hợp bất đồng bộ ngoài hệ thống được phê duyệt thành phạm vi mới.
- Threat model hoặc quy định buộc cô lập tiến trình/credential mạnh hơn.

Nếu tách service, phải giải quyết lại ownership dữ liệu, audit, authorization, idempotency, outbox và giao dịch; không dùng shared database như microservice giả.

## 5. Phương án đã cân nhắc nhưng không chọn

| Phương án | Lý do chưa chọn |
|---|---|
| Microservice theo F-01–F-04 | Giao dịch và dữ liệu liên kết dày; tăng secret/network/deploy/quan sát nhưng chưa có nhu cầu scale độc lập đã đo |
| Lưu tệp bytea trong PostgreSQL | Làm phình backup/WAL, khó tách retention/durability và streaming |
| Cho trình duyệt upload thẳng bằng credential/presigned URL dài | Khó kiểm size/quyền/audit thống nhất, tăng rủi ro lộ URL/token |
| Điều khiển ZAP qua API | Ngoài phạm vi BR-02 |
| Dựa hoàn toàn vào role claim của IdP | Không biểu diễn asset/task grant và quyền nhạy cảm riêng, thu hồi không đồng bộ với DB |
| Cache quyền dài hạn | Làm chậm thu hồi quyền; authorization DB trong mỗi request là baseline an toàn |
| Auto-migrate khi API khởi động | Runtime cần DDL và nhiều replica có thể tranh migration; khó kiểm soát cửa sổ deploy |
| RLS được coi là lớp quyền duy nhất | Chưa có policy RLS và vẫn không bao phủ object storage/workflow/SoD |

## 6. Cách thay đổi quyết định

ADR mới phải ghi động lực, phương án, ảnh hưởng tới BR/UC/bảng/API/UI/test/triển khai, kế hoạch chuyển đổi và người phê duyệt Tech Lead + ATTT + vận hành khi liên quan. Không sửa lịch sử quyết định Accepted để tạo cảm giác thiết kế chưa từng thay đổi.
