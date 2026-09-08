-- Dữ liệu giả chỉ dùng trong database kiểm tra; SLA 60 phút không phải chính sách đề xuất.
SET search_path=htqlhdk,public;
INSERT INTO app_user(id,username,display_name,unit_name) VALUES
 (1,'owner','Chủ tài sản','TEST'),(2,'security','ATTT','TEST'),(3,'operator','Vận hành','TEST'),
 (4,'fixer','Kỹ thuật','TEST'),(5,'approver','Người duyệt','TEST'),(6,'reviewer','Người rà soát','TEST');
INSERT INTO team(id,code,name,unit_name) VALUES(1,'TEAM','Nhóm thử','TEST');
INSERT INTO team_member(team_id,user_id) VALUES(1,4),(1,3);
INSERT INTO asset(id,code,name,domain,base_url,environment,unit_name,owner_id,criticality,created_by)
 VALUES(1,'A1','Tài sản thử 1','one.example.invalid','https://one.example.invalid','test','TEST',1,'Theo chính sách',1),
 (2,'A2','Tài sản thử 2','two.example.invalid','https://two.example.invalid','test','TEST',1,'Theo chính sách',1);
INSERT INTO asset_scope_revision(id,asset_id,version_no,change_reason,created_by)
 VALUES(1,1,1,'Phạm vi thử',1),(2,2,1,'Phạm vi thử',1);
INSERT INTO asset_scope_rule(scope_id,effect,target_type,target_value,match_mode)
 VALUES(1,'Include','Domain','one.example.invalid','Exact'),(2,'Include','Domain','two.example.invalid','Exact');
INSERT INTO asset_scan_window(scope_id,weekday_iso,start_time,end_time,timezone)
 VALUES(1,1,'08:00','12:00','Asia/Bangkok'),(2,1,'08:00','12:00','Asia/Bangkok');
UPDATE asset_scope_revision SET status='Pending Approval',confirmed_by=1,confirmed_at='2026-01-01T00:00Z';
UPDATE asset_scope_revision SET status='Approved',decided_by=2,decided_at='2026-01-01T00:01Z',decision_reason='Duyệt thử';
UPDATE asset SET current_scope_id=id,status='Active';
INSERT INTO scan_schedule(id,code,name,asset_id,scope_id,frequency,recurrence_rule,timezone,effective_from,next_due_at,planned_duration_minutes,operations_team_id,created_by,target_urls)
 VALUES(1,'S1','Lịch thử',1,1,'Weekly','FREQ=WEEKLY;BYDAY=MO','Asia/Bangkok','2026-01-01T00:00Z','2026-01-05T01:00Z',60,1,2,ARRAY['https://one.example.invalid/']);
UPDATE scan_schedule SET status='Active',approved_by=2,approved_at='2026-01-01T00:02Z';
INSERT INTO scan_occurrence(id,code,asset_id,scope_id,schedule_id,purpose,planned_at,planned_end_at,started_at,ended_at,operations_team_id,performed_by,status,target_urls)
 VALUES(1,'SCAN1',1,1,1,'Periodic','2026-01-05T01:00Z','2026-01-05T02:00Z','2026-01-05T01:00Z','2026-01-05T02:00Z',1,3,'Completed',ARRAY['https://one.example.invalid/']),
 (2,'VERIFY1',1,1,NULL,'Verification','2026-01-12T01:00Z','2026-01-12T02:00Z','2026-01-12T01:00Z','2026-01-12T02:00Z',1,3,'Completed',ARRAY['https://one.example.invalid/']);
INSERT INTO import_batch(id,code,asset_id,scan_id,file_sha256,zap_version,template_name,imported_by,created_at,status,total_records,raw_object_key,raw_original_name,raw_size_bytes,raw_format,raw_key_ref)
 VALUES(1,'B1',1,1,repeat('a',64),'test','Traditional JSON Report',3,'2026-01-05T03:00Z','Processing',1,'test/raw1','zap.json',100,'JSON','test-key'),
 (2,'B2',1,2,repeat('b',64),'test','Traditional JSON Report',3,'2026-01-12T03:00Z','Processing',0,'test/raw2','verify.json',100,'JSON','test-key');
INSERT INTO finding(id,code,asset_id,fingerprint,identity_version,identity_key_ref,identity_ciphertext,source_alert_ref,affected_url_masked,title_masked,first_seen_at,last_seen_at)
 VALUES(1,'F1',1,repeat('c',64),1,'test-key',decode('abcd','hex'),'10001','https://one.example.invalid/a','Phát hiện thử','2026-01-05T01:00Z','2026-01-05T01:00Z'),
 (2,'F2',2,repeat('d',64),1,'test-key',decode('abcd','hex'),'10002','https://two.example.invalid/a','Phát hiện tài sản 2','2026-01-05T01:00Z','2026-01-05T01:00Z');
INSERT INTO finding_occurrence(id,asset_id,finding_id,scan_id,observed_at) VALUES(1,1,1,1,'2026-01-05T01:00Z');
INSERT INTO finding_observation(id,asset_id,scan_id,occurrence_id,batch_id,record_locator,source_severity,source_confidence,title_masked)
 VALUES(1,1,1,1,1,'/site/0/alerts/0/instances/0','High',2,'Nguồn thử');
UPDATE import_batch SET status='Completed',success_records=1,finished_at='2026-01-05T03:01Z' WHERE id=1;
UPDATE import_batch SET status='Completed',finished_at='2026-01-12T03:01Z' WHERE id=2;
INSERT INTO evidence(id,finding_id,kind,summary_masked,created_by)
 VALUES(1,1,'Analysis','Bằng chứng thử',2),(2,1,'Remediation','Bằng chứng sửa thử',4),(3,2,'Analysis','Bằng chứng tài sản 2',2);
INSERT INTO finding_review(id,finding_id,review_kind,proposed_severity,reason,context_masked,evidence_id,assessed_by,status,applied_at)
 VALUES(1,1,'Severity','High','Đã phân tích','Bối cảnh thử',1,2,'Applied',CURRENT_TIMESTAMP),
 (2,2,'Severity','Medium','Đã phân tích','Bối cảnh thử',3,2,'Applied',CURRENT_TIMESTAMP);
UPDATE finding SET current_review_id=id,status='Triaged';
INSERT INTO sla_rule(id,policy_ref,unit_name,severity,duration_minutes,effective_from,approved_by,approved_at)
 VALUES(1,'TEST-v1','TEST','High',60,'2026-01-01T00:00Z',2,'2026-01-01T00:00Z');
INSERT INTO remediation_ticket(id,code,finding_id,created_by) VALUES(1,'T1',1,2);
INSERT INTO ticket_event(id,ticket_id,finding_id,event_type,team_id,assignee_id,due_at,sla_rule_id,reason_masked,changed_by,changed_at)
 VALUES(1,1,1,'Assignment',1,4,'2026-01-05T05:00Z',1,'Giao thử',2,'2026-01-05T04:00Z');
UPDATE remediation_ticket SET current_event_id=1 WHERE id=1;
UPDATE finding SET status='Assigned' WHERE id=1;
UPDATE finding SET status='In Progress' WHERE id=1;
INSERT INTO ticket_event(id,ticket_id,finding_id,previous_event_id,event_type,team_id,assignee_id,due_at,sla_rule_id,progress_percent,reason_masked,remediation_action_masked,evidence_id,remediated_by,remediated_at,changed_by,changed_at)
 VALUES(2,1,1,1,'Progress',1,4,'2026-01-05T05:00Z',1,100,'Đã sửa','Biện pháp thử',2,4,'2026-01-05T05:00Z',4,'2026-01-05T05:00Z');
UPDATE remediation_ticket SET current_event_id=2 WHERE id=1;
UPDATE finding SET status='Pending Verification' WHERE id=1;
INSERT INTO verification_request(id,asset_id,finding_id,ticket_id,scope_id,remediation_event_id,operations_team_id,requested_by,requested_at)
 VALUES(1,1,1,1,1,2,1,2,'2026-01-05T06:00Z');
INSERT INTO risk_acceptance(id,finding_id,previous_finding_status,requested_by,requested_at,responsible_user_id,reason,business_impact_masked,mitigation_masked,requested_until)
 VALUES(1,2,'Triaged',1,'2026-01-05T00:00Z',1,'Lý do thử','Ảnh hưởng thử','Giảm thiểu thử','2026-12-31T00:00Z');
UPDATE finding SET status='Pending Risk Acceptance' WHERE id=2;
-- Fixture kiểm tra SQL cấu trúc; hợp đồng ghi audit/workflow ở tài liệu 04.
DO $$ DECLARE r record; seq text; maxid bigint;
BEGIN
 FOR r IN SELECT table_name FROM information_schema.columns WHERE table_schema='htqlhdk' AND column_name='id' AND is_identity='YES' LOOP
   seq:=pg_get_serial_sequence('htqlhdk.'||r.table_name,'id');
   EXECUTE format('SELECT coalesce(max(id),0) FROM htqlhdk.%I',r.table_name) INTO maxid;
   PERFORM setval(seq,greatest(maxid,1),maxid>0);
 END LOOP;
END $$;
