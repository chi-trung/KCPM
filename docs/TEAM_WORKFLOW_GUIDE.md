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

---

### 🟢 Minh Phụng — Tasks hiện tại

#### Task: Fix KIEM-28 — Include taskId in Accept Response
**Status**: TO DO → Cần làm

**Hướng dẫn step-by-step:**

```bash
# 1. Checkout branch
git checkout main && git pull origin main
git checkout -b bugfix/KIEM-28-include-taskId

# 2. Sửa file
# File: backend/src/WastePlatform.Application/Reports/Commands/AcceptReportCommand.cs
# Tìm handler method → thêm taskId vào response object

# 3. Viết test
# File: backend/tests/WastePlatform.Tests/Application/Reports/AcceptReportCommandHandlerTests.cs
# Thêm test case verify taskId in response

# 4. Commit
git add .
git commit -m "fix(KIEM-28): include taskId in report accept response"

# 5. Push & PR
git push origin bugfix/KIEM-28-include-taskId
# Tạo PR trên GitHub với title: "KIEM-28: Include taskId in accept response"
```

**Chi tiết fix:**
1. Mở `AcceptReportCommandHandler.cs`
2. Trong `Handle()` method, sau khi accept report, tìm dòng trả về result
3. Thêm `TaskId = createdTask.Id` vào response DTO
4. Viết 1 test case verify response chứa taskId

---

### 🟡 Nguyễn Hoàng Phụng — Tasks hiện tại

#### Task: Fix E2E Allure Suite Missing
**Status**: IN PROGRESS

**Hướng dẫn:**

```bash
# 1. Checkout branch
git checkout main && git pull origin main
git checkout -b bugfix/KIEM-e2e-allure-fix

# 2. Verify E2E suite xuất hiện
# Mở https://chi-trung.github.io/KCPM/report-main/#suites
# Phải thấy 3 suites: Backend Tests, API Tests, E2E Tests

# 3. Nếu E2E vẫn thiếu → check allure-results
# File: .github/workflows/frontend-e2e.yml
# Verify "Debug - list allure-results" step output

# 4. Commit & PR nếu cần thay đổi
```

#### Task: Cập nhật Traceability Matrix
**Status**: TO DO

```bash
git checkout -b docs/KIEM-traceability-update

# Sửa: docs/TRACEABILITY_MATRIX.md
# - Thêm tất cả KIEM issues mới (KIEM-31+)
# - Cập nhật bug status
# - Thêm Sprint info

git commit -m "docs(KIEM-SP3): update traceability matrix with sprint tasks"
git push origin docs/KIEM-traceability-update
# Tạo PR
```

---

### 🟠 Thanh Duy — Tasks hiện tại

Tất cả tasks đã DONE ✅ (KIEM-29 đã fix, KIEM-7 tests đã viết, KIEM-18/22 tests đã viết)

Nếu muốn contribute thêm:
- Review test coverage cho Complaints module
- Thêm edge case tests cho CollectionTask

---

### 🔴 11A6_03_Đăng — Tasks hiện tại

Tất cả tasks đã DONE ✅ (Admin/Analytics/Citizen/Search tests đã viết, Manual test Excel đã tạo)

Nếu muốn contribute thêm:
- Thêm integration tests cho Admin API
- Review & update Excel test cases

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
