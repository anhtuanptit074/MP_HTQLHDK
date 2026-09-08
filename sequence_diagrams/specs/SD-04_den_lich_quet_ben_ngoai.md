# SD-04 — Đến lịch và ghi nhận lần quét bên ngoài

Phiên bản 1.0 · 05/09/2026 · Đặc tả tương tác, chưa phải mã ứng dụng.

## 1. Mục tiêu và truy vết

**Kết quả cần đạt:** Tạo tối đa một Planned cho mỗi kỳ; Ops ghi kết quả thực tế; Completed chuyển sang nhập báo cáo SD-05.

**Use Case:** UC-S05, UC-S06, UC-S07. [Danh mục và hợp đồng UC](../../dinh_dang_use_case_activity.md).

**Activity:** [AD-11](../../visual_paradigm_uml_md/12_activity_thuc_hien_lan_quet.md).

**Tác nhân:** Sự kiện thời gian, Nhóm vận hành quét.

## 2. Điều kiện và dữ liệu

**Tiền điều kiện:** Một kỳ kiểm tra lịch hoặc yêu cầu cập nhật từ Ops có quyền; hệ thống chỉ quản lý lịch và kết quả thực hiện.

**Đầu vào:** Thời điểm kiểm tra; schedule_id; kỳ dự kiến; scan_id; thời điểm thực hiện; kết quả Completed/Failed/Missed và lý do.

**Bảng liên quan:** `asset`, `scan_schedule`, `scan_occurrence`, `audit_log`, `notification`. Xem [từ điển dữ liệu](../../database/02_tu_dien_du_lieu.md).

**Quy tắc chung:** [SD-00](SD-00_kiem_quyen_va_giao_dich.md) áp dụng trên từng yêu cầu, bao gồm các nhánh lỗi không lặp lại đầy đủ trong hình. Không có quyền thì dừng trước bước đọc/ghi nghiệp vụ tiếp theo.

## 3. Các lifeline, từ trái sang phải

| Bí danh | Thành phần | Loại UML |
|---|---|---|
| `Clock` | Sự kiện thời gian | actor |
| `Ops` | Nhóm vận hành quét | actor |
| `UI` | Giao diện | boundary |
| `Svc` | Dịch vụ lịch và lần quét | control |
| `DB` | CSDL HTQLHDK | database |
| `ZAP` | OWASP ZAP bên ngoài | participant |

`Svc` là thành phần logic trong backend; không yêu cầu triển khai thành microservice. `DB` là cơ sở dữ liệu gồm các bảng liên quan, không phải một actor người dùng.

## 4. Trình tự nghiệp vụ

1. Tác vụ lịch đọc lịch đến hạn và kiểm tài sản Active, lịch Active, scope và khung giờ.
2. Khóa lịch, kiểm kỳ đã tạo, lưu Planned với snapshot mục tiêu/cấu hình và cập nhật next_due_at cùng giao dịch.
3. Ops xem lần quét được giao, thực hiện OWASP ZAP ngoài hệ thống và xuất báo cáo.
4. Ops cập nhật kết quả thực tế; Failed/Missed bắt buộc lý do.
5. Hoàn thành mới nhập báo cáo; Result Imported do SD-05 xác lập.

## 5. Nhánh thay thế và ngoại lệ

- Chưa đến kỳ/lịch Paused/tài sản không Active: không tạo Planned.
- Hai tác vụ cùng xử lý một kỳ: khóa scan_schedule, kiểm lại kỳ đã có; chỉ tạo một lần quét.
- Failed hoặc Missed: lưu lý do, audit; không tự chạy thay thế.
- Scope đổi trước khi thực hiện: rà soát quyền/phạm vi hiện hành, không tự dùng scope chưa duyệt.

Mọi lỗi kỹ thuật, từ chối và audit bắt buộc theo SD-00. Các bước ghi ký hiệu `TX` bao gồm kiểm điều kiện hiện hành, ghi nghiệp vụ và audit nguyên tử; không giữ khóa khi chờ thao tác người dùng. Dữ liệu chỉ được trả sau khi quyền và các kiểm soát bắt buộc đạt.

## 6. Sơ đồ và mã nguồn

[Ảnh PNG](../rendered/SD-04_den_lich_quet_ben_ngoai.png) · [SVG phóng to](../rendered/SD-04_den_lich_quet_ben_ngoai.svg) · [PlantUML](../plantuml/SD-04_den_lich_quet_ben_ngoai.puml) · [Mermaid](../mermaid/SD-04_den_lich_quet_ben_ngoai.mmd)

![SD-04 — Đến lịch và ghi nhận lần quét bên ngoài](../rendered/SD-04_den_lich_quet_ben_ngoai.svg)

```plantuml
@startuml
hide footbox
skinparam backgroundColor #FFFFFF
skinparam shadowing false
skinparam defaultFontName Arial
skinparam defaultFontSize 13
skinparam sequenceMessageAlign center
skinparam maxMessageSize 58
skinparam BoxPadding 8
skinparam ArrowColor #334155
skinparam sequenceParticipantBackgroundColor #EAF2FB
skinparam sequenceParticipantBorderColor #334155
skinparam sequenceLifeLineBorderColor #94A3B8
skinparam sequenceGroupBorderColor #64748B
skinparam sequenceGroupHeaderFontColor #0F172A
skinparam sequenceGroupBackgroundColor #F8FAFC
skinparam sequenceReferenceBackgroundColor #EFF6FF
skinparam sequenceReferenceBorderColor #93C5FD
skinparam NoteBackgroundColor #FFF7E8
skinparam NoteBorderColor #D9A441
autonumber
title SD-04 — Đến lịch và ghi nhận lần quét bên ngoài
actor "Sự kiện thời gian" as Clock
actor "Nhóm vận hành quét" as Ops
boundary "Giao diện" as UI
control "Dịch vụ lịch và lần quét" as Svc
database "CSDL HTQLHDK" as DB
participant "OWASP ZAP bên ngoài" as ZAP
Clock -> Svc : Kiểm tra các lịch đến kỳ
Svc -> DB : BEGIN; khóa lịch; đọc asset và scope hiện hành
DB --> Svc : Điều kiện và next_due_at
alt Chưa đến kỳ hoặc không đủ điều kiện
  Svc -> DB : Kết thúc TX; không tạo Planned
else Đến kỳ hợp lệ
  Svc -> DB : Kiểm lần quét cùng schedule_id và planned_at
  alt Kỳ đã được tạo
    Svc -> DB : Kết thúc TX; giữ lần quét hiện có
  else Chưa tạo
    Svc -> DB : TX: Planned + snapshot scope/target/config
    Svc -> DB : Cùng TX: next_due_at + audit; COMMIT
    ref over Svc,DB : SD-18: thông báo nội bộ sau commit
  end
end
Ops -> UI : Xem lần quét được giao
UI -> Svc : Lấy kế hoạch quét trong phạm vi
ref over UI,DB : SD-00: kiểm Ops và tài sản
Svc --> UI : Kế hoạch và mục tiêu đã duyệt
Ops -> ZAP : Thực hiện quét bên ngoài hệ thống
ZAP --> Ops : Kết quả thực hiện và báo cáo JSON/XML
Ops -> UI : Ghi kết quả thực hiện
UI -> Svc : scan_id, thời gian, kết quả và lý do
ref over UI,DB : SD-00: kiểm quyền, trạng thái và dữ liệu
alt Hoàn thành
  Svc -> DB : TX: scan_occurrence Completed, thời gian, người chạy + audit
  Svc --> UI : Cho phép nhập báo cáo theo SD-05
else Thất bại hoặc bỏ lỡ có lý do
  Svc -> DB : TX: Failed hoặc Missed + outcome_reason + audit
  Svc --> UI : Đã ghi nhận; không tự chạy lại
else Thiếu lý do hoặc trạng thái không hợp lệ
  Svc --> UI : Từ chối cập nhật
end
note over Ops,ZAP : Chỉ Ops tương tác ZAP; không có mũi tên từ backend gọi ZAP
@enduml
```

## 7. Lưu ý khi thiết kế và triển khai

Chống trùng kỳ là hợp đồng ứng dụng trên khóa lịch; không khẳng định database hiện có UNIQUE(schedule_id, planned_at). Danh tính tác vụ hệ thống và audit actor_kind phân biệt với người dùng. Thông báo lịch sắp đến hạn/bỏ lỡ theo SD-18, không thêm dịch vụ email.

[Danh mục sơ đồ](../README.md) · [Quy ước](../00_quy_uoc_va_thanh_phan.md) · [Ma trận truy vết](../01_ma_tran_truy_vet.md)
