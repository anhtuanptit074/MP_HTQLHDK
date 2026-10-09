# Hướng dẫn chạy local và test

## 1. Điều kiện tiên quyết

Trên máy đã chuẩn bị môi trường ngày 2026-10-05, Node/npm, Docker và PowerShell 7 đã có sẵn. SDK .NET 10.0.400 nằm ở `.codex_tmp/dotnet`; các script tự nhận diện SDK này. Không cần thêm SDK vào PATH toàn hệ thống.

- Git 2.50+.
- Node.js 24 LTS hoặc mới hơn trong cùng major được CI chấp nhận; npm 11+.
- .NET SDK đúng `global.json` (10.0.400, không prerelease).
- Docker Desktop/Engine có Compose v2.
- PowerShell 7 khuyến nghị; Windows PowerShell vẫn dùng được cho script hiện tại.

## 2. Dựng lần đầu

1. Sao chép `.env.local.example` thành `.env.local`.
2. Thay hai placeholder mật khẩu bằng hai chuỗi local ngẫu nhiên khác nhau. Không gửi file này qua chat/email và không commit.
3. Chạy `./scripts/bootstrap-local.ps1`.
4. Chạy `./scripts/migrate.ps1 -LoadTestFixture` để khởi động PostgreSQL, áp dụng V001, xác minh 27 bảng và nạp fixture giả.
5. Chạy `./scripts/test.ps1`.

Frontend chạy bằng `npm run dev` trong `src/frontend`. API chạy bằng `dotnet run --project src/backend/Htqlhdk.Api`; worker mặc định không bật job nghiệp vụ ở giai đoạn 10.

### Khởi động nhanh trên máy đã chuẩn bị

Mở Docker Desktop, rồi chạy tại thư mục gốc dự án:

```powershell
.\start-local.cmd
```

Lệnh này tự tìm PowerShell 7, nhận diện SDK local, áp dụng/xác minh migration, chuẩn bị tài khoản database ứng dụng và chạy API/frontend ở nền. Dữ liệu hiện có được giữ; không nạp lại fixture. Các địa chỉ:

- Frontend: `http://127.0.0.1:5173/`.
- API: `http://127.0.0.1:5000/`.
- Health: `http://127.0.0.1:5000/health/live`.
- PostgreSQL: `127.0.0.1:54329`, database `htqlhdk_local` (hoặc giá trị trong `.env.local`).

Dừng API/frontend:

```powershell
.\start-local.cmd -Stop
```

PostgreSQL vẫn chạy. Log và PID local nằm trong `.codex_tmp/local-run/`, không đưa vào Git. `start-local.cmd` sử dụng PowerShell 7 trên PATH hoặc bản đi kèm runtime Codex trên máy này.

Trong PowerShell 7, chạy kiểm tra mà giữ dữ liệu PostgreSQL hiện tại:

```powershell
.\scripts\test.ps1
.\scripts\migrate.ps1
```

`test.ps1 -IncludeDatabase` nạp lại fixture và thay thế dữ liệu nghiệp vụ local. Chỉ dùng khi chủ động muốn đặt lại bộ dữ liệu thử.

Kết quả chuẩn bị và giới hạn chạy thử nằm trong [báo cáo môi trường local](09_local_readiness.md).

## 3. Lặp lại từ trạng thái sạch

Local/test được phép xóa volume bằng thao tác chủ động `docker compose --env-file .env.local down --volumes`, sau khi người thực hiện xác minh đúng project `htqlhdk-local`. Sau đó chạy lại migration và fixture. Không dùng thao tác này cho staging/production.

Migration V001 có advisory lock, checksum và trạng thái `Running/Applied`. Lần chạy lại sau khi `Applied` là no-op. Nếu lần trước dừng giữa chừng, script từ chối chạy tiếp để tránh che giấu database nửa vời; local/test phải tạo database sạch, còn môi trường cao hơn phải theo runbook khôi phục.

## 4. Lỗi thường gặp

- Thiếu `.env.local`: tạo từ file mẫu và thay placeholder.
- Port 54329 bận: đổi `HTQLHDK_DB_PORT` trong `.env.local`.
- Sai SDK .NET: cài 10.0.400; `global.json` cố ý chặn prerelease.
- Checksum migration lệch: không sửa V001. Hoàn nguyên thay đổi ngoài ý muốn hoặc tạo migration V002.
- Fixture bị từ chối: database có tên chứa `prod/production`; chọn database local/test khác.

## 5. Kiểm tra trước pull request

Chạy `./scripts/test.ps1 -IncludeDatabase`. Kết quả phải bao gồm build backend, lint/format/test/build frontend, kiểm OpenAPI/UI/database artifact, migration và fixture trên database sạch.
