# TC-REP-004: Get Report by ID - Invalid/Non-existent ID

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-REP-004 |
| **Jira Task** | WRP-BE-TESTS-002 |
| **Module** | Reports |
| **Priority** | Medium |
| **Test Type** | Negative |
| **API Endpoint** | `GET /api/reports/{reportId}` |
| **Created Date** | 2025-05-17 |

## 🎯 Objective

Verify that API properly handles requests for non-existent or invalid report IDs with appropriate error responses.

## ✅ Pre-conditions

1. Backend server running
2. Valid authentication token

## 🔧 Test Scenarios

### Scenario 1: Non-existent UUID
```
GET /api/reports/11111111-1111-1111-1111-111111111111
```
(UUID đúng format nhưng không tồn tại)

### Scenario 2: Invalid UUID Format
```
GET /api/reports/invalid-id-123
```

### Scenario 3: Empty ID
```
GET /api/reports/
```

### Scenario 4: Special Characters
```
GET /api/reports/<script>alert('xss')</script>
```

## ✔️ Expected Results

### Response Status

| Scenario | HTTP Code |
|----------|-----------|
| Non-existent UUID | `404` (Not Found) |
| Invalid format | `400` (Bad Request) hoặc `404` |
| Empty ID | `400` hoặc route không match |
| Special chars | `400` hoặc sanitized |

### Error Response (Scenario 1 & 2)
```json
{
  "success": false,
  "error": "Report not found",
  "message": "No report found with the provided ID"
}
```

### Security Requirements
- Không leak thông tin hệ thống
- Không expose stack trace
- Error message generic, không cho biết ID có tồn tại hay không

## 🔄 Actual Results

### Execution Date: ⬜ Not executed

| Scenario | HTTP Status | Error Message | Pass/Fail |
|----------|-------------|---------------|-----------|
| Non-existent UUID | ⬜ | ⬜ | ⬜ |
| Invalid format | ⬜ | ⬜ | ⬜ |
| Empty ID | ⬜ | ⬜ | ⬜ |
| Special chars | ⬜ | ⬜ | ⬜ |

## 📊 Status

⬜ **Not Tested** | ⬜ **Pass** | ⬜ **Fail**

## 🔗 Related Test Cases

- TC-REP-003: Get report valid ID (Positive)

---

**Tested By**: Nguyễn Minh Phụng **Date**: 2026-05-26
