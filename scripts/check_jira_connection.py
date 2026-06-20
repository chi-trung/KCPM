#!/usr/bin/env python3
"""
check_jira_connection.py
------------------------
Quick connectivity check for Jira API.
Run locally to verify credentials before first CI run.

Usage:
  JIRA_BASE_URL=JIRA_BASE_URL \
  JIRA_API_EMAIL=your.email@gmail.com \
  JIRA_API_TOKEN=your_token \
  python3 scripts/check_jira_connection.py
"""
import json
import os
import sys
import urllib.request
import urllib.error
from base64 import b64encode


JIRA_BASE  = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
JIRA_EMAIL = os.environ.get("JIRA_API_EMAIL", "") or os.environ.get("JIRA_EMAIL", "")
JIRA_TOKEN = os.environ.get("JIRA_API_TOKEN", "")

if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
    print("❌ Missing required env vars: JIRA_BASE_URL, JIRA_API_EMAIL, JIRA_API_TOKEN")
    sys.exit(1)

creds = b64encode(f"{JIRA_EMAIL}:{JIRA_TOKEN}".encode()).decode()
headers = {
    "Authorization": f"Basic {creds}",
    "Accept": "application/json",
}


def check(path: str, label: str) -> bool:
    url = f"{JIRA_BASE}/rest/api/3/{path}"
    try:
        req = urllib.request.Request(url, headers=headers)
        with urllib.request.urlopen(req, timeout=10) as resp:
            data = json.loads(resp.read())
            print(f"✅ {label}: OK")
            return True
    except urllib.error.HTTPError as e:
        body = e.read().decode("utf-8", errors="replace")[:200]
        print(f"❌ {label}: HTTP {e.code}")
        print(f"   URL: {url}")
        print(f"   Response: {body}")
        if e.code == 401:
            print(f"   ⚠️  401 means: email + token combination is WRONG")
            print(f"   ▶  Check 1: Is JIRA_API_EMAIL the same email you use to login at id.atlassian.com?")
            print(f"   ▶  Check 2: Was the token created with THIS email account?")
            print(f"   ▶  Check 3: Is JIRA_BASE_URL in format https://your-org.atlassian.net (no /jira at end)?")
        elif e.code == 403:
            print(f"   ⚠️  403 means: authenticated OK but no permission for this resource")
        return False
    except Exception as e:
        print(f"❌ {label}: {e}")
        return False


print(f"\n🔍 Jira Connectivity Check")
print(f"   Base URL  : {JIRA_BASE}")
print(f"   Email     : {JIRA_EMAIL}")
print(f"   Token     : {'*' * 10}{JIRA_TOKEN[-4:] if len(JIRA_TOKEN) >= 4 else '(short)'}\n")

ok1 = check("myself",           "GET /myself (auth check)")
ok2 = check("project/KIEM",     "GET /project/KIEM")
ok3 = check("issue/KIEM-5",     "GET /issue/KIEM-5 (backend/reports test issue)")
ok4 = check("issue/KIEM-14",    "GET /issue/KIEM-14 (E2E collector issue)")
ok5 = check("issue/KIEM-21",    "GET /issue/KIEM-21 (Postman API issue)")

print()
if all([ok1, ok2, ok3, ok4, ok5]):
    print("🎉 All checks passed! Jira integration is ready.")
else:
    print("⚠️  Some checks failed. Fix credentials before CI runs.")
    sys.exit(1)
