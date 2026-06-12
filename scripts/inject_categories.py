#!/usr/bin/env python3
"""
inject_categories.py
--------------------
Post-process an Allure HTML report to inject custom categories.json
into the report's data directory. This overrides the categories
that allure-report-action generates automatically.

Usage:
  python3 scripts/inject_categories.py <report_dir> <categories_source>

Example:
  python3 scripts/inject_categories.py report-main Waste-Recycling-Platform/allure-categories.json
"""
import json
import sys
import shutil
from pathlib import Path


def main():
    if len(sys.argv) < 3:
        print("Usage: inject_categories.py <report_dir> <categories_json_source>")
        sys.exit(1)

    report_dir  = Path(sys.argv[1])
    source_file = Path(sys.argv[2])

    if not source_file.exists():
        print(f"[inject_categories] WARNING: source {source_file} not found, skipping")
        sys.exit(0)

    if not report_dir.exists():
        print(f"[inject_categories] WARNING: report dir {report_dir} not found, skipping")
        sys.exit(0)

    # Try to find categories.json in report data directory
    data_dir = report_dir / "data"
    if not data_dir.exists():
        # Some allure versions put it directly in report root
        data_dir = report_dir

    dest = data_dir / "categories.json"

    # Read source to verify it's valid JSON
    try:
        with open(source_file, encoding="utf-8") as f:
            cats = json.load(f)
        print(f"[inject_categories] Source has {len(cats)} categories")
    except Exception as e:
        print(f"[inject_categories] ERROR reading source: {e}")
        sys.exit(1)

    # Copy into report data dir
    shutil.copy2(source_file, dest)
    print(f"[inject_categories] Injected {source_file} -> {dest}")

    # Also update the widget
    widget_dir = report_dir / "widgets"
    if widget_dir.exists():
        widget_dest = widget_dir / "categories.json"
        # Build widget format from categories list
        widget_data = {"total": len(cats), "items": cats}
        with open(widget_dest, "w", encoding="utf-8") as f:
            json.dump(widget_data, f, ensure_ascii=False)
        print(f"[inject_categories] Updated widget {widget_dest}")

    print("[inject_categories] Done.")


if __name__ == "__main__":
    main()
