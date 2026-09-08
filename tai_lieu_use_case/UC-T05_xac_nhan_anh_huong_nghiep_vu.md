# UC-T05 — Xác nhận ảnh hưởng nghiệp vụ

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-T05` |
| Nhóm | `T` |
| Tác nhân trực tiếp | Chủ sở hữu tài sản |
| Nguồn nghiệp vụ | Mục 3.2; 6.4 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-05](../tai_lieu_activity/AD-05_phan_tich_va_xac_nhan_hoac_dieu_chinh_final_severity.md) |
| Sơ đồ Use Case | [20_use_case_phan_tich_muc_do](../visual_paradigm_uml_md/20_use_case_phan_tich_muc_do.md) |

**Mục tiêu:** Cung cấp bối cảnh theo tài sản khi ATTT yêu cầu; không quyết định Final Severity.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chủ sở hữu tài sản.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Xác nhận ảnh hưởng nghiệp vụ”.

**Tiền điều kiện:** Tác nhân có quyền xem/đánh giá Finding trong phạm vi; triage áp dụng cho Finding New, điều chỉnh mức độ dùng giá trị hiện hành.

**Dữ liệu vào/ra liên quan:** Finding, Risk/Confidence nguồn, evidence đã che, bối cảnh tài sản, Final Severity/lý do, người/thời điểm đánh giá và rà soát.

**Thực thể:** Finding, SeverityAssessment, Evidence và lịch sử thay đổi mức độ.

## 4. Luồng chính

1. ATTT yêu cầu Owner cung cấp bối cảnh ảnh hưởng của Finding/Asset.
2. Owner xác nhận mức quan trọng, dữ liệu/chức năng/người dùng bị ảnh hưởng và kiểm soát giảm thiểu.
3. Hệ thống lưu ý kiến, người và thời điểm.
4. ATTT sử dụng thông tin làm đầu vào UC-T02; Owner không quyết Final Severity.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Thiếu căn cứ hoặc rà soát chưa chấp thuận | Giữ mức độ/trạng thái hiện hành; bổ sung và đánh giá lại. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Cung cấp bối cảnh theo tài sản khi ATTT yêu cầu; không quyết định Final Severity.
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

Luồng thực thi chi tiết: [AD-05](../tai_lieu_activity/AD-05_phan_tich_va_xac_nhan_hoac_dieu_chinh_final_severity.md). Mã PlantUML/sơ đồ: [20_use_case_phan_tich_muc_do](../visual_paradigm_uml_md/20_use_case_phan_tich_muc_do.md).

## 9. Tiêu chí kiểm tra

- [ ] Cung cấp bối cảnh theo tài sản khi ATTT yêu cầu; không quyết định Final Severity.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
