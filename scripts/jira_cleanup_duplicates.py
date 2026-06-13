#!/usr/bin/env python3
"""
jira_cleanup_duplicates.py
---------------------------
DELETE all duplicate Jira issues (KIEM-75+).

These were accidentally created by the "both" workflow action.
Valid issues: KIEM-3 to KIEM-73 (original), everything else is a duplicate.

Run via: .github/workflows/create-jira-issues.yml (action=cleanup_duplicates)
"""
import json
import os
import sys
import urllib.request
import urllib.error
from base64 import b64encode

if sys.stdout.encoding and sys.stdout.encoding.lower() not in ("utf-8", "utf8"):
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

JIRA_BASE = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
JIRA_EMAIL = os.environ.get("JIRA_API_EMAIL", "") or os.environ.get("JIRA_EMAIL", "")
JIRA_TOKEN = os.environ.get("JIRA_API_TOKEN", "")


def _hdr():
    c = b64encode(f"{JIRA_EMAIL}:{JIRA_TOKEN}".encode()).decode()
    return {"Authorization": f"Basic {c}", "Content-Type": "application/json", "Accept": "application/json"}


def jira(method, path, body=None):
    url = f"{JIRA_BASE}/rest/api/3/{path}"
    data = json.dumps(body).encode("utf-8") if body else None
    req = urllib.request.Request(url, data=data, headers=_hdr(), method=method)
    try:
        with urllib.request.urlopen(req, timeout=30) as r:
            raw = r.read()
            return json.loads(raw) if raw else {"ok": True}
    except urllib.error.HTTPError as e:
        body_err = e.read().decode("utf-8", errors="replace")[:300]
        return {"error": str(e.code), "detail": body_err}
    except Exception as e:
        return {"error": str(e)}


def get_all_issues():
    """Get ALL issues in KIEM project using the new /search/jql endpoint."""
    from urllib.parse import quote
    jql = quote("project = KIEM ORDER BY key ASC")
    all_issues = []
    start = 0
    while True:
        resp = jira("GET", f"search/jql?jql={jql}&maxResults=100&startAt={start}&fields=key,summary,status")
        if "issues" not in resp:
            print(f"  Search failed: {resp}")
            break
        issues = resp["issues"]
        all_issues.extend(issues)
        if len(all_issues) >= resp.get("total", 0):
            break
        start += len(issues)
    return all_issues


def delete_issue(issue_key):
    """Delete an issue permanently."""
    result = jira("DELETE", f"issue/{issue_key}")
    if "error" in result:
        print(f"  ❌ DELETE {issue_key} failed: {result.get('error')} - {result.get('detail', '')[:100]}")
        return False
    print(f"  🗑️ DELETED {issue_key}")
    return True


def main():
    if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
        print("Missing JIRA credentials.")
        sys.exit(1)

    me = jira("GET", "myself")
    if "error" in me:
        print(f"Auth failed: {me}")
        sys.exit(1)
    print(f"Authenticated as: {me.get('displayName', '?')}")

    # Get all issues
    print("\n=== Fetching all issues ===")
    all_issues = get_all_issues()
    print(f"Found {len(all_issues)} total issues")

    # Valid issues: KIEM-3 to KIEM-73
    VALID_MAX = 73

    # Find duplicates (KIEM-74+)
    duplicates = []
    for issue in all_issues:
        key = issue["key"]
        try:
            num = int(key.replace("KIEM-", ""))
        except ValueError:
            continue
        if num > VALID_MAX:
            summary = issue["fields"]["summary"]
            status = issue["fields"]["status"]["name"]
            duplicates.append((key, num, summary, status))

    print(f"\nFound {len(duplicates)} duplicate issues (KIEM-{VALID_MAX + 1}+)")

    if not duplicates:
        print("No duplicates to delete!")
        return

    # Try to DELETE each duplicate
    deleted = 0
    failed = 0
    for key, num, summary, status in duplicates:
        print(f"\n  [{key}] [{status}] {summary[:60]}...")
        if delete_issue(key):
            deleted += 1
        else:
            failed += 1

    print(f"\n{'='*60}")
    print(f"Results: {deleted} deleted, {failed} failed, {len(duplicates)} total")

    if failed > 0:
        print(f"\n⚠️ {failed} issues could not be deleted (may need admin permission).")
        print("Those issues have been left as-is. Manual deletion may be required.")


if __name__ == "__main__":
    main()
