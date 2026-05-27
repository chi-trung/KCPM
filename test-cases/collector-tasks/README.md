# CollectorTask Module Test Cases

**Jira Task:** KIEM-12 / WRP-BE-TESTS-012  
**Module:** CollectorTask — Nhiệm vụ thu gom  
**Module Owner:** Nguyễn Minh Phụng  
**Last Updated:** 2026-05-27  

---

## Test Case List

| TC ID | Tên | Endpoint | Loại | Layer |
|---|---|---|---|---|
| TC-TASK-001 | Get Collector Tasks List | GET /api/collector/tasks | Positive + Negative | Controller |
| TC-TASK-002 | Get Task By ID | GET /api/collector/tasks/{id} | Positive + Negative | Controller |
| TC-TASK-003 | Set On The Way | PUT /api/collector/tasks/{id}/on-the-way | Positive + Negative | Controller |
| TC-TASK-004 | Complete Task | PUT /api/collector/tasks/{id}/complete | Positive + Negative | Controller |
| TC-TASK-005 | Domain State Transition Rules | Entity | Positive + Negative | Domain |
| TC-TASK-006 | Get Collector Stats | GET /api/collector/tasks/stats | Positive + Negative | Controller |

---

## State Machine

```
Created --> Assigned --> OnTheWay --> Collected
```

- `Assigned -> OnTheWay`: Collector confirms they are on the way
- `OnTheWay -> Collected`: Collector completes the task with weight and optional images
- All other transitions throw `InvalidOperationException`

---

## Unit Test Files

| File | Layer | Test Count |
|---|---|---|
| `Controllers/CollectorTaskControllerTests.cs` | Controller (existing) | 3 tests |
| `Controllers/CollectorTaskControllerExtendedTests.cs` | Controller (new) | 10 tests |
| `Domain/CollectionTaskTests.cs` | Domain (existing) | 6 tests |

**Total: 19 unit tests**

---

## Test Execution

```bash
# Run all CollectorTask tests
dotnet test --filter "FullyQualifiedName~CollectorTask"

# Run controller tests only
dotnet test --filter "FullyQualifiedName~CollectorTaskController"

# Run domain tests only
dotnet test --filter "FullyQualifiedName~CollectionTaskTests"
```

---

## Test Execution Status

| TC ID | Automated | Manual | Status |
|---|---|---|---|
| TC-TASK-001 | xUnit | — | Pending CI run |
| TC-TASK-002 | xUnit | — | Pending CI run |
| TC-TASK-003 | xUnit | — | Pending CI run |
| TC-TASK-004 | xUnit | — | Pending CI run |
| TC-TASK-005 | xUnit | — | Pending CI run |
| TC-TASK-006 | xUnit | — | Pending CI run |

---

## Dependencies

- `CollectionTask` domain entity (`Domain/Entities/CollectionTask.cs`)
- `CollectorTaskController` (`API/Controllers/CollectorTaskController.cs`)
- `WastePlatformDbContext` with InMemory provider for unit tests
- `INotificationService` mock
- `IHubContext<TaskHub>` mock
