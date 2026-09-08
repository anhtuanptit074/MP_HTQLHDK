# Definition of Done

Một pull request chỉ “Done” khi tất cả mục áp dụng đã đạt:

## Yêu cầu và thiết kế

- [ ] Liên kết UC/BR/F/API operation/UI ID liên quan; không mở rộng phạm vi ngầm.
- [ ] Workflow, quyền, tách nhiệm vụ, audit, masking và lỗi an toàn khớp baseline đã duyệt.
- [ ] Thay đổi hợp đồng được cập nhật OpenAPI/truy vết trước hoặc cùng code.

## Mã nguồn và dữ liệu

- [ ] Code đã lint/format, build sạch, không warning bị bỏ qua.
- [ ] Migration mới bất biến, checksum đúng, có verify và kế hoạch rollback/forward-fix.
- [ ] Không có secret, tài khoản/dữ liệu lỗ hổng thật, raw report chưa làm sạch hoặc log nhạy cảm.
- [ ] Dependency mới có lý do, license/nguồn tin cậy, phiên bản exact và kết quả vulnerability review.

## Kiểm thử

- [ ] Unit/contract/integration/authorization-negative/state/concurrency test theo rủi ro đã chạy.
- [ ] PostgreSQL thật được dùng cho constraint/transaction/lock quan trọng; không thay bằng in-memory.
- [ ] Test failure/rollback/audit và dữ liệu partial/duplicate khi liên quan.
- [ ] Frontend có keyboard, screen reader wording, responsive và permission-denied checks khi liên quan.
- [ ] Bằng chứng test có command, phiên bản, kết quả và giới hạn; không chứa secret/payload thật.

## Review và vận hành

- [ ] Reviewer độc lập phù hợp đã duyệt; thay đổi auth/masking/file/audit/migration có ATTT/DBA review tương ứng.
- [ ] Metric/alert/runbook/feature flag/rollback được cập nhật nếu hành vi vận hành thay đổi.
- [ ] Tài liệu người dùng/vận hành và decision log được cập nhật khi cần.
- [ ] CI, dependency review và secret scan đạt; không merge bằng cách bỏ qua gate.

