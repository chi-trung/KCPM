# Ma trận Truy vết (Traceability Matrix) — Waste Recycling Platform

**Cập nhật lần cuối**: 2026-06-12 (auto-logged từ CI Backend Tests Run #435)  
**Allure Report**: https://chi-trung.github.io/KCPM/report-main/  
**Jira Board**: https://ut-team-36.atlassian.net/jira/software/projects/KIEM/boards/3  
**CI/CD**: https://github.com/chi-trung/KCPM/actions

---

## 1. Mục đích

Traceability Matrix nối yêu cầu → Jira issue → test case → automation file → CI evidence.  
Mỗi CI run tự động log kết quả lên Jira bằng token của nhóm trưởng (Nguyễn Chí Trung).

---

## 2. Bảng mã hóa Test Case

| Prefix | Ý nghĩa | Ví dụ |
|---|---|---|
| TC-AUTH | Auth/Login/Register (JWT, roles) | TC-AUTH-001 |
| TC-REPORT | Citizen waste report lifecycle | TC-REPORT-001 |
| TC-NOTI | Notifications & SignalR real-time | TC-NOTI-001 |
| TC-COMPLAINT | Complaints flow | TC-COMPLAINT-001 |
| TC-ADMIN | Admin management | TC-ADMIN-001 |
| TC-ANALYTICS | Analytics & reporting | TC-ANALYTICS-001 |
| TC-CITIZEN | Citizen module | TC-CITIZEN-001 |
| TC-CATEGORY | WasteCategory management | TC-CATEGORY-001 |
| TC-COLLECTOR | Collector module | TC-COLLECTOR-001 |
| TC-CTASK | CollectorTask workflow | TC-CTASK-001 |
| TC-ENTASK | Enterprise Task management | TC-ENTASK-001 |
| TC-ENCOL | Enterprise Collectors & Reward Rules | TC-ENCOL-001 |
| TC-COLTASK | CollectionTask / CollectionImage | TC-COLTASK-001 |
| TC-SIGNALR | SignalR real-time | TC-SIGNALR-001 |
| TC-FILE | File Uploads & Storage | TC-FILE-001 |
| TC-SECURITY | Security & Role-based access | TC-SECURITY-001 |
| TC-AUDIT | AuditLog & Error paths | TC-AUDIT-001 |
| TC-SEARCH | Search, Pagination & Filters | TC-SEARCH-001 |
| TC-E2E | End-to-end browser flows | TC-E2E-001 |
| TC-DEPLOY | Deployment & infrastructure | TC-DEPLOY-001 |
| TC-STATIC | Static analysis / SonarCloud | TC-STATIC-001 |

---

## 3. Ma trận đầy đủ (xUnit Backend Tests)

> Tất cả xUnit test được CI tự động chạy, log kết quả lên Jira, và upload lên Allure report.  
> **xUnit file path** relative to: `Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/`

| Jira Key | Title (WRP-BE-TESTS) | Assignee | xUnit Test Files | Postman Collection | Status CI |
|---|---|---|---|---|---|
| **KIEM-4** | 001 - Auth Module Testing | Nguyễn Chí Trung | `Controllers/AuthControllerTests.cs`, `Services/JwtServiceTests.cs`, `Integration/JwtBearerIntegrationTests.cs` | `01 - Auth (Login/Register)` | ✅ Auto-logged |
| **KIEM-5** | 002 - Reports Module Testing | Minh Phụng | `Application/Reports/AcceptReportCommandHandlerTests.cs`, `CreateReportCommandHandlerTests.cs`, `GetAllReportsQueryHandlerTests.cs`, `GetEnterpriseReportsQueryHandlerTests.cs`, `GetMyReportsQueryHandlerTests.cs`, `GetReportByIdQueryHandlerTests.cs`, `RejectReportCommandHandlerTests.cs` | `05 - Reports`, `06 - Citizen Reports` | ✅ Auto-logged |
| **KIEM-6** | 003 - Notifications Module Testing | Nguyễn Hoàng Phụng | `Application/Notifications/NotificationServiceTests.cs`, `Controllers/NotificationControllerTests.cs`, `Infrastructure/NotificationRepositoryTests.cs` | `09 - Notifications` | ✅ Auto-logged |
| **KIEM-7** | 004 - Complaints Module Testing | Thanh Duy | `Application/Complaints/CreateComplaintCommandHandlerTests.cs`, `RejectComplaintCommandHandlerTests.cs`, `ResolveComplaintCommandHandlerTests.cs` | `10 - Complaints` | ✅ Auto-logged |
| **KIEM-8** | 005 - Admin Module Testing | 11A6_03_Đăng | `Controllers/AnalyticsControllerTests.cs` + `AdminModuleTests.cs` (root tests folder), `AdminApiIntegrationTests.cs` | `02 - Admin`, `Admin Users` | ✅ Auto-logged |
| **KIEM-9** | 006 - Analytics Module Testing | 11A6_03_Đăng | `Controllers/AnalyticsControllerTests.cs`, `AnalyticsModuleTests.cs`, `AnalyticsApiIntegrationTests.cs` | `04 - Analytics` | ✅ Auto-logged |
| **KIEM-10** | 007 - Public Analytics Testing | Thanh Duy | `Controllers/AnalyticsControllerTests.cs` (public endpoints) | `04 - Analytics (public)` | ✅ Auto-logged |
| **KIEM-12** | 009 - WasteCategory Module Testing | Nguyễn Hoàng Phụng | `Controllers/WasteCategoryControllerTests.cs`, `Application/WasteCategories/GetAllCategoriesQueryHandlerTests.cs`, `GetCategoryByIdQueryHandlerTests.cs`, `Infrastructure/WasteCategoryRepositoryTests.cs` | `03 - WasteCategory` | ✅ Auto-logged |
| **KIEM-13** | 010 - Citizen Module Testing | 11A6_03_Đăng | `Application/Citizens/CitizenModuleTests.cs` | `06 - Citizen Profile` | ✅ Auto-logged |
| **KIEM-14** | 011 - Collector Module Testing | Nguyễn Chí Trung | `Controllers/CollectorControllerTests.cs` + E2E | Postman `07 - Collector` + `frontend/e2e/collector_task_test.js` | ✅ Auto-logged |
| **KIEM-15** | 012 - CollectorTask Module Testing | Minh Phụng | `Controllers/CollectorTaskControllerTests.cs`, `CollectorTaskControllerExtendedTests.cs`, `Application/Tasks/AssignCollectorCommandHandlerTests.cs` | `08 - CollectorTask` | ✅ Auto-logged |
| **KIEM-16** | 013 - Enterprise Task Module Testing | Nguyễn Chí Trung | `Controllers/EnterpriseTaskControllerTests.cs` + E2E | Postman `Enterprise Tasks` + `frontend/e2e/enterprise_assign_test.js` | ✅ Auto-logged |
| **KIEM-17** | 014 - Enterprise Collectors & Reward Rules | Nguyễn Chí Trung | `Controllers/EnterpriseRewardRuleControllerTests.cs`, `Infrastructure/RewardPointsRepositoryTests.cs` | `Enterprise Rewards` | ✅ Auto-logged |
| **KIEM-18** | 015 - CollectionTask / CollectionImage | Thanh Duy | `Domain/CollectionTaskDomainTests.cs`, `Domain/CollectionTaskTests.cs` | — | ✅ Auto-logged |
| **KIEM-19** | 016 - SignalR Real-time Tests | Nguyễn Chí Trung | `SignalR/SignalRRealTimeNotifierTests.cs` | — | ✅ Auto-logged |
| **KIEM-20** | 017 - File Uploads & Storage Tests | Minh Phụng | `Infrastructure/CollectorEvidenceUploadTests.cs`, `LocalFileStorageServiceTests.cs` | File upload requests | ✅ Auto-logged |
| **KIEM-21** | 018 - Security & Role-based Access Tests | Nguyễn Hoàng Phụng | `Integration/AdminEnterpriseAuthorizationTests.cs`, `Integration/JwtBearerIntegrationTests.cs` | Postman `Security/Auth` | ✅ Auto-logged |
| **KIEM-22** | 019 - AuditLog & Error Path Tests | Thanh Duy | `Controllers/AuditLogAndErrorPathTests.cs` | — | ✅ Auto-logged |
| **KIEM-23** | 020 - Search, Pagination & Filters Tests | 11A6_03_Đăng | `Search/SearchPaginationFiltersTests.cs` | — | ✅ Auto-logged |

---

## 4. Ma trận E2E Tests (CodeceptJS)

| Jira Key | Feature | Test Case ID | E2E File | Status |
|---|---|---|---|---|
| KIEM-14 | Collector đăng nhập, truy cập tasks, không vào enterprise route | TC-E2E-004 | `frontend/e2e/collector_task_test.js` | ✅ Auto-logged |
| KIEM-16 | Enterprise đăng nhập và truy cập task management | TC-E2E-003 | `frontend/e2e/enterprise_assign_test.js` | ✅ Auto-logged |
| KIEM-21 | Public pages render and auth entry points available | TC-E2E-001 | `frontend/e2e/smoke_test.js` | ✅ Auto-logged |
| KIEM-21 | Citizen đăng ký và điều hướng đến create-report form | TC-E2E-002 | `frontend/e2e/citizen_report_test.js` | ✅ Auto-logged |

---

## 5. Ma trận Postman API Tests (Newman)

| Jira Key | Feature | Postman Folder / Collection | Status |
|---|---|---|---|
| KIEM-4 | Auth/Login/Register endpoints | `WastePlatform API - Professional QA Suite` > `01 - Auth` | ✅ Auto-logged |
| KIEM-21 | Security & Role-based access (JWT, Admin, Enterprise) | `Security/Auth` | ✅ Auto-logged |

---

## 6. CI/CD & Infrastructure

| Jira Key | Feature | Test Case ID | Automation File | Status |
|---|---|---|---|---|
| CI/CD | Server deploy after quality gate pass | TC-DEPLOY-001 | `.github/workflows/deploy-server.yml` | ✅ Running |
| CI/CD | Backend health check after deployment | TC-DEPLOY-002 | `deploy-server.yml` > `curl /api/health` | ✅ Running |
| CI/CD | Static analysis by SonarCloud | TC-STATIC-001 | `.github/workflows/sonar.yml` | ✅ Running |

---

## 7. Bug Issues (Manual Testing)

| Jira Key | Title | Assignee | Status |
|---|---|---|---|
| KIEM-26 | [BUG] Missing mandatory image validation in Create | Nguyễn Hoàng Phụng | IN PROGRESS |
| KIEM-27 | [BUG] PUT /notifications/{id}/read returns 200 for 404 | Nguyễn Hoàng Phụng | DONE |
| KIEM-28 | Include taskId in report accept response | Minh Phụng | TO DO |
| KIEM-29 | [BUG] Missing maximum 5 images validation | Thanh Duy | TO DO |

---

## 8. Allure Report Links

- **Main report (all 3 suites)**: https://chi-trung.github.io/KCPM/report-main/
- **Behaviors (by feature)**: https://chi-trung.github.io/KCPM/report-main/#behaviors
- **Suites (3 groups)**: https://chi-trung.github.io/KCPM/report-main/#suites
- **Categories (failure analysis)**: https://chi-trung.github.io/KCPM/report-main/#categories

---

## 9. Quy tắc cập nhật

- Mỗi Jira issue phải có ít nhất 1 xUnit test file hoặc E2E/Postman test.
- CI tự động log kết quả lên Jira sau mỗi run (sử dụng `scripts/jira_log_test_execution.py`).
- ISSUE_MAP trong script xác định Jira issue nào nhận comment từ loại test nào.
- Bug issues (KIEM-26, 27, 28, 29) không tự động log — cần manual testing.
- Thành viên nhóm: Nguyễn Chí Trung (trưởng nhóm), Minh Phụng, Nguyễn Hoàng Phụng, Thanh Duy, 11A6_03_Đăng.
