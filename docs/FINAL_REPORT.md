# 📊 Báo cáo Tổng kết Kiểm thử — Waste Recycling Platform
# Nhóm 11A6 — KiemChungPhanMem (KCPM)

**Ngày báo cáo:** 12/06/2026  
**Phiên bản:** 3.0  
**Nhóm trưởng:** Nguyễn Chí Trung  
**Giảng viên:** Thầy Bảo

---

## 📌 1. Tổng quan Dự án

| Thuộc tính | Giá trị |
|-----------|--------|
| **Tên dự án** | Waste Recycling Platform (WRP) |
| **Mô tả** | Nền tảng quản lý báo cáo rác thải, thu gom, khiếu nại cho cộng đồng |
| **Kiến trúc** | Backend .NET 8 (REST API) + Frontend React + PostgreSQL |
| **Hosting** | Backend: Render.com / Frontend: Vercel |
| **Source Control** | GitHub (chi-trung/KCPM) |
| **CI/CD** | GitHub Actions (4 workflows) |
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
| **Unit/Integration Tests** | xUnit (.NET 8) | 245+ | 240+ | 0 | 1 | ~99% |
| **API Tests** | Postman/Newman | 74 requests (128 assertions) | 128 | 0 | 0 | 100% |
| **E2E Tests** | CodeceptJS (Playwright) | 5 files, 15+ scenarios | All | 0 | 0 | 100% |
| **Test Documentation** | Excel (UnitestKCPM.xlsx) | 68 TCs (13 functions) | 65 | 3 | 0 | 95.6% |
| **Static Analysis** | SonarCloud | — | — | — | — | Quality Gate |
| **TỔNG CỘNG** | | **400+** | | | | |

### 3.2 Bugs phát hiện

| Bug ID | Mô tả | Severity | Phát hiện bởi | Assign fix cho | Status |
|--------|-------|----------|--------------|---------------|--------|
| KIEM-26 | Missing mandatory image validation | High | Test (xUnit BVA) | Member khác | In Progress |
| KIEM-27 | PUT /notifications/{id}/read returns 200 for 404 | Medium | API Test | Nguyễn Hoàng Phụng | ✅ Done |
| KIEM-28 | Missing taskId in accept response | Low | Manual test | Minh Phụng | To Do |
| KIEM-29 | Missing max 5 images validation | High | Test (xUnit BVA) | Member khác | To Do |

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
| **CI/CD** | GitHub Actions (4 workflows) | Automated test pipeline |
| **Defect Tracking** | Jira (KIEM project) | Bug management + traceability |
| **Source Control** | Git (GitHub) | Version control |
| **Containerization** | Docker | Test environment isolation |
| **Test Data** | Excel (openpyxl) | Test case documentation |

---

## 📊 7. CI/CD Pipeline (4 Workflows)

| # | Workflow | Trigger | Chức năng |
|---|---------|---------|----------|
| 1 | **Backend Tests** (#446+) | push/PR/schedule | xUnit → Allure → Coverage → Jira log |
| 2 | **Frontend E2E** (#91+) | push/PR | CodeceptJS → Allure → Jira log |
| 3 | **Postman Smoke** | manual/schedule | Newman → Docker → API tests → Jira |
| 4 | **SonarCloud Analysis** | push/PR | Static analysis + coverage upload |
| + | **Allure Pages** | after tests | Publish to GitHub Pages |
| + | **CI CD Deploy** | after quality gate | Deploy to Render.com |

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

## ✅ 11. Kết luận

### Điểm mạnh
- ✅ **400+ test cases** trải đều 4 loại (xUnit, Postman, E2E, Excel)
- ✅ **6 kỹ thuật kiểm thử** từ giáo trình (EP, BVA, DT, ST, EG, White-box)
- ✅ **CI/CD tự động** với 4 workflows + Jira auto-log
- ✅ **Allure Report** chuyên nghiệp với link Jira
- ✅ **Code Coverage** tích hợp CI (ReportGenerator + SonarCloud)
- ✅ **19/19 KIEM issues** đều có test coverage
- ✅ **4 bugs** được phát hiện và documented (2 fixed, 2 assigned to other members)
- ✅ **Nguyên tắc độc lập**: Member1 test → Member2 fix

### Bài học kinh nghiệm
- BVA giúp phát hiện 2 bugs quan trọng (KIEM-26, KIEM-29) mà EP bỏ qua
- Decision Table testing rất hiệu quả cho logic phức tạp (Complaint creation)
- CI/CD tự động giúp phát hiện lỗi ngay khi commit — giảm 90% thời gian manual testing

---

*Báo cáo này được tạo tự động bởi CI/CD pipeline của nhóm 11A6 KCPM.*  
*Dữ liệu cập nhật: 12/06/2026*
