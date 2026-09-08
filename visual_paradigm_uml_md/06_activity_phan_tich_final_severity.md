# AD-05 — Phân tích và xác nhận hoặc điều chỉnh Final Severity

**Nguồn:** mục 6; 7.2–7.3 của tài liệu nguồn.

**Điều kiện vào:** ATTT có quyền; New khi triage, hoặc Finding đã đánh giá khi điều chỉnh mức độ.

**Kết quả:** New → Triaged chỉ sau xác nhận hợp lệ; điều chỉnh severity không tự đổi trạng thái workflow.

**Truy vết UC:** UC-T01, UC-T02, UC-T05.

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
title AD-05 — Phân tích và xác nhận hoặc điều chỉnh Final Severity
|Chuyên viên ATTT|
start
:Mở phát hiện để triage hoặc điều chỉnh severity;
|Hệ thống|
if (Đúng quyền ATTT và phạm vi?) then (Có)
else (Không)
  :Từ chối; audit;
  stop
endif
|Chuyên viên ATTT|
:Xem tài sản, vị trí, bằng chứng đã che,\nRisk, Confidence và bối cảnh nghiệp vụ;
if (Cần xác nhận ảnh hưởng nghiệp vụ?) then (Có)
  |Chủ sở hữu tài sản|
  :Cung cấp / xác nhận ảnh hưởng;
endif
|Chuyên viên ATTT|
if (Triage New và là kết quả sai / ngoài phạm vi?) then (Có)
  :Thực hiện AD-07;
  stop
endif
:Đề xuất Final Severity và nhập lý do;\nKhông sửa Risk / Confidence nguồn;
|Hệ thống|
if (Đủ bằng chứng, bối cảnh và lý do?) then (Có)
else (Không)
  :Không áp dụng; giữ giá trị / trạng thái cũ;\nYêu cầu bổ sung rồi gửi lại AD-05;
  stop
endif
if (Hạ mức Critical / High\nvà chính sách yêu cầu rà soát?) then (Có)
  :Giữ mức hiện hành; gửi đề xuất rà soát;
  |Người rà soát thứ hai theo thẩm quyền|
  :Rà soát căn cứ và xác nhận quyết định;
  |Hệ thống|
  if (Đúng thẩm quyền, khác người đánh giá\nvà đã đồng ý?) then (Có)
    :Lưu reviewed_by / reviewed_at;
  else (Không)
    :Chưa áp dụng; giữ giá trị / trạng thái cũ;\nLưu kết quả và trả ATTT rà soát lại;
    stop
  endif
endif
|Hệ thống|
:Lưu Final Severity, severity_reason,\nassessed_by / assessed_at và trước / sau;\nGiữ nguyên source_severity / source_confidence;
if (Finding trước đánh giá = New?) then (Có)
  :Finding = Triaged;
else (Không)
  :Giữ trạng thái workflow;\nThời hạn bị ảnh hưởng xử lý theo AD-16;
endif
:Audit; thông báo Critical / High theo AD-16;
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
