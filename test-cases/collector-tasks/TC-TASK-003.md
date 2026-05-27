# TC-TASK-003: Set On The Way — Valid and Invalid Transitions

**Module:** CollectorTask  
**Jira Task:** KIEM-12 / WRP-BE-TESTS-012  
**Type:** Positive + Negative  
**Endpoint:** `PUT /api/collector/tasks/{id}/on-the-way`  
**Role Required:** Collector  

---

## Objective

Verify that a Collector can transition an Assigned task to OnTheWay, and that invalid transitions are rejected with a 400 error.

---

## Preconditions

- Collector user is authenticated
- A task with `status = Assigned` is assigned to this Collector

---

## State Machine

```
Assigned --> OnTheWay --> Collected
```

---

## Test Scenarios

| # | Scenario | Task Status Before | Expected Status | Expected Body |
|---|---|---|---|---|
| 1 | Valid: Assigned -> OnTheWay | `Assigned` | 200 OK | `{ "message": "Task status updated to OnTheWay.", "taskId": "..." }` |
| 2 | Invalid: Already OnTheWay -> OnTheWay | `OnTheWay` | 400 Bad Request | `{ "message": "Task must be Assigned before going OnTheWay" }` |
| 3 | Task not found | random UUID | 404 Not Found | `{ "message": "Task not found or not assigned to you." }` |
| 4 | No token | — | 401 Unauthorized | — |

---

## Side Effects (Scenario 1)

- `CollectionTask.Status` changes to `OnTheWay`
- A `TaskStatusLog` entry with `Status = OnTheWay` is created
- SignalR event `TaskStatusUpdated` is broadcast to all clients
- Notification `NotifyCollectorOnTheWayAsync` is sent to the Citizen

---

## Unit Test Coverage

- `CollectorTaskControllerTests.SetOnTheWay_WhenTaskBelongsToCollector_ShouldUpdateStatusBroadcastAndNotify` (existing)
- `CollectorTaskControllerExtendedTests.SetOnTheWay_WhenAlreadyOnTheWay_ShouldReturnBadRequest`
- `CollectorTaskControllerExtendedTests.SetOnTheWay_WhenTaskNotFound_ShouldReturnNotFound`

---

**Tested By:** Nguyễn Minh Phụng **Date:** 2026-05-27
