# AD-07 — False Positive và bản ghi sai hoặc ngoài phạm vi

**Nguồn:** mục 7.2–7.3; 8.5 của tài liệu nguồn.

**Điều kiện vào:** ATTT xử lý Finding New; hoặc rà soát lại False Positive tái xuất hiện.

**Kết quả:** New → False Positive/Cancelled theo nguồn; tái rà soát không tự bỏ qua hoặc thêm cạnh trạng thái mới.

**Truy vết UC:** UC-T03, UC-T04.

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
title AD-07 — False Positive và bản ghi sai hoặc ngoài phạm vi
|Chuyên viên ATTT|
start
:Mở Finding New hoặc yêu cầu rà soát lại False Positive;
|Hệ thống|
if (Đúng quyền ATTT / phạm vi?) then (Có)
else (Không)
  :Từ chối; audit;
  stop
endif
|Chuyên viên ATTT|
:Rà soát bằng chứng, vị trí, scope và lý do;
if (Đang rà soát lại False Positive tái xuất hiện?) then (Có)
  :Lưu kết luận rà soát và bằng chứng mới;
  |Hệ thống|
  :Giữ occurrence mới, lịch sử và kết luận;\nKhông tự xóa hoặc bỏ qua phát hiện;
  note right
Nguồn 7.3 yêu cầu rà soát lại nhưng chưa quy định
cạnh chuyển khi đảo ngược kết luận False Positive.
Không tự chuyển FP sang New / Reopened.
Nếu cần đảo kết luận, ghi nhận để ATTT xử lý theo
quy tắc nghiệp vụ được xác nhận, không tự cấp quyền mới.
end note
  stop
endif
|Chuyên viên ATTT|
if (Bản ghi sai hoặc ngoài phạm vi?) then (Có)
  :Nhập lý do hủy;
  |Hệ thống|
  :Finding = Cancelled; giữ RawReport; audit;
  stop
endif
|Chuyên viên ATTT|
:Nhập lý do False Positive và bằng chứng;
|Hệ thống|
if (Đủ lý do và bằng chứng?) then (Có)
else (Không)
  :Giữ New; yêu cầu bổ sung rồi gửi lại AD-07;
  stop
endif
if (Mức nguồn hoặc Final Severity là High / Critical?) then (Có)
  :Gửi rà soát; chưa chuyển False Positive;
  |Người rà soát thứ hai theo thẩm quyền|
  :Đánh giá và xác nhận;
  |Hệ thống|
  if (Đúng thẩm quyền, khác người đề xuất\nvà đã chấp thuận?) then (Có)
    :Lưu người và thời điểm rà soát;
  else (Không)
    :Giữ New; lưu kết quả; ATTT rà soát lại;
    stop
  endif
endif
|Hệ thống|
:Finding = False Positive;\nGiữ dữ liệu / bằng chứng, audit;\nTái xuất hiện phải được xử lý bằng AD-12;
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
