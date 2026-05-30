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


def slugify(value: str) -> str:
    slug = ''.join(ch.lower() if ch.isalnum() else '-' for ch in value).strip('-')
    while '--' in slug:
        slug = slug.replace('--', '-')
    return slug or 'owner'


def build_owner_cards(owners: list[str], owner_base: str, published_root: Path) -> str:
    if not owners:
        return '<div class="empty">Chưa có owner nào được sync.</div>'

    cards = []
    for owner in owners:
        slug = slugify(owner)
        owner_html = f'{owner_base}owners/{slug}/'
        owner_pdf = f'{owner_base}owners/{slug}/report.pdf'
        owner_pdf_exists = (published_root / 'report-extra' / 'owners' / slug / 'report.pdf').exists()
        pdf_href = owner_pdf if owner_pdf_exists else owner_html
        pdf_label = 'Tải PDF' if owner_pdf_exists else 'Mở report'
        cards.append(
          f'<div class="owner-card">'
            f'<a class="owner-link" href="{owner_html}">'
              f'<span class="owner-name">{owner}</span>'
              f'<span class="owner-sub">Mở báo cáo theo owner</span>'
            f'</a>'
            f'<div class="owner-actions">'
              f'<a class="btn mini ghost" href="{pdf_href}">{pdf_label}</a>'
            f'</div>'
          f'</div>'
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
    report_pdf_url = f'{root_url}report-main/report.pdf'
    validation_url = f'{root_url}report-extra/validation/'
    owner_url = f'{root_url}report-extra/'
    owner_reports_root = site_output / 'report-extra' / 'owners'
    team_pdf_exists = (site_output / 'report-main' / 'report.pdf').exists()

    generated_at = datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')

    site_output.mkdir(parents=True, exist_ok=True)

    # Minimal landing page: show only owners list and a small header
    html = f'''<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>KCPM — Owners</title>
  <style>
    body {{ font-family: Inter, system-ui, -apple-system, "Segoe UI", Roboto, sans-serif; background: #07111f; color: #eaf1ff; margin: 0; }}
    .wrap {{ max-width: 980px; margin: 32px auto; padding: 20px; }}
    h1 {{ margin: 0 0 8px; font-size: 28px; }}
    .meta {{ color: #9db0ce; font-size: 13px; margin-bottom: 18px; }}
    .owner-grid {{ display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 12px; }}
    .owner-card {{ background: rgba(255,255,255,0.03); padding: 12px 14px; border-radius: 10px; border: 1px solid rgba(255,255,255,0.06); }}
    .owner-card a {{ color: inherit; text-decoration: none; display: block; }}
    .owner-name {{ font-weight: 700; }}
    .owner-sub {{ font-size: 13px; color: #9db0ce; margin-top: 6px; }}
    .footer {{ margin-top: 18px; color: #9db0ce; font-size: 13px; }}
  </style>
</head>
<body>
  <div class="wrap">
    <h1>Team owner reports</h1>
    <div class="meta">Generated {generated_at} — click a member to open their report</div>

    <div class="owner-grid">
      {build_owner_cards(owners, root_url, site_output)}
    </div>

    <div class="footer">Main report: <a href="{report_url}">{report_url}</a></div>
  </div>
</body>
</html>'''

    (site_output / 'index.html').write_text(html, encoding='utf8')
    print('Wrote landing page to', site_output / 'index.html')


if __name__ == '__main__':
    main()