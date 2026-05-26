# TC-REP-003: Get Report by ID - Valid Request

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-REP-003 |
| **Jira Task** | WRP-BE-TESTS-002 |
| **Module** | Reports |
| **Priority** | High |
| **Test Type** | Positive |
| **API Endpoint** | `GET /api/reports/{reportId}` |
| **Created Date** | 2025-05-17 |

## 🎯 Objective

Verify that users can retrieve a specific report by its ID with complete and accurate information.

## ✅ Pre-conditions

1. Backend server running
2. Valid report exists (from TC-REP-001 or seed data)
3. `reportId` variable set in Postman environment

## 🔧 Test Data

**Path Parameter:**
```
reportId: {reportId} (from Postman variable)
```

**Headers:**
```
Authorization: Bearer {citizenToken}
```

## 📝 Test Steps

1. Ensure `reportId` variable exists (run TC-REP-001 first or use existing ID)
2. Send GET request to `/api/reports/{{reportId}}`
3. Verify response status
4. Check all fields match created report
5. Verify image URLs are accessible

## ✔️ Expected Results

### Response Status
- **HTTP Code**: `200`
- **Response Time**: < 1000ms

### Response Body (Actual API Structure)
```json
{
  "id": "ba46f4e6-3e7d-4e3c-81c7-acda3e40cbeb",
  "citizenId": "5c76be7c-b6bb-49d8-9a78-02ac881064ca",
  "citizenName": "Test User",
  "wasteCategoryId": 1,
  "categoryName": "Rác thải sinh hoạt",
  "description": "Sample waste report from Postman",
  "latitude": 10.7769,
  "longitude": 106.7009,
  "address": "123 Nguyen Trai, District 1",
  "status": "Pending",
  "aiSuggestion": "General waste - manual review needed",
  "createdAt": "2026-05-17T14:28:35Z",
  "imageUrls": [
    "a1daf53e-d8f0-4067-aa8e-2374af32134a.png"
  ],
  "rewardPoints": []
}
```

### Data Integrity Checks
- All fields match original creation data
- Timestamps in correct format (ISO 8601)
- Image URLs are valid paths
- Coordinates within valid range

## 🔄 Actual Results

### Execution Date: ⬜ Not executed

| Check | Result |
|-------|--------|
| HTTP Status 200 | ⬜ |
| All fields present | ⬜ |
| Data matches creation | ⬜ |
| Image URLs valid | ⬜ |
| Response time < 1000ms | ⬜ ms |

## 📊 Status

⬜ **Not Tested** | ⬜ **Pass** | ⬜ **Fail**

## 🐛 Defects (if any)

| Defect ID | Description | Severity | Status |
|-----------|-------------|----------|--------|
| ⬜ | ⬜ | ⬜ | ⬜ |

## 🔗 Related Test Cases

- TC-REP-001: Create report (Prerequisite)
- TC-REP-004: Get report invalid ID (Negative)

---

**Tested By**: Nguyễn Minh Phụng **Date**: 2026-05-26
