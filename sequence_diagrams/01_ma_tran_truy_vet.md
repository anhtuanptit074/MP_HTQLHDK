# Ma trận truy vết Sequence — Use Case — Activity — Database

Nguồn tên và mã UC: danh mục hiện hành của [đặc tả](../dinh_dang_use_case_activity.md). Có 63 UC; các thao tác nội bộ có thể cùng nằm trong một sequence nghiệp vụ. AD-01 là luồng tổng thể được phân rã thành các sequence dưới đây, không tạo một sequence khổng lồ cho toàn hệ thống.

## 1. Từng Use Case

| Use Case | Tên theo danh mục | Sequence |
|---|---|---|
| UC-A01 | Khai báo tài sản | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md) |
| UC-A02 | Cập nhật thông tin tài sản | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md), [SD-02](specs/SD-02_vong_doi_tai_san.md) |
| UC-A03 | Xác nhận thông tin tài sản | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md) |
| UC-A04 | Phê duyệt phạm vi quét | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md) |
| UC-A05 | Tạm dừng tài sản | [SD-02](specs/SD-02_vong_doi_tai_san.md) |
| UC-A06 | Ngừng hoạt động tài sản | [SD-02](specs/SD-02_vong_doi_tai_san.md) |
| UC-A07 | Xem tài sản theo phạm vi | [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) |
| UC-S01 | Tạo lịch quét | [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| UC-S02 | Cập nhật lịch quét | [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| UC-S03 | Kích hoạt hoặc tiếp tục lịch | [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| UC-S04 | Tạm dừng lịch | [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| UC-S05 | Theo dõi lịch đến hạn | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md) |
| UC-S06 | Cập nhật trạng thái lần quét | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md) |
| UC-S07 | Ghi nhận quét thất bại hoặc bỏ lỡ | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md) |
| UC-S08 | Xác nhận thời gian quét | [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| UC-I01 | Nhập báo cáo ZAP | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) |
| UC-I02 | Kiểm tra quyền nhập | [SD-00](specs/SD-00_kiem_quyen_va_giao_dich.md), [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) |
| UC-I03 | Kiểm tra tài sản và lần quét | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) |
| UC-I04 | Kiểm tra định dạng báo cáo | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) |
| UC-I05 | Kiểm tra phạm vi URL | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) |
| UC-I06 | Lưu báo cáo gốc bất biến | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) |
| UC-I07 | Che dữ liệu nhạy cảm | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) |
| UC-I08 | Chuẩn hóa dữ liệu ZAP | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) |
| UC-I09 | Chống nhập trùng | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) |
| UC-I10 | Tạo hoặc cập nhật phát hiện | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) |
| UC-I11 | Ghi nhận lỗi nhập | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) |
| UC-I12 | Xem kết quả lô nhập | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) |
| UC-T01 | Rà soát phát hiện mới | [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md) |
| UC-T02 | Xác nhận Final Severity | [SD-07](specs/SD-07_phan_tich_final_severity.md) |
| UC-T03 | Xác nhận False Positive | [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md) |
| UC-T04 | Hủy phát hiện sai hoặc ngoài phạm vi | [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md) |
| UC-T05 | Xác nhận ảnh hưởng nghiệp vụ | [SD-07](specs/SD-07_phan_tich_final_severity.md) |
| UC-T06 | Xem lịch sử thay đổi mức độ | [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) |
| UC-R01 | Tạo phiếu khắc phục | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) |
| UC-R02 | Phân công xử lý | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) |
| UC-R03 | Tiếp nhận phiếu | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) |
| UC-R04 | Cập nhật tiến độ | [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md) |
| UC-R05 | Cập nhật biện pháp và bằng chứng | [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md) |
| UC-R06 | Chuyển chờ xác minh | [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md) |
| UC-R07 | Tạo yêu cầu quét xác minh | [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md) |
| UC-R08 | Cập nhật kết quả quét xác minh | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) |
| UC-R09 | Đối chiếu dấu vân tay phát hiện | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) |
| UC-R10 | Đóng phát hiện | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) |
| UC-R11 | Mở lại phát hiện | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md), [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md) |
| UC-R12 | Theo dõi hạn xử lý | [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| UC-R13 | Điều chỉnh phân công hoặc thời hạn | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) |
| UC-RA01 | Tạo đề nghị chấp nhận rủi ro | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md) |
| UC-RA02 | Rà soát đề nghị | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md), [SD-13](specs/SD-13_gia_han_rui_ro.md) |
| UC-RA03 | Phê duyệt chấp nhận rủi ro | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md), [SD-13](specs/SD-13_gia_han_rui_ro.md) |
| UC-RA04 | Từ chối hoặc yêu cầu bổ sung | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md), [SD-13](specs/SD-13_gia_han_rui_ro.md) |
| UC-RA05 | Theo dõi ngày hết hạn | [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md) |
| UC-RA06 | Mở lại khi hết điều kiện rủi ro | [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md) |
| UC-RA07 | Gia hạn chấp nhận rủi ro | [SD-13](specs/SD-13_gia_han_rui_ro.md) |
| UC-C01 | Gán vai trò và phạm vi tài sản | [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md) |
| UC-C02 | Thu hồi quyền | [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md) |
| UC-C03 | Xem nhật ký thay đổi quyền | [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md) |
| UC-C04 | Xem báo cáo gốc theo quyền | [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) |
| UC-C05 | Xuất hoặc tải báo cáo | [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) |
| UC-C06 | Xem lịch sử nghiệp vụ | [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) |
| UC-C07 | Ghi nhật ký hành động nhạy cảm | [SD-00](specs/SD-00_kiem_quyen_va_giao_dich.md) |
| UC-C08 | Nhận thông báo nghiệp vụ | [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| UC-C09 | Xem dữ liệu nghiệp vụ theo phạm vi | [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) |
| UC-C10 | Thu hồi liên kết báo cáo | [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) |

## 2. Từng Activity

| Activity | Sequence |
|---|---|
| [AD-02](../visual_paradigm_uml_md/03_activity_khai_bao_phe_duyet_tai_san.md) | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md) |
| [AD-03](../visual_paradigm_uml_md/04_activity_quan_ly_lich_quet.md) | [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| [AD-04](../visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md) | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) |
| [AD-05](../visual_paradigm_uml_md/06_activity_phan_tich_final_severity.md) | [SD-07](specs/SD-07_phan_tich_final_severity.md) |
| [AD-06](../visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md) | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md), [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) |
| [AD-07](../visual_paradigm_uml_md/09_activity_false_positive_ngoai_pham_vi.md) | [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md) |
| [AD-08](../visual_paradigm_uml_md/08_activity_chap_nhan_rui_ro.md) | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md) |
| [AD-09](../visual_paradigm_uml_md/10_activity_bao_mat_bao_cao.md) | [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) |
| [AD-10](../visual_paradigm_uml_md/11_activity_mo_lai_phat_hien.md) | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md), [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md) |
| [AD-11](../visual_paradigm_uml_md/12_activity_thuc_hien_lan_quet.md) | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md) |
| [AD-12](../visual_paradigm_uml_md/13_activity_chuan_hoa_va_tai_xuat_hien.md) | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) |
| [AD-13](../visual_paradigm_uml_md/14_activity_gia_han_rui_ro.md) | [SD-13](specs/SD-13_gia_han_rui_ro.md) |
| [AD-14](../visual_paradigm_uml_md/15_activity_vong_doi_tai_san.md) | [SD-02](specs/SD-02_vong_doi_tai_san.md) |
| [AD-15](../visual_paradigm_uml_md/16_activity_phan_quyen.md) | [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md) |
| [AD-16](../visual_paradigm_uml_md/17_activity_thoi_han_thong_bao.md) | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |

## 3. Từng bảng dữ liệu

Tên bảng đối chiếu trực tiếp với [01_schema.sql](../database/sql/01_schema.sql). Bảng ở đây có thể được đọc để kiểm điều kiện, không nhất thiết được ghi trong mọi nhánh.

| Bảng | Sequence sử dụng |
|---|---|
| `app_user` | [SD-00](specs/SD-00_kiem_quyen_va_giao_dich.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md), [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md) |
| `team` | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) |
| `team_member` | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) |
| `user_role` | [SD-00](specs/SD-00_kiem_quyen_va_giao_dich.md), [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md), [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md) |
| `asset` | [SD-00](specs/SD-00_kiem_quyen_va_giao_dich.md), [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md), [SD-02](specs/SD-02_vong_doi_tai_san.md), [SD-03](specs/SD-03_quan_ly_lich_quet.md), [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md), [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md) |
| `asset_scope_revision` | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md), [SD-02](specs/SD-02_vong_doi_tai_san.md), [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| `asset_scope_rule` | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md), [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| `asset_scan_window` | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md), [SD-03](specs/SD-03_quan_ly_lich_quet.md) |
| `scan_schedule` | [SD-02](specs/SD-02_vong_doi_tai_san.md), [SD-03](specs/SD-03_quan_ly_lich_quet.md), [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `scan_occurrence` | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md), [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `import_batch` | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `import_error` | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) |
| `finding` | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md), [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md), [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md), [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md), [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md), [SD-13](specs/SD-13_gia_han_rui_ro.md), [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `finding_occurrence` | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) |
| `finding_observation` | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md), [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) |
| `evidence` | [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md), [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) |
| `finding_review` | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md), [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) |
| `sla_rule` | [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `remediation_ticket` | [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md), [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `ticket_event` | [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md), [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `verification_request` | [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) |
| `verification_result` | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) |
| `risk_acceptance` | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md), [SD-13](specs/SD-13_gia_han_rui_ro.md), [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `audit_log` | [SD-00](specs/SD-00_kiem_quyen_va_giao_dich.md), [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md), [SD-02](specs/SD-02_vong_doi_tai_san.md), [SD-03](specs/SD-03_quan_ly_lich_quet.md), [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md), [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md), [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md), [SD-07](specs/SD-07_phan_tich_final_severity.md), [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md), [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md), [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md), [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md), [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md), [SD-13](specs/SD-13_gia_han_rui_ro.md), [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md), [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md), [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md), [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |
| `report_export` | [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) |
| `report_export_asset` | [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) |
| `notification` | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md), [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md), [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md), [SD-13](specs/SD-13_gia_han_rui_ro.md), [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md), [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) |

[Danh mục](README.md) · [Quy ước và giới hạn](00_quy_uoc_va_thanh_phan.md)
