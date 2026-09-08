# Đánh giá tài liệu đặc tả và bộ sơ đồ Use Case / Activity

> **Bản đánh giá trước hiệu chỉnh, giữ để truy vết.** Bộ tài liệu đã được sửa theo yêu cầu tiếp theo của người dùng. Xem [đặc tả hiện hành](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/dinh_dang_use_case_activity.md) và [kết quả kiểm tra sau sửa](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/24_kiem_tra_va_truy_vet.md). Số dòng và kết luận bên dưới áp dụng cho bản trước sửa, không phải bộ mã hiện hành.

Ngày đánh giá: 04/09/2026.

**Kết luận: Chưa đạt để chốt bộ sơ đồ làm căn cứ thiết kế hoặc nghiệm thu.** Phạm vi tổng thể đúng, nhưng còn sai luồng điều khiển, thiếu một số yêu cầu có sẵn trong tài liệu nguồn và chưa thống nhất mã truy vết. Tài liệu đặc tả là bản nháp có nhiều nội dung phù hợp; các đoạn PlantUML trong chính tài liệu đó cũng cần sửa.

## 1. Căn cứ và giới hạn đánh giá

- **Căn cứ nghiệp vụ duy nhất:** [doi_tuong_sd_quy_trinh_hd.md](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md), phiên bản 2.2.
- **Tài liệu cần đánh giá:** [dinh_dang_use_case_activity.md](C:/Users/Admin/Downloads/dinh_dang_use_case_activity.md).
- **Bộ mã cần đánh giá:** toàn bộ 8 tệp chứa PlantUML và README trong [visual_paradigm_uml_md](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md).

Những câu “cần xây dựng”, “tiêu chí nghiệm thu”, “không mở rộng” trong tài liệu đặc tả được xem là nội dung cần kiểm tra. Chúng không tự trở thành yêu cầu nghiệp vụ mới nếu không có căn cứ trong tài liệu nguồn.

Đã đọc và đối chiếu nội dung, điều kiện chuyển trạng thái, trách nhiệm tác nhân và các đường đi trong mã. Đã kiểm tra sơ bộ cặp bắt đầu/kết thúc, cặp `if/endif` và tham chiếu bí danh Use Case. **Chưa chạy trình dựng PlantUML hoặc kiểm tra hình trong Visual Paradigm**, nên chưa kết luận về khả năng biên dịch, bố cục, đường nối chồng chéo hoặc chất lượng ảnh xuất.

Tài liệu kỹ thuật bên ngoài chỉ dùng kiểm tra cách biểu diễn UML/PlantUML, không dùng bổ sung chức năng cho dự án. Trong PlantUML, hành động được nối theo thứ tự khai báo; nhánh điều kiện phải biểu diễn được đường tiếp tục hoặc kết thúc tương ứng. Đây là cơ sở đọc các lỗi đường đi dưới đây. [Tài liệu Activity Diagram của PlantUML](https://plantuml.com/activity-diagram-beta).

## 2. Mức đáp ứng hiện tại

| Nội dung | Đánh giá | Căn cứ chính |
|---|---|---|
| Bốn chức năng F-01–F-04 | Đạt về phân nhóm | Bộ mã có đủ khai báo tài sản, lịch quét, nhập ZAP, khắc phục |
| Ranh giới với ZAP | Đạt | Quét nằm ngoài hệ thống; chưa thấy chức năng điều khiển ZAP hoặc nhập công cụ khác |
| Bảy vai trò nghiệp vụ | Đạt về danh sách | Use Case khai báo đủ 7 actor |
| Quyền và phân tách nhiệm vụ | Đạt một phần | Đúng vai trò đóng phát hiện, nhưng thiếu người thực hiện rà soát thứ hai và một số quyền xem/xuất |
| Use Case và quan hệ | Chưa đạt | Có quan hệ sai nghĩa, chức năng chưa được biểu diễn và mã UC không khớp đặc tả |
| Activity cho luồng thuận | Đạt một phần | Đã thể hiện phần lớn chuỗi nghiệp vụ chính |
| Activity cho điều kiện và ngoại lệ | Chưa đạt | Có đường đi bỏ qua điều kiện bắt buộc; thiếu xác minh không hợp lệ, tái xuất hiện False Positive và một số tình huống rủi ro |
| Bảo mật báo cáo | Chưa đạt độ bao phủ | Có masking và lưu bản gốc, nhưng thiếu quy trình truy cập/xuất/tải/thu hồi có kiểm soát |
| Truy vết nguồn → đặc tả → sơ đồ | Chưa đạt | Cùng mã UC nhưng khác ý nghĩa; thiếu bảng ánh xạ sang tệp và nhánh cụ thể |

Không phát hiện phân hệ mới rõ ràng ngoài phạm vi, như CI/CD, đa công cụ quét, SSO hoặc BI. Các package “Chấp nhận rủi ro”, “Kiểm soát và bảo mật” tự chúng không phải mở rộng chức năng: nguồn đã yêu cầu các nội dung này ở mức hỗ trợ. Cần ghi rõ quan hệ hỗ trợ F-01–F-04 để tránh diễn giải thành phân hệ độc lập.

Tài liệu đặc tả liệt kê **59 UC**; tệp Use Case có **45 phần tử usecase**. Thư mục có **1 Use Case Diagram và 7 Activity Diagram**. Chênh lệch số lượng không đồng nghĩa thiếu đúng 14 chức năng: một số UC được gộp, đổi tên hoặc đổi nhóm. Việc đánh giá phải dựa trên hành vi và quyền, không dựa vào số hình hay số ellipse.

## 3. Các lỗi cần sửa trước khi chốt

### 3.1. Thiếu bằng chứng vẫn đi tiếp đến yêu cầu xác minh — ưu tiên cao

**Vị trí:** [Activity khắc phục, dòng 29](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md:29); [Activity tổng thể, dòng 97](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/02_activity_tong_the.md:97).

Ở sơ đồ chi tiết, nhánh “Không có biện pháp và bằng chứng” ghi “Không cho chuyển trạng thái”, yêu cầu bổ sung rồi nhập lại vào luồng chung tại `endif`. Sau đó vẫn tạo `VerificationRequest`, mặc dù chưa kiểm tra lại và chưa chuyển sang `Pending Verification`. Sơ đồ tổng thể cũng cho nhánh thiếu dữ liệu đi tiếp.

**Nguồn:** mục 5.4.2; mục 7.2, chuyển `In Progress → Pending Verification`; mục 7.5. [Điều kiện chuyển trạng thái, dòng 436](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:436).

**Sửa trong phạm vi:** bổ sung → kiểm tra lại; chỉ nhánh đủ biện pháp và bằng chứng được chuyển `Pending Verification` rồi tạo yêu cầu xác minh. Nếu kết thúc lần thao tác khi thiếu dữ liệu, phải thể hiện rõ giữ nguyên trạng thái và việc gửi lại được kiểm tra từ đầu.

### 3.2. Chỉ yêu cầu rà soát thứ hai rồi tự đi tiếp — ưu tiên cao

**Vị trí:** [Activity Final Severity, dòng 22](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/06_activity_phan_tich_final_severity.md:22) và dòng 42–51.

- Nhánh False Positive của Critical/High chỉ “Yêu cầu rà soát thứ hai”, sau đó chuyển ngay `False Positive`.
- Nhánh hạ Critical/High lưu Final Severity trước khi yêu cầu người rà soát thứ hai, rồi vẫn chuyển `Triaged`.
- Không có hành động của người rà soát, kết quả đồng ý/không đồng ý hoặc dữ liệu người/thời điểm rà soát.

**Nguồn:** mục 6.1, 6.2; mục 7.2 và 7.3. [Yêu cầu rà soát thứ hai, dòng 450](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:450).

**Sửa trong phạm vi:** phân biệt giá trị đề xuất với kết quả được xác nhận; khi cần rà soát, chờ người có thẩm quyền xác nhận và lưu người/thời điểm rà soát trước khi áp dụng kết quả. Nếu chưa được chấp thuận thì tiếp tục rà soát, không tự chuyển sang kết quả cuối. Không cần tự đặt thêm trạng thái Finding mới. Với hạ mức độ, giữ điều kiện “theo chính sách”; với False Positive Critical/High, nguồn yêu cầu rà soát thứ hai.

### 3.3. Chưa chặn đóng phát hiện khi báo cáo xác minh không hợp lệ — ưu tiên cao

**Vị trí:** [Activity khắc phục, dòng 44](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/07_activity_khac_phuc_va_xac_minh.md:44); [Activity tổng thể, dòng 106](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/02_activity_tong_the.md:106).

Mã xử lý báo cáo rồi chỉ hỏi fingerprint còn hay không. Không có bước gọi rõ quy trình nhập F-03 và nhánh kết quả xác minh không hợp lệ. Nếu đối chiếu không có dữ liệu vì nhập lỗi, đường “không còn fingerprint” có thể bị hiểu thành được đóng.

**Nguồn:** mục 7.2 quy định `Pending Verification → Reopened` khi còn lỗ hổng **hoặc kết quả không hợp lệ**; mục 7.5 yêu cầu báo cáo ZAP mới được nhập theo F-03. [Nguồn, dòng 437](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:437).

**Sửa trong phạm vi:** nhập báo cáo xác minh theo F-03 → kiểm tra kết quả hợp lệ và liên kết đúng lần quét xác minh → đối chiếu fingerprint → ATTT quyết định. Kết quả không hợp lệ phải đi nhánh ATTT mở lại theo nguồn. Ở sơ đồ tổng thể, cũng cần có hành động ATTT xác nhận trước bước hệ thống chuyển `Reopened` do xác minh thất bại; quyền hệ thống tự mở lại khi **Closed xuất hiện trong báo cáo mới** là trường hợp khác.

### 3.4. Luồng chấp nhận rủi ro thiếu các điều kiện bắt buộc — ưu tiên cao

**Vị trí:** [Activity chấp nhận rủi ro, dòng 18](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/08_activity_chap_nhan_rui_ro.md:18).

Mã có kiểm tra người tạo khác người duyệt và có nhập ngày hết hạn, nhưng còn các khoảng trống:

- “Kiểm tra dữ liệu bắt buộc” không có nhánh thất bại.
- Chưa biểu diễn kiểm tra thẩm quyền và điều kiện quyết định ngay trước khi phê duyệt.
- Yêu cầu bổ sung → bổ sung → kết thúc, chưa có đường gửi lại/rà soát lại.
- Sau phê duyệt chỉ kiểm tra hết hạn một lần; nếu chưa hết hạn thì kết thúc, chưa thể hiện lần kiểm tra tiếp theo hoặc quy trình riêng được kích hoạt khi đến hạn.
- Chỉ xử lý hết hạn; thiếu **vi phạm điều kiện hoặc rủi ro tăng**.
- Có “Áp dụng quyết định mới”, nhưng chưa có luồng tạo/phê duyệt gia hạn và giữ nguyên quyết định cũ.

**Nguồn:** mục 3.4; mục 7.2 và 7.4. [Các điều kiện rủi ro, dòng 440](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:440); [Gia hạn là quyết định mới, dòng 463](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:463).

**Sửa trong phạm vi:** bổ sung các guard và đường quay lại; biểu diễn việc kiểm tra khi đến hạn hoặc tham chiếu một Activity xử lý sự kiện có cùng nghiệp vụ nguồn. ATTT có thể xác nhận vi phạm/rủi ro tăng; không tự bổ sung công cụ giám sát hay thuật toán phát hiện rủi ro mới. Chỉ xử lý hết hạn đối với quyết định còn áp dụng, có xét việc khắc phục hoặc gia hạn hợp lệ.

### 3.5. Chống trùng lô nhập và trạng thái kết thúc chưa nhất quán — ưu tiên cao

**Vị trí:** [Activity nhập báo cáo, dòng 13](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md:13), dòng 44–52 và 77–83.

Mã tạo ImportBatch, lưu RawReport, chuyển `Processing`, sau đó mới phát hiện báo cáo trùng và dừng với câu “Không tạo lô / Finding trùng”. Chưa thể hiện cách tránh giữ lại lô vừa tạo hoặc liên kết về lô đã có. Tại nhánh không có quyền, lô đã được tạo ở `Validating` nhưng cũng kết thúc mà không có cách xử lý bản ghi đó.

**Nguồn:** mục 5.3.5 quy định khóa lô gồm **tài sản + lần quét + checksum**, nhập lại không tạo lô mới. [Nguồn, dòng 314](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:314).

**Sửa trong phạm vi:** thể hiện rõ kiểm tra khóa trùng và tái sử dụng kết quả lô cũ trước khi lưu một lô nghiệp vụ mới, hoặc làm rõ bản ghi tạm không được giữ thành lô mới. Không tự thêm trạng thái ngoài danh sách nguồn. Các nhánh dừng phải có hậu điều kiện rõ ràng và ghi nhật ký thao tác theo nguồn.

### 3.6. Nhập báo cáo chưa bao phủ đúng lỗi và phát hiện tái xuất hiện — ưu tiên cao

**Vị trí:** [Activity nhập báo cáo, dòng 38](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/05_activity_nhap_bao_cao_owasp_zap.md:38), dòng 54–83.

| Tình huống | Mã hiện tại | Yêu cầu đối chiếu / hướng sửa |
|---|---|---|
| Lỗi kỹ thuật khi xử lý | Không có nhánh ImportBatch `Failed` | Bổ sung nhánh lỗi kỹ thuật, ghi kết quả và nhật ký; mục 5.3.6 |
| Có bản ghi lỗi | Luôn `Partially Completed` | Trạng thái này đòi hỏi có cả thành công và lỗi; không áp dụng vô điều kiện cho trường hợp toàn bộ bản ghi lỗi |
| False Positive xuất hiện lại | Chỉ thêm occurrence và `last_seen` như mọi trạng thái khác ngoài Closed | Phải yêu cầu rà soát lại; mục 7.3 |
| URL ngoài phạm vi | Từ chối cả lô, không có bước yêu cầu ATTT rà soát | Bổ sung nhánh bản ghi ngoài phạm vi và yêu cầu rà soát theo mục 7.6; phân biệt lỗi lô với lỗi bản ghi |
| Báo cáo hợp lệ không có Alert | `repeat` đọc một Alert trước khi kiểm tra còn Alert | Dùng điều kiện trước vòng lặp để không giả định luôn có ít nhất một Alert; đặc biệt cần cho báo cáo xác minh không còn phát hiện |
| Tạo fingerprint | Chỉ ghi tên thao tác | Gắn note/tham chiếu công thức ở mục 5.3.5; không dùng Attack/Evidence |

Nguồn liên quan: [trạng thái lô, dòng 328](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:328), [False Positive tái xuất hiện, dòng 453](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:453), [ngoại lệ, dòng 480](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:480).

Điểm đã đúng: mã tạo occurrence cho Finding mới; Closed tái xuất hiện chuyển Reopened; nhánh tổng quát vẫn cập nhật `last_seen` cho Risk Accepted. Không cần thêm lại các hành vi đã có này, chỉ cần ghi rõ truy vết và ngoại lệ.

### 3.7. Use Case dùng sai quan hệ và diễn đạt quyền chưa đủ rõ

**Vị trí:** [Use Case tổng thể, dòng 109](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/01_use_case_tong_the.md:109), dòng 123–144.

| Quan hệ / biểu diễn | Vấn đề | Cách sửa |
|---|---|---|
| `Xuất/tải báo cáo include Xem nhật ký` | Xuất báo cáo cần ghi nhật ký, không bắt người dùng thực hiện xem nhật ký | Đổi hành vi được include thành ghi nhật ký; giữ UC xem nhật ký riêng |
| `Phân tích phát hiện include Xác nhận Final Severity` | Mâu thuẫn với luồng phân loại có thể kết thúc bằng False Positive/Cancelled | Thể hiện xác nhận severity trên nhánh phát hiện hợp lệ; mô tả điều kiện trong đặc tả hoặc điều chỉnh cách tách UC |
| `Chuyển chờ xác minh include Cập nhật biện pháp và bằng chứng` | Nguồn yêu cầu kiểm tra dữ liệu đã có, không bắt cập nhật lại ở mọi lần chuyển | Đổi thành kiểm tra đủ biện pháp/bằng chứng, giữ cập nhật là hành vi riêng |
| `Tạo đề nghị rủi ro extend Phân tích phát hiện` | Chỉ gắn với triage, chưa diễn đạt việc đề nghị từ Assigned/In Progress | Dùng UC đề nghị có tiền điều kiện đúng ba trạng thái; chỉ giữ extend nếu xác định đúng điểm mở rộng |
| `Mở lại extend Đánh giá kết quả xác minh` | Phù hợp cho một tình huống, chưa bao phủ tái xuất hiện sau Closed và Risk Accepted | Giữ được nếu giới hạn rõ điều kiện; mô tả thêm các trigger đã có trong nguồn |
| Owner → Phân tích; ScanOps → Yêu cầu xác minh | Association chỉ nói có tham gia, chưa phân biệt hỗ trợ với người quyết định | Ghi rõ Owner cung cấp ảnh hưởng; Ops tiếp nhận/thực hiện yêu cầu; ATTT khởi tạo và quyết định |

`include` và `extend` diễn đạt việc đưa hành vi vào Use Case; chúng không thay cho trình tự “xảy ra trước/sau”. Actor–Use Case association diễn đạt sự tham gia, không tự nó là ma trận cấp quyền. Vì vậy, các đường nối hỗ trợ nêu trên cần được làm rõ, không nên kết luận chỉ dựa vào đường nối rằng actor đã được cấp quyền quyết định. [Hướng dẫn Use Case của Visual Paradigm](https://www.visual-paradigm.com/support/documents/vpuserguide/94/2575/84257_usecasediagr.html).

### 3.8. Bảo mật truy cập, xuất/tải và audit chưa được thể hiện đủ

**Vị trí:** [nhóm kiểm soát trong Use Case, dòng 70](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/01_use_case_tong_the.md:70); toàn bộ danh sách Activity hiện có.

Chưa có Activity hoặc luồng tương đương bao phủ đầy đủ xác thực, role + asset scope, quyền RawReport riêng, từ chối khi không có quyền, masking báo cáo xuất, nhãn dữ liệu nhạy cảm, người xuất/thời điểm/mã truy vết, liên kết có hạn và thu hồi. Nguồn yêu cầu ghi nhật ký cả **tạo, tải, thu hồi**, cùng truy cập báo cáo gốc/bằng chứng nhạy cảm.

Trong Use Case, quyền xuất báo cáo mới nối với Owner, ATTT và Auditor; còn thiếu ScanOps, nhóm khắc phục và người duyệt rủi ro theo phạm vi ở nguồn. Admin nối vào “Xem nhật ký” chung nhưng chưa giới hạn nhật ký thay đổi quyền. Quyền RawReport không nên đồng nhất với vai trò ATTT hoặc với quyền xem Finding đã che dữ liệu.

**Nguồn:** [ma trận quyền, dòng 165](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:165), [kiểm soát truy cập, dòng 505](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:505), [xuất/tải và audit, dòng 523](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:523).

**Sửa trong phạm vi:** bổ sung Activity bảo mật báo cáo hoặc luồng tương đương; ghi rõ phạm vi xem nhật ký theo vai trò. Không tự cấp cho Admin quyền xem nội dung lỗ hổng và không dựng phân hệ báo cáo mới.

Audit đang nằm cuối một số nhánh thành công, nên các nhánh dừng sớm chưa thể hiện đủ nhật ký từ chối/lỗi. Trong sơ đồ khắc phục, audit đóng có thể thấy rõ nhưng audit mở lại, đổi thời hạn và mọi lần chuyển trạng thái chưa được biểu diễn xuyên suốt. Có thể dùng quy tắc chung hoặc hành vi dùng lại, miễn chỉ rõ áp dụng cả nhánh thành công và thất bại.

### 3.9. Chưa đến lịch vẫn đi tới thực hiện quét; chưa diễn đạt lịch định kỳ

**Vị trí:** [Activity lịch quét, dòng 37](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/04_activity_quan_ly_lich_quet.md:37).

Nếu “Đến thời điểm quét?” là Không, luồng vẫn ra khỏi `endif` rồi đi đến “Xem lần quét Planned” và chạy ZAP, trong khi nhánh này chưa tạo lần quét. Sơ đồ cũng kết thúc sau một lần thực hiện, chưa có tham chiếu đến các kỳ tiếp theo hay điều kiện lịch còn Active.

**Nguồn:** mục 5.2.1–5.2.3: tần suất, ngày hiệu lực/kết thúc, múi giờ, tạo lần quét khi đến lịch và trạng thái lịch. [Nguồn, dòng 234](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/doi_tuong_sd_quy_trinh_hd.md:234).

**Sửa trong phạm vi:** nhánh chưa đến hạn quay về chờ hoặc kết thúc lần kiểm tra; nhánh đến hạn và lịch đủ điều kiện mới tạo Planned và giao vận hành. Có thể tách Activity “xử lý một lần quét” với tiền điều kiện Planned; không bắt buộc dùng vòng lặp vô hạn. Giữ bước cập nhật Failed/Missed kèm lý do, vốn đã phù hợp.

### 3.10. Thiếu một số hành vi quản lý đã có trong nguồn

| Hành vi cần thể hiện | Hiện trạng | Nguồn |
|---|---|---|
| Thay đổi phạm vi tài sản phải phê duyệt lại | Có UC cập nhật, nhưng chưa có guard/luồng phê duyệt lại | Mục 5.1.2, dòng 217 |
| ATTT cũng được khai báo tài sản | Use Case có, Activity khai báo chỉ thể hiện Chủ sở hữu là người tạo | Mục 4, dòng 179 |
| Tạm dừng/ngừng tài sản, bảo toàn lịch sử | Có UC gộp, Activity chỉ đến Active | Mục 5.1.2–5.1.3 |
| Cập nhật/tạm dừng lịch và lịch sử thay đổi | Có UC, Activity chỉ tạo/kích hoạt/thực hiện | Mục 5.2.2 |
| Theo dõi sắp đến hạn/quá hạn | Chỉ có đặt thời hạn ban đầu, chưa có luồng theo dõi | Mục 5.4.2 và 9.1 |
| Thay đổi severity gắn thay đổi thời hạn | Chưa có lịch sử thời hạn cũ/mới/lý do | Mục 6.5 |
| Reopened → Assigned | Có “Phân công lại” nhưng chưa nêu thời hạn mới và cập nhật Assigned | Mục 7.2, dòng 444 |
| Cancelled cho bản ghi sai/ngoài phạm vi | Có trong Activity, thiếu mục tiêu tương ứng trong Use Case; lý do/audit chưa rõ | Mục 7.2 và 8.5 |
| Quyền xem dữ liệu của các vai trò, nhất là kiểm toán | Chưa được bao phủ đầy đủ bằng UC xem hoặc ràng buộc phạm vi | Mục 3.2–3.3 |
| Thông báo nghiệp vụ tối thiểu | Chưa thể hiện đủ sự kiện trong mã | Mục 9.1 |

Không phải mỗi dòng cần một sơ đồ mới hay một UC riêng. Có thể dùng luồng phụ, note hoặc bảng truy vết. Thời hạn phải theo chính sách đơn vị; không tự thêm SLA cố định, kênh email hay nền tảng ticket ngoài hệ thống.

### 3.11. Sơ đồ tổng thể chưa kết nối với các nhánh đặc thù

[Activity tổng thể](C:/Users/Admin/Documents/ChatGPT/HTQLHDK/visual_paradigm_uml_md/02_activity_tong_the.md) chưa có điểm rẽ hoặc tham chiếu đến chấp nhận rủi ro từ Triaged/Assigned/In Progress. Các điểm kiểm tra tài sản/phạm vi khi lập lịch chỉ là hành động tuần tự, chưa có đường chặn khi không đạt. Quy trình khắc phục còn thiếu liên kết rõ về Assigned sau mở lại.

Một sơ đồ tổng thể không cần lặp lại mọi chi tiết. Tuy nhiên, khi bỏ chi tiết, cần gọi hoặc tham chiếu rõ Activity chuyên biệt và giữ nguyên điều kiện vào/ra; không để sơ đồ tổng thể thể hiện đường đi trái với sơ đồ chi tiết.

## 4. Những điểm cần sửa trong chính tài liệu đặc tả

### 4.1. Không dùng “Hệ thống” làm actor của chính hệ thống

Ở danh mục UC-I02…UC-I11, UC-R09, UC-C07 và một số UC tự động, cột “Actor chính” ghi “Hệ thống”. Nên đổi cách trình bày thành **xử lý nội bộ của hệ thống**, không suy ra một actor ngoài biên mang tên Hệ thống. Những kiểm tra nội bộ có thể là bước Activity hoặc hành vi được include khi phù hợp. Lane Hệ thống trong Activity là hợp lý. [Vị trí đặc tả, dòng 255](C:/Users/Admin/Downloads/dinh_dang_use_case_activity.md:255).

ZAP là công cụ ngoài và báo cáo được vận hành tải thủ công; không bắt buộc vẽ ZAP như actor trực tiếp của hệ thống quản lý. Note bối cảnh đang dùng trong bộ mã phù hợp với phạm vi này. Khái niệm actor là vai trò bên ngoài tương tác với đối tượng được mô hình hóa. [Visual Paradigm, phần Actor](https://www.visual-paradigm.com/support/documents/vpuserguide/94/2575/84257_usecasediagr.html#actor).

### 4.2. Quan hệ đề xuất và mã mẫu còn sai nghĩa

- [Dòng 397](C:/Users/Admin/Downloads/dinh_dang_use_case_activity.md:397): “Mở lại extend Đóng phát hiện khi xác minh còn lỗ hổng” không đúng mục tiêu. Khi còn lỗ hổng thì không đạt tiền điều kiện đóng; đây là kết quả thay thế trong đánh giá xác minh.
- [Dòng 1048](C:/Users/Admin/Downloads/dinh_dang_use_case_activity.md:1048): mã mẫu include xác nhận severity trong cả phân tích và tạo phiếu; tạo phiếu nên sử dụng kết quả triage đã có theo tiền điều kiện, không hàm ý luôn xác nhận lại severity.
- Dòng 1050: “Đóng/mở lại include Yêu cầu quét xác minh” dùng include để biểu diễn bước xảy ra trước đó. Trường hợp Closed tái xuất hiện trong báo cáo mới và Risk Accepted hết hạn không phải lúc nào cũng tạo yêu cầu xác minh trước khi mở lại.
- [Mã AD-04, dòng 1380](C:/Users/Admin/Downloads/dinh_dang_use_case_activity.md:1380): phần mô tả có kiểm tra lô trùng và lỗi kỹ thuật, nhưng mã mẫu không có nhánh kiểm tra trùng lô hoặc Failed; nhánh Finding mới cũng chưa thể hiện occurrence ban đầu. Không nên lấy mã mẫu này làm bản chuẩn để thay ngược mã hiện có.
- Mã AD-06 và AD-08 vẫn thiếu các điều kiện xác minh/rủi ro nêu ở phần đánh giá; các nhánh “Bổ sung rồi stop” chưa nêu điểm gửi lại.

### 4.3. Đặc tả đã bỏ sót hoặc làm yếu một số quy tắc nguồn

- Chưa nêu rõ yêu cầu **phê duyệt lại khi thay đổi phạm vi tài sản**.
- Có “chống trùng” và “fingerprint”, nhưng chưa chép đầy đủ khóa lô và thành phần fingerprint, gồm HTTP method khi có.
- Chưa bảo toàn rõ mẫu **Traditional JSON / Traditional XML** và giới hạn chỉ tải tệp, không nhận kết quả qua API. Mã không thể hiện thêm API, nên đây là thiếu độ chặt của đặc tả, không phải bằng chứng đã mở rộng API.
- Luồng mở lại/rủi ro chưa bao phủ rõ **rủi ro tăng** và **kết quả xác minh không hợp lệ**.
- Chưa nêu đủ lịch sử thời hạn cũ/mới khi severity thay đổi; nhật ký thu hồi báo cáo xuất còn thiếu trong mô tả xuất/tải.

Các nội dung này đều có trong nguồn ở mục 5.1.2, 5.3, 6.5, 7.2 và 8.4.

### 4.4. Không suy diễn thêm quyền hoặc chuyển trạng thái

[UC-S06, dòng 575](C:/Users/Admin/Downloads/dinh_dang_use_case_activity.md:575) đặt cả `Result Imported` trong danh sách trạng thái cho người vận hành chọn. Cần phân biệt **trạng thái của đối tượng** với **trạng thái được người dùng cập nhật thủ công**. Theo luồng nhập đang đặc tả, Result Imported phải phản ánh kết quả nhập thực tế; không nên diễn đạt thành người vận hành có thể tự đánh dấu mà chưa nhập báo cáo.

[Chuỗi trạng thái, dòng 1710](C:/Users/Admin/Downloads/dinh_dang_use_case_activity.md:1710) vẽ lịch theo `Draft → Active → Paused → Active → Expired → Cancelled`. Nguồn chỉ liệt kê trạng thái lịch, không quy định mọi lịch phải đi theo chuỗi này hoặc Expired phải chuyển Cancelled. Nên dùng danh sách trạng thái/luồng đã có căn cứ; không biến chuỗi minh họa thành ma trận chuyển trạng thái bắt buộc.

### 4.5. Mã UC chưa thống nhất giữa đặc tả và bộ mã

| Mã | Ý nghĩa trong đặc tả | Ý nghĩa trong mã hiện có |
|---|---|---|
| UC-A06 | Ngừng hoạt động tài sản | Xem tài sản |
| UC-S04 | Tạm dừng lịch | Theo dõi lịch đến hạn |
| UC-I06 | Lưu báo cáo gốc bất biến | Che dữ liệu nhạy cảm |
| UC-R01 | Tạo phiếu khắc phục | Phân tích phát hiện |
| UC-R10 | Đóng phát hiện | Yêu cầu quét xác minh |
| UC-C05 | Xuất/tải báo cáo | Xem nhật ký |

Trong mã, bí danh dùng dấu gạch dưới, nhưng khác biệt không chỉ ở dấu phân cách: cùng số đã mang nghĩa khác. Cần chọn một bộ mã thống nhất hoặc có bảng ánh xạ minh bạch; không được dùng ma trận truy vết của đặc tả để diễn giải thẳng mã hiện có.

### 4.6. Số lượng hình là quyết định trình bày

Mục 11 ghi “tối thiểu 08” nhưng liệt kê AD-01…AD-10; mục 38 tiếp tục đề xuất 10 hình. Đây là bất nhất biên tập. Nguồn không bắt buộc đúng 10 Activity hay đúng 6 Use Case Diagram.

Hiện chưa có tệp AD-07, AD-09, AD-10 độc lập. Trong đó, False Positive đã có một phần ở AD-05; mở lại đã có một phần ở AD-04, AD-06, AD-08. Vì vậy, cần bổ sung **hành vi thiếu**, không mặc định phải tạo đủ ba tệp. Riêng quy trình bảo mật báo cáo chưa có Activity tương đương nên nên bổ sung.

## 5. Các chỗ nguồn chưa đủ chi tiết: không tự quyết bằng chức năng mới

1. **Lỗi phạm vi của cả lô và của một bản ghi:** mục 5.3.6 nêu lô ngoài phạm vi là Rejected; mục 7.6 nêu từ chối bản ghi và yêu cầu ATTT rà soát. Có thể phân biệt lô sai tài sản/phạm vi với một số bản ghi sai trong lô, nhưng cách phân biệt cần được ghi nhất quán khi hoàn thiện, không âm thầm loại cả lô trong mọi tình huống.
2. **Không có bản ghi thành công và có lỗi bản ghi:** nguồn định nghĩa Partially Completed là có cả thành công và lỗi, Failed là lỗi kỹ thuật. Không nên tự gán một trạng thái mới hoặc mặc định đây là Failed; cần xác định đó là lỗi cấu trúc/lô hay lỗi xử lý trước khi áp dụng trạng thái nguồn.
3. **R/đề nghị của nhóm khắc phục đối với rủi ro:** ma trận quyền cho phép tham gia đề nghị, nhưng ma trận chuyển trạng thái chỉ nêu Chủ tài sản/ATTT tạo đề nghị chính thức. Có thể thể hiện nhóm khắc phục cung cấp ý kiến/đề xuất; không tự cấp quyền chuyển Pending Risk Acceptance cho nhóm này.
4. **Vòng đời tài sản/lịch ngoài Finding:** nguồn có danh sách trạng thái nhưng chưa có ma trận chi tiết mọi cạnh chuyển. Chỉ mô hình hóa những hành vi đã được mô tả và ghi rõ giả định; không suy ra quyền chuyển tùy ý.

Các điểm chưa rõ này không ngăn việc sửa các lỗi chắc chắn ở phần 3.

## 6. Truy vết 15 quy tắc tổng hợp của nguồn

| Quy tắc | Mức thể hiện trong bộ mã | Cần hoàn thiện |
|---|---|---|
| BR-01 Active và phạm vi đã duyệt | Một phần | Điều kiện chặn ở tổng thể, phê duyệt lại khi đổi scope, kiểm tra điều kiện lịch |
| BR-02 ZAP bên ngoài | Đạt | Giữ nguyên ranh giới |
| BR-03 Chỉ báo cáo ZAP JSON/XML | Đạt ở mức loại đầu vào | Ghi rõ mẫu báo cáo hỗ trợ và tải tệp theo nguồn |
| BR-04 Truy vết đến tài sản/lần quét/lô/bản gốc | Một phần | Gắn liên kết và occurrence rõ ở bước tạo/cập nhật Finding |
| BR-05 Báo cáo gốc bất biến + toàn vẹn | Đã thể hiện luồng chính | Giữ riêng quyền truy cập và xử lý lô trùng |
| BR-06 Không nhập trùng | Chưa nhất quán | Khóa lô, thứ tự lưu và hậu điều kiện nhánh trùng |
| BR-07 Closed tái xuất hiện → Reopened | Đã thể hiện | Hoàn thiện audit và phân công lại với hạn mới |
| BR-08 Risk/Confidence/Final Severity tách riêng | Đã thể hiện | Không làm mất giá trị nguồn khi đánh giá |
| BR-09 Lý do và lịch sử severity | Một phần | Kết quả rà soát thứ hai, người/thời điểm và ảnh hưởng tới thời hạn |
| BR-10 Nhóm khắc phục không tự đóng | Đạt về actor đóng | Sửa điều kiện vào xác minh và hợp lệ của kết quả |
| BR-11 Duyệt rủi ro độc lập, có hạn | Một phần | Thẩm quyền, dữ liệu, điều kiện quyết định và xử lý hết hạn |
| BR-12 Che bí mật trước hiển thị/xuất | Một phần | Hoàn thiện luồng xuất/tải và nội dung thông báo |
| BR-13 Audit truy cập/chuyển trạng thái | Chưa đủ | Cả nhánh lỗi, mở lại, báo cáo gốc, thu hồi và các thay đổi bắt buộc |
| BR-14 Giữ dữ liệu truy vết | Một phần | Ràng buộc cho tài sản/lịch/phiếu, không chỉ RawReport |
| BR-15 Hỗ trợ trong bốn chức năng | Đạt về phạm vi quan sát được | Chú thích package hỗ trợ, không thêm phân hệ |

## 7. Thứ tự hoàn thiện để đạt yêu cầu

1. Sửa ngay các đường đi có thể bỏ qua điều kiện: bằng chứng, rà soát thứ hai, xác minh hợp lệ, duyệt rủi ro, thời điểm lịch và chống trùng lô.
2. Chuẩn hóa danh mục/mã UC và quyền; sửa các include/extend sai nghĩa.
3. Bổ sung hành vi còn thiếu trong nguồn: bảo mật báo cáo, False Positive tái xuất hiện, mở lại do rủi ro tăng/vi phạm, gia hạn, phê duyệt lại scope và theo dõi hạn.
4. Cập nhật ma trận truy vết: mỗi yêu cầu nguồn có UC, Activity/tệp, nhánh/guard hoặc note tương ứng; các phần gộp phải ghi rõ.
5. Dựng thử toàn bộ hình bằng công cụ đích, kiểm tra lane của hành động tự động, nhãn nhánh, đường quay lại và khả năng đọc. Tách hình theo nhóm khi cần, không tăng số lượng hình chỉ để đạt con số đề xuất.

Điều kiện chốt là mỗi yêu cầu trong nguồn được thể hiện nhất quán, các đường đi sai bị chặn và mọi đề xuất đều có căn cứ trong nguồn. Hiện chưa đáp ứng điều kiện đó. Báo cáo này chỉ bổ sung kết quả đánh giá; tài liệu nguồn, tài liệu đặc tả và các tệp mã sơ đồ không bị chỉnh sửa.
