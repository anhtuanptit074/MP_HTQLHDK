import { createHash } from "node:crypto";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath, pathToFileURL } from "node:url";

const here = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(here, "../..");
const runtime = path.resolve(
  process.argv[2] ??
    path.join(
      root,
      "src/frontend/node_modules/@electric-sql/pglite/dist/index.js",
    ),
);
const { PGlite } = await import(pathToFileURL(runtime).href);
const manifest = JSON.parse(
  await fs.readFile(path.join(here, "V001__manifest.json"), "utf8"),
);

const sources = [];
for (const entry of manifest.files) {
  const sql = await fs.readFile(path.join(root, entry.path), "utf8");
  const canonicalSql = sql.replace(/\r\n?/g, "\n");
  const hash = createHash("sha256").update(canonicalSql).digest("hex");
  if (hash !== entry.sha256) {
    throw new Error(`Migration checksum drift: ${entry.path}`);
  }
  sources.push(canonicalSql);
}

const db = new PGlite();
await db.exec(`
  CREATE SCHEMA htqlhdk_migration;
  CREATE TABLE htqlhdk_migration.schema_migration (
    version varchar(40) PRIMARY KEY,
    description text NOT NULL,
    status varchar(20) NOT NULL CHECK (status IN ('Running', 'Applied', 'Failed')),
    schema_sha256 char(64) NOT NULL,
    guards_sha256 char(64) NOT NULL,
    reference_sha256 char(64) NOT NULL,
    started_at timestamptz NOT NULL DEFAULT CURRENT_TIMESTAMP,
    applied_at timestamptz
  );
`);

async function applyBaseline() {
  const result = await db.query(
    "SELECT status FROM htqlhdk_migration.schema_migration WHERE version = 'V001'",
  );
  if (result.rows[0]?.status === "Applied") {
    return "no-op";
  }
  if (result.rows.length > 0) {
    throw new Error(
      "Incomplete V001 attempt must not be resumed automatically.",
    );
  }

  await db.query(
    `INSERT INTO htqlhdk_migration.schema_migration
      (version, description, status, schema_sha256, guards_sha256, reference_sha256)
     VALUES ('V001', $1, 'Running', $2, $3, $4)`,
    [manifest.description, ...manifest.files.map((entry) => entry.sha256)],
  );
  for (const sql of sources) {
    await db.exec(sql);
  }
  await db.exec(`
    UPDATE htqlhdk_migration.schema_migration
    SET status = 'Applied', applied_at = CURRENT_TIMESTAMP
    WHERE version = 'V001' AND status = 'Running';
  `);
  return "applied";
}

const first = await applyBaseline();
const second = await applyBaseline();
const tableCount = await db.query(
  "SELECT count(*)::int AS count FROM information_schema.tables WHERE table_schema = 'htqlhdk' AND table_type = 'BASE TABLE'",
);
const migration = await db.query(
  "SELECT status, applied_at IS NOT NULL AS has_applied_at FROM htqlhdk_migration.schema_migration WHERE version = 'V001'",
);

if (
  first !== "applied" ||
  second !== "no-op" ||
  tableCount.rows[0].count !== 27 ||
  migration.rows[0].status !== "Applied" ||
  !migration.rows[0].has_applied_at
) {
  throw new Error("Migration repeatability verification failed.");
}

console.log(
  "PASS: V001 applied on a clean database; second run was a no-op; 27 tables verified.",
);
