import assert from 'node:assert/strict';
import { readFile, writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import { randomUUID } from 'node:crypto';

const root = process.env.HTQLHDK_TEST_ROOT;
const base = process.env.HTQLHDK_TEST_URL || 'http://127.0.0.1:5002';
assert(root, 'Use scripts/test-core13.ps1 or configure an isolated CI database.');
const passwords = new Map((await readFile(join(root, '.codex_tmp/local-accounts.txt'), 'utf8')).trim().split(/\r?\n/).map(line => line.split('\t')));
const results = [];
const mark = name => { results.push({ name, passed: true }); console.log('PASS: ' + name); };
class Client {
  cookies = new Map(); token;
  async call(path, method = 'GET', body, version, expected = 200, headers = {}) {
    const options = { method, headers: { Cookie: [...this.cookies].map(([key, value]) => key + '=' + value).join('; '), ...headers } };
    if (this.token && method !== 'GET') options.headers['X-CSRF-Token'] = this.token;
    if (version !== undefined) options.headers['If-Match'] = '"' + version + '"';
    if (body instanceof FormData) options.body = body;
    else if (body) { options.body = JSON.stringify(body); options.headers['Content-Type'] = 'application/json'; }
    const response = await fetch(base + '/api/v1/' + path, options);
    for (const cookie of response.headers.getSetCookie()) { const [key, ...parts] = cookie.split(';')[0].split('='); this.cookies.set(key, parts.join('=')); }
    const content = await response.text();
    assert.equal(response.status, expected, method + ' ' + path + ': ' + content.slice(0, 500));
    return response.headers.get('content-type')?.includes('json') ? JSON.parse(content) : content;
  }
  async csrf() { this.token = (await this.call('auth/csrf')).token; }
  async login(username) { await this.csrf(); await this.call('auth/login', 'POST', { username, password: passwords.get(username) }); await this.csrf(); return this; }
}
const clients = Object.fromEntries(await Promise.all(['admin', 'owner', 'owner2', 'security', 'operator', 'remediator', 'auditor'].map(async username => [username, await new Client().login(username)])));
const { admin, owner, owner2, security, operator, remediator, auditor } = clients;
const anon = new Client(); await anon.call('context', 'GET', undefined, undefined, 401);
await anon.csrf(); await anon.call('auth/login', 'POST', { username: 'svc-zap', password: 'service-cannot-login' }, undefined, 401);
const noCsrf = new Client(); noCsrf.cookies = admin.cookies;
await noCsrf.call('users', 'POST', { username: 'must-not-create' }, undefined, 400);
mark('Authentication, service-account rejection and CSRF');
const directory = (await admin.call('users')).items;
const uid = name => directory.find(user => user.username === name).id;
assert.equal((await admin.call('assets')).items.length, 0);
assert.equal((await admin.call('findings')).items.length, 0);
await admin.call('access-assets');
const shop = (await owner.call('assets')).items.find(asset => asset.code === 'SHOP-DEMO');
await owner2.call('assets/' + shop.id, 'GET', undefined, undefined, 404);
await owner2.call('assets/' + shop.id + '/transitions', 'POST', { toStatus: 'Suspended', reason: 'wrong owner' }, shop.version, 404);
await auditor.call('assets/' + shop.id + '/transitions', 'POST', { toStatus: 'Suspended', reason: 'read only' }, shop.version, 403);
mark('Admin separation, cross-asset IDOR and read-only auditor');

const suffix = Date.now().toString(36);
const url = 'https://core-' + suffix + '.example.invalid/';
const asset = await owner.call('assets', 'POST', { code: 'IT-' + suffix, name: 'HTTP integration test', baseUrl: url, environment: 'Test', ownerId: uid('owner'), securityOfficerId: uid('security'), criticality: 'High' });
for (const [name, role] of [['security','SECURITY'],['operator','SCAN_OPERATOR'],['remediator','REMEDIATOR'],['auditor','AUDITOR']]) await admin.call('access-grants', 'POST', { userId: uid(name), roleCode: role, assetId: asset.id });
const scopeInput = { changeReason: 'Approved integration fixture', rules: [{ effect: 'Include', targetType: 'Domain', targetValue: new URL(url).hostname, matchMode: 'Exact' }], windows: Array.from({length:7}, (_, index) => ({ weekdayIso:index+1,startTime:'00:00',endTime:'00:00',endsNextDay:true,timezone:'Asia/Bangkok' })) };
let scope = await owner.call('assets/' + asset.id + '/scopes', 'POST', scopeInput);
await security.call(`assets/${asset.id}/scopes/${scope.id}/decision`, 'POST', { decision: 'Approved', effectiveFrom: new Date(Date.now()-3600000).toISOString(), reason:'not confirmed' }, scope.version, 409);
scope = await owner.call(`assets/${asset.id}/scopes/${scope.id}/confirm`, 'POST', { reason: 'I own this website' }, scope.version);
scope = await security.call(`assets/${asset.id}/scopes/${scope.id}/decision`, 'POST', { decision:'Approved', effectiveFrom:new Date(Date.now()-3600000).toISOString(), effectiveUntil:new Date(Date.now()+604800000).toISOString(), reason:'Separated approval' }, scope.version);
await owner.call(`assets/${asset.id}/scopes/${scope.id}`, 'PUT', scopeInput, scope.version, 409);
mark('Asset scope confirmation, separate approval and immutable approved scope');

let profile = await operator.call('profiles', 'POST', { code:'P-'+suffix,name:'Integration passive',spiderKind:'Traditional',passiveScanEnabled:true,activeScanEnabled:false,maxDurationMinutes:10,requestDelayMs:250,threadPerHost:2,authKind:'None',reportFormat:'JSON',serviceUserId:uid('svc-zap') });
profile = await operator.call('profiles/'+profile.id+'/submit','POST', {reason:'Ready'}, profile.version);
profile = await security.call('profiles/'+profile.id+'/decision','POST', {decision:'Approved',reason:'Passive approved'},profile.version);
await operator.call('profiles/'+profile.id,'PUT',{ serviceUserId:uid('svc-zap'),name:'Alter active' },profile.version,409);
const scheduled = new Date(Date.now()-60000).toISOString();
let plan = await security.call('plans','POST',{code:'PPLAN-'+suffix,name:'Integration daily',scopeId:scope.id,zapProfileId:profile.id,operatorId:uid('operator'),recurrenceRule:'FREQ=DAILY',timezone:'Asia/Bangkok',effectiveFrom:scheduled});
plan = await security.call('plans/'+plan.id+'/transitions','POST',{toStatus:'Active',reason:'Approved'},plan.version);
await security.call('plans/'+plan.id+'/transitions','POST',{toStatus:'Paused',reason:'stale'},1,412);
let run = await operator.call('plans/'+plan.id+'/runs','POST',{scheduledFor:scheduled});
run = await operator.call('runs/'+run.id+'/record-result','POST',{scanStatus:'Completed',startedAt:scheduled,finishedAt:new Date(Date.now()-1000).toISOString(),reason:'Controlled external test'},run.version);
mark('Profile approval, scan scheduling and stale-version protection');

const report = (urls, name = url) => ({'@version':'2.17.0',site:[{'@name':name,alerts:urls.length ? [{pluginid:'10020',name:'=HYPERLINK("https://evil.invalid")',riskcode:'2',confidence:'2',desc:'password=secret token=private',solution:'Use secure header',instances:urls.map(uri=>({uri,method:'GET',param:''}))}] : []}]});
async function upload(runId, version, json, expected=200) { const body=new FormData(); body.append('format','JSON'); body.append('file',new Blob([JSON.stringify(json)],{type:'application/json'}),'zap.json'); return operator.call('runs/'+runId+'/report','POST',body,version,expected); }
await upload(run.id,run.version,report([url,'https://outside.example.invalid/']),422);
run = await operator.call('runs/'+run.id);
assert.equal(run.reportStatus,'Rejected');
assert.equal((await security.call('findings')).items.filter(f=>f.assetId===asset.id).length,0);
run = await upload(run.id,run.version,report([url+'?token=one']));
assert.equal(run.reportStatus,'Processed');
await upload(run.id,run.version,report([url+'?token=one']));
await upload(run.id,run.version,report([url+'?token=two']),409);
let finding = (await security.call('findings')).items.find(f=>f.assetId===asset.id);
assert.equal(finding.sourceSeverity,'Medium'); assert.equal(finding.finalSeverity,null);
const detail = await security.call('findings/'+finding.id);
assert.equal(detail.occurrences.items.length,1);
assert(!JSON.stringify(detail).includes('token=one')); assert(!JSON.stringify(detail).includes('password=secret'));
await owner.call('runs/'+run.id+'/raw-report','GET',undefined,undefined,403,{'X-Access-Reason':'Need report'});
await admin.call('runs/'+run.id+'/raw-report','GET',undefined,undefined,404,{'X-Access-Reason':'Need report'});
await security.call('runs/'+run.id+'/raw-report','GET',undefined,undefined,200,{'X-Access-Reason':'Review source report'});
mark('Atomic rejection, safe retry, deduplication, masking and raw-report permissions');

finding = await security.call('findings/'+finding.id+'/assess','POST',{finalSeverity:'High',reason:'Business impact'},finding.version);
let ticket = await security.call('findings/'+finding.id+'/tickets','POST',{assigneeId:uid('remediator'),dueAt:new Date(Date.now()+86400000).toISOString(),reason:'Assign remediation'},finding.version);
await operator.call('tickets/'+ticket.id+'/progress','POST',{progressPercent:20,remediationActionMasked:'Attempt'},ticket.version,403);
ticket = await remediator.call('tickets/'+ticket.id+'/progress','POST',{progressPercent:60,remediationActionMasked:'Add Content-Security-Policy',evidenceSummaryMasked:'Test response headers'},ticket.version);
const evidenceBytes = 'Controlled evidence for integration only';
const evidenceUpload = new FormData(); evidenceUpload.append('summary','Response header evidence'); evidenceUpload.append('file',new Blob([evidenceBytes]),'evidence.txt');
ticket = await remediator.call('tickets/'+ticket.id+'/evidence','POST',evidenceUpload,ticket.version);
assert.equal(ticket.hasEvidence,true);
assert.equal(await security.call('tickets/'+ticket.id+'/evidence','GET',undefined,undefined,200,{'X-Access-Reason':'Review remediation evidence'}),evidenceBytes);
await owner.call('tickets/'+ticket.id+'/evidence','GET',undefined,undefined,403,{'X-Access-Reason':'Not permitted'});
await remediator.call('tickets/'+ticket.id+'/submit-verification','POST',{remediationActionMasked:'Fix'},ticket.version,422);
ticket = await remediator.call('tickets/'+ticket.id+'/submit-verification','POST',{remediationActionMasked:'Configured frame-ancestors',evidenceSummaryMasked:'Header check on test build'},ticket.version);
assert.equal(ticket.finding.status,'Pending Verification');
await remediator.call('tickets/'+ticket.id+'/verification-plans','POST',{scopeId:scope.id,zapProfileId:profile.id,operatorId:uid('operator'),requestedFor:new Date().toISOString()},undefined,403);
mark('Independent severity, assignment, assignee-only progress and evidence gate');

async function verify(present) {
  const when = new Date(Date.now()+200).toISOString();
  const verification = await security.call('tickets/'+ticket.id+'/verification-plans','POST',{scopeId:scope.id,zapProfileId:profile.id,operatorId:uid('operator'),requestedFor:when});
  let verificationRun = await operator.call('plans/'+verification.id+'/runs','POST',{scheduledFor:when});
  await new Promise(resolve=>setTimeout(resolve,250));
  verificationRun = await operator.call('runs/'+verificationRun.id+'/record-result','POST',{scanStatus:'Completed',startedAt:when,finishedAt:new Date().toISOString(),reason:'Isolated verification report'},verificationRun.version);
  verificationRun = await upload(verificationRun.id,verificationRun.version,report(present?[url+'?token=three']:[]));
  await operator.call('runs/'+verificationRun.id+'/verification-decision','POST',{coverageConfirmed:true,reason:'Cannot self approve'},verificationRun.version,403);
  if (!present) await security.call('runs/'+verificationRun.id+'/verification-decision','POST',{coverageConfirmed:false,reason:'Insufficient coverage'},verificationRun.version,422);
  const decision = await security.call('runs/'+verificationRun.id+'/verification-decision','POST',{coverageConfirmed:true,fingerprintPresent:false,reason:'Human coverage reviewed'},verificationRun.version);
  assert.equal(decision.status,present?'Reopened':'Closed');
  return decision;
}
finding = await verify(true);
ticket = await security.call('findings/'+finding.id+'/tickets','POST',{assigneeId:uid('remediator'),dueAt:new Date(Date.now()+86400000).toISOString(),reason:'Repair recurrence'},finding.version);
ticket = await remediator.call('tickets/'+ticket.id+'/submit-verification','POST',{remediationActionMasked:'Completed second fix',evidenceSummaryMasked:'Independent checks now pass'},ticket.version);
finding = await verify(false);
mark('Verification uses actual fingerprint presence, coverage and closes only after repair');

const next = new Date(new Date(scheduled).getTime()+86400000).toISOString();
// Use a fresh scan plan to reproduce recurrence immediately after the closure.
const recurrenceTime = new Date(Date.now()+200).toISOString();
let recurrencePlan = await security.call('plans','POST',{code:'RECUR-'+suffix,name:'Recurrence check',scopeId:scope.id,zapProfileId:profile.id,operatorId:uid('operator'),recurrenceRule:'FREQ=DAILY',timezone:'Asia/Bangkok',effectiveFrom:recurrenceTime});
recurrencePlan = await security.call('plans/'+recurrencePlan.id+'/transitions','POST',{toStatus:'Active',reason:'Controlled recurrence'},recurrencePlan.version);
let recurrenceRun = await operator.call('plans/'+recurrencePlan.id+'/runs','POST',{scheduledFor:recurrenceTime});
await new Promise(resolve=>setTimeout(resolve,250));
recurrenceRun = await operator.call('runs/'+recurrenceRun.id+'/record-result','POST',{scanStatus:'Completed',startedAt:recurrenceTime,finishedAt:new Date().toISOString(),reason:'Later run'},recurrenceRun.version);
await upload(recurrenceRun.id,recurrenceRun.version,report([url+'?token=other']));
assert.equal((await security.call('findings/'+finding.id)).status,'Reopened');
mark('Later scans reopen closed findings without losing the original ticket/history');

const csv = await security.call('reports/export'); assert(csv.includes('"\'=HYPERLINK('));
const ownerCsv = await owner2.call('reports/export'); assert(!ownerCsv.includes(asset.code));
const audits = (await security.call('audit-logs?page[size]=200')).items;
assert(audits.some(log=>log.action==='tickets.progress' && log.beforeMasked && log.afterMasked));
assert(!JSON.stringify(audits).includes(passwords.get('security')));
const grants = (await admin.call('access-grants')).items;
const scoped = grants.find(g=>g.assetId===asset.id && g.userId===uid('auditor'));
await admin.call('access-grants/'+scoped.id+'/revoke','POST',{reason:'Test immediate revocation'});
await auditor.call('assets/'+asset.id,'GET',undefined,undefined,404);
mark('Scoped CSV, formula protection, before/after audit and immediate role revocation');
let account = await admin.call('users','POST',{username:'test-'+suffix,displayName:'Integration eligible auditor',accountKind:'Human',password:randomUUID()+'Aa1!'});
// Reset before login to a known ephemeral password; no production credentials are altered.
const testPassword = randomUUID()+'Aa1!';
account = await admin.call('users/'+account.id,'PATCH',{password:testPassword},account.version);
passwords.set('test-'+suffix,testPassword);
const eligibleAuditor = await new Client().login('test-'+suffix);
await admin.call('access-grants','POST',{userId:account.id,roleCode:'AUDITOR'});
assert.equal((await eligibleAuditor.call('assets')).items.length,0);
await eligibleAuditor.call('assets/'+asset.id,'GET',undefined,undefined,404);
await admin.call('access-grants','POST',{userId:account.id,roleCode:'AUDITOR',assetId:asset.id});
await eligibleAuditor.call('assets/'+asset.id);
account = await admin.call('users/'+account.id,'PATCH',{password:randomUUID()+'Aa1!'},account.version);
await eligibleAuditor.call('context','GET',undefined,undefined,401);
mark('NULL eligibility grants never act as wildcards; password reset invalidates existing sessions');

const currentAsset = await security.call('assets/'+asset.id);
await security.call('assets/'+asset.id+'/assignment','POST',{ownerId:uid('owner2'),securityOfficerId:uid('security'),reason:'Transfer ownership in isolated test'},currentAsset.version);
await owner.call('assets/'+asset.id,'GET',undefined,undefined,404);
await owner2.call('assets/'+asset.id);
mark('Ownership transfer immediately changes business access without granting admin access');
const currentPlan = (await security.call('plans')).items.find(p=>p.id===plan.id);
await security.call('plans/'+plan.id+'/transitions','POST',{toStatus:'Paused',reason:'Pause before next period'},currentPlan.version);
await operator.call('plans/'+plan.id+'/runs','POST',{scheduledFor:next},undefined,422);
const pendingTime = new Date(Date.now()+60000).toISOString();
let cancelPlan = await security.call('plans','POST',{code:'CANCEL-'+suffix,name:'Cancellation check',scopeId:scope.id,zapProfileId:profile.id,operatorId:uid('operator'),recurrenceRule:'FREQ=DAILY',timezone:'Asia/Bangkok',effectiveFrom:pendingTime});
cancelPlan = await security.call('plans/'+cancelPlan.id+'/transitions','POST',{toStatus:'Active',reason:'Controlled cancellation'},cancelPlan.version);
let cancelRun = await operator.call('plans/'+cancelPlan.id+'/runs','POST',{scheduledFor:pendingTime});
cancelRun = await operator.call('runs/'+cancelRun.id+'/cancel','POST',{reason:'Stop queued run'},cancelRun.version);
assert.equal(cancelRun.scanStatus,'Cancelled');
mark('Paused plans cannot dispatch and queued runs can be cancelled');
const otherName = 'operator-'+suffix;
const otherPassword = randomUUID()+'Aa1!';
const otherAccount = await admin.call('users','POST',{username:otherName,displayName:'Other assigned operator',accountKind:'Human',password:otherPassword});
await admin.call('access-grants','POST',{userId:otherAccount.id,roleCode:'SCAN_OPERATOR',assetId:asset.id});
passwords.set(otherName,otherPassword);
const otherOperator = await new Client().login(otherName);
const otherWhen = new Date(Date.now()-500).toISOString();
let otherPlan = await security.call('plans','POST',{code:'OTHER-'+suffix,name:'Other operator task',scopeId:scope.id,zapProfileId:profile.id,operatorId:otherAccount.id,recurrenceRule:'FREQ=DAILY',timezone:'Asia/Bangkok',effectiveFrom:otherWhen});
otherPlan = await security.call('plans/'+otherPlan.id+'/transitions','POST',{toStatus:'Active',reason:'Independent task'},otherPlan.version);
let otherRun = await otherOperator.call('plans/'+otherPlan.id+'/runs','POST',{scheduledFor:otherWhen});
otherRun = await otherOperator.call('runs/'+otherRun.id+'/record-result','POST',{scanStatus:'Completed',startedAt:otherWhen,finishedAt:new Date().toISOString(),reason:'Different assignment'},otherRun.version);
const otherBody = new FormData(); otherBody.append('format','JSON'); otherBody.append('file',new Blob([JSON.stringify(report([url+'other?token=fake']))]),'zap.json');
await otherOperator.call('runs/'+otherRun.id+'/report','POST',otherBody,otherRun.version);
await operator.call('runs/'+otherRun.id,'GET',undefined,undefined,404);
assert(!(await operator.call('plans')).items.some(p=>p.id===otherPlan.id));
const otherFinding = (await security.call('findings')).items.find(f=>f.assetId===asset.id && f.affectedUrlMasked.includes('/other?'));
assert(otherFinding);
await operator.call('findings/'+otherFinding.id,'GET',undefined,undefined,404);
await remediator.call('findings/'+otherFinding.id,'GET',undefined,undefined,404);
assert(!(await operator.call('findings')).items.some(f=>f.id===otherFinding.id));
mark('Operators only see assigned plans/runs/source findings; remediators only see assigned findings');
await writeFile(join(root,'http-results.json'),JSON.stringify({passed:results.length,results},null,2));
console.log(`PASS: ${results.length} core13 HTTP integration groups. Test records remain only in the isolated database.`);
