# Cơ sở dữ liệu HTQLHDK — bản 2.0, 27 bảng

Đã rút từ **43 xuống 27 bảng**, giảm **16 bảng (37,2%)**, theo nguồn nghiệp vụ duy nhất [doi_tuong_sd_quy_trinh_hd.md](../doi_tuong_sd_quy_trinh_hd.md) phiên bản 2.2. Bộ hiện tại có **27 bảng, 344 thuộc tính và 91 khóa ngoại**.

Thiết kế giữ bốn chức năng: khai báo tài sản, lịch quét, nhập báo cáo OWASP ZAP và theo dõi khắc phục. Các bảng quyền, nhật ký, thông báo và xuất báo cáo chỉ hỗ trợ bốn chức năng đó. OWASP ZAP tiếp tục chạy bên ngoài hệ thống.

27 bảng là phương án cân đối cho phạm vi tài liệu: gộp cấu hình và lịch sử dùng chung, đồng thời giữ riêng những dữ liệu cần để chống trùng và xác minh. Đây không phải khẳng định có một số bảng tối thiểu duy nhất cho mọi cách triển khai.

| Tài liệu | Nội dung |
|---|---|
| [Thiết kế tổng thể](01_thiet_ke_co_so_du_lieu.md) | Danh sách 27 bảng, quan hệ, khóa, nguyên tắc gộp và workflow |
| [Từ điển dữ liệu](02_tu_dien_du_lieu.md) | Từng thuộc tính, kiểu, PK/FK, UNIQUE và CHECK |
| [Sơ đồ ERD](03_so_do_erd.md) | Bốn sơ đồ có Mermaid, PlantUML, SVG và PNG |
| [Ràng buộc và giao dịch](04_rang_buoc_va_giao_dich.md) | Điều kiện DB thực thi và hợp đồng bắt buộc ở ứng dụng |
| [Kiểm tra và giới hạn](05_kiem_tra_va_gioi_han.md) | Kết quả thực chạy và phần chưa triển khai |
| [Đối chiếu phạm vi và 43 → 27](06_doi_chieu_pham_vi.md) | Nguồn của từng bảng, từng phép gộp và thông tin được giữ |
| [Giải thích chi tiết 27 bảng](07_giai_thich_chi_tiet_cac_bang.md) | Vai trò, chức năng, nội dung lưu trữ, quan hệ và vòng đời của từng bảng |
| [Quy trình xây dựng hệ thống](08_quy_trinh_xay_dung_he_thong.md) | Toàn bộ quy trình từ yêu cầu, thiết kế, lập trình, kiểm thử đến triển khai và vận hành |

## Bộ SQL

Chạy tuần tự trên **database PostgreSQL trống**:

1. [01_schema.sql](sql/01_schema.sql): tạo schema, 27 bảng, kiểu dữ liệu, khóa và chỉ mục.
2. [02_guards_and_views.sql](sql/02_guards_and_views.sql): trigger bảo vệ dữ liệu và view tra cứu.
3. [03_reference_data.sql](sql/03_reference_data.sql): ma trận quyền cố định dưới dạng view; không thêm bảng cấu hình.

SQL không tạo tài khoản nghiệp vụ, không tự cấp quyền người dùng và không đặt thời hạn SLA thật. Tên database/kết nối do môi trường triển khai cung cấp. Đây là bộ tạo mới, không phải migration từ bản 43 bảng; không xóa hoặc chuyển dữ liệu đang vận hành.

**75/75 kiểm tra SQL đạt** trên PostgreSQL 17.5 nhúng. Các test kiểm cấu trúc, khóa, một số trigger và view. Phân quyền API, parser ZAP, mã hóa kho tệp, masking, ghi audit tự động và các giao dịch ứng dụng chưa được triển khai trong nhiệm vụ thiết kế database.
