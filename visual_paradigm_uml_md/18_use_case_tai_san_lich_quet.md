# UC-01 — Tài sản và lịch quét

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
title UC-01 — Tài sản và lịch quét
actor "Chuyên viên ATTT" as Security
actor "Chủ sở hữu tài sản" as Owner
actor "Nhóm vận hành quét" as Ops
actor "Nhóm khắc phục kỹ thuật" as Fix
actor "Người phê duyệt rủi ro" as Risk
actor "Kiểm toán viên" as Auditor
rectangle "Hệ thống quản lý quét lỗ hổng website định kỳ" {
  usecase "UC-A01\nKhai báo tài sản" as UC_A01
  usecase "UC-A02\nCập nhật thông tin tài sản" as UC_A02
  usecase "UC-A03\nXác nhận thông tin tài sản" as UC_A03
  usecase "UC-A04\nPhê duyệt phạm vi quét" as UC_A04
  usecase "UC-A05\nTạm dừng tài sản" as UC_A05
  usecase "UC-A06\nNgừng hoạt động tài sản" as UC_A06
  usecase "UC-A07\nXem tài sản theo phạm vi" as UC_A07
  usecase "UC-S01\nTạo lịch quét" as UC_S01
  usecase "UC-S02\nCập nhật lịch quét" as UC_S02
  usecase "UC-S03\nKích hoạt hoặc tiếp tục\nlịch" as UC_S03
  usecase "UC-S04\nTạm dừng lịch" as UC_S04
  usecase "UC-S05\nTheo dõi lịch đến hạn" as UC_S05
  usecase "UC-S06\nCập nhật trạng thái lần\nquét" as UC_S06
  usecase "UC-S07\nGhi nhận quét thất bại hoặc\nbỏ lỡ" as UC_S07
  usecase "UC-S08\nXác nhận thời gian quét" as UC_S08
}
Owner --- UC_A01
Security --- UC_A01
Owner --- UC_A02
Security --- UC_A02
Owner --- UC_A03
Security --- UC_A04
Security --- UC_A05
Security --- UC_A06
Security --- UC_A07
Owner --- UC_A07
Ops --- UC_A07
UC_A07 --- Fix
UC_A07 --- Risk
UC_A07 --- Auditor
Security --- UC_S01
Security --- UC_S02
Security --- UC_S03
Security --- UC_S04
Ops --- UC_S05
Security --- UC_S05
Ops --- UC_S06
Ops --- UC_S07
Owner --- UC_S08
legend bottom
Nguồn 3.3, 5.1–5.2. Xem tài sản theo phạm vi; scope thay đổi phải duyệt lại.
S08 chỉ xác nhận thời gian; ATTT mới kích hoạt lịch.
Result Imported do F-03, không phải lựa chọn cập nhật thủ công của Ops.
endlegend
@enduml
```
