-- Các guard được thực thi ở DB; đây không thay thế tầng phân quyền/workflow của ứng dụng.
BEGIN;
SET search_path = htqlhdk, public;

CREATE FUNCTION deny_delete() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  RAISE EXCEPTION 'Không xóa cứng dữ liệu lịch sử: %', TG_TABLE_NAME USING ERRCODE = '23514';
END $$;
CREATE FUNCTION deny_mutation() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  RAISE EXCEPTION 'Bản ghi bất biến: %', TG_TABLE_NAME USING ERRCODE = '23514';
END $$;
DO $$ DECLARE r record;
BEGIN
  FOR r IN SELECT tablename FROM pg_tables WHERE schemaname = 'htqlhdk' LOOP
    EXECUTE format('CREATE TRIGGER no_hard_delete BEFORE DELETE ON htqlhdk.%I FOR EACH ROW EXECUTE FUNCTION htqlhdk.deny_delete()', r.tablename);
    EXECUTE format('CREATE TRIGGER no_truncate BEFORE TRUNCATE ON htqlhdk.%I FOR EACH STATEMENT EXECUTE FUNCTION htqlhdk.deny_delete()', r.tablename);
  END LOOP;
END $$;
DO $$ DECLARE n text;
BEGIN
  FOREACH n IN ARRAY ARRAY['finding_observation','finding_occurrence','evidence','import_error',
    'ticket_event','audit_log','verification_result','report_export_asset','sla_rule'] LOOP
    EXECUTE format('CREATE TRIGGER immutable_record BEFORE UPDATE ON htqlhdk.%I FOR EACH ROW EXECUTE FUNCTION htqlhdk.deny_mutation()', n);
  END LOOP;
END $$;

CREATE FUNCTION touch_updated_at() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN NEW.updated_at := CURRENT_TIMESTAMP; RETURN NEW; END $$;
CREATE TRIGGER asset_updated BEFORE UPDATE ON asset FOR EACH ROW EXECUTE FUNCTION touch_updated_at();
CREATE TRIGGER schedule_updated BEFORE UPDATE ON scan_schedule FOR EACH ROW EXECUTE FUNCTION touch_updated_at();

CREATE FUNCTION guard_scope_child() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
DECLARE scope_status text; sid bigint;
BEGIN
  IF TG_OP = 'UPDATE' THEN
    IF NEW.scope_id <> OLD.scope_id THEN
      RAISE EXCEPTION 'Không chuyển mục phạm vi sang phiên bản khác' USING ERRCODE = '23514';
    END IF;
  END IF;
  sid := NEW.scope_id;
  SELECT status INTO scope_status FROM asset_scope_revision WHERE id = sid FOR UPDATE;
  IF scope_status IN ('Approved','Rejected') THEN
    RAISE EXCEPTION 'Phạm vi đã có quyết định được giữ để truy vết; thay đổi dùng phiên bản mới' USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER protect_scope_rule BEFORE INSERT OR UPDATE ON asset_scope_rule FOR EACH ROW EXECUTE FUNCTION guard_scope_child();
CREATE TRIGGER protect_scan_window BEFORE INSERT OR UPDATE ON asset_scan_window FOR EACH ROW EXECUTE FUNCTION guard_scope_child();

CREATE FUNCTION guard_scope_revision() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  IF TG_OP = 'INSERT' AND NEW.status <> 'Draft' THEN
    RAISE EXCEPTION 'Khởi tạo phạm vi ở Draft trước khi thêm rule và khung giờ' USING ERRCODE = '23514';
  END IF;
  IF TG_OP = 'UPDATE' THEN
    IF OLD.status IN ('Approved','Rejected') THEN
      RAISE EXCEPTION 'Quyết định phạm vi bất biến; tạo phiên bản mới' USING ERRCODE = '23514';
    END IF;
    IF NEW.asset_id <> OLD.asset_id OR NEW.version_no <> OLD.version_no THEN
      RAISE EXCEPTION 'Không đổi định danh phiên bản phạm vi' USING ERRCODE = '23514';
    END IF;
    IF NEW.status = 'Approved' AND OLD.status <> 'Pending Approval' THEN
      RAISE EXCEPTION 'Phạm vi cần gửi Pending Approval trước phê duyệt' USING ERRCODE = '23514';
    END IF;
  END IF;
  IF NEW.status IN ('Pending Approval','Approved') THEN
    IF NOT EXISTS (SELECT 1 FROM asset_scope_rule WHERE scope_id = NEW.id AND effect = 'Include')
      OR NOT EXISTS (SELECT 1 FROM asset_scan_window WHERE scope_id = NEW.id) THEN
      RAISE EXCEPTION 'Thiếu phạm vi Include hoặc khung giờ quét' USING ERRCODE = '23514';
    END IF;
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER protect_scope_revision BEFORE INSERT OR UPDATE ON asset_scope_revision FOR EACH ROW EXECUTE FUNCTION guard_scope_revision();

CREATE FUNCTION guard_asset_scope() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  IF NEW.current_scope_id IS NOT NULL THEN
    PERFORM 1 FROM asset_scope_revision WHERE id = NEW.current_scope_id AND asset_id = NEW.id AND status = 'Approved' FOR SHARE;
    IF NOT FOUND THEN
      RAISE EXCEPTION 'Phạm vi hiện hành phải đã được duyệt và cùng tài sản' USING ERRCODE = '23514';
    END IF;
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_asset_scope BEFORE INSERT OR UPDATE ON asset FOR EACH ROW EXECUTE FUNCTION guard_asset_scope();

CREATE FUNCTION guard_active_schedule() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  IF NEW.status = 'Active' THEN
    IF cardinality(NEW.target_urls) = 0 THEN RAISE EXCEPTION 'Lịch Active cần URL mục tiêu' USING ERRCODE = '23514'; END IF;
    PERFORM 1 FROM asset a JOIN asset_scope_revision s ON s.id = NEW.scope_id AND s.asset_id = a.id
      WHERE a.id = NEW.asset_id AND a.status = 'Active' AND a.current_scope_id = s.id AND s.status = 'Approved' FOR SHARE OF a,s;
    IF NOT FOUND THEN
      RAISE EXCEPTION 'Lịch Active cần tài sản Active và phạm vi hiện hành đã duyệt' USING ERRCODE = '23514';
    END IF;
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_active_schedule BEFORE INSERT OR UPDATE ON scan_schedule FOR EACH ROW EXECUTE FUNCTION guard_active_schedule();

CREATE FUNCTION guard_import_asset() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  PERFORM 1 FROM asset a JOIN scan_occurrence s ON s.asset_id = a.id
    WHERE a.id = NEW.asset_id AND a.status = 'Active' AND s.id = NEW.scan_id
      AND s.status IN ('Completed','Result Imported') FOR SHARE OF a,s;
  IF NOT FOUND THEN
    RAISE EXCEPTION 'Nhập báo cáo cần tài sản Active, đúng lần quét đã hoàn thành' USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_import_asset BEFORE INSERT ON import_batch FOR EACH ROW EXECUTE FUNCTION guard_import_asset();

CREATE FUNCTION guard_observation_batch() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
DECLARE batch_status import_status;
BEGIN
  SELECT status INTO batch_status FROM import_batch WHERE id = NEW.batch_id FOR UPDATE;
  IF batch_status IS DISTINCT FROM 'Processing'::import_status THEN
    RAISE EXCEPTION 'Chỉ ghi observation khi lô Processing' USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_observation_batch BEFORE INSERT ON finding_observation FOR EACH ROW EXECUTE FUNCTION guard_observation_batch();


CREATE FUNCTION guard_batch_source() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  IF NEW.status = 'Rejected' AND EXISTS (SELECT 1 FROM finding_observation WHERE batch_id=NEW.id) THEN
    RAISE EXCEPTION 'Lô đã có dòng hợp lệ không thể chuyển Rejected' USING ERRCODE='23514';
  END IF;
  IF TG_OP='UPDATE' AND ROW(NEW.id,NEW.code,NEW.asset_id,NEW.scan_id,NEW.file_sha256,NEW.imported_by,NEW.created_at,
    NEW.raw_object_key,NEW.raw_original_name,NEW.raw_size_bytes,NEW.raw_format,NEW.raw_key_ref)
    IS DISTINCT FROM ROW(OLD.id,OLD.code,OLD.asset_id,OLD.scan_id,OLD.file_sha256,OLD.imported_by,OLD.created_at,
    OLD.raw_object_key,OLD.raw_original_name,OLD.raw_size_bytes,OLD.raw_format,OLD.raw_key_ref) THEN
    RAISE EXCEPTION 'Nguồn và metadata báo cáo gốc bất biến; chỉ cập nhật tiến trình nhập' USING ERRCODE='23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER protect_batch_source BEFORE INSERT OR UPDATE ON import_batch FOR EACH ROW EXECUTE FUNCTION guard_batch_source();

CREATE FUNCTION guard_review() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
DECLARE f finding; old_level final_severity; level_for_fp final_severity;
BEGIN
  IF TG_OP='UPDATE' THEN
    IF OLD.status IN ('Applied','Rejected') THEN RAISE EXCEPTION 'Kết luận phân tích bất biến' USING ERRCODE='23514'; END IF;
    IF NEW.finding_id <> OLD.finding_id OR NEW.review_kind <> OLD.review_kind THEN
      RAISE EXCEPTION 'Không chuyển hồ sơ phân tích sang Finding hoặc loại khác' USING ERRCODE='23514';
    END IF;
  END IF;
  SELECT * INTO f FROM finding WHERE id=NEW.finding_id FOR UPDATE;
  SELECT proposed_severity INTO old_level FROM finding_review WHERE id=f.current_review_id;
  IF NEW.review_kind='Severity' THEN
    IF NEW.status='Applied' THEN
      IF NEW.previous_review_id IS DISTINCT FROM f.current_review_id OR NEW.old_severity IS DISTINCT FROM old_level THEN
        RAISE EXCEPTION 'Đánh giá phải dựa trên mức độ hiện hành' USING ERRCODE='23514';
      END IF;
      IF NULLIF(btrim(NEW.context_masked),'') IS NULL THEN
        RAISE EXCEPTION 'Xác nhận mức độ cần bối cảnh tài sản' USING ERRCODE='23514';
      END IF;
    END IF;
    IF NEW.old_severity IN ('High','Critical') AND NEW.proposed_severity < NEW.old_severity THEN
      NEW.second_review_required := true;
    END IF;
  ELSIF NEW.review_kind='False Positive' THEN
    level_for_fp := old_level;
    IF level_for_fp IS NULL THEN
      SELECT max(o.source_severity)::text::final_severity INTO level_for_fp
        FROM finding_observation o JOIN finding_occurrence c ON c.id=o.occurrence_id WHERE c.finding_id=NEW.finding_id;
    END IF;
    IF level_for_fp IN ('High','Critical') THEN NEW.second_review_required := true; END IF;
  END IF;
  IF NEW.trigger_occurrence_id IS NOT NULL AND NOT EXISTS
    (SELECT 1 FROM finding_occurrence WHERE id=NEW.trigger_occurrence_id AND finding_id=NEW.finding_id) THEN
    RAISE EXCEPTION 'Lần xuất hiện không thuộc Finding' USING ERRCODE='23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_review BEFORE INSERT OR UPDATE ON finding_review FOR EACH ROW EXECUTE FUNCTION guard_review();

CREATE FUNCTION guard_ticket_event() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
DECLARE t remediation_ticket; p ticket_event;
BEGIN
  SELECT * INTO t FROM remediation_ticket WHERE id=NEW.ticket_id FOR UPDATE;
  IF NEW.previous_event_id IS DISTINCT FROM t.current_event_id THEN
    RAISE EXCEPTION 'Phiếu đã thay đổi; lấy bản hiện hành trước khi ghi sự kiện' USING ERRCODE='23514';
  END IF;
  SELECT * INTO p FROM ticket_event WHERE id=NEW.previous_event_id;
  IF p.id IS NOT NULL AND NEW.event_type IN ('Assignment','Deadline') AND
    ROW(NEW.progress_percent,NEW.root_cause_masked,NEW.remediation_action_masked,NEW.evidence_id,NEW.remediated_by,NEW.remediated_at)
    IS DISTINCT FROM ROW(p.progress_percent,p.root_cause_masked,p.remediation_action_masked,p.evidence_id,p.remediated_by,p.remediated_at) THEN
    RAISE EXCEPTION 'Đổi phân công/hạn phải giữ nguyên bản khắc phục; cập nhật khắc phục dùng Progress' USING ERRCODE='23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_ticket_event BEFORE INSERT ON ticket_event FOR EACH ROW EXECUTE FUNCTION guard_ticket_event();

CREATE FUNCTION guard_ticket_current() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
DECLARE e ticket_event;
BEGIN
  IF ROW(NEW.id,NEW.code,NEW.finding_id,NEW.created_by,NEW.created_at) IS DISTINCT FROM
     ROW(OLD.id,OLD.code,OLD.finding_id,OLD.created_by,OLD.created_at) THEN
    RAISE EXCEPTION 'Định danh phiếu bất biến' USING ERRCODE='23514';
  END IF;
  IF NEW.current_event_id IS DISTINCT FROM OLD.current_event_id THEN
    SELECT * INTO e FROM ticket_event WHERE id=NEW.current_event_id AND ticket_id=NEW.id AND finding_id=NEW.finding_id;
    IF e.id IS NULL OR e.previous_event_id IS DISTINCT FROM OLD.current_event_id THEN
      RAISE EXCEPTION 'Bản hiện hành phải nối tiếp sự kiện trước của cùng phiếu' USING ERRCODE='23514';
    END IF;
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_ticket_current BEFORE UPDATE ON remediation_ticket FOR EACH ROW EXECUTE FUNCTION guard_ticket_current();

CREATE FUNCTION guard_risk() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  IF TG_OP='UPDATE' THEN
    IF ROW(NEW.finding_id,NEW.requested_by,NEW.requested_at,NEW.renewal_of_id,NEW.previous_finding_status)
      IS DISTINCT FROM ROW(OLD.finding_id,OLD.requested_by,OLD.requested_at,OLD.renewal_of_id,OLD.previous_finding_status) THEN
      RAISE EXCEPTION 'Không thay định danh/người đề nghị của hồ sơ rủi ro' USING ERRCODE='23514';
    END IF;
    IF OLD.status IN ('Approved','Rejected') AND NOT
      (OLD.status='Approved' AND OLD.applied_at IS NULL AND NEW.applied_at IS NOT NULL AND
       (to_jsonb(NEW)-'applied_at')=(to_jsonb(OLD)-'applied_at')) THEN
      RAISE EXCEPTION 'Quyết định rủi ro bất biến; gia hạn bằng hồ sơ mới' USING ERRCODE='23514';
    END IF;
  END IF;
  IF NEW.renewal_of_id IS NOT NULL AND NOT EXISTS
    (SELECT 1 FROM risk_acceptance WHERE id=NEW.renewal_of_id AND finding_id=NEW.finding_id AND status='Approved' AND applied_at IS NOT NULL) THEN
    RAISE EXCEPTION 'Gia hạn phải trỏ hồ sơ đã duyệt và đã áp dụng của cùng Finding' USING ERRCODE='23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_risk BEFORE INSERT OR UPDATE ON risk_acceptance FOR EACH ROW EXECUTE FUNCTION guard_risk();

CREATE FUNCTION guard_export() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
  IF (to_jsonb(NEW)-ARRAY['revoked_by','revoked_at']) IS DISTINCT FROM
     (to_jsonb(OLD)-ARRAY['revoked_by','revoked_at']) OR OLD.revoked_at IS NOT NULL THEN
    RAISE EXCEPTION 'Tệp xuất và liên kết bất biến, chỉ cho phép thu hồi lần đầu' USING ERRCODE='23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER protect_export BEFORE UPDATE ON report_export FOR EACH ROW EXECUTE FUNCTION guard_export();
CREATE FUNCTION guard_finding() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
DECLARE edge_allowed boolean;
BEGIN
  IF NEW.current_review_id IS NOT NULL THEN
    PERFORM 1 FROM finding_review WHERE id = NEW.current_review_id AND finding_id = NEW.id AND status = 'Applied' AND review_kind = 'Severity';
    IF NOT FOUND THEN RAISE EXCEPTION 'Final Severity phải trỏ đến đánh giá Applied cùng Finding' USING ERRCODE = '23514'; END IF;
  END IF;
  IF TG_OP = 'INSERT' THEN
    IF NEW.status <> 'New' THEN RAISE EXCEPTION 'Finding phải khởi tạo New' USING ERRCODE = '23514'; END IF;
  ELSE
    IF NEW.asset_id <> OLD.asset_id OR NEW.fingerprint <> OLD.fingerprint OR NEW.identity_version <> OLD.identity_version OR NEW.identity_key_ref <> OLD.identity_key_ref OR NEW.identity_ciphertext <> OLD.identity_ciphertext THEN
      RAISE EXCEPTION 'Định danh Finding bất biến; đổi thuật toán/khóa cần migration được kiểm soát' USING ERRCODE = '23514';
    END IF;
    IF NEW.status <> OLD.status THEN
      edge_allowed := CASE OLD.status
        WHEN 'New' THEN NEW.status IN ('Triaged','False Positive','Cancelled')
        WHEN 'Triaged' THEN NEW.status IN ('Assigned','Pending Risk Acceptance')
        WHEN 'Assigned' THEN NEW.status IN ('In Progress','Pending Risk Acceptance')
        WHEN 'In Progress' THEN NEW.status IN ('Pending Verification','Pending Risk Acceptance')
        WHEN 'Pending Verification' THEN NEW.status IN ('Closed','Reopened')
        WHEN 'Closed' THEN NEW.status = 'Reopened'
        WHEN 'Pending Risk Acceptance' THEN NEW.status IN ('Risk Accepted','Triaged','Assigned','In Progress')
        WHEN 'Risk Accepted' THEN NEW.status = 'Reopened'
        WHEN 'Reopened' THEN NEW.status = 'Assigned'
        ELSE false END;
      IF NEW.status = 'Triaged' AND NEW.current_review_id IS NULL THEN RAISE EXCEPTION 'Triaged cần Final Severity đã xác nhận' USING ERRCODE='23514'; END IF;
      IF OLD.status = 'Pending Verification' THEN
        PERFORM 1 FROM verification_result WHERE id=NEW.verification_result_id AND finding_id=NEW.id AND outcome=NEW.status::text;
        IF NOT FOUND THEN RAISE EXCEPTION 'Đóng/mở sau xác minh cần kết quả tương ứng' USING ERRCODE='23514'; END IF;
      END IF;
      IF NOT edge_allowed THEN RAISE EXCEPTION 'Cạnh chuyển trạng thái không có trong nguồn: % -> %', OLD.status, NEW.status USING ERRCODE = '23514'; END IF;
    END IF;
    NEW.row_version := OLD.row_version + 1;
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_finding BEFORE INSERT OR UPDATE ON finding FOR EACH ROW EXECUTE FUNCTION guard_finding();

CREATE FUNCTION guard_verification_result() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
DECLARE req verification_request; scan scan_occurrence; batch import_batch; fix ticket_event;
BEGIN
  SELECT * INTO req FROM verification_request WHERE id = NEW.request_id FOR UPDATE;
  SELECT * INTO scan FROM scan_occurrence WHERE id = NEW.scan_id FOR SHARE;
  SELECT * INTO fix FROM ticket_event WHERE id = req.remediation_event_id;
  IF NEW.is_valid THEN
    SELECT * INTO batch FROM import_batch WHERE id = NEW.batch_id FOR SHARE;
    IF batch.id IS NULL OR batch.status NOT IN ('Completed','Partially Completed') THEN
      RAISE EXCEPTION 'Lô lỗi hoặc chưa hoàn tất không đủ căn cứ xác minh hợp lệ' USING ERRCODE = '23514';
    END IF;
    IF fix.remediated_at IS NULL OR fix.remediated_by IS NULL OR scan.purpose <> 'Verification' OR scan.started_at IS NULL OR scan.started_at < fix.remediated_at
      OR scan.status NOT IN ('Completed','Result Imported') THEN
      RAISE EXCEPTION 'Cần lần quét xác minh mới sau bản khắc phục' USING ERRCODE = '23514';
    END IF;
    IF fix.evidence_id IS NULL OR NULLIF(btrim(fix.remediation_action_masked),'') IS NULL THEN
      RAISE EXCEPTION 'Thiếu biện pháp/bằng chứng khắc phục' USING ERRCODE = '23514';
    END IF;
    IF NEW.fingerprint_present IS FALSE AND EXISTS (
      SELECT 1 FROM finding_occurrence o JOIN finding_observation b ON b.occurrence_id = o.id
      WHERE o.finding_id = NEW.finding_id AND o.scan_id = NEW.scan_id) THEN
      RAISE EXCEPTION 'Fingerprint vẫn được ghi nhận trong lần quét xác minh' USING ERRCODE = '23514';
    END IF;
  END IF;
  IF NEW.outcome = 'Closed' AND NEW.evaluated_by = fix.remediated_by THEN
    RAISE EXCEPTION 'Người cập nhật bản khắc phục không tự xác nhận đóng' USING ERRCODE = '23514';
  END IF;
  RETURN NEW;
END $$;
CREATE TRIGGER check_verification_result BEFORE INSERT ON verification_result FOR EACH ROW EXECUTE FUNCTION guard_verification_result();


-- Audit là bảng ghi sự kiện, không phải cơ chế tự ghi mọi truy cập.
-- Ứng dụng bắt buộc ghi thay đổi + audit cùng giao dịch; truy cập bị từ chối ghi ở giao dịch độc lập.
CREATE FUNCTION guard_audit_occurrence() RETURNS trigger LANGUAGE plpgsql SET search_path = htqlhdk, pg_temp AS $$
BEGIN
 IF NEW.trigger_occurrence_id IS NOT NULL AND NOT EXISTS
   (SELECT 1 FROM finding_occurrence WHERE id=NEW.trigger_occurrence_id AND finding_id=NEW.finding_id) THEN
   RAISE EXCEPTION 'Audit phải liên kết lần xuất hiện cùng Finding' USING ERRCODE='23514';
 END IF;
 RETURN NEW;
END $$;
CREATE TRIGGER check_audit_occurrence BEFORE INSERT ON audit_log FOR EACH ROW EXECUTE FUNCTION guard_audit_occurrence();

-- Các view chỉ chọn dữ liệu đã che; vẫn phải lọc quyền ở ứng dụng cho từng truy vấn.
CREATE VIEW v_finding_current AS
SELECT f.id,f.code,f.asset_id,f.source_alert_ref,f.affected_url_masked,f.parameter_masked,f.input_vector_masked,
 f.http_method,f.title_masked,f.status,f.first_seen_at,f.last_seen_at,f.row_version,f.current_review_id,
 s.proposed_severity AS final_severity,s.reason AS severity_reason,s.assessed_by,s.assessed_at,s.reviewed_by,s.reviewed_at
FROM finding f LEFT JOIN finding_review s ON s.id=f.current_review_id;
CREATE VIEW v_ticket_current AS
SELECT t.id,t.code,t.finding_id,f.asset_id,f.status,s.proposed_severity AS final_severity,
 e.id AS event_id,e.team_id,e.assignee_id,e.due_at,e.sla_rule_id,e.progress_percent,e.remediated_by,e.remediated_at,
 (e.due_at<CURRENT_TIMESTAMP AND f.status IN ('Triaged','Assigned','In Progress','Pending Verification','Reopened','Pending Risk Acceptance')) AS is_overdue
FROM remediation_ticket t JOIN finding f ON f.id=t.finding_id
LEFT JOIN finding_review s ON s.id=f.current_review_id LEFT JOIN ticket_event e ON e.id=t.current_event_id;
CREATE VIEW v_ticket_history AS
SELECT e.id,e.ticket_id,e.finding_id,e.previous_event_id,e.event_type,
 p.assignee_id AS old_assignee_id,e.assignee_id AS new_assignee_id,p.due_at AS old_due_at,e.due_at AS new_due_at,
 e.progress_percent,e.reason_masked,e.changed_by,e.changed_at,e.evidence_id
FROM ticket_event e LEFT JOIN ticket_event p ON p.id=e.previous_event_id;
CREATE VIEW v_finding_history AS
SELECT id,finding_id,asset_id,from_status,to_status,actor_id,actor_kind,occurred_at,reason_masked,
 evidence_id,finding_review_id,verification_result_id,risk_acceptance_id,trigger_occurrence_id,correlation_id
FROM audit_log WHERE to_status IS NOT NULL;
CREATE VIEW v_import_batch_summary AS
SELECT id,code,asset_id,scan_id,imported_by,created_at,finished_at,status,total_records,success_records,error_records,failure_code
FROM import_batch;
-- Thời hạn kiểm ở lúc đọc/job; không dùng CHECK(valid_until > now()).
CREATE VIEW v_latest_applied_risk AS
SELECT DISTINCT ON (finding_id) id,finding_id,renewal_of_id,decided_by,conditions_masked,valid_from,valid_until,applied_at,
 (valid_from<=CURRENT_TIMESTAMP AND valid_until>CURRENT_TIMESTAMP) AS within_time_window
FROM risk_acceptance WHERE status='Approved' AND applied_at IS NOT NULL ORDER BY finding_id,applied_at DESC,id DESC;
REVOKE ALL ON ALL FUNCTIONS IN SCHEMA htqlhdk FROM PUBLIC;
REVOKE ALL ON ALL TABLES IN SCHEMA htqlhdk FROM PUBLIC;
COMMIT;
