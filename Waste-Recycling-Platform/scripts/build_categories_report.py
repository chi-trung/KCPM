#!/usr/bin/env python3
"""
build_categories_report.py
--------------------------
Post-process Allure report to apply custom categories.json rules.

This script:
1. Reads allure test result JSON files (*-result.json) from allure-results/
2. Applies custom categories rules (matching by status + messageRegex/traceRegex)
3. Builds proper Allure-compatible data/categories.json and widgets/categories.json
4. Injects them into the generated report directory

Usage:
    python3 build_categories_report.py <report_dir> <results_dir> <categories_rules_file>

Example:
    python3 build_categories_report.py report-main Waste-Recycling-Platform/allure-results \
        Waste-Recycling-Platform/allure-categories.json
"""

import sys
import os
import json
import re
import glob
import uuid
from collections import defaultdict

def load_json(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return json.load(f)
    except Exception as e:
        print(f"[cats] ERROR loading {path}: {e}")
        return None

def save_json(path, data):
    try:
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, 'w', encoding='utf-8') as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
        print(f"[cats] Saved: {path}")
    except Exception as e:
        print(f"[cats] ERROR saving {path}: {e}")

def match_category(result, rule):
    """Check if a test result matches a category rule."""
    # Check status match
    matched_statuses = rule.get('matchedStatuses', [])
    if matched_statuses and result.get('status') not in matched_statuses:
        return False

    # Get message and trace from result
    message = result.get('statusDetails', {}).get('message', '') or \
              result.get('statusMessage', '') or ''
    trace = result.get('statusDetails', {}).get('trace', '') or \
            result.get('statusTrace', '') or ''

    # Check messageRegex
    msg_regex = rule.get('messageRegex', '')
    if msg_regex:
        if not re.search(msg_regex, message, re.IGNORECASE):
            return False

    # Check traceRegex
    trace_regex = rule.get('traceRegex', '')
    if trace_regex:
        if not re.search(trace_regex, trace, re.IGNORECASE):
            return False

    return True

def build_categories(results_dir, categories_rules_file, report_dir):
    """Build categories.json files for the Allure report."""

    # Load categories rules
    rules = load_json(categories_rules_file)
    if not rules:
        print("[cats] No categories rules found")
        return False

    print(f"[cats] Loaded {len(rules)} category rules")

    # Find all test result files
    result_files = glob.glob(os.path.join(results_dir, '*-result.json'))
    print(f"[cats] Found {len(result_files)} result files")

    if not result_files:
        print("[cats] No result files found, skipping")
        return False

    # Group results by category
    # category_map: {category_name: {message_key: [test_results]}}
    category_map = defaultdict(lambda: defaultdict(list))
    unmatched = []

    for result_path in result_files:
        result = load_json(result_path)
        if not result:
            continue

        # Skip passed tests (categories only for failed/broken)
        status = result.get('status', 'unknown')
        if status in ('passed', 'skipped'):
            continue

        # Try each rule in order
        matched = False
        for rule in rules:
            if match_category(result, rule):
                cat_name = rule['name']
                message = result.get('statusDetails', {}).get('message', '') or \
                          result.get('statusMessage', '') or 'No message'
                # Truncate message for grouping key
                msg_key = message[:120] if message else 'No message'
                category_map[cat_name][msg_key].append(result)
                matched = True
                break

        if not matched and status in ('failed', 'broken'):
            # Default: Product defects for failed, Test defects for broken
            default_cat = 'Product defects' if status == 'failed' else 'Test defects'
            message = result.get('statusDetails', {}).get('message', '') or \
                      result.get('statusMessage', '') or 'No message'
            msg_key = message[:120] if message else 'No message'
            category_map[default_cat][msg_key].append(result)
            unmatched.append(result.get('name', 'unknown'))

    print(f"[cats] Categories distribution:")
    for cat_name, msgs in category_map.items():
        total = sum(len(v) for v in msgs.values())
        print(f"[cats]   {cat_name}: {total} tests")

    if unmatched:
        print(f"[cats] Unmatched (→ Product defects): {len(unmatched)} tests")

    # Build data/categories.json (Allure internal format)
    categories_children = []
    all_total = 0

    for cat_name, msgs in sorted(category_map.items()):
        cat_uid = str(uuid.uuid4()).replace('-', '')[:16]
        msg_children = []
        cat_total = 0

        for msg_key, test_results in msgs.items():
            msg_uid = str(uuid.uuid4()).replace('-', '')[:16]
            test_children = []

            for r in test_results:
                uid = r.get('uuid', str(uuid.uuid4()).replace('-', '')[:16])
                # Use short uid for test child (16 hex chars)
                uid = uid.replace('-', '')[:16]
                test_children.append({
                    "name": r.get('name', 'Unknown test'),
                    "uid": uid,
                    "parentUid": msg_uid,
                    "status": r.get('status', 'failed'),
                    "time": r.get('time', {}),
                    "flaky": r.get('flaky', False),
                    "newFailed": r.get('newFailed', False),
                    "newPassed": r.get('newPassed', False),
                    "newBroken": r.get('newBroken', False),
                    "retriesCount": r.get('retriesCount', 0),
                    "retriesStatusChange": r.get('retriesStatusChange', False),
                    "parameters": r.get('parameters', []),
                    "tags": []
                })
                cat_total += 1
                all_total += 1

            msg_children.append({
                "name": msg_key,
                "children": test_children,
                "uid": msg_uid
            })

        categories_children.append({
            "name": cat_name,
            "children": msg_children,
            "uid": cat_uid
        })

    # data/categories.json format
    data_categories = {
        "uid": str(uuid.uuid4()).replace('-', '')[:32],
        "name": "categories",
        "children": categories_children
    }

    # widgets/categories.json format
    widgets_categories = {
        "total": len(category_map),
        "items": []
    }

    for cat_name, msgs in sorted(category_map.items()):
        cat_total = sum(len(v) for v in msgs.values())
        failed = sum(1 for tests in msgs.values() for t in tests if t.get('status') == 'failed')
        broken = sum(1 for tests in msgs.values() for t in tests if t.get('status') == 'broken')

        widgets_categories["items"].append({
            "uid": str(uuid.uuid4()).replace('-', '')[:32],
            "name": cat_name,
            "statistic": {
                "failed": failed,
                "broken": broken,
                "skipped": 0,
                "passed": 0,
                "unknown": 0,
                "total": cat_total
            }
        })

    # Save files to report
    data_path = os.path.join(report_dir, 'data', 'categories.json')
    widgets_path = os.path.join(report_dir, 'widgets', 'categories.json')

    save_json(data_path, data_categories)
    save_json(widgets_path, widgets_categories)

    print(f"[cats] Categories built: {len(category_map)} categories, {all_total} total tests")
    return True

if __name__ == '__main__':
    if len(sys.argv) < 4:
        print("Usage: build_categories_report.py <report_dir> <results_dir> <categories_rules_file>")
        sys.exit(1)

    report_dir = sys.argv[1]
    results_dir = sys.argv[2]
    categories_file = sys.argv[3]

    if not os.path.isdir(report_dir):
        print(f"[cats] ERROR: Report directory not found: {report_dir}")
        sys.exit(1)

    if not os.path.isdir(results_dir):
        print(f"[cats] ERROR: Results directory not found: {results_dir}")
        sys.exit(1)

    if not os.path.isfile(categories_file):
        print(f"[cats] ERROR: Categories file not found: {categories_file}")
        sys.exit(1)

    success = build_categories(results_dir, categories_file, report_dir)
    sys.exit(0 if success else 1)
