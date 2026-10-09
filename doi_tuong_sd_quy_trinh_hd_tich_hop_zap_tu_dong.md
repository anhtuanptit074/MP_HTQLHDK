# BÁO CÁO PHÂN TÍCH ĐỐI TƯỢNG SỬ DỤNG VÀ QUY TRÌNH HOẠT ĐỘNG

## HỆ THỐNG QUẢN LÝ QUÉT LỖ HỔNG WEBSITE ĐỊNH KỲ TÍCH HỢP TỰ ĐỘNG OWASP ZAP

| Thuộc tính | Nội dung |
| --- | --- |
| Phạm vi chức năng chính | Khai báo tài sản, lập lịch và điều phối quét, tự động tiếp nhận kết quả, theo dõi khắc phục |
| Trọng tâm thiết kế | Kiểm soát phạm vi quét, tự động hóa an toàn, phân loại mức độ, bảo mật báo cáo và workflow xử lý |
| Công cụ quét bên ngoài | OWASP ZAP, được hệ thống điều phối qua API |

---

## 1. Giới thiệu và giới hạn phạm vi

### 1.1. Mục tiêu đề tài

Hệ thống quản lý quét lỗ hổng website định kỳ được xây dựng để quản lý tập trung:

1. Các tài sản website được phép quét.
2. Kế hoạch, lịch quét và cấu hình quét đã được phê duyệt.
3. Việc tự động khởi chạy, theo dõi và kết thúc lần quét OWASP ZAP qua API.
4. Việc tự động tạo, tiếp nhận, kiểm tra và lưu báo cáo do OWASP ZAP sinh ra.
5. Quá trình phân tích, phân công, khắc phục và xác minh lỗ hổng.

Hệ thống không tự phát triển bộ máy dò tìm lỗ hổng. OWASP ZAP vẫn là công cụ thực hiện quét kỹ thuật; hệ thống đóng vai trò quản lý nghiệp vụ và điều phối tự động. Mọi lần quét phải gắn với tài sản, phạm vi, cấu hình, lịch hoặc yêu cầu xác minh đã được phê duyệt.

### 1.2. Phạm vi chức năng chính

| Mã | Chức năng | Nội dung |
| --- | --- | --- |
| F-01 | Khai báo tài sản | Quản lý website, chủ sở hữu, mức độ quan trọng, môi trường và phạm vi được phép quét |
| F-02 | Lịch và điều phối quét | Lập lịch, tạo lần quét, gọi OWASP ZAP qua API, theo dõi tiến độ, dừng và ghi nhận kết quả thực hiện |
| F-03 | Tiếp nhận kết quả tự động | Yêu cầu ZAP sinh báo cáo, tự động nhận báo cáo, kiểm tra, chuẩn hóa, chống trùng và lưu kết quả |
| F-04 | Theo dõi khắc phục | Phân công, cập nhật tiến độ, quản lý thời hạn, tự động quét xác minh, đóng hoặc mở lại lỗ hổng |

Phân quyền, nhật ký, thông báo, quản lý cấu hình tích hợp và báo cáo chỉ được triển khai ở mức cần thiết để hỗ trợ bốn chức năng trên. Các nội dung này không tạo thành phân hệ nghiệp vụ độc lập.

### 1.3. Ranh giới tích hợp với OWASP ZAP

OWASP ZAP là hệ thống bên ngoài, được kết nối có kiểm soát với hệ thống quản lý. Ranh giới trách nhiệm được xác định như sau:

- Hệ thống quản lý lưu tài sản, phạm vi, lịch, cấu hình tham chiếu và trạng thái nghiệp vụ.
- Bộ điều phối của hệ thống tạo yêu cầu quét khi đến lịch hoặc khi có yêu cầu xác minh hợp lệ.
- Hệ thống gọi API của OWASP ZAP để nạp cấu hình, khởi chạy kế hoạch quét, theo dõi tiến độ, dừng khi cần và yêu cầu sinh báo cáo.
- OWASP ZAP thực hiện spider, passive scan, active scan và tạo cảnh báo theo cấu hình được phê duyệt.
- Hệ thống chủ động nhận hoặc tải báo cáo từ ZAP, kiểm tra nguồn, tính toàn vẹn và phạm vi trước khi xử lý.
- OWASP ZAP không được truy cập trực tiếp cơ sở dữ liệu nghiệp vụ của hệ thống.
- Kết quả ZAP là dữ liệu nguồn; chuyên viên an toàn thông tin chịu trách nhiệm xác nhận kết quả và mức độ cuối cùng.
- Tự động hóa không thay thế quyết định nghiệp vụ của con người về kết quả sai, mức độ cuối cùng, chấp nhận rủi ro hoặc đóng lỗ hổng.

### 1.4. Ngoài phạm vi

- Tự phát triển bộ máy quét hoặc quy tắc phát hiện lỗ hổng thay cho OWASP ZAP.
- Quét tài sản, domain, URL hoặc môi trường chưa được phê duyệt.
- Tự động khai thác lỗ hổng ngoài các hành vi kiểm thử thuộc chính sách quét ZAP đã được phê duyệt.
- Tự động sửa mã nguồn, thư viện hoặc cấu hình website.
- Nhập kết quả từ công cụ khác ngoài OWASP ZAP.
- Cho phép người dùng gửi Automation Plan, script hoặc lệnh tùy ý tới ZAP.
- Cho phép ZAP hoặc tài khoản tích hợp truy cập trực tiếp dữ liệu nghiệp vụ không cần thiết.
- Thay thế hệ thống quản lý tài sản doanh nghiệp hoặc phần mềm quản lý công việc tổng quát.
- Xây dựng phân hệ SSO, email, sao lưu, giám sát hạ tầng hoặc BI riêng.
- Tích hợp CI/CD, kho mã nguồn hoặc hệ thống quản lý ticket bên ngoài.

---

## 2. Thuật ngữ nghiệp vụ

| Thuật ngữ | Giải thích |
| --- | --- |
| Tài sản | Website hoặc ứng dụng web thuộc phạm vi quản lý |
| Phạm vi quét | Domain, subdomain, URL cho phép và URL loại trừ đã được xác nhận |
| Hồ sơ quét ZAP | Bộ cấu hình có phiên bản quy định context, spider, chính sách quét, xác thực, giới hạn và mẫu báo cáo |
| Lịch quét | Kế hoạch thời gian và tần suất để hệ thống tự động yêu cầu ZAP thực hiện quét |
| Lần quét | Một lần thực hiện cụ thể được hệ thống tạo và OWASP ZAP xử lý |
| Phiên thực thi ZAP | Bản ghi kỹ thuật theo dõi vòng đời trao đổi giữa hệ thống và ZAP |
| Tài khoản dịch vụ | Định danh kỹ thuật không đăng nhập giao diện, dùng cho giao tiếp hệ thống với ZAP |
| Lô tiếp nhận | Một lần hệ thống tiếp nhận và xử lý báo cáo OWASP ZAP tự động |
| Báo cáo gốc | Tệp JSON hoặc XML nguyên bản do OWASP ZAP sinh ra và hệ thống tiếp nhận |
| Phát hiện | Bản ghi lỗ hổng được chuẩn hóa từ một cảnh báo ZAP |
| Lần xuất hiện | Việc một phát hiện được ghi nhận trong một lần quét cụ thể |
| Phiếu xử lý | Bản ghi dùng để phân công và theo dõi khắc phục phát hiện |
| Quét xác minh | Lần quét ZAP được hệ thống tự động khởi chạy để kiểm tra sau khắc phục |
| Kết quả sai | Cảnh báo được xác nhận không phản ánh lỗ hổng thực tế |
| Chấp nhận rủi ro | Quyết định có thời hạn cho phép tạm thời chưa khắc phục lỗ hổng |

---

## 3. Đối tượng sử dụng, tác nhân tích hợp và phân quyền

### 3.1. Đối tượng sử dụng

| Đối tượng | Trách nhiệm trong phạm vi đề tài |
| --- | --- |
| Quản trị viên | Quản lý tài khoản, vai trò và phạm vi quyền; không mặc nhiên được xem báo cáo hoặc bằng chứng lỗ hổng |
| Chuyên viên an toàn thông tin | Phê duyệt tài sản, phạm vi, hồ sơ quét và lịch; phân tích kết quả, xác nhận mức độ, phân công và xác minh khắc phục |
| Chủ sở hữu tài sản | Cung cấp thông tin website, xác nhận phạm vi, khung giờ, ảnh hưởng nghiệp vụ và theo dõi xử lý |
| Nhóm vận hành tích hợp ZAP | Quản lý kết nối và hồ sơ quét, giám sát phiên thực thi, xử lý lỗi, dừng hoặc chạy lại theo thẩm quyền |
| Nhóm khắc phục kỹ thuật | Tiếp nhận phiếu, sửa lỗi, cập nhật tiến độ và bằng chứng |
| Người phê duyệt rủi ro | Phê duyệt hoặc từ chối đề nghị chấp nhận rủi ro theo thẩm quyền |

Nhóm vận hành tích hợp ZAP không phải thực hiện thao tác chạy quét, xuất báo cáo và tải báo cáo thủ công trong luồng thông thường. Vai trò này tập trung vào cấu hình, giám sát và xử lý ngoại lệ của quá trình tự động.

### 3.2. Tác nhân và thành phần kỹ thuật

| Tác nhân hoặc thành phần | Loại | Trách nhiệm |
| --- | --- | --- |
| OWASP ZAP | Hệ thống bên ngoài | Nhận yêu cầu qua API, thực hiện quét, cung cấp tiến độ, cảnh báo và báo cáo |
| Bộ lập lịch | Thành phần nội bộ | Xác định lịch đến hạn và tạo lần quét duy nhất |
| Bộ điều phối ZAP | Thành phần nội bộ | Kiểm tra điều kiện, gọi API, theo dõi trạng thái, thu nhận báo cáo và ghi lỗi kỹ thuật |
| Tài khoản dịch vụ tích hợp | Định danh kỹ thuật | Xác thực giao tiếp với ZAP; không đăng nhập giao diện và không thay mặt người dùng ra quyết định nghiệp vụ |

Bộ lập lịch và bộ điều phối là thành phần bên trong ranh giới hệ thống nên không được xem là người dùng. OWASP ZAP là tác nhân hệ thống bên ngoài trong mô hình use case tích hợp.

### 3.3. Chức năng của từng đối tượng

#### Quản trị viên

- Gán vai trò và phạm vi tài sản cho người dùng.
- Tạo, khóa hoặc thu hồi định danh kỹ thuật theo quy trình được phê duyệt.
- Xem nhật ký thay đổi quyền và trạng thái dịch vụ tổng quát.
- Không sửa kết quả quét, mức độ, phiếu xử lý hoặc quyết định rủi ro.
- Không mặc nhiên được xem khóa API, báo cáo gốc hoặc bằng chứng nhạy cảm.

#### Chuyên viên an toàn thông tin

- Rà soát và phê duyệt tài sản, phạm vi quét và khung giờ cho phép.
- Tạo hoặc phê duyệt lịch quét và hồ sơ quét ZAP.
- Quyết định có cho phép active scan đối với từng loại tài sản và môi trường hay không.
- Theo dõi kết quả nghiệp vụ của các lần quét tự động.
- Phân tích phát hiện được hệ thống tiếp nhận từ OWASP ZAP.
- Xác nhận mức độ nghiêm trọng và thời hạn xử lý.
- Xác nhận kết quả đúng, kết quả sai hoặc ngoài phạm vi.
- Tạo phiếu và phân công nhóm khắc phục.
- Tạo yêu cầu quét xác minh; đóng hoặc mở lại phát hiện.

#### Chủ sở hữu tài sản

- Khai báo hoặc đề nghị cập nhật thông tin website.
- Xác nhận chủ sở hữu, mức độ quan trọng, phạm vi và khung giờ quét.
- Cung cấp đầu mối phối hợp khi việc quét có thể ảnh hưởng hoạt động.
- Xem phát hiện thuộc tài sản của mình.
- Xác nhận ảnh hưởng nghiệp vụ và theo dõi tiến độ.
- Đề nghị chấp nhận rủi ro khi chưa thể khắc phục đúng hạn.

#### Nhóm vận hành tích hợp ZAP

- Cấu hình kết nối ZAP, tham chiếu bí mật và thông số vận hành không nhạy cảm.
- Tạo phiên bản hồ sơ quét để chuyên viên ATTT phê duyệt.
- Giám sát hàng đợi, phiên thực thi, timeout và tình trạng kết nối.
- Xử lý lỗi xác thực, lỗi ZAP, lỗi sinh báo cáo và lỗi tiếp nhận dữ liệu.
- Dừng lần quét khi có dấu hiệu vượt phạm vi, gây ảnh hưởng hoặc theo yêu cầu có thẩm quyền.
- Chạy lại lần quét thất bại sau khi được phép; không tự động chạy lại nếu trạng thái trước đó không rõ ràng.
- Không tự thay đổi mức độ cuối cùng, xác nhận kết quả sai hoặc đóng phát hiện.

#### Nhóm khắc phục kỹ thuật

- Tiếp nhận phiếu được giao.
- Phân tích nguyên nhân và thực hiện biện pháp khắc phục.
- Cập nhật tiến độ, nhận xét và bằng chứng.
- Chuyển phiếu sang chờ xác minh khi hoàn thành.
- Không tự khởi chạy một lần quét ngoài workflow và không tự xác nhận đóng lỗ hổng.

#### Người phê duyệt rủi ro

- Xem lý do, ảnh hưởng, biện pháp bù trừ và thời hạn đề nghị.
- Phê duyệt, từ chối hoặc yêu cầu bổ sung.
- Không tự phê duyệt đề nghị do chính mình tạo.

### 3.4. Ma trận phân quyền tối thiểu

Ký hiệu: C là tạo, R là xem, U là cập nhật hoặc thực hiện, A là phê duyệt, dấu gạch ngang là không có quyền mặc định.

| Chức năng | Quản trị | ATTT | Chủ tài sản | Vận hành tích hợp | Khắc phục | Phê duyệt rủi ro |
| --- | --- | --- | --- | --- | --- | --- |
| Tài sản và phạm vi | Gán quyền | C/R/U/A | C/R/U/xác nhận | R | R theo phiếu | R |
| Hồ sơ quét ZAP | Gán quyền | R/A | R giới hạn | C/R/U | - | - |
| Lịch quét | Gán quyền | C/R/U/A | R/xác nhận thời gian | R/giám sát | R | R |
| Phiên thực thi ZAP | - | R/dừng theo thẩm quyền | R trạng thái tài sản | R/U/xử lý lỗi | R theo phiếu | - |
| Báo cáo và lô tiếp nhận | Gán quyền | R/phân tích | R theo tài sản | R/xử lý lỗi kỹ thuật | R theo phiếu | R theo thẩm quyền |
| Xác nhận mức độ | - | R/U | R/góp ý ảnh hưởng | R | R | A khi cần rà soát |
| Phiếu khắc phục | - | C/R/U | R/góp ý | R trạng thái | R/U | R |
| Chấp nhận rủi ro | - | C/R/khuyến nghị | C/R/đề nghị | R | R/đề nghị | R/A |
| Đóng hoặc mở lại | - | U | R | R/cung cấp trạng thái kỹ thuật | R | R |
| Xuất báo cáo | - | Theo phạm vi | Theo tài sản | Theo nhiệm vụ | Theo phiếu | Theo thẩm quyền |

Tài khoản dịch vụ chỉ có quyền kỹ thuật tối thiểu để thực hiện luồng tích hợp. Tài khoản này không có phiên đăng nhập người dùng và không có quyền phê duyệt nghiệp vụ.

### 3.5. Nguyên tắc phân tách nhiệm vụ

- Người khắc phục không được tự đóng phát hiện.
- Người quản lý kết nối hoặc phiên thực thi ZAP không được tự xác nhận mức độ cuối cùng.
- Người tạo hồ sơ quét không tự phê duyệt phiên bản do chính mình tạo khi chính sách yêu cầu phê duyệt độc lập.
- Người đề nghị chấp nhận rủi ro không được tự phê duyệt.
- Quản trị viên không mặc nhiên có quyền xem nội dung lỗ hổng.
- Tài khoản dịch vụ không được sử dụng để che giấu người đã phê duyệt hoặc yêu cầu một hành động.
- Mọi hành động tự động phải ghi rõ nguồn là hệ thống và truy vết đến lịch, yêu cầu hoặc người phê duyệt tương ứng.

---

## 4. Quy trình hoạt động tổng thể

1. Chủ sở hữu hoặc chuyên viên ATTT khai báo tài sản website.
2. Chủ sở hữu xác nhận thông tin và khung giờ; chuyên viên ATTT phê duyệt phạm vi quét.
3. Nhóm vận hành tích hợp tạo hồ sơ quét có phiên bản; chuyên viên ATTT phê duyệt hồ sơ phù hợp với tài sản.
4. Chuyên viên ATTT tạo và kích hoạt lịch quét định kỳ.
5. Khi đến hạn, bộ lập lịch tạo một lần quét duy nhất và đưa vào hàng đợi.
6. Bộ điều phối kiểm tra tài sản, phạm vi, lịch và hồ sơ quét còn hiệu lực.
7. Hệ thống gọi API OWASP ZAP để khởi chạy kế hoạch quét và ghi mã phiên thực thi.
8. Hệ thống theo dõi tiến độ, timeout và lỗi; người có thẩm quyền có thể yêu cầu dừng.
9. Khi quét hoàn thành, hệ thống yêu cầu ZAP sinh báo cáo và tự động tiếp nhận báo cáo.
10. Hệ thống kiểm tra nguồn, định dạng, kích thước, giá trị toàn vẹn và phạm vi; sau đó lưu báo cáo gốc bất biến.
11. Hệ thống chuẩn hóa, chống trùng và tạo hoặc cập nhật phát hiện.
12. Chuyên viên ATTT phân tích và xác nhận mức độ nghiêm trọng.
13. Hệ thống tạo hoặc liên kết phiếu; chuyên viên ATTT phân công nhóm khắc phục.
14. Nhóm khắc phục cập nhật tiến độ và bằng chứng.
15. Khi có yêu cầu xác minh hợp lệ, hệ thống tự động tạo và điều phối lần quét xác minh.
16. Chuyên viên ATTT đóng hoặc mở lại phát hiện dựa trên kết quả và mức độ bao phủ của lần quét xác minh.

Mọi bước phải liên kết được với tài sản, phạm vi, hồ sơ quét, phiên thực thi, thời điểm, tác nhân kỹ thuật và người phê duyệt hoặc yêu cầu khi có.

---

## 5. Phân tích bốn chức năng chính

### 5.1. Khai báo tài sản

#### 5.1.1. Dữ liệu tài sản

| Nhóm | Trường thông tin chính |
| --- | --- |
| Nhận diện | Mã tài sản, tên website, domain, URL gốc, môi trường |
| Sở hữu | Đơn vị, chủ sở hữu, nhóm kỹ thuật, chuyên viên ATTT phụ trách |
| Nghiệp vụ | Mức độ quan trọng, loại dữ liệu xử lý, mức độ phơi nhiễm Internet |
| Phạm vi quét | Domain, subdomain, URL cho phép, URL loại trừ, biểu thức include/exclude |
| Thời gian | Khung giờ được phép quét, ngày tạo, ngày cập nhật |
| Tự động hóa | Cho phép quét tự động, loại quét được phép, hồ sơ ZAP mặc định |
| Kiểm soát | Người tạo, người xác nhận, người phê duyệt, trạng thái |

#### 5.1.2. Quy tắc nghiệp vụ

- Mã tài sản là duy nhất.
- Domain và URL phải đúng định dạng.
- Hệ thống cảnh báo tài sản trùng domain hoặc URL.
- Tài sản phải có chủ sở hữu, mức độ quan trọng, phạm vi và khung giờ quét trước khi kích hoạt.
- Mọi URL và context gửi tới ZAP phải nằm trong phạm vi đã phê duyệt.
- Active scan chỉ được bật khi có phê duyệt rõ ràng cho tài sản và môi trường tương ứng.
- Thay đổi phạm vi, xác thực hoặc loại quét phải được chuyên viên ATTT phê duyệt lại.
- Tài sản Suspended hoặc Retired không được tạo lần quét mới.
- Tài sản ngừng hoạt động không bị xóa cứng để giữ lịch sử.

#### 5.1.3. Trạng thái tài sản

| Trạng thái | Ý nghĩa |
| --- | --- |
| Draft | Đang khai báo, chưa được dùng để tạo lịch hoạt động |
| Pending Approval | Đã đủ dữ liệu và chờ phê duyệt |
| Active | Được phép gắn hồ sơ và kích hoạt lịch quét |
| Suspended | Tạm dừng quét do thay đổi, sự cố hoặc yêu cầu kiểm soát |
| Retired | Tài sản ngừng hoạt động, chỉ giữ lịch sử |

### 5.2. Lịch và điều phối quét tự động

#### 5.2.1. Dữ liệu lịch quét

- Mã và tên lịch.
- Tài sản và phiên bản phạm vi quét.
- Hồ sơ quét ZAP đã được phê duyệt.
- Tần suất: hàng ngày, hàng tuần, hàng tháng hoặc theo chu kỳ được hỗ trợ.
- Ngày hiệu lực và ngày kết thúc.
- Múi giờ, ngày giờ dự kiến và khung giờ được phép quét.
- Thời gian tối đa và giới hạn tài nguyên.
- Cờ cho phép điều phối tự động.
- Người tạo, người phê duyệt và nhóm vận hành tích hợp phụ trách.
- Trạng thái lịch.

#### 5.2.2. Dữ liệu hồ sơ quét ZAP

- Mã, tên và số phiên bản hồ sơ.
- Loại spider và các giới hạn crawl.
- Bật hoặc tắt passive scan, active scan.
- Chính sách và tập quy tắc active scan được phép.
- Context, phương thức xác thực và tham chiếu bí mật.
- Giới hạn thời gian, số luồng, tốc độ hoặc tải theo chính sách.
- Mẫu báo cáo JSON hoặc XML dùng làm nguồn xử lý.
- Người tạo, người phê duyệt, ngày hiệu lực và trạng thái.

Hồ sơ đã được phê duyệt phải bất biến. Khi cần thay đổi, hệ thống tạo phiên bản mới để giữ khả năng truy vết.

#### 5.2.3. Quy tắc điều phối

- Chỉ tài sản Active, phạm vi Approved, lịch Active và hồ sơ ZAP Active mới được điều phối.
- Bộ điều phối phải kiểm tra lại phạm vi ngay trước khi gửi yêu cầu tới ZAP.
- Mỗi kỳ lịch chỉ tạo tối đa một lần quét nghiệp vụ.
- Không chạy đồng thời nhiều lần quét trên cùng tài sản nếu chính sách không cho phép.
- Hệ thống lưu mã phiên hoặc plan ID do ZAP trả về để theo dõi.
- Hệ thống định kỳ lấy trạng thái từ ZAP cho tới khi hoàn thành, thất bại, bị dừng hoặc hết thời gian.
- Không tự động chạy lại một active scan đã bắt đầu gửi request nếu không xác định chắc chắn trạng thái trước đó.
- Người có thẩm quyền có thể tạm dừng lịch hoặc yêu cầu dừng phiên thực thi.
- Thay đổi lịch, hồ sơ, phạm vi hoặc yêu cầu dừng phải được ghi lịch sử.

#### 5.2.4. Trạng thái

| Đối tượng | Trạng thái |
| --- | --- |
| Hồ sơ quét ZAP | Draft, Pending Approval, Active, Retired |
| Lịch quét | Draft, Active, Paused, Expired, Cancelled |
| Lần quét | Planned, Queued, Running, Completed, Failed, Timed Out, Cancelled, Result Received, Result Processed |
| Phiên thực thi ZAP | Preparing, Starting, Running, Collecting Report, Report Received, Failed, Timed Out, Cancelled |

`Failed` dùng khi ZAP hoặc luồng tích hợp kết thúc có lỗi xác định. `Timed Out` dùng khi vượt thời gian cho phép. `Missed` không còn là trạng thái thông thường; nếu bộ lập lịch không tạo được lần quét đúng hạn, hệ thống ghi sự kiện bỏ lỡ lịch và phát cảnh báo vận hành.

### 5.3. Tự động tiếp nhận và xử lý kết quả OWASP ZAP

#### 5.3.1. Nguồn và định dạng đầu vào

- Định dạng ưu tiên: Traditional JSON Report.
- Định dạng dự phòng: Traditional XML Report.
- Báo cáo HTML hoặc PDF có thể được tạo để người có quyền đọc, nhưng không dùng làm nguồn tự động tạo phát hiện.
- Báo cáo phải được sinh từ đúng phiên thực thi ZAP và gắn với một tài sản, một lần quét xác định.
- Luồng thông thường không yêu cầu người dùng tải tệp lên thủ công.

#### 5.3.2. Quy trình nhận báo cáo

1. Bộ điều phối xác nhận phiên thực thi đã hoàn thành và thuộc lần quét hợp lệ.
2. Hệ thống gọi API yêu cầu ZAP tạo báo cáo theo mẫu được phê duyệt.
3. Hệ thống nhận nội dung hoặc tải báo cáo bằng kênh tích hợp được bảo vệ.
4. Hệ thống kiểm tra mã phản hồi, loại nội dung, kích thước và cấu trúc JSON hoặc XML.
5. Hệ thống xác nhận báo cáo nhận diện OWASP ZAP, đúng phiên bản và đúng tài sản dự kiến.
6. Hệ thống kiểm tra mọi domain và URL trong báo cáo thuộc phạm vi đã phê duyệt.
7. Hệ thống tính SHA-256, lưu metadata và báo cáo gốc ở trạng thái bất biến.
8. Hệ thống che token, cookie, mật khẩu và dữ liệu nhạy cảm khỏi phần hiển thị.
9. Hệ thống tạo lô tiếp nhận và ánh xạ cảnh báo ZAP vào mô hình phát hiện chuẩn.
10. Hệ thống kiểm tra lô trùng và phát hiện trùng.
11. Hệ thống tạo phát hiện mới hoặc ghi nhận lần xuất hiện mới.
12. Hệ thống tổng hợp kết quả thành công, lỗi và chuyển phát hiện sang chờ phân tích.

Nếu báo cáo không hợp lệ, hệ thống không tạo phát hiện và phải giữ thông tin lỗi đã che để nhóm vận hành tích hợp xử lý.

#### 5.3.3. Thông tin lô tiếp nhận

- Tài sản, lần quét và phiên thực thi ZAP liên quan.
- Phiên bản OWASP ZAP.
- Tên mẫu và định dạng báo cáo.
- Mã phiên, plan ID hoặc mã tương quan.
- Thời điểm bắt đầu quét, kết thúc, tạo báo cáo, nhận và xử lý.
- Tài khoản dịch vụ thực hiện giao tiếp.
- Người hoặc lịch đã khởi nguồn lần quét.
- Tệp báo cáo gốc, SHA-256, kích thước và vị trí lưu trữ.

#### 5.3.4. Ánh xạ trường OWASP ZAP

| Trường ZAP | Trường hệ thống | Quy tắc |
| --- | --- | --- |
| Alert Reference hoặc Rule ID | source_alert_ref | Giữ nguyên để truy vết quy tắc ZAP |
| Name | title | Tên phát hiện nguồn |
| URL hoặc Node Name | affected_url | Kiểm tra thuộc phạm vi và chuẩn hóa URL |
| Method | http_method | Chuẩn hóa phương thức HTTP |
| Parameter hoặc Input Vector | parameter, input_vector | Dùng để xác định vị trí ảnh hưởng |
| Risk | source_severity | Giữ nguyên Informational, Low, Medium hoặc High |
| Confidence | source_confidence | Lưu riêng, không thay thế mức độ nghiêm trọng |
| Attack và Evidence | source_attack, source_evidence | Che dữ liệu nhạy cảm trước khi hiển thị |
| Description, Other Info | description, technical_details | Lưu thông tin kỹ thuật nguồn |
| Solution, Reference | remediation_guidance, references | Dùng làm hướng dẫn tham khảo |
| CWE ID, WASC ID | cwe_id, wasc_id | Phục vụ phân loại và báo cáo |
| Alert Tags | source_tags | Lưu thẻ nguồn khi có |

#### 5.3.5. Chống tiếp nhận và xử lý trùng

- Khóa lô gồm tài sản, lần quét, phiên thực thi và SHA-256 của báo cáo.
- Nhận lại cùng báo cáo không tạo lô mới hoặc phát hiện mới.
- Mỗi yêu cầu API thay đổi trạng thái phải có mã tương quan hoặc khóa idempotency của hệ thống.
- Dấu vân tay phát hiện gồm tài sản, Alert Reference hoặc Rule ID, URL chuẩn hóa, tham số hoặc Input Vector và phương thức HTTP khi có.
- Không dùng Attack hoặc Evidence trong dấu vân tay vì có thể thay đổi và chứa dữ liệu nhạy cảm.
- Phát hiện đang mở xuất hiện lại chỉ cập nhật `last_seen` và thêm lần xuất hiện.
- Phát hiện đã đóng xuất hiện lại được chuyển sang `Reopened` sau khi kiểm tra quy tắc liên quan.

#### 5.3.6. Trạng thái lô tiếp nhận

| Trạng thái | Ý nghĩa |
| --- | --- |
| Received | Hệ thống đã nhận báo cáo và metadata |
| Validating | Đang kiểm tra nguồn, cấu trúc, tính toàn vẹn và phạm vi |
| Processing | Đang chuẩn hóa và chống trùng |
| Completed | Tất cả bản ghi hợp lệ đã được xử lý |
| Partially Completed | Có cả bản ghi thành công và lỗi |
| Rejected | Báo cáo sai nguồn, sai định dạng, sai tài sản hoặc ngoài phạm vi |
| Failed | Lỗi kỹ thuật khi tiếp nhận hoặc xử lý |

Lô `Partially Completed` phải hiển thị bản ghi lỗi mà không làm mất bản ghi hợp lệ. Lô `Rejected` không được tạo phát hiện.

### 5.4. Theo dõi khắc phục

#### 5.4.1. Dữ liệu phiếu xử lý

- Tài sản, phát hiện và vị trí ảnh hưởng.
- Mức độ nghiêm trọng cuối cùng.
- Mô tả và bằng chứng đã che dữ liệu nhạy cảm.
- Nhóm, người phụ trách và thời hạn xử lý.
- Trạng thái, tiến độ và nhận xét.
- Nguyên nhân và biện pháp khắc phục.
- Bằng chứng khắc phục.
- Yêu cầu, lần quét và kết quả xác minh tự động.
- Lịch sử phân công và chuyển trạng thái.

#### 5.4.2. Quy tắc theo dõi

- Chuyên viên ATTT phân công sau khi xác nhận phát hiện.
- Nhóm khắc phục phải cập nhật biện pháp và bằng chứng trước khi yêu cầu xác minh.
- Yêu cầu xác minh hợp lệ tạo một lần quét mới, không sử dụng lại báo cáo cũ.
- Hệ thống ghi nhận phiếu sắp đến hạn hoặc quá hạn.
- Việc thay đổi người phụ trách, thời hạn hoặc trạng thái phải lưu lịch sử.
- Chỉ chuyên viên ATTT được đóng phát hiện sau khi có kết quả xác minh hợp lệ và đủ mức độ bao phủ.

---

## 6. Phân loại mức độ nghiêm trọng

### 6.1. Nguyên tắc

- Risk do OWASP ZAP cung cấp là mức độ nguồn, không phải quyết định cuối cùng.
- Confidence của ZAP thể hiện độ tin cậy, không phải mức độ nghiêm trọng.
- Hệ thống không tự xác nhận kết quả chỉ vì báo cáo được tiếp nhận thành công.
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

OWASP ZAP cung cấp Risk nguồn gồm Informational, Low, Medium và High. Hệ thống không tự động đổi High thành Critical. Final Severity chỉ được nâng thành Critical sau khi chuyên viên ATTT đánh giá tác động và bối cảnh thực tế.

### 6.4. Yếu tố đánh giá bối cảnh

- Tài sản có công khai Internet hay không.
- Môi trường production hay thử nghiệm.
- Mức độ quan trọng của tài sản.
- Loại và độ nhạy cảm của dữ liệu.
- Khả năng khai thác thực tế.
- Phạm vi người dùng hoặc chức năng bị ảnh hưởng.
- Kiểm soát giảm thiểu đang có.
- Chất lượng bằng chứng và Confidence của ZAP.
- Mức độ bao phủ và tính hợp lệ của lần quét tự động.

### 6.5. Liên kết với thời hạn xử lý

Thời hạn xử lý được xác định từ Final Severity theo chính sách của đơn vị. Hệ thống lưu bảng thời hạn được phê duyệt nhưng không tự đặt một bộ thời hạn ngoài yêu cầu của đơn vị. Khi mức độ thay đổi, hệ thống lưu cả thời hạn cũ, thời hạn mới và lý do.

---

## 7. Workflow xử lý

### 7.1. Trạng thái phát hiện

| Trạng thái | Ý nghĩa | Chủ thể chính |
| --- | --- | --- |
| New | Phát hiện mới được xử lý từ báo cáo tự động, chờ phân tích | Chuyên viên ATTT |
| Triaged | Đã xác nhận kết quả và mức độ | Chuyên viên ATTT |
| Assigned | Đã phân công và có thời hạn | Chuyên viên ATTT |
| In Progress | Đang khắc phục | Nhóm khắc phục |
| Pending Verification | Đã xử lý, chờ quét xác minh tự động | Hệ thống và ATTT |
| Closed | Kết quả xác minh hợp lệ cho thấy không còn lỗ hổng | Chuyên viên ATTT |
| Reopened | Lỗ hổng còn tồn tại hoặc xuất hiện lại | ATTT và nhóm khắc phục |
| False Positive | Được xác nhận là kết quả sai | Chuyên viên ATTT |
| Pending Risk Acceptance | Chờ quyết định chấp nhận rủi ro | Người phê duyệt rủi ro |
| Risk Accepted | Rủi ro được chấp nhận có thời hạn | Chủ tài sản và người phê duyệt |
| Cancelled | Bản ghi sai hoặc ngoài phạm vi | Chuyên viên ATTT |

### 7.2. Ma trận chuyển trạng thái

| Từ | Sang | Người hoặc tác nhân thực hiện | Điều kiện bắt buộc |
| --- | --- | --- | --- |
| Báo cáo hợp lệ | New | Hệ thống | Báo cáo đúng nguồn, đúng phạm vi, xử lý thành công và chưa trùng |
| New | Triaged | Chuyên viên ATTT | Xác nhận tài sản, vị trí, bằng chứng và Final Severity |
| New | False Positive | Chuyên viên ATTT | Có lý do và bằng chứng; Critical/High cần rà soát thứ hai |
| New | Cancelled | Chuyên viên ATTT | Bản ghi sai hoặc ngoài phạm vi; không xóa báo cáo gốc |
| Triaged | Assigned | Chuyên viên ATTT | Có người phụ trách và thời hạn |
| Assigned | In Progress | Nhóm khắc phục | Xác nhận tiếp nhận |
| In Progress | Pending Verification | Nhóm khắc phục | Có biện pháp và bằng chứng khắc phục |
| Pending Verification | Closed | Chuyên viên ATTT | Báo cáo xác minh hợp lệ, đủ bao phủ và không còn dấu vân tay phát hiện |
| Pending Verification | Reopened | Chuyên viên ATTT | Kết quả xác minh còn lỗ hổng, không hợp lệ hoặc không đủ bao phủ |
| Closed | Reopened | Hệ thống hoặc ATTT | Dấu vân tay xuất hiện trong báo cáo ZAP mới và đáp ứng quy tắc mở lại |
| Triaged, Assigned, In Progress | Pending Risk Acceptance | Chủ tài sản hoặc ATTT | Có lý do, ảnh hưởng, biện pháp giảm thiểu và ngày hết hạn đề nghị |
| Pending Risk Acceptance | Risk Accepted | Người phê duyệt rủi ro | Đúng thẩm quyền, có điều kiện và ngày hết hạn |
| Pending Risk Acceptance | Trạng thái trước | Người phê duyệt rủi ro | Từ chối hoặc yêu cầu tiếp tục khắc phục |
| Risk Accepted | Reopened | Hệ thống hoặc ATTT | Hết hạn, vi phạm điều kiện hoặc rủi ro tăng |
| Reopened | Assigned | Chuyên viên ATTT | Phân công lại và xác định thời hạn mới |

Mọi chuyển trạng thái phải ghi trạng thái trước, trạng thái sau, tác nhân thực hiện, thời điểm, nhận xét và bằng chứng liên quan. Chuyển trạng thái do hệ thống thực hiện phải truy vết đến quy tắc và dữ liệu nguồn.

### 7.3. Kết quả sai

1. Chuyên viên ATTT nhập lý do và bằng chứng xác minh.
2. Với mức Critical hoặc High, người rà soát thứ hai xác nhận.
3. Phát hiện chuyển sang False Positive nhưng không bị xóa.
4. Nếu dấu vân tay xuất hiện lại, hệ thống tạo lần xuất hiện mới và yêu cầu rà soát lại; không tự bỏ qua vĩnh viễn.

### 7.4. Chấp nhận rủi ro

1. Chủ tài sản hoặc chuyên viên ATTT tạo đề nghị.
2. Đề nghị gồm lý do, tác động, biện pháp giảm thiểu, người chịu trách nhiệm và ngày hết hạn.
3. Người có thẩm quyền phê duyệt hoặc từ chối.
4. Hệ thống vẫn ghi nhận `last_seen` khi lỗ hổng tiếp tục xuất hiện trong các báo cáo mới.
5. Khi hết hạn mà chưa khắc phục hoặc gia hạn hợp lệ, phát hiện chuyển sang Reopened.

Gia hạn là quyết định mới; không ghi đè quyết định cũ.

### 7.5. Quét xác minh tự động

1. Nhóm khắc phục cập nhật biện pháp và bằng chứng.
2. Chuyên viên ATTT phê duyệt yêu cầu xác minh.
3. Hệ thống tạo lần quét xác minh gắn với phát hiện và cấu hình đã phê duyệt.
4. Bộ điều phối gọi OWASP ZAP qua API và theo dõi phiên thực thi.
5. Hệ thống tự động tiếp nhận, kiểm tra và xử lý báo cáo mới theo F-03.
6. Hệ thống đối chiếu dấu vân tay và kiểm tra mức độ bao phủ của lần quét.
7. Chuyên viên ATTT đóng nếu không còn lỗ hổng và kết quả đủ tin cậy; mở lại nếu vẫn còn hoặc kết quả không hợp lệ.

Không đóng phát hiện chỉ dựa trên xác nhận của nhóm khắc phục hoặc chỉ vì báo cáo không chứa cảnh báo. Phải xác nhận lần quét đã truy cập đúng phạm vi và hoàn thành các bước cần thiết.

### 7.6. Tình huống ngoại lệ

| Tình huống | Cách xử lý |
| --- | --- |
| Không tạo được lần quét đúng lịch | Ghi sự kiện bỏ lỡ lịch, cảnh báo vận hành và không âm thầm bỏ qua |
| Không kết nối được ZAP | Đánh dấu Failed, che chi tiết nhạy cảm và thông báo nhóm vận hành tích hợp |
| ZAP không phản hồi đúng hạn | Đánh dấu Timed Out; chỉ chạy lại sau khi xác định trạng thái phiên cũ |
| ZAP quét thất bại | Đánh dấu Failed, lưu mã lỗi và không tạo phát hiện từ dữ liệu không hoàn chỉnh |
| Phát hiện vượt phạm vi khi đang quét | Dừng khi có thể, đánh dấu lỗi phạm vi và yêu cầu ATTT rà soát |
| Báo cáo sai định dạng hoặc sai nguồn | Chuyển lô Rejected, không tạo phát hiện |
| Báo cáo không tải được | Giữ trạng thái lỗi tiếp nhận; cho phép thao tác lấy lại có kiểm soát |
| Một phần bản ghi lỗi | Chuyển Partially Completed, giữ bản ghi hợp lệ và hiển thị lỗi |
| URL trong báo cáo ngoài phạm vi | Từ chối bản ghi, cảnh báo và yêu cầu ATTT rà soát |
| Phát hiện trùng | Thêm lần xuất hiện, không tạo phiếu trùng |
| Bằng chứng chứa bí mật | Che khỏi giao diện, log và thông báo; hạn chế quyền xem báo cáo gốc |
| Yêu cầu dừng không có hiệu lực ngay | Tiếp tục giám sát, chặn bước tiếp theo và ghi rõ trạng thái chưa xác định |

---

## 8. Bảo mật báo cáo và tích hợp OWASP ZAP

### 8.1. Đối tượng cần bảo vệ

- API key, token, thông tin xác thực và tham chiếu bí mật.
- Cấu hình context, include/exclude, header xác thực và hồ sơ quét.
- Báo cáo OWASP ZAP gốc.
- Phát hiện đã chuẩn hóa.
- URL, tham số, Attack, Evidence, request, response và thông tin kỹ thuật.
- Bằng chứng khắc phục và kết quả xác minh.
- Tệp báo cáo được xuất từ hệ thống.

Các dữ liệu trên là dữ liệu nhạy cảm vì có thể hỗ trợ truy cập trái phép hoặc khai thác website nếu bị lộ.

### 8.2. Bảo vệ kết nối API

- Chỉ bộ điều phối được phép kết nối tới API ZAP qua mạng riêng hoặc danh sách địa chỉ cho phép.
- API phải yêu cầu khóa xác thực; khóa được lưu trong kho bí mật và không lưu dạng rõ trong cơ sở dữ liệu, hồ sơ quét hoặc mã nguồn.
- Ưu tiên truyền khóa trong header; không ghi khóa vào URL, log, lỗi hoặc báo cáo.
- Kênh truyền phải được bảo vệ bằng TLS hoặc lớp mạng riêng tương đương; có thể bổ sung mTLS khi môi trường hỗ trợ.
- ZAP không được công khai API trực tiếp ra Internet.
- Tài khoản dịch vụ và khóa tích hợp phải có vòng đời, thời hạn, xoay vòng và thu hồi.
- Hệ thống phải giới hạn timeout, kích thước phản hồi và số lần gọi lại.
- Chỉ cho phép tập API cần thiết; không cho người dùng chuyển tiếp lệnh, đường dẫn tệp hoặc script tùy ý.
- Lỗi trả về cho giao diện phải được chuẩn hóa và che thông tin hạ tầng.

### 8.3. Kiểm soát phạm vi và an toàn quét

- Bộ điều phối dựng yêu cầu từ dữ liệu đã phê duyệt, không nhận trực tiếp URL tùy ý từ người dùng.
- Domain, scheme, port, context và biểu thức include/exclude phải được kiểm tra trước và sau khi gửi tới ZAP.
- Active scan mặc định tắt đối với hồ sơ mới.
- Tài sản production phải có khung giờ, thời gian tối đa và giới hạn tải được phê duyệt.
- Cung cấp cơ chế dừng khẩn cấp và tạm dừng toàn bộ lịch tự động.
- Không tự chạy lại khi có khả năng phiên cũ vẫn đang gửi request.
- Mỗi lần quét phải sử dụng ảnh chụp cấu hình bất biến để có thể giải thích kết quả.

### 8.4. Kiểm soát truy cập dữ liệu

- Người dùng phải được xác thực trước khi truy cập.
- Quyền được kiểm tra theo vai trò và phạm vi tài sản.
- Mặc định từ chối khi không có quyền rõ ràng.
- Quyền xem báo cáo gốc tách riêng với quyền xem phát hiện đã được che dữ liệu.
- Chủ sở hữu chỉ xem tài sản của mình; nhóm khắc phục chỉ xem phiếu được giao.
- Nhóm vận hành tích hợp chỉ xem nội dung cần thiết để xử lý sự cố.
- Quản trị viên không mặc nhiên được xem nội dung lỗ hổng.
- Kiểm tra quyền phải thực hiện tại phía máy chủ cho màn hình, API và tệp tải xuống.

### 8.5. Mã hóa, che dữ liệu và tính toàn vẹn

- Mã hóa dữ liệu khi truyền và khi lưu bằng cơ chế được đơn vị phê duyệt.
- Cookie, token, mật khẩu, khóa API và dữ liệu cá nhân không cần thiết phải được che khỏi giao diện và báo cáo xuất.
- Không đưa Attack, Evidence, request/response thô hoặc bí mật vào thông báo và log vận hành thông thường.
- Báo cáo gốc được lưu bất biến và có SHA-256.
- Metadata phải ghi nguồn ZAP, phiên thực thi, thời điểm nhận và phiên bản công cụ.
- Người dùng nghiệp vụ và tài khoản dịch vụ không được sửa hoặc thay thế báo cáo gốc sau khi tiếp nhận.

### 8.6. Xuất và tải báo cáo

- Chỉ người có quyền và đúng phạm vi tài sản mới được xuất hoặc tải.
- Báo cáo xuất áp dụng che dữ liệu theo quyền người nhận.
- Tệp xuất có nhãn dữ liệu nhạy cảm, người xuất, thời điểm và mã truy vết.
- Liên kết tải xuống có thời hạn và có thể thu hồi.
- Không sử dụng liên kết công khai lâu dài.
- Mọi lần tạo, tải và thu hồi báo cáo đều được ghi nhật ký.

### 8.7. Nhật ký bắt buộc

- Tạo, sửa, phê duyệt và thay đổi trạng thái tài sản, phạm vi, hồ sơ ZAP hoặc lịch quét.
- Tạo lần quét tự động, gửi yêu cầu, nhận mã phiên, cập nhật tiến độ, dừng, timeout và lỗi.
- Yêu cầu ZAP sinh báo cáo; tiếp nhận, từ chối và xử lý lô báo cáo.
- Xem hoặc tải báo cáo gốc và bằng chứng nhạy cảm.
- Thay đổi mức độ, người phụ trách và thời hạn.
- Chuyển trạng thái, đóng, mở lại và xác nhận kết quả sai.
- Tạo, phê duyệt, từ chối hoặc gia hạn chấp nhận rủi ro.
- Tạo, tải và thu hồi báo cáo xuất.
- Tạo, xoay vòng, thu hồi tham chiếu bí mật và thay đổi quyền truy cập, không ghi giá trị bí mật.

Nhật ký phải ghi tác nhân, thời điểm, hành động, đối tượng, mã tương quan, kết quả và giá trị trước sau khi phù hợp. Người dùng nghiệp vụ không được sửa nhật ký.

### 8.8. Lưu trữ

- Thời hạn lưu báo cáo gốc, artifact, phát hiện, phiếu và nhật ký tuân theo chính sách của đơn vị.
- Không xóa cứng tài sản, hồ sơ quét đã sử dụng, phát hiện hoặc phiếu đang có giá trị lịch sử.
- Hết thời hạn lưu phải xử lý theo tác vụ được phê duyệt và có nhật ký.
- Dữ liệu tạm tại ZAP hoặc bộ điều phối phải được xóa an toàn sau khi đã xác nhận lưu trữ thành công.

---

## 9. Nội dung bổ trợ trực tiếp

Các nội dung dưới đây không phải phân hệ chức năng độc lập. Chúng chỉ hỗ trợ điều phối quét, workflow và bảo mật báo cáo.

### 9.1. Thông báo tối thiểu

- Lịch quét sắp đến hạn nhưng không đủ điều kiện thực hiện.
- Không tạo được lần quét đúng lịch.
- Kết nối ZAP thất bại, phiên quét timeout, bị dừng hoặc kết thúc lỗi.
- Báo cáo không nhận được, bị từ chối hoặc chỉ xử lý được một phần.
- Có phát hiện Critical hoặc High.
- Phiếu mới được giao, sắp đến hạn hoặc quá hạn.
- Có yêu cầu hoặc kết quả quét xác minh.
- Đề nghị chấp nhận rủi ro chờ phê duyệt hoặc sắp hết hạn.

Thông báo chỉ chứa mã tài sản, mã lần quét hoặc phát hiện, trạng thái và liên kết vào hệ thống; không chứa bằng chứng hay dữ liệu xác thực nhạy cảm.

### 9.2. Báo cáo tối thiểu

- Danh sách phát hiện theo tài sản, mức độ và trạng thái.
- Lịch quét và trạng thái các lần quét tự động.
- Tỷ lệ phiên thực thi thành công, lỗi, timeout hoặc bị dừng.
- Lô tiếp nhận thành công, lỗi hoặc bị từ chối.
- Phiếu đang xử lý, sắp đến hạn và quá hạn.
- Phát hiện đã đóng, mở lại, kết quả sai hoặc chấp nhận rủi ro.

Không xây dựng phân hệ BI hoặc dashboard phân tích ngoài dữ liệu cần thiết để theo dõi vận hành và xử lý lỗ hổng.

---

## 10. Dữ liệu nghiệp vụ cốt lõi

| Thực thể | Mục đích |
| --- | --- |
| UserRole | Vai trò và phạm vi tài sản, phục vụ phân quyền |
| ServiceIdentity | Định danh kỹ thuật không đăng nhập dùng cho luồng tích hợp |
| Asset, AssetScope | Tài sản, phiên bản phạm vi và giới hạn quét |
| ZapConnection | Metadata kết nối ZAP và tham chiếu bí mật; không chứa giá trị bí mật dạng rõ |
| ZapScanProfile | Hồ sơ quét ZAP có phiên bản và trạng thái phê duyệt |
| ScanSchedule, ScanOccurrence | Lịch và từng lần quét được hệ thống tạo |
| ZapExecution | Trạng thái kỹ thuật, mã phiên, snapshot cấu hình, timeout và lỗi đã che |
| ZapArtifact | Metadata báo cáo hoặc artifact bất biến nhận từ ZAP |
| ImportBatch, RawReport, ImportError | Lô tiếp nhận, báo cáo ZAP gốc và lỗi xử lý |
| Finding, FindingOccurrence | Phát hiện chuẩn hóa và các lần xuất hiện |
| SeverityAssessment | Lịch sử xác nhận và điều chỉnh mức độ |
| RemediationTicket, Assignment | Phiếu xử lý và phân công |
| Evidence, VerificationRequest | Bằng chứng và yêu cầu xác minh |
| RiskAcceptance | Đề nghị và quyết định chấp nhận rủi ro |
| AuditLog | Nhật ký cho hành động người dùng và hành động tự động nhạy cảm |

Không bổ sung thực thể quản lý công cụ quét đa nhà cung cấp, CI/CD, kho mã nguồn, email, SSO hoặc quản lý công việc ngoài phạm vi.

---

## 11. Quy tắc nghiệp vụ tổng hợp

| Mã | Quy tắc |
| --- | --- |
| BR-01 | Chỉ tài sản Active và phạm vi đã phê duyệt mới được kích hoạt lịch quét |
| BR-02 | Hệ thống điều phối OWASP ZAP qua API; OWASP ZAP chịu trách nhiệm thực hiện quét kỹ thuật |
| BR-03 | Chỉ lịch Active gắn với hồ sơ ZAP Active mới được tự động tạo lần quét |
| BR-04 | Mọi target gửi tới ZAP phải nằm trong phạm vi và khung giờ đã phê duyệt |
| BR-05 | Active scan chỉ được thực hiện khi tài sản và hồ sơ quét cho phép rõ ràng |
| BR-06 | Mỗi kỳ lịch chỉ tạo tối đa một lần quét nghiệp vụ |
| BR-07 | Không tự chạy lại một active scan khi trạng thái phiên trước chưa được xác định |
| BR-08 | Kết quả chỉ được xử lý từ báo cáo OWASP ZAP JSON hoặc XML nhận qua luồng tích hợp hợp lệ |
| BR-09 | Mỗi kết quả phải truy vết tới tài sản, lần quét, phiên ZAP, lô tiếp nhận và báo cáo gốc |
| BR-10 | Báo cáo gốc bất biến và có kiểm tra toàn vẹn |
| BR-11 | Tiếp nhận lại cùng báo cáo không tạo dữ liệu trùng |
| BR-12 | Phát hiện xuất hiện lại sau khi đóng phải được mở lại theo quy tắc workflow |
| BR-13 | Risk, Confidence và Final Severity là các trường riêng |
| BR-14 | Điều chỉnh Final Severity phải có lý do và lịch sử |
| BR-15 | Nhóm khắc phục không tự đóng phát hiện |
| BR-16 | Chấp nhận rủi ro có người phê duyệt độc lập và ngày hết hạn |
| BR-17 | Bí mật trong cấu hình và báo cáo ZAP phải được che trước khi hiển thị, ghi log hoặc xuất |
| BR-18 | Mọi lời gọi API, truy cập báo cáo gốc và chuyển trạng thái quan trọng phải được ghi nhật ký |
| BR-19 | Tài khoản dịch vụ không được đăng nhập giao diện hoặc phê duyệt nghiệp vụ |
| BR-20 | Không cho phép chuyển tiếp script, Automation Plan hoặc lệnh tùy ý từ người dùng tới ZAP |
| BR-21 | Không xóa cứng dữ liệu có giá trị truy vết |
| BR-22 | Nội dung hỗ trợ không được phát triển thành phân hệ ngoài bốn chức năng chính |

---

## 12. Ma trận truy vết yêu cầu

| Yêu cầu | Phần tài liệu đáp ứng |
| --- | --- |
| Khai báo tài sản và phạm vi | Mục 5.1 |
| Lịch và điều phối quét tự động | Mục 5.2 |
| Điều khiển OWASP ZAP qua API | Mục 1.3, 4, 5.2 và 13 |
| Tự động sinh và tiếp nhận báo cáo | Mục 5.3 |
| Theo dõi khắc phục | Mục 5.4 |
| Phân loại mức độ nghiêm trọng | Mục 6 |
| Workflow xử lý và quét xác minh | Mục 7 |
| Bảo mật API và báo cáo lỗ hổng | Mục 8 |
| Đối tượng sử dụng và tác nhân tích hợp | Mục 3 |
| Dữ liệu, nhật ký và thông báo cần thiết | Mục 9, 10 và 11 |

---

## 13. Phụ lục tích hợp OWASP ZAP

### 13.1. Phương án tích hợp

Luồng tích hợp chuẩn được thực hiện như sau:

1. Bộ điều phối tạo cấu hình hoặc Automation Framework Plan từ hồ sơ đã phê duyệt.
2. Hệ thống gọi API ZAP để chạy kế hoạch hoặc khởi tạo các bước spider và scan tương ứng.
3. Hệ thống nhận `planId`, `scanId` hoặc mã phiên và lưu cùng `ZapExecution`.
4. Hệ thống lấy tiến độ theo chu kỳ có giới hạn; không tạo yêu cầu quét trùng.
5. Khi hoàn thành, hệ thống gọi Report Generation API để tạo Traditional JSON Report và báo cáo đọc nếu cần.
6. Hệ thống nhận báo cáo qua kênh được bảo vệ, tính SHA-256 và lưu artifact bất biến.
7. Hệ thống tạo lô `Received` để F-03 kiểm tra, chuẩn hóa và chống trùng.
8. Dữ liệu tạm trên ZAP được dọn dẹp sau khi lưu trữ thành công theo chính sách.

Việc ZAP thực hiện quét không đồng nghĩa ZAP được quyền quyết định trạng thái nghiệp vụ. ZAP chỉ cung cấp tiến độ, cảnh báo và báo cáo; hệ thống quản lý chịu trách nhiệm workflow và chuyên viên ATTT chịu trách nhiệm quyết định chuyên môn.

### 13.2. Nguyên tắc triển khai

- Ưu tiên một phiên hoặc môi trường ZAP biệt lập cho từng lần quét để tránh trộn dữ liệu giữa tài sản.
- Khóa phiên bản ZAP, add-on và mẫu báo cáo đã kiểm thử.
- Chỉ tạo cấu hình từ trường dữ liệu cho phép; không nhận YAML hoặc script tùy ý.
- Không cho ZAP kết nối cơ sở dữ liệu ứng dụng.
- Tách tài khoản dịch vụ của hệ thống khỏi tài khoản người dùng.
- Kiểm thử trước trên môi trường staging và bắt đầu bằng passive scan.
- Bật active scan theo từng hồ sơ sau khi đánh giá tải và có phê duyệt.
- Theo dõi timeout, tải hệ thống đích, dung lượng báo cáo và hàng đợi.
- Cung cấp cơ chế tạm dừng lịch tự động và dừng khẩn cấp.
- Việc nâng phiên bản ZAP hoặc add-on phải tạo phiên bản hồ sơ mới hoặc có biên bản kiểm thử tương thích.

### 13.3. Tài liệu chính thức

| Tài liệu | Mục đích |
| --- | --- |
| [ZAP Documentation](https://www.zaproxy.org/docs/) | Trang tài liệu tổng hợp |
| [ZAP API](https://www.zaproxy.org/docs/desktop/start/features/api/) | Khả năng tương tác với ZAP bằng JSON, HTML hoặc XML và các giới hạn bảo mật API |
| [ZAP Automation Framework](https://www.zaproxy.org/docs/desktop/addons/automation-framework/) | Chạy kế hoạch, theo dõi tiến độ và điều phối các job quét |
| [Report Generation API](https://www.zaproxy.org/docs/desktop/addons/report-generation/api/) | Sinh báo cáo bằng API |
| [Report Generation Automation](https://www.zaproxy.org/docs/desktop/addons/report-generation/automation/) | Cấu hình job tạo báo cáo trong Automation Framework |
| [ZAP Alerts](https://www.zaproxy.org/docs/desktop/start/features/alerts/) | Trường Risk, Confidence, URL, Parameter, Evidence, CWE và WASC |
| [ZAP Alert Catalogue](https://www.zaproxy.org/docs/alerts/) | Danh mục Alert ID và quy tắc phát hiện |
| [Report Templates](https://www.zaproxy.org/docs/desktop/addons/report-generation/templates/) | Mẫu Traditional JSON và Traditional XML |

Ngày đối chiếu tài liệu: 14/09/2026. Khi triển khai, phải ghi nhận phiên bản OWASP ZAP, add-on, API và mẫu báo cáo đã kiểm thử để tránh thay đổi không được kiểm soát.

---

## 14. Tiêu chí hoàn thành phân tích

Tài liệu đủ đầu vào cho bước thiết kế và triển khai khi các bên xác nhận:

- Trường dữ liệu, trạng thái và phạm vi tài sản.
- Cấu trúc, tần suất, múi giờ và trạng thái lịch quét.
- Cơ chế chống tạo trùng lần quét khi lịch đến hạn.
- Cấu trúc, phiên bản và quy trình phê duyệt hồ sơ quét ZAP.
- Danh sách API được phép sử dụng, cơ chế xác thực, timeout và dừng khẩn cấp.
- Cách theo dõi trạng thái ZAP và xử lý trạng thái không xác định.
- Mẫu báo cáo OWASP ZAP dùng làm nguồn xử lý.
- Quy tắc tiếp nhận, kiểm tra, lưu bất biến, ánh xạ và chống trùng kết quả.
- Quy tắc xác nhận Final Severity.
- Trạng thái và điều kiện chuyển của workflow.
- Quy tắc xác minh mức độ bao phủ trước khi đóng phát hiện.
- Quyền xem, tải, xuất và yêu cầu che dữ liệu báo cáo.
- Thẩm quyền dừng quét, chạy lại, đóng, mở lại, xác nhận kết quả sai và chấp nhận rủi ro.

Các bước tiếp theo gồm xây dựng hoặc cập nhật use case, sơ đồ trạng thái, sơ đồ tuần tự tích hợp ZAP, mô hình dữ liệu, hợp đồng API nội bộ, đặc tả bộ tiếp nhận báo cáo và giao diện cho bốn chức năng chính.

---

## 15. Kết luận

Hệ thống được giới hạn trong bốn chức năng: khai báo tài sản, quản lý lịch và điều phối quét, tự động tiếp nhận kết quả OWASP ZAP, và theo dõi khắc phục. Phân loại mức độ nghiêm trọng, bảo mật báo cáo, bảo vệ kết nối API và workflow xử lý được thiết kế xuyên suốt bốn chức năng này.

OWASP ZAP là hệ thống quét kỹ thuật bên ngoài và được hệ thống quản lý điều phối qua API. Các thao tác theo lịch, theo dõi tiến độ, sinh báo cáo, tiếp nhận kết quả và khởi chạy quét xác minh được tự động hóa. Nhóm vận hành tích hợp chỉ quản lý cấu hình và xử lý ngoại lệ; nhóm khắc phục sửa lỗi; chuyên viên ATTT vẫn giữ quyền xác nhận chuyên môn và quyết định đóng hoặc mở lại phát hiện.

Tự động hóa không làm giảm yêu cầu kiểm soát. Mọi lần quét phải đúng phạm vi, đúng cấu hình, đúng thời gian, có khả năng dừng và truy vết đầy đủ. Báo cáo gốc phải được bảo vệ, lưu bất biến và liên kết được với phiên thực thi ZAP. Phân quyền, nhật ký, thông báo và báo cáo tối thiểu chỉ tồn tại để bảo vệ dữ liệu và hỗ trợ workflow, không mở rộng hệ thống ra ngoài phạm vi đề tài.
