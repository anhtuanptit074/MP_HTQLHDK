# DỮ LIỆU MẪU VÀ TRUY VẤN MINH HỌA CHO MÔ HÌNH 13 BẢNG

## 1. Mục đích

Tài liệu này cung cấp dữ liệu mẫu dễ đọc cho mô hình cơ sở dữ liệu 13 bảng của hệ thống quản lý quét lỗ hổng website tích hợp OWASP ZAP.

Dữ liệu minh họa một luồng hoàn chỉnh:

1. Tạo người dùng và cấp quyền theo tài sản.
2. Khai báo tài sản và phạm vi quét.
3. Tạo hồ sơ ZAP và kế hoạch quét.
4. Ghi nhận lần chạy và báo cáo ZAP.
5. Chuẩn hóa phát hiện.
6. Phân công khắc phục.
7. Quét xác minh và đóng phát hiện.
8. Ghi nhật ký các hành động quan trọng.

Các tên miền sử dụng miền `example.vn` và các tham chiếu khóa chỉ là dữ liệu giả. Bằng chứng, payload và thông tin xác thực đã được rút gọn hoặc che.

## 2. Quan hệ tổng quát

```text
app_user
  ├── user_role ── asset
  ├── asset (owner, security officer)
  ├── zap_scan_profile
  ├── scan_plan
  ├── finding
  ├── remediation_ticket
  └── audit_log

asset
  └── asset_scope_revision
        ├── asset_scope_rule
        ├── asset_scan_window
        └── scan_plan
              └── scan_run
                    └── finding_occurrence ── finding
                                                └── remediation_ticket
                                                      └── scan_plan loại Verification
```

## 3. Dữ liệu mẫu của 13 bảng

### 3.1. `app_user`

Lưu người dùng nghiệp vụ và tài khoản dịch vụ. Vai trò không được lưu trực tiếp trong bảng này.

| id | username | display_name | account_kind | is_active | created_at |
|---:|---|---|---|:---:|---|
| 1 | `admin01` | Nguyễn Văn Admin | Human | true | 2026-01-02 08:00 +07 |
| 2 | `owner.shop` | Trần Quốc Bình | Human | true | 2026-01-02 08:05 +07 |
| 3 | `security.an` | Nguyễn Hoàng An | Human | true | 2026-01-02 08:10 +07 |
| 4 | `operator.ha` | Vũ Thanh Hà | Human | true | 2026-01-02 08:15 +07 |
| 5 | `remediator.cuong` | Lê Minh Cường | Human | true | 2026-01-02 08:20 +07 |
| 6 | `auditor.lan` | Đặng Ngọc Lan | Human | true | 2026-01-02 08:25 +07 |
| 7 | `svc.zap` | Tài khoản tích hợp ZAP | Service | true | 2026-01-02 08:30 +07 |
| 8 | `owner.portal` | Phạm Thu Trang | Human | true | 2026-01-02 08:35 +07 |

Ghi chú: tài khoản `Service` không có mật khẩu đăng nhập giao diện và không được thực hiện phê duyệt nghiệp vụ.

### 3.2. `user_role`

Mỗi dòng thể hiện một vai trò của người dùng trên một tài sản. `asset_id` chỉ để `NULL` đối với vai trò toàn hệ thống như `ADMIN`.

| id | user_id | role_code | asset_id | granted_by | granted_at | revoked_at |
|---:|---:|---|---:|---:|---|---|
| 201 | 1 | ADMIN | NULL | 1 | 2026-01-02 09:00 +07 | NULL |
| 202 | 3 | SECURITY | 101 | 1 | 2026-01-05 10:00 +07 | NULL |
| 203 | 3 | SECURITY | 102 | 1 | 2026-01-06 10:00 +07 | NULL |
| 204 | 3 | SECURITY | 103 | 1 | 2026-01-07 10:00 +07 | NULL |
| 205 | 4 | SCAN_OPERATOR | 101 | 1 | 2026-01-05 10:05 +07 | NULL |
| 206 | 4 | SCAN_OPERATOR | 102 | 1 | 2026-01-06 10:05 +07 | NULL |
| 207 | 5 | REMEDIATOR | 101 | 1 | 2026-01-05 10:10 +07 | NULL |
| 208 | 5 | REMEDIATOR | 102 | 1 | 2026-01-06 10:10 +07 | NULL |
| 209 | 6 | AUDITOR | 101 | 1 | 2026-01-05 10:15 +07 | NULL |
| 210 | 6 | AUDITOR | 102 | 1 | 2026-01-06 10:15 +07 | NULL |

Chủ tài sản không cần một dòng `OWNER` trùng lặp trong `user_role`; chủ sở hữu chính được xác định bằng `asset.owner_id`.

### 3.3. `asset`

Lưu website cần quản lý và quét.

| id | code | name | base_url | environment | owner_id | security_officer_id | criticality | internet_exposed | status |
|---:|---|---|---|---|---:|---:|---|:---:|---|
| 101 | ASSET-SHOP | Website bán hàng | `https://shop.example.vn` | Production | 2 | 3 | Critical | true | Active |
| 102 | ASSET-PORTAL | Cổng thông tin khách hàng | `https://portal.example.vn` | Production | 8 | 3 | High | true | Active |
| 103 | ASSET-INTRANET | Cổng nội bộ | `https://intranet.example.vn` | Staging | 2 | 3 | Medium | false | Suspended |

Trong ví dụ này, người dùng 3 là ATTT phụ trách cả ba tài sản nhưng quyền vẫn được cấp riêng cho từng tài sản trong `user_role`.

### 3.4. `asset_scope_revision`

Lưu các phiên bản phạm vi quét của tài sản.

| id | asset_id | version_no | status | change_reason | created_by | confirmed_by | decided_by | effective_from | effective_until |
|---:|---:|---:|---|---|---:|---:|---:|---|---|
| 1001 | 101 | 1 | Approved | Phạm vi production ban đầu | 2 | 2 | 3 | 2026-01-06 07:00 +07 | NULL |
| 1002 | 102 | 1 | Approved | Phạm vi cổng khách hàng | 8 | 8 | 3 | 2026-01-07 07:00 +07 | NULL |
| 1003 | 103 | 1 | Approved | Phạm vi staging nội bộ | 2 | 2 | 3 | 2026-01-08 07:00 +07 | NULL |

Phạm vi 1003 đã được duyệt nhưng tài sản 103 đang `Suspended`, vì vậy không được tạo lần quét mới.

### 3.5. `asset_scope_rule`

Lưu từng domain hoặc URL được phép quét hay phải loại trừ.

| id | scope_id | effect | target_type | target_value | match_mode |
|---:|---:|---|---|---|---|
| 1101 | 1001 | Include | Domain | `shop.example.vn` | Exact |
| 1102 | 1001 | Include | URL | `https://shop.example.vn/api/` | Prefix |
| 1103 | 1001 | Exclude | URL | `https://shop.example.vn/payment/` | Prefix |
| 1104 | 1001 | Exclude | URL | `https://shop.example.vn/logout` | Exact |
| 1105 | 1002 | Include | Domain | `portal.example.vn` | Exact |
| 1106 | 1002 | Exclude | URL | `https://portal.example.vn/admin/` | Prefix |
| 1107 | 1003 | Include | Domain | `intranet.example.vn` | Exact |

Khi một URL đồng thời khớp luật `Include` và `Exclude`, luật `Exclude` được ưu tiên.

### 3.6. `asset_scan_window`

Lưu khung giờ được phép quét của từng phiên bản phạm vi.

| id | scope_id | weekday_iso | start_time | end_time | ends_next_day | timezone |
|---:|---:|---:|---|---|:---:|---|
| 1201 | 1001 | 7 | 01:00 | 05:00 | false | Asia/Bangkok |
| 1202 | 1001 | 3 | 22:00 | 02:00 | true | Asia/Bangkok |
| 1203 | 1002 | 6 | 00:30 | 04:30 | false | Asia/Bangkok |
| 1204 | 1003 | 2 | 08:00 | 12:00 | false | Asia/Bangkok |

`weekday_iso = 1` là thứ Hai và `weekday_iso = 7` là Chủ nhật. Dòng 1202 là khung giờ kéo dài qua nửa đêm.

### 3.7. `zap_scan_profile`

Lưu hồ sơ OWASP ZAP có phiên bản và đã được phê duyệt.

| id | code | version_no | status | spider_kind | passive_scan_enabled | active_scan_enabled | max_duration_minutes | request_delay_ms | auth_kind | report_format | service_user_id |
|---:|---|---:|---|---|:---:|:---:|---:|---:|---|---|---:|
| 2001 | ZAP-WEB-FULL | 1 | Active | Traditional | true | true | 180 | 250 | Header | JSON | 7 |
| 2002 | ZAP-WEB-PASSIVE | 1 | Active | Ajax | true | false | 90 | 500 | None | JSON | 7 |

Hồ sơ 2001 sử dụng tham chiếu bí mật `vault://htqlhdk/zap/shop-auth`; giá trị token thật không lưu trong cơ sở dữ liệu.

### 3.8. `scan_plan`

Lưu cả kế hoạch quét định kỳ và yêu cầu quét xác minh.

| id | code | version_no | plan_kind | scope_id | zap_profile_id | operator_id | recurrence_rule | remediation_ticket_id | requested_for | status |
|---:|---|---:|---|---:|---:|---:|---|---:|---|---|
| 3001 | PLAN-SHOP-WEEKLY | 1 | Periodic | 1001 | 2001 | 4 | `FREQ=WEEKLY;BYDAY=SU` | NULL | NULL | Active |
| 3002 | PLAN-PORTAL-WEEKLY | 1 | Periodic | 1002 | 2002 | 4 | `FREQ=WEEKLY;BYDAY=SA` | NULL | NULL | Active |
| 3003 | VERIFY-SHOP-001 | 1 | Verification | 1001 | 2001 | 4 | NULL | 6001 | 2026-09-15 08:00 +07 | Active |

Tài sản của kế hoạch được suy ra qua `scan_plan.scope_id → asset_scope_revision.asset_id`, không lưu lặp `asset_id` trong `scan_plan`.

### 3.9. `scan_run`

Mỗi dòng là một lần chạy cụ thể, gồm trạng thái thực thi ZAP và báo cáo nguồn.

| id | plan_id | code | scheduled_for | scan_status | external_session_id | report_status | failure_code | verification_outcome |
|---:|---:|---|---|---|---|---|---|---|
| 4001 | 3001 | RUN-SHOP-20260901 | 2026-09-01 08:00 +07 | Completed | zap-session-001 | Processed | NULL | NULL |
| 4002 | 3001 | RUN-SHOP-20260908 | 2026-09-08 08:00 +07 | Failed | zap-session-002 | Not Received | ZAP_CONNECTION_LOST | NULL |
| 4003 | 3002 | RUN-PORTAL-20260903 | 2026-09-03 08:00 +07 | Completed | zap-session-003 | Processed | NULL | NULL |
| 4004 | 3003 | RUN-VERIFY-SHOP-001 | 2026-09-15 08:00 +07 | Completed | zap-session-004 | Processed | NULL | Closed |
| 4005 | 3002 | RUN-PORTAL-20260920 | 2026-09-20 08:00 +07 | Running | zap-session-005 | Not Received | NULL | NULL |

Thông tin báo cáo của lần chạy 4001:

| Thuộc tính | Giá trị mẫu |
|---|---|
| `report_object_key` | `reports/shop/20260901.json` |
| `report_sha256` | `eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee` |
| `report_size_bytes` | 245760 |
| `report_format` | JSON |
| `report_key_ref` | `kms://demo/report-key` |

Thông tin xác minh của lần chạy 4004:

| Thuộc tính | Giá trị mẫu |
|---|---|
| `coverage_confirmed` | true |
| `fingerprint_present` | false |
| `verification_outcome` | Closed |
| `evaluated_by` | 3 |
| `verification_reason` | Đã bao phủ đúng endpoint và không còn fingerprint |

### 3.10. `finding`

Lưu phát hiện chuẩn hóa và trạng thái nghiệp vụ hiện hành.

| id | asset_id | code | source_alert_ref | title_masked | final_severity | status | assessed_by | decision_by |
|---:|---:|---|---|---|---|---|---:|---:|
| 5001 | 101 | FIND-SHOP-001 | 40018 | SQL Injection tại API đơn hàng | High | Closed | 3 | 3 |
| 5002 | 101 | FIND-SHOP-002 | 10021 | Thiếu Content-Security-Policy | Medium | In Progress | 3 | NULL |
| 5003 | 102 | FIND-PORTAL-001 | 40012 | Cross Site Scripting phản xạ | Critical | Assigned | 3 | NULL |
| 5004 | 102 | FIND-PORTAL-002 | 10096 | Timestamp Disclosure | Informational | False Positive | 3 | 3 |

Dấu vân tay minh họa:

| finding_id | fingerprint |
|---:|---|
| 5001 | `aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa` |
| 5002 | `bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb` |
| 5003 | `cccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccc` |
| 5004 | `dddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddd` |

### 3.11. `finding_occurrence`

Là bảng nối N–N giữa `finding` và `scan_run`, cho biết phát hiện xuất hiện trong lần quét nào.

| id | finding_id | scan_run_id | record_locator | source_severity | source_confidence | cwe_id | wasc_id |
|---:|---:|---:|---|---|---:|---:|---:|
| 7001 | 5001 | 4001 | `/site/0/alerts/0/instances/0` | High | 3 | 89 | 19 |
| 7002 | 5002 | 4001 | `/site/0/alerts/1/instances/0` | Medium | 2 | 693 | 15 |
| 7003 | 5003 | 4003 | `/site/0/alerts/0/instances/0` | High | 3 | 79 | 8 |
| 7004 | 5004 | 4003 | `/site/0/alerts/1/instances/0` | Informational | 1 | 200 | 13 |

Lần xác minh 4004 không có occurrence của finding 5001. Kết hợp với `coverage_confirmed = true`, hệ thống có căn cứ để kết luận fingerprint không còn tồn tại.

### 3.12. `remediation_ticket`

Lưu phân công và tiến độ khắc phục hiện hành. Trạng thái workflow được lấy từ `finding.status`.

| id | finding_id | code | assignee_id | assigned_by | due_at | progress_percent | remediated_at |
|---:|---:|---|---:|---:|---|---:|---|
| 6001 | 5001 | TICKET-SHOP-001 | 5 | 3 | 2026-09-11 00:00 +07 | 100 | 2026-09-09 16:00 +07 |
| 6002 | 5002 | TICKET-SHOP-002 | 5 | 3 | 2026-09-19 00:00 +07 | 60 | NULL |
| 6003 | 5003 | TICKET-PORTAL-001 | 5 | 3 | 2026-09-26 00:00 +07 | 20 | NULL |

Thông tin khắc phục của phiếu 6001:

| Thuộc tính | Giá trị mẫu |
|---|---|
| `root_cause_masked` | Câu truy vấn ghép chuỗi từ tham số đường dẫn |
| `remediation_action_masked` | Chuyển sang prepared statement và kiểm tra kiểu dữ liệu |
| `evidence_summary_masked` | Đã kiểm thử hồi quy API đơn hàng |
| `evidence_object_key` | `evidence/shop/5001.bin` |
| `evidence_key_ref` | `kms://demo/evidence-key` |

### 3.13. `audit_log`

Lưu nhật ký append-only cho hành động bảo mật và thay đổi nghiệp vụ.

| id | actor_id | actor_kind | occurred_at | category | action | object_type | object_id | outcome |
|---:|---:|---|---|---|---|---|---|---|
| 8001 | 1 | Human | 2026-01-05 10:00 +07 | Permission | `role.grant` | user_role | 202 | Success |
| 8002 | 3 | Human | 2026-09-01 11:00 +07 | Business | `finding.assess` | finding | 5001 | Success |
| 8003 | 7 | Service | 2026-09-01 10:10 +07 | Import | `report.process` | scan_run | 4001 | Success |
| 8004 | NULL | System | 2026-09-08 08:22 +07 | System | `scan.fail` | scan_run | 4002 | Failure |
| 8005 | 5 | Human | 2026-09-09 16:00 +07 | Business | `ticket.complete_remediation` | remediation_ticket | 6001 | Success |
| 8006 | 3 | Human | 2026-09-15 11:30 +07 | Business | `finding.close` | finding | 5001 | Success |

Ví dụ ảnh trước và sau của bản ghi 8006:

```json
{
  "before_masked": { "status": "Pending Verification" },
  "after_masked": { "status": "Closed" },
  "reason_masked": "Quét xác minh đủ bao phủ và không còn fingerprint"
}
```

## 4. Truy vấn minh họa

Các truy vấn dưới đây giả định các bảng nằm trong schema `htqlhdk13`.

```sql
SET search_path = htqlhdk13, public;
```

### 4.1. Xem người dùng và quyền trên từng tài sản

```sql
SELECT
    u.username,
    u.display_name,
    ur.role_code,
    COALESCE(a.code, '(TOÀN HỆ THỐNG)') AS asset_code,
    COALESCE(a.name, 'Không cấp quyền đọc mọi tài sản') AS asset_name
FROM user_role ur
JOIN app_user u ON u.id = ur.user_id
LEFT JOIN asset a ON a.id = ur.asset_id
WHERE ur.revoked_at IS NULL
ORDER BY u.username, ur.role_code, asset_code;
```

Kết quả tiêu biểu:

| username | role_code | asset_code |
|---|---|---|
| `admin01` | ADMIN | (TOÀN HỆ THỐNG) |
| `security.an` | SECURITY | ASSET-SHOP |
| `security.an` | SECURITY | ASSET-PORTAL |
| `operator.ha` | SCAN_OPERATOR | ASSET-SHOP |

### 4.2. Xem tài sản cùng chủ sở hữu và ATTT phụ trách

```sql
SELECT
    a.code AS asset_code,
    a.name AS asset_name,
    owner.display_name AS owner_name,
    security.display_name AS security_officer,
    a.criticality,
    a.status
FROM asset a
JOIN app_user owner ON owner.id = a.owner_id
JOIN app_user security ON security.id = a.security_officer_id
ORDER BY a.code;
```

### 4.3. Xem phạm vi include/exclude của một tài sản

```sql
SELECT
    a.code AS asset_code,
    scope.version_no,
    rule.effect,
    rule.target_type,
    rule.target_value,
    rule.match_mode
FROM asset a
JOIN asset_scope_revision scope ON scope.asset_id = a.id
JOIN asset_scope_rule rule ON rule.scope_id = scope.id
WHERE a.code = 'ASSET-SHOP'
  AND scope.status = 'Approved'
ORDER BY
    CASE rule.effect WHEN 'Include' THEN 1 ELSE 2 END,
    rule.id;
```

Kết quả cho thấy `shop.example.vn` và đường dẫn `/api/` được quét, còn `/payment/` và `/logout` bị loại trừ.

### 4.4. Xem lịch sử quét và tài sản tương ứng

`scan_run` không lưu trực tiếp `asset_id`. Tài sản được truy qua `scan_plan` và `asset_scope_revision`.

```sql
SELECT
    run.code AS run_code,
    asset.code AS asset_code,
    plan.plan_kind,
    run.scheduled_for,
    run.scan_status,
    run.report_status,
    run.failure_code
FROM scan_run run
JOIN scan_plan plan ON plan.id = run.plan_id
JOIN asset_scope_revision scope ON scope.id = plan.scope_id
JOIN asset asset ON asset.id = scope.asset_id
ORDER BY run.scheduled_for;
```

Kết quả tiêu biểu:

| run_code | asset_code | plan_kind | scan_status | report_status | failure_code |
|---|---|---|---|---|---|
| RUN-SHOP-20260901 | ASSET-SHOP | Periodic | Completed | Processed | NULL |
| RUN-SHOP-20260908 | ASSET-SHOP | Periodic | Failed | Not Received | ZAP_CONNECTION_LOST |
| RUN-VERIFY-SHOP-001 | ASSET-SHOP | Verification | Completed | Processed | NULL |

### 4.5. Thống kê phát hiện theo tài sản và mức độ cuối cùng

```sql
SELECT
    asset.code AS asset_code,
    finding.final_severity,
    COUNT(*) AS finding_count
FROM finding
JOIN asset ON asset.id = finding.asset_id
GROUP BY asset.code, finding.final_severity
ORDER BY asset.code,
    CASE finding.final_severity
        WHEN 'Critical' THEN 1
        WHEN 'High' THEN 2
        WHEN 'Medium' THEN 3
        WHEN 'Low' THEN 4
        ELSE 5
    END;
```

Kết quả mẫu:

| asset_code | final_severity | finding_count |
|---|---|---:|
| ASSET-PORTAL | Critical | 1 |
| ASSET-PORTAL | Informational | 1 |
| ASSET-SHOP | High | 1 |
| ASSET-SHOP | Medium | 1 |

### 4.6. Tính thời điểm xuất hiện đầu tiên và gần nhất

`first_seen_at` và `last_seen_at` không lưu lặp trong `finding`; chúng được tính từ các lần xuất hiện.

```sql
SELECT
    finding.code AS finding_code,
    finding.title_masked,
    MIN(run.started_at) AS first_seen_at,
    MAX(run.started_at) AS last_seen_at,
    COUNT(*) AS occurrence_count
FROM finding
JOIN finding_occurrence occurrence
    ON occurrence.finding_id = finding.id
JOIN scan_run run
    ON run.id = occurrence.scan_run_id
GROUP BY finding.id, finding.code, finding.title_masked
ORDER BY finding.code;
```

### 4.7. Tìm phiếu khắc phục quá hạn hoặc sắp đến hạn

Ví dụ lấy ngày `21/09/2026` làm thời điểm báo cáo để kết quả không phụ thuộc ngày chạy thực tế.

```sql
SELECT
    ticket.code AS ticket_code,
    finding.code AS finding_code,
    finding.final_severity,
    assignee.display_name AS assignee,
    ticket.progress_percent,
    ticket.due_at,
    CASE
        WHEN finding.status IN ('Closed', 'False Positive', 'Cancelled')
            THEN 'Đã kết thúc'
        WHEN ticket.due_at < TIMESTAMPTZ '2026-09-21 00:00:00+00'
            THEN 'Quá hạn'
        WHEN ticket.due_at < TIMESTAMPTZ '2026-09-24 00:00:00+00'
            THEN 'Sắp đến hạn'
        ELSE 'Trong hạn'
    END AS due_state
FROM remediation_ticket ticket
JOIN finding ON finding.id = ticket.finding_id
JOIN app_user assignee ON assignee.id = ticket.assignee_id
ORDER BY ticket.due_at;
```

Kết quả mẫu:

| ticket_code | finding_code | progress_percent | due_state |
|---|---|---:|---|
| TICKET-SHOP-001 | FIND-SHOP-001 | 100 | Đã kết thúc |
| TICKET-SHOP-002 | FIND-SHOP-002 | 60 | Quá hạn |
| TICKET-PORTAL-001 | FIND-PORTAL-001 | 20 | Trong hạn |

### 4.8. Xem kết quả quét xác minh của phiếu khắc phục

```sql
SELECT
    ticket.code AS ticket_code,
    finding.code AS finding_code,
    plan.code AS verification_plan,
    run.code AS verification_run,
    run.coverage_confirmed,
    run.fingerprint_present,
    run.verification_outcome,
    evaluator.display_name AS evaluated_by,
    run.evaluated_at
FROM remediation_ticket ticket
JOIN finding ON finding.id = ticket.finding_id
JOIN scan_plan plan
    ON plan.remediation_ticket_id = ticket.id
   AND plan.plan_kind = 'Verification'
LEFT JOIN scan_run run ON run.plan_id = plan.id
LEFT JOIN app_user evaluator ON evaluator.id = run.evaluated_by
ORDER BY ticket.code, run.scheduled_for;
```

Kết quả của `TICKET-SHOP-001`:

| coverage_confirmed | fingerprint_present | verification_outcome | evaluated_by |
|:---:|:---:|---|---|
| true | false | Closed | Nguyễn Hoàng An |

### 4.9. Tìm các lần quét cần chú ý

```sql
SELECT
    code,
    scan_status,
    heartbeat_at,
    failure_code,
    failure_message_masked
FROM scan_run
WHERE scan_status IN ('Failed', 'Timed Out')
   OR (
        scan_status = 'Running'
        AND heartbeat_at < TIMESTAMPTZ '2026-09-20 03:00:00+00'
   )
ORDER BY scheduled_for;
```

Truy vấn trả về lần quét 4002 bị mất kết nối ZAP và lần quét 4005 đang chạy nhưng heartbeat đã cũ.

### 4.10. Xem lịch sử audit của một phát hiện

```sql
SELECT
    audit.occurred_at,
    COALESCE(actor.display_name, audit.actor_kind) AS actor,
    audit.action,
    audit.outcome,
    audit.before_masked,
    audit.after_masked,
    audit.reason_masked
FROM audit_log audit
LEFT JOIN app_user actor ON actor.id = audit.actor_id
WHERE audit.object_type = 'finding'
  AND audit.object_id = '5001'
ORDER BY audit.occurred_at;
```

Kết quả thể hiện hai mốc chính của `FIND-SHOP-001`:

1. Chuyên viên ATTT xác nhận mức `High` và chuyển phát hiện sang `Triaged`.
2. Sau quét xác minh, chuyên viên ATTT chuyển trạng thái từ `Pending Verification` sang `Closed`.

## 5. Lưu ý khi sử dụng

- Đây là dữ liệu minh họa, không phải dữ liệu production.
- Không sử dụng mật khẩu, token, cookie hoặc API key thật trong dữ liệu seed.
- Tất cả thời điểm cần lưu bằng `timestamptz`; giao diện có thể hiển thị theo `Asia/Bangkok`.
- Không xóa bản ghi `user_role` khi thu hồi quyền; cập nhật `revoked_by` và `revoked_at`.
- Không sửa phiên bản phạm vi hoặc hồ sơ ZAP đã được phê duyệt; thay đổi phải tạo phiên bản mới.
- Không lưu `asset_id` lặp trong `scan_plan` và `scan_run`; truy theo quan hệ scope.
- Không lưu `first_seen_at` và `last_seen_at` trong `finding`; tính từ `finding_occurrence` và `scan_run`.
- Nhật ký `audit_log` phải append-only và chỉ chứa dữ liệu đã che.
