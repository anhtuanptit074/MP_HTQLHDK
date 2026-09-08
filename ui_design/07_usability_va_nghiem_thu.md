# Usability, review vai trò và nghiệm thu giai đoạn 9

## 1. Nhiệm vụ review theo vai trò

| Vai trò | Kịch bản prototype/wireframe phải hoàn thành | Tiêu chí thành công |
|---|---|---|
| SECURITY | Phê duyệt scope; review severity; phân công; kết luận Closed/Reopened; xem business audit | Không nhầm Source/Final; thấy đủ guard/căn cứ; không có action ngoài workflow |
| OWNER | Tạo Asset Draft; xác nhận scope/impact; theo dõi ticket; tạo/bổ sung risk | Hiểu scope đề nghị chưa áp dụng; không thấy action SECURITY/RISK_APPROVER |
| SCAN_OPS | Xem scan Planned; ghi Completed/Failed; upload; hiểu duplicate/Partial | Không thấy nút chạy ZAP; không coi duplicate là lỗi; Partial rõ coverage |
| REMEDIATOR | Mở current ticket; accept/progress/evidence; submit verification; xử lý stale | Chỉ thấy task hiện hành; không thể Closed/risk approve; draft giữ khi 412 |
| RISK_APPROVER | Review Needs Information/Approve/Reject; đọc previous status; xem renewal | Không tự duyệt đề nghị của mình; field quyết định điều kiện; hiểu tính bất biến/hạn |
| AUDITOR | Lọc Permission/Business audit, xem history masked, tải theo quyền riêng | Không thấy mutation; truy vết ai/khi nào/from-to/reason/correlation |
| ADMIN | Cấp/thu hồi role+asset+sensitive flag, xem permission audit | Không đọc Finding/raw; asset null không hiểu như wildcard; sensitive grant có lý do |

Mỗi buổi review dùng dữ liệu giả đã masking, không dùng báo cáo ZAP thật. Ghi người tham gia, vai trò, nhiệm vụ, lỗi nghiêm trọng, quyết định và action owner; không thu thập dữ liệu cá nhân không cần thiết.

## 2. Câu hỏi usability trọng yếu

1. Người dùng có chỉ đúng Final Severity mà không chọn nhầm Source Risk/Confidence?
2. OWNER có hiểu scope revision mới chưa áp dụng trước khi SECURITY duyệt?
3. SCAN_OPS có nhận ra hệ thống chỉ ghi nhận scan ngoài, không chạy ZAP?
4. REMEDIATOR có hiểu 100% progress chưa đủ để Closed và ai có thẩm quyền kết luận?
5. SECURITY có phân biệt “invalid report → Reopened” với “giữ Pending” không?
6. RISK_APPROVER có biết Reject trả Finding về đúng trạng thái trước và decision là bất biến?
7. Mọi vai trò có hiểu `[Đã che theo chính sách]`, quyền raw/evidence riêng và việc download được audit?
8. Khi stale/permission revoked/network uncertain, người dùng có biết action đã hay chưa hoàn tất?

## 3. Mức nghiêm trọng vấn đề

| Mức | Định nghĩa | Điều kiện cổng |
|---|---|---|
| P0 | Có thể lộ dữ liệu, vượt quyền, tự đóng/tự duyệt, mất evidence/history hoặc tạo chuyển trạng thái sai | Phải sửa và review lại trước giai đoạn 10 |
| P1 | Có thể khiến đa số người dùng chọn sai Finding/scope/decision hoặc hiểu sai kết quả Partial/stale | Phải có giải pháp được duyệt trước implementation |
| P2 | Làm chậm/khó hiểu nhưng có đường hoàn thành an toàn | Có owner và kế hoạch trước UAT |
| P3 | Nhất quán hình thức/viết nhãn nhỏ | Ghi backlog, không cản baseline |

## 4. Acceptance checklist theo sản phẩm bàn giao

### Sitemap và luồng

- [x] Bao phủ toàn bộ màn hình tối thiểu giai đoạn 9.
- [x] Điều hướng theo permission và asset/task scope; UI-32 không lộ existence.
- [x] F-01–F-04, risk và luồng ngoại lệ có flow.
- [x] Không có route/action điều khiển ZAP hoặc phân hệ ngoài phạm vi.

### Wireframe/prototype

- [x] Có shell, Asset/scope, Import/Partial/Duplicate, Finding, Verification, Risk và state chung.
- [x] Action terminal hiển thị trạng thái nguồn/đích, guard, ảnh hưởng và lý do.
- [x] Prototype thể hiện role gating và ba khái niệm severity.
- [ ] Đại diện bảy vai trò hoàn thành kịch bản và ký duyệt.

### Design system/masking

- [x] Có token semantic, component/state/content pattern.
- [x] Có matrix Full/Masked/Metadata/Stream/Omitted theo bảy vai trò.
- [x] Object key, key reference, token hash và raw payload không có trong màn hình thông thường.
- [ ] ATTT/Chủ dữ liệu duyệt masking policy và quyền sensitive cụ thể.

### Accessibility/responsive

- [x] Có tiêu chí WCAG 2.2 AA, keyboard, focus, forms, live region và dialog.
- [x] Có hành vi ở 320/480/768/1200px và table reflow.
- [x] Có bảy kịch bản test thủ công.
- [ ] Chạy axe/keyboard/NVDA trên implementation ở giai đoạn 10–12.

### Truy vết

- [x] 32 màn hình được nhận diện.
- [x] 57 operation được ánh xạ vào nhóm màn hình.
- [x] Error code quan trọng có UI state tương ứng.
- [x] Quy tắc PR evidence đã định nghĩa.

## 5. Quyết định còn chờ

| Quyết định | Chủ sở hữu | Ảnh hưởng UI |
|---|---|---|
| Masking policy chi tiết | ATTT/Chủ dữ liệu | Field nào M/F, loại evidence được stream |
| Upload/rate limit | Vận hành/ATTT | Help text, validation và retry |
| SLA/notification horizon | Nghiệp vụ | Due/sắp hạn label, filter, notification |
| Risk authority/validity policy | Người có thẩm quyền | Form decision và validation text |
| Retention/export expiry | Chủ dữ liệu | Hạn mặc định; UI không tự điền trước khi chốt |
| IdP/identity mapping | Tech Lead/ATTT | Login/account error; không ảnh hưởng permission model |

## 6. Kết luận cổng

Bộ thiết kế đáp ứng sản phẩm tài liệu của giai đoạn 9 và sẵn sàng review. Điều kiện “đại diện từng vai trò duyệt prototype” và các policy do con người có thẩm quyền quyết định vẫn mở; vì vậy trạng thái là **baseline hoàn chỉnh, chờ review/phê duyệt**, không tự nhận đã nghiệm thu.
