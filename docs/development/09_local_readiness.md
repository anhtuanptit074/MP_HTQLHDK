# Kết quả chạy thử local — 2026-10-05

Bản ứng dụng hiện hành dùng mô hình **13 bảng**, theo lựa chọn chuyển đổi của người dùng. Backend, frontend và worker đã có nghiệp vụ; dữ liệu cũ vẫn giữ trong database riêng.

| Thành phần | Kết quả xác minh |
|---|---|
| Runtime | Node 24.14.0, npm 11.9.0, .NET SDK 10.0.400, PowerShell 7.6.5 |
| PostgreSQL | 17.11, Docker Desktop/WSL2; kết nối local `127.0.0.1:54329` |
| Database hiện hành | `htqlhdk_core_local`, đúng 13 bảng nghiệp vụ |
| Migration mới | Core13 V001/V002, checksum khóa nội dung; chạy lại không sửa dữ liệu |
| Database cũ | `htqlhdk_local` được giữ; không chạy fixture cũ lên dữ liệu dùng thử |
| Backend/worker | Build Release đạt, 0 cảnh báo/lỗi; 31 kiểm thử đơn vị đạt |
| Frontend | Lint/format/build đạt; 7 kiểm thử đạt |
| Schema 13 bảng | 24 kiểm tra checksum, khóa và trigger đạt trên PostgreSQL nhúng |
| Nghiệp vụ HTTP | 13 nhóm kiểm thử xuyên suốt đạt trên `htqlhdk_core_test_local` |
| Worker | 1 kiểm thử tích hợp PostgreSQL đạt với runner mô phỏng; lập lịch một lần, nhập XML, xử lý hủy và mất heartbeat |
| Giao diện | Đăng nhập ATTT, danh sách/chi tiết phát hiện, biểu mẫu đánh giá và lựa chọn lịch ngày/tuần/tháng đã kiểm tra trong trình duyệt |
| Môi trường đang chạy | Frontend 5173, API 5000, readiness trả 200 Healthy; worker chạy với tự động quét tắt; khởi động lần hai không tạo trùng dịch vụ |
| Thiết kế cũ | 75 kiểm tra SQL và hồ sơ 57 API vẫn đạt; đây là kiểm tra tài liệu cũ, không phải số API mới |

Các nhóm HTTP xác minh đăng nhập/cookie/CSRF, khóa và thu hồi phiên, phân quyền theo tài sản/nhiệm vụ, phê duyệt độc lập, chống ghi đè phiên bản cũ, nhập báo cáo nguyên tử, báo cáo gốc bất biến, che dữ liệu, bằng chứng mã hóa, khắc phục/xác minh, đóng/mở lại, chuyển người sở hữu, hủy quét, xuất CSV an toàn và audit trước/sau.

Đã chạy đạt toàn bộ `scripts/test.ps1 -IncludeDatabase` sau các thay đổi cuối. Kiểm tra chỉ đọc xác nhận database cũ vẫn có 30 bảng; database dùng thử mới vẫn có hai tài sản mẫu. Ảnh giao diện local lưu tại `.codex_tmp/core13-preview.png`.

## Dùng thử

- Khởi động bằng `start-local.cmd`, mở [frontend](http://127.0.0.1:5173/).
- [API](http://127.0.0.1:5000/) và [readiness](http://127.0.0.1:5000/health/ready) kiểm tra ứng dụng/database.
- Mật khẩu mẫu nằm ở `.codex_tmp/local-accounts.txt`; tài khoản gồm `admin`, `owner`, `owner2`, `security`, `operator`, `remediator`, `auditor`. Tài khoản `svc-zap` không đăng nhập giao diện.
- Dừng bằng `start-local.cmd -Stop`; dữ liệu database được giữ.
- Xem [hướng dẫn chức năng và kiểm thử](10_core13_application.md).

## Phạm vi đã kiểm tra

Đã kiểm tra với dữ liệu giả và runner mô phỏng. Chưa chạy Docker ZAP quét website thực; tự động quét mặc định tắt. Chưa xác minh triển khai production, hệ thống xác thực doanh nghiệp, backup/khôi phục khóa mã hóa hoặc sức chịu tải lớn. Các kết quả đạt ở trên xác nhận bản chạy thử local và phạm vi nghiệp vụ 13 bảng, không thay thế kiểm thử các điều kiện triển khai đó.
