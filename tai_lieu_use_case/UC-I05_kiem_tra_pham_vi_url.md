# UC-I05 — Kiểm tra phạm vi URL

## 1. Nhận diện và mục tiêu

| Thuộc tính | Nội dung |
|---|---|
| ID | `UC-I05` |
| Nhóm | `I` |
| Tác nhân trực tiếp | Không có actor trực tiếp; hành vi nội bộ do UC cha gọi |
| Nguồn nghiệp vụ | Mục 5.3.3; 7.6 của `doi_tuong_sd_quy_trinh_hd.md` |
| Activity liên quan | [AD-04](../tai_lieu_activity/AD-04_nhap_va_kiem_tra_bao_cao_zap.md) |
| Sơ đồ Use Case | [19_use_case_nhap_bao_cao_zap](../visual_paradigm_uml_md/19_use_case_nhap_bao_cao_zap.md) |

**Mục tiêu:** Lô sai tài sản/phạm vi bị từ chối; bản ghi sai phạm vi có ImportError và yêu cầu ATTT rà soát.

Use Case này chỉ thực hiện hành vi phục vụ F-01 đến F-04. Nội dung bảo mật, audit, thông báo hoặc rủi ro chỉ là kiểm soát/hỗ trợ cho bốn chức năng, không tạo phân hệ ngoài phạm vi.

## 2. Vai trò và trách nhiệm

**Actor chính/tham gia:** Không có actor trực tiếp; hành vi nội bộ do UC cha gọi.

Đây là xử lý nội bộ của hệ thống, không vẽ “Hệ thống” như một actor. Xử lý chỉ chạy trong ngữ cảnh UC cha đã kiểm tra quyền; không thể được gọi để bỏ qua K-01.

## 3. Kích hoạt, tiền điều kiện và dữ liệu

**Kích hoạt:** Một UC/Activity liên quan gọi xử lý “Kiểm tra phạm vi URL”.

**Tiền điều kiện:** Có ngữ cảnh tài sản, lần quét và lô nhập; với UC do người dùng khởi tạo, Ops phải có quyền nhập trên tài sản.

**Dữ liệu vào/ra liên quan:** Tài sản, ScanOccurrence, tệp Traditional JSON/XML, checksum, phiên bản/mẫu ZAP, thời gian quét/nhập, Alert và các trường ánh xạ.

**Thực thể:** ImportBatch, RawReport, ImportError, Finding và FindingOccurrence.

## 4. Luồng chính

1. Kiểm tra phạm vi lô và từng URL.
2. lô sai tài sản/phạm vi bị Rejected, bản ghi sai tạo ImportError và yêu cầu ATTT rà soát.
3. Nếu kiểm tra thất bại, dừng xử lý và không thực hiện bước đọc/ghi nghiệp vụ tiếp theo.
4. Trả kết quả cùng dữ liệu truy vết cho UC/Activity gọi; ghi audit khi thuộc phạm vi K-02.

## 5. Luồng thay thế và ngoại lệ

| Điều kiện | Xử lý bắt buộc |
|---|---|
| Không đủ quyền hoặc sai asset scope | Từ chối mặc định, audit; không thực hiện đọc/ghi tiếp theo. |
| Lô đã tồn tại | Trả kết quả lô cũ, không tạo dữ liệu trùng. |
| Sai tệp/phạm vi | Rejected trước tạo Finding. |
| Lỗi kỹ thuật | Failed; không dùng làm kết quả xác minh hợp lệ. |

Những điểm nguồn chưa quy định cạnh chuyển hoặc quyền cụ thể không được suy diễn thành chức năng mới. Activity được liên kết là bản quyết định thứ tự, guard và trạng thái đầu ra khi nội dung tóm tắt ở đây chưa đủ chi tiết.

## 6. Hậu điều kiện và bất biến

- Lô sai tài sản/phạm vi bị từ chối; bản ghi sai phạm vi có ImportError và yêu cầu ATTT rà soát.
- Không sửa Risk/Confidence nguồn, RawReport hoặc AuditLog ngoài các thao tác nguồn cho phép.
- Không xóa cứng dữ liệu có giá trị truy vết.
- Mọi quyền được kiểm tra tại thời điểm thao tác; quyền đã thu hồi không tiếp tục có hiệu lực.

## 7. Quyền, bảo mật và audit

- K-01: xác thực, kiểm tra role + asset scope và quyền riêng phía máy chủ trước đọc/ghi.
- K-02: ghi người, thời điểm, hành động, đối tượng, kết quả; trước/sau và evidence/nhận xét khi phù hợp.
- Dữ liệu nhạy cảm được che khi hiển thị/xuất; RawReport bất biến và cần quyền riêng.
- Thông báo không chứa Attack, Evidence, token, cookie, mật khẩu hoặc bí mật khác.

## 8. Quan hệ và truy vết

`include` được gọi từ [UC-I01](UC-I01_nhap_bao_cao_zap.md).

Luồng thực thi chi tiết: [AD-04](../tai_lieu_activity/AD-04_nhap_va_kiem_tra_bao_cao_zap.md). Mã PlantUML/sơ đồ: [19_use_case_nhap_bao_cao_zap](../visual_paradigm_uml_md/19_use_case_nhap_bao_cao_zap.md).

## 9. Tiêu chí kiểm tra

- [ ] Lô sai tài sản/phạm vi bị từ chối; bản ghi sai phạm vi có ImportError và yêu cầu ATTT rà soát.
- [ ] Không có actor ngoài danh sách nguồn được cấp quyền quyết định.
- [ ] Mọi nhánh từ chối/lỗi và thay đổi nhạy cảm được audit theo K-02.
- [ ] Dữ liệu đầu ra truy vết được tới đối tượng nguồn liên quan; không hard delete.
