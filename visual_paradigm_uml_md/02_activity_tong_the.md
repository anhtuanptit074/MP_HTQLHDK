# AD-01 — Quy trình tổng thể

**Nguồn:** mục 4; 7; 8 của tài liệu nguồn.

**Điều kiện vào:** Bắt đầu quản lý tài sản hoặc tiếp tục từ quy trình con tương ứng.

**Kết quả:** Kết quả từng quy trình con được bảo toàn; không bỏ qua guard khi nối luồng.

**Truy vết UC:** UC-C07.

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
title AD-01 — Quy trình tổng thể
|Chủ sở hữu / ATTT|
start
:Thực hiện AD-02\nKhai báo, xác nhận, phê duyệt tài sản;
|Hệ thống|
if (Tài sản Active và scope đã duyệt?) then (Có)
else (Không)
  :Chưa được lập lịch hoạt động;
  stop
endif
|Chuyên viên ATTT|
:Thực hiện AD-03\nTạo / kích hoạt lịch đủ điều kiện;
|Hệ thống|
:Thực hiện AD-11 cho từng kỳ\nChờ đến lịch, vận hành quét ngoài hệ thống;
if (Có báo cáo sau quét Completed?) then (Có)
else (Không)
  :Giữ kết quả Planned / Failed / Missed\nKhông tự chạy lần thay thế;
  stop
endif
|Nhóm vận hành quét|
:Thực hiện AD-04\nNhập báo cáo, xử lý bằng AD-12;
|Hệ thống|
if (Có Finding mới cần phân tích?) then (Có)
else (Không)
  :Giữ kết quả nhập / lô trùng / lỗi;\nFinding tái xuất hiện xử lý tại AD-12;
  stop
endif
|Chuyên viên ATTT|
:Thực hiện AD-05 cho mỗi Finding New;\nAD-07 xử lý kết quả sai / ngoài phạm vi;
|Hệ thống|
if (Finding = Triaged?) then (Có)
else (Không)
  :Giữ trạng thái và kết quả rà soát;
  stop
endif
|Chủ sở hữu / ATTT|
if (Đề nghị chấp nhận rủi ro?) then (Có)
  :Thực hiện AD-08;
  |Hệ thống|
  if (Finding = Risk Accepted?) then (Có)
    :AD-10 theo dõi hết điều kiện;\nAD-13 xử lý đề nghị gia hạn;
    stop
  else (Không)
    if (Còn Pending Risk Acceptance?) then (Có)
      :Chờ quyết định / bổ sung theo AD-08;
      stop
    endif
  endif
endif
|Chuyên viên ATTT|
:Thực hiện AD-06\nPhân công, khắc phục, xác minh;
note right
AD-06 có điểm rẽ AD-08 từ Assigned / In Progress.
Xác minh nhập qua AD-04; không hợp lệ thì ATTT mở lại.
Reopened được phân công với thời hạn mới.
end note
|Hệ thống|
:Bảo toàn trạng thái đầu ra AD-06;\nAD-09 bảo vệ mọi truy cập báo cáo;\nAD-15 phân quyền, AD-16 theo dõi hạn / thông báo;
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
