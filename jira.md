# Jira Sprint Issues — Waste Recycling Platform (KCPM)

## Epic: WRP Testing & Quality Assurance

---

## Sprint 1: Test Planning & Infrastructure Setup

### [Task] KIEM-SP1-01: Thiết lập CI/CD Pipeline với GitHub Actions
- **Assignee**: Nguyễn Chí Trung
- **Priority**: High
- **Labels**: sprint-1, ci-cd, infrastructure
- **Description**:
  Thiết lập 9 GitHub Actions workflows cho dự án:
  1. backend-tests.yml — Chạy 451 xUnit tests
  2. frontend-e2e.yml — Chạy 19 E2E scenarios (CodeceptJS + Playwright)
  3. sonar.yml — SonarCloud static analysis
  4. deploy-server.yml — CI/CD deploy pipeline
  5. allure-gh-pages.yml — Tạo Allure report trên GitHub Pages
  6. postman-smoke.yml — API smoke tests (Newman)
  7. health-check.yml — Health check mỗi 6 giờ
  8. jira-key-enforcement.yml — Validate PR title
  9. create-jira-issues.yml — Auto-create Jira issues
  
  **Acceptance Criteria:**
  - [ ] Tất cả 9 workflows chạy thành công trên main
  - [ ] CI tự động trigger khi push code
  - [ ] Allure report publish lên GitHub Pages

### [Task] KIEM-SP1-02: Thiết lập Test Plan & Testing Strategy
- **Assignee**: Nguyễn Chí Trung
- **Priority**: High
- **Labels**: sprint-1, documentation, test-plan
- **Description**:
  Tạo Test Plan và Testing Strategy cho dự án, bao gồm:
  - Scope of testing
  - Test levels (Unit, Integration, E2E, API)
  - Test types (Functional, Non-functional, Security)
  - Entry/Exit criteria
  - Test environment
  - Tools: xUnit, CodeceptJS, Postman/Newman, SonarCloud, Allure
  
  **Files:**
  - `docs/TEST_PLAN.md`
  - `docs/TESTING_STRATEGY.md`
  
  **Acceptance Criteria:**
  - [ ] Test Plan có đầy đủ sections theo template chuẩn
  - [ ] Testing Strategy áp dụng kỹ thuật Ch.4 giáo trình

### [Task] KIEM-SP1-03: Deploy Production Environment
- **Assignee**: Nguyễn Chí Trung
- **Priority**: High
- **Labels**: sprint-1, deployment, infrastructure
- **Description**:
  Deploy full-stack application lên production:
  - Backend: Render.com (Docker, .NET 8)
  - Frontend: Vercel (Next.js)
  - Database: Aiven MySQL (free tier)
  - Seed data: 5 categories, 8 accounts, enterprise/collector profiles
  
  **URLs:**
  - Frontend: https://kcpm.vercel.app
  - Backend: https://kcpm-backend.onrender.com
  - Swagger: https://kcpm-backend.onrender.com/swagger
  
  **Acceptance Criteria:**
  - [ ] Admin login thành công
  - [ ] CRUD operations hoạt động
  - [ ] Health check endpoint trả về 200

### [Task] KIEM-SP1-04: Thiết lập Jira Project & Traceability Matrix
- **Assignee**: Nguyễn Chí Trung
- **Priority**: Medium
- **Labels**: sprint-1, documentation, jira
- **Description**:
  Thiết lập Jira project KIEM với:
  - Sprint board (Kanban)
  - Issue types: Task, Bug, Story
  - Tạo Traceability Matrix mapping: Requirement → Jira → Test Case → CI Evidence
  - Tự động log CI results lên Jira comments
  
  **Files:**
  - `docs/TRACEABILITY_MATRIX.md`
  - `scripts/jira_log_test_execution.py` (in backend-tests.yml)
  
  **Acceptance Criteria:**
  - [ ] Ma trận truy vết đầy đủ cho tất cả modules
  - [ ] CI tự động log kết quả lên Jira issues

### [Task] KIEM-SP1-05: Thiết lập Postman Collection cho API Testing
- **Assignee**: Minh Phụng
- **Priority**: Medium
- **Labels**: sprint-1, api-testing, postman
- **Description**:
  Tạo Postman Collection "WastePlatform API - Professional QA Suite" với:
  - 10 folders (Auth, Admin, WasteCategory, Analytics, Reports, Citizen, Collector, CollectorTask, Notifications, Complaints)
  - 74 requests, 128 assertions
  - Environment variables cho auth token
  - Pre-request scripts cho auto-login
  
  **Acceptance Criteria:**
  - [ ] Newman chạy thành công trên CI (0 failures)
  - [ ] Tất cả folders có assertions
  - [ ] Collection export ở `Waste-Recycling-Platform/postman/`

---

## Sprint 2: Test Development & Execution

### [Task] KIEM-SP2-01: Viết Unit Tests cho Auth Module
- **Assignee**: Nguyễn Chí Trung
- **Priority**: High
- **Labels**: sprint-2, unit-test, auth
- **Description**:
  Viết xUnit tests cho Auth module, áp dụng kỹ thuật:
  - **Equivalence Partitioning (EP)**: valid/invalid email, password
  - **Error Guessing**: JWT expired, malformed token, null input
  
  **Test files:**
  - `AuthControllerTests.cs`
  - `JwtServiceTests.cs`
  - `JwtBearerIntegrationTests.cs`
  
  **Jira**: KIEM-4
  
  **Branch**: `feature/KIEM-4-auth-tests`
  
  **Acceptance Criteria:**
  - [ ] ≥ 20 test methods
  - [ ] Tất cả pass trên CI
  - [ ] Allure report hiển thị đúng

### [Task] KIEM-SP2-02: Viết Unit Tests cho Reports Module (BVA + State Transition)
- **Assignee**: Minh Phụng
- **Priority**: High
- **Labels**: sprint-2, unit-test, reports, bva, state-transition
- **Description**:
  Viết xUnit tests cho Reports module, áp dụng kỹ thuật:
  - **Boundary Value Analysis (BVA)**: images count (0, 1, 5, 6), lat/long values
  - **State Transition Testing**: report lifecycle (Pending → Accepted → Assigned → Completed/Rejected)
  
  **Test files:**
  - `CreateReportCommandHandlerTests.cs` (BVA-02..07)
  - `AcceptReportCommandHandlerTests.cs`
  - `WasteReportTests.cs` (ST-05/07/08)
  
  **Jira**: KIEM-5
  
  **Branch**: `feature/KIEM-5-reports-tests`
  
  **Acceptance Criteria:**
  - [ ] BVA tests cho boundary values (0, 1, 5, 6 images)
  - [ ] State transition tests cho tất cả valid/invalid transitions
  - [ ] ≥ 30 test methods

### [Task] KIEM-SP2-03: Viết Unit Tests cho Notifications Module
- **Assignee**: Nguyễn Hoàng Phụng
- **Priority**: High
- **Labels**: sprint-2, unit-test, notifications
- **Description**:
  Viết xUnit tests cho Notifications module:
  - **EP**: valid/invalid notification IDs
  - **Error Guessing**: mark-as-read for 404, unauthorized access
  
  **Test files:**
  - `NotificationServiceTests.cs`
  - `NotificationControllerTests.cs`
  - `NotificationRepositoryTests.cs`
  
  **Jira**: KIEM-6
  
  **Branch**: `feature/KIEM-6-notification-tests`
  
  **Acceptance Criteria:**
  - [ ] ≥ 15 test methods
  - [ ] Repository + Service + Controller layers tested
  - [ ] Bug KIEM-27 covered by test

### [Task] KIEM-SP2-04: Viết Unit Tests cho Complaints Module (Decision Table)
- **Assignee**: Thanh Duy
- **Priority**: High
- **Labels**: sprint-2, unit-test, complaints, decision-table
- **Description**:
  Viết xUnit tests cho Complaints module, áp dụng kỹ thuật:
  - **Decision Table Testing (DT)**: 6 combinations of complaint creation conditions
  - **Error Guessing**: empty description, null enterprise
  
  **Test files:**
  - `CreateComplaintCommandHandlerTests.cs` (DT-01..06)
  - `RejectComplaintCommandHandlerTests.cs`
  - `ResolveComplaintCommandHandlerTests.cs`
  
  **Jira**: KIEM-7
  
  **Branch**: `feature/KIEM-7-complaint-tests`
  
  **Acceptance Criteria:**
  - [ ] Decision table có ≥ 6 test cases
  - [ ] Tất cả pass trên CI

### [Task] KIEM-SP2-05: Viết Unit Tests cho Admin + Analytics Module
- **Assignee**: Đăng
- **Priority**: High
- **Labels**: sprint-2, unit-test, admin, analytics
- **Description**:
  Viết xUnit tests cho Admin và Analytics modules:
  - **EP**: valid/invalid admin operations
  - **Error Guessing**: unauthorized access, missing data
  
  **Test files:**
  - `AdminModuleTests.cs`
  - `AdminApiIntegrationTests.cs`
  - `AnalyticsModuleTests.cs`
  - `AnalyticsApiIntegrationTests.cs`
  
  **Jira**: KIEM-8, KIEM-9
  
  **Branch**: `feature/KIEM-8-admin-analytics-tests`
  
  **Acceptance Criteria:**
  - [ ] ≥ 25 test methods (Admin + Analytics combined)
  - [ ] Integration tests cho API endpoints

### [Task] KIEM-SP2-06: Viết E2E Tests (CodeceptJS + Playwright)
- **Assignee**: Nguyễn Chí Trung
- **Priority**: High
- **Labels**: sprint-2, e2e, codeceptjs, playwright
- **Description**:
  Viết 5 E2E test files với 19 scenarios:
  1. `smoke_test.js` — Public pages, auth entry points
  2. `citizen_report_test.js` — Citizen registration, create report
  3. `enterprise_assign_test.js` — Enterprise login, task management
  4. `collector_task_test.js` — Collector login, task access
  5. `citizen_complaint_test.js` — Complaint flow (DT + Error Guessing)
  
  **Branch**: `feature/KIEM-e2e-tests`
  
  **Acceptance Criteria:**
  - [ ] 19 scenarios pass trên CI
  - [ ] Allure report hiển thị E2E suite
  - [ ] Screenshots on failure

### [Task] KIEM-SP2-07: Viết Unit Tests cho WasteCategory + Security
- **Assignee**: Nguyễn Hoàng Phụng
- **Priority**: Medium
- **Labels**: sprint-2, unit-test, category, security
- **Description**:
  Viết xUnit tests cho WasteCategory và Security modules:
  
  **Test files:**
  - `WasteCategoryControllerTests.cs`
  - `GetAllCategoriesQueryHandlerTests.cs`
  - `AdminEnterpriseAuthorizationTests.cs`
  - `JwtBearerIntegrationTests.cs`
  
  **Jira**: KIEM-12, KIEM-21
  
  **Branch**: `feature/KIEM-12-category-security-tests`
  
  **Acceptance Criteria:**
  - [ ] ≥ 20 test methods
  - [ ] Role-based access tests (Admin, Enterprise, Citizen)

### [Task] KIEM-SP2-08: Viết Unit Tests cho CollectorTask + File Uploads
- **Assignee**: Minh Phụng
- **Priority**: Medium
- **Labels**: sprint-2, unit-test, collector, file-upload
- **Description**:
  Viết xUnit tests cho CollectorTask và File Upload modules:
  
  **Test files:**
  - `CollectorTaskControllerTests.cs`
  - `CollectorTaskControllerExtendedTests.cs`
  - `AssignCollectorCommandHandlerTests.cs`
  - `CollectorEvidenceUploadTests.cs`
  - `LocalFileStorageServiceTests.cs`
  
  **Jira**: KIEM-15, KIEM-20
  
  **Branch**: `feature/KIEM-15-collector-file-tests`
  
  **Acceptance Criteria:**
  - [ ] ≥ 20 test methods
  - [ ] File upload edge cases tested

### [Task] KIEM-SP2-09: Viết Unit Tests cho CollectionTask + Public Analytics
- **Assignee**: Thanh Duy
- **Priority**: Medium
- **Labels**: sprint-2, unit-test, collection-task
- **Description**:
  Viết xUnit tests cho CollectionTask domain và Public Analytics:
  
  **Test files:**
  - `CollectionTaskDomainTests.cs`
  - `CollectionTaskTests.cs`
  - `AuditLogAndErrorPathTests.cs`
  
  **Jira**: KIEM-10, KIEM-18, KIEM-22
  
  **Branch**: `feature/KIEM-18-collection-audit-tests`
  
  **Acceptance Criteria:**
  - [ ] Domain logic tests cho state transitions
  - [ ] Error path coverage

### [Task] KIEM-SP2-10: Viết Unit Tests cho Citizen + Search/Pagination
- **Assignee**: Đăng
- **Priority**: Medium
- **Labels**: sprint-2, unit-test, citizen, search
- **Description**:
  Viết xUnit tests cho Citizen module và Search/Pagination:
  
  **Test files:**
  - `CitizenModuleTests.cs`
  - `SearchPaginationFiltersTests.cs`
  
  **Jira**: KIEM-13, KIEM-23
  
  **Branch**: `feature/KIEM-13-citizen-search-tests`
  
  **Acceptance Criteria:**
  - [ ] ≥ 15 test methods
  - [ ] Pagination edge cases (page 0, negative, beyond max)

---

## Sprint 3: Quality Assurance, Bug Fixing & Final Report

### [Task] KIEM-SP3-01: Fix SonarCloud Quality Gate
- **Assignee**: Nguyễn Chí Trung
- **Priority**: Critical
- **Labels**: sprint-3, sonarcloud, security
- **Description**:
  Fix SonarCloud Quality Gate (16 vulnerabilities → 0):
  - Xóa hardcoded secrets trong appsettings.json
  - Thay hardcoded password trong CreateUserCommand.cs
  - Thêm path sanitization trong Python scripts
  - Cập nhật SonarCloud exclusions
  
  **Commits:**
  - `1d50e4c`, `6b9a678`, `bf05e20`
  
  **Acceptance Criteria:**
  - [ ] SonarCloud: 0 open vulnerabilities
  - [ ] Quality Gate: new_security_rating ≤ A

### [Task] KIEM-SP3-02: Fix Bug KIEM-29 — Max 5 Images Validation
- **Assignee**: Thanh Duy
- **Priority**: High
- **Labels**: sprint-3, bug-fix, bva
- **Description**:
  **Bug**: API cho phép upload > 5 hình khi tạo report, nhưng yêu cầu chỉ cho phép tối đa 5.
  
  **Fix**: Thêm validation `if (request.Images.Count > 5)` trong `CreateReportCommand.cs`
  
  **BVA Boundary Values:**
  - 0 images → rejected (minimum 1)
  - 1 image → accepted ✅
  - 5 images → accepted ✅ (boundary)
  - 6 images → rejected (maximum 5)
  
  **Acceptance Criteria:**
  - [ ] API trả về error khi > 5 images
  - [ ] xUnit test cover boundary values

### [Task] KIEM-SP3-03: Fix Bug KIEM-28 — Include taskId in Response
- **Assignee**: Minh Phụng
- **Priority**: Medium
- **Labels**: sprint-3, bug-fix
- **Description**:
  **Bug**: PUT /api/reports/{id}/accept response thiếu `taskId` field.
  
  **Steps to reproduce:**
  1. Login as Enterprise
  2. Accept a pending report
  3. Check response body → missing `taskId`
  
  **Expected**: Response includes `taskId` for client to navigate to task detail.
  
  **Branch**: `bugfix/KIEM-28-include-taskId`
  
  **Acceptance Criteria:**
  - [ ] Response body includes `taskId`
  - [ ] xUnit test verifies taskId in response

### [Task] KIEM-SP3-04: Fix E2E Allure Suite Missing
- **Assignee**: Nguyễn Hoàng Phụng
- **Priority**: Medium
- **Labels**: sprint-3, e2e, allure, bug-fix
- **Description**:
  **Bug**: Allure Report chỉ hiển thị 2 suites (API Tests + Backend Tests), thiếu E2E Tests.
  
  **Nguyên nhân**: Frontend start script sử dụng `node .next/standalone/server.js` → server không start đúng trên CI.
  
  **Fix**: Thay `"start": "next start"` trong `package.json`
  
  **Branch**: `bugfix/KIEM-e2e-allure-fix`
  
  **Acceptance Criteria:**
  - [ ] Allure Report hiển thị 3 suites
  - [ ] E2E test results merge vào report

### [Task] KIEM-SP3-05: Tạo Final Report & Deployment Guide
- **Assignee**: Nguyễn Chí Trung
- **Priority**: High
- **Labels**: sprint-3, documentation, report
- **Description**:
  Tạo và cập nhật tài liệu cuối cùng:
  - `docs/FINAL_REPORT.md` — Báo cáo tổng kết (455 tests, 9 workflows)
  - `docs/DEPLOYMENT_GUIDE.md` — Hướng dẫn deploy
  - `docs/CI_CD_WORKFLOWS.md` — Chi tiết 9 workflows
  - `docs/DEMO.md` — Kịch bản demo cho thầy
  
  **Acceptance Criteria:**
  - [ ] Tất cả docs có nội dung đầy đủ
  - [ ] Test counts chính xác (451 xUnit, 19 E2E, 74 Postman)

### [Task] KIEM-SP3-06: Viết Manual Test Cases (Excel)
- **Assignee**: Đăng
- **Priority**: Medium
- **Labels**: sprint-3, manual-testing, documentation
- **Description**:
  Tạo file Excel UnitestKCPM.xlsx với 68 test cases cho 13 functions:
  - Mỗi TC có: ID, Description, Steps, Expected Result, Actual Result, Status
  - Áp dụng kỹ thuật: EP, BVA, DT, ST, Error Guessing
  
  **Acceptance Criteria:**
  - [ ] 68 test cases covering 13 functions
  - [ ] Pass rate ≥ 95%
  - [ ] Có screenshots evidence

### [Task] KIEM-SP3-07: Cập nhật Traceability Matrix
- **Assignee**: Nguyễn Hoàng Phụng
- **Priority**: Medium
- **Labels**: sprint-3, documentation, traceability
- **Description**:
  Cập nhật Traceability Matrix với:
  - Tất cả Jira issues (KIEM-4 → KIEM-38+)
  - Bug status updates
  - Test technique mapping (Ch.4)
  - Allure report links
  
  **Acceptance Criteria:**
  - [ ] Tất cả issues có test case mapping
  - [ ] Bug issues có status cập nhật

### [Task] KIEM-SP3-08: Chuẩn bị Demo cho Thầy
- **Assignee**: Nguyễn Chí Trung
- **Priority**: High
- **Labels**: sprint-3, demo, presentation
- **Description**:
  Chuẩn bị demo cho thầy, bao gồm:
  1. Kiến trúc hệ thống (Client-Server)
  2. CI/CD Pipeline demo (push code → 9 workflows chạy)
  3. Test results (451 xUnit, 19 E2E, 74 Postman)
  4. Allure Report demo
  5. SonarCloud Quality Gate
  6. Bug tracking trên Jira
  7. Production app demo
  
  **Files**: `docs/DEMO.md` (696 dòng kịch bản chi tiết)

---

## Summary

| Sprint | Tasks | Focus |
|--------|-------|-------|
| Sprint 1 | 5 tasks | Infrastructure, CI/CD, Test Planning |
| Sprint 2 | 10 tasks | Test Development & Execution |
| Sprint 3 | 8 tasks | Quality, Bug Fix, Documentation |
| **Total** | **23 tasks** | |

### Task Distribution

| Member | Sprint 1 | Sprint 2 | Sprint 3 | Total |
|--------|----------|----------|----------|-------|
| Nguyễn Chí Trung | SP1-01, 02, 03, 04 | SP2-01, 06 | SP3-01, 05, 08 | **9** |
| Minh Phụng | SP1-05 | SP2-02, 08 | SP3-03 | **4** |
| Nguyễn Hoàng Phụng | — | SP2-03, 07 | SP3-04, 07 | **4** |
| Thanh Duy | — | SP2-04, 09 | SP3-02 | **3** |
| Đăng | — | SP2-05, 10 | SP3-06 | **3** |