# UC-I01 — Nhập báo cáo ZAP

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-I01` |
| Nhóm | `I` |
| Tác nhân trực tiếp | Nhóm vận hành quét |
| Nguồn nghiệp vụ | Mục 5.3 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-04](../tai_lieu_activity/AD-04_nhap_va_kiem_tra_bao_cao_zap.md) |
| Sơ đồ Use Case | [19_use_case_nhap_bao_cao_zap](../visual_paradigm_uml_md/19_use_case_nhap_bao_cao_zap.md), [21_use_case_khac_phuc_xac_minh](../visual_paradigm_uml_md/21_use_case_khac_phuc_xac_minh.md) |

**Mục tiêu:** Tải Traditional JSON/XML vào một tài sản và một lần quét; trả kết quả lô hoặc tham chiếu lô cũ.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Nhóm vận hành quét.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Nhập báo cáo ZAP”.

**Tiền điều kiện:** Có ngữ cảnh tài sản, lần quét và lô nhập; với UC do người dùng khởi tạo, Ops phải có quyền nhập trên tài sản.

**Dữ liệu vào/ra liên quan:** Tài sản, ScanOccurrence, tệp Traditional JSON/XML, checksum, phiên bản/mẫu ZAP, thời gian quét/nhập, Alert và các trường ánh xạ.

**Thực thể:** ImportBatch, RawReport, ImportError, Finding và FindingOccurrence.

## 4. Luồng chính

1. Ops chọn đúng Asset/ScanOccurrence và tải Traditional JSON/XML cùng dữ liệu truy vết.
2. Hệ thống gọi UC-I02–UC-I05 và tính khóa lô trước khi ghi dữ liệu mới.
3. Nếu không trùng và hợp lệ, lưu RawReport bất biến rồi gọi UC-I07–UC-I10.
4. Tổng hợp Completed/Partially Completed/Rejected/Failed, cập nhật Result Imported khi phù hợp và trả kết quả.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Lô đã tồn tại | Trả kết quả lô cũ, không tạo dữ liệu trùng. |
| Sai tệp/phạm vi | Rejected trước tạo Finding. |
| Lỗi kỹ thuật | Failed; không dùng làm kết quả xác minh hợp lệ. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Tải Traditional JSON/XML vào một tài sản và một lần quét; trả kết quả lô hoặc tham chiếu lô cũ.
- Không sửa Risk/Confidence nguồn, RawReport hoặc AuditLog ngoài các thao tác nguồn cho phép.
- Không xóa cứng dữ liệu có giá trị truy vết.
- Mọi quyền được kiểm tra tại thời điểm thao tác; quyền đã thu hồi không tiếp tục có hiệu lực.

## 7. Quyền, bảo mật và audit

- K-01: xác thực, kiểm tra role + asset scope và quyền riêng phía máy chủ trước đọc/ghi.
- K-02: ghi người, thời điểm, hành động, đối tượng, kết quả; trước/sau và evidence/nhận xét khi phù hợp.
- Dữ liệu nhạy cảm được che khi hiển thị/xuất; RawReport bất biến và cần quyền riêng.
- Thông báo không chứa Attack, Evidence, token, cookie, mật khẩu hoặc bí mật khác.

## 8. Quan hệ và truy vết

`include` đi tới [UC-I02](UC-I02_kiem_tra_quyen_nhap.md); `include` đi tới [UC-I03](UC-I03_kiem_tra_tai_san_va_lan_quet.md); `include` đi tới [UC-I04](UC-I04_kiem_tra_dinh_dang_bao_cao.md); `include` đi tới [UC-I05](UC-I05_kiem_tra_pham_vi_url.md); `include` đi tới [UC-I06](UC-I06_luu_bao_cao_goc_bat_bien.md); `include` đi tới [UC-I07](UC-I07_che_du_lieu_nhay_cam.md); `include` đi tới [UC-I08](UC-I08_chuan_hoa_du_lieu_zap.md); `include` đi tới [UC-I09](UC-I09_chong_nhap_trung.md); `include` đi tới [UC-I10](UC-I10_tao_hoac_cap_nhat_phat_hien.md); `extend` được gọi từ [UC-I11](UC-I11_ghi_nhan_loi_nhap.md) với điều kiện `[lỗi nhập]`; `include` được gọi từ [UC-R08](UC-R08_cap_nhat_ket_qua_quet_xac_minh.md).

Luồng thực thi chi tiết: [AD-04](../tai_lieu_activity/AD-04_nhap_va_kiem_tra_bao_cao_zap.md). Mã PlantUML/sơ đồ: [19_use_case_nhap_bao_cao_zap](../visual_paradigm_uml_md/19_use_case_nhap_bao_cao_zap.md), [21_use_case_khac_phuc_xac_minh](../visual_paradigm_uml_md/21_use_case_khac_phuc_xac_minh.md).

## 9. Tiêu chí kiểm tra

- [ ] Tải Traditional JSON/XML vào một tài sản và một lần quét; trả kết quả lô hoặc tham chiếu lô cũ.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
