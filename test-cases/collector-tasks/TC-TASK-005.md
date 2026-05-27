# TC-TASK-005: Domain State Transition Rules

**Module:** CollectorTask (Domain Layer)  
**Jira Task:** KIEM-12 / WRP-BE-TESTS-012  
**Type:** Positive + Negative  
**Layer:** Domain Entity  

---

## Objective

Verify that the `CollectionTask` domain entity enforces state transition rules correctly and logs each transition.

---

## State Machine

```
[Created] --> Assigned --> OnTheWay --> Collected
```

All other transitions are invalid and must throw `InvalidOperationException`.

---

## Test Scenarios

### Valid Transitions

| # | From | To | Expected Outcome |
|---|---|---|---|
| 1 | Created (Assigned) | OnTheWay | Status = OnTheWay, 1 StatusLog entry |
| 2 | OnTheWay | Collected | Status = Collected, 2 StatusLog entries, CompletedAt set |

### Invalid Transitions

| # | From | To | Expected Exception | Expected Message |
|---|---|---|---|---|
| 3 | OnTheWay | OnTheWay | `InvalidOperationException` | "Task must be Assigned before going OnTheWay" |
| 4 | Collected | OnTheWay | `InvalidOperationException` | "Task must be Assigned before going OnTheWay" |
| 5 | Assigned | Collected (skip OnTheWay) | `InvalidOperationException` | "Task must be OnTheWay before Collected" |
| 6 | Collected | Collected | `InvalidOperationException` | "Task must be OnTheWay before Collected" |

### Data Integrity

| # | Scenario | Expected |
|---|---|---|
| 7 | Complete with weight = 12.5 and notes | `CollectedWeightKg = 12.5`, `Notes` saved |
| 8 | Complete with null notes | `Notes = null`, no exception |
| 9 | Create two tasks | Each has unique `Id` |
| 10 | Full lifecycle | 2 StatusLogs: OnTheWay + Collected |

---

## Unit Test Coverage

- `CollectionTaskTests.SetOnTheWay_WhenAssigned_ShouldChangeStatusAndCreateLog` (existing)
- `CollectionTaskTests.SetOnTheWay_WhenNotAssigned_ShouldThrow` (existing)
- `CollectionTaskTests.Complete_WhenOnTheWay_ShouldMoveToCollectedAndPersistDetails` (existing)
- `CollectionTaskTests.Complete_WhenNotOnTheWay_ShouldThrow` (existing)
- `CollectionTaskTests.Create_ShouldInitializeTaskWithAssignedStatusAndIds` (existing)
- `CollectionTaskTests.AssignCollector_ShouldStoreCollectorId` (existing)

---

**Tested By:** Nguyễn Minh Phụng **Date:** 2026-05-27
