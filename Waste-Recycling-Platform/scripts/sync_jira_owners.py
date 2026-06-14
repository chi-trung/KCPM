#!/usr/bin/env python3
"""
Query Jira for assignees of issues referenced in Allure result JSONs and emit jira-owner-map.json.

Writes both to the results directory and to the repo root as `jira-owner-map.json`.

Usage: run from repository root where `Waste-Recycling-Platform/allure-results` exists.
Requires environment variables: JIRA_BASE_URL, JIRA_EMAIL, JIRA_API_TOKEN
"""
import base64
import json
import os
import sys
from pathlib import Path
from urllib.parse import urlparse
import urllib.request
import urllib.error


def load_owner_aliases():
    """Load owner-aliases.json for deduplicating display names."""
    alias_path = Path(__file__).parent / 'owner-aliases.json'
    if alias_path.exists():
        try:
            return json.loads(alias_path.read_text(encoding='utf-8'))
        except Exception:
            pass
    return {}


def normalize_owner_name(name, aliases=None):
    """Map variant display names to canonical name using alias config."""
    if not name:
        return name
    if aliases is None:
        aliases = {}
    return aliases.get(name, aliases.get(name.strip(), name))

RESULTS_DIRS = [
    os.path.join('Waste-Recycling-Platform', 'allure-results'),
    os.path.join('allure-results'),
]
DEFAULT_BOARD_ID = os.environ.get('JIRA_BOARD_ID', '3')
# Write owner map to whichever results folder exists and to repo root
OUT_PATHS = [
    os.path.join('Waste-Recycling-Platform', 'allure-results', 'jira-owner-map.json'),
    os.path.join('allure-results', 'jira-owner-map.json'),
    'jira-owner-map.json',
]


def collect_issue_keys(results_dir):
    keys = set()
    if not os.path.isdir(results_dir):
        return keys
    for fname in os.listdir(results_dir):
        if not fname.lower().endswith('.json'):
            continue
        if fname in ('categories.json', 'executor.json', 'environment.properties'):
            continue
        path = os.path.join(results_dir, fname)
        try:
            with open(path, 'r', encoding='utf8') as f:
                data = json.load(f)
        except Exception:
            continue

        # Support files where the root is a list of items (Allure sometimes
        # emits arrays). Normalize to a list of dicts to scan uniformly.
        entries = []
        if isinstance(data, dict):
            entries = [data]
        elif isinstance(data, list):
            entries = [e for e in data if isinstance(e, dict)]
        else:
            continue

        for data in entries:
            if not (data.get('labels') or data.get('name') or data.get('fullName') or data.get('links')):
                continue

            labels = data.get('labels') or []
            links = data.get('links') or []

            # labels like {name: 'issue', value: 'KIEM-4'}
            for label in labels:
                if isinstance(label, dict) and label.get('name') in ('issue', 'Issue', 'ISSUE'):
                    val = label.get('value')
                    if val:
                        # extract key like KIEM-123 from full url or value
                        parts = val.strip().split('/')
                        key = parts[-1] if parts else val.strip()
                        keys.add(key)
            # links with type issue
            for link in links:
                if isinstance(link, dict) and link.get('type') == 'issue':
                    name = (link.get('name') or '').strip()
                    if name:
                        keys.add(name)
                    else:
                        # try extracting key from url if name missing
                        url = link.get('url') or ''
                        if isinstance(url, str) and '/browse/' in url:
                            parts = url.rstrip('/').split('/')
                            candidate = parts[-1]
                            if candidate:
                                keys.add(candidate.strip())
                        else:
                            # fallback: last path segment
                            try:
                                parts = (link.get('url') or '').rstrip('/').split('/')
                                if parts:
                                    keys.add(parts[-1].strip())
                            except Exception:
                                pass
    return keys


def discover_all_keys():
    keys = set()
    for d in RESULTS_DIRS:
        keys.update(collect_issue_keys(d))
    return keys


def normalize_jira_base_url(base_url):
    parsed = urlparse(base_url)
    if parsed.scheme and parsed.netloc:
        return f'{parsed.scheme}://{parsed.netloc}'
    return base_url.rstrip('/')


def load_local_owner_map():
    paths = [
        os.path.join('Waste-Recycling-Platform', 'allure-results', 'local-owner-map.json'),
        os.path.join('local-owner-map.json'),
    ]
    for path in paths:
        if not os.path.exists(path):
            continue
        try:
            with open(path, 'r', encoding='utf8') as f:
                data = json.load(f)
            print('Loaded local owner map from', path)
            return data
        except Exception as e:
            print('Failed to load local owner map from', path, e, file=sys.stderr)
    return {}


def write_owner_map(owner_map):
    for out in OUT_PATHS:
        try:
            # ensure parent dir exists
            parent = os.path.dirname(out)
            if parent and not os.path.isdir(parent):
                try:
                    os.makedirs(parent, exist_ok=True)
                except Exception:
                    pass
            with open(out, 'w', encoding='utf8') as f:
                json.dump(owner_map, f, ensure_ascii=False, indent=2)
            print('Wrote', out)
        except Exception as e:
            print('Failed to write', out, e, file=sys.stderr)


def get_env_var(name):
    v = os.environ.get(name)
    if not v:
        print(f'Environment variable {name} is required', file=sys.stderr)
        sys.exit(2)
    return v


def query_jira_issue(base_url, auth_header, issue_key):
    normalized_base = normalize_jira_base_url(base_url)
    # Try multiple endpoints/approaches to tolerate API differences and permission reveals
    candidates = [
        normalized_base.rstrip('/') + f'/rest/api/3/issue/{issue_key}?fields=assignee',
        normalized_base.rstrip('/') + f'/rest/api/2/issue/{issue_key}?fields=assignee',
        normalized_base.rstrip('/') + f'/rest/api/3/search?jql=key%3D{issue_key}&fields=assignee',
        normalized_base.rstrip('/') + f'/rest/api/2/search?jql=key%3D{issue_key}&fields=assignee',
    ]

    for url in candidates:
        try:
            print('Request URL:', url)
            req = urllib.request.Request(url, headers={'Authorization': auth_header, 'Accept': 'application/json'})
            with urllib.request.urlopen(req, timeout=30) as resp:
                body = resp.read().decode('utf8')
                try:
                    data = json.loads(body)
                except Exception:
                    data = None
                # If search API returned issues array, normalize to first issue
                if data and 'issues' in data and isinstance(data.get('issues'), list) and data['issues']:
                    return data['issues'][0]
                return data
        except urllib.error.HTTPError as e:
            # Try to read response body for debugging; some Jira instances return JSON error details
            try:
                body = e.read().decode('utf8')
            except Exception:
                body = ''
            print(f'Jira HTTP error for {issue_key}: {e.code} {e.reason} - {body}', file=sys.stderr)
            if e.code in (401, 403):
                print('Jira auth failed. Check JIRA_BASE_URL, JIRA_EMAIL and JIRA_API_TOKEN.', file=sys.stderr)
                sys.exit(3)
            # on 404 or other errors, continue trying other candidate endpoints
            continue
        except Exception as e:
            print(f'Error querying Jira for {issue_key}: {e}', file=sys.stderr)
            continue
    return None


def fetch_board_issues(base_url, auth_header, board_id):
    normalized_base = normalize_jira_base_url(base_url)
    board_issues = []
    start_at = 0
    max_results = 1000

    while True:
        url = normalized_base.rstrip('/') + (
            f'/rest/agile/1.0/board/{board_id}/issue?startAt={start_at}'
            f'&maxResults={max_results}&fields=assignee'
        )
        print('Request URL:', url)
        req = urllib.request.Request(url, headers={'Authorization': auth_header, 'Accept': 'application/json'})
        try:
            with urllib.request.urlopen(req, timeout=30) as resp:
                data = json.loads(resp.read().decode('utf8'))
        except urllib.error.HTTPError as e:
            try:
                body = e.read().decode('utf8')
            except Exception:
                body = ''
            print(f'Jira board HTTP error: {e.code} {e.reason} - {body}', file=sys.stderr)
            return []
        except Exception as e:
            print(f'Error querying Jira board {board_id}: {e}', file=sys.stderr)
            return []

        issues = data.get('issues') or []
        if not isinstance(issues, list):
            issues = []
        board_issues.extend([issue for issue in issues if isinstance(issue, dict)])

        total = int(data.get('total') or len(board_issues))
        if start_at + len(issues) >= total or not issues:
            break
        start_at += len(issues)

    return board_issues


def build_owner_entry(assignee, aliases=None):
    if not assignee:
        return {'displayName': None, 'accountId': None, 'email': None, 'unassigned': True}
    display_name = normalize_owner_name(assignee.get('displayName'), aliases)
    return {
        'displayName': display_name,
        'accountId': assignee.get('accountId'),
        'email': assignee.get('emailAddress'),
        'unassigned': not bool(display_name),
    }
    


def main():
    aliases = load_owner_aliases()
    if aliases:
        print(f'Loaded {len(aliases)} owner aliases')

    keys = discover_all_keys()
    print('Found issue keys in results:', keys)
    if not keys:
        print('No Jira issue keys found. Writing empty map.')
        write_owner_map({})
        print('Done')
        return

    local_map = load_local_owner_map()

    base = os.environ.get('JIRA_BASE_URL')
    email = os.environ.get('JIRA_EMAIL')
    token = os.environ.get('JIRA_API_TOKEN')
    if not base or not email or not token:
        # If Jira secrets are missing, still use the local owner map so the
        # report can show the intended assignee names instead of unassigned.
        print('Warning: Jira secrets missing; using local owner map fallback.', file=sys.stderr)
        owner_map = {}
        for key in sorted(keys):
            fallback = local_map.get(key) if isinstance(local_map, dict) else None
            owner_map[key] = build_owner_entry(fallback, aliases)
        for alias, entry in (local_map or {}).items():
            if alias not in owner_map and isinstance(entry, dict):
                owner_map[alias] = build_owner_entry(entry)
        write_owner_map(owner_map)
        print(f'Wrote owner map with {len(owner_map)} keys from local fallback')
        print('Done')
        return

    normalized_base = normalize_jira_base_url(base)
    if normalized_base != base.rstrip('/'):
        print('Normalized JIRA_BASE_URL to:', normalized_base)

    basic = base64.b64encode(f"{email}:{token}".encode('utf8')).decode('ascii')
    auth_header = f'Basic {basic}'

    owner_map = {}

    board_issues = fetch_board_issues(normalized_base, auth_header, DEFAULT_BOARD_ID)
    for issue in board_issues:
        key = issue.get('key')
        if not key or key not in keys:
            continue
        fields = issue.get('fields') or {}
        owner_map[key] = build_owner_entry(fields.get('assignee'), aliases)

    for key in sorted(keys):
        if key in owner_map and not owner_map[key].get('unassigned'):
            continue
        print('Querying', key)
        data = query_jira_issue(normalized_base, auth_header, key)
        if not data:
            fallback = local_map.get(key) if isinstance(local_map, dict) else None
            owner_map[key] = build_owner_entry(fallback, aliases) if fallback else build_owner_entry(None, aliases)
            continue
        fields = data.get('fields') or {}
        assignee = fields.get('assignee')
        if not assignee:
            fallback = local_map.get(key) if isinstance(local_map, dict) else None
            owner_map[key] = build_owner_entry(fallback, aliases) if fallback else build_owner_entry(None, aliases)
        else:
            owner_map[key] = build_owner_entry(assignee, aliases)

    # Backfill any missing local aliases so manual labels like `auth` can still resolve.
    if isinstance(local_map, dict):
        for alias, entry in local_map.items():
            if alias not in owner_map and isinstance(entry, dict):
                owner_map[alias] = {
                    'displayName': entry.get('displayName'),
                    'accountId': entry.get('accountId'),
                    'email': entry.get('email'),
                    'unassigned': not bool(entry.get('displayName')),
                }

    resolved = [k for k,v in owner_map.items() if not v.get('unassigned')]
    print(f'Resolved {len(resolved)} / {len(owner_map)} issues with assignees')
    # show sample mapping
    for k in list(owner_map.keys())[:10]:
        print(k, '->', owner_map[k])
    # persist the resolved owner map so downstream steps can inject owners
    try:
        write_owner_map(owner_map)
    except Exception:
        print('Warning: failed to write owner map files', file=sys.stderr)

    print('Done')


if __name__ == '__main__':
    main()
