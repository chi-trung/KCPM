# TC-TASK-006: Get Collector Stats

**Module:** CollectorTask  
**Jira Task:** KIEM-12 / WRP-BE-TESTS-012  
**Type:** Positive + Negative  
**Endpoint:** `GET /api/collector/tasks/stats`  
**Role Required:** Collector  

---

## Objective

Verify that the stats endpoint returns correct aggregated counts of tasks by status and total collected weight.

---

## Preconditions

- Collector user is authenticated
- Tasks exist in various statuses for this Collector

---

## Test Scenarios

| # | Scenario | Task Setup | Expected Status | Expected Body |
|---|---|---|---|---|
| 1 | Collector has 1 Assigned task | 1 task in `Assigned` | 200 OK | `TotalAssigned=1, TotalOnTheWay=0, TotalCollected=0, TotalWeightKg=0` |
| 2 | Mixed statuses | 1 Assigned, 1 OnTheWay, 1 Collected (5kg) | 200 OK | `TotalAssigned=1, TotalOnTheWay=1, TotalCollected=1, TotalWeightKg=5` |
| 3 | Collector has no tasks | No tasks in DB | 200 OK | All counts = 0 |
| 4 | Collector profile not found | Valid JWT, no Collector record | 401 Unauthorized | `{ "message": "Collector profile not found." }` |
| 5 | No token | — | 401 Unauthorized | — |

---

## Expected Response Body (Scenario 1)

```json
{
  "totalAssigned": 1,
  "totalOnTheWay": 0,
  "totalCollected": 0,
  "totalWeightKg": 0.0
}
```

---

## Unit Test Coverage

- `CollectorTaskControllerExtendedTests.GetStats_WhenCollectorHasTasks_ShouldReturnCorrectCounts`
- `CollectorTaskControllerExtendedTests.GetStats_WhenCollectorProfileNotFound_ShouldReturnUnauthorized`

---

**Tested By:** Nguyễn Minh Phụng **Date:** 2026-05-27
