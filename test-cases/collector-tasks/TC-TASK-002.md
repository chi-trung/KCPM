# TC-TASK-002: Get Task By ID

**Module:** CollectorTask  
**Jira Task:** KIEM-12 / WRP-BE-TESTS-012  
**Type:** Positive + Negative  
**Endpoint:** `GET /api/collector/tasks/{id}`  
**Role Required:** Collector  

---

## Objective

Verify that a Collector can retrieve full detail of a single task by its ID, and that tasks belonging to other collectors are not accessible.

---

## Preconditions

- Collector user is authenticated
- A `CollectionTask` assigned to this Collector exists in the database

---

## Test Scenarios

| # | Scenario | Input | Expected Status | Expected Body |
|---|---|---|---|---|
| 1 | Valid task ID, belongs to this Collector | `{id}` = existing task | 200 OK | Full task object with `statusLogs`, `images`, `report` |
| 2 | Task ID does not exist | `{id}` = random UUID | 404 Not Found | `{ "message": "Task not found or not assigned to you." }` |
| 3 | Task belongs to a different Collector | `{id}` = another collector's task | 404 Not Found | `{ "message": "Task not found or not assigned to you." }` |
| 4 | No token | No Authorization header | 401 Unauthorized | — |

---

## Expected Response Body (Scenario 1)

```json
{
  "id": "<uuid>",
  "reportId": "<uuid>",
  "status": "Assigned",
  "collectedWeightKg": null,
  "notes": null,
  "assignedAt": "<datetime>",
  "completedAt": null,
  "report": {
    "id": "<uuid>",
    "description": "...",
    "address": "...",
    "imageUrls": []
  },
  "images": [],
  "statusLogs": []
}
```

---

## Unit Test Coverage

- `CollectorTaskControllerExtendedTests.GetTaskById_WhenTaskBelongsToCollector_ShouldReturnOkWithDetails`
- `CollectorTaskControllerExtendedTests.GetTaskById_WhenTaskNotFound_ShouldReturnNotFound`
- `CollectorTaskControllerExtendedTests.GetTaskById_WhenTaskBelongsToOtherCollector_ShouldReturnNotFound`

---

**Tested By:** Nguyễn Minh Phụng **Date:** 2026-05-27
