# 🟦 EPIC: WRP-BE-TESTS — Backend API Testing & QA
> **Mô tả:** Hệ thống kiểm thử Backend API & Đảm bảo chất lượng (Sử dụng Postman + Jira + GitHub Evidence).

---

## 🟩 TASK 1 — AUTH MODULE
**Mã công việc:** `WRP-BE-TESTS-001` — Auth Module Testing (Register / Login / Profile)  
* **Postman Setup:** Auth collection setup + JWT environment variables.

| Mã Kịch Bản (TC ID) | Tên Kịch Bản Kiểm Thử (Test Case Name) | Loại (Type) | Trạng thái |
| :--- | :--- | :--- | :--- |
| **TC-AUTH-001** | Register valid user | Tích cực (Positive) | ⬜ TBD |
| **TC-AUTH-002** | Register missing field | Tiêu cực (Negative) | ⬜ TBD |
| **TC-AUTH-003** | Register duplicate email | Tiêu cực (Negative) | ⬜ TBD |
| **TC-AUTH-004** | Login valid credentials | Tích cực (Positive) | ⬜ TBD |
| **TC-AUTH-005** | Login wrong password | Tiêu cực (Negative) | ⬜ TBD |
| **TC-AUTH-006** | Login non-existing user | Tiêu cực (Negative) | ⬜ TBD |
| **TC-AUTH-007** | Get profile (`/me`) valid token | Tích cực (Positive) | ⬜ TBD |
| **TC-AUTH-008** | Get profile without token | Tiêu cực (Negative) | ⬜ TBD |

---

## 🟩 TASK 2 — REPORTS MODULE
**Mã công việc:** `WRP-BE-TESTS-002` — Reports Module Testing (Waste Reports Lifecycle)  
* **Postman Setup:** Reports folder + file upload tests (form-data).

| Mã Kịch Bản (TC ID) | Tên Kịch Bản Kiểm Thử (Test Case Name) | Loại (Type) | Trạng thái |
| :--- | :--- | :--- | :--- |
| **TC-REP-001** | Create report valid (image + data) | Tích cực (Positive) | ⬜ TBD |
| **TC-REP-002** | Create report missing field | Tiêu cực (Negative) | ⬜ TBD |
| **TC-REP-003** | Get report by ID valid | Tích cực (Positive) | ⬜ TBD |
| **TC-REP-004** | Get report invalid ID | Tiêu cực (Negative) | ⬜ TBD |
| **TC-REP-005** | Accept report (authorized role) | Tích cực (Positive) | ⬜ TBD |
| **TC-REP-006** | Reject report with reason | Tích cực (Positive) | ⬜ TBD |
| **TC-REP-007** | Invalid state transition | Tiêu cực (Negative) | ⬜ TBD |
| **TC-REP-008** | Upload image invalid format | Tiêu cực (Negative) | ⬜ TBD |

---

## 🟩 TASK 3 — NOTIFICATIONS MODULE
**Mã công việc:** `WRP-BE-TESTS-003` — Notifications Module Testing  
* **Postman Setup:** Notifications folder + environment token reuse.

| Mã Kịch Bản (TC ID) | Tên Kịch Bản Kiểm Thử (Test Case Name) | Loại (Type) | Trạng thái |
| :--- | :--- | :--- | :--- |
| **TC-NOTIF-001** | Get notifications list (valid token) | Tích cực (Positive) | ⬜ TBD |
| **TC-NOTIF-002** | Get notifications without token | Tiêu cực (Negative) | ⬜ TBD |
| **TC-NOTIF-003** | Get unread count | Tích cực (Positive) | ⬜ TBD |
| **TC-NOTIF-004** | Mark notification as read | Tích cực (Positive) | ⬜ TBD |
| **TC-NOTIF-005** | Mark all notifications as read | Tích cực (Positive) | ⬜ TBD |
| **TC-NOTIF-006** | Invalid notification ID | Tiêu cực (Negative) | ⬜ TBD |

---

## 🟩 TASK 4 — COMPLAINTS MODULE
**Mã công việc:** `WRP-BE-TESTS-004` — Complaints Module Testing (Admin + Citizen Flow)  
* **Postman Setup:** Complaints admin/citizen separation tests.

| Mã Kịch Bản (TC ID) | Tên Kịch Bản Kiểm Thử (Test Case Name) | Loại (Type) | Trạng thái |
| :--- | :--- | :--- | :--- |
| **TC-COMP-001** | Create complaint valid | Tích cực (Positive) | ⬜ TBD |
| **TC-COMP-002** | Create complaint missing field | Tiêu cực (Negative) | ⬜ TBD |
| **TC-COMP-003** | Get complaint by ID | Tích cực (Positive) | ⬜ TBD |
| **TC-COMP-004** | Get complaint invalid ID | Tiêu cực (Negative) | ⬜ TBD |
| **TC-COMP-005** | Resolve complaint (admin only) | Tích cực (Positive) | ⬜ TBD |
| **TC-COMP-006** | Reject complaint with reason (admin only) | Tích cực (Positive) | ⬜ TBD |
| **TC-COMP-007** | Unauthorized resolve attempt | Tiêu cực (Negative) | ⬜ TBD |

---

## 🟩 TASK 5 — ADMIN MODULE
**Mã công việc:** `WRP-BE-TESTS-005` — Admin Module Testing (Users / Enterprises / Analytics)  
* **Postman Setup:** Admin folder + role-based JWT tokens.

| Mã Kịch Bản (TC ID) | Tên Kịch Bản Kiểm Thử (Test Case Name) | Loại (Type) | Trạng thái |
| :--- | :--- | :--- | :--- |
| **TC-ADMIN-001** | Get users list (admin) | Tích cực (Positive) | ⬜ TBD |
| **TC-ADMIN-002** | Get users without admin role | Tiêu cực (Negative) | ⬜ TBD |
| **TC-ADMIN-003** | Create user (admin) | Tích cực (Positive) | ⬜ TBD |
| **TC-ADMIN-004** | Toggle user status | Tích cực (Positive) | ⬜ TBD |
| **TC-ADMIN-005** | Update user role | Tích cực (Positive) | ⬜ TBD |
| **TC-ADMIN-006** | Get enterprises list | Tích cực (Positive) | ⬜ TBD |
| **TC-ADMIN-007** | Verify enterprise | Tích cực (Positive) | ⬜ TBD |
| **TC-ADMIN-008** | Reject enterprise | Tích cực (Positive) | ⬜ TBD |
| **TC-ADMIN-009** | Get analytics overview | Tích cực (Positive) | ⬜ TBD |
| **TC-ADMIN-010** | Get analytics reports by date range | Tích cực (Positive) | ⬜ TBD |

---

## 🟩 TASK 6 — ANALYTICS MODULE
**Mã công việc:** `WRP-BE-TESTS-006` — Analytics Module Testing  
* **Postman Setup:** Analytics folder + date query tests.

| Mã Kịch Bản (TC ID) | Tên Kịch Bản Kiểm Thử (Test Case Name) | Loại (Type) | Trạng thái |
| :--- | :--- | :--- | :--- |
| **TC-ANALYTICS-001** | Get admin overview analytics | Tích cực (Positive) | ⬜ TBD |
| **TC-ANALYTICS-002** | Get report analytics by date range | Tích cực (Positive) | ⬜ TBD |
| **TC-ANALYTICS-003** | Get user analytics | Tích cực (Positive) | ⬜ TBD |
| **TC-ANALYTICS-004** | Get waste analytics | Tích cực (Positive) | ⬜ TBD |
| **TC-ANALYTICS-005** | Unauthorized access | Tiêu cực (Negative) | ⬜ TBD |

---

## 🟩 TASK 7 — PUBLIC ANALYTICS MODULE
**Mã công việc:** `WRP-BE-TESTS-007` — Public Analytics Testing  
* **Postman Setup:** Public endpoints (no auth required).

| Mã Kịch Bản (TC ID) | Tên Kịch Bản Kiểm Thử (Test Case Name) | Loại (Type) | Trạng thái |
| :--- | :--- | :--- | :--- |
| **TC-PUBLIC-001** | Get public analytics reports | Tích cực (Positive) | ⬜ TBD |
| **TC-PUBLIC-002** | Invalid date range | Tiêu cực (Negative) | ⬜ TBD |

---

## 🟩 TASK 8 — CI + POSTMAN EVIDENCE
**Mã công việc:** `WRP-BE-TESTS-008` — CI/CD + Test Evidence (GitHub Actions + Postman)

### 🚀 CI/CD Integration
* **CI-001** — Setup Newman Postman runner
* **CI-002** — Export Postman collection to GitHub
* **CI-003** — Run API tests in CI pipeline

### 📊 Test Evidences
* **EVD-001** — Save Postman run results (CLI/HTML)
* **EVD-002** — Attach screenshots of test results
* **EVD-003** — Link Jira tickets with commits

### 🐙 GitHub Actions Workflow
* **GH-001** — Commit Postman collection JSON
* **GH-002** — Commit test updates after bug fixes

---

## 🟦 Additional Tasks (scanned from source)
These tasks were added after scanning backend controllers and domain entities to cover modules not listed previously.

## 🟩 TASK 9 — WASTE CATEGORY MODULE
**Mã công việc:** `WRP-BE-TESTS-009` — WasteCategory Module Testing
* Kiểm tra: GET list, GET by id, validation, edge cases (empty, paging).

## 🟩 TASK 10 — CITIZEN MODULE
**Mã công việc:** `WRP-BE-TESTS-010` — Citizen Module Testing
* Kiểm tra: profile GET/PUT, rewards endpoints, leaderboards, auth-required cases.

## 🟩 TASK 11 — COLLECTOR MODULE
**Mã công việc:** `WRP-BE-TESTS-011` — Collector Module Testing
* Kiểm tra: collector profile, availability PATCH, auth role checks.

## 🟩 TASK 12 — COLLECTOR TASK MODULE
**Mã công việc:** `WRP-BE-TESTS-012` — CollectorTask Module Testing
* Kiểm tra: list tasks, task detail, set on-the-way, complete (form+images), status transitions, image uploads.

## 🟩 TASK 13 — ENTERPRISE TASK MODULE
**Mã công việc:** `WRP-BE-TESTS-013` — Enterprise Task Module Testing
* Kiểm tra: enterprise task listing, assign collector, assign/unassign, enterprise-specific filters.

## 🟩 TASK 14 — ENTERPRISE COLLECTORS & REWARD RULES
**Mã công việc:** `WRP-BE-TESTS-014` — Enterprise Collectors & Reward Rules Testing
* Kiểm tra: CRUD collectors, update reward rules bulk, reward points accrual.

## 🟩 TASK 15 — COLLECTION TASK & IMAGES
**Mã công việc:** `WRP-BE-TESTS-015` — CollectionTask / CollectionImage Tests
* Kiểm tra: domain rules for `CollectionTask` transitions, image persistence, DB referential integrity.

## 🟩 TASK 16 — SIGNALR / REAL-TIME NOTIFICATIONS
**Mã công việc:** `WRP-BE-TESTS-016` — SignalR Real-time Tests
* Kiểm tra: hub endpoints, message send on events (complaint resolved, task assigned), fallback behavior.

## 🟩 TASK 17 — FILE UPLOADS & STORAGE
**Mã công việc:** `WRP-BE-TESTS-017` — File Uploads & Storage Tests
* Kiểm tra: form-data uploads, allowed formats/size limits, missing files handling, CI-friendly mocking for newman.

## 🟩 TASK 18 — SECURITY & ROLE-BASED ACCESS
**Mã công việc:** `WRP-BE-TESTS-018` — Security & Role-based Access Tests
* Kiểm tra: endpoints requiring Admin/Enterprise/Citizen roles reject other roles, JWT expiry, invalid token handling.

## 🟩 TASK 19 — AUDIT LOG / ERROR PATHS
**Mã công việc:** `WRP-BE-TESTS-019` — AuditLog & Error Path Tests
* Kiểm tra: error responses, 500 handling, audit entries for critical actions (verify/resolve/assign).

## 🟩 TASK 20 — SEARCH / PAGINATION / FILTERS
**Mã công việc:** `WRP-BE-TESTS-020` — Search, Pagination & Filters Tests
* Kiểm tra: query params, date-range boundaries, totalPages calculation, invalid params handling.