#!/usr/bin/env python3
"""
Inject owner labels into existing Allure result JSONs in-place using jira-owner-map.json.

Usage: run from repository root; modifies files under Waste-Recycling-Platform/allure-results
"""
import json
import os
import re
import sys
from pathlib import Path

RESULTS_DIR = os.path.join('Waste-Recycling-Platform', 'allure-results')
JIRA_MAP_PATHS = [
    os.path.join(RESULTS_DIR, 'jira-owner-map.json'),
    'jira-owner-map.json',
]

LOCAL_MAP_PATHS = [
    os.path.join(RESULTS_DIR, 'local-owner-map.json'),
    os.path.join('Waste-Recycling-Platform', 'scripts', 'local-owner-map.json'),
    'local-owner-map.json',
]


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


def load_jira_map():
    for p in JIRA_MAP_PATHS:
        if os.path.exists(p):
            try:
                with open(p, 'r', encoding='utf8') as f:
                    data = json.load(f)
                    print('Loaded jira map from', p)
                    return data
            except Exception as e:
                print('Failed to load', p, e)
    print('No jira-owner-map.json found; nothing to inject')
    return {}


def load_local_map():
    for p in LOCAL_MAP_PATHS:
        if os.path.exists(p):
            try:
                with open(p, 'r', encoding='utf8') as f:
                    data = json.load(f)
                    print('Loaded local owner map from', p)
                    return data
            except Exception as e:
                print('Failed to load', p, e)
    return {}


def extract_issue_keys_from_json(data):
    keys = set()
    # data is expected to be a dict representing a single result entry
    if not isinstance(data, dict):
        return keys

    for label in data.get('labels') or []:
        if isinstance(label, dict) and label.get('name') in ('issue', 'Issue', 'ISSUE') and label.get('value'):
            parts = str(label.get('value')).split('/')
            keys.add(parts[-1])
    for link in data.get('links') or []:
        if isinstance(link, dict) and link.get('type') == 'issue':
            name = link.get('name') or ''
            if name:
                keys.add(name)
            else:
                url = link.get('url') or ''
                if isinstance(url, str):
                    keys.add(url.rstrip('/').split('/')[-1])
    # also try to pull from testCaseId or name fields
    test_case = data.get('testCaseId') or data.get('name')
    if test_case:
        m = re.search(r'([A-Z]+-\d+)', str(test_case))
        if m:
            keys.add(m.group(1))
    return keys


def main():
    jira_map = load_jira_map()
    local_map = load_local_map()

    if not jira_map and not local_map:
        print('Empty jira map and no local owner map; exiting')
        return

    modified = 0
    owners = set()
    aliases = load_owner_aliases()
    if aliases:
        print(f'Loaded {len(aliases)} owner aliases')

    for fname in os.listdir(RESULTS_DIR):
        if not fname.lower().endswith('.json'):
            continue
        path = os.path.join(RESULTS_DIR, fname)
        try:
            with open(path, 'r', encoding='utf8') as f:
                data = json.load(f)
        except Exception:
            continue
        # Support files where the root may be a dict (single entry) or a list of entries
        is_list = isinstance(data, list)
        if is_list:
            entries = [e for e in data if isinstance(e, dict)]
        elif isinstance(data, dict):
            entries = [data]
        else:
            continue

        file_changed = False
        for entry in entries:
            issue_keys = extract_issue_keys_from_json(entry)
            assigned = []
            for k in issue_keys:
                info = jira_map.get(k) if jira_map else None
                if info and info.get('displayName'):
                    assigned.append(info.get('displayName'))
                else:
                    # fallback to local map by issue key
                    lm = local_map.get(k) if local_map else None
                    if lm and isinstance(lm, dict) and lm.get('displayName'):
                        assigned.append(lm.get('displayName'))

            # if no assignee found by issue keys, try mapping from existing raw owner label
            if not assigned:
                labels_raw = [l.get('value') for l in (entry.get('labels') or []) if isinstance(l, dict) and l.get('name') == 'owner']
                for raw in labels_raw:
                    if not raw:
                        continue
                    # try exact match in local_map for owner label
                    lm = local_map.get(raw)
                    if lm and isinstance(lm, dict) and lm.get('displayName'):
                        assigned.append(lm.get('displayName'))
                        break

            if not assigned:
                continue

            resolved_owner = next((a for a in sorted(set(a.strip() for a in assigned if a and a.strip()))), None)
            resolved_owner = normalize_owner_name(resolved_owner, aliases)
            if not resolved_owner:
                continue

            # ensure labels list
            labels = entry.get('labels') or []
            if not isinstance(labels, list):
                labels = []
            changed = False

            # Replace any pre-existing placeholder owner labels with the real Jira assignee
            for label in labels:
                if isinstance(label, dict) and label.get('name') == 'owner':
                    if label.get('value') != resolved_owner:
                        label['value'] = resolved_owner
                        changed = True

            if not any(isinstance(l, dict) and l.get('name') == 'owner' for l in labels):
                labels.append({'name': 'owner', 'value': resolved_owner})
                changed = True

            owners.add(resolved_owner)

            if changed:
                entry['labels'] = labels
                file_changed = True

        if file_changed:
            try:
                with open(path, 'w', encoding='utf8') as f:
                    json.dump(data, f, ensure_ascii=False, indent=2)
                modified += 1
            except Exception as e:
                print('Failed to write', path, e)

    print(f'Injected owners into {modified} result files')
    print('Owners discovered:', owners)


if __name__ == '__main__':
    if not os.path.isdir(RESULTS_DIR):
        print('Results dir missing:', RESULTS_DIR)
        sys.exit(0)
    main()
