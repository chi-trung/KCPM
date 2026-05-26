# TC-REP-002: Create Report with Missing Required Fields

## 📋 Test Information

| Field | Value |
|-------|-------|
| **Test Case ID** | TC-REP-002 |
| **Jira Task** | WRP-BE-TESTS-002 |
| **Module** | Reports |
| **Priority** | High |
| **Test Type** | Negative |
| **API Endpoint** | `POST /api/reports/create` |
| **Created Date** | 2025-05-17 |

## 🎯 Objective

Verify that the API properly rejects report creation requests with missing required fields and returns clear validation error messages.

## ✅ Pre-conditions

1. Backend server running
2. Valid citizen token available

## 🔧 Test Scenarios

### Scenario 1: Missing WasteCategoryId
```json
{
  "latitude": 10.7769,
  "longitude": 106.7009,
  "description": "Test report",
  "address": "Test address"
  // Missing: WasteCategoryId
}
```

### Scenario 2: Missing Location (Latitude/Longitude)
```json
{
  "wasteCategoryId": 1,
  "description": "Test report",
  "address": "Test address"
  // Missing: latitude, longitude
}
```

### Scenario 3: Missing Description
```json
{
  "wasteCategoryId": 1,
  "latitude": 10.7769,
  "longitude": 106.7009,
  "address": "Test address"
  // Missing: description
}
```

### Scenario 4: Missing Image
```json
{
  "wasteCategoryId": 1,
  "latitude": 10.7769,
  "longitude": 106.7009,
  "description": "Test report",
  "address": "Test address"
  // Missing: images (if required)
}
```

## ✔️ Expected Results

### Response Status
- **HTTP Code**: `400` (Bad Request) or `422` (Unprocessable Entity)
- **Response Time**: < 1000ms

### Error Response Structure
```json
{
  "success": false,
  "error": "Validation failed",
  "errors": [
    {
      "field": "wasteCategoryId",
      "message": "Waste category is required"
    }
  ]
}
```

### Expected Error Messages

| Missing Field | Expected Error Message |
|---------------|-------------------------|
| WasteCategoryId | "Waste category is required" |
| Latitude | "Latitude is required" |
| Longitude | "Longitude is required" |
| Description | "Description is required" |
| Images | "At least one image is required" (if mandatory) |

### Database Verification
- No new report record created
- No partial data saved

## 🔄 Actual Results

### Execution Date: ⬜ Not executed

| Scenario | HTTP Status | Error Message | Pass/Fail |
|----------|-------------|---------------|-----------|
| Missing WasteCategoryId | ⬜ | ⬜ | ⬜ |
| Missing Location | ⬜ | ⬜ | ⬜ |
| Missing Description | ⬜ | ⬜ | ⬜ |
| Missing Image | ⬜ | ⬜ | ⬜ |

## 📊 Status

⬜ **Not Tested** | ⬜ **Pass** | ⬜ **Fail**

## 🐛 Defects (if any)

| Defect ID | Description | Severity | Status |
|-----------|-------------|----------|--------|
| ⬜ | ⬜ | ⬜ | ⬜ |

## 🔗 Related Test Cases

- TC-REP-001: Create report valid (Positive)
- TC-REP-008: Upload invalid image format (Negative)

## 📝 Notes

- Test each missing field in isolation first
- Then test multiple missing fields combined
- Verify error messages are user-friendly
- Check if validation happens before or after image upload (performance)

---

**Tested By**: Nguyễn Minh Phụng **Date**: 2026-05-26
