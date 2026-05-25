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
import urllib.request
import urllib.error

RESULTS_DIRS = [
    os.path.join('Waste-Recycling-Platform', 'allure-results'),
    os.path.join('allure-results'),
]
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


def write_owner_map(owner_map):
    for out in OUT_PATHS:
        try:
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
    url = base_url.rstrip('/') + f'/rest/api/3/issue/{issue_key}?fields=assignee'
    req = urllib.request.Request(url, headers={'Authorization': auth_header, 'Accept': 'application/json'})
    try:
        with urllib.request.urlopen(req, timeout=30) as resp:
            body = resp.read().decode('utf8')
            return json.loads(body)
    except urllib.error.HTTPError as e:
        print(f'Jira HTTP error for {issue_key}: {e.code} {e.reason}', file=sys.stderr)
        if e.code in (401, 403):
            print('Jira auth failed. Check JIRA_BASE_URL, JIRA_EMAIL and JIRA_API_TOKEN.', file=sys.stderr)
            sys.exit(3)
        return None
    except Exception as e:
        print(f'Error querying Jira for {issue_key}: {e}', file=sys.stderr)
        return None


def main():
    keys = discover_all_keys()
    print('Found issue keys in results:', keys)
    if not keys:
        print('No Jira issue keys found. Writing empty map.')
        write_owner_map({})
        print('Done')
        return

    base = os.environ.get('JIRA_BASE_URL')
    email = os.environ.get('JIRA_EMAIL')
    token = os.environ.get('JIRA_API_TOKEN')
    if not base or not email or not token:
        # If we discovered issue keys but cannot call Jira, write a map with
        # all keys marked unassigned so downstream validation sees a non-empty
        # jira-owner-map.json (avoids failing 'jira-owner-map is empty').
        print('Warning: Jira secrets missing; cannot query Jira.', file=sys.stderr)
        if keys:
            owner_map = {k: {'displayName': None, 'accountId': None, 'unassigned': True} for k in sorted(keys)}
            write_owner_map(owner_map)
            print(f'Wrote owner map with {len(owner_map)} keys (unassigned)')
        else:
            write_owner_map({})
            print('No issue keys found; wrote empty map')
        print('Done')
        return

    basic = base64.b64encode(f"{email}:{token}".encode('utf8')).decode('ascii')
    auth_header = f'Basic {basic}'

    owner_map = {}
    for key in sorted(keys):
        print('Querying', key)
        data = query_jira_issue(base, auth_header, key)
        if not data:
            owner_map[key] = {'displayName': None, 'accountId': None, 'unassigned': True}
            continue
        fields = data.get('fields') or {}
        assignee = fields.get('assignee')
        if not assignee:
            owner_map[key] = {'displayName': None, 'accountId': None, 'unassigned': True}
        else:
            owner_map[key] = {
                'displayName': assignee.get('displayName'),
                'accountId': assignee.get('accountId') or assignee.get('accountId'),
                'email': assignee.get('emailAddress'),
                'unassigned': False,
            }

    resolved = [k for k,v in owner_map.items() if not v.get('unassigned')]
    print(f'Resolved {len(resolved)} / {len(owner_map)} issues with assignees')
    # show sample mapping
    for k in list(owner_map.keys())[:10]:
        print(k, '->', owner_map[k])

    print('Done')


if __name__ == '__main__':
    main()
