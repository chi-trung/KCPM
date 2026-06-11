# Traceability Matrix - Waste Recycling Platform

## 1. Muc dich

Traceability Matrix dung de noi yeu cau, Jira issue, test case, automation file va evidence. Day la bang chung quan trong trong mon Kiem chung phan mem vi no tra loi cau hoi:

- Yeu cau nao da duoc test?
- Test bang ky thuat nao?
- Test nam o file nao?
- Ket qua chay test duoc luu o dau?
- Ai phu trach va trang thai hien tai la gi?

Format nay tham khao tu `UnitestCuaBao.xlsx`: co ma test, muc tieu, buoc test, expected result, requirement lien quan, status, ngay chay va nguoi chay.

## 2. Ma hoa test case de dung thong nhat

| Prefix | Y nghia | Vi du |
|---|---|---|
| TC-AUTH | Auth/Login/Register | TC-AUTH-001 |
| TC-REPORT | Citizen waste report | TC-REPORT-001 |
| TC-TASK | Enterprise/Collector task flow | TC-TASK-001 |
| TC-NOTI | Notifications/SignalR | TC-NOTI-001 |
| TC-ADMIN | Admin management | TC-ADMIN-001 |
| TC-E2E | End-to-end user journey | TC-E2E-001 |
| TC-DEPLOY | Deployment verification | TC-DEPLOY-001 |

## 3. Matrix hien tai

| Jira Key | Requirement / Feature | Test Case ID | Test Level | Test Design Technique | Automation / Evidence File | Expected Evidence | Status |
|---|---|---|---|---|---|---|---|
| KIEM-4 | User can register/login and receive JWT by role | TC-AUTH-001 | Unit/API | Equivalence partitioning, negative testing | `backend/tests/WastePlatform.Tests/Controllers/AuthControllerTests.cs`, Postman `01 - Auth` | xUnit TRX, Allure, Newman report | Existing |
| KIEM-4 | Invalid login is rejected | TC-AUTH-002 | Unit/API | Error guessing, negative testing | `AuthControllerTests.cs`, Postman login invalid request | 401/400 evidence | Existing/Need map |
| KIEM-14 | Collector can view assigned tasks | TC-TASK-001 | API/E2E | State transition | `CollectorTaskControllerTests.cs`, Postman `08 - Collector Tasks` | Newman + Allure | Existing/Need E2E |
| KIEM-14 | Collector can update task status | TC-TASK-002 | Unit/API/E2E | State transition, decision table | `CollectionTaskDomainTests.cs`, `CollectorTaskControllerExtendedTests.cs` | xUnit + Newman + E2E screenshot | Existing/Need E2E |
| KIEM-16 | Enterprise can assign collector to task | TC-TASK-003 | Unit/API/E2E | Decision table, state transition | `AssignCollectorCommandHandlerTests.cs`, `EnterpriseTaskControllerTests.cs`, Postman `PUT Assign Collector` | xUnit + Newman + Allure | Existing/Need E2E |
| KIEM-17 | Enterprise can view/update reward rules | TC-REWARD-001 | API | Boundary value, equivalence partitioning | `EnterpriseRewardRuleControllerTests.cs`, Postman reward rule requests | xUnit + Newman | Existing |
| KIEM-19 | User can receive/read notifications | TC-NOTI-001 | Unit/API/Integration | State transition | `NotificationServiceTests.cs`, `NotificationControllerTests.cs`, `SignalRRealTimeNotifierTests.cs` | xUnit + Allure | Existing |
| KIEM-ADMIN | Admin can manage users/enterprises/complaints | TC-ADMIN-001 | Unit/API | Role-based access, negative testing | `AdminModuleTests.cs`, `AdminApiIntegrationTests.cs` | xUnit + Allure | Existing |
| KIEM-FE | Public pages render and auth entry points are available | TC-E2E-001 | System/E2E | Smoke testing | `frontend/e2e/smoke_test.js` | CodeceptJS output, screenshots on fail | Existing |
| KIEM-FE | Citizen đăng ký và điều hướng đến create-report form | TC-E2E-002 | System/E2E | End-to-end, Error Guessing | `frontend/e2e/citizen_report_test.js` | CodeceptJS output, screenshots on fail | Added |
| KIEM-16 | Enterprise đăng nhập và truy cập task management | TC-E2E-003 | System/E2E | State Transition, Role-based Access | `frontend/e2e/enterprise_assign_test.js` | CodeceptJS output, screenshots on fail | Added |
| KIEM-14 | Collector đăng nhập, truy cập tasks, không vào enterprise route | TC-E2E-004 | System/E2E | State Transition Guard, Error Guessing | `frontend/e2e/collector_task_test.js` | CodeceptJS output, screenshots on fail | Added |
| KIEM-DEPLOY | Server deploy only after quality gate pass | TC-DEPLOY-001 | Deployment/System | Smoke testing | `.github/workflows/deploy-server.yml` | GitHub Actions deploy log | Existing/Improved |
| KIEM-DEPLOY | Backend responds after deployment | TC-DEPLOY-002 | Deployment/System | Smoke testing | `.github/workflows/deploy-server.yml` post-deploy curl `/api/health` | Health check log | Added |
| KIEM-STATIC | Code is reviewed by static analysis | TC-STATIC-001 | Static Testing | Static analysis | `.github/workflows/sonar.yml` | SonarCloud run | Existing |

## 4. Test case detail template

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

## 5. Quy tac cap nhat

- Moi Jira issue co code thay doi phai co it nhat 1 test case ID.
- Moi test case quan trong phai map den it nhat 1 automation file hoac manual evidence.
- Neu automation chua co, status ghi `Need automation`, khong ghi lap lo.
- Bug tim thay phai co buoc tai tao, actual result, expected result va issue/defect link.
