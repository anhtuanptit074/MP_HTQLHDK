# Thiết kế cấu hình, secrets và quản lý tệp

## 1. Phân loại cấu hình

| Loại | Ví dụ | Nơi lưu | Quy tắc |
|---|---|---|---|
| Không bí mật, bất biến theo release | API version, feature compatibility, parser version | Source/release config | Review, version control, không chứa endpoint credential |
| Không bí mật, theo môi trường | Public origin, issuer URL, bucket name, timeout, limit | Deployment config | Typed schema, validate startup, khác môi trường không rebuild image |
| Chính sách nghiệp vụ đã duyệt | SLA, retention ref, masking ref, risk authority | Bảng/config được duyệt | Có version, người phê duyệt, hiệu lực; không hard-code |
| Secret | DB credential, OIDC client secret nếu cần, object credential | Secrets Manager | Không repo/image/log; cấp qua workload identity; rotation |
| Khóa mã hóa/HMAC | KMS key, fingerprint HMAC key | KMS/HSM | App chỉ giữ key reference/handle; cấm export plaintext nếu nền tảng hỗ trợ |

Ứng dụng dừng startup nếu thiếu/không hợp lệ cấu hình bắt buộc. Error chỉ nêu tên khóa cấu hình, không in giá trị. Endpoint quản trị/health không trả toàn bộ cấu hình.

## 2. Danh mục cấu hình bắt buộc

| Nhóm | Tên logic | Secret | Trạng thái giá trị |
|---|---|---:|---|
| Identity | `Auth.Issuer`, `Auth.ClientId`, `Auth.Audience`, callback/logout origins | Một phần | Chờ IdP của đơn vị |
| Session | idle/absolute timeout, Data Protection key ref | Một phần | Chờ chính sách; key ref bắt buộc |
| Database | primary/audit/worker connection refs, command timeout, pool bounds | Có | Tên/giá trị theo môi trường |
| Object | endpoint/region, bucket/prefix, KMS key refs, staging TTL | Một phần | Chờ provider/retention |
| Upload | max bytes, max concurrent/user/asset, allowed JSON/XML, read timeout | Không | Chờ policy và load test |
| Parser | max depth/nodes/string, batch size, worker concurrency, retry cap | Không | Chốt qua test corpus/load test |
| Download | mode stream/presign, TTL, bandwidth/concurrency | Không | Stream là mặc định; TTL chờ policy nếu bật presign |
| Job | poll interval, lease, retry/backoff, notification horizon | Không | Giá trị kỹ thuật test ở giai đoạn 10–12; horizon nghiệp vụ chờ policy |
| Observability | collector endpoint, sampling, safe field allowlist | Có thể | Theo nền tảng; body capture luôn tắt |
| Backup | target/key ref/schedule/retention | Có | Chờ RPO/RTO/retention |

Mọi giá trị “chờ” có owner và tiêu chí ở [05](05_moi_truong_dung_luong_sao_luu.md); không có default production ngầm. Dev/test có thể dùng giá trị giả được đánh dấu environment và bị kiểm tra cấm khi `Environment=Production`.

## 3. Quản lý vòng đời secret và khóa

1. Tạo secret/key bằng identity quản trị tách biệt; ghi owner, purpose, environment, created/rotate/expire metadata.
2. Workload lấy secret qua workload identity/managed identity; không dùng credential cloud dài hạn trong biến CI hoặc image.
3. API, worker, migrator, audit, backup có secret/identity riêng; không dùng chung production/staging.
4. Rotation theo cơ chế dual credential/key version: cấp mới → deploy đọc mới → xác minh → thu hồi cũ. Không ghi đè `raw_key_ref`/`encryption_key_ref` lịch sử nếu chưa re-encrypt có kiểm soát.
5. Key reference có version. Fingerprint HMAC giữ `algorithm_version/key_version`; đổi khóa cần migration/rehash có đối soát để không nhân đôi Finding.
6. Truy cập/rotation/disable key và secret được audit bởi nền tảng quản lý; cảnh báo hành vi bất thường.
7. Break-glass có danh tính cá nhân, MFA, thời hạn ngắn và review sau sử dụng; không có shared admin secret thường trực.

## 4. Luồng upload và finalize báo cáo gốc

### 4.1. Trước khi nhận byte

- Xác thực user, CSRF/session và quyền `import` đúng asset/scan; asset Active, scan Completed, đúng scope.
- Kiểm metadata sơ bộ nhưng không tin `Content-Type`, extension, filename hoặc checksum do client gửi.
- Áp quota/rate/concurrency; sinh `upload_id`, correlation ID và object key ngẫu nhiên dưới `staging/{environment}/{upload_id}`.

### 4.2. Streaming vào staging

1. Stream từng chunk; tính SHA-256 server-side, đếm byte, kiểm timeout/rate. Không buffer cả file.
2. Dừng ngay khi vượt giới hạn. Tên gốc chỉ giữ bản sanitized làm nhãn, không dùng làm path/header trực tiếp.
3. Sniff JSON/XML từ byte đầu và parser an toàn sơ bộ. Archive, HTML, binary hoặc empty được phân loại `Other`/không hợp lệ; không parse hoặc hiển thị inline.
4. Ghi staging bằng encryption KMS và policy không public; object chưa thể tải/parse bởi người dùng.

Upload hoàn tất trong hard limit luôn đi tiếp để tạo lô có truy vết: tệp JSON/XML sai cấu trúc hoặc `Other` được finalize riêng tư rồi kết thúc `Rejected`, không tạo Finding. Upload bị ngắt hoặc vượt hard limit trước khi nhận đủ byte không tạo `import_batch` vì chưa có checksum/toàn bộ nguồn; API xóa staging theo cleanup policy và ghi audit Failure/Denied với mã lỗi an toàn.

### 4.3. Chống trùng và ghi metadata

1. Sau khi có checksum, kiểm `(asset_id, scan_id, file_sha256)` trong transaction và chỉ sau authorization, kể cả khi định dạng sẽ bị Rejected.
2. Nếu trùng, trả batch hiện có theo idempotent semantics; staging object được cleanup có audit kỹ thuật, không tiết lộ batch của asset/scan khác.
3. Nếu mới, tạo `import_batch` Received với permanent generated key, size/type/name sanitized, `raw_key_ref` và audit Success. Unique constraint phân xử race.
4. Finalize bằng server-side copy/move từ staging sang `raw/{asset_partition}/{batch_id}/{uuid}` theo quyền service, verify size/checksum, deny overwrite.
5. Chỉ chuyển Validating/Processing sau khi object permanent đọc lại thành công. Nếu finalize lỗi, giữ batch Failed/Received theo hợp đồng lỗi và reconciliation; không tạo link.

Do `raw_object_key` hiện là NOT NULL và bất biến, permanent key phải được sinh trước khi insert. Không cập nhật key sau đó. Với thứ tự trên, insert metadata thất bại chỉ để lại staging object. Nếu finalize đã thành công nhưng transaction cập nhật trạng thái sau đó thất bại, reconciliation nhận batch còn ở Received/Failed cùng permanent object qua `upload_id`, kiểm checksum rồi tiếp tục hoặc cô lập theo runbook; không thay raw key.

### 4.4. Parse an toàn

- Worker đọc stream bằng service identity, không có Internet/website egress.
- JSON dùng streaming reader với giới hạn depth/token/string/record; XML tắt DTD, external entity, external schema, XInclude và network resolver.
- Chỉ chấp nhận OWASP ZAP Traditional JSON/XML đã nhận diện bằng cấu trúc/template; không thực thi script/macro, không render HTML.
- Chuẩn hóa URL bằng parser chuẩn, kiểm Include/Exclude trên URL canonical; Exclude ưu tiên.
- Mỗi record transaction ngắn; lỗi record ghi `import_error.message_masked`. Payload/Attack/Evidence không vào exception/log.
- Trước Completed/Partial, đối soát tổng số record. Lỗi kỹ thuật đưa Failed và giữ truy vết; không báo thành công giả.

## 5. Bằng chứng và report export

### 5.1. Evidence

- Cùng pipeline streaming/type/size/malware control theo loại được đơn vị cho phép; quyền `can_view_sensitive_evidence` tách riêng.
- Object key `evidence/{asset_partition}/{finding_id}/{uuid}`; metadata DB gắn Finding và actor.
- Không nhúng trực tiếp trong thông báo. Preview chỉ tạo representation an toàn, không mở file active content trong origin ứng dụng.

### 5.2. Export

1. Chụp người nhận, masking policy ref và danh sách asset; kiểm quyền tất cả asset.
2. Worker tạo tệp mới đã che, đóng nhãn Sensitive + trace code + exported_at; không tái dùng một file cho người nhận khác.
3. Stream tới `exports/{recipient_partition}/{export_id}/{uuid}`, tính checksum và mã hóa KMS.
4. Ghi metadata, asset list, hash token ngẫu nhiên và audit nhất quán; token plaintext chỉ trả một lần.
5. Mỗi download kiểm recipient, quyền hiện hành cho mọi asset, `expires_at`, `revoked_at`, checksum/object metadata; audit cả Success/Denied/Failure.
6. Thu hồi chỉ cập nhật trạng thái thu hồi và audit; không sửa recipient/nội dung/hạn để “tái phát hành”.

## 6. Chính sách object storage tối thiểu

| Kiểm soát | `staging/` | `raw/` | `evidence/` | `exports/` |
|---|---|---|---|---|
| Public/anonymous | Cấm | Cấm | Cấm | Cấm |
| API read | Không | Theo download service | Theo quyền riêng | Theo download service |
| Worker read/write | Upload/finalize/cleanup | Read cho parser, create-only finalize | Theo job | Create/read theo export |
| Overwrite | Cấm | Cấm | Cấm | Cấm |
| Delete runtime thường | Cleanup staging có điều kiện | Cấm | Cấm | Cấm; retention job được duyệt riêng |
| Encryption | KMS | KMS + key ref DB | KMS + key ref DB | KMS + key ref DB |
| Versioning/Object Lock | Theo provider | Bật; lock theo retention đã duyệt | Bật nếu policy yêu cầu | Bật nếu policy yêu cầu |

Retention deletion là workload quản trị riêng có dry-run, manifest, dual approval khi chính sách yêu cầu, audit và reconciliation; không cấp Delete cho API/worker bình thường.

## 7. Download và HTTP an toàn

- Response nhạy cảm: `Cache-Control: no-store`, `X-Content-Type-Options: nosniff`, `Content-Disposition: attachment` với filename encode an toàn.
- Không range-download nếu cơ chế audit/checksum/provider chưa hỗ trợ đúng; nếu bật phải giới hạn và vẫn audit mỗi request.
- Không trả object key thật, bucket, key ref, stack trace hoặc token hash trong API.
- Error response có code/correlation ID, không lặp lại URL/Attack/Evidence/secret.
- Browser không được mở raw HTML/SVG/XML trong origin chính; tải attachment từ download endpoint riêng.

## 8. Đối soát và xử lý sự cố

Job đối soát định kỳ (chu kỳ chờ policy kỹ thuật) kiểm:

- Batch có metadata nhưng thiếu/sai size/checksum object.
- Object staging quá TTL chưa liên kết.
- Object permanent không có metadata; chỉ đánh dấu/quarantine, không xóa ngay.
- Export hết hạn/thu hồi nhưng vẫn có đường truy cập được.
- KMS key ref không resolve hoặc credential sắp hết hạn.

Mọi chênh lệch tạo metric/alert và bản ghi System audit an toàn. Không tự “sửa” bằng thay object raw hoặc xóa metadata lịch sử.
