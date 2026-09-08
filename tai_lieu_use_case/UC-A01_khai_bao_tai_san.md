# UC-A01 — Khai báo tài sản

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-A01` |
| Nhóm | `A` |
| Tác nhân trực tiếp | Chủ sở hữu tài sản, Chuyên viên ATTT |
| Nguồn nghiệp vụ | Mục 4; 5.1 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-02](../tai_lieu_activity/AD-02_khai_bao_cap_nhat_va_phe_duyet_tai_san.md) |
| Sơ đồ Use Case | [18_use_case_tai_san_lich_quet](../visual_paradigm_uml_md/18_use_case_tai_san_lich_quet.md) |

**Mục tiêu:** Tạo Draft với mã duy nhất, domain/URL hợp lệ; cảnh báo trùng, không tự chặn chỉ vì trùng domain.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Chủ sở hữu tài sản, Chuyên viên ATTT.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Khai báo tài sản”.

**Tiền điều kiện:** Tác nhân đã xác thực, có quyền trên tài sản; dữ liệu hoặc trạng thái tài sản phù hợp với thao tác.

**Dữ liệu vào/ra liên quan:** Mã/tên website, domain/URL, môi trường, đơn vị/chủ sở hữu/nhóm kỹ thuật/ATTT, mức quan trọng, dữ liệu xử lý, mức phơi nhiễm, URL cho phép/loại trừ, khung giờ.

**Thực thể:** Asset, AssetScope và lịch sử phê duyệt phạm vi.

## 4. Luồng chính

1. Actor mở chức năng khai báo và nhập dữ liệu tài sản.
2. Hệ thống kiểm tra K-01, trường bắt buộc, mã duy nhất và định dạng domain/URL.
3. Nếu nghi trùng domain/URL, hệ thống cảnh báo để actor rà soát; cảnh báo không tự tạo bản ghi trùng.
4. Actor xác nhận lưu; hệ thống tạo Asset ở Draft và audit.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Dữ liệu/trạng thái không hợp lệ | Không áp dụng thay đổi; giữ dữ liệu hiện hành, trả lỗi để sửa và gửi lại. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Tạo Draft với mã duy nhất, domain/URL hợp lệ; cảnh báo trùng, không tự chặn chỉ vì trùng domain.
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

Luồng thực thi chi tiết: [AD-02](../tai_lieu_activity/AD-02_khai_bao_cap_nhat_va_phe_duyet_tai_san.md). Mã PlantUML/sơ đồ: [18_use_case_tai_san_lich_quet](../visual_paradigm_uml_md/18_use_case_tai_san_lich_quet.md).

## 9. Tiêu chí kiểm tra

- [ ] Tạo Draft với mã duy nhất, domain/URL hợp lệ; cảnh báo trùng, không tự chặn chỉ vì trùng domain.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
