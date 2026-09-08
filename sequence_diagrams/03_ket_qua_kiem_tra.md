# Kết quả kiểm tra bộ Sequence Diagram

Ngày kiểm tra: 05/09/2026. Trạng thái: **PASS**.

## 1. Kết quả tự động

| STT | Hạng mục | Kết quả | Chi tiết |
|---:|---|---|---|
| 1 | Danh mục sequence | Đạt | 19 mục, 19 mã duy nhất |
| 2 | Đặc tả | Đạt | 19/19 tệp |
| 3 | PlantUML | Đạt | 19/19 tệp |
| 4 | Mermaid | Đạt | 19/19 tệp |
| 5 | PNG | Đạt | 19/19 tệp |
| 6 | SVG | Đạt | 19/19 tệp |
| 7 | Cú pháp PlantUML | Đạt | 19/19 hợp lệ |
| 8 | Tham chiếu SD | Đạt | không có mã lạ |
| 9 | Cấu trúc Mermaid | Đạt | 19/19 qua bộ kiểm tra tĩnh |
| 10 | Bao phủ Use Case | Đạt | 63/63 UC; thiếu []; lạ [] |
| 11 | Bao phủ bảng dữ liệu | Đạt | 27/27 bảng; thiếu []; lạ [] |
| 12 | Liên kết nội bộ | Đạt | tất cả tồn tại |
| 13 | Ảnh PNG | Đạt | 19 ảnh hợp lệ; min 573x1368, max height 3598 |
| 14 | Ảnh không có cảnh báo PlantUML | Đạt | không có banner cảnh báo |
| 15 | Ranh giới OWASP ZAP | Đạt | chỉ Ops trao đổi với ZAP bên ngoài |

Tổng: **15/15 hạng mục đạt**. Chi tiết máy đọc: [validation.json](validation/validation.json). Hash nguồn dùng để đối chiếu: [source_hashes.json](validation/source_hashes.json).

## 2. Kiểm tra trực quan

Đã tạo [ảnh tổng hợp 19 sơ đồ](validation/sequence_contact_sheet.png) và xem toàn bộ bố cục. Đã xem riêng SD-11 ở kích thước gốc sau khi sửa ranh giới giao dịch: giai đoạn chuẩn bị đánh giá chỉ đọc và trả căn cứ; yêu cầu xác nhận sau đó mở giao dịch ngắn mới, khóa và kiểm lại quyền, chu kỳ, bản sửa, lô và fingerprint trước khi ghi Closed/Reopened. Không giữ giao dịch mở khi chờ ATTT thao tác.

Ảnh dùng phông Arial, nền trắng, actor/lifeline phân biệt, Combined Fragment có nhãn guard, `ref` màu xanh và số thông điệp tự động. SVG được cung cấp để đọc hình dài ở độ phóng lớn; PNG phù hợp chèn báo cáo.

## 3. Giới hạn kiểm chứng

- PlantUML đã được parser PlantUML 1.2026.7 kiểm và dựng thành SVG/PNG. Mermaid được sinh từ cùng mô hình, đã kiểm header, message và cân bằng `alt/opt/loop/end`; môi trường hiện tại không có Mermaid renderer nên chưa dựng ảnh từ `.mmd`.
- Đây là kiểm tra tài liệu và tính nhất quán tĩnh, chưa phải kiểm thử backend, parser ZAP, quyền máy chủ, giao dịch đồng thời, kho tệp hay workflow đang chạy.
- Chưa tạo hoặc nhập dự án Visual Paradigm `.vpp`. Việc dựng ảnh thành công không chứng minh mọi phiên bản Visual Paradigm có thể nhập PlantUML/Mermaid thành phần tử UML chỉnh sửa được.
- Một số cơ chế được nêu là trách nhiệm triển khai ứng dụng: khóa lịch chống tạo kỳ trùng, chuẩn hóa/HMAC fingerprint, đối soát tệp chưa liên kết, kiểm lại manifest bản xuất và thử lại thông báo. Chúng không được báo là đã có trong SQL hiện hành.

[Danh mục bộ tài liệu](README.md) · [Tình huống đối chiếu](02_tinh_huong_kiem_tra.md)
