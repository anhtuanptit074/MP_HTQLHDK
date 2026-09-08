# AD-12 — Chuẩn hóa và xử lý từng phát hiện nhập

**Nguồn:** mục 5.3.4–5.3.5; 7.3–7.4 của tài liệu nguồn.

**Điều kiện vào:** Chỉ các bản ghi đã qua kiểm tra của lô không trùng từ AD-04.

**Kết quả:** Finding/occurrence được truy vết; số liệu xử lý trả AD-04, lỗi kỹ thuật trả nhánh Failed.

**Truy vết UC:** UC-I08, UC-I09, UC-I10.

**Quy ước xử lý:** Lỗi kỹ thuật dừng xử lý và trả Failed cho AD-04. Finding/occurrence đã ghi phải giữ truy vết, không được nhân đôi khi đọc lại kết quả cùng lô. AD-10 tại đây chỉ xét Closed và bản ghi hợp lệ đã ghi nhận, không xét việc cả lô là bằng chứng xác minh đủ điều kiện đóng.

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
title AD-12 — Chuẩn hóa và xử lý từng phát hiện nhập
|Hệ thống|
start
while (Còn bản ghi hợp lệ chưa xử lý?) is (Có)
  :Đọc Alert; ánh xạ trường nguồn;\nGiữ Risk và Confidence riêng; che bản hiển thị;
  :Chuẩn hóa URL và tạo fingerprint\nasset + Alert Ref / Rule ID + URL +\nparameter / input vector + HTTP method nếu có;
  note right
Không dùng Attack hoặc Evidence làm fingerprint.
Không tự nâng High thành Critical.
end note
  if (Đã có Finding cùng fingerprint?) then (Có)
    :Liên kết Finding hiện có;\nThêm FindingOccurrence và cập nhật last_seen;\nKhông tạo phiếu trùng;
    if (Finding = Closed?) then (Có)
      :Gọi AD-10 với báo cáo mới đã nhập hợp lệ;\nKết quả: Reopened khi đủ căn cứ;
    else (Không)
      if (Finding = False Positive?) then (Có)
        :Tạo yêu cầu ATTT rà soát lại theo AD-07;\nKhông tự bỏ qua vĩnh viễn;
      else (Không)
        :Giữ trạng thái hiện hành;\nRisk Accepted vẫn cập nhật last_seen;
      endif
    endif
  else (Không)
    :Tạo Finding = New;\nTạo FindingOccurrence ban đầu và last_seen;
  endif
  :Liên kết Asset, ScanOccurrence, ImportBatch, RawReport;\nLưu kết quả và audit các chuyển trạng thái;
endwhile (Không)
:Trả thống kê cho AD-04;\n0 bản ghi không tạo Finding / occurrence giả;
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
