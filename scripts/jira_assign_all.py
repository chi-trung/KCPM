#!/usr/bin/env python3
"""
jira_assign_all.py
-------------------
Find ALL unassigned Jira issues in KIEM project and assign them
evenly across 5 team members.

Uses POST /rest/api/3/search/jql for JQL search (Jira Cloud v3).
Uses PUT /rest/api/3/issue/{key}/assignee for assignment.
"""
import json, os, sys, urllib.request, urllib.error
from base64 import b64encode
from urllib.parse import quote

if sys.stdout.encoding and sys.stdout.encoding.lower() not in ("utf-8", "utf8"):
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

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
        body_err = e.read().decode('utf-8', errors='replace')[:300]
        print(f"  HTTP {e.code}: {body_err}")
        return {"error": str(e.code), "detail": body_err}
    except Exception as e:
        return {"error": str(e)}


# ── Team members ──
MEMBER_ORDER = ["chi_trung", "minh_phung", "hoang_phung", "thanh_duy", "dang"]
MEMBERS = {}


def find_members():
    """Find account IDs for all 5 team members by searching project members."""
    # Use assignable user search for the project
    resp = jira("GET", "user/assignable/search?project=KIEM&maxResults=50")
    if not isinstance(resp, list):
        print(f"  Could not search users: {resp}")
        return

    # Map known names to keys
    name_map = {
        "chi_trung": ["trung", "chi"],
        "minh_phung": ["minh ph"],
        "hoang_phung": ["hoang", "hoàng ph"],
        "thanh_duy": ["duy", "thanh d"],
        "dang": ["đăng", "dang", "11a6"],
    }

    for user in resp:
        display = user.get("displayName", "").lower()
        account_id = user.get("accountId", "")
        for key, patterns in name_map.items():
            if key not in MEMBERS:
                for pattern in patterns:
                    if pattern in display:
                        MEMBERS[key] = account_id
                        print(f"  Found {key}: {user['displayName']} ({account_id[:12]}...)")
                        break

    # Fallback: if still missing, list all users
    for key in MEMBER_ORDER:
        if key not in MEMBERS:
            print(f"  WARNING: Not found: {key}")

    if not MEMBERS:
        print("\n  All assignable users:")
        for user in resp:
            print(f"    {user.get('displayName', '?')} ({user.get('accountId', '?')[:12]}...)")


def find_unassigned():
    """Query Jira for ALL unassigned issues in KIEM project."""
    # Use GET /rest/api/3/search with JQL in query params
    jql = quote("project = KIEM AND assignee is EMPTY ORDER BY key ASC")
    resp = jira("GET", f"search?jql={jql}&maxResults=100&fields=key,summary,status,assignee")

    if "issues" not in resp:
        # Fallback: try the newer search/jql endpoint with POST
        print("  GET search failed, trying POST /search/jql...")
        body = {
            "jql": "project = KIEM AND assignee is EMPTY ORDER BY key ASC",
            "maxResults": 100,
            "fields": ["key", "summary", "status", "assignee"]
        }
        resp = jira("POST", "search/jql", body)

    if "issues" not in resp:
        print(f"  Both search methods failed: {resp}")
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
        print(f"  SKIP {issue_key} -> {member_key} (no account ID)")
        return False

    body = {"accountId": account_id}
    result = jira("PUT", f"issue/{issue_key}/assignee", body)
    if "error" not in result:
        print(f"  OK: {issue_key} -> {member_key}")
        return True
    else:
        print(f"  FAILED: {issue_key}: {result}")
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

    if not MEMBERS:
        print("ERROR: No team members found. Cannot assign.")
        sys.exit(1)

    # Step 2: Find all unassigned issues
    print("\n=== Step 2: Find unassigned issues ===")
    unassigned = find_unassigned()
    print(f"\nFound {len(unassigned)} unassigned issues")

    if not unassigned:
        print("No unassigned issues found. All issues already assigned!")
        return

    # Step 3: Distribute evenly using round-robin
    available = [k for k in MEMBER_ORDER if k in MEMBERS]
    print(f"\n=== Step 3: Assign {len(unassigned)} issues to {len(available)} members ===")
    ok = 0
    distribution = {k: [] for k in available}

    for i, issue in enumerate(unassigned):
        member_key = available[i % len(available)]
        if assign_issue(issue["key"], member_key):
            distribution[member_key].append(issue["key"])
            ok += 1

    # Summary
    print(f"\n{'='*60}")
    print(f"Assigned {ok}/{len(unassigned)} issues")
    print("\nDistribution:")
    for member in available:
        keys = distribution.get(member, [])
        print(f"  {member}: {len(keys)} issues: {', '.join(keys)}")


if __name__ == "__main__":
    main()
