# Hướng dẫn ứng dụng 13 bảng

## Bắt đầu

Chạy `start-local.cmd` sau khi Docker Desktop sẵn sàng, rồi mở `http://127.0.0.1:5173/`. Lần đầu script tạo database riêng, mã hóa kho báo cáo và sinh mật khẩu ngẫu nhiên cho các tài khoản mẫu trong `.codex_tmp/local-accounts.txt`. Các lần sau không đặt lại mật khẩu, kích hoạt lại tài khoản đã khóa hoặc cấp lại quyền đã thu hồi.

Nếu vừa lấy mã nguồn, chạy `scripts/test.ps1` để chuẩn bị phụ thuộc/biên dịch trước. Dùng `start-local.cmd -Stop` trước khi cập nhật backend. Chạy lại `start-local.cmd` để mở bản đã cập nhật.

## Quy trình dùng thử đầy đủ

1. Đăng nhập `admin`: tạo người dùng Human/Service, cấp điều kiện nhận vai trò và quyền trên tài sản cụ thể. ADMIN chỉ thấy tên/mã tài sản trong danh sách cấp quyền; không tự đọc báo cáo lỗ hổng.
2. Đăng nhập `owner`: khai báo tài sản với URL HTTP/HTTPS không có query hoặc thông tin đăng nhập. Chọn người ATTT có điều kiện SECURITY. Tạo phiên bản phạm vi với luật Include/Exclude và khung giờ, rồi xác nhận phạm vi.
3. `admin` cấp SECURITY trên tài sản mới cho người ATTT được chọn; cấp SCAN_OPERATOR và REMEDIATOR trên tài sản cho người nhận nhiệm vụ. Quyền không gắn tài sản không thay thế bước này.
4. `operator` tạo hồ sơ ZAP nháp với tài khoản Service và gửi duyệt. `security` duyệt hồ sơ; active scan cần được cho phép riêng. Hồ sơ Active bất biến; tạo phiên bản mới nếu đổi cấu hình.
5. `security` phê duyệt phạm vi sau xác nhận của chủ tài sản, tạo kế hoạch định kỳ và kích hoạt. Phạm vi có ngày bắt đầu/kết thúc; các khoảng đã duyệt của cùng tài sản không được chồng nhau.
6. `operator` tạo lần quét đúng RRULE, xem lần quét được giao, ghi kết quả quét ngoài và tải báo cáo Traditional JSON/XML. Thời điểm bắt đầu được lệch tối đa 5 phút so với lịch, thời lượng không vượt cấu hình/khung giờ. Khi bật worker, các bước thực thi/nhập báo cáo được tự động hóa.
7. `security` đánh giá Final Severity với căn cứ, kết luận False Positive/Cancelled trước phân công nếu phù hợp, hoặc giao phiếu khắc phục và hạn xử lý. Source Risk/Confidence do ZAP cung cấp luôn được giữ riêng.
8. `remediator` cập nhật tiến độ, biện pháp, bằng chứng; có thể tải tệp bằng chứng tối đa 10 MB. Sau khi hoàn thành, gửi xác minh. Người khác không được sửa phiếu được giao này.
9. `security` tạo kế hoạch Verification sau thời điểm khắc phục. `operator` thực thi/nhập báo cáo. `security` xác nhận coverage và kết luận: fingerprint còn trong dữ liệu thực tế → Reopened; không còn và coverage đủ → Closed. Người khắc phục không tự đóng phát hiện. Báo cáo không hợp lệ/thiếu không cho phép kết luận đóng.
10. Dùng báo cáo tổng hợp/CSV và nhật ký để kiểm tra kết quả. CSV chỉ có dữ liệu đã che trong phạm vi người xuất, bảo vệ công thức khi mở trong bảng tính. AUDITOR chỉ đọc trong tài sản được cấp quyền.

Dữ liệu mẫu có hai website giả và hai phát hiện chưa phân loại. Có thể bắt đầu bước 7 với tài khoản `security`, sau đó chuyển sang `remediator`. Các domain `.invalid` chỉ là dữ liệu mô phỏng.

## Quyền, lịch sử và tính nhất quán

Năm role cố định: ADMIN, SECURITY, SCAN_OPERATOR, REMEDIATOR, AUDITOR. Chủ tài sản suy ra từ `asset.owner_id`. Không thêm bảng vai trò/OWNER. SECURITY thao tác tài sản khi đồng thời là người ATTT phụ trách và có grant rõ ràng trên tài sản đó. SCAN_OPERATOR chỉ đọc kế hoạch/lần quét và phát hiện nguồn thuộc nhiệm vụ được giao; REMEDIATOR chỉ đọc phát hiện/phiếu được giao. Việc thu hồi quyền, khóa tài khoản và đổi người phụ trách được kiểm tra lại ở mỗi yêu cầu. Đổi mật khẩu làm hết hiệu lực cookie cũ.

Các thao tác thay đổi tài nguyên hiện có dùng phiên bản hiện hành (`If-Match`); giao diện tự gửi phiên bản. API trả 428 khi thiếu hoặc 412 khi dữ liệu đã thay đổi. Dùng “Làm mới” trước khi thử lại. Quyền đọc và trạng thái được kiểm tra tại backend, không phụ thuộc nút có hiện trên giao diện hay không.

Phạm vi Approved không được sửa nội dung hoặc quyết định duyệt. Riêng phạm vi chưa có ngày kết thúc, SECURITY được đặt **một lần** thời điểm kết thúc ở tương lai, có lý do và audit. Đây là cách kết thúc phiên bản cũ trước khi duyệt phiên bản tiếp theo không chồng khoảng hiệu lực. Phạm vi đã có ngày kết thúc không được thay đổi lại. Chuyển người sở hữu/ATTT không sửa lịch sử phê duyệt cũ.

Một lần quét chỉ có một báo cáo nguồn JSON hoặc XML đã xử lý thành công. Nhập lại cùng checksum là thao tác idempotent; nguồn khác bị từ chối. Toàn bộ báo cáo được xác thực trước khi ghi phát hiện, sau đó ghi trong một giao dịch. Báo cáo lỗi không để lại bản ghi nhập dở và có trạng thái Rejected. Fingerprint dùng HMAC từ danh tính chuẩn hóa theo tài sản/plugin/phương thức/URL/tên tham số, không đưa giá trị query bí mật vào fingerprint.

Báo cáo gốc, danh tính fingerprint và tệp bằng chứng được mã hóa AES-256-GCM. Truy cập nguồn gốc cần SECURITY đúng tài sản, lý do truy cập và audit. Màn hình/CSV/audit chỉ dùng các trường được cho phép, đã che query, Attack, Evidence và bí mật phổ biến. Audit thay đổi được ghi cùng giao dịch nghiệp vụ và giữ trạng thái trước/sau; audit không được sửa/xóa.

Thông báo công việc, mức ưu tiên, quá hạn và số liệu báo cáo được suy ra bằng truy vấn; không tạo thêm bảng. Mô hình triển khai vẫn đúng 13 bảng. SLA theo cấu hình, nhóm/team, phê duyệt nhiều tầng, risk acceptance và bảng lưu thông báo/export của thiết kế trước không thuộc phạm vi tinh gọn này.

## Tự động ZAP

Mặc định worker chạy nhưng không phát lệnh quét. Để bật sau khi có mục tiêu thử nghiệm và các quyết định duyệt, dừng dịch vụ rồi chạy `start-local.cmd -EnableZap`. Script dùng image ZAP khóa digest trong cấu hình; Docker cần truy cập mục tiêu theo network đã cấu hình.

Worker tạo kỳ quét từ RRULE theo múi giờ IANA, kiểm tra tài sản/phạm vi/hồ sơ/tài khoản/quyền/khung giờ ngay trước khi chạy, nhận việc một lần bằng khóa giao dịch, ghi heartbeat và xử lý yêu cầu hủy. Kỳ bị trễ quá 5 phút không tự quét bù. Running mất heartbeat được đánh dấu Failed, container sở hữu được dừng; không tự khởi chạy lại kỳ đó. Thời lượng và tài nguyên Docker có giới hạn. Kết quả JSON/XML được nhập qua cùng giao dịch nghiệp vụ như nhập thủ công.

Hỗ trợ RRULE DAILY/WEEKLY/MONTHLY, INTERVAL, một BYDAY cho WEEKLY hoặc một BYMONTHDAY cho MONTHLY. Ngày không tồn tại (ví dụ 31 tháng 2) và giờ không tồn tại do DST được bỏ qua. Xác thực mục tiêu hỗ trợ None hoặc Header với tham chiếu `env:HTQLHDK_ZAP_...`; không nhập token thật vào hồ sơ/giao diện. Phải cấp biến bí mật cho tiến trình worker trước khi chạy.

Tham khảo [ZAP Automation Framework](https://www.zaproxy.org/docs/desktop/addons/automation-framework/) về chế độ chạy và exit code. Lần xác minh này dùng runner mô phỏng; chưa thực hiện quét website thực.

## API đang triển khai

Các đường dẫn dưới đây bắt đầu bằng `/api/v1/`. Đăng nhập bằng cookie; lấy token ở `GET auth/csrf` và gửi `X-CSRF-Token` cho thao tác ghi. Phiên bản dùng `If-Match: "<version>"` đối với tài nguyên đã tồn tại. Danh sách phân trang dùng `page[after]` và `page[size]`; lỗi trả Problem Details với mã tra cứu.

| Nhóm | Đường dẫn và thao tác |
| --- | --- |
| Phiên đăng nhập | `GET auth/csrf`; `POST auth/login`, `auth/logout`; `GET context` |
| Người dùng, quyền | `GET/POST users`; `PATCH users/{id}`; `GET access-assets`; `GET/POST access-grants`; `POST access-grants/{id}/revoke` |
| Tài sản | `GET/POST assets`; `GET/PATCH assets/{id}`; `POST assets/{id}/assignment`, `assets/{id}/transitions` |
| Phạm vi | `POST assets/{id}/scopes`; `PUT assets/{id}/scopes/{scopeId}`; `POST .../{scopeId}/confirm`, `.../{scopeId}/decision`, `.../{scopeId}/end-effect` |
| Hồ sơ ZAP | `GET/POST profiles`; `PUT profiles/{id}`; `POST profiles/{id}/submit`, `profiles/{id}/decision`, `profiles/{id}/retire` |
| Kế hoạch | `GET/POST plans`; `PUT plans/{id}`; `POST plans/{id}/transitions`, `plans/{id}/runs` |
| Lần quét | `GET runs`, `runs/{id}`; `POST runs/{id}/cancel`, `runs/{id}/record-result`, `runs/{id}/verification-decision` |
| Báo cáo nguồn | `POST runs/{id}/report` với multipart `file`, `format`; `GET runs/{id}/raw-report` với `X-Access-Reason` |
| Phát hiện | `GET findings`, `findings/{id}`; `POST findings/{id}/assess`, `findings/{id}/disposition`, `findings/{id}/tickets` |
| Khắc phục | `GET tickets`, `tickets/{id}`; `POST tickets/{id}/assignment`, `tickets/{id}/progress`, `tickets/{id}/submit-verification`, `tickets/{id}/verification-plans` |
| Bằng chứng | `POST tickets/{id}/evidence` với multipart `file`, `summary`; `GET tickets/{id}/evidence` với `X-Access-Reason` |
| Tổng hợp, lịch sử | `GET notifications`, `reports`, `reports/export`, `audit-logs` |

Kiểm tra sống/sẵn sàng tại `/health/live` và `/health/ready`. Đặc tả 57 API trong `api_design` là tài liệu của mô hình trước; dùng bảng trên và `tests/core13/http-flow.mjs` để đối chiếu bản 13 bảng hiện hành.

## Chạy kiểm thử

`scripts/test.ps1` cài đúng phụ thuộc từ lockfile, kiểm tra thiết kế cũ, build/test backend, lint/format/test/build frontend và 24 kiểm tra schema 13 bảng. `scripts/test.ps1 -IncludeDatabase` thêm 13 nhóm HTTP cùng kiểm thử worker PostgreSQL trên `htqlhdk_core_test_local`.

Kiểm thử database riêng dùng dữ liệu giả và mật khẩu riêng, không đổi tài sản/phiếu/tài khoản trong `htqlhdk_core_local` hoặc `htqlhdk_local`. Kết quả HTTP lưu ở `.codex_tmp/core13-tests/http-results.json`; kết quả schema ở `ops/core-migrations/verification-results.json`. `scripts/test-core13.ps1` yêu cầu bản Release đã biên dịch.

Pipeline CI có job core13 riêng để tạo schema trống, cấp tài khoản database không có quyền quản trị, seed dữ liệu giả và chạy cùng luồng HTTP/worker. CI chưa được chạy trên máy chủ từ xa trong lần phát triển local này.

## Database và khóa

`HTQLHDK_SCHEMA_MODEL=core13`, `HTQLHDK_DB_NAME=htqlhdk_core_local`. Migration mới nằm ở `ops/core-migrations`; migration cũ giữ ở `ops/migrations`. Checksum được ghi trong comment schema, không tạo bảng metadata thứ 14. SQL và comment phiên bản được áp dụng trong cùng giao dịch.

Không tự chuyển dữ liệu từ mô hình cũ vì cấu trúc, quyền và lịch sử thay đổi khác nhau. Database cũ được giữ để đối chiếu hoặc xây dựng công cụ chuyển đổi riêng.

Khóa local nằm ở `local-data/keys/core13.key`, nguồn mã hóa ở `local-data/core13/artifacts`, cookie key ở `.codex_tmp/session-keys`. Giữ backup database và các khóa cùng nhau nếu cần khôi phục dữ liệu. Ngoài Development, phải cấp khóa mã hóa sẵn qua `HTQLHDK_ENCRYPTION_KEY_FILE`; cookie yêu cầu HTTPS. Triển khai production và khôi phục backup chưa được xác minh trong bản chạy thử này.
