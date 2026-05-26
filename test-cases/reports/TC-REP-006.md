# TC-REP-006: Reject Report with Reason - Authorized Role

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-REP-006 |
| **Jira Task** | WRP-BE-TESTS-002 |
| **Module** | Reports |
| **Priority** | High |
| **Test Type** | Positive |
| **API Endpoint** | `POST /api/reports/{reportId}/reject` |
| **Created Date** | 2025-05-17 |

## 🎯 Objective

Verify that authorized roles can reject a report with a valid reason, and the system properly records the rejection and notifies the citizen.

## ✅ Pre-conditions

1. Backend server running
2. Report exists with status "Pending"
3. Enterprise/Admin authenticated

## 🔧 Test Data

**Path Parameter:**
```
reportId: {reportId}
```

**Request Body:**
```json
{
  "reason": "Ngoài khu vực phục vụ - Quận 5 ngoài phạm vi",
  "category": "OUT_OF_SERVICE_AREA"
}
```

**Rejection Reasons hợp lệ:**
- "Ngoài khu vực phục vụ"
- "Hình ảnh không rõ / không hợp lệ"
- "Thông tin không đầy đủ"
- "Không phải rác thải tái chế được"

## ✔️ Expected Results

### Response Status
- **HTTP Code**: `200`
- **Response Time**: < 1500ms

### Response Body
```json
{
  "success": true,
  "message": "Report rejected successfully",
  "data": {
    "reportId": "uuid",
    "status": "Rejected",
    "previousStatus": "Pending",
    "rejectedBy": {
      "userId": "enterprise-id",
      "name": "Green Recycle Co."
    },
    "rejectedAt": "2025-05-17T11:30:00Z",
    "reason": "Ngoài khu vực phục vụ - Quận 5 ngoài phạm vi",
    "rejectionCategory": "OUT_OF_SERVICE_AREA"
  }
}
```

### Database Changes
- `status` = "Rejected"
- `rejected_by`, `rejected_at` populated
- `rejection_reason` saved

### Side Effects
- Notification to citizen with reason
- Report không còn trong danh sách available cho enterprise khác

## 🔄 Actual Results

### Execution Date: ⬜ Not executed

| Check | Result |
|-------|--------|
| HTTP 200 | ⬜ |
| Status Rejected | ⬜ |
| Reason saved | ⬜ |
| Citizen notified | ⬜ |

## 📊 Status

⬜ **Not Tested** | ⬜ **Pass** | ⬜ **Fail**

---

**Tested By**: Nguyễn Minh Phụng **Date**: 2026-05-26
