# SD-12 — Đề nghị và phê duyệt chấp nhận rủi ro

Phiên bản 1.0 · 05/09/2026 · Đặc tả tương tác, chưa phải mã ứng dụng.

## 1. Mục tiêu và truy vết

**Kết quả cần đạt:** Đề nghị được duyệt → Risk Accepted; bị từ chối → đúng trạng thái trước; bổ sung vẫn chờ quyết định.

**Use Case:** UC-RA01, UC-RA02, UC-RA03, UC-RA04. [Danh mục và hợp đồng UC](../../dinh_dang_use_case_activity.md).

**Activity:** [AD-08](../../visual_paradigm_uml_md/08_activity_chap_nhan_rui_ro.md).

**Tác nhân:** Chủ tài sản, Chuyên viên ATTT, Người duyệt rủi ro.

## 2. Điều kiện và dữ liệu

**Tiền điều kiện:** Chủ tài sản/ATTT đề nghị từ Triaged, Assigned hoặc In Progress; người duyệt độc lập, có quyền và thẩm quyền thật.

**Đầu vào:** Lý do, tác động, biện pháp giảm thiểu, người chịu trách nhiệm, hạn đề nghị; khuyến nghị ATTT; quyết định, điều kiện và hạn có hiệu lực.

**Bảng liên quan:** `finding`, `risk_acceptance`, `audit_log`, `notification`. Xem [từ điển dữ liệu](../../database/02_tu_dien_du_lieu.md).

**Quy tắc chung:** [SD-00](SD-00_kiem_quyen_va_giao_dich.md) áp dụng trên từng yêu cầu, bao gồm các nhánh lỗi không lặp lại đầy đủ trong hình. Không có quyền thì dừng trước bước đọc/ghi nghiệp vụ tiếp theo.

## 3. Các lifeline, từ trái sang phải

| Bí danh | Thành phần | Loại UML |
|---|---|---|
| `Owner` | Chủ tài sản | actor |
| `Sec` | Chuyên viên ATTT | actor |
| `Approver` | Người duyệt rủi ro | actor |
| `UI` | Giao diện | boundary |
| `Svc` | Dịch vụ chấp nhận rủi ro | control |
| `DB` | CSDL HTQLHDK | database |

`Svc` là thành phần logic trong backend; không yêu cầu triển khai thành microservice. `DB` là cơ sở dữ liệu gồm các bảng liên quan, không phải một actor người dùng.

## 4. Trình tự nghiệp vụ

1. Chủ tài sản hoặc ATTT tạo đề nghị; lưu previous_finding_status trước chuyển Pending Risk Acceptance.
2. ATTT bổ sung khuyến nghị; người duyệt xem hồ sơ và có thể yêu cầu bổ sung.
3. Bổ sung ghi before/after, gửi lại để rà soát; không tự coi là đã duyệt.
4. Khi quyết định, kiểm độc lập, thẩm quyền và hồ sơ còn phù hợp trạng thái hiện hành.
5. Duyệt lưu điều kiện, thời hạn, applied_at và Risk Accepted cùng audit; từ chối khôi phục previous_finding_status.

## 5. Nhánh thay thế và ngoại lệ

- Người đề nghị tự duyệt hoặc không có thẩm quyền: từ chối và audit.
- Thiếu lý do/tác động/giảm thiểu/hạn: chưa tạo đề nghị.
- Hồ sơ cũ khi một quyết định khác đã được áp dụng: không áp dụng chồng.
- Yêu cầu bổ sung: risk_acceptance Needs Information nhưng Finding vẫn Pending Risk Acceptance.

Mọi lỗi kỹ thuật, từ chối và audit bắt buộc theo SD-00. Các bước ghi ký hiệu `TX` bao gồm kiểm điều kiện hiện hành, ghi nghiệp vụ và audit nguyên tử; không giữ khóa khi chờ thao tác người dùng. Dữ liệu chỉ được trả sau khi quyền và các kiểm soát bắt buộc đạt.

## 6. Sơ đồ và mã nguồn

[Ảnh PNG](../rendered/SD-12_de_nghi_phe_duyet_rui_ro.png) · [SVG phóng to](../rendered/SD-12_de_nghi_phe_duyet_rui_ro.svg) · [PlantUML](../plantuml/SD-12_de_nghi_phe_duyet_rui_ro.puml) · [Mermaid](../mermaid/SD-12_de_nghi_phe_duyet_rui_ro.mmd)

![SD-12 — Đề nghị và phê duyệt chấp nhận rủi ro](../rendered/SD-12_de_nghi_phe_duyet_rui_ro.svg)

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
title SD-12 — Đề nghị và phê duyệt chấp nhận rủi ro
actor "Chủ tài sản" as Owner
actor "Chuyên viên ATTT" as Sec
actor "Người duyệt rủi ro" as Approver
boundary "Giao diện" as UI
control "Dịch vụ chấp nhận rủi ro" as Svc
database "CSDL HTQLHDK" as DB
Owner -> UI : Tạo đề nghị chấp nhận rủi ro
UI -> Svc : Finding, lý do, tác động, giảm thiểu, trách nhiệm, hạn
ref over UI,DB : SD-00: Owner/ATTT; đúng trạng thái nguồn và đủ thông tin
Svc -> DB : TX: risk_acceptance Pending, previous_finding_status
Svc -> DB : Cùng TX: Finding Pending Risk Acceptance + audit; COMMIT
Sec -> UI : Bổ sung khuyến nghị ATTT
UI -> Svc : Khuyến nghị trên hồ sơ
ref over UI,DB : SD-00: ATTT; hồ sơ đang chờ; lưu cập nhật + audit
loop Khi người duyệt yêu cầu bổ sung
  Approver -> UI : Yêu cầu thông tin bổ sung
  UI -> Svc : Nội dung yêu cầu
  ref over UI,DB : SD-00: kiểm quyền duyệt và hồ sơ hiện hành
  Svc -> DB : TX: Needs Information + audit trước/sau
  Owner -> UI : Bổ sung và gửi lại
  UI -> Svc : Thông tin đã bổ sung
  ref over UI,DB : SD-00: kiểm người đề nghị có quyền
  Svc -> DB : TX: Pending + thông tin mới + audit trước/sau
end
Approver -> UI : Phê duyệt hoặc từ chối
UI -> Svc : Quyết định, thẩm quyền, lý do, điều kiện và hạn
ref over UI,DB : SD-00: độc lập; khóa Finding/hồ sơ; kiểm trạng thái hiện hành
alt Tự duyệt, sai thẩm quyền hoặc hồ sơ không còn phù hợp
  Svc --> UI : Từ chối áp dụng; không đổi Finding
else Quyết định hợp lệ
  alt Chấp thuận với điều kiện và hạn hợp lệ
    Svc -> DB : TX: Approved, quyết định, valid_from/until, applied_at
    Svc -> DB : Cùng TX: Finding Risk Accepted + audit; COMMIT
  else Từ chối hoặc yêu cầu tiếp tục khắc phục
    Svc -> DB : TX: Rejected + quyết định
    Svc -> DB : Cùng TX: khôi phục đúng previous_finding_status + audit; COMMIT
  end
  Svc --> UI : Kết quả và thời hạn có hiệu lực
end
@enduml
```

## 7. Lưu ý khi thiết kế và triển khai

Nhóm khắc phục có thể cung cấp ý kiến nhưng không tự tạo chuyển trạng thái rủi ro chính thức. Quyết định Approved/Rejected bất biến; bổ sung khi còn Pending/Needs Information có audit trước/sau. Có thể có nhiều hồ sơ chờ theo schema; ứng dụng kiểm đúng hồ sơ và trạng thái tại thời điểm áp dụng, không tự thêm UNIQUE ngoài nguồn. Nếu valid_from ở tương lai, chỉ áp dụng khi đến hiệu lực và còn đủ điều kiện; không coi approved là đang được chấp nhận trước valid_from.

[Danh mục sơ đồ](../README.md) · [Quy ước](../00_quy_uoc_va_thanh_phan.md) · [Ma trận truy vết](../01_ma_tran_truy_vet.md)
