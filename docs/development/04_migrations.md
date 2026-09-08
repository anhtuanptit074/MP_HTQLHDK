# Migration baseline và fixture

## 1. Baseline

`ops/migrations/V001__baseline.psql` áp dụng ba SQL nguồn đã kiểm chứng theo thứ tự schema → guard/view → reference data. `V001__manifest.json` khóa SHA-256; script kiểm tra giai đoạn 10 fail khi nội dung lệch.

Metadata migration nằm trong schema `htqlhdk_migration`, tách khỏi 27 bảng nghiệp vụ. Advisory lock theo phiên ngăn hai runner áp dụng đồng thời. Trạng thái `Running` còn lại sau lỗi là dấu hiệu cần khôi phục, không tự đánh dấu thành công.

## 2. Quy tắc migration mới

- Không sửa migration đã có ở bất kỳ môi trường dùng chung nào.
- Mỗi thay đổi có mã tăng dần (`V002__...`), mục tiêu, precondition, forward SQL, verify SQL, tác động lock/dung lượng và kế hoạch rollback/forward-fix.
- Migration phải chạy bằng identity riêng, một runner, backup/checkpoint phù hợp và ghi version/checksum.
- Thay đổi phá vỡ dùng expand → migrate/backfill → switch → contract qua nhiều release.
- Không chạy DDL dài cùng lúc với worker import/expiry nếu chưa đánh giá lock.

## 3. Rollback

Không có down migration tự động cho production. Rollback lựa chọn theo loại thay đổi: rollback ứng dụng khi schema tương thích; forward-fix; hoặc restore theo runbook đã kiểm thử. Trước migration phá vỡ phải ghi điểm không thể quay lại và tiêu chí quyết định.

Chỉ local/test được xóa database/volume để tái tạo từ sạch, sau khi xác minh rõ project và database đích. Không dùng lệnh reset cho staging/production.

## 4. Fixture

- `database/sql/03_reference_data.sql` chỉ chứa ma trận role/permission cố định; không có tài khoản hay SLA.
- `database/validation/fixture.sql` và `tests/fixtures/` chỉ dùng cho test, miền `.invalid`, dữ liệu giả và cảnh báo rõ mọi SLA giả không phải policy.
- Loader fixture chỉ chấp nhận database có tên kết thúc bằng `_local`, `_test` hoặc `_ci`; trước mỗi lần nạp, trigger chống xóa được tạm vô hiệu trong transaction của tài khoản migration, dữ liệu nghiệp vụ cũ được xóa và identity được đặt lại để pipeline có thể chạy lặp an toàn. Trigger được bật lại trước khi nạp fixture; metadata migration ở schema `htqlhdk_migration` không bị thay đổi.

## 5. Bằng chứng bắt buộc

Chạy V001 trên database PostgreSQL 17.11 sạch; chạy verify 27 bảng/timezone/version; nạp fixture; chạy lại migration để chứng minh no-op; và lưu log CI không chứa secret/payload.
