#!/usr/bin/env python3
"""
jira_log_test_execution.py
--------------------------
Auto-log CI test results as a Jira comment on the relevant issue(s).

Reads:
  - ALLURE_REPORT_URL   → link to the live Allure report
  - GITHUB_RUN_ID       → GitHub Actions run ID
  - GITHUB_RUN_NUMBER   → run number for display
  - GITHUB_REF_NAME     → branch name
  - GITHUB_SHA          → commit SHA (short)
  - GITHUB_REPOSITORY   → owner/repo
  - GITHUB_WORKFLOW     → workflow name
  - JIRA_BASE_URL       → e.g. https://ut-team-36.atlassian.net
  - JIRA_EMAIL          → atlassian account email
  - JIRA_API_TOKEN      → Jira API token (from secrets)
  - JIRA_PROJECT_KEY    → e.g. KIEM
  - TEST_TYPE           → 'backend' | 'e2e' | 'postman' | 'all'
  - PASSED              → number of passed tests
  - FAILED              → number of failed tests
  - TOTAL               → total tests
  - DURATION_SEC        → test duration in seconds (optional)

Usage:
  python3 jira_log_test_execution.py

This script:
1. Builds a rich Jira comment with test summary table
2. Finds the most relevant Jira issue based on TEST_TYPE
3. Posts the comment to that issue
4. Also posts a summary comment to the main EPIC issue (if found)
"""

import json
import os
import sys
import urllib.request
import urllib.error
from base64 import b64encode
from datetime import datetime, timezone

# ── env ────────────────────────────────────────────────────────────────────────
JIRA_BASE    = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
JIRA_EMAIL   = os.environ.get("JIRA_EMAIL", "") or os.environ.get("JIRA_API_EMAIL", "")
JIRA_TOKEN   = os.environ.get("JIRA_API_TOKEN", "")
PROJECT_KEY  = os.environ.get("JIRA_PROJECT_KEY", "KIEM")

RUN_ID       = os.environ.get("GITHUB_RUN_ID", "0")
RUN_NUMBER   = os.environ.get("GITHUB_RUN_NUMBER", "0")
BRANCH       = os.environ.get("GITHUB_REF_NAME", "main")
SHA          = os.environ.get("GITHUB_SHA", "")[:7]
REPO         = os.environ.get("GITHUB_REPOSITORY", "chi-trung/KCPM")
WORKFLOW     = os.environ.get("GITHUB_WORKFLOW", "CI")
TEST_TYPE    = os.environ.get("TEST_TYPE", "all").lower()   # backend | e2e | postman | all

PASSED       = int(os.environ.get("PASSED",   "0"))
FAILED       = int(os.environ.get("FAILED",   "0"))
TOTAL        = int(os.environ.get("TOTAL",    "0"))
DURATION_SEC = os.environ.get("DURATION_SEC", "")

ALLURE_URL   = os.environ.get("ALLURE_REPORT_URL",
               f"https://{REPO.split('/')[0]}.github.io/{REPO.split('/')[1]}/report-main/")
GH_RUN_URL   = f"https://github.com/{REPO}/actions/runs/{RUN_ID}"

TIMESTAMP    = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")

# ── Jira issue mapping ─────────────────────────────────────────────────────────
# Map TEST_TYPE → primary Jira issue keys to comment on
ISSUE_MAP = {
    "backend":  ["KIEM-4"],   # xUnit backend tests → Auth/Collector module
    "postman":  ["KIEM-21"],  # Postman API smoke tests
    "e2e":      ["KIEM-14", "KIEM-16"],  # E2E → Collector Task + Enterprise Assign
    "all":      ["KIEM-4"],   # full merge → main CI issue
}

# ── helpers ────────────────────────────────────────────────────────────────────
def auth_header() -> dict:
    creds = b64encode(f"{JIRA_EMAIL}:{JIRA_TOKEN}".encode()).decode()
    return {
        "Authorization": f"Basic {creds}",
        "Content-Type": "application/json",
        "Accept": "application/json",
    }


def jira_request(method: str, path: str, body: dict | None = None) -> dict:
    url = f"{JIRA_BASE}/rest/api/3/{path}"
    data = json.dumps(body).encode("utf-8") if body else None
    req = urllib.request.Request(url, data=data, headers=auth_header(), method=method)
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            return json.loads(resp.read())
    except urllib.error.HTTPError as e:
        err = e.read().decode()
        print(f"[jira] HTTP {e.code} on {method} {url}: {err[:300]}", file=sys.stderr)
        return {"error": str(e.code)}
    except Exception as e:
        print(f"[jira] Error on {method} {url}: {e}", file=sys.stderr)
        return {"error": str(e)}


def find_issues_by_type(test_type: str) -> list[str]:
    """Return Jira issue keys to comment on based on test type."""
    return ISSUE_MAP.get(test_type, ISSUE_MAP["all"])


def format_duration(sec_str: str) -> str:
    if not sec_str:
        return "N/A"
    try:
        sec = float(sec_str)
        m, s = divmod(int(sec), 60)
        return f"{m}m {s}s" if m else f"{s}s"
    except ValueError:
        return sec_str


def pass_rate() -> str:
    if TOTAL == 0:
        return "N/A"
    return f"{round(PASSED / TOTAL * 100, 1)}%"


def status_emoji() -> str:
    if FAILED == 0 and TOTAL > 0:
        return "✅"
    if FAILED > 0:
        return "❌"
    return "⚠️"


def build_comment_body() -> dict:
    """Build Atlassian Document Format (ADF) comment body."""
    emoji   = status_emoji()
    rate    = pass_rate()
    dur     = format_duration(DURATION_SEC)
    label   = TEST_TYPE.upper()

    # Plain-text summary (fallback)
    summary = (
        f"{emoji} *[{label}] CI Test Execution – Run #{RUN_NUMBER}*\n\n"
        f"| Field | Value |\n"
        f"|---|---|\n"
        f"| Status | {emoji} {'PASSED' if FAILED == 0 else 'FAILED'} |\n"
        f"| Tests | {PASSED} passed / {FAILED} failed / {TOTAL} total |\n"
        f"| Pass rate | {rate} |\n"
        f"| Duration | {dur} |\n"
        f"| Branch | {BRANCH} |\n"
        f"| Commit | {SHA} |\n"
        f"| Run | #{RUN_NUMBER} |\n"
        f"| Allure | {ALLURE_URL} |\n"
        f"| GitHub Run | {GH_RUN_URL} |\n"
        f"| Logged | {TIMESTAMP} |\n"
    )

    # ADF body
    return {
        "body": {
            "type": "doc",
            "version": 1,
            "content": [
                {
                    "type": "heading",
                    "attrs": {"level": 3},
                    "content": [
                        {"type": "text", "text": f"{emoji} [{label}] CI Test Execution – Run #{RUN_NUMBER}"}
                    ]
                },
                {
                    "type": "table",
                    "attrs": {"isNumberColumnEnabled": False, "layout": "default"},
                    "content": [
                        _table_row("Field", "Value", header=True),
                        _table_row("Status", f"{'✅ PASSED' if FAILED == 0 and TOTAL > 0 else ('❌ FAILED' if FAILED > 0 else '⚠️ NO DATA')}"),
                        _table_row("Tests", f"{PASSED} passed  /  {FAILED} failed  /  {TOTAL} total"),
                        _table_row("Pass rate", rate),
                        _table_row("Duration", dur),
                        _table_row("Branch", BRANCH),
                        _table_row("Commit", SHA),
                        _table_row("Run #", RUN_NUMBER),
                        _table_row("Allure Report", ALLURE_URL),
                        _table_row("GitHub Run", GH_RUN_URL),
                        _table_row("Logged at", TIMESTAMP),
                    ]
                },
                {
                    "type": "paragraph",
                    "content": [
                        {"type": "text", "text": f"Automated by GitHub Actions · {WORKFLOW}",
                         "marks": [{"type": "em"}]}
                    ]
                }
            ]
        }
    }


def _table_row(label: str, value: str, header: bool = False) -> dict:
    cell_type = "tableHeader" if header else "tableCell"
    return {
        "type": "tableRow",
        "content": [
            {
                "type": cell_type,
                "attrs": {},
                "content": [{"type": "paragraph", "content": [
                    {"type": "text", "text": label, "marks": [{"type": "strong"}] if header else []}
                ]}]
            },
            {
                "type": cell_type,
                "attrs": {},
                "content": [{"type": "paragraph", "content": [
                    {"type": "text", "text": value}
                ]}]
            }
        ]
    }


def post_comment(issue_key: str, body: dict) -> bool:
    print(f"[jira] Posting comment to {issue_key}...")
    resp = jira_request("POST", f"issue/{issue_key}/comment", body)
    if "id" in resp:
        print(f"[jira] ✅ Comment created on {issue_key}: id={resp['id']}")
        return True
    print(f"[jira] ❌ Failed to post comment on {issue_key}: {resp}")
    return False


def transition_issue_if_needed(issue_key: str) -> None:
    """If all tests pass, move issue to 'Done' / 'In Review' if it's still 'In Progress'."""
    if FAILED > 0 or TOTAL == 0:
        return  # don't auto-transition on failure

    # Get current status
    resp = jira_request("GET", f"issue/{issue_key}?fields=status")
    if "error" in resp:
        return

    current = resp.get("fields", {}).get("status", {}).get("name", "")
    print(f"[jira] {issue_key} current status: {current}")

    # Get available transitions
    transitions = jira_request("GET", f"issue/{issue_key}/transitions")
    if "error" in transitions:
        return

    target_names = {"done", "in review", "resolved", "closed"}
    for t in transitions.get("transitions", []):
        if t.get("name", "").lower() in target_names:
            print(f"[jira] Transitioning {issue_key} → {t['name']} (id={t['id']})")
            jira_request("POST", f"issue/{issue_key}/transitions",
                         {"transition": {"id": t["id"]}})
            break


def main():
    if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
        print("[jira] Missing JIRA_BASE_URL / JIRA_EMAIL / JIRA_API_TOKEN — skipping")
        sys.exit(0)

    print(f"[jira] Test type: {TEST_TYPE}")
    print(f"[jira] Results: {PASSED}P / {FAILED}F / {TOTAL}T  ({pass_rate()})")
    print(f"[jira] Allure: {ALLURE_URL}")

    issue_keys = find_issues_by_type(TEST_TYPE)
    comment_body = build_comment_body()

    success_count = 0
    for key in issue_keys:
        if post_comment(key, comment_body):
            success_count += 1
            transition_issue_if_needed(key)

    print(f"[jira] Done. Posted to {success_count}/{len(issue_keys)} issues.")
    sys.exit(0 if success_count > 0 else 1)


if __name__ == "__main__":
    main()
