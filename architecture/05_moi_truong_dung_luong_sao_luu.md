# Yêu cầu môi trường, dung lượng và sao lưu

## 1. Ma trận môi trường

| Môi trường | Mục đích | Dữ liệu | Identity/secrets | Yêu cầu tách biệt |
|---|---|---|---|---|
| Local Dev | Phát triển/unit test | Fixture giả, ZAP sample đã khử bí mật | Dev-only; không dùng production issuer/key | Database/object namespace riêng từng dev |
| Integration/Test | Integration, parser, concurrency, security automation | Synthetic + corpus đã làm sạch | Test IdP/KMS/bucket riêng | Không route tới production; reset có kiểm soát |
| Staging/UAT | Release candidate, UAT, restore/deploy rehearsal | Dữ liệu giả/đã phê duyệt | Credential riêng, topology gần production | Không dùng backup production nếu chưa có quy trình masking/phê duyệt |
| Production | Dữ liệu thật | Sensitive | Workload identity và key production | Network/account/project/subscription tách; không debug trực tiếp |
| DR/Restore isolated | Restore drill/khôi phục | Bản backup mã hóa | Break-glass có thời hạn | Không public, egress hạn chế, xóa theo biên bản sau drill |

Production và staging dùng cùng image digest/release artifact; khác bằng config/secret. Không build riêng mã cho production. Migration artifact được ký/checksum và chạy ngoài startup ứng dụng.

## 2. Yêu cầu nền tảng tối thiểu

| Thành phần | Yêu cầu kiến trúc | Tiêu chí kiểm tra |
|---|---|---|
| Edge | TLS 1.2 tối thiểu, 1.3 ưu tiên; WAF/rate/body limit; trusted proxy config | TLS scan, spoofed forwarding header test |
| Web/API | Container non-root, read-only root FS nếu khả thi, ephemeral temp quota, ≥2 replicas nếu availability policy yêu cầu | Container hardening + rolling test |
| Worker | Deployment riêng, no public ingress, bounded CPU/RAM/concurrency, import egress deny | Network policy + stress/kill test |
| PostgreSQL 17 | Private endpoint, encrypted storage/transport, PITR-capable backup, UTC, connection/statement timeout | Connectivity/privilege/backup test |
| Object storage | Private endpoint, block public, KMS encryption, versioning, access log, generated key | Policy scan + tamper/download test |
| KMS/Secrets | Workload identity, audit, rotation/version, least privilege | Rotation drill |
| Telemetry | Secure collector, allowlist field, access/retention policy | Log-leak and access test |
| Time/DNS | NTP/trusted time, internal DNS/allowlist | Time skew/expiry test |

Không chốt Kubernetes, cloud hay on-prem ở giai đoạn này. Nền tảng được chọn phải chứng minh các khả năng trên; nếu thiếu, tạo ADR mô tả control thay thế và rủi ro được chấp nhận.

## 3. Worksheet dung lượng — không tự đặt số liệu nghiệp vụ

### 3.1. Đầu vào phải thu thập

| Biến | Ý nghĩa | Owner | Hiện trạng |
|---|---|---|---|
| `A` | Số asset Active trung bình/đỉnh | Nghiệp vụ | Chờ cung cấp |
| `S` | Số scan/asset/tháng | SCAN_OPS | Chờ cung cấp |
| `Ravg`, `Rp95`, `Rmax` | Dung lượng raw report trung bình/P95/tối đa | SCAN_OPS | Chờ mẫu đại diện |
| `Oavg`, `Omax` | Observation mỗi scan trung bình/đỉnh | ATTT/QA | Chờ corpus |
| `Eavg` | Evidence trung bình theo Finding | Nghiệp vụ | Chờ cung cấp |
| `Xavg` | Export/tháng và kích thước | Nghiệp vụ | Chờ cung cấp |
| `Upeak` | User/request/upload đồng thời đỉnh | Chủ nhiệm/vận hành | Chờ cung cấp |
| `Mraw`, `Mev`, `Mexp`, `Maudit` | Tháng/năm retention từng loại | Chính sách | Chờ phê duyệt |
| `G` | Hệ số tăng trưởng năm | Chủ nhiệm | Chờ cung cấp |
| `H` | Headroom vận hành | Vận hành | Chờ phê duyệt/test |

### 3.2. Công thức sizing

- Raw object/tháng: `Draw = A × S × Ravg`.
- Observation/tháng: `Nobs = A × S × Oavg`.
- Object retained: `Dobject = Draw × Mraw + Devidence × Mev + Dexport × Mexp`, sau đó nhân versioning/replication overhead theo provider.
- Database retained: đo bytes/row thực tế trên corpus cho observation/finding/audit/ticket; `Ddb = Σ(rows_i × bytes_i × retention_i) + index/WAL headroom`.
- Network upload/tháng gần `Draw`; download thêm raw/evidence/export theo audit use case.
- Worker throughput yêu cầu: `max(files/hour_peak × parse_time_p95)` với backlog recovery window do vận hành chốt.
- Backup target phải chứa ít nhất full/base backup + WAL/change log + object versions trong retention backup đã duyệt.

Không lấy kích thước file tối đa làm kích thước trung bình. Sau khi có corpus, chạy benchmark với P50/P95/P99 và tăng dữ liệu đến ít nhất tải đỉnh dự kiến × headroom được duyệt.

## 4. Capacity gate

Trước staging/UAT phải có worksheet đã điền và kết quả:

1. Upload/parse report `Rmax` không OOM, không chặn API và trả lỗi an toàn khi vượt limit.
2. Hai worker không claim trùng; backlog sau gián đoạn được xử lý trong recovery window.
3. Worklist/query/export P95 đạt SLO do đơn vị duyệt, pagination không bỏ lọt/nhân đôi.
4. Connection pool không vượt DB max; timeout/circuit behavior không gây retry storm.
5. Storage alert ở ngưỡng còn đủ thời gian phản ứng; ngưỡng do vận hành duyệt.
6. Backup/restore duration đạt RPO/RTO đã duyệt.

## 5. Mục tiêu sao lưu và DR cần phê duyệt

| Mục | Thiết kế cung cấp | Giá trị còn chờ |
|---|---|---|
| Database | Base/full backup + WAL/PITR, checksum, encrypted target | RPO, RTO, lịch, retention, region/site |
| Object | Versioning + inventory/manifest + replication/backup theo provider | Retention, replication site, recovery point |
| Config | Versioned non-secret config và manifest release | Retention/repository chuẩn |
| Secret/KMS | Metadata/key policy backup theo nhà cung cấp; recovery procedure | Key escrow/DR authority; không export plaintext mặc định |
| Audit | Nằm trong DB backup; tùy policy có export append-only riêng | Audit retention và legal hold |

RPO/RTO không được coi là đạt trước khi restore drill đo được. Backup “thành công” chỉ là tín hiệu tạo bản sao, không phải bằng chứng khôi phục.

## 6. Runbook backup/restore mức kiến trúc

### 6.1. Backup

1. Backup identity riêng lấy snapshot/base backup và WAL/change stream theo lịch.
2. Object inventory ghi key, version, size, checksum/KMS ref; backup/replicate không làm public object.
3. Ghi release version, migration version, thời điểm nhất quán và manifest checksum.
4. Mã hóa target bằng key tách môi trường; bật immutability theo retention đã duyệt.
5. Giám sát freshness, completion, restore-readability, dung lượng và quyền truy cập; alert không chứa tên tệp nhạy cảm.

### 6.2. Restore drill

1. Phê duyệt drill, tạo network/account cô lập và identity có thời hạn.
2. Restore PostgreSQL tới target time; xác minh migration version, constraints, guards, 27 bảng nghiệp vụ và audit immutability.
3. Restore/mount object versions đúng mốc; không thay raw object.
4. Chạy đối soát metadata ↔ object: tồn tại, size, checksum, KMS ref, sample decrypt/read.
5. Chạy smoke test quyền, upload mẫu, workflow, download/revoke và audit bằng dữ liệu an toàn.
6. Đo recovery point/time, ghi chênh lệch và quyết định đạt/không đạt.
7. Thu hồi identity, xóa môi trường drill theo quy trình được phê duyệt và lưu biên bản.

### 6.3. Thứ tự khôi phục production

KMS/secrets và network policy → PostgreSQL → object storage/version mapping → migration compatibility → API/worker ở chế độ read-only/paused jobs → reconciliation → smoke test → mở traffic/jobs. Không cho worker xử lý expiry/import khi DB và object chưa đối soát.

## 7. Deployment và rollback

- Trước deploy: backup/PITR point được xác minh, migration dry-run trên staging, image digest/SBOM/scan đạt.
- Migration trước app chỉ khi backward compatible; schema breaking dùng expand/contract qua nhiều release.
- API/worker kiểm schema version khi startup và từ chối chạy nếu ngoài compatibility range.
- Rollback app chỉ về image tương thích schema. Nếu migration đã ghi dữ liệu không đảo ngược, dùng forward-fix hoặc restore theo người có thẩm quyền, không chạy down script phá dữ liệu.
- Worker drain/lease hết trước rollback; retry vẫn idempotent.

## 8. Monitoring tối thiểu

Metric kỹ thuật: request rate/error/latency, auth denied, upload bytes/reject, import queue age/status/time, job lease/retry, DB pool/lock/slow query, object error/reconciliation mismatch, KMS/secret failure, audit write failure, storage/backup freshness/restore result. Nhãn metric không chứa full URL mục tiêu, parameter, Attack, Evidence, filename hoặc user email nếu không cần.

Alert có runbook và owner; mức ngưỡng cụ thể chốt từ SLO/capacity test, không tự đặt trong thiết kế.

