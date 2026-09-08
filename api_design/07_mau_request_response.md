# Mẫu request/response trọng yếu

Các ID chỉ minh họa hợp đồng, không phải dữ liệu seed. Header `Authorization` được trình duyệt/session layer xử lý và không được log. Mọi response thực tế có `X-Correlation-ID`.

## 1. Tạo Asset Draft

```http
POST /api/v1/assets
Idempotency-Key: 01J74DSB6BYY8XFKS7Q7M33J7A
Content-Type: application/json
```

```json
{
  "code": "WEB-PORTAL-01",
  "name": "Cổng dịch vụ",
  "domain": "portal.example.invalid",
  "baseUrl": "https://portal.example.invalid",
  "environment": "Production",
  "ownerId": 120,
  "technicalTeamId": 8,
  "securityOfficerId": 34,
  "criticality": "Theo danh mục đã phê duyệt",
  "dataClassification": "Theo chính sách đơn vị",
  "internetExposed": true,
  "unitName": "Đơn vị A"
}
```

```http
HTTP/1.1 201 Created
ETag: "1"
X-Correlation-ID: 4de18426-2239-4e80-a0ab-57e1dd0c9a34
```

```json
{
  "id": 501,
  "version": "1",
  "code": "WEB-PORTAL-01",
  "name": "Cổng dịch vụ",
  "baseUrlMasked": "https://portal.example.invalid",
  "status": "Draft",
  "currentScopeId": null
}
```

## 2. Owner xác nhận scope

```http
POST /api/v1/assets/501/scopes/702/confirm
If-Match: "3"
Idempotency-Key: 01J74E10V66MVP1S6HMAZ1R0XD
Content-Type: application/json
```

```json
{"reason":"Đã kiểm tra sở hữu, mức quan trọng, URL và khung giờ."}
```

Kết quả: scope `Draft → Pending Approval`; nếu đây là tài sản mới thì Asset `Draft → Pending Approval`. Scope cũ của Asset đang Active vẫn có hiệu lực cho đến khi SECURITY phê duyệt revision mới.

## 3. Upload ZAP và replay chống trùng

```http
POST /api/v1/assets/501/scan-occurrences/904/imports
Idempotency-Key: 01J74EABF4TGMX4N0A6VQKJ8EW
Content-Type: multipart/form-data; boundary=...
```

Part `format` có giá trị `Traditional JSON`; part `file` là byte report. Lô mới trả `201` và `Received`. Nếu `(asset, scan, SHA-256 byte gốc)` đã tồn tại và actor vẫn có quyền, API trả `200` với chính lô cũ; không tiết lộ checksum để dò tệp.

```json
{
  "id": 1002,
  "version": "1",
  "assetId": 501,
  "scanId": 904,
  "format": "Traditional JSON",
  "status": "Received",
  "counts": {"total": 0, "succeeded": 0, "failed": 0}
}
```

## 4. SECURITY kết luận Closed

```http
POST /api/v1/verification-requests/1301/decision
If-Match: "12"
Idempotency-Key: 01J74EM1GNS71R0C7TGQZZZ1J4
Content-Type: application/json
```

```json
{
  "scanId": 905,
  "importId": 1003,
  "outcome": "Closed",
  "isValid": true,
  "coverageConfirmed": true,
  "fingerprintPresent": false,
  "reason": "Lần quét mới sau bản sửa bao phủ mục tiêu và không còn fingerprint.",
  "evidenceId": 1402
}
```

Máy chủ không tin riêng các boolean từ client: phải tự kiểm scan/import/asset/scope/timestamp/fingerprint và tách nhiệm vụ. Thành công tạo `VerificationResult`, chuyển Finding và yêu cầu, ghi audit trong cùng transaction.

## 5. RISK_APPROVER phê duyệt có thời hạn

```http
POST /api/v1/risk-acceptances/1501/decision
If-Match: "4"
Idempotency-Key: 01J74F0A88BVYQF19Z76N4AXQK
Content-Type: application/json
```

```json
{
  "decision": "Approve",
  "reason": "Chấp nhận theo căn cứ và điều kiện đã được phê duyệt.",
  "authorityRef": "Mã văn bản/thẩm quyền thực tế",
  "conditionsMasked": "Điều kiện đã che dữ liệu nhạy cảm.",
  "validFrom": "2026-09-08T00:00:00Z",
  "validUntil": "2026-10-08T00:00:00Z"
}
```

Ngày trên chỉ minh họa định dạng, không phải SLA/retention mặc định. Máy chủ kiểm người duyệt khác requester, đúng authority và khoảng hiệu lực hợp lệ trước `Pending Risk Acceptance → Risk Accepted`.

## 6. Lỗi stale version

```http
HTTP/1.1 412 Precondition Failed
Content-Type: application/problem+json
X-Correlation-ID: 12f4487e-ee47-4c48-aa12-7d9c8d38d740
```

```json
{
  "type": "https://errors.htqlhdk.invalid/CON-002",
  "title": "Không thể thực hiện yêu cầu",
  "status": 412,
  "code": "CON-002",
  "detail": "Dữ liệu đã thay đổi; hãy tải lại.",
  "correlationId": "12f4487e-ee47-4c48-aa12-7d9c8d38d740",
  "retryable": false
}
```
