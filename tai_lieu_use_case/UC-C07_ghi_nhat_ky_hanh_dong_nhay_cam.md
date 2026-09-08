# UC-C07 — Ghi nhật ký hành động nhạy cảm

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-C07` |
| Nhóm | `C` |
| Tác nhân trực tiếp | Không có actor trực tiếp; hành vi nội bộ do UC cha gọi |
| Nguồn nghiệp vụ | Mục 7.2; 8.5 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-01](../tai_lieu_activity/AD-01_quy_trinh_tong_the.md), [AD-09](../tai_lieu_activity/AD-09_xem_du_lieu_bao_cao_goc_xuat_tai_va_thu_hoi.md), [AD-15](../tai_lieu_activity/AD-15_gan_thu_hoi_quyen_va_xem_nhat_ky_quyen.md) |
| Sơ đồ Use Case | [23_use_case_quyen_bao_cao_audit](../visual_paradigm_uml_md/23_use_case_quyen_bao_cao_audit.md) |

**Mục tiêu:** Nội bộ áp dụng mọi nhánh: người/thời điểm/hành động/đối tượng/kết quả/trước-sau và bằng chứng liên quan.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Không có actor trực tiếp; hành vi nội bộ do UC cha gọi.

Đây là xử lý nội bộ của hệ thống, không vẽ “Hệ thống” như một actor. Xử lý chỉ chạy trong ngữ cảnh UC cha đã kiểm tra quyền; không thể được gọi để bỏ qua K-01.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Một UC/Activity liên quan gọi xử lý “Ghi nhật ký hành động nhạy cảm”.

**Tiền điều kiện:** Người dùng đã xác thực; mọi thao tác kiểm tra lại role, asset scope và quyền riêng tại máy chủ.

**Dữ liệu vào/ra liên quan:** Người dùng/vai trò/phạm vi, loại dữ liệu/thao tác, RawReport hoặc bản xuất, nhãn/mã truy vết/link có hạn và bản ghi audit.

**Thực thể:** UserRole, phạm vi tài sản, dữ liệu báo cáo và AuditLog.

## 4. Luồng chính

1. UC cha phát sinh hành động thuộc danh sách audit bắt buộc.
2. Hệ thống tạo AuditLog với người/thời điểm/hành động/đối tượng/kết quả.
3. Ghi trước/sau, nhận xét/evidence liên quan khi phù hợp.
4. Thất bại/từ chối cũng được ghi; không phụ thuộc UC xem nhật ký.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Quyền bị thu hồi hoặc link hết hạn/đã thu hồi | Từ chối, audit; không cung cấp dữ liệu. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Nội bộ áp dụng mọi nhánh: người/thời điểm/hành động/đối tượng/kết quả/trước-sau và bằng chứng liên quan.
- Không sửa Risk/Confidence nguồn, RawReport hoặc AuditLog ngoài các thao tác nguồn cho phép.
- Không xóa cứng dữ liệu có giá trị truy vết.
- Mọi quyền được kiểm tra tại thời điểm thao tác; quyền đã thu hồi không tiếp tục có hiệu lực.

## 7. Quyền, bảo mật và audit

- K-01: xác thực, kiểm tra role + asset scope và quyền riêng phía máy chủ trước đọc/ghi.
- K-02: ghi người, thời điểm, hành động, đối tượng, kết quả; trước/sau và evidence/nhận xét khi phù hợp.
- Dữ liệu nhạy cảm được che khi hiển thị/xuất; RawReport bất biến và cần quyền riêng.
- Thông báo không chứa Attack, Evidence, token, cookie, mật khẩu hoặc bí mật khác.

## 8. Quan hệ và truy vết

`include` được gọi từ [UC-C01](UC-C01_gan_vai_tro_va_pham_vi_tai_san.md); `include` được gọi từ [UC-C02](UC-C02_thu_hoi_quyen.md); `include` được gọi từ [UC-C04](UC-C04_xem_bao_cao_goc_theo_quyen.md); `include` được gọi từ [UC-C05](UC-C05_xuat_hoac_tai_bao_cao.md); `include` được gọi từ [UC-C10](UC-C10_thu_hoi_lien_ket_bao_cao.md).

Luồng thực thi chi tiết: [AD-01](../tai_lieu_activity/AD-01_quy_trinh_tong_the.md), [AD-09](../tai_lieu_activity/AD-09_xem_du_lieu_bao_cao_goc_xuat_tai_va_thu_hoi.md), [AD-15](../tai_lieu_activity/AD-15_gan_thu_hoi_quyen_va_xem_nhat_ky_quyen.md). Mã PlantUML/sơ đồ: [23_use_case_quyen_bao_cao_audit](../visual_paradigm_uml_md/23_use_case_quyen_bao_cao_audit.md).

## 9. Tiêu chí kiểm tra

- [ ] Nội bộ áp dụng mọi nhánh: người/thời điểm/hành động/đối tượng/kết quả/trước-sau và bằng chứng liên quan.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
