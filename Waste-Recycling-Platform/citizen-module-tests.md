# Citizen Module Testing - KIEM-10
## WRP-BE-TESTS-010: Comprehensive Test Case Documentation

**Test Objective:** Verify all Citizen module functionality including profile management, rewards, leaderboards, and auth-required access control.

**Scope:** 
- GET/PUT profile endpoints
- Rewards endpoints (list, detail)
- Leaderboards (public, personal)
- Authentication and authorization validation

---

## Test Case Summary
**Total Test Cases:** 26 (across 3 tiers)

| Category | Count | Details |
|----------|-------|---------|
| Profile Management | 6 | GET profile, PUT profile, update validation |
| Rewards | 5 | GET rewards list, filter, detail retrieval |
| Leaderboards | 4 | Top contributors, rankings, personal stats |
| Auth & Access Control | 8 | Token validation, role-based access, missing auth |
| Edge Cases | 3 | Empty data, null values, malformed requests |

---

## Test Cases by Category

### 1. Citizen Profile Tests (6 cases)

#### TC-101: Get Citizen Profile - Success
- **Endpoint:** GET /api/citizen/profile
- **Auth:** Required (Citizen token)
- **Expected Result:** 200 OK
- **Response Fields:**
  - citizenId (UUID)
  - fullName (string)
  - email (string)
  - phone (string)
  - address (string)
  - avatar (URL)
  - verificationStatus (verified/pending/unverified)
  - totalPoints (integer)
  - joinDate (datetime)
  - preferredLanguage (string)

#### TC-102: Get Profile - Missing Authentication
- **Endpoint:** GET /api/citizen/profile
- **Auth:** None
- **Expected:** 401 Unauthorized
- **Response:** {"error": "Missing or invalid authentication token"}

#### TC-103: Get Profile - Invalid/Expired Token
- **Endpoint:** GET /api/citizen/profile
- **Auth:** Expired JWT
- **Expected:** 401 Unauthorized
- **Response:** {"error": "Token expired"}

#### TC-104: Update Citizen Profile - Valid Data
- **Endpoint:** PUT /api/citizen/profile
- **Auth:** Required (Citizen token)
- **Request Body:**
  ```json
  {
    "fullName": "Updated Name",
    "phone": "+84912345678",
    "address": "123 Main St",
    "preferredLanguage": "vi"
  }
  ```
- **Expected:** 200 OK
- **Response:** Updated profile object

#### TC-105: Update Profile - Invalid Email Format
- **Endpoint:** PUT /api/citizen/profile
- **Auth:** Required
- **Request:** 
  ```json
  {
    "email": "invalid-email"
  }
  ```
- **Expected:** 400 Bad Request
- **Error:** "Invalid email format"

#### TC-106: Update Profile - Enterprise User Access Denied
- **Endpoint:** PUT /api/citizen/profile
- **Auth:** Enterprise JWT token
- **Expected:** 403 Forbidden
- **Response:** {"error": "Enterprise users cannot update citizen profile"}

---

### 2. Citizen Rewards Tests (5 cases)

#### TC-201: Get Rewards List - No Filter
- **Endpoint:** GET /api/citizen/rewards
- **Auth:** Required (Citizen)
- **Query Params:** None
- **Expected:** 200 OK
- **Response:**
  ```json
  {
    "totalRewards": 15,
    "rewards": [
      {
        "rewardId": "uuid",
        "name": "Green Hero Badge",
        "description": "50+ reports submitted",
        "points": 100,
        "category": "reporting",
        "unlockedDate": "2026-01-15T10:30:00Z",
        "active": true
      }
    ]
  }
  ```

#### TC-202: Get Rewards - Filter by Category
- **Endpoint:** GET /api/citizen/rewards
- **Query Params:** ?category=reporting
- **Auth:** Required
- **Expected:** 200 OK
- **Response:** Only rewards matching category

#### TC-203: Get Reward Detail
- **Endpoint:** GET /api/citizen/rewards/{rewardId}
- **Auth:** Required
- **Path Param:** Valid UUID
- **Expected:** 200 OK
- **Response:** Single reward object with detailed info

#### TC-204: Get Reward - Invalid Reward ID
- **Endpoint:** GET /api/citizen/rewards/invalid-uuid
- **Auth:** Required
- **Expected:** 404 Not Found
- **Response:** {"error": "Reward not found"}

#### TC-205: Get Rewards - Unauthorized Access
- **Endpoint:** GET /api/citizen/rewards
- **Auth:** Invalid token
- **Expected:** 401 Unauthorized

---

### 3. Citizen Leaderboard Tests (4 cases)

#### TC-301: Get Top Contributors Leaderboard
- **Endpoint:** GET /api/citizen/leaderboards/top-contributors
- **Auth:** Optional (public data with limited visibility if not auth)
- **Query Params:** ?limit=10&period=month
- **Expected:** 200 OK
- **Response:**
  ```json
  {
    "period": "month",
    "generatedAt": "2026-05-26T00:00:00Z",
    "topContributors": [
      {
        "rank": 1,
        "citizenId": "uuid",
        "citizenName": "Nguyễn A",
        "reportsSubmitted": 45,
        "points": 4500,
        "badgeCount": 8
      }
    ]
  }
  ```

#### TC-302: Get Personal Leaderboard Stats
- **Endpoint:** GET /api/citizen/leaderboards/personal
- **Auth:** Required (Citizen)
- **Expected:** 200 OK
- **Response:**
  ```json
  {
    "myRank": 25,
    "myPoints": 1850,
    "myReportsCount": 18,
    "percentile": 75,
    "topInRegion": 5,
    "nextMilestonePoints": 2000,
    "progressPercentage": 92.5
  }
  ```

#### TC-303: Get Leaderboard - Time Period Validation
- **Endpoint:** GET /api/citizen/leaderboards/top-contributors
- **Query Params:** ?period=invalid_period
- **Expected:** 400 Bad Request
- **Response:** {"error": "Invalid period. Allowed: day, week, month, year, all"}

#### TC-304: Get Leaderboard - Large Limit
- **Endpoint:** GET /api/citizen/leaderboards/top-contributors
- **Query Params:** ?limit=1000
- **Expected:** 200 OK
- **Response:** Limited to max 100 records regardless of requested limit

---

### 4. Authentication & Authorization Tests (8 cases)

#### TC-401: Access Citizen Endpoint - No Token
- **Endpoint:** GET /api/citizen/profile
- **Auth:** None
- **Expected:** 401 Unauthorized

#### TC-402: Access Citizen Endpoint - Admin Token
- **Endpoint:** GET /api/citizen/profile
- **Auth:** Admin JWT
- **Expected:** 403 Forbidden (Admin cannot use citizen endpoints)

#### TC-403: Access Citizen Endpoint - Enterprise Token
- **Endpoint:** GET /api/citizen/profile
- **Auth:** Enterprise JWT
- **Expected:** 403 Forbidden

#### TC-404: Access Citizen Endpoint - Collector Token
- **Endpoint:** GET /api/citizen/profile
- **Auth:** Collector JWT
- **Expected:** 403 Forbidden

#### TC-405: Token Expiration Check
- **Endpoint:** GET /api/citizen/profile
- **Auth:** Expired Citizen token (created 61 minutes ago)
- **Expected:** 401 Unauthorized
- **Response:** {"error": "Token has expired"}

#### TC-406: Invalid Token Format
- **Endpoint:** GET /api/citizen/profile
- **Auth Header:** "Bearer invalid-jwt-format"
- **Expected:** 401 Unauthorized

#### TC-407: Token Revocation
- **Scenario:** Token was valid, then revoked
- **Endpoint:** GET /api/citizen/profile
- **Auth:** Revoked token
- **Expected:** 401 Unauthorized

#### TC-408: Cross-Citizen Access Prevention
- **Scenario:** Citizen A token accessing Citizen B profile
- **Endpoint:** GET /api/citizen/profile
- **Auth:** Citizen A token, but system somehow attempts Citizen B access
- **Expected:** 403 Forbidden or citizen only sees own data

---

### 5. Edge Cases & Error Handling (3 cases)

#### TC-501: Empty Rewards List
- **Endpoint:** GET /api/citizen/rewards
- **Auth:** Required
- **Scenario:** User has no rewards yet
- **Expected:** 200 OK
- **Response:** {"totalRewards": 0, "rewards": []}

#### TC-502: Null/Missing Request Fields
- **Endpoint:** PUT /api/citizen/profile
- **Request:** Missing required fields
- **Expected:** 400 Bad Request
- **Response:** {"error": "fullName is required"}

#### TC-503: Very Long Input Strings
- **Endpoint:** PUT /api/citizen/profile
- **Request:** fullName = 1000+ characters
- **Expected:** 400 Bad Request
- **Response:** {"error": "Field exceeds maximum length (500 chars)"}

---

## Test Execution Strategy

### Unit Test Layer (CitizenModuleTests.cs)
- Test business logic without HTTP
- Mock dependencies (database, auth)
- Validate data transformations
- Test validation rules

### Integration Test Layer (CitizenApiIntegrationTests.cs)
- Hit real HTTP endpoints
- Validate response structure
- Test role-based access
- Verify error responses
- Performance measurements

---

## Validation Criteria

### Response Status Codes
- **200:** Successful GET/PUT
- **400:** Bad Request (validation error)
- **401:** Unauthorized (missing/invalid token)
- **403:** Forbidden (insufficient permissions)
- **404:** Not Found (resource doesn't exist)
- **500:** Internal Server Error

### Common Error Validations
- Error messages are clear and actionable
- Error codes are consistent
- Error responses include timestamp
- No sensitive data in error messages

### Performance Requirements
- GET endpoints: < 1 second
- PUT endpoints: < 2 seconds
- Leaderboard queries (100 records): < 3 seconds

---

## Test Environment Setup

### Required Test Data
- Test Citizen accounts with various rewards
- Leaderboard data (at least 100 citizens)
- Revoked tokens for revocation tests
- Expired tokens (manually generated with past expiry)

### Database State
- Clean leaderboard cache before tests
- Ensure rewards are properly indexed
- Verify token blacklist is working

---

## Notes for Test Implementation

1. **Token Management:** Use JWT library to create valid tokens with specific roles and expiry times
2. **Leaderboard Caching:** May be cached; clear cache before tests to ensure fresh data
3. **Profile Privacy:** Ensure citizens can only see their own detailed profile
4. **Rewards System:** Verify reward unlock conditions are met before testing access
5. **Async Operations:** All HTTP calls should use async/await patterns
