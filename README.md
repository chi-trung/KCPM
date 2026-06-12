# KCPM — Waste Recycling Platform

> **Kiểm Chứng Phần Mềm** | UIT Team 36 | Môn Kiểm Chứng Phần Mềm

[![Backend Tests](https://github.com/chi-trung/KCPM/actions/workflows/backend-tests.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/backend-tests.yml)
[![Frontend E2E](https://github.com/chi-trung/KCPM/actions/workflows/frontend-e2e.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/frontend-e2e.yml)
[![Postman Smoke](https://github.com/chi-trung/KCPM/actions/workflows/postman-smoke.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/postman-smoke.yml)
[![Allure Report](https://github.com/chi-trung/KCPM/actions/workflows/allure-gh-pages.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/allure-gh-pages.yml)
[![SonarCloud](https://github.com/chi-trung/KCPM/actions/workflows/sonar.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/sonar.yml)

---

## 📊 Test Reports

| Report | URL |
|--------|-----|
| **Allure Report (Live)** | [chi-trung.github.io/KCPM/report-main](https://chi-trung.github.io/KCPM/report-main/) |
| **Jira Board** | [ut-team-36.atlassian.net](https://ut-team-36.atlassian.net/jira/software/projects/KIEM/boards/3) |

---

## 🏗️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 8, MySQL, xUnit |
| **Frontend** | Next.js, React |
| **E2E Tests** | CodeceptJS + Playwright |
| **API Tests** | Postman + Newman |
| **CI/CD** | GitHub Actions |
| **Test Report** | Allure (auto-deploy to GitHub Pages) |
| **Code Quality** | SonarCloud |
| **Issue Tracking** | Jira (auto-logged from CI) |

---

## 👥 Team

| Thành viên | Phụ trách | KIEM Tasks |
|-----------|-----------| -----------|
| Nguyễn Chí Trung | Auth, Collector, CI/CD | KIEM-21 |
| Minh Phụng | Reports, File Upload | KIEM-5 |
| Nguyễn Hoàng Phụng | Waste, Security | KIEM-21 |
| Đăng | Accept/Reject, Complaints | KIEM-22 |
| Thanh Duy | Task, Analytics | KIEM-15, KIEM-19 |

---

## 🚀 CI/CD Pipeline

```
Push to main
    ├─ Backend Tests (xUnit)     → Allure results artifact + Jira comment (KIEM-5)
    ├─ Frontend E2E (Playwright) → e2e-allure-results artifact + Jira comment (KIEM-14)
    ├─ Postman Smoke (Newman)    → merged into allure-results + Jira comment (KIEM-21)
    ├─ SonarCloud Analysis       → code quality gate
    └─ Allure Pages Deploy       → GitHub Pages (auto-triggered after Backend Tests)
             ↓
  https://chi-trung.github.io/KCPM/report-main/
  Suites: E2E Tests | API Tests (Postman) | Backend Tests (xUnit)
  Behaviors: E2E Frontend epic | xUnit epics (KIEM-5, KIEM-12, KIEM-15...)
```

---

## 🔑 Required GitHub Secrets

| Secret | Description | How to get |
|--------|-------------|------------|
| `JIRA_BASE_URL` | Jira instance URL | `https://ut-team-36.atlassian.net` |
| `JIRA_API_EMAIL` | Atlassian account email | Email of Jira account |
| `JIRA_API_TOKEN` | Atlassian API token | [id.atlassian.com/manage-profile/security/api-tokens](https://id.atlassian.com/manage-profile/security/api-tokens) |
| `JIRA_API_TOKEN` | ⚠️ Must be **Atlassian API Token**, NOT Personal Access Token (PAT) | Create new token at link above |
| `SONAR_TOKEN` | SonarCloud token | SonarCloud project settings |

> **Note:** To verify Jira credentials work locally, run:
> ```bash
> JIRA_BASE_URL=https://ut-team-36.atlassian.net \
> JIRA_API_EMAIL=your.email@gmail.com \
> JIRA_API_TOKEN=your_token \
> python3 scripts/check_jira_connection.py
> ```

---

## 📁 Structure

```
KCPM/
├── .github/workflows/          # CI/CD workflows
│   ├── backend-tests.yml       # xUnit tests + Jira logging
│   ├── frontend-e2e.yml        # CodeceptJS E2E + Jira logging
│   ├── postman-smoke.yml       # Newman API tests + Jira logging
│   ├── postman-weekly-report.yml  # Weekly full Postman run
│   ├── allure-gh-pages.yml     # Allure report deploy (auto-triggered)
│   ├── sonar.yml               # SonarCloud analysis
│   └── deploy-server.yml       # Production deploy
├── Waste-Recycling-Platform/
│   ├── backend/                # ASP.NET Core API
│   ├── frontend/               # Next.js app
│   │   └── e2e/               # CodeceptJS test files (BDD style)
│   ├── postman/                # Postman collections
│   ├── scripts/                # Allure/report helper scripts (Python)
│   │   ├── build_categories_report.py  # Build Allure categories widget
│   │   ├── normalize_allure_suites.py  # Ensure 3 Allure suite groups
│   │   ├── generate_per_owner_reports.py  # Per-owner Allure reports
│   │   └── create_validation_artifacts.py  # CI validation artifacts
│   └── allure-categories.json  # Failure category rules (14 categories)
├── scripts/                    # Project-level Python scripts
│   ├── jira_log_test_execution.py  # Auto-post CI results to Jira
│   └── check_jira_connection.py    # Verify Jira credentials locally
├── docs/                       # Project documentation
│   └── TRACEABILITY_MATRIX.md  # Requirement-to-test mapping
├── history-chat/               # Dev session notes
└── test-cases/                 # Manual test documentation
```

