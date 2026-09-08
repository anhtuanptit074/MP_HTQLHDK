# Quy tắc branch, pull request và review

## Mô hình

- `main` luôn ở trạng thái có thể build; cấm push trực tiếp khi đưa repository lên nền tảng quản lý mã nguồn.
- Nhánh ngắn: `feature/<uc-or-ticket>-mo-ta`, `fix/<ticket>-mo-ta`, `docs/<topic>`; rebase/merge theo quy định nhóm, không giữ nhánh tích hợp dài hạn.
- Mọi thay đổi qua pull request nhỏ, một mục tiêu, có truy vết và bằng chứng test.

## Gate đề xuất phải cấu hình trên nền tảng

- Bắt buộc CI `quality`, `database`, `dependency-review` khi áp dụng.
- Ít nhất một reviewer độc lập; hai reviewer cho auth/authorization, masking, raw/evidence/export, audit, migration phá vỡ và security control P0.
- Product/ nghiệp vụ review khi đổi workflow/thuật ngữ; DBA review SQL/lock/index; ATTT review quyền/secrets/parser/file.
- Cấm merge khi còn conversation chưa giải quyết, check thất bại hoặc branch chưa cập nhật với main theo chính sách.

Repository local không thể tự áp đặt branch protection. Project Owner phải bật rule trên GitHub/GitLab trước khi nhiều người cùng phát triển và lưu ảnh/chính sách làm bằng chứng.

## Pull request

Dùng `.github/pull_request_template.md`. Không dán log chứa dữ liệu thật; chỉ ghi đường dẫn artifact CI đã được kiểm soát. Reviewer kiểm cả action bị từ chối, đường lỗi và rollback, không chỉ happy path.

