import { normalizeEnvironmentName } from './config';

export function App() {
  const environment = normalizeEnvironmentName(import.meta.env.VITE_HTQLHDK_ENVIRONMENT);

  return (
    <main className="app-shell">
      <section className="status-panel" aria-labelledby="status-title">
        <p className="eyebrow">HTQLHDK · {environment}</p>
        <h1 id="status-title">Nền tảng phát triển đã sẵn sàng</h1>
        <p>
          Giai đoạn 10 chỉ cung cấp khung ứng dụng, tiêu chuẩn và kiểm tra tự động. Chức năng nghiệp
          vụ được triển khai theo từng lát cắt ở giai đoạn 11.
        </p>
        <dl>
          <div>
            <dt>Giao diện</dt>
            <dd>React 19.2 + TypeScript</dd>
          </div>
          <div>
            <dt>API/Worker</dt>
            <dd>ASP.NET Core/.NET 10 LTS</dd>
          </div>
          <div>
            <dt>Dữ liệu</dt>
            <dd>PostgreSQL 17</dd>
          </div>
        </dl>
      </section>
    </main>
  );
}
