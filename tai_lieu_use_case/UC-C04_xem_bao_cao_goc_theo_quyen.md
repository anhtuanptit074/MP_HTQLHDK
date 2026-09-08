# UC-C04 — Xem báo cáo gốc theo quyền

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-C04` |
| Nhóm | `C` |
| Tác nhân trực tiếp | Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên |
| Nguồn nghiệp vụ | Mục 8.2; 8.5 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-09](../tai_lieu_activity/AD-09_xem_du_lieu_bao_cao_goc_xuat_tai_va_thu_hoi.md) |
| Sơ đồ Use Case | [23_use_case_quyen_bao_cao_audit](../visual_paradigm_uml_md/23_use_case_quyen_bao_cao_audit.md) |

**Mục tiêu:** Cần quyền RawReport riêng cùng phạm vi; không sửa bản gốc; ghi audit đọc/tải.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm vận hành quét, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Xem báo cáo gốc theo quyền”.

**Tiền điều kiện:** Người dùng đã xác thực; mọi thao tác kiểm tra lại role, asset scope và quyền riêng tại máy chủ.

**Dữ liệu vào/ra liên quan:** Người dùng/vai trò/phạm vi, loại dữ liệu/thao tác, RawReport hoặc bản xuất, nhãn/mã truy vết/link có hạn và bản ghi audit.

**Thực thể:** UserRole, phạm vi tài sản, dữ liệu báo cáo và AuditLog.

## 4. Luồng chính

1. Actor yêu cầu xem/tải RawReport.
2. Hệ thống kiểm tra role, asset scope và quyền RawReport riêng.
3. Kiểm tra checksum; cung cấp bản gốc bất biến qua kênh được bảo vệ.
4. Audit mọi lần đọc/tải; không sửa bản gốc để masking.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Quyền bị thu hồi hoặc link hết hạn/đã thu hồi | Từ chối, audit; không cung cấp dữ liệu. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Cần quyền RawReport riêng cùng phạm vi; không sửa bản gốc; ghi audit đọc/tải.
- Không sửa Risk/Confidence nguồn, RawReport hoặc AuditLog ngoài các thao tác nguồn cho phép.
- Không xóa cứng dữ liệu có giá trị truy vết.
- Mọi quyền được kiểm tra tại thời điểm thao tác; quyền đã thu hồi không tiếp tục có hiệu lực.

## 7. Quyền, bảo mật và audit

- K-01: xác thực, kiểm tra role + asset scope và quyền riêng phía máy chủ trước đọc/ghi.
- K-02: ghi người, thời điểm, hành động, đối tượng, kết quả; trước/sau và evidence/nhận xét khi phù hợp.
- Dữ liệu nhạy cảm được che khi hiển thị/xuất; RawReport bất biến và cần quyền riêng.
- Thông báo không chứa Attack, Evidence, token, cookie, mật khẩu hoặc bí mật khác.

## 8. Quan hệ và truy vết

`include` đi tới [UC-C07](UC-C07_ghi_nhat_ky_hanh_dong_nhay_cam.md).

Luồng thực thi chi tiết: [AD-09](../tai_lieu_activity/AD-09_xem_du_lieu_bao_cao_goc_xuat_tai_va_thu_hoi.md). Mã PlantUML/sơ đồ: [23_use_case_quyen_bao_cao_audit](../visual_paradigm_uml_md/23_use_case_quyen_bao_cao_audit.md).

## 9. Tiêu chí kiểm tra

- [ ] Cần quyền RawReport riêng cùng phạm vi; không sửa bản gốc; ghi audit đọc/tải.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
