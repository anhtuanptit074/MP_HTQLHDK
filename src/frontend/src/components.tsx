import { useEffect, useRef, useState } from 'react';
import type { FormEvent, ReactNode } from 'react';
import { localDate, number, text } from './api';
import type { Row } from './api';

export const labels: Record<string, string> = {
  Draft: 'Bản nháp',
  'Pending Approval': 'Chờ phê duyệt',
  Approved: 'Đã phê duyệt',
  Rejected: 'Bị từ chối',
  Active: 'Đang hoạt động',
  Suspended: 'Tạm dừng',
  Retired: 'Ngừng sử dụng',
  Paused: 'Tạm dừng',
  Expired: 'Hết hiệu lực',
  Cancelled: 'Đã hủy',
  Planned: 'Đã lên lịch',
  Queued: 'Trong hàng đợi',
  Running: 'Đang quét',
  Completed: 'Hoàn thành',
  Failed: 'Thất bại',
  'Timed Out': 'Hết thời gian',
  'Not Received': 'Chưa nhận',
  Validating: 'Đang kiểm tra',
  Processed: 'Đã xử lý',
  New: 'Chưa phân loại',
  Triaged: 'Đã phân loại',
  Assigned: 'Đã phân công',
  'In Progress': 'Đang khắc phục',
  'Pending Verification': 'Chờ xác minh',
  Closed: 'Đã đóng',
  Reopened: 'Mở lại',
  'False Positive': 'Kết quả sai',
  Critical: 'Nghiêm trọng',
  High: 'Cao',
  Medium: 'Trung bình',
  Low: 'Thấp',
  Informational: 'Thông tin',
  Unassessed: 'Chưa đánh giá',
  ADMIN: 'Quản trị quyền',
  SECURITY: 'An toàn thông tin',
  SCAN_OPERATOR: 'Vận hành quét',
  REMEDIATOR: 'Khắc phục',
  AUDITOR: 'Kiểm toán',
  Periodic: 'Định kỳ',
  Verification: 'Xác minh',
  Success: 'Thành công',
  Denied: 'Bị từ chối',
  Failure: 'Lỗi',
  Human: 'Người dùng',
  Service: 'Dịch vụ',
};
export function Badge({ value }: { value: unknown }) {
  const code = String(value || 'Unassessed');
  return (
    <span className={`badge badge-${code.replaceAll(' ', '-').toLowerCase()}`}>
      {labels[code] || code}
    </span>
  );
}
export type Field = {
  name: string;
  label: string;
  type?:
    | 'text'
    | 'number'
    | 'password'
    | 'textarea'
    | 'select'
    | 'datetime-local'
    | 'checkbox'
    | 'recurrence';
  options?: { value: string | number; label: string }[];
  required?: boolean;
  value?: string | number | boolean;
  hint?: string;
};
export function Dialog({
  title,
  children,
  onClose,
}: {
  title: string;
  children: ReactNode;
  onClose: () => void;
}) {
  const ref = useRef<HTMLDialogElement>(null);
  useEffect(() => {
    ref.current?.showModal();
    const element = ref.current;
    return () => element?.close();
  }, []);
  return (
    <dialog
      ref={ref}
      onCancel={onClose}
      onClick={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div className="dialog-head">
        <div>
          <span className="eyebrow">HTQLHDK</span>
          <h2>{title}</h2>
        </div>
        <button className="icon-button" onClick={onClose} aria-label="Đóng">
          ×
        </button>
      </div>
      {children}
    </dialog>
  );
}
export function Form({
  fields,
  submit,
  onSubmit,
  children,
}: {
  fields: Field[];
  submit?: string;
  onSubmit: (body: Row) => Promise<void>;
  children?: ReactNode;
}) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  async function handle(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const data = new FormData(event.currentTarget);
    const body: Row = {};
    fields.forEach((field) => {
      const value = data.get(field.name);
      body[field.name] =
        field.type === 'checkbox'
          ? value === 'on'
          : field.type === 'number' || field.name.endsWith('Id')
            ? value
              ? Number(value)
              : null
            : field.type === 'datetime-local'
              ? value
                ? new Date(String(value)).toISOString()
                : null
              : value;
    });
    setBusy(true);
    setError('');
    try {
      await onSubmit(body);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Không thể lưu dữ liệu.');
    } finally {
      setBusy(false);
    }
  }
  return (
    <form onSubmit={(event) => void handle(event)} className="form-grid">
      {fields.map((field) => (
        <div
          key={field.name}
          className={
            field.type === 'textarea' || field.type === 'recurrence' ? 'field full' : 'field'
          }
        >
          <label htmlFor={field.type === 'recurrence' ? undefined : field.name}>
            {field.label}
            {field.required !== false && field.type !== 'checkbox' ? ' *' : ''}
          </label>
          {field.type === 'recurrence' ? (
            <RecurrenceField name={field.name} value={String(field.value || 'FREQ=DAILY')} />
          ) : field.type === 'select' ? (
            <select
              id={field.name}
              name={field.name}
              required={field.required !== false}
              defaultValue={String(field.value ?? '')}
            >
              <option value="">Chọn…</option>
              {field.options?.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          ) : field.type === 'textarea' ? (
            <textarea
              id={field.name}
              name={field.name}
              rows={3}
              required={field.required !== false}
              defaultValue={String(field.value ?? '')}
            />
          ) : (
            <input
              id={field.name}
              name={field.name}
              type={field.type ?? 'text'}
              required={field.required !== false && field.type !== 'checkbox'}
              defaultChecked={field.type === 'checkbox' && Boolean(field.value)}
              defaultValue={
                field.type === 'checkbox'
                  ? undefined
                  : field.type === 'datetime-local'
                    ? field.required === false && !field.value
                      ? ''
                      : localDate(field.value)
                    : String(field.value ?? '')
              }
              min={field.type === 'number' ? 0 : undefined}
            />
          )}
          {field.hint && <small>{field.hint}</small>}
        </div>
      ))}
      {children}
      {error && (
        <p className="error-banner full" role="alert">
          {error}
        </p>
      )}
      <div className="form-footer">
        <small>* Trường bắt buộc</small>
        <button className="primary" disabled={busy}>
          {busy ? 'Đang xử lý…' : submit || 'Lưu thay đổi'}
        </button>
      </div>
    </form>
  );
}
const weekdays = [
  { value: 'MO', label: 'Thứ Hai' },
  { value: 'TU', label: 'Thứ Ba' },
  { value: 'WE', label: 'Thứ Tư' },
  { value: 'TH', label: 'Thứ Năm' },
  { value: 'FR', label: 'Thứ Sáu' },
  { value: 'SA', label: 'Thứ Bảy' },
  { value: 'SU', label: 'Chủ nhật' },
];
export function recurrenceSummary(value: string): string {
  const parts = Object.fromEntries(value.split(';').map((part) => part.split('=')));
  const interval = Number(parts.INTERVAL || 1);
  return parts.FREQ === 'WEEKLY'
    ? `Mỗi ${interval} tuần · ${weekdays.find((day) => day.value === parts.BYDAY)?.label || 'theo ngày bắt đầu'}`
    : parts.FREQ === 'MONTHLY'
      ? `Mỗi ${interval} tháng · ngày ${parts.BYMONTHDAY || 'bắt đầu'}`
      : `Mỗi ${interval} ngày`;
}
function RecurrenceField({ name, value }: { name: string; value: string }) {
  const parts = Object.fromEntries(value.split(';').map((part) => part.split('=')));
  const [frequency, setFrequency] = useState(parts.FREQ || 'DAILY');
  const [interval, setInterval] = useState(Number(parts.INTERVAL || 1));
  const [day, setDay] = useState(parts.BYDAY || 'MO');
  const [monthDay, setMonthDay] = useState(Number(parts.BYMONTHDAY || 1));
  const rule = `FREQ=${frequency};INTERVAL=${interval}${frequency === 'WEEKLY' ? `;BYDAY=${day}` : frequency === 'MONTHLY' ? `;BYMONTHDAY=${monthDay}` : ''}`;
  return (
    <div className="recurrence-editor">
      <input type="hidden" name={name} value={rule} />
      <select
        aria-label="Chu kỳ quét"
        value={frequency}
        onChange={(event) => setFrequency(event.target.value)}
      >
        <option value="DAILY">Theo ngày</option>
        <option value="WEEKLY">Theo tuần</option>
        <option value="MONTHLY">Theo tháng</option>
      </select>
      <span>Mỗi</span>
      <input
        aria-label="Số chu kỳ giữa hai lần quét"
        type="number"
        min={1}
        max={365}
        required
        value={interval}
        onChange={(event) => setInterval(Number(event.target.value) || 1)}
      />
      <span>{frequency === 'DAILY' ? 'ngày' : frequency === 'WEEKLY' ? 'tuần' : 'tháng'}</span>
      {frequency === 'WEEKLY' && (
        <select aria-label="Thứ quét" value={day} onChange={(event) => setDay(event.target.value)}>
          {weekdays.map((item) => (
            <option key={item.value} value={item.value}>
              {item.label}
            </option>
          ))}
        </select>
      )}
      {frequency === 'MONTHLY' && (
        <select
          aria-label="Ngày quét trong tháng"
          value={monthDay}
          onChange={(event) => setMonthDay(Number(event.target.value))}
        >
          {Array.from({ length: 31 }, (_, index) => (
            <option key={index + 1} value={index + 1}>
              Ngày {index + 1}
            </option>
          ))}
        </select>
      )}
    </div>
  );
}
export function ScopeForm({
  asset,
  scope,
  onSubmit,
}: {
  asset: Row;
  scope?: Row;
  onSubmit: (body: Row) => Promise<void>;
}) {
  const defaults = scope?.rules as Row[] | undefined;
  const [rules, setRules] = useState<Row[]>(
    defaults || [
      {
        effect: 'Include',
        targetType: 'Domain',
        targetValue: new URL(text(asset, 'baseUrl')).hostname,
        matchMode: 'Exact',
      },
    ],
  );
  const [windows, setWindows] = useState<Row[]>(
    (scope?.windows as Row[] | undefined) || [
      {
        weekdayIso: 1,
        startTime: '01:00',
        endTime: '05:00',
        endsNextDay: false,
        timezone: 'Asia/Bangkok',
      },
    ],
  );
  return (
    <Form
      fields={[
        {
          name: 'changeReason',
          label: 'Lý do thay đổi phạm vi',
          type: 'textarea',
          value: text(scope || {}, 'changeReason'),
        },
      ]}
      onSubmit={(body) => onSubmit({ ...body, rules, windows })}
    >
      <section className="full scope-editor">
        <h3>Luật phạm vi</h3>
        <p className="muted">
          Luật loại trừ luôn được ưu tiên. Chỉ quét URL thuộc phạm vi được duyệt.
        </p>
        {rules.map((rule, index) => (
          <div className="repeat-row" key={index}>
            <select
              aria-label="Hiệu lực luật"
              value={text(rule, 'effect')}
              onChange={(e) =>
                setRules(rules.map((r, i) => (i === index ? { ...r, effect: e.target.value } : r)))
              }
            >
              <option value="Include">Cho phép</option>
              <option value="Exclude">Loại trừ</option>
            </select>
            <select
              aria-label="Loại mục tiêu"
              value={text(rule, 'targetType')}
              onChange={(e) =>
                setRules(
                  rules.map((r, i) => (i === index ? { ...r, targetType: e.target.value } : r)),
                )
              }
            >
              <option>Domain</option>
              <option>Subdomain</option>
              <option>URL</option>
            </select>
            <input
              aria-label="Giá trị phạm vi"
              value={text(rule, 'targetValue')}
              required
              onChange={(e) =>
                setRules(
                  rules.map((r, i) => (i === index ? { ...r, targetValue: e.target.value } : r)),
                )
              }
            />
            <select
              aria-label="Cách khớp"
              value={text(rule, 'matchMode')}
              onChange={(e) =>
                setRules(
                  rules.map((r, i) => (i === index ? { ...r, matchMode: e.target.value } : r)),
                )
              }
            >
              <option>Exact</option>
              <option>Prefix</option>
            </select>
            <button
              type="button"
              className="icon-button"
              aria-label="Xóa luật"
              onClick={() => setRules(rules.filter((_, i) => i !== index))}
            >
              ×
            </button>
          </div>
        ))}
        <button
          type="button"
          className="secondary"
          onClick={() =>
            setRules([
              ...rules,
              { effect: 'Exclude', targetType: 'URL', targetValue: '', matchMode: 'Prefix' },
            ])
          }
        >
          + Thêm luật
        </button>
        <h3>Khung giờ được phép quét</h3>
        {windows.map((window, index) => (
          <div className="repeat-row" key={index}>
            <select
              aria-label="Thứ"
              value={number(window, 'weekdayIso')}
              onChange={(e) =>
                setWindows(
                  windows.map((w, i) =>
                    i === index ? { ...w, weekdayIso: Number(e.target.value) } : w,
                  ),
                )
              }
            >
              {[1, 2, 3, 4, 5, 6, 7].map((day) => (
                <option key={day} value={day}>
                  {day === 7 ? 'Chủ nhật' : `Thứ ${day + 1}`}
                </option>
              ))}
            </select>
            <input
              type="time"
              aria-label="Giờ bắt đầu"
              value={text(window, 'startTime')}
              required
              onChange={(e) =>
                setWindows(
                  windows.map((w, i) => (i === index ? { ...w, startTime: e.target.value } : w)),
                )
              }
            />
            <input
              type="time"
              aria-label="Giờ kết thúc"
              value={text(window, 'endTime')}
              required
              onChange={(e) =>
                setWindows(
                  windows.map((w, i) => (i === index ? { ...w, endTime: e.target.value } : w)),
                )
              }
            />
            <label>
              <input
                type="checkbox"
                checked={Boolean(window.endsNextDay)}
                onChange={(e) =>
                  setWindows(
                    windows.map((w, i) =>
                      i === index ? { ...w, endsNextDay: e.target.checked } : w,
                    ),
                  )
                }
              />{' '}
              Qua đêm
            </label>
            <button
              type="button"
              className="icon-button"
              aria-label="Xóa khung giờ"
              onClick={() => setWindows(windows.filter((_, i) => i !== index))}
            >
              ×
            </button>
          </div>
        ))}
        <button
          type="button"
          className="secondary"
          onClick={() =>
            setWindows([
              ...windows,
              {
                weekdayIso: 1,
                startTime: '01:00',
                endTime: '05:00',
                endsNextDay: false,
                timezone: 'Asia/Bangkok',
              },
            ])
          }
        >
          + Thêm khung giờ
        </button>
        <p className="muted">Múi giờ: Asia/Bangkok (UTC+07)</p>
      </section>
    </Form>
  );
}
