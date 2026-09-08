# Accessibility và responsive

## 1. Chuẩn mục tiêu

Baseline hướng tới WCAG 2.2 AA cho Web UI. Giai đoạn 10–12 phải dùng kiểm tra tự động kết hợp keyboard và screen reader thủ công; “không có lỗi tự động” không đồng nghĩa đạt accessibility.

## 2. Tiêu chí bắt buộc

| Nhóm | Tiêu chí nghiệm thu |
|---|---|
| Keyboard | Mọi route/action/form/dialog/table control dùng được không chuột; thứ tự focus theo DOM; không keyboard trap ngoài modal có cơ chế thoát |
| Focus | Focus visible ≥ 2px tương phản; sau navigation vào `h1`; mở dialog vào heading/field đầu, đóng trả trigger; lỗi submit focus summary |
| Semantics | Một `h1`; heading không nhảy cấp; landmark header/nav/main; table có caption/header/scope; timeline là list |
| Forms | Mỗi field có label thật; required/optional bằng text; help/error liên kết `aria-describedby`; không dùng placeholder thay label |
| Validation | Tóm tắt lỗi ở đầu form + lỗi cạnh field; giữ giá trị; nêu cách sửa; không chỉ màu/icon |
| Dynamic | Trạng thái upload/import/action dùng `aria-live=polite`; lỗi blocking dùng alert; không announce từng poll/progress giả |
| Dialog | `role=dialog`, accessible name, modal focus trap, Escape khi an toàn, hủy luôn có; destructive action không auto-focus nút xác nhận |
| Color | Text thường ≥4.5:1, chữ lớn ≥3:1, control/focus/meaningful graphic ≥3:1; severity/status có text/ký hiệu |
| Zoom/reflow | Dùng được ở 200% zoom và 320 CSS px; không mất action/nội dung; ngoại trừ bảng cần scroll trong container có nhãn |
| Motion | Tôn trọng `prefers-reduced-motion`; không animation lặp; progress indeterminate có text |
| Files | File picker keyboard-accessible; status và lỗi gắn tới file; không tự mở/preview nội dung nguy hiểm |
| Time | Hiển thị timezone; không dựa duy nhất vào relative time; screen reader đọc rõ ngày/giờ |

## 3. Responsive breakpoints hành vi

Không coi breakpoint là thiết bị cố định; thiết kế theo không gian nội dung:

| Bề rộng | Hành vi |
|---|---|
| ≥ 1200px | Sidebar cố định; list/detail có thể 2 cột khi mỗi vùng vẫn ≥ 480px; filter một hàng |
| 768–1199px | Sidebar thu gọn; list/detail tuần tự; filter wrap; dialog tối đa chiều rộng đọc thoải mái |
| 480–767px | Mobile drawer; action bar wrap; detail theo một cột; tabs cho phép scroll có chỉ báo hoặc đổi select nếu quá nhiều |
| 320–479px | Một cột; table quan trọng chuyển card/list semantic hoặc container scroll có label; primary action full row khi cần |

Không dùng list/detail song song cho Verification/Risk decision ở chiều rộng nhỏ vì dễ nhầm đối tượng. Header sticky không che focus/anchor. Dialog action quan trọng ở 320px có content scroll nhưng footer action vẫn nằm trong flow, không che field.

## 4. Table responsive

- Asset/Finding/Ticket worklist giữ tối thiểu: code, tên/summary, trạng thái, trường quyết định chính và action mở chi tiết.
- Cột phụ chuyển vào disclosure “Chi tiết hàng”, không biến thành tooltip-only.
- Audit và history dùng container scroll ngang có `aria-label`, giữ header đầu và cho phép copy text.
- Không dùng drag-and-drop là cách duy nhất sắp xếp/chọn; checkbox có label chứa mã đối tượng.
- Cursor pagination dùng nút “Trang trước/Tiếp theo” hoặc “Tải thêm” có trạng thái disabled rõ; sau tải focus không nhảy bất ngờ.

## 5. Screen reader wording trọng yếu

- `Critical, Final Severity`; `High, Source Risk`; `Medium, Source Confidence` — luôn đọc ngữ cảnh trước mức.
- `Đã che theo chính sách` thay vì đọc chuỗi dấu sao.
- `Trạng thái thay đổi từ Pending Verification sang Closed` cho timeline.
- `5 lỗi trên 148 bản ghi; 143 bản ghi đã nhập thành công` cho Partial.
- `Bạn không thể thực hiện hành động này vì cần người đánh giá độc lập` cho SoD.

## 6. Kịch bản kiểm thử thủ công

1. SECURITY chỉ dùng bàn phím tạo review, gặp validation, sửa lỗi, mở second-review state và không vô tình apply.
2. OWNER ở 320px xác nhận scope/impact; diff và cảnh báo vẫn đọc được ở 200% zoom.
3. SCAN_OPS upload bằng keyboard, theo dõi Received→Processing→Partial bằng screen reader; không bị announce mỗi poll.
4. REMEDIATOR cập nhật progress/evidence, gặp stale 412 và giữ được nội dung form.
5. RISK_APPROVER chọn ba quyết định; field điều kiện/thẩm quyền thay đổi đúng và được announce.
6. AUDITOR duyệt bảng audit bằng keyboard, mở before/after masked và copy correlation ID.
7. Người có cờ sensitive mở dialog tải, đọc cảnh báo audit, nhập lý do; người không cờ nhận UI an toàn.

## 7. Công cụ/bằng chứng giai đoạn 10–12

Chạy axe-core/Playwright hoặc công cụ tương đương trên route chính; lint semantic; contrast check cả light/dark/high-contrast; test Chrome/Edge theo chuẩn đơn vị; NVDA + Chrome/Edge trên Windows; keyboard-only ở 100%/200%; viewport 320, 768, 1024, 1440. Lưu báo cáo theo release và issue có owner; exception WCAG phải có phê duyệt/risk record, không đánh dấu đạt giả.
