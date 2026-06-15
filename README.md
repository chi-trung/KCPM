# ♻️ KCPM — Waste Recycling Platform

> **Kiểm Chứng Phần Mềm** | UIT Team 36 | Đại học Công nghệ Thông tin — ĐHQG TP.HCM

[![Backend Tests](https://github.com/chi-trung/KCPM/actions/workflows/backend-tests.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/backend-tests.yml)
[![Frontend E2E](https://github.com/chi-trung/KCPM/actions/workflows/frontend-e2e.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/frontend-e2e.yml)
[![Postman Smoke](https://github.com/chi-trung/KCPM/actions/workflows/postman-smoke.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/postman-smoke.yml)
[![Allure Report](https://github.com/chi-trung/KCPM/actions/workflows/allure-gh-pages.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/allure-gh-pages.yml)
[![SonarCloud](https://github.com/chi-trung/KCPM/actions/workflows/sonar.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/sonar.yml)
[![CI CD Deploy](https://github.com/chi-trung/KCPM/actions/workflows/deploy-server.yml/badge.svg)](https://github.com/chi-trung/KCPM/actions/workflows/deploy-server.yml)

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=chi-trung_KCPM&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=chi-trung_KCPM)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=chi-trung_KCPM&metric=bugs)](https://sonarcloud.io/summary/new_code?id=chi-trung_KCPM)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=chi-trung_KCPM&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=chi-trung_KCPM)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=chi-trung_KCPM&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=chi-trung_KCPM)

[![Branch Coverage](https://img.shields.io/endpoint?url=https://chi-trung.github.io/KCPM/badges/coverage-badge.json)](https://github.com/chi-trung/KCPM/actions/workflows/backend-tests.yml)
[![Line Coverage](https://img.shields.io/endpoint?url=https://chi-trung.github.io/KCPM/badges/line-coverage-badge.json)](https://github.com/chi-trung/KCPM/actions/workflows/backend-tests.yml)
[![Method Coverage](https://img.shields.io/endpoint?url=https://chi-trung.github.io/KCPM/badges/method-coverage-badge.json)](https://github.com/chi-trung/KCPM/actions/workflows/backend-tests.yml)

---

## 📖 Giới thiệu

**Waste Recycling Platform** là hệ thống quản lý thu gom rác thải tái chế, kết nối **người dân**, **doanh nghiệp thu gom**, và **cơ quan quản lý**. Dự án được xây dựng trong khuôn khổ môn **Kiểm Chứng Phần Mềm** nhằm áp dụng các quy trình kiểm thử phần mềm chuyên nghiệp.

### Chức năng chính

| Vai trò | Chức năng |
|---------|-----------|
| 🟢 **Citizen** | Tạo báo cáo rác (GPS, ảnh, phân loại), theo dõi trạng thái, đổi điểm thưởng |
| 🔵 **Enterprise** | Quản lý đội thu gom, phân công công việc, xem thống kê |
| 🟠 **Collector** | Nhận và cập nhật trạng thái công việc thu gom |
| 🔴 **Admin** | Quản lý người dùng, duyệt doanh nghiệp, xử lý khiếu nại |

---

## 🌐 Live Demo

| Component | URL | Ghi chú |
|-----------|-----|---------|
| **🖥️ Frontend** | [kcpm.vercel.app](https://kcpm.vercel.app) | Next.js trên Vercel |
| **⚙️ Backend API** | [kcpm-backend.onrender.com](https://kcpm-backend.onrender.com/api/health) | .NET 8 trên Render |
| **📖 Swagger UI** | [Swagger](https://kcpm-backend.onrender.com/swagger) | API Documentation |

> ⚠️ **Backend chạy trên Render Free Tier** — server tự ngủ sau 15 phút không có request.
> Lần đầu truy cập sẽ mất **30-60 giây** để khởi động (cold start). Chỉ cần đợi!

### 🔑 Tài khoản test

Tất cả tài khoản dùng password: `password`

| Email | Role | Tên |
|-------|------|-----|
| `admin@gmail.com` | Admin | System Administrator |
| `nguyenvana@gmail.com` | Citizen | Nguyễn Văn A |
| `lethib@gmail.com` | Citizen | Lê Thị B |
| `greenlife@gmail.com` | Enterprise | Green Life CEO |
| `collector1@gmail.com` | Collector | Phạm Minh Dũng |

👉 Xem đầy đủ: [`docs/TEST_ACCOUNTS.md`](docs/TEST_ACCOUNTS.md)

---

## 📊 Test Reports

| Report | URL |
|--------|-----|
| **Allure Report (Live)** | [chi-trung.github.io/KCPM/report-main](https://chi-trung.github.io/KCPM/report-main/) |
| **Jira Board** | [ut-team-36.atlassian.net](https://ut-team-36.atlassian.net/jira/software/projects/KIEM/boards/3) |
| **SonarCloud** | [sonarcloud.io/chi-trung_KCPM](https://sonarcloud.io/summary/overall?id=chi-trung_KCPM) |

---

## 🏗️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 8, Entity Framework Core, MySQL (Aiven) |
| **Frontend** | Next.js 14, React, Vercel |
| **Database** | MySQL 8.x (Aiven Cloud) |
| **E2E Tests** | CodeceptJS + Playwright |
| **API Tests** | Postman + Newman |
| **Unit Tests** | xUnit + Moq |
| **CI/CD** | GitHub Actions (11 workflows) |
| **Test Report** | Allure (auto-deploy to GitHub Pages) |
| **Code Quality** | SonarCloud (Quality Gate) |
| **Issue Tracking** | Jira (auto-logged from CI) |
| **Hosting** | Vercel (Frontend) + Render (Backend) + Aiven (Database) |

---

## 🏛️ Kiến trúc hệ thống

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│  Next.js Frontend│────▶│  .NET 8 Backend API  │────▶│  MySQL (Aiven)  │
│  (Vercel)        │     │  (Render - Docker)   │     │                 │
└─────────────────┘     └──────────────────────┘     └─────────────────┘
         │                        │
         │                        ├── JWT Authentication
         │                        ├── BCrypt Password Hash
         │                        ├── EF Core + Auto Migration
         │                        └── Seed Data (8 accounts)
         │
    ┌────┴────────────────────────────────────────┐
    │           GitHub Actions CI/CD               │
    │  ┌─────────┐ ┌──────────┐ ┌───────────────┐ │
    │  │ xUnit   │ │ E2E      │ │ Postman Smoke │ │
    │  │ Backend │ │ Playwright│ │ Newman        │ │
    │  └────┬────┘ └────┬─────┘ └──────┬────────┘ │
    │       └────────────┴──────────────┘          │
    │                    ▼                          │
    │        Allure Report (GitHub Pages)           │
    │        SonarCloud Quality Gate                │
    │        Jira Auto-Comment                      │
    └──────────────────────────────────────────────┘
```

---

## 🚀 CI/CD Pipeline

11 GitHub Actions workflows tự động:

```
Push to main
    ├─ Backend Tests (xUnit)     → Allure results + Jira comment (KIEM-5)
    ├─ Frontend E2E (Playwright) → E2E results + Jira comment (KIEM-14)
    ├─ Postman Smoke (Newman)    → API tests + Jira comment (KIEM-21)
    ├─ SonarCloud Analysis       → Code quality gate
    ├─ Deploy Backend            → Docker build + Render deploy
    └─ Allure Pages Deploy       → GitHub Pages (auto-triggered)
             ↓
   https://chi-trung.github.io/KCPM/report-main/
```

---

## 👥 Team

| Thành viên | Phụ trách | KIEM Tasks |
|-----------|-----------|------------|
| Nguyễn Chí Trung | Auth, Collector, CI/CD | KIEM-21 |
| Minh Phụng | Reports, File Upload | KIEM-5 |
| Nguyễn Hoàng Phụng | Waste, Security | KIEM-21 |
| Đăng | Accept/Reject, Complaints | KIEM-22 |
| Thanh Duy | Task, Analytics | KIEM-15, KIEM-19 |

---

## 🛠️ Cài đặt local

### Yêu cầu
- .NET 8 SDK
- Node.js 18+
- MySQL 8.x (hoặc Docker)

### Backend
```bash
cd Waste-Recycling-Platform/backend
dotnet restore
dotnet run --project src/WastePlatform.API
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend
```bash
cd Waste-Recycling-Platform/frontend
npm install
npm run dev
# App: http://localhost:3000
```

### Chạy tests
```bash
# Backend unit tests
cd Waste-Recycling-Platform/backend
dotnet test

# Frontend E2E
cd Waste-Recycling-Platform/frontend
npx codeceptjs run --steps

# Postman API tests
cd Waste-Recycling-Platform/postman
newman run WastePlatform.postman_collection.json
```

---

## 🔑 GitHub Secrets (cho CI/CD)

| Secret | Description |
|--------|-------------|
| `JIRA_BASE_URL` | `https://ut-team-36.atlassian.net` |
| `JIRA_API_EMAIL` | Atlassian account email |
| `JIRA_API_TOKEN` | [Tạo API token](https://id.atlassian.com/manage-profile/security/api-tokens) |
| `SONAR_TOKEN` | SonarCloud project token |

---

## 📁 Cấu trúc dự án

```
KCPM/
├── .github/workflows/           # 11 CI/CD workflows
│   ├── backend-tests.yml        # xUnit tests + Jira logging
│   ├── frontend-e2e.yml         # CodeceptJS E2E + Jira logging
│   ├── postman-smoke.yml        # Newman API tests + Jira logging
│   ├── allure-gh-pages.yml      # Allure report deploy
│   ├── sonar.yml                # SonarCloud analysis
│   ├── deploy-server.yml        # Production deploy
│   └── health-check.yml         # Keep services alive
├── Waste-Recycling-Platform/
│   ├── backend/                 # ASP.NET Core 8 API
│   │   ├── src/
│   │   │   ├── WastePlatform.API/          # Controllers, Middleware
│   │   │   ├── WastePlatform.Application/  # CQRS Commands/Queries
│   │   │   ├── WastePlatform.Domain/       # Entities, Interfaces
│   │   │   └── WastePlatform.Infrastructure/ # EF Core, Repositories
│   │   ├── tests/               # xUnit unit tests
│   │   └── Dockerfile           # Docker config (optimized for Render)
│   ├── frontend/                # Next.js 14 app
│   │   ├── src/                 # Pages, Components
│   │   └── e2e/                 # CodeceptJS E2E tests
│   └── postman/                 # Postman collections
├── docs/                        # Tài liệu dự án
│   ├── TEST_ACCOUNTS.md         # Tài khoản test (verified)
│   ├── DEPLOYMENT_GUIDE.md      # Hướng dẫn deploy
│   ├── DEMO.md                  # Kịch bản demo
│   └── TRACEABILITY_MATRIX.md   # Ma trận truy vết yêu cầu
├── scripts/                     # Python scripts
│   ├── jira_log_test_execution.py
│   └── check_jira_connection.py
└── test-cases/                  # Manual test documentation
```

---

## 📚 Tài liệu

| Tài liệu | Mô tả |
|-----------|--------|
| [`docs/TEST_ACCOUNTS.md`](docs/TEST_ACCOUNTS.md) | Tài khoản test đã verified |
| [`docs/DEPLOYMENT_GUIDE.md`](docs/DEPLOYMENT_GUIDE.md) | Hướng dẫn deploy chi tiết |
| [`docs/DEMO.md`](docs/DEMO.md) | Kịch bản demo cho thầy |
| [`docs/TRACEABILITY_MATRIX.md`](docs/TRACEABILITY_MATRIX.md) | Ma trận yêu cầu - test case |
| [`docs/FINAL_REPORT.md`](docs/FINAL_REPORT.md) | Báo cáo cuối kỳ |

---

## 📄 License

This project is for educational purposes — UIT Software Verification course (Kiểm Chứng Phần Mềm).
