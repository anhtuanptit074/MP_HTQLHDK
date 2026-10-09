# Bộ sơ đồ cập nhật — HTQLHDK tích hợp tự động OWASP ZAP

Bộ sơ đồ này được xây dựng từ tài liệu `doi_tuong_sd_quy_trinh_hd_tich_hop_zap_tu_dong.md`, phần bổ sung `docs/ZAP_AUTOMATION_FRAMEWORK_CHANGES.md` và thiết kế cơ sở dữ liệu hiện hành.

| Mã | Sơ đồ | Phạm vi |
|---|---|---|
| 01 | ERD tích hợp ZAP tự động | Các thực thể cốt lõi từ tài sản, lịch quét, phiên ZAP, báo cáo, phát hiện đến khắc phục và xác minh |
| 02 | Sơ đồ hoạt động tổng thể | Luồng nghiệp vụ từ khai báo tài sản đến đóng hoặc mở lại phát hiện |
| 03 | Sơ đồ trạng thái phiên ZAP | Vòng đời kỹ thuật của `zap_execution`, gồm nhánh thành công, lỗi, quá hạn và hủy |
| 04 | Sơ đồ luồng dữ liệu mức 1 | Tác nhân ngoài, bốn tiến trình nghiệp vụ chính và bốn nhóm kho dữ liệu của hệ thống |
| 05 | Sơ đồ kiến trúc hệ thống | Kiến trúc tổng thể từ trình duyệt, edge, Web/API/Worker đến ZAP runner, PostgreSQL, kho báo cáo và dịch vụ bảo mật |

Mỗi sơ đồ có ba định dạng:

- `plantuml/*.puml`: nguồn có thể chỉnh sửa hoặc nhập vào Visual Paradigm/PlantUML.
- `rendered/*.svg`: bản vector để chèn vào tài liệu.
- `rendered/*.png`: bản ảnh để xem nhanh hoặc chèn vào Word.

Quy ước quan trọng:

- OWASP ZAP là hệ thống bên ngoài, không truy cập trực tiếp cơ sở dữ liệu nghiệp vụ.
- Bộ lập lịch và bộ điều phối ZAP là thành phần nội bộ, sử dụng tài khoản dịch vụ có quyền tối thiểu.
- `scan_occurrence` giữ trạng thái nghiệp vụ; `zap_execution` giữ trạng thái kỹ thuật của phiên chạy ZAP.
- Báo cáo gốc và metadata artifact là bất biến sau khi tiếp nhận.
- Kết quả ZAP chỉ là mức độ nguồn; chuyên viên ATTT xác nhận mức độ cuối cùng và quyết định đóng/mở lại.
