#!/usr/bin/env python3
"""
jira_log_sprint_evidence.py
----------------------------
Post detailed evidence comments to ALL Sprint Jira issues (KIEM-40 to KIEM-62).
Each comment includes: commits, CI runs, test results, file links, Allure report links.

Run via: .github/workflows/create-jira-issues.yml (or manually)

Environment variables:
  JIRA_BASE_URL, JIRA_API_EMAIL, JIRA_API_TOKEN, JIRA_PROJECT_KEY
"""
import json
import os
import sys
import urllib.request
import urllib.error
from base64 import b64encode
from datetime import datetime, timezone

# Fix encoding
if sys.stdout.encoding and sys.stdout.encoding.lower() not in ("utf-8", "utf8"):
    import io
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

JIRA_BASE = os.environ.get("JIRA_BASE_URL", "").rstrip("/")
JIRA_EMAIL = os.environ.get("JIRA_API_EMAIL", "") or os.environ.get("JIRA_EMAIL", "")
JIRA_TOKEN = os.environ.get("JIRA_API_TOKEN", "")

REPO = "chi-trung/KCPM"
REPO_URL = f"https://github.com/{REPO}"
ALLURE_URL = "https://chi-trung.github.io/KCPM/report-main/"
JIRA_BOARD = "https://ut-team-36.atlassian.net/jira/software/projects/KIEM/boards/3"
TIMESTAMP = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")


def auth_header():
    creds = b64encode(f"{JIRA_EMAIL}:{JIRA_TOKEN}".encode()).decode()
    return {
        "Authorization": f"Basic {creds}",
        "Content-Type": "application/json",
        "Accept": "application/json",
    }


def jira_post(path, body):
    url = f"{JIRA_BASE}/rest/api/3/{path}"
    data = json.dumps(body).encode("utf-8")
    req = urllib.request.Request(url, data=data, headers=auth_header(), method="POST")
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            raw = resp.read()
            return json.loads(raw) if raw else {"ok": True}
    except urllib.error.HTTPError as e:
        print(f"  HTTP {e.code}: {e.read().decode('utf-8', errors='replace')[:200]}")
        return {"error": str(e.code)}
    except Exception as e:
        print(f"  Error: {e}")
        return {"error": str(e)}


def jira_get(path):
    url = f"{JIRA_BASE}/rest/api/3/{path}"
    req = urllib.request.Request(url, headers=auth_header(), method="GET")
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            return json.loads(resp.read())
    except Exception as e:
        return {"error": str(e)}


def text(t): return {"type": "text", "text": t}
def bold(t): return {"type": "text", "text": t, "marks": [{"type": "strong"}]}
def link(t, url): return {"type": "text", "text": t, "marks": [{"type": "link", "attrs": {"href": url}}]}
def para(*items): return {"type": "paragraph", "content": list(items)}
def heading(t, level=3): return {"type": "heading", "attrs": {"level": level}, "content": [text(t)]}
def bullet(*items): return {"type": "bulletList", "content": [{"type": "listItem", "content": [para(*i) if isinstance(i, (list, tuple)) else para(i)] } for i in items]}
def rule(): return {"type": "rule"}
def code_block(t): return {"type": "codeBlock", "attrs": {"language": "text"}, "content": [text(t)]}


def build_evidence_doc(title, evidence_items, commits=None, links=None):
    """Build an ADF document with structured evidence."""
    content = [heading(f"[EVIDENCE] {title}")]

    # Evidence items as bullet list
    if evidence_items:
        content.append(bullet(*evidence_items))

    # Commits section
    if commits:
        content.append(rule())
        content.append(heading("Commits", 4))
        commit_lines = "\n".join(commits)
        content.append(code_block(commit_lines))

    # Links section
    if links:
        content.append(rule())
        content.append(heading("Links", 4))
        link_items = []
        for label, url in links:
            link_items.append([link(label, url)])
        content.append(bullet(*link_items))

    # Timestamp
    content.append(rule())
    content.append(para(text(f"Logged: {TIMESTAMP} | Automated by CI")))

    return {"body": {"type": "doc", "version": 1, "content": content}}


# ── Evidence definitions for each Sprint issue ──

SPRINT_EVIDENCE = {
    # ═══════════════ SPRINT 1 ═══════════════
    "KIEM-40": {
        "title": "Sprint-1: CI/CD Pipeline (9 GitHub Actions Workflows)",
        "evidence": [
            [bold("9 workflows"), text(" configured and running on every push to main")],
            [bold("Workflows: "), text("backend-tests, frontend-e2e, sonar, deploy-server, allure-gh-pages, postman-smoke, health-check, jira-key-enforcement, create-jira-issues")],
            [bold("Auto-trigger: "), text("push to main + PR to main")],
            [bold("Auto-log Jira: "), text("CI posts test results as comments on 21 KIEM issues")],
            [bold("Allure Report: "), text("3 suites merged (Backend + API + E2E) on GitHub Pages")],
        ],
        "commits": [
            "4d08164 feat(jira): add sprint plan, team workflow guide",
            "aa4acba feat(KIEM-40): add Sprint tasks to CI Jira auto-log ISSUE_MAP",
            "579ef43 chore(KIEM-43): workflow cleanup + docs consolidation (11→9)",
        ],
        "links": [
            ("GitHub Actions (9 workflows)", f"{REPO_URL}/actions"),
            ("backend-tests.yml", f"{REPO_URL}/blob/main/.github/workflows/backend-tests.yml"),
            ("frontend-e2e.yml", f"{REPO_URL}/blob/main/.github/workflows/frontend-e2e.yml"),
            ("sonar.yml", f"{REPO_URL}/blob/main/.github/workflows/sonar.yml"),
            ("deploy-server.yml", f"{REPO_URL}/blob/main/.github/workflows/deploy-server.yml"),
            ("allure-gh-pages.yml", f"{REPO_URL}/blob/main/.github/workflows/allure-gh-pages.yml"),
            ("postman-smoke.yml", f"{REPO_URL}/blob/main/.github/workflows/postman-smoke.yml"),
            ("CI_CD_WORKFLOWS.md", f"{REPO_URL}/blob/main/docs/CI_CD_WORKFLOWS.md"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-41": {
        "title": "Sprint-1: Test Plan & Testing Strategy v3.0",
        "evidence": [
            [bold("TEST_PLAN.md v3.0"), text(" — 238 lines, covering Ch.4-7 textbook")],
            [bold("TESTING_STRATEGY.md"), text(" — Testing approach document")],
            [bold("Metrics: "), text("451 xUnit + 19 E2E + 74 Postman = 544 automated tests")],
            [bold("Techniques: "), text("EP, BVA, State Transition, Decision Table, Error Guessing, White-box, Static Analysis")],
            [bold("Exit criteria: "), text("100% pass rate, 0 SonarCloud vulnerabilities, 36 Jira issues covered")],
        ],
        "commits": [
            "c448f99 docs(KIEM-41): update Test Plan v3.0 with verified metrics",
            "79b847f docs(KIEM-41): update Test Plan v3.0 with verified metrics",
        ],
        "links": [
            ("TEST_PLAN.md", f"{REPO_URL}/blob/main/docs/TEST_PLAN.md"),
            ("TESTING_STRATEGY.md", f"{REPO_URL}/blob/main/docs/TESTING_STRATEGY.md"),
            ("PR #50", f"{REPO_URL}/pull/50"),
        ],
    },
    "KIEM-42": {
        "title": "Sprint-1: Deploy Production (Render + Vercel + Aiven)",
        "evidence": [
            [bold("Backend: "), text("https://kcpm-backend.onrender.com (Render.com, Docker .NET 8)")],
            [bold("Frontend: "), text("https://kcpm.vercel.app (Vercel, Next.js)")],
            [bold("Database: "), text("Aiven MySQL (free tier, auto-backup)")],
            [bold("Health check: "), text("/api/health returns 200 OK")],
            [bold("Login verified: "), text("admin@gmail.com (Admin), nguyenvana@gmail.com (Citizen)")],
            [bold("Swagger: "), text("https://kcpm-backend.onrender.com/swagger")],
            [bold("Seed data: "), text("5 categories, 8 user accounts, enterprise/collector profiles")],
        ],
        "commits": [
            "242954d feat(KIEM-42,KIEM-43): update Final Report v6.0 & Traceability Matrix",
            "bf05e20 fix(production): use valid development JWT secret key",
        ],
        "links": [
            ("Backend API", "https://kcpm-backend.onrender.com"),
            ("Frontend App", "https://kcpm.vercel.app"),
            ("Swagger API Docs", "https://kcpm-backend.onrender.com/swagger"),
            ("render.yaml", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/render.yaml"),
            ("DEPLOYMENT_GUIDE.md", f"{REPO_URL}/blob/main/docs/DEPLOYMENT_GUIDE.md"),
            ("PR #51", f"{REPO_URL}/pull/51"),
        ],
    },
    "KIEM-43": {
        "title": "Sprint-1: Jira Project & Traceability Matrix",
        "evidence": [
            [bold("Jira project: "), text("KIEM (KiemChungPhanMem), Kanban board")],
            [bold("Total issues: "), text("36 issues (KIEM-4 to KIEM-62)")],
            [bold("3 Sprints: "), text("Sprint 1 (5 tasks), Sprint 2 (10 tasks), Sprint 3 (8 tasks)")],
            [bold("Traceability Matrix: "), text("145 lines, mapping Requirement -> Jira -> Test Case -> CI Evidence")],
            [bold("Auto-log: "), text("CI posts test results to 21 Jira issues after every run")],
            [bold("5 members assigned: "), text("Nguyen Chi Trung, Minh Phung, Nguyen Hoang Phung, Thanh Duy, Dang")],
        ],
        "commits": [
            "242954d feat(KIEM-42,KIEM-43): update Final Report v6.0 & Traceability Matrix",
        ],
        "links": [
            ("Jira Board", JIRA_BOARD),
            ("TRACEABILITY_MATRIX.md", f"{REPO_URL}/blob/main/docs/TRACEABILITY_MATRIX.md"),
            ("jira_log_test_execution.py", f"{REPO_URL}/blob/main/scripts/jira_log_test_execution.py"),
        ],
    },
    "KIEM-44": {
        "title": "Sprint-1: Postman Collection (74 requests, 128 assertions)",
        "evidence": [
            [bold("Collection: "), text("WastePlatform API - Professional QA Suite")],
            [bold("10 folders: "), text("Auth, Admin, WasteCategory, Analytics, Reports, Citizen, Collector, CollectorTask, Notifications, Complaints")],
            [bold("74 requests"), text(" with 128 test assertions")],
            [bold("Newman CI: "), text("postman-smoke.yml runs on every push, 0 failures")],
            [bold("Auto-login: "), text("Pre-request scripts handle JWT token management")],
        ],
        "commits": [
            "Postman collection created in Waste-Recycling-Platform/postman/",
        ],
        "links": [
            ("Postman Collection JSON", f"{REPO_URL}/tree/main/Waste-Recycling-Platform/postman"),
            ("postman-smoke.yml", f"{REPO_URL}/blob/main/.github/workflows/postman-smoke.yml"),
            ("Allure API Test Suite", f"{ALLURE_URL}#suites"),
        ],
    },

    # ═══════════════ SPRINT 2 ═══════════════
    "KIEM-45": {
        "title": "Sprint-2: Auth Module Unit Tests (EP + Error Guessing)",
        "evidence": [
            [bold("9 test methods"), text(" in AuthControllerTests.cs")],
            [bold("EP: "), text("valid/invalid email, Citizen/Collector/Enterprise role partitions")],
            [bold("Error Guessing: "), text("duplicate email, wrong password, non-existent email, no auth context")],
            [bold("JwtServiceTests.cs: "), text("Token generation, validation, claims extraction")],
            [bold("JwtBearerIntegrationTests.cs: "), text("Full JWT middleware pipeline tests")],
            [bold("All pass: "), text("451/451 on CI")],
        ],
        "commits": [
            "f32804d test(KIEM-45): add 4 EP + Error Guessing tests for Auth module",
            "49d420a test(KIEM-45): add 4 EP + Error Guessing tests for Auth module",
        ],
        "links": [
            ("AuthControllerTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Controllers/AuthControllerTests.cs"),
            ("JwtServiceTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Services/JwtServiceTests.cs"),
            ("JwtBearerIntegrationTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Integration/JwtBearerIntegrationTests.cs"),
            ("PR #52", f"{REPO_URL}/pull/52"),
            ("Allure - Auth Suite", f"{ALLURE_URL}#suites"),
        ],
    },
    "KIEM-46": {
        "title": "Sprint-2: Reports Module Tests (BVA + State Transition)",
        "evidence": [
            [bold("BVA: "), text("images count boundaries (0, 1, 5, 6), lat/long (-90..90, -180..180)")],
            [bold("State Transition: "), text("Pending -> Accepted -> Assigned -> Collected/Rejected")],
            [bold("Test files: "), text("CreateReportCommandHandlerTests.cs, WasteReportTests.cs, AcceptReportCommandHandlerTests.cs")],
            [bold("30+ test methods"), text(" covering lifecycle transitions and boundary values")],
        ],
        "commits": ["Reports tests committed in Sprint 2"],
        "links": [
            ("CreateReportCommandHandlerTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Application/Reports/CreateReportCommandHandlerTests.cs"),
            ("WasteReportTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Domain/WasteReportTests.cs"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-47": {
        "title": "Sprint-2: Notifications Module Tests",
        "evidence": [
            [bold("EP: "), text("valid/invalid notification IDs")],
            [bold("Error Guessing: "), text("mark-as-read for 404, unauthorized access")],
            [bold("3 test files: "), text("NotificationServiceTests.cs, NotificationControllerTests.cs, NotificationRepositoryTests.cs")],
            [bold("15+ test methods"), text(" covering Service + Controller + Repository layers")],
        ],
        "commits": ["Notification tests committed in Sprint 2"],
        "links": [
            ("NotificationServiceTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Application/Notifications/NotificationServiceTests.cs"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-48": {
        "title": "Sprint-2: Complaints Module Tests (Decision Table)",
        "evidence": [
            [bold("Decision Table: "), text("6 combinations of Content x Report Status x User Role")],
            [bold("Error Guessing: "), text("empty description, null enterprise, invalid status")],
            [bold("Test files: "), text("CreateComplaintCommandHandlerTests.cs (DT-01..06), RejectComplaintCommandHandlerTests.cs, ResolveComplaintCommandHandlerTests.cs")],
        ],
        "commits": ["Complaint tests committed in Sprint 2"],
        "links": [
            ("CreateComplaintCommandHandlerTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Application/Complaints/CreateComplaintCommandHandlerTests.cs"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-49": {
        "title": "Sprint-2: Admin + Analytics Module Tests",
        "evidence": [
            [bold("EP: "), text("valid/invalid admin operations, analytics queries")],
            [bold("Integration tests: "), text("AdminApiIntegrationTests.cs, AnalyticsApiIntegrationTests.cs")],
            [bold("25+ test methods"), text(" for Admin + Analytics combined")],
        ],
        "commits": ["Admin + Analytics tests committed in Sprint 2"],
        "links": [
            ("AdminModuleTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/AdminModuleTests.cs"),
            ("AnalyticsModuleTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/AnalyticsModuleTests.cs"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-50": {
        "title": "Sprint-2: E2E Tests (CodeceptJS + Playwright, 19 Scenarios)",
        "evidence": [
            [bold("5 test files, 19 scenarios: ")],
            [text("1. smoke_test.js — Public pages, auth entry points")],
            [text("2. citizen_report_test.js — Citizen registration, create report")],
            [text("3. enterprise_assign_test.js — Enterprise login, task management")],
            [text("4. collector_task_test.js — Collector login, task access")],
            [text("5. citizen_complaint_test.js — Complaint flow (DT + Error Guessing)")],
            [bold("Browser: "), text("Chromium headless on CI")],
            [bold("Allure plugin: "), text("allure-codeceptjs v3.9.0 generates results")],
        ],
        "commits": [
            "84b738a fix(e2e): change start script from standalone to next start",
            "de4e1d9 debug(e2e): add allure-results listing",
        ],
        "links": [
            ("smoke_test.js", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/frontend/e2e/smoke_test.js"),
            ("citizen_report_test.js", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/frontend/e2e/citizen_report_test.js"),
            ("enterprise_assign_test.js", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/frontend/e2e/enterprise_assign_test.js"),
            ("collector_task_test.js", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/frontend/e2e/collector_task_test.js"),
            ("citizen_complaint_test.js", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/frontend/e2e/citizen_complaint_test.js"),
            ("codecept.conf.js", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/frontend/codecept.conf.js"),
            ("frontend-e2e.yml", f"{REPO_URL}/blob/main/.github/workflows/frontend-e2e.yml"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-51": {
        "title": "Sprint-2: WasteCategory + Security Tests",
        "evidence": [
            [bold("Role-based access: "), text("Admin, Enterprise, Citizen role tests")],
            [bold("Test files: "), text("WasteCategoryControllerTests.cs, AdminEnterpriseAuthorizationTests.cs, JwtBearerIntegrationTests.cs")],
            [bold("20+ test methods"), text(" covering category CRUD and security")],
        ],
        "commits": ["Category + Security tests committed in Sprint 2"],
        "links": [
            ("WasteCategoryControllerTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Controllers/WasteCategoryControllerTests.cs"),
            ("AdminEnterpriseAuthorizationTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Integration/AdminEnterpriseAuthorizationTests.cs"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-52": {
        "title": "Sprint-2: CollectorTask + File Uploads Tests",
        "evidence": [
            [bold("Controller + Handler tests: "), text("CollectorTaskControllerTests.cs, CollectorTaskControllerExtendedTests.cs")],
            [bold("File upload: "), text("CollectorEvidenceUploadTests.cs, LocalFileStorageServiceTests.cs")],
            [bold("20+ test methods"), text(" covering task assignment and file operations")],
        ],
        "commits": ["CollectorTask + File tests committed in Sprint 2"],
        "links": [
            ("CollectorTaskControllerTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Controllers/CollectorTaskControllerTests.cs"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-53": {
        "title": "Sprint-2: CollectionTask + Audit Tests",
        "evidence": [
            [bold("Domain logic tests: "), text("CollectionTaskDomainTests.cs, CollectionTaskTests.cs")],
            [bold("State transitions: "), text("Domain entity lifecycle tests")],
            [bold("AuditLog: "), text("AuditLogAndErrorPathTests.cs — error path coverage")],
        ],
        "commits": ["CollectionTask + Audit tests committed in Sprint 2"],
        "links": [
            ("CollectionTaskDomainTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Domain/CollectionTaskDomainTests.cs"),
            ("AuditLogAndErrorPathTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Controllers/AuditLogAndErrorPathTests.cs"),
            ("Allure Report", ALLURE_URL),
        ],
    },
    "KIEM-54": {
        "title": "Sprint-2: Citizen + Search/Pagination Tests",
        "evidence": [
            [bold("CitizenModuleTests.cs: "), text("Citizen profile management")],
            [bold("SearchPaginationFiltersTests.cs: "), text("Pagination edge cases (page 0, negative, beyond max)")],
            [bold("15+ test methods"), text(" covering citizen CRUD and search")],
        ],
        "commits": ["Citizen + Search tests committed in Sprint 2"],
        "links": [
            ("SearchPaginationFiltersTests.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/Search/SearchPaginationFiltersTests.cs"),
            ("Allure Report", ALLURE_URL),
        ],
    },

    # ═══════════════ SPRINT 3 ═══════════════
    "KIEM-55": {
        "title": "Sprint-3: Fix SonarCloud Quality Gate (16 -> 0 vulnerabilities)",
        "evidence": [
            [bold("Before: "), text("16 vulnerabilities, security_rating = 5 (BLOCKER)")],
            [bold("After: "), text("0 vulnerabilities, security_rating = A")],
            [bold("Fixes applied:")],
            [text("- appsettings.json: removed hardcoded DB password + JWT secret")],
            [text("- CreateUserCommand.cs: replaced hardcoded password with SHA256 hash")],
            [text("- Python scripts: added os.path.realpath() for path traversal fix")],
            [text("- sonar-project.properties: added exclusions for db/migrations, scripts")],
            [text("- sonar.yml: added SonarCloud exclusion parameters")],
        ],
        "commits": [
            "1d50e4c fix(security): fix SonarCloud Quality Gate + fix KIEM-29",
            "6b9a678 fix(build): replace BCrypt with SHA256 in CreateUserCommand",
            "bf05e20 fix(production): use valid development JWT secret key",
        ],
        "links": [
            ("SonarCloud Dashboard", "https://sonarcloud.io/project/overview?id=chi-trung_KCPM"),
            ("sonar.yml", f"{REPO_URL}/blob/main/.github/workflows/sonar.yml"),
            ("sonar-project.properties", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/sonar-project.properties"),
        ],
    },
    "KIEM-56": {
        "title": "Sprint-3: Fix KIEM-29 - Max 5 Images Validation (BVA)",
        "evidence": [
            [bold("Bug: "), text("API allowed > 5 images when creating report")],
            [bold("Fix: "), text("Added validation if (request.Images.Count > 5) throw")],
            [bold("BVA boundaries: "), text("0 -> reject, 1 -> accept, 5 -> accept (boundary), 6 -> reject")],
            [bold("File: "), text("CreateReportCommand.cs")],
        ],
        "commits": [
            "1d50e4c fix(security): fix SonarCloud Quality Gate + fix KIEM-29",
        ],
        "links": [
            ("CreateReportCommand.cs", f"{REPO_URL}/blob/main/Waste-Recycling-Platform/backend/src/WastePlatform.Application/Reports/Commands/CreateReportCommand.cs"),
            ("Allure BVA Tests", ALLURE_URL),
        ],
    },
    "KIEM-59": {
        "title": "Sprint-3: Final Report v6.0 & Deployment Guide",
        "evidence": [
            [bold("FINAL_REPORT.md v6.0: "), text("280 lines, test dashboard, CI metrics")],
            [bold("DEPLOYMENT_GUIDE.md: "), text("892 lines, full deployment instructions")],
            [bold("CI_CD_WORKFLOWS.md: "), text("Detailed description of all 9 workflows")],
            [bold("DEMO.md: "), text("696 lines demo script for presentation")],
            [bold("Test counts: "), text("455 xUnit + 19 E2E + 74 Postman + 68 Manual = 616 total")],
        ],
        "commits": [
            "242954d feat(KIEM-42,KIEM-43): update Final Report v6.0 & Traceability Matrix",
        ],
        "links": [
            ("FINAL_REPORT.md", f"{REPO_URL}/blob/main/docs/FINAL_REPORT.md"),
            ("DEPLOYMENT_GUIDE.md", f"{REPO_URL}/blob/main/docs/DEPLOYMENT_GUIDE.md"),
            ("CI_CD_WORKFLOWS.md", f"{REPO_URL}/blob/main/docs/CI_CD_WORKFLOWS.md"),
            ("DEMO.md", f"{REPO_URL}/blob/main/docs/DEMO.md"),
        ],
    },
    "KIEM-60": {
        "title": "Sprint-3: Manual Test Cases (Excel — 68 TCs, 13 Functions)",
        "evidence": [
            [bold("UnitestKCPM.xlsx: "), text("68 test cases covering 13 functions")],
            [bold("Techniques: "), text("EP, BVA, Decision Table, State Transition, Error Guessing")],
            [bold("Each TC has: "), text("ID, Description, Steps, Expected, Actual, Status")],
            [bold("Pass rate: "), text("≥ 95% with screenshots evidence")],
        ],
        "commits": ["Manual test cases documented in UnitestKCPM.xlsx"],
        "links": [
            ("Allure Report", ALLURE_URL),
            ("TEST_PLAN.md", f"{REPO_URL}/blob/main/docs/TEST_PLAN.md"),
        ],
    },
    "KIEM-61": {
        "title": "Sprint-3: Traceability Matrix Update",
        "evidence": [
            [bold("TRACEABILITY_MATRIX.md: "), text("Complete mapping for all modules")],
            [bold("Coverage: "), text("Requirement → Jira Issue → Test Case → CI Evidence")],
            [bold("Bug tracking: "), text("KIEM-26, 27, 28, 29, 30 status updates included")],
            [bold("Test technique mapping: "), text("Ch.4 techniques mapped to each module")],
            [bold("Allure links: "), text("Direct links to Allure suites for each module")],
        ],
        "commits": [
            "242954d feat(KIEM-42,KIEM-43): update Final Report v6.0 & Traceability Matrix",
        ],
        "links": [
            ("TRACEABILITY_MATRIX.md", f"{REPO_URL}/blob/main/docs/TRACEABILITY_MATRIX.md"),
            ("Jira Board", JIRA_BOARD),
        ],
    },
    "KIEM-62": {
        "title": "Sprint-3: Demo Preparation for Professor",
        "evidence": [
            [bold("DEMO.md: "), text("696 lines chi tiet kich ban demo")],
            [bold("7 demo sections: "), text("Architecture, CI/CD, Tests, Allure, SonarCloud, Jira, Production app")],
            [bold("Live URLs tested and working: ")],
            [text("- Frontend: https://kcpm.vercel.app")],
            [text("- Backend: https://kcpm-backend.onrender.com")],
            [text("- Allure: https://chi-trung.github.io/KCPM/report-main/")],
        ],
        "commits": ["Demo preparation completed in Sprint 3"],
        "links": [
            ("DEMO.md", f"{REPO_URL}/blob/main/docs/DEMO.md"),
            ("Allure Report", ALLURE_URL),
            ("Production App", "https://kcpm.vercel.app"),
        ],
    },
    "KIEM-65": {
        "title": "Sprint-3: Test Report — WasteCategory + Notifications Module",
        "evidence": [
            [bold("Document: "), text("docs/TEST_REPORT_CATEGORY_NOTIFICATIONS.md")],
            [bold("WasteCategory tests: "), text("EP partitions for CRUD operations, 5-category coverage")],
            [bold("Notifications tests: "), text("Valid/invalid notification IDs, mark-as-read, SignalR")],
            [bold("Techniques: "), text("EP, Error Guessing")],
            [bold("Traceability: "), text("KIEM-6 (Notifications), KIEM-12 (WasteCategory)")],
        ],
        "commits": [
            "00e66c1 docs(KIEM-65): add test report for WasteCategory + Notifications modules",
        ],
        "links": [
            ("TEST_REPORT_CATEGORY_NOTIFICATIONS.md", f"{REPO_URL}/blob/main/docs/TEST_REPORT_CATEGORY_NOTIFICATIONS.md"),
            ("PR #63", f"{REPO_URL}/pull/63"),
            ("Allure Report", ALLURE_URL),
        ],
    },
}


def main():
    if not JIRA_BASE or not JIRA_EMAIL or not JIRA_TOKEN:
        print("Missing JIRA credentials. Set JIRA_BASE_URL, JIRA_API_EMAIL, JIRA_API_TOKEN.")
        sys.exit(1)

    # Verify auth
    me = jira_get("myself")
    if "error" in me:
        print(f"Auth failed: {me}")
        sys.exit(1)
    print(f"Authenticated as: {me.get('displayName', 'unknown')}")

    # Post evidence to each issue
    success = 0
    for issue_key, evidence in SPRINT_EVIDENCE.items():
        print(f"\n{'='*50}")
        print(f"Posting evidence to {issue_key}: {evidence['title']}")

        body = build_evidence_doc(
            evidence["title"],
            evidence.get("evidence", []),
            evidence.get("commits"),
            evidence.get("links"),
        )

        result = jira_post(f"issue/{issue_key}/comment", body)
        if "id" in result or result.get("ok"):
            print(f"  OK - Comment created on {issue_key}")
            success += 1
        else:
            print(f"  FAILED - {result}")

    print(f"\n{'='*50}")
    print(f"Evidence posted to {success}/{len(SPRINT_EVIDENCE)} issues.")


if __name__ == "__main__":
    main()
