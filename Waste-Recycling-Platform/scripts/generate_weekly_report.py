#!/usr/bin/env python3
"""
generate_weekly_report.py
─────────────────────────
Generate a professional weekly HTML test report from CI test results.
Output: site-output/reports/weekly/index.html (+ week-N.html archive)

Reads:
  - TRX files (backend xUnit results)
  - Postman JSON report
  - jira-owner-map.json
  - Environment variables: GITHUB_*, ALLURE_REPORT_URL, etc.
"""

import json
import os
import re
import sys
from datetime import datetime, timezone, timedelta
from pathlib import Path


def find_trx():
    """Find the latest .trx file."""
    for root, dirs, files in os.walk('TestResults'):
        for f in files:
            if f.endswith('.trx'):
                return os.path.join(root, f)
    for root, dirs, files in os.walk('.'):
        for f in files:
            if f.endswith('.trx'):
                return os.path.join(root, f)
    return None


def parse_trx(trx_path):
    """Parse TRX for pass/fail/total/duration."""
    import xml.etree.ElementTree as ET
    result = {'passed': 0, 'failed': 0, 'total': 0, 'duration': 'N/A'}
    if not trx_path or not os.path.exists(trx_path):
        return result
    try:
        tree = ET.parse(trx_path)
        root = tree.getroot()
        ns = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
        counters = root.find('.//t:ResultSummary/t:Counters', ns)
        if counters is not None:
            result['passed'] = int(counters.get('passed', 0))
            result['failed'] = int(counters.get('failed', 0))
            result['total'] = int(counters.get('total', 0))
        times = root.find('.//t:Times', ns)
        if times is not None:
            start = times.get('start', '')
            finish = times.get('finish', '')
            if start and finish:
                try:
                    s = datetime.fromisoformat(start.replace('Z', '+00:00'))
                    f = datetime.fromisoformat(finish.replace('Z', '+00:00'))
                    sec = int((f - s).total_seconds())
                    m, s = divmod(sec, 60)
                    result['duration'] = f'{m}m {s}s' if m else f'{s}s'
                except Exception:
                    pass
    except Exception as e:
        print(f'Warning: Failed to parse TRX: {e}')
    return result


def parse_postman_report():
    """Parse postman-report.json for pass/fail."""
    result = {'passed': 0, 'failed': 0, 'total': 0, 'duration': 'N/A'}
    candidates = [
        'TestResults/postman-report.json',
        'postman-report.json',
    ]
    for path in candidates:
        if os.path.exists(path):
            try:
                with open(path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                run = data.get('run', {})
                stats = run.get('stats', {}).get('assertions', {})
                result['total'] = stats.get('total', 0)
                result['failed'] = stats.get('failed', 0)
                result['passed'] = result['total'] - result['failed']
                timings = run.get('timings', {})
                if timings.get('completed'):
                    ms = timings.get('completed', 0) - timings.get('started', 0)
                    sec = int(ms / 1000)
                    m, s = divmod(sec, 60)
                    result['duration'] = f'{m}m {s}s' if m else f'{s}s'
            except Exception as e:
                print(f'Warning: Failed to parse Postman report: {e}')
            break
    return result


def load_owners():
    """Load owners from jira-owner-map or validation summary."""
    owners = []
    # Try validation summary first
    for path in ['report-extra/validation/summary.json', 'validation-temp/summary.json']:
        if os.path.exists(path):
            try:
                with open(path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                owners = data.get('owners', [])
                if owners:
                    return owners
            except Exception:
                pass
    # Fallback: jira-owner-map (multiple possible locations)
    for path in [
        'jira-owner-map.json',
        'Waste-Recycling-Platform/allure-results/jira-owner-map.json',
        'allure-results/jira-owner-map.json',
    ]:
        if os.path.exists(path):
            try:
                with open(path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                seen = set()
                for k, v in data.items():
                    name = v.get('displayName')
                    if name and name not in seen:
                        owners.append(name)
                        seen.add(name)
                if owners:
                    return sorted(owners)
            except Exception:
                pass
    # Final fallback: hardcoded known team members
    if not owners:
        owners = [
            'Đăng',
            'Minh Phụng',
            'Nguyễn Chí Trung',
            'Nguyễn Hoàng Phụng-CNTT',
            'Thanh Duy',
        ]
    return owners


def slugify(value):
    if not value:
        return 'unassigned'
    s = value.lower()
    s = re.sub(r'[^a-z0-9]+', '-', s)
    s = s.strip('-')
    return s or 'owner'


def pass_rate(passed, total):
    if total == 0:
        return 'N/A'
    return f'{round(passed / total * 100, 1)}%'


def status_badge(passed, failed, total):
    if failed == 0 and total > 0:
        return '<span class="badge badge-pass">✅ ALL PASSED</span>'
    if failed > 0:
        return f'<span class="badge badge-fail">❌ {failed} FAILED</span>'
    return '<span class="badge badge-warn">⚠️ NO DATA</span>'


def generate_html(backend, postman, owners, now):
    """Generate the weekly report HTML."""
    week_num = now.isocalendar()[1]
    year = now.year
    date_str = now.strftime('%Y-%m-%d %H:%M UTC')
    
    repo = os.environ.get('GITHUB_REPOSITORY', 'chi-trung/KCPM')
    run_id = os.environ.get('GITHUB_RUN_ID', '0')
    run_number = os.environ.get('GITHUB_RUN_NUMBER', '0')
    branch = os.environ.get('GITHUB_REF_NAME', 'main')
    sha = os.environ.get('GITHUB_SHA', '')[:7]
    allure_url = os.environ.get('ALLURE_REPORT_URL', f'https://chi-trung.github.io/KCPM/report-main/')
    gh_run_url = f'https://github.com/{repo}/actions/runs/{run_id}'
    
    # Overall stats
    total_tests = backend['total'] + postman['total']
    total_passed = backend['passed'] + postman['passed']
    total_failed = backend['failed'] + postman['failed']
    overall_rate = pass_rate(total_passed, total_tests)
    
    # Owner rows
    owner_rows = ''
    colors = ['#7dd3fc', '#a78bfa', '#34d399', '#fb923c', '#f472b6', '#facc15']
    for i, owner in enumerate(owners):
        slug = slugify(owner)
        color = colors[i % len(colors)]
        initials = ''.join([w[0].upper() for w in owner.replace('_', ' ').split() if w][:2])
        owner_url = f'https://chi-trung.github.io/KCPM/report-extra/owners/{slug}/'
        owner_rows += f'''
        <tr>
          <td>
            <span class="avatar" style="background:{color}">{initials}</span>
            <a href="{owner_url}">{owner}</a>
          </td>
          <td><a href="{owner_url}" class="link">View Report →</a></td>
        </tr>'''

    html = f'''<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>KCPM Weekly Report — Week {week_num}, {year}</title>
  <meta name="description" content="Weekly test execution report for Waste Recycling Platform — Week {week_num}, {year}" />
  <link rel="preconnect" href="https://fonts.googleapis.com" />
  <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin />
  <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&display=swap" rel="stylesheet" />
  <style>
    :root {{
      --bg: #0b0f1a;
      --surface: rgba(255,255,255,0.04);
      --surface-hover: rgba(255,255,255,0.07);
      --border: rgba(255,255,255,0.08);
      --text: #e8edf5;
      --text-secondary: #8896ab;
      --accent: #7dd3fc;
      --accent2: #a78bfa;
      --green: #34d399;
      --red: #f87171;
      --radius: 16px;
      --radius-sm: 12px;
    }}
    * {{ box-sizing: border-box; margin: 0; padding: 0; }}
    body {{
      font-family: 'Inter', system-ui, -apple-system, sans-serif;
      color: var(--text);
      background: var(--bg);
      min-height: 100vh;
    }}
    body::before {{
      content: '';
      position: fixed;
      top: -200px; left: -100px;
      width: 600px; height: 600px;
      background: radial-gradient(circle, rgba(125,211,252,0.08) 0%, transparent 70%);
      pointer-events: none;
    }}
    a {{ color: var(--accent); text-decoration: none; }}
    a:hover {{ text-decoration: underline; }}
    .container {{
      position: relative; z-index: 1;
      max-width: 900px; margin: 0 auto;
      padding: 48px 24px 64px;
    }}
    .header {{ margin-bottom: 40px; }}
    .header-top {{ display: flex; align-items: center; gap: 14px; margin-bottom: 8px; }}
    .logo {{
      width: 44px; height: 44px;
      border-radius: var(--radius-sm);
      background: linear-gradient(135deg, var(--accent), var(--accent2));
      display: grid; place-items: center;
      font-weight: 800; font-size: 15px; color: var(--bg);
      flex-shrink: 0;
    }}
    h1 {{ font-size: 26px; font-weight: 800; letter-spacing: -0.03em; }}
    .subtitle {{ color: var(--text-secondary); font-size: 14px; margin-top: 6px; margin-left: 58px; }}
    .section-title {{
      text-transform: uppercase; letter-spacing: 0.08em;
      font-size: 12px; font-weight: 600;
      color: var(--text-secondary);
      margin: 32px 0 16px;
    }}
    .card {{
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius);
      padding: 24px;
      margin-bottom: 16px;
    }}
    .stat-grid {{
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 12px;
    }}
    .stat-item {{
      background: var(--surface);
      border: 1px solid var(--border);
      border-radius: var(--radius-sm);
      padding: 16px;
      text-align: center;
    }}
    .stat-value {{
      font-size: 28px; font-weight: 800;
      letter-spacing: -0.02em;
      margin-bottom: 4px;
    }}
    .stat-label {{
      font-size: 12px; color: var(--text-secondary);
      text-transform: uppercase; letter-spacing: 0.05em;
    }}
    .stat-value.green {{ color: var(--green); }}
    .stat-value.red {{ color: var(--red); }}
    .stat-value.accent {{ color: var(--accent); }}
    table {{
      width: 100%;
      border-collapse: collapse;
      font-size: 14px;
    }}
    th, td {{
      padding: 10px 14px;
      text-align: left;
      border-bottom: 1px solid var(--border);
    }}
    th {{
      color: var(--text-secondary);
      font-weight: 600; font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }}
    .badge {{
      display: inline-block;
      padding: 4px 12px;
      border-radius: 999px;
      font-size: 13px; font-weight: 600;
    }}
    .badge-pass {{ background: rgba(52,211,153,0.15); color: var(--green); }}
    .badge-fail {{ background: rgba(248,113,113,0.15); color: var(--red); }}
    .badge-warn {{ background: rgba(251,191,36,0.15); color: #fbbf24; }}
    .avatar {{
      display: inline-flex;
      align-items: center; justify-content: center;
      width: 28px; height: 28px;
      border-radius: 8px;
      font-size: 11px; font-weight: 700;
      color: var(--bg);
      margin-right: 8px;
      vertical-align: middle;
    }}
    .link {{ color: var(--accent); font-weight: 500; }}
    .meta {{
      margin-top: 32px;
      padding-top: 16px;
      border-top: 1px solid var(--border);
      color: var(--text-secondary);
      font-size: 12px;
    }}
    .meta a {{ color: var(--accent); }}
    .back-link {{
      display: inline-flex; align-items: center; gap: 6px;
      color: var(--text-secondary); font-size: 13px;
      margin-bottom: 24px;
    }}
    .back-link:hover {{ color: var(--accent); text-decoration: none; }}
    @media (max-width: 640px) {{
      .stat-grid {{ grid-template-columns: repeat(2, 1fr); }}
      h1 {{ font-size: 22px; }}
    }}
  </style>
</head>
<body>
<div class="container">

  <a href="../../" class="back-link">← Back to Dashboard</a>

  <div class="header">
    <div class="header-top">
      <div class="logo">W{week_num}</div>
      <h1>Weekly Test Report</h1>
    </div>
    <div class="subtitle">
      Waste Recycling Platform — Week {week_num}, {year} — {date_str}
    </div>
  </div>

  <div class="section-title">Executive Summary</div>
  <div class="stat-grid">
    <div class="stat-item">
      <div class="stat-value accent">{total_tests}</div>
      <div class="stat-label">Total Tests</div>
    </div>
    <div class="stat-item">
      <div class="stat-value green">{total_passed}</div>
      <div class="stat-label">Passed</div>
    </div>
    <div class="stat-item">
      <div class="stat-value {'red' if total_failed > 0 else 'green'}">{total_failed}</div>
      <div class="stat-label">Failed</div>
    </div>
    <div class="stat-item">
      <div class="stat-value accent">{overall_rate}</div>
      <div class="stat-label">Pass Rate</div>
    </div>
  </div>

  <div class="section-title">Backend Tests (xUnit .NET)</div>
  <div class="card">
    <table>
      <tr><th>Metric</th><th>Value</th></tr>
      <tr><td>Status</td><td>{status_badge(backend['passed'], backend['failed'], backend['total'])}</td></tr>
      <tr><td>Tests</td><td>{backend['passed']} passed / {backend['failed']} failed / {backend['total']} total</td></tr>
      <tr><td>Pass Rate</td><td>{pass_rate(backend['passed'], backend['total'])}</td></tr>
      <tr><td>Duration</td><td>{backend['duration']}</td></tr>
    </table>
  </div>

  <div class="section-title">Postman API Tests (Newman)</div>
  <div class="card">
    <table>
      <tr><th>Metric</th><th>Value</th></tr>
      <tr><td>Status</td><td>{status_badge(postman['passed'], postman['failed'], postman['total'])}</td></tr>
      <tr><td>Assertions</td><td>{postman['passed']} passed / {postman['failed']} failed / {postman['total']} total</td></tr>
      <tr><td>Pass Rate</td><td>{pass_rate(postman['passed'], postman['total'])}</td></tr>
      <tr><td>Duration</td><td>{postman['duration']}</td></tr>
    </table>
  </div>

  <div class="section-title">Team Members</div>
  <div class="card">
    <table>
      <tr><th>Member</th><th>Individual Report</th></tr>
      {owner_rows if owner_rows else '<tr><td colspan="2">No owners synced yet</td></tr>'}
    </table>
  </div>

  <div class="section-title">Links</div>
  <div class="card">
    <table>
      <tr><td>📊 Full Allure Report</td><td><a href="{allure_url}">View Report →</a></td></tr>
      <tr><td>🔧 GitHub Actions Run</td><td><a href="{gh_run_url}">Run #{run_number} →</a></td></tr>
      <tr><td>🎯 Jira Board</td><td><a href="Jira Board">KCPM Board →</a></td></tr>
      <tr><td>🏠 Dashboard</td><td><a href="https://chi-trung.github.io/KCPM/">KCPM Dashboard →</a></td></tr>
    </table>
  </div>

  <div class="meta">
    <p>Auto-generated by CI • <a href="{gh_run_url}">Run #{run_number}</a> •
       Branch: {branch} • Commit: {sha}</p>
  </div>

</div>
</body>
</html>'''
    return html, week_num, year


def main():
    now = datetime.now(timezone.utc)
    print(f'Generating weekly report for week {now.isocalendar()[1]}, {now.year}')

    # Parse results
    trx_path = find_trx()
    print(f'TRX file: {trx_path}')
    backend = parse_trx(trx_path)
    postman = parse_postman_report()
    owners = load_owners()

    print(f'Backend: {backend}')
    print(f'Postman: {postman}')
    print(f'Owners: {owners}')

    # Generate HTML
    html, week_num, year = generate_html(backend, postman, owners, now)

    # Write output
    out_dir = os.environ.get('WEEKLY_OUTPUT_DIR', 'site-output/reports/weekly')
    os.makedirs(out_dir, exist_ok=True)

    # Write index.html (latest)
    index_path = os.path.join(out_dir, 'index.html')
    with open(index_path, 'w', encoding='utf-8') as f:
        f.write(html)
    print(f'Wrote {index_path}')

    # Write archived copy
    archive_path = os.path.join(out_dir, f'week-{week_num}-{year}.html')
    with open(archive_path, 'w', encoding='utf-8') as f:
        f.write(html)
    print(f'Wrote {archive_path}')

    # Write summary JSON for trend tracking
    summary = {
        'week': week_num,
        'year': year,
        'timestamp': now.isoformat(),
        'backend': backend,
        'postman': postman,
        'owners': owners,
        'total_tests': backend['total'] + postman['total'],
        'total_passed': backend['passed'] + postman['passed'],
        'total_failed': backend['failed'] + postman['failed'],
    }
    summary_path = os.path.join(out_dir, 'latest.json')
    with open(summary_path, 'w', encoding='utf-8') as f:
        json.dump(summary, f, ensure_ascii=False, indent=2)
    print(f'Wrote {summary_path}')

    print('Weekly report generation complete')


if __name__ == '__main__':
    main()
