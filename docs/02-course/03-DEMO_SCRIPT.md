# 🎤 Kịch Bản Demo CI/CD — Kiểm Chứng Phần Mềm

> **Cập nhật**: 13/06/2026

---

## 🏗️ Tổng Quan Hệ Thống

```
🧑‍💻 Developer push code lên GitHub
         │
         ▼
┌─────────────────────────────────────────────────────────┐
│                   GitHub Actions                         │
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌─────────┐ │
│  │ Backend  │  │ Frontend │  │ Sonar    │  │ CI/CD   │ │
│  │ Tests    │  │ E2E      │  │ Cloud    │  │ Deploy  │ │
│  │ (xUnit)  │  │(Codecept)│  │(Analysis)│  │ Server  │ │
│  └────┬─────┘  └──────────┘  └──────────┘  └─────────┘ │
│       │                                                  │
│       ▼ (nếu PASS)                                       │
│  ┌──────────┐                                            │
│  │ Allure   │                                            │
│  │ Pages    │                                            │
│  └──────────┘                                            │
│                                                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌─────────┐ │
│  │ Health   │  │ Jira Key │  │ Postman  │  │ Create  │ │
│  │ Check    │  │Enforce   │  │ Smoke    │  │ Jira    │ │
│  │ (6h)     │  │ (PR)     │  │ (API)    │  │ Issues  │ │
│  └──────────┘  └──────────┘  └──────────┘  └─────────┘ │
└─────────────────────────────────────────────────────────┘
         │
         ▼
┌──────────────────────────────────────────┐
│  🌐 Production                           │
│  Frontend: kcpm.vercel.app               │
│  Backend:  kcpm-backend.onrender.com     │
│  Report:   chi-trung.github.io/KCPM     │
└──────────────────────────────────────────┘
```

---

## 📋 Bảng Tóm Tắt 9 Workflows

| # | Tên Workflow | File YAML | Khi nào chạy? | Làm gì? | Output? |
|---|-------------|-----------|---------------|---------|---------|
| 1 | Backend Tests | `backend-tests.yml` | Push main / PR / Hàng ngày 21h UTC | Chạy 245+ unit tests | TRX, Allure, Coverage badges, Jira log |
| 2 | Frontend E2E | `frontend-e2e.yml` | Push main / PR | Chạy 15+ E2E tests trên trình duyệt | Screenshots, Allure results, Jira log |
| 3 | SonarCloud | `sonar.yml` | Push main / PR | Quét bugs, code smells, vulnerabilities | Quality Gate trên sonarcloud.io |
| 4 | Postman Smoke | `postman-smoke.yml` | PR / Hàng ngày 21h / Manual | Chạy API tests qua Newman + Docker | JUnit XML, Allure, Jira comments |
| 5 | Allure Pages | `allure-gh-pages.yml` | Sau Backend Tests pass | Merge 3 nguồn test → 1 report đẹp | GitHub Pages report |
| 6 | Deploy Server | `deploy-server.yml` | Push main | Quality gate → SSH deploy lên server | Server production updated |
| 7 | Health Check | `health-check.yml` | Mỗi 6 giờ | Ping 4 services kiểm tra còn sống | Step Summary ✅/❌ |
| 8 | Jira Key | `jira-key-enforcement.yml` | Mở/sửa PR | Kiểm tra PR title + commits có Jira key | Block merge nếu thiếu |
| 9 | Create Jira | `create-jira-issues.yml` | Bấm tay (manual) | Tạo issues trên Jira tự động | Jira issues created |

---

## 🔍 Chi Tiết Từng Workflow

---

### 🔵 Workflow #1: Backend Tests (`backend-tests.yml`)

```
📌 Trigger:  push main | PR vào main | 21:00 UTC hàng ngày | bấm tay
🖥️ Runner:   Windows (windows-latest)
⏱️ Thời gian: ~5-7 phút
```

**Khi developer push code → workflow này TỰ ĐỘNG chạy:**

```
Step 1: Checkout code
   └─ Tải toàn bộ source code từ GitHub về máy CI

Step 2: Setup .NET 8
   └─ Cài đặt .NET SDK 8.0 + cache NuGet packages

Step 3: Setup Java 17
   └─ Cài JDK (cần cho Allure CLI)

Step 4: Install Allure CLI
   └─ npm install -g allure-commandline

Step 5: Restore packages
   └─ dotnet restore (tải dependencies)

Step 6: ✅ CHẠY TESTS
   └─ dotnet test với:
      • XPlat Code Coverage (đo coverage)
      • TRX logger (xuất kết quả dạng XML)
      • Allure adapter (tạo allure-results/)
   └─ OUTPUT: 245+ tests pass/fail

Step 7: Tạo Allure metadata
   └─ Ghi environment.properties + executor.json
      (ghi lại: branch nào, workflow nào, run số mấy)

Step 8: Generate Allure report
   └─ allure generate → tạo HTML report

Step 9: Upload test artifacts
   └─ Upload lên GitHub: TRX + allure-report (giữ 30 ngày)

Step 10: 📊 GENERATE COVERAGE REPORT
   └─ dotnet-reportgenerator: tạo HTML coverage
   └─ Tạo badges JSON cho shields.io
   └─ Tạo TextSummary + JsonSummary

Step 11: Upload coverage report
   └─ Upload artifact (giữ 30 ngày)

Step 12: 📈 PUBLISH COVERAGE BADGES
   └─ Checkout gh-pages branch
   └─ Push badge JSON vào /badges/
   └─ Hiện trên README qua shields.io

Step 13: 📝 WRITE STEP SUMMARY
   └─ Parse file TRX → đếm Pass/Fail
   └─ Hiện bảng coverage trong GitHub Actions tab
   └─ Bạn thấy bảng đẹp khi mở workflow run

Step 14: 📋 LOG KẾT QUẢ LÊN JIRA
   └─ Parse TRX → lấy số pass/fail
   └─ Gọi Python script: jira_log_test_execution.py
   └─ Script tự động comment lên Jira issue:
      "✅ Backend Tests: 245 passed, 0 failed"

Step 15: Upload Allure results
   └─ Upload allure-results (cho workflow Allure Pages dùng)
```

**Sau khi workflow này PASS → tự động trigger:**
- Workflow #5 (Allure Pages Report)

---

### 🟢 Workflow #2: Frontend E2E (`frontend-e2e.yml`)

```
📌 Trigger:  push main | PR vào main | bấm tay
🖥️ Runner:   Ubuntu (ubuntu-latest)
⏱️ Thời gian: ~3-5 phút
```

**Làm gì:**

```
Step 1: Checkout code
Step 2: Setup Node.js 20
Step 3: npm install (frontend dependencies)
Step 4: Install Playwright Chromium
   └─ Tải trình duyệt headless để test UI

Step 5: ✅ CHẠY E2E TESTS
   └─ npm run e2e:ci
   └─ CodeceptJS mở Chromium → tự click/nhập giống người dùng
   └─ 5 test files, 15+ scenarios:
      • smoke_test.js → trang public có load không
      • citizen_report_test.js → citizen đăng ký, tạo report
      • enterprise_assign_test.js → enterprise assign task
      • collector_task_test.js → collector xem tasks
      • citizen_complaint_test.js → citizen tạo complaint

Step 6: Tạo Allure metadata
   └─ executor.json + environment.properties

Step 7: Generate Allure report

Step 8: Upload E2E artifacts
   └─ Screenshots khi test fail + allure-results

Step 9: 📋 LOG LÊN JIRA
   └─ Parse allure summary → gọi jira_log_test_execution.py
```

**Output:** Screenshots (nếu fail), Allure results (merge vào main report)

---

### 🟡 Workflow #3: SonarCloud Analysis (`sonar.yml`)

```
📌 Trigger:  push main | PR | bấm tay
🖥️ Runner:   Ubuntu
📊 Dashboard: https://sonarcloud.io/summary/overall?id=chi-trung_KCPM
```

**Chạy 2 jobs SONG SONG:**

```
┌─────────────────────────────────┐  ┌──────────────────────────────┐
│ Job 1: sonar-backend (.NET)     │  │ Job 2: sonar-frontend (JS)   │
│                                 │  │                              │
│ 1. Setup .NET 8 + JDK 17       │  │ 1. Setup Node.js 20          │
│ 2. Install SonarScanner         │  │ 2. npm install               │
│ 3. dotnet restore               │  │ 3. npm test --coverage       │
│ 4. Begin SonarCloud scan        │  │ 4. SonarCloud scan           │
│ 5. dotnet build Release         │  │    (sources=frontend/src)    │
│ 6. dotnet test + coverage       │  │    (tests=frontend/e2e)      │
│ 7. End SonarCloud scan          │  │                              │
│    → Upload lên SonarCloud      │  │    → Upload lên SonarCloud   │
└─────────────────────────────────┘  └──────────────────────────────┘
```

**SonarCloud phân tích:**
- 🐛 **Bugs** — lỗi logic tiềm ẩn
- 🔓 **Vulnerabilities** — lỗ hổng bảo mật
- 🧹 **Code Smells** — code khó bảo trì
- 📊 **Coverage** — bao nhiêu % code được test
- ✅ **Quality Gate** — PASSED hay FAILED

---

### 🔴 Workflow #4: Postman Smoke Tests (`postman-smoke.yml`)

```
📌 Trigger:  PR | Hàng ngày 21h UTC | bấm tay
🖥️ Runner:   Ubuntu (PowerShell shell)
🔧 Tools:    Docker + Newman
```

**Flow chi tiết:**

```
Step 1: Checkout code

Step 2: 🔍 TÌM JIRA KEY
   └─ Scan PR title / branch name / commit message
   └─ Tìm pattern KIEM-XX (ví dụ: KIEM-5)
   └─ Lưu vào biến $JIRA_KEY

Step 3-4: Docker Buildx + Build Backend image
   └─ Build Docker image từ Dockerfile
   └─ Cache layer (GHA cache) → build nhanh hơn

Step 5: 🐳 START BACKEND + DATABASE
   └─ docker compose up -d db backend
   └─ Backend chạy trên localhost:8080
   └─ MySQL chạy song song

Step 6: ⏳ CHỜ API SẴN SÀNG
   └─ Retry 60 lần × 5 giây
   └─ Gọi GET http://localhost:8080/api/health
   └─ Chờ đến khi trả 200 OK

Step 7: Install Newman
   └─ npm install -g newman + reporters

Step 8: Chọn scope (smoke / all)
   └─ PR → chỉ smoke (folder "00 - Smoke & Public")
   └─ Manual → chọn full

Step 9: 🔑 PRE-LOGIN
   └─ Gọi POST /api/auth/login với 4 roles:
      • citizen: nguyenvana@gmail.com
      • enterprise: greenlife@gmail.com
      • admin: admin@gmail.com
      • collector: collector1@gmail.com
   └─ Lấy JWT token cho từng role
   └─ Set vào Postman environment variables

Step 10: ✅ CHẠY POSTMAN COLLECTION
   └─ newman run "WastePlatform API - Professional QA Suite"
   └─ Dùng JWT tokens đã lấy ở step 9
   └─ 74 requests, 128 assertions
   └─ Reporters: CLI + JUnit + JSON + Allure

Step 11: Upload results
   └─ JUnit XML + JSON report + Allure results

Step 12: 🔍 XÁC NHẬN JIRA KEY
   └─ Gọi Jira API kiểm tra KIEM-XX tồn tại

Step 13: ✅ COMMENT PASS LÊN JIRA
   └─ Nếu tests pass → tự động comment:
      "✅ Postman Smoke PASSED (74/74)"
   └─ Kèm link GitHub Actions run

Step 14: 🔄 CHUYỂN STATUS JIRA
   └─ Nếu push → transition sang "In Progress"
   └─ Nếu merged → transition sang "Done"

Step 15: ❌ COMMENT FAIL (nếu fail)
   └─ Comment: "❌ Postman Smoke FAILED"

Step 16: Dump Docker logs (nếu fail)
Step 17: docker compose down -v (dọn dẹp)
Step 18: Log to Jira (jira_log_test_execution.py)
```

---

### 📊 Workflow #5: Allure Pages Report (`allure-gh-pages.yml`)

```
📌 Trigger:  Sau "Backend Tests" PASS | bấm tay
🖥️ Runner:   Ubuntu
📊 Output:   https://chi-trung.github.io/KCPM/report-main/
⚙️ Độ phức tạp: 536 dòng YAML — workflow phức tạp nhất!
```

**Đây là workflow QUAN TRỌNG NHẤT cho evidence:**

```
Phase 1: CHẠY LẠI POSTMAN TESTS
   └─ Build Docker → Start Backend → Newman run
   └─ Tạo allure-results/ từ Postman

Phase 2: THU THẬP KẾT QUẢ TỪ 3 NGUỒN
   ┌─────────────────────────────────────────────────┐
   │                                                  │
   │  Nguồn 1: Backend Tests (xUnit)                 │
   │  └─ Download artifact "allure-results"           │
   │     từ workflow Backend Tests gần nhất            │
   │                                                  │
   │  Nguồn 2: Postman API Tests (Newman)             │
   │  └─ Từ Phase 1 vừa chạy ở trên                  │
   │                                                  │
   │  Nguồn 3: Frontend E2E (CodeceptJS)              │
   │  └─ Download artifact "e2e-allure-results"       │
   │     từ workflow Frontend E2E gần nhất             │
   │                                                  │
   └─────────────────────────────────────────────────┘

Phase 3: MERGE + ENRICH
   └─ Copy tất cả allure-results vào 1 thư mục
   └─ Sync owners từ Jira API (ai phụ trách test nào)
   └─ Inject owner vào từng test result
   └─ Normalize suites → 3 nhóm: xUnit / Postman / E2E
   └─ Restore history (20 runs gần nhất) cho trend chart

Phase 4: GENERATE REPORTS
   └─ allure generate → report-main/ (report chính)
   └─ Generate per-owner reports (report riêng từng thành viên)
   └─ Build validation page
   └─ Build site index

Phase 5: DEPLOY LÊN GITHUB PAGES
   └─ Push toàn bộ lên branch gh-pages
   └─ GitHub Pages tự động publish
   └─ Live tại: https://chi-trung.github.io/KCPM/report-main/
```

**Output cuối cùng:**
```
https://chi-trung.github.io/KCPM/
├── report-main/          ← Report chính (3 suites merged)
│   ├── #suites           ← xUnit + Postman + E2E
│   ├── #behaviors        ← Theo feature (Auth, Reports...)
│   ├── #graph            ← Trend chart (20 runs)
│   └── #categories       ← Phân loại lỗi
├── report-extra/
│   └── owners/           ← Report riêng từng thành viên
├── badges/               ← Coverage badges
└── index.html            ← Trang chính
```

---

### 🚀 Workflow #6: CI CD Deploy Server (`deploy-server.yml`)

```
📌 Trigger:  push main | bấm tay (chọn ref)
🖥️ Runner:   Ubuntu
```

**2 Jobs tuần tự (Job 2 chỉ chạy nếu Job 1 pass):**

```
┌─────────────────────────────────────────┐
│ Job 1: QUALITY GATE (phải pass hết)     │
│                                          │
│ ① Backend Tests (dotnet test)            │
│ ② Frontend E2E (CodeceptJS + Playwright) │
│ ③ Postman Smoke (Docker + Newman)        │
│                                          │
│ Tất cả 3 phải PASS → mới deploy         │
└────────────────┬────────────────────────┘
                 │ ✅ PASS
                 ▼
┌─────────────────────────────────────────┐
│ Job 2: DEPLOY TO SERVER                  │
│                                          │
│ ① Kiểm tra secrets (SSH key, host...)   │
│ ② SSH vào server (appleboy/ssh-action)  │
│ ③ git pull origin main                   │
│ ④ Tạo .env từ GitHub Secrets             │
│ ⑤ docker compose up --build              │
│ ⑥ Health check: curl /api/health         │
│    (30 lần retry × 5 giây)               │
│ ⑦ Báo cáo deploy thành công              │
└─────────────────────────────────────────┘
```

---

---

### 🏥 Workflow #7: Health Check (`health-check.yml`)

```
📌 Trigger:  Mỗi 6 giờ tự động | bấm tay
🎯 Mục đích: Monitor uptime + giữ Render free tier không tắt
```

```
Kiểm tra 4 services:

┌──────────────────────────────────────────────────────────────┐
│ Service            │ URL                                      │
│────────────────────│──────────────────────────────────────────│
│ ① Backend API      │ kcpm-backend.onrender.com/api/health    │
│ ② Frontend         │ kcpm.vercel.app                         │
│ ③ Swagger UI       │ kcpm-backend.onrender.com/swagger/...   │
│ ④ Allure Report    │ chi-trung.github.io/KCPM/report-main/   │
└──────────────────────────────────────────────────────────────┘

Output: Step Summary hiện bảng ✅/❌ cho từng service

⚡ Quan trọng: Render Free Tier tắt server sau 15 phút idle.
   Workflow này ping mỗi 6h → giữ server luôn sống.
```

---

### 🔑 Workflow #8: Jira Key Enforcement (`jira-key-enforcement.yml`)

```
📌 Trigger:  Mở PR / Sửa PR / Thêm commit vào PR
🎯 Mục đích: BẮT BUỘC mọi PR + commit phải có Jira key
```

```
Job 1: Kiểm tra PR TITLE
   └─ Regex: /[A-Z][A-Z0-9]+-\d+/
   └─ ✅ "KIEM-123 - Fix bug validation"
   └─ ❌ "Fix bug" → BLOCK merge

Job 2: Kiểm tra MỌI COMMIT trong PR
   └─ Lấy danh sách commits qua GitHub API
   └─ Skip merge commits (tự động)
   └─ Kiểm tra từng commit message có Jira key
   └─ ❌ Nếu thiếu → liệt kê commits sai + BLOCK merge

Ví dụ output khi FAIL:
   "The following commit messages are missing Jira keys:
    - abc1234: fix typo
    - def5678: update readme
    Use format like: KIEM-123: short description"
```

---

### 📋 Workflow #9: Create Jira Issues (`create-jira-issues.yml`)

```
📌 Trigger:  Bấm tay (manual only)
🎯 Mục đích: Tạo Jira issues tự động từ test plan
```

```
Step 1: Checkout code
Step 2: Setup Python 3.11
Step 3: pip install requests
Step 4: Chạy scripts/create_jira_issues.py
   └─ Đọc test plan definition
   └─ Gọi Jira API tạo issues
   └─ Optional: link vào Epic (input: epic_issue_key)
```

---

---

## ⛓️ Chuỗi Phụ Thuộc — Cái Nào Trigger Cái Nào

```
┌─────────────────────────────────────────────────────────────┐
│                    KHI PUSH CODE LÊN MAIN                    │
│                                                              │
│  Tự động chạy ĐỒNG THỜI (4 workflows):                     │
│                                                              │
│  ① Backend Tests ──────┐                                    │
│  ② Frontend E2E        │                                    │
│  ③ SonarCloud          │                                    │
│  ④ CI CD Deploy Server │                                    │
│                        │                                     │
│  Vercel auto-deploy ───┼── Frontend live (~1 phút)          │
│                        │                                     │
│  Sau ① PASS (~5 phút): │                                    │
│  └── ⑤ Allure Pages ──┼── Report published (~10 phút)      │
│                                                              │
│  TỔNG THỜI GIAN: ~15 phút từ push → production              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    KHI MỞ PULL REQUEST                        │
│                                                              │
│  ① Backend Tests                                             │
│  ② Frontend E2E                                              │
│  ③ SonarCloud                                                │
│  ④ Postman Smoke (chạy smoke folder)                        │
│  ⑨ Jira Key Enforcement (block merge nếu thiếu key)        │
│                                                              │
│  → PHẢI pass hết mới được merge vào main                    │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    TỰ ĐỘNG THEO LỊCH                         │
│                                                              │
│  Mỗi 6 giờ:      ⑧ Health Check (ping 4 services)          │
│  21:00 UTC daily: ① Backend Tests + ④ Postman Smoke         │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    BẤM TAY (MANUAL)                           │
│                                                              │
│  ⑨ Create Jira Issues                                       │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 Timeline Khi Push 1 Commit

```
t = 0s      👨‍💻 git push origin main
               │
t = 5s      GitHub Actions khởi động:
               ├── ① Backend Tests (Windows, ~5-7 min)
               ├── ② Frontend E2E (Ubuntu, ~3-5 min)
               ├── ③ SonarCloud (Ubuntu, ~4-6 min)
               └── ④ CI CD Deploy Server (Ubuntu, ~10 min)
               │
               Vercel nhận webhook → build frontend
               │
t = 60s     🟢 Vercel: Frontend LIVE ✅
               │
t = 3 min   🟢 Frontend E2E: PASS ✅ (15+ scenarios)
               │
t = 5 min   🟢 SonarCloud: Quality Gate PASSED ✅
               │
t = 6 min   🟢 Backend Tests: 245+ tests PASSED ✅
               │   ├── Coverage badges pushed to gh-pages
               │   ├── Jira comment: "245 passed, 0 failed"
               │   └── Triggers:
               │       └── ⑤ Allure Pages Report
               │
               t = 10 min  ④ CI CD Deploy: Quality gate pass → SSH deploy ✅
               │
               t = 12 min  🟢 Allure Pages: Report published ✅
               └── Live: chi-trung.github.io/KCPM/report-main/

t = 12 min  ✅ TẤT CẢ HOÀN TẤT
               • Frontend: kcpm.vercel.app ✅
               • Backend: kcpm-backend.onrender.com ✅
               • Report: chi-trung.github.io/KCPM ✅
               • SonarCloud: Quality Gate ✅
               • Jira: Auto-commented ✅
```

---

## 🔐 GitHub Secrets Sử Dụng

| Secret | Dùng cho workflow | Lấy từ đâu |
|--------|------------------|-------------|
| `SONAR_TOKEN` | #3 SonarCloud | sonarcloud.io → My Account → Security |
| `JIRA_BASE_URL` | #1,2,4,5 | `JIRA_BASE_URL` |
| `JIRA_API_EMAIL` | #1,2,4,5,11 | Email tài khoản Atlassian |
| `JIRA_API_TOKEN` | #1,2,4,5,11 | Atlassian → Account → API tokens |
| `DEPLOY_HOST` | #6 Deploy Server | IP server SSH |
| `DEPLOY_USER` | #6 Deploy Server | SSH username |
| `DEPLOY_SSH_KEY` | #6 Deploy Server | SSH private key |

---

## 🗣️ Script Nói Cho Thầy

### Mở đầu:
> "Thưa thầy, hệ thống CI/CD của nhóm em gồm **9 GitHub Actions workflows** tự động hóa toàn bộ quy trình kiểm thử, triển khai, và báo cáo."

### Giải thích luồng chính:
> "Khi developer push code lên nhánh main, GitHub Actions **tự động** chạy 4 workflows đồng thời:
> 1. **Backend Tests** — chạy 245+ unit tests bằng xUnit, đo code coverage, log kết quả lên Jira
> 2. **Frontend E2E** — mở trình duyệt headless bằng Playwright, chạy 15+ E2E scenarios
> 3. **SonarCloud** — quét static analysis: bugs, vulnerabilities, code smells
> 4. **CI/CD Deploy Server** — chạy quality gate (3 loại test phải pass) rồi mới deploy"

### Giải thích chuỗi phụ thuộc:
> "Sau khi Backend Tests **pass**, tự động trigger:
> - **Allure Pages** — thu thập kết quả từ **3 nguồn** (xUnit + Postman + E2E), merge thành 1 report đẹp, deploy lên GitHub Pages"

### Giải thích schedule:
> "Ngoài ra, **mỗi 6 giờ** có Health Check tự động ping 4 services kiểm tra uptime. **Hàng ngày 21h** chạy lại Backend Tests + Postman Smoke để phát hiện regression."

### Giải thích PR guard:
> "Khi mở Pull Request, workflow **Jira Key Enforcement** kiểm tra: PR title và mọi commit message **bắt buộc** phải có Jira key (VD: KIEM-5). Thiếu → không cho merge."

### Kết:
> "Tổng cộng, từ lúc push code đến khi app live trên production mất khoảng **15 phút**, hoàn toàn tự động, có evidence đầy đủ trên Allure Report và Jira."

---

## 📱 Demo Links Nhanh

| Mục | URL |
|-----|-----|
| Frontend | https://kcpm.vercel.app |
| Backend Swagger | https://kcpm-backend.onrender.com/swagger |
| GitHub Actions | https://github.com/chi-trung/KCPM/actions |
| Allure Report | https://chi-trung.github.io/KCPM/report-main/ |
| SonarCloud | https://sonarcloud.io/summary/overall?id=chi-trung_KCPM |
| Jira Board | JIRA_BASE_URL/jira/software/projects/KIEM |

### Tài khoản demo

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@gmail.com | password |
| Citizen | nguyenvana@gmail.com | password |
| Enterprise | greenlife@gmail.com | password |
| Collector | collector1@gmail.com | password |

---

*Tài liệu này được tạo cho mục đích demo. Xem thêm: [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) | [CI_CD_WORKFLOWS.md](./CI_CD_WORKFLOWS.md)*
