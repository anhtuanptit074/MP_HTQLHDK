# Đặc tả xây dựng Use Case Diagram và Activity Diagram

## Hệ thống quản lý quét lỗ hổng website định kỳ

| Thuộc tính | Nội dung |
|---|---|
| Phiên bản đặc tả | 2.0 — hiệu chỉnh ngày 04/09/2026 |
| Nguồn nghiệp vụ duy nhất | `doi_tuong_sd_quy_trinh_hd.md`, phiên bản 2.2 |
| Phạm vi | F-01 tài sản; F-02 lịch quét; F-03 nhập báo cáo ZAP; F-04 theo dõi khắc phục |
| Bộ mã áp dụng | `visual_paradigm_uml_md`, 7 sơ đồ Use Case và 16 Activity |
| Bản chuẩn để vẽ | Các khối PlantUML trong từng tệp Markdown; đặc tả không giữ một bộ mã mẫu thứ hai |

## 1. Phạm vi và cách đọc

Tài liệu này thay thế đặc tả đầu vào trước, thống nhất mã Use Case và các luồng với tài liệu nguồn. Nguồn nghiệp vụ không bị chỉnh sửa. Khi cần quy tắc chưa có trong nguồn, không tự tạo quyền, trạng thái, SLA hoặc chức năng mới.

ZAP được vận hành chạy bên ngoài hệ thống; xuất Traditional JSON (ưu tiên) hoặc Traditional XML (dự phòng) rồi tải tệp thủ công. Hệ thống không active scan, không gửi payload, không điều khiển ZAP, không nhận kết quả qua API và không nhập công cụ khác. HTML/Markdown/PDF chỉ để đọc hoặc đính kèm tham khảo, không tự tạo Finding.

Phân quyền, audit, thông báo, báo cáo và chấp nhận rủi ro chỉ hỗ trợ bốn chức năng. Không thêm SSO, email server, BI, CI/CD, kho mã nguồn hoặc ticket bên ngoài. Bố cục thành nhiều sơ đồ không tạo thêm phân hệ.

Mã UC là định danh chuẩn; dấu gạch dưới trong bí danh PlantUML chỉ thay dấu gạch ngang, không đổi số hoặc ý nghĩa. Các UC xử lý nội bộ có cột tác nhân là “—; nội bộ”; không vẽ Hệ thống thành actor của chính nó. Lane Hệ thống trong Activity là hợp lệ. Tác nhân ZAP không cần nối trực tiếp vào Use Case vì báo cáo được Ops tải thủ công.

## 2. Vai trò và quyền

Giữ bảy vai trò nguồn: Quản trị viên, Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên.

Người rà soát thứ hai là người có thẩm quyền theo chính sách, khác người đánh giá; không tạo vai trò nghiệp vụ thứ tám. Vai trò người phê duyệt rủi ro tham gia rà soát mức độ khi được phân quyền như ma trận nguồn. Người khắc phục không tự đóng, Ops không quyết Final Severity, người đề nghị không tự duyệt rủi ro, kiểm toán chỉ đọc, Admin không mặc nhiên xem lỗ hổng.

### 2.1. Ma trận giữ nguyên từ nguồn mục 3.3

Ký hiệu: C là tạo, R là xem, U là cập nhật hoặc thực hiện, A là phê duyệt, dấu gạch ngang là không có quyền mặc định.

| Chức năng | Quản trị | ATTT | Chủ tài sản | Vận hành quét | Khắc phục | Phê duyệt rủi ro | Kiểm toán |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Tài sản | Gán quyền | C/R/U/A | C/R/U/xác nhận | R | R theo phiếu | R | R |
| Lịch quét | Gán quyền | C/R/U/A | R/xác nhận thời gian | R/U trạng thái | R | R | R |
| Nhập báo cáo ZAP | Gán quyền | R/phân tích | R theo tài sản | C/R/U | R theo phiếu | R | R |
| Xác nhận mức độ | - | R/U | R/góp ý ảnh hưởng | R | R | A khi cần rà soát | R |
| Phiếu khắc phục | - | C/R/U | R/góp ý | R | R/U | R | R |
| Chấp nhận rủi ro | - | C/R/khuyến nghị | C/R/đề nghị | R | R/đề nghị | R/A | R |
| Đóng hoặc mở lại | - | U | R | R/cung cấp kết quả | R | R | R |
| Xuất báo cáo | - | Theo phạm vi | Theo tài sản | Theo nhiệm vụ | Theo phiếu | Theo thẩm quyền | Theo phạm vi |

### 2.2. Quy tắc diễn giải quyền

- Association chỉ biểu diễn sự tham gia. Actor phụ không được kế thừa quyền quyết định của actor chính.
- Nhóm khắc phục có thể cung cấp đề xuất rủi ro; chỉ Chủ tài sản/ATTT tạo đề nghị chính thức và chuyển Pending Risk Acceptance theo nguồn 7.2.
- Quyền xem RawReport và bằng chứng nhạy cảm được kiểm tra riêng. Các đường nối với UC-C04 chỉ áp dụng khi có quyền riêng, không cấp mặc định cho cả vai trò.
- Admin xem nhật ký thay đổi quyền; UC-C06 không mở toàn bộ lịch sử nghiệp vụ cho Admin.
- Result Imported phản ánh kết quả F-03; người vận hành không chọn thủ công trạng thái này thay thao tác nhập.

## 3. Các quy tắc dùng chung cho mọi UC và Activity

| Mã | Quy tắc và cách xử lý thất bại | Nguồn |
|---|---|---|
| K-01 | Xác thực; kiểm tra quyền thao tác và asset scope hiện hành tại máy chủ cho màn hình, API nội bộ, tệp tải. Không đạt: từ chối, audit, không thực hiện bước đọc/ghi tiếp theo. | 8.2 |
| K-02 | Mọi hành động thuộc 8.5, kể cả từ chối/lỗi và nhánh kết thúc sớm, phải audit người, thời điểm, hành động, đối tượng, kết quả, trước/sau khi phù hợp. Mọi chuyển trạng thái có nhận xét và bằng chứng liên quan. Nhật ký không được người dùng nghiệp vụ sửa. | 7.2; 8.5 |
| K-03 | Mã hóa khi truyền/lưu theo cơ chế đơn vị; che bí mật ở giao diện và báo cáo xuất. RawReport bất biến, checksum, quyền riêng. Không sửa bản gốc để masking. | 8.3 |
| K-04 | Không xóa cứng dữ liệu có giá trị lịch sử. Thời hạn lưu theo chính sách; hết hạn chỉ xử lý bằng tác vụ đã phê duyệt và audit, không tự tạo chức năng xóa tùy ý. | 8.6 |
| K-05 | Thời hạn xử lý dựa trên Final Severity và bảng thời hạn đơn vị đã phê duyệt. Không đặt SLA cố định mới. Đổi mức độ/hạn phải lưu hạn cũ/mới và lý do. | 6.5 |
| K-06 | Gọi AD khác phải giữ đủ tiền điều kiện, các nhánh lỗi và hậu điều kiện. Một bước ghi “kiểm tra” luôn tuân K-01/K-02, không có quyền thì không đi tiếp. | 3.4; 4; 7.2 |
| K-07 | Phạm vi đề nghị chưa duyệt không được dùng để quét/lập lịch/nhập. Cập nhật scope phải ATTT duyệt lại. | 5.1.2 |
| K-08 | Mở lại phải phân biệt ba nguồn: xác minh do ATTT; Closed tái xuất hiện do hệ thống/ATTT; rủi ro hết điều kiện do hệ thống/ATTT. Không dùng quyết định khắc phục của Fix để đóng. | 7.2; 7.5 |

Legend K-01/K-02 áp dụng cho tất cả đường đi, không chỉ nhánh thành công. Kiểm tra quyền được lặp lại ở thời điểm thực hiện thao tác nhạy cảm để tránh dùng quyền đã thu hồi.

## 4. Danh mục Use Case thống nhất

Các hành vi mới được đặt mã riêng để diễn đạt yêu cầu đã có: UC-S08 (nguồn 3.3), UC-R13 (5.4.2; 6.5), UC-C09 (3.3; 9.2), UC-C10 (8.4). Đây là phân rã nội dung nguồn, không bổ sung chức năng. Các mã khác giữ ý nghĩa danh mục của đặc tả cũ; bộ mã sơ đồ đã được sửa theo danh mục này.

### 4.1. Nhóm A

| ID | Use Case | Actor chính / tham gia trực tiếp | Nguồn | Activity |
|---|---|---|---|---|
| UC-A01 | Khai báo tài sản | Chủ sở hữu tài sản, Chuyên viên ATTT | 4; 5.1 | [AD-02](visual_paradigm_uml_md/03_activity_khai_bao_phe_duyet_tai_san.md) |
| UC-A02 | Cập nhật thông tin tài sản | Chủ sở hữu tài sản, Chuyên viên ATTT | 3.2; 5.1.2 | [AD-02](visual_paradigm_uml_md/03_activity_khai_bao_phe_duyet_tai_san.md), [AD-14](visual_paradigm_uml_md/15_activity_vong_doi_tai_san.md) |
| UC-A03 | Xác nhận thông tin tài sản | Chủ sở hữu tài sản | 3.2; 4; 5.1 | [AD-02](visual_paradigm_uml_md/03_activity_khai_bao_phe_duyet_tai_san.md) |
| UC-A04 | Phê duyệt phạm vi quét | Chuyên viên ATTT | 3.2; 5.1 | [AD-02](visual_paradigm_uml_md/03_activity_khai_bao_phe_duyet_tai_san.md) |
| UC-A05 | Tạm dừng tài sản | Chuyên viên ATTT | 3.3; 5.1.3 | [AD-14](visual_paradigm_uml_md/15_activity_vong_doi_tai_san.md) |
| UC-A06 | Ngừng hoạt động tài sản | Chuyên viên ATTT | 3.3; 5.1.2–5.1.3 | [AD-14](visual_paradigm_uml_md/15_activity_vong_doi_tai_san.md) |
| UC-A07 | Xem tài sản theo phạm vi | Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên | 3.3; 8.2 | [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |

### 4.2. Nhóm S

| ID | Use Case | Actor chính / tham gia trực tiếp | Nguồn | Activity |
|---|---|---|---|---|
| UC-S01 | Tạo lịch quét | Chuyên viên ATTT | 5.2 | [AD-03](visual_paradigm_uml_md/04_activity_quan_ly_lich_quet.md) |
| UC-S02 | Cập nhật lịch quét | Chuyên viên ATTT | 5.2.2 | [AD-03](visual_paradigm_uml_md/04_activity_quan_ly_lich_quet.md) |
| UC-S03 | Kích hoạt hoặc tiếp tục lịch | Chuyên viên ATTT | 5.2.2–5.2.3 | [AD-03](visual_paradigm_uml_md/04_activity_quan_ly_lich_quet.md) |
| UC-S04 | Tạm dừng lịch | Chuyên viên ATTT | 5.2.2 | [AD-03](visual_paradigm_uml_md/04_activity_quan_ly_lich_quet.md) |
| UC-S05 | Theo dõi lịch đến hạn | Nhóm vận hành quét, Chuyên viên ATTT | 3.2; 5.2; 9.1 | [AD-11](visual_paradigm_uml_md/12_activity_thuc_hien_lan_quet.md) |
| UC-S06 | Cập nhật trạng thái lần quét | Nhóm vận hành quét | 5.2.3 | [AD-11](visual_paradigm_uml_md/12_activity_thuc_hien_lan_quet.md) |
| UC-S07 | Ghi nhận quét thất bại hoặc bỏ lỡ | Nhóm vận hành quét | 5.2.3; 7.6 | [AD-11](visual_paradigm_uml_md/12_activity_thuc_hien_lan_quet.md) |
| UC-S08 | Xác nhận thời gian quét | Chủ sở hữu tài sản | 3.3 | [AD-03](visual_paradigm_uml_md/04_activity_quan_ly_lich_quet.md) |

### 4.3. Nhóm I

| ID | Use Case | Actor chính / tham gia trực tiếp | Nguồn | Activity |
|---|---|---|---|---|
| UC-I01 | Nhập báo cáo ZAP | Nhóm vận hành quét | 5.3 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) |
| UC-I02 | Kiểm tra quyền nhập | —; xử lý nội bộ | 5.3.3; 8.2 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) |
| UC-I03 | Kiểm tra tài sản và lần quét | —; xử lý nội bộ | 5.3.3 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) |
| UC-I04 | Kiểm tra định dạng báo cáo | —; xử lý nội bộ | 5.3.1–5.3.3 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) |
| UC-I05 | Kiểm tra phạm vi URL | —; xử lý nội bộ | 5.3.3; 7.6 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) |
| UC-I06 | Lưu báo cáo gốc bất biến | —; xử lý nội bộ | 5.3.3; 8.3 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) |
| UC-I07 | Che dữ liệu nhạy cảm | —; xử lý nội bộ | 5.3.3; 8.3 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md), [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |
| UC-I08 | Chuẩn hóa dữ liệu ZAP | —; xử lý nội bộ | 5.3.4 | [AD-12](visual_paradigm_uml_md/13_activity_chuan_hoa_va_tai_xuat_hien.md) |
| UC-I09 | Chống nhập trùng | —; xử lý nội bộ | 5.3.5 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md), [AD-12](visual_paradigm_uml_md/13_activity_chuan_hoa_va_tai_xuat_hien.md) |
| UC-I10 | Tạo hoặc cập nhật phát hiện | —; xử lý nội bộ | 5.3.5; 7.3–7.4 | [AD-12](visual_paradigm_uml_md/13_activity_chuan_hoa_va_tai_xuat_hien.md) |
| UC-I11 | Ghi nhận lỗi nhập | —; xử lý nội bộ | 5.3.6; 7.6; 8.5 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) |
| UC-I12 | Xem kết quả lô nhập | Nhóm vận hành quét, Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên | 3.3; 5.3.6 | [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md), [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |

### 4.4. Nhóm T

| ID | Use Case | Actor chính / tham gia trực tiếp | Nguồn | Activity |
|---|---|---|---|---|
| UC-T01 | Rà soát phát hiện mới | Chuyên viên ATTT | 6; 7.2 | [AD-05](visual_paradigm_uml_md/06_activity_phan_tich_final_severity.md) |
| UC-T02 | Xác nhận Final Severity | Chuyên viên ATTT | 6 | [AD-05](visual_paradigm_uml_md/06_activity_phan_tich_final_severity.md) |
| UC-T03 | Xác nhận False Positive | Chuyên viên ATTT | 7.2–7.3 | [AD-07](visual_paradigm_uml_md/09_activity_false_positive_ngoai_pham_vi.md) |
| UC-T04 | Hủy phát hiện sai hoặc ngoài phạm vi | Chuyên viên ATTT | 7.2; 8.5 | [AD-07](visual_paradigm_uml_md/09_activity_false_positive_ngoai_pham_vi.md) |
| UC-T05 | Xác nhận ảnh hưởng nghiệp vụ | Chủ sở hữu tài sản | 3.2; 6.4 | [AD-05](visual_paradigm_uml_md/06_activity_phan_tich_final_severity.md) |
| UC-T06 | Xem lịch sử thay đổi mức độ | Chuyên viên ATTT, Kiểm toán viên | 3.2; 6.1–6.2 | [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |

### 4.5. Nhóm R

| ID | Use Case | Actor chính / tham gia trực tiếp | Nguồn | Activity |
|---|---|---|---|---|
| UC-R01 | Tạo phiếu khắc phục | Chuyên viên ATTT | 4; 5.4 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R02 | Phân công xử lý | Chuyên viên ATTT | 5.4; 7.2 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R03 | Tiếp nhận phiếu | Nhóm khắc phục kỹ thuật | 3.2; 7.2 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R04 | Cập nhật tiến độ | Nhóm khắc phục kỹ thuật | 5.4 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R05 | Cập nhật biện pháp và bằng chứng | Nhóm khắc phục kỹ thuật | 5.4; 7.5 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R06 | Chuyển chờ xác minh | Nhóm khắc phục kỹ thuật | 7.2; 7.5 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R07 | Tạo yêu cầu quét xác minh | Chuyên viên ATTT | 7.5 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R08 | Cập nhật kết quả quét xác minh | Nhóm vận hành quét | 7.5 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md), [AD-04](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) |
| UC-R09 | Đối chiếu dấu vân tay phát hiện | —; xử lý nội bộ | 7.2; 7.5 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R10 | Đóng phát hiện | Chuyên viên ATTT | 5.4.2; 7.2; 7.5 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) |
| UC-R11 | Mở lại phát hiện | Chuyên viên ATTT | 7.2 | [AD-06](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md), [AD-10](visual_paradigm_uml_md/11_activity_mo_lai_phat_hien.md) |
| UC-R12 | Theo dõi hạn xử lý | Chuyên viên ATTT, Nhóm khắc phục kỹ thuật, Chủ sở hữu tài sản | 5.4.2; 9.1–9.2 | [AD-16](visual_paradigm_uml_md/17_activity_thoi_han_thong_bao.md) |
| UC-R13 | Điều chỉnh phân công hoặc thời hạn | Chuyên viên ATTT | 5.4.2; 6.5 | [AD-16](visual_paradigm_uml_md/17_activity_thoi_han_thong_bao.md) |

### 4.6. Nhóm RA

| ID | Use Case | Actor chính / tham gia trực tiếp | Nguồn | Activity |
|---|---|---|---|---|
| UC-RA01 | Tạo đề nghị chấp nhận rủi ro | Chủ sở hữu tài sản, Chuyên viên ATTT | 7.2; 7.4 | [AD-08](visual_paradigm_uml_md/08_activity_chap_nhan_rui_ro.md) |
| UC-RA02 | Rà soát đề nghị | Người phê duyệt rủi ro | 3.2; 7.4 | [AD-08](visual_paradigm_uml_md/08_activity_chap_nhan_rui_ro.md) |
| UC-RA03 | Phê duyệt chấp nhận rủi ro | Người phê duyệt rủi ro | 7.2; 7.4 | [AD-08](visual_paradigm_uml_md/08_activity_chap_nhan_rui_ro.md) |
| UC-RA04 | Từ chối hoặc yêu cầu bổ sung | Người phê duyệt rủi ro | 3.2; 7.2 | [AD-08](visual_paradigm_uml_md/08_activity_chap_nhan_rui_ro.md) |
| UC-RA05 | Theo dõi ngày hết hạn | Chuyên viên ATTT, Chủ sở hữu tài sản | 7.4; 9.1 | [AD-10](visual_paradigm_uml_md/11_activity_mo_lai_phat_hien.md), [AD-16](visual_paradigm_uml_md/17_activity_thoi_han_thong_bao.md) |
| UC-RA06 | Mở lại khi hết điều kiện rủi ro | Chuyên viên ATTT | 7.2; 7.4 | [AD-10](visual_paradigm_uml_md/11_activity_mo_lai_phat_hien.md) |
| UC-RA07 | Gia hạn chấp nhận rủi ro | Chủ sở hữu tài sản, Chuyên viên ATTT | 7.4 | [AD-13](visual_paradigm_uml_md/14_activity_gia_han_rui_ro.md) |

### 4.7. Nhóm C

| ID | Use Case | Actor chính / tham gia trực tiếp | Nguồn | Activity |
|---|---|---|---|---|
| UC-C01 | Gán vai trò và phạm vi tài sản | Quản trị viên | 3.2; 8.2 | [AD-15](visual_paradigm_uml_md/16_activity_phan_quyen.md) |
| UC-C02 | Thu hồi quyền | Quản trị viên | 3.2; 8.2 | [AD-15](visual_paradigm_uml_md/16_activity_phan_quyen.md) |
| UC-C03 | Xem nhật ký thay đổi quyền | Quản trị viên, Kiểm toán viên | 3.2; 8.5 | [AD-15](visual_paradigm_uml_md/16_activity_phan_quyen.md), [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |
| UC-C04 | Xem báo cáo gốc theo quyền | Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên | 8.2; 8.5 | [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |
| UC-C05 | Xuất hoặc tải báo cáo | Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên | 3.3; 8.4 | [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |
| UC-C06 | Xem lịch sử nghiệp vụ | Chuyên viên ATTT, Kiểm toán viên | 3.2; 8.5 | [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |
| UC-C07 | Ghi nhật ký hành động nhạy cảm | —; xử lý nội bộ | 7.2; 8.5 | [AD-01](visual_paradigm_uml_md/02_activity_tong_the.md), [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md), [AD-15](visual_paradigm_uml_md/16_activity_phan_quyen.md) |
| UC-C08 | Nhận thông báo nghiệp vụ | Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro | 9.1 | [AD-16](visual_paradigm_uml_md/17_activity_thoi_han_thong_bao.md) |
| UC-C09 | Xem dữ liệu nghiệp vụ theo phạm vi | Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên | 3.3; 8.2; 9.2 | [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |
| UC-C10 | Thu hồi liên kết báo cáo | Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên | 8.4 | [AD-09](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) |

## 5. Hợp đồng thực hiện từng Use Case

**Tiền điều kiện chung:** tác nhân có quyền thao tác theo ma trận và K-01; xử lý nội bộ chỉ được gọi từ UC cha đã kiểm tra quyền. **Luồng thực hiện:** theo các Activity được liên kết ở mục 4, đúng điều kiện vào/ra tại mỗi tệp. **Ngoại lệ chung:** mất quyền hoặc sai phạm vi thì từ chối; không áp dụng thay đổi chưa được duyệt; lỗi/nhánh dừng ghi audit theo K-02. Bảng dưới xác định mục tiêu, điều kiện riêng và kết quả cần kiểm chứng cho từng mã, kể cả các hành vi nội bộ.

| ID | Điều kiện riêng, hành vi và hậu điều kiện |
|---|---|
| UC-A01 | Tạo Draft với mã duy nhất, domain/URL hợp lệ; cảnh báo trùng, không tự chặn chỉ vì trùng domain. |
| UC-A02 | Lưu lịch sử; thay đổi scope phải qua chủ sở hữu xác nhận và ATTT phê duyệt lại trước khi sử dụng. |
| UC-A03 | Xác nhận sở hữu, mức quan trọng, phạm vi; đủ thông tin mới Pending Approval. |
| UC-A04 | Chỉ Active sau khi ATTT duyệt dữ liệu đầy đủ và scope; từ chối giữ bản đề nghị để sửa, không áp dụng scope mới. |
| UC-A05 | Tạm dừng quét khi thay đổi/sự cố; Suspended không được kích hoạt lịch hoặc nhập kết quả. |
| UC-A06 | Retired chỉ giữ lịch sử, không xóa cứng. |
| UC-A07 | Chỉ dữ liệu thuộc role + asset scope; Fix theo phiếu; quyền này không bao gồm RawReport. |
| UC-S01 | Tạo Draft có tần suất, múi giờ, ngày hiệu lực/kết thúc, khung giờ, scope và nhóm vận hành. |
| UC-S02 | Kiểm tra lại điều kiện áp dụng; lưu lịch sử tài sản, scope và tần suất thay đổi. |
| UC-S03 | Chỉ Active nếu tài sản Active, scope đã duyệt, lịch hợp lệ; tiếp tục từ Paused cũng kiểm tra lại. |
| UC-S04 | Paused; giữ toàn bộ lịch sử và các lần quét. |
| UC-S05 | Xem lịch/lần quét đến hạn; hệ thống tạo Planned khi lịch đủ điều kiện và đến kỳ. |
| UC-S06 | Ghi Completed, Failed hoặc Missed theo thực tế; Result Imported chỉ do kết quả nhập F-03. |
| UC-S07 | Failed khi đã quét nhưng lỗi; Missed khi không thực hiện đúng lịch; bắt buộc lý do. |
| UC-S08 | Chủ sở hữu xác nhận thời gian thuộc tài sản của mình; không thay quyền ATTT kích hoạt lịch. |
| UC-I01 | Tải Traditional JSON/XML vào một tài sản và một lần quét; trả kết quả lô hoặc tham chiếu lô cũ. |
| UC-I02 | Xử lý nội bộ: xác thực + role + asset scope phía máy chủ; không đủ quyền thì từ chối và audit. |
| UC-I03 | Xử lý nội bộ: Asset Active; ScanOccurrence hợp lệ và thuộc tài sản đã chọn. |
| UC-I04 | Kiểm tra mẫu Traditional JSON/XML, loại tệp, kích thước và cấu trúc; không parse HTML/MD/PDF để tạo Finding. |
| UC-I05 | Lô sai tài sản/phạm vi bị từ chối; bản ghi sai phạm vi có ImportError và yêu cầu ATTT rà soát. |
| UC-I06 | Lưu nguyên bản có checksum trong vùng kiểm soát, mã hóa; không sửa nội dung để masking. |
| UC-I07 | Che token, cookie, mật khẩu, khóa API và dữ liệu cá nhân không cần thiết ở bản hiển thị/xuất. |
| UC-I08 | Giữ trường nguồn, ánh xạ theo bảng; không gộp Risk với Confidence hoặc tự gán Critical. |
| UC-I09 | Khóa lô = asset + occurrence + checksum; fingerprint theo nguồn; không nhân đôi Finding, occurrence của cùng lô hoặc phiếu. |
| UC-I10 | Finding mới New + occurrence; đã có thì thêm occurrence/last_seen; Closed mở lại, False Positive yêu cầu rà soát lại. |
| UC-I11 | Rejected không tạo Finding; Partially Completed có thành công và lỗi; Failed là lỗi kỹ thuật; lưu lỗi và audit. |
| UC-I12 | Xem kết quả, số bản ghi và lỗi trong phạm vi; không ngầm cấp quyền báo cáo gốc. |
| UC-T01 | New được phân loại đúng/sai/ngoài phạm vi; chỉ nhánh hợp lệ đi xác nhận severity. |
| UC-T02 | Có bằng chứng, bối cảnh, lý do, người/thời điểm, trước/sau; hạ Critical/High phải chờ rà soát theo chính sách. |
| UC-T03 | Có lý do và bằng chứng; Critical/High phải có xác nhận người thứ hai; không xóa Finding. |
| UC-T04 | New → Cancelled khi bản ghi sai/ngoài phạm vi; lý do, audit và giữ RawReport. |
| UC-T05 | Cung cấp bối cảnh theo tài sản khi ATTT yêu cầu; không quyết định Final Severity. |
| UC-T06 | Xem giá trị trước/sau, lý do, người/thời điểm đánh giá và rà soát theo quyền. |
| UC-R01 | Finding Triaged; tạo hoặc liên kết phiếu hiện có, không tạo phiếu trùng. |
| UC-R02 | Triaged/Reopened → Assigned chỉ khi có người phụ trách và hạn; Reopened có thời hạn mới. |
| UC-R03 | Người được giao xác nhận; Assigned → In Progress. |
| UC-R04 | Chỉ phiếu được giao; lưu tiến độ, nguyên nhân, nhận xét và lịch sử. |
| UC-R05 | Cập nhật biện pháp/evidence có che dữ liệu; không tự đóng Finding. |
| UC-R06 | Chỉ In Progress đủ biện pháp và bằng chứng mới Pending Verification; thiếu thì bổ sung và kiểm tra lại. |
| UC-R07 | Pending Verification; gắn tài sản/phạm vi, Finding và nhóm vận hành. |
| UC-R08 | Chạy ZAP ngoài hệ thống, nhập báo cáo mới qua UC-I01 và liên kết VerificationRequest. |
| UC-R09 | Xử lý nội bộ: đối chiếu với báo cáo xác minh hợp lệ; thiếu kết quả không đồng nghĩa không còn fingerprint. |
| UC-R10 | Pending Verification + ZAP mới hợp lệ + không còn fingerprint + ATTT xác nhận → Closed. |
| UC-R11 | ATTT mở lại khi xác minh còn lỗi/không hợp lệ; hệ thống hoặc ATTT mở lại Closed tái xuất hiện hoặc Risk Accepted hết điều kiện. |
| UC-R12 | Xem sắp đến hạn/quá hạn theo phạm vi; thời hạn dựa trên chính sách đơn vị. |
| UC-R13 | Lưu người/hạn cũ, mới, lý do, người thực hiện và thời điểm; không thay đổi trạng thái trái mục 7.2. |
| UC-RA01 | Chỉ Triaged/Assigned/In Progress; lý do, tác động, giảm thiểu, người chịu trách nhiệm, hạn đề nghị. |
| UC-RA02 | Xem đề nghị và khuyến nghị ATTT; có thể yêu cầu bổ sung rồi rà soát lại. |
| UC-RA03 | Đúng thẩm quyền, độc lập, điều kiện và ngày hết hạn hợp lệ; Pending Risk Acceptance → Risk Accepted. |
| UC-RA04 | Từ chối/tiếp tục khắc phục: khôi phục trạng thái trước; bổ sung: vẫn chờ, gửi lại để rà soát. |
| UC-RA05 | Hệ thống kiểm tra theo sự kiện thời gian; người dùng xem trong phạm vi; không chỉ kiểm tra ngay lúc phê duyệt. |
| UC-RA06 | Hết hạn chưa khắc phục/gia hạn hợp lệ, vi phạm điều kiện hoặc rủi ro tăng → Reopened; có audit. |
| UC-RA07 | Tạo quyết định mới qua người duyệt độc lập; không ghi đè quyết định cũ, không tự thêm cạnh Risk Accepted → Pending Risk Acceptance. |
| UC-C01 | Chỉ quyền phục vụ F-01–F-04; mặc định từ chối, ghi nhật ký trước/sau. |
| UC-C02 | Thu hồi có nhật ký; các lần đọc/tải tiếp theo kiểm tra quyền hiện hành. |
| UC-C03 | Admin chỉ xem nhật ký thay đổi quyền; không ngầm đọc audit chứa nội dung lỗ hổng. |
| UC-C04 | Cần quyền RawReport riêng cùng phạm vi; không sửa bản gốc; ghi audit đọc/tải. |
| UC-C05 | Kiểm tra quyền, masking bản xuất, nhãn/người/thời điểm/mã truy vết, link hết hạn và audit tạo/tải. |
| UC-C06 | Chỉ đọc lịch sử theo quyền; người dùng nghiệp vụ không sửa nhật ký. |
| UC-C07 | Nội bộ áp dụng mọi nhánh: người/thời điểm/hành động/đối tượng/kết quả/trước-sau và bằng chứng liên quan. |
| UC-C08 | Chỉ sự kiện nguồn; thông báo gồm mã tài sản/phát hiện, trạng thái, liên kết, không Attack/Evidence/secret. |
| UC-C09 | Danh sách phát hiện, lịch/lần quét, lô nhập và phiếu theo phạm vi; không tạo phân hệ BI. |
| UC-C10 | Chỉ người có quyền thu hồi trong phạm vi; vô hiệu liên kết và ghi audit; không xóa dữ liệu lịch sử. |

### 5.1. Quy tắc include / extend

- UC-I01 phân rã kiểm tra/chuẩn hóa/lưu/chống trùng bằng include; thứ tự, điểm dừng do trùng/lỗi và kết quả cụ thể nằm ở AD-04/AD-12. Không coi mọi include là một chuỗi luôn chạy hết sau khi đã từ chối.
- UC-T02, UC-T03, UC-T04 mở rộng UC-T01 tại điểm “sau phân loại kết quả”, với điều kiện tương ứng hợp lệ, kết quả sai, bản ghi sai/ngoài phạm vi. UC-T02 còn dùng độc lập để điều chỉnh mức độ đã có, không tự chuyển mọi Finding về Triaged.
- UC-R08 include UC-I01 vì nhập xác minh dùng lại F-03; UC-R10 include đối chiếu fingerprint. “Đóng” không include “tạo yêu cầu xác minh” và “mở lại” không extend “đóng”.
- UC-RA01 có tiền điều kiện Triaged/Assigned/In Progress, không chỉ extend triage. Gia hạn là quyết định mới theo AD-13.
- Các thao tác nhạy cảm sử dụng UC-C07 ghi nhật ký; không include UC xem nhật ký. Điều kiện đủ bằng chứng của R06 là kiểm tra dữ liệu đã có, không bắt người dùng cập nhật lại.

## 6. Dữ liệu phải thể hiện hoặc truy vết

### 6.1. Tài sản — giữ cấu trúc nguồn 5.1.1

| Nhóm | Trường thông tin chính |
| --- | --- |
| Nhận diện | Mã tài sản, tên website, domain, URL gốc, môi trường |
| Sở hữu | Đơn vị, chủ sở hữu, nhóm kỹ thuật, chuyên viên ATTT phụ trách |
| Nghiệp vụ | Mức độ quan trọng, loại dữ liệu xử lý, mức độ phơi nhiễm Internet |
| Phạm vi quét | Domain, subdomain, URL cho phép, URL loại trừ |
| Thời gian | Khung giờ được phép quét, ngày tạo, ngày cập nhật |
| Kiểm soát | Người tạo, người xác nhận, người phê duyệt, trạng thái |

### 6.2. Lịch, lần quét và lô nhập

Lịch gồm mã/tên, tài sản, scope, tần suất (ngày/tuần/tháng/chu kỳ chọn), ngày hiệu lực/kết thúc, múi giờ, ngày giờ dự kiến/khung giờ, nhóm vận hành, ghi chú cấu hình ZAP tham chiếu không chứa mật khẩu/token, trạng thái. Mỗi kỳ có ScanOccurrence riêng; hệ thống không tự khởi động ZAP hay tạo lần quét thay thế khi thất bại/bỏ lỡ.

Lô nhập lưu tài sản, lần quét, phiên bản ZAP, tên mẫu/định dạng, mã/tên phiên quét nếu có, thời gian bắt đầu/kết thúc/nhập, người nhập, RawReport và checksum. Mỗi kết quả truy vết tới Asset, ScanOccurrence, ImportBatch, RawReport và FindingOccurrence.

Khóa lô: `(asset, scan_occurrence, checksum)`. Kiểm tra khóa trước khi tạo bản ghi lô nghiệp vụ mới để nhập lại không tạo thêm lô/occurrence/Finding/phiếu. Thao tác nhập lại chỉ trả kết quả lô hiện có, kể cả trạng thái đang xử lý hoặc lỗi; không tự định nghĩa cơ chế chạy lại ngoài nguồn.

Fingerprint: `(asset, Alert Reference hoặc Rule ID, URL chuẩn hóa, parameter hoặc input_vector, HTTP method khi có)`. Không dùng Attack/Evidence. Tái xuất hiện của Finding đang mở chỉ thêm occurrence/last_seen, không tạo phiếu trùng. Closed chuyển Reopened; False Positive yêu cầu rà soát lại; Risk Accepted tiếp tục cập nhật last_seen.

### 6.3. Ánh xạ ZAP — nguồn 5.3.4

| Trường ZAP | Trường hệ thống | Quy tắc |
| --- | --- | --- |
| Alert Reference hoặc Rule ID | source_alert_ref | Giữ nguyên để truy vết quy tắc ZAP |
| Name | title | Tên phát hiện nguồn |
| URL hoặc Node Name | affected_url | Kiểm tra thuộc phạm vi và chuẩn hóa URL |
| Parameter hoặc Input Vector | parameter, input_vector | Dùng để xác định vị trí ảnh hưởng |
| Risk | source_severity | Giữ nguyên Informational, Low, Medium hoặc High |
| Confidence | source_confidence | Lưu riêng, không thay thế mức độ nghiêm trọng |
| Attack và Evidence | source_attack, source_evidence | Che dữ liệu nhạy cảm trước khi hiển thị |
| Description, Other Info | description, technical_details | Lưu thông tin kỹ thuật nguồn |
| Solution, Reference | remediation_guidance, references | Dùng làm hướng dẫn tham khảo |
| CWE ID, WASC ID | cwe_id, wasc_id | Phục vụ phân loại và báo cáo |
| Alert Tags | source_tags | Lưu thẻ nguồn khi có |

### 6.4. Severity, phiếu và quyết định rủi ro

- `source_severity`, `source_confidence`, `final_severity` tách riêng; `severity_reason`, `assessed_by/at`, `reviewed_by/at` khi cần; trước/sau và thay đổi thời hạn. ZAP Risk chỉ Informational/Low/Medium/High. Final Severity gồm thêm Critical, chỉ ATTT xác nhận theo bằng chứng và bối cảnh.
- Phiếu: tài sản/Finding/vị trí, Final Severity, mô tả/bằng chứng đã che, nhóm/người/hạn, trạng thái/tiến độ/nhận xét, nguyên nhân/biện pháp/evidence, yêu cầu/kết quả xác minh, lịch sử phân công/chuyển trạng thái. Tạo hoặc liên kết phiếu, không nhân đôi theo mỗi occurrence.
- VerificationRequest liên kết Finding, tài sản/phạm vi, nhóm vận hành và lần quét xác minh. Báo cáo mới đi qua F-03; lô lỗi, bị từ chối hoặc đối chiếu không đủ căn cứ không chứng minh đã khắc phục. Với lô một phần, ATTT phải xác nhận kết quả liên quan mục tiêu xác minh hợp lệ; không tự suy ra vắng fingerprint là hết lỗi.
- RiskAcceptance: lý do, tác động, giảm thiểu, người chịu trách nhiệm, người đề nghị, người duyệt, thẩm quyền, điều kiện, hạn, quyết định và lịch sử; gia hạn liên kết quyết định cũ, không ghi đè.
- Thực thể giữ trong nguồn 10: UserRole, Asset/AssetScope, ScanSchedule/ScanOccurrence, ImportBatch/RawReport/ImportError, Finding/FindingOccurrence, SeverityAssessment, RemediationTicket/Assignment, Evidence/VerificationRequest, RiskAcceptance, AuditLog. Không thêm thực thể phân hệ ngoài phạm vi.

## 7. Trạng thái và chuyển trạng thái

### 7.1. Trạng thái các đối tượng

| Đối tượng | Danh sách trạng thái nguồn |
|---|---|
| Asset | Draft, Pending Approval, Active, Suspended, Retired |
| ScanSchedule | Draft, Active, Paused, Expired, Cancelled |
| ScanOccurrence | Planned, Completed, Failed, Missed, Cancelled, Result Imported |
| ImportBatch | Received, Validating, Processing, Completed, Partially Completed, Rejected, Failed |

Danh sách không phải chuỗi bắt buộc. Không suy diễn Expired → Cancelled hoặc khôi phục Retired. Cancelled của lịch/lần quét được giữ trong mô hình dữ liệu; nguồn chưa chỉ định đầy đủ actor/guard thao tác hủy nên không tạo UC cấp quyền hủy mới. Tiếp tục sau tạm dừng phải kiểm tra lại điều kiện, không dùng scope chưa duyệt.

### 7.2. Finding — nguyên văn bảng nguồn 7.1

| Trạng thái | Ý nghĩa | Chủ thể chính |
| --- | --- | --- |
| New | Phát hiện mới nhập, chờ phân tích | Chuyên viên ATTT |
| Triaged | Đã xác nhận kết quả và mức độ | Chuyên viên ATTT |
| Assigned | Đã phân công và có thời hạn | Chuyên viên ATTT |
| In Progress | Đang khắc phục | Nhóm khắc phục |
| Pending Verification | Đã xử lý, chờ quét xác minh | Nhóm vận hành và ATTT |
| Closed | Quét xác minh xác nhận không còn lỗ hổng | Chuyên viên ATTT |
| Reopened | Lỗ hổng còn tồn tại hoặc xuất hiện lại | ATTT và nhóm khắc phục |
| False Positive | Được xác nhận là kết quả sai | Chuyên viên ATTT |
| Pending Risk Acceptance | Chờ quyết định chấp nhận rủi ro | Người phê duyệt rủi ro |
| Risk Accepted | Rủi ro được chấp nhận có thời hạn | Chủ tài sản và người phê duyệt |
| Cancelled | Bản ghi sai hoặc ngoài phạm vi | Chuyên viên ATTT |

### 7.3. Chuyển Finding — giữ nguyên nguồn 7.2

| Từ | Sang | Người thực hiện | Điều kiện bắt buộc |
| --- | --- | --- | --- |
| New | Triaged | Chuyên viên ATTT | Xác nhận tài sản, vị trí, bằng chứng và Final Severity |
| New | False Positive | Chuyên viên ATTT | Có lý do và bằng chứng; Critical/High cần rà soát thứ hai |
| New | Cancelled | Chuyên viên ATTT | Bản ghi sai hoặc ngoài phạm vi; không xóa báo cáo gốc |
| Triaged | Assigned | Chuyên viên ATTT | Có người phụ trách và thời hạn |
| Assigned | In Progress | Nhóm khắc phục | Xác nhận tiếp nhận |
| In Progress | Pending Verification | Nhóm khắc phục | Có biện pháp và bằng chứng khắc phục |
| Pending Verification | Closed | Chuyên viên ATTT | Báo cáo ZAP của lần quét xác minh không còn dấu vân tay phát hiện |
| Pending Verification | Reopened | Chuyên viên ATTT | Kết quả xác minh còn lỗ hổng hoặc không hợp lệ |
| Closed | Reopened | Hệ thống hoặc ATTT | Dấu vân tay xuất hiện trong báo cáo ZAP mới |
| Triaged, Assigned, In Progress | Pending Risk Acceptance | Chủ tài sản hoặc ATTT | Có lý do, ảnh hưởng, biện pháp giảm thiểu và ngày hết hạn đề nghị |
| Pending Risk Acceptance | Risk Accepted | Người phê duyệt rủi ro | Đúng thẩm quyền, có điều kiện và ngày hết hạn |
| Pending Risk Acceptance | Trạng thái trước | Người phê duyệt rủi ro | Từ chối hoặc yêu cầu tiếp tục khắc phục |
| Risk Accepted | Reopened | Hệ thống hoặc ATTT | Hết hạn, vi phạm điều kiện hoặc rủi ro tăng |
| Reopened | Assigned | Chuyên viên ATTT | Phân công lại và xác định thời hạn mới |

Mọi chuyển trạng thái phải ghi trạng thái trước, trạng thái sau, người thực hiện, thời điểm, nhận xét và bằng chứng liên quan.

Không tạo trạng thái “chờ rà soát thứ hai” cho Finding: giữ mức độ/trạng thái hiện hành cho tới kết quả được xác nhận. Gia hạn không tự chuyển Risk Accepted → Pending Risk Acceptance; đề nghị gia hạn là hồ sơ chờ quyết định riêng. Nếu đã Reopened, tiếp tục phân công rồi chỉ đề nghị lại từ trạng thái nguồn cho phép.

## 8. Quy tắc ngoại lệ và những điểm nguồn chưa quy định chi tiết

| Tình huống | Cách biểu diễn trong bộ sơ đồ | Căn cứ / giới hạn |
|---|---|---|
| Tệp/lô sai định dạng, cấu trúc hoặc sai tài sản/phạm vi | Rejected trước ghi Finding | 5.3.6 |
| Một số bản ghi sai cấu trúc/phạm vi, còn bản ghi hợp lệ | Ghi ImportError, giữ bản ghi hợp lệ, Partially Completed; ngoài phạm vi yêu cầu ATTT rà soát | 5.3.6; 7.6 |
| Tất cả bản ghi sai cấu trúc/phạm vi | Kiểm tra trước ghi Finding, Rejected do lô không có nội dung hợp lệ | Cách phân rã kiểm tra 5.3.3/5.3.6; không gán Partially Completed khi không có thành công |
| Tệp hợp lệ không có Alert | Completed, không tạo Finding giả, vòng lặp kiểm tra trước | Không có lỗi bản ghi; hỗ trợ kết quả xác minh không còn phát hiện theo 7.5 |
| Lỗi kỹ thuật ở bất kỳ bước sau tạo lô | Failed, không coi kết quả là xác minh hợp lệ, audit và thông báo | 5.3.6; không dùng Rejected để che lỗi kỹ thuật |
| Nhập lại cùng khóa lô | Trả lô cũ; không tạo dữ liệu trùng | 5.3.5 |
| False Positive tái xuất hiện | Thêm occurrence, last_seen, yêu cầu rà soát và lưu kết luận | 7.3 chưa chỉ định cạnh đảo ngược False Positive; không tự thêm FP → New/Reopened |
| Scope tài sản được đề nghị sửa | Giữ scope đã duyệt; scope đề nghị chỉ áp dụng khi được duyệt lại; nếu tạm dừng thì dùng Suspended | Phân biệt dữ liệu đề nghị với dữ liệu hiệu lực để giữ 5.1.2; không thêm trạng thái Asset |
| Risk Accepted hết hạn / vi phạm / rủi ro tăng | AD-10 xử lý theo sự kiện; chỉ xét quyết định và trạng thái hiện hành | 7.2–7.4; ATTT xác nhận vi phạm/rủi ro tăng, không thêm hệ thống giám sát mới |
| Bổ sung đề nghị rủi ro | Giữ Pending Risk Acceptance, gửi lại để rà soát | 3.2 cho phép yêu cầu bổ sung; từ chối mới khôi phục trạng thái trước theo 7.2 |

Các cách phân rã trên phục vụ dựng sơ đồ từ quy tắc nguồn. Đối với kết quả đảo ngược False Positive hoặc các cạnh hủy/khôi phục vòng đời mà nguồn chưa quy định, bộ sơ đồ chỉ lưu kết luận/yêu cầu liên quan, không đặt thêm cạnh chuyển hoặc quyền thao tác. Không coi việc thiếu quy tắc nguồn là quyền tự mở rộng.

## 9. Bảo mật, báo cáo và thông báo

AD-09 bảo vệ dữ liệu hiển thị, RawReport, evidence, báo cáo xuất; kiểm tra quyền lại khi tải và khi thu hồi, dùng liên kết có hạn/thu hồi được, không có link công khai lâu dài. Bản xuất được che theo quyền người nhận, gắn nhãn nhạy cảm, người xuất, thời điểm, mã truy vết. Audit mọi tạo/tải/thu hồi, đọc/tải bản gốc hoặc bằng chứng nhạy cảm. Cung cấp RawReport nguyên bản chỉ theo quyền riêng; masking áp dụng bản hiển thị/xuất và không thay thế tệp gốc.

AD-16 chỉ thông báo các sự kiện nguồn 9.1: lịch sắp đến hạn/bỏ lỡ; lô Failed/Partially Completed; Critical/High; phiếu mới/sắp hạn/quá hạn; yêu cầu xác minh; đề nghị rủi ro chờ duyệt/sắp hết hạn. Thông báo chỉ chứa mã tài sản, mã phát hiện, trạng thái và liên kết vào hệ thống. Người nhận theo nhiệm vụ/phạm vi, không tự thêm email hay dịch vụ ngoài.

Báo cáo chỉ gồm danh sách nguồn 9.2: Finding theo tài sản/mức độ/trạng thái; lịch và lần quét; lô nhập; phiếu đang xử lý/sắp hạn/quá hạn; phát hiện Closed/Reopened/False Positive/Risk Accepted. Không thêm dashboard phân tích/BI độc lập.

## 10. Danh mục Activity và hợp đồng liên kết

| ID | Tệp | Điều kiện vào | Kết quả |
|---|---|---|---|
| AD-01 | [Quy trình tổng thể](visual_paradigm_uml_md/02_activity_tong_the.md) | Bắt đầu quản lý tài sản hoặc tiếp tục từ quy trình con tương ứng. | Kết quả từng quy trình con được bảo toàn; không bỏ qua guard khi nối luồng. |
| AD-02 | [Khai báo, cập nhật và phê duyệt tài sản](visual_paradigm_uml_md/03_activity_khai_bao_phe_duyet_tai_san.md) | Chủ sở hữu hoặc ATTT có quyền khai báo/cập nhật; tài sản ngừng hoạt động xử lý AD-14. | Draft/Pending Approval/Active; scope mới chỉ được áp dụng khi ATTT duyệt. |
| AD-03 | [Tạo, cập nhật, kích hoạt và tạm dừng lịch](visual_paradigm_uml_md/04_activity_quan_ly_lich_quet.md) | ATTT có quyền quản lý lịch theo tài sản. | Draft/Active/Paused; lần quét được xử lý ở AD-11. |
| AD-04 | [Nhập và kiểm tra báo cáo ZAP](visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) | Ops tải một báo cáo Traditional JSON/XML vào tài sản và lần quét xác định. | Kết quả lô cũ hoặc Completed/Partially Completed/Rejected/Failed; Rejected không tạo Finding. |
| AD-05 | [Phân tích và xác nhận hoặc điều chỉnh Final Severity](visual_paradigm_uml_md/06_activity_phan_tich_final_severity.md) | ATTT có quyền; New khi triage, hoặc Finding đã đánh giá khi điều chỉnh mức độ. | New → Triaged chỉ sau xác nhận hợp lệ; điều chỉnh severity không tự đổi trạng thái workflow. |
| AD-06 | [Khắc phục và quét xác minh](visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) | ATTT tiếp tục Triaged/Reopened; người được giao tiếp tục Assigned/In Progress; xác minh từ Pending Verification. | Closed chỉ do ATTT với ZAP mới hợp lệ; còn lỗi hoặc kết quả không hợp lệ → Reopened. |
| AD-07 | [False Positive và bản ghi sai hoặc ngoài phạm vi](visual_paradigm_uml_md/09_activity_false_positive_ngoai_pham_vi.md) | ATTT xử lý Finding New; hoặc rà soát lại False Positive tái xuất hiện. | New → False Positive/Cancelled theo nguồn; tái rà soát không tự bỏ qua hoặc thêm cạnh trạng thái mới. |
| AD-08 | [Đề nghị và phê duyệt chấp nhận rủi ro](visual_paradigm_uml_md/08_activity_chap_nhan_rui_ro.md) | Chủ tài sản/ATTT đề nghị từ Triaged, Assigned hoặc In Progress. | Risk Accepted hoặc khôi phục trạng thái trước; bổ sung chưa quyết định thì vẫn Pending Risk Acceptance. |
| AD-09 | [Xem dữ liệu, báo cáo gốc, xuất, tải và thu hồi](visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) | Người dùng đã được gán quyền cụ thể; quyền RawReport tách riêng. | Chỉ trả dữ liệu đúng phạm vi; bản gốc bất biến; mọi truy cập nhạy cảm và tạo/tải/thu hồi có audit. |
| AD-10 | [Mở lại sau Closed hoặc hết điều kiện chấp nhận rủi ro](visual_paradigm_uml_md/11_activity_mo_lai_phat_hien.md) | AD-12 báo fingerprint tái xuất hiện; hoặc đến hạn quyết định rủi ro; hoặc ATTT xác nhận vi phạm/rủi ro tăng. | Reopened khi có trigger hợp lệ; nếu không thì giữ trạng thái. Mỗi sự kiện mới gọi lại Activity. |
| AD-11 | [Đến lịch và ghi nhận một lần quét bên ngoài](visual_paradigm_uml_md/12_activity_thuc_hien_lan_quet.md) | Một kỳ kiểm tra lịch; Ops có quyền cập nhật trạng thái lần quét. | Không đến kỳ thì không tạo Planned; Failed/Missed có lý do; thành công chuyển F-03. |
| AD-12 | [Chuẩn hóa và xử lý từng phát hiện nhập](visual_paradigm_uml_md/13_activity_chuan_hoa_va_tai_xuat_hien.md) | Chỉ các bản ghi đã qua kiểm tra của lô không trùng từ AD-04. | Finding/occurrence được truy vết; số liệu xử lý trả AD-04, lỗi kỹ thuật trả nhánh Failed. |
| AD-13 | [Gia hạn bằng quyết định rủi ro mới](visual_paradigm_uml_md/14_activity_gia_han_rui_ro.md) | Có quyết định rủi ro được đề nghị gia hạn. | Quyết định cũ bất biến; quyết định gia hạn chỉ hiệu lực khi được duyệt độc lập. |
| AD-14 | [Tạm dừng, tiếp tục và ngừng tài sản](visual_paradigm_uml_md/15_activity_vong_doi_tai_san.md) | ATTT có quyền trên tài sản; không định nghĩa thêm trạng thái. | Suspended/Retired hoặc Active sau kiểm tra lại; lịch sử được bảo toàn. |
| AD-15 | [Gán, thu hồi quyền và xem nhật ký quyền](visual_paradigm_uml_md/16_activity_phan_quyen.md) | Admin quản trị quyền; kiểm toán xem nhật ký theo quyền. | Thay đổi quyền có audit; mọi truy cập dữ liệu sau đó đánh giá quyền hiện hành. |
| AD-16 | [Thời hạn xử lý và thông báo tối thiểu](visual_paradigm_uml_md/17_activity_thoi_han_thong_bao.md) | Sự kiện phân công, đổi severity/hạn hoặc kỳ kiểm tra dữ liệu nghiệp vụ. | Hạn theo chính sách đơn vị, có lịch sử; thông báo chỉ gồm dữ liệu tối thiểu. |

AD-01 là điều phối; các Activity con có thể được khởi tạo độc lập khi đủ điều kiện, không bắt người dùng làm lại từ khai báo tài sản mỗi lần xử lý. AD-10/AD-11/AD-16 chạy cho mỗi sự kiện/kỳ kiểm tra; kết thúc một lượt không có nghĩa bỏ theo dõi các kỳ sau. AD-08/AD-13 là quy trình con chờ phê duyệt; chúng không đóng lỗ hổng.

## 11. Truy vết quy tắc nguồn

| Mã | Quy tắc |
| --- | --- |
| BR-01 | Chỉ tài sản Active và phạm vi đã phê duyệt mới được lập lịch hoạt động |
| BR-02 | OWASP ZAP thực hiện quét bên ngoài; hệ thống không thực hiện hoặc điều khiển quét |
| BR-03 | Kết quả chỉ được nhập từ báo cáo OWASP ZAP JSON hoặc XML |
| BR-04 | Mỗi kết quả phải truy vết tới tài sản, lần quét, lô nhập và báo cáo gốc |
| BR-05 | Báo cáo gốc bất biến và có kiểm tra toàn vẹn |
| BR-06 | Nhập lại cùng báo cáo không tạo dữ liệu trùng |
| BR-07 | Phát hiện xuất hiện lại sau khi đóng phải được mở lại |
| BR-08 | Risk, Confidence và Final Severity là các trường riêng |
| BR-09 | Điều chỉnh Final Severity phải có lý do và lịch sử |
| BR-10 | Nhóm khắc phục không tự đóng phát hiện |
| BR-11 | Chấp nhận rủi ro có người phê duyệt độc lập và ngày hết hạn |
| BR-12 | Bí mật trong báo cáo ZAP phải được che trước khi hiển thị hoặc xuất |
| BR-13 | Mọi truy cập báo cáo gốc và chuyển trạng thái quan trọng phải được ghi nhật ký |
| BR-14 | Không xóa cứng dữ liệu có giá trị truy vết |
| BR-15 | Nội dung hỗ trợ không được phát triển thành phân hệ ngoài bốn chức năng chính |

| Quy tắc | UC | Activity / ràng buộc |
|---|---|---|
| BR-01 | UC-A04, UC-S03, UC-I03 | AD-02, AD-03, AD-11, AD-14 |
| BR-02 | UC-S06, UC-R08 | AD-11, AD-06 |
| BR-03 | UC-I01, UC-I04 | AD-04 |
| BR-04 | UC-I10, UC-R08 | AD-04, AD-12, AD-06 |
| BR-05 | UC-I06, UC-C04 | AD-04, AD-09 |
| BR-06 | UC-I09 | AD-04, AD-12 |
| BR-07 | UC-R11 | AD-12, AD-10 |
| BR-08 | UC-I08, UC-T02 | AD-12, AD-05 |
| BR-09 | UC-T02, UC-T06, UC-R13 | AD-05, AD-09, AD-16 |
| BR-10 | UC-R06, UC-R10 | AD-06 |
| BR-11 | UC-RA03, UC-RA07 | AD-08, AD-10, AD-13 |
| BR-12 | UC-I07, UC-C05, UC-C08 | AD-04, AD-09, AD-16 |
| BR-13 | UC-C07 | K-02 ở mọi Activity; AD-09, AD-15 |
| BR-14 | UC-A06, UC-C06 | K-04; AD-07, AD-09, AD-14 |
| BR-15 | UC-C01 đến UC-C10, UC-RA01 đến UC-RA07 | UC-00; AD-01, AD-09, AD-15, AD-16 |

## 12. Điều kiện kiểm tra bộ sơ đồ

1. Mọi mã UC xuất hiện trong sơ đồ chi tiết có tên khớp danh mục; các hành vi nội bộ không có actor Hệ thống.
2. Mọi cạnh chuyển Finding khớp nguồn 7.2; việc rà soát lại False Positive không tự thêm cạnh mới.
3. Thiếu bằng chứng không đi tới xác minh; rà soát thứ hai chưa chấp thuận không áp dụng kết quả; báo cáo xác minh không hợp lệ không được đóng.
4. Nhập trùng không tạo dữ liệu mới; báo cáo 0 Alert không tạo dữ liệu giả; lỗi lô, lỗi bản ghi và lỗi kỹ thuật phân biệt đúng.
5. Chấp nhận/gia hạn rủi ro phải đúng thẩm quyền, độc lập, có điều kiện/hạn; pending renewal không kéo dài hạn cũ. Sự kiện vi phạm/rủi ro tăng vẫn được xét.
6. Chưa đến kỳ/lịch không đủ điều kiện không tạo Planned; Fail/Missed có lý do và không tự chạy quét thay thế.
7. Dữ liệu nhạy cảm, quyền RawReport, xuất/tải/thu hồi và audit theo nguồn; các hoạt động hỗ trợ không thành phân hệ mới.
8. Tệp nguồn giữ nguyên checksum; toàn bộ PlantUML dựng được; kiểm tra hình về lane, nhãn nhánh, chữ tiếng Việt và khả năng đọc.

Kết quả kiểm tra và giới hạn công cụ ghi trong `visual_paradigm_uml_md/24_kiem_tra_va_truy_vet.md`. Tài liệu đánh giá trước sửa chỉ là lịch sử, không là bản đặc tả hiện hành.

## 13. Tài liệu giải thích cho từng Use Case và Activity

- [Danh mục 63 tài liệu Use Case](tai_lieu_use_case/README.md).
- [Danh mục 16 tài liệu Activity Diagram](tai_lieu_activity/README.md).

Tài liệu từng UC và AD áp dụng mã, quyền, K-01 đến K-08, dữ liệu, trạng thái và nguồn của đặc tả này. Khi nội dung tóm tắt khác với guard/trình tự trên Activity, sơ đồ Activity và quy tắc nguồn quyết định luồng thực thi; không được dùng tệp giải thích để mở rộng quyền hoặc trạng thái.
