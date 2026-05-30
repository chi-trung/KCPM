# TC-REP-005: Accept Report - Authorized Role (Enterprise/Admin)

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-REP-005 |
| **Jira Task** | WRP-BE-TESTS-002 |
| **Module** | Reports |
| **Priority** | High |
| **Test Type** | Positive |
| **API Endpoint** | `POST /api/reports/{reportId}/accept` |
| **Created Date** | 2025-05-17 |

## 🎯 Objective

Verify that authorized roles (Enterprise or Admin) can successfully accept a pending waste report and change its status appropriately.

## ✅ Pre-conditions

1. Backend server running
2. Report exists with status "Pending" (from TC-REP-001)
3. Enterprise or Admin user authenticated
4. Report is in service area của Enterprise (nếu có phân vùng)

## 🔧 Test Data

**Path Parameter:**
```
reportId: {reportId} (existing pending report)
```

**Request Body:**
```json
{
  "notes": "Tiếp nhận và xử lý trong 24h",
  "estimatedCollectionTime": "2025-05-18T14:00:00Z"
}
```

**Headers:**
```
Authorization: Bearer {enterpriseToken} hoặc {adminToken}
```

## 📝 Test Steps

1. Create/get a pending report (TC-REP-001)
2. Login as Enterprise hoặc Admin
3. Send POST request đến endpoint accept
4. Verify response status
5. Check report status changed to "Accepted"
6. Verify notification sent to citizen

## ✔️ Expected Results

### Response Status
- **HTTP Code**: `200`
- **Response Time**: < 1500ms

### Response Body (Expected - Verify with Actual API)
```json
{
  "message": "Report accepted successfully",
  "reportId": "ba46f4e6-3e7d-4e3c-81c7-acda3e40cbeb",
  "reportStatus": "Accepted"
}
```

### Database Changes
- `status` = "Accepted"
- `accepted_by` = enterprise/admin ID
- `accepted_at` = timestamp
- `notes` saved

### Side Effects
- Notification created for citizen
- Report appears in enterprise's task list
- Audit log entry created

## 🔄 Actual Results

### Execution Date: ⬜ Not executed

| Check | Result |
|-------|--------|
| HTTP Status 200 | ⬜ |
| Status changed to Accepted | ⬜ |
| acceptedBy populated | ⬜ |
| Notification created | ⬜ |
| Audit log entry | ⬜ |

## 📊 Status

⬜ **Not Tested** | ⬜ **Pass** | ⬜ **Fail**

## 🔗 Related Test Cases

- TC-REP-001: Create report (Prerequisite)
- TC-REP-006: Reject report
- TC-REP-007: Invalid state transition

## 📝 Notes

- Test với cả Enterprise và Admin roles
- Test accept report đã được accept (should fail - TC-REP-007)
- Test accept report đã bị reject (should fail - TC-REP-007)
- Verify business rules: Enterprise chỉ accept report trong service area

---

**Tested By**: Nguyễn Minh Phụng **Date**: 2026-05-26
