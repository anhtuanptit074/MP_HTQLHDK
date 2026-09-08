# Design system tối thiểu

## 1. Định hướng

Giao diện là công cụ vận hành bảo mật: mật độ vừa phải, ưu tiên đọc bảng và lịch sử, màu sắc tiết chế, action terminal có ngữ cảnh. Tên sản phẩm hiển thị `HTQLHDK`; thuật ngữ nghiệp vụ tiếng Việt, mã trạng thái/API giữ nguyên trong tooltip hoặc phần kỹ thuật khi cần truy vết.

## 2. Design tokens

| Nhóm | Token semantic | Quy tắc |
|---|---|---|
| Surface | canvas, navigation, surface, raised, overlay | Không dùng màu severity làm nền trang |
| Text | primary, secondary, disabled, inverse | Tương phản WCAG AA; text phụ vẫn ≥ 4.5:1 ở cỡ thường |
| Border | subtle, default, strong, focus | Focus ring không trùng border thường |
| Action | primary, secondary, ghost, destructive | Mỗi vùng chỉ một primary action |
| Status | info, success, warning, danger, neutral | Luôn kèm text/icon; không truyền nghĩa chỉ bằng màu |
| Severity | informational, low, medium, high, critical | Chỉ cho Final/Source Severity có nhãn rõ; không tô cả row |
| Space | 4, 8, 12, 16, 24, 32 | Lưới 4px; vùng chạm tối thiểu 44×44px trên thiết bị coarse pointer |
| Radius | 4, 8, 12 | 8px mặc định; không dùng pill cho mọi control |
| Type | body 14/20, compact 12/16, heading 18–28 | UI production dùng font hệ thống; không nhỏ hơn 12px visible |

Giá trị màu cụ thể phải được kiểm bằng công cụ contrast ở giai đoạn 10. Baseline đề xuất primary xanh lam/xanh teal trung tính; Critical đỏ, High cam đậm, Medium vàng nâu, Low xanh lam, Informational xám. Không dùng xanh lá cho mức Low vì xanh lá dành cho trạng thái thành công.

## 3. Component inventory

| Component | Biến thể | Quy tắc nghiệp vụ |
|---|---|---|
| App shell | desktop sidebar, mobile drawer | Nav lấy từ permission context; không coi ẩn nav là authorization |
| Breadcrumb | 2–4 cấp | Không đưa URL/Attack nhạy cảm vào label |
| Data table | standard, compact, selectable | Header sticky nếu trang dài; sort/filter server-side; cursor pagination |
| Status badge | neutral/info/success/warning/danger | Hiển thị nguyên nhãn trạng thái; icon + text |
| Severity label | five levels | Luôn có prefix Source/Final ở vùng dễ nhầm |
| Field value | plain, masked, omitted | Masked có nhãn “Đã che”; omitted không để khoảng trống mơ hồ |
| Timeline | audit, ticket event, review | Ai · khi nào · action · from/to · reason; thứ tự mới nhất/cũ nhất có nhãn |
| Diff | added, removed, changed | Ký hiệu `+ − ~` và text; không chỉ xanh/đỏ |
| Alert | info, warning, error, success | Inline cho trạng thái bền vững; toast chỉ xác nhận ngắn |
| Stepper | upload, asset onboarding | Cho phép quay lại trước submit; không giả progress của worker |
| Dialog | confirm, decision, sensitive access | Focus trap; escape/hủy; reason/evidence theo action |
| File picker | ZAP, evidence | Tên đã làm sạch; size/type; không preview HTML/XML chủ động |
| Progress | determinate/indeterminate | Dùng indeterminate nếu API không có phần trăm thật; không bịa % |
| Empty state | initial/filter/permission changed | Nêu nguyên nhân và next action hợp lệ |
| Problem panel | validation/403-404/409/412/5xx | Mã lỗi + correlation ID; không lộ chi tiết nội bộ |

## 4. Content style

- Action dùng động từ rõ: “Gửi Owner xác nhận”, “Phê duyệt phạm vi”, “Chuyển chờ xác minh”, “Kết luận Closed”, “Thu hồi liên kết”.
- Không dùng “Xử lý”, “OK”, “Có/Không” cho action terminal thiếu ngữ cảnh.
- Nhãn thời gian: `07/09/2026 16:20 ICT (Asia/Bangkok)`; chi tiết có UTC khi cần audit.
- Mã đối tượng không thay tên: `FND-004218 · Thiếu kiểm soát truy cập`.
- Message lỗi nêu điều người dùng có thể làm; correlation ID ở dòng phụ.
- Dùng “Không thể truy cập tài nguyên” cho cả 403/404 ngoài scope.
- Nội dung masked ghi `[Đã che theo chính sách]`; không dùng dấu `***` khiến người dùng tưởng giá trị mật khẩu.

## 5. Trạng thái và action hierarchy

- Primary: action tiếp theo duy nhất của luồng hiện tại.
- Secondary: xem chi tiết, lưu Draft, tải lại.
- Destructive: retire/revoke/reject khi có hậu quả; không đồng nhất “Reject” nghiệp vụ với lỗi hệ thống.
- Terminal workflow action luôn ở dialog, không đặt cạnh nhau dưới dạng icon-only.
- Khi action không hợp lệ do trạng thái, ưu tiên không hiển thị. Nếu người dùng cần hiểu lý do (SoD, thiếu evidence), hiển thị disabled control kèm giải thích có thể truy cập bằng bàn phím.

## 6. Pattern lịch sử

Timeline dùng cùng cấu trúc ở Asset, Finding, Ticket, Risk và Audit:

```text
07/09/2026 16:20 ICT · Nguyễn B · Tiến độ
Assigned → In Progress
Lý do: Đã bắt đầu áp dụng bản sửa.       Evidence: EV-00141 (tóm tắt đã che)
```

Before/after JSON trong Audit không render như HTML; hiển thị tree text đã encode, key allowlist, diff có thể copy từng giá trị đã che.

## 7. Không được làm

- Không lưu access token/raw report/evidence trong localStorage, cache client lâu dài hoặc analytics.
- Không tạo external link từ URL ZAP chưa qua allowlist.
- Không auto-submit action terminal, kể cả khi route được mở từ thông báo.
- Không dùng infinite scroll cho audit/workflow history; dùng cursor pagination có “Tải thêm”.
- Không cho sửa record lịch sử Applied/Approved/Rejected/VerificationResult/TicketEvent.
- Không tự tính/chèn SLA, hạn risk hay upload limit nếu policy chưa có.
