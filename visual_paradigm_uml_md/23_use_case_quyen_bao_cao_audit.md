# UC-06 — Quyền và bảo mật, hỗ trợ F-01 đến F-04

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
title UC-06 — Quyền và bảo mật, hỗ trợ F-01 đến F-04
actor "Quản trị viên" as Admin
actor "Chuyên viên ATTT" as Security
actor "Chủ sở hữu tài sản" as Owner
actor "Nhóm vận hành quét" as Ops
actor "Nhóm khắc phục kỹ thuật" as Fix
actor "Người phê duyệt rủi ro" as Risk
actor "Kiểm toán viên" as Auditor
rectangle "Hệ thống quản lý quét lỗ hổng website định kỳ" {
  usecase "UC-C01\nGán vai trò và phạm vi tài\nsản" as UC_C01
  usecase "UC-C02\nThu hồi quyền" as UC_C02
  usecase "UC-C03\nXem nhật ký thay đổi quyền" as UC_C03
  usecase "UC-C04\nXem báo cáo gốc theo quyền" as UC_C04
  usecase "UC-C05\nXuất hoặc tải báo cáo" as UC_C05
  usecase "UC-C06\nXem lịch sử nghiệp vụ" as UC_C06
  usecase "UC-C07\nGhi nhật ký hành động nhạy\ncảm\n(nội bộ)" as UC_C07
  usecase "UC-C08\nNhận thông báo nghiệp vụ" as UC_C08
  usecase "UC-C09\nXem dữ liệu nghiệp vụ theo\nphạm vi" as UC_C09
  usecase "UC-C10\nThu hồi liên kết báo cáo" as UC_C10
}
Admin --- UC_C01
Admin --- UC_C02
Admin --- UC_C03
UC_C03 --- Auditor
Security --- UC_C04
Owner --- UC_C04
Ops --- UC_C04
UC_C04 --- Fix
UC_C04 --- Risk
UC_C04 --- Auditor
Security --- UC_C05
Owner --- UC_C05
Ops --- UC_C05
UC_C05 --- Fix
UC_C05 --- Risk
UC_C05 --- Auditor
Security --- UC_C06
UC_C06 --- Auditor
Security --- UC_C08
Owner --- UC_C08
Ops --- UC_C08
UC_C08 --- Fix
UC_C08 --- Risk
Security --- UC_C09
Owner --- UC_C09
Ops --- UC_C09
UC_C09 --- Fix
UC_C09 --- Risk
UC_C09 --- Auditor
Security --- UC_C10
Owner --- UC_C10
Ops --- UC_C10
UC_C10 --- Fix
UC_C10 --- Risk
UC_C10 --- Auditor
UC_C01 ...> UC_C07 : <<include>>
UC_C02 ...> UC_C07 : <<include>>
UC_C04 ...> UC_C07 : <<include>>
UC_C05 ...> UC_C07 : <<include>>
UC_C10 ...> UC_C07 : <<include>>
legend bottom
Nguồn 3, 8, 9. Association không đồng nghĩa được xem mọi dữ liệu.
Mỗi thao tác đều kiểm tra quyền riêng + asset scope; Fix theo phiếu, Owner theo tài sản.
Admin chỉ quản lý quyền / xem nhật ký quyền. C04 cần quyền RawReport riêng.
C07 là ghi nhật ký nội bộ; C03/C06 là xem nhật ký. C10 cần quyền thu hồi.
C08 chỉ sự kiện nguồn 9.1; dữ liệu C09 chỉ danh sách tối thiểu nguồn 9.2.
endlegend
@enduml
```
