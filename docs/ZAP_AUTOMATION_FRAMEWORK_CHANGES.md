# Bổ sung triển khai ZAP Automation Framework

## Mục đích

Tài liệu này là phần bổ sung duy nhất cho phương án hệ thống tự động điều phối OWASP ZAP. Các tài liệu yêu cầu, kiến trúc, API, database và hướng dẫn gốc được giữ nguyên để bảo toàn baseline đã phê duyệt. Khi phương án này được phê duyệt chính thức, nội dung tại đây phải được hợp nhất vào baseline bằng quy trình quản lý thay đổi.

## Thay đổi phạm vi

Hệ thống bổ sung khả năng tự động:

1. Gắn một `scan_schedule` với cấu hình ZAP có phiên bản.
2. Tạo một `scan_occurrence` khi lịch tự động đến hạn.
3. Sinh Automation Framework plan YAML từ Asset, Scope và ZAP profile đã phê duyệt.
4. Khởi chạy một container ZAP biệt lập cho mỗi lần quét.
5. Thu nhận Traditional JSON/HTML Report, kiểm checksum và lưu artifact.
6. Tạo `import_batch` ở trạng thái `Received` để luồng F-03 xử lý tiếp.
7. Ghi nhận trạng thái kỹ thuật, timeout, lỗi đã che và audit hệ thống.

Luồng quét thủ công cũ vẫn được giữ: lịch có `automation_enabled=false` không được Worker điều phối.

## Kiến trúc bổ sung

```text
scan_schedule
    -> scan_occurrence
    -> zap_execution
    -> Htqlhdk.Worker
    -> ZAP 2.17.0 container + Automation Framework plan
    -> Traditional JSON/HTML
    -> zap_artifact + import_batch Received
    -> F-03 parser
```

`Htqlhdk.Api` không giữ Docker socket, không nhận ZAP API key và không trực tiếp khởi chạy scanner. `Htqlhdk.Worker` là ranh giới điều phối. Bản triển khai đầu tiên dùng Docker CLI trên một runner host được kiểm soát; production nên thay `IZapRunner` bằng Kubernetes Job/ECS Task hoặc cơ chế orchestration có quyền tối thiểu.

## Database V002

Migration `V002__zap_automation.psql` thêm:

- `zap_scan_profile`: profile bất biến theo phiên bản; chỉ lưu tham chiếu secret dạng `env:HTQLHDK_ZAP_*`.
- `zap_execution`: trạng thái kỹ thuật, lease, heartbeat, image digest và snapshot cấu hình.
- `zap_artifact`: metadata báo cáo bất biến.
- `scan_schedule.automation_enabled` và `scan_schedule.zap_profile_id`.
- View `v_zap_execution_summary` và các trigger bảo vệ snapshot/artifact.

`scan_occurrence` tiếp tục giữ trạng thái nghiệp vụ. `zap_execution` giữ trạng thái chi tiết của container, nhờ đó không phá hợp đồng trạng thái cũ.

Một profile Active phải tham chiếu `service_user_id`. Đây là tài khoản dịch vụ không đăng nhập, không có mật khẩu và được dùng cho `scan_occurrence.performed_by` cùng `import_batch.imported_by` vì hai cột baseline yêu cầu FK tới `app_user`.

## Cấu hình mới

Các biến không bí mật:

| Biến | Ý nghĩa |
|---|---|
| `HTQLHDK_ZAP_ENABLED` | Bật Worker điều phối; mặc định `false`. |
| `HTQLHDK_DATABASE_URL` | Npgsql connection string của Worker. |
| `HTQLHDK_ZAP_IMAGE` | Image ZAP bắt buộc có digest. |
| `HTQLHDK_ZAP_WORK_ROOT` | Thư mục tạm sinh plan/báo cáo. |
| `HTQLHDK_ZAP_ARTIFACT_ROOT` | Kho artifact local cho môi trường phát triển. |
| `HTQLHDK_ZAP_DOCKER_NETWORK` | Network container được phép dùng. |

Secret header dùng tên bắt đầu bằng `HTQLHDK_ZAP_` và profile chỉ lưu tham chiếu, ví dụ `env:HTQLHDK_ZAP_AUTH_TOKEN`. Giá trị không được ghi vào YAML, database, log hoặc audit.

## Automation plan

Plan được sinh riêng theo `zap_execution`, gồm:

1. Context với URL và regex include/exclude lấy từ scope đã phê duyệt.
2. Traditional Spider hoặc Client Spider theo profile.
3. `passiveScan-wait`.
4. `activeScan` nếu profile bật và đã được phê duyệt.
5. `passiveScan-wait` lần cuối.
6. Traditional JSON và Traditional HTML report.

Worker không nhận YAML tùy ý từ người dùng và không bật ZAP API file transfer. Cách này tránh biến ZAP runner thành điểm thực thi script tùy ý.

## Trạng thái

`zap_execution` dùng chuỗi:

```text
Queued -> Preparing Plan -> Starting Runner -> Running
       -> Collecting Report -> Report Received
```

Trạng thái kết thúc lỗi: `Failed`, `Timed Out`, `Cancelled`.

Khi nhận báo cáo hợp lệ, Worker cập nhật `scan_occurrence` thành `Completed`, lưu artifact và tạo `import_batch` `Received` trong cùng giao dịch database. Luồng F-03 hiện hữu chịu trách nhiệm chuyển lô qua `Validating`, `Processing` và trạng thái cuối; Worker ZAP không tự đóng Finding.

## Kiểm soát an toàn

- Image ZAP khóa tại `2.17.0@sha256:781a2bdaea47324e7bab583e2263f21d257b0aee61ed51521a5be45f5f5081ef`.
- Chỉ lịch Active, Asset Active, Scope Approved và ZAP profile Active mới được dispatch.
- Mỗi scan dùng một container, work directory và snapshot riêng.
- Container có giới hạn CPU/RAM, `no-new-privileges` và không được kết nối database.
- Active scan mặc định tắt trong profile mới.
- Worker không log stdout/stderr thô của ZAP vì có thể chứa URL hoặc dữ liệu nhạy cảm.
- Report có giới hạn kích thước, kiểm JSON, nhận diện OWASP ZAP, checksum SHA-256 và đối chiếu host mục tiêu.
- Không tự retry một scan đã bắt đầu gửi request. `max_attempts` mặc định là 1.
- Worker phát hiện lease/heartbeat hết hạn, đánh dấu lần chạy là `Failed` và yêu cầu rà soát thủ công thay vì âm thầm quét lại.
- Báo cáo không còn Finding chỉ là bằng chứng; SECURITY vẫn phải xác nhận coverage trước khi đóng.

## Triển khai

1. Apply V001 rồi V002; migration cũ không bị sửa.
2. Tạo `app_user` dành cho service, không có password và không cấp role người dùng.
3. Tạo/duyệt `zap_scan_profile`; bắt đầu với passive scan.
4. Gắn profile vào lịch và đặt `automation_enabled=true` sau khi phê duyệt mục tiêu.
5. Cấp `HTQLHDK_DATABASE_URL`, thư mục work/artifact và secret cần thiết cho Worker.
6. Chạy Worker trên host có Docker CLI và quyền chỉ đủ để tạo/dừng ZAP container.
7. Xác minh với một website staging được phép trước khi bật active scan.

Môi trường production không nên mount Docker socket vào API hoặc Worker container tổng quát. Hãy chạy Worker trên runner chuyên dụng hoặc hiện thực một `IZapRunner` dùng orchestrator của nền tảng.

## Giới hạn hiện tại

- Artifact store đi kèm là filesystem dành cho local/pilot; production cần adapter object storage và mã hóa thật.
- Header token authentication được hỗ trợ; browser/form/TOTP cần profile renderer bổ sung và kiểm thử riêng.
- Worker tạo `import_batch` `Received`; parser F-03 đầy đủ vẫn thuộc lát cắt nghiệp vụ F-03 của giai đoạn 11.
- Scheduler hỗ trợ `FREQ=DAILY`, `WEEKLY`, `MONTHLY`, `INTERVAL`, một `BYDAY` cho lịch tuần và một `BYMONTHDAY` cho lịch tháng; các RRULE khác bị từ chối thay vì bị tự suy diễn.
