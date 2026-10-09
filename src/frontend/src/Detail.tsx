import { useState } from 'react';
import { date, number, page, request, text } from './api';
import type { Row } from './api';
import { Badge, labels, recurrenceSummary } from './components';
import { actions, choices, reason } from './forms';
import type { FormSpec, Inputs } from './forms';

const describe: Record<string, string> = {
  baseUrl: 'URL tài sản',
  environment: 'Môi trường',
  criticality: 'Mức quan trọng',
  dataClassification: 'Phân loại dữ liệu',
  ownerId: 'Chủ tài sản',
  securityOfficerId: 'ATTT phụ trách',
  affectedUrlMasked: 'URL đã che',
  finalSeverity: 'Final Severity',
  severityReason: 'Căn cứ đánh giá',
  assigneeId: 'Người khắc phục',
  dueAt: 'Hạn xử lý',
  rootCauseMasked: 'Nguyên nhân',
  remediationActionMasked: 'Biện pháp',
  evidenceSummaryMasked: 'Bằng chứng khắc phục',
  recurrenceRule: 'Chu kỳ quét',
  timezone: 'Múi giờ',
  operatorId: 'Người vận hành',
  effectiveFrom: 'Hiệu lực từ',
  effectiveUntil: 'Hiệu lực đến',
  scheduledFor: 'Thời điểm quét',
  startedAt: 'Bắt đầu',
  finishedAt: 'Kết thúc',
  zapVersion: 'Phiên bản ZAP',
  failureCode: 'Mã lỗi',
  failureMessageMasked: 'Chi tiết lỗi',
  verificationOutcome: 'Kết quả xác minh',
  verificationReason: 'Căn cứ xác minh',
  spiderKind: 'Spider',
  activeScanEnabled: 'Active scan',
  maxDurationMinutes: 'Thời gian tối đa (phút)',
  authKind: 'Kiểu xác thực',
  reportFormat: 'Định dạng báo cáo',
  serviceUserId: 'Tài khoản dịch vụ',
  decisionReason: 'Căn cứ quyết định',
  status: 'Trạng thái',
  scanStatus: 'Trạng thái thực thi',
  reportStatus: 'Trạng thái báo cáo',
  progressPercent: 'Tiến độ (%)',
  roleCode: 'Vai trò',
  userId: 'Người dùng',
  displayName: 'Họ tên',
  accountKind: 'Loại tài khoản',
  isActive: 'Đang sử dụng',
  revokedAt: 'Ngày thu hồi',
  assessedBy: 'Người đánh giá',
  confirmedBy: 'Người xác nhận',
  decidedBy: 'Người phê duyệt',
  approvedBy: 'Người phê duyệt',
  updatedBy: 'Người cập nhật',
  coverageConfirmed: 'Đã xác nhận phạm vi xác minh',
  fingerprintPresent: 'Phát hiện còn tồn tại',
  versionNo: 'Phiên bản',
};
function AuditSnapshot({
  value,
  title,
  inputs,
}: {
  value: unknown;
  title: string;
  inputs: Inputs;
}) {
  const snapshot =
    value && typeof value === 'object' && !Array.isArray(value) ? (value as Row) : {};
  return (
    <section className="scope-card">
      <h3>{title}</h3>
      {Object.keys(snapshot).length === 0 ? (
        <p className="muted">Chưa có dữ liệu tại thời điểm này.</p>
      ) : (
        <dl className="detail-grid">
          {Object.entries(snapshot)
            .filter(([key]) => describe[key])
            .map(([key, entry]) => (
              <div key={key}>
                <dt>{describe[key]}</dt>
                <dd>
                  {entry === null
                    ? '—'
                    : key.endsWith('At') || key.endsWith('From') || key.endsWith('Until')
                      ? date(entry)
                      : key.endsWith('Id') || key.endsWith('By')
                        ? text(
                            inputs.users.find((user) => number(user, 'id') === Number(entry)) || {},
                            'displayName',
                          ) || String(entry)
                        : typeof entry === 'boolean'
                          ? entry
                            ? 'Có'
                            : 'Không'
                          : labels[String(entry)] || String(entry)}
                </dd>
              </div>
            ))}
          {Array.isArray(snapshot.rules) && (
            <div>
              <dt>Luật phạm vi</dt>
              <dd>
                {(snapshot.rules as Row[])
                  .map(
                    (rule) =>
                      `${text(rule, 'effect')} · ${text(rule, 'targetValue')} · ${text(rule, 'matchMode')}`,
                  )
                  .join(' ; ')}
              </dd>
            </div>
          )}
          {Array.isArray(snapshot.windows) && (
            <div>
              <dt>Khung giờ</dt>
              <dd>
                {(snapshot.windows as Row[])
                  .map(
                    (window) =>
                      `${number(window, 'weekdayIso') === 7 ? 'CN' : `T${number(window, 'weekdayIso') + 1}`} ${text(window, 'startTime')}–${text(window, 'endTime')}${window.endsNextDay ? ' (+1 ngày)' : ''}`,
                  )
                  .join(' ; ')}
              </dd>
            </div>
          )}
        </dl>
      )}
    </section>
  );
}
export function Detail({
  tab,
  row,
  inputs,
  setForm,
  setScopeForm,
  reload,
  openRun,
}: {
  tab: string;
  row: Row;
  inputs: Inputs;
  setForm: (form: FormSpec) => void;
  setScopeForm: (form: { asset: Row; scope?: Row }) => void;
  reload: () => void;
  openRun: (run: Row) => void;
}) {
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);
  const [evidenceSummary, setEvidenceSummary] = useState('');
  const owner = number(row, 'ownerId') === inputs.context.userId;
  const security =
    inputs.context.grants.some((g) => g.role === 'SECURITY' && g.assetId === number(row, 'id')) &&
    number(row, 'securityOfficerId') === inputs.context.userId;
  const assetId = number(row, 'assetId');
  const operator =
    number(row, 'operatorId') === inputs.context.userId &&
    inputs.context.grants.some((g) => g.role === 'SCAN_OPERATOR' && g.assetId === assetId);
  async function upload(file: File) {
    setBusy(true);
    setError('');
    const body = new FormData();
    body.append('file', file);
    body.append('format', file.name.toLowerCase().endsWith('.xml') ? 'XML' : 'JSON');
    try {
      await request(`runs/${number(row, 'id')}/report`, 'POST', body, number(row, 'version'));
      reload();
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Không nhập được báo cáo.');
    } finally {
      setBusy(false);
    }
  }
  async function uploadEvidence(file: File) {
    setBusy(true);
    setError('');
    const body = new FormData();
    body.append('file', file);
    body.append('summary', evidenceSummary);
    try {
      await request(`tickets/${number(row, 'id')}/evidence`, 'POST', body, number(row, 'version'));
      reload();
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Không tải được bằng chứng.');
    } finally {
      setBusy(false);
    }
  }
  return (
    <div className="detail-body">
      <div className="detail-top">
        <span className="code">{text(row, 'code') || `#${number(row, 'id')}`}</span>
        <Badge value={row.status || row.scanStatus || row.roleCode || row.outcome} />
      </div>
      <dl className="detail-grid">
        {Object.entries(describe)
          .filter(([key]) => row[key] !== undefined && row[key] !== null)
          .map(([key, label]) => (
            <div key={key}>
              <dt>{label}</dt>
              <dd>
                {key.endsWith('At') ||
                key.endsWith('From') ||
                key.endsWith('Until') ||
                key === 'scheduledFor'
                  ? date(row[key])
                  : key === 'recurrenceRule'
                    ? recurrenceSummary(text(row, key))
                    : key.endsWith('Id')
                      ? text(
                          inputs.users.find((u) => number(u, 'id') === number(row, key)) || {},
                          'displayName',
                        ) || text(row, key)
                      : typeof row[key] === 'boolean'
                        ? row[key]
                          ? 'Có'
                          : 'Không'
                        : labels[text(row, key)] || text(row, key)}
              </dd>
            </div>
          ))}
      </dl>
      {error && (
        <p className="error-banner" role="alert">
          {error}
        </p>
      )}
      <div className="actions detail-actions">
        {actions(tab, row, inputs).map((action, i) => (
          <button
            className={i === 0 ? 'primary' : 'secondary'}
            key={action.label}
            onClick={() => setForm(action.spec)}
          >
            {action.label}
          </button>
        ))}
        {tab === 'assets' &&
          (owner || security) &&
          ['Draft', 'Active', 'Suspended'].includes(text(row, 'status')) && (
            <button className="primary" onClick={() => setScopeForm({ asset: row })}>
              + Phiên bản phạm vi
            </button>
          )}
        {tab === 'runs' &&
          operator &&
          row.scanStatus === 'Completed' &&
          row.reportStatus !== 'Processed' && (
            <label className="primary upload-label">
              {busy ? 'Đang nhập…' : '↑ Nhập báo cáo JSON/XML'}
              <input
                type="file"
                accept=".json,.xml"
                disabled={busy}
                onChange={(event) => {
                  const file = event.target.files?.[0];
                  if (file) void upload(file);
                }}
              />
            </label>
          )}
      </div>
      {tab === 'tickets' &&
        number(row, 'assigneeId') === inputs.context.userId &&
        ['Assigned', 'In Progress'].includes(text((row.finding as Row) || {}, 'status')) && (
          <section className="scope-card">
            <h3>Tệp bằng chứng khắc phục</h3>
            <p className="muted">
              Tệp tối đa 10 MB được lưu mã hóa. Nhập mô tả trước khi chọn tệp.
            </p>
            <label className="field">
              Mô tả bằng chứng
              <textarea
                value={evidenceSummary}
                onChange={(event) => setEvidenceSummary(event.target.value)}
              />
            </label>
            <label className="secondary upload-label">
              ↑ Chọn tệp bằng chứng
              <input
                type="file"
                disabled={busy || evidenceSummary.trim().length < 3}
                onChange={(event) => {
                  const file = event.target.files?.[0];
                  if (file) void uploadEvidence(file);
                }}
              />
            </label>
          </section>
        )}
      {tab === 'assets' && Array.isArray(row.scopes) && (
        <section className="detail-section">
          <h3>Phiên bản phạm vi quét</h3>
          {(row.scopes as Row[]).map((scope) => (
            <article className="scope-card" key={number(scope, 'id')}>
              <div className="panel-heading">
                <strong>Phiên bản {text(scope, 'versionNo')}</strong>
                <Badge value={scope.status} />
              </div>
              <p>{text(scope, 'changeReason')}</p>
              <div className="scope-rules">
                {(scope.rules as Row[]).map((rule) => (
                  <div key={number(rule, 'id')}>
                    <Badge value={rule.effect === 'Include' ? 'Approved' : 'Rejected'} />{' '}
                    <code>{text(rule, 'targetValue')}</code>
                    <small>{text(rule, 'matchMode')}</small>
                  </div>
                ))}
              </div>
              <p className="muted">
                {(scope.windows as Row[])
                  .map(
                    (w) =>
                      `${number(w, 'weekdayIso') === 7 ? 'CN' : `T${number(w, 'weekdayIso') + 1}`} ${text(w, 'startTime')}–${text(w, 'endTime')}${w.endsNextDay ? ' (+1 ngày)' : ''}`,
                  )
                  .join(' · ')}{' '}
                · Asia/Bangkok
              </p>
              <div className="actions">
                {scope.status === 'Approved' && !scope.effectiveUntil && security && (
                  <button
                    className="secondary"
                    onClick={() =>
                      setForm({
                        title: 'Đặt ngày kết thúc phạm vi',
                        path: `assets/${number(row, 'id')}/scopes/${number(scope, 'id')}/end-effect`,
                        version: number(scope, 'version'),
                        fields: [
                          {
                            name: 'effectiveUntil',
                            label: 'Kết thúc ở thời điểm tương lai',
                            type: 'datetime-local',
                          },
                          reason,
                        ],
                      })
                    }
                  >
                    Đặt ngày kết thúc
                  </button>
                )}
                {scope.status === 'Draft' && (owner || security) && (
                  <button className="secondary" onClick={() => setScopeForm({ asset: row, scope })}>
                    Sửa phạm vi
                  </button>
                )}
                {scope.status === 'Draft' && owner && (
                  <button
                    className="primary"
                    onClick={() =>
                      setForm({
                        title: 'Chủ tài sản xác nhận phạm vi',
                        path: `assets/${number(row, 'id')}/scopes/${number(scope, 'id')}/confirm`,
                        fields: [reason],
                        version: number(scope, 'version'),
                      })
                    }
                  >
                    Xác nhận phạm vi
                  </button>
                )}
                {scope.status === 'Pending Approval' && security && (
                  <button
                    className="primary"
                    onClick={() =>
                      setForm({
                        title: 'Phê duyệt phạm vi',
                        path: `assets/${number(row, 'id')}/scopes/${number(scope, 'id')}/decision`,
                        version: number(scope, 'version'),
                        fields: [
                          {
                            name: 'decision',
                            label: 'Quyết định',
                            type: 'select',
                            options: choices(['Approved', 'Rejected']),
                          },
                          { name: 'effectiveFrom', label: 'Hiệu lực từ', type: 'datetime-local' },
                          {
                            name: 'effectiveUntil',
                            label: 'Hiệu lực đến (tùy chọn)',
                            type: 'datetime-local',
                            required: false,
                          },
                          reason,
                        ],
                      })
                    }
                  >
                    Phê duyệt / từ chối
                  </button>
                )}
              </div>
            </article>
          ))}
        </section>
      )}
      {tab === 'findings' && page(row.occurrences).length > 0 && (
        <section className="detail-section">
          <h3>Dữ liệu nguồn theo lần quét</h3>
          {page(row.occurrences).map((occurrence) => (
            <article className="scope-card" key={number(occurrence, 'id')}>
              <div className="panel-heading">
                <span>Lần quét #{number(occurrence, 'scanRunId')}</span>
                <Badge value={occurrence.sourceSeverity} />
              </div>
              <p>Source Confidence: {text(occurrence, 'sourceConfidence')} / 4</p>
              <p>{text(occurrence, 'descriptionMasked')}</p>
              <small>{text(occurrence, 'remediationGuidanceMasked')}</small>
            </article>
          ))}
        </section>
      )}
      {tab === 'tickets' && page(row.verificationRuns).length > 0 && (
        <section className="detail-section">
          <h3>Lịch sử xác minh</h3>
          {page(row.verificationRuns).map((run) => (
            <button className="work-item" key={number(run, 'id')} onClick={() => openRun(run)}>
              <span>
                {text(run, 'code')}
                <small>{date(run.scheduledFor)}</small>
              </span>
              <Badge value={run.verificationOutcome || run.scanStatus} />
            </button>
          ))}
        </section>
      )}
      {tab === 'audit-logs' && (
        <section className="detail-section">
          <p>{text(row, 'reasonMasked')}</p>
          <AuditSnapshot title="Trước thay đổi" value={row.beforeMasked} inputs={inputs} />
          <AuditSnapshot title="Sau thay đổi" value={row.afterMasked} inputs={inputs} />
          <small className="muted">Mã tra cứu: {text(row, 'correlationId')}</small>
        </section>
      )}
    </div>
  );
}
