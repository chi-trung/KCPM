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

---

## 👥 Team

| Thành viên | Phụ trách | KIEM Tasks |
|-----------|-----------|-----------|
| Nguyễn Chí Trung | Auth, Collector, CI/CD | KIEM-4, KIEM-21 |
| Minh Phụng | Reports, File Upload | KIEM-5 |
| Nguyễn Hoàng Phụng | Waste, Security | KIEM-3 |
| Đăng | Accept/Reject, Complaints | KIEM-13, KIEM-16 |
| Thanh Duy | Task, Analytics | KIEM-10, KIEM-19 |

---

## 🚀 CI/CD Pipeline

```
Push to main
    ├─ Backend Tests (xUnit)     → Allure results artifact
    ├─ Frontend E2E (Playwright) → e2e-allure-results artifact
    ├─ Postman Smoke (Newman)    → merged into allure-results
    ├─ SonarCloud Analysis       → code quality gate
    └─ Allure Pages Deploy       → GitHub Pages (auto-triggered)
             ↓
  https://chi-trung.github.io/KCPM/report-main/
  Suites: E2E Tests | API Tests (Postman) | Backend Tests (xUnit)
```

---

## 📁 Structure

```
KCPM/
├── .github/workflows/          # CI/CD workflows
│   ├── backend-tests.yml       # xUnit tests
│   ├── frontend-e2e.yml        # CodeceptJS E2E
│   ├── postman-smoke.yml       # Newman API tests
│   ├── allure-gh-pages.yml     # Allure report deploy
│   ├── sonar.yml               # SonarCloud analysis
│   └── deploy-server.yml       # Production deploy
├── Waste-Recycling-Platform/
│   ├── backend/                # ASP.NET Core API
│   ├── frontend/               # Next.js app
│   │   └── e2e/               # CodeceptJS test files
│   ├── postman/                # Postman collections
│   ├── scripts/                # Allure/CI helper scripts
│   └── allure-categories.json  # Allure failure categories
├── history-chat/               # Dev session notes
└── test-cases/                 # Manual test documentation
```
