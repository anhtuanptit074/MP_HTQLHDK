# ERD — mô hình 27 bảng

Các hình chỉ hiển thị thuộc tính chính. PK/FK ghép và ràng buộc đầy đủ xem từ điển/SQL. Bảy vai trò là miền giá trị cố định; ma trận quyền là view, không phải bảng nghiệp vụ bổ sung.

## 01 tai san phan quyen

```mermaid
erDiagram
    app_user {
        bigint id PK
    }
    team {
        bigint id PK
        varchar code
    }
    team_member {
        bigint id PK
        bigint team_id FK
        bigint user_id FK
    }
    user_role {
        bigint id PK
        bigint user_id FK
        user_role_code role_code
        bigint asset_id FK
    }
    asset {
        bigint id PK, FK
        varchar code
        bigint owner_id FK
        bigint technical_team_id FK
        bigint security_officer_id FK
        asset_status status
        bigint current_scope_id FK
    }
    asset_scope_revision {
        bigint id PK
        bigint asset_id FK
        integer version_no
        text status
    }
    asset_scope_rule {
        bigint id PK
        bigint scope_id FK
        text effect
        text target_value
    }
    asset_scan_window {
        bigint id PK
        bigint scope_id FK
    }
    team ||--o{ team_member : "team_id"
    app_user ||--o{ team_member : "user_id"
    app_user ||--o{ user_role : "user_id"
    asset |o--o{ user_role : "asset_id"
    app_user |o--o{ asset : "owner_id"
    team |o--o{ asset : "technical_team_id"
    app_user |o--o{ asset : "security_officer_id"
    asset_scope_revision |o--o| asset : "current_scope_id"
    asset ||--o{ asset_scope_revision : "asset_id"
    asset_scope_revision ||--o{ asset_scope_rule : "scope_id"
    asset_scope_revision ||--o{ asset_scan_window : "scope_id"
```

[SVG](diagrams/01_tai_san_phan_quyen.svg) · [PNG](diagrams/01_tai_san_phan_quyen.png) · [PlantUML](diagrams/01_tai_san_phan_quyen.puml)

## 02 lich quet nhap ket qua

```mermaid
erDiagram
    asset {
        bigint id PK, FK
        varchar code
        bigint owner_id FK
        bigint technical_team_id FK
        bigint security_officer_id FK
        asset_status status
        bigint current_scope_id FK
    }
    asset_scope_revision {
        bigint id PK
        bigint asset_id FK
        integer version_no
        text status
    }
    scan_schedule {
        bigint id PK
        varchar code
        bigint asset_id FK
        bigint scope_id FK
        bigint operations_team_id FK
        schedule_status status
        text_array target_urls
    }
    scan_occurrence {
        bigint id PK
        varchar code
        bigint asset_id FK
        bigint scope_id FK
        bigint schedule_id FK
        bigint operations_team_id FK
        scan_status status
        text_array target_urls
    }
    import_batch {
        bigint id PK
        varchar code
        bigint asset_id FK
        bigint scan_id FK
        varchar file_sha256
        import_status status
        text raw_object_key
    }
    import_error {
        bigint id PK
        bigint batch_id FK
    }
    finding {
        bigint id PK, FK
        varchar code
        bigint asset_id FK
        varchar fingerprint
        finding_status status
        bigint current_review_id FK
        bigint verification_result_id FK
    }
    finding_occurrence {
        bigint id PK
        bigint asset_id FK
        bigint finding_id FK
        bigint scan_id FK
    }
    finding_observation {
        bigint id PK
        bigint asset_id FK
        bigint scan_id FK
        bigint occurrence_id FK
        bigint batch_id FK
        source_severity source_severity
        smallint source_confidence
    }
    asset_scope_revision |o--o| asset : "current_scope_id"
    asset ||--o{ asset_scope_revision : "asset_id"
    asset ||--o{ scan_schedule : "asset_id"
    asset_scope_revision ||--o{ scan_schedule : "scope_id"
    asset ||--o{ scan_occurrence : "asset_id"
    asset_scope_revision ||--o{ scan_occurrence : "scope_id"
    scan_schedule |o--o{ scan_occurrence : "schedule_id"
    asset ||--o{ import_batch : "asset_id"
    scan_occurrence ||--o{ import_batch : "scan_id"
    import_batch ||--o{ import_error : "batch_id"
    asset ||--o{ finding : "asset_id"
    finding ||--o{ finding_occurrence : "finding_id"
    scan_occurrence ||--o{ finding_occurrence : "scan_id"
    finding_occurrence ||--o{ finding_observation : "occurrence_id"
    import_batch ||--o{ finding_observation : "batch_id"
```

[SVG](diagrams/02_lich_quet_nhap_ket_qua.svg) · [PNG](diagrams/02_lich_quet_nhap_ket_qua.png) · [PlantUML](diagrams/02_lich_quet_nhap_ket_qua.puml)

## 03 phan tich khac phuc

```mermaid
erDiagram
    scan_occurrence {
        bigint id PK
        varchar code
        bigint asset_id FK
        bigint scope_id FK
        bigint schedule_id FK
        bigint operations_team_id FK
        scan_status status
        text_array target_urls
    }
    import_batch {
        bigint id PK
        varchar code
        bigint asset_id FK
        bigint scan_id FK
        varchar file_sha256
        import_status status
        text raw_object_key
    }
    finding {
        bigint id PK, FK
        varchar code
        bigint asset_id FK
        varchar fingerprint
        finding_status status
        bigint current_review_id FK
        bigint verification_result_id FK
    }
    evidence {
        bigint id PK
        bigint finding_id FK
    }
    finding_review {
        bigint id PK
        bigint finding_id FK
        text review_kind
        text status
        bigint previous_review_id FK
        final_severity proposed_severity
        bigint evidence_id FK
        bigint trigger_occurrence_id FK
    }
    sla_rule {
        bigint id PK
        final_severity severity
    }
    remediation_ticket {
        bigint id PK, FK
        varchar code
        bigint finding_id FK
        bigint current_event_id FK
    }
    ticket_event {
        bigint id PK
        bigint ticket_id FK
        bigint finding_id FK
        bigint previous_event_id FK
        bigint team_id FK
        bigint assignee_id FK
        timestamptz due_at
        bigint sla_rule_id FK
        bigint evidence_id FK
    }
    verification_request {
        bigint id PK
        bigint asset_id FK
        bigint finding_id FK
        bigint ticket_id FK
        bigint scope_id FK
        bigint remediation_event_id FK
        bigint operations_team_id FK
        text status
    }
    verification_result {
        bigint id PK
        bigint request_id FK
        bigint finding_id FK
        bigint asset_id FK
        bigint scan_id FK
        bigint batch_id FK
    }
    scan_occurrence ||--o{ import_batch : "scan_id"
    finding_review |o--o| finding : "current_review_id"
    verification_result |o--o| finding : "verification_result_id"
    finding ||--o{ evidence : "finding_id"
    finding ||--o{ finding_review : "finding_id"
    finding_review |o--o{ finding_review : "previous_review_id"
    evidence |o--o{ finding_review : "evidence_id"
    finding ||--o{ remediation_ticket : "finding_id"
    ticket_event |o--o| remediation_ticket : "current_event_id"
    remediation_ticket ||--o{ ticket_event : "ticket_id"
    ticket_event |o--o{ ticket_event : "previous_event_id"
    sla_rule |o--o{ ticket_event : "sla_rule_id"
    evidence |o--o{ ticket_event : "evidence_id"
    finding ||--o{ verification_request : "finding_id"
    remediation_ticket ||--o{ verification_request : "ticket_id"
    ticket_event ||--o{ verification_request : "remediation_event_id"
    verification_request ||--o{ verification_result : "request_id"
    scan_occurrence ||--o{ verification_result : "scan_id"
    import_batch |o--o{ verification_result : "batch_id"
```

[SVG](diagrams/03_phan_tich_khac_phuc.svg) · [PNG](diagrams/03_phan_tich_khac_phuc.png) · [PlantUML](diagrams/03_phan_tich_khac_phuc.puml)

## 04 rui ro bao mat

```mermaid
erDiagram
    app_user {
        bigint id PK
    }
    asset {
        bigint id PK, FK
        varchar code
        bigint owner_id FK
        bigint technical_team_id FK
        bigint security_officer_id FK
        asset_status status
        bigint current_scope_id FK
    }
    finding {
        bigint id PK, FK
        varchar code
        bigint asset_id FK
        varchar fingerprint
        finding_status status
        bigint current_review_id FK
        bigint verification_result_id FK
    }
    risk_acceptance {
        bigint id PK
        bigint finding_id FK
        bigint renewal_of_id FK
        bigint responsible_user_id FK
        text status
        timestamptz valid_until
    }
    audit_log {
        bigint id PK
        bigint actor_id FK
        bigint asset_id FK
        bigint finding_id FK
        bigint evidence_id FK
        bigint verification_result_id FK
        bigint risk_acceptance_id FK
        bigint finding_review_id FK
        bigint trigger_occurrence_id FK
    }
    report_export {
        bigint id PK
        bigint recipient_id FK
        varchar token_hash
        timestamptz expires_at
    }
    report_export_asset {
        bigint export_id PK, FK
        bigint asset_id PK, FK
    }
    notification {
        bigint id PK
        bigint recipient_id FK
        bigint asset_id FK
        bigint finding_id FK
    }
    app_user |o--o{ asset : "owner_id"
    app_user |o--o{ asset : "security_officer_id"
    asset ||--o{ finding : "asset_id"
    finding ||--o{ risk_acceptance : "finding_id"
    risk_acceptance |o--o{ risk_acceptance : "renewal_of_id"
    app_user ||--o{ risk_acceptance : "responsible_user_id"
    app_user |o--o{ audit_log : "actor_id"
    asset |o--o{ audit_log : "asset_id"
    finding |o--o{ audit_log : "finding_id"
    risk_acceptance |o--o{ audit_log : "risk_acceptance_id"
    app_user ||--o{ report_export : "recipient_id"
    report_export ||--o{ report_export_asset : "export_id"
    asset ||--o{ report_export_asset : "asset_id"
    app_user ||--o{ notification : "recipient_id"
    asset ||--o{ notification : "asset_id"
    finding |o--o{ notification : "finding_id"
```

[SVG](diagrams/04_rui_ro_bao_mat.svg) · [PNG](diagrams/04_rui_ro_bao_mat.png) · [PlantUML](diagrams/04_rui_ro_bao_mat.puml)
