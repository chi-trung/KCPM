# WRP-BE-TESTS-006: Analytics Module Testing

## Test Cases Summary

This document outlines the comprehensive test cases for the Analytics Module of the Waste Recycling Platform. These tests verify analytics functionality across Admin, Enterprise, and Public levels with emphasis on **date range query handling**.

---

## Analytics Module Architecture

### Endpoint Layers

```
Admin Analytics (Authorized - Admin Role)
    ├─ GET /api/admin/analytics/overview
    ├─ GET /api/admin/analytics/reports (with date range)
    ├─ GET /api/admin/analytics/users
    ├─ GET /api/admin/analytics/waste (with date range)
    └─ GET /api/admin/analytics/summary (with date range)

Enterprise Analytics (Authorized - Enterprise Role)
    └─ GET /api/enterprise/analytics/reports (scoped, with date range)

Public Analytics (No Auth Required)
    └─ GET /api/public/analytics/reports (last 3 months default, with date range)
```

---

## Test Cases by Category

### 1. Admin Analytics Overview (2 tests)

#### TC-ANALYTICS-001: Get Admin Overview
- **Endpoint:** `GET /api/admin/analytics/overview`
- **Authentication:** Admin Token (Bearer)
- **Query Parameters:** None
- **Expected Status:** 200 OK
- **Response Should Contain:**
  - totalReports (integer)
  - totalComplaints (integer)
  - totalUsers (integer)
  - totalEnterprises (integer)
  - totalCollectors (integer)
- **Purpose:** Verify admin can see overall platform statistics
- **Assertion:** 
  ```
  Status code = 200
  Response has all required metrics
  All metrics are non-negative integers
  ```

#### TC-ANALYTICS-002: Unauthorized Access to Admin Overview
- **Endpoint:** `GET /api/admin/analytics/overview`
- **Authentication:** Citizen Token (non-admin)
- **Expected Status:** 403 Forbidden
- **Purpose:** Verify non-admin cannot access admin analytics
- **Assertion:** Status code = 403

---

### 2. Admin Report Analytics with Date Range (6 tests)

#### TC-ANALYTICS-003: Get Report Analytics - No Date Filter
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** (empty)
- **Authentication:** Admin Token
- **Expected Status:** 200
- **Behavior:** Should use default date range (last 1 month)
- **Response Contains:**
  - reportCount (integer)
  - statusBreakdown (object with Pending, Approved, Rejected counts)
  - categoryDistribution (array of waste categories)
- **Purpose:** Verify default date handling
- **Assertion:** Status code = 200, response contains expected fields

#### TC-ANALYTICS-004: Get Report Analytics - Valid Date Range
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** 
  ```
  ?startDate=2026-01-01&endDate=2026-12-31
  ```
- **Authentication:** Admin Token
- **Expected Status:** 200
- **Expected Behavior:** Returns reports within specified range
- **Purpose:** Verify date range filtering works correctly
- **Assertion:** 
  ```
  Status code = 200
  All reports in response have createdDate between startDate and endDate
  Response not empty (if reports exist in range)
  ```

#### TC-ANALYTICS-005: Get Report Analytics - Only Start Date
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2026-01-01`
- **Expected Status:** 200
- **Expected Behavior:** Uses today as endDate
- **Purpose:** Verify partial date range handling
- **Assertion:** Status code = 200, data from startDate to now

#### TC-ANALYTICS-006: Get Report Analytics - Only End Date
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?endDate=2026-12-31`
- **Expected Status:** 200
- **Expected Behavior:** Uses default start (1 month before) as startDate
- **Purpose:** Verify partial date range handling
- **Assertion:** Status code = 200, data calculated correctly

#### TC-ANALYTICS-007: Get Report Analytics - Invalid Date Range (start > end)
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2026-12-31&endDate=2026-01-01`
- **Expected Status:** 400 Bad Request
- **Expected Response:** Error message about invalid date range
- **Purpose:** Verify validation of logical date ranges
- **Assertion:** Status code = 400, error message present

#### TC-ANALYTICS-008: Get Report Analytics - Invalid Date Format
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2026/01/01&endDate=invalid`
- **Expected Status:** 400 Bad Request
- **Purpose:** Verify date format validation (ISO 8601 required)
- **Assertion:** Status code = 400

---

### 3. Admin User Analytics (1 test)

#### TC-ANALYTICS-009: Get User Analytics
- **Endpoint:** `GET /api/admin/analytics/users`
- **Authentication:** Admin Token
- **Expected Status:** 200
- **Response Contains:**
  - totalUsers (integer)
  - byRole (object: Citizen, Collector, Enterprise, Admin counts)
  - byVerificationStatus (Verified, Unverified counts)
  - activeCount (integer)
- **Purpose:** Verify user demographic analytics
- **Assertion:** Status code = 200, all user metrics present

---

### 4. Admin Waste Analytics with Date Range (3 tests)

#### TC-ANALYTICS-010: Get Waste Analytics - No Date Filter
- **Endpoint:** `GET /api/admin/analytics/waste`
- **Query Parameters:** (empty)
- **Expected Status:** 200
- **Expected Behavior:** Uses default range (last 1 month)
- **Response Contains:**
  - wasteByCategory (array with categoryName, quantity, unit)
  - monthlyDistribution (array with month, total)
- **Purpose:** Verify waste statistics
- **Assertion:** Status code = 200

#### TC-ANALYTICS-011: Get Waste Analytics - With Date Range
- **Endpoint:** `GET /api/admin/analytics/waste`
- **Query Parameters:** `?startDate=2026-01-01&endDate=2026-06-30`
- **Expected Status:** 200
- **Purpose:** Verify waste data within date range
- **Assertion:** Status code = 200, data filtered correctly

#### TC-ANALYTICS-012: Get Waste Analytics - Future Dates
- **Endpoint:** `GET /api/admin/analytics/waste`
- **Query Parameters:** `?startDate=2027-01-01&endDate=2027-12-31`
- **Expected Status:** 200
- **Expected Behavior:** Returns empty result (no future data)
- **Purpose:** Verify handling of future date ranges
- **Assertion:** Status code = 200, empty or zero results

---

### 5. Admin Summary Analytics (1 test)

#### TC-ANALYTICS-013: Get Analytics Summary
- **Endpoint:** `GET /api/admin/analytics/summary`
- **Query Parameters:** `?startDate=2026-01-01&endDate=2026-12-31`
- **Expected Status:** 200
- **Response Contains:**
  - overview (object)
  - reports (ReportAnalyticsDto)
  - users (UserAnalyticsDto)
  - waste (WasteAnalyticsDto)
- **Purpose:** Verify comprehensive analytics summary
- **Assertion:** Status code = 200, all summary sections present

---

### 6. Enterprise Analytics with Date Range (3 tests)

#### TC-ANALYTICS-014: Get Enterprise Report Analytics
- **Endpoint:** `GET /api/enterprise/analytics/reports`
- **Authentication:** Enterprise Token
- **Query Parameters:** `?startDate=2026-01-01&endDate=2026-12-31`
- **Expected Status:** 200
- **Expected Behavior:** Returns only data for that enterprise's scope
- **Purpose:** Verify enterprise-scoped analytics
- **Assertion:** Status code = 200, data scoped to enterprise

#### TC-ANALYTICS-015: Enterprise Analytics - Invalid Date Range
- **Endpoint:** `GET /api/enterprise/analytics/reports`
- **Query Parameters:** `?startDate=2026-12-31&endDate=2026-01-01`
- **Expected Status:** 400 Bad Request
- **Purpose:** Verify date validation for enterprise endpoint
- **Assertion:** Status code = 400

#### TC-ANALYTICS-016: Enterprise Analytics - No Auth
- **Endpoint:** `GET /api/enterprise/analytics/reports`
- **Authentication:** None
- **Expected Status:** 401 Unauthorized
- **Purpose:** Verify enterprise endpoint requires authentication
- **Assertion:** Status code = 401

---

### 7. Public Analytics - No Auth Required (4 tests)

#### TC-ANALYTICS-017: Get Public Report Analytics - No Auth
- **Endpoint:** `GET /api/public/analytics/reports`
- **Authentication:** None
- **Query Parameters:** (empty)
- **Expected Status:** 200
- **Expected Behavior:** Returns last 3 months data (default)
- **Response Contains:** Public waste statistics
- **Purpose:** Verify public endpoint accessibility
- **Assertion:** Status code = 200

#### TC-ANALYTICS-018: Public Analytics - With Date Range
- **Endpoint:** `GET /api/public/analytics/reports`
- **Query Parameters:** `?startDate=2026-01-01&endDate=2026-06-30`
- **Expected Status:** 200
- **Purpose:** Verify public endpoint accepts date filtering
- **Assertion:** Status code = 200, data within date range

#### TC-ANALYTICS-019: Public Analytics - Invalid Date Range
- **Endpoint:** `GET /api/public/analytics/reports`
- **Query Parameters:** `?startDate=2026-12-31&endDate=2026-01-01`
- **Expected Status:** 400 Bad Request
- **Purpose:** Verify public endpoint validates dates
- **Assertion:** Status code = 400

#### TC-ANALYTICS-020: Public Analytics - Very Old Dates
- **Endpoint:** `GET /api/public/analytics/reports`
- **Query Parameters:** `?startDate=2020-01-01&endDate=2020-12-31`
- **Expected Status:** 200
- **Expected Behavior:** Returns empty (no data from 2020)
- **Purpose:** Verify handling of historical dates
- **Assertion:** Status code = 200, empty results

---

### 8. Edge Cases & Performance (8 tests)

#### TC-ANALYTICS-021: Same Day Date Range
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2026-06-15&endDate=2026-06-15`
- **Expected Status:** 200
- **Purpose:** Verify single-day analytics query
- **Assertion:** Status code = 200

#### TC-ANALYTICS-022: Timezone Handling
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2026-01-01T00:00:00Z&endDate=2026-01-31T23:59:59Z`
- **Expected Status:** 200
- **Purpose:** Verify ISO 8601 UTC timestamps
- **Assertion:** Status code = 200

#### TC-ANALYTICS-023: Boundary Date - Start of Year
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2026-01-01&endDate=2026-01-31`
- **Expected Status:** 200
- **Purpose:** Verify year boundary handling
- **Assertion:** Status code = 200

#### TC-ANALYTICS-024: Boundary Date - End of Year
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2026-12-01&endDate=2026-12-31`
- **Expected Status:** 200
- **Purpose:** Verify year-end handling
- **Assertion:** Status code = 200

#### TC-ANALYTICS-025: Large Date Range (Multiple Years)
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2024-01-01&endDate=2026-12-31`
- **Expected Status:** 200
- **Purpose:** Verify handling of multi-year ranges
- **Assertion:** Status code = 200, response time < 5 seconds

#### TC-ANALYTICS-026: Response Time - Large Dataset
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=2020-01-01&endDate=2026-12-31`
- **Measurement:** Response time in milliseconds
- **Expected:** < 3000ms
- **Purpose:** Verify performance with large dataset
- **Assertion:** Response time acceptable

#### TC-ANALYTICS-027: Null Date Parameters
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=null&endDate=null`
- **Expected Status:** 200 (with defaults) or 400
- **Purpose:** Verify null parameter handling
- **Assertion:** Defined behavior

#### TC-ANALYTICS-028: Empty String Date Parameters
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `?startDate=&endDate=`
- **Expected Status:** 200 (with defaults) or 400
- **Purpose:** Verify empty string handling
- **Assertion:** Defined behavior

---

## Test Data Requirements

### Pre-Test Setup
- ✅ Admin user with valid token
- ✅ Enterprise user with valid token
- ✅ Sample waste reports (various dates)
- ✅ Sample users (various roles)
- ✅ Sample enterprises (verified/unverified)

### Database State
- Reports from: 2020-01-01 onwards
- Mix of waste categories
- Various report statuses (Pending, Approved, Rejected)

---

## Test Execution Strategy

### Execution Order
1. Public endpoints first (no auth needed)
2. Enterprise endpoints (enterprise auth)
3. Admin endpoints (admin auth)
4. Error/edge cases last

### Assertions Priority
| Priority | Check |
|---|---|
| P1 | Status code correct |
| P2 | Date range filtering works |
| P3 | Response structure valid |
| P4 | Response time acceptable |
| P5 | Error messages clear |

---

## Expected Test Results

**Target:** 28/28 PASS (100%)

| Category | Count | Status |
|---|---|---|
| Admin Overview | 2 | ✓ |
| Admin Report Analytics | 6 | ✓ |
| Admin User Analytics | 1 | ✓ |
| Admin Waste Analytics | 3 | ✓ |
| Admin Summary | 1 | ✓ |
| Enterprise Analytics | 3 | ✓ |
| Public Analytics | 4 | ✓ |
| Edge Cases | 8 | ✓ |
| **Total** | **28** | **100%** |

---

## Notes for QA Team

1. **Date Range is Critical:** Most tests focus on `startDate` and `endDate` parameters
2. **Authentication Levels:** Test each endpoint with correct and wrong auth levels
3. **Performance:** Monitor response times for large date ranges
4. **Timezone:** All dates should be ISO 8601 UTC format
5. **Error Messages:** Validate error responses are meaningful

---

## Integration with CI/CD Pipeline

- Tests executed in GitHub Actions on every PR
- Newman automation with Postman collection
- Results posted to Jira for tracking
- Only merge when all tests PASS
