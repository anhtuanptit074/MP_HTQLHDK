# Kiểm tra sau hiệu chỉnh Use Case và Activity

Ngày: 04/09/2026. Phạm vi: đặc tả 2.0 và bộ mã sơ đồ trong thư mục này.

## 1. Kết quả

| Hạng mục | Kết quả |
|---|---|
| Danh mục chuẩn | 63 mã UC; mỗi mã có hợp đồng, căn cứ nguồn và Activity liên quan |
| Use Case | 7 sơ đồ: 1 tổng quan, 6 chi tiết |
| Activity | 16 sơ đồ: AD-01 đến AD-16 |
| Dựng hình | 23/23 sơ đồ dựng thành SVG và PNG bằng PlantUML 1.2026.7 |
| Đồng bộ | Khối PlantUML trong Markdown khớp nội dung tệp `.puml` tương ứng |
| Mã UC | Tên/ID trong hình chi tiết khớp danh mục; không có tham chiếu bí danh chưa định nghĩa |
| Liên kết tài liệu | Đã kiểm tra các liên kết tới đặc tả, mã và hình dựng |
| Kiểm tra hình | Đã xem toàn bộ qua ảnh tổng hợp và kiểm tra riêng các hình có nhãn include/extend dày; chỉnh khoảng cách các quan hệ |
| Tài liệu nguồn | SHA-256 trước/sau giống nhau; không sửa nội dung hoặc định dạng |

SHA-256 của `doi_tuong_sd_quy_trinh_hd.md`:

```text
8BE9F0D04240F73443B3923D806E5653C922665AF2F3EC1F69961E73E58A7DB4
```

## 2. Đối chiếu các tình huống quan trọng

Đây là kiểm tra tĩnh đường đi trên sơ đồ và hợp đồng liên kết, không phải kiểm thử một ứng dụng đã triển khai.

| Tình huống | Kết quả được thể hiện | Vị trí |
|---|---|---|
| Thiếu biện pháp hoặc bằng chứng | Giữ In Progress, bổ sung và kiểm tra lại trước Pending Verification | AD-06 |
| Hạ Critical/High cần rà soát | Chờ người thứ hai xác nhận; không áp dụng nếu chưa đồng ý | AD-05 |
| False Positive Critical/High | Có người thứ hai khác người đề xuất, đúng thẩm quyền và chấp thuận | AD-07 |
| Báo cáo xác minh không hợp lệ | ATTT xác nhận, Reopened; không đi tới Closed | AD-06 |
| Báo cáo xác minh hợp lệ không còn fingerprint | ATTT đóng, lưu người/thời điểm/báo cáo/audit | AD-06 |
| Reopened cần xử lý tiếp | Phân công lại kèm thời hạn mới, chuyển Assigned | AD-06 |
| Nhập cùng tài sản/lần quét/checksum | Trả lô hiện có; không tạo lô, occurrence, Finding hoặc phiếu trùng | AD-04 |
| Báo cáo hợp lệ 0 Alert | Vòng lặp không chạy; không tạo Finding giả, Completed | AD-04, AD-12 |
| Lỗi cấu trúc/phạm vi cả lô | Rejected trước ghi Finding | AD-04 |
| Có bản ghi hợp lệ và bản ghi lỗi | Giữ bản ghi hợp lệ và ImportError, Partially Completed | AD-04 |
| Lỗi kỹ thuật xử lý | Failed theo hợp đồng ngoại lệ, không dùng để xác minh thành công | AD-04, AD-12 |
| Closed tái xuất hiện | Thêm occurrence/last_seen, gọi mở lại có căn cứ | AD-12, AD-10 |
| False Positive tái xuất hiện | Yêu cầu rà soát lại, lưu kết luận; không tự bỏ qua | AD-12, AD-07 |
| Risk Accepted tiếp tục xuất hiện | Cập nhật last_seen, giữ trạng thái trừ khi hết điều kiện | AD-12, AD-10 |
| Người đề nghị tự phê duyệt / sai thẩm quyền | Từ chối quyết định, không áp dụng Risk Accepted | AD-08, AD-13 |
| Yêu cầu bổ sung đề nghị | Gửi lại và quay về người duyệt; không tự coi bổ sung là phê duyệt | AD-08, AD-13 |
| Gia hạn còn chờ | Hạn cũ vẫn có hiệu lực, không tự kéo dài | AD-13, AD-10 |
| Hết hạn / vi phạm / rủi ro tăng | Xét trạng thái và quyết định hiện hành, mở lại khi đủ căn cứ | AD-10 |
| Chưa đến kỳ / lịch hoặc tài sản không Active | Không tạo và không giao Planned | AD-11 |
| ZAP Failed/Missed | Có lý do, audit; không tự khởi chạy lần thay thế | AD-11 |
| Scope thay đổi chưa duyệt | Không dùng scope mới; cần xác nhận và ATTT duyệt lại | AD-02, AD-03, AD-14 |
| Mất quyền / sai asset scope | Từ chối theo K-01 ở mọi thao tác, kể cả tải tệp | Mọi AD; chi tiết AD-09, AD-15 |
| Xem RawReport | Quyền riêng, checksum, bản gốc bất biến, audit | AD-09 |
| Xuất/tải/thu hồi | Masking bản xuất, nhãn, người/thời điểm/mã truy vết, hạn/thu hồi liên kết, audit | AD-09 |
| Đổi severity hoặc thời hạn | Lưu trước/sau, lý do; thời hạn theo chính sách đơn vị | AD-05, AD-16 |

## 3. Các thay đổi so với bản đánh giá trước

- Đồng bộ ý nghĩa mã UC theo đặc tả, tách phân tích/mức độ về nhóm T và khắc phục về nhóm R.
- Bốn mã được phân rã thêm từ yêu cầu nguồn: S08 xác nhận thời gian, R13 điều chỉnh phân công/hạn, C09 xem dữ liệu nghiệp vụ, C10 thu hồi liên kết. Không thêm chức năng ngoài nguồn.
- Phân biệt actor với hành vi nội bộ; tách ghi nhật ký khỏi xem nhật ký; sửa quan hệ include/extend không đúng nghĩa.
- Bổ sung các guard/đường quay lại; chuyển quy trình định kỳ sang xử lý từng sự kiện/lần quét, không đi tiếp khi chưa đến lịch.
- Bổ sung luồng bảo mật báo cáo, False Positive, tái xuất hiện/mở lại, gia hạn, vòng đời tài sản, phân quyền, theo dõi hạn và thông báo tối thiểu.
- Mã mẫu chỉ còn một bản trong mỗi tệp Markdown; `.puml` và ảnh dựng được tạo từ đúng khối mã đó.
- Bản đặc tả cập nhật được lưu trong dự án để liên kết ổn định, đồng thời cập nhật tệp người dùng cung cấp trong Downloads.

## 4. Giới hạn theo đúng nguồn

Nguồn chưa định nghĩa cạnh chuyển để đảo ngược False Positive sau rà soát lại, hoặc toàn bộ actor/guard hủy và khôi phục vòng đời tài sản/lịch. Bộ sơ đồ giữ yêu cầu ghi nhận, rà soát và các trạng thái nguồn, không tự đặt cạnh hoặc quyền mới. Các cách phân rã kiểm tra lỗi lô/bản ghi và scope đề nghị được giải thích tại mục 8 của đặc tả.

Đã dựng bằng PlantUML; chưa nhập trực tiếp vào một dự án Visual Paradigm `.vpp`. Tệp SVG/PNG dùng để xem, `.puml` hoặc khối mã trong Markdown dùng cho công cụ có hỗ trợ PlantUML. Khả năng đọc khối mã qua từng phiên bản giao diện Visual Paradigm không được suy ra từ việc dựng thành công.

[Danh mục và hình dựng](README.md) · [Đặc tả hiện hành](../dinh_dang_use_case_activity.md)

## 5. Tài liệu giải thích chi tiết

Đã tạo và kiểm tra hai bộ tài liệu Markdown đi kèm:

- [63 tài liệu Use Case](../tai_lieu_use_case/README.md), mỗi mã có nhận diện/mục tiêu, actor, tiền điều kiện, dữ liệu, luồng chính, ngoại lệ, hậu điều kiện, quyền/audit, quan hệ truy vết và tiêu chí kiểm tra.
- [16 tài liệu Activity Diagram](../tai_lieu_activity/README.md), mỗi hình có phạm vi, swimlane, bảng bước, decision/loop, dữ liệu/trạng thái, ngoại lệ và cách đọc/kiểm tra hình.

Kiểm tra tự động xác nhận đủ `63/63` tài liệu UC và `16/16` tài liệu Activity, đúng cấu trúc bắt buộc và không có liên kết nội bộ bị hỏng. SHA-256 của tài liệu nguồn vẫn giữ nguyên như mục 1.
