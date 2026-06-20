#!/usr/bin/env python3
"""
create_jira_issues.py — Batch-create Jira issues from jira.md
Reads sprint/task definitions and creates them via Jira REST API.

Environment variables required:
  JIRA_BASE_URL   — e.g. JIRA_BASE_URL
  JIRA_API_EMAIL  — Jira account email
  JIRA_API_TOKEN  — Jira API token
  JIRA_PROJECT_KEY — e.g. KIEM
  EPIC_ISSUE_KEY  — (optional) existing Epic key
"""
import json
import os
import re
import sys
import requests

BASE_URL = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
EMAIL = os.environ.get("JIRA_API_EMAIL", "")
TOKEN = os.environ.get("JIRA_API_TOKEN", "")
PROJECT_KEY = os.environ.get("JIRA_PROJECT_KEY", "KIEM")
EPIC_KEY = os.environ.get("EPIC_ISSUE_KEY", "")

AUTH = (EMAIL, TOKEN)
HEADERS = {"Content-Type": "application/json", "Accept": "application/json"}

# ── Member email → accountId mapping (populated at runtime) ──
MEMBER_MAP = {}  # will be filled by lookup_members()

# ── Sprint definitions ──────────────────────────────────────────────
SPRINTS = [
    {
        "name": "Sprint 1: Test Planning & Infrastructure",
        "tasks": [
            {
                "summary": "[Sprint-1] Thiet lap CI/CD Pipeline voi GitHub Actions (9 workflows)",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "High",
                "labels": ["sprint-1", "ci-cd", "infrastructure"],
                "description": (
                    "Thiet lap 9 GitHub Actions workflows:\n"
                    "1. backend-tests.yml — 455 xUnit tests\n"
                    "2. frontend-e2e.yml — 19 E2E scenarios\n"
                    "3. sonar.yml — SonarCloud analysis\n"
                    "4. deploy-server.yml — CI/CD deploy\n"
                    "5. allure-gh-pages.yml — Allure report\n"
                    "6. postman-smoke.yml — API tests\n"
                    "7. health-check.yml — Health check\n"
                    "8. jira-key-enforcement.yml — PR validation\n"
                    "9. create-jira-issues.yml — Jira automation\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-1] Thiet lap Test Plan & Testing Strategy",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "High",
                "labels": ["sprint-1", "documentation", "test-plan"],
                "description": (
                    "Tao Test Plan va Testing Strategy:\n"
                    "- Scope, levels, types\n"
                    "- Entry/Exit criteria\n"
                    "- Tools: xUnit, CodeceptJS, Postman, SonarCloud, Allure\n\n"
                    "Files: docs/TEST_PLAN.md, docs/TESTING_STRATEGY.md\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-1] Deploy Production (Render + Vercel + Aiven)",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "High",
                "labels": ["sprint-1", "deployment"],
                "description": (
                    "Deploy full-stack:\n"
                    "- Backend: Render.com (.NET 8 Docker)\n"
                    "- Frontend: Vercel (Next.js)\n"
                    "- DB: Aiven MySQL\n\n"
                    "URLs:\n"
                    "- Frontend: https://kcpm.vercel.app\n"
                    "- Backend: https://kcpm-backend.onrender.com\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-1] Thiet lap Jira Project & Traceability Matrix",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "Medium",
                "labels": ["sprint-1", "jira", "traceability"],
                "description": (
                    "Thiet lap Jira KIEM project:\n"
                    "- Kanban board\n"
                    "- Traceability Matrix\n"
                    "- CI auto-log results len Jira\n\n"
                    "File: docs/TRACEABILITY_MATRIX.md\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-1] Thiet lap Postman Collection cho API Testing",
                "assignee_name": "Minh Phung",
                "priority": "Medium",
                "labels": ["sprint-1", "api-testing", "postman"],
                "description": (
                    "Tao Postman Collection:\n"
                    "- 10 folders, 74 requests, 128 assertions\n"
                    "- Auto-login, environment variables\n"
                    "- Newman CI integration\n\n"
                    "Status: DONE"
                ),
            },
        ],
    },
    {
        "name": "Sprint 2: Test Development & Execution",
        "tasks": [
            {
                "summary": "[Sprint-2] Unit Tests — Auth Module (EP + Error Guessing)",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "High",
                "labels": ["sprint-2", "unit-test", "auth"],
                "description": (
                    "xUnit tests cho Auth module:\n"
                    "- EP: valid/invalid email, password\n"
                    "- Error Guessing: JWT expired, malformed, null\n\n"
                    "Files: AuthControllerTests.cs, JwtServiceTests.cs, JwtBearerIntegrationTests.cs\n"
                    "Jira: KIEM-4\n"
                    "Branch: feature/KIEM-4-auth-tests\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] Unit Tests — Reports Module (BVA + State Transition)",
                "assignee_name": "Minh Phung",
                "priority": "High",
                "labels": ["sprint-2", "unit-test", "reports", "bva"],
                "description": (
                    "xUnit tests cho Reports module:\n"
                    "- BVA: images count (0,1,5,6), lat/long\n"
                    "- State Transition: report lifecycle\n\n"
                    "Files: CreateReportCommandHandlerTests.cs, WasteReportTests.cs\n"
                    "Jira: KIEM-5\n"
                    "Branch: feature/KIEM-5-reports-tests\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] Unit Tests — Notifications Module",
                "assignee_name": "Nguyen Hoang Phung",
                "priority": "High",
                "labels": ["sprint-2", "unit-test", "notifications"],
                "description": (
                    "xUnit tests cho Notifications:\n"
                    "- EP: valid/invalid IDs\n"
                    "- Error Guessing: 404, unauthorized\n\n"
                    "Files: NotificationServiceTests.cs, NotificationControllerTests.cs\n"
                    "Jira: KIEM-6\n"
                    "Branch: feature/KIEM-6-notification-tests\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] Unit Tests — Complaints Module (Decision Table)",
                "assignee_name": "Thanh Duy",
                "priority": "High",
                "labels": ["sprint-2", "unit-test", "complaints", "decision-table"],
                "description": (
                    "xUnit tests cho Complaints:\n"
                    "- Decision Table: 6 combinations\n"
                    "- Error Guessing: empty, null\n\n"
                    "Files: CreateComplaintCommandHandlerTests.cs (DT-01..06)\n"
                    "Jira: KIEM-7\n"
                    "Branch: feature/KIEM-7-complaint-tests\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] Unit Tests — Admin + Analytics Module",
                "assignee_name": "Dang",
                "priority": "High",
                "labels": ["sprint-2", "unit-test", "admin", "analytics"],
                "description": (
                    "xUnit tests cho Admin + Analytics:\n"
                    "- EP: valid/invalid admin operations\n"
                    "- Error Guessing: unauthorized, missing data\n\n"
                    "Files: AdminModuleTests.cs, AnalyticsModuleTests.cs\n"
                    "Jira: KIEM-8, KIEM-9\n"
                    "Branch: feature/KIEM-8-admin-analytics-tests\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] E2E Tests (CodeceptJS + Playwright) — 19 Scenarios",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "High",
                "labels": ["sprint-2", "e2e", "codeceptjs"],
                "description": (
                    "5 E2E test files, 19 scenarios:\n"
                    "1. smoke_test.js\n"
                    "2. citizen_report_test.js\n"
                    "3. enterprise_assign_test.js\n"
                    "4. collector_task_test.js\n"
                    "5. citizen_complaint_test.js\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] Unit Tests — WasteCategory + Security",
                "assignee_name": "Nguyen Hoang Phung",
                "priority": "Medium",
                "labels": ["sprint-2", "unit-test", "category", "security"],
                "description": (
                    "xUnit tests cho WasteCategory + Security:\n"
                    "- Role-based access tests\n"
                    "- Controller + Repository tests\n\n"
                    "Jira: KIEM-12, KIEM-21\n"
                    "Branch: feature/KIEM-12-category-security-tests\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] Unit Tests — CollectorTask + File Uploads",
                "assignee_name": "Minh Phung",
                "priority": "Medium",
                "labels": ["sprint-2", "unit-test", "collector"],
                "description": (
                    "xUnit tests cho CollectorTask + File Uploads:\n"
                    "- Controller + Handler tests\n"
                    "- File upload edge cases\n\n"
                    "Jira: KIEM-15, KIEM-20\n"
                    "Branch: feature/KIEM-15-collector-file-tests\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] Unit Tests — CollectionTask + Audit",
                "assignee_name": "Thanh Duy",
                "priority": "Medium",
                "labels": ["sprint-2", "unit-test", "collection-task"],
                "description": (
                    "xUnit tests cho CollectionTask + AuditLog:\n"
                    "- Domain logic, state transitions\n"
                    "- Error path coverage\n\n"
                    "Jira: KIEM-10, KIEM-18, KIEM-22\n"
                    "Branch: feature/KIEM-18-collection-audit-tests\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-2] Unit Tests — Citizen + Search/Pagination",
                "assignee_name": "Dang",
                "priority": "Medium",
                "labels": ["sprint-2", "unit-test", "citizen", "search"],
                "description": (
                    "xUnit tests cho Citizen + Search:\n"
                    "- Pagination edge cases\n"
                    "- Filter combinations\n\n"
                    "Jira: KIEM-13, KIEM-23\n"
                    "Branch: feature/KIEM-13-citizen-search-tests\n\n"
                    "Status: DONE"
                ),
            },
        ],
    },
    {
        "name": "Sprint 3: Quality Assurance & Final Report",
        "tasks": [
            {
                "summary": "[Sprint-3] Fix SonarCloud Quality Gate (16 → 0 vulnerabilities)",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "Highest",
                "labels": ["sprint-3", "sonarcloud", "security"],
                "description": (
                    "Fix SonarCloud Quality Gate:\n"
                    "- Xoa hardcoded secrets (appsettings.json)\n"
                    "- Thay hardcoded password (CreateUserCommand.cs)\n"
                    "- Path sanitization (Python scripts)\n"
                    "- Update exclusions (sonar.yml)\n\n"
                    "Result: 0 open vulnerabilities\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-3][BUG] Fix KIEM-29 — Max 5 Images Validation (BVA)",
                "assignee_name": "Thanh Duy",
                "priority": "High",
                "labels": ["sprint-3", "bug-fix", "bva"],
                "description": (
                    "Bug: API cho phep > 5 hinh khi tao report.\n"
                    "Fix: Them validation if (Images.Count > 5)\n"
                    "BVA: 0→reject, 1→ok, 5→ok, 6→reject\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-3][BUG] Fix KIEM-28 — Include taskId in Accept Response",
                "assignee_name": "Minh Phung",
                "priority": "Medium",
                "labels": ["sprint-3", "bug-fix"],
                "description": (
                    "Bug: PUT /api/reports/{id}/accept response thieu taskId.\n"
                    "Expected: Response includes taskId.\n\n"
                    "Branch: bugfix/KIEM-28-include-taskId\n\n"
                    "Status: TO DO"
                ),
            },
            {
                "summary": "[Sprint-3][BUG] Fix E2E Allure Suite Missing from Report",
                "assignee_name": "Nguyen Hoang Phung",
                "priority": "Medium",
                "labels": ["sprint-3", "e2e", "allure", "bug-fix"],
                "description": (
                    "Bug: Allure Report chi hien 2 suites, thieu E2E.\n"
                    "Fix: Thay start script -> next start\n"
                    "Branch: bugfix/KIEM-e2e-allure-fix\n\n"
                    "Status: IN PROGRESS"
                ),
            },
            {
                "summary": "[Sprint-3] Tao Final Report & Deployment Guide",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "High",
                "labels": ["sprint-3", "documentation"],
                "description": (
                    "Tao va cap nhat tai lieu:\n"
                    "- FINAL_REPORT.md (451 tests, 11 workflows)\n"
                    "- DEPLOYMENT_GUIDE.md (892 dong)\n"
                    "- CI_CD_WORKFLOWS.md\n"
                    "- DEMO.md (696 dong)\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-3] Viet Manual Test Cases (Excel - 68 TCs)",
                "assignee_name": "Dang",
                "priority": "Medium",
                "labels": ["sprint-3", "manual-testing"],
                "description": (
                    "Tao Excel UnitestKCPM.xlsx:\n"
                    "- 68 test cases, 13 functions\n"
                    "- EP, BVA, DT, ST techniques\n"
                    "- Screenshots evidence\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-3] Cap nhat Traceability Matrix",
                "assignee_name": "Nguyen Hoang Phung",
                "priority": "Medium",
                "labels": ["sprint-3", "documentation", "traceability"],
                "description": (
                    "Cap nhat Traceability Matrix:\n"
                    "- Tat ca Jira issues\n"
                    "- Bug status updates\n"
                    "- Test technique mapping (Ch.4)\n\n"
                    "Status: DONE"
                ),
            },
            {
                "summary": "[Sprint-3] Chuan bi Demo cho Thay",
                "assignee_name": "Nguyen Chi Trung",
                "priority": "High",
                "labels": ["sprint-3", "demo", "presentation"],
                "description": (
                    "Chuan bi demo:\n"
                    "1. Kien truc Client-Server\n"
                    "2. CI/CD Pipeline (9 workflows)\n"
                    "3. Test results (451 xUnit, 19 E2E, 74 Postman)\n"
                    "4. Allure Report\n"
                    "5. SonarCloud Quality Gate\n"
                    "6. Jira Bug tracking\n"
                    "7. Production app\n\n"
                    "File: docs/DEMO.md\n\n"
                    "Status: DONE"
                ),
            },
        ],
    },
]


def lookup_members():
    """Try to find Jira accountIds for each team member."""
    global MEMBER_MAP
    search_names = [
        "Nguyen Chi Trung", "Minh Phung", "Nguyen Hoang Phung",
        "Thanh Duy", "Dang",
    ]
    for name in search_names:
        try:
            r = requests.get(
                f"{BASE_URL}/rest/api/3/user/search",
                params={"query": name.split()[-1], "maxResults": 10},
                auth=AUTH, headers=HEADERS, timeout=10,
            )
            if r.ok:
                users = r.json()
                for u in users:
                    display = u.get("displayName", "")
                    if any(part.lower() in display.lower() for part in name.split()):
                        MEMBER_MAP[name] = u["accountId"]
                        print(f"  ✅ Mapped '{name}' → {u['accountId']} ({display})")
                        break
        except Exception as e:
            print(f"  ⚠️  Could not lookup '{name}': {e}")

    print(f"\nMember map ({len(MEMBER_MAP)} found): {list(MEMBER_MAP.keys())}")


def create_issue(summary, description, labels, priority, assignee_name=None):
    """Create a single Jira issue."""
    fields = {
        "project": {"key": PROJECT_KEY},
        "summary": summary,
        "description": {
            "version": 1,
            "type": "doc",
            "content": [
                {
                    "type": "paragraph",
                    "content": [{"type": "text", "text": description}],
                }
            ],
        },
        "issuetype": {"name": "Task"},
        "priority": {"name": priority},
        "labels": labels,
    }

    # Try to assign if we have the accountId
    if assignee_name and assignee_name in MEMBER_MAP:
        fields["assignee"] = {"accountId": MEMBER_MAP[assignee_name]}

    payload = json.dumps({"fields": fields})
    r = requests.post(
        f"{BASE_URL}/rest/api/3/issue",
        data=payload, auth=AUTH, headers=HEADERS, timeout=15,
    )

    if r.ok:
        key = r.json().get("key", "?")
        print(f"  ✅ Created {key}: {summary}")
        return key
    else:
        print(f"  ❌ Failed: {summary}")
        print(f"     Status: {r.status_code}, Body: {r.text[:200]}")
        return None


def transition_to_done(issue_key):
    """Move issue to Done status."""
    try:
        # Get available transitions
        r = requests.get(
            f"{BASE_URL}/rest/api/3/issue/{issue_key}/transitions",
            auth=AUTH, headers=HEADERS, timeout=10,
        )
        if r.ok:
            transitions = r.json().get("transitions", [])
            done_id = None
            for t in transitions:
                if t["name"].lower() in ("done", "hoàn thành"):
                    done_id = t["id"]
                    break
            if done_id:
                requests.post(
                    f"{BASE_URL}/rest/api/3/issue/{issue_key}/transitions",
                    data=json.dumps({"transition": {"id": done_id}}),
                    auth=AUTH, headers=HEADERS, timeout=10,
                )
                print(f"     → Moved {issue_key} to Done")
    except Exception as e:
        print(f"     ⚠️  Could not transition {issue_key}: {e}")


def main():
    if not BASE_URL or not EMAIL or not TOKEN:
        print("❌ Missing JIRA_BASE_URL, JIRA_API_EMAIL, or JIRA_API_TOKEN")
        sys.exit(1)

    print(f"🔗 Jira: {BASE_URL}")
    print(f"📋 Project: {PROJECT_KEY}\n")

    # Step 1: Lookup member accountIds
    print("👥 Looking up team members...")
    lookup_members()

    # Step 2: Create issues for each sprint
    created = []
    for sprint in SPRINTS:
        print(f"\n{'='*60}")
        print(f"📌 {sprint['name']}")
        print(f"{'='*60}")

        for task in sprint["tasks"]:
            key = create_issue(
                summary=task["summary"],
                description=task["description"],
                labels=task.get("labels", []),
                priority=task.get("priority", "Medium"),
                assignee_name=task.get("assignee_name"),
            )
            if key:
                created.append(key)
                # Move completed tasks to Done
                if "Status: DONE" in task["description"]:
                    transition_to_done(key)

    # Summary
    print(f"\n{'='*60}")
    print(f"✅ Created {len(created)} issues: {', '.join(created)}")
    print(f"{'='*60}")


if __name__ == "__main__":
    main()
