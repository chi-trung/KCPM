# 📋 Hướng dẫn Thành viên — Quy trình Làm việc Nhóm

## 👥 Thành viên & Phân công Sprint

| Thành viên | Role | Sprint 1 | Sprint 2 | Sprint 3 |
|-----------|------|----------|----------|----------|
| **Nguyễn Chí Trung** (Team Lead) | Test Manager | CI/CD, Deploy, Test Plan, Jira | Auth tests, E2E tests | SonarCloud fix, Final Report, Demo |
| **Minh Phụng** | Developer/Tester | Postman Collection | Reports tests (BVA), CollectorTask tests | Fix KIEM-28 (taskId) |
| **Nguyễn Hoàng Phụng** | Developer/Tester | — | Notifications tests, Category+Security tests | Fix E2E Allure, Traceability Matrix |
| **Thanh Duy** | Developer/Tester | — | Complaints tests (DT), CollectionTask tests | Fix KIEM-29 (max images) |
| **11A6_03_Đăng** | Developer/Tester | — | Admin+Analytics tests, Citizen+Search tests | Manual Test Cases (Excel) |

---

## 🔄 Quy trình Git chuẩn (Bắt buộc)

### Bước 1: Nhận task trên Jira
```
Mở Jira Board → Kéo task từ TO DO → IN PROGRESS
```

### Bước 2: Tạo branch riêng
```bash
# Checkout từ main
git checkout main
git pull origin main

# Tạo branch theo naming convention
git checkout -b feature/KIEM-XX-ten-task
# Hoặc cho bug:
git checkout -b bugfix/KIEM-XX-ten-bug
```

**Naming convention:**
| Loại | Format | Ví dụ |
|------|--------|-------|
| Feature | `feature/KIEM-XX-ten-ngan` | `feature/KIEM-5-reports-tests` |
| Bug fix | `bugfix/KIEM-XX-ten-bug` | `bugfix/KIEM-28-include-taskId` |
| Hotfix | `hotfix/KIEM-XX-ten` | `hotfix/KIEM-29-max-images` |

### Bước 3: Code & Commit
```bash
# Commit message PHẢI có Jira key
git add .
git commit -m "feat(KIEM-XX): mô tả ngắn gọn"

# Hoặc:
git commit -m "fix(KIEM-28): include taskId in accept response"
git commit -m "test(KIEM-5): add BVA tests for images count"
```

**Commit prefix:**
| Prefix | Khi nào dùng |
|--------|-------------|
| `feat` | Thêm tính năng mới |
| `fix` | Sửa bug |
| `test` | Thêm/sửa test |
| `docs` | Cập nhật tài liệu |
| `refactor` | Tái cấu trúc code |
| `ci` | Thay đổi CI/CD |

### Bước 4: Push & Tạo Pull Request
```bash
git push origin feature/KIEM-XX-ten-task
```

Sau đó mở GitHub → Create Pull Request:
- **Title**: `KIEM-XX: Mô tả task` (Jira key enforcement sẽ check)
- **Description**: Mô tả chi tiết thay đổi
- **Reviewer**: Nguyễn Chí Trung (Team Lead)

### Bước 5: Đợi Review & Merge
- Team Lead review PR
- Fix comments nếu có
- Team Lead merge vào main
- CI tự động chạy tests

### Bước 6: Cập nhật Jira
- Kéo task từ IN PROGRESS → DONE
- Comment link PR vào Jira issue

---

## 📝 Hướng dẫn Chi tiết theo Thành viên

> ⚠️ **MỖI THÀNH VIÊN** phải có ít nhất 2-3 tasks, mỗi task phải có:
> 1. Branch riêng
> 2. Commit với Jira key  
> 3. PR → Team Lead review → Merge
> 4. CI chạy thành công
> 5. Jira comment với minh chứng

---

### 🟢 Minh Phụng — 3 Tasks

#### Task 1: Fix KIEM-28 — Include taskId in Accept Response
**Jira**: KIEM-57 | **Status**: TO DO

```bash
# 1. Branch
git checkout main && git pull origin main
git checkout -b bugfix/KIEM-28-include-taskId

# 2. Sửa file
# File: backend/src/WastePlatform.Application/Reports/Commands/AcceptReportCommand.cs
# Tìm handler → thêm taskId vào response DTO

# 3. Viết test
# File: backend/tests/.../AcceptReportCommandHandlerTests.cs
# Test: AcceptReport_ShouldReturn_TaskId()

# 4. Commit & Push
git add .
git commit -m "fix(KIEM-28): include taskId in report accept response"
git push origin bugfix/KIEM-28-include-taskId
# → Tạo PR: "KIEM-28: Include taskId in accept response"
```

#### Task 2: Viết báo cáo test Reports Module (BVA + State Transition)
**Jira**: Sẽ được tạo tự động

```bash
git checkout -b feature/KIEM-XX-reports-test-report

# Tạo: docs/TEST_REPORT_REPORTS_MODULE.md
# Nội dung:
# 1. Liệt kê test cases đã viết (KIEM-5)
# 2. Kỹ thuật: BVA (lat/long: -90..90, -180..180), State Transition (Pending→Accepted→Collected)
# 3. Bảng test data + expected results
# 4. Screenshot Allure Report
# 5. Link: https://chi-trung.github.io/KCPM/report-main/#suites

git commit -m "docs(KIEM-XX): add Reports module test report"
git push origin feature/KIEM-XX-reports-test-report
```

#### Task 3: Enhance Postman Collection — thêm test assertions
```bash
git checkout -b feature/KIEM-XX-postman-enhancements

# Mở Postman Collection → thêm assertions cho:
# - File Upload endpoint: validate response structure
# - CollectorTask endpoints: status code checks
# - Reports endpoints: validate required fields

git commit -m "test(KIEM-XX): enhance Postman assertions for File Upload"
git push origin feature/KIEM-XX-postman-enhancements
```

---

### 🟡 Nguyễn Hoàng Phụng — 3 Tasks

#### Task 1: Fix E2E Allure Suite Missing
**Jira**: KIEM-58 | **Status**: TO DO

```bash
git checkout -b bugfix/KIEM-58-e2e-allure-fix

# Check: https://chi-trung.github.io/KCPM/report-main/#suites
# Phải thấy 3 suites: Backend, API, E2E
# Nếu E2E thiếu → check allure-results output trong frontend-e2e.yml

git commit -m "fix(KIEM-58): restore E2E suite in Allure report"
git push origin bugfix/KIEM-58-e2e-allure-fix
```

#### Task 2: Viết báo cáo test WasteCategory + Notifications Module
```bash
git checkout -b feature/KIEM-XX-category-notifications-report

# Tạo: docs/TEST_REPORT_CATEGORY_NOTIFICATIONS.md
# 1. WasteCategory tests (KIEM-12): EP partitions, CRUD operations
# 2. Notifications tests (KIEM-6): valid/invalid IDs, mark-as-read
# 3. Kỹ thuật: EP, Error Guessing
# 4. Screenshot Allure
# 5. Truy vết: KIEM-6, KIEM-12

git commit -m "docs(KIEM-XX): add Category & Notifications test report"
git push origin feature/KIEM-XX-category-notifications-report
```

#### Task 3: Viết test mới cho Role-based Access Control (Security)
```bash
git checkout -b feature/KIEM-XX-rbac-security-tests

# Thêm tests cho KIEM-21:
# 1. Admin-only: tạo user, xóa user, xem analytics → 200
# 2. Enterprise-only: tạo collector, assign task → 200
# 3. Citizen-only: tạo report, tạo complaint → 200
# 4. Cross-role: citizen access admin endpoint → 403

# File: AdminEnterpriseAuthorizationTests.cs

git commit -m "test(KIEM-XX): add RBAC security test cases"
git push origin feature/KIEM-XX-rbac-security-tests
```

---

### 🟠 Thanh Duy — 3 Tasks

#### Task 1: Viết báo cáo test Complaints + CollectionTask Module
```bash
git checkout -b feature/KIEM-XX-complaints-collection-report

# Tạo: docs/TEST_REPORT_COMPLAINTS_COLLECTION.md
# 1. Complaints Module: Decision Table testing (6 combinations)
#    | Content | Report Status | User Role | Expected |
#    |---------|--------------|-----------|----------|
#    | Valid   | Pending      | Citizen   | OK       |
#    | Empty   | Pending      | Citizen   | 400      |
#    | ...     | ...          | ...       | ...      |
# 2. CollectionTask: State Transition diagram
# 3. Kỹ thuật: Decision Table (Ch.4), State Transition (Ch.4)
# 4. Screenshot Allure Results

git commit -m "docs(KIEM-XX): add Complaints & CollectionTask test report"
git push origin feature/KIEM-XX-complaints-collection-report
```

#### Task 2: Viết thêm BVA tests cho CollectionTask
```bash
git checkout -b feature/KIEM-XX-collection-bva-tests

# Thêm BVA tests:
# 1. Min images: 0 (reject), 1 (accept)
# 2. Max images: 5 (accept), 6 (reject)
# 3. Empty content validation
# 4. Invalid status transitions

# File: CollectionTaskDomainTests.cs

git commit -m "test(KIEM-XX): add BVA tests for CollectionTask"
git push origin feature/KIEM-XX-collection-bva-tests
```

#### Task 3: Thêm AuditLog tests cho Complaints operations
```bash
git checkout -b feature/KIEM-XX-audit-complaint-tests

# Bổ sung KIEM-22:
# 1. Audit log khi tạo complaint
# 2. Audit log khi resolve complaint
# 3. Error path: complaint cho report không tồn tại

# File: AuditLogAndErrorPathTests.cs

git commit -m "test(KIEM-XX): add audit log tests for Complaints"
git push origin feature/KIEM-XX-audit-complaint-tests
```

---

### 🔴 11A6_03_Đăng — 3 Tasks

#### Task 1: Viết báo cáo test Admin + Analytics + Citizen Module
```bash
git checkout -b feature/KIEM-XX-admin-citizen-report

# Tạo: docs/TEST_REPORT_ADMIN_CITIZEN.md
# 1. Admin Module (KIEM-8): CRUD user, manage roles
# 2. Analytics Module (KIEM-9): dashboard stats, data accuracy
# 3. Citizen Module (KIEM-13): profile management, report history
# 4. Kỹ thuật: EP, Error Guessing
# 5. Screenshot Allure

git commit -m "docs(KIEM-XX): add Admin & Citizen test report"
git push origin feature/KIEM-XX-admin-citizen-report
```

#### Task 2: Viết Manual Test Cases cho Search & Pagination
```bash
git checkout -b feature/KIEM-XX-manual-search-pagination

# Tạo: docs/MANUAL_TEST_SEARCH_PAGINATION.md
# 1. Search by keyword: valid, empty, special characters
# 2. Pagination: page 1, last page, page 0 (invalid), negative page
# 3. Filters: by status, by category, by date range
# 4. Sort: by date ASC/DESC, by status
# 5. Kỹ thuật: BVA (boundaries), EP (partitions)

git commit -m "docs(KIEM-XX): add manual test cases for Search & Pagination"
git push origin feature/KIEM-XX-manual-search-pagination
```

#### Task 3: Viết thêm Integration Tests cho Admin Analytics
```bash
git checkout -b feature/KIEM-XX-analytics-integration-tests

# Thêm integration tests:
# 1. GET /api/admin/analytics → validate response structure
# 2. Registered users count accuracy
# 3. Reports by status breakdown
# 4. Error: non-admin access → 403 Forbidden

# File: AnalyticsModuleTests.cs

git commit -m "test(KIEM-XX): add integration tests for Analytics"
git push origin feature/KIEM-XX-analytics-integration-tests
```

---

## 📊 Phân bổ Tasks (Chia đều)

| Thành viên | Existing | New | **Total** | Task Types |
|-----------|----------|-----|-----------|------------|
| **Minh Phụng** | 1 (KIEM-57) | 2 | **3** | Bug fix + Doc + Test |
| **Nguyễn Hoàng Phụng** | 1 (KIEM-58) | 2 | **3** | Bug fix + Doc + Test |
| **Thanh Duy** | 0 | 3 | **3** | Doc + BVA + Audit |
| **11A6_03_Đăng** | 0 | 3 | **3** | Doc + Manual + Integration |
| **Nguyễn Chí Trung** | ✅ Done (9 tasks, 6 PRs) | 0 | **Review all PRs** | Team Lead |


---

## 🚀 Quick Reference

### Cách kiểm tra CI
1. Push code → GitHub Actions tự chạy
2. Mở https://github.com/chi-trung/KCPM/actions
3. Check **Backend Tests** workflow → 451 tests phải pass
4. Check **Frontend E2E** → 19 scenarios
5. Check **SonarCloud** → Quality Gate

### Cách xem Allure Report
- https://chi-trung.github.io/KCPM/report-main/
- Tab **Suites** → 3 suites (Backend, API, E2E)
- Tab **Behaviors** → tests theo feature

### Cách xem Test Coverage
- https://chi-trung.github.io/KCPM/ (GitHub Pages site index)
- Coverage badge trên README

### Production URLs
| Service | URL |
|---------|-----|
| Frontend | https://kcpm.vercel.app |
| Backend API | https://kcpm-backend.onrender.com |
| Swagger | https://kcpm-backend.onrender.com/swagger |
| Allure Report | https://chi-trung.github.io/KCPM/report-main/ |

### Test Accounts
| Email | Password | Role |
|-------|----------|------|
| admin@gmail.com | password | Admin |
| greenlife@gmail.com | password | Enterprise |
| nguyenvana@gmail.com | password | Citizen |
| collector1@gmail.com | password | Collector |
