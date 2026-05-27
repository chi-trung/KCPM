# TC-TASK-004: Complete Task — Valid and Invalid Cases

**Module:** CollectorTask  
**Jira Task:** KIEM-12 / WRP-BE-TESTS-012  
**Type:** Positive + Negative  
**Endpoint:** `PUT /api/collector/tasks/{id}/complete`  
**Content-Type:** `multipart/form-data`  
**Role Required:** Collector  

---

## Objective

Verify that a Collector can complete a task in OnTheWay status with valid weight, and that invalid inputs (wrong state, missing/invalid weight, not found) are rejected correctly.

---

## Preconditions

- Collector user is authenticated
- Task is in `OnTheWay` status (must have called SetOnTheWay first)

---

## Test Scenarios

| # | Scenario | Task Status | WeightKg | Expected Status | Expected Body |
|---|---|---|---|---|---|
| 1 | Valid: complete with weight | `OnTheWay` | `12.5` | 200 OK | `{ "message": "Task completed successfully.", "taskId": "...", "reward": null/object }` |
| 2 | Valid: complete with reward rule active | `OnTheWay` | `12.5` | 200 OK | `reward.points > 0` |
| 3 | Invalid: WeightKg is not a number | `OnTheWay` | `"abc"` | 400 Bad Request | `{ "message": "Invalid or missing WeightKg." }` |
| 4 | Invalid: task is still Assigned (not OnTheWay) | `Assigned` | `10` | 400 Bad Request | `{ "message": "Task must be OnTheWay before Collected" }` |
| 5 | Task not found | random UUID | `10` | 404 Not Found | `{ "message": "Task not found or not assigned to you." }` |
| 6 | No token | — | — | 401 Unauthorized | — |

---

## Form Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `WeightKg` | decimal string | Yes | Weight of collected waste in kg |
| `Notes` | string | No | Optional collector notes |
| `Images` | file[] | No | Optional confirmation images (.jpg, .jpeg, .png, .gif) |

---

## Side Effects (Scenario 1)

- `CollectionTask.Status` changes to `Collected`
- `CollectionTask.CollectedWeightKg` and `Notes` are saved
- `WasteReport.Status` transitions to `Collected`
- If reward rule exists: `RewardPoints` record is created for Citizen
- SignalR event `TaskStatusUpdated` is broadcast to all clients
- SignalR event `RewardReceived` is sent to Citizen (if reward applies)

---

## Unit Test Coverage

- `CollectorTaskControllerTests.CompleteTask_WithRewardRule_ShouldCollectTaskCreateRewardAndBroadcast` (existing)
- `CollectorTaskControllerTests.CompleteTask_WithInvalidWeight_ShouldReturnBadRequest` (existing)
- `CollectorTaskControllerExtendedTests.CompleteTask_WhenTaskNotFound_ShouldReturnNotFound`
- `CollectorTaskControllerExtendedTests.CompleteTask_WhenTaskNotOnTheWay_ShouldReturnBadRequest`

---

**Tested By:** Nguyễn Minh Phụng **Date:** 2026-05-27
