# TC-REP-007: Invalid State Transition

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-REP-007 |
| **Jira Task** | WRP-BE-TESTS-002 |
| **Module** | Reports |
| **Priority** | Medium |
| **Test Type** | Negative |
| **Created Date** | 2025-05-17 |

## 🎯 Objective

Verify that the system prevents invalid state transitions (e.g., accepting an already accepted/rejected/completed report).

## ✅ Pre-conditions

1. Backend server running
2. Reports exist in various states

## 🔧 Test Scenarios

### Invalid Transitions

| Current State | Action | Expected Result |
|---------------|--------|-----------------|
| Accepted | Accept again | ❌ Error |
| Rejected | Accept | ❌ Error |
| Rejected | Reject again | ❌ Error |
| Completed | Accept | ❌ Error |
| Completed | Reject | ❌ Error |

## ✔️ Expected Results

### Response Status
- **HTTP Code**: `400` (Bad Request) hoặc `409` (Conflict)
- **Response Time**: < 1000ms

### Error Response
```json
{
  "message": "Report can only be accepted if it is in Pending status. Current status: Accepted"
}
```

### Valid State Machine
```
Pending → Accepted → Completed
   ↓         ↓
Rejected   (không thể reject sau accept)
```

## 🔄 Actual Results

### Execution Date: ⬜ Not executed

| Transition | HTTP Status | Error Message | Pass/Fail |
|------------|-------------|---------------|-----------|
| Accepted → Accept | ⬜ | ⬜ | ⬜ |
| Rejected → Accept | ⬜ | ⬜ | ⬜ |
| Completed → Reject | ⬜ | ⬜ | ⬜ |

## 📊 Status

⬜ **Not Tested** | ⬜ **Pass** | ⬜ **Fail**

## 📝 Notes

- State machine enforcement là critical business rule
- Error message nên rõ ràng về state hiện tại
- Không cho phép "undo" reject (có thể cần feature riêng)

---

**Tested By**: Nguyễn Minh Phụng **Date**: 2026-05-26
