#!/usr/bin/env python3
"""
jira_cleanup_duplicates.py
---------------------------
Close all duplicate Jira issues by transitioning them to Done and 
prefixing titles with [DUPLICATE].

Duplicate ranges:
  - KIEM-74 to KIEM-83 (duplicates of KIEM-63 to KIEM-72)
  - KIEM-84 to KIEM-135+ (duplicates created by accidental re-runs)

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


# Known valid issue range (non-duplicates)
VALID_ISSUES = set(range(3, 74))  # KIEM-3 to KIEM-73 are valid
# KIEM-74+ are potential duplicates

def get_all_issues():
    """Get ALL issues in KIEM project."""
    from urllib.parse import quote
    jql = quote("project = KIEM ORDER BY key ASC")
    all_issues = []
    start = 0
    while True:
        resp = jira("GET", f"search?jql={jql}&maxResults=100&startAt={start}&fields=key,summary,status")
        if "issues" not in resp:
            print(f"  Search failed: {resp}")
            break
        issues = resp["issues"]
        all_issues.extend(issues)
        if len(all_issues) >= resp.get("total", 0):
            break
        start += len(issues)
    return all_issues


def get_transitions(issue_key):
    """Get available transitions for an issue."""
    resp = jira("GET", f"issue/{issue_key}/transitions")
    if "transitions" in resp:
        return {t["name"]: t["id"] for t in resp["transitions"]}
    return {}


def transition_to_done(issue_key):
    """Transition an issue to Done status."""
    transitions = get_transitions(issue_key)
    
    # Try direct "Done" transition
    if "Done" in transitions:
        result = jira("POST", f"issue/{issue_key}/transitions", {"transition": {"id": transitions["Done"]}})
        return "error" not in result
    
    # Try "In Progress" first, then "Done"
    if "In Progress" in transitions:
        jira("POST", f"issue/{issue_key}/transitions", {"transition": {"id": transitions["In Progress"]}})
        # Now try Done again
        transitions2 = get_transitions(issue_key)
        if "Done" in transitions2:
            result = jira("POST", f"issue/{issue_key}/transitions", {"transition": {"id": transitions2["Done"]}})
            return "error" not in result
    
    return False


def mark_as_duplicate(issue_key, summary):
    """Prefix title with [DUPLICATE] and transition to Done."""
    if not summary.startswith("[DUPLICATE]"):
        new_summary = f"[DUPLICATE] {summary}"
        jira("PUT", f"issue/{issue_key}", {"fields": {"summary": new_summary}})
        print(f"  Renamed: {new_summary[:80]}")
    
    if transition_to_done(issue_key):
        print(f"  ✅ {issue_key} → Done")
        return True
    else:
        print(f"  ⚠️ {issue_key} — could not transition to Done")
        return False


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

    # Find duplicates (KIEM-74+)
    duplicates = []
    for issue in all_issues:
        key = issue["key"]
        num = int(key.replace("KIEM-", ""))
        summary = issue["fields"]["summary"]
        status = issue["fields"]["status"]["name"]
        
        if num >= 74 and status != "Done":
            duplicates.append((key, summary, status))
        elif num >= 74 and status == "Done" and not summary.startswith("[DUPLICATE]"):
            # Already done but not marked — just rename
            duplicates.append((key, summary, status))

    print(f"\nFound {len(duplicates)} duplicate issues to process")

    # Process duplicates
    success = 0
    for key, summary, status in duplicates:
        print(f"\n  Processing {key} [{status}]: {summary[:60]}...")
        if status == "Done":
            # Just rename
            if not summary.startswith("[DUPLICATE]"):
                jira("PUT", f"issue/{key}", {"fields": {"summary": f"[DUPLICATE] {summary}"}})
                print(f"  Renamed to [DUPLICATE]")
            success += 1
        else:
            if mark_as_duplicate(key, summary):
                success += 1

    print(f"\n{'='*60}")
    print(f"Processed {success}/{len(duplicates)} duplicates")


if __name__ == "__main__":
    main()
