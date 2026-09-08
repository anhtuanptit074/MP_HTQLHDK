# Ma trận trường hiển thị và masking

## 1. Mức hiển thị

| Mức | Ký hiệu | Ý nghĩa |
|---|---|---|
| Full-safe | F | Trường đã được phân loại an toàn cho màn hình trong scope |
| Masked | M | Server trả bản đã che; UI encode và hiển thị nhãn “Đã che” |
| Metadata only | D | Chỉ ID, loại, kích thước/trạng thái/tóm tắt masked; không nội dung |
| Separate audited stream | S | Endpoint riêng + cờ quyền nhạy cảm + lý do + audit mỗi lần |
| Omitted | – | Không có trong response/UI |

UI không tự “unmask”. Mọi quyền Full/Stream do API quyết định tại request hiện hành. Bảng dưới giả định actor đã vượt role + asset + owner/task scope.

## 2. Theo loại dữ liệu và vai trò

| Dữ liệu | SECURITY | OWNER | SCAN_OPS | REMEDIATOR | RISK_APPROVER | AUDITOR | ADMIN |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| Asset code/name/environment/status | F | F | F | F | F | F | – |
| Domain/base URL an toàn | F | F | F | M/F theo policy | M/F theo policy | M/F theo policy | – |
| Scope Include/Exclude/window | F | F | F | M/F theo task | M/F theo decision | M/F theo audit scope | – |
| Schedule/scan metadata | F | F | F | F theo task | F | F | – |
| Import status/counts/errors | F/M | F/M | F/M | F/M theo task | F/M | F/M | – |
| Raw ZAP metadata | D | D | D | D theo task | D | D | – |
| Raw ZAP bytes | S nếu raw flag | S nếu raw flag | S nếu raw flag | S nếu task+raw flag | S nếu raw flag | S nếu raw flag | – |
| Finding code/title/status/timestamps | F | F | F | F theo task | F | F | – |
| Source Risk | F | F | F | F | F | F | – |
| Source Confidence | F | F | F | F | F | F | – |
| Final Severity + reason | F/M | F/M | F/M | F/M | F/M | F/M | – |
| Attack/Evidence/URL/parameter nguồn | M | M | M | M | M | M | – |
| Evidence metadata/summary | D/M | D/M | D/M | D/M theo task | D/M | D/M | – |
| Evidence bytes | S nếu evidence flag | S nếu evidence flag | S nếu evidence flag | S nếu task+flag | S nếu flag | S nếu flag | – |
| Ticket assignee/progress/due/history | F/M | F/M | F/M | F/M theo task | F/M | F/M | – |
| Verification result/căn cứ masked | F/M | F/M | F/M | F/M theo task | F/M | F/M | – |
| Risk request/conditions/decision | F/M | F/M | – | – | F/M | F/M | – |
| Export metadata | F | F | F | F theo task | F | F | – |
| Export bytes | S theo recipient + all assets | S | S | S theo task | S | S | – |
| Permission audit | – | – | – | – | – | F | F |
| Business audit | F theo asset | – | – | – | – | F theo asset | – |
| Grant management | – | – | – | – | – | đọc audit | F metadata, không business data |

ADMIN không mặc nhiên xem Asset/Finding/raw/evidence. Cờ nhạy cảm không cấp qua UI cho chính người quản trị và luôn cần asset cụ thể + lý do.

## 3. Quy tắc masking nội dung

| Loại | Hiển thị thông thường | Không bao giờ hiển thị/log |
|---|---|---|
| URL | scheme/host/path theo policy; query key có thể giữ, value che | userinfo, token, session, secret query value |
| Header | allowlist tên cần thiết, value che theo policy | Authorization, Cookie, Set-Cookie, API key value |
| Attack/Evidence ZAP | summary masked, loại/độ dài nếu hữu ích | payload khai thác nguyên bản nếu chưa có quyền riêng |
| File | tên đã làm sạch, loại, size, thời gian, trạng thái | object key, encryption key ref, token hash, local path |
| Audit reason/before-after | bản đã che, key allowlist | request body thô, raw report, evidence bytes |
| Notification | asset code, finding code, trạng thái, internal path | title kỹ thuật nhạy cảm, Attack/Evidence, external URL, token |
| Error | business error code, safe detail, correlation ID | SQLSTATE, stack trace, internal host/path/key |

## 4. Pattern truy cập nhạy cảm

1. Người dùng chọn “Tải bản gốc” hoặc “Tải bằng chứng”.
2. Dialog hiển thị asset/finding/import/evidence code, lý do quyền được cấp và thông báo hành động được audit.
3. Người dùng nhập `X-Access-Reason` theo yêu cầu; UI không lưu lại reason vào local storage/autocomplete.
4. API kiểm lại role + asset/task + cờ riêng; 403/404 dùng màn hình an toàn.
5. Response stream `private, no-store`, tên tệp do server làm sạch; UI không preview tự động.
6. Nếu checksum mismatch, dừng tải/preview và hiển thị lỗi toàn vẹn; không cung cấp byte nghi ngờ.

## 5. Test fixture masking tối thiểu

Fixture phải chứa URL query token, Authorization header, cookie, password field, JWT-like string, API key, email/PII theo policy, HTML/script string và XML entity. Golden test xác nhận các giá trị này không xuất hiện trong DOM, clipboard action, export, notification, audit viewer, browser log hoặc analytics. Chỉ dùng dữ liệu giả/đã làm sạch.
