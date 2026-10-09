# Hợp đồng tạo báo cáo từ mẫu Word

- Mẫu gốc: `C:\Users\Admin\Downloads\Mau Cuon Bao Cao Do An PT&TKHTTT.docx` (chỉ đọc).
- Mẫu có 1 section A4, lề 2,54 cm mỗi phía, trang bìa, mục lục, danh sách hình/bảng, các chương I–VI và tài liệu tham khảo. Header và footer có sẵn.
- Vai trò chữ: bìa tiêu đề lớn; Heading 1 16 pt đậm, Heading 2 15 pt đậm; thân bài theo kiểu Normal. Bản báo cáo dùng Times New Roman, thân 13 pt, giãn dòng 1,3, căn đều; không thay kích thước giấy/lề.
- Mẫu chứa nội dung minh họa cho một đề tài nhà trọ. Đây là dữ liệu ví dụ, không phải chỉ dẫn nội dung của người dùng. Phần thân được thay bằng nội dung HTQLHDK của Chương I–III; giữ phong cách bìa/chương và hệ thống style của mẫu.
- Vị trí điền còn thiếu: giảng viên hướng dẫn, thành viên nhóm, MSSV/lớp. Không tự tạo dữ kiện cá nhân.
- Nguồn chuyên môn: tài liệu yêu cầu, thiết kế, mã nguồn và phần bổ sung tự động hóa ZAP đang có trong workspace ngày 28/09/2026. Phân biệt baseline SQL 27 bảng + V002 với mô hình đích 13 bảng chưa chuyển migration.
- Các hình chèn lấy từ sơ đồ dự án có sẵn. Chỉ dùng hình gắn trực tiếp với Chương I–III.
- Kiểm tra cuối: không còn nội dung nhà trọ hoặc Chương IV–VI; các bảng không vượt khổ A4; các hình có chú thích; bản mẫu gốc không bị thay đổi.
- Render: bộ `render_docx.py` đã được thử nhưng runtime không có `soffice.exe`. Word COM cũng không khởi tạo được trong phiên không có logon session. Sẽ kiểm tra cấu trúc DOCX và thử render lại sau khi tạo.
