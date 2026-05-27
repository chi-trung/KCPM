# TC-TASK-001: Get Collector Tasks List

**Module:** CollectorTask  
**Jira Task:** KIEM-12 / WRP-BE-TESTS-012  
**Type:** Positive + Negative  
**Endpoint:** `GET /api/collector/tasks`  
**Role Required:** Collector  

---

## Objective

Verify that a Collector can retrieve their assigned task list, optionally filtered by status.

---

## Preconditions

- Collector user is authenticated (JWT token with role `Collector`)
- At least one `CollectionTask` is assigned to the Collector in the database

---

## Test Scenarios

| # | Scenario | Input | Expected Status | Expected Body |
|---|---|---|---|---|
| 1 | Valid Collector, no filter | Authorization: Bearer `<collector_token>` | 200 OK | Array of task objects |
| 2 | Valid Collector, filter `?status=Assigned` | `?status=Assigned` | 200 OK | Only tasks with `status = "Assigned"` |
| 3 | Valid Collector, filter `?status=OnTheWay` | `?status=OnTheWay` | 200 OK | Only tasks with `status = "OnTheWay"` |
| 4 | Valid Collector, no tasks assigned | Valid token, no tasks in DB | 200 OK | Empty array `[]` |
| 5 | No token | No Authorization header | 401 Unauthorized | — |
| 6 | Collector profile not found in DB | Valid JWT but no Collector record | 401 Unauthorized | `{ "message": "Collector profile not found for current user." }` |

---

## Expected Response Body (Scenario 1)

```json
[
  {
    "id": "<uuid>",
    "reportId": "<uuid>",
    "enterpriseId": "<uuid>",
    "collectorId": "<uuid>",
    "status": "Assigned",
    "collectedWeightKg": null,
    "notes": null,
    "assignedAt": "<datetime>",
    "completedAt": null,
    "report": {
      "id": "<uuid>",
      "description": "...",
      "address": "...",
      "latitude": 10.0,
      "longitude": 106.0,
      "status": "Assigned",
      "categoryName": "...",
      "citizenName": "...",
      "citizenPhone": "..."
    }
  }
]
```

---

## Unit Test Coverage

- `CollectorTaskControllerExtendedTests.GetTasks_WhenCollectorHasTasks_ShouldReturnOkWithList`
- `CollectorTaskControllerExtendedTests.GetTasks_WithStatusFilter_ShouldReturnFilteredList`
- `CollectorTaskControllerExtendedTests.GetTasks_WhenCollectorProfileNotFound_ShouldReturnUnauthorized`

---

**Tested By:** Nguyễn Minh Phụng **Date:** 2026-05-27
