#!/usr/bin/env python3
"""
Generate per-owner Allure reports by filtering result JSON files that contain a label 'owner'.
Writes reports into 'allure-report/owners/<owner>/' so they are published with the main report.

Usage: run from repository root (where `Waste-Recycling-Platform/allure-results` exists) and
have `allure` CLI available in PATH.
"""
import json
import os
import re
import shutil
import subprocess

RESULTS_DIR = os.path.join('Waste-Recycling-Platform', 'allure-results')
BASE_OUT = 'owner-report-temp'
OUTPUT_BASE = os.path.join('allure-report', 'owners')

# optional jira owner map produced by sync_jira_owners.py
JIRA_MAP_PATHS = [
    os.path.join(RESULTS_DIR, 'jira-owner-map.json'),
    'jira-owner-map.json',
]

if not os.path.isdir(RESULTS_DIR):
    print('No results dir found at', RESULTS_DIR)
    raise SystemExit(0)

owners = set()

# load jira-owner-map if present
jira_map = {}
for p in JIRA_MAP_PATHS:
    if os.path.exists(p):
        try:
            with open(p, 'r', encoding='utf8') as f:
                jira_map = json.load(f)
            print('Loaded jira owner map from', p)
            break
        except Exception:
            jira_map = {}

if not jira_map:
    print('Skipping owner reports: jira-owner-map.json empty')
    raise SystemExit(0)


def slugify(name: str) -> str:
    if not name:
        return 'unassigned'
    s = name.lower()
    s = re.sub(r"[^a-z0-9]+", '-', s)
    s = s.strip('-')
    return s or 'owner'


def is_test_result(data):
    if not isinstance(data, dict):
        return False
    if 'labels' not in data:
        return False
    if not (data.get('name') or data.get('fullName') or data.get('links')):
        return False
    return True


shutil.rmtree(BASE_OUT, ignore_errors=True)
os.makedirs(BASE_OUT, exist_ok=True)


# Collect owners from JSON result files (owner label OR via jira map -> issues)
for fname in os.listdir(RESULTS_DIR):
    path = os.path.join(RESULTS_DIR, fname)
    if not fname.lower().endswith('.json'):
        continue
    try:
        with open(path, 'r', encoding='utf8') as f:
            data = json.load(f)
    except Exception:
        continue
    if not is_test_result(data):
        continue

    # existing owner label
    labels = data.get('labels') or []
    for label in labels:
        if not isinstance(label, dict):
            continue
        if label.get('name') == 'owner' and label.get('value'):
            owners.add(label.get('value'))
    # fallback: look for issue labels/links and map via jira_map
    issue_keys = set()
    for label in labels:
        if isinstance(label, dict) and label.get('name') in ('issue', 'Issue', 'ISSUE') and label.get('value'):
            parts = str(label.get('value')).split('/')
            issue_keys.add(parts[-1])
    for link in data.get('links') or []:
        if isinstance(link, dict) and link.get('type') == 'issue':
            name = link.get('name') or ''
            if name:
                issue_keys.add(name)
    for k in issue_keys:
        info = jira_map.get(k)
        if info and info.get('displayName'):
            owners.add(info.get('displayName'))

if not owners:
    print('Skipping owner reports: no owners discovered')
    raise SystemExit(0)

print('Discovered owners:', owners)

for owner in owners:
    # safe folder name
    owner_safe = slugify(owner)
    dest_results = os.path.join(BASE_OUT, owner_safe)
    shutil.rmtree(dest_results, ignore_errors=True)
    os.makedirs(dest_results, exist_ok=True)

    for metadata_name in ('categories.json', 'environment.properties', 'executor.json'):
        metadata_src = os.path.join(RESULTS_DIR, metadata_name)
        if os.path.exists(metadata_src):
            shutil.copy2(metadata_src, os.path.join(dest_results, metadata_name))

    history_src = os.path.join(RESULTS_DIR, 'history')
    history_dst = os.path.join(dest_results, 'history')
    if os.path.isdir(history_src):
        shutil.copytree(history_src, history_dst, dirs_exist_ok=True)

    # copy all matching JSON result files (and attachments if present), inject owner label when mapped from jira
    for fname in os.listdir(RESULTS_DIR):
        src = os.path.join(RESULTS_DIR, fname)
        if not fname.lower().endswith('.json'):
            continue
        try:
            with open(src, 'r', encoding='utf8') as f:
                data = json.load(f)
        except Exception:
            continue

        if not is_test_result(data):
            continue

        matched = False
        # 1) direct owner label match
        for label in data.get('labels') or []:
            if isinstance(label, dict) and label.get('name') == 'owner' and label.get('value') == owner:
                matched = True
                break

        # 2) map via issue keys -> jira_map
        if not matched:
            issue_keys = set()
            for label in data.get('labels') or []:
                if isinstance(label, dict) and label.get('name') in ('issue', 'Issue', 'ISSUE') and label.get('value'):
                    parts = str(label.get('value')).split('/')
                    issue_keys.add(parts[-1])
            for link in data.get('links') or []:
                if isinstance(link, dict) and link.get('type') == 'issue':
                    name = link.get('name') or ''
                    if name:
                        issue_keys.add(name)
            for k in issue_keys:
                info = jira_map.get(k)
                if info and info.get('displayName') == owner:
                    matched = True
                    break

        if not matched:
            continue

        # prepare destination JSON path; inject owner label if not present
        dst_json_path = os.path.join(dest_results, fname)
        data_labels = data.get('labels') or []
        has_owner = any(isinstance(l, dict) and l.get('name') == 'owner' for l in data_labels)
        if not has_owner:
            # prefer jira_map-based owner from issue keys
            assigned = None
            issue_keys = set()
            for label in data.get('labels') or []:
                if isinstance(label, dict) and label.get('name') in ('issue', 'Issue', 'ISSUE') and label.get('value'):
                    parts = str(label.get('value')).split('/')
                    issue_keys.add(parts[-1])
            for link in data.get('links') or []:
                if isinstance(link, dict) and link.get('type') == 'issue':
                    name = link.get('name') or ''
                    if name:
                        issue_keys.add(name)
            for k in issue_keys:
                info = jira_map.get(k)
                if info and info.get('displayName'):
                    assigned = info.get('displayName')
                    break
            if assigned is None:
                assigned = owner
            data_labels.append({'name': 'owner', 'value': assigned})
            data['labels'] = data_labels

        # write modified json into dest
        try:
            with open(dst_json_path, 'w', encoding='utf8') as f:
                json.dump(data, f, ensure_ascii=False, indent=2)
        except Exception:
            # fallback: copy raw
            shutil.copy2(src, dst_json_path)

        # attempt to copy possible attachment files next to json (same name with .txt/.bin etc)
        base = os.path.splitext(fname)[0]
        for ext in ('.txt', '.log', '.bin', '.png', '.jpg', '.json'):
            att = os.path.join(RESULTS_DIR, base + ext)
            if os.path.exists(att):
                shutil.copy2(att, os.path.join(dest_results, os.path.basename(att)))

    # generate report if we have results
    if os.listdir(dest_results):
        out_dir = os.path.join(BASE_OUT, owner_safe)
        shutil.rmtree(out_dir, ignore_errors=True)
        os.makedirs(out_dir, exist_ok=True)
        print(f'Generating report for owner {owner} -> {out_dir}')
        try:
            subprocess.check_call(['allure', 'generate', dest_results, '--clean', '-o', out_dir])
        except subprocess.CalledProcessError as e:
            print('allure generate failed for', owner, 'exit', e.returncode)
    else:
        print('No result files for owner', owner)

print('Per-owner generation complete')

# summary
generated = []
if os.path.isdir(BASE_OUT):
    for d in os.listdir(BASE_OUT):
        report_dir = os.path.join(BASE_OUT, d, 'report')
        if os.path.isdir(report_dir):
            generated.append(d)

shutil.rmtree(OUTPUT_BASE, ignore_errors=True)
os.makedirs('allure-report', exist_ok=True)
if os.path.isdir(BASE_OUT):
    shutil.move(BASE_OUT, OUTPUT_BASE)

print('Generated owner reports:', generated)

print('Generated owner reports:', generated)
