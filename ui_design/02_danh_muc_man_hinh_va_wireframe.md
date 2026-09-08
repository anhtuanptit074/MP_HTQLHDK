# Danh mục màn hình và wireframe

## 1. Danh mục màn hình

| ID | Route đề xuất | Màn hình | Vai trò chính | Action chính |
|---|---|---|---|---|
| UI-01 | `/login` | Đăng nhập OIDC | Chưa xác thực | Đăng nhập |
| UI-02 | `/work` | Công việc của tôi | Mọi vai trò nghiệp vụ | Mở worklist liên quan |
| UI-03 | `/me/access` | Danh tính/quyền hiện hành | Mọi người dùng | Xem role, asset scope, quyền nhạy cảm |
| UI-04 | `/assets` | Danh sách tài sản | Vai trò có `asset.read` | Lọc trạng thái, mở chi tiết, tạo nếu có quyền |
| UI-05 | `/assets/new` | Tạo Asset Draft | OWNER, SECURITY | Lưu Draft |
| UI-06 | `/assets/:id` | Chi tiết tài sản | Theo asset scope | Sửa, suspend/resume/retire nếu hợp lệ |
| UI-07 | `/assets/:id/scopes/:scopeId` | Scope revision/rule/window | OWNER, SECURITY; read theo scope | Sửa Draft, gửi, xác nhận, quyết định |
| UI-08 | `/schedules` | Danh sách lịch | Theo asset scope | Mở lịch/tạo lịch |
| UI-09 | `/schedules/:id` | Chi tiết lịch | OWNER, SECURITY, SCAN_OPS, read roles | Sửa/confirm time/transition theo quyền |
| UI-10 | `/scans` | Danh sách lần quét | Theo asset/task scope | Tạo/ghi trạng thái khi được phép |
| UI-11 | `/scans/:id` | Chi tiết lần quét | Theo scope | Ghi Completed/Failed/Missed, mở lô nhập |
| UI-12 | `/imports/new` | Upload ZAP | SCAN_OPS | Chọn Asset + Scan Completed, upload |
| UI-13 | `/imports/:id` | Chi tiết lô nhập | Theo scope | Theo dõi, xem lỗi, tải raw nếu có quyền riêng |
| UI-14 | `/findings` | Worklist Finding | Theo scope | Lọc trạng thái/severity/asset/hạn |
| UI-15 | `/findings/:id` | Chi tiết Finding | Theo scope | Mở review/ticket/risk/xác minh hợp lệ |
| UI-16 | `/findings/:id/review/new` | Đánh giá severity/disposition | SECURITY | Tạo review |
| UI-17 | `/reviews/:id/impact` | Xác nhận tác động | OWNER | Xác nhận impact vào review Proposed |
| UI-18 | `/reviews/:id/second-review` | Rà soát độc lập | SECURITY/RISK_APPROVER đủ thẩm quyền | Approve/Reject review |
| UI-19 | `/tickets` | Danh sách phiếu | Theo asset/task scope | Lọc người phụ trách/hạn/trạng thái |
| UI-20 | `/tickets/:id` | Chi tiết phiếu | Theo task scope | Tiếp nhận/cập nhật/đổi phân công/hạn |
| UI-21 | `/tickets/:id/progress` | Cập nhật tiến độ/evidence | Current REMEDIATOR | Append Progress snapshot |
| UI-22 | `/verification-requests/:id` | Chi tiết yêu cầu xác minh | SECURITY, assigned SCAN_OPS/REMEDIATOR | Lập scan/mở kết quả/kết luận |
| UI-23 | `/verification-requests/:id/decision` | Kết luận xác minh | SECURITY độc lập | Closed/Reopened |
| UI-24 | `/risks` | Worklist rủi ro | OWNER, SECURITY, RISK_APPROVER, AUDITOR | Lọc chờ/sắp hết hạn |
| UI-25 | `/findings/:id/risk/new` | Tạo đề nghị rủi ro | OWNER, SECURITY | Gửi đề nghị |
| UI-26 | `/risks/:id` | Chi tiết hồ sơ/quyết định | Theo scope | Bổ sung/quyết định/gia hạn |
| UI-27 | `/reports` | Danh sách báo cáo xuất | Vai trò có report permission | Tạo/mở/tải/thu hồi |
| UI-28 | `/reports/new` | Tạo báo cáo xuất | Theo all-asset authorization | Chọn asset/người nhận/policy/hạn |
| UI-29 | `/notifications` | Hộp thông báo | Chính người nhận | Mở, đánh dấu đã đọc |
| UI-30 | `/audit` | Nhật ký quyền/nghiệp vụ | ADMIN/SECURITY/AUDITOR theo category | Lọc, xem before/after đã che |
| UI-31 | `/access` | Quản lý quyền | ADMIN | Cấp/thu hồi grant |
| UI-32 | `/unavailable` | Không thể truy cập | Mọi người dùng | Quay lại worklist an toàn |

Không dùng route cho thao tác nội bộ như masking/fingerprint/audit writer. Route guard chỉ tối ưu trải nghiệm; deep link luôn gọi API và xử lý 403/404 an toàn.

## 2. Shell desktop

```text
┌───────────────────────────────────────────────────────────────────────────┐
│ HTQLHDK                      [Tài sản: Tất cả được phép]  [🔔] [Người dùng]│
├────────────────┬──────────────────────────────────────────────────────────┤
│ Công việc      │ Breadcrumb                                               │
│ Tài sản        │ Tiêu đề                                      [Action hợp lệ]│
│ Lịch & lần quét│ Mô tả ngắn / trạng thái dữ liệu                           │
│ Lô nhập        ├──────────────────────────────────────────────────────────┤
│ Phát hiện      │ Filter cục bộ                                             │
│ Phiếu          │                                                           │
│ Xác minh       │ Table / detail content                                    │
│ Rủi ro         │                                                           │
│ Báo cáo        │                                                           │
│ ─────────────  │                                                           │
│ Nhật ký*       │                                                           │
│ Quyền*         │                                                           │
└────────────────┴──────────────────────────────────────────────────────────┘
```

Asset picker chỉ lọc trong tập quyền; không gửi/tự hiển thị asset ngoài scope. Sidebar ẩn mục không có permission. Trên màn hình hẹp, sidebar thành menu mở bằng nút có nhãn và giữ focus đúng chuẩn.

## 3. Wireframe Asset + scope

```text
Tài sản / WEB-PORTAL-01                         [Active]
[Tổng quan] [Phạm vi] [Lịch quét] [Lịch sử]

Scope đang áp dụng     Revision 4 · Approved · 05/09/2026
  Include (3)          Exclude (5)          Khung giờ (2, Asia/Bangkok)

Đề nghị Revision 5    Pending Approval
  Chưa ảnh hưởng lịch/import hiện tại
  Owner xác nhận: Nguyễn A · 07/09/2026 10:15 ICT
  [Xem thay đổi]                         [Phê duyệt] [Từ chối]
```

- Diff URL/rule hiển thị thêm/xóa/thay đổi bằng ký hiệu và text, không chỉ màu.
- Phê duyệt/từ chối mở dialog cho thấy scope hiện hành, scope đề nghị, ảnh hưởng tới lịch và trường lý do bắt buộc.
- Retire là destructive business action nhưng không xóa dữ liệu; dùng dialog gõ lại mã Asset hoặc checkbox xác nhận theo review usability.

## 4. Wireframe Import

```text
Nhập báo cáo OWASP ZAP
1 Chọn tài sản → 2 Chọn lần quét Completed → 3 Chọn tệp → 4 Kiểm tra

Tài sản       WEB-PORTAL-01 (Active)
Lần quét      SCAN-20260907-01 · Completed · Traditional scan
Định dạng     Traditional JSON / Traditional XML
Tệp           [Chọn tệp] report.json
               Giới hạn: lấy từ cấu hình đã duyệt

[Hủy]                                              [Tải lên]
```

Sau submit, vùng kết quả là nội dung bền vững trong trang:

```text
Lô IMP-001002 · Processing
[██████████░░░░] Đang chuẩn hóa bản ghi
Không cần giữ trang này mở. [Về danh sách]

hoặc

Lô đã tồn tại · IMP-000944 · Completed
Báo cáo trùng được liên kết tới lô cũ; không tạo dữ liệu mới. [Mở lô]
```

Chi tiết Partial:

```text
Partially Completed
Tổng 148  Thành công 143  Lỗi 5
⚠ Không dùng lô này để kết luận “không còn fingerprint” nếu coverage chưa đủ.
[Bản ghi hợp lệ] [Lỗi đã che]
Vị trí              Mã lỗi      Thông điệp an toàn         Cần review scope
/site/2/alerts/7    IMP-SCOPE   URL ngoài phạm vi          Có
```

## 5. Wireframe Finding detail

```text
FND-004218 · Thiếu kiểm soát truy cập                 [Pending Verification]
Asset WEB-PORTAL-01 · Lần đầu 01/09 · Gần nhất 07/09

Source Risk           Confidence             Final Severity
High                  Medium                 Critical
Từ ZAP, không sửa     Từ ZAP, không sửa      SECURITY xác nhận · có lịch sử

[Tổng quan] [Nguồn xuất hiện] [Đánh giá] [Phiếu] [Xác minh] [Rủi ro] [Lịch sử]

Biện pháp hiện hành: [nội dung đã che]
Evidence: EV-00141 · Remediation · [Tóm tắt đã che]

Action theo SECURITY: [Tạo yêu cầu xác minh]
Action theo REMEDIATOR: không có — đã gửi chờ xác minh
```

Không cho phép copy nhanh trường nguồn nhạy cảm nếu policy không cho. Nút tải evidence chỉ xuất hiện khi API context có cờ riêng; click mở xác nhận “lượt tải sẽ được ghi audit” và yêu cầu lý do.

## 6. Wireframe Verification decision

```text
Kết luận yêu cầu VER-001301
Finding: FND-004218 · Pending Verification
Bản sửa: 06/09/2026 16:20 ICT · người thực hiện Nguyễn B
Scan: SCAN-00905 · Verification · sau bản sửa
Import: IMP-01003 · Completed

Kiểm tra từ hệ thống
✓ Cùng asset và scope     ✓ Scan sau bản sửa
✓ Import đủ giá trị       ✓ Coverage đã được xác nhận
○ Fingerprint không còn

Kết luận  ( ) Closed   ( ) Reopened
Lý do *   [                                                   ]

⚠ Closed sẽ kết thúc chu kỳ hiện tại. Nếu fingerprint xuất hiện lại,
  hệ thống chuyển Reopened. Nhóm khắc phục không thể tự đóng.
[Hủy]                                      [Xác nhận kết luận]
```

Nếu evaluator thuộc danh sách người thực sự khắc phục, form không hiển thị action và giải thích cần người SECURITY độc lập; API vẫn trả AUTH-005 nếu deep link/race.

## 7. Wireframe Risk decision

```text
Đề nghị RA-001501 · Pending              Trạng thái trước: Assigned
Requester: Owner A                       Responsible: Owner A
Hạn đề nghị: 08/10/2026                  [Xem lịch sử]

Lý do / Tác động / Giảm thiểu / Khuyến nghị ATTT: nội dung đã che

Quyết định *  [Yêu cầu bổ sung | Phê duyệt | Từ chối]
Tham chiếu thẩm quyền * [                                    ]
Điều kiện *             [                                    ]
Hiệu lực từ/đến *       [                    ] [              ]
Lý do *                 [                                    ]

Từ chối sẽ khôi phục đúng trạng thái Assigned.
Phê duyệt sẽ chuyển Risk Accepted có thời hạn; quyết định không sửa lại.
```

Các trường authority/condition/time chỉ xuất hiện và bắt buộc với quyết định tương ứng. “Yêu cầu bổ sung” không ghi giả quyết định terminal.

## 8. Trạng thái hiển thị bắt buộc cho mỗi màn hình dữ liệu

| Trạng thái | Cách hiển thị |
|---|---|
| Initial loading | Skeleton theo hình dạng nội dung, có text screen-reader “Đang tải”; không spinner toàn trang vô hạn |
| Background refresh | Giữ dữ liệu cũ, chỉ báo nhỏ “Đang cập nhật”; action quan trọng khóa trong lúc xác minh version |
| Empty | Nêu vì sao trống và action hợp lệ nếu có; không dùng hình minh họa thay thông tin |
| Filter empty | “Không có kết quả theo bộ lọc”; giữ bộ lọc và nút xóa lọc |
| Safe error | Problem detail an toàn + correlation ID có nút sao chép; không stack trace |
| Denied/not found | Cùng UI-32, không xác nhận tồn tại; liên kết về worklist |
| Stale 412 | Inline conflict panel, giữ draft, hiển thị dữ liệu mới, không auto overwrite |
| Partial import | Counts + warning coverage + danh sách lỗi masked |
| Duplicate import | Neutral info banner + link lô cũ; không toast đỏ |
| Offline/network | Nêu action chưa được gửi/không rõ; retry dùng cùng idempotency key |

## 9. Dialog action quan trọng

Mỗi dialog có: tên action + đối tượng; trạng thái nguồn/đích; điều kiện đã đạt/chưa đạt; ảnh hưởng; trường lý do/evidence/authority bắt buộc; nút hủy trước, nút xác nhận sau; correlation ID khi lỗi. Các action gồm phê duyệt/từ chối scope, suspend/retire Asset, terminal scan, apply review, submit verification, Closed/Reopened, risk terminal decision, tải raw/evidence, tạo/tải/thu hồi export và revoke grant.
