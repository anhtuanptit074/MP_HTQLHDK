# GIẢI THÍCH CHI TIẾT MÔ HÌNH CƠ SỞ DỮ LIỆU 13 BẢNG

## 1. Mục đích tài liệu

Tài liệu này giải thích mô hình BCNF tối giản được chốt trong [08_thiet_ke_3nf_tinh_gon.md](08_thiet_ke_3nf_tinh_gon.md). Mô hình chỉ phục vụ bốn chức năng chính: quản lý tài sản/phạm vi, lập kế hoạch và điều phối OWASP ZAP, tự động tiếp nhận kết quả, theo dõi khắc phục/xác minh.

Mô hình có 13 bảng vật lý:

1. `app_user`
2. `user_role`
3. `asset`
4. `asset_scope_revision`
5. `asset_scope_rule`
6. `asset_scan_window`
7. `zap_scan_profile`
8. `scan_plan`
9. `scan_run`
10. `finding`
11. `finding_occurrence`
12. `remediation_ticket`
13. `audit_log`

Kiểu dữ liệu trong tài liệu là kiểu đề xuất cho PostgreSQL. `NOT NULL` được hiểu là bắt buộc; dấu “—” ở cột quan hệ nghĩa là thuộc tính không phải khóa ngoại.

## 2. Phân nhóm và luồng dữ liệu

| Nhóm | Bảng | Kết quả nghiệp vụ |
|---|---|---|
| Định danh và quyền | `app_user`, `user_role` | Xác định người/tài khoản dịch vụ và quyền tối thiểu |
| Tài sản và phạm vi | `asset`, `asset_scope_revision`, `asset_scope_rule`, `asset_scan_window` | Chỉ cho quét tài sản, URL và thời điểm đã được duyệt |
| Điều phối ZAP | `zap_scan_profile`, `scan_plan`, `scan_run` | Tạo kế hoạch, gọi ZAP, theo dõi và lưu hồ sơ từng lần quét |
| Kết quả | `finding`, `finding_occurrence` | Chống trùng phát hiện và lưu lần xuất hiện theo lần quét |
| Khắc phục | `remediation_ticket` | Giao việc, cập nhật tiến độ, bằng chứng và yêu cầu xác minh |
| Truy vết | `audit_log` | Ghi hành động nhạy cảm và lịch sử trước/sau |

Luồng liên kết chính:

```text
Asset → Scope Revision → Scope Rule / Scan Window
                         ↓
Zap Profile ─────────→ Scan Plan → Scan Run → Finding Occurrence ← Finding
                                      ↑                              ↓
                                      └── Verification Plan ← Remediation Ticket

App User → User Role và các FK người tạo/phê duyệt/phụ trách
Mọi hành động nhạy cảm → Audit Log
```

## 3. Giải thích từng bảng

## 3.1. `app_user` — người dùng và tài khoản dịch vụ

### Vai trò và chức năng

Lưu định danh tối thiểu của người sử dụng hệ thống và tài khoản kỹ thuật dùng cho kết nối ZAP. Bảng không lưu cơ cấu tổ chức, team hay role trực tiếp.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | Khóa chính nội bộ |
| `username` | varchar(120) | Có | Tên đăng nhập duy nhất |
| `display_name` | text | Có | Tên hiển thị |
| `account_kind` | varchar(10) | Có | `Human` hoặc `Service` |
| `password_hash` | text | Không | Chuỗi băm mật khẩu của tài khoản Human khi dùng đăng nhập nội bộ |
| `is_active` | boolean | Có | Tài khoản còn được sử dụng hay không |
| `created_at` | timestamptz | Có | Thời điểm tạo |

### Khóa và ràng buộc

- PK: `id`.
- UNIQUE: `username`.
- CHECK `account_kind IN ('Human','Service')`.
- Tài khoản Service phải có `password_hash IS NULL`.
- Tài khoản Service không được làm người xác nhận/phê duyệt/đánh giá nghiệp vụ.

### Quan hệ

- 1–N với `user_role` qua `user_role.user_id`.
- Được `asset` tham chiếu ở vai trò owner, ATTT phụ trách và người tạo.
- Được `zap_scan_profile` tham chiếu ở vai trò service user, người tạo và người phê duyệt.
- Được `scan_plan`, `scan_run`, `finding`, `remediation_ticket`, `audit_log` tham chiếu theo vai trò tương ứng.

### Vòng đời và dữ liệu suy ra

Tài khoản không bị xóa cứng nếu đã được tham chiếu. Khóa tài khoản bằng `is_active = false`. Quyền hiệu lực được truy từ `user_role`, không lưu trong bảng này.

## 3.2. `user_role` — quyền tối thiểu theo người dùng và tài sản

### Vai trò và chức năng

Ghi việc cấp/thu hồi một vai trò cố định cho người dùng. Bảng trả lời câu hỏi “người này có được phép đảm nhiệm vai trò này không”; các bảng nghiệp vụ trả lời câu hỏi “người này đang phụ trách đối tượng nào”.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `user_id` | bigint | Có | FK → `app_user.id` |
| `role_code` | varchar(30) | Có | ADMIN, SECURITY, SCAN_OPERATOR, REMEDIATOR, AUDITOR |
| `asset_id` | bigint | Không | FK → `asset.id`; NULL cho quyền toàn hệ thống được cho phép |
| `granted_by` | bigint | Có | FK → `app_user.id`; người cấp |
| `granted_at` | timestamptz | Có | Thời điểm cấp |
| `revoked_by` | bigint | Không | FK → `app_user.id`; người thu hồi |
| `revoked_at` | timestamptz | Không | Thời điểm thu hồi |

### Khóa và ràng buộc

- PK: `id`.
- Partial UNIQUE cho quyền đang hiệu lực: `(user_id, role_code, asset_id)` với `revoked_at IS NULL`; khi `asset_id` nullable cần dùng `NULLS NOT DISTINCT` hoặc hai chỉ mục có điều kiện.
- ADMIN bắt buộc `asset_id IS NULL`.
- SECURITY đọc báo cáo gốc phải có `asset_id` cụ thể.
- `revoked_by` và `revoked_at` cùng NULL hoặc cùng khác NULL.
- Người cấp/thu hồi và người được cấp phải là tài khoản Human đang hoạt động.

### Quan hệ

- N–1 đến `app_user` qua `user_id`, `granted_by`, `revoked_by`.
- N–1 tùy chọn đến `asset` qua `asset_id`.
- Không có bảng `role`/`permission`; `role_code` là miền giá trị cố định.

### Cách sử dụng

- Gán `asset.security_officer_id` yêu cầu SECURITY phù hợp.
- Gán `scan_plan.operator_id` yêu cầu SCAN_OPERATOR phù hợp.
- Gán `remediation_ticket.assignee_id` yêu cầu REMEDIATOR phù hợp.
- Owner được xác định bởi `asset.owner_id`, không tạo OWNER role trùng lặp.

## 3.3. `asset` — tài sản website

### Vai trò và chức năng

Là gốc của dữ liệu nghiệp vụ. Mọi kế hoạch, lần quét, phát hiện và phiếu khắc phục đều truy ngược được về một asset.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `code` | varchar(40) | Có | Mã tài sản duy nhất |
| `name` | text | Có | Tên website/ứng dụng |
| `base_url` | text | Có | URL gốc; hostname được phân tích khi cần |
| `environment` | varchar(30) | Có | Production, Staging, Test hoặc giá trị được đơn vị duyệt |
| `owner_id` | bigint | Có trước Active | FK → `app_user.id` |
| `security_officer_id` | bigint | Có trước Active | FK → `app_user.id` |
| `criticality` | varchar(20) | Có trước Active | Mức quan trọng nghiệp vụ |
| `data_classification` | varchar(30) | Không | Phân loại dữ liệu xử lý |
| `internet_exposed` | boolean | Có trước Active | Có công khai Internet hay không |
| `status` | varchar(30) | Có | Draft, Pending Approval, Active, Suspended, Retired |
| `created_by` | bigint | Có | FK → `app_user.id` |
| `created_at` | timestamptz | Có | Thời điểm tạo |
| `updated_at` | timestamptz | Có | Thời điểm cập nhật cuối |

### Khóa và ràng buộc

- PK: `id`; UNIQUE: `code`.
- `base_url` phải dùng HTTP/HTTPS, không chứa credential hoặc fragment.
- Asset Active phải có owner, ATTT, criticality, exposure và ít nhất một scope Approved đang hiệu lực.
- Suspended/Retired không được tạo scan run mới.

### Quan hệ

- N–1 đến `app_user` qua owner, ATTT và người tạo.
- 1–N với `user_role` và `asset_scope_revision`.
- 1–N với `finding`.
- Không nối trực tiếp với `scan_plan`/`scan_run`; quan hệ đi qua scope revision để giữ đúng phiên bản phạm vi.

### Dữ liệu không lưu

- `domain`: suy ra từ `base_url`; domain bổ sung nằm ở scope rule.
- `current_scope_id`: suy ra từ revision Approved trong khoảng hiệu lực.
- Số lần quét, số finding và tình trạng khắc phục: truy qua các bảng liên quan.

## 3.4. `asset_scope_revision` — phiên bản phạm vi quét

### Vai trò và chức năng

Đóng gói một phiên bản phạm vi đã được owner xác nhận và ATTT phê duyệt. Việc version hóa cho phép giải thích chính xác một lần quét đã dùng phạm vi nào mà không lưu snapshot trùng lặp trên scan run.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `asset_id` | bigint | Có | FK → `asset.id` |
| `version_no` | integer | Có | Số phiên bản tăng trong từng asset |
| `status` | varchar(30) | Có | Draft, Pending Approval, Approved, Rejected |
| `change_reason` | text | Có | Lý do tạo/thay đổi phạm vi |
| `created_by` | bigint | Có | FK → `app_user.id` |
| `created_at` | timestamptz | Có | Thời điểm tạo |
| `confirmed_by` | bigint | Không | FK → `app_user.id`; owner xác nhận |
| `confirmed_at` | timestamptz | Không | Thời điểm xác nhận |
| `decided_by` | bigint | Không | FK → `app_user.id`; ATTT quyết định |
| `decided_at` | timestamptz | Không | Thời điểm quyết định |
| `decision_reason` | text | Không | Lý do duyệt/từ chối |
| `effective_from` | timestamptz | Không | Bắt đầu hiệu lực khi Approved |
| `effective_until` | timestamptz | Không | Kết thúc hiệu lực, NULL nếu chưa xác định |

### Khóa và ràng buộc

- PK: `id`; UNIQUE `(asset_id, version_no)`.
- Approved/Rejected phải có người, thời điểm và lý do quyết định.
- Approved phải có `effective_from`; `effective_until > effective_from` khi có.
- Các khoảng Approved của cùng asset không chồng lấn.
- Approved/Rejected bất biến; thay đổi tạo revision mới.

### Quan hệ

- N–1 đến `asset`.
- 1–N đến `asset_scope_rule`, `asset_scan_window`, `scan_plan`.
- N–1 đến `app_user` qua người tạo/xác nhận/quyết định.

### Dữ liệu suy ra

Scope hiện hành là revision Approved thỏa `effective_from <= now()` và `effective_until IS NULL OR effective_until > now()`.

## 3.5. `asset_scope_rule` — luật include/exclude

### Vai trò và chức năng

Lưu từng domain, subdomain hoặc URL được phép hay bị loại trừ. Đây là nguồn để bộ điều phối dựng target và kiểm tra mọi URL trước/sau khi nhận kết quả từ ZAP.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `scope_id` | bigint | Có | FK → `asset_scope_revision.id` |
| `effect` | varchar(10) | Có | Include hoặc Exclude |
| `target_type` | varchar(15) | Có | Domain, Subdomain hoặc URL |
| `target_value` | text | Có | Giá trị chuẩn hóa, không chứa bí mật |
| `match_mode` | varchar(10) | Có | Exact hoặc Prefix |

### Khóa và ràng buộc

- PK: `id`.
- UNIQUE `(scope_id, effect, target_type, target_value, match_mode)`.
- Prefix chỉ áp dụng cho URL; domain/subdomain dùng Exact theo quy tắc host.
- Exclude được ưu tiên khi một URL đồng thời khớp Include và Exclude.
- Không sửa rule của revision Approved.

### Quan hệ

- N–1 đến `asset_scope_revision`.
- Không nối trực tiếp với scan run; scan run truy rule qua plan và scope.

### Lý do tồn tại riêng

Một scope có nhiều rule. Gộp thành mảng/JSON sẽ vi phạm mục tiêu chuẩn hóa và làm yếu UNIQUE, FK và kiểm tra từng target.

## 3.6. `asset_scan_window` — khung giờ quét

### Vai trò và chức năng

Lưu các cửa sổ thời gian mà asset được phép quét, đặc biệt quan trọng đối với production và active scan.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `scope_id` | bigint | Có | FK → `asset_scope_revision.id` |
| `weekday_iso` | smallint | Có | 1–7, thứ Hai đến Chủ nhật |
| `start_time` | time | Có | Giờ bắt đầu địa phương |
| `end_time` | time | Có | Giờ kết thúc địa phương |
| `ends_next_day` | boolean | Có | Cửa sổ kéo qua nửa đêm |
| `timezone` | text | Có | Múi giờ IANA, ví dụ Asia/Bangkok |

### Khóa và ràng buộc

- PK: `id`.
- UNIQUE `(scope_id, weekday_iso, start_time, end_time, ends_next_day, timezone)`.
- `weekday_iso BETWEEN 1 AND 7`.
- Nếu không qua ngày thì `end_time > start_time`; nếu qua ngày thì cho phép `end_time <= start_time`.
- Không sửa window của revision Approved.

### Quan hệ và sử dụng

- N–1 đến `asset_scope_revision`.
- Worker truy window qua `scan_plan.scope_id` trước khi tạo hoặc bắt đầu `scan_run`.

## 3.7. `zap_scan_profile` — hồ sơ cấu hình OWASP ZAP

### Vai trò và chức năng

Lưu cấu hình quét được hệ thống cho phép gửi đến ZAP. Profile là whitelist cấu hình đã duyệt, không cho người dùng gửi Automation Plan/script tùy ý.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `code` | varchar(40) | Có | Mã ổn định của profile |
| `name` | text | Có | Tên hiển thị |
| `version_no` | integer | Có | Phiên bản |
| `status` | varchar(20) | Có | Draft, Pending Approval, Active, Retired |
| `spider_kind` | varchar(20) | Có | Traditional hoặc Client |
| `passive_scan_enabled` | boolean | Có | Bật passive scan |
| `active_scan_enabled` | boolean | Có | Bật active scan; mặc định false |
| `spider_max_minutes` | integer | Có | Timeout spider |
| `passive_wait_max_minutes` | integer | Có | Thời gian chờ passive scan |
| `active_scan_max_minutes` | integer | Có | Timeout active scan |
| `max_rule_minutes` | integer | Có | Giới hạn một rule |
| `request_delay_ms` | integer | Có | Khoảng nghỉ giữa request |
| `thread_per_host` | integer | Có | Số luồng tối đa mỗi host |
| `auth_kind` | varchar(20) | Có | None hoặc Header |
| `auth_header_name` | text | Không | Tên header xác thực |
| `auth_secret_ref` | text | Không | Tham chiếu kho bí mật, không chứa secret thật |
| `report_format` | varchar(10) | Có | JSON hoặc XML |
| `service_user_id` | bigint | Có | FK → `app_user.id`, phải là Service |
| `created_by` | bigint | Có | FK → `app_user.id` |
| `created_at` | timestamptz | Có | Thời điểm tạo |
| `approved_by` | bigint | Không | FK → `app_user.id`, Human có SECURITY |
| `approved_at` | timestamptz | Không | Thời điểm duyệt |
| `retired_at` | timestamptz | Không | Thời điểm ngừng sử dụng |

### Khóa và ràng buộc

- PK: `id`; UNIQUE `(code, version_no)`.
- Active phải được phê duyệt; Retired phải có `retired_at`.
- Active/Retired bất biến; cấu hình mới tạo version mới.
- `service_user_id` phải trỏ tài khoản Service đang hoạt động.
- Header auth phải có tên header hợp lệ và secret reference hợp lệ.

### Quan hệ

- N–1 đến `app_user` qua service user/người tạo/người duyệt.
- 1–N với `scan_plan`.
- Không tạo `zap_connection`; endpoint và API key thực nằm trong cấu hình triển khai/kho bí mật.

## 3.8. `scan_plan` — kế hoạch quét định kỳ hoặc xác minh

### Vai trò và chức năng

Biểu diễn nguyên nhân hợp lệ để hệ thống tạo một lần quét. Dùng chung một bảng cho hai loại kế hoạch giúp loại bỏ bảng verification request riêng nhưng vẫn giữ quan hệ nguồn rõ ràng.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `code` | varchar(40) | Có | Mã kế hoạch |
| `version_no` | integer | Có | Phiên bản kế hoạch |
| `name` | text | Có | Tên hiển thị |
| `plan_kind` | varchar(20) | Có | Periodic hoặc Verification |
| `scope_id` | bigint | Có | FK → `asset_scope_revision.id` |
| `zap_profile_id` | bigint | Có | FK → `zap_scan_profile.id` |
| `operator_id` | bigint | Có | FK → `app_user.id`, người vận hành phụ trách |
| `status` | varchar(20) | Có | Draft, Active, Paused, Expired, Cancelled |
| `recurrence_rule` | text | Tùy loại | RRULE giới hạn cho Periodic |
| `timezone` | text | Tùy loại | Múi giờ Periodic |
| `effective_from` | timestamptz | Tùy loại | Bắt đầu lịch Periodic |
| `effective_until` | timestamptz | Không | Kết thúc lịch Periodic |
| `remediation_ticket_id` | bigint | Tùy loại | FK → `remediation_ticket.id` cho Verification |
| `requested_for` | timestamptz | Tùy loại | Thời điểm mong muốn quét xác minh |
| `created_by` | bigint | Có | FK → `app_user.id` |
| `created_at` | timestamptz | Có | Thời điểm tạo |
| `approved_by` | bigint | Không | FK → `app_user.id` |
| `approved_at` | timestamptz | Không | Thời điểm duyệt/kích hoạt |

### Khóa và ràng buộc

- PK: `id`; UNIQUE `(code, version_no)`.
- Periodic bắt buộc recurrence/timezone/effective_from và không có remediation ticket.
- Verification bắt buộc remediation ticket/requested_for và không có recurrence.
- Active yêu cầu scope Approved còn hiệu lực và profile Active.
- Operator phải có SCAN_OPERATOR phù hợp với asset của scope.
- Verification plan và ticket phải thuộc cùng asset.
- Plan đã có run không được sửa scope, profile hoặc lịch; thay đổi tạo version mới.

### Quan hệ

- N–1 đến `asset_scope_revision`, `zap_scan_profile`, `app_user`.
- N–1 tùy chọn đến `remediation_ticket` đối với Verification.
- 1–N đến `scan_run`; riêng Verification là 1–0..1.

### Dữ liệu không lưu

- `asset_id`: truy qua scope.
- `next_due_at`: tính từ recurrence và lần chạy gần nhất.
- `target_urls`: truy từ scope rules.

## 3.9. `scan_run` — hồ sơ lịch sử từng lần quét

### Vai trò và chức năng

Mỗi dòng là một lần thực thi cụ thể. Bảng hợp nhất lần quét nghiệp vụ, phiên ZAP và metadata báo cáo nguồn vì trong mô hình tối giản chúng có quan hệ 1:1.

Đây là bảng trả lời: lần quét nào đã được tạo, ZAP đã chạy ra sao, báo cáo nào đã được nhận, lỗi gì xảy ra và kết quả xác minh là gì.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `plan_id` | bigint | Có | FK → `scan_plan.id` |
| `code` | varchar(40) | Có | Mã lần quét duy nhất |
| `scheduled_for` | timestamptz | Có | Mốc kỳ quét hoặc lịch xác minh |
| `scan_status` | varchar(20) | Có | Planned, Queued, Running, Completed, Failed, Timed Out, Cancelled |
| `requested_at` | timestamptz | Không | Thời điểm đưa vào hàng đợi/gửi yêu cầu |
| `started_at` | timestamptz | Không | Thời điểm ZAP bắt đầu |
| `heartbeat_at` | timestamptz | Không | Heartbeat kỹ thuật gần nhất |
| `finished_at` | timestamptz | Không | Thời điểm kết thúc |
| `external_plan_id` | text | Không | Plan ID do ZAP/runner trả về |
| `external_session_id` | text | Không | Session ID bên ngoài |
| `cancel_requested_at` | timestamptz | Không | Thời điểm yêu cầu dừng |
| `runner_name` | text | Không | Worker/runner thực hiện |
| `image_reference` | text | Không | Image/version thực tế đã chạy |
| `zap_version` | text | Không | Phiên bản ZAP thực tế |
| `exit_code` | integer | Không | Mã kết thúc runner |
| `failure_code` | varchar(80) | Không | Mã lỗi chuẩn hóa |
| `failure_message_masked` | text | Không | Lỗi đã che bí mật |
| `report_status` | varchar(20) | Có | Not Received, Validating, Processed, Rejected, Failed |
| `report_object_key` | text | Không | Khóa tệp nguồn trong kho riêng |
| `report_sha256` | char(64) | Không | SHA-256 tệp nguồn |
| `report_size_bytes` | bigint | Không | Kích thước tệp |
| `report_format` | varchar(10) | Không | JSON hoặc XML thực nhận |
| `report_key_ref` | text | Không | Tham chiếu khóa mã hóa |
| `report_received_at` | timestamptz | Không | Thời điểm nhận báo cáo |
| `coverage_confirmed` | boolean | Không | Độ bao phủ được xác nhận cho verification |
| `fingerprint_present` | boolean | Không | Dấu vân tay còn xuất hiện hay không |
| `verification_outcome` | varchar(10) | Không | Closed hoặc Reopened |
| `verification_reason` | text | Không | Lý do đánh giá |
| `evaluated_by` | bigint | Không | FK → `app_user.id` |
| `evaluated_at` | timestamptz | Không | Thời điểm ATTT đánh giá |

### Khóa và ràng buộc

- PK: `id`; UNIQUE `code`; UNIQUE `(plan_id, scheduled_for)` để chống tạo trùng kỳ.
- Verification plan có tối đa một run.
- Terminal status phải có `finished_at`; Failed/Timed Out phải có `failure_code`.
- Metadata tệp hoặc cùng NULL, hoặc đầy đủ object key/hash/size/format/key ref.
- Report Processed chỉ khi scan Completed và metadata tệp hợp lệ.
- Các cột verification chỉ được dùng khi `scan_plan.plan_kind = 'Verification'`.
- Closed yêu cầu coverage true, fingerprint false và báo cáo Processed.

### Quan hệ

- N–1 đến `scan_plan`.
- 1–N đến `finding_occurrence`.
- N–1 tùy chọn đến `app_user` qua người đánh giá.
- Asset, scope, profile, service user và operator được truy duy nhất qua plan; không lặp FK.

### Vòng đời

Một run được tạo Planned/Queued, chuyển Running, sau đó terminal. Không xóa cứng run đã bắt đầu. Report metadata và danh tính run bất biến sau khi nhận; chỉ trạng thái kỹ thuật và đánh giá verification được cập nhật theo workflow có audit.

### Dữ liệu suy ra

- Số finding: COUNT `finding_occurrence`.
- Tài sản: run → plan → scope → asset.
- Thời lượng: `finished_at - started_at`.
- Thành công theo kỳ: trạng thái và report status, không cần bảng summary.

## 3.10. `finding` — phát hiện chuẩn hóa

### Vai trò và chức năng

Lưu danh tính ổn định của một lỗ hổng trên một asset. Nhiều lần quét gặp cùng dấu vân tay vẫn dùng một finding; lịch sử xuất hiện nằm trong `finding_occurrence`.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `asset_id` | bigint | Có | FK → `asset.id` |
| `code` | varchar(40) | Có | Mã hiển thị duy nhất |
| `fingerprint` | char(64) | Có | HMAC/SHA-256 định danh chống trùng |
| `identity_version` | smallint | Có | Phiên bản thuật toán chuẩn hóa |
| `identity_key_ref` | text | Có | Tham chiếu khóa tạo fingerprint |
| `identity_ciphertext` | bytea | Có | Bộ định danh chuẩn hóa được mã hóa |
| `source_alert_ref` | text | Có | ZAP Alert Reference/Rule ID |
| `affected_url_masked` | text | Có | URL hiển thị đã che |
| `parameter_masked` | text | Không | Tham số ảnh hưởng đã che |
| `input_vector_masked` | text | Không | Vị trí đầu vào đã che |
| `http_method` | varchar(10) | Có | HTTP method chuẩn hóa; rỗng nếu không biết |
| `title_masked` | text | Có | Tên phát hiện an toàn để hiển thị |
| `final_severity` | varchar(20) | Không | Informational, Low, Medium, High, Critical |
| `severity_reason` | text | Không | Lý do xác nhận/điều chỉnh |
| `assessed_by` | bigint | Không | FK → `app_user.id` |
| `assessed_at` | timestamptz | Không | Thời điểm đánh giá |
| `status` | varchar(30) | Có | Trạng thái workflow hiện hành |
| `decision_reason` | text | Không | Lý do False Positive/Cancelled/đóng/mở lại |
| `decision_by` | bigint | Không | FK → `app_user.id` |
| `decision_at` | timestamptz | Không | Thời điểm quyết định |
| `created_at` | timestamptz | Có | Thời điểm tạo finding |
| `row_version` | bigint | Có | Kiểm soát cập nhật đồng thời |

### Khóa và ràng buộc

- PK: `id`; UNIQUE `code`; UNIQUE `(asset_id, fingerprint)`.
- Fingerprint không dùng Attack/Evidence.
- Triaged trở đi phải có final severity, lý do và người đánh giá.
- False Positive/Cancelled/Closed/Reopened phải có decision metadata phù hợp.
- Chỉ ATTT có SECURITY đúng asset được đánh giá và chuyển trạng thái quyết định.

### Quan hệ

- N–1 đến `asset`.
- N–1 đến `app_user` qua người đánh giá/quyết định.
- 1–N đến `finding_occurrence`.
- 1–0..1 đến `remediation_ticket` trong mô hình tối giản.

### Dữ liệu không lưu

- First seen/last seen: MIN/MAX run time qua occurrence.
- Source severity/confidence: thuộc từng occurrence.
- Lịch sử final severity và status: lấy từ audit log.
- Kết quả xác minh: ticket → verification plan → scan run.

## 3.11. `finding_occurrence` — lần xuất hiện của phát hiện

### Vai trò và chức năng

Là bảng nối M:N giữa finding và scan run. Đồng thời giữ chi tiết nguồn ZAP có thể thay đổi giữa các lần quét.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `finding_id` | bigint | Có | FK → `finding.id` |
| `scan_run_id` | bigint | Có | FK → `scan_run.id` |
| `record_locator` | text | Có | JSON pointer/XML path của bản ghi nguồn |
| `source_severity` | varchar(20) | Có | Informational, Low, Medium, High |
| `source_confidence` | smallint | Có | Confidence ZAP chuẩn hóa |
| `source_attack_masked` | text | Không | Payload attack đã che |
| `source_evidence_masked` | text | Không | Evidence nguồn đã che |
| `description_masked` | text | Không | Mô tả nguồn đã che |
| `technical_details_masked` | text | Không | Chi tiết kỹ thuật đã che |
| `remediation_guidance_masked` | text | Không | Khuyến nghị nguồn |
| `cwe_id` | integer | Không | CWE từ ZAP |
| `wasc_id` | integer | Không | WASC từ ZAP |
| `created_at` | timestamptz | Có | Thời điểm chuẩn hóa |

### Khóa và ràng buộc

- PK: `id`.
- UNIQUE `(finding_id, scan_run_id)` để một finding chỉ có một occurrence chuẩn trong một run.
- UNIQUE `(scan_run_id, record_locator)` để một bản ghi nguồn không xử lý hai lần.
- Finding và run phải thuộc cùng asset; cần trigger hoặc kiểm tra trong transaction nhập.
- Chỉ tạo occurrence khi report status chuyển Processed trong cùng transaction.

### Quan hệ

- N–1 đến `finding` và `scan_run`.
- Asset được suy ra từ cả hai đường và phải khớp; không lưu `asset_id` lần thứ ba.

### Dữ liệu suy ra

Occurrence time dùng thời gian run, không cần `observed_at`. Source title/URL định danh ổn định lấy từ finding; dữ liệu nguồn biến đổi nằm ở occurrence.

## 3.12. `remediation_ticket` — phiếu khắc phục

### Vai trò và chức năng

Lưu trạng thái làm việc hiện hành của hoạt động khắc phục một finding: ai phụ trách, hạn, tiến độ, biện pháp và bằng chứng. Lịch sử thay đổi nằm trong audit log.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `finding_id` | bigint | Có | FK → `finding.id`, UNIQUE |
| `code` | varchar(40) | Có | Mã phiếu duy nhất |
| `assignee_id` | bigint | Có | FK → `app_user.id` |
| `assigned_by` | bigint | Có | FK → `app_user.id` |
| `assigned_at` | timestamptz | Có | Thời điểm giao |
| `due_at` | timestamptz | Có | Hạn xử lý được quyết định |
| `progress_percent` | smallint | Có | 0–100, người xử lý cập nhật |
| `root_cause_masked` | text | Không | Nguyên nhân đã che |
| `remediation_action_masked` | text | Không | Biện pháp đã thực hiện |
| `evidence_summary_masked` | text | Không | Tóm tắt bằng chứng |
| `evidence_object_key` | text | Không | Tệp bằng chứng trong kho riêng |
| `evidence_sha256` | char(64) | Không | Checksum bằng chứng |
| `evidence_key_ref` | text | Không | Tham chiếu khóa mã hóa |
| `remediated_at` | timestamptz | Không | Thời điểm xác nhận đã triển khai bản sửa |
| `created_by` | bigint | Có | FK → `app_user.id` |
| `created_at` | timestamptz | Có | Thời điểm tạo |
| `updated_by` | bigint | Có | FK → `app_user.id` |
| `updated_at` | timestamptz | Có | Thời điểm cập nhật cuối |

### Khóa và ràng buộc

- PK: `id`; UNIQUE `code`; UNIQUE `finding_id`.
- Assignee phải có REMEDIATOR phù hợp với asset của finding.
- Assigned by/created by phải có SECURITY phù hợp.
- `progress_percent BETWEEN 0 AND 100`.
- Bằng chứng tệp hoặc cùng NULL, hoặc đầy đủ object key/hash/key ref.
- Trước Pending Verification phải có remediation action, bằng chứng và `remediated_at`.

### Quan hệ

- N–1 nhưng UNIQUE đến `finding`, tạo quan hệ finding 1–0..1 ticket.
- N–1 đến `app_user` qua assignee/người giao/người tạo/người cập nhật.
- 1–N đến `scan_plan` loại Verification.

### Dữ liệu không lưu

- Trạng thái phiếu: dùng duy nhất `finding.status`.
- Lịch sử phân công/tiến độ/hạn: audit before/after.
- SLA rule: không thuộc phạm vi lõi; `due_at` là quyết định đã được nhập.
- Verification result: truy qua verification plan và scan run.

## 3.13. `audit_log` — nhật ký 

### Vai trò và chức năng

Ghi hành động nhạy cảm của người dùng, tài khoản dịch vụ hoặc hệ thống: cấp quyền, duyệt scope/profile/plan, điều phối ZAP, nhận báo cáo, đổi severity/status, cập nhật phiếu và truy cập báo cáo gốc.

### Thuộc tính

| Cột | Kiểu đề xuất | Bắt buộc | Quan hệ/ý nghĩa |
|---|---|---:|---|
| `id` | bigint identity | Có | PK |
| `actor_id` | bigint | Không | FK → `app_user.id`; NULL cho System/Unauthenticated |
| `actor_kind` | varchar(20) | Có | Human, Service, System, Unauthenticated |
| `occurred_at` | timestamptz | Có | Thời gian máy chủ |
| `category` | varchar(20) | Có | Permission, Business, Access, Import, Export, System |
| `action` | varchar(100) | Có | Mã hành động ổn định |
| `object_type` | varchar(50) | Có | Loại đối tượng chính |
| `object_id` | text | Không | ID đối tượng dạng chuỗi; cho phép ghi truy cập bị từ chối |
| `outcome` | varchar(20) | Có | Success, Denied, Failure |
| `before_masked` | jsonb | Không | Ảnh trước đã che |
| `after_masked` | jsonb | Không | Ảnh sau đã che |
| `reason_masked` | text | Không | Lý do/kết quả an toàn |
| `correlation_id` | uuid | Có | Gom các audit record của cùng request/transaction |

### Khóa và ràng buộc

- PK: `id`.
- Append-only: cấm UPDATE, DELETE và TRUNCATE đối với tài khoản ứng dụng.
- Human/Service phải có `actor_id`; System/Unauthenticated phải không có actor user.
- Không ghi token, API key, password, cookie, attack/evidence thô.

### Quan hệ

- N–1 tùy chọn đến `app_user` qua actor.
- `object_type/object_id` là tham chiếu audit đa hình, không phải FK nghiệp vụ.
- Không lặp `asset_id`, `finding_id` hoặc nhiều FK nullable; đối tượng liên quan được truy qua object chính và correlation ID.

### Vai trò đối với lịch sử

Audit giữ lịch sử trước/sau của các thuộc tính hiện hành như final severity, finding status, assignee, due date và tiến độ. Audit không thay thế các bảng nghiệp vụ khi thực hiện FK hoặc điều phối quét.

## 4. Ma trận quan hệ giữa các bảng

| Bảng cha | Bảng con | Bội số | FK chính | Ý nghĩa |
|---|---|---|---|---|
| `app_user` | `user_role` | 1–N | `user_id` | Vai trò được cấp cho người dùng |
| `asset` | `user_role` | 1–N tùy chọn | `asset_id` | Giới hạn role theo tài sản |
| `asset` | `asset_scope_revision` | 1–N | `asset_id` | Lịch sử phạm vi |
| `asset_scope_revision` | `asset_scope_rule` | 1–N | `scope_id` | Luật include/exclude |
| `asset_scope_revision` | `asset_scan_window` | 1–N | `scope_id` | Khung giờ được phép |
| `asset_scope_revision` | `scan_plan` | 1–N | `scope_id` | Kế hoạch dùng đúng scope version |
| `zap_scan_profile` | `scan_plan` | 1–N | `zap_profile_id` | Kế hoạch dùng profile version |
| `remediation_ticket` | `scan_plan` | 1–N tùy chọn | `remediation_ticket_id` | Các lần yêu cầu xác minh |
| `scan_plan` | `scan_run` | 1–N | `plan_id` | Hồ sơ các lần thực hiện kế hoạch |
| `asset` | `finding` | 1–N | `asset_id` | Phát hiện thuộc tài sản |
| `finding` | `finding_occurrence` | 1–N | `finding_id` | Các lần phát hiện xuất hiện |
| `scan_run` | `finding_occurrence` | 1–N | `scan_run_id` | Các phát hiện trong lần quét |
| `finding` | `remediation_ticket` | 1–0..1 | `finding_id` UNIQUE | Workflow khắc phục tối giản |
| `app_user` | nhiều bảng | 1–N | các cột `*_by`, `*_id` | Người tạo, duyệt, đánh giá, phụ trách |
| `app_user` | `audit_log` | 1–N tùy chọn | `actor_id` | Tác nhân của sự kiện audit |

## 5. Ràng buộc liên bảng quan trọng

Các quy tắc sau không thể bảo đảm chỉ bằng một FK đơn và cần transaction/trigger:

1. Người được gán `asset.security_officer_id` có SECURITY đúng asset.
2. `scan_plan.operator_id` có SCAN_OPERATOR đúng asset của scope.
3. Verification plan và remediation ticket thuộc cùng asset.
4. `finding_occurrence.finding_id` và `scan_run_id` cùng trỏ về một asset.
5. `remediation_ticket.assignee_id` có REMEDIATOR đúng asset của finding.
6. Verification outcome Closed chỉ khi run hoàn thành, report Processed, coverage được xác nhận và fingerprint không còn.
7. Tài khoản Service không thực hiện phê duyệt/đánh giá nghiệp vụ.
8. Mọi thay đổi status/severity/assignment/due date phải ghi audit cùng transaction.

## 6. Truy vấn thay cho dữ liệu lưu lặp

| Thông tin | Cách xác định |
|---|---|
| Scope hiện hành | Revision Approved trong khoảng hiệu lực |
| Tài sản của scan run | Run → Plan → Scope → Asset |
| Target URL | Include rules trừ exclude rules |
| Kỳ tiếp theo | Recurrence rule + MAX `scheduled_for` |
| First/last seen của finding | MIN/MAX thời gian run qua occurrence |
| Số finding của run | COUNT occurrence theo run |
| Lịch sử severity/status | Audit theo object/correlation |
| Phiếu gần/quá hạn | `due_at` so với thời gian hiện tại và `finding.status` |
| Kết quả xác minh mới nhất | Ticket → Verification Plan → Run mới nhất |
| Tỷ lệ run thành công | GROUP BY `scan_run.scan_status` |

## 7. Tài liệu liên quan

- [Thiết kế BCNF tối giản](08_thiet_ke_3nf_tinh_gon.md)
- [ERD 13 bảng](03_so_do_erd.md)
- [Nguồn yêu cầu tích hợp ZAP](../doi_tuong_sd_quy_trinh_hd_tich_hop_zap_tu_dong.md)

