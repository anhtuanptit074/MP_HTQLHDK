# AD-02 — Khai báo, cập nhật và phê duyệt tài sản

**Nguồn:** mục 4; 5.1; 8.5 của tài liệu nguồn.

**Điều kiện vào:** Chủ sở hữu hoặc ATTT có quyền khai báo/cập nhật; tài sản ngừng hoạt động xử lý AD-14.

**Kết quả:** Draft/Pending Approval/Active; scope mới chỉ được áp dụng khi ATTT duyệt.

**Truy vết UC:** UC-A01, UC-A02, UC-A03, UC-A04.

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
title AD-02 — Khai báo, cập nhật và phê duyệt tài sản
|Người khai báo: Chủ sở hữu / ATTT|
start
:Chọn tạo hoặc sửa tài sản;
|Hệ thống|
if (Xác thực và đúng quyền / phạm vi?) then (Có)
else (Không)
  :Từ chối; ghi AuditLog;
  stop
endif
repeat
  |Người khai báo: Chủ sở hữu / ATTT|
  :Nhập / sửa nhận diện, sở hữu, bối cảnh,\nURL cho phép / loại trừ, khung giờ;
  |Hệ thống|
  :Kiểm tra mã duy nhất, domain / URL,\ndữ liệu bắt buộc khi gửi duyệt;
  if (Trùng domain / URL với tài sản khác?) then (Có)
    :Cảnh báo để người khai báo rà soát;\nKhông tự từ chối chỉ vì cảnh báo trùng;
  endif
repeat while (Dữ liệu chưa hợp lệ?) is (Có) not (Không)
if (Tài sản mới?) then (Có)
  :Lưu Asset = Draft;
else (Không)
  if (Thay đổi phạm vi quét?) then (Có)
    :Lưu đề nghị scope mới, giữ scope đã duyệt;\nKhông dùng scope đề nghị cho lịch / nhập;
  else (Không)
    :Lưu cập nhật theo quyền và lịch sử;\nGiữ trạng thái hiện hành;
    stop
  endif
endif
|Chủ sở hữu tài sản|
:Xác nhận sở hữu, mức quan trọng và scope;
if (Thông tin chính xác và đầy đủ?) then (Có)
else (Không)
  |Hệ thống|
  :Giữ bản khai báo / đề nghị chưa áp dụng;\nTrả lại để sửa và gửi lại từ đầu AD-02;
  stop
endif
|Hệ thống|
:Lưu người xác nhận, thời điểm;\nTài sản mới = Pending Approval;
|Chuyên viên ATTT|
:Rà soát chủ sở hữu, mức quan trọng,\nURL cho phép / loại trừ và khung giờ;
if (Phê duyệt?) then (Có)
  |Hệ thống|
  :Kiểm tra lại quyền, dữ liệu, phiên bản scope;\nÁp dụng scope được duyệt; Asset = Active;\nLưu người / thời điểm phê duyệt và lịch sử;
else (Không)
  |Chuyên viên ATTT|
  :Nhập lý do từ chối;
  |Hệ thống|
  :Tài sản mới về Draft;\nThay đổi scope bị từ chối không được áp dụng;\nGiữ lịch sử, sửa và gửi lại theo AD-02;
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
