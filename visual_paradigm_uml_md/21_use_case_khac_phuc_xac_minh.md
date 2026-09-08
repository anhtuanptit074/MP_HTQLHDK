# UC-04 — Khắc phục và xác minh

Các mã UC và quyền được định nghĩa thống nhất trong tài liệu đặc tả. Association là tham gia; quyền quyết định theo vai trò và phạm vi, không suy ra từ chiều mũi tên.

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
left to right direction
title UC-04 — Khắc phục và xác minh
actor "Chuyên viên ATTT" as Security
actor "Chủ sở hữu tài sản" as Owner
actor "Nhóm vận hành quét" as Ops
actor "Nhóm khắc phục kỹ thuật" as Fix
rectangle "Hệ thống quản lý quét lỗ hổng website định kỳ" {
  usecase "UC-R01\nTạo phiếu khắc phục" as UC_R01
  usecase "UC-R02\nPhân công xử lý" as UC_R02
  usecase "UC-R03\nTiếp nhận phiếu" as UC_R03
  usecase "UC-R04\nCập nhật tiến độ" as UC_R04
  usecase "UC-R05\nCập nhật biện pháp và bằng\nchứng" as UC_R05
  usecase "UC-R06\nChuyển chờ xác minh" as UC_R06
  usecase "UC-R07\nTạo yêu cầu quét xác minh" as UC_R07
  usecase "UC-R08\nCập nhật kết quả quét xác\nminh" as UC_R08
  usecase "UC-R09\nĐối chiếu dấu vân tay phát\nhiện\n(nội bộ)" as UC_R09
  usecase "UC-R10\nĐóng phát hiện" as UC_R10
  usecase "UC-R11\nMở lại phát hiện" as UC_R11
  usecase "UC-R12\nTheo dõi hạn xử lý" as UC_R12
  usecase "UC-R13\nĐiều chỉnh phân công hoặc\nthời hạn" as UC_R13
  usecase "UC-I01\nNhập báo cáo ZAP" as UC_I01
}
Security --- UC_R01
Security --- UC_R02
UC_R03 --- Fix
UC_R04 --- Fix
UC_R05 --- Fix
UC_R06 --- Fix
Security --- UC_R07
Ops --- UC_R08
Security --- UC_R10
Security --- UC_R11
Security --- UC_R12
UC_R12 --- Fix
Owner --- UC_R12
Security --- UC_R13
Ops --- UC_I01
UC_R08 ...> UC_I01 : <<include>>
UC_R10 ...> UC_R09 : <<include>>
legend bottom
Nguồn 5.4, 7.2, 7.5. Các bước nối theo Activity, không dùng include để chỉ thứ tự.
R06 kiểm tra biện pháp / bằng chứng đã có; không bắt cập nhật lại nếu đã đủ.
R10 chỉ ATTT, Pending Verification và báo cáo mới hợp lệ không còn fingerprint.
R11 còn được kích hoạt tự động từ Closed tái xuất hiện / Risk Accepted hết điều kiện (AD-10).
endlegend
@enduml
```
