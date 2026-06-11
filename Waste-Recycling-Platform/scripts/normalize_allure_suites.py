#!/usr/bin/env python3
"""
normalize_allure_suites.py
--------------------------
Post-process allure-results JSON files to group suites into exactly 3 categories:
  - "E2E Tests"        → TC-E2E-xxx, Frontend smoke, CodeceptJS results
  - "API Tests"        → Postman / Newman results
  - "Backend Tests"    → xUnit / WastePlatform.Tests / dotnet results

Run this BEFORE `allure generate` so the Suites tree shows only 3 top-level items.
"""
import json
import sys
from pathlib import Path


RESULTS_DIR = Path(sys.argv[1]) if len(sys.argv) > 1 else Path("Waste-Recycling-Platform/allure-results")

PARENT_E2E      = "E2E Tests"
PARENT_API      = "API Tests (Postman)"
PARENT_BACKEND  = "Backend Tests (xUnit)"

SUITE_E2E      = "E2E"
SUITE_API      = "Postman"
SUITE_BACKEND  = "xUnit"


def classify(data: dict) -> tuple[str, str]:
    """Return (parentSuite, suite) based on the result content."""
    labels      = data.get("labels", [])
    full_name   = data.get("fullName", "") or ""
    name        = data.get("name", "") or ""
    suite_val   = next((l["value"] for l in labels if l["name"] == "suite"),    "")
    parent_val  = next((l["value"] for l in labels if l["name"] == "parentSuite"), "")
    package_val = next((l["value"] for l in labels if l["name"] == "package"),  "")

    # --- E2E signals ---
    e2e_keywords = ["TC-E2E", "Frontend smoke", "citizen_report", "enterprise_assign",
                    "collector_task", "waste-platform-frontend", "e2e", "codeceptjs",
                    "CodeceptJS", "playwright"]
    for kw in e2e_keywords:
        if kw.lower() in suite_val.lower()   \
                or kw.lower() in full_name.lower() \
                or kw.lower() in name.lower()      \
                or kw.lower() in package_val.lower():
            return PARENT_E2E, SUITE_E2E

    # --- Postman/Newman signals ---
    postman_keywords = ["Postman", "Newman", "newman", "pm.test", "postman",
                        "WastePlatform.professional", "API Test", "Smoke"]
    for kw in postman_keywords:
        if kw.lower() in suite_val.lower()   \
                or kw.lower() in full_name.lower() \
                or kw.lower() in name.lower()      \
                or kw.lower() in package_val.lower():
            return PARENT_API, SUITE_API

    # --- Default → Backend / xUnit ---
    return PARENT_BACKEND, SUITE_BACKEND


def normalize(path: Path) -> bool:
    try:
        raw  = path.read_text(encoding="utf-8")
        data = json.loads(raw)
    except Exception as e:
        print(f"  SKIP {path.name}: {e}")
        return False

    parent, suite = classify(data)

    labels = data.get("labels", [])
    # Remove old parentSuite / suite labels, keep everything else
    labels = [l for l in labels if l["name"] not in ("parentSuite", "suite")]
    labels.append({"name": "parentSuite", "value": parent})
    labels.append({"name": "suite",       "value": suite})
    data["labels"] = labels

    path.write_text(json.dumps(data, ensure_ascii=False, indent=None), encoding="utf-8")
    return True


def main():
    if not RESULTS_DIR.exists():
        print(f"[normalize_allure_suites] Directory not found: {RESULTS_DIR}")
        sys.exit(0)

    result_files = list(RESULTS_DIR.glob("*-result.json"))
    print(f"[normalize_allure_suites] Processing {len(result_files)} result files in {RESULTS_DIR}")

    counts = {PARENT_E2E: 0, PARENT_API: 0, PARENT_BACKEND: 0}
    for f in result_files:
        parent, _ = classify(json.loads(f.read_text(encoding="utf-8")) if f.stat().st_size > 0 else {})
        normalize(f)
        counts[parent] = counts.get(parent, 0) + 1

    print("[normalize_allure_suites] Suite assignment summary:")
    for k, v in counts.items():
        print(f"  {k}: {v} tests")
    print("[normalize_allure_suites] Done.")


if __name__ == "__main__":
    main()
