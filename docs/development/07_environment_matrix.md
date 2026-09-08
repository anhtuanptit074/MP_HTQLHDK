# Ma trận môi trường

| Thuộc tính | Local | Test/CI | Staging/UAT | Production |
|---|---|---|---|---|
| Dữ liệu | Giả/đã làm sạch | Fixture tái tạo | Giả/khử định danh | Thật |
| PostgreSQL | Container 17.11 khóa digest | Service 17.11 sạch mỗi run | Managed/private, gần production | Managed/private, PITR |
| Secret | `.env.local`, không commit | Secret CI ngắn hạn/test-only | Secret manager + workload identity | Secret manager + workload identity riêng |
| Object storage | Adapter giả hoặc private local ở giai đoạn 11 | Fake/ephemeral | Private + KMS staging | Private + KMS production |
| External egress | Tắt mặc định | Tắt | Allowlist được duyệt | Allowlist được duyệt |
| Log | Safe allowlist | Safe allowlist, artifact ngắn hạn | Tách quyền/retention theo policy | Tách quyền/retention theo policy |
| Migration | Developer migrator | CI migrator | Pipeline có phê duyệt | Pipeline có backup/change approval |
| Scanner | Không điều khiển ZAP | Không điều khiển ZAP | Import file đã kiểm soát | Import file đã kiểm soát |

Các giá trị cụ thể cho upload/rate limit, SLA, retention, RPO/RTO, alert threshold và risk authority để `TBD-POLICY`; không sao chép giá trị fixture hoặc local lên staging/production.

## Promotion gate

Artifact cùng commit/checksum đi từ Test → Staging → Production; không rebuild khác nội dung giữa môi trường. Mỗi promotion kiểm cấu hình bắt buộc, secret source, migration compatibility, health/smoke test và rollback readiness. Production không nhận fixture, dev dependency hoặc debug endpoint.

