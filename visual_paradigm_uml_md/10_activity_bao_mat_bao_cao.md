# AD-09 — Xem dữ liệu, báo cáo gốc, xuất, tải và thu hồi

**Nguồn:** mục 3.3; 8; 9.2 của tài liệu nguồn.

**Điều kiện vào:** Người dùng đã được gán quyền cụ thể; quyền RawReport tách riêng.

**Kết quả:** Chỉ trả dữ liệu đúng phạm vi; bản gốc bất biến; mọi truy cập nhạy cảm và tạo/tải/thu hồi có audit.

**Truy vết UC:** UC-A07, UC-I07, UC-I12, UC-T06, UC-C03, UC-C04, UC-C05, UC-C06, UC-C07, UC-C09, UC-C10.

```plantuml
@startuml
skinparam shadowing false
skinparam defaultFontName Arial
skinparam defaultFontSize 13
skinparam backgroundColor #FFFFFF
skinparam ArrowColor #334155
skinparam activityBackgroundColor #EFF6FF
skinparam activityBorderColor #334155
skinparam activityDiamondBackgroundColor #FFF7ED
skinparam activityDiamondBorderColor #9A3412
skinparam usecaseBackgroundColor #EFF6FF
skinparam usecaseBorderColor #334155
skinparam packageStyle rectangle
skinparam maxMessageSize 70
!pragma useVerticalIf on
title AD-09 — Xem dữ liệu, báo cáo gốc, xuất, tải và thu hồi
|Người dùng theo vai trò được cấp|
start
:Yêu cầu xem dữ liệu / xem bản gốc /\nxuất / tải / thu hồi liên kết;
|Hệ thống|
if (Xác thực và quyền thao tác + phạm vi hiện hành?) then (Có)
else (Không)
  :Từ chối mặc định; audit;
  stop
endif
if (Liên quan RawReport / bằng chứng nhạy cảm?) then (Có)
  if (Có quyền riêng phù hợp?) then (Có)
  else (Không)
    :Từ chối; audit;
    stop
  endif
endif
switch (Thao tác)
case (Thu hồi liên kết)
  :Kiểm tra quyền thu hồi với đối tượng;\nVô hiệu liên kết, giữ dữ liệu gốc;\nAudit người / thời điểm / kết quả;
case (Tải tệp qua liên kết)
  if (Liên kết còn hạn, chưa bị thu hồi,\nngười nhận còn quyền và đúng phạm vi?) then (Có)
    :Trả tệp được phép qua kênh mã hóa;\nKiểm tra masking theo quyền hiện hành; audit tải;
  else (Không)
    :Từ chối tải; audit;
  endif
case (Xuất báo cáo)
  :Chọn dữ liệu tối thiểu theo mục 9.2;\nChe bí mật theo quyền người nhận;
  :Tạo bản xuất có nhãn nhạy cảm,\nngười xuất, thời điểm, mã truy vết;\nBảo vệ khi lưu và truyền;
  :Tạo liên kết có thời hạn, có thể thu hồi;\nKhông tạo link công khai lâu dài; audit tạo;
case (Xem / tải RawReport)
  :Kiểm tra toàn vẹn; cung cấp bản gốc theo quyền riêng;\nKhông sửa / thay thế bản gốc; audit đọc / tải;
case (Xem dữ liệu / lịch sử)
  :Lọc theo role, tài sản, nhiệm vụ / phiếu;\nHiển thị bản đã che dữ liệu;\nNhật ký chỉ đọc; Admin chỉ nhật ký quyền;
  :Ghi audit nếu truy cập bằng chứng nhạy cảm;
endswitch
stop
legend bottom
K-01: Mọi thao tác người dùng phải kiểm tra quyền hiện hành + phạm vi phía máy chủ.
Không đạt K-01: từ chối, ghi audit, không thực hiện bước đọc / ghi kế tiếp.
K-02: Mọi thay đổi, phê duyệt, từ chối, lỗi và truy cập nhạy cảm theo nguồn 8.5 đều ghi audit.
Audit: người, thời điểm, đối tượng, hành động, kết quả; trước / sau và bằng chứng khi có.
Các lời gọi AD-xx phải bảo toàn tiền điều kiện, mọi nhánh lỗi và trạng thái đầu ra của AD được gọi.
endlegend
@enduml
```
