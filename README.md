# HTQLHDK — nền tảng phát triển

Repository cho Hệ thống quản lý quét lỗ hổng website định kỳ. Giai đoạn 10 thiết lập nền tảng lặp lại được cho React 19.2, ASP.NET Core/.NET 10 LTS và PostgreSQL 17; chưa triển khai nghiệp vụ giai đoạn 11.

## Bắt đầu nhanh

1. Cài Git, Node.js 24+, .NET SDK 10.0.400 và Docker Desktop/Engine.
2. Sao chép `.env.local.example` thành `.env.local`, thay hai giá trị `REPLACE_*` bằng hai mật khẩu local khác nhau.
3. Chạy `docker compose --env-file .env.local up -d postgres`.
4. Chạy `./scripts/migrate.ps1` để tạo schema baseline.
5. Chạy `./scripts/test.ps1` để kiểm tra backend, frontend, OpenAPI, database artifact và hồ sơ thiết kế.

Chi tiết và cách xử lý lỗi nằm trong [hướng dẫn local/test](docs/development/01_local_setup.md).

## Cấu trúc

| Thư mục | Mục đích |
|---|---|
| `src/backend` | API và worker cùng release, module hóa |
| `src/frontend` | React/TypeScript SPA |
| `database` | Thiết kế và SQL nguồn đã kiểm chứng |
| `ops/migrations` | Migration bất biến, checksum và xác minh |
| `tests` | Fixture giả/đã làm sạch và contract checks |
| `scripts` | Bootstrap, migration và kiểm tra thống nhất |
| `docs/development` | Quy chuẩn phát triển/vận hành giai đoạn 10 |
| `.github` | Pipeline, dependency update và mẫu pull request |

## Nguyên tắc an toàn

- Không commit `.env.local`, secret, token, chứng thư, raw ZAP hoặc dữ liệu lỗ hổng thật.
- API/worker không điều khiển ZAP; kết quả quét chỉ được nhập qua hợp đồng đã duyệt.
- Log chỉ chứa mã định danh nội bộ, correlation ID, action, outcome và số liệu tổng hợp; không log URL đầy đủ, Attack, Evidence, parameter, object key hay token.
- Migration đã áp dụng không được sửa. Thay đổi mới phải là migration mới và có checksum.
- Pull request phải liên kết UC/BR/API/UI và kèm bằng chứng test theo [Definition of Done](docs/development/05_definition_of_done.md).

## Tài liệu chính

- [Ma trận môi trường](docs/development/07_environment_matrix.md)
- [Coding convention và log an toàn](docs/development/02_coding_standards.md)
- [Cấu hình và secrets](docs/development/03_configuration_and_secrets.md)
- [Migration và fixture](docs/development/04_migrations.md)
- [Branch/review](docs/development/06_branch_and_review.md)
- [Definition of Done](docs/development/05_definition_of_done.md)

