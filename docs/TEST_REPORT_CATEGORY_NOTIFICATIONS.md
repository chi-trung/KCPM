# Báo Cáo Kiểm Thử Module WasteCategory & Notifications

> **Người thực hiện:** Nguyễn Chí Trung  
> **KIEM Issues:** KIEM-6 (Notifications), KIEM-12 (WasteCategory)  
> **Ngày:** 2026-06-13  
> **Sprint:** Sprint 3  
> **Allure Report:** [https://chi-trung.github.io/KCPM/report-main/](https://chi-trung.github.io/KCPM/report-main/)

---

## 1. Tổng Quan

Module WasteCategory và Notifications được kiểm thử tổng cộng **26 test cases** sử dụng kỹ thuật:

| Kỹ thuật | Mô tả | Module áp dụng |
|----------|--------|----------------|
| **Equivalence Partitioning (EP)** | Phân vùng tương đương cho input hợp lệ/không hợp lệ | WasteCategory, Notifications |
| **Error Guessing** | Dự đoán lỗi thường gặp (404, unauthorized, null) | Notifications |
| **Boundary Value Analysis (BVA)** | Giá trị biên cho ID (0, max, non-existent) | WasteCategory |

---

## 2. Module WasteCategory — KIEM-12

### 2.1 Test Files

| File | Tầng | Số test | Kỹ thuật |
|------|------|---------|----------|
| `WasteCategoryControllerTests.cs` | Controller | 3 | EP |
| `WasteCategoryRepositoryTests.cs` | Infrastructure | 3 | EP, BVA |
| `GetCategoryByIdQueryHandlerTests.cs` | Application | 2 | EP |
| **Tổng** | | **8** | |

### 2.2 Chi Tiết Test Cases

#### WasteCategoryControllerTests.cs

| # | Test Case | Input | Expected | Kỹ thuật | Kết quả |
|---|-----------|-------|----------|----------|---------|
| TC-CAT-01 | GetAllCategories trả về OK | GET /api/waste-categories | 200 OK + danh sách categories | EP (valid) | ✅ Pass |
| TC-CAT-02 | GetCategoryById tìm thấy | GET /api/waste-categories/{valid-id} | 200 OK + category data | EP (valid) | ✅ Pass |
| TC-CAT-03 | GetCategoryById không tìm thấy | GET /api/waste-categories/{invalid-id} | 404 Not Found | EP (invalid) | ✅ Pass |

#### WasteCategoryRepositoryTests.cs

| # | Test Case | Input | Expected | Kỹ thuật | Kết quả |
|---|-----------|-------|----------|----------|---------|
| TC-CAT-04 | GetAllAsync trả về danh sách | DB có categories | List<WasteCategory> non-empty | EP (valid) | ✅ Pass |
| TC-CAT-05 | GetByIdAsync tìm thấy | ID tồn tại trong DB | WasteCategory object | EP (valid) | ✅ Pass |
| TC-CAT-06 | GetByIdAsync không tìm thấy | ID = non-existent GUID | null | BVA (boundary) | ✅ Pass |

#### GetCategoryByIdQueryHandlerTests.cs

| # | Test Case | Input | Expected | Kỹ thuật | Kết quả |
|---|-----------|-------|----------|----------|---------|
| TC-CAT-07 | Handler trả về DTO khi tìm thấy | Valid category ID | CategoryDto != null | EP (valid) | ✅ Pass |
| TC-CAT-08 | Handler trả về null khi không tìm thấy | Non-existent ID | null | EP (invalid) | ✅ Pass |

### 2.3 EP Partitions — WasteCategory

```
┌─────────────────────────────────────────────────┐
│              WasteCategory ID                    │
├──────────────────┬──────────────────────────────┤
│  Valid Partition  │  Invalid Partition           │
│  (ID tồn tại)    │  (ID không tồn tại/null)     │
│  → 200 OK        │  → 404 Not Found             │
└──────────────────┴──────────────────────────────┘
```

---

## 3. Module Notifications — KIEM-6

### 3.1 Test Files

| File | Tầng | Số test | Kỹ thuật |
|------|------|---------|----------|
| `NotificationServiceTests.cs` | Application | 4 | EP |
| `NotificationControllerTests.cs` | Controller | 7 | EP, Error Guessing |
| `NotificationRepositoryTests.cs` | Infrastructure | 7 | EP, Error Guessing |
| **Tổng** | | **18** | |

### 3.2 Chi Tiết Test Cases

#### NotificationServiceTests.cs

| # | Test Case | Input | Expected | Kỹ thuật | Kết quả |
|---|-----------|-------|----------|----------|---------|
| TC-NTF-01 | NotifyReportCreated — persist + push | userId, reportId | Notification created + SignalR push | EP (valid) | ✅ Pass |
| TC-NTF-02 | NotifyReportRejected — có lý do | userId, reportId, reason="Rác sai loại" | Message chứa reason | EP (valid) | ✅ Pass |
| TC-NTF-03 | NotifyReportRejected — không có lý do | userId, reportId, reason=null | Default message | Error Guessing | ✅ Pass |
| TC-NTF-04 | NotifyComplaintEscalated — admin notification | complaintId | Admin notification created, NO realtime push | EP (valid) | ✅ Pass |

#### NotificationControllerTests.cs

| # | Test Case | Input | Expected | Kỹ thuật | Kết quả |
|---|-----------|-------|----------|----------|---------|
| TC-NTF-05 | GetMyNotifications — valid user | GET /api/notifications (JWT) | 200 OK + notifications list | EP (valid) | ✅ Pass |
| TC-NTF-06 | GetMyNotifications — unauthorized | No JWT token | 401 Unauthorized | Error Guessing | ✅ Pass |
| TC-NTF-07 | MarkAsRead — valid notification | PUT /api/notifications/{valid-id}/read | 200 OK | EP (valid) | ✅ Pass |
| TC-NTF-08 | MarkAsRead — notification 404 | PUT /api/notifications/{non-existent}/read | 404 Not Found | Error Guessing | ✅ Pass |
| TC-NTF-09 | MarkAllAsRead — valid user | PUT /api/notifications/read-all | 200 OK | EP (valid) | ✅ Pass |
| TC-NTF-10 | GetUnreadCount — valid user | GET /api/notifications/unread-count | 200 OK + count | EP (valid) | ✅ Pass |
| TC-NTF-11 | DeleteNotification — valid | DELETE /api/notifications/{id} | 200 OK | EP (valid) | ✅ Pass |

#### NotificationRepositoryTests.cs

| # | Test Case | Input | Expected | Kỹ thuật | Kết quả |
|---|-----------|-------|----------|----------|---------|
| TC-NTF-12 | GetByUserIdAsync — có notifications | userId with notifications | List non-empty | EP (valid) | ✅ Pass |
| TC-NTF-13 | GetByUserIdAsync — không có | userId without notifications | Empty list | EP (invalid) | ✅ Pass |
| TC-NTF-14 | CreateAsync — persist notification | New notification | Saved to DB | EP (valid) | ✅ Pass |
| TC-NTF-15 | MarkAsReadAsync — valid | Existing notification ID | IsRead = true | EP (valid) | ✅ Pass |
| TC-NTF-16 | MarkAsReadAsync — non-existent | Non-existent ID | No error (idempotent) | Error Guessing | ✅ Pass |
| TC-NTF-17 | MarkAllAsReadAsync — batch update | userId | All user notifications read | EP (valid) | ✅ Pass |
| TC-NTF-18 | GetUnreadCountAsync — accurate count | userId with 3 unread | count = 3 | BVA | ✅ Pass |

### 3.3 EP Partitions — Notifications

```
┌─────────────────────────────────────────────────────────────┐
│              Notification Operations                         │
├──────────────────────┬──────────────────────────────────────┤
│  Valid User + Valid   │  Invalid/Missing Auth               │
│  Notification ID      │  or Non-existent ID                 │
│  → 200 OK            │  → 401/404                           │
├──────────────────────┼──────────────────────────────────────┤
│  Mark-as-read        │  Mark-as-read non-existent           │
│  → IsRead = true     │  → Idempotent (no error)            │
└──────────────────────┴──────────────────────────────────────┘
```

---

## 4. Commits Liên Quan

| # | Commit Hash | Message |
|---|-------------|---------|
| 1 | `fdf6641d` | KIEM-12: Done WasteCategory Module Testing |
| 2 | `93b83984` | KIEM-12: complete waste category test report |
| 3 | `895bdc29` | KIEM-12: add waste category update test data and report |
| 4 | `4a683d93` | KIEM-12: Additional Allure metadata |
| 5 | `d22a05f5` | KIEM-19: add SignalR execution details |
| 6 | `a9ac88b8` | KIEM-19: fix notification repository attachments |
| 7 | `29cc31e9` | KIEM-19: enrich remaining backend tests |

---

## 5. Pull Requests

| PR | Title | Status |
|----|-------|--------|
| #40 | KIEM-12: WasteCategory Tests | ✅ Merged |
| #41 | KIEM-12: Additional Allure metadata | ✅ Merged |
| #25 | KIEM-19: SignalR + Notifications | ✅ Merged |

---

## 6. Kết Quả Tổng Hợp

| Metric | Giá trị |
|--------|---------|
| **Tổng test cases** | 26 |
| **Pass** | 26 |
| **Fail** | 0 |
| **Pass Rate** | 100% |
| **Kỹ thuật áp dụng** | EP, BVA, Error Guessing |
| **CI Status** | ✅ Backend Tests (455 pass) |
| **Allure Report** | [Link](https://chi-trung.github.io/KCPM/report-main/) |

---

## 7. Truy Vết Yêu Cầu

| KIEM Issue | Module | Test File | Số Tests |
|------------|--------|-----------|----------|
| KIEM-12 | WasteCategory | 3 files | 8 |
| KIEM-6 | Notifications | 3 files | 18 |
| **Total** | | **6 files** | **26** |

---

*Báo cáo được tạo bởi Nguyễn Chí Trung — Team Leader*  
*Sprint 3 — Môn Kiểm Chứng Phần Mềm*
