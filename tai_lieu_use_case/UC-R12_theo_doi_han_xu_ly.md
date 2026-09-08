# UC-R12 — Theo dõi hạn xử lý

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-R12` |
| Nhóm | `R` |
| Tác nhân trực tiếp | Chuyên viên ATTT, Nhóm khắc phục kỹ thuật, Chủ sở hữu tài sản |
| Nguồn nghiệp vụ | Mục 5.4.2; 9.1–9.2 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-16](../tai_lieu_activity/AD-16_thoi_han_xu_ly_va_thong_bao_toi_thieu.md) |
| Sơ đồ Use Case | [21_use_case_khac_phuc_xac_minh](../visual_paradigm_uml_md/21_use_case_khac_phuc_xac_minh.md) |

**Mục tiêu:** Xem sắp đến hạn/quá hạn theo phạm vi; thời hạn dựa trên chính sách đơn vị.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chuyên viên ATTT, Nhóm khắc phục kỹ thuật, Chủ sở hữu tài sản.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Theo dõi hạn xử lý”.

**Tiền điều kiện:** Tác nhân có quyền theo vai trò và phiếu; trạng thái Finding phải thuộc các trạng thái đầu vào được nêu trong AD-06/AD-16.

**Dữ liệu vào/ra liên quan:** Finding/phiếu, nhóm/người phụ trách, thời hạn, tiến độ/nguyên nhân/biện pháp/evidence, yêu cầu và báo cáo xác minh.

**Thực thể:** Finding, RemediationTicket, Assignment, Evidence và VerificationRequest.

## 4. Luồng chính

1. Actor mở danh sách phiếu/Finding thuộc tài sản hoặc nhiệm vụ.
2. Hệ thống tính/đọc thời hạn hiện hành theo bảng chính sách đã phê duyệt.
3. Phân loại đang xử lý, sắp đến hạn hoặc quá hạn để hiển thị/thông báo tối thiểu.
4. Không tự đổi trạng thái hoặc tự đặt SLA ngoài chính sách.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Sai trạng thái hoặc thiếu dữ liệu bắt buộc | Không chuyển trạng thái; yêu cầu hoàn thiện. |
| Kết quả xác minh không hợp lệ | Không đóng; ATTT mở lại theo UC-R11. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Xem sắp đến hạn/quá hạn theo phạm vi; thời hạn dựa trên chính sách đơn vị.
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

Luồng thực thi chi tiết: [AD-16](../tai_lieu_activity/AD-16_thoi_han_xu_ly_va_thong_bao_toi_thieu.md). Mã PlantUML/sơ đồ: [21_use_case_khac_phuc_xac_minh](../visual_paradigm_uml_md/21_use_case_khac_phuc_xac_minh.md).

## 9. Tiêu chí kiểm tra

- [ ] Xem sắp đến hạn/quá hạn theo phạm vi; thời hạn dựa trên chính sách đơn vị.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
