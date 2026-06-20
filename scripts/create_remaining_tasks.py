#!/usr/bin/env python3
"""
create_remaining_tasks.py
--------------------------
Create remaining Sprint 3 tasks and distribute evenly to ALL 4 team members
(excluding Team Leader who already completed his tasks).

Each member gets 2-3 tasks with clear deliverables and branch naming.
"""
import json
import os
import sys
import urllib.request
import urllib.error
from base64 import b64encode

JIRA_BASE = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
JIRA_EMAIL = os.environ.get("JIRA_API_EMAIL", "") or os.environ.get("JIRA_EMAIL", "")
JIRA_TOKEN = os.environ.get("JIRA_API_TOKEN", "")
PROJECT_KEY = os.environ.get("JIRA_PROJECT_KEY", "KIEM")

# Member account IDs on Jira (from previous batch creation)
MEMBERS = {
    "minh_phung": None,      # Will search by name
    "hoang_phung": None,
    "thanh_duy": None,
    "dang": None,
}

MEMBER_SEARCH = {
    "minh_phung": "Minh",
    "hoang_phung": "Hoàng Phụng",
    "thanh_duy": "Duy",
    "dang": "Đăng",
}


def auth_header():
    creds = b64encode(f"{JIRA_EMAIL}:{JIRA_TOKEN}".encode()).decode()
    return {
        "Authorization": f"Basic {creds}",
        "Content-Type": "application/json",
        "Accept": "application/json",
    }


def jira_request(method, path, body=None):
    url = f"{JIRA_BASE}/rest/api/3/{path}"
    data = json.dumps(body).encode("utf-8") if body else None
    req = urllib.request.Request(url, data=data, headers=auth_header(), method=method)
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            raw = resp.read()
            return json.loads(raw) if raw else {"ok": True}
    except urllib.error.HTTPError as e:
        err = e.read().decode("utf-8", errors="replace")[:300]
        print(f"  HTTP {e.code}: {err}")
        return {"error": str(e.code)}
    except Exception as e:
        return {"error": str(e)}


def find_member_ids():
    """Search for each member's accountId by display name."""
    for key, search_name in MEMBER_SEARCH.items():
        resp = jira_request("GET", f"user/search?query={search_name}&maxResults=5")
        if isinstance(resp, list):
            for user in resp:
                display = user.get("displayName", "")
                if search_name.lower() in display.lower():
                    MEMBERS[key] = user["accountId"]
                    print(f"  Found {key}: {display} ({user['accountId'][:12]}...)")
                    break
        if not MEMBERS[key]:
            print(f"  WARNING: Could not find {key} ({search_name})")


# ── New tasks to create ──
# Each member gets 2-3 meaningful tasks with clear deliverables

NEW_TASKS = [
    # ═══ Minh Phụng (already has KIEM-57) — add 2 more ═══
    {
        "summary": "[Sprint-3] Viết báo cáo test Reports Module (BVA + State Transition)",
        "description": (
            "Tạo file docs/TEST_REPORT_REPORTS_MODULE.md chi tiết:\n"
            "1. Liệt kê test cases đã viết cho Reports Module (KIEM-5)\n"
            "2. Kỹ thuật áp dụng: BVA (lat/long boundaries), State Transition (Pending→Accepted→Collected)\n"
            "3. Test data sử dụng\n"
            "4. Kết quả: pass/fail, screenshots\n"
            "5. Link Allure Report\n\n"
            "Branch: feature/KIEM-XX-reports-test-report\n"
            "Commit: docs(KIEM-XX): add Reports module test report"
        ),
        "assignee": "minh_phung",
        "labels": ["sprint-3", "documentation", "reports"],
    },
    {
        "summary": "[Sprint-3] Enhance Postman Collection — thêm test assertions cho File Upload",
        "description": (
            "Mở Postman Collection, thêm test assertions cho:\n"
            "1. File Upload endpoint — validate response structure\n"
            "2. CollectorTask endpoints — status code checks\n"
            "3. Reports endpoints — validate required fields\n\n"
            "Deliverables:\n"
            "- Updated postman collection JSON\n"
            "- Commit message: test(KIEM-XX): enhance Postman assertions\n"
            "- Branch: feature/KIEM-XX-postman-enhancements"
        ),
        "assignee": "minh_phung",
        "labels": ["sprint-3", "api-test", "postman"],
    },

    # ═══ Nguyễn Hoàng Phụng (already has KIEM-58) — add 2 more ═══
    {
        "summary": "[Sprint-3] Viết báo cáo test WasteCategory + Notifications Module",
        "description": (
            "Tạo file docs/TEST_REPORT_CATEGORY_NOTIFICATIONS.md chi tiết:\n"
            "1. WasteCategory tests: EP partitions tested, CRUD operations\n"
            "2. Notifications tests: valid/invalid notification IDs, mark-as-read\n"
            "3. Kỹ thuật áp dụng: EP, Error Guessing\n"
            "4. Screenshot Allure results\n"
            "5. Truy vết: KIEM-6 (Notifications), KIEM-12 (WasteCategory)\n\n"
            "Branch: feature/KIEM-XX-category-notifications-report\n"
            "Commit: docs(KIEM-XX): add Category & Notifications test report"
        ),
        "assignee": "hoang_phung",
        "labels": ["sprint-3", "documentation", "notifications"],
    },
    {
        "summary": "[Sprint-3] Viết test mới cho Role-based Access Control (Security)",
        "description": (
            "Thêm test cases cho KIEM-21 (Security & Role-based Access):\n"
            "1. Admin-only endpoints: tạo user, xóa user, xem analytics\n"
            "2. Enterprise-only: tạo collector, assign task\n"
            "3. Citizen-only: tạo report, tạo complaint\n"
            "4. Cross-role access: citizen access admin endpoint → 403\n\n"
            "File: AdminEnterpriseAuthorizationTests.cs hoặc tạo mới\n"
            "Branch: feature/KIEM-XX-rbac-security-tests\n"
            "Commit: test(KIEM-XX): add RBAC security test cases"
        ),
        "assignee": "hoang_phung",
        "labels": ["sprint-3", "security", "unit-test"],
    },

    # ═══ Thanh Duy — add 3 tasks ═══
    {
        "summary": "[Sprint-3] Viết báo cáo test Complaints + CollectionTask Module",
        "description": (
            "Tạo file docs/TEST_REPORT_COMPLAINTS_COLLECTION.md chi tiết:\n"
            "1. Complaints Module: Decision Table testing (6 combinations)\n"
            "2. CollectionTask Module: State Transition testing\n"
            "3. Kỹ thuật áp dụng: Decision Table (Ch.4), State Transition (Ch.4)\n"
            "4. Bảng Decision Table với conditions + expected results\n"
            "5. State diagram cho CollectionTask lifecycle\n"
            "6. Link Allure Report + Jira issues\n\n"
            "Branch: feature/KIEM-XX-complaints-collection-report\n"
            "Commit: docs(KIEM-XX): add Complaints & CollectionTask test report"
        ),
        "assignee": "thanh_duy",
        "labels": ["sprint-3", "documentation", "complaints"],
    },
    {
        "summary": "[Sprint-3] Viết thêm BVA tests cho CollectionTask — Image upload boundaries",
        "description": (
            "Thêm Boundary Value Analysis tests cho CollectionTask:\n"
            "1. Min images: 0 (reject), 1 (accept)\n"
            "2. Max images: 5 (accept), 6 (reject)\n"
            "3. Empty content validation\n"
            "4. Status transitions: verify invalid transitions are rejected\n\n"
            "File: CollectionTaskDomainTests.cs hoặc tạo mới\n"
            "Branch: feature/KIEM-XX-collection-bva-tests\n"
            "Commit: test(KIEM-XX): add BVA tests for CollectionTask"
        ),
        "assignee": "thanh_duy",
        "labels": ["sprint-3", "bva", "unit-test"],
    },
    {
        "summary": "[Sprint-3] Fix KIEM-22 — Thêm AuditLog cho Complaints operations",
        "description": (
            "KIEM-22 (AuditLog & Error Path Tests) cần bổ sung:\n"
            "1. Audit log khi tạo complaint\n"
            "2. Audit log khi resolve complaint\n"
            "3. Error path: complaint cho report không tồn tại\n"
            "4. Test file: AuditLogAndErrorPathTests.cs\n\n"
            "Branch: feature/KIEM-XX-audit-complaint-tests\n"
            "Commit: test(KIEM-XX): add audit log tests for Complaints"
        ),
        "assignee": "thanh_duy",
        "labels": ["sprint-3", "audit", "unit-test"],
    },

    # ═══ Đăng — add 3 tasks ═══
    {
        "summary": "[Sprint-3] Viết báo cáo test Admin + Analytics + Citizen Module",
        "description": (
            "Tạo file docs/TEST_REPORT_ADMIN_CITIZEN.md chi tiết:\n"
            "1. Admin Module: CRUD user, manage roles\n"
            "2. Analytics Module: dashboard stats, data accuracy\n"
            "3. Citizen Module: profile management, report history\n"
            "4. Kỹ thuật áp dụng: EP, Error Guessing\n"
            "5. Screenshot Allure results cho mỗi module\n\n"
            "Branch: feature/KIEM-XX-admin-citizen-report\n"
            "Commit: docs(KIEM-XX): add Admin & Citizen test report"
        ),
        "assignee": "dang",
        "labels": ["sprint-3", "documentation", "admin"],
    },
    {
        "summary": "[Sprint-3] Viết Manual Test Cases Excel cho Search & Pagination",
        "description": (
            "Tạo file docs/MANUAL_TEST_SEARCH_PAGINATION.xlsx hoặc .md:\n"
            "1. Search by keyword: valid, empty, special characters\n"
            "2. Pagination: page 1, last page, page 0 (invalid), negative page\n"
            "3. Filters: by status, by category, by date range\n"
            "4. Sort: by date ASC/DESC, by status\n"
            "5. Kỹ thuật: BVA (page boundaries), EP (valid/invalid search)\n\n"
            "Branch: feature/KIEM-XX-manual-search-pagination\n"
            "Commit: docs(KIEM-XX): add manual test cases for Search & Pagination"
        ),
        "assignee": "dang",
        "labels": ["sprint-3", "manual-test", "search"],
    },
    {
        "summary": "[Sprint-3] Viết thêm Integration Tests cho Admin Analytics endpoint",
        "description": (
            "Thêm integration tests cho Analytics dashboard:\n"
            "1. GET /api/admin/analytics — validate response structure\n"
            "2. Registered users count accuracy\n"
            "3. Reports by status breakdown\n"
            "4. Error: non-admin access → 403 Forbidden\n\n"
            "File: AnalyticsModuleTests.cs hoặc AnalyticsApiIntegrationTests.cs\n"
            "Branch: feature/KIEM-XX-analytics-integration-tests\n"
            "Commit: test(KIEM-XX): add integration tests for Analytics"
        ),
        "assignee": "dang",
        "labels": ["sprint-3", "integration-test", "analytics"],
    },
]


def create_issue(task):
    """Create a single Jira issue with labels and assignment."""
    assignee_key = task["assignee"]
    account_id = MEMBERS.get(assignee_key)

    fields = {
        "project": {"key": PROJECT_KEY},
        "summary": task["summary"],
        "description": {
            "type": "doc",
            "version": 1,
            "content": [
                {
                    "type": "paragraph",
                    "content": [{"type": "text", "text": task["description"]}]
                }
            ]
        },
        "issuetype": {"name": "Task"},
        "labels": task.get("labels", []),
    }

    if account_id:
        fields["assignee"] = {"accountId": account_id}

    body = {"fields": fields}
    result = jira_request("POST", "issue", body)

    if "key" in result:
        print(f"  CREATED: {result['key']} — {task['summary']}")
        return result["key"]
    else:
        print(f"  FAILED: {task['summary']} — {result}")
        return None


def main():
    if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
        print("Missing JIRA credentials.")
        sys.exit(1)

    # Verify auth
    me = jira_request("GET", "myself")
    if "error" in me:
        print(f"Auth failed: {me}")
        sys.exit(1)
    print(f"Authenticated as: {me.get('displayName', 'unknown')}")

    # Find member account IDs
    print("\nSearching for team members...")
    find_member_ids()

    # Create issues
    print(f"\nCreating {len(NEW_TASKS)} new tasks...")
    created = []
    for task in NEW_TASKS:
        key = create_issue(task)
        if key:
            created.append((key, task["summary"], task["assignee"]))

    # Summary
    print(f"\n{'='*60}")
    print(f"Created {len(created)}/{len(NEW_TASKS)} tasks")
    print(f"\nDistribution:")
    from collections import Counter
    dist = Counter(t[2] for t in created)
    for member, count in dist.items():
        print(f"  {member}: {count} new tasks")
    
    print(f"\nIssue keys:")
    for key, summary, assignee in created:
        print(f"  {key}: {summary} → {assignee}")


if __name__ == "__main__":
    main()
