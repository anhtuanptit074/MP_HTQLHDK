# HTQLHDK — quản lý quét lỗ hổng website định kỳ

Ứng dụng React/TypeScript, ASP.NET Core/.NET 10 và PostgreSQL 17 đã triển khai nghiệp vụ theo **mô hình tinh gọn 13 bảng**: tài sản và phạm vi, hồ sơ ZAP, kế hoạch/lần quét, nhập báo cáo, phân loại phát hiện, khắc phục, xác minh, phân quyền và nhật ký.

## Chạy thử

1. Mở Docker Desktop.
2. Nếu chưa cấu hình, sao chép `.env.local.example` thành `.env.local`, đặt hai mật khẩu database khác nhau. Các công cụ cần có: Node.js 24+, .NET SDK 10.0.400 và Docker.
3. Nếu vừa tải repository, chạy `scripts/test.ps1` để cài phụ thuộc và biên dịch.
4. Chạy `start-local.cmd` tại thư mục gốc. Script tạo/migrate database 13 bảng, chuẩn bị tài khoản mẫu, khởi động API, frontend và worker.
5. Mở [ứng dụng local](http://127.0.0.1:5173/). Tài khoản và mật khẩu mẫu được lưu riêng trong `.codex_tmp/local-accounts.txt`; không có mật khẩu cố định trong mã nguồn.

Dừng bằng `start-local.cmd -Stop`. PostgreSQL và dữ liệu tiếp tục được giữ. Sau khi sửa mã backend, dừng dịch vụ trước khi biên dịch lại.

Worker sẵn sàng nhưng **tự động quét ZAP mặc định tắt**. Có thể nhập báo cáo JSON/XML từ lần quét ngoài để dùng thử đầy đủ luồng nghiệp vụ. Chỉ bật `start-local.cmd -EnableZap` sau khi khai báo mục tiêu thử nghiệm, phạm vi, khung giờ và hồ sơ được duyệt. Chi tiết trong [hướng dẫn bản 13 bảng](docs/development/10_core13_application.md).

## Nghiệp vụ và quyền

| Vai trò | Công việc |
|---|---|
| Chủ tài sản | Khai báo, xem tài sản sở hữu, xác nhận phạm vi; được suy ra từ người sở hữu tài sản |
| ADMIN | Tài khoản, cấp/thu hồi quyền, nhật ký quyền và đăng nhập |
| SECURITY | Phê duyệt phạm vi và kế hoạch tài sản phụ trách, đánh giá mức độ, phân công, kết luận xác minh, truy cập báo cáo gốc có lý do |
| SCAN_OPERATOR | Hồ sơ quét, kế hoạch/lần quét được giao, nhập báo cáo và kết quả nguồn thuộc nhiệm vụ |
| REMEDIATOR | Phát hiện/phiếu được giao, tiến độ, biện pháp và bằng chứng khắc phục |
| AUDITOR | Đọc dữ liệu đã che và nhật ký trong tài sản được cấp quyền |

ADMIN không tự có quyền đọc dữ liệu lỗ hổng hoặc báo cáo gốc. Quyền có `asset_id=NULL` chỉ thể hiện điều kiện nhận nhiệm vụ, không mở quyền trên mọi tài sản. Kiểm tra quyền và phiên bản dữ liệu được thực hiện lại tại API cho mỗi yêu cầu.

Chỉ đóng phát hiện sau khắc phục khi báo cáo quét xác minh đã xử lý thành công, fingerprint không còn xuất hiện và người phê duyệt độc lập xác nhận coverage. Phát hiện xuất hiện lại trong lần quét sau sẽ được mở lại.

## Kiểm thử và dữ liệu

- `scripts/test.ps1`: biên dịch, kiểm thử backend/frontend, kiểm tra thiết kế cũ và ràng buộc schema 13 bảng.
- `scripts/test.ps1 -IncludeDatabase`: thêm các luồng HTTP và worker mô phỏng trên **database riêng** `htqlhdk_core_test_local`.
- `scripts/test-core13.ps1`: chạy riêng kiểm thử HTTP/worker sau khi đã biên dịch.
- Database dùng thử: `htqlhdk_core_local`, đúng 13 bảng trong schema `htqlhdk`.
- Database cũ `htqlhdk_local` được giữ nguyên. Bản mới dùng dữ liệu mẫu riêng; không tự chuyển đổi dữ liệu từ mô hình 30 bảng.

## Mã nguồn và tài liệu

| Thư mục | Nội dung |
|---|---|
| `src/backend/Htqlhdk.Core` | Nghiệp vụ, quyền, parser ZAP, mã hóa và lịch quét |
| `src/backend/Htqlhdk.Api` | Đăng nhập, cookie/CSRF, API nghiệp vụ và health check database |
| `src/backend/Htqlhdk.Worker` | Lập lịch, thực thi Docker ZAP, heartbeat/hủy và nhập báo cáo |
| `src/frontend` | Màn hình tiếng Việt và thao tác theo quyền |
| `ops/core-migrations` | Migration 13 bảng, checksum và kiểm thử ràng buộc |
| `ops/migrations`, `api_design`, `ui_design` | Hồ sơ và migration của thiết kế trước; giữ để tham khảo |
| `tests/core13` | Kiểm thử nghiệp vụ và phân quyền qua HTTP |

Không commit `.env.local`, khóa mã hóa, cookie, mật khẩu mẫu, báo cáo gốc hoặc dữ liệu lỗ hổng thật. Migration đã áp dụng được giữ bất biến. Đọc [báo cáo chạy thử](docs/development/09_local_readiness.md) và [thiết kế 13 bảng](database/08_thiet_ke_3nf_tinh_gon.md).
