# Hồ sơ thiết kế giao diện — Giai đoạn 9

| Thuộc tính | Giá trị |
|---|---|
| Hệ thống | Hệ thống quản lý quét lỗ hổng website định kỳ |
| Phiên bản | 1.0 |
| Ngày lập | 2026-09-07 |
| Trạng thái | Baseline thiết kế, sẵn sàng review theo vai trò |
| Phạm vi | F-01 đến F-04 và hỗ trợ trực tiếp |
| Nguồn API | [`api_design/01_openapi.yaml`](../api_design/01_openapi.yaml), 57 operations |
| Nguồn workflow | [`tai_lieu_activity/`](../tai_lieu_activity/README.md), 16 activity diagrams |
| Nguồn use case | [`tai_lieu_use_case/`](../tai_lieu_use_case/README.md), 63 use cases |

## 1. Sản phẩm bàn giao

| Sản phẩm bắt buộc | Tài liệu đáp ứng |
|---|---|
| Sitemap và user flow | [01 — Sitemap và user flow](01_sitemap_va_user_flow.md) |
| Wireframe cho luồng chính và ngoại lệ | [02 — Danh mục màn hình và wireframe](02_danh_muc_man_hinh_va_wireframe.md) |
| Design system nhỏ, nhất quán | [03 — Design system](03_design_system.md) |
| Trường hiển thị và masking theo vai trò | [04 — Ma trận hiển thị và masking](04_ma_tran_hien_thi_masking.md) |
| Accessibility và responsive | [05 — Accessibility và responsive](05_accessibility_responsive.md) |
| Truy vết màn hình–API–UC | [06 — Ma trận truy vết UI/API](06_ma_tran_truy_vet_ui_api.md) |
| Tiêu chí review/hoàn thành | [07 — Usability và nghiệm thu](07_usability_va_nghiem_thu.md) |

Prototype tương tác trong cuộc hội thoại minh họa shell desktop, worklist Finding, chi tiết Finding và sự khác biệt hành động giữa SECURITY/OWNER/SCAN_OPS/REMEDIATOR/RISK_APPROVER/AUDITOR. Prototype là minh họa hành vi, không phải frontend production và không thay OpenAPI.

## 2. Nguyên tắc baseline

- Điều hướng theo công việc: Tài sản, Lịch & lần quét, Lô nhập, Phát hiện, Phiếu khắc phục, Xác minh, Rủi ro, Báo cáo; Audit/Quyền chỉ hiện với vai trò tương ứng.
- Không tạo dashboard BI. Trang đầu là “Công việc của tôi”, chỉ tổng hợp worklist tối thiểu từ phạm vi đã có.
- UI ẩn/vô hiệu hóa action không hợp lệ để giảm sai sót, nhưng API luôn kiểm lại quyền, trạng thái và tách nhiệm vụ.
- Source Risk, Confidence và Final Severity luôn ở ba trường riêng, nhãn đầy đủ; không dùng một màu/badge chung khiến người dùng nhầm.
- Raw report, evidence nhạy cảm và export dùng bước xác nhận riêng, yêu cầu lý do khi API yêu cầu, cảnh báo audit và không hiển thị object key/key ref/token hash.
- Không render HTML từ ZAP. Chuỗi nguồn hiển thị như text đã encode/mask; URL không tự động thành liên kết ngoài.
- Trạng thái loading/empty/error/denied/stale/partial/duplicate đều có bản thiết kế cụ thể; không dùng toast là nơi duy nhất chứa thông tin quan trọng.
- Thời gian hiển thị theo timezone người dùng kèm timezone; giá trị gửi API vẫn là UTC. Lịch luôn cho thấy timezone IANA của lịch.

## 3. Phạm vi prototype và điểm cần ký duyệt

Prototype ưu tiên lát cắt F-03/F-04 vì có rủi ro nhầm mức độ, lộ dữ liệu và chuyển trạng thái cao nhất; toàn bộ màn hình còn lại có wireframe và hợp đồng trường/action trong tài liệu. Trước khi lập trình cần:

1. Đại diện từng vai trò review đúng worklist, thuật ngữ và action.
2. ATTT/Chủ dữ liệu duyệt chi tiết masking, quyền raw/evidence và nội dung cảnh báo tải.
3. Product Owner duyệt nhãn trạng thái tiếng Việt nhưng giữ mã trạng thái API ổn định.
4. Nhóm frontend/backend duyệt mapping 57 operation và hành vi stale/idempotent/partial.
5. Người phụ trách accessibility xác nhận keyboard, screen reader, contrast và responsive acceptance.

Các giá trị upload limit, SLA, retention, risk authority và notification horizon vẫn lấy từ policy đã duyệt; giao diện không tự đặt mặc định nghiệp vụ.
