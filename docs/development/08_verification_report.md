# Báo cáo xác minh baseline giai đoạn 10

| Thuộc tính | Giá trị |
|---|---|
| Ngày chạy | 2026-09-08 |
| Trạng thái | PASS cục bộ; còn cổng quản trị repository dùng chung |
| Node/npm | 24.14.0 / 11.9.0 |
| .NET SDK dùng build | 10.0.400 |
| Database artifact engine | PostgreSQL 17.5 nhúng |
| PostgreSQL mục tiêu CI/local | 17.11 Alpine 3.24, khóa OCI index digest |

## Kết quả

| Cổng | Kết quả |
|---|---|
| API design | 57 operation duy nhất, 57 contract, 63 use case được ánh xạ |
| UI design | 32 màn hình, đủ mapping 57 operation |
| Database artifact | 75/75 kiểm tra đạt |
| Migration V001 | SHA-256 đúng; áp dụng từ database sạch; lần hai no-op; đủ 27 bảng |
| PostgreSQL native | PostgreSQL 17.11 Alpine 3.24 đúng OCI digest; container healthy; migration/verify đạt |
| Fixture native | Nạp thành công và chạy lặp an toàn trên database `_local`; trigger chống xóa được bật lại trước khi nạp |
| Backend | Restore/build Release đạt, 0 warning, 0 error |
| Frontend lint/format | Đạt |
| Frontend test | 1 file, 3 test đạt |
| Frontend production build | Đạt |
| npm production audit | 0 vulnerability tại thời điểm chạy |
| Secret/artifact scan | Không thấy pattern secret độ tin cậy cao hoặc raw scanner/session ngoài fixture |

## Cổng quản trị repository dùng chung

Kiểm thử native cục bộ đã hoàn tất. `psql` trên Windows không phải điều kiện bắt buộc vì wrapper chạy client bên trong container PostgreSQL. Pipeline `database` vẫn phải đạt ở lần chạy GitHub Actions đầu tiên để lưu bằng chứng độc lập cho PostgreSQL 17.11.

Repository hiện chưa có remote. Branch protection, secret scanning của nền tảng và reviewer bắt buộc chỉ có thể bật sau khi repository được tạo và đẩy lên GitHub/GitLab; đây là bước quản trị cần tài khoản đích và quyền quản trị repository.
