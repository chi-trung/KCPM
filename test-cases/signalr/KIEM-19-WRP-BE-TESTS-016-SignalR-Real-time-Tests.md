# 🔔 KIEM-19: WRP-BE-TESTS-016 - SignalR Real-time Notifications

**Status:** 🟦 IN PROGRESS  
**Branch:** `KIEM-19-WRP-BE-TESTS-016-SignalR-Real-time-Tests`  
**Jira Link:** KIEM-19  
**Module:** SignalR Hub - Real-time Notifications & Task Updates  
**Report Style:** Allure HTML + raw websocket evidence

## 📎 Allure Evidence

- Raw results folder: `TestResults/backend-allure-report` or `Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/bin/Release/net8.0/allure-results`
- Generated HTML report: `TestResults/backend-allure-report/index.html`
- This SignalR run was verified by a live websocket handshake and a real `NewNotification` payload.

## 🎯 Week-1 Scope

For the SignalR week, validate at least these 3 checks first:

1. `TC-SIGNALR-001` Connect to hub valid token
2. `TC-SIGNALR-004` Receive notification on event
3. `TC-SIGNALR-005` Task assigned notification

This week I validated the first 2 flows by connecting to the hub and receiving a real notification event.

---

## 📋 Test Case Summary

| TC ID | Test Case Name | Type | Status | Priority |
|:---:|:---|:---:|:---:|:---:|
| **TC-SIGNALR-001** | Connect to hub valid token | ✅ Positive | ✅ Pass | 🔴 High |
| **TC-SIGNALR-002** | Connect to hub invalid token | ❌ Negative | ⬜ TBD | 🔴 High |
| **TC-SIGNALR-003** | Connect without authentication | ❌ Negative | ⬜ TBD | 🔴 High |
| **TC-SIGNALR-004** | Receive notification on event | ✅ Positive | ✅ Pass | 🔴 High |
| **TC-SIGNALR-005** | Task assigned notification | ✅ Positive | ⬜ TBD | 🔴 High |
| **TC-SIGNALR-006** | Report status changed notification | ✅ Positive | ⬜ TBD | 🔴 High |
| **TC-SIGNALR-007** | Complaint resolved notification | ✅ Positive | ⬜ TBD | 🟡 Medium |
| **TC-SIGNALR-008** | Disconnect gracefully | ✅ Positive | ⬜ TBD | 🟡 Medium |
| **TC-SIGNALR-009** | Reconnect after disconnect | ✅ Positive | ⬜ TBD | 🟡 Medium |
| **TC-SIGNALR-010** | Message delivery timeout | ❌ Negative | ⬜ TBD | 🟢 Low |

---

## 🎯 Test Objectives

- ✅ Validate WebSocket connection establishment with JWT authentication
- ✅ Validate authentication rejection for invalid/missing tokens
- ✅ Validate real-time notifications are delivered to connected clients
- ✅ Validate different event types trigger appropriate notifications
- ✅ Validate graceful connection handling (connect/disconnect/reconnect)
- ✅ Validate message delivery and ordering
- ✅ Validate hub connection limit and timeout handling

## 📊 Week-1 Execution Summary

| Test Case ID | Result | Evidence |
|:---:|:---:|:---|
| TC-SIGNALR-001 | ✅ Pass | Live websocket handshake to `/hubs/task` |
| TC-SIGNALR-004 | ✅ Pass | Live `NewNotification` payload after report creation |

## 🧪 Actual Execution

| Item | Value |
|------|-------|
| Execution Date | 2026-05-23 |
| Hub URL | `ws://localhost:8080/hubs/task` |
| Auth Token Source | Citizen registration during live run |
| Trigger Action | `POST /api/reports/create` |
| Triggered Event | `NewNotification` |
| Overall Status | SignalR live test passed |

---

## 📝 Detailed Test Cases

### TC-SIGNALR-001: Connect to hub valid token ✅ (Positive)

**Objective:** Verify that user can establish WebSocket connection with valid JWT token

**Preconditions:**
- User is authenticated with valid JWT token
- SignalR hub endpoint: `ws://localhost:5000/signalR/notifications`
- Server is running and hub is listening

**Steps:**
```
1. Establish WebSocket connection to /signalR/notifications
   Headers:
   {
     "Authorization": "Bearer {{auth_token}}"
   }
   
2. Send invoke request: "JoinGroup" with groupId="notifications"
```

**Expected Result:**
- ✅ Connection established: Status `101 Switching Protocols`
- ✅ Hub returns: `{"type":"invocation","invocationId":"1","result":true}`
- ✅ Client successfully joined group "notifications"
- ✅ Connection remains open (no auto-disconnect)
- ✅ Server logs connection event

**Actual Result:**
- ✅ Connection established and handshake completed
- ✅ Server accepted the token via `access_token` query string
- ✅ WebSocket stayed open for event listening

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-001`

---

### TC-SIGNALR-002: Connect to hub invalid token ❌ (Negative)

**Objective:** Verify that SignalR hub rejects connection with invalid JWT token

**Preconditions:**
- Invalid/expired JWT token: `invalid_token_xyz`
- SignalR hub endpoint is running

**Steps:**
```
1. Attempt to establish WebSocket connection
   Headers:
   {
     "Authorization": "Bearer invalid_token_xyz"
   }
```

**Expected Result:**
- ✅ Connection rejected: Status `401 Unauthorized`
- ✅ Error message: "Unauthorized"
- ✅ Hub closes connection immediately
- ✅ No message exchange occurs

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-002`

---

### TC-SIGNALR-003: Connect without authentication ❌ (Negative)

**Objective:** Verify that SignalR hub requires authentication

**Preconditions:**
- No authentication token provided
- SignalR hub requires authentication

**Steps:**
```
1. Attempt to establish WebSocket connection
   Headers:
   {
     // No Authorization header
   }
```

**Expected Result:**
- ✅ Connection rejected: Status `401 Unauthorized`
- ✅ Connection closed by server
- ✅ Error logged on server

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-003`

---

### TC-SIGNALR-004: Receive notification on event ✅ (Positive)

**Objective:** Verify that connected clients receive real-time notifications

**Preconditions:**
- User 1 is connected to SignalR hub
- User 2 will trigger an event
- Both users have valid JWT tokens

**Steps:**
```
1. User 1: Connect to hub and join group "notifications"
2. User 2: Perform action (e.g., create complaint)
3. Verify User 1 receives notification message
```

**Expected Result:**
- ✅ User 1 receives message within 1 second:
  ```json
  {
    "type": "Notification",
    "title": "New Complaint",
    "message": "A new complaint has been filed",
    "timestamp": "2026-05-18T10:30:00Z"
  }
  ```
- ✅ Message contains correct action details
- ✅ Connection remains active after message

**Actual Result:**
- ✅ A real `NewNotification` payload arrived on the websocket
- ✅ The payload contained `title`, `message`, `actionUrl`, `relatedEntityId`, and `createdAt`
- ✅ The event was produced by creating a report for the same connected user

**Captured Payload:**
```json
{
  "type": 1,
  "target": "NewNotification",
  "arguments": [
    {
      "id": "9583ff99-b1f6-413d-9985-a4c063821c6f",
      "type": 0,
      "title": "Báo cáo đã gửi thành công",
      "message": "Báo cáo #7f2bccec của bạn đã được gửi và đang chờ xác nhận.",
      "actionUrl": "/citizen/reports/7f2bccec-e87e-435d-af5e-753d6566f185",
      "relatedEntityId": "7f2bccec-e87e-435d-af5e-753d6566f185",
      "relatedEntityType": "Report",
      "createdAt": "2026-05-23T15:22:19.0168515Z"
    }
  ]
}
```

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-004`

---

### TC-SIGNALR-005: Task assigned notification ✅ (Positive)

**Objective:** Verify that collectors receive notifications when assigned new tasks

**Preconditions:**
- Collector is connected to SignalR hub
- Admin/Enterprise has permissions to assign tasks
- System has pending collection tasks

**Steps:**
```
1. Collector: Connect to hub
2. Enterprise: Assign task to collector via POST /api/tasks/assign
3. Verify collector receives notification
```

**Expected Result:**
- ✅ Collector receives task assignment notification:
  ```json
  {
    "type": "TaskAssigned",
    "taskId": "<uuid>",
    "taskName": "Collect waste at Location A",
    "priority": "High",
    "location": "Building 5, Floor 2",
    "estimatedDuration": 30
  }
  ```
- ✅ Notification includes all task details
- ✅ Collector can act on notification immediately

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-005`

---

### TC-SIGNALR-006: Report status changed notification ✅ (Positive)

**Objective:** Verify that citizens receive notifications when report status changes

**Preconditions:**
- Citizen is connected to SignalR hub
- Citizen has submitted waste report
- Admin/Staff has permission to update report status

**Steps:**
```
1. Citizen: Connect to hub
2. Admin: Update report status from "Pending" to "Approved"
   PUT /api/reports/{reportId}/approve
3. Verify citizen receives status change notification
```

**Expected Result:**
- ✅ Citizen receives notification:
  ```json
  {
    "type": "ReportStatusChanged",
    "reportId": "<uuid>",
    "oldStatus": "Pending",
    "newStatus": "Approved",
    "message": "Your waste report has been approved",
    "reward": 50
  }
  ```
- ✅ Notification includes status details and rewards

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-006`

---

### TC-SIGNALR-007: Complaint resolved notification ✅ (Positive)

**Objective:** Verify that complaint filers receive notifications when complaint is resolved

**Preconditions:**
- Citizen is connected to SignalR hub
- Citizen has filed complaint
- Admin has permission to resolve complaints

**Steps:**
```
1. Citizen: Connect to hub and join complaints group
2. Admin: Resolve complaint via PUT /api/complaints/{id}/resolve
3. Verify citizen receives resolution notification
```

**Expected Result:**
- ✅ Citizen receives notification:
  ```json
  {
    "type": "ComplaintResolved",
    "complaintId": "<uuid>",
    "resolution": "Issue has been addressed",
    "resolvedDate": "2026-05-18T10:35:00Z"
  }
  ```

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-007`

---

### TC-SIGNALR-008: Disconnect gracefully ✅ (Positive)

**Objective:** Verify that clients can disconnect cleanly without errors

**Preconditions:**
- Client is connected to SignalR hub
- Connection has been active for > 5 seconds

**Steps:**
```
1. Send disconnect request or close WebSocket
2. Verify server cleans up connection
3. Check no dangling connections remain
```

**Expected Result:**
- ✅ WebSocket closes with status `1000 Normal Closure`
- ✅ Server removes user from all groups
- ✅ No connection leak in server logs
- ✅ No error messages logged

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-008`

---

### TC-SIGNALR-009: Reconnect after disconnect ✅ (Positive)

**Objective:** Verify that clients can reconnect after temporary disconnection

**Preconditions:**
- Client has disconnected from hub
- Same user session still valid

**Steps:**
```
1. Connect to hub
2. Wait 5 seconds
3. Disconnect
4. Wait 2 seconds
5. Reconnect with same token
```

**Expected Result:**
- ✅ Reconnection successful
- ✅ No loss of user state
- ✅ Can receive new notifications immediately
- ✅ Reconnection time < 1 second

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-009`

---

### TC-SIGNALR-010: Message delivery timeout ❌ (Negative)

**Objective:** Verify that system handles slow/delayed message delivery

**Preconditions:**
- Network simulates 5 second delay
- Client has timeout set to 3 seconds

**Steps:**
```
1. Simulate slow network (5s latency)
2. Send message from one client
3. Measure delivery time and timeout behavior
```

**Expected Result:**
- ✅ Message delivery timeout after 3 seconds
- ✅ Connection drops gracefully
- ✅ Client receives timeout error
- ✅ Automatic reconnect attempted

**Evidence Location:** `postman-results/results.json` → `TC-SIGNALR-010`

---

## 🔄 Test Execution Flow

```
1. Setup Test Environment
   ├─ Start SignalR Hub Server
   ├─ Prepare authentication tokens
   └─ Create test users (Collector, Citizen, Admin)

2. Connection Tests
   ├─ TC-SIGNALR-001: Valid connection
   ├─ TC-SIGNALR-002: Invalid token
   └─ TC-SIGNALR-003: No authentication

3. Notification Tests
   ├─ TC-SIGNALR-004: Generic notification
   ├─ TC-SIGNALR-005: Task assignment
   ├─ TC-SIGNALR-006: Report status change
   └─ TC-SIGNALR-007: Complaint resolution

4. Connection Lifecycle Tests
   ├─ TC-SIGNALR-008: Graceful disconnect
   ├─ TC-SIGNALR-009: Reconnect
   └─ TC-SIGNALR-010: Message timeout

5. Generate Reports
   ├─ Connection statistics
   ├─ Message delivery metrics
   └─ Error handling report
```

---

## 📊 Test Results

**Last Run:** 2026-05-23  
**Pass Rate:** 2/2 verified live  
**Average Connection Time:** < 1s for handshake  
**Message Delivery Rate:** 1/1 live notification delivered

| Test Case | Status | Duration | Notes |
|:---|:---:|:---:|:---|
| TC-SIGNALR-001 | ✅ Pass | < 1s | WebSocket handshake completed |
| TC-SIGNALR-002 | ⬜ TBD | - | - |
| TC-SIGNALR-003 | ⬜ TBD | - | - |
| TC-SIGNALR-004 | ✅ Pass | < 1s | Received `NewNotification` from live report creation |
| TC-SIGNALR-005 | ⬜ TBD | - | - |
| TC-SIGNALR-006 | ⬜ TBD | - | - |
| TC-SIGNALR-007 | ⬜ TBD | - | - |
| TC-SIGNALR-008 | ⬜ TBD | - | - |
| TC-SIGNALR-009 | ⬜ TBD | - | - |
| TC-SIGNALR-010 | ⬜ TBD | - | - |

---

## 🛠️ Testing Tools & Setup

**SignalR Hub Client:** Postman WebSocket extension or custom C# test client  
**Test Framework:** xUnit + SignalR.Client  
**Environment Variables:**
```json
{
  "signalr_url": "ws://localhost:5000/signalR/notifications",
  "auth_token": "{{token_from_login}}",
  "test_collector_token": "{{collector_auth_token}}",
  "test_citizen_token": "{{citizen_auth_token}}",
  "test_admin_token": "{{admin_auth_token}}"
}
```

---

## 🔧 SignalR Hub Endpoints

**Hub URL:** `/signalR/notifications`

**Methods (Client can invoke):**
- `JoinGroup(string groupId)` - Join notification group
- `LeaveGroup(string groupId)` - Leave notification group
- `SendMessage(string message)` - Send broadcast message

**Server Push Methods (Server sends to client):**
- `ReceiveNotification(NotificationDto notification)` - Send notification
- `TaskAssigned(TaskDto task)` - Task assignment event
- `ReportStatusChanged(ReportStatusChangeDto change)` - Report status update
- `ComplaintResolved(ComplaintDto complaint)` - Complaint resolution

---

## ✅ Checklist for Completion

- [ ] All 10 test cases written and documented
- [x] WebSocket connection tests passing
- [x] Event notification tests passing
- [ ] SignalR hub methods verified
- [ ] Load testing completed (10+ concurrent connections)
- [ ] Connection timeout handling verified
- [ ] Reconnect logic validated
- [ ] Test results exported and archived
- [ ] Code coverage > 85% for SignalR handlers
- [ ] PR created and merged
- [ ] Jira issue marked Done

---

## 📎 References

- **SignalR Documentation:** https://learn.microsoft.com/en-us/aspnet/core/signalr/
- **Hub Implementation:** [SignalR Hub Code](../../Waste-Recycling-Platform/backend/src/WastePlatform.Infrastructure/SignalR/)
- **API Documentation:** [Backend README](../../Waste-Recycling-Platform/backend/README.md)
- **Testing Guide:** [Testing Playbook](../../Waste-Recycling-Platform/docs/testing-playbook.md)
- **Jira Ticket:** https://jira.example.com/browse/KIEM-19

---

**Last Updated:** 2026-05-18  
**Author:** KIEM-19 Test Team  
**Status:** 🟦 In Progress
