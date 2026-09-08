# UC-T06 — Xem lịch sử thay đổi mức độ

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-T06` |
| Nhóm | `T` |
| Tác nhân trực tiếp | Chuyên viên ATTT, Kiểm toán viên |
| Nguồn nghiệp vụ | Mục 3.2; 6.1–6.2 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-09](../tai_lieu_activity/AD-09_xem_du_lieu_bao_cao_goc_xuat_tai_va_thu_hoi.md) |
| Sơ đồ Use Case | [20_use_case_phan_tich_muc_do](../visual_paradigm_uml_md/20_use_case_phan_tich_muc_do.md) |

**Mục tiêu:** Xem giá trị trước/sau, lý do, người/thời điểm đánh giá và rà soát theo quyền.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chuyên viên ATTT, Kiểm toán viên.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Xem lịch sử thay đổi mức độ”.

**Tiền điều kiện:** Tác nhân có quyền xem/đánh giá Finding trong phạm vi; triage áp dụng cho Finding New, điều chỉnh mức độ dùng giá trị hiện hành.

**Dữ liệu vào/ra liên quan:** Finding, Risk/Confidence nguồn, evidence đã che, bối cảnh tài sản, Final Severity/lý do, người/thời điểm đánh giá và rà soát.

**Thực thể:** Finding, SeverityAssessment, Evidence và lịch sử thay đổi mức độ.

## 4. Luồng chính

1. ATTT/Auditor mở lịch sử mức độ trong phạm vi.
2. Hệ thống kiểm tra quyền và lọc Finding/Asset.
3. Hiển thị source/final severity, lý do, trước/sau, assessed_by/at và reviewed_by/at khi có.
4. Dữ liệu chỉ đọc; truy cập nhạy cảm được audit.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Thiếu căn cứ hoặc rà soát chưa chấp thuận | Giữ mức độ/trạng thái hiện hành; bổ sung và đánh giá lại. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Xem giá trị trước/sau, lý do, người/thời điểm đánh giá và rà soát theo quyền.
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

Luồng thực thi chi tiết: [AD-09](../tai_lieu_activity/AD-09_xem_du_lieu_bao_cao_goc_xuat_tai_va_thu_hoi.md). Mã PlantUML/sơ đồ: [20_use_case_phan_tich_muc_do](../visual_paradigm_uml_md/20_use_case_phan_tich_muc_do.md).

## 9. Tiêu chí kiểm tra

- [ ] Xem giá trị trước/sau, lý do, người/thời điểm đánh giá và rà soát theo quyền.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
