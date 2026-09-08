# BÁO CÁO PHÂN TÍCH ĐỐI TƯỢNG SỬ DỤNG VÀ QUY TRÌNH HOẠT ĐỘNG

## HỆ THỐNG QUẢN LÝ QUÉT LỖ HỔNG WEBSITE ĐỊNH KỲ

| Thuộc tính | Nội dung |
| --- | --- |
| Phiên bản | 2.2 |
| Trạng thái | Hoàn thiện phân tích nghiệp vụ trong phạm vi đề tài |
| Công cụ quét bên ngoài | OWASP ZAP |
| Phạm vi chức năng chính | Khai báo tài sản, lịch quét, nhập kết quả, theo dõi khắc phục |
| Trọng tâm thiết kế | Phân loại mức độ nghiêm trọng, bảo mật báo cáo lỗ hổng, workflow xử lý |

---

## 1. Giới thiệu và giới hạn phạm vi

### 1.1. Mục tiêu đề tài

Hệ thống quản lý quét lỗ hổng website định kỳ được xây dựng để quản lý tập trung:

1. Các tài sản website được phép quét.
2. Kế hoạch và lịch quét định kỳ.
3. Kết quả quét được nhập từ OWASP ZAP.
4. Quá trình phân tích, phân công, khắc phục và xác minh lỗ hổng.

Hệ thống tập trung thiết kế cách phân loại mức độ nghiêm trọng, bảo vệ báo cáo lỗ hổng và kiểm soát workflow xử lý từ lúc nhận kết quả đến khi đóng hoặc mở lại lỗ hổng.

### 1.2. Phạm vi chức năng chính

| Mã | Chức năng | Nội dung |
| --- | --- | --- |
| F-01 | Khai báo tài sản | Quản lý thông tin website, chủ sở hữu, mức độ quan trọng và phạm vi được phép quét |
| F-02 | Lịch quét | Lập và theo dõi lịch quét định kỳ do nhóm vận hành thực hiện bằng OWASP ZAP |
| F-03 | Nhập kết quả | Nhập báo cáo OWASP ZAP, kiểm tra, chuẩn hóa, chống trùng và lưu kết quả |
| F-04 | Theo dõi khắc phục | Phân công, cập nhật tiến độ, quản lý thời hạn, quét xác minh, đóng hoặc mở lại lỗ hổng |

Phân quyền, nhật ký, thông báo và báo cáo chỉ được triển khai ở mức cần thiết để hỗ trợ bốn chức năng trên, bảo mật báo cáo và workflow. Các nội dung này không tạo thành phân hệ nghiệp vụ độc lập.

### 1.3. Ranh giới với OWASP ZAP

OWASP ZAP là công cụ thực hiện quét kỹ thuật bên ngoài hệ thống. Phạm vi của hệ thống quản lý được xác định như sau:

- Hệ thống lưu lịch dự kiến và trạng thái thực hiện quét.
- Nhóm vận hành chạy OWASP ZAP bên ngoài hệ thống theo lịch đã lập.
- Sau khi quét xong, nhóm vận hành xuất báo cáo từ ZAP và nhập vào hệ thống.
- Hệ thống không tự dò tìm, không gửi payload và không thực hiện active scan.
- Hệ thống không khởi chạy, dừng hoặc điều khiển OWASP ZAP qua API trong phạm vi đề tài.
- Kết quả từ ZAP là dữ liệu đầu vào; chuyên viên an toàn thông tin chịu trách nhiệm xác nhận kết quả và mức độ cuối cùng.

### 1.4. Ngoài phạm vi

- Tự phát triển bộ máy quét hoặc quy tắc phát hiện lỗ hổng.
- Tự động tấn công, khai thác thử hoặc tạo tải lên website.
- Điều khiển OWASP ZAP, ZAP Automation Framework hoặc container ZAP từ hệ thống.
- Nhập kết quả từ công cụ khác ngoài OWASP ZAP.
- Tự động sửa mã nguồn, thư viện hoặc cấu hình website.
- Thay thế hệ thống quản lý tài sản doanh nghiệp hoặc phần mềm quản lý công việc tổng quát.
- Xây dựng phân hệ SSO, email, sao lưu, giám sát hạ tầng hoặc BI riêng.
- Tích hợp CI/CD, kho mã nguồn hoặc hệ thống quản lý ticket bên ngoài.

---

## 2. Thuật ngữ nghiệp vụ

| Thuật ngữ | Giải thích |
| --- | --- |
| Tài sản | Website hoặc ứng dụng web thuộc phạm vi quản lý |
| Phạm vi quét | Domain, subdomain, URL cho phép và URL loại trừ đã được xác nhận |
| Lịch quét | Kế hoạch thời gian và tần suất thực hiện quét bằng OWASP ZAP |
| Lần quét | Một lần quét cụ thể được nhóm vận hành thực hiện bên ngoài hệ thống |
| Lô nhập | Một lần tải báo cáo OWASP ZAP vào hệ thống |
| Báo cáo gốc | Tệp JSON hoặc XML nguyên bản được xuất từ OWASP ZAP |
| Phát hiện | Bản ghi lỗ hổng được chuẩn hóa từ một cảnh báo ZAP |
| Lần xuất hiện | Việc một phát hiện được ghi nhận trong một lần quét cụ thể |
| Phiếu xử lý | Bản ghi dùng để phân công và theo dõi khắc phục phát hiện |
| Quét xác minh | Lần quét ZAP mới nhằm kiểm tra lỗ hổng sau khắc phục |
| Kết quả sai | Cảnh báo được xác nhận không phản ánh lỗ hổng thực tế |
| Chấp nhận rủi ro | Quyết định có thời hạn cho phép tạm thời chưa khắc phục lỗ hổng |

---

## 3. Đối tượng sử dụng và phân quyền

### 3.1. Đối tượng sử dụng

| Đối tượng | Trách nhiệm trong phạm vi đề tài |
| --- | --- |
| Quản trị viên | Cấp quyền cần thiết; không mặc nhiên được xem báo cáo hoặc bằng chứng lỗ hổng |
| Chuyên viên an toàn thông tin | Rà soát tài sản, phân tích kết quả, xác nhận mức độ, phân công và xác minh khắc phục |
| Chủ sở hữu tài sản | Cung cấp thông tin website, xác nhận phạm vi và ảnh hưởng nghiệp vụ, theo dõi xử lý |
| Nhóm vận hành quét | Thực hiện quét bằng OWASP ZAP bên ngoài hệ thống, cập nhật trạng thái và nhập báo cáo |
| Nhóm khắc phục kỹ thuật | Tiếp nhận phiếu, sửa lỗi, cập nhật tiến độ và bằng chứng |
| Người phê duyệt rủi ro | Phê duyệt hoặc từ chối đề nghị chấp nhận rủi ro theo thẩm quyền |
| Kiểm toán viên | Xem dữ liệu và lịch sử được cấp quyền; không sửa dữ liệu nghiệp vụ |

### 3.2. Chức năng của từng đối tượng

#### Quản trị viên

- Gán vai trò và phạm vi tài sản cho người dùng.
- Thu hồi quyền khi không còn nhu cầu.
- Xem nhật ký thay đổi quyền.
- Không sửa kết quả quét, mức độ, phiếu xử lý hoặc quyết định rủi ro.

#### Chuyên viên an toàn thông tin

- Rà soát và phê duyệt tài sản, phạm vi quét.
- Tạo hoặc phê duyệt lịch quét.
- Phân tích phát hiện được nhập từ OWASP ZAP.
- Xác nhận mức độ nghiêm trọng và thời hạn xử lý.
- Xác nhận kết quả đúng, kết quả sai hoặc ngoài phạm vi.
- Tạo phiếu và phân công nhóm khắc phục.
- Yêu cầu quét xác minh, đóng hoặc mở lại phát hiện.

#### Chủ sở hữu tài sản

- Khai báo hoặc đề nghị cập nhật thông tin website.
- Xác nhận chủ sở hữu, mức độ quan trọng và phạm vi quét.
- Xem phát hiện thuộc tài sản của mình.
- Xác nhận ảnh hưởng nghiệp vụ và theo dõi tiến độ.
- Đề nghị chấp nhận rủi ro khi chưa thể khắc phục đúng hạn.

#### Nhóm vận hành quét

- Theo dõi lịch đến hạn.
- Thực hiện quét bằng OWASP ZAP ở bên ngoài hệ thống.
- Cập nhật trạng thái hoàn thành, thất bại hoặc bỏ lỡ.
- Xuất báo cáo ZAP và nhập vào đúng tài sản, đúng lần quét.
- Thực hiện quét xác minh theo yêu cầu.
- Không tự thay đổi mức độ cuối cùng hoặc đóng phát hiện.

#### Nhóm khắc phục kỹ thuật

- Tiếp nhận phiếu được giao.
- Phân tích nguyên nhân và thực hiện biện pháp khắc phục.
- Cập nhật tiến độ, nhận xét và bằng chứng.
- Chuyển phiếu sang chờ xác minh khi hoàn thành.
- Không tự xác nhận đóng lỗ hổng.

#### Người phê duyệt rủi ro

- Xem lý do, ảnh hưởng, biện pháp bù trừ và thời hạn đề nghị.
- Phê duyệt, từ chối hoặc yêu cầu bổ sung.
- Không tự phê duyệt đề nghị do chính mình tạo.

#### Kiểm toán viên

- Xem tài sản, lịch quét, lô nhập, phiếu và lịch sử được cấp quyền.
- Kiểm tra thay đổi mức độ, kết quả sai và quyết định rủi ro.
- Không sửa, xóa hoặc chuyển trạng thái dữ liệu nghiệp vụ.

### 3.3. Ma trận phân quyền tối thiểu

Ký hiệu: C là tạo, R là xem, U là cập nhật hoặc thực hiện, A là phê duyệt, dấu gạch ngang là không có quyền mặc định.

| Chức năng | Quản trị | ATTT | Chủ tài sản | Vận hành quét | Khắc phục | Phê duyệt rủi ro | Kiểm toán |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Tài sản | Gán quyền | C/R/U/A | C/R/U/xác nhận | R | R theo phiếu | R | R |
| Lịch quét | Gán quyền | C/R/U/A | R/xác nhận thời gian | R/U trạng thái | R | R | R |
| Nhập báo cáo ZAP | Gán quyền | R/phân tích | R theo tài sản | C/R/U | R theo phiếu | R | R |
| Xác nhận mức độ | - | R/U | R/góp ý ảnh hưởng | R | R | A khi cần rà soát | R |
| Phiếu khắc phục | - | C/R/U | R/góp ý | R | R/U | R | R |
| Chấp nhận rủi ro | - | C/R/khuyến nghị | C/R/đề nghị | R | R/đề nghị | R/A | R |
| Đóng hoặc mở lại | - | U | R | R/cung cấp kết quả | R | R | R |
| Xuất báo cáo | - | Theo phạm vi | Theo tài sản | Theo nhiệm vụ | Theo phiếu | Theo thẩm quyền | Theo phạm vi |

### 3.4. Nguyên tắc phân tách nhiệm vụ

- Người khắc phục không được tự đóng phát hiện.
- Người nhập báo cáo không được sửa báo cáo gốc hoặc tự xác nhận mức độ cuối cùng.
- Người đề nghị chấp nhận rủi ro không được tự phê duyệt.
- Kiểm toán viên chỉ có quyền đọc.
- Quản trị viên không mặc nhiên có quyền xem nội dung lỗ hổng.

---

## 4. Quy trình hoạt động tổng thể

1. Chủ sở hữu hoặc chuyên viên ATTT khai báo tài sản website.
2. Chủ sở hữu xác nhận thông tin; chuyên viên ATTT phê duyệt phạm vi quét.
3. Chuyên viên ATTT tạo lịch quét định kỳ.
4. Nhóm vận hành thực hiện quét bên ngoài hệ thống bằng OWASP ZAP.
5. Nhóm vận hành cập nhật trạng thái lần quét và nhập báo cáo ZAP.
6. Hệ thống kiểm tra, lưu báo cáo gốc, chuẩn hóa và chống trùng kết quả.
7. Chuyên viên ATTT phân tích và xác nhận mức độ nghiêm trọng.
8. Hệ thống tạo hoặc liên kết phiếu; chuyên viên ATTT phân công nhóm khắc phục.
9. Nhóm khắc phục cập nhật tiến độ và bằng chứng.
10. Nhóm vận hành thực hiện quét xác minh bằng ZAP và nhập báo cáo mới.
11. Chuyên viên ATTT đóng hoặc mở lại phát hiện dựa trên kết quả xác minh.

Mọi bước phải liên kết được với tài sản, người thực hiện, thời điểm và bản ghi liên quan.

---

## 5. Phân tích bốn chức năng chính

### 5.1. Khai báo tài sản

#### 5.1.1. Dữ liệu tài sản

| Nhóm | Trường thông tin chính |
| --- | --- |
| Nhận diện | Mã tài sản, tên website, domain, URL gốc, môi trường |
| Sở hữu | Đơn vị, chủ sở hữu, nhóm kỹ thuật, chuyên viên ATTT phụ trách |
| Nghiệp vụ | Mức độ quan trọng, loại dữ liệu xử lý, mức độ phơi nhiễm Internet |
| Phạm vi quét | Domain, subdomain, URL cho phép, URL loại trừ |
| Thời gian | Khung giờ được phép quét, ngày tạo, ngày cập nhật |
| Kiểm soát | Người tạo, người xác nhận, người phê duyệt, trạng thái |

#### 5.1.2. Quy tắc nghiệp vụ

- Mã tài sản là duy nhất.
- Domain và URL phải đúng định dạng.
- Hệ thống cảnh báo tài sản trùng domain hoặc URL.
- Tài sản phải có chủ sở hữu, mức độ quan trọng và phạm vi quét trước khi kích hoạt.
- Mọi URL trong lịch quét phải thuộc phạm vi đã phê duyệt.
- Thay đổi phạm vi quét phải được chuyên viên ATTT phê duyệt lại.
- Tài sản ngừng hoạt động không bị xóa cứng để giữ lịch sử.

#### 5.1.3. Trạng thái tài sản

| Trạng thái | Ý nghĩa |
| --- | --- |
| Draft | Đang khai báo, chưa được dùng để tạo lịch hoạt động |
| Pending Approval | Đã đủ dữ liệu và chờ phê duyệt |
| Active | Được phép lập lịch quét và nhập kết quả |
| Suspended | Tạm dừng quét do thay đổi hoặc sự cố |
| Retired | Tài sản ngừng hoạt động, chỉ giữ lịch sử |

### 5.2. Lịch quét

#### 5.2.1. Dữ liệu lịch quét

- Mã và tên lịch.
- Tài sản và phạm vi quét.
- Tần suất: hàng ngày, hàng tuần, hàng tháng hoặc theo chu kỳ được chọn.
- Ngày hiệu lực và ngày kết thúc.
- Múi giờ, ngày giờ dự kiến và khung giờ được phép quét.
- Nhóm vận hành phụ trách.
- Ghi chú cấu hình ZAP tham chiếu, không chứa mật khẩu hoặc token.
- Trạng thái lịch.

#### 5.2.2. Quy tắc nghiệp vụ

- Chỉ tài sản Active mới được kích hoạt lịch.
- Phạm vi lịch phải nằm trong phạm vi tài sản đã phê duyệt.
- Hệ thống cảnh báo lịch trùng thời gian trên cùng tài sản.
- Hệ thống tạo bản ghi lần quét dự kiến khi đến lịch.
- Nhóm vận hành cập nhật kết quả thực hiện; hệ thống không tự chạy OWASP ZAP.
- Lịch được tạm dừng nhưng không xóa lịch sử.
- Thay đổi tài sản, phạm vi hoặc tần suất phải lưu lịch sử.

#### 5.2.3. Trạng thái

| Đối tượng | Trạng thái |
| --- | --- |
| Lịch quét | Draft, Active, Paused, Expired, Cancelled |
| Lần quét | Planned, Completed, Failed, Missed, Cancelled, Result Imported |

Failed dùng khi ZAP chạy không thành công; Missed dùng khi không thực hiện đúng lịch. Nhóm vận hành phải ghi lý do. Hệ thống chỉ ghi nhận và theo dõi, không tự khởi động lần quét thay thế.

### 5.3. Nhập kết quả OWASP ZAP

#### 5.3.1. Định dạng đầu vào

- Định dạng ưu tiên: Traditional JSON Report.
- Định dạng dự phòng: Traditional XML Report.
- Báo cáo HTML, Markdown hoặc PDF chỉ dùng để đọc hoặc đính kèm tham khảo, không dùng làm nguồn tự động tạo phát hiện.
- Mỗi báo cáo chỉ được nhập vào một tài sản và một lần quét xác định.

Phạm vi hiện tại sử dụng thao tác tải tệp báo cáo. Nhận kết quả qua API không thuộc phạm vi chức năng của phiên bản tài liệu này.

#### 5.3.2. Thông tin lô nhập

- Tài sản và lần quét liên quan.
- Phiên bản OWASP ZAP.
- Tên mẫu và định dạng báo cáo.
- Mã hoặc tên phiên quét nếu có.
- Thời điểm bắt đầu, kết thúc và nhập báo cáo.
- Người nhập.
- Tệp báo cáo gốc và giá trị kiểm tra toàn vẹn.

#### 5.3.3. Quy trình nhập

1. Kiểm tra quyền nhập đối với tài sản.
2. Kiểm tra tài sản Active và lần quét hợp lệ.
3. Kiểm tra loại tệp, kích thước và cấu trúc JSON hoặc XML.
4. Kiểm tra domain, URL trong báo cáo thuộc phạm vi tài sản.
5. Lưu báo cáo gốc ở trạng thái bất biến.
6. Che token, cookie, mật khẩu và dữ liệu nhạy cảm khỏi phần hiển thị.
7. Ánh xạ cảnh báo ZAP vào mô hình phát hiện chuẩn.
8. Kiểm tra lô nhập trùng và phát hiện trùng.
9. Tạo phát hiện mới hoặc ghi nhận lần xuất hiện mới.
10. Tổng hợp kết quả thành công, lỗi và chuyển phát hiện sang chờ phân tích.

#### 5.3.4. Ánh xạ trường OWASP ZAP

| Trường ZAP | Trường hệ thống | Quy tắc |
| --- | --- | --- |
| Alert Reference hoặc Rule ID | source_alert_ref | Giữ nguyên để truy vết quy tắc ZAP |
| Name | title | Tên phát hiện nguồn |
| URL hoặc Node Name | affected_url | Kiểm tra thuộc phạm vi và chuẩn hóa URL |
| Parameter hoặc Input Vector | parameter, input_vector | Dùng để xác định vị trí ảnh hưởng |
| Risk | source_severity | Giữ nguyên Informational, Low, Medium hoặc High |
| Confidence | source_confidence | Lưu riêng, không thay thế mức độ nghiêm trọng |
| Attack và Evidence | source_attack, source_evidence | Che dữ liệu nhạy cảm trước khi hiển thị |
| Description, Other Info | description, technical_details | Lưu thông tin kỹ thuật nguồn |
| Solution, Reference | remediation_guidance, references | Dùng làm hướng dẫn tham khảo |
| CWE ID, WASC ID | cwe_id, wasc_id | Phục vụ phân loại và báo cáo |
| Alert Tags | source_tags | Lưu thẻ nguồn khi có |

#### 5.3.5. Chống nhập trùng

- Khóa lô nhập gồm tài sản, lần quét và giá trị kiểm tra toàn vẹn của tệp.
- Nhập lại cùng tệp không tạo lô mới hoặc phát hiện mới.
- Dấu vân tay phát hiện gồm tài sản, Alert Reference hoặc Rule ID, URL chuẩn hóa, tham số hoặc Input Vector và phương thức HTTP khi có.
- Không dùng Attack hoặc Evidence trong dấu vân tay vì có thể thay đổi và chứa dữ liệu nhạy cảm.
- Phát hiện đang mở xuất hiện lại chỉ cập nhật last_seen và thêm lần xuất hiện.
- Phát hiện đã đóng xuất hiện lại được chuyển sang Reopened.

#### 5.3.6. Trạng thái lô nhập

| Trạng thái | Ý nghĩa |
| --- | --- |
| Received | Đã nhận tệp |
| Validating | Đang kiểm tra cấu trúc và phạm vi |
| Processing | Đang chuẩn hóa và chống trùng |
| Completed | Tất cả bản ghi hợp lệ đã được xử lý |
| Partially Completed | Có cả bản ghi thành công và lỗi |
| Rejected | Tệp sai định dạng, sai tài sản hoặc ngoài phạm vi |
| Failed | Lỗi kỹ thuật khi xử lý |

Lô Partially Completed phải hiển thị bản ghi lỗi mà không làm mất bản ghi hợp lệ. Lô Rejected không được tạo phát hiện.

### 5.4. Theo dõi khắc phục

#### 5.4.1. Dữ liệu phiếu xử lý

- Tài sản, phát hiện và vị trí ảnh hưởng.
- Mức độ nghiêm trọng cuối cùng.
- Mô tả và bằng chứng đã che dữ liệu nhạy cảm.
- Nhóm, người phụ trách và thời hạn xử lý.
- Trạng thái, tiến độ và nhận xét.
- Nguyên nhân và biện pháp khắc phục.
- Bằng chứng khắc phục.
- Yêu cầu và kết quả quét xác minh.
- Lịch sử phân công và chuyển trạng thái.

#### 5.4.2. Quy tắc theo dõi

- Chuyên viên ATTT phân công sau khi xác nhận phát hiện.
- Nhóm khắc phục phải cập nhật biện pháp và bằng chứng trước khi yêu cầu xác minh.
- Hệ thống ghi nhận phiếu sắp đến hạn hoặc quá hạn.
- Việc thay đổi người phụ trách, thời hạn hoặc trạng thái phải lưu lịch sử.
- Chỉ chuyên viên ATTT được đóng phát hiện sau khi có kết quả xác minh hợp lệ.

---

## 6. Phân loại mức độ nghiêm trọng

### 6.1. Nguyên tắc

- Risk do OWASP ZAP cung cấp là mức độ nguồn, không phải quyết định cuối cùng.
- Confidence của ZAP thể hiện độ tin cậy, không phải mức độ nghiêm trọng.
- Chuyên viên ATTT xác nhận Final Severity dựa trên bằng chứng và bối cảnh tài sản.
- Mọi điều chỉnh phải lưu giá trị trước, sau, lý do, người thực hiện và thời điểm.
- Hạ mức Critical hoặc High phải có lý do và người rà soát thứ hai theo chính sách.

### 6.2. Các giá trị cần lưu

| Trường | Ý nghĩa |
| --- | --- |
| source_severity | Risk nguyên bản của OWASP ZAP |
| source_confidence | Confidence nguyên bản của OWASP ZAP |
| final_severity | Mức độ cuối cùng do chuyên viên ATTT xác nhận |
| severity_reason | Căn cứ và lý do xác nhận hoặc điều chỉnh |
| assessed_by, assessed_at | Người và thời điểm đánh giá |
| reviewed_by, reviewed_at | Người và thời điểm rà soát khi bắt buộc |

### 6.3. Thang mức độ cuối cùng

| Mức độ | Tiêu chí tổng quát |
| --- | --- |
| Critical | Có khả năng gây chiếm quyền, vượt xác thực nghiêm trọng, lộ lượng lớn dữ liệu hoặc ảnh hưởng tài sản trọng yếu |
| High | Có khả năng gây ảnh hưởng lớn đến dữ liệu, quyền truy cập hoặc hoạt động website |
| Medium | Có ảnh hưởng đáng kể nhưng cần thêm điều kiện hoặc phạm vi ảnh hưởng hạn chế |
| Low | Ảnh hưởng nhỏ, khó khai thác hoặc có kiểm soát giảm thiểu hiệu quả |
| Informational | Quan sát kỹ thuật chưa tạo thành lỗ hổng trực tiếp |

OWASP ZAP chỉ cung cấp Risk gồm Informational, Low, Medium và High. Hệ thống không tự động đổi High thành Critical. Final Severity chỉ được nâng thành Critical sau khi chuyên viên ATTT đánh giá tác động và bối cảnh thực tế.

### 6.4. Yếu tố đánh giá bối cảnh

- Tài sản có công khai Internet hay không.
- Môi trường production hay thử nghiệm.
- Mức độ quan trọng của tài sản.
- Loại và độ nhạy cảm của dữ liệu.
- Khả năng khai thác thực tế.
- Phạm vi người dùng hoặc chức năng bị ảnh hưởng.
- Kiểm soát giảm thiểu đang có.
- Chất lượng bằng chứng và Confidence của ZAP.

### 6.5. Liên kết với thời hạn xử lý

Thời hạn xử lý được xác định từ Final Severity theo chính sách của đơn vị. Hệ thống lưu bảng thời hạn được phê duyệt nhưng không tự đặt một bộ thời hạn ngoài yêu cầu của đơn vị. Khi mức độ thay đổi, hệ thống lưu cả thời hạn cũ, thời hạn mới và lý do.

---

## 7. Workflow xử lý

### 7.1. Trạng thái phát hiện

| Trạng thái | Ý nghĩa | Chủ thể chính |
| --- | --- | --- |
| New | Phát hiện mới nhập, chờ phân tích | Chuyên viên ATTT |
| Triaged | Đã xác nhận kết quả và mức độ | Chuyên viên ATTT |
| Assigned | Đã phân công và có thời hạn | Chuyên viên ATTT |
| In Progress | Đang khắc phục | Nhóm khắc phục |
| Pending Verification | Đã xử lý, chờ quét xác minh | Nhóm vận hành và ATTT |
| Closed | Quét xác minh xác nhận không còn lỗ hổng | Chuyên viên ATTT |
| Reopened | Lỗ hổng còn tồn tại hoặc xuất hiện lại | ATTT và nhóm khắc phục |
| False Positive | Được xác nhận là kết quả sai | Chuyên viên ATTT |
| Pending Risk Acceptance | Chờ quyết định chấp nhận rủi ro | Người phê duyệt rủi ro |
| Risk Accepted | Rủi ro được chấp nhận có thời hạn | Chủ tài sản và người phê duyệt |
| Cancelled | Bản ghi sai hoặc ngoài phạm vi | Chuyên viên ATTT |

### 7.2. Ma trận chuyển trạng thái

| Từ | Sang | Người thực hiện | Điều kiện bắt buộc |
| --- | --- | --- | --- |
| New | Triaged | Chuyên viên ATTT | Xác nhận tài sản, vị trí, bằng chứng và Final Severity |
| New | False Positive | Chuyên viên ATTT | Có lý do và bằng chứng; Critical/High cần rà soát thứ hai |
| New | Cancelled | Chuyên viên ATTT | Bản ghi sai hoặc ngoài phạm vi; không xóa báo cáo gốc |
| Triaged | Assigned | Chuyên viên ATTT | Có người phụ trách và thời hạn |
| Assigned | In Progress | Nhóm khắc phục | Xác nhận tiếp nhận |
| In Progress | Pending Verification | Nhóm khắc phục | Có biện pháp và bằng chứng khắc phục |
| Pending Verification | Closed | Chuyên viên ATTT | Báo cáo ZAP của lần quét xác minh không còn dấu vân tay phát hiện |
| Pending Verification | Reopened | Chuyên viên ATTT | Kết quả xác minh còn lỗ hổng hoặc không hợp lệ |
| Closed | Reopened | Hệ thống hoặc ATTT | Dấu vân tay xuất hiện trong báo cáo ZAP mới |
| Triaged, Assigned, In Progress | Pending Risk Acceptance | Chủ tài sản hoặc ATTT | Có lý do, ảnh hưởng, biện pháp giảm thiểu và ngày hết hạn đề nghị |
| Pending Risk Acceptance | Risk Accepted | Người phê duyệt rủi ro | Đúng thẩm quyền, có điều kiện và ngày hết hạn |
| Pending Risk Acceptance | Trạng thái trước | Người phê duyệt rủi ro | Từ chối hoặc yêu cầu tiếp tục khắc phục |
| Risk Accepted | Reopened | Hệ thống hoặc ATTT | Hết hạn, vi phạm điều kiện hoặc rủi ro tăng |
| Reopened | Assigned | Chuyên viên ATTT | Phân công lại và xác định thời hạn mới |

Mọi chuyển trạng thái phải ghi trạng thái trước, trạng thái sau, người thực hiện, thời điểm, nhận xét và bằng chứng liên quan.

### 7.3. Kết quả sai

1. Chuyên viên ATTT nhập lý do và bằng chứng xác minh.
2. Với mức Critical hoặc High, người rà soát thứ hai xác nhận.
3. Phát hiện chuyển sang False Positive nhưng không bị xóa.
4. Nếu dấu vân tay xuất hiện lại, hệ thống tạo lần xuất hiện mới và yêu cầu rà soát lại; không tự bỏ qua vĩnh viễn.

### 7.4. Chấp nhận rủi ro

1. Chủ tài sản hoặc chuyên viên ATTT tạo đề nghị.
2. Đề nghị gồm lý do, tác động, biện pháp giảm thiểu, người chịu trách nhiệm và ngày hết hạn.
3. Người có thẩm quyền phê duyệt hoặc từ chối.
4. Hệ thống vẫn ghi nhận last_seen khi lỗ hổng tiếp tục xuất hiện trong các báo cáo mới.
5. Khi hết hạn mà chưa khắc phục hoặc gia hạn hợp lệ, phát hiện chuyển sang Reopened.

Gia hạn là quyết định mới; không ghi đè quyết định cũ.

### 7.5. Quét xác minh

1. Nhóm khắc phục cập nhật biện pháp và bằng chứng.
2. Chuyên viên ATTT tạo yêu cầu xác minh.
3. Nhóm vận hành chạy OWASP ZAP bên ngoài hệ thống.
4. Nhóm vận hành nhập báo cáo ZAP mới theo quy trình F-03.
5. Hệ thống đối chiếu dấu vân tay với phát hiện cần xác minh.
6. Chuyên viên ATTT đóng nếu không còn lỗ hổng hoặc mở lại nếu vẫn còn.

Không đóng phát hiện chỉ dựa trên xác nhận của nhóm khắc phục.

### 7.6. Tình huống ngoại lệ

| Tình huống | Cách xử lý |
| --- | --- |
| Không thực hiện đúng lịch | Đánh dấu Missed và ghi lý do |
| ZAP quét thất bại | Đánh dấu Failed và ghi lỗi vận hành |
| Tệp sai định dạng | Chuyển lô Rejected, không tạo phát hiện |
| Một phần bản ghi lỗi | Chuyển Partially Completed, giữ bản ghi hợp lệ và hiển thị lỗi |
| URL ngoài phạm vi | Từ chối bản ghi và yêu cầu ATTT rà soát |
| Không xác định được tài sản | Không cho nhập cho đến khi chọn đúng tài sản |
| Phát hiện trùng | Thêm lần xuất hiện, không tạo phiếu trùng |
| Bằng chứng chứa bí mật | Che khỏi giao diện và hạn chế quyền xem báo cáo gốc |

---

## 8. Bảo mật báo cáo lỗ hổng

### 8.1. Đối tượng cần bảo vệ

- Báo cáo OWASP ZAP gốc.
- Phát hiện đã chuẩn hóa.
- URL, tham số, Attack, Evidence và thông tin kỹ thuật.
- Bằng chứng khắc phục và kết quả xác minh.
- Tệp báo cáo được xuất từ hệ thống.

Các dữ liệu trên được xem là dữ liệu nhạy cảm vì có thể hỗ trợ việc khai thác website nếu bị lộ.

### 8.2. Kiểm soát truy cập

- Người dùng phải được xác thực trước khi truy cập.
- Quyền được kiểm tra theo vai trò và phạm vi tài sản.
- Mặc định từ chối khi không có quyền rõ ràng.
- Quyền xem báo cáo gốc tách riêng với quyền xem phát hiện đã được che dữ liệu.
- Chủ sở hữu chỉ xem tài sản của mình; nhóm khắc phục chỉ xem phiếu được giao.
- Quản trị viên không mặc nhiên được xem nội dung lỗ hổng.
- Kiểm tra quyền phải thực hiện tại phía máy chủ cho màn hình, API và tệp tải xuống.

### 8.3. Mã hóa và che dữ liệu

- Mã hóa dữ liệu khi truyền và khi lưu bằng cơ chế được đơn vị phê duyệt.
- Cookie, token, mật khẩu, khóa API và dữ liệu cá nhân không cần thiết phải được che khỏi giao diện và báo cáo xuất.
- Không đưa Attack, Evidence hoặc bí mật vào thông báo.
- Báo cáo gốc được lưu bất biến và có giá trị kiểm tra toàn vẹn.
- Người dùng nghiệp vụ không được sửa hoặc thay thế báo cáo gốc.

### 8.4. Xuất và tải báo cáo

- Chỉ người có quyền và đúng phạm vi tài sản mới được xuất hoặc tải.
- Báo cáo xuất áp dụng che dữ liệu theo quyền người nhận.
- Tệp xuất có nhãn dữ liệu nhạy cảm, người xuất, thời điểm và mã truy vết.
- Liên kết tải xuống có thời hạn và có thể thu hồi.
- Không sử dụng liên kết công khai lâu dài.
- Mọi lần tạo, tải và thu hồi báo cáo đều được ghi nhật ký.

### 8.5. Nhật ký bắt buộc

- Tạo, sửa, phê duyệt và thay đổi trạng thái tài sản hoặc lịch quét.
- Nhập, từ chối và xử lý lô báo cáo ZAP.
- Xem hoặc tải báo cáo gốc và bằng chứng nhạy cảm.
- Thay đổi mức độ, người phụ trách và thời hạn.
- Chuyển trạng thái, đóng, mở lại và xác nhận kết quả sai.
- Tạo, phê duyệt, từ chối hoặc gia hạn chấp nhận rủi ro.
- Tạo, tải và thu hồi báo cáo xuất.
- Thay đổi quyền truy cập.

Nhật ký phải ghi người thực hiện, thời điểm, hành động, đối tượng, kết quả và giá trị trước sau khi phù hợp. Người dùng nghiệp vụ không được sửa nhật ký.

### 8.6. Lưu trữ

- Thời hạn lưu báo cáo gốc, phát hiện, phiếu và nhật ký tuân theo chính sách của đơn vị.
- Không xóa cứng tài sản, phát hiện hoặc phiếu đang có giá trị lịch sử.
- Hết thời hạn lưu phải xử lý theo tác vụ được phê duyệt và có nhật ký.

---

## 9. Nội dung bổ trợ trực tiếp

Các nội dung dưới đây không phải phân hệ chức năng độc lập. Chúng chỉ hỗ trợ lịch quét, workflow và bảo mật báo cáo.

### 9.1. Thông báo tối thiểu

- Lịch quét sắp đến hạn hoặc bị bỏ lỡ.
- Lô nhập thất bại hoặc hoàn thành một phần.
- Có phát hiện Critical hoặc High.
- Phiếu mới được giao, sắp đến hạn hoặc quá hạn.
- Có yêu cầu quét xác minh.
- Đề nghị chấp nhận rủi ro chờ phê duyệt hoặc sắp hết hạn.

Thông báo chỉ chứa mã tài sản, mã phát hiện, trạng thái và liên kết vào hệ thống; không chứa bằng chứng nhạy cảm.

### 9.2. Báo cáo tối thiểu

- Danh sách phát hiện theo tài sản, mức độ và trạng thái.
- Lịch quét và trạng thái các lần quét.
- Lô nhập thành công, lỗi hoặc bị từ chối.
- Phiếu đang xử lý, sắp đến hạn và quá hạn.
- Phát hiện đã đóng, mở lại, kết quả sai hoặc chấp nhận rủi ro.

Không xây dựng phân hệ BI hoặc dashboard phân tích ngoài các dữ liệu cần theo dõi trên.

---

## 10. Dữ liệu nghiệp vụ cốt lõi

| Thực thể | Mục đích |
| --- | --- |
| UserRole | Vai trò và phạm vi tài sản, phục vụ phân quyền |
| Asset, AssetScope | Tài sản và phạm vi quét |
| ScanSchedule, ScanOccurrence | Lịch và từng lần quét do nhóm vận hành thực hiện |
| ImportBatch, RawReport, ImportError | Lô nhập, báo cáo ZAP gốc và lỗi nhập |
| Finding, FindingOccurrence | Phát hiện chuẩn hóa và các lần xuất hiện |
| SeverityAssessment | Lịch sử xác nhận và điều chỉnh mức độ |
| RemediationTicket, Assignment | Phiếu xử lý và phân công |
| Evidence, VerificationRequest | Bằng chứng và yêu cầu xác minh |
| RiskAcceptance | Đề nghị và quyết định chấp nhận rủi ro |
| AuditLog | Nhật ký cho các hành động nhạy cảm trong phạm vi đề tài |

Không bổ sung thực thể quản lý công cụ quét đa nhà cung cấp, CI/CD, kho mã nguồn, email, SSO hoặc quản lý công việc ngoài phạm vi.

---

## 11. Quy tắc nghiệp vụ tổng hợp

| Mã | Quy tắc |
| --- | --- |
| BR-01 | Chỉ tài sản Active và phạm vi đã phê duyệt mới được lập lịch hoạt động |
| BR-02 | OWASP ZAP thực hiện quét bên ngoài; hệ thống không thực hiện hoặc điều khiển quét |
| BR-03 | Kết quả chỉ được nhập từ báo cáo OWASP ZAP JSON hoặc XML |
| BR-04 | Mỗi kết quả phải truy vết tới tài sản, lần quét, lô nhập và báo cáo gốc |
| BR-05 | Báo cáo gốc bất biến và có kiểm tra toàn vẹn |
| BR-06 | Nhập lại cùng báo cáo không tạo dữ liệu trùng |
| BR-07 | Phát hiện xuất hiện lại sau khi đóng phải được mở lại |
| BR-08 | Risk, Confidence và Final Severity là các trường riêng |
| BR-09 | Điều chỉnh Final Severity phải có lý do và lịch sử |
| BR-10 | Nhóm khắc phục không tự đóng phát hiện |
| BR-11 | Chấp nhận rủi ro có người phê duyệt độc lập và ngày hết hạn |
| BR-12 | Bí mật trong báo cáo ZAP phải được che trước khi hiển thị hoặc xuất |
| BR-13 | Mọi truy cập báo cáo gốc và chuyển trạng thái quan trọng phải được ghi nhật ký |
| BR-14 | Không xóa cứng dữ liệu có giá trị truy vết |
| BR-15 | Nội dung hỗ trợ không được phát triển thành phân hệ ngoài bốn chức năng chính |

---

## 12. Ma trận truy vết yêu cầu

| Yêu cầu | Phần tài liệu đáp ứng |
| --- | --- |
| Khai báo tài sản | Mục 5.1 |
| Lịch quét | Mục 5.2 |
| Nhập kết quả | Mục 5.3 |
| Theo dõi khắc phục | Mục 5.4 |
| Phân loại mức độ nghiêm trọng | Mục 6 |
| Workflow xử lý | Mục 7 |
| Bảo mật báo cáo lỗ hổng | Mục 8 |
| OWASP ZAP là công cụ bên ngoài | Mục 1.3, 5.3 và 13 |
| Nội dung bổ trợ cần thiết | Mục 3, 9 và 10 |

---

## 13. Phụ lục tài liệu OWASP ZAP

Phụ lục này chỉ cung cấp tài liệu tham khảo cho nhóm vận hành và thiết kế bộ nhập báo cáo. Nội dung không bổ sung chức năng điều khiển OWASP ZAP vào hệ thống.

### 13.1. Cách sử dụng trong phạm vi đề tài

1. Nhóm vận hành cấu hình và chạy OWASP ZAP bên ngoài hệ thống.
2. Chỉ quét website và phạm vi đã được phê duyệt.
3. Sau khi quét, xuất Traditional JSON Report hoặc Traditional XML Report.
4. Tải báo cáo vào chức năng F-03.
5. Giữ phiên bản ZAP và tên mẫu báo cáo để truy vết.

### 13.2. Tài liệu chính thức

| Tài liệu | Mục đích |
| --- | --- |
| [ZAP Documentation](https://www.zaproxy.org/docs/) | Trang tài liệu tổng hợp |
| [ZAP Getting Started](https://www.zaproxy.org/getting-started/) | Hướng dẫn bắt đầu sử dụng ZAP |
| [ZAP Alerts](https://www.zaproxy.org/docs/desktop/start/features/alerts/) | Trường Risk, Confidence, URL, Parameter, Evidence, CWE và WASC |
| [ZAP Alert Catalogue](https://www.zaproxy.org/docs/alerts/) | Danh mục Alert ID và quy tắc phát hiện |
| [Report Generation](https://www.zaproxy.org/docs/desktop/addons/report-generation/) | Cách tạo báo cáo ZAP |
| [Report Templates](https://www.zaproxy.org/docs/desktop/addons/report-generation/templates/) | Mẫu Traditional JSON và Traditional XML |

Ngày đối chiếu tài liệu: 30/08/2026. Khi triển khai, phải ghi nhận phiên bản OWASP ZAP và mẫu báo cáo đã kiểm thử để tránh thay đổi định dạng không được kiểm soát.

---

## 14. Tiêu chí hoàn thành phân tích

Tài liệu đủ đầu vào cho bước tiếp theo khi các bên xác nhận:

- Trường dữ liệu và trạng thái tài sản.
- Cấu trúc, tần suất và trạng thái lịch quét.
- Mẫu báo cáo OWASP ZAP dùng để nhập.
- Quy tắc kiểm tra, ánh xạ và chống trùng kết quả.
- Quy tắc xác nhận Final Severity.
- Trạng thái và điều kiện chuyển của workflow.
- Quyền xem, tải, xuất và yêu cầu che dữ liệu báo cáo.
- Thẩm quyền đóng, mở lại, xác nhận kết quả sai và chấp nhận rủi ro.

Các bước tiếp theo chỉ gồm xây dựng use case, sơ đồ trạng thái, mô hình dữ liệu, đặc tả bộ nhập báo cáo ZAP và giao diện cho bốn chức năng chính.

---

## 15. Kết luận

Tài liệu giới hạn hệ thống trong bốn chức năng: khai báo tài sản, quản lý lịch quét, nhập kết quả OWASP ZAP và theo dõi khắc phục. Phân loại mức độ nghiêm trọng, bảo mật báo cáo và workflow xử lý được thiết kế xuyên suốt bốn chức năng này.

OWASP ZAP chịu trách nhiệm thực hiện quét ở bên ngoài. Hệ thống không tự quét, không điều khiển ZAP và không mở rộng sang công cụ quét khác. Phân quyền, nhật ký, thông báo tối thiểu và báo cáo tối thiểu chỉ tồn tại để bảo vệ dữ liệu và hỗ trợ workflow, không tạo thành các phân hệ ngoài yêu cầu đề tài.
