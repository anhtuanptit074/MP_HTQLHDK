# Hồ sơ kiến trúc hệ thống — Giai đoạn 6

| Thuộc tính | Giá trị |
|---|---|
| Hệ thống | Hệ thống quản lý quét lỗ hổng website định kỳ |
| Phiên bản hồ sơ | 1.0 |
| Ngày lập | 2026-09-07 |
| Trạng thái | Baseline kỹ thuật, sẵn sàng review/phê duyệt |
| Phạm vi | F-01 đến F-04 và các kiểm soát hỗ trợ trực tiếp |
| Nguồn nghiệp vụ | [`doi_tuong_sd_quy_trinh_hd.md`](../doi_tuong_sd_quy_trinh_hd.md), phiên bản 2.2 |
| Nguồn quy trình | [`quy_trinh_xay_dung_he_thong.md`](../quy_trinh_xay_dung_he_thong.md), mục 10 |

## 1. Kết quả giai đoạn 6

Bộ hồ sơ này chốt một kiến trúc có thể triển khai cho bốn chức năng đã duyệt, không bổ sung điều khiển OWASP ZAP, SSO, email, BI, ticket ngoài hoặc scanner khác. Kiến trúc dùng backend module hóa thay vì microservice; API và worker có thể chạy thành hai workload từ cùng release để cô lập tài nguyên và quyền.

| Sản phẩm bàn giao theo quy trình | Tài liệu đáp ứng |
|---|---|
| Sơ đồ kiến trúc logic và triển khai | [01 — Kiến trúc logic và triển khai](01_kien_truc_logic_va_trien_khai.md) |
| Danh sách quyết định và lý do | [02 — Quyết định kiến trúc](02_quyet_dinh_kien_truc.md) |
| Mô hình đe dọa sơ bộ | [03 — Mô hình đe dọa](03_mo_hinh_de_doa.md) |
| Quản lý cấu hình, secrets và tệp | [04 — Cấu hình, secrets và tệp](04_cau_hinh_secrets_va_quan_ly_tep.md) |
| Môi trường, dung lượng và backup | [05 — Môi trường, dung lượng và sao lưu](05_moi_truong_dung_luong_sao_luu.md) |
| Truy vết, kiểm tra và cổng hoàn thành | [06 — Truy vết và nghiệm thu](06_ma_tran_truy_vet_va_nghiem_thu.md) |

Nguồn sơ đồ có thể chỉnh sửa nằm trong [`diagrams/`](diagrams/). Bản SVG/PNG được dựng từ cùng nguồn nằm trong [`rendered/`](rendered/).

## 2. Baseline đã chốt

- Web UI: React 19.2 + TypeScript, build thành tệp tĩnh; package patch được khóa trong lockfile ở giai đoạn 10.
- Backend: ASP.NET Core trên .NET 10 LTS, REST API theo OpenAPI.
- Truy cập dữ liệu: EF Core 10/Npgsql 10; giao dịch quan trọng có SQL/lock tường minh và luôn dựa thêm vào constraint/guard PostgreSQL.
- Database: PostgreSQL 17, chạy minor mới nhất đã được kiểm thử trước khi phát hành; bộ SQL hiện tại trở thành migration baseline.
- Kiến trúc ứng dụng: modular monolith gồm F-01, F-02, F-03, F-04 và các module Identity/Authorization, Audit, File, Notification/Report dùng chung.
- Tác vụ nền: worker cùng release, claim công việc bền vững từ PostgreSQL; chưa dùng message broker.
- Tệp: kho object riêng tư tương thích S3 hoặc dịch vụ tương đương, mã hóa bằng KMS, không public bucket/object.
- Xác thực production: OIDC Authorization Code + PKCE với nhà cung cấp định danh hiện có của đơn vị; không xây kho mật khẩu/SSO mới.
- Triển khai: OCI container sau reverse proxy/WAF; database, object storage, KMS/secrets và telemetry không mở trực tiếp cho người dùng.
- Quan sát: OpenTelemetry, log có cấu trúc và metric kỹ thuật; không đưa Attack/Evidence/token/tệp thô vào log.

## 3. Điều chưa được phép coi là đã phê duyệt

Hồ sơ kỹ thuật đã hoàn chỉnh nhưng các giá trị sau vẫn cần chủ sở hữu chính sách hoặc hạ tầng của đơn vị ký xác nhận: nhà cung cấp IdP/KMS/object storage, hạn upload và tần suất, SLA, retention, masking policy, RPO/RTO, mức sẵn sàng, cửa sổ bảo trì, topology production và định mức tài nguyên. Nếu chọn OIDC như baseline, thiết kế database hiện tại còn phải bổ sung định danh `(issuer, subject)` bất biến trước khi lập trình; không ánh xạ bằng cách lower-case `sub` vào `username`. Tài liệu cung cấp vị trí cấu hình, công thức sizing, kiểm soát tối thiểu và tiêu chí xác minh; không tự điền giá trị nghiệp vụ.

## 4. Kết luận cổng giai đoạn

Các sản phẩm bắt buộc của giai đoạn 6 đã có và ba điều kiện kỹ thuật trong quy trình đều được đáp ứng ở mức thiết kế:

1. Database, tệp và khóa có phương án bảo vệ cụ thể.
2. Mỗi quy tắc BR-01 đến BR-15 đã có thành phần chịu trách nhiệm và cách kiểm chứng.
3. Runtime backend không dùng tài khoản owner/superuser hoặc tài khoản migration.

Trạng thái cuối là **sẵn sàng review/phê duyệt**, không tự thay thế biên bản phê duyệt của Tech Lead, ATTT và vận hành. Danh sách việc cần ký/chứng minh nằm ở [mục 4 của tài liệu nghiệm thu](06_ma_tran_truy_vet_va_nghiem_thu.md#4-cổng-hoàn-thành-giai-đoạn-6).
