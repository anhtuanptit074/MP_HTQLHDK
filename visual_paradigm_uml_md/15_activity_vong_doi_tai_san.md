# AD-14 — Tạm dừng, tiếp tục và ngừng tài sản

**Nguồn:** mục 5.1.2–5.1.3; 8.5–8.6 của tài liệu nguồn.

**Điều kiện vào:** ATTT có quyền trên tài sản; không định nghĩa thêm trạng thái.

**Kết quả:** Suspended/Retired hoặc Active sau kiểm tra lại; lịch sử được bảo toàn.

**Truy vết UC:** UC-A02, UC-A05, UC-A06.

**Giới hạn trạng thái:** Tiếp tục từ Suspended là cách biểu diễn ý nghĩa tạm dừng; phải kiểm tra lại điều kiện nguồn. Không suy diễn chuỗi chuyển bắt buộc hay khôi phục Retired. Những cạnh vòng đời chưa được nguồn xác định không được cấp quyền chuyển tùy ý.

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
title AD-14 — Tạm dừng, tiếp tục và ngừng tài sản
|Chuyên viên ATTT|
start
:Chọn tài sản và thao tác;
|Hệ thống|
if (Đúng quyền và phạm vi?) then (Có)
else (Không)
  :Từ chối; audit;
  stop
endif
if (Asset = Retired?) then (Có)
  :Giữ Retired, chỉ xem lịch sử;\nKhông thực hiện thay đổi vòng đời;
  stop
endif
|Chuyên viên ATTT|
switch (Thao tác)
case (Tạm dừng do thay đổi / sự cố)
  :Ghi căn cứ tạm dừng;
  |Hệ thống|
  :Asset = Suspended; audit;\nKhông cho kích hoạt lịch / nhập kết quả;
case (Ngừng hoạt động)
  |Chuyên viên ATTT|
  :Xác nhận ngừng hoạt động;
  |Hệ thống|
  :Asset = Retired; chỉ giữ lịch sử; audit;\nKhông xóa cứng Asset, Finding, phiếu;
case (Tiếp tục sau tạm dừng)
  |Hệ thống|
  if (Asset Suspended và đã đủ điều kiện hoạt động?) then (Có)
    |Chuyên viên ATTT|
    :Rà soát sở hữu, mức quan trọng, scope và khung giờ;
    |Hệ thống|
    if (Có thay đổi scope chưa được duyệt?) then (Có)
      :Thực hiện AD-02 để xác nhận / phê duyệt lại;\nChưa dùng scope mới khi chưa được duyệt;
    else (Không)
      |Chuyên viên ATTT|
      :Xác nhận tiếp tục;
      |Hệ thống|
      :Asset = Active; audit;
    endif
  else (Không)
    :Giữ trạng thái hiện tại; không tự khôi phục Retired;
  endif
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
