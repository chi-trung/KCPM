#!/usr/bin/env python3
"""
Build a polished landing page for the published GitHub Pages site.

The page acts as a small hub in front of the generated Allure reports so the
published site feels closer to a curated dashboard instead of a raw report
dump.
"""
import json
import os
from datetime import datetime, timezone
from pathlib import Path


def read_summary(summary_path: Path) -> dict:
    if not summary_path.exists():
        return {}
    try:
        return json.loads(summary_path.read_text(encoding='utf8'))
    except Exception:
        return {}


def badge(label: str, value: str, tone: str = 'neutral') -> str:
    return f'<span class="badge {tone}"><strong>{label}</strong><em>{value}</em></span>'


def build_owner_cards(owners: list[str], owner_base: str) -> str:
    if not owners:
        return '<div class="empty">Chưa có owner nào được sync.</div>'

    cards = []
    for owner in owners:
        slug = ''.join(ch.lower() if ch.isalnum() else '-' for ch in owner).strip('-')
        cards.append(
          f'<a class="owner-card" href="{owner_base}owners/{slug}/">'
            f'<span class="owner-name">{owner}</span>'
            f'<span class="owner-sub">Mở báo cáo theo owner</span>'
            f'</a>'
        )
    return ''.join(cards)


def main() -> None:
    site_output = Path(os.environ.get('SITE_OUTPUT_DIR', 'site-output'))
    validation_dir = Path(os.environ.get('VALIDATION_OUTPUT_DIR', 'report-extra/validation'))
    report_main = Path(os.environ.get('ALLURE_MAIN_REPORT_DIR', 'report-main'))
    report_extra = Path(os.environ.get('ALLURE_PUBLISH_DIR', 'report-extra'))

    summary = read_summary(validation_dir / 'summary.json')
    owners = summary.get('owners') or []
    owner_slugs = summary.get('owner_slugs') or []
    jira_total = int(summary.get('jira_total') or 0)
    jira_resolved = int(summary.get('jira_resolved') or 0)
    injected_count = int(summary.get('injected_count') or 0)
    xunit_present = bool(summary.get('xunit_present'))
    postman_present = bool(summary.get('postman_present'))
    history_exists = bool(summary.get('history_exists'))
    categories_exists = bool(summary.get('categories_exists'))
    executor_exists = bool(summary.get('executor_exists'))

    repo = os.environ.get('GITHUB_REPOSITORY', '')
    repo_owner, repo_name = repo.split('/', 1) if '/' in repo else ('', '')
    root_url = f'https://{repo_owner}.github.io/{repo_name}/' if repo_owner and repo_name else './'
    report_url = f'{root_url}report-main/'
    validation_url = f'{root_url}report-extra/validation/'
    owner_url = f'{root_url}report-extra/'

    generated_at = datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')

    site_output.mkdir(parents=True, exist_ok=True)

    html = f'''<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>KCPM Allure Hub</title>
  <style>
    :root {{
      --bg: #07111f;
      --bg2: #0d1726;
      --panel: rgba(15, 25, 40, 0.86);
      --panel-soft: rgba(255, 255, 255, 0.06);
      --text: #eaf1ff;
      --muted: #9db0ce;
      --accent: #7dd3fc;
      --accent2: #a78bfa;
      --good: #34d399;
      --warn: #fbbf24;
      --line: rgba(255, 255, 255, 0.1);
      --shadow: 0 22px 60px rgba(0, 0, 0, 0.35);
      --radius: 22px;
    }}
    * {{ box-sizing: border-box; }}
    body {{
      margin: 0;
      font-family: Inter, Segoe UI, system-ui, -apple-system, BlinkMacSystemFont, sans-serif;
      color: var(--text);
      background:
        radial-gradient(circle at top left, rgba(125, 211, 252, 0.22), transparent 28%),
        radial-gradient(circle at top right, rgba(167, 139, 250, 0.18), transparent 24%),
        linear-gradient(160deg, var(--bg), var(--bg2));
      min-height: 100vh;
    }}
    a {{ color: inherit; text-decoration: none; }}
    .wrap {{ max-width: 1240px; margin: 0 auto; padding: 32px 20px 48px; }}
    .hero {{
      display: grid;
      grid-template-columns: 1.4fr 0.9fr;
      gap: 20px;
      align-items: stretch;
    }}
    .panel {{
      background: var(--panel);
      border: 1px solid var(--line);
      box-shadow: var(--shadow);
      border-radius: var(--radius);
      backdrop-filter: blur(18px);
    }}
    .hero-main {{ padding: 30px; }}
    .kicker {{
      display: inline-flex;
      align-items: center;
      gap: 10px;
      padding: 8px 12px;
      border: 1px solid var(--line);
      border-radius: 999px;
      color: var(--muted);
      font-size: 13px;
      margin-bottom: 18px;
      background: rgba(255,255,255,0.03);
    }}
    h1 {{ margin: 0; font-size: clamp(36px, 5vw, 60px); line-height: 1.0; letter-spacing: -0.03em; }}
    .lead {{ margin: 18px 0 0; max-width: 70ch; color: var(--muted); font-size: 16px; line-height: 1.7; }}
    .actions {{ display: flex; flex-wrap: wrap; gap: 12px; margin-top: 24px; }}
    .btn {{
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
      padding: 14px 18px;
      border-radius: 14px;
      border: 1px solid var(--line);
      font-weight: 700;
      transition: transform .18s ease, border-color .18s ease, background .18s ease;
    }}
    .btn:hover {{ transform: translateY(-1px); border-color: rgba(255,255,255,0.24); }}
    .btn.primary {{ background: linear-gradient(135deg, var(--accent), var(--accent2)); color: #07111f; }}
    .btn.ghost {{ background: rgba(255,255,255,0.04); color: var(--text); }}
    .stats {{ padding: 22px; display: grid; gap: 12px; }}
    .stat-grid {{ display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }}
    .stat {{ padding: 16px; border-radius: 18px; background: rgba(255,255,255,0.05); border: 1px solid var(--line); }}
    .stat .label {{ display: block; color: var(--muted); font-size: 12px; text-transform: uppercase; letter-spacing: .12em; }}
    .stat .value {{ display: block; margin-top: 8px; font-size: 28px; font-weight: 800; }}
    .section {{ margin-top: 22px; display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 20px; }}
    .card {{ padding: 24px; }}
    .card h2 {{ margin: 0 0 8px; font-size: 22px; }}
    .card p {{ margin: 0; color: var(--muted); line-height: 1.7; }}
    .badge-row {{ display: flex; flex-wrap: wrap; gap: 10px; margin-top: 18px; }}
    .badge {{
      display: inline-flex;
      gap: 8px;
      align-items: center;
      padding: 10px 12px;
      border-radius: 14px;
      background: rgba(255,255,255,0.05);
      border: 1px solid var(--line);
    }}
    .badge strong {{ font-size: 12px; color: var(--muted); text-transform: uppercase; letter-spacing: .08em; }}
    .badge em {{ font-style: normal; font-weight: 800; }}
    .badge.good em {{ color: var(--good); }}
    .badge.warn em {{ color: var(--warn); }}
    .badge.neutral em {{ color: var(--text); }}
    .owners {{ margin-top: 22px; }}
    .owner-grid {{ display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }}
    .owner-card {{
      padding: 16px;
      border-radius: 18px;
      background: rgba(255,255,255,0.05);
      border: 1px solid var(--line);
      display: flex;
      flex-direction: column;
      gap: 6px;
      min-height: 94px;
    }}
    .owner-name {{ font-weight: 800; font-size: 16px; }}
    .owner-sub {{ color: var(--muted); font-size: 13px; }}
    .empty {{ padding: 14px 0; color: var(--muted); }}
    .footer {{ margin-top: 22px; color: var(--muted); font-size: 13px; }}
    @media (max-width: 960px) {{
      .hero, .section, .owner-grid, .stat-grid {{ grid-template-columns: 1fr; }}
    }}
  </style>
</head>
<body>
  <div class="wrap">
    <section class="hero">
      <div class="panel hero-main">
        <div class="kicker">KCPM · Allure Hub · generated {generated_at}</div>
        <h1>Test report portal for backend, owner sync, and validation.</h1>
        <p class="lead">
          This landing page sits in front of the generated Allure reports so the site feels more like a curated dashboard.
          It keeps the main report, per-owner reports, and validation links in one place.
        </p>
        <div class="actions">
          <a class="btn primary" href="{report_url}">Open main Allure report</a>
          <a class="btn ghost" href="{validation_url}">Open validation page</a>
        </div>
      </div>
      <aside class="panel stats">
        <div class="stat-grid">
          <div class="stat"><span class="label">Jira issues</span><span class="value">{jira_total}</span></div>
          <div class="stat"><span class="label">Resolved</span><span class="value">{jira_resolved}</span></div>
          <div class="stat"><span class="label">Injected owners</span><span class="value">{injected_count}</span></div>
          <div class="stat"><span class="label">Owners</span><span class="value">{len(owners)}</span></div>
        </div>
        <div class="badge-row">
          {badge('xUnit', 'present' if xunit_present else 'missing', 'good' if xunit_present else 'warn')}
          {badge('Postman', 'present' if postman_present else 'missing', 'good' if postman_present else 'warn')}
          {badge('History', 'present' if history_exists else 'missing', 'good' if history_exists else 'warn')}
          {badge('Categories', 'present' if categories_exists else 'missing', 'good' if categories_exists else 'warn')}
          {badge('Executor', 'present' if executor_exists else 'missing', 'good' if executor_exists else 'warn')}
        </div>
      </aside>
    </section>

    <section class="section">
      <article class="panel card">
        <h2>What this page gives you</h2>
        <p>
          A clean front door for the CI output, with direct links into the Allure report and the validation page.
          It is intentionally lighter than Allure itself, but more useful as a team landing page.
        </p>
        <div class="badge-row">
          <span class="badge neutral"><strong>report</strong><em>main Allure</em></span>
          <span class="badge neutral"><strong>report</strong><em>owner views</em></span>
          <span class="badge neutral"><strong>report</strong><em>validation</em></span>
        </div>
      </article>
      <article class="panel card owners">
        <h2>Owners discovered in this run</h2>
        <p>These are the assignee names extracted from Jira and injected into Allure result labels.</p>
        <div class="owner-grid">
          {build_owner_cards(owners, owner_url if owner_url.endswith('/') else owner_url + '/')}
        </div>
      </article>
    </section>

    <div class="footer">
      Root portal: {root_url} · Main report: {report_url} · Validation: {validation_url}
    </div>
  </div>
</body>
</html>'''

    (site_output / 'index.html').write_text(html, encoding='utf8')
    print('Wrote landing page to', site_output / 'index.html')


if __name__ == '__main__':
    main()