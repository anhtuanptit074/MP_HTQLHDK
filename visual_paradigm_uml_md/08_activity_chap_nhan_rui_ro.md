# AD-08 — Đề nghị và phê duyệt chấp nhận rủi ro

**Nguồn:** mục 3.4; 7.2; 7.4 của tài liệu nguồn.

**Điều kiện vào:** Chủ tài sản/ATTT đề nghị từ Triaged, Assigned hoặc In Progress.

**Kết quả:** Risk Accepted hoặc khôi phục trạng thái trước; bổ sung chưa quyết định thì vẫn Pending Risk Acceptance.

**Truy vết UC:** UC-RA01, UC-RA02, UC-RA03, UC-RA04.

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
title AD-08 — Đề nghị và phê duyệt chấp nhận rủi ro
|Chủ sở hữu / ATTT|
start
:Mở Finding và tạo đề nghị;
|Hệ thống|
if (Đúng quyền / phạm vi và trạng thái\nTriaged, Assigned hoặc In Progress?) then (Có)
else (Không)
  :Từ chối; audit;
  stop
endif
repeat
  |Chủ sở hữu / ATTT|
  :Nhập lý do, tác động, giảm thiểu,\nngười chịu trách nhiệm, ngày hết hạn đề nghị;
  |Hệ thống|
  :Kiểm tra dữ liệu; thiếu thì yêu cầu bổ sung;
repeat while (Dữ liệu chưa đầy đủ / hợp lệ?) is (Có) not (Không)
:Lưu trạng thái trước;\nFinding = Pending Risk Acceptance; audit;\nThông báo người duyệt theo AD-16;
repeat
  |Người phê duyệt rủi ro|
  :Xem đề nghị và khuyến nghị của ATTT;\nChọn phê duyệt, từ chối hoặc bổ sung;
  |Hệ thống|
  if (Đúng thẩm quyền / phạm vi\nvà người duyệt khác người đề nghị?) then (Có)
  else (Không)
    :Từ chối quyết định; giữ Pending Risk Acceptance;\nAudit; chuyển người có thẩm quyền xử lý;
    stop
  endif
  |Người phê duyệt rủi ro|
  if (Yêu cầu bổ sung?) then (Có)
    :Ghi nội dung cần bổ sung;
    |Hệ thống|
    :Giữ Pending Risk Acceptance; audit;
    |Chủ sở hữu / ATTT|
    :Bổ sung và gửi lại;
    |Hệ thống|
    :Kiểm tra lại dữ liệu; chưa đủ thì chưa trình duyệt;
  endif
repeat while (Quyết định vừa rồi là yêu cầu bổ sung?) is (Có) not (Không)
|Người phê duyệt rủi ro|
if (Phê duyệt?) then (Có)
  :Xác nhận điều kiện áp dụng và ngày hết hạn;
  |Hệ thống|
  if (Vẫn đúng thẩm quyền, độc lập,\nđủ điều kiện, ngày hết hạn hợp lệ\nvà Finding còn Pending Risk Acceptance?) then (Có)
    :Lưu quyết định; Finding = Risk Accepted; audit;\nAD-12 vẫn cập nhật last_seen;\nAD-10 xử lý khi đến hạn / vi phạm / rủi ro tăng;
  else (Không)
    :Không áp dụng; giữ Pending Risk Acceptance;\nGhi lý do, gửi lại để rà soát;
  endif
else (Từ chối / tiếp tục khắc phục)
  :Nhập lý do;
  |Hệ thống|
  :Khôi phục trạng thái trước; audit;\nTiếp tục AD-06 theo trạng thái được khôi phục;
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
