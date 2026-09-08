# AD-03 — Tạo, cập nhật, kích hoạt và tạm dừng lịch

**Nguồn:** mục 3.3; 5.2; 8.5 của tài liệu nguồn.

**Điều kiện vào:** ATTT có quyền quản lý lịch theo tài sản.

**Kết quả:** Draft/Active/Paused; lần quét được xử lý ở AD-11.

**Truy vết UC:** UC-S01, UC-S02, UC-S03, UC-S04, UC-S08.

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
title AD-03 — Tạo, cập nhật, kích hoạt và tạm dừng lịch
|Chuyên viên ATTT|
start
:Chọn thao tác trên lịch;
|Hệ thống|
if (Đúng quyền và phạm vi?) then (Có)
else (Không)
  :Từ chối và audit;
  stop
endif
|Chuyên viên ATTT|
if (Tạm dừng lịch đang Active?) then (Có)
  |Hệ thống|
  :ScanSchedule = Paused;\nGhi lịch sử, giữ các lần quét;
  stop
endif
|Chuyên viên ATTT|
:Nhập / rà soát mã, tài sản, scope, tần suất,\nngày hiệu lực / kết thúc, múi giờ, khung giờ,\nnhóm vận hành, ghi chú ZAP không chứa bí mật;
|Chủ sở hữu tài sản|
:Xác nhận thời gian theo phạm vi tài sản;
|Hệ thống|
:Kiểm tra dữ liệu và URL lịch thuộc scope đã duyệt;
if (Dữ liệu / phạm vi hợp lệ?) then (Có)
else (Không)
  :Trả lỗi; chưa áp dụng thay đổi;\nSửa và gửi lại theo AD-03;
  stop
endif
if (Trùng thời gian cùng tài sản?) then (Có)
  :Cảnh báo xung đột để ATTT rà soát;
endif
|Chuyên viên ATTT|
:Xác nhận lưu / áp dụng;
|Hệ thống|
if (Tạo mới?) then (Có)
  :Lưu ScanSchedule = Draft;
else (Không)
  :Lưu lịch sử thay đổi;\nKhông xóa dữ liệu lịch trước;
endif
|Chuyên viên ATTT|
if (Kích hoạt / tiếp tục / áp dụng lịch Active?) then (Có)
  |Hệ thống|
  if (Asset Active, scope đã duyệt,\nthời gian và lịch đủ điều kiện?) then (Có)
    :Áp dụng cập nhật; ScanSchedule = Active;\nGhi lịch sử; AD-11 xử lý từng kỳ;
  else (Không)
    :Không kích hoạt / không áp dụng cập nhật;\nGiữ bản đang có hiệu lực và ghi lý do;
  endif
else (Không)
  |Hệ thống|
  :Giữ Draft / Paused, lưu thay đổi theo quyền;
endif
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
