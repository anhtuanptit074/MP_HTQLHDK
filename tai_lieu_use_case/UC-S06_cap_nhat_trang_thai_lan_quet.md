# UC-S06 — Cập nhật trạng thái lần quét

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-S06` |
| Nhóm | `S` |
| Tác nhân trực tiếp | Nhóm vận hành quét |
| Nguồn nghiệp vụ | Mục 5.2.3 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-11](../tai_lieu_activity/AD-11_den_lich_va_ghi_nhan_mot_lan_quet_ben_ngoai.md) |
| Sơ đồ Use Case | [18_use_case_tai_san_lich_quet](../visual_paradigm_uml_md/18_use_case_tai_san_lich_quet.md) |

**Mục tiêu:** Ghi Completed, Failed hoặc Missed theo thực tế; Result Imported chỉ do kết quả nhập F-03.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Nhóm vận hành quét.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Cập nhật trạng thái lần quét”.

**Tiền điều kiện:** Tác nhân đã xác thực, có quyền trên lịch/tài sản; khi kích hoạt, Asset phải Active và phạm vi đã được duyệt.

**Dữ liệu vào/ra liên quan:** Mã/tên lịch, tài sản/scope, tần suất, ngày hiệu lực/kết thúc, múi giờ, thời điểm/khung giờ, nhóm vận hành, ghi chú ZAP không chứa bí mật, trạng thái/lý do.

**Thực thể:** ScanSchedule, ScanOccurrence và lịch sử lịch quét.

## 4. Luồng chính

1. Ops mở ScanOccurrence Planned được giao.
2. Sau khi vận hành ZAP bên ngoài, Ops cập nhật Completed hoặc kết quả không thành công.
3. Hệ thống kiểm tra quyền và dữ liệu trạng thái.
4. Result Imported chỉ được hệ thống ghi sau UC-I01, không phải lựa chọn thủ công.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Dữ liệu/trạng thái không hợp lệ | Không áp dụng thay đổi; giữ dữ liệu hiện hành, trả lỗi để sửa và gửi lại. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Ghi Completed, Failed hoặc Missed theo thực tế; Result Imported chỉ do kết quả nhập F-03.
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

Luồng thực thi chi tiết: [AD-11](../tai_lieu_activity/AD-11_den_lich_va_ghi_nhan_mot_lan_quet_ben_ngoai.md). Mã PlantUML/sơ đồ: [18_use_case_tai_san_lich_quet](../visual_paradigm_uml_md/18_use_case_tai_san_lich_quet.md).

## 9. Tiêu chí kiểm tra

- [ ] Ghi Completed, Failed hoặc Missed theo thực tế; Result Imported chỉ do kết quả nhập F-03.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
