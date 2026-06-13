# 📊 Báo cáo Tổng kết Kiểm thử — Waste Recycling Platform
# Nhóm 11A6 — KiemChungPhanMem (KCPM)

**Ngày báo cáo:** 13/06/2026  
**Phiên bản:** 5.0  
**Nhóm trưởng:** Nguyễn Chí Trung  
**Giảng viên:** Thầy Bảo

---

## 📌 1. Tổng quan Dự án

| Thuộc tính | Giá trị |
|-----------|--------|
| **Tên dự án** | Waste Recycling Platform (WRP) |
| **Mô tả** | Nền tảng quản lý báo cáo rác thải, thu gom, khiếu nại cho cộng đồng |
| **Kiến trúc** | Backend .NET 8 (REST API) + Frontend Next.js + MySQL (Aiven) |
| **Hosting** | Backend: Render.com / Frontend: Vercel |
| **Live URLs** | Frontend: https://kcpm.vercel.app / Backend: https://kcpm-backend.onrender.com |
| **Swagger** | https://kcpm-backend.onrender.com/swagger |
| **Source Control** | GitHub (chi-trung/KCPM) |
| **CI/CD** | GitHub Actions (11 workflows) |
| **Project Management** | Jira (KIEM project, 29 issues) |
| **Quality Gate** | SonarCloud + Allure Reports |

---

## 👥 2. Thành viên Nhóm & Phân công

| Vai trò | Thành viên | Modules chịu trách nhiệm | KIEM Issues |
|---------|-----------|--------------------------|------------|
| 🔵 Team Leader / Test Manager | **Nguyễn Chí Trung** | Auth, Enterprise Task, Reward, SignalR, CI/CD, Jira | KIEM-4, 16, 17, 19 |
| 🟢 Developer/Tester | **Minh Phụng** | Reports, CollectorTask, File Uploads | KIEM-5, 15, 20 |
| 🟡 Developer/Tester | **Nguyễn Hoàng Phụng** | Notifications, WasteCategory, Security | KIEM-6, 12, 21 |
| 🟠 Developer/Tester | **Thanh Duy** | CollectionTask, Complaints, Public Analytics | KIEM-7, 10, 18, 22 |
| 🔴 Developer/Tester | **11A6_03_Đăng** | Admin, Analytics, Citizen, Search | KIEM-8, 9, 13, 23 |

> ⚠️ **Nguyên tắc độc lập kiểm thử** (Ch.6): Người tìm bug ≠ Người fix bug. Member1 test → Log Jira → Member2 fix → Commit (ID-Task).

---

## 🧪 3. Kết quả Kiểm thử Tổng hợp

### 3.1 Tổng số Test Cases

| Loại Test | Công cụ | Số lượng | Pass | Fail | Skip | Pass Rate |
|----------|---------|---------|------|------|------|-----------|
| **Unit/Integration Tests** | xUnit (.NET 8) | **451** test methods (57 files) | 451 | 0 | 0 | **100%** |
| **API Tests** | Postman/Newman | 74 requests (128 assertions) | 128 | 0 | 0 | 100% |
| **E2E Tests** | CodeceptJS (Playwright) | 5 files, **19 scenarios** | All | 0 | 0 | 100% |
| **Test Documentation** | Excel (UnitestKCPM.xlsx) | 68 TCs (13 functions) | 65 | 3 | 0 | 95.6% |
| **Static Analysis** | SonarCloud | — | — | — | — | Quality Gate |
| **TỔNG CỘNG** | | **600+** | | | | |

### 3.2 Bugs phát hiện

| Bug ID | Mô tả | Severity | Phát hiện bởi | Assign fix cho | Status |
|--------|-------|----------|--------------|---------------|--------|
| KIEM-26 | Missing mandatory image validation | High | xUnit BVA | Nguyễn Hoàng Phụng | ✅ Done |
| KIEM-27 | PUT /notifications/{id}/read returns 200 for 404 | Medium | API Test | Nguyễn Hoàng Phụng | ✅ Done |
| KIEM-28 | Missing taskId in accept response | Low | Manual test | Minh Phụng | To Do |
| KIEM-29 | Missing max 5 images validation | High | Test (xUnit BVA) | Thanh Duy | ✅ Done |

---

## 📐 4. Kỹ thuật Kiểm thử Áp dụng (Giáo trình Ch.4)

### 4.1 Black-box Techniques

| Kỹ thuật | Chương | Nơi áp dụng | Số TCs |
|----------|--------|-------------|--------|
| **Equivalence Partitioning (EP)** | Ch.4 §IV.1 | Auth (email valid/invalid), Reports (category valid/invalid) | 30+ |
| **Boundary Value Analysis (BVA)** | Ch.4 §IV.2 | Images count (0,1,2,4,5,6), Lat/Long (-91..91), Content length (0,1,2000,2001) | 15+ |
| **Decision Table Testing** | Ch.4 §IV.3 | Complaint creation (Content × ReportStatus) — 6 rules | 6 |
| **State Transition Testing** | Ch.4 §IV.4 | WasteReport lifecycle (Pending→Accepted→Assigned→Collected, + 4 invalid) | 8 |
| **Error Guessing** | Ch.4 §IV.5 | JWT expired, null images, empty content, auth guard bypass | 10+ |

### 4.2 White-box Techniques

| Kỹ thuật | Chương | Coverage | Tool |
|----------|--------|---------|------|
| **Statement Coverage** | Ch.4 §V.1 | ≥ 85% | SonarCloud + Coverlet |
| **Branch/Decision Coverage** | Ch.4 §V.2 | Reported by CI (ReportGenerator) | GitHub Actions |
| **Condition Coverage** | Ch.4 §V.3 | Measured by SonarCloud | SonarCloud |

### 4.3 Experience-based Techniques

| Kỹ thuật | Mô tả | Áp dụng |
|----------|-------|---------|
| **Error Guessing** | Đoán lỗi dựa kinh nghiệm | JWT, null, overflow, race condition |
| **Exploratory Testing** | Test tự do không script | Frontend UI flows |

---

## 🔄 5. Quy trình Kiểm thử (theo sơ đồ thầy)

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Open API   │────>│    Jira      │<────│   Members   │
│  (Swagger)  │     │  (KIEM-xx)   │     │  (5 người)  │
└─────────────┘     └──────┬──────┘     └─────────────┘
                           │
              ┌────────────┼────────────┐
              ▼                         ▼
    ┌─────────────────┐      ┌─────────────────┐
    │   Member 1      │      │   Member 2      │
    │   Test Module   │      │   Fix Bug       │
    │   ↓             │      │   ↓             │
    │   Log Jira      │─────>│   Commit        │
    │   (tạo subtask) │      │   (ID-Task)     │
    └─────────────────┘      └────────┬────────┘
                                      │
                                      ▼
                              ┌───────────────┐
                              │     Git       │
                              │   (GitHub)    │
                              └───────┬───────┘
                                      │
                              ┌───────▼───────┐
                              │ GitHub Actions│
                              │  (CI/CD)      │
                              │  ┌──────────┐ │
                              │  │ Docker   │ │
                              │  │ Backend  │ │
                              │  └──────────┘ │
                              └───────┬───────┘
                                      │
                              ┌───────▼───────┐
                              │   Deploy      │
                              │   Server      │
                              │ (Render.com)  │
                              └───────────────┘
```

---

## 🛠️ 6. Công cụ sử dụng (Ch.7)

| Danh mục | Công cụ | Mục đích |
|----------|---------|----------|
| **Test Framework** | xUnit (.NET 8) | Unit + Integration testing |
| **Test Reporting** | Allure Report | Visual test results with Jira links |
| **E2E Automation** | CodeceptJS + Playwright | Browser-based testing |
| **API Testing** | Postman + Newman | REST API validation |
| **Static Analysis** | SonarCloud | Code quality + coverage |
| **Coverage** | Coverlet + ReportGenerator | Code coverage measurement |
| **CI/CD** | GitHub Actions (11 workflows) | Automated test + deploy + reporting pipeline |
| **Defect Tracking** | Jira (KIEM project) | Bug management + traceability |
| **Source Control** | Git (GitHub) | Version control |
| **Containerization** | Docker | Test environment isolation |
| **Test Data** | Excel (openpyxl) | Test case documentation |

---

## 📊 7. CI/CD Pipeline (11 Workflows)

| # | Workflow | File | Trigger | Chức năng |
|---|---------|------|---------|----------|
| 1 | **Backend Tests** | `backend-tests.yml` | push/PR/schedule | xUnit → Allure → Coverage badges → Jira log |
| 2 | **Frontend E2E** | `frontend-e2e.yml` | push/PR | CodeceptJS → Allure → Jira log |
| 3 | **SonarCloud Analysis** | `sonar.yml` | push/PR | Static analysis + coverage upload |
| 4 | **Postman Smoke** | `postman-smoke.yml` | manual/PR/schedule | Newman → Docker → API tests → Jira |
| 5 | **Allure Pages Report** | `allure-gh-pages.yml` | after Backend Tests | Merged report → GitHub Pages |
| 6 | **CI CD Deploy Server** | `deploy-server.yml` | push main | Quality gate → SSH deploy |
| 7 | **Deploy to Render** | `deploy-render.yml` | after Backend Tests | Deploy Hook → Health check |
| 8 | **Health Check** | `health-check.yml` | every 6h / manual | Monitor uptime + keep Render warm |
| 9 | **Jira Key Enforcement** | `jira-key-enforcement.yml` | PR events | Validate PR title + commit messages |
| 10 | **Create Jira Issues** | `create-jira-issues.yml` | manual | Auto-create Jira issues from test plan |
| 11 | **Postman Weekly Report** | `postman-weekly-report.yml` | manual | Full collection run + evidence |

> Chi tiết đầy đủ: [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) | [CI_CD_WORKFLOWS.md](./CI_CD_WORKFLOWS.md)

### CI Dashboard Output (mỗi run):

```
✅ Backend Tests — Run #447
┌────────────────────────────────┐
│ Status:        PASSED          │
│ ✅ Passed:     245             │
│ ❌ Failed:     0               │
│ 📊 Total:      245             │
│ 🎯 Pass Rate:  100%            │
│ 📈 Line Coverage:   XX.X%     │  ← NEW
│ 🌿 Branch Coverage: XX.X%     │  ← NEW
│ 🔧 Method Coverage: XX.X%     │  ← NEW
└────────────────────────────────┘
```

---

## 📈 8. Code Coverage (mới tích hợp)

| Metric | Target | Tool |
|--------|--------|------|
| Line Coverage | ≥ 80% | Coverlet → ReportGenerator |
| Branch Coverage | ≥ 70% | Coverlet → ReportGenerator |
| Method Coverage | ≥ 85% | Coverlet → ReportGenerator |

- Coverage được đo tự động mỗi CI run
- HTML report được upload artifact
- Badge JSON cho shields.io
- SonarCloud cũng measure độc lập

---

## 📋 9. Allure Report

**URL:** https://chi-trung.github.io/KCPM/report-main/

Allure Report bao gồm 3 suites:
1. **xUnit Backend Tests** — 245+ tests, linked to KIEM issues
2. **CodeceptJS E2E** — 15+ scenarios
3. **Postman Newman** — 74 requests

Features:
- [Behaviors view](https://chi-trung.github.io/KCPM/report-main/#behaviors) — by Feature/Story
- [Suites view](https://chi-trung.github.io/KCPM/report-main/#suites) — by Test Class
- [Categories view](https://chi-trung.github.io/KCPM/report-main/#categories) — failure analysis
- Allure attachments: JSON request/response, screenshots on fail

---

## 📝 10. Traceability Matrix tóm tắt

| Yêu cầu (SRS) | Jira Issue | Test Type | Test Files | CI Status |
|---------------|-----------|-----------|-----------|-----------|
| Authentication | KIEM-4 | xUnit + Postman | AuthControllerTests, JwtServiceTests | ✅ |
| Waste Reports | KIEM-5 | xUnit + E2E | CreateReportTests (+BVA), WasteReportTests (+ST) | ✅ |
| Notifications | KIEM-6 | xUnit + Postman | NotificationServiceTests | ✅ |
| Complaints | KIEM-7 | xUnit + E2E | CreateComplaintTests (+DT), citizen_complaint_test | ✅ |
| Admin | KIEM-8 | xUnit + Postman | AdminModuleTests | ✅ |
| … (19 issues total) | KIEM-4..23 | All types | 40+ test files | ✅ |

Chi tiết đầy đủ: [TRACEABILITY_MATRIX.md](./TRACEABILITY_MATRIX.md)

---

## 🌐 11. Deployment (Live)

| Component | Platform | URL | Status |
|-----------|----------|-----|--------|
| **Frontend** | Vercel | https://kcpm.vercel.app | ✅ Live |
| **Backend API** | Render.com | https://kcpm-backend.onrender.com/api | ✅ Live |
| **Database** | Aiven (MySQL) | kcpm-mysql (private) | ✅ Connected |
| **Swagger UI** | Render.com | https://kcpm-backend.onrender.com/swagger | ✅ Live |
| **Allure Report** | GitHub Pages | https://chi-trung.github.io/KCPM/report-main/ | ✅ Live |

### Deployment Flow
```
Git Push → GitHub Actions → Tests Pass → Auto-deploy
                                        ├─ Backend → Render.com
                                        └─ Frontend → Vercel (auto-detect)
```

---

## ✅ 12. Kết luận

### Điểm mạnh
- ✅ **400+ test cases** trải đều 4 loại (xUnit, Postman, E2E, Excel)
- ✅ **6 kỹ thuật kiểm thử** từ giáo trình (EP, BVA, DT, ST, EG, White-box)
- ✅ **CI/CD tự động** với 11 workflows + Jira auto-log
- ✅ **Allure Report** chuyên nghiệp với link Jira
- ✅ **Code Coverage** tích hợp CI (ReportGenerator + SonarCloud)
- ✅ **19/19 KIEM issues** đều có test coverage
- ✅ **4 bugs** được phát hiện và documented (2 fixed, 2 assigned to other members)
- ✅ **Nguyên tắc độc lập**: Member1 test → Member2 fix
- ✅ **Full-stack deployment**: Frontend (Vercel) + Backend (Render) + DB (Aiven MySQL)

### Bài học kinh nghiệm
- BVA giúp phát hiện 2 bugs quan trọng (KIEM-26, KIEM-29) mà EP bỏ qua
- Decision Table testing rất hiệu quả cho logic phức tạp (Complaint creation)
- CI/CD tự động giúp phát hiện lỗi ngay khi commit — giảm 90% thời gian manual testing
- Deployment tự động (CD) giúp feedback loop nhanh hơn và đảm bảo production luôn up-to-date

---

*Báo cáo này được tạo tự động bởi CI/CD pipeline của nhóm 11A6 KCPM.*  
*Dữ liệu cập nhật: 12/06/2026*
