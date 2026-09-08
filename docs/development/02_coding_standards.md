# Coding convention, lỗi và log an toàn

## Backend C#/.NET

- Nullable bật, warning là lỗi, C# 14 và file-scoped namespace.
- API DTO là allowlist; không bind trực tiếp entity database và không nhận actor/time/status do client tự đặt.
- Luồng I/O phải async và nhận `CancellationToken`; không `.Result`/`.Wait()`.
- Command nghiệp vụ đi qua authorization role + asset/task scope + trạng thái, transaction, audit rồi mới trả response.
- Giao dịch nhạy cảm tuân thủ lock order đã chốt; không giữ transaction khi upload/parse object.
- Lỗi công khai dùng Problem Details + mã lỗi trong `api_design/02_danh_muc_ma_loi.md`; không trả stack trace, SQL, object key hoặc trạng thái nội bộ.

## Frontend React/TypeScript

- TypeScript strict; không `any`, không tắt lint theo file để lách lỗi.
- Client API sinh/kiểm từ OpenAPI; không tự tạo kiểu request/response trái hợp đồng.
- Action UI kiểm role/state để giảm lỗi, nhưng không coi UI là cơ chế phân quyền.
- Nội dung ZAP, URL, Attack/Evidence luôn render như text đã encode/mask; không dùng `dangerouslySetInnerHTML`.
- Mỗi màn hình có loading, empty, error, denied/not-found an toàn, stale và trạng thái ngoại lệ tương ứng.
- Accessibility theo `ui_design/05_accessibility_responsive.md` và WCAG 2.2 AA.

## Format và lint

- `.editorconfig` là chuẩn chung; C# build với analyzers `latest-recommended` và warning-as-error.
- Frontend dùng ESLint flat config, Prettier và TypeScript compiler. Không merge khi bất kỳ bước nào lỗi.
- SQL dùng tên snake_case, từ khóa rõ ràng, migration bất biến và statement có mục đích audit/rollback rõ.

## Log/telemetry allowlist

Được phép: correlation/trace ID, mã action, outcome, status code, duration, record count, ID nội bộ đã được phép và mã lỗi an toàn.

Không được log: request/response body mặc định; full target URL/domain nếu không cần; parameter; Attack/Evidence; filename gốc; email; token/cookie/Authorization; checksum dùng làm access token; object key/key reference; connection string; secret; SQL parameter nhạy cảm.

Denied/failure phải audit bằng giao dịch độc lập theo thiết kế, nhưng thông điệp không được xác nhận sự tồn tại của tài nguyên ngoài scope.

