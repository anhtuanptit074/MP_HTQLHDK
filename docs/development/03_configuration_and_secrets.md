# Cấu hình và secrets

## 1. Thứ tự nguồn cấu hình

1. Giá trị mặc định không nhạy cảm trong mã/appsettings.
2. File cấu hình theo môi trường chỉ chứa giá trị không nhạy cảm.
3. Biến môi trường/workload configuration cho endpoint và feature flag.
4. Secret manager/KMS qua workload identity cho password, client secret, key và credential object storage.

Production không dùng `.env`, user-secrets hay secret dài hạn trong pipeline. `.env.local` chỉ dành cho máy cá nhân và bị `.gitignore` loại trừ.

## 2. Quy tắc

- Tên biến bắt đầu `HTQLHDK_`; không đặt secret trong `VITE_*` vì mọi giá trị frontend đều công khai.
- Mỗi môi trường có identity, database account, object credential và encryption key riêng.
- Tách tài khoản migrator khỏi API/worker; runtime không có quyền owner/superuser/DDL.
- Secret không xuất hiện trong exception, command line lưu lịch sử, log build, test snapshot hoặc báo cáo scan.
- Rotation phải hỗ trợ gối đầu hai phiên bản khi nền tảng cho phép; thu hồi bản cũ sau smoke test.
- KMS/key reference chỉ ở backend/metadata cần thiết và không trả qua API thông thường.

## 3. Cổng triển khai

Trước staging/production phải có: mapping tên cấu hình–nguồn secret–owner; bằng chứng workload identity; test secret thiếu/hết hạn/thu hồi; log sample không lộ dữ liệu; kế hoạch rotation; và phê duyệt policy upload/SLA/retention. Không dùng giá trị local làm mặc định production.

## 4. Khi nghi lộ secret

Không chỉ xóa khỏi commit. Phải thu hồi/rotate ngay, kiểm audit, xác định phạm vi, làm sạch lịch sử theo quy trình kho mã nguồn và ghi incident. Không dán secret vào issue hoặc pull request.

