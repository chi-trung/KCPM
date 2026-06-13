#!/usr/bin/env python3
"""
jira_assign_all.py
-------------------
Find ALL unassigned Jira issues in KIEM project and assign them
evenly across 5 team members.

Run via: .github/workflows/create-jira-issues.yml (action=assign_all)
"""
import json, os, sys, urllib.request, urllib.error
from base64 import b64encode
from urllib.parse import quote

JIRA_BASE = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
JIRA_EMAIL = os.environ.get("JIRA_API_EMAIL", "") or os.environ.get("JIRA_EMAIL", "")
JIRA_TOKEN = os.environ.get("JIRA_API_TOKEN", "")

def _hdr():
    c = b64encode(f"{JIRA_EMAIL}:{JIRA_TOKEN}".encode()).decode()
    return {"Authorization":f"Basic {c}","Content-Type":"application/json","Accept":"application/json"}

def jira(method, path, body=None):
    url = f"{JIRA_BASE}/rest/api/3/{path}"
    data = json.dumps(body).encode("utf-8") if body else None
    req = urllib.request.Request(url, data=data, headers=_hdr(), method=method)
    try:
        with urllib.request.urlopen(req, timeout=30) as r:
            raw = r.read()
            return json.loads(raw) if raw else {"ok": True}
    except urllib.error.HTTPError as e:
        print(f"  HTTP {e.code}: {e.read().decode('utf-8',errors='replace')[:200]}")
        return {"error": str(e.code)}
    except Exception as e:
        return {"error": str(e)}


# ── Team members ──
MEMBER_SEARCH = {
    "chi_trung": "Trung",
    "minh_phung": "Minh",
    "hoang_phung": "Hoàng Phụng",
    "thanh_duy": "Duy",
    "dang": "Đăng",
}
MEMBER_ORDER = ["chi_trung", "minh_phung", "hoang_phung", "thanh_duy", "dang"]
MEMBERS = {}


def find_members():
    """Find account IDs for all 5 team members."""
    for key, search in MEMBER_SEARCH.items():
        resp = jira("GET", f"user/search?query={search}&maxResults=5")
        if isinstance(resp, list):
            for user in resp:
                display = user.get("displayName", "")
                if search.lower() in display.lower():
                    MEMBERS[key] = user["accountId"]
                    print(f"  Found {key}: {display} ({user['accountId'][:12]}...)")
                    break
        if key not in MEMBERS:
            print(f"  WARNING: Not found: {key} ({search})")


def find_unassigned():
    """Query Jira for ALL unassigned issues in KIEM project."""
    jql = quote("project = KIEM AND assignee is EMPTY ORDER BY key ASC")
    resp = jira("GET", f"search?jql={jql}&maxResults=100&fields=key,summary,status,assignee")
    if "issues" not in resp:
        print(f"  Search failed: {resp}")
        return []
    
    issues = []
    for issue in resp["issues"]:
        key = issue["key"]
        summary = issue["fields"]["summary"]
        status = issue["fields"]["status"]["name"]
        issues.append({"key": key, "summary": summary, "status": status})
        print(f"  {key} [{status}] {summary}")
    
    return issues


def assign_issue(issue_key, member_key):
    """Assign a Jira issue to a team member."""
    account_id = MEMBERS.get(member_key)
    if not account_id:
        print(f"  SKIP {issue_key} — no account ID for {member_key}")
        return False

    body = {"accountId": account_id}
    result = jira("PUT", f"issue/{issue_key}/assignee", body)
    if "error" not in result:
        print(f"  OK — {issue_key} -> {member_key}")
        return True
    else:
        print(f"  FAILED — {issue_key}: {result}")
        return False


def main():
    if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
        print("Missing JIRA credentials.")
        sys.exit(1)

    me = jira("GET", "myself")
    if "error" in me:
        print(f"Auth failed: {me}")
        sys.exit(1)
    print(f"Authenticated as: {me.get('displayName','?')}")

    # Step 1: Find team members
    print("\n=== Step 1: Find team members ===")
    find_members()
    print(f"Found {len(MEMBERS)}/5 members")

    # Step 2: Find all unassigned issues
    print("\n=== Step 2: Find unassigned issues ===")
    unassigned = find_unassigned()
    print(f"\nFound {len(unassigned)} unassigned issues")

    if not unassigned:
        print("No unassigned issues found. Done!")
        return

    # Step 3: Distribute evenly using round-robin
    print(f"\n=== Step 3: Assign {len(unassigned)} issues to {len(MEMBERS)} members ===")
    ok = 0
    distribution = {k: [] for k in MEMBER_ORDER}

    for i, issue in enumerate(unassigned):
        member_key = MEMBER_ORDER[i % len(MEMBER_ORDER)]
        if assign_issue(issue["key"], member_key):
            distribution[member_key].append(issue["key"])
            ok += 1

    # Summary
    print(f"\n{'='*60}")
    print(f"Assigned {ok}/{len(unassigned)} issues")
    print("\nDistribution:")
    for member in MEMBER_ORDER:
        keys = distribution.get(member, [])
        print(f"  {member}: {len(keys)} issues — {', '.join(keys)}")


if __name__ == "__main__":
    main()
