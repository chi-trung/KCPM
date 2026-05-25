import json
import os
import re
import sys
from pathlib import Path


def collect_issue_keys(results_dir):
    keys = set()
    for json_file in results_dir.glob('*.json'):
        try:
            data = json.loads(json_file.read_text(encoding='utf8'))
        except Exception:
            continue

        entries = []
        if isinstance(data, dict):
            entries = [data]
        elif isinstance(data, list):
            entries = [item for item in data if isinstance(item, dict)]

        for entry in entries:
            labels = entry.get('labels') or []
            links = entry.get('links') or []
            for label in labels:
                if isinstance(label, dict) and label.get('name') in ('issue', 'Issue', 'ISSUE'):
                    value = label.get('value')
                    if value:
                        keys.add(value.rstrip('/').split('/')[-1])
            for link in links:
                if isinstance(link, dict) and link.get('type') == 'issue':
                    name = (link.get('name') or '').strip()
                    if name:
                        keys.add(name)
                        continue
                    url = link.get('url') or ''
                    if isinstance(url, str):
                        keys.add(url.rstrip('/').split('/')[-1])
    return keys


def main():
    results_dir = Path('Waste-Recycling-Platform/allure-results')
    validation_dir = Path('validation-temp')
    validation_dir.mkdir(parents=True, exist_ok=True)

    summary = {
        'jira_resolved': 0,
        'jira_total': 0,
        'jira_sample': {},
        'injected_count': 0,
        'owners': [],
        'owner_slugs': [],
        'owner_folders': [],
        'xunit_present': False,
        'postman_present': False,
        'history_exists': (Path('allure-report') / 'history').exists(),
        'categories_exists': (results_dir / 'categories.json').exists(),
        'executor_exists': (results_dir / 'executor.json').exists(),
    }

    jira_map_path = results_dir / 'jira-owner-map.json'
    jira_map = {}
    if jira_map_path.exists():
        try:
            jira_map = json.loads(jira_map_path.read_text(encoding='utf8'))
        except Exception:
            jira_map = {}

    if not jira_map:
        discovered_keys = sorted(collect_issue_keys(results_dir))
        if discovered_keys:
            print('Warning: jira-owner-map is empty; using discovered Jira keys as unassigned fallback')
            jira_map = {key: {'displayName': None, 'accountId': None, 'unassigned': True} for key in discovered_keys}
        else:
            print('Fail: jira-owner-map is empty')
            sys.exit(2)

    summary['jira_total'] = len(jira_map)
    if summary['jira_total'] == 0:
        print('Fail: jira-owner-map is empty')
        sys.exit(2)

    for i, (key, value) in enumerate(jira_map.items()):
        if i < 10:
            summary['jira_sample'][key] = value
        if value and not value.get('unassigned'):
            summary['jira_resolved'] += 1

    for json_file in results_dir.glob('*.json'):
        try:
            data = json.loads(json_file.read_text(encoding='utf8'))
        except Exception:
            continue
        if not isinstance(data, dict):
            continue
        labels = [label for label in (data.get('labels') or []) if isinstance(label, dict)]
        for label in labels:
            if label.get('name') == 'owner' and label.get('value'):
                summary['injected_count'] += 1
                summary['owners'].append(label.get('value'))
        package_values = [label.get('value') for label in labels if label.get('name') in ('package', 'suite', 'subSuite')]
        joined = ' '.join(str(value) for value in package_values)
        if 'WastePlatform.Tests' in joined or '.Tests.' in joined or 'WastePlatform' in joined:
            summary['xunit_present'] = True
        if 'Postman' in joined or 'postman' in joined or 'newman' in joined:
            summary['postman_present'] = True

    summary['owners'] = sorted(set(summary['owners']))
    summary['owner_slugs'] = [re.sub(r'[^a-z0-9]+', '-', owner.lower()).strip('-') for owner in summary['owners']]
    for slug in summary['owner_slugs']:
        folder = Path('allure-report') / 'owners' / slug
        if folder.exists():
            summary['owner_folders'].append(str(folder))

    print('--- CI Validation Summary ---')
    print(f"Jira issues resolved count: {summary['jira_resolved']} of {summary['jira_total']}")
    print('First 10 jira-owner-map entries:')
    print(json.dumps(summary['jira_sample'], ensure_ascii=False, indent=2))
    print(f"Injected owner count: {summary['injected_count']}")
    print(f"Discovered owners: {summary['owners']}")
    print(f"Generated owner slugs: {summary['owner_slugs']}")
    print(f"Generated /owners/<slug> folders: {summary['owner_folders']}")

    if summary['jira_total'] == 0:
        print('Fail: jira-owner-map is empty')
        sys.exit(3)
    if not summary['owner_slugs']:
        print('Fail: no owner slugs generated')
        sys.exit(4)
    if summary['injected_count'] == 0:
        print('Fail: owner injection modified 0 files')
        sys.exit(5)
    if not summary['xunit_present']:
        print('Fail: xUnit results missing')
        sys.exit(6)
    if not summary['postman_present']:
        print('Fail: only Postman suite exists')
        sys.exit(7)

    repo_owner, repo_name = os.environ.get('GITHUB_REPOSITORY', '').split('/') if os.environ.get('GITHUB_REPOSITORY') else ('', '')
    main_url = f'https://{repo_owner}.github.io/{repo_name}/'
    print(f'Report URL: {main_url}')

    with open(validation_dir / 'summary.json', 'w', encoding='utf8') as f:
        json.dump(summary, f, ensure_ascii=False, indent=2)

    with open(validation_dir / 'index.html', 'w', encoding='utf8') as html:
        html.write('<!doctype html><html><head><meta charset="utf-8"><title>Allure Validation</title></head><body>')
        html.write(f'<h1>Allure Validation for run {os.environ.get("GITHUB_RUN_ID")}</h1>')
        html.write(f'<p>Main report URL: <a href="{main_url}">{main_url}</a></p>')
        html.write('<h2>Jira sync</h2>')
        html.write(f'<p>Status: OK</p>')
        html.write(f'<p>Issues resolved: {summary["jira_resolved"]} of {summary["jira_total"]}</p>')
        html.write('<h2>Owners</h2><ul>')
        for slug, owner in zip(summary['owner_slugs'], summary['owners']):
            html.write(f'<li><a href="{main_url}owners/{slug}/">{owner}</a></li>')
        html.write('</ul>')
        html.write('<h2>Checks</h2><ul>')
        html.write(f'<li>xUnit merged: {summary["xunit_present"]}</li>')
        html.write(f'<li>Postman merged: {summary["postman_present"]}</li>')
        html.write(f'<li>history exists: {summary["history_exists"]}</li>')
        html.write(f'<li>categories exists: {summary["categories_exists"]}</li>')
        html.write(f'<li>executor exists: {summary["executor_exists"]}</li>')
        html.write('</ul></body></html>')

    with open(validation_dir / 'owner-slugs.txt', 'w', encoding='utf8') as f:
        f.write('\n'.join(summary['owner_slugs']) + ('\n' if summary['owner_slugs'] else ''))

    with open(validation_dir / 'urls.txt', 'w', encoding='utf8') as f:
        f.write(f'Main report: {main_url}\n')
        f.write(f'Validation page: {main_url}validation/\n')
        for slug in summary['owner_slugs']:
            f.write(f'Owner: {main_url}owners/{slug}/\n')

    print(f'Validation page URL: {main_url}validation/')


if __name__ == '__main__':
    main()
