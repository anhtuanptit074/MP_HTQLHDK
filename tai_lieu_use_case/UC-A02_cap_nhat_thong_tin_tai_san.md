# UC-A02 — Cập nhật thông tin tài sản

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-A02` |
| Nhóm | `A` |
| Tác nhân trực tiếp | Chủ sở hữu tài sản, Chuyên viên ATTT |
| Nguồn nghiệp vụ | Mục 3.2; 5.1.2 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-02](../tai_lieu_activity/AD-02_khai_bao_cap_nhat_va_phe_duyet_tai_san.md), [AD-14](../tai_lieu_activity/AD-14_tam_dung_tiep_tuc_va_ngung_tai_san.md) |
| Sơ đồ Use Case | [18_use_case_tai_san_lich_quet](../visual_paradigm_uml_md/18_use_case_tai_san_lich_quet.md) |

**Mục tiêu:** Lưu lịch sử; thay đổi scope phải qua chủ sở hữu xác nhận và ATTT phê duyệt lại trước khi sử dụng.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chủ sở hữu tài sản, Chuyên viên ATTT.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Cập nhật thông tin tài sản”.

**Tiền điều kiện:** Tác nhân đã xác thực, có quyền trên tài sản; dữ liệu hoặc trạng thái tài sản phù hợp với thao tác.

**Dữ liệu vào/ra liên quan:** Mã/tên website, domain/URL, môi trường, đơn vị/chủ sở hữu/nhóm kỹ thuật/ATTT, mức quan trọng, dữ liệu xử lý, mức phơi nhiễm, URL cho phép/loại trừ, khung giờ.

**Thực thể:** Asset, AssetScope và lịch sử phê duyệt phạm vi.

## 4. Luồng chính

1. Actor mở Asset trong phạm vi và sửa dữ liệu được phép.
2. Hệ thống kiểm tra dữ liệu và xác định thay đổi có liên quan scope hay không.
3. Thay đổi thường được lưu với lịch sử; scope mới chỉ là đề nghị và chưa được dùng.
4. Nếu đổi scope, chuyển sang UC-A03 và UC-A04 trước khi áp dụng.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Dữ liệu/trạng thái không hợp lệ | Không áp dụng thay đổi; giữ dữ liệu hiện hành, trả lỗi để sửa và gửi lại. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Lưu lịch sử; thay đổi scope phải qua chủ sở hữu xác nhận và ATTT phê duyệt lại trước khi sử dụng.
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

Luồng thực thi chi tiết: [AD-02](../tai_lieu_activity/AD-02_khai_bao_cap_nhat_va_phe_duyet_tai_san.md), [AD-14](../tai_lieu_activity/AD-14_tam_dung_tiep_tuc_va_ngung_tai_san.md). Mã PlantUML/sơ đồ: [18_use_case_tai_san_lich_quet](../visual_paradigm_uml_md/18_use_case_tai_san_lich_quet.md).

## 9. Tiêu chí kiểm tra

- [ ] Lưu lịch sử; thay đổi scope phải qua chủ sở hữu xác nhận và ATTT phê duyệt lại trước khi sử dụng.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
