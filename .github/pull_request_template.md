## Mục tiêu

Mô tả ngắn thay đổi và điều không nằm trong phạm vi.

## Truy vết bắt buộc

- UC/BR/F:
- API operation/mã lỗi:
- UI ID (nếu có):
- ADR/threat/control (nếu có):
- Ticket/decision:

## Thay đổi dữ liệu

- [ ] Không thay đổi schema/migration.
- [ ] Có migration mới, checksum, verify và kế hoạch rollback/forward-fix.

## Bằng chứng kiểm thử

- Command/CI run:
- Kết quả:
- Negative/state/concurrency/failure cases:
- Giới hạn chưa kiểm chứng:

## Security và vận hành

- [ ] Không có secret, dữ liệu lỗ hổng thật, raw ZAP chưa làm sạch hoặc log nhạy cảm.
- [ ] Quyền + asset/task scope + tách nhiệm vụ được kiểm tra ở backend.
- [ ] Masking/audit/idempotency/optimistic locking được cập nhật khi áp dụng.
- [ ] Dependency mới đã kiểm license, nguồn, phiên bản và vulnerability.
- [ ] Runbook/metric/alert/configuration được cập nhật khi hành vi vận hành đổi.

## Review chuyên môn

- [ ] Nghiệp vụ/Product Owner
- [ ] Backend/Frontend
- [ ] DBA
- [ ] ATTT
- [ ] Accessibility

