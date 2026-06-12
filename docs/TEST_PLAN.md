# Kế hoạch Kiểm thử Phần mềm (Test Plan)
# Dự án: Waste Recycling Platform (WRP)
# Môn học: Kiểm thử Phần mềm — Chương 6: Quản lý Kiểm thử

**Phiên bản:** 2.0  
**Ngày lập:** 12/06/2026  
**Nhóm:** 11A6 — KiemChungPhanMem (KCPM)  
**Giảng viên hướng dẫn:** Thầy Bảo  

---

## 1. Giới thiệu (Introduction)

### 1.1 Mục tiêu Kiểm thử

Theo giáo trình Chương 1 — **Kiểm thử nhằm mục đích**:
- Tìm ra các lỗi (defects/bugs) trong hệ thống WRP trước khi phát hành
- Xác minh (Verification) phần mềm đúng đặc tả yêu cầu
- Thẩm định (Validation) phần mềm đáp ứng nhu cầu người dùng thực tế
- Đảm bảo chất lượng cho các chức năng: Auth, Reports, Complaints, Notifications, CollectionTask

### 1.2 Phạm vi Kiểm thử (Scope)

**Trong phạm vi:**
| Phạm vi | Mô tả |
|---------|-------|
| Backend API | REST API (.NET 8, xUnit tests) |
| Frontend E2E | CodeceptJS (Citizen flows) |
| Postman API | Collection tests (Auth, Security) |
| Database | InMemory tests (integration) |

**Ngoài phạm vi:**
- Performance/Load testing
- Security penetration testing
- Mobile app testing

---

## 2. Phân tích Rủi ro (Risk Analysis)

Theo giáo trình Chương 6 — **Rủi ro và mức độ ưu tiên**:

| # | Rủi ro | Khả năng | Ảnh hưởng | Mức độ | Biện pháp |
|---|--------|----------|-----------|--------|-----------|
| R01 | Validation ảnh thiếu (KIEM-26) | Cao | Cao | **Critical** | Bug fix + BVA test |
| R02 | Max 5 ảnh chưa enforce (KIEM-29) | Cao | Trung bình | **High** | Bug fix + BVA test |
| R03 | JWT token hết hạn không xử lý | Trung bình | Cao | **High** | xUnit tests KIEM-4 |
| R04 | Race condition khi nhiều user gửi report | Thấp | Cao | **Medium** | Integration test |
| R05 | SignalR disconnect khi mạng yếu | Trung bình | Trung bình | **Medium** | Mock test KIEM-19 |

---

## 3. Mức độ Kiểm thử (Test Levels)

Theo giáo trình Chương 2 — **Các mức kiểm thử**:

```
┌─────────────────────────────────┐
│     System/E2E Testing          │  ← CodeceptJS (browser)
│  (citizen_report, collector)    │
├─────────────────────────────────┤
│     Integration Testing         │  ← xUnit + InMemory EF Core
│  (Controller → Service → Repo)  │
├─────────────────────────────────┤
│     Unit Testing                │  ← xUnit (domain logic)
│  (Domain entities, Handlers)    │
└─────────────────────────────────┘
```

| Mức | Tool | Số lượng tests | Coverage target |
|-----|------|----------------|-----------------|
| Unit | xUnit (.NET) | 245 tests | ≥ 85% branch |
| Integration | xUnit + InMemory | ~60 tests | ≥ 70% |
| E2E | CodeceptJS | 8 scenarios | Critical paths |
| API | Postman | ~20 requests | Auth + Security |

---

## 4. Kỹ thuật Thiết kế Test Cases

Theo giáo trình Chương 4 — **Phân loại kỹ thuật**:

### 4.1 Black-box Techniques (Hộp đen)

| Kỹ thuật | Áp dụng cho | KIEM Issues |
|----------|-------------|-------------|
| **Equivalence Partitioning (EP)** | Email format, Role validation | KIEM-4, KIEM-5 |
| **Boundary Value Analysis (BVA)** | Latitude/Longitude (-90..90, -180..180), Images count (1..5), Content length (1..2000) | KIEM-5, KIEM-26, KIEM-29 |
| **State Transition Testing** | WasteReport lifecycle: Pending→Accepted→Assigned→Collected | KIEM-5, KIEM-8 |
| **Decision Table Testing** | Complaint creation rules (Content×Report status×User role) | KIEM-7 |
| **Error Guessing** | JWT expired, duplicate email, null inputs | KIEM-4, KIEM-20 |

### 4.2 White-box Techniques (Hộp trắng)

| Kỹ thuật | Áp dụng cho | Mục tiêu |
|----------|-------------|----------|
| **Statement Coverage** | Domain logic handlers | 100% statements |
| **Branch/Decision Coverage** | Validation conditions | 100% branches |
| **Condition Coverage** | Complex compound conditions | ≥ 90% |

### 4.3 Experience-based Techniques

| Kỹ thuật | Mô tả |
|----------|-------|
| **Error Guessing** | Tester đoán lỗi dựa trên kinh nghiệm (null, empty, overflow) |
| **Exploratory Testing** | Tự do explore UI sau mỗi sprint |

---

## 5. Test Cases tổng hợp (Summary)

| Function Sheet | Kỹ thuật | Số UTCIDs | Jira | Assignee |
|----------------|----------|-----------|------|---------|
| F01: Auth (Đăng ký/Đăng nhập) | EP + Error Guessing | 5 | KIEM-4 | Nguyễn Chí Trung |
| F02: Create Report | BVA + EP | 6 | KIEM-5 | Minh Phụng |
| F03: Accept/Reject Report | State Transition | 5 | KIEM-8 | 11A6_03_Đăng |
| F04: CollectionTask State | State Transition | 6 | KIEM-18 | Thanh Duy |
| F05: Assign Collector | EP + Error Guessing | 4 | KIEM-16 | Nguyễn Chí Trung |
| F06: Complaints | Decision Table | 5 | KIEM-7 | 11A6_03_Đăng |
| F07: Notifications + SignalR | EP | 4 | KIEM-6 | Nguyễn Chí Trung |
| F08: File Upload & JWT | BVA + EP | 5 | KIEM-20 | Minh Phụng |
| F09: Waste Category | EP | 3 | KIEM-12 | Nguyễn Hoàng Phụng |
| F10: E2E — Citizen Report Flow | E2E Scenarios | 3 | KIEM-14 | Nguyễn Chí Trung |
| **F11: BVA — Images Upload (1≤n≤5)** | **Standard BVA** | **8** | **KIEM-26/29** | **Nguyễn Hoàng Phụng** |
| **F12: Decision Table — Complaints** | **Decision Table** | **6** | **KIEM-7** | **Thanh Duy** |
| **F13: State Transition — Report** | **State Transition** | **8** | **KIEM-5** | **Minh Phụng** |
| **Tổng cộng** | | **68** | | |

---

## 6. Trách nhiệm Kiểm thử (Test Organization)

Theo giáo trình Chương 6 — **Tổ chức kiểm thử**:

| Vai trò | Thành viên | Trách nhiệm |
|---------|-----------|-------------|
| **Team Leader / Test Manager** | Nguyễn Chí Trung | Lập kế hoạch, log Jira, CI/CD |
| **Developer/Tester** | Minh Phụng | F02, F08, F13 — Reports & File Upload |
| **Developer/Tester** | Nguyễn Hoàng Phụng | F09, F11 — WasteCategory & BVA Images |
| **Developer/Tester** | Thanh Duy | F04, F07, F12 — CollectionTask & Decision Table |
| **Developer/Tester** | 11A6_03_Đăng | F03, F06 — Admin & Complaints |

> ⚠️ Theo nguyên tắc **độc lập kiểm thử** (Ch.6): Developer không tự test code của mình. Team Leader (Nguyễn Chí Trung) log Jira CI cho toàn nhóm.

---

## 7. Công cụ Kiểm thử (Test Tools)

Theo giáo trình Chương 7:

| Công cụ | Loại | Mục đích |
|---------|------|----------|
| **xUnit** | Unit/Integration testing framework | Backend .NET 8 |
| **Allure** | Test reporting | Báo cáo kết quả với Jira link |
| **CodeceptJS** | E2E automation | Browser-based testing |
| **Postman** | API testing | Auth & security validation |
| **GitHub Actions** | CI/CD | Tự động chạy tests mỗi commit |
| **Jira** | Defect tracking | Theo dõi bug & test progress |
| **SonarCloud** | Static analysis | Code quality metrics |

---

## 8. Môi trường Kiểm thử (Test Environment)

| Môi trường | URL | Mục đích |
|-----------|-----|---------|
| **CI/CD (GitHub Actions)** | windows-latest runner | Automated tests |
| **Staging (render.com)** | https://wrp-api.onrender.com | Integration tests |
| **Local** | localhost:8080 / localhost:3000 | Dev & debug |

**Test data:**
- InMemory database cho unit tests (isolation)
- Seeded data script: `seed_e2e_accounts.ps1`

---

## 9. Lịch trình Kiểm thử (Test Schedule)

| Giai đoạn | Hoạt động | Thời gian |
|----------|-----------|----------|
| Sprint 1 | Unit tests (Auth, Reports) | Tuần 1-2 |
| Sprint 2 | Integration + API tests | Tuần 3-4 |
| Sprint 3 | E2E + Bug regression | Tuần 5-6 |
| Sprint 4 | BVA + Decision Table + Bug fix | Tuần 7 (hiện tại) |

---

## 10. Tiêu chí Dừng Kiểm thử (Exit Criteria)

Theo giáo trình Chương 6 — **Khi nào dừng kiểm thử**:

- ✅ Tất cả 68 test cases đã chạy
- ✅ Pass rate ≥ 95% (65/68 = 95.6%)  
- ✅ Không có Critical/High bug chưa fix
- ✅ Coverage ≥ 85% statements (CI report)
- ✅ Jira: tất cả 19 KIEM test issues được log

**Bug exceptions:**  
- KIEM-26 (F = Bug open) — đang fix
- KIEM-29 (F = Bug open) — đang fix

---

## 11. Defect Report (Báo cáo Lỗi)

Theo giáo trình Chương 5 — **Lỗi phần mềm**:

| Defect ID | Mô tả | Severity | Status | Assigned |
|-----------|-------|----------|--------|---------|
| KIEM-26 | Missing mandatory image validation in Create Report | High | In Progress | Nguyễn Hoàng Phụng |
| KIEM-27 | PUT /notifications/{id}/read returns 200 for 404 | Medium | Done ✅ | Nguyễn Hoàng Phụng |
| KIEM-28 | Include taskId in report accept response | Low | To Do | Minh Phụng |
| KIEM-29 | Missing maximum 5 images validation constraint | High | To Do | Thanh Duy |

---

## 12. Metrics & Đo lường (Test Metrics)

| Metric | Giá trị hiện tại | Target |
|--------|-----------------|--------|
| Total Test Cases | 68 | ≥ 60 |
| Pass Rate | 95.6% (65/68) | ≥ 95% |
| Bug Detection Rate | 4 bugs found | N/A |
| KIEM Issues Covered | 19/19 (100%) | 100% |
| Kỹ thuật sử dụng | EP, BVA, ST, DT, EG, White-box | ≥ 5 kỹ thuật |

---

*Document này được tạo theo chuẩn giáo trình Kiểm thử Phần mềm Chương 6: Quản lý Kiểm thử.*  
*Cập nhật: 12/06/2026 — Nhóm 11A6 KCPM*
