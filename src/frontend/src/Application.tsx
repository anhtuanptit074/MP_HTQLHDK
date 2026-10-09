import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { ApiError, date, download, number, page, refreshCsrf, request, text } from './api';
import type { Context, Page, Row } from './api';
import { Badge, Dialog, Form, labels, ScopeForm } from './components';
import { creation } from './forms';
import type { FormSpec, Inputs } from './forms';
import { Detail } from './Detail';

type Tab =
  | 'home'
  | 'assets'
  | 'profiles'
  | 'plans'
  | 'runs'
  | 'findings'
  | 'tickets'
  | 'reports'
  | 'audit-logs'
  | 'users'
  | 'access-grants';
type Column = [string, string, ('badge' | 'date' | 'progress')?];
const tabs: { id: Tab; label: string; icon: string; description: string }[] = [
  {
    id: 'home',
    label: 'Công việc của tôi',
    icon: '◈',
    description: 'Những việc cần chú ý trong phạm vi bạn phụ trách.',
  },
  {
    id: 'assets',
    label: 'Tài sản & phạm vi',
    icon: '▣',
    description: 'Quản lý website và phạm vi quét được phê duyệt.',
  },
  {
    id: 'profiles',
    label: 'Hồ sơ ZAP',
    icon: '⌘',
    description: 'Cấu hình quét có phiên bản và kiểm soát phê duyệt.',
  },
  {
    id: 'plans',
    label: 'Kế hoạch quét',
    icon: '▦',
    description: 'Lịch quét định kỳ và yêu cầu xác minh sau khắc phục.',
  },
  {
    id: 'runs',
    label: 'Lần quét & báo cáo',
    icon: '◎',
    description: 'Theo dõi thực thi và tiếp nhận báo cáo OWASP ZAP.',
  },
  {
    id: 'findings',
    label: 'Phát hiện',
    icon: '◇',
    description: 'Đánh giá mức độ, phân loại và theo dõi phát hiện.',
  },
  {
    id: 'tickets',
    label: 'Khắc phục & xác minh',
    icon: '✓',
    description: 'Phân công, bằng chứng và xác minh khắc phục.',
  },
  {
    id: 'reports',
    label: 'Báo cáo tổng hợp',
    icon: '▤',
    description: 'Số liệu tổng hợp từ dữ liệu bạn có quyền xem.',
  },
  {
    id: 'audit-logs',
    label: 'Nhật ký',
    icon: '◷',
    description: 'Lịch sử thao tác và quyết định trong phạm vi được cấp.',
  },
  {
    id: 'users',
    label: 'Người dùng',
    icon: '♙',
    description: 'Quản lý tài khoản người dùng và tài khoản dịch vụ.',
  },
  {
    id: 'access-grants',
    label: 'Phân quyền',
    icon: '⊞',
    description: 'Cấp và thu hồi vai trò theo từng tài sản.',
  },
];
const columns: Partial<Record<Tab, Column[]>> = {
  assets: [
    ['code', 'Mã tài sản'],
    ['name', 'Website'],
    ['environment', 'Môi trường'],
    ['criticality', 'Quan trọng'],
    ['status', 'Trạng thái', 'badge'],
  ],
  profiles: [
    ['code', 'Mã hồ sơ'],
    ['name', 'Tên hồ sơ'],
    ['versionNo', 'Phiên bản'],
    ['reportFormat', 'Định dạng'],
    ['status', 'Trạng thái', 'badge'],
  ],
  plans: [
    ['code', 'Kế hoạch'],
    ['assetName', 'Tài sản'],
    ['planKind', 'Loại', 'badge'],
    ['nextDueAt', 'Kỳ tiếp theo · UTC+07', 'date'],
    ['status', 'Trạng thái', 'badge'],
  ],
  runs: [
    ['code', 'Lần quét'],
    ['planName', 'Kế hoạch'],
    ['scheduledFor', 'Thời điểm · UTC+07', 'date'],
    ['scanStatus', 'Thực thi', 'badge'],
    ['reportStatus', 'Báo cáo', 'badge'],
  ],
  findings: [
    ['code', 'Mã phát hiện'],
    ['titleMasked', 'Phát hiện'],
    ['assetName', 'Tài sản'],
    ['sourceSeverity', 'Source Risk', 'badge'],
    ['finalSeverity', 'Final Severity', 'badge'],
    ['status', 'Trạng thái', 'badge'],
  ],
  tickets: [
    ['code', 'Phiếu'],
    ['titleMasked', 'Phát hiện'],
    ['dueAt', 'Hạn xử lý · UTC+07', 'date'],
    ['progressPercent', 'Tiến độ', 'progress'],
    ['status', 'Trạng thái', 'badge'],
  ],
  'audit-logs': [
    ['occurredAt', 'Thời điểm · UTC+07', 'date'],
    ['actorId', 'Người thực hiện'],
    ['action', 'Thao tác'],
    ['objectType', 'Đối tượng'],
    ['objectId', 'Mã'],
    ['outcome', 'Kết quả', 'badge'],
  ],
  users: [
    ['username', 'Tên đăng nhập'],
    ['displayName', 'Họ tên'],
    ['accountKind', 'Loại', 'badge'],
    ['isActive', 'Đang sử dụng'],
  ],
  'access-grants': [
    ['displayName', 'Người dùng'],
    ['roleCode', 'Vai trò', 'badge'],
    ['assetName', 'Tài sản'],
    ['grantedAt', 'Ngày cấp · UTC+07', 'date'],
    ['revokedAt', 'Ngày thu hồi', 'date'],
  ],
};

export function Application() {
  const [context, setContext] = useState<Context | null>(null);
  const [checking, setChecking] = useState(true);
  const [tab, setTab] = useState<Tab>('home');
  const [data, setData] = useState<Row>({});
  const [users, setUsers] = useState<Row[]>([]);
  const [profiles, setProfiles] = useState<Row[]>([]);
  const [scopes, setScopes] = useState<Row[]>([]);
  const [grantAssets, setGrantAssets] = useState<Row[]>([]);
  const [selected, setSelected] = useState<Row | null>(null);
  const [form, setForm] = useState<FormSpec | null>(null);
  const [scopeForm, setScopeForm] = useState<{ asset: Row; scope?: Row } | null>(null);
  const [search, setSearch] = useState('');
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');
  const [loading, setLoading] = useState(false);
  const [refresh, setRefresh] = useState(0);
  const [cursor, setCursor] = useState(0);
  const has = (role: string) => Boolean(context?.grants.some((g) => g.role === role));
  const eligible = has('SECURITY') || has('SCAN_OPERATOR');
  const reload = () => {
    setSelected(null);
    setForm(null);
    setScopeForm(null);
    setNotice('Đã lưu thành công.');
    setRefresh((r) => r + 1);
  };

  useEffect(() => {
    request<Context>('context')
      .then((result) => {
        setContext(result);
        if (result.isAdmin && !result.assets.items.length) setTab('users');
      })
      .catch((caught: unknown) => {
        if (!(caught instanceof ApiError && caught.status === 401))
          setError(caught instanceof Error ? caught.message : 'Không thể kết nối.');
      })
      .finally(() => setChecking(false));
  }, []);
  useEffect(() => {
    if (!context) return;
    let active = true;
    request<Page>('users')
      .then((result) => {
        if (active) setUsers(result.items);
      })
      .catch(() => {});
    request<Context>('context')
      .then(async (result) => {
        if (!active) return;
        setContext(result);
        const loaded = await Promise.allSettled(
          result.assets.items.map((asset) => request<Row>(`assets/${number(asset, 'id')}`)),
        );
        if (active)
          setScopes(
            loaded.flatMap((value) =>
              value.status === 'fulfilled' ? (value.value.scopes as Row[]) || [] : [],
            ),
          );
      })
      .catch(() => {});
    if (eligible)
      request<Page>('profiles')
        .then((result) => {
          if (active) setProfiles(result.items);
        })
        .catch(() => {});
    if (context.isAdmin)
      request<Page>('access-assets')
        .then((result) => {
          if (active) setGrantAssets(result.items);
        })
        .catch(() => {});
    return () => {
      active = false;
    };
  }, [context?.userId, context?.isAdmin, refresh, eligible]);
  useEffect(() => {
    if (!context) return;
    let active = true;
    setLoading(true);
    setError('');
    request<Row>(tab === 'home' ? 'notifications' : `${tab}?page[size]=100&page[after]=${cursor}`)
      .then((result) => {
        if (active) setData(result);
      })
      .catch((caught: unknown) => {
        if (!active) return;
        if (caught instanceof ApiError && caught.status === 401) setContext(null);
        else setError(caught instanceof Error ? caught.message : 'Không tải được dữ liệu.');
      })
      .finally(() => {
        if (active) setLoading(false);
      });
    return () => {
      active = false;
    };
  }, [context?.userId, tab, refresh, cursor]);
  async function login(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setLoading(true);
    setError('');
    const values = new FormData(event.currentTarget);
    try {
      await refreshCsrf();
      await request('auth/login', 'POST', {
        username: values.get('username'),
        password: values.get('password'),
      });
      await refreshCsrf();
      const result = await request<Context>('context');
      setContext(result);
      setTab(result.isAdmin && !result.assets.items.length ? 'users' : 'home');
      setRefresh((r) => r + 1);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Đăng nhập thất bại.');
    } finally {
      setLoading(false);
    }
  }
  async function logout() {
    await request('auth/logout', 'POST');
    setContext(null);
    setSelected(null);
    setTab('home');
    await refreshCsrf();
  }
  async function open(row: Row, target: Tab = tab) {
    setError('');
    try {
      setSelected(
        ['assets', 'findings', 'tickets', 'runs'].includes(target)
          ? await request<Row>(`${target}/${number(row, 'id')}`)
          : row,
      );
      if (target !== tab) setTab(target);
    } catch (caught) {
      setError(caught instanceof Error ? caught.message : 'Không tải được chi tiết.');
    }
  }
  const info = tabs.find((item) => item.id === tab)!;
  const visible = tabs.filter((item) =>
    ['users', 'access-grants'].includes(item.id)
      ? context?.isAdmin
      : item.id === 'profiles'
        ? eligible
        : item.id === 'audit-logs'
          ? context?.isAdmin || has('SECURITY') || has('AUDITOR')
          : !context?.isAdmin || Boolean(context.assets.items.length),
  );
  const inputs: Inputs | null = context
    ? { context, users, profiles, scopes, assets: grantAssets }
    : null;
  const rows = page(data).filter((row) =>
    Object.values(row).some((value) =>
      String(value || '')
        .toLocaleLowerCase('vi-VN')
        .includes(search.toLocaleLowerCase('vi-VN')),
    ),
  );
  const canCreate =
    tab === 'assets' ||
    (tab === 'plans' && has('SECURITY')) ||
    (tab === 'profiles' && has('SCAN_OPERATOR')) ||
    (['users', 'access-grants'].includes(tab) && context?.isAdmin);
  if (checking) return <div className="loading-screen">Đang kết nối hệ thống…</div>;
  if (!context)
    return (
      <div className="login-layout">
        <section className="login-story">
          <div className="brand">
            <span className="brand-mark">H</span> HTQLHDK
          </div>
          <div>
            <span className="eyebrow">WEBSITE SECURITY OPERATIONS</span>
            <h1>
              Kiểm soát rủi ro.
              <br />
              Theo sát từng
              <br />
              <span>lần khắc phục.</span>
            </h1>
            <p>
              Một nơi quản lý tài sản, phạm vi quét, kết quả OWASP ZAP và công việc khắc phục của
              bạn.
            </p>
            <div className="login-chips">
              <span>Phạm vi được duyệt</span>
              <span>Phân quyền theo nhiệm vụ</span>
              <span>Truy vết đầy đủ</span>
            </div>
          </div>
          <small>Hệ thống quản lý quét lỗ hổng website định kỳ</small>
        </section>
        <section className="login-card">
          <span className="eyebrow">CHÀO MỪNG TRỞ LẠI</span>
          <h2>Đăng nhập</h2>
          <p className="muted">Sử dụng tài khoản do quản trị viên cấp.</p>
          <form onSubmit={(event) => void login(event)}>
            <label className="field">
              Tên đăng nhập
              <input name="username" autoComplete="username" required autoFocus />
            </label>
            <label className="field">
              Mật khẩu
              <input name="password" type="password" autoComplete="current-password" required />
            </label>
            {error && (
              <p role="alert" className="error-banner">
                {error}
              </p>
            )}
            <button className="primary" disabled={loading}>
              {loading ? 'Đang đăng nhập…' : 'Đăng nhập →'}
            </button>
          </form>
          <small>Quyền truy cập được kiểm tra theo tài sản và nhiệm vụ được giao.</small>
        </section>
      </div>
    );
  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="brand">
          <span className="brand-mark">H</span>
          <div>
            HTQLHDK<small>SECURITY OPERATIONS</small>
          </div>
        </div>
        <div className="workspace-label">KHÔNG GIAN LÀM VIỆC</div>
        <nav aria-label="Điều hướng chính">
          {visible.map((item) => (
            <button
              key={item.id}
              className={tab === item.id ? 'nav-item active' : 'nav-item'}
              onClick={() => {
                setTab(item.id);
                setCursor(0);
                setSearch('');
                setSelected(null);
                setNotice('');
              }}
            >
              <span>{item.icon}</span>
              {item.label}
            </button>
          ))}
        </nav>
        <div className="sidebar-bottom">
          <span className="online-dot" /> Phiên làm việc được bảo vệ
          <small>Quyền theo phạm vi · UTC+07</small>
        </div>
      </aside>
      <div className="main-shell">
        <header className="topbar">
          <div className="breadcrumb">
            Không gian làm việc <span>/</span> <strong>{info.label}</strong>
          </div>
          <div className="user-menu">
            <span className="avatar">{context.displayName.charAt(0)}</span>
            <div>
              {context.displayName}
              <small>
                {context.isAdmin
                  ? labels.ADMIN
                  : has('SECURITY')
                    ? labels.SECURITY
                    : has('SCAN_OPERATOR')
                      ? labels.SCAN_OPERATOR
                      : has('REMEDIATOR')
                        ? labels.REMEDIATOR
                        : has('AUDITOR')
                          ? labels.AUDITOR
                          : 'Chủ tài sản'}
              </small>
            </div>
            <button className="text-button" onClick={() => void logout()}>
              Đăng xuất
            </button>
          </div>
        </header>
        <main>
          <div className="page-heading">
            <div>
              <span className="eyebrow">VẬN HÀNH BẢO MẬT WEBSITE</span>
              <h1>{info.label}</h1>
              <p>{info.description}</p>
            </div>
            <div className="actions">
              <button
                className="secondary"
                onClick={() => {
                  setNotice('');
                  setRefresh((r) => r + 1);
                }}
              >
                ↻ Làm mới
              </button>
              {canCreate && (
                <button className="primary" onClick={() => setForm(creation(tab, inputs!))}>
                  +{' '}
                  {tab === 'assets'
                    ? 'Khai báo tài sản'
                    : tab === 'plans'
                      ? 'Tạo kế hoạch'
                      : tab === 'profiles'
                        ? 'Tạo hồ sơ'
                        : tab === 'users'
                          ? 'Tạo tài khoản'
                          : 'Cấp quyền'}
                </button>
              )}
            </div>
          </div>
          {notice && (
            <div className="success-banner" role="status">
              ✓ {notice}
            </div>
          )}
          {error && (
            <div className="error-banner" role="alert">
              {error}
            </div>
          )}
          {loading ? (
            <div className="panel empty-state">Đang tải dữ liệu…</div>
          ) : tab === 'home' ? (
            <>
              <div className="welcome-panel">
                <div>
                  <span className="eyebrow">CÔNG VIỆC HÔM NAY</span>
                  <h2>Xin chào, {context.displayName}.</h2>
                  <p>Ưu tiên những phát hiện quan trọng và công việc đến hạn.</p>
                </div>
                <div className="welcome-symbol">◎</div>
              </div>
              <div className="stat-grid">
                {[
                  ['Tài sản trong phạm vi', context.assets.items.length, 'assets'],
                  ['Phát hiện cần chú ý', page(data.urgentFindings).length, 'findings'],
                  ['Phiếu quá hạn', page(data.overdueTickets).length, 'tickets'],
                  ['Lần quét gặp lỗi', page(data.failedRuns).length, 'runs'],
                ].map(([label, count, target]) => (
                  <button
                    className="stat-card"
                    key={String(label)}
                    onClick={() => setTab(target as Tab)}
                  >
                    <span>{label}</span>
                    <strong>{count}</strong>
                    <small>Xem chi tiết ↗</small>
                  </button>
                ))}
              </div>
              <div className="work-grid">
                {[
                  ['urgentFindings', 'Phát hiện ưu tiên', 'findings'],
                  ['overdueTickets', 'Khắc phục quá hạn', 'tickets'],
                  ['failedRuns', 'Lần quét cần kiểm tra', 'runs'],
                ].map(([key, title, target]) => (
                  <section className="panel" key={key}>
                    <div className="panel-heading">
                      <h3>{title}</h3>
                      <span className="count">{page(data[key]).length}</span>
                    </div>
                    {page(data[key]).length ? (
                      page(data[key]).map((row) => (
                        <button
                          className="work-item"
                          key={number(row, 'id')}
                          onClick={() => void open(row, target as Tab)}
                        >
                          <span>
                            <strong>{text(row, 'titleMasked') || text(row, 'code')}</strong>
                            <small>
                              {text(row, 'code')} · {date(row.dueAt || row.scheduledFor)}
                            </small>
                          </span>
                          <Badge value={row.finalSeverity || row.scanStatus || row.status} />
                        </button>
                      ))
                    ) : (
                      <div className="empty-inline">
                        <span>✓</span>Không có công việc cần chú ý.
                      </div>
                    )}
                  </section>
                ))}
              </div>
            </>
          ) : tab === 'reports' ? (
            <>
              <div className="report-actions">
                <p className="muted">Dữ liệu đã che, chỉ bao gồm phạm vi bạn được phép truy cập.</p>
                <button
                  className="primary"
                  onClick={() =>
                    void download('reports/export', 'bao-cao-phat-hien.csv').catch((e: unknown) =>
                      setError(e instanceof Error ? e.message : 'Không tải được báo cáo.'),
                    )
                  }
                >
                  ↓ Xuất CSV
                </button>
              </div>
              <div className="work-grid">
                {[
                  ['byStatus', 'Theo trạng thái', 'status'],
                  ['bySeverity', 'Theo mức độ cuối cùng', 'severity'],
                ].map(([key, title, label]) => (
                  <section className="panel" key={key}>
                    <div className="panel-heading">
                      <h3>{title}</h3>
                    </div>
                    {page(data[key]).length ? (
                      page(data[key]).map((row) => (
                        <div className="summary-row" key={text(row, label)}>
                          <Badge value={row[label]} />
                          <div className="bar-track">
                            <div
                              style={{
                                width: `${(number(row, 'count') / Math.max(...page(data[key]).map((r) => number(r, 'count')), 1)) * 100}%`,
                              }}
                            />
                          </div>
                          <strong>{number(row, 'count')}</strong>
                        </div>
                      ))
                    ) : (
                      <div className="empty-inline">Chưa có dữ liệu trong phạm vi của bạn.</div>
                    )}
                  </section>
                ))}
              </div>
            </>
          ) : (
            <section className="panel table-panel">
              <div className="table-toolbar">
                <div>
                  <h3>{info.label}</h3>
                  <span className="count">{rows.length}</span>
                </div>
                <input
                  className="search-input"
                  aria-label="Tìm kiếm trong danh sách"
                  placeholder="Tìm theo mã, tên hoặc trạng thái…"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </div>
              <div className="table-scroll">
                <table>
                  <thead>
                    <tr>
                      {columns[tab]?.map(([key, label]) => (
                        <th key={key}>{label}</th>
                      ))}
                      <th aria-label="Thao tác" />
                    </tr>
                  </thead>
                  <tbody>
                    {rows.map((row) => (
                      <tr key={number(row, 'id')}>
                        {columns[tab]?.map(([key, , kind]) => (
                          <td key={key}>
                            {kind === 'badge' ? (
                              <Badge value={row[key]} />
                            ) : kind === 'date' ? (
                              date(row[key])
                            ) : kind === 'progress' ? (
                              <div className="progress">
                                <span style={{ width: `${number(row, key)}%` }} />
                                <small>{number(row, key)}%</small>
                              </div>
                            ) : key === 'isActive' ? (
                              row[key] ? (
                                'Có'
                              ) : (
                                'Đã khóa'
                              )
                            ) : (
                              <span className={key === 'code' ? 'code' : ''}>
                                {text(row, key) || '—'}
                              </span>
                            )}
                          </td>
                        ))}
                        <td>
                          <button className="row-action" onClick={() => void open(row)}>
                            Chi tiết ↗
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {!rows.length && (
                <div className="empty-state">
                  <span>◈</span>
                  <h3>{search ? 'Không có kết quả phù hợp' : 'Chưa có dữ liệu'}</h3>
                  <p>
                    {search
                      ? 'Thử thay đổi nội dung tìm kiếm.'
                      : 'Dữ liệu sẽ xuất hiện khi được tạo hoặc cấp quyền trong phạm vi của bạn.'}
                  </p>
                </div>
              )}
              <div className="table-footer">
                <span>Dữ liệu được lọc theo quyền hiện hành.</span>
                <div>
                  {cursor > 0 && (
                    <button className="text-button" onClick={() => setCursor(0)}>
                      Về đầu
                    </button>
                  )}
                  {page(data).length === 100 && (
                    <button
                      className="text-button"
                      onClick={() => setCursor(number(page(data).at(-1) || {}, 'id'))}
                    >
                      Trang tiếp →
                    </button>
                  )}
                </div>
              </div>
            </section>
          )}
        </main>
        <footer className="app-footer">
          HTQLHDK <span>Phạm vi quét được duyệt · Dữ liệu nhạy cảm được bảo vệ</span>
        </footer>
      </div>
      {selected && (
        <Dialog
          title={
            text(selected, 'name') ||
            text(selected, 'titleMasked') ||
            text(selected, 'displayName') ||
            text(selected, 'code') ||
            'Chi tiết'
          }
          onClose={() => setSelected(null)}
        >
          <Detail
            tab={tab}
            row={selected}
            inputs={inputs!}
            setForm={setForm}
            setScopeForm={setScopeForm}
            reload={reload}
            openRun={(row) => void open(row, 'runs')}
          />
        </Dialog>
      )}
      {form && (
        <Dialog title={form.title} onClose={() => setForm(null)}>
          <Form
            fields={form.fields}
            onSubmit={async (body) => {
              if (form.method === 'GET') {
                await download(
                  form.path,
                  form.path.endsWith('/evidence') ? 'bang-chung.bin' : 'zap-report',
                  String(body.reason),
                );
                setForm(null);
              } else {
                await request(form.path, form.method || 'POST', body, form.version);
                reload();
              }
            }}
          />
        </Dialog>
      )}
      {scopeForm && (
        <Dialog
          title={scopeForm.scope ? 'Sửa phạm vi nháp' : 'Tạo phiên bản phạm vi'}
          onClose={() => setScopeForm(null)}
        >
          <ScopeForm
            asset={scopeForm.asset}
            scope={scopeForm.scope}
            onSubmit={async (body) => {
              await request(
                `assets/${number(scopeForm.asset, 'id')}/scopes${scopeForm.scope ? `/${number(scopeForm.scope, 'id')}` : ''}`,
                scopeForm.scope ? 'PUT' : 'POST',
                body,
                scopeForm.scope ? number(scopeForm.scope, 'version') : undefined,
              );
              reload();
            }}
          />
        </Dialog>
      )}
    </div>
  );
}
