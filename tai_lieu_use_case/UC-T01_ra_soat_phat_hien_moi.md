# UC-T01 — Rà soát phát hiện mới

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-T01` |
| Nhóm | `T` |
| Tác nhân trực tiếp | Chuyên viên ATTT |
| Nguồn nghiệp vụ | Mục 6; 7.2 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-05](../tai_lieu_activity/AD-05_phan_tich_va_xac_nhan_hoac_dieu_chinh_final_severity.md) |
| Sơ đồ Use Case | [20_use_case_phan_tich_muc_do](../visual_paradigm_uml_md/20_use_case_phan_tich_muc_do.md) |

**Mục tiêu:** New được phân loại đúng/sai/ngoài phạm vi; chỉ nhánh hợp lệ đi xác nhận severity.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chuyên viên ATTT.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Rà soát phát hiện mới”.

**Tiền điều kiện:** Tác nhân có quyền xem/đánh giá Finding trong phạm vi; triage áp dụng cho Finding New, điều chỉnh mức độ dùng giá trị hiện hành.

**Dữ liệu vào/ra liên quan:** Finding, Risk/Confidence nguồn, evidence đã che, bối cảnh tài sản, Final Severity/lý do, người/thời điểm đánh giá và rà soát.

**Thực thể:** Finding, SeverityAssessment, Evidence và lịch sử thay đổi mức độ.

## 4. Luồng chính

1. ATTT mở Finding New và xem evidence đã che, Risk/Confidence, vị trí và bối cảnh.
2. Khi cần, yêu cầu Owner cung cấp ảnh hưởng nghiệp vụ.
3. Phân loại thành hợp lệ, False Positive hoặc bản ghi sai/ngoài phạm vi.
4. Gọi UC-T02, UC-T03 hoặc UC-T04 theo đúng nhánh; chưa có quyết định thì giữ New.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Thiếu căn cứ hoặc rà soát chưa chấp thuận | Giữ mức độ/trạng thái hiện hành; bổ sung và đánh giá lại. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- New được phân loại đúng/sai/ngoài phạm vi; chỉ nhánh hợp lệ đi xác nhận severity.
- Không sửa Risk/Confidence nguồn, RawReport hoặc AuditLog ngoài các thao tác nguồn cho phép.
- Không xóa cứng dữ liệu có giá trị truy vết.
- Mọi quyền được kiểm tra tại thời điểm thao tác; quyền đã thu hồi không tiếp tục có hiệu lực.

## 7. Quyền, bảo mật và audit

- K-01: xác thực, kiểm tra role + asset scope và quyền riêng phía máy chủ trước đọc/ghi.
- K-02: ghi người, thời điểm, hành động, đối tượng, kết quả; trước/sau và evidence/nhận xét khi phù hợp.
- Dữ liệu nhạy cảm được che khi hiển thị/xuất; RawReport bất biến và cần quyền riêng.
- Thông báo không chứa Attack, Evidence, token, cookie, mật khẩu hoặc bí mật khác.

## 8. Quan hệ và truy vết

`extend` được gọi từ [UC-T02](UC-T02_xac_nhan_final_severity.md) với điều kiện `[New hợp lệ]`; `extend` được gọi từ [UC-T03](UC-T03_xac_nhan_false_positive.md) với điều kiện `[New kết quả sai]`; `extend` được gọi từ [UC-T04](UC-T04_huy_phat_hien_sai_hoac_ngoai_pham_vi.md) với điều kiện `[New sai bản ghi / ngoài phạm vi]`.

Luồng thực thi chi tiết: [AD-05](../tai_lieu_activity/AD-05_phan_tich_va_xac_nhan_hoac_dieu_chinh_final_severity.md). Mã PlantUML/sơ đồ: [20_use_case_phan_tich_muc_do](../visual_paradigm_uml_md/20_use_case_phan_tich_muc_do.md).

## 9. Tiêu chí kiểm tra

- [ ] New được phân loại đúng/sai/ngoài phạm vi; chỉ nhánh hợp lệ đi xác nhận severity.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
