#!/usr/bin/env python3
"""
Generate per-owner Allure reports by filtering result JSON files that contain a label 'owner'.
Writes reports into 'allure-report/owners/<owner>/' so they are published with the main report.

Usage: run from repository root (where `Waste-Recycling-Platform/allure-results` exists) and
have `allure` CLI available in PATH.
"""
import json
import os
import shutil
import subprocess

RESULTS_DIR = os.path.join('Waste-Recycling-Platform', 'allure-results')
OUTPUT_BASE = os.path.join('allure-report', 'owners')

if not os.path.isdir(RESULTS_DIR):
    print('No results dir found at', RESULTS_DIR)
    raise SystemExit(0)

owners = set()
# Collect owners from JSON result files
for fname in os.listdir(RESULTS_DIR):
    path = os.path.join(RESULTS_DIR, fname)
    if not fname.lower().endswith('.json'):
        continue
    try:
        with open(path, 'r', encoding='utf8') as f:
            data = json.load(f)
    except Exception:
        continue
    labels = data.get('labels') or []
    for label in labels:
        if not isinstance(label, dict):
            continue
        if label.get('name') == 'owner' and label.get('value'):
            owners.add(label.get('value'))

print('Discovered owners:', owners)

for owner in owners:
    # safe folder name
    owner_safe = owner.replace(' ', '_').replace('/', '_')
    dest_results = os.path.join('tmp_owner_results', owner_safe)
    shutil.rmtree(dest_results, ignore_errors=True)
    os.makedirs(dest_results, exist_ok=True)

    # copy all matching JSON result files (and attachments if present)
    for fname in os.listdir(RESULTS_DIR):
        src = os.path.join(RESULTS_DIR, fname)
        if fname.lower().endswith('.json'):
            try:
                with open(src, 'r', encoding='utf8') as f:
                    data = json.load(f)
            except Exception:
                continue
            labels = data.get('labels') or []
            for label in labels:
                if label.get('name') == 'owner' and label.get('value') == owner:
                    shutil.copy2(src, os.path.join(dest_results, fname))
                    # attempt to copy possible attachment files next to json (same name with .txt/.bin)
                    base = os.path.splitext(fname)[0]
                    for ext in ('.txt', '.log', '.bin', '.png', '.jpg', '.json'):
                        att = os.path.join(RESULTS_DIR, base + ext)
                        if os.path.exists(att):
                            shutil.copy2(att, os.path.join(dest_results, os.path.basename(att)))
                    break

    # generate report if we have results
    if os.listdir(dest_results):
        out_dir = os.path.join('allure-report', 'owners', owner_safe)
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
