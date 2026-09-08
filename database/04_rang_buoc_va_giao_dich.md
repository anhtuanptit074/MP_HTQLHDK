# Ràng buộc và giao dịch — mô hình 27 bảng

Nguồn duy nhất: [doi_tuong_sd_quy_trinh_hd.md](../doi_tuong_sd_quy_trinh_hd.md) phiên bản 2.2. SQL đã có ràng buộc cấu trúc và các guard được liệt kê dưới đây. **SQL chưa phải một ứng dụng workflow hoặc phân quyền hoàn chỉnh.** Không cấp kết nối sửa bảng trực tiếp cho người dùng nghiệp vụ.

## 1. Ranh giới thực thi

| Quy tắc nguồn | DB thực thi trong bộ SQL | Ứng dụng bắt buộc thực hiện |
|---|---|---|
| BR-01: phạm vi/lịch | FK scope cùng tài sản; scope hiện hành đã duyệt; guard lịch Active có mục tiêu và tài sản Active | Xác thực URL/host/path, Exclude ưu tiên, múi giờ, khung giờ, xung đột lịch, hiệu lực, phê duyệt lại thay đổi theo nguồn |
| BR-02: ZAP ngoài hệ thống | Lưu lịch/lần quét, trạng thái và lý do lỗi | Vận hành chạy ZAP ngoài; không tự chạy thay thế khi Missed/Failed |
| BR-03: chỉ ZAP JSON/XML | Miền raw_format và mẫu Traditional cho lô thành công | Đọc nội dung thật, kiểm schema, mẫu, phiên bản, kích thước; HTML/MD/PDF chỉ tham khảo |
| BR-04: truy vết nguồn | FK ghép giữa Observation → Occurrence/Batch → Scan/Asset | Mỗi dòng hợp lệ tạo/liên kết đủ cả ba cấp dữ liệu trong giao dịch |
| BR-05: bản gốc bất biến | Trigger cấm sửa metadata nguồn của ImportBatch | Mã hóa byte tệp, kho riêng bất biến, checksum khi ghi/đọc; SQL không mã hóa hộ |
| BR-06: chống trùng | UNIQUE lô, fingerprint, occurrence, vị trí dòng | Trả lô có sẵn khi trùng; không tạo phiếu trùng từ kết quả nhập; đối soát bộ đếm |
| BR-07: tái xuất hiện | Cạnh Closed → Reopened, lưu Occurrence và FK audit | Kiểm đó là lần xuất hiện mới sau lần đóng; cập nhật last_seen và lịch sử |
| BR-08: mức độ | Source Risk và Confidence nằm ở Observation; Final Severity ở Review | ATTT xác nhận; không tự đổi High thành Critical; người nhập không tự xác nhận mức cuối |
| BR-09: lịch sử mức độ | Review Applied bất biến, có lý do/bằng chứng/bối cảnh; con trỏ cùng Finding; hạ High/Critical cần rà soát độc lập | Kiểm quyền, căn cứ chính sách/thẩm quyền người rà soát; cập nhật mức độ và hạn cùng giao dịch |
| BR-10: đóng | Kết quả hợp lệ có báo cáo, độ bao phủ, không fingerprint, scan sau bản sửa; chặn tác giả bản sửa tự kết luận Closed; cạnh đóng trỏ kết quả tương ứng | ATTT có quyền; bản sửa/yêu cầu/kết quả còn phù hợp chu kỳ hiện hành; xác nhận độ bao phủ và tất cả người thực sự khắc phục |
| BR-11: rủi ro | Người đề nghị khác người duyệt; lý do/thẩm quyền/điều kiện/hạn; quyết định bất biến; gia hạn trỏ hồ sơ đã áp dụng | Kiểm thẩm quyền thật, trạng thái, điều kiện chấp nhận; từ chối trả đúng trạng thái trước; xử lý hết hạn/vi phạm/rủi ro tăng |
| BR-12: bảo mật | Tách trường đã che, tham chiếu khóa và dữ liệu mã hóa; quyền nhạy cảm false mặc định | Che bí mật mọi nơi, mã hóa truyền/lưu; lọc quyền ở máy chủ, kể cả tải tệp |
| BR-13: nhật ký | Audit bất biến, FK căn cứ cùng Finding, chuyển trạng thái cần lý do | Ghi audit cho mọi hành động bắt buộc; SQL không tự bắt SELECT hay tự ghi mọi UPDATE |
| BR-14: lưu lịch sử | FK RESTRICT, chặn DELETE/TRUNCATE; bản kết luận/sự kiện bất biến | Tạm dừng/ngừng dùng thay xóa; xử lý hết hạn lưu bằng tác vụ được phê duyệt, ngoài luồng thường |
| BR-15: giới hạn đề tài | Chỉ 27 bảng đã đối chiếu nguồn | Không thêm điều khiển ZAP, đa scanner, CI/CD, ticket ngoài, SSO/email/BI riêng |

Guard Finding kiểm tập cạnh nhưng không tự kiểm mọi điều kiện trên cạnh. Ví dụ: DB cho phép một cạnh khôi phục từ Pending Risk Acceptance thuộc tập hợp hợp lệ, nhưng ứng dụng phải đọc đúng hồ sơ và khôi phục đúng `previous_finding_status`. Không coi các cờ `is_valid`, `coverage_confirmed`, vai trò hoặc tên trường `*_masked` là bằng chứng rằng hệ thống đã tự xác minh, phân quyền hoặc che dữ liệu.

## 2. Hợp đồng giao dịch chung

1. Xác thực người dùng ở máy chủ; đọc quyền hiện hành, trạng thái tài sản và vai trò đúng hành động.
2. Bắt đầu giao dịch. Khóa các đối tượng liên quan theo thứ tự ổn định: tài sản → Finding → phiếu/hồ sơ/yêu cầu. Kiểm lại quyền và trạng thái khi cần để tránh thay đổi xen kẽ.
3. Kiểm dữ liệu đầu vào, điều kiện nguồn, phiên bản hiện hành; chỉ dùng dữ liệu từ máy chủ cho người/thời điểm thực hiện.
4. Ghi nghiệp vụ, các con trỏ hiện hành và `audit_log` cùng giao dịch. Nếu thiếu audit hoặc thất bại ở bất kỳ bước nào, rollback.
5. Commit; gửi thông báo tối thiểu nếu có. Audit từ chối/lỗi phải được ghi trong giao dịch riêng để không mất khi rollback nghiệp vụ.

`finding.row_version` tăng khi cập nhật. Máy chủ sử dụng giá trị đã đọc để phát hiện thao tác dựa trên bản cũ. Guard và khóa hàng hỗ trợ tính nhất quán, nhưng chưa thay thế kiểm thử giao dịch nhiều phiên của ứng dụng.

## 3. Nhập và chuẩn hóa kết quả

Kiểm quyền nhập, Asset Active và lần quét đã hoàn thành đúng tài sản. Kiểm khóa `(asset_id,scan_id,file_sha256)` trước khi lưu metadata mới. Khi hai yêu cầu nhập trùng đồng thời, UNIQUE phân xử; bên thua lấy lô đã tồn tại, không tạo bản thứ hai. Chỉ trả thông tin lô sau khi kiểm quyền, tránh lộ dữ liệu qua checksum.

Tệp gốc đi vào kho riêng, giữ nguyên byte. `import_batch` lưu metadata bất biến và trạng thái Received → Validating → Processing; các trạng thái kết quả theo nguồn. Sai loại/cấu trúc toàn tệp: Rejected, không tạo Finding. Tệp có dòng hợp lệ và lỗi: giữ dòng hợp lệ + ImportError, kết thúc Partially Completed. Lỗi kỹ thuật: Failed. Lô hợp lệ không Alert: Completed với tổng số bằng 0.

Trong Processing, mỗi dòng hợp lệ thực hiện cùng giao dịch: tạo hoặc lấy Finding theo fingerprint; tạo hoặc lấy Occurrence theo Finding/scan; ghi Observation theo vị trí dòng; cập nhật first_seen/last_seen, xét tái xuất hiện, ghi audit. Lỗi dòng rollback phần dòng đó rồi ghi ImportError đã che; không giữ Finding mồ côi do dòng lỗi. Không tạo phiếu tự động cho mỗi dòng nhập.

Fingerprint dùng dữ liệu chưa che đã chuẩn hóa ổn định; bản hiển thị đã che không dùng để tính danh tính. Quy ước kỹ thuật đề xuất: HMAC-SHA-256 với khóa định danh trong kho khóa, có phiên bản; mã hóa tuple danh tính đầy đủ để kiểm tra va chạm. Chuẩn hóa host/scheme/port mặc định, đường dẫn/tham số theo quy ước nhất quán; giữ dữ liệu có ý nghĩa với nhận diện theo nguồn. Không cho việc che token làm hai vị trí khác nhau thành một. Không đưa Attack/Evidence vào fingerprint. Chính sách chuẩn hóa cần được hiện thực và kiểm thử bằng tệp ZAP thật trước sử dụng.

Hai tệp khác nhau cùng scan có thể chứa cùng Finding: dùng chung Occurrence, giữ Observation riêng. Số dòng thành công đếm Observation, không đếm Finding mới. CHECK bộ đếm không tự đếm bảng con; ứng dụng tổng hợp và đối soát trước khi hoàn tất lô. Báo cáo nhập lại không phát sinh phiếu trùng. Khi quét sau đóng có fingerprint thì mở lại; khi False Positive tái xuất hiện thì thêm Review loại Recheck, không tự thêm cạnh trạng thái ngoài nguồn.

## 4. Phân tích và Final Severity

Khóa Finding, kiểm quyền ATTT và nguồn đang đánh giá. Người nhập kết quả không được tự xác nhận mức độ cuối của chính kết quả đó. Lưu Review loại Severity với mức cũ, mức đề nghị, lý do, bối cảnh và bằng chứng. Hạ High/Critical bắt buộc rà soát thứ hai; trigger tự bật cờ trong tình huống này, người rà soát phải khác người đánh giá. Máy chủ kiểm thêm chính sách và thẩm quyền.

Khi Applied, phải dựa trên `current_review_id` còn hiện hành. Cập nhật Review, con trỏ Finding, hạn phiếu nếu mức độ ảnh hưởng SLA và audit cùng giao dịch. Cho phép nhiều đề nghị đang chờ; khi áp dụng phải kiểm lại tính phù hợp, không áp các đề nghị cũ xung đột.

False Positive cần lý do và bằng chứng; High/Critical cần người rà soát độc lập. Nếu chưa có Final Severity, guard dùng mức nguồn để không bỏ lọt yêu cầu rà soát cảnh báo High. Cancelled giữ nguồn gốc và lý do; không xóa tệp. Khi chuyển trạng thái, audit liên kết `finding_review_id` tới đúng kết luận đã Applied.

## 5. Phiếu, phân công và hạn xử lý

Phiếu là định danh ổn định. Không có UNIQUE trên `finding_id` nhằm tránh đặt giới hạn một phiếu cho cả vòng đời khi nguồn chưa quy định. F-04 phải tìm/liên kết phiếu phù hợp; không tự tạo phiếu mới mỗi lần Finding xuất hiện hoặc Reopened.

Khi thay đổi phiếu: khóa phiếu; đọc `current_event_id`; thêm `ticket_event` có `previous_event_id` bằng con trỏ đó; cập nhật `current_event_id` sang sự kiện mới; ghi audit; commit. Sự kiện là snapshot đầy đủ, không phải bản vá chỉ có trường thay đổi. `v_ticket_history` nối bản trước để trả người phụ trách cũ/mới và hạn cũ/mới. Guard chặn cập nhật dựa trên con trỏ đã cũ và chặn lùi con trỏ.

Assignment cần nhóm/người/hạn/SLA. Chỉ thay hạn hoặc người phụ trách phải sao chép nguyên tiến độ/biện pháp/bằng chứng và `remediated_by`/`remediated_at` từ bản trước. Khi cập nhật khắc phục thực sự dùng Progress. Thông tin người/thời điểm sửa do máy chủ xác định; người đổi hạn không trở thành người sửa. Trước Pending Verification phải có biện pháp, bằng chứng và bản sửa rõ ràng; 100% tiến độ không đủ để đóng.

SLA dùng dòng được đơn vị phê duyệt phù hợp đơn vị, Final Severity và thời điểm áp dụng. `policy_ref` là mã phiên bản quy định thật; `unit_name` phải được chuẩn hóa để tránh khác chính tả. Khi đổi chính sách tạo phiên bản mới, không sửa dòng cũ đang được phiếu tham chiếu. Khi đổi hạn lưu sự kiện mới và lý do; không có lịch ngày nghỉ hoặc số ngày SLA tự đặt.

## 6. Xác minh và đóng/mở lại

Yêu cầu xác minh tham chiếu bản `ticket_event` chứa khắc phục. Vận hành chạy ZAP mới ngoài hệ thống, nhập qua F-03. Kết quả lưu riêng, không ghi đè các lần đánh giá trước và không giới hạn một yêu cầu/kết quả đang chờ ngoài nguồn.

ATTT kiểm yêu cầu, bản sửa và lần quét còn phù hợp với lần xử lý hiện hành. Được đóng khi báo cáo mới sau khắc phục, đúng tài sản, đúng mục tiêu/độ bao phủ, xử lý nhập đủ giá trị và không còn fingerprint. Guard kiểm các cờ, thời điểm, trạng thái lô, FK và nguồn có mặt; ứng dụng phải chứng minh nội dung các cờ. Với lô Partial, không dùng dòng bị lỗi/mất để kết luận vắng lỗ hổng.

Nếu còn fingerprint hoặc báo cáo không hợp lệ thì kết luận Reopened theo nguồn. DB chặn kết luận Closed bởi `remediated_by` của bản sửa; máy chủ kiểm mọi người thực sự tham gia khắc phục và quyền ATTT. Không cấm một người chỉ vì từng được phân công trong quá khứ. Nguồn yêu cầu quét mới sau khắc phục, không đặt thêm điều kiện scan phải sau ngày lập yêu cầu.

Giao dịch đóng gồm: khóa Finding/yêu cầu/bản sửa; xác minh điều kiện; INSERT VerificationResult; UPDATE Finding với `verification_result_id` và trạng thái; ghi AuditLog với result/evidence cùng Finding; cập nhật trạng thái yêu cầu; commit. Không tái sử dụng kết quả của chu kỳ cũ để đóng một lần Reopened mới. Ghi audit thất bại thì rollback toàn bộ.

## 7. Chấp nhận, từ chối và gia hạn rủi ro

Owner/ATTT tạo đề nghị từ Triaged/Assigned/In Progress, lưu `previous_finding_status` trước khi chuyển Pending Risk Acceptance. Nhóm khắc phục không tự quyết định chuyển trạng thái rủi ro. Nhiều hồ sơ có thể đang chờ; mỗi lần áp dụng vẫn phải khóa và kiểm trạng thái hiện hành để tránh quyết định xung đột.

Khi hồ sơ Pending/Needs Information được bổ sung, lưu before/after đã che vào AuditLog có `risk_acceptance_id`. Người duyệt khác người đề nghị; quyền và căn cứ thẩm quyền phải được máy chủ kiểm. Duyệt cần lý do, điều kiện và hạn; từ chối khôi phục **đúng** trạng thái trước của hồ sơ được xử lý.

Kết luận Approved/Rejected bất biến. Nếu Approved chưa áp dụng, chỉ được đánh dấu `applied_at` đúng một lần, cùng giao dịch áp dụng quyết định; các trường quyết định khác không đổi. Gia hạn là bản ghi mới trỏ hồ sơ đã duyệt/áp dụng qua `renewal_of_id`; lịch sử các quyết định giữ nguyên. Gia hạn đang chờ không kéo dài hạn cũ và không tự chuyển Risk Accepted sang một trạng thái không có trong nguồn.

`v_latest_applied_risk` chỉ chọn quyết định Approved đã áp dụng mới nhất; `within_time_window` chỉ cho biết còn trong khoảng thời gian, không chứng minh mọi điều kiện nghiệp vụ còn thỏa mãn. Tác vụ ứng dụng phải kiểm hết hạn, vi phạm điều kiện hoặc rủi ro tăng và mở lại đúng quy trình. Không bỏ qua hạn chỉ vì tác vụ chưa chạy.

## 8. Quyền, xuất/tải và audit

`v_role_permission` là ma trận cố định, không cấp quyền thực thi database hoặc tự lọc dữ liệu. Mọi truy vấn màn hình/API/view phải lọc người dùng đang hoạt động, grant chưa thu hồi và đúng tài sản. Owner còn phải đúng chủ tài sản; Remediator chỉ xem phiếu được giao. Grant NULL asset không wildcard. Admin chỉ quản lý quyền và audit quyền, không mặc nhiên đọc lỗ hổng; Auditor không sửa nghiệp vụ.

Hai cờ nhạy cảm trên `user_role` có nghĩa là quyền tường minh trong đúng asset, không phải quyền mặc định theo vai trò. Cấp hoặc thay đổi cờ phải lưu lý do/audit; cách giữ lịch sử khuyến nghị là thu hồi dòng cũ và tạo dòng grant mới cùng giao dịch. Nội dung bản gốc/bằng chứng nhạy cảm chỉ trả qua endpoint riêng sau kiểm quyền và audit.

Một `report_export` là một lần phát hành đã che theo người nhận, có tệp riêng, trace_code, nhãn, người/thời điểm xuất, danh sách tài sản qua bảng nối, token hash, hạn và thu hồi. Chỉ lưu hash token ngẫu nhiên, không lưu token thật. Máy chủ tạo token an toàn, xác thực người nhận và kiểm quyền hiện hành trên **mọi** tài sản trong tệp cho mỗi lượt tải. Token hoặc một link chưa hết hạn không thay quyền. Ghi danh sách tài sản và phát hành tệp cùng quy trình nhất quán; không thêm nội dung/tài sản sau khi đã phát hành. Thu hồi được ghi vào chính bản xuất; không sửa người nhận, nội dung hoặc hạn để tái phát hành ngầm.

Tệp gốc, evidence và báo cáo xuất dùng kho riêng; metadata không phải URL công khai. Cần TLS, mã hóa lưu trữ và quản lý khóa theo cơ chế của đơn vị. View không thay RLS: bộ này chưa có chính sách RLS, tài khoản kết nối hay cơ chế xác thực máy chủ. REVOKE PUBLIC là nền tảng cài đặt, không phải phân quyền người dùng hoàn chỉnh.

Audit lưu actor, thời gian, hành động, đối tượng, kết quả, lý do và ảnh trước/sau khi phù hợp. Chuyển Finding dùng cột from/to cùng FK căn cứ; các thay đổi khác dùng ảnh JSON đã che và object_id. Truy cập bị từ chối tới ID không tồn tại ghi chuỗi object_id, không tạo FK giả. Audit lỗi/từ chối phải tồn tại dù rollback. Không đưa tệp thô, token, Attack/Evidence hoặc bí mật vào nhật ký hay thông báo.
