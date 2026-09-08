# UC-I12 — Xem kết quả lô nhập

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-I12` |
| Nhóm | `I` |
| Tác nhân trực tiếp | Nhóm vận hành quét, Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên |
| Nguồn nghiệp vụ | Mục 3.3; 5.3.6 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-04](../tai_lieu_activity/AD-04_nhap_va_kiem_tra_bao_cao_zap.md), [AD-09](../tai_lieu_activity/AD-09_xem_du_lieu_bao_cao_goc_xuat_tai_va_thu_hoi.md) |
| Sơ đồ Use Case | [19_use_case_nhap_bao_cao_zap](../visual_paradigm_uml_md/19_use_case_nhap_bao_cao_zap.md) |

**Mục tiêu:** Xem kết quả, số bản ghi và lỗi trong phạm vi; không ngầm cấp quyền báo cáo gốc.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Nhóm vận hành quét, Chuyên viên ATTT, Chủ sở hữu tài sản, Nhóm khắc phục kỹ thuật, Người phê duyệt rủi ro, Kiểm toán viên.

Actor khởi tạo hoặc tham gia đúng phần việc trong luồng. Association trên hình chỉ biểu diễn sự tham gia; quyền quyết định phải theo ma trận và điều kiện nguồn.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Tác nhân chọn chức năng “Xem kết quả lô nhập”.

**Tiền điều kiện:** Có ngữ cảnh tài sản, lần quét và lô nhập; với UC do người dùng khởi tạo, Ops phải có quyền nhập trên tài sản.

**Dữ liệu vào/ra liên quan:** Tài sản, ScanOccurrence, tệp Traditional JSON/XML, checksum, phiên bản/mẫu ZAP, thời gian quét/nhập, Alert và các trường ánh xạ.

**Thực thể:** ImportBatch, RawReport, ImportError, Finding và FindingOccurrence.

## 4. Luồng chính

1. Actor mở lô nhập trong phạm vi được cấp.
2. Hệ thống kiểm tra role, asset scope và quyền xem loại dữ liệu tương ứng.
3. Hiển thị trạng thái, tổng số thành công/lỗi, ImportError và Finding mới/trùng phù hợp.
4. Không ngầm cung cấp RawReport; bản gốc phải qua UC-C04.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Lô đã tồn tại | Trả kết quả lô cũ, không tạo dữ liệu trùng. |
| Sai tệp/phạm vi | Rejected trước tạo Finding. |
| Lỗi kỹ thuật | Failed; không dùng làm kết quả xác minh hợp lệ. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Xem kết quả, số bản ghi và lỗi trong phạm vi; không ngầm cấp quyền báo cáo gốc.
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

Luồng thực thi chi tiết: [AD-04](../tai_lieu_activity/AD-04_nhap_va_kiem_tra_bao_cao_zap.md), [AD-09](../tai_lieu_activity/AD-09_xem_du_lieu_bao_cao_goc_xuat_tai_va_thu_hoi.md). Mã PlantUML/sơ đồ: [19_use_case_nhap_bao_cao_zap](../visual_paradigm_uml_md/19_use_case_nhap_bao_cao_zap.md).

## 9. Tiêu chí kiểm tra

- [ ] Xem kết quả, số bản ghi và lỗi trong phạm vi; không ngầm cấp quyền báo cáo gốc.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
