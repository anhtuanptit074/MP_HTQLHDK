BEGIN;
SET search_path=htqlhdk,public;
CREATE OR REPLACE FUNCTION guard_scope() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF TG_OP='DELETE' THEN RAISE EXCEPTION 'Scope is immutable' USING ERRCODE='23514'; END IF;
 IF TG_OP='UPDATE' AND OLD.status IN ('Approved','Rejected') THEN
  -- A single future end date can terminate an unbounded approval. Scope content and approval history stay immutable.
  IF NOT (OLD.status='Approved' AND OLD.effective_until IS NULL AND NEW.effective_until>now()
   AND NEW.effective_until>OLD.effective_from AND (to_jsonb(NEW)-'effective_until')=(to_jsonb(OLD)-'effective_until')) THEN
   RAISE EXCEPTION 'Scope is immutable' USING ERRCODE='23514';
  END IF;
 END IF;
 IF NEW.status='Approved' THEN
  IF NEW.confirmed_by=NEW.decided_by OR NOT EXISTS(SELECT 1 FROM app_user WHERE id=NEW.decided_by AND account_kind='Human') THEN
   RAISE EXCEPTION 'Scope needs independent human approval' USING ERRCODE='23514';
  END IF;
  PERFORM pg_advisory_xact_lock(NEW.asset_id);
  IF EXISTS(SELECT 1 FROM asset_scope_revision s WHERE s.asset_id=NEW.asset_id AND s.id<>NEW.id AND s.status='Approved'
    AND tstzrange(s.effective_from,s.effective_until,'[)') && tstzrange(NEW.effective_from,NEW.effective_until,'[)')) THEN
   RAISE EXCEPTION 'Approved scope intervals overlap' USING ERRCODE='23514';
  END IF;
 END IF; RETURN NEW;
END $$;
DROP TRIGGER scope_guard ON asset_scope_revision;
CREATE TRIGGER scope_guard BEFORE INSERT OR UPDATE OR DELETE ON asset_scope_revision FOR EACH ROW EXECUTE FUNCTION guard_scope();

CREATE FUNCTION guard_occurrence_asset() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF NOT EXISTS(SELECT 1 FROM finding f JOIN scan_run r ON r.id=NEW.scan_run_id JOIN scan_plan p ON p.id=r.plan_id
   JOIN asset_scope_revision s ON s.id=p.scope_id WHERE f.id=NEW.finding_id AND f.asset_id=s.asset_id AND r.scan_status='Completed') THEN
  RAISE EXCEPTION 'Occurrence asset/run mismatch' USING ERRCODE='23514';
 END IF; RETURN NEW;
END $$;
CREATE TRIGGER occurrence_asset_guard BEFORE INSERT ON finding_occurrence FOR EACH ROW EXECUTE FUNCTION guard_occurrence_asset();

CREATE FUNCTION guard_plan_asset() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF NEW.plan_kind='Verification' AND NOT EXISTS(SELECT 1 FROM remediation_ticket t JOIN finding f ON f.id=t.finding_id
   JOIN asset_scope_revision s ON s.asset_id=f.asset_id WHERE t.id=NEW.remediation_ticket_id AND s.id=NEW.scope_id) THEN
  RAISE EXCEPTION 'Verification scope/ticket asset mismatch' USING ERRCODE='23514';
 END IF;
 IF NOT EXISTS(SELECT 1 FROM app_user WHERE id=NEW.operator_id AND account_kind='Human') THEN
  RAISE EXCEPTION 'Operator must be human' USING ERRCODE='23514';
 END IF; RETURN NEW;
END $$;
CREATE TRIGGER plan_asset_guard BEFORE INSERT OR UPDATE ON scan_plan FOR EACH ROW EXECUTE FUNCTION guard_plan_asset();

CREATE FUNCTION guard_profile_accounts() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF NOT EXISTS(SELECT 1 FROM app_user WHERE id=NEW.service_user_id AND account_kind='Service') THEN
  RAISE EXCEPTION 'Profile account must be service' USING ERRCODE='23514';
 END IF;
 IF NEW.status IN('Active','Retired') AND (NEW.created_by=NEW.approved_by OR NOT EXISTS(SELECT 1 FROM app_user WHERE id=NEW.approved_by AND account_kind='Human')) THEN
  RAISE EXCEPTION 'Profile needs independent human approval' USING ERRCODE='23514';
 END IF; RETURN NEW;
END $$;
CREATE TRIGGER profile_account_guard BEFORE INSERT OR UPDATE ON zap_scan_profile FOR EACH ROW EXECUTE FUNCTION guard_profile_accounts();
CREATE INDEX ix_occurrence_run ON finding_occurrence(scan_run_id,finding_id);
CREATE INDEX ix_plan_scope ON scan_plan(scope_id,status);
COMMIT;
