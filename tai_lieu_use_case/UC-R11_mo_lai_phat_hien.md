# UC-R11 — Mở lại phát hiện

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-R11` |
| Nhóm | `R` |
| Tác nhân trực tiếp | Chuyên viên ATTT |
| Nguồn nghiệp vụ | Mục 7.2 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-06](../tai_lieu_activity/AD-06_khac_phuc_va_quet_xac_minh.md), [AD-10](../tai_lieu_activity/AD-10_mo_lai_sau_closed_hoac_het_dieu_kien_chap_nhan_rui_ro.md) |
| Sơ đồ Use Case | [21_use_case_khac_phuc_xac_minh](../visual_paradigm_uml_md/21_use_case_khac_phuc_xac_minh.md) |

**Mục tiêu:** ATTT mở lại khi xác minh còn lỗi/không hợp lệ; hệ thống hoặc ATTT mở lại Closed tái xuất hiện hoặc Risk Accepted hết điều kiện.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chuyên viên ATTT.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Mở lại phát hiện”.

**Tiền điều kiện:** Tác nhân có quyền theo vai trò và phiếu; trạng thái Finding phải thuộc các trạng thái đầu vào được nêu trong AD-06/AD-16.

**Dữ liệu vào/ra liên quan:** Finding/phiếu, nhóm/người phụ trách, thời hạn, tiến độ/nguyên nhân/biện pháp/evidence, yêu cầu và báo cáo xác minh.

**Thực thể:** Finding, RemediationTicket, Assignment, Evidence và VerificationRequest.

## 4. Luồng chính

1. Nhận một trong ba trigger hợp lệ: xác minh còn/không hợp lệ; Closed tái xuất hiện; Risk Accepted hết điều kiện.
2. Kiểm tra actor/trạng thái/bằng chứng theo từng trigger.
3. Chuyển Finding sang Reopened và audit nếu đủ căn cứ.
4. ATTT thực hiện UC-R02 với thời hạn mới.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Sai trạng thái hoặc thiếu dữ liệu bắt buộc | Không chuyển trạng thái; yêu cầu hoàn thiện. |
| Kết quả xác minh không hợp lệ | Không đóng; ATTT mở lại theo UC-R11. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- ATTT mở lại khi xác minh còn lỗi/không hợp lệ; hệ thống hoặc ATTT mở lại Closed tái xuất hiện hoặc Risk Accepted hết điều kiện.
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

Luồng thực thi chi tiết: [AD-06](../tai_lieu_activity/AD-06_khac_phuc_va_quet_xac_minh.md), [AD-10](../tai_lieu_activity/AD-10_mo_lai_sau_closed_hoac_het_dieu_kien_chap_nhan_rui_ro.md). Mã PlantUML/sơ đồ: [21_use_case_khac_phuc_xac_minh](../visual_paradigm_uml_md/21_use_case_khac_phuc_xac_minh.md).

## 9. Tiêu chí kiểm tra

- [ ] ATTT mở lại khi xác minh còn lỗi/không hợp lệ; hệ thống hoặc ATTT mở lại Closed tái xuất hiện hoặc Risk Accepted hết điều kiện.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
