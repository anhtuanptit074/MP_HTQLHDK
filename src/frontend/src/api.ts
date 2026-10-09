export type Row = Record<string, unknown>;
export type Page = { items: Row[]; page?: { after?: number } };
export type Grant = { id: number; role: string; assetId: number | null };
export type Context = {
  userId: number;
  displayName: string;
  grants: Grant[];
  assets: Page;
  isAdmin: boolean;
};
let csrf = '';

export class ApiError extends Error {
  constructor(
    message: string,
    public status: number,
    public code: string,
  ) {
    super(message);
  }
}

export async function refreshCsrf() {
  const response = await fetch('/api/v1/auth/csrf', { credentials: 'same-origin' });
  const data = (await response.json()) as { token: string };
  csrf = data.token;
}

export async function request<T>(
  path: string,
  method = 'GET',
  body?: Row | FormData,
  version?: number,
): Promise<T> {
  if (method !== 'GET' && !csrf) await refreshCsrf();
  const headers: Record<string, string> = {};
  if (method !== 'GET') headers['X-CSRF-Token'] = csrf;
  if (version !== undefined) headers['If-Match'] = `"${version}"`;
  if (body && !(body instanceof FormData)) headers['Content-Type'] = 'application/json';
  const response = await fetch(`/api/v1/${path}`, {
    method,
    headers,
    credentials: 'same-origin',
    body: body instanceof FormData ? body : body ? JSON.stringify(body) : undefined,
    signal: method === 'GET' ? AbortSignal.timeout(15000) : undefined,
  }).catch(() => {
    throw new ApiError(
      'Không thể kết nối hệ thống. Vui lòng kiểm tra dịch vụ và thử lại.',
      503,
      'CONNECTION',
    );
  });
  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as {
      title?: string;
      code?: string;
    };
    throw new ApiError(
      problem.title ||
        (response.status === 401 ? 'Vui lòng đăng nhập.' : 'Không thể thực hiện yêu cầu.'),
      response.status,
      problem.code || 'REQUEST_FAILED',
    );
  }
  return response.status === 204 ? (undefined as T) : ((await response.json()) as T);
}

export function text(row: Row, key: string): string {
  return row[key] === null || row[key] === undefined ? '' : String(row[key]);
}
export function number(row: Row, key: string): number {
  return Number(row[key] ?? 0);
}
export function page(value: unknown): Row[] {
  return value && typeof value === 'object' && 'items' in value ? (value as Page).items : [];
}
export function date(value: unknown): string {
  if (!value) return '—';
  return new Date(String(value)).toLocaleString('vi-VN', {
    timeZone: 'Asia/Bangkok',
    dateStyle: 'short',
    timeStyle: 'short',
  });
}
export function localDate(value: unknown): string {
  const instant = value ? new Date(String(value)) : new Date();
  const shifted = new Date(instant.getTime() - instant.getTimezoneOffset() * 60000);
  return shifted.toISOString().slice(0, 16);
}
export async function download(path: string, name: string, reason?: string) {
  const response = await fetch(`/api/v1/${path}`, {
    credentials: 'same-origin',
    headers: reason ? { 'X-Access-Reason': reason } : {},
  });
  if (!response.ok) {
    const error = (await response.json().catch(() => ({}))) as { title?: string };
    throw new Error(error.title || 'Không thể tải tệp.');
  }
  const url = URL.createObjectURL(await response.blob());
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = name;
  anchor.click();
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
