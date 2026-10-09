import type { Context, Row } from './api';
import { number, text } from './api';
import type { Field } from './components';
import { labels } from './components';

export type FormSpec = {
  title: string;
  path: string;
  fields: Field[];
  method?: string;
  version?: number;
};
export type Inputs = {
  context: Context;
  users: Row[];
  profiles: Row[];
  scopes: Row[];
  assets: Row[];
};
export const reason: Field = { name: 'reason', label: 'Lý do / căn cứ', type: 'textarea' };
export const choices = (values: string[]) =>
  values.map((value) => ({ value, label: labels[value] || value }));
export const pick = (
  name: string,
  label: string,
  rows: Row[],
  value?: number,
  display = 'displayName',
): Field => ({
  name,
  label,
  type: 'select',
  value,
  options: rows.map((r) => ({
    value: number(r, 'id'),
    label: text(r, display) || text(r, 'code'),
  })),
});
const when = (name: string, label: string, value?: string): Field => ({
  name,
  label,
  type: 'datetime-local',
  value: value || new Date().toISOString(),
});
const memo = (name: string, label: string, value?: string): Field => ({
  name,
  label,
  type: 'textarea',
  value,
});
const select = (name: string, label: string, values: string[], value?: string): Field => ({
  name,
  label,
  type: 'select',
  options: choices(values),
  value,
});

export function creation(tab: string, inputs: Inputs, row: Row = {}): FormSpec | null {
  const humans = inputs.users.filter((u) => text(u, 'accountKind') === 'Human');
  const field = (name: string, label: string, value?: string | number): Field => ({
    name,
    label,
    value: value ?? text(row, name),
  });
  if (tab === 'assets')
    return {
      title: 'Khai báo tài sản',
      path: 'assets',
      fields: [
        field('code', 'Mã tài sản'),
        field('name', 'Tên website'),
        field('baseUrl', 'URL gốc'),
        select(
          'environment',
          'Môi trường',
          ['Test', 'Staging', 'Production'],
          text(row, 'environment') || 'Test',
        ),
        pick('ownerId', 'Chủ tài sản', humans, number(row, 'ownerId') || inputs.context.userId),
        pick('securityOfficerId', 'ATTT phụ trách', humans, number(row, 'securityOfficerId')),
        select(
          'criticality',
          'Mức quan trọng nghiệp vụ',
          ['Low', 'Medium', 'High', 'Critical'],
          text(row, 'criticality') || 'Medium',
        ),
        field(
          'dataClassification',
          'Phân loại dữ liệu',
          text(row, 'dataClassification') || 'Internal',
        ),
        {
          name: 'internetExposed',
          label: 'Công khai trên Internet',
          type: 'checkbox',
          value: Boolean(row.internetExposed),
        },
      ],
    };
  if (tab === 'plans')
    return {
      title: 'Tạo kế hoạch quét',
      path: 'plans',
      fields: [
        field('code', 'Mã kế hoạch'),
        field('name', 'Tên kế hoạch'),
        pick(
          'scopeId',
          'Phạm vi đã duyệt',
          inputs.scopes
            .filter((s) => s.status === 'Approved')
            .map((s) => ({
              ...s,
              displayName: `${inputs.context.assets.items.find((a) => a.id === s.assetId)?.name} · phiên bản ${s.versionNo}`,
            })),
          number(row, 'scopeId'),
        ),
        pick(
          'zapProfileId',
          'Hồ sơ ZAP đã duyệt',
          inputs.profiles.filter((p) => p.status === 'Active'),
          number(row, 'zapProfileId'),
          'name',
        ),
        pick('operatorId', 'Người vận hành', humans, number(row, 'operatorId')),
        {
          ...field(
            'recurrenceRule',
            'Chu kỳ quét',
            text(row, 'recurrenceRule') || 'FREQ=WEEKLY;BYDAY=MO',
          ),
          type: 'recurrence',
          hint: 'Giờ quét lấy từ thời điểm bắt đầu hiệu lực; ngày không tồn tại trong tháng được bỏ qua.',
        },
        field('timezone', 'Múi giờ', text(row, 'timezone') || 'Asia/Bangkok'),
        when('effectiveFrom', 'Bắt đầu hiệu lực', text(row, 'effectiveFrom')),
        {
          ...when('effectiveUntil', 'Kết thúc (tùy chọn)', text(row, 'effectiveUntil')),
          required: false,
          value: text(row, 'effectiveUntil'),
        },
      ],
    };
  if (tab === 'profiles')
    return {
      title: 'Tạo phiên bản hồ sơ ZAP',
      path: 'profiles',
      fields: [
        field('code', 'Mã hồ sơ'),
        field('name', 'Tên hồ sơ'),
        select(
          'spiderKind',
          'Spider',
          ['Traditional', 'Client'],
          text(row, 'spiderKind') || 'Traditional',
        ),
        {
          ...field(
            'maxDurationMinutes',
            'Thời gian tối đa (phút)',
            number(row, 'maxDurationMinutes') || 30,
          ),
          type: 'number',
        },
        {
          ...field(
            'requestDelayMs',
            'Khoảng nghỉ request (ms)',
            number(row, 'requestDelayMs') || 250,
          ),
          type: 'number',
        },
        {
          ...field('threadPerHost', 'Luồng trên host', number(row, 'threadPerHost') || 2),
          type: 'number',
        },
        select(
          'reportFormat',
          'Định dạng báo cáo',
          ['JSON', 'XML'],
          text(row, 'reportFormat') || 'JSON',
        ),
        pick(
          'serviceUserId',
          'Tài khoản dịch vụ',
          inputs.users.filter((u) => u.accountKind === 'Service'),
          number(row, 'serviceUserId'),
        ),
        select('authKind', 'Xác thực', ['None', 'Header'], text(row, 'authKind') || 'None'),
        { ...field('authHeaderName', 'Tên header'), required: false },
        {
          ...field('authSecretRef', 'Tham chiếu bí mật'),
          required: false,
          hint: 'env:HTQLHDK_ZAP_…; không nhập token thật.',
        },
        {
          name: 'activeScanEnabled',
          label: 'Đề nghị bật active scan',
          type: 'checkbox',
          value: Boolean(row.activeScanEnabled),
        },
      ],
    };
  if (tab === 'users')
    return {
      title: 'Tạo tài khoản',
      path: 'users',
      fields: [
        field('username', 'Tên đăng nhập'),
        field('displayName', 'Họ tên'),
        select('accountKind', 'Loại', ['Human', 'Service'], 'Human'),
        {
          name: 'password',
          label: 'Mật khẩu (người dùng)',
          type: 'password',
          required: false,
          hint: 'Tối thiểu 12 ký tự. Tài khoản dịch vụ không có mật khẩu.',
        },
      ],
    };
  if (tab === 'access-grants')
    return {
      title: 'Cấp quyền theo tài sản',
      path: 'access-grants',
      fields: [
        pick('userId', 'Người dùng', humans),
        select('roleCode', 'Vai trò', [
          'ADMIN',
          'SECURITY',
          'SCAN_OPERATOR',
          'REMEDIATOR',
          'AUDITOR',
        ]),
        {
          ...pick('assetId', 'Tài sản', inputs.assets, undefined, 'name'),
          required: false,
          hint: 'ADMIN để trống. Vai trò không gắn tài sản không cấp quyền trên mọi tài sản.',
        },
      ],
    };
  return null;
}
export function actions(
  tab: string,
  row: Row,
  inputs: Inputs,
): { label: string; spec: FormSpec }[] {
  const { context } = inputs;
  const has = (role: string, asset?: number) =>
    context.grants.some((g) => g.role === role && (asset === undefined || g.assetId === asset));
  const humans = inputs.users.filter((u) => u.accountKind === 'Human');
  const id = number(row, 'id');
  const asset =
    number(row, 'assetId') ||
    (tab === 'assets' ? id : number((row.finding as Row) || {}, 'assetId'));
  const security =
    has('SECURITY', asset) &&
    number(
      context.assets.items.find((item) => number(item, 'id') === asset) || {},
      'securityOfficerId',
    ) === context.userId;
  const result: { label: string; spec: FormSpec }[] = [];
  const add = (label: string, path: string, fields: Field[], method = 'POST') =>
    result.push({
      label,
      spec: { title: label, path, fields, method, version: number(row, 'version') || undefined },
    });
  const status = text(row, 'status');
  if (
    tab === 'assets' &&
    status === 'Draft' &&
    (number(row, 'ownerId') === context.userId || security)
  )
    add('Sửa thông tin tài sản', `assets/${id}`, creation('assets', inputs, row)!.fields, 'PATCH');
  if (tab === 'assets' && security && ['Active', 'Suspended'].includes(status))
    add('Chuyển trạng thái tài sản', `assets/${id}/transitions`, [
      select('toStatus', 'Trạng thái', [status === 'Active' ? 'Suspended' : 'Active', 'Retired']),
      reason,
    ]);
  if (tab === 'assets' && security && status !== 'Retired')
    add('Chuyển người phụ trách', `assets/${id}/assignment`, [
      pick('ownerId', 'Chủ tài sản', humans, number(row, 'ownerId')),
      pick(
        'securityOfficerId',
        'ATTT phụ trách (đã được cấp quyền tài sản)',
        humans,
        number(row, 'securityOfficerId'),
      ),
      reason,
    ]);
  if (tab === 'tickets' && security && row.hasEvidence)
    add('Tải bằng chứng gốc', `tickets/${id}/evidence`, [reason], 'GET');
  if (tab === 'profiles') {
    if (status === 'Draft' && number(row, 'createdBy') === context.userId && has('SCAN_OPERATOR')) {
      add('Sửa hồ sơ nháp', `profiles/${id}`, creation('profiles', inputs, row)!.fields, 'PUT');
      add('Gửi hồ sơ để duyệt', `profiles/${id}/submit`, [reason]);
    }
    if (status === 'Pending Approval' && has('SECURITY'))
      add('Phê duyệt hồ sơ ZAP', `profiles/${id}/decision`, [
        select('decision', 'Quyết định', ['Approved', 'Rejected']),
        { name: 'activeScanApproved', label: 'Cho phép active scan theo hồ sơ', type: 'checkbox' },
        reason,
      ]);
    if (status === 'Active' && has('SECURITY'))
      add('Ngừng sử dụng hồ sơ', `profiles/${id}/retire`, [reason]);
  }
  if (tab === 'plans') {
    if (security && ['Draft', 'Paused', 'Active'].includes(status))
      add('Chuyển trạng thái kế hoạch', `plans/${id}/transitions`, [
        select(
          'toStatus',
          'Trạng thái',
          status === 'Active' ? ['Paused', 'Cancelled'] : ['Active', 'Cancelled'],
        ),
        reason,
      ]);
    if (security && ['Draft', 'Paused'].includes(status) && row.planKind === 'Periodic')
      add('Sửa kế hoạch chưa chạy', `plans/${id}`, creation('plans', inputs, row)!.fields, 'PUT');
    if (status === 'Active' && (security || number(row, 'operatorId') === context.userId))
      add('Tạo lần quét đúng lịch', `plans/${id}/runs`, [
        when('scheduledFor', 'Thời điểm quét', text(row, 'nextDueAt')),
      ]);
  }
  if (tab === 'findings' && security) {
    if (['New', 'Triaged', 'Assigned', 'In Progress', 'Reopened'].includes(status))
      add('Xác nhận Final Severity', `findings/${id}/assess`, [
        select(
          'finalSeverity',
          'Mức độ cuối cùng',
          ['Informational', 'Low', 'Medium', 'High', 'Critical'],
          text(row, 'finalSeverity'),
        ),
        reason,
      ]);
    if (['New', 'Triaged'].includes(status))
      add('Kết quả sai / ngoài phạm vi', `findings/${id}/disposition`, [
        select('toStatus', 'Kết luận', ['False Positive', 'Cancelled']),
        reason,
      ]);
    if (['Triaged', 'Reopened'].includes(status))
      add('Phân công khắc phục', `findings/${id}/tickets`, [
        pick('assigneeId', 'Người khắc phục', humans),
        when('dueAt', 'Hạn xử lý', new Date(Date.now() + 7 * 86400000).toISOString()),
        reason,
      ]);
  }
  if (tab === 'tickets') {
    const findingStatus = text((row.finding as Row) || row, 'status');
    const progress = [
      memo('rootCauseMasked', 'Nguyên nhân', text(row, 'rootCauseMasked')),
      memo('remediationActionMasked', 'Biện pháp khắc phục', text(row, 'remediationActionMasked')),
      memo(
        'evidenceSummaryMasked',
        'Bằng chứng / kết quả kiểm tra',
        text(row, 'evidenceSummaryMasked'),
      ),
    ];
    if (
      number(row, 'assigneeId') === context.userId &&
      has('REMEDIATOR', asset) &&
      ['Assigned', 'In Progress'].includes(findingStatus)
    ) {
      add('Cập nhật tiến độ', `tickets/${id}/progress`, [
        {
          name: 'progressPercent',
          label: 'Tiến độ (%)',
          type: 'number',
          value: number(row, 'progressPercent'),
        },
        ...progress,
      ]);
      add('Gửi xác minh', `tickets/${id}/submit-verification`, progress.slice(1));
    }
    if (security && ['Assigned', 'In Progress', 'Reopened'].includes(findingStatus))
      add('Đổi phân công / hạn xử lý', `tickets/${id}/assignment`, [
        pick('assigneeId', 'Người khắc phục', humans, number(row, 'assigneeId')),
        when('dueAt', 'Hạn xử lý', text(row, 'dueAt')),
        reason,
      ]);
    if (security && findingStatus === 'Pending Verification')
      add('Lập kế hoạch xác minh', `tickets/${id}/verification-plans`, [
        pick(
          'scopeId',
          'Phạm vi đã duyệt',
          inputs.scopes
            .filter((s) => number(s, 'assetId') === asset && s.status === 'Approved')
            .map((s) => ({ ...s, displayName: `Phiên bản ${s.versionNo}` })),
        ),
        pick(
          'zapProfileId',
          'Hồ sơ ZAP',
          inputs.profiles.filter((p) => p.status === 'Active'),
          undefined,
          'name',
        ),
        pick('operatorId', 'Người vận hành', humans),
        when('requestedFor', 'Thời điểm xác minh', new Date(Date.now() + 15 * 60000).toISOString()),
      ]);
  }
  if (tab === 'runs') {
    const operator = number(row, 'operatorId') === context.userId && has('SCAN_OPERATOR', asset);
    if (
      ['Planned', 'Queued', 'Running'].includes(text(row, 'scanStatus')) &&
      (security || operator)
    )
      add('Dừng / hủy lần quét', `runs/${id}/cancel`, [reason]);
    if (['Planned', 'Queued'].includes(text(row, 'scanStatus')) && operator)
      add('Ghi nhận quét ngoài hệ thống', `runs/${id}/record-result`, [
        select('scanStatus', 'Kết quả', ['Completed', 'Failed']),
        when('startedAt', 'Bắt đầu'),
        when('finishedAt', 'Kết thúc'),
        reason,
      ]);
    if (row.reportStatus === 'Processed' && security)
      add('Tải báo cáo gốc', `runs/${id}/raw-report`, [reason], 'GET');
    if (
      row.planKind === 'Verification' &&
      row.reportStatus === 'Processed' &&
      !row.verificationOutcome &&
      security
    )
      add('Kết luận xác minh', `runs/${id}/verification-decision`, [
        {
          name: 'coverageConfirmed',
          label: 'Tôi xác nhận coverage đầy đủ cho phạm vi cần xác minh',
          type: 'checkbox',
        },
        reason,
      ]);
  }
  if (tab === 'users' && context.isAdmin)
    add(
      'Sửa / khóa tài khoản',
      `users/${id}`,
      [
        { name: 'displayName', label: 'Tên hiển thị', value: text(row, 'displayName') },
        {
          name: 'isActive',
          label: 'Cho phép hoạt động',
          type: 'checkbox',
          value: Boolean(row.isActive),
        },
        {
          name: 'password',
          label: 'Mật khẩu mới (để trống giữ nguyên)',
          type: 'password',
          required: false,
        },
      ],
      'PATCH',
    );
  if (tab === 'access-grants' && context.isAdmin && !row.revokedAt)
    add('Thu hồi quyền', `access-grants/${id}/revoke`, [reason]);
  return result;
}
