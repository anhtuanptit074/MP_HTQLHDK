# Danh mục mã lỗi nghiệp vụ — API v1

## 1. Envelope lỗi

Mọi lỗi dùng `application/problem+json`:

```json
{
  "type": "https://errors.htqlhdk.invalid/AUTH-003",
  "title": "Không thể thực hiện yêu cầu",
  "status": 403,
  "code": "AUTH-003",
  "detail": "Bạn không có quyền thực hiện thao tác này.",
  "correlationId": "4de18426-2239-4e80-a0ab-57e1dd0c9a34",
  "errors": [{"field": "file", "code": "FILE-002"}],
  "retryable": false
}
```

`detail` không xác nhận sự tồn tại của đối tượng ngoài phạm vi, không chứa dữ liệu ZAP/tệp/token. `errors` chỉ có tên trường công khai. Stack trace, SQLSTATE, object key và policy nội bộ không ra ngoài API.

## 2. Quy tắc HTTP và retry

| HTTP | Ý nghĩa chuẩn | Retry |
|---|---|---|
| 400 | JSON/header/query sai cú pháp | Chỉ sau khi sửa request |
| 401 | Chưa/còn không xác thực | Sau khi lấy phiên/token hợp lệ |
| 403 | Không đủ quyền; dùng cả khi cần tránh dò ID | Không; xin/cấp lại quyền ngoài request |
| 404 | Tài nguyên trong phạm vi không tồn tại | Không |
| 409 | Trùng idempotency khác payload, sai trạng thái, unique conflict | Đọc lại tài nguyên rồi quyết định |
| 412 | `If-Match`/row version cũ | GET lại, không tự ghi đè |
| 413 | Vượt giới hạn upload đã cấu hình | Không với cùng tệp |
| 415 | Media type/report format không hỗ trợ | Không với cùng tệp |
| 422 | Dữ liệu hợp lệ cú pháp nhưng vi phạm quy tắc nghiệp vụ | Chỉ sau khi sửa dữ liệu/điều kiện |
| 429 | Vượt rate limit | Có, theo `Retry-After`; mutation giữ cùng idempotency key |
| 500 | Lỗi nội bộ đã rollback | Chỉ khi `retryable=true`, giữ cùng key |
| 503 | Phụ thuộc tạm thời không sẵn sàng | Theo `Retry-After`, giữ cùng key |

## 3. Mã lỗi dùng chung

| Mã | HTTP | Khi phát sinh | Thông điệp an toàn | Retry |
|---|---:|---|---|---|
| AUTH-001 | 401 | Thiếu/không hợp lệ/hết hạn danh tính OIDC | Cần đăng nhập lại. | Có sau xác thực |
| AUTH-002 | 403 | Tài khoản ứng dụng inactive hoặc ánh xạ danh tính chưa có | Tài khoản chưa được phép sử dụng. | Không |
| AUTH-003 | 403 | Thiếu permission/asset grant/owner/task scope | Bạn không có quyền thực hiện thao tác này. | Không |
| AUTH-004 | 403 | Thiếu quyền raw report/evidence nhạy cảm | Bạn không có quyền truy cập nội dung nhạy cảm. | Không |
| AUTH-005 | 403 | Vi phạm tách nhiệm vụ | Người thực hiện không đáp ứng yêu cầu độc lập. | Không |
| REQ-001 | 400 | Body/query/header sai schema | Yêu cầu không hợp lệ. | Sau sửa |
| REQ-002 | 400 | Thiếu `Idempotency-Key` | Cần khóa chống lặp cho thao tác này. | Sau sửa |
| REQ-003 | 409 | Cùng key nhưng payload khác | Khóa chống lặp đã được dùng cho yêu cầu khác. | Không |
| RES-001 | 404 | Đối tượng hợp lệ trong phạm vi không tồn tại | Không tìm thấy tài nguyên. | Không |
| CON-001 | 428 | Thiếu `If-Match` khi bắt buộc | Cần phiên bản tài nguyên hiện hành. | Sau GET |
| CON-002 | 412 | `row_version`/ETag đã thay đổi | Dữ liệu đã thay đổi; hãy tải lại. | Sau GET |
| CON-003 | 409 | Unique conflict cạnh tranh đã được phân giải | Yêu cầu trùng với dữ liệu hiện có. | Đọc response |
| STATE-001 | 409 | Trạng thái nguồn không hợp lệ | Trạng thái hiện tại không cho phép thao tác. | Sau GET |
| STATE-002 | 422 | Thiếu điều kiện/căn cứ chuyển trạng thái | Chưa đủ điều kiện chuyển trạng thái. | Sau bổ sung |
| DATA-001 | 422 | FK/đối tượng không cùng asset | Dữ liệu liên kết không hợp lệ. | Sau sửa |
| DATA-002 | 422 | Giá trị policy chưa cấu hình/phê duyệt | Cấu hình nghiệp vụ chưa sẵn sàng. | Sau cấu hình |
| AUDIT-001 | 503 | Không ghi được audit bắt buộc | Tạm thời không thể thực hiện thao tác. | Có |
| FILE-001 | 413 | Tệp vượt giới hạn cấu hình | Tệp vượt giới hạn cho phép. | Không |
| FILE-002 | 415 | Không phải Traditional JSON/XML | Định dạng báo cáo không được hỗ trợ. | Không |
| FILE-003 | 422 | Checksum/kích thước/nội dung không khớp | Không thể xác minh tính toàn vẹn tệp. | Không |
| FILE-004 | 503 | Kho tệp/KMS lỗi trước finalize | Tạm thời không thể lưu hoặc đọc tệp. | Có |
| FILE-005 | 410 | Export/link đã hết hạn hoặc bị thu hồi | Nội dung tải không còn hiệu lực. | Không |
| RATE-001 | 429 | Vượt giới hạn request/upload/export | Vui lòng thử lại sau. | Có |
| SYS-001 | 500 | Lỗi không dự kiến; đã rollback | Không thể hoàn tất yêu cầu. | Theo cờ |
| SYS-002 | 503 | DB/object/KMS phụ thuộc tạm lỗi | Dịch vụ tạm thời chưa sẵn sàng. | Có |

## 4. Mã theo phân hệ

| Mã | HTTP | Điều kiện nghiệp vụ |
|---|---:|---|
| AST-001 | 409 | Mã tài sản đã tồn tại |
| AST-002 | 422 | Kích hoạt khi thiếu Owner/criticality/scope Approved hiện hành |
| AST-003 | 422 | Scope thiếu Include rule hoặc khung giờ hợp lệ |
| AST-004 | 409 | Sửa scope đã Approved/Rejected; phải tạo revision mới |
| AST-005 | 422 | Owner chưa xác nhận scope |
| SCH-001 | 422 | Lịch Active nhưng Asset không Active hoặc scope không hiện hành/Approved |
| SCH-002 | 422 | Target URL ngoài scope; Exclude có ưu tiên |
| SCH-003 | 422 | Lịch xung đột/ngoài khung giờ theo policy đã duyệt |
| SCH-004 | 409 | Cạnh trạng thái lịch/lần quét không hợp lệ |
| IMP-001 | 422 | Asset không Active hoặc scan chưa Completed/không cùng asset |
| IMP-002 | 422 | JSON/XML sai schema/template/version được hỗ trợ |
| IMP-003 | 422 | Báo cáo chứa mục tiêu ngoài scope |
| IMP-004 | 409 | Lô `(asset, scan, sha256)` đã tồn tại; response trả lô cũ |
| IMP-005 | 409 | Lô đã ở trạng thái kết thúc, không xử lý lại |
| IMP-006 | 422 | Partial không đủ coverage để dùng kết luận vắng fingerprint |
| FND-001 | 422 | Người nhập nguồn tự xác nhận Final Severity từ chính nguồn đó |
| FND-002 | 422 | Hạ High/Critical thiếu rà soát thứ hai độc lập |
| FND-003 | 409 | Review không còn là review hiện hành/phù hợp |
| FND-004 | 422 | False Positive/Cancelled thiếu lý do hoặc evidence |
| TKT-001 | 422 | Phân công thiếu assignee/team/hạn/SLA |
| TKT-002 | 403 | REMEDIATOR không phải người/nhóm được giao hiện hành |
| TKT-003 | 409 | `previous_event_id` không phải con trỏ hiện hành |
| TKT-004 | 422 | Chuyển Pending Verification thiếu biện pháp/evidence/người và thời điểm khắc phục |
| VER-001 | 422 | Scan không mới sau bản sửa hoặc không đúng asset/scope |
| VER-002 | 422 | Import chưa Completed/Partial đủ giá trị xác minh |
| VER-003 | 422 | Thiếu xác nhận coverage/fingerprint |
| VER-004 | 403 | Người kết luận đã tham gia khắc phục thực tế |
| VER-005 | 409 | Kết quả thuộc chu kỳ cũ/đã được áp dụng |
| RSK-001 | 409 | Finding không ở Triaged/Assigned/In Progress khi lập đề nghị |
| RSK-002 | 403 | Người duyệt là người đề nghị hoặc không đủ thẩm quyền |
| RSK-003 | 422 | Thiếu lý do/tác động/giảm thiểu/điều kiện/hạn/tham chiếu thẩm quyền |
| RSK-004 | 422 | Hạn không sau thời điểm quyết định hoặc vi phạm policy |
| RSK-005 | 409 | Quyết định Approved/Rejected đã bất biến |
| RSK-006 | 409 | Gia hạn không trỏ quyết định Approved đã áp dụng |
| RPT-001 | 403 | Không còn quyền trên một trong các asset của export |
| RPT-002 | 422 | Export không có asset hoặc người nhận hợp lệ |
| RPT-003 | 409 | Export đã phát hành không được đổi nội dung/danh sách asset |

## 5. Audit lỗi

`AUTH-*`, truy cập raw/evidence/export, mọi mutation bị từ chối, `STATE-*`, lỗi transaction và lỗi object/KMS phải ghi `audit_log` outcome `Denied` hoặc `Failure` trong giao dịch độc lập với cùng correlation ID. Audit chỉ lưu ID đã chuẩn hóa, action, object type/ID, asset khi đã xác định an toàn, reason đã che và mã lỗi; không sao chép request body.
