# Cơ sở dữ liệu HTQLHDK — mô hình triển khai 13 bảng

Ứng dụng hiện hành đã chuyển sang **13 bảng**, theo [thiết kế BCNF tối giản](08_thiet_ke_3nf_tinh_gon.md). Migration triển khai nằm trong `ops/core-migrations/V001__core13.sql` và `V002__core_integrity.sql`; kiểm tra checksum và ràng buộc tại `ops/core-migrations/verify-core13.mjs`. Database dùng thử `htqlhdk_core_local` tách riêng khỏi database cũ `htqlhdk_local`.

Backend đã triển khai quyền, parser JSON/XML, mã hóa, che dữ liệu, audit và giao dịch nghiệp vụ. Xem [hướng dẫn ứng dụng 13 bảng](../docs/development/10_core13_application.md). Các phần SQL/kiểm tra 27–30 bảng bên dưới là hồ sơ cũ được giữ để tham khảo, không phải schema đang chạy.

Thiết kế giữ bốn chức năng: khai báo tài sản, lịch quét, nhập báo cáo OWASP ZAP và theo dõi khắc phục. Các bảng quyền, nhật ký, thông báo và xuất báo cáo chỉ hỗ trợ bốn chức năng đó. OWASP ZAP tiếp tục chạy bên ngoài hệ thống.

27 bảng là phương án cân đối cho phạm vi tài liệu: gộp cấu hình và lịch sử dùng chung, đồng thời giữ riêng những dữ liệu cần để chống trùng và xác minh. Đây không phải khẳng định có một số bảng tối thiểu duy nhất cho mọi cách triển khai.

| Tài liệu | Nội dung |
|---|---|
| [Thiết kế tổng thể cũ](01_thiet_ke_co_so_du_lieu.md) | Mô hình 27 bảng trước khi tinh gọn |
| [Từ điển dữ liệu cũ](02_tu_dien_du_lieu.md) | Thuộc tính của mô hình 27 bảng trước khi tinh gọn |
| [ERD 13 bảng](03_so_do_erd.md) | ERD tổng thể và các sơ đồ con bằng Mermaid; có tệp nguồn Mermaid/PlantUML |
| [Ràng buộc và giao dịch](04_rang_buoc_va_giao_dich.md) | Điều kiện DB thực thi và hợp đồng bắt buộc ở ứng dụng |
| [Kiểm tra và giới hạn](05_kiem_tra_va_gioi_han.md) | Kết quả thực chạy và phần chưa triển khai |
| [Đối chiếu phạm vi và 43 → 27](06_doi_chieu_pham_vi.md) | Nguồn của từng bảng, từng phép gộp và thông tin được giữ |
| [Giải thích chi tiết 27 bảng cũ](07_giai_thich_chi_tiet_cac_bang.md) | Tài liệu tham khảo của mô hình trước khi tinh gọn |
| [Thiết kế BCNF tối giản](08_thiet_ke_3nf_tinh_gon.md) | Mô hình lõi 13 bảng, giữ user_role tối thiểu và không lưu dữ liệu có thể suy ra bằng truy vấn |
| [Giải thích chi tiết 13 bảng](09_giai_thich_chi_tiet_13_bang.md) | Vai trò, thuộc tính, khóa, quan hệ, vòng đời và dữ liệu suy ra của từng bảng |
| [Quy trình xây dựng hệ thống](08_quy_trinh_xay_dung_he_thong.md) | Toàn bộ quy trình từ yêu cầu, thiết kế, lập trình, kiểm thử đến triển khai và vận hành |

## Bộ SQL cũ (27 bảng trước tự động hóa ZAP)

Chạy tuần tự trên **database PostgreSQL trống**:

1. [01_schema.sql](sql/01_schema.sql): tạo schema, 27 bảng, kiểu dữ liệu, khóa và chỉ mục.
2. [02_guards_and_views.sql](sql/02_guards_and_views.sql): trigger bảo vệ dữ liệu và view tra cứu.
3. [03_reference_data.sql](sql/03_reference_data.sql): ma trận quyền cố định dưới dạng view; không thêm bảng cấu hình.

Các tệp SQL này thuộc mô hình trước khi tinh gọn. Migration 13 bảng hiện hành dùng bộ SQL riêng ở `ops/core-migrations`; không chạy hai mô hình vào cùng schema/database.

**75/75 kiểm tra SQL đạt** trên PostgreSQL 17.5 nhúng. Các test kiểm cấu trúc, khóa, một số trigger và view. Phân quyền API, parser ZAP, mã hóa kho tệp, masking, ghi audit tự động và các giao dịch ứng dụng chưa được triển khai trong nhiệm vụ thiết kế database.
