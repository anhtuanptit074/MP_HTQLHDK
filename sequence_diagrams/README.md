# Tài liệu thiết kế Sequence Diagram — HTQLHDK

Phiên bản 1.0 · Ngày 05/09/2026

Bộ tài liệu gồm **18 sơ đồ nghiệp vụ và 1 sơ đồ kiểm quyền/giao dịch dùng chung**, phục vụ vẽ UML Sequence Diagram và làm rõ tương tác giữa giao diện, dịch vụ nghiệp vụ, database, kho tệp và người sử dụng.

Nguồn đối chiếu: [nghiệp vụ v2.2](../doi_tuong_sd_quy_trinh_hd.md), [Use Case/Activity hiện hành](../dinh_dang_use_case_activity.md), [database v2.0 gồm 27 bảng](../database/README.md). Tên UC, trạng thái và tên bảng giữ theo tài liệu hiện có.

**Ranh giới đã chốt:** nhóm vận hành chạy OWASP ZAP bên ngoài và tải tệp Traditional JSON/XML vào hệ thống. Bộ sơ đồ không thiết kế lời gọi backend điều khiển ZAP. Các thành phần dịch vụ là cách phân chia trách nhiệm trong ứng dụng, không yêu cầu kiến trúc microservice.

## Cách dùng

1. Đọc [quy ước và thành phần](00_quy_uoc_va_thanh_phan.md), sau đó xem SD-00 để hiểu kiểm quyền và giao dịch chung.
2. Chọn nghiệp vụ ở bảng dưới. Mỗi đặc tả có tiền điều kiện, dữ liệu vào/ra, thứ tự lifeline, trình tự, nhánh lỗi, bảng dữ liệu liên quan và mã PlantUML.
3. Dùng PNG để chèn báo cáo; SVG để phóng to. Mở `.puml` hoặc `.mmd` bằng công cụ hỗ trợ định dạng tương ứng.
4. Nếu dựng thủ công trong Visual Paradigm: tạo UML Sequence Diagram, đặt actor/lifeline theo thứ tự trong đặc tả, nối thông điệp từ trên xuống và dùng Combined Fragment cho các nhánh `alt`, `opt`, `loop`. `ref` gọi lại sơ đồ đã nêu và giữ đầy đủ điều kiện của luồng được gọi.

Không khẳng định mã PlantUML/Mermaid có thể nhập trực tiếp thành phần tử UML chỉnh sửa được trong mọi phiên bản Visual Paradigm. Bộ này chưa tạo dự án `.vpp`.

## Danh mục

| Mã | Nghiệp vụ | Use Case | Xem hình |
|---|---|---|---|
| SD-00 | [Kiểm quyền và giao dịch dùng chung](specs/SD-00_kiem_quyen_va_giao_dich.md) | UC-I02, UC-C07 | [PNG](rendered/SD-00_kiem_quyen_va_giao_dich.png) · [SVG](rendered/SD-00_kiem_quyen_va_giao_dich.svg) |
| SD-01 | [Khai báo, cập nhật và phê duyệt tài sản](specs/SD-01_khai_bao_phe_duyet_tai_san.md) | UC-A01, UC-A02, UC-A03, UC-A04 | [PNG](rendered/SD-01_khai_bao_phe_duyet_tai_san.png) · [SVG](rendered/SD-01_khai_bao_phe_duyet_tai_san.svg) |
| SD-02 | [Tạm dừng, tiếp tục và ngừng tài sản](specs/SD-02_vong_doi_tai_san.md) | UC-A02, UC-A05, UC-A06 | [PNG](rendered/SD-02_vong_doi_tai_san.png) · [SVG](rendered/SD-02_vong_doi_tai_san.svg) |
| SD-03 | [Tạo, sửa, kích hoạt và tạm dừng lịch quét](specs/SD-03_quan_ly_lich_quet.md) | UC-S01, UC-S02, UC-S03, UC-S04, UC-S08 | [PNG](rendered/SD-03_quan_ly_lich_quet.png) · [SVG](rendered/SD-03_quan_ly_lich_quet.svg) |
| SD-04 | [Đến lịch và ghi nhận lần quét bên ngoài](specs/SD-04_den_lich_quet_ben_ngoai.md) | UC-S05, UC-S06, UC-S07 | [PNG](rendered/SD-04_den_lich_quet_ben_ngoai.png) · [SVG](rendered/SD-04_den_lich_quet_ben_ngoai.svg) |
| SD-05 | [Nhập và kiểm tra báo cáo OWASP ZAP](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) | UC-I01, UC-I02, UC-I03, UC-I04, UC-I05, UC-I06, UC-I07, UC-I09, UC-I11, UC-I12 | [PNG](rendered/SD-05_nhap_kiem_tra_bao_cao_zap.png) · [SVG](rendered/SD-05_nhap_kiem_tra_bao_cao_zap.svg) |
| SD-06 | [Chuẩn hóa, chống trùng và xử lý tái xuất hiện](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) | UC-I07, UC-I08, UC-I09, UC-I10, UC-I11, UC-R11 | [PNG](rendered/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.png) · [SVG](rendered/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.svg) |
| SD-07 | [Phân tích, xác nhận và điều chỉnh Final Severity](specs/SD-07_phan_tich_final_severity.md) | UC-T01, UC-T02, UC-T05, UC-T06 | [PNG](rendered/SD-07_phan_tich_final_severity.png) · [SVG](rendered/SD-07_phan_tich_final_severity.svg) |
| SD-08 | [Kết quả sai, ngoài phạm vi và rà soát lại](specs/SD-08_false_positive_ngoai_pham_vi.md) | UC-T01, UC-T03, UC-T04, UC-T06 | [PNG](rendered/SD-08_false_positive_ngoai_pham_vi.png) · [SVG](rendered/SD-08_false_positive_ngoai_pham_vi.svg) |
| SD-09 | [Tạo phiếu, phân công, tiếp nhận và điều chỉnh hạn](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) | UC-R01, UC-R02, UC-R03, UC-R13 | [PNG](rendered/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.png) · [SVG](rendered/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.svg) |
| SD-10 | [Cập nhật khắc phục và yêu cầu xác minh](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md) | UC-R04, UC-R05, UC-R06, UC-R07 | [PNG](rendered/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.png) · [SVG](rendered/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.svg) |
| SD-11 | [Quét xác minh, đóng hoặc mở lại phát hiện](specs/SD-11_quet_xac_minh_dong_mo_lai.md) | UC-R08, UC-R09, UC-R10, UC-R11 | [PNG](rendered/SD-11_quet_xac_minh_dong_mo_lai.png) · [SVG](rendered/SD-11_quet_xac_minh_dong_mo_lai.svg) |
| SD-12 | [Đề nghị và phê duyệt chấp nhận rủi ro](specs/SD-12_de_nghi_phe_duyet_rui_ro.md) | UC-RA01, UC-RA02, UC-RA03, UC-RA04 | [PNG](rendered/SD-12_de_nghi_phe_duyet_rui_ro.png) · [SVG](rendered/SD-12_de_nghi_phe_duyet_rui_ro.svg) |
| SD-13 | [Gia hạn bằng quyết định rủi ro mới](specs/SD-13_gia_han_rui_ro.md) | UC-RA02, UC-RA03, UC-RA04, UC-RA07 | [PNG](rendered/SD-13_gia_han_rui_ro.png) · [SVG](rendered/SD-13_gia_han_rui_ro.svg) |
| SD-14 | [Hết điều kiện chấp nhận rủi ro và mở lại](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md) | UC-RA05, UC-RA06, UC-R11 | [PNG](rendered/SD-14_het_dieu_kien_rui_ro_mo_lai.png) · [SVG](rendered/SD-14_het_dieu_kien_rui_ro_mo_lai.svg) |
| SD-15 | [Xem dữ liệu, lịch sử và báo cáo gốc](specs/SD-15_xem_du_lieu_bao_cao_goc.md) | UC-A07, UC-I12, UC-T06, UC-C04, UC-C06, UC-C09 | [PNG](rendered/SD-15_xem_du_lieu_bao_cao_goc.png) · [SVG](rendered/SD-15_xem_du_lieu_bao_cao_goc.svg) |
| SD-16 | [Xuất, tải và thu hồi liên kết báo cáo](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) | UC-C05, UC-C10 | [PNG](rendered/SD-16_xuat_tai_thu_hoi_bao_cao.png) · [SVG](rendered/SD-16_xuat_tai_thu_hoi_bao_cao.svg) |
| SD-17 | [Gán, thu hồi quyền và xem nhật ký quyền](specs/SD-17_gan_thu_hoi_quyen_audit.md) | UC-C01, UC-C02, UC-C03 | [PNG](rendered/SD-17_gan_thu_hoi_quyen_audit.png) · [SVG](rendered/SD-17_gan_thu_hoi_quyen_audit.svg) |
| SD-18 | [Theo dõi thời hạn và thông báo nội bộ](specs/SD-18_thoi_han_va_thong_bao.md) | UC-R12, UC-C08 | [PNG](rendered/SD-18_thoi_han_va_thong_bao.png) · [SVG](rendered/SD-18_thoi_han_va_thong_bao.svg) |

## Thứ tự nên đọc và vẽ

Luồng chính: **SD-01 → SD-03 → SD-04 → SD-05 → SD-06 → SD-07 → SD-09 → SD-10 → SD-11**.

Các nhánh bổ sung: SD-02 vòng đời tài sản; SD-08 kết quả sai; SD-12–14 rủi ro; SD-15–17 quyền và báo cáo; SD-18 thông báo. SD-00 áp dụng xuyên suốt.

Một sequence có thể bao phủ nhiều UC liên quan; một UC có thể xuất hiện ở nhiều sequence. Không cần ép 63 UC thành 63 hình riêng. [Ma trận truy vết](01_ma_tran_truy_vet.md) cho biết từng UC được thể hiện ở đâu.

## Các tệp bàn giao

| Tệp/thư mục | Nội dung |
|---|---|
| `specs/` | 19 đặc tả chi tiết kèm PlantUML và hình |
| `plantuml/` | 19 mã `.puml` để chỉnh sửa/dựng hình |
| `mermaid/` | 19 mã `.mmd` cùng trình tự thông điệp và nhánh |
| `rendered/` | SVG và PNG dựng từ PlantUML |
| [Quy ước](00_quy_uoc_va_thanh_phan.md) | Ký hiệu, lifeline, quyền, giao dịch và ranh giới thiết kế |
| [Truy vết](01_ma_tran_truy_vet.md) | Từng UC, Activity và 27 bảng được sử dụng ở sơ đồ nào |
| [Tình huống kiểm tra](02_tinh_huong_kiem_tra.md) | Các đường đi và kết quả cần đối chiếu khi vẽ/triển khai |
| [Kết quả kiểm tra](03_ket_qua_kiem_tra.md) | Kiểm mã, dựng hình, liên kết và giới hạn kiểm chứng |

Đây là tài liệu thiết kế tương tác. Parser ZAP, phân quyền máy chủ, kho tệp, workflow và giao dịch được mô tả chưa phải chức năng đã lập trình hoặc kiểm thử vận hành.
