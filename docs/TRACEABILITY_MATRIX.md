# Ma trận Truy vết (Traceability Matrix) - Waste Recycling Platform

## 1. Muc dich

Traceability Matrix dùng để nối yêu cầu, Jira issue, test case, automation file và evidence. Đây là bằng chứng quan trọng trong môn Kiểm chứng phần mềm vì nó trả lời câu hỏi:

- Yêu cầu nào đã được test?
- Test bằng kỹ thuật nào?
- Test nằm ở file nào?
- Kết quả chạy test được lưu ở đâu?
- Ai phụ trách và trạng thái hiện tại là gì?

Format này tham khảo từ `UnitestCuaBao.xlsx`: có mã test, mục tiêu, bước test, expected result, requirement liên quan, status, ngày chạy và người chạy.

## 2. Mã hóa test case để dùng thống nhất

| Prefix | Y nghia | Vi du |
|---|---|---|
| TC-AUTH | Auth/Login/Register | TC-AUTH-001 |
| TC-REPORT | Citizen waste report | TC-REPORT-001 |
| TC-TASK | Enterprise/Collector task flow | TC-TASK-001 |
| TC-NOTI | Notifications/SignalR | TC-NOTI-001 |
| TC-ADMIN | Admin management | TC-ADMIN-001 |
| TC-E2E | End-to-end user journey | TC-E2E-001 |
| TC-DEPLOY | Deployment verification | TC-DEPLOY-001 |

## 3. Matrix hiện tại

| Jira Key | Requirement / Feature | Test Case ID | Test Level | Test Design Technique | Automation / Evidence File | Expected Evidence | Status |
|---|---|---|---|---|---|---|---|
| KIEM-5  | User can register/login and receive JWT by role | TC-AUTH-001 | Unit/API | Equivalence partitioning, negative testing | `backend/tests/WastePlatform.Tests/Controllers/AuthControllerTests.cs`, Postman `01 - Auth` | xUnit TRX, [Allure report](https://chi-trung.github.io/KCPM/report-main/) | ✅ Automated |
| KIEM-5  | Invalid login is rejected | TC-AUTH-002 | Unit/API | Error guessing, negative testing | `AuthControllerTests.cs`, Postman login invalid request | 401/400 evidence | ✅ Automated |
| KIEM-14 | Collector can view assigned tasks | TC-TASK-001 | API/E2E | State transition | `CollectorTaskControllerTests.cs`, Postman `08 - Collector Tasks` | Newman + Allure | ✅ Automated |
| KIEM-14 | Collector can update task status | TC-TASK-002 | Unit/API/E2E | State transition, decision table | `CollectionTaskDomainTests.cs`, `CollectorTaskControllerExtendedTests.cs` | xUnit + Newman + E2E screenshot | ✅ Automated |
| KIEM-14 | Collector đăng nhập, truy cập tasks, không vào enterprise route | TC-E2E-004 | System/E2E | State Transition Guard, Error Guessing | `frontend/e2e/collector_task_test.js` | CodeceptJS output, screenshots on fail | ✅ Added |
| KIEM-16 | Enterprise can assign collector to task | TC-TASK-003 | Unit/API/E2E | Decision table, state transition | `AssignCollectorCommandHandlerTests.cs`, `EnterpriseTaskControllerTests.cs`, Postman `PUT Assign Collector` | xUnit + Newman + Allure | ✅ Automated |
| KIEM-16 | Enterprise đăng nhập và truy cập task management | TC-E2E-003 | System/E2E | State Transition, Role-based Access | `frontend/e2e/enterprise_assign_test.js` | CodeceptJS output, screenshots on fail | ✅ Added |
| KIEM-17 | Enterprise can view/update reward rules | TC-REWARD-001 | API | Boundary value, equivalence partitioning | `EnterpriseRewardRuleControllerTests.cs`, Postman reward rule requests | xUnit + Newman | ✅ Automated |
| KIEM-19 | User can receive/read notifications | TC-NOTI-001 | Unit/API/Integration | State transition | `NotificationServiceTests.cs`, `NotificationControllerTests.cs`, `SignalRRealTimeNotifierTests.cs` | xUnit + Allure | ✅ Automated |
| KIEM-21 | Public pages render and auth entry points are available | TC-E2E-001 | System/E2E | Smoke testing | `frontend/e2e/smoke_test.js` | CodeceptJS output, screenshots on fail | ✅ Added |
| KIEM-21 | Citizen đăng ký và điều hướng đến create-report form | TC-E2E-002 | System/E2E | End-to-end, Error Guessing | `frontend/e2e/citizen_report_test.js` | CodeceptJS output, screenshots on fail | ✅ Added |
| KIEM-FE | Admin can manage users/enterprises/complaints | TC-ADMIN-001 | Unit/API | Role-based access, negative testing | `AdminModuleTests.cs`, `AdminApiIntegrationTests.cs` | xUnit + Allure | ✅ Automated |
| CI/CD   | Server deploy only after quality gate pass | TC-DEPLOY-001 | Deployment/System | Smoke testing | `.github/workflows/deploy-server.yml` | GitHub Actions deploy log | ✅ Automated |
| CI/CD   | Backend responds after deployment | TC-DEPLOY-002 | Deployment/System | Smoke testing | `.github/workflows/deploy-server.yml` post-deploy curl `/api/health` | Health check log | ✅ Added |
| CI/CD   | Code is reviewed by static analysis | TC-STATIC-001 | Static Testing | Static analysis | `.github/workflows/sonar.yml` | [SonarCloud](https://sonarcloud.io/project/overview?id=chi-trung_KCPM) | ✅ Running |

## 4. Chi tiết test case (template)

Dung template nay khi viet file `.md` moi trong `test-cases`:

```md
# TC-XXX-001 - Short Title

## Requirement
- Jira Key: KIEM-XX
- Feature: ...
- Test basis: requirement/code/business flow

## Test Design Technique
- Equivalence partitioning / Boundary value / Decision table / State transition / Error guessing

## Preconditions
- Test account:
- Test data:
- Environment:

## Steps
1. ...
2. ...
3. ...

## Expected Result
1. ...
2. ...

## Automation Mapping
- xUnit:
- Postman:
- E2E:
- CI workflow:

## Execution Record
- Status: Passed/Failed/Blocked/Untested
- Executed by:
- Executed date:
- Evidence link:
- Defect link, if failed:
```

## 5. Quy tắc cập nhật

- Mỗi Jira issue có code thay đổi phải có ít nhất 1 test case ID.
- Mỗi test case quan trọng phải map đến ít nhất 1 automation file hoặc manual evidence.
- Nếu automation chưa có, status ghi `Need automation`, không ghi lặp lộn.
- Bug tìm thấy phải có bước tái tạo, actual result, expected result và issue/defect link.

## 6. Allure Report Links

- **Main report (all tests)**: https://chi-trung.github.io/KCPM/report-main/
- **Behaviors (by epic/feature)**: https://chi-trung.github.io/KCPM/report-main/#behaviors
- **Suites (3 groups)**: https://chi-trung.github.io/KCPM/report-main/#suites
- **Categories (failure analysis)**: https://chi-trung.github.io/KCPM/report-main/#categories
