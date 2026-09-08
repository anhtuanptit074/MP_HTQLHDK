# UC-S08 — Xác nhận thời gian quét

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-S08` |
| Nhóm | `S` |
| Tác nhân trực tiếp | Chủ sở hữu tài sản |
| Nguồn nghiệp vụ | Mục 3.3 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-03](../tai_lieu_activity/AD-03_tao_cap_nhat_kich_hoat_va_tam_dung_lich.md) |
| Sơ đồ Use Case | [18_use_case_tai_san_lich_quet](../visual_paradigm_uml_md/18_use_case_tai_san_lich_quet.md) |

**Mục tiêu:** Chủ sở hữu xác nhận thời gian thuộc tài sản của mình; không thay quyền ATTT kích hoạt lịch.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chủ sở hữu tài sản.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Xác nhận thời gian quét”.

**Tiền điều kiện:** Tác nhân đã xác thực, có quyền trên lịch/tài sản; khi kích hoạt, Asset phải Active và phạm vi đã được duyệt.

**Dữ liệu vào/ra liên quan:** Mã/tên lịch, tài sản/scope, tần suất, ngày hiệu lực/kết thúc, múi giờ, thời điểm/khung giờ, nhóm vận hành, ghi chú ZAP không chứa bí mật, trạng thái/lý do.

**Thực thể:** ScanSchedule, ScanOccurrence và lịch sử lịch quét.

## 4. Luồng chính

1. Chủ sở hữu mở đề nghị lịch thuộc tài sản.
2. Rà soát ngày/giờ, múi giờ và khung giờ.
3. Xác nhận hoặc yêu cầu điều chỉnh thời gian.
4. ATTT vẫn là người quyết định kích hoạt lịch.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Dữ liệu/trạng thái không hợp lệ | Không áp dụng thay đổi; giữ dữ liệu hiện hành, trả lỗi để sửa và gửi lại. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Chủ sở hữu xác nhận thời gian thuộc tài sản của mình; không thay quyền ATTT kích hoạt lịch.
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

Luồng thực thi chi tiết: [AD-03](../tai_lieu_activity/AD-03_tao_cap_nhat_kich_hoat_va_tam_dung_lich.md). Mã PlantUML/sơ đồ: [18_use_case_tai_san_lich_quet](../visual_paradigm_uml_md/18_use_case_tai_san_lich_quet.md).

## 9. Tiêu chí kiểm tra

- [ ] Chủ sở hữu xác nhận thời gian thuộc tài sản của mình; không thay quyền ATTT kích hoạt lịch.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
