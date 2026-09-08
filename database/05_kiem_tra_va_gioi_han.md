# Kiểm tra và giới hạn — bản 2.0

## 1. Kết quả thực chạy

- **75/75 kiểm tra đạt**, ghi nhận tại `2026-09-04T15:45:52.222Z` (UTC).
- Môi trường: PGlite 0.3.14, PostgreSQL 17.5 nhúng WebAssembly; mỗi lần chạy tạo database kiểm tra mới trong bộ nhớ.
- Ba tệp SQL được thực thi tuần tự, sau đó nạp fixture giả và kiểm các trường hợp trong giao dịch rollback riêng.
- Mô hình hiện tại: **27 bảng, 344 thuộc tính và 91 khóa ngoại**. Kết quả kiểm số bảng/FK được so với model.json.
- [Kết quả chi tiết](validation/database_test_results.json) có tên ca, kết quả, SQLSTATE và SHA-256 của SQL/fixture đã chạy. [Kiểm tra bộ tài liệu](validation/artifact_checks.json) kiểm model/từ điển, liên kết, ảnh và tài liệu nguồn giữ nguyên.

Các ca tập trung vào: phạm vi/tài sản; nhập trùng và metadata gốc bất biến sau gộp; dòng nguồn và Partial/Rejected; mức độ và rà soát độc lập; lịch sử/con trỏ phiếu; đóng qua kết quả xác minh mới; chấp nhận/gia hạn rủi ro; cờ nhạy cảm/ma trận quyền; xuất/thu hồi; FK căn cứ audit và bảo vệ lịch sử.

## 2. Kiểm tra chưa bao phủ

Đây là kiểm tra **database**, chưa phải nghiệm thu hệ thống hoàn chỉnh. Chưa kiểm máy chủ PostgreSQL độc lập, nhiều kết nối đồng thời, tải lớn, restore hoặc triển khai thực tế. Chưa có parser ZAP thật, API xác thực/phân quyền, RLS, mã hóa kho tệp, masking tự động, bộ lập kỳ lịch, kiểm URL theo scope, job hết hạn, hoặc dịch vụ ghi audit tự động.

Các ca về quyền chỉ kiểm dữ liệu mặc định, CHECK và ma trận view. Chúng không chứng minh API đã ngăn truy cập trái quyền. Các ca xác minh kiểm điều kiện SQL, không chứng minh tệp ZAP bao phủ đủ mục tiêu. Fixture là dữ liệu cấu trúc tối thiểu, không thay thế giao dịch ứng dụng và audit bắt buộc ở [tài liệu 04](04_rang_buoc_va_giao_dich.md).

Không tự đặt UNIQUE giới hạn số hồ sơ chờ để tránh vấn đề đồng thời. Ứng dụng phải khóa, kiểm phiên bản/hồ sơ hiện hành và ghi lịch sử nguyên tử. Không khẳng định thiết kế 27 bảng có hiệu năng tốt hơn bản 43 khi chưa có benchmark thực tế.

## 3. Tái kiểm tra

Từ thư mục dự án chạy `node database/validation/verify_database.mjs`. Trình kiểm tra dùng PGlite đã có ở `.codex_tmp/database_design/qa/node_modules`; có thể truyền đường dẫn tuyệt đối tới module PGlite làm tham số đầu tiên. Không dùng fixture với database thật.

Các tệp thiết kế chuẩn nằm ở `database/`. Công cụ sinh hiện hành là `.codex_tmp/database_design/build_compact.py` và `build_compact_guards.py`; sau đó chạy test, sinh tài liệu bằng `build_compact_docs.py`, dựng ảnh PlantUML và chạy `check_artifacts.py`. Các bản sinh cũ trong `archive_43` chỉ để đối chiếu, không phải bộ thiết kế đang áp dụng.

## 4. Phạm vi đã hoàn thành

Đã cung cấp mô hình bảng, thuộc tính, PK/FK, quan hệ, ràng buộc, chỉ mục, view, guard SQL, sơ đồ ERD và hợp đồng giao dịch bắt buộc cho ứng dụng. Không chạy di trú/xóa dữ liệu trên database thật và không sửa tài liệu nghiệp vụ nguồn.
