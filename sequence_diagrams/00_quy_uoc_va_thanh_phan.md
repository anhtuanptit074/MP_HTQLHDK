# Quy ước và thành phần trong Sequence Diagram

## 1. Mức độ chi tiết

Mỗi hình biểu diễn một mục tiêu nghiệp vụ hoặc một nhóm mục tiêu liên tiếp. Actor gửi yêu cầu qua giao diện; dịch vụ kiểm quyền, xử lý và truy cập dữ liệu; database trả kết quả. Actor người dùng không gọi SQL trực tiếp. Sự kiện thời gian kích hoạt tác vụ nội bộ, không là người dùng có quyền wildcard.

Các tên “Dịch vụ tài sản”, “Dịch vụ nhập báo cáo”, “Dịch vụ khắc phục” là thành phần logic có thể cùng nằm trong một backend. Không suy từ số lifeline ra số máy chủ, tiến trình hoặc bảng. Bốn chức năng F-01–F-04 giữ nguyên; các thành phần quyền, audit, thông báo và báo cáo phục vụ chúng.

## 2. Thành phần

| Loại | Vai trò |
|---|---|
| actor | Người dùng có trách nhiệm nghiệp vụ; riêng Sự kiện thời gian là nguồn kích hoạt nội bộ |
| boundary | Giao diện nhận thao tác và hiển thị dữ liệu đã được máy chủ kiểm soát |
| control | Dịch vụ thực hiện quy tắc, kiểm quyền, quản lý giao dịch, parser hoặc thông báo |
| database | CSDL PostgreSQL theo mô hình 27 bảng hiện có |
| collections | Kho tệp riêng lưu báo cáo gốc, evidence, bản xuất; không là URL công khai |
| participant | OWASP ZAP bên ngoài; chỉ nhóm vận hành thao tác trực tiếp |

Trong Mermaid, boundary/control/database/collections được biểu diễn bằng `participant`; vai trò chi tiết giữ trong bảng lifeline của đặc tả. Mã bí danh `Svc` chỉ có ý nghĩa trong hình tương ứng.

## 3. Thông điệp và Combined Fragment

| Ký hiệu | Ý nghĩa |
|---|---|
| Mũi tên liền | Yêu cầu/lời gọi tại mức thiết kế; chưa phải endpoint hoặc chữ ký hàm cố định |
| Mũi tên nét đứt | Kết quả trả về |
| Mũi tên về chính lifeline | Tính toán/kiểm tra nội bộ |
| `alt` / `else` | Nhánh loại trừ nhau theo điều kiện; chỉ chạy nhánh có guard đúng |
| `opt` | Chạy khi có điều kiện tùy chọn |
| `loop` | Lặp theo tập bản ghi/người nhận; có thể chạy 0 lần |
| `ref` | Gọi hợp đồng của sơ đồ khác, bao gồm tiền điều kiện và lỗi; Mermaid thể hiện bằng Note có mã SD |
| `TX` | Giao dịch ngắn có khóa/kiểm trạng thái, ghi nghiệp vụ + audit và commit; thất bại rollback theo SD-00 |
| `BEGIN`, `COMMIT`, `ROLLBACK` | Ranh giới giao dịch khi cần nhấn mạnh tính nguyên tử |

Số tự động trong hình là số thông điệp, không phải mã UC. Số thứ tự mục “Trình tự nghiệp vụ” là bước nghiệp vụ, có thể phân rã thành nhiều thông điệp. Hình cố ý không vẽ activation bar cho toàn bộ khoảng chờ con người nhằm tránh ngụ ý một lời gọi đồng bộ kéo dài qua nhiều yêu cầu.

## 4. Hợp đồng kiểm quyền

Mọi yêu cầu người dùng tuân SD-00/K-01: tài khoản hoạt động, vai trò, grant chưa thu hồi, đúng asset và nhiệm vụ. Owner còn phải đúng chủ tài sản, Remediator giới hạn theo phiếu được giao. `asset_id = NULL` không là quyền trên mọi tài sản. Quản trị viên quản lý quyền và nhật ký quyền; không mặc định đọc lỗ hổng. Quyền đọc bản gốc và evidence nhạy cảm là hai cờ riêng.

Lời gọi `ref SD-00` không phải kiểm quyền một lần cho cả phiên: nó áp dụng cho từng yêu cầu tiếp theo, đặc biệt các lần đổi actor, phê duyệt, đóng/mở lại và tải tệp. Hết quyền phải kết thúc yêu cầu trước bước nghiệp vụ kế tiếp. Các tác vụ nội bộ chỉ xử lý dữ liệu đúng điều kiện và ghi actor hệ thống; không tự cấp quyền người dùng.

## 5. Giao dịch và dữ liệu đồng thời

Khóa theo thứ tự ổn định: asset → Finding → phiếu/hồ sơ/yêu cầu. Kiểm `finding.row_version`, `current_review_id`, `current_event_id` hoặc phiên bản nghiệp vụ tương ứng ở điểm áp dụng. Khi đọc nhiều đối tượng cùng loại, khóa theo ID tăng dần để giảm nguy cơ deadlock. Retry lỗi khóa/giao dịch chỉ khi chưa có commit và phải kiểm lại điều kiện; không nhân đôi sự kiện.

Ghi dữ liệu nghiệp vụ, con trỏ và audit chuyển trạng thái trong cùng TX. Không giữ khóa khi upload/parse tệp, gọi kho tệp hoặc chờ một người khác duyệt. Yêu cầu duyệt lần sau mở TX mới và kiểm lại quyền, trạng thái, phiên bản, căn cứ. Audit từ chối/lỗi ghi riêng sau rollback. Audit cần có actor, thời điểm, hành động, đối tượng, kết quả, lý do, trước/sau và FK bằng chứng phù hợp.

Kho tệp và PostgreSQL không được giả định dùng chung một giao dịch. Tệp chuẩn bị nằm trong vùng riêng; chỉ phát hành khi metadata + audit đã commit. Lỗi SQL sau ghi tệp không được để lại link truy cập; cần đối soát tệp chưa liên kết. Không xóa báo cáo gốc đã gắn lô khi dọn tệp tạm.

Với nhập báo cáo: khóa lô chống trùng `(asset_id, scan_id, file_sha256)`; danh tính Finding theo `(asset_id, fingerprint)`; lần xuất hiện theo `(finding_id, scan_id)`; dòng nguồn theo `(batch_id, record_locator)`. Mỗi dòng hợp lệ được ghi nguyên tử; lỗi dòng không để lại Finding mồ côi. Lỗi kỹ thuật sau các dòng đã commit đưa lô Failed và giữ truy vết, không báo thành công toàn lô.

## 6. Trạng thái cần phân biệt

| Đối tượng | Ví dụ trạng thái và ý nghĩa |
|---|---|
| scan_schedule | Active nghĩa là lịch có hiệu lực, không có nghĩa đang chạy ZAP |
| scan_occurrence | Completed nghĩa là Ops đã quét xong; Result Imported do nghiệp vụ nhập kết quả xác lập |
| import_batch | Completed/Partially Completed/Rejected/Failed mô tả xử lý tệp, không là trạng thái khắc phục |
| finding | New/Triaged/Assigned/In Progress/Pending Verification/Closed/Reopened… theo đúng bảng chuyển nguồn |
| finding_review | Proposed/Applied/Rejected; “chờ người thứ hai” không thêm trạng thái Finding |
| verification_request | Requested/Evaluated; kết quả từng lần lưu riêng ở verification_result |
| risk_acceptance | Pending/Needs Information/Approved/Rejected; trạng thái hồ sơ tách khỏi Finding |

Final Severity hiện hành đọc qua `finding.current_review_id` đến `finding_review.proposed_severity`; Risk/Confidence nguồn ở `finding_observation`. Không thêm Critical vào source_severity. Sự kiện phiếu là snapshot đầy đủ theo chuỗi `previous_event_id`, con trỏ `current_event_id`.

## 7. Lựa chọn kỹ thuật đề xuất và giới hạn nguồn

- Tên dịch vụ, thông điệp, ranh giới TX và đối soát tệp/thông báo là thiết kế ứng dụng đề xuất nhằm thực thi hợp đồng database; chưa phải mã đã triển khai.
- Chống trùng kỳ quét dùng khóa lịch và kiểm kỳ hiện có; không khẳng định SQL có khóa UNIQUE kỳ quét chưa được tạo.
- Không đặt số ngày SLA hoặc số giờ “sắp đến hạn”. Dùng chính sách đơn vị đã được duyệt.
- Không tự thêm cạnh đảo ngược False Positive, khôi phục Retired hoặc quyền hủy lịch/lần quét khi nguồn chưa đủ actor/guard.
- Gia hạn đang chờ không đổi hạn cũ; chỉ Approved đã áp dụng và còn điều kiện mới được dùng. Quyết định chưa đến valid_from không được áp dụng trước thời điểm hiệu lực.
- Xác minh sau khắc phục cần dữ liệu đủ bao phủ; “không tìm thấy” trong dữ liệu thiếu/lỗi không phải “đã hết lỗ hổng”.
- Không triển khai điều khiển ZAP/API, multi-scanner, CI/CD, ticket ngoài, email, SSO hoặc BI độc lập.

[Danh mục](README.md) · [Hợp đồng database gốc](../database/04_rang_buoc_va_giao_dich.md)
