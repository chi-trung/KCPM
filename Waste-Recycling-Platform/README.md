# 🗑️ Crowdsourced Waste Collection & Recycling Platform

> A web-based platform connecting **Citizens**, **Recycling Enterprises**, and **Collectors** to streamline waste reporting, collection, and reward redemption—built with C# .NET 8 + Next.js 14.

---

## 🌐 Live Demo

| Service | URL | Status |
|---------|-----|--------|
| **Frontend** | https://kcpm.vercel.app | ✅ Live |
| **Backend API** | https://kcpm-backend.onrender.com/api | ✅ Live |
| **Swagger UI** | https://kcpm-backend.onrender.com/swagger | ✅ Live |
| **Allure Report** | https://chi-trung.github.io/KCPM/report-main/ | ✅ Live |
| **SonarCloud** | https://sonarcloud.io/summary/overall?id=chi-trung_KCPM | ✅ Live |

### Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@gmail.com | password |
| Citizen | nguyenvana@gmail.com | password |
| Enterprise | greenlife@gmail.com | password |
| Collector | collector1@gmail.com | password |

---

## ✨ Key Features

- 📸 **Citizens** report waste with GPS location, photos & optional AI classification
- 🏭 **Enterprises** accept reports within their service area and dispatch collectors
- 🚛 **Collectors** update task status in real-time and confirm collection with photos
- 🏆 **Reward system** — Citizens earn points automatically when their waste is collected
- 📊 **Admin panel** — manage users, approve enterprises, resolve complaints
- 📱 **PWA-ready** — works on mobile as a Progressive Web App

---

## 🔁 CI/CD Deploy Server (For Verification Evidence)

This repository now supports an automated deploy workflow using GitHub Actions:

- Workflow file: `.github/workflows/deploy-server.yml`
- Trigger:
    - Push to `main` (automatic)
    - Manual run via `workflow_dispatch`
- Flow:
    - Run backend test quality gate first
    - If tests pass, deploy to server via SSH and Docker Compose

### Required GitHub Secrets

Set these in **Settings → Secrets and variables → Actions**:

| Secret | Purpose |
|---|---|
| `DEPLOY_HOST` | Server IP or domain |
| `DEPLOY_USER` | SSH user on server |
| `DEPLOY_SSH_KEY` | Private SSH key used by GitHub Actions |
| `DEPLOY_PORT` | SSH port (optional, default `22`) |
| `DEPLOY_PATH` | Path on server to deploy repo (optional, default `/opt/kcpm`) |
| `DEPLOY_REPO_TOKEN` | GitHub token with repo read access for server-side git pull |
| `MYSQL_ROOT_PASSWORD` | MySQL root password |
| `MYSQL_DATABASE` | MySQL database name |
| `MYSQL_USER` | MySQL app user |
| `MYSQL_PASSWORD` | MySQL app password |
| `JWT_SECRET` | JWT secret key |
| `JWT_ISSUER` | JWT issuer |
| `JWT_AUDIENCE` | JWT audience |
| `ASPNETCORE_ENVIRONMENT` | Backend runtime environment (e.g., `Production`) |
| `NEXT_PUBLIC_API_URL` | Frontend API base URL |

### Why this matters for Software Verification class

This setup creates auditable evidence per Jira task/member contribution:

- Jira issue key in branch/commit/PR
- CI quality gate result (test artifacts)
- Deploy execution logs and timestamps
- End-to-end trace from task to production deployment

---

## 📊 Automated API Test Reporting (Allure + GitHub Pages)

This repository includes a fully automated workflow to run Postman API tests and publish an interactive **Allure Report** to GitHub Pages. This makes it extremely easy to submit weekly testing evidence for Software Verification class without requiring the teacher to download or run any code.

- **Workflow file**: `.github/workflows/allure-gh-pages.yml`
- **Trigger**: Push to `main` or Manual run via `workflow_dispatch`
- **What it does**:
  1. Starts the local backend environment using `docker-compose`.
  2. Runs the Postman collection `WastePlatform API - Professional QA Suite.postman_collection.json` via Newman.
  3. Generates a visual Allure test report, including historical trends.
  4. Deploys the report to the `gh-pages` branch.
- **View the Report**: The latest report is always available at `https://<your-username>.github.io/<repo-name>/` (Please make sure GitHub Pages is enabled in Settings -> Pages -> Source: `gh-pages` branch).

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| **Backend** | C# .NET 8 — ASP.NET Core Web API (Clean Architecture + CQRS with MediatR) |
| **Frontend** | Next.js 14 (App Router) — React 18, TypeScript, Tailwind CSS |
| **Database** | MySQL 8.0 + EF Core via Pomelo provider |
| **Auth** | JWT — access token (1 h) + refresh token (30 d) |
| **State (FE)** | Zustand (auth) + TanStack Query (server state + polling) |
| **Infra** | Docker Compose — Nginx reverse proxy |
| **CI/CD** | GitHub Actions |

---

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/) *(recommended for DB)*
- MySQL 8.0 *(or use the Docker Compose DB service)*

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/<your-org>/waste-platform.git
cd waste-platform
```

### 2. Configure Environment Variables

```bash
cp .env.example .env
```

Edit `.env`:

| Variable | Description | Example |
|---|---|---|
| `DATABASE_URL` | MySQL connection string | `Server=localhost;Database=wasteplatform;User=root;Password=secret` |
| `JWT_SECRET` | 256-bit secret for signing JWTs | *(generate with `openssl rand -hex 32`)* |
| `JWT_EXPIRY_MINUTES` | Access token lifetime | `60` |
| `JWT_REFRESH_DAYS` | Refresh token lifetime | `30` |
| `STORAGE_BUCKET` | Cloud storage bucket name | `waste-platform-images` |
| `AI_SERVICE_URL` | Internal AI classification endpoint | `http://ai-service:8000` |

### 3. Start the Database

```bash
docker compose up -d db
```

### 4. Run Database Migrations

```bash
cd backend
dotnet ef database update --project src/WastePlatform.Infrastructure --startup-project src/WastePlatform.API
```

*(Or run the migration SQL files in `db/migrations/` manually in order.)*

### 5. Start the Backend API

```bash
cd backend
dotnet run --project src/WastePlatform.API
# API available at http://localhost:5000
# Swagger UI at  http://localhost:5000/swagger
```

### 6. Start the Frontend

```bash
cd frontend
npm install
npm run dev
# App available at http://localhost:3000
```

### 7. Full Stack with Docker Compose

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend (via Nginx) | http://localhost |
| API (via Nginx) | http://localhost/api |
| Swagger | http://localhost/api/swagger |
| MySQL | localhost:3306 |

---

## 🏗️ Architecture

### Monorepo Structure

```
waste-platform/
├── backend/              # C# .NET 8 — Clean Architecture
│   ├── src/
│   │   ├── WastePlatform.Domain/         # Entities, Enums, Value Objects, Domain Events
│   │   ├── WastePlatform.Application/    # Use Cases (Commands/Queries via MediatR)
│   │   ├── WastePlatform.Infrastructure/ # EF Core, Repositories, External Services
│   │   └── WastePlatform.API/            # Controllers, Middleware, DTOs
│   └── tests/
│       ├── WastePlatform.Domain.Tests/
│       ├── WastePlatform.Application.Tests/
│       └── WastePlatform.Integration.Tests/
│
├── frontend/             # Next.js 14 (App Router)
│   └── src/
│       ├── app/
│       │   ├── (auth)/          # /login, /register
│       │   ├── (citizen)/       # /dashboard, /reports, /rewards, /complaints
│       │   ├── (enterprise)/    # /dashboard, /reports, /tasks, /analytics
│       │   ├── (collector)/     # /tasks, /history
│       │   └── (admin)/         # /dashboard, /users, /enterprises, /complaints
│       ├── components/          # Reusable UI components
│       ├── hooks/               # useAuth, useGeolocation, usePolling, ...
│       ├── lib/api/             # Axios client + per-domain API modules
│       └── types/               # Auto-generated from openapi.yaml
│
├── db/
│   ├── migrations/       # Versioned SQL migration files
│   └── seeds/            # Seed data (waste categories, admin user)
│
├── docs/                 # openapi.yaml, design docs
├── docker-compose.yml
└── .github/workflows/    # CI/CD pipelines
```

### Clean Architecture Layers

```
┌──────────────────────────────────────┐
│  Layer 4 — API (Controllers / DTOs)  │
│  Layer 3 — Infrastructure (DB / S3)  │
│  Layer 2 — Application (Use Cases)   │
│  Layer 1 — Domain (Entities / Rules) │ ← No external dependencies
└──────────────────────────────────────┘
```

Dependency rule: **outer layers depend inward, never the reverse.**

---

## 🔄 Sequence Diagrams

### 1. Citizen Creates a Waste Report (with AI Classification)

```mermaid
sequenceDiagram
    actor C as Citizen
    participant App as Web App
    participant API as API Server
    participant AI as AI Service
    participant DB as Database
    participant Storage as Cloud Storage

    C->>App: Upload photo + enter description
    App->>Storage: Upload image
    Storage-->>App: image_url

    App->>AI: POST /ai/classify { image_url }
    AI-->>App: { suggestion: "Recyclable", confidence: 0.87 }

    C->>App: Confirm/change waste category
    App->>API: POST /reports { category, location, image_urls }
    API->>DB: INSERT waste_reports (status=pending)
    DB-->>API: report created
    API-->>App: { report_id, status: "pending" }
    App-->>C: "Report submitted successfully!"
```

### 2. Enterprise Accepts Report & Assigns Collector

```mermaid
sequenceDiagram
    actor E as Enterprise
    actor COL as Collector
    participant App as Enterprise Web
    participant API as API Server
    participant DB as Database
    participant Notif as Notification Service

    E->>App: View incoming reports in service area
    App->>API: GET /enterprise/reports/incoming
    API->>DB: SELECT reports WHERE status=pending AND in service_area
    DB-->>API: [reports list]
    API-->>App: reports list

    E->>App: Select report → Accept
    App->>API: PATCH /enterprise/reports/:id { status: "accepted" }
    API->>DB: INSERT collection_tasks + UPDATE waste_reports
    API-->>App: { task_id }

    E->>App: Assign task to Collector
    App->>API: PATCH /enterprise/tasks/:id { collector_id }
    API->>DB: UPDATE collection_tasks SET collector_id
    API->>Notif: Push → Collector
    Notif-->>COL: "You have a new collection task!"
```

### 3. Collector Completes Collection → Citizen Earns Points

```mermaid
sequenceDiagram
    actor COL as Collector
    actor C as Citizen
    participant App as Collector Web
    participant API as API Server
    participant DB as Database
    participant Notif as Notification Service

    COL->>App: Start heading to location
    App->>API: PATCH /collector/tasks/:id/status { status: "on_the_way" }
    API->>DB: UPDATE task + INSERT status_log
    API->>Notif: Notify Citizen "Collector is on the way"
    Notif-->>C: "Collector is coming to pick up your waste"

    COL->>App: Complete collection + upload confirmation photo
    App->>API: PATCH /collector/tasks/:id/status { status:"collected", weight_kg, image_urls }
    API->>DB: UPDATE task, report, INSERT images + status_log

    Note over API,DB: Calculate reward points for Citizen
    API->>DB: SELECT reward_rules for enterprise + waste_category
    DB-->>API: { points_per_report: 15, bonus_quality: 5 }
    API->>DB: INSERT reward_points { citizen_id, points: 20 }

    API->>Notif: Notify Citizen "+20 points!"
    Notif-->>C: "✅ Waste collected! +20 reward points"
```

### 4. Citizen Files a Complaint → Admin Resolves

```mermaid
sequenceDiagram
    actor C as Citizen
    actor ADM as Admin
    participant API as API Server
    participant DB as Database
    participant Notif as Notification Service

    C->>API: POST /complaints { report_id, content }
    API->>DB: INSERT complaints (status=open)
    API->>Notif: Alert Admin about new complaint
    Notif-->>ADM: "New complaint requires review"

    ADM->>API: GET /admin/complaints/:id
    API->>DB: SELECT complaint + related report + task
    DB-->>API: full detail
    API-->>ADM: complaint detail

    ADM->>API: PATCH /admin/complaints/:id { status:"resolved", admin_response }
    API->>DB: UPDATE complaints SET status=resolved
    API->>Notif: Notify Citizen complaint resolved
    Notif-->>C: "Your complaint has been resolved by Admin"
```

---

## 📦 Key Dependencies

### Backend (NuGet)

| Package | Purpose |
|---|---|
| `Pomelo.EntityFrameworkCore.MySql` | MySQL 8 EF Core provider |
| `MediatR` | CQRS — Commands & Queries |
| `FluentValidation` | Request validation |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT authentication |
| `BCrypt.Net-Next` | Password hashing |
| `Serilog` | Structured logging |
| `AspNetCoreRateLimit` | Rate limiting |
| `Swashbuckle.AspNetCore` | Swagger UI |

### Frontend (npm)

| Package | Purpose |
|---|---|
| `axios` | HTTP client with JWT interceptor |
| `zustand` | Global auth state |
| `@tanstack/react-query` | Server state, caching, polling |
| `react-hook-form` + `zod` | Form validation |
| `react-leaflet` | Interactive map + GPS picker |
| `next-pwa` | PWA support |
| `openapi-typescript` | Auto-generate types from openapi.yaml |

---

## 🔐 Authentication Flow

1. `POST /auth/register` → create account (citizen / enterprise / collector)
2. `POST /auth/login` → returns `{ accessToken, refreshToken }`
3. All protected routes require `Authorization: Bearer <accessToken>` header
4. On 401 → frontend auto-calls `POST /auth/refresh` with `refreshToken` cookie
5. Role-based access enforced via `[AuthorizeRole("citizen")]` attribute on controllers

---

## 🧪 Testing

### Backend

```bash
cd backend
# Unit + integration tests
dotnet test

# Specific project
dotnet test tests/WastePlatform.Application.Tests
```

### Frontend

```bash
cd frontend
npm test           # Jest unit tests
npm run e2e        # Playwright E2E tests (if configured)
```

---

## 🚢 Deployment

### Docker Compose (Recommended)

```bash
# Production build
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

Nginx routes:
- `/` → `frontend:3000`
- `/api/*` → `backend:5000`

### CI/CD (GitHub Actions — 9 Workflows)

| # | Workflow | Trigger | Action |
|---|---------|---------|--------|
| 1 | `backend-tests.yml` | Push / PR / Schedule | xUnit → Allure → Coverage → Jira log |
| 2 | `frontend-e2e.yml` | Push / PR | CodeceptJS + Playwright E2E tests |
| 3 | `sonar.yml` | Push / PR | SonarCloud static analysis |
| 4 | `postman-smoke.yml` | PR / Schedule / Manual | Newman API tests → Docker → Jira |
| 5 | `allure-gh-pages.yml` | After Backend Tests | Merged Allure report → GitHub Pages |
| 6 | `deploy-server.yml` | Push to main | Quality gate → SSH deploy |
| 7 | `health-check.yml` | Every 6h / Manual | Monitor uptime |
| 8 | `jira-key-enforcement.yml` | PR events | Validate Jira keys in PR/commits |
| 9 | `create-jira-issues.yml` | Manual | Create Jira issues from test plan |

> 📚 Full documentation: [DEPLOYMENT_GUIDE.md](../docs/DEPLOYMENT_GUIDE.md) | [CI_CD_WORKFLOWS.md](../docs/CI_CD_WORKFLOWS.md)

---

## 🔧 Available Commands

### Backend

```bash
dotnet run --project src/WastePlatform.API         # Start API server
dotnet test                                         # Run all tests
dotnet ef migrations add <Name> --project src/WastePlatform.Infrastructure \
  --startup-project src/WastePlatform.API          # Add migration
dotnet ef database update ...                       # Apply migrations
```

### Frontend

```bash
npm run dev        # Development server (http://localhost:3000)
npm run build      # Production build
npm run lint       # ESLint check
npm run type-check # TypeScript check
npx openapi-typescript docs/openapi.yaml -o src/types/api.ts  # Regenerate API types
```

---

## 🗂️ Environment Variables Reference

| Variable | Required | Description |
|---|---|---|
| `DATABASE_URL` | ✅ | MySQL connection string |
| `JWT_SECRET` | ✅ | 256-bit signing secret |
| `JWT_EXPIRY_MINUTES` | ✅ | Access token expiry (default: `60`) |
| `JWT_REFRESH_DAYS` | ✅ | Refresh token expiry (default: `30`) |
| `STORAGE_BUCKET` | ✅ | Cloud storage bucket name |
| `AI_SERVICE_URL` | ⚠️ Optional | AI classification service URL |

---

## 🏛️ Architecture Decision Records

| # | Decision | Rationale |
|---|---|---|
| ADR-01 | C# .NET 8 Backend | Stable LTS, strong typing, EF Core |
| ADR-02 | Next.js 14 App Router | Route groups per role, SSR, PWA ready |
| ADR-03 | Clean Architecture | Domain logic decoupled from DB/framework |
| ADR-04 | CQRS with MediatR | Clear read/write separation |
| ADR-05 | JWT stateless auth (1h + 30d) | No session store required |
| ADR-06 | MySQL 8.0 | Team familiarity; spatial queries via Haversine |
| ADR-07 | Zustand + React Query | Zustand for auth, React Query for server state |

---

## 🤝 Contributing

1. Fork the repo and create a feature branch: `git checkout -b feat/my-feature`
2. Follow the **Clean Architecture** layer rules — no domain code in API layer
3. Ensure all tests pass: `dotnet test` and `npm test`
4. Submit a Pull Request against `main`

---

## 📄 License

MIT © 2026 Waste Platform Team
