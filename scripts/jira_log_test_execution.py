#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
jira_log_test_execution.py
--------------------------
Auto-log CI test results as a Jira comment on the relevant issue(s).

Reads (env vars):
  JIRA_BASE_URL, JIRA_API_EMAIL (or JIRA_EMAIL), JIRA_API_TOKEN
  JIRA_PROJECT_KEY, TEST_TYPE, PASSED, FAILED, TOTAL, DURATION_SEC
  ALLURE_REPORT_URL, GITHUB_RUN_ID, GITHUB_RUN_NUMBER, etc.
"""

import json
import os
import re
import subprocess
import sys
import urllib.request
import urllib.error
from base64 import b64encode
from datetime import datetime, timezone

# Fix Windows console encoding (cp1252 cannot encode emoji)
if sys.stdout.encoding and sys.stdout.encoding.lower() not in ("utf-8", "utf8"):
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")
if sys.stderr.encoding and sys.stderr.encoding.lower() not in ("utf-8", "utf8"):
    import io
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding="utf-8", errors="replace")

# ── env ───────────────────────────────────────────────────────────────────────
JIRA_BASE    = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
JIRA_EMAIL   = (os.environ.get("JIRA_EMAIL", "")
                or os.environ.get("JIRA_API_EMAIL", ""))
JIRA_TOKEN   = os.environ.get("JIRA_API_TOKEN", "")
PROJECT_KEY  = os.environ.get("JIRA_PROJECT_KEY", "KIEM")

RUN_ID       = os.environ.get("GITHUB_RUN_ID", "0")
RUN_NUMBER   = os.environ.get("GITHUB_RUN_NUMBER", "0")
BRANCH       = os.environ.get("GITHUB_REF_NAME", "main")
SHA          = os.environ.get("GITHUB_SHA", "")[:7]
FULL_SHA     = os.environ.get("GITHUB_SHA", "")
REPO         = os.environ.get("GITHUB_REPOSITORY", "chi-trung/KCPM")
WORKFLOW     = os.environ.get("GITHUB_WORKFLOW", "CI")
PR_TITLE     = os.environ.get("PR_TITLE", "")
EPIC_KEY     = os.environ.get("JIRA_EPIC_KEY", "KIEM-3")  # Fallback epic for summary
TEST_TYPE    = os.environ.get("TEST_TYPE", "all").lower()

try:
    PASSED   = int(os.environ.get("PASSED",   "0"))
    FAILED   = int(os.environ.get("FAILED",   "0"))
    TOTAL    = int(os.environ.get("TOTAL",    "0"))
except ValueError:
    PASSED = FAILED = TOTAL = 0

DURATION_SEC = os.environ.get("DURATION_SEC", "")

ALLURE_URL   = os.environ.get(
    "ALLURE_REPORT_URL",
    f"https://chi-trung.github.io/KCPM/report-main/"
)
GH_RUN_URL   = f"https://github.com/{REPO}/actions/runs/{RUN_ID}"
TIMESTAMP    = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")

# ── Jira issue mapping ────────────────────────────────────────────────────────
# Map TEST_TYPE -> Jira issue keys to comment on.
# Board: Jira Board
#
# COMPLETE KIEM issue map (verified 2026-06-12, 24 total issues):
#
# WRP-BE-TESTS (xUnit coverage confirmed):
#   KIEM-4  = WRP-BE-TESTS-001 - Auth Module Testing                     (Nguyễn Chí Trung)
#   KIEM-5  = WRP-BE-TESTS-002 - Reports Module Testing                  (Minh Phụng)
#   KIEM-6  = WRP-BE-TESTS-003 - Notifications Module Testing            (Nguyễn Hoàng Phụng)
#   KIEM-7  = WRP-BE-TESTS-004 - Complaints Module Testing               (Thanh Duy)
#   KIEM-8  = WRP-BE-TESTS-005 - Admin Module Testing                    (Đăng)
#   KIEM-9  = WRP-BE-TESTS-006 - Analytics Module Testing                (Đăng)
#   KIEM-10 = WRP-BE-TESTS-007 - Public Analytics Testing               (Thanh Duy)
#   KIEM-12 = WRP-BE-TESTS-009 - WasteCategory Module Testing           (Nguyễn Hoàng Phụng)
#   KIEM-13 = WRP-BE-TESTS-010 - Citizen Module Testing                 (Đăng)
#   KIEM-14 = WRP-BE-TESTS-011 - Collector Module Testing               (Nguyễn Chí Trung)
#   KIEM-15 = WRP-BE-TESTS-012 - CollectorTask Module Testing           (Minh Phụng)
#   KIEM-16 = WRP-BE-TESTS-013 - Enterprise Task Module Testing         (Nguyễn Chí Trung)
#   KIEM-17 = WRP-BE-TESTS-014 - Enterprise Collectors & Reward Rules  (Nguyễn Chí Trung)
#   KIEM-18 = WRP-BE-TESTS-015 - CollectionTask / CollectionImage       (Thanh Duy)
#   KIEM-19 = WRP-BE-TESTS-016 - SignalR Real-time Tests               (Nguyễn Chí Trung)
#   KIEM-20 = WRP-BE-TESTS-017 - File Uploads & Storage Tests          (Minh Phụng)
#   KIEM-21 = WRP-BE-TESTS-018 - Security & Role-based Access Tests    (Nguyễn Hoàng Phụng)
#   KIEM-22 = WRP-BE-TESTS-019 - AuditLog & Error Path Tests           (Thanh Duy)
#   KIEM-23 = WRP-BE-TESTS-020 - Search, Pagination & Filters Tests    (Đăng)
#
# Non-test issues (BUGs/features - NOT auto-logged):
#   KIEM-3  = WRP-BE-TESTS parent epic (IN PROGRESS)
#   KIEM-26 = [BUG] Missing mandatory image validation (IN PROGRESS)
#   KIEM-27 = [BUG] PUT /notifications/{id}/read returns 200 for 404 (DONE)
#   KIEM-28 = Include taskId in report accept response (TO DO)
#   KIEM-29 = [BUG] Missing max 5 images validation (TO DO)
#
# Team leader (trungnc7062@ut.edu.vn) logs on behalf of all members via CI token.
#
# Mapping logic (UPDATED 2026-06-16):
#   1. Parse KIEM-<id> from branch name, commit message, PR title
#   2. Comment ONLY on the relevant issue(s) + Epic summary
#   3. Fallback to Epic KIEM-3 for scheduled/main runs without keys
#
# KNOWN_ISSUES: used for validation only (not for broadcast)
KNOWN_ISSUES = {
    # ── Original test module issues ──
    "KIEM-4", "KIEM-5", "KIEM-6", "KIEM-7", "KIEM-8",
    "KIEM-9", "KIEM-10", "KIEM-12", "KIEM-13", "KIEM-14",
    "KIEM-15", "KIEM-16", "KIEM-17", "KIEM-18", "KIEM-19",
    "KIEM-20", "KIEM-21", "KIEM-22", "KIEM-23",
    # ── Sprint-1 infrastructure ──
    "KIEM-40", "KIEM-44",
    # ── Sprint-2 task issues ──
    "KIEM-45", "KIEM-46", "KIEM-47", "KIEM-48", "KIEM-49",
    "KIEM-50", "KIEM-51", "KIEM-52", "KIEM-53", "KIEM-54",
    # ── Sprint-3 bug fix issues ──
    "KIEM-55", "KIEM-56",
    # ── Bug issues ──
    "KIEM-26", "KIEM-27", "KIEM-28", "KIEM-29",
    # ── Epic ──
    "KIEM-3",
}


def extract_kiem_keys() -> list:
    """Extract KIEM-<id> keys from branch name, commit message, and PR title.

    Returns a deduplicated, sorted list of KIEM keys found.
    Only returns keys that exist in KNOWN_ISSUES for safety.
    """
    sources = []
    keys_found = set()

    # Source 1: Branch name (e.g. KIEM-12-waste-category-tests)
    if BRANCH and BRANCH != "main":
        sources.append(("branch", BRANCH))

    # Source 2: PR title (e.g. "KIEM-5: Reports Module Testing")
    if PR_TITLE:
        sources.append(("pr_title", PR_TITLE))

    # Source 3: Latest commit message
    try:
        result = subprocess.run(
            ["git", "log", "-1", "--format=%s", FULL_SHA or "HEAD"],
            capture_output=True, text=True, timeout=10,
            cwd=os.environ.get("GITHUB_WORKSPACE", ".")
        )
        if result.returncode == 0 and result.stdout.strip():
            sources.append(("commit", result.stdout.strip()))
    except Exception as e:
        _log(f"[jira] Could not read commit message: {e}")

    # Parse KIEM-<number> from all sources
    pattern = re.compile(r"KIEM-\d+")
    for source_name, source_text in sources:
        matches = pattern.findall(source_text)
        for m in matches:
            if m in KNOWN_ISSUES:
                keys_found.add(m)
                _log(f"[jira] Found {m} in {source_name}: {source_text[:80]}")
            else:
                _log(f"[jira] Ignoring unknown key {m} from {source_name}")

    return sorted(keys_found)

# ── helpers ───────────────────────────────────────────────────────────────────
def _log(msg: str) -> None:
    """Print safe UTF-8 message, replacing unencodable chars."""
    try:
        print(msg)
    except Exception:
        print(msg.encode("ascii", errors="replace").decode("ascii"))


def auth_header() -> dict:
    creds = b64encode(f"{JIRA_EMAIL}:{JIRA_TOKEN}".encode()).decode()
    return {
        "Authorization": f"Basic {creds}",
        "Content-Type":  "application/json",
        "Accept":        "application/json",
    }


def jira_request(method: str, path: str, body: dict = None) -> dict:
    url  = f"{JIRA_BASE}/rest/api/3/{path}"
    data = json.dumps(body).encode("utf-8") if body else None
    req  = urllib.request.Request(url, data=data, headers=auth_header(), method=method)
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            raw = resp.read()
            if not raw:  # 204 No Content (e.g. POST /transitions returns empty body)
                return {"ok": True, "status": resp.status}
            return json.loads(raw)
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")
        _log(f"[jira] HTTP {e.code} on {method} {url}: {err[:300]}")
        return {"error": str(e.code), "body": err[:300]}
    except Exception as e:
        _log(f"[jira] Error on {method} {url}: {e}")
        return {"error": str(e)}


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


def status_label() -> str:
    """ASCII-safe status for CI logs."""
    if FAILED == 0 and TOTAL > 0:
        return "PASSED"
    if FAILED > 0:
        return "FAILED"
    return "NO_DATA"


def status_emoji() -> str:
    """Unicode emoji only used in Jira ADF body, not in CI logs."""
    if FAILED == 0 and TOTAL > 0:
        return "\u2705"   # ✅
    if FAILED > 0:
        return "\u274C"   # ❌
    return "\u26A0\uFE0F" # ⚠️


def build_comment_body() -> dict:
    """Build Atlassian Document Format (ADF) comment body."""
    emoji = status_emoji()
    rate  = pass_rate()
    dur   = format_duration(DURATION_SEC)
    label = TEST_TYPE.upper()

    return {
        "body": {
            "type": "doc",
            "version": 1,
            "content": [
                {
                    "type": "heading",
                    "attrs": {"level": 3},
                    "content": [
                        {"type": "text",
                         "text": f"{emoji} [{label}] CI Test Execution - Run #{RUN_NUMBER}"}
                    ]
                },
                {
                    "type": "table",
                    "attrs": {"isNumberColumnEnabled": False, "layout": "default"},
                    "content": [
                        _table_row("Field",        "Value",        header=True),
                        _table_row("Status",       f"{'PASSED' if FAILED == 0 and TOTAL > 0 else ('FAILED' if FAILED > 0 else 'NO DATA')}"),
                        _table_row("Tests",        f"{PASSED} passed / {FAILED} failed / {TOTAL} total"),
                        _table_row("Pass rate",    rate),
                        _table_row("Duration",     dur),
                        _table_row("Branch",       BRANCH),
                        _table_row("Commit",       SHA),
                        _table_row("Run #",        RUN_NUMBER),
                        _link_row("Allure Report",  ALLURE_URL, "View Allure Report"),
                        _link_row("GitHub Run",     GH_RUN_URL, f"Run #{RUN_NUMBER}"),
                        _table_row("Logged at",    TIMESTAMP),
                    ]
                },
                {
                    "type": "paragraph",
                    "content": [
                        {"type": "text",
                         "text": f"Automated by GitHub Actions - {WORKFLOW}",
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
                    {"type": "text", "text": label,
                     "marks": [{"type": "strong"}] if header else []}
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


def _link_row(label: str, url: str, link_text: str) -> dict:
    """Create a table row with a clickable hyperlink in the value cell."""
    return {
        "type": "tableRow",
        "content": [
            {
                "type": "tableCell",
                "attrs": {},
                "content": [{"type": "paragraph", "content": [
                    {"type": "text", "text": label}
                ]}]
            },
            {
                "type": "tableCell",
                "attrs": {},
                "content": [{"type": "paragraph", "content": [
                    {
                        "type": "text",
                        "text": link_text,
                        "marks": [{"type": "link", "attrs": {"href": url}}]
                    }
                ]}]
            }
        ]
    }


def post_comment(issue_key: str, body: dict) -> bool:
    _log(f"[jira] Posting comment to {issue_key}...")
    resp = jira_request("POST", f"issue/{issue_key}/comment", body)
    if "id" in resp:
        _log(f"[jira] OK - Comment created on {issue_key}: id={resp['id']}")
        return True
    _log(f"[jira] FAIL - Could not post to {issue_key}: {resp}")
    # If 404, provide helpful hint
    if resp.get("error") == "404":
        _log(f"[jira] HINT: Issue {issue_key} not found. Check project key and issue exists.")
    return False


def transition_issue_if_needed(issue_key: str) -> None:
    """If all tests pass (100%), try to move IN PROGRESS issue to Done/In Review.
    Skips issues that are already Done, or when there are failures.
    """
    if FAILED > 0 or TOTAL == 0:
        return  # Don't transition if any tests failed

    resp = jira_request("GET", f"issue/{issue_key}?fields=status")
    if "error" in resp:
        return

    current = resp.get("fields", {}).get("status", {}).get("name", "")
    _log(f"[jira] {issue_key} current status: {current}")

    # Skip if already Done/Closed - don't re-transition
    already_done = {"done", "closed", "resolved", "completed"}
    if current.lower() in already_done:
        _log(f"[jira] {issue_key} already {current} - skipping transition")
        return

    transitions = jira_request("GET", f"issue/{issue_key}/transitions")
    if "error" in transitions:
        return

    target_names = {"done", "in review", "resolved", "closed"}
    for t in transitions.get("transitions", []):
        if t.get("name", "").lower() in target_names:
            _log(f"[jira] Transitioning {issue_key} to {t['name']} (id={t['id']})")
            result = jira_request("POST", f"issue/{issue_key}/transitions",
                         {"transition": {"id": t["id"]}})
            if "error" not in result:
                _log(f"[jira] {issue_key} transitioned to {t['name']} successfully")
            break


def main():
    if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
        _log("[jira] Missing JIRA_BASE_URL / JIRA_EMAIL (or JIRA_API_EMAIL) / JIRA_API_TOKEN -- skipping")
        sys.exit(0)

    _log(f"[jira] Test type   : {TEST_TYPE}")
    _log(f"[jira] Results     : {PASSED} passed / {FAILED} failed / {TOTAL} total ({pass_rate()})")
    _log(f"[jira] Status      : {status_label()}")
    _log(f"[jira] Branch      : {BRANCH}")
    _log(f"[jira] PR Title    : {PR_TITLE or '(none)'}")
    _log(f"[jira] Allure URL  : {ALLURE_URL}")
    _log(f"[jira] GitHub Run  : {GH_RUN_URL}")
    _log(f"[jira] Jira Base   : {JIRA_BASE}")

    # Diagnostic: show credential info (partially masked)
    parts = JIRA_EMAIL.split('@')
    if len(parts) == 2:
        local, domain = parts
        email_masked = local[:3] + "***@" + domain  # e.g. "huy***@gmail.com"
    else:
        email_masked = "***"
    token_prefix = JIRA_TOKEN[:8] if len(JIRA_TOKEN) >= 8 else "(short token)"
    _log(f"[jira] Email       : {email_masked}")
    _log(f"[jira] Token prefix: {token_prefix}... (len={len(JIRA_TOKEN)})")
    _log(f"[jira] Token hint  : {'Atlassian Cloud token (ATAT...)' if JIRA_TOKEN.startswith('ATAT') else 'WARNING: does NOT start with ATAT - may be wrong token type'}")

    # Verify auth before attempting to post
    rest_url = f"{JIRA_BASE}/rest/api/3/myself"
    _log(f"[jira] Auth URL    : {rest_url}")
    _log(f"[jira] Checking Jira auth...")
    me = jira_request("GET", "myself")
    if "error" in me:
        err_body = me.get("body", "no response body")
        _log(f"[jira] WARN: /myself failed - HTTP {me['error']}")
        _log(f"[jira] Response body: {err_body[:200]}")
        _log(f"[jira] HINT #1: Token ATATT3xF... is correct format (ATAT)")
        _log(f"[jira] HINT #2: Verify email {email_masked} matches id.atlassian.com login email")
        _log(f"[jira] HINT #3: Ensure JIRA_BASE_URL is https://your-org.atlassian.net (no /jira suffix)")
        _log(f"[jira]          Current URL ends with: ...{JIRA_BASE[-30:] if len(JIRA_BASE) > 30 else JIRA_BASE}")
        sys.exit(0)   # Don't try to post if auth fails
    _log(f"[jira] Auth OK - logged in as: {me.get('emailAddress', me.get('displayName', 'unknown'))}")

    # ── Smart key extraction (replaces hardcoded ISSUE_MAP broadcast) ──
    extracted_keys = extract_kiem_keys()
    comment_body = build_comment_body()
    success_count = 0

    if extracted_keys:
        # Comment on specific issues found in branch/commit/PR
        _log(f"[jira] Targeted mode: commenting on {len(extracted_keys)} issue(s): {', '.join(extracted_keys)}")
        for key in extracted_keys:
            if post_comment(key, comment_body):
                success_count += 1
                transition_issue_if_needed(key)
    else:
        # Fallback: scheduled runs or main-branch pushes without KIEM keys
        _log(f"[jira] Fallback mode: no KIEM keys found in branch/commit/PR")
        _log(f"[jira] Commenting on Epic {EPIC_KEY} only (sprint summary)")
        if post_comment(EPIC_KEY, comment_body):
            success_count += 1

    total_targets = len(extracted_keys) if extracted_keys else 1
    _log(f"[jira] Done. Posted to {success_count}/{total_targets} issue(s).")
    # Exit 0 always -- Jira logging should never break CI
    sys.exit(0)


if __name__ == "__main__":
    main()
