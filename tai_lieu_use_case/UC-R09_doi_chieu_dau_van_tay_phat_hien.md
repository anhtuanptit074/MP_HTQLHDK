# UC-R09 — Đối chiếu dấu vân tay phát hiện

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-R09` |
| Nhóm | `R` |
| Tác nhân trực tiếp | Không có actor trực tiếp; hành vi nội bộ do UC cha gọi |
| Nguồn nghiệp vụ | Mục 7.2; 7.5 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-06](../tai_lieu_activity/AD-06_khac_phuc_va_quet_xac_minh.md) |
| Sơ đồ Use Case | [21_use_case_khac_phuc_xac_minh](../visual_paradigm_uml_md/21_use_case_khac_phuc_xac_minh.md) |

**Mục tiêu:** Xử lý nội bộ: đối chiếu với báo cáo xác minh hợp lệ; thiếu kết quả không đồng nghĩa không còn fingerprint.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Không có actor trực tiếp; hành vi nội bộ do UC cha gọi.

Đây là xử lý nội bộ của hệ thống, không vẽ “Hệ thống” như một actor. Xử lý chỉ chạy trong ngữ cảnh UC cha đã kiểm tra quyền; không thể được gọi để bỏ qua K-01.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Một UC/Activity liên quan gọi xử lý “Đối chiếu dấu vân tay phát hiện”.

**Tiền điều kiện:** Tác nhân có quyền theo vai trò và phiếu; trạng thái Finding phải thuộc các trạng thái đầu vào được nêu trong AD-06/AD-16.

**Dữ liệu vào/ra liên quan:** Finding/phiếu, nhóm/người phụ trách, thời hạn, tiến độ/nguyên nhân/biện pháp/evidence, yêu cầu và báo cáo xác minh.

**Thực thể:** Finding, RemediationTicket, Assignment, Evidence và VerificationRequest.

## 4. Luồng chính

1. Hệ thống xác nhận báo cáo mới hợp lệ, đúng Finding/Asset/lần xác minh.
2. Đối chiếu fingerprint theo công thức nguồn.
3. Trả kết quả còn/không còn hoặc không đủ căn cứ.
4. Không coi lỗi nhập hay thiếu dữ liệu là không còn fingerprint.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Sai trạng thái hoặc thiếu dữ liệu bắt buộc | Không chuyển trạng thái; yêu cầu hoàn thiện. |
| Kết quả xác minh không hợp lệ | Không đóng; ATTT mở lại theo UC-R11. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Xử lý nội bộ: đối chiếu với báo cáo xác minh hợp lệ; thiếu kết quả không đồng nghĩa không còn fingerprint.
- Không sửa Risk/Confidence nguồn, RawReport hoặc AuditLog ngoài các thao tác nguồn cho phép.
- Không xóa cứng dữ liệu có giá trị truy vết.
- Mọi quyền được kiểm tra tại thời điểm thao tác; quyền đã thu hồi không tiếp tục có hiệu lực.

## 7. Quyền, bảo mật và audit

- K-01: xác thực, kiểm tra role + asset scope và quyền riêng phía máy chủ trước đọc/ghi.
- K-02: ghi người, thời điểm, hành động, đối tượng, kết quả; trước/sau và evidence/nhận xét khi phù hợp.
- Dữ liệu nhạy cảm được che khi hiển thị/xuất; RawReport bất biến và cần quyền riêng.
- Thông báo không chứa Attack, Evidence, token, cookie, mật khẩu hoặc bí mật khác.

## 8. Quan hệ và truy vết

`include` được gọi từ [UC-R10](UC-R10_dong_phat_hien.md).

Luồng thực thi chi tiết: [AD-06](../tai_lieu_activity/AD-06_khac_phuc_va_quet_xac_minh.md). Mã PlantUML/sơ đồ: [21_use_case_khac_phuc_xac_minh](../visual_paradigm_uml_md/21_use_case_khac_phuc_xac_minh.md).

## 9. Tiêu chí kiểm tra

- [ ] Xử lý nội bộ: đối chiếu với báo cáo xác minh hợp lệ; thiếu kết quả không đồng nghĩa không còn fingerprint.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
