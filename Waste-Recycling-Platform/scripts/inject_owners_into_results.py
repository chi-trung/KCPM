#!/usr/bin/env python3
"""
Inject owner labels into existing Allure result JSONs in-place using jira-owner-map.json.

Usage: run from repository root; modifies files under Waste-Recycling-Platform/allure-results
"""
import json
import os
import re
import sys

RESULTS_DIR = os.path.join('Waste-Recycling-Platform', 'allure-results')
JIRA_MAP_PATHS = [
    os.path.join(RESULTS_DIR, 'jira-owner-map.json'),
    'jira-owner-map.json',
]


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


def extract_issue_keys_from_json(data):
    keys = set()
    for label in data.get('labels') or []:
        if isinstance(label, dict) and label.get('name') in ('issue', 'Issue', 'ISSUE') and label.get('value'):
            parts = str(label.get('value')).split('/')
            keys.add(parts[-1])
    for link in data.get('links') or []:
        if isinstance(link, dict) and link.get('type') == 'issue':
            name = link.get('name') or ''
            if name:
                keys.add(name)
    # also try to pull from testCaseId or name fields
    test_case = data.get('testCaseId') or data.get('name')
    if test_case:
        m = re.search(r'([A-Z]+-\d+)', str(test_case))
        if m:
            keys.add(m.group(1))
    return keys


def main():
    jira_map = load_jira_map()
    if not jira_map:
        print('Empty jira map; exiting')
        return

    modified = 0
    owners = set()
    for fname in os.listdir(RESULTS_DIR):
        if not fname.lower().endswith('.json'):
            continue
        path = os.path.join(RESULTS_DIR, fname)
        try:
            with open(path, 'r', encoding='utf8') as f:
                data = json.load(f)
        except Exception:
            continue

        issue_keys = extract_issue_keys_from_json(data)
        assigned = []
        for k in issue_keys:
            info = jira_map.get(k)
            if info and info.get('displayName'):
                assigned.append(info.get('displayName'))

        if not assigned:
            continue

        resolved_owner = next((a for a in sorted(set(a.strip() for a in assigned if a and a.strip()))), None)
        if not resolved_owner:
            continue

        # ensure labels list
        labels = data.get('labels') or []
        changed = False

        # Replace any pre-existing placeholder owner labels (auth/backend/qa)
        # with the real Jira assignee so Allure shows the person, not the suite.
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
            data['labels'] = labels
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
