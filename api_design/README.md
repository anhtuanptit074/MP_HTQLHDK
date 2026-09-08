# Hồ sơ thiết kế API và giao dịch nghiệp vụ — Giai đoạn 8

| Thuộc tính | Giá trị |
|---|---|
| Hệ thống | Hệ thống quản lý quét lỗ hổng website định kỳ |
| Phiên bản hồ sơ | 1.0 |
| Ngày lập | 2026-09-07 |
| Trạng thái | Baseline thiết kế, sẵn sàng review/phê duyệt |
| Phạm vi | F-01 đến F-04 và kiểm soát hỗ trợ trực tiếp |
| API baseline | REST JSON `/api/v1`, OpenAPI 3.1 |
| Nguồn nghiệp vụ | [`doi_tuong_sd_quy_trinh_hd.md`](../doi_tuong_sd_quy_trinh_hd.md), phiên bản 2.2 |
| Nguồn dữ liệu | [`database/`](../database/README.md), 27 bảng |
| Nguồn kiến trúc | [`architecture/`](../architecture/README.md), ADR-001 đến ADR-020 |

## 1. Sản phẩm bàn giao

| Sản phẩm bắt buộc | Tài liệu đáp ứng |
|---|---|
| OpenAPI/API specification | [01 — OpenAPI 3.1](01_openapi.yaml) |
| Danh mục mã lỗi nghiệp vụ | [02 — Mã lỗi](02_danh_muc_ma_loi.md) |
| Hợp đồng giao dịch cho mọi cạnh workflow | [03 — Hợp đồng giao dịch](03_hop_dong_giao_dich.md) |
| Ma trận API–vai trò–tài sản–audit | [04 — Ma trận quyền và audit](04_ma_tran_api_quyen_audit.md) |
| Sequence diagram Import, Close/Reopen, Risk Approval | [05 — Sequence diagram](05_sequence_diagrams.md) |
| Truy vết UC, kiểm tra và cổng hoàn thành | [06 — Truy vết và nghiệm thu](06_truy_vet_va_nghiem_thu.md) |
| Mẫu request/response trọng yếu | [07 — Mẫu request/response](07_mau_request_response.md) |

## 2. Quy ước bắt buộc

- OIDC chỉ xác thực danh tính. Quyền nghiệp vụ được đọc lại từ `user_role` trong mỗi request và mặc định từ chối.
- `asset_id = NULL` không phải wildcard. OWNER phải là chủ tài sản; REMEDIATOR còn phải có phiếu hiện hành được giao.
- API thông thường chỉ trả dữ liệu đã che. Raw report, evidence nhạy cảm và file export chỉ được stream qua endpoint riêng sau kiểm quyền và ghi audit.
- Mutation có thể lặp lại dùng `Idempotency-Key`; mutation trên tài nguyên có `row_version` dùng `If-Match: \"<row_version>\"`.
- Mọi response có `X-Correlation-ID`; client có thể gửi giá trị UUID hợp lệ, nếu không máy chủ sinh mới.
- Danh sách dùng cursor ổn định (`page[after]`, `page[size]`); không dùng offset cho worklist thay đổi liên tục.
- Audit thành công nằm cùng giao dịch nghiệp vụ. Audit Denied/Failure nằm trong giao dịch độc lập. Không ghi token, tệp thô, Attack/Evidence hay bí mật vào audit/log/thông báo.
- Thời gian API là RFC 3339 UTC; lịch giữ timezone IANA. Giá trị policy chưa duyệt phải đến từ cấu hình đã được phê duyệt.
- Không có endpoint khởi chạy/dừng/điều khiển OWASP ZAP.

## 3. Ranh giới OpenAPI

OpenAPI mô tả hợp đồng HTTP công khai giữa Web UI và backend. Các UC `include` nội bộ như kiểm schema ZAP, masking, fingerprint, chống trùng, deadline và expiry được ánh xạ tới xử lý máy chủ/worker trong tài liệu truy vết; chúng không bị biến thành endpoint có thể gọi để bỏ qua giao dịch cha.

Các extension `x-*` trong từng operation là phần hợp đồng của dự án:

- `x-use-cases`: UC/BR được phục vụ.
- `x-permissions`: permission bắt buộc; luôn kết hợp asset/task scope.
- `x-asset-scope`: cách chống IDOR và giới hạn dữ liệu.
- `x-state-transition`: trạng thái nguồn/đích và guard.
- `x-transaction`: khóa, dữ liệu ghi và thứ tự.
- `x-audit`: action/outcome/căn cứ phải lưu.
- `x-test-cases`: ca kiểm thử tối thiểu.

## 4. Các quyết định còn chờ phê duyệt

Nhà cung cấp IdP/KMS/object storage, giới hạn upload, rate limit, masking policy chi tiết, SLA, retention và RPO/RTO vẫn là cấu hình/policy chờ đơn vị ký. Trước lập trình phải bổ sung ánh xạ OIDC bất biến `(issuer, subject)` cho `app_user` như hồ sơ kiến trúc đã nêu; không ánh xạ bằng username hoặc email.

## 5. Cách kiểm tra hồ sơ

1. Parse `01_openapi.yaml` bằng parser YAML/OpenAPI 3.1 trong CI.
2. Kiểm `operationId` duy nhất, mọi operation có security, response lỗi chuẩn, `x-use-cases`, quyền, phạm vi, audit và test.
3. Đối chiếu mọi UC trong `tai_lieu_use_case/README.md` với mục 2 của tài liệu truy vết.
4. Review các cạnh trạng thái với enum/guard trong `database/sql/01_schema.sql` và `02_guards_and_views.sql`.
5. Chạy contract, authorization-negative, transaction rollback và concurrent integration test trên PostgreSQL thật ở giai đoạn 11–12.
