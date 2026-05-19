# 🔐 KIEM-4: WRP-BE-TESTS-001 - Auth Module Testing

**Status:** 🟦 IN PROGRESS  
**Branch:** `KIEM-4-WRP-BE-TESTS-001-Auth-Module`  
**Jira Link:** KIEM-4  
**Module:** Authentication (Register / Login / Profile)

---

## 📋 Test Case Summary

| TC ID | Test Case Name | Type | Status | Priority |
|:---:|:---|:---:|:---:|:---:|
| **TC-AUTH-001** | Register valid user | ✅ Positive | ⬜ TBD | 🔴 High |
| **TC-AUTH-002** | Register missing field | ❌ Negative | ⬜ TBD | 🔴 High |
| **TC-AUTH-003** | Register duplicate email | ❌ Negative | ⬜ TBD | 🔴 High |
| **TC-AUTH-004** | Login valid credentials | ✅ Positive | ⬜ TBD | 🔴 High |
| **TC-AUTH-005** | Login wrong password | ❌ Negative | ⬜ TBD | 🔴 High |
| **TC-AUTH-006** | Login non-existing user | ❌ Negative | ⬜ TBD | 🔴 High |
| **TC-AUTH-007** | Get profile (`/me`) valid token | ✅ Positive | ⬜ TBD | 🟡 Medium |
| **TC-AUTH-008** | Get profile without token | ❌ Negative | ⬜ TBD | 🟡 Medium |

---

## 🎯 Test Objectives

- ✅ Validate user registration with valid inputs
- ✅ Validate error handling for invalid registration inputs
- ✅ Validate authentication with valid credentials
- ✅ Validate authentication rejection with invalid credentials
- ✅ Validate JWT token generation and validation
- ✅ Validate user profile retrieval with authentication
- ✅ Validate authorization enforcement on protected endpoints

---

## 📝 Detailed Test Cases

### TC-AUTH-001: Register valid user ✅ (Positive)

**Objective:** Verify that a user can register successfully with valid data

**Preconditions:**
- Test database is clean (or user email doesn't exist)
- API server is running
- Postman collection is imported

**Steps:**
```
1. Send POST request to /api/auth/register
   {
     "email": "testuser@example.com",
     "password": "ValidPassword123!",
     "firstName": "Test",
     "lastName": "User"
   }
```

**Expected Result:**
- ✅ Response Status: `201 Created`
- ✅ Response Body contains:
  ```json
  {
    "success": true,
    "message": "Registration successful",
    "data": {
      "userId": "<uuid>",
      "email": "testuser@example.com",
      "firstName": "Test",
      "lastName": "User"
    }
  }
  ```
- ✅ User record created in database
- ✅ Response time < 500ms

**Evidence Location:** `postman-results/results.json` → `TC-AUTH-001`

---

### TC-AUTH-002: Register missing field ❌ (Negative)

**Objective:** Verify that registration fails when required fields are missing

**Preconditions:**
- API server is running
- Postman collection is imported

**Steps:**
```
1. Send POST request to /api/auth/register
   {
     "email": "testuser2@example.com",
     "password": "ValidPassword123!"
     // Missing: firstName, lastName
   }
```

**Expected Result:**
- ✅ Response Status: `400 Bad Request`
- ✅ Response Body contains error message:
  ```json
  {
    "success": false,
    "errors": [
      "firstName is required",
      "lastName is required"
    ]
  }
  ```
- ✅ No user record created in database

**Evidence Location:** `postman-results/results.json` → `TC-AUTH-002`

---

### TC-AUTH-003: Register duplicate email ❌ (Negative)

**Objective:** Verify that registration fails for duplicate email addresses

**Preconditions:**
- User with email `duplicate@example.com` already exists
- API server is running

**Steps:**
```
1. Send POST request to /api/auth/register
   {
     "email": "duplicate@example.com",
     "password": "ValidPassword123!",
     "firstName": "Test2",
     "lastName": "User2"
   }
```

**Expected Result:**
- ✅ Response Status: `409 Conflict`
- ✅ Response Body contains:
  ```json
  {
    "success": false,
    "message": "Email already registered"
  }
  ```

**Evidence Location:** `postman-results/results.json` → `TC-AUTH-003`

---

### TC-AUTH-004: Login valid credentials ✅ (Positive)

**Objective:** Verify that user can login with correct email and password

**Preconditions:**
- User exists: `testuser@example.com` / `ValidPassword123!`
- API server is running

**Steps:**
```
1. Send POST request to /api/auth/login
   {
     "email": "testuser@example.com",
     "password": "ValidPassword123!"
   }
```

**Expected Result:**
- ✅ Response Status: `200 OK`
- ✅ Response Body contains:
  ```json
  {
    "success": true,
    "data": {
      "accessToken": "eyJhbGc...<JWT_TOKEN>",
      "refreshToken": "ref_...",
      "expiresIn": 3600,
      "user": {
        "userId": "<uuid>",
        "email": "testuser@example.com",
        "firstName": "Test",
        "lastName": "User"
      }
    }
  }
  ```
- ✅ JWT token is valid and contains user claims
- ✅ Response time < 500ms

**Evidence Location:** `postman-results/results.json` → `TC-AUTH-004`

---

### TC-AUTH-005: Login wrong password ❌ (Negative)

**Objective:** Verify that login fails with incorrect password

**Preconditions:**
- User exists: `testuser@example.com` / `ValidPassword123!`
- API server is running

**Steps:**
```
1. Send POST request to /api/auth/login
   {
     "email": "testuser@example.com",
     "password": "WrongPassword123!"
   }
```

**Expected Result:**
- ✅ Response Status: `401 Unauthorized`
- ✅ Response Body contains:
  ```json
  {
    "success": false,
    "message": "Invalid email or password"
  }
  ```
- ✅ No token generated

**Evidence Location:** `postman-results/results.json` → `TC-AUTH-005`

---

### TC-AUTH-006: Login non-existing user ❌ (Negative)

**Objective:** Verify that login fails for non-existent user

**Preconditions:**
- API server is running
- No user with email `nonexistent@example.com` exists

**Steps:**
```
1. Send POST request to /api/auth/login
   {
     "email": "nonexistent@example.com",
     "password": "AnyPassword123!"
   }
```

**Expected Result:**
- ✅ Response Status: `401 Unauthorized`
- ✅ Response Body contains:
  ```json
  {
    "success": false,
    "message": "Invalid email or password"
  }
  ```

**Evidence Location:** `postman-results/results.json` → `TC-AUTH-006`

---

### TC-AUTH-007: Get profile valid token ✅ (Positive)

**Objective:** Verify that user can retrieve their profile with valid JWT token

**Preconditions:**
- User is authenticated with valid JWT token
- Token is stored in Postman environment variable `{{auth_token}}`
- API server is running

**Steps:**
```
1. Send GET request to /api/auth/me
   Headers:
   {
     "Authorization": "Bearer {{auth_token}}"
   }
```

**Expected Result:**
- ✅ Response Status: `200 OK`
- ✅ Response Body contains:
  ```json
  {
    "success": true,
    "data": {
      "userId": "<uuid>",
      "email": "testuser@example.com",
      "firstName": "Test",
      "lastName": "User",
      "role": "Citizen"
    }
  }
  ```
- ✅ Response time < 300ms

**Evidence Location:** `postman-results/results.json` → `TC-AUTH-007`

---

### TC-AUTH-008: Get profile without token ❌ (Negative)

**Objective:** Verify that profile endpoint requires authentication

**Preconditions:**
- API server is running
- User has no valid authentication token

**Steps:**
```
1. Send GET request to /api/auth/me
   Headers:
   {
     // No Authorization header
   }
```

**Expected Result:**
- ✅ Response Status: `401 Unauthorized`
- ✅ Response Body contains:
  ```json
  {
    "success": false,
    "message": "Authorization token is missing or invalid"
  }
  ```

**Evidence Location:** `postman-results/results.json` → `TC-AUTH-008`

---

## 🔄 Test Execution Flow

```
1. Setup Test Environment
   ├─ Prepare test database
   ├─ Clear existing test users
   └─ Start API server

2. Execute Positive Tests
   ├─ TC-AUTH-001: Register valid user
   ├─ TC-AUTH-004: Login valid credentials
   └─ TC-AUTH-007: Get profile valid token

3. Execute Negative Tests
   ├─ TC-AUTH-002: Register missing field
   ├─ TC-AUTH-003: Register duplicate email
   ├─ TC-AUTH-005: Login wrong password
   ├─ TC-AUTH-006: Login non-existing user
   └─ TC-AUTH-008: Get profile without token

4. Generate Reports
   ├─ Postman HTML report
   ├─ Test result summary
   └─ Coverage metrics
```

---

## 📊 Test Results

**Last Run:** TBD  
**Pass Rate:** TBD  
**Coverage:** TBD  
**Duration:** TBD

| Test Case | Status | Duration | Notes |
|:---|:---:|:---:|:---|
| TC-AUTH-001 | ⬜ TBD | - | - |
| TC-AUTH-002 | ⬜ TBD | - | - |
| TC-AUTH-003 | ⬜ TBD | - | - |
| TC-AUTH-004 | ⬜ TBD | - | - |
| TC-AUTH-005 | ⬜ TBD | - | - |
| TC-AUTH-006 | ⬜ TBD | - | - |
| TC-AUTH-007 | ⬜ TBD | - | - |
| TC-AUTH-008 | ⬜ TBD | - | - |

---

## 🛠️ Postman Setup

**Collection:** `/postman/WastePlatform.professional.postman_collection.json`  
**Environment:** `/postman/WastePlatform.professional.postman_environment.json`

**Environment Variables:**
```json
{
  "base_url": "http://localhost:5000",
  "auth_token": "{{token_from_login}}",
  "refresh_token": "{{refresh_token_from_login}}",
  "test_email": "testuser@example.com",
  "test_password": "ValidPassword123!"
}
```

---

## ✅ Checklist for Completion

- [ ] All 8 test cases written and documented
- [ ] Postman collection updated with all endpoints
- [ ] Environment variables configured
- [ ] All tests passing locally
- [ ] GitHub Actions CI/CD passing
- [ ] Code coverage > 80%
- [ ] Test results exported and archived
- [ ] PR created and merged
- [ ] Jira issue marked Done

---

## 📎 References

- **API Documentation:** [Backend README](../../Waste-Recycling-Platform/backend/README.md)
- **Test Strategy:** [Testing Playbook](../../Waste-Recycling-Platform/docs/testing-playbook.md)
- **CI/CD Guide:** [CI/CD Workflow](../../docs/CI_CD_WORKFLOW.md)
- **Jira Ticket:** KIEM-4
- **PR Link:** TBD

---

**Last Updated:** 2026-05-18  
**Author:** KIEM-4 Test Team  
**Status:** 🟦 In Progress
