# ERD — MÔ HÌNH BCNF TỐI GIẢN 13 BẢNG

ERD này thay thế sơ đồ 27 bảng trước đây và bám theo [thiết kế BCNF tối giản](08_thiet_ke_3nf_tinh_gon.md). Sơ đồ tổng thể ưu tiên khóa và quan hệ; danh sách đầy đủ mọi thuộc tính nằm trong [tài liệu giải thích chi tiết](09_giai_thich_chi_tiet_13_bang.md).

## 1. ERD tổng thể

```mermaid
erDiagram
    app_user {
        bigint id PK
        varchar username UK
        varchar account_kind
        boolean is_active
    }

    user_role {
        bigint id PK
        bigint user_id FK
        varchar role_code
        bigint asset_id FK
        bigint granted_by FK
        bigint revoked_by FK
    }

    asset {
        bigint id PK
        varchar code UK
        text base_url
        bigint owner_id FK
        bigint security_officer_id FK
        varchar status
        bigint created_by FK
    }

    asset_scope_revision {
        bigint id PK
        bigint asset_id FK
        integer version_no UK
        varchar status
        timestamptz effective_from
        timestamptz effective_until
    }

    asset_scope_rule {
        bigint id PK
        bigint scope_id FK
        varchar effect
        varchar target_type
        text target_value
        varchar match_mode
    }

    asset_scan_window {
        bigint id PK
        bigint scope_id FK
        smallint weekday_iso
        time start_time
        time end_time
        text timezone
    }

    zap_scan_profile {
        bigint id PK
        varchar code UK
        integer version_no UK
        varchar status
        boolean active_scan_enabled
        bigint service_user_id FK
        bigint created_by FK
        bigint approved_by FK
    }

    scan_plan {
        bigint id PK
        varchar code UK
        integer version_no UK
        varchar plan_kind
        bigint scope_id FK
        bigint zap_profile_id FK
        bigint operator_id FK
        bigint remediation_ticket_id FK
        varchar status
    }

    scan_run {
        bigint id PK
        bigint plan_id FK
        varchar code UK
        timestamptz scheduled_for UK
        varchar scan_status
        varchar report_status
        text report_object_key
        char report_sha256
        varchar verification_outcome
        bigint evaluated_by FK
    }

    finding {
        bigint id PK
        bigint asset_id FK
        varchar code UK
        char fingerprint UK
        varchar final_severity
        varchar status
        bigint assessed_by FK
        bigint decision_by FK
    }

    finding_occurrence {
        bigint id PK
        bigint finding_id FK
        bigint scan_run_id FK
        text record_locator UK
        varchar source_severity
        smallint source_confidence
    }

    remediation_ticket {
        bigint id PK
        bigint finding_id FK,UK
        varchar code UK
        bigint assignee_id FK
        bigint assigned_by FK
        timestamptz due_at
        smallint progress_percent
    }

    audit_log {
        bigint id PK
        bigint actor_id FK
        varchar actor_kind
        varchar action
        varchar object_type
        text object_id
        uuid correlation_id
    }

    app_user ||--o{ user_role : "duoc_cap"
    asset |o--o{ user_role : "gioi_han_asset"
    app_user ||--o{ user_role : "cap_thu_hoi"

    app_user ||--o{ asset : "owner_tao_ATTT"
    asset ||--o{ asset_scope_revision : "co_phien_ban"
    asset_scope_revision ||--o{ asset_scope_rule : "co_luat"
    asset_scope_revision ||--o{ asset_scan_window : "co_khung_gio"

    app_user ||--o{ zap_scan_profile : "service_tao_duyet"
    asset_scope_revision ||--o{ scan_plan : "pham_vi"
    zap_scan_profile ||--o{ scan_plan : "cau_hinh_ZAP"
    app_user ||--o{ scan_plan : "operator_tao_duyet"

    finding ||--o| remediation_ticket : "co_phieu"
    remediation_ticket |o--o{ scan_plan : "yeu_cau_xac_minh"
    scan_plan ||--o{ scan_run : "sinh_lan_quet"

    asset ||--o{ finding : "co_phat_hien"
    finding ||--o{ finding_occurrence : "xuat_hien"
    scan_run ||--o{ finding_occurrence : "ghi_nhan"

    app_user ||--o{ finding : "danh_gia_quyet_dinh"
    app_user ||--o{ remediation_ticket : "giao_va_xu_ly"
    app_user |o--o{ scan_run : "danh_gia_xac_minh"
    app_user |o--o{ audit_log : "thuc_hien"
```

## 2. ERD tài sản, phạm vi và quyền

```mermaid
erDiagram
    app_user {
        bigint id PK
        varchar username UK
        text display_name
        varchar account_kind
        boolean is_active
    }
    user_role {
        bigint id PK
        bigint user_id FK
        varchar role_code
        bigint asset_id FK
        bigint granted_by FK
        timestamptz granted_at
        bigint revoked_by FK
        timestamptz revoked_at
    }
    asset {
        bigint id PK
        varchar code UK
        text name
        text base_url
        varchar environment
        bigint owner_id FK
        bigint security_officer_id FK
        varchar criticality
        varchar status
        bigint created_by FK
    }
    asset_scope_revision {
        bigint id PK
        bigint asset_id FK
        integer version_no UK
        varchar status
        bigint created_by FK
        bigint confirmed_by FK
        bigint decided_by FK
        timestamptz effective_from
        timestamptz effective_until
    }
    asset_scope_rule {
        bigint id PK
        bigint scope_id FK
        varchar effect
        varchar target_type
        text target_value
        varchar match_mode
    }
    asset_scan_window {
        bigint id PK
        bigint scope_id FK
        smallint weekday_iso
        time start_time
        time end_time
        boolean ends_next_day
        text timezone
    }

    app_user ||--o{ user_role : "user_id"
    app_user ||--o{ user_role : "granted_revoked_by"
    asset |o--o{ user_role : "asset_id"
    app_user ||--o{ asset : "owner_security_creator"
    asset ||--o{ asset_scope_revision : "asset_id"
    app_user ||--o{ asset_scope_revision : "created_confirmed_decided"
    asset_scope_revision ||--o{ asset_scope_rule : "scope_id"
    asset_scope_revision ||--o{ asset_scan_window : "scope_id"
```

## 3. ERD quét ZAP, kết quả và khắc phục

```mermaid
erDiagram
    asset_scope_revision {
        bigint id PK
        bigint asset_id FK
        integer version_no
        varchar status
    }
    zap_scan_profile {
        bigint id PK
        varchar code UK
        integer version_no
        varchar status
        boolean active_scan_enabled
        varchar report_format
        bigint service_user_id FK
    }
    scan_plan {
        bigint id PK
        varchar code UK
        varchar plan_kind
        bigint scope_id FK
        bigint zap_profile_id FK
        bigint operator_id FK
        bigint remediation_ticket_id FK
        text recurrence_rule
        timestamptz requested_for
    }
    scan_run {
        bigint id PK
        bigint plan_id FK
        varchar code UK
        timestamptz scheduled_for
        varchar scan_status
        text external_plan_id
        text external_session_id
        varchar report_status
        text report_object_key
        char report_sha256
        varchar verification_outcome
    }
    finding {
        bigint id PK
        bigint asset_id FK
        varchar code UK
        char fingerprint UK
        varchar final_severity
        varchar status
    }
    finding_occurrence {
        bigint id PK
        bigint finding_id FK
        bigint scan_run_id FK
        text record_locator
        varchar source_severity
        smallint source_confidence
    }
    remediation_ticket {
        bigint id PK
        bigint finding_id FK,UK
        varchar code UK
        bigint assignee_id FK
        timestamptz due_at
        smallint progress_percent
        text evidence_object_key
    }

    asset_scope_revision ||--o{ scan_plan : "scope_id"
    zap_scan_profile ||--o{ scan_plan : "zap_profile_id"
    remediation_ticket |o--o{ scan_plan : "verification"
    scan_plan ||--o{ scan_run : "plan_id"
    scan_run ||--o{ finding_occurrence : "scan_run_id"
    finding ||--o{ finding_occurrence : "finding_id"
    finding ||--o| remediation_ticket : "finding_id"
```

## 4. Diễn giải bội số

| Quan hệ | Diễn giải |
|---|---|
| Asset 1–N ScopeRevision | Một asset có nhiều phiên bản phạm vi; mỗi revision thuộc đúng một asset |
| ScopeRevision 1–N Rule/Window | Một phiên bản có nhiều luật và nhiều khung giờ |
| ScopeRevision 1–N ScanPlan | Nhiều kế hoạch có thể dùng cùng một phiên bản đã duyệt |
| ZapProfile 1–N ScanPlan | Một profile version có thể được nhiều kế hoạch dùng |
| ScanPlan 1–N ScanRun | Lịch định kỳ sinh nhiều run; Verification bị giới hạn tối đa một run |
| Asset 1–N Finding | Một asset có nhiều phát hiện |
| Finding N–N ScanRun | Thể hiện bằng `finding_occurrence` |
| Finding 1–0..1 Ticket | Mô hình tối giản có tối đa một phiếu hiện hành cho finding |
| Ticket 1–N Verification Plan | Mỗi lần yêu cầu/quét lại tạo plan Verification mới |
| User 1–N Role | Một người có thể có nhiều role/phạm vi asset |

## 5. Tệp nguồn sơ đồ

- [Mermaid](diagrams/05_erd_13_bang.mmd)
- [PlantUML](diagrams/05_erd_13_bang.puml)

Hai tệp nguồn biểu diễn cùng ERD tổng thể. Mermaid trong tài liệu này có thể được render trực tiếp bởi công cụ hỗ trợ Mermaid.
