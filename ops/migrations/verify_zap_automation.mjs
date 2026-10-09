import fs from "node:fs/promises";
import path from "node:path";
import { createHash } from "node:crypto";
import { fileURLToPath, pathToFileURL } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, "../..");
const runtime = path.resolve(process.argv[2]);
const { PGlite } = await import(pathToFileURL(runtime).href);
const manifest = JSON.parse(
  await fs.readFile(path.join(here, "V002__manifest.json"), "utf8"),
);
const source = await fs.readFile(
  path.join(root, manifest.files[0].path),
  "utf8",
);
const canonical = source.replace(/\r\n?/g, "\n");
const sha256 = createHash("sha256").update(canonical).digest("hex");
if (sha256 !== manifest.files[0].sha256) {
  throw new Error("V002 migration checksum drift.");
}

const db = new PGlite();
for (const file of ["01_schema.sql", "02_guards_and_views.sql", "03_reference_data.sql"]) {
  await db.exec(await fs.readFile(path.join(root, "database/sql", file), "utf8"));
}
await db.exec(source);

const count = await db.query(
  "SELECT count(*)::int AS count FROM information_schema.tables WHERE table_schema='htqlhdk' AND table_type='BASE TABLE'",
);
const columns = await db.query(
  "SELECT automation_enabled, zap_profile_id FROM htqlhdk.scan_schedule LIMIT 0",
);
if (count.rows[0].count !== 30 || columns.fields.length !== 2) {
  throw new Error("V002 schema verification failed.");
}

await db.exec(`
  SET search_path=htqlhdk,public;
  INSERT INTO app_user(id,username,display_name) VALUES
    (1,'owner-v002','Owner'),(2,'security-v002','Security'),(3,'svc-zap-v002','ZAP Service');
  INSERT INTO team(id,code,name) VALUES(1,'OPS-V002','Operations');
  INSERT INTO asset(id,code,name,domain,base_url,environment,owner_id,criticality,status,created_by)
    VALUES(1,'ASSET-V002','Asset','example.invalid','https://example.invalid','test',1,'Test','Draft',1);
  INSERT INTO asset_scope_revision(id,asset_id,version_no,status,change_reason,created_by)
    VALUES(1,1,1,'Draft','Test',1);
  INSERT INTO asset_scope_rule(scope_id,effect,target_type,target_value,match_mode)
    VALUES(1,'Include','Domain','example.invalid','Exact');
  INSERT INTO asset_scan_window(scope_id,weekday_iso,start_time,end_time,timezone)
    VALUES(1,1,'00:00','23:59','UTC');
  UPDATE asset_scope_revision
    SET status='Pending Approval',confirmed_by=1,confirmed_at=CURRENT_TIMESTAMP WHERE id=1;
  UPDATE asset_scope_revision
    SET status='Approved',decided_by=2,decided_at=CURRENT_TIMESTAMP,decision_reason='Approved' WHERE id=1;
  UPDATE asset SET current_scope_id=1,status='Active' WHERE id=1;
  INSERT INTO zap_scan_profile
    (id,code,name,version_no,status,service_user_id,created_by,approved_by,approved_at)
    VALUES(1,'PASSIVE','Passive',1,'Active',3,2,2,CURRENT_TIMESTAMP);
  INSERT INTO scan_schedule
    (id,code,name,asset_id,scope_id,frequency,recurrence_rule,timezone,effective_from,next_due_at,
     planned_duration_minutes,operations_team_id,status,created_by,approved_by,approved_at,target_urls,
     automation_enabled,zap_profile_id)
    VALUES(1,'S-V002','Schedule',1,1,'Weekly','FREQ=WEEKLY;BYDAY=MO','UTC',CURRENT_TIMESTAMP,
           CURRENT_TIMESTAMP,60,1,'Active',2,2,CURRENT_TIMESTAMP,ARRAY['https://example.invalid/'],true,1);
`);

let rejected = false;
try {
  await db.exec(
    "UPDATE htqlhdk.scan_schedule SET recurrence_rule='FREQ=HOURLY' WHERE id=1",
  );
} catch (error) {
  rejected = error.code === "23514";
}
if (!rejected) {
  throw new Error("Unsupported automated recurrence was not rejected.");
}

let unsupportedOptionRejected = false;
try {
  await db.exec(
    "UPDATE htqlhdk.scan_schedule SET recurrence_rule='FREQ=WEEKLY;COUNT=2' WHERE id=1",
  );
} catch (error) {
  unsupportedOptionRejected = error.code === "23514";
}
if (!unsupportedOptionRejected) {
  throw new Error("Unsupported automated recurrence option was not rejected.");
}

let profileMutationRejected = false;
try {
  await db.exec(
    "UPDATE htqlhdk.zap_scan_profile SET active_scan_enabled=true WHERE id=1",
  );
} catch (error) {
  profileMutationRejected = error.code === "23514";
}
if (!profileMutationRejected) {
  throw new Error("Approved ZAP profile mutation was not rejected.");
}

let inUseRetirementRejected = false;
try {
  await db.exec(
    "UPDATE htqlhdk.zap_scan_profile SET status='Retired', retired_at=CURRENT_TIMESTAMP WHERE id=1",
  );
} catch (error) {
  inUseRetirementRejected = error.code === "23514";
}
if (!inUseRetirementRejected) {
  throw new Error("Retirement of an in-use ZAP profile was not rejected.");
}

console.log("PASS: V002 creates 30 tables and protects approved ZAP automation configuration.");
await db.close();
