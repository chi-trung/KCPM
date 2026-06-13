#!/usr/bin/env python3
"""
jira_audit.py — Full audit of ALL Jira issues in KIEM project.
Lists every issue with: key, summary, status, assignee, comment count.
Outputs a markdown table for easy review.

Run via GitHub Actions: action=audit_board
"""
import json, os, sys, urllib.request, urllib.error
from base64 import b64encode

if sys.stdout.encoding and sys.stdout.encoding.lower() not in ("utf-8", "utf8"):
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

JIRA_BASE  = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
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
            return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as e:
        return {"error": str(e.code), "detail": e.read().decode("utf-8", errors="replace")[:200]}
    except Exception as e:
        return {"error": str(e)}


def get_comment_count(issue_key):
    """Get comment count for an issue."""
    resp = jira("GET", f"issue/{issue_key}/comment?maxResults=0")
    if "error" in resp:
        return -1
    return resp.get("total", 0)


def main():
    if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
        print("Missing JIRA credentials.")
        sys.exit(1)

    me = jira("GET", "myself")
    if "error" in me:
        print(f"Auth failed: {me}")
        sys.exit(1)
    print(f"Authenticated as: {me.get('displayName', '?')}")

    # Get ALL issues
    from urllib.parse import quote
    jql = quote("project = KIEM ORDER BY key ASC")
    all_issues = []
    start = 0
    while True:
        resp = jira("GET", f"search?jql={jql}&maxResults=100&startAt={start}&fields=key,summary,status,assignee,comment")
        if "issues" not in resp:
            print(f"Search failed: {resp}")
            break
        issues = resp["issues"]
        all_issues.extend(issues)
        if len(all_issues) >= resp.get("total", 0):
            break
        start += len(issues)

    print(f"\nTotal issues: {len(all_issues)}")
    print()

    # Print table header
    print("| # | Key | Status | Comments | Assignee | Summary |")
    print("|---|-----|--------|----------|----------|---------|")

    problems = []
    for i, issue in enumerate(all_issues, 1):
        key = issue["key"]
        summary = issue["fields"]["summary"][:60]
        status = issue["fields"]["status"]["name"]
        assignee = issue["fields"].get("assignee")
        assignee_name = assignee.get("displayName", "?") if assignee else "Unassigned"

        # Get comment count from the comment field
        comments = issue["fields"].get("comment", {})
        comment_count = comments.get("total", 0) if isinstance(comments, dict) else 0

        # Flag issues that are Done but have 0 comments
        flag = ""
        if status == "Done" and comment_count == 0:
            flag = " ⚠️"
            problems.append(key)

        print(f"| {i} | {key} | {status} | {comment_count}{flag} | {assignee_name[:20]} | {summary} |")

    # Summary
    print()
    print(f"## Summary")
    done_count = sum(1 for i in all_issues if i["fields"]["status"]["name"] == "Done")
    todo_count = sum(1 for i in all_issues if i["fields"]["status"]["name"] == "To Do")
    ip_count = sum(1 for i in all_issues if i["fields"]["status"]["name"] == "In Progress")
    print(f"- Done: {done_count}")
    print(f"- In Progress: {ip_count}")
    print(f"- To Do: {todo_count}")
    print(f"- Total: {len(all_issues)}")

    if problems:
        print(f"\n## ⚠️ Issues marked Done with 0 comments ({len(problems)}):")
        for key in problems:
            print(f"  - {key}")


if __name__ == "__main__":
    main()
