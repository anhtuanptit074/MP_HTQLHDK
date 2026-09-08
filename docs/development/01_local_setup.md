# Hướng dẫn chạy local và test

## 1. Điều kiện tiên quyết

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

