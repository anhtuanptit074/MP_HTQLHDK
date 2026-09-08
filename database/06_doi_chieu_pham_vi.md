# Đối chiếu phạm vi và tối ưu 43 → 27 bảng

Nguồn nghiệp vụ duy nhất: [doi_tuong_sd_quy_trinh_hd.md](../doi_tuong_sd_quy_trinh_hd.md) phiên bản 2.2. Tài liệu nguồn không bị chỉnh sửa để phù hợp với thiết kế. Các bảng mới là cách lưu các yêu cầu đã có, không phải danh sách chức năng mở rộng.

## 1. Cách giảm 16 bảng

| Phép gộp/bỏ bảng cấu hình | Trước | Sau | Giảm |
|---|---:|---:|---:|
| Bảng đơn vị → thuộc tính mô tả | 1 | 0 | 1 |
| Role/Permission/RolePermission → ENUM + view cố định | 3 | 0 | 3 |
| UserRole + quyền nhạy cảm | 2 | 1 | 1 |
| Hai bảng mục tiêu → cấu hình trong lịch/lần quét hiện có | 2 | 0 | 2 |
| ImportBatch + RawReport | 2 | 1 | 1 |
| SeverityAssessment + DispositionReview | 2 | 1 | 1 |
| SLAPolicy + SLARule | 2 | 1 | 1 |
| Assignment + TicketUpdate + DeadlineChange | 3 | 1 | 2 |
| RiskRequest + RiskDecision | 2 | 1 | 1 |
| AuditLog + StateHistory + RiskReviewEvent | 3 | 1 | 2 |
| ExportReport + DownloadGrant | 2 | 1 | 1 |
| **Tổng phần thay đổi** | **24** | **8** | **16** |

19 bảng còn lại tiếp tục dùng, gồm một bảng chỉ đổi tên `export_report_asset` thành `report_export_asset`. Tổng sau tối ưu: 19 + 8 = **27 bảng**. Số cột giảm ít hơn số bảng vì vẫn phải giữ bằng chứng, nguồn và lịch sử theo yêu cầu.

## 2. Ánh xạ từng bảng cũ

| Bảng trong bản 43 | Nơi lưu trong bản 27 | Thông tin được giữ/căn cứ |
|---|---|---|
| `org_unit` | thuộc tính `unit_name` | Đơn vị là dữ liệu mô tả ở người dùng, nhóm, tài sản, SLA; không quản lý cơ cấu tổ chức. |
| `app_user` | `app_user` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `team` | `team` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `team_member` | `team_member` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `app_role` | ENUM `user_role_code` | Bảy vai trò cố định, không xây chức năng tạo vai trò động. |
| `permission` | view `v_role_permission` | Danh sách hành động cố định trong SQL/ứng dụng. |
| `role_permission` | view `v_role_permission` | Ánh xạ quyền cố định; không mất kiểm vai trò + asset ở máy chủ. |
| `asset` | `asset` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `user_role` | `user_role` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `sensitive_access_grant` | `user_role` | Hai cờ quyền nhạy cảm tường minh, mặc định false, lý do, cấp/thu hồi và audit. |
| `asset_scope_revision` | `asset_scope_revision` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `asset_scope_rule` | `asset_scope_rule` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `asset_scan_window` | `asset_scan_window` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `scan_schedule` | `scan_schedule` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `schedule_target` | `scan_schedule.target_urls` | Danh sách URL cấu hình nhỏ; không chứa ID quan hệ hoặc bí mật. |
| `scan_occurrence` | `scan_occurrence` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `scan_target` | `scan_occurrence.target_urls` | Snapshot URL lần quét, độc lập cấu hình lịch sau này. |
| `raw_report` | `import_batch.raw_*` và `file_sha256` | Metadata bất biến ngay trong lô; tệp byte gốc giữ riêng trong kho mã hóa. |
| `import_batch` | `import_batch` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `import_error` | `import_error` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `finding` | `finding` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `finding_occurrence` | `finding_occurrence` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `finding_observation` | `finding_observation` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `evidence` | `evidence` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `severity_assessment` | `finding_review`, loại Severity | Giữ mức trước/sau, lý do, bối cảnh, bằng chứng, người đánh giá/rà soát và lịch sử. |
| `disposition_review` | `finding_review`, loại False Positive/Cancelled/Recheck | CHECK phân loại, giữ review/evidence/lần xuất hiện và kết luận. |
| `sla_policy` | `sla_rule` | Gộp mã phiên bản, đơn vị, hiệu lực, người/thời điểm duyệt vào dòng thời hạn. |
| `sla_rule` | `sla_rule` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `remediation_ticket` | `remediation_ticket` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `ticket_assignment` | `ticket_event`, loại Assignment | Mỗi lần phân công là snapshot bất biến, chuỗi cũ/mới và con trỏ hiện hành. |
| `ticket_update` | `ticket_event`, loại Progress | Giữ tiến độ, ghi chú qua reason_masked, nguyên nhân, hành động, evidence và tác giả bản sửa. |
| `ticket_deadline_change` | `ticket_event`, loại Deadline | Hạn cũ qua previous_event_id, hạn mới/dòng SLA/lý do/người/thời điểm ở sự kiện mới. |
| `verification_request` | `verification_request` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `verification_result` | `verification_result` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `risk_acceptance_request` | `risk_acceptance` | Giữ đề nghị, trạng thái trước và thông tin bổ sung; bổ sung/rà soát có audit. |
| `risk_decision` | `risk_acceptance` | Giữ người duyệt độc lập, thẩm quyền, điều kiện, thời hạn; gia hạn bằng bản ghi mới. |
| `risk_review_event` | `audit_log` có `risk_acceptance_id` | Giữ hành động rà soát, yêu cầu bổ sung, trước/sau, actor và thời gian. |
| `finding_state_history` | `audit_log` có `finding_id`, from/to và FK căn cứ | Lịch sử chuyển trạng thái truy vấn qua v_finding_history; ghi cùng giao dịch. |
| `audit_log` | `audit_log` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |
| `export_report` | `report_export` | Metadata tệp, nhãn, người/thời điểm xuất, masking và trace_code. |
| `export_report_asset` | `report_export_asset` | Giữ bảng nối quan hệ cho báo cáo nhiều tài sản; chỉ đổi tên thống nhất. |
| `download_grant` | `report_export` | Người nhận, token hash, hạn, thu hồi cùng lần phát hành; phát hành lại tạo dòng mới. |
| `notification` | `notification` | Giữ bảng; cập nhật FK tới các bảng đã gộp khi cần. |

## 3. Căn cứ của toàn bộ 27 bảng

| Bảng | Vai trò trong đề tài | Mục tài liệu nguồn |
|---|---|---|
| `app_user` | Người dùng | 3; 8.2 |
| `team` | Nhóm công tác | 3; 5.1; 5.2; 5.4 |
| `team_member` | Thành viên nhóm | 3; 5.4 |
| `user_role` | Vai trò, phạm vi tài sản và quyền nhạy cảm | 3; 8.2 |
| `asset` | Tài sản website | 5.1 |
| `asset_scope_revision` | Phiên bản phạm vi quét | 5.1.2; 8.5 |
| `asset_scope_rule` | Mục cho phép hoặc loại trừ | 5.1; 5.3.3 |
| `asset_scan_window` | Khung giờ quét được phép | 5.1; 5.2 |
| `scan_schedule` | Lịch quét định kỳ | 5.2 |
| `scan_occurrence` | Lần quét cụ thể | 5.2; 7.5 |
| `import_batch` | Lô nhập và metadata báo cáo ZAP gốc | 5.3; 8.2–8.3 |
| `import_error` | Lỗi lô hoặc bản ghi | 5.3.6; 7.6 |
| `finding` | Phát hiện chuẩn hóa | 5.3.4–5.3.5; 6; 7 |
| `finding_occurrence` | Lần xuất hiện trong một lần quét | 2; 5.3.5 |
| `finding_observation` | Chi tiết cảnh báo nguồn theo lô | 5.3.4; BR-04; BR-06 |
| `evidence` | Bằng chứng của phát hiện | 5.4; 6; 7; 8 |
| `finding_review` | Đánh giá mức độ và kết luận phát hiện | 6; 7.2–7.3 |
| `sla_rule` | Thời hạn được phê duyệt theo mức độ | 6.5 |
| `remediation_ticket` | Phiếu khắc phục | 5.4; 7.6 |
| `ticket_event` | Lịch sử phân công, tiến độ và thời hạn phiếu | 5.4; 6.5; 7.2; 7.5 |
| `verification_request` | Yêu cầu xác minh khắc phục | 7.5 |
| `verification_result` | Kết quả đánh giá xác minh | 7.2; 7.5 |
| `risk_acceptance` | Hồ sơ đề nghị và quyết định chấp nhận rủi ro | 3.2; 7.2; 7.4 |
| `audit_log` | Nhật ký chung, lịch sử workflow và thay đổi hồ sơ | 7.2; 8.5 |
| `report_export` | Tệp xuất và liên kết tải có hạn | 8.4 |
| `report_export_asset` | Tài sản được đưa vào báo cáo xuất | 8.4 |
| `notification` | Thông báo tối thiểu trong hệ thống | 9.1 |

## 4. Phần không đưa trở lại

Không có bảng công cụ quét đa nhà cung cấp, điều khiển ZAP/API, CI/CD, kho mã nguồn, ticket ngoài, cấu hình SSO/email/BI, lịch làm việc/lịch nghỉ hoặc quản lý cơ cấu tổ chức. Không tự đặt thời hạn SLA.

Không có quyền tạo tài sản theo phạm vi đơn vị; tên đơn vị là mô tả, không phải điều kiện cấp quyền. Không dùng UNIQUE để áp đặt một phiếu cho toàn bộ vòng đời Finding, một review/risk request/verification request đang chờ, hoặc một kết quả duy nhất cho yêu cầu xác minh. Vẫn phải chống phiếu trùng do nhập trùng và kiểm đúng hồ sơ hiện hành trước khi áp dụng.

Không cấm một người xác minh chỉ vì họ từng được phân công trong quá khứ. Kiểm tách nhiệm vụ dựa trên người thực sự khắc phục và quyền ATTT. Không thêm điều kiện quét xác minh phải diễn ra sau thời điểm lập yêu cầu; nguồn yêu cầu báo cáo mới sau khắc phục.

Các lựa chọn như identity PK, ENUM, FK ghép, mảng URL cấu hình, chỉ mục, checksum, snapshot và view là cách triển khai lưu trữ. Chúng không bổ sung ca sử dụng. Tệp SQL là thiết kế tạo mới; ánh xạ trên là giải thích mô hình, chưa phải chương trình chuyển dữ liệu thật từ bản 43 bảng.
