#!/usr/bin/env python3
"""
Build a polished landing page for the published GitHub Pages site.
The page acts as a small hub in front of the generated Allure reports so the
published site feels closer to a curated dashboard instead of a raw report
dump.
"""
import json
import os
import re
from datetime import datetime, timezone
from pathlib import Path


def read_summary(summary_path: Path) -> dict:
    if not summary_path.exists():
        return {}
    try:
        return json.loads(summary_path.read_text(encoding='utf-8-sig'))
    except Exception:
        return {}


def badge(label: str, value: str, tone: str = 'neutral') -> str:
    return f'<span class="badge {tone}"><strong>{label}</strong><em>{value}</em></span>'


def slugify(value: str) -> str:
    if not value:
        return 'unassigned'
    s = value.lower()
    s = re.sub(r"[^a-z0-9]+", '-', s)
    s = s.strip('-')
    return s or 'owner'


def build_owner_cards(owners: list, owner_base: str, published_root: Path) -> str:
    if not owners:
        return '<div class="empty">No owners synced yet.</div>'
    cards = []
    for owner in owners:
        owner_safe = slugify(owner)
        owner_dir = published_root / 'report-extra' / 'owners' / owner_safe
        if owner_dir.exists():
            href = f'{owner_base}report-extra/owners/{owner_safe}/'
            cards.append(
                f'<a class="member-item link" href="{href}">{owner}</a>'
            )
        else:
            cards.append(
                f'<div class="member-item muted">{owner}</div>'
            )
    return ''.join(cards)


def build_suite_cards(report_url: str) -> str:
    suites = [
        ('Postman API Test', 'API report', 'API'),
        ('xUnit Backend Test', 'Backend report', 'BE'),
        ('E2E Frontend Test', 'E2E report', 'E2E'),
    ]
    cards = []
    for title, subtitle, icon in suites:
        cards.append(
            f'<a class="suite-card" href="{report_url}">'
            f'<div class="suite-icon">{icon}</div>'
            f'<div class="suite-copy">'
            f'<div class="suite-title">{title}</div>'
            f'<div class="suite-sub">{subtitle}</div>'
            f'</div>'
            f'</a>'
        )
    return ''.join(cards)


def main() -> None:
    # Sanitize all path inputs from environment variables (SonarCloud: path traversal prevention)
    site_output = Path(os.path.realpath(os.environ.get('SITE_OUTPUT_DIR', 'site-output')))
    validation_dir = Path(os.path.realpath(os.environ.get('VALIDATION_OUTPUT_DIR', 'report-extra/validation')))
    report_main = Path(os.path.realpath(os.environ.get('ALLURE_MAIN_REPORT_DIR', 'report-main')))
    report_extra = Path(os.path.realpath(os.environ.get('ALLURE_PUBLISH_DIR', 'report-extra')))

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
    owner_base = root_url

    generated_at = datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')

    site_output.mkdir(parents=True, exist_ok=True)

    suite_cards_html = build_suite_cards(report_url)
    owner_cards_html = build_owner_cards(owners, owner_base, site_output.parent if site_output.parent.exists() else Path('.'))

    # Status badges
    status_items = []
    if xunit_present:
        status_items.append(badge('xUnit', 'present', 'ok'))
    if postman_present:
        status_items.append(badge('Postman', 'present', 'ok'))
    if history_exists:
        status_items.append(badge('History', 'enabled', 'ok'))
    if categories_exists:
        status_items.append(badge('Categories', 'configured', 'ok'))
    if injected_count > 0:
        status_items.append(badge('Injected', str(injected_count), 'info'))
    if jira_total > 0:
        ratio = f'{jira_resolved}/{jira_total}'
        tone = 'ok' if jira_resolved == jira_total else 'warn'
        status_items.append(badge('Jira', ratio, tone))
    badges_html = ''.join(status_items)

    html = f'''<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>KCPM Test Dashboard</title>
  <style>
    :root {{
      --bg: #050b16;
      --bg2: #0a1322;
      --panel: rgba(12, 19, 34, 0.92);
      --panel-soft: rgba(255, 255, 255, 0.035);
      --line: rgba(255, 255, 255, 0.08);
      --text: #ecf3ff;
      --muted: #95a7c3;
      --accent: #7dd3fc;
      --accent2: #a78bfa;
      --shadow: 0 20px 60px rgba(0, 0, 0, 0.42);
      --radius: 20px;
    }}
    * {{ box-sizing: border-box; }}
    body {{
      margin: 0;
      font-family: Inter, "Segoe UI", system-ui, -apple-system, BlinkMacSystemFont, sans-serif;
      color: var(--text);
      background: radial-gradient(circle at top left, rgba(125, 211, 252, 0.18), transparent 28%),
                  radial-gradient(circle at top right, rgba(167, 139, 250, 0.13), transparent 24%),
                  linear-gradient(160deg, var(--bg), var(--bg2));
      min-height: 100vh;
    }}
    a {{ color: inherit; text-decoration: none; }}
    .wrap {{
      max-width: 1180px;
      margin: 0 auto;
      padding: 34px 20px 42px;
    }}
    .header {{ margin-bottom: 22px; }}
    h1 {{
      margin: 0;
      font-size: clamp(34px, 5vw, 58px);
      line-height: 1;
      letter-spacing: -0.03em;
    }}
    .meta {{
      margin-top: 10px;
      color: var(--muted);
      font-size: 13px;
    }}
    .section-title {{
      margin: 32px 0 14px;
      color: var(--muted);
      font-size: 12px;
      letter-spacing: .18em;
      text-transform: uppercase;
    }}
    .suite-grid {{
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 16px;
    }}
    .suite-card {{
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 18px 18px 18px 20px;
      border-radius: var(--radius);
      background: linear-gradient(180deg, rgba(255,255,255,0.05), rgba(255,255,255,0.03));
      border: 1px solid var(--line);
      box-shadow: var(--shadow);
      min-height: 110px;
      transition: transform .18s ease, border-color .18s ease, background .18s ease;
    }}
    .suite-card:hover {{
      transform: translateY(-2px);
      border-color: rgba(125, 211, 252, 0.28);
      background: rgba(255,255,255,0.06);
    }}
    .suite-icon {{
      width: 42px;
      height: 42px;
      flex: 0 0 42px;
      display: grid;
      place-items: center;
      border-radius: 13px;
      background: rgba(255,255,255,0.06);
      font-size: 14px;
      font-weight: 700;
      letter-spacing: -0.02em;
    }}
    .suite-copy {{ min-width: 0; }}
    .suite-title {{
      font-size: 16px;
      font-weight: 800;
      letter-spacing: -0.02em;
    }}
    .suite-sub {{
      margin-top: 5px;
      color: var(--muted);
      font-size: 13px;
    }}
    .member-list {{
      display: grid;
      grid-template-columns: 1fr;
      gap: 10px;
      max-width: 320px;
    }}
    .member-item {{
      padding: 14px 16px;
      border-radius: 14px;
      border: 1px solid var(--line);
      background: var(--panel-soft);
      font-weight: 700;
      letter-spacing: -0.01em;
    }}
    .member-item.link {{
      display: block;
      transition: transform .18s ease, border-color .18s ease, background .18s ease;
    }}
    .member-item.link:hover {{
      transform: translateY(-2px);
      border-color: rgba(125, 211, 252, 0.28);
      background: rgba(255, 255, 255, 0.06);
    }}
    .member-item.muted {{ opacity: 0.5; }}
    .empty {{
      color: var(--muted);
      padding: 12px 0;
    }}
    .badges {{ display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; }}
    .badge {{
      display: inline-flex;
      gap: 6px;
      align-items: center;
      padding: 4px 10px;
      border-radius: 20px;
      font-size: 12px;
      border: 1px solid var(--line);
      background: var(--panel-soft);
    }}
    .badge.ok {{ border-color: rgba(74, 222, 128, 0.3); color: #4ade80; }}
    .badge.warn {{ border-color: rgba(251, 191, 36, 0.3); color: #fbbf24; }}
    .badge.info {{ border-color: rgba(125, 211, 252, 0.3); color: var(--accent); }}
    .badge strong {{ font-weight: 600; }}
    .badge em {{ font-style: normal; opacity: 0.8; }}
    .cta {{
      display: inline-block;
      margin-top: 24px;
      padding: 14px 28px;
      border-radius: 14px;
      background: linear-gradient(135deg, var(--accent), var(--accent2));
      color: #050b16;
      font-weight: 800;
      font-size: 15px;
      letter-spacing: -0.01em;
      transition: transform .18s ease, box-shadow .18s ease;
      box-shadow: 0 4px 24px rgba(125, 211, 252, 0.25);
    }}
    .cta:hover {{
      transform: translateY(-2px);
      box-shadow: 0 8px 32px rgba(125, 211, 252, 0.38);
    }}
    footer {{
      margin-top: 48px;
      color: var(--muted);
      font-size: 12px;
      border-top: 1px solid var(--line);
      padding-top: 16px;
    }}
  </style>
</head>
<body>
  <div class="wrap">
    <div class="header">
      <h1>KCPM<br>Test Dashboard</h1>
      <div class="meta">Waste Recycling Platform &mdash; Quality Gate Report &mdash; {generated_at}</div>
      {f'<div class="badges">{badges_html}</div>' if badges_html else ''}
    </div>

    <div class="section-title">Test Suites</div>
    <div class="suite-grid">
      {suite_cards_html}
    </div>

    <a class="cta" href="{report_url}">Open Full Allure Report &rarr;</a>

    {f"""<div class="section-title">Team Members</div>
    <div class="member-list">{owner_cards_html}</div>""" if owners else ''}

    <footer>
      Auto-generated by CI &bull; <a href="{report_url}" style="color:var(--accent)">View Allure Report</a>
    </footer>
  </div>
</body>
</html>'''

    index_path = site_output / 'index.html'
    index_path.write_text(html, encoding='utf-8')
    print(f'[build_site_index] Written {index_path}')


if __name__ == '__main__':
    main()
