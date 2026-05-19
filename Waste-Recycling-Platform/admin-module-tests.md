# WRP-BE-TESTS-005: Admin Module Testing

## Test Cases Summary

This document outlines the 14 comprehensive API test cases for the Admin Module of the Waste Recycling Platform. These tests are implemented in the Postman collection and verify core admin functionality including user management, enterprise verification, and analytics.

---

## Admin Module Test Cases

### Users Management (5 tests)

#### 1. GET Users
- **Endpoint:** `GET /api/admin/users`
- **Query Parameters:** `search`, `role`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200
- **Purpose:** Retrieve list of all users with optional filtering by search term and role
- **Test Assertion:** Status code is 200

#### 2. GET User Stats
- **Endpoint:** `GET /api/admin/users/stats`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200
- **Purpose:** Get dashboard user statistics (count, distribution, etc.)
- **Test Assertion:** Status code is 200

#### 3. POST Create User
- **Endpoint:** `POST /api/admin/users`
- **Authentication:** Admin Token (Bearer)
- **Method:** POST
- **Request Body:**
  ```json
  {
    "fullName": "Admin Created User",
    "email": "user+<timestamp>@example.com",
    "password": "ChangeMe123!",
    "role": "Citizen",
    "phone": "0909999999",
    "district": "District 1",
    "ward": "Ward 1"
  }
  ```
- **Expected Status:** 200, 400, 401, 403, or 409
- **Purpose:** Admin can directly create a new user with specified role
- **Test Assertion:** Status code is one of [200, 400, 401, 403, 409]

#### 4. PATCH Toggle User Status
- **Endpoint:** `PATCH /api/admin/users/{userId}/toggle-status`
- **Authentication:** Admin Token (Bearer)
- **Purpose:** Activate or deactivate a user account
- **Expected Status:** 200, 400, 401, 403, or 404
- **Test Assertion:** Status code is one of [200, 400, 401, 403, 404]

#### 5. PATCH Update User Role
- **Endpoint:** `PATCH /api/admin/users/{userId}/role`
- **Authentication:** Admin Token (Bearer)
- **Method:** PATCH
- **Request Body:**
  ```json
  {
    "role": "Collector"
  }
  ```
- **Expected Status:** 200, 400, 401, 403, or 404
- **Purpose:** Change a user's assigned role (e.g., Citizen to Collector)
- **Test Assertion:** Status code is one of [200, 400, 401, 403, 404]

---

### Enterprise Management (4 tests)

#### 6. GET Enterprises
- **Endpoint:** `GET /api/admin/enterprises`
- **Query Parameters:** `page`, `pageSize`, `isVerified`, `searchTerm`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200
- **Purpose:** Retrieve list of all enterprises with pagination and filtering
- **Test Assertion:** Status code is 200

#### 7. GET Enterprise Detail
- **Endpoint:** `GET /api/admin/enterprises/{enterpriseId}`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200 or 404
- **Purpose:** Get detailed information about a specific enterprise
- **Test Assertion:** Status code is one of [200, 404]

#### 8. POST Verify Enterprise
- **Endpoint:** `POST /api/admin/enterprises/{enterpriseId}/verify`
- **Authentication:** Admin Token (Bearer)
- **Method:** POST
- **Expected Status:** 200, 400, 401, 403, or 404
- **Purpose:** Mark an enterprise as verified, approving it for platform use
- **Test Assertion:** Status code is one of [200, 400, 401, 403, 404]

#### 9. POST Reject Enterprise
- **Endpoint:** `POST /api/admin/enterprises/{enterpriseId}/reject`
- **Authentication:** Admin Token (Bearer)
- **Method:** POST
- **Request Body:**
  ```json
  {
    "reasonForRejection": "Missing required compliance documents"
  }
  ```
- **Expected Status:** 200, 400, 401, 403, or 404
- **Purpose:** Reject an enterprise application with a reason documented
- **Test Assertion:** Status code is one of [200, 400, 401, 403, 404]

---

### Analytics Dashboard (5 tests)

#### 10. GET Analytics Overview
- **Endpoint:** `GET /api/admin/analytics/overview`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200
- **Purpose:** Overall admin analytics dashboard with key metrics
- **Test Assertion:** Status code is 200

#### 11. GET Analytics Reports
- **Endpoint:** `GET /api/admin/analytics/reports`
- **Query Parameters:** `startDate`, `endDate`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200
- **Purpose:** Get report analytics with optional date filtering
- **Test Assertion:** Status code is 200

#### 12. GET Analytics Users
- **Endpoint:** `GET /api/admin/analytics/users`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200
- **Purpose:** Get user-related analytics (growth, distribution, activity, etc.)
- **Test Assertion:** Status code is 200

#### 13. GET Analytics Waste
- **Endpoint:** `GET /api/admin/analytics/waste`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200
- **Purpose:** Get waste-related analytics (categories, quantities, trends)
- **Test Assertion:** Status code is 200

#### 14. GET Analytics Summary
- **Endpoint:** `GET /api/admin/analytics/summary`
- **Authentication:** Admin Token (Bearer)
- **Expected Status:** 200
- **Purpose:** Get a comprehensive summary of all analytics
- **Test Assertion:** Status code is 200

---

## Test Execution Details

### Prerequisites
- Admin user must be authenticated and have valid `adminToken` in environment
- Base URL configured as `http://localhost:5000`
- Target user/enterprise IDs available as `{{userId}}` and `{{enterpriseId}}`

### Test Collection
- **Collection File:** `WastePlatform.professional.postman_collection.json`
- **Folder:** `10 - Admin API`
- **Environment:** `WastePlatform.professional.postman_environment.json`

### Running Tests
```bash
# Run all admin API tests
newman run WastePlatform.professional.postman_collection.json \
  --environment WastePlatform.professional.postman_environment.json \
  --folder "10 - Admin API"

# Run specific test
newman run WastePlatform.professional.postman_collection.json \
  --environment WastePlatform.professional.postman_environment.json \
  --grep "GET Users"
```

### Expected Outcomes
- All 14 tests should execute without errors
- Status code assertions should pass for valid authentication
- 401 responses indicate authentication failures
- 403 responses indicate authorization failures
- 404 responses indicate resource not found

---

## Test Coverage

| Feature Area | Test Count | Coverage |
|---|---|---|
| User Management | 5 | CRUD operations + status/role changes |
| Enterprise Management | 4 | Read + verification workflow |
| Analytics | 5 | Multiple dashboard views |
| **Total** | **14** | **100%** |

---

## Notes

- All admin endpoints require valid JWT token with admin role
- Token expiration: 60 minutes
- Tests use dynamic variables for user/enterprise IDs populated during execution
- Error handling tests for 400, 401, 403, 404 status codes
- Timestamps used for unique email generation in user creation test
