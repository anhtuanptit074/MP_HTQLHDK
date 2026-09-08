# UC-RA07 — Gia hạn chấp nhận rủi ro

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-RA07` |
| Nhóm | `RA` |
| Tác nhân trực tiếp | Chủ sở hữu tài sản, Chuyên viên ATTT |
| Nguồn nghiệp vụ | Mục 7.4 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-13](../tai_lieu_activity/AD-13_gia_han_bang_quyet_dinh_rui_ro_moi.md) |
| Sơ đồ Use Case | [22_use_case_chap_nhan_rui_ro](../visual_paradigm_uml_md/22_use_case_chap_nhan_rui_ro.md) |

**Mục tiêu:** Tạo quyết định mới qua người duyệt độc lập; không ghi đè quyết định cũ, không tự thêm cạnh Risk Accepted → Pending Risk Acceptance.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chủ sở hữu tài sản, Chuyên viên ATTT.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Gia hạn chấp nhận rủi ro”.

**Tiền điều kiện:** Có Finding trong phạm vi; tạo đề nghị mới chỉ từ Triaged, Assigned hoặc In Progress; người duyệt phải đúng thẩm quyền và độc lập.

**Dữ liệu vào/ra liên quan:** Finding, trạng thái trước, lý do/tác động/giảm thiểu, người chịu trách nhiệm, người đề nghị/duyệt, điều kiện, ngày hết hạn và quyết định cũ khi gia hạn.

**Thực thể:** Finding, RiskAcceptance và lịch sử quyết định.

## 4. Luồng chính

1. Owner/ATTT tạo hồ sơ gia hạn dẫn chiếu quyết định cũ.
2. Người có thẩm quyền độc lập rà soát/từ chối/yêu cầu bổ sung.
3. Nếu duyệt và Finding vẫn Risk Accepted, lưu quyết định mới, không ghi đè quyết định cũ.
4. Đề nghị đang chờ không kéo dài hạn cũ; đã Reopened không tự trở về Risk Accepted.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Sai thẩm quyền hoặc người duyệt là người đề nghị | Không áp dụng quyết định; giữ trạng thái đang chờ hoặc quyết định hiện hành. |
| Yêu cầu bổ sung | Bổ sung rồi quay lại rà soát, không tự phê duyệt. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Tạo quyết định mới qua người duyệt độc lập; không ghi đè quyết định cũ, không tự thêm cạnh Risk Accepted → Pending Risk Acceptance.
- Không sửa Risk/Confidence nguồn, RawReport hoặc AuditLog ngoài các thao tác nguồn cho phép.
- Không xóa cứng dữ liệu có giá trị truy vết.
- Mọi quyền được kiểm tra tại thời điểm thao tác; quyền đã thu hồi không tiếp tục có hiệu lực.

## 7. Quyền, bảo mật và audit

- K-01: xác thực, kiểm tra role + asset scope và quyền riêng phía máy chủ trước đọc/ghi.
- K-02: ghi người, thời điểm, hành động, đối tượng, kết quả; trước/sau và evidence/nhận xét khi phù hợp.
- Dữ liệu nhạy cảm được che khi hiển thị/xuất; RawReport bất biến và cần quyền riêng.
- Thông báo không chứa Attack, Evidence, token, cookie, mật khẩu hoặc bí mật khác.

## 8. Quan hệ và truy vết

Không có include/extend trực tiếp trong sơ đồ chi tiết; quan hệ trình tự nằm trong Activity.

Luồng thực thi chi tiết: [AD-13](../tai_lieu_activity/AD-13_gia_han_bang_quyet_dinh_rui_ro_moi.md). Mã PlantUML/sơ đồ: [22_use_case_chap_nhan_rui_ro](../visual_paradigm_uml_md/22_use_case_chap_nhan_rui_ro.md).

## 9. Tiêu chí kiểm tra

- [ ] Tạo quyết định mới qua người duyệt độc lập; không ghi đè quyết định cũ, không tự thêm cạnh Risk Accepted → Pending Risk Acceptance.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
