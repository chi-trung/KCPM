#!/usr/bin/env python3
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
REPO         = os.environ.get("GITHUB_REPOSITORY", "chi-trung/KCPM")
WORKFLOW     = os.environ.get("GITHUB_WORKFLOW", "CI")
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
# Board: https://ut-team-36.atlassian.net/jira/software/projects/KIEM/boards/3
# Verified issue keys (from board screenshot 2026-06-12):
#   KIEM-5  = WRP-BE-TESTS-002 Reports Module Testing (IN PROGRESS)
#   KIEM-14 = WRP-BE-TESTS-011 Collector Module Testing (DONE)
#   KIEM-15 = CollectorTask Module Testing
#   KIEM-21 = WRP-BE-TESTS-018 Security & Role-based Access Tests (IN PROGRESS)
#   KIEM-22 = WRP-BE-TESTS-019 AuditLog & Error Path Tests (IN PROGRESS)
ISSUE_MAP = {
    "backend": ["KIEM-5", "KIEM-22", "KIEM-21"],           # Reports Module - main backend test issue
    "postman": ["KIEM-21"],          # Security & Role-based Access Tests
    "e2e":     ["KIEM-14"],          # Collector Module Testing
    "all":     ["KIEM-5"],           # default fallback
}

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
    """If all tests pass, try to move issue to Done/In Review."""
    if FAILED > 0 or TOTAL == 0:
        return

    resp = jira_request("GET", f"issue/{issue_key}?fields=status")
    if "error" in resp:
        return

    current = resp.get("fields", {}).get("status", {}).get("name", "")
    _log(f"[jira] {issue_key} current status: {current}")

    transitions = jira_request("GET", f"issue/{issue_key}/transitions")
    if "error" in transitions:
        return

    target_names = {"done", "in review", "resolved", "closed"}
    for t in transitions.get("transitions", []):
        if t.get("name", "").lower() in target_names:
            _log(f"[jira] Transitioning {issue_key} to {t['name']} (id={t['id']})")
            jira_request("POST", f"issue/{issue_key}/transitions",
                         {"transition": {"id": t["id"]}})
            break


def main():
    if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
        _log("[jira] Missing JIRA_BASE_URL / JIRA_EMAIL (or JIRA_API_EMAIL) / JIRA_API_TOKEN -- skipping")
        sys.exit(0)

    _log(f"[jira] Test type   : {TEST_TYPE}")
    _log(f"[jira] Results     : {PASSED} passed / {FAILED} failed / {TOTAL} total ({pass_rate()})")
    _log(f"[jira] Status      : {status_label()}")
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

    issue_keys   = ISSUE_MAP.get(TEST_TYPE, ISSUE_MAP["all"])
    comment_body = build_comment_body()

    success_count = 0
    for key in issue_keys:
        if post_comment(key, comment_body):
            success_count += 1
            transition_issue_if_needed(key)

    _log(f"[jira] Done. Posted to {success_count}/{len(issue_keys)} issues.")
    # Exit 0 always -- Jira logging should never break CI
    sys.exit(0)


if __name__ == "__main__":
    main()
