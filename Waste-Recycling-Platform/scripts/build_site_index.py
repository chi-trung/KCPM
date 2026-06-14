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
    icons = {
        'xUnit': '🧪',
        'Postman': '📬',
        'History': '📊',
        'Categories': '🏷️',
        'Injected': '💉',
        'Jira': '🎯',
    }
    icon = icons.get(label, '•')
    return f'<span class="badge {tone}"><span class="badge-icon">{icon}</span><strong>{label}</strong><em>{value}</em></span>'


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
    colors = ['#7dd3fc', '#a78bfa', '#34d399', '#fb923c', '#f472b6', '#facc15']
    for i, owner in enumerate(owners):
        owner_safe = slugify(owner)
        owner_dir = published_root / 'report-extra' / 'owners' / owner_safe
        color = colors[i % len(colors)]
        initials = ''.join([w[0].upper() for w in owner.replace('_', ' ').split() if w][:2])
        if owner_dir.exists():
            href = f'{owner_base}report-extra/owners/{owner_safe}/'
            cards.append(
                f'<a class="member-card link" href="{href}">'
                f'<div class="member-avatar" style="background:{color}">{initials}</div>'
                f'<span class="member-name">{owner}</span>'
                f'<svg class="member-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 12h14M12 5l7 7-7 7"/></svg>'
                f'</a>'
            )
        else:
            cards.append(
                f'<div class="member-card muted">'
                f'<div class="member-avatar" style="background:{color};opacity:.4">{initials}</div>'
                f'<span class="member-name">{owner}</span>'
                f'</div>'
            )
    return ''.join(cards)


def build_suite_cards(report_url: str) -> str:
    suites = [
        ('Postman API Test', 'API integration tests', 'API', '📬', '#7dd3fc'),
        ('xUnit Backend Test', 'Unit & integration tests', 'BE', '🧪', '#a78bfa'),
        ('E2E Frontend Test', 'End-to-end UI tests', 'E2E', '🌐', '#34d399'),
    ]
    cards = []
    for title, subtitle, abbr, icon, color in suites:
        cards.append(
            f'<a class="suite-card" href="{report_url}">'
            f'<div class="suite-icon" style="background:linear-gradient(135deg, {color}22, {color}11);border-color:{color}33;color:{color}">'
            f'<span class="suite-emoji">{icon}</span>'
            f'</div>'
            f'<div class="suite-body">'
            f'<div class="suite-title">{title}</div>'
            f'<div class="suite-sub">{subtitle}</div>'
            f'</div>'
            f'<svg class="suite-arrow" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 12h14M12 5l7 7-7 7"/></svg>'
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
  <meta name="description" content="Quality Gate Report for Waste Recycling Platform — KCPM project test dashboard with Allure reports." />
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet" />
  <style>
    :root {{
      --bg: #0b0f1a;
      --surface: rgba(255, 255, 255, 0.04);
      --surface-hover: rgba(255, 255, 255, 0.07);
      --border: rgba(255, 255, 255, 0.08);
      --border-hover: rgba(125, 211, 252, 0.3);
      --text: #e8edf5;
      --text-secondary: #8896ab;
      --accent: #7dd3fc;
      --accent2: #a78bfa;
      --green: #34d399;
      --radius: 16px;
      --radius-sm: 12px;
    }}

    * {{ box-sizing: border-box; margin: 0; padding: 0; }}

    body {{
      font-family: 'Inter', system-ui, -apple-system, sans-serif;
      color: var(--text);
      background: var(--bg);
      min-height: 100vh;
      overflow-x: hidden;
    }}

    /* Ambient glow background */
    body::before {{
      content: '';
      position: fixed;
      top: -200px;
      left: -100px;
      width: 600px;
      height: 600px;
      background: radial-gradient(circle, rgba(125, 211, 252, 0.08) 0%, transparent 70%);
      pointer-events: none;
      z-index: 0;
    }}
    body::after {{
      content: '';
      position: fixed;
      top: -100px;
      right: -100px;
      width: 500px;
      height: 500px;
      background: radial-gradient(circle, rgba(167, 139, 250, 0.06) 0%, transparent 70%);
      pointer-events: none;
      z-index: 0;
    }}

    a {{ color: inherit; text-decoration: none; }}

    .container {{
      position: relative;
      z-index: 1;
      max-width: 960px;
      margin: 0 auto;
      padding: 48px 24px 64px;
    }}

    /* ── Header ── */
    .header {{
      margin-bottom: 40px;
    }}
    .header-top {{
      display: flex;
      align-items: center;
      gap: 14px;
      margin-bottom: 8px;
    }}
    .logo {{
      width: 44px;
      height: 44px;
      border-radius: var(--radius-sm);
      background: linear-gradient(135deg, var(--accent), var(--accent2));
      display: grid;
      place-items: center;
      font-weight: 800;
      font-size: 16px;
      color: var(--bg);
      flex-shrink: 0;
    }}
    h1 {{
      font-size: 28px;
      font-weight: 800;
      letter-spacing: -0.03em;
      line-height: 1.2;
    }}
    .subtitle {{
      color: var(--text-secondary);
      font-size: 14px;
      margin-top: 6px;
      margin-left: 58px;
    }}

    /* ── Badges ── */
    .badges {{
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-top: 20px;
    }}
    .badge {{
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 5px 12px;
      border-radius: 999px;
      font-size: 12px;
      border: 1px solid var(--border);
      background: var(--surface);
      transition: border-color .2s;
    }}
    .badge:hover {{ border-color: var(--border-hover); }}
    .badge-icon {{ font-size: 13px; }}
    .badge.ok {{ border-color: rgba(52, 211, 153, 0.25); }}
    .badge.ok strong {{ color: var(--green); }}
    .badge.warn {{ border-color: rgba(251, 191, 36, 0.25); }}
    .badge.warn strong {{ color: #fbbf24; }}
    .badge.info {{ border-color: rgba(125, 211, 252, 0.25); }}
    .badge.info strong {{ color: var(--accent); }}
    .badge strong {{ font-weight: 600; }}
    .badge em {{ font-style: normal; opacity: 0.6; }}

    /* ── Section titles ── */
    .section-label {{
      font-size: 11px;
      font-weight: 600;
      letter-spacing: 0.12em;
      text-transform: uppercase;
      color: var(--text-secondary);
      margin-bottom: 16px;
    }}

    /* ── Suite Cards ── */
    .suites {{
      margin-bottom: 32px;
    }}
    .suite-list {{
      display: flex;
      flex-direction: column;
      gap: 10px;
    }}
    .suite-card {{
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 16px 20px;
      border-radius: var(--radius);
      background: var(--surface);
      border: 1px solid var(--border);
      transition: all .2s ease;
      cursor: pointer;
    }}
    .suite-card:hover {{
      background: var(--surface-hover);
      border-color: var(--border-hover);
      transform: translateY(-1px);
      box-shadow: 0 8px 32px rgba(0,0,0,0.2);
    }}
    .suite-icon {{
      width: 44px;
      height: 44px;
      flex-shrink: 0;
      border-radius: var(--radius-sm);
      border: 1px solid;
      display: grid;
      place-items: center;
      font-size: 20px;
    }}
    .suite-body {{ flex: 1; min-width: 0; }}
    .suite-title {{
      font-size: 15px;
      font-weight: 700;
      letter-spacing: -0.01em;
    }}
    .suite-sub {{
      color: var(--text-secondary);
      font-size: 13px;
      margin-top: 2px;
    }}
    .suite-arrow {{
      width: 18px;
      height: 18px;
      color: var(--text-secondary);
      flex-shrink: 0;
      opacity: 0;
      transform: translateX(-4px);
      transition: all .2s;
    }}
    .suite-card:hover .suite-arrow {{
      opacity: 1;
      transform: translateX(0);
    }}

    /* ── CTA Button ── */
    .cta-wrap {{
      margin-bottom: 48px;
    }}
    .cta {{
      display: inline-flex;
      align-items: center;
      gap: 8px;
      padding: 12px 24px;
      border-radius: var(--radius-sm);
      background: linear-gradient(135deg, var(--accent), var(--accent2));
      color: var(--bg);
      font-weight: 700;
      font-size: 14px;
      transition: all .2s ease;
      box-shadow: 0 4px 20px rgba(125, 211, 252, 0.2);
    }}
    .cta:hover {{
      transform: translateY(-1px);
      box-shadow: 0 8px 30px rgba(125, 211, 252, 0.3);
    }}

    /* ── Team Members ── */
    .team {{
      margin-bottom: 48px;
    }}
    .member-grid {{
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 10px;
    }}
    .member-card {{
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      border-radius: var(--radius-sm);
      background: var(--surface);
      border: 1px solid var(--border);
      transition: all .2s ease;
    }}
    .member-card.link {{
      cursor: pointer;
    }}
    .member-card.link:hover {{
      background: var(--surface-hover);
      border-color: var(--border-hover);
      transform: translateY(-1px);
    }}
    .member-card.muted {{ opacity: 0.45; }}
    .member-avatar {{
      width: 34px;
      height: 34px;
      border-radius: 10px;
      display: grid;
      place-items: center;
      font-weight: 700;
      font-size: 12px;
      color: var(--bg);
      flex-shrink: 0;
    }}
    .member-name {{
      font-weight: 600;
      font-size: 14px;
      flex: 1;
      min-width: 0;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }}
    .member-arrow {{
      width: 16px;
      height: 16px;
      color: var(--text-secondary);
      flex-shrink: 0;
      opacity: 0;
      transition: opacity .2s;
    }}
    .member-card.link:hover .member-arrow {{ opacity: 1; }}

    /* ── Footer ── */
    footer {{
      color: var(--text-secondary);
      font-size: 12px;
      padding-top: 20px;
      border-top: 1px solid var(--border);
    }}
    footer a {{
      color: var(--accent);
      transition: opacity .2s;
    }}
    footer a:hover {{ opacity: 0.8; }}

    /* ── Responsive ── */
    @media (max-width: 640px) {{
      .container {{ padding: 32px 16px 48px; }}
      h1 {{ font-size: 24px; }}
      .subtitle {{ margin-left: 0; margin-top: 10px; }}
      .header-top {{ flex-wrap: wrap; }}
      .member-grid {{ grid-template-columns: 1fr; }}
    }}
  </style>
</head>
<body>
  <div class="container">
    <header class="header">
      <div class="header-top">
        <div class="logo">QA</div>
        <h1>KCPM Test Dashboard</h1>
      </div>
      <div class="subtitle">Waste Recycling Platform &mdash; Quality Gate Report &mdash; {generated_at}</div>
      {f'<div class="badges">{badges_html}</div>' if badges_html else ''}
    </header>

    <section class="suites">
      <div class="section-label">Test Suites</div>
      <div class="suite-list">
        {suite_cards_html}
      </div>
    </section>

    <div class="cta-wrap">
      <a class="cta" href="{report_url}">Open Full Allure Report &rarr;</a>
    </div>

    {f"""<section class="team">
      <div class="section-label">Team Members</div>
      <div class="member-grid">{owner_cards_html}</div>
    </section>""" if owners else ''}

    <footer>
      Auto-generated by CI &bull; <a href="{report_url}">View Allure Report</a>
    </footer>
  </div>
</body>
</html>'''

    index_path = site_output / 'index.html'
    index_path.write_text(html, encoding='utf-8')
    print(f'[build_site_index] Written {index_path}')


if __name__ == '__main__':
    main()
