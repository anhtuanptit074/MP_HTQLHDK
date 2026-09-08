-- Vai trò/quyền cố định của đề tài: VIEW, không tạo bảng cấu hình quyền tổng quát.
-- Không tạo tài khoản, không cấp quyền người dùng, không tự đặt thời hạn SLA.
BEGIN;
SET search_path = htqlhdk, public;
CREATE VIEW v_role_permission AS
SELECT role_code::user_role_code AS role_code,permission_code
FROM (VALUES
 ('ADMIN',ARRAY['access.manage','audit.permission.read']),
 ('SECURITY',ARRAY['asset.read','asset.declare','asset.update','asset.approve','schedule.read','schedule.manage','import.read','finding.read','finding.assess','finding.disposition','review.second','ticket.read','ticket.assign','verification.request','verification.decide','risk.request','risk.recommend','audit.business.read','report.export','report.download','report.revoke']),
 ('OWNER',ARRAY['asset.read','asset.declare','asset.update','asset.confirm','schedule.read','schedule.confirm_time','import.read','finding.read','ticket.read','risk.request','report.export','report.download','report.revoke']),
 ('SCAN_OPS',ARRAY['asset.read','schedule.read','scan.record','import.create','import.read','finding.read','ticket.read','report.export','report.download','report.revoke']),
 ('REMEDIATOR',ARRAY['asset.read','schedule.read','import.read','finding.read','ticket.read','ticket.update','report.export','report.download','report.revoke']),
 ('RISK_APPROVER',ARRAY['asset.read','schedule.read','import.read','finding.read','ticket.read','risk.decide','review.second','report.export','report.download','report.revoke']),
 ('AUDITOR',ARRAY['asset.read','schedule.read','import.read','finding.read','ticket.read','audit.permission.read','audit.business.read','report.export','report.download','report.revoke'])
) AS r(role_code,permissions) CROSS JOIN LATERAL unnest(permissions) AS p(permission_code);
COMMENT ON VIEW v_role_permission IS 'Ma trận vai trò cố định; cần asset_id khớp quyền hiện hành. REMEDIATOR còn bị giới hạn phiếu được giao. review.second cần thẩm quyền/chính sách cụ thể. Không tự cấp raw report.';
REVOKE ALL ON v_role_permission FROM PUBLIC;
COMMIT;
