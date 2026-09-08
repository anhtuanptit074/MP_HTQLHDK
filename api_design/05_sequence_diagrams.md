# Sequence diagram bắt buộc — Giai đoạn 8

Các sơ đồ là nguồn Mermaid có thể chỉnh sửa. `AuthZ` luôn kiểm permission + asset + owner/task scope; `Audit` Success nằm trong transaction, Denied/Failure ở transaction riêng.

## 1. Import ZAP, chống trùng và xử lý từng record

```mermaid
sequenceDiagram
    autonumber
    actor Ops as SCAN_OPS
    participant API as Import API
    participant Auth as AuthZ
    participant Obj as Private Object/KMS
    participant DB as PostgreSQL
    participant W as Import Worker

    Ops->>API: POST .../imports + file + Idempotency-Key
    API->>Auth: import.create(asset, scan)
    Auth->>DB: active grant + Asset Active + Scan Completed
    alt denied / wrong scope
        Auth->>DB: independent audit Denied
        API-->>Ops: 403/422 + correlationId
    else allowed
        API->>Obj: stream quarantine; size/hash/type
        Obj-->>API: private immutable object ref
        API->>DB: TX lock asset/scan; dedupe(asset,scan,sha256)
        alt duplicate
            DB-->>API: existing authorized batch
            API->>Obj: discard/reconcile quarantine object
            API-->>Ops: 200 existing batch
        else new
            API->>DB: INSERT batch Received + audit Success; COMMIT
            API-->>Ops: 201 batch Received
            W->>DB: claim Received idempotently
            W->>Obj: read + verify checksum
            W->>DB: status Validating/Processing
            loop each flattened Alert instance
                W->>DB: SAVEPOINT; normalize + HMAC fingerprint
                W->>DB: get/create Finding + Occurrence; INSERT masked Observation
                alt record invalid
                    W->>DB: rollback savepoint; INSERT masked ImportError
                else Closed fingerprint reappears
                    W->>DB: Closed→Reopened + audit
                end
            end
            W->>DB: reconcile counts; finalize Completed/Partial + scan Result Imported + audit
        end
    end
```

## 2. Kết luận Closed/Reopened

```mermaid
sequenceDiagram
    autonumber
    actor Sec as SECURITY
    participant API as Verification API
    participant Auth as AuthZ/SoD
    participant DB as PostgreSQL
    participant Obj as Private Object
    participant N as Notification Worker

    Sec->>API: POST /verification-requests/{id}/decision<br/>If-Match + Idempotency-Key
    API->>Auth: verification.decide(asset); evaluator independence
    Auth->>DB: current grant + remediation participants
    alt denied
        Auth->>DB: independent audit Denied
        API-->>Sec: 403 VER-004/AUTH-003
    else allowed
        API->>DB: BEGIN; lock asset→finding→ticket event→request→batch
        API->>DB: verify Pending Verification + version + current cycle
        API->>Obj: verify raw object checksum if evidence read required
        API->>DB: verify scan after remediation, asset/scope, import and coverage
        alt valid + covered + fingerprint absent
            API->>DB: INSERT VerificationResult(Closed)
            API->>DB: Finding→Closed; Request→Evaluated; audit Success
        else invalid or fingerprint present
            API->>DB: INSERT VerificationResult(Reopened)
            API->>DB: Finding→Reopened; Request→Evaluated; audit Success
        end
        alt audit/write fails
            API->>DB: ROLLBACK
            API->>DB: independent audit Failure
            API-->>Sec: 503/500
        else commit
            API->>DB: COMMIT
            API-->>Sec: 200 Finding + new ETag
            N->>DB: create minimal notification idempotently
        end
    end
```

## 3. Risk Approval, từ chối và yêu cầu bổ sung

```mermaid
sequenceDiagram
    autonumber
    actor Req as OWNER/SECURITY requester
    actor App as RISK_APPROVER
    participant API as Risk API
    participant Auth as AuthZ/Authority/SoD
    participant DB as PostgreSQL
    participant N as Notification Worker

    Req->>API: POST /findings/{id}/risk-acceptances
    API->>Auth: risk.request + asset scope
    API->>DB: TX lock finding; verify Triaged/Assigned/In Progress
    API->>DB: INSERT Pending(previous status); Finding→Pending Risk Acceptance; audit; COMMIT
    API-->>Req: 201 Pending

    App->>API: POST /risk-acceptances/{id}/decision
    API->>Auth: risk.decide + authority + approver != requester
    alt denied
        Auth->>DB: independent audit Denied
        API-->>App: 403 RSK-002
    else authorized
        API->>DB: BEGIN; lock asset→finding→risk
        alt Needs Information
            API->>DB: risk→Needs Information + masked audit; COMMIT
            API-->>App: 200 Needs Information
        else Approve with valid conditions/time
            API->>DB: risk→Approved + applied_at
            API->>DB: Finding→Risk Accepted + linked audit; COMMIT
            API-->>App: 200 Approved
        else Reject
            API->>DB: risk→Rejected
            API->>DB: Finding→exact previous status + linked audit; COMMIT
            API-->>App: 200 Rejected
        end
        N->>DB: minimal notification idempotently
    end
```

## 4. Mở lại do tái xuất hiện hoặc hết điều kiện rủi ro

```mermaid
sequenceDiagram
    autonumber
    participant W as Import/Expiry Worker
    participant DB as PostgreSQL
    participant N as Notification Worker
    W->>DB: claim occurrence/risk record idempotently
    W->>DB: BEGIN; lock asset→finding→evidence record
    alt Closed + new fingerprint after close
        W->>DB: INSERT occurrence/observation
        W->>DB: Finding Closed→Reopened + audit
    else Risk Accepted expired/violated/risk increased
        W->>DB: verify latest applied decision and rule
        W->>DB: Finding Risk Accepted→Reopened + audit
    end
    W->>DB: COMMIT
    N->>DB: notify SECURITY/OWNER/assigned team (minimal content)
```
