# Tình huống đối chiếu khi vẽ và triển khai

Danh sách này là **kịch bản kiểm tra thiết kế**, chưa phải kết quả kiểm thử phần mềm. Dùng để rà soát guard, nhánh lỗi, audit và trạng thái đầu ra. Khi lập trình, có thể chuyển các tình huống có rủi ro thành kiểm thử API/giao dịch phù hợp.

| Mã | Sequence | Tình huống | Kết quả mong đợi |
|---|---|---|---|
| SQ-T01 | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md) | Mã tài sản trùng | Trả lỗi; không thêm asset trùng mã. |
| SQ-T02 | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md) | Domain trùng nhưng mã khác | Cảnh báo để rà soát; không tự chặn chỉ vì domain trùng. |
| SQ-T03 | [SD-01](specs/SD-01_khai_bao_phe_duyet_tai_san.md) | Scope mới chưa duyệt | Giữ scope có hiệu lực; không dùng bản đề nghị cho lịch/nhập. |
| SQ-T04 | [SD-02](specs/SD-02_vong_doi_tai_san.md) | Tài sản Suspended/Retired | Không kích hoạt lịch/nhập kết quả; giữ lịch sử. |
| SQ-T05 | [SD-03](specs/SD-03_quan_ly_lich_quet.md) | Mục tiêu thuộc Exclude hoặc ngoài scope | Không áp dụng lịch. |
| SQ-T06 | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md) | Chưa đến kỳ hoặc lịch không Active | Không tạo Planned. |
| SQ-T07 | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md) | Hai tác vụ cùng xử lý một kỳ | Khóa lịch và kiểm kỳ; chỉ tạo một Planned. |
| SQ-T08 | [SD-04](specs/SD-04_den_lich_quet_ben_ngoai.md) | Ops ghi Failed/Missed | Bắt buộc lý do; không tự chạy lại ZAP. |
| SQ-T09 | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) | Nhập lại cùng asset/scan/checksum, kể cả đồng thời | Trả lô cũ, không thêm dữ liệu; kiểm quyền trước trả. |
| SQ-T10 | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) | HTML/PDF/Markdown làm nguồn nhập | Từ chối tạo Finding; chỉ JSON/XML Traditional được parse. |
| SQ-T11 | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) | Tệp sai toàn cục hoặc tất cả dòng sai | Rejected trước tạo Finding. |
| SQ-T12 | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) | Báo cáo hợp lệ 0 Alert | Completed; tổng 0; không tạo Finding giả. |
| SQ-T13 | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) | Có dòng đúng và lỗi | Partially Completed; giữ Observation và ImportError phù hợp. |
| SQ-T14 | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) | Lỗi kỹ thuật sau khi vài dòng commit | Failed; giữ truy vết, không dùng để xác minh hợp lệ. |
| SQ-T15 | [SD-05](specs/SD-05_nhap_kiem_tra_bao_cao_zap.md) | Kho tệp thành công nhưng metadata rollback | Không phát hành tệp/link; đối soát tệp chưa liên kết. |
| SQ-T16 | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) | Hai báo cáo khác nhau cùng scan chứa cùng fingerprint | Một Occurrence, nhiều Observation theo lô/vị trí. |
| SQ-T17 | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) | Nhập muộn báo cáo cũ sau khi Finding Closed | Không mở lại nếu không có lần xuất hiện mới sau đóng; không lùi last_seen. |
| SQ-T18 | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) | Fingerprint tái xuất hiện sau Closed | Reopened; audit liên kết occurrence mới. |
| SQ-T19 | [SD-06](specs/SD-06_chuan_hoa_chong_trung_tai_xuat_hien.md) | False Positive tái xuất hiện | Thêm Recheck; giữ trạng thái, không tự bỏ qua. |
| SQ-T20 | [SD-07](specs/SD-07_phan_tich_final_severity.md) | Nguồn High, Confidence cao | Không tự gán Final Severity Critical. |
| SQ-T21 | [SD-07](specs/SD-07_phan_tich_final_severity.md) | Hạ Critical/High | Chờ người thứ hai đúng thẩm quyền; chưa áp dụng khi chưa duyệt. |
| SQ-T22 | [SD-07](specs/SD-07_phan_tich_final_severity.md) | Đề nghị mức độ dựa trên current_review_id cũ | Không ghi đè; yêu cầu rà soát lại. |
| SQ-T23 | [SD-07](specs/SD-07_phan_tich_final_severity.md) | Đổi mức ảnh hưởng hạn nhưng thiếu SLA được duyệt | Chưa áp dụng giao dịch thay mức/hạn; không tự đặt số ngày. |
| SQ-T24 | [SD-08](specs/SD-08_false_positive_ngoai_pham_vi.md) | False Positive High khi chưa có mức cuối | Dùng mức nguồn để yêu cầu rà soát độc lập. |
| SQ-T25 | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) | Phân công lại Reopened | Có người chịu trách nhiệm và hạn mới; không tự nhân đôi phiếu. |
| SQ-T26 | [SD-09](specs/SD-09_phan_cong_tiep_nhan_dieu_chinh_han.md) | Thay hạn sau khi đã có bằng chứng khắc phục | Giữ nguyên tác giả/thời điểm bản sửa và evidence trong snapshot mới. |
| SQ-T27 | [SD-10](specs/SD-10_cap_nhat_khac_phuc_yeu_cau_xac_minh.md) | Tiến độ 100% nhưng thiếu evidence/biện pháp | Giữ In Progress, yêu cầu bổ sung. |
| SQ-T28 | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) | Người thực sự sửa tự xác nhận đóng | Từ chối; không chuyển Closed. |
| SQ-T29 | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) | Lô partial thiếu vùng mục tiêu xác minh | Không coi vắng fingerprint là hết lỗ hổng; ATTT kết luận Reopened nếu đánh giá không hợp lệ. |
| SQ-T30 | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) | Scan sau khắc phục nhưng trước ngày tạo yêu cầu | Không tự từ chối chỉ vì requested_at; xét tính mới và chu kỳ hiện hành. |
| SQ-T31 | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) | Kết quả quét hợp lệ, bao phủ đủ, không fingerprint | ATTT xác nhận; Result + Finding Closed + request Evaluated + audit cùng TX. |
| SQ-T32 | [SD-11](specs/SD-11_quet_xac_minh_dong_mo_lai.md) | Dùng kết quả chu kỳ cũ cho Finding mới mở lại | Từ chối áp dụng. |
| SQ-T33 | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md) | Người đề nghị tự duyệt rủi ro | Từ chối và audit. |
| SQ-T34 | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md) | Người duyệt yêu cầu bổ sung | Hồ sơ Needs Information; Finding vẫn Pending Risk Acceptance. |
| SQ-T35 | [SD-12](specs/SD-12_de_nghi_phe_duyet_rui_ro.md) | Từ chối đề nghị | Khôi phục đúng previous_finding_status của hồ sơ được áp dụng. |
| SQ-T36 | [SD-13](specs/SD-13_gia_han_rui_ro.md) | Gia hạn đang chờ, hạn cũ đã đến | Không kéo dài hạn; SD-14 xét quyết định hiện hành và mở lại. |
| SQ-T37 | [SD-14](specs/SD-14_het_dieu_kien_rui_ro_mo_lai.md) | Sự kiện hết hạn của quyết định đã được thay bằng gia hạn hợp lệ | Không áp sự kiện cũ; xét quyết định mới nhất đã áp dụng. |
| SQ-T38 | [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) | Admin chỉ có quyền quản trị, cố đọc raw | Từ chối; không có quyền raw mặc định. |
| SQ-T39 | [SD-15](specs/SD-15_xem_du_lieu_bao_cao_goc.md) | Checksum bản gốc không khớp | Không trả byte tệp; audit lỗi. |
| SQ-T40 | [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) | Token đúng nhưng sai người nhận/hết hạn/thu hồi | Từ chối tải. |
| SQ-T41 | [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) | Mất quyền một asset hoặc phiếu trong bản xuất nhiều tài sản | Từ chối toàn bộ tệp; cần bản xuất mới phù hợp. |
| SQ-T42 | [SD-16](specs/SD-16_xuat_tai_thu_hoi_bao_cao.md) | Audit tải lỗi | Không phát hành dữ liệu nhạy cảm. |
| SQ-T43 | [SD-17](specs/SD-17_gan_thu_hoi_quyen_audit.md) | Thu hồi grant sau khi đã tạo link | Lượt tải tiếp theo kiểm quyền hiện hành và bị từ chối. |
| SQ-T44 | [SD-18](specs/SD-18_thoi_han_va_thong_bao.md) | Tác vụ thông báo chạy lặp lại | Không tạo trùng recipient + dedupe_key; không chứa secret. |
| SQ-T45 | [SD-00](specs/SD-00_kiem_quyen_va_giao_dich.md) | Lỗi audit trong giao dịch đổi trạng thái | Rollback nghiệp vụ, ghi lỗi bằng cơ chế riêng. |
| SQ-T46 | [SD-00](specs/SD-00_kiem_quyen_va_giao_dich.md) | Quyền bị thu hồi hoặc phiên bản thay đổi trước áp dụng | Từ chối hoặc yêu cầu tải lại; không ghi đè. |

[Danh mục](README.md) · [Kết quả kiểm mã và dựng hình](03_ket_qua_kiem_tra.md)
