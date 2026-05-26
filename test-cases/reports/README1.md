# 📊 Reports Module Test Cases

> Waste Reports Lifecycle Testing - WRP-BE-TESTS-002

## 📋 Overview

Module này kiểm thử vòng đời của báo cáo rác thải (Waste Report Lifecycle):
- Citizen tạo báo cáo
- Enterprise/Admin tiếp nhận hoặc từ chối
- Trạng thái chuyển đổi đúng quy trình

## 🧪 Danh Sách Test Cases

| TC ID | Tên Test Case | Type | Priority | API Endpoint | Status |
|-------|---------------|------|----------|--------------|--------|
| TC-REP-001 | Create report valid (image + data) | Positive | Critical | POST /api/reports/create | ⬜ |
| TC-REP-002 | Create report missing field | Negative | High | POST /api/reports/create | ⬜ |
| TC-REP-003 | Get report by ID valid | Positive | High | GET /api/reports/{id} | ⬜ |
| TC-REP-004 | Get report invalid ID | Negative | Medium | GET /api/reports/{id} | ⬜ |
| TC-REP-005 | Accept report (authorized role) | Positive | High | POST /api/reports/{id}/accept | ⬜ |
| TC-REP-006 | Reject report with reason | Positive | High | POST /api/reports/{id}/reject | ⬜ |
| TC-REP-007 | Invalid state transition | Negative | Medium | Various | ⬜ |
| TC-REP-008 | Upload image invalid format | Negative | Medium | POST /api/reports/create | ⬜ |

**Tổng: 8 test cases** (4 Positive + 4 Negative)

## 🔄 Report Lifecycle State Machine

```
┌─────────┐     Create      ┌─────────┐
│  Start  │ ──────────────→ │ Pending │
└─────────┘                 └────┬────┘
                                 │
                    ┌────────────┼────────────┐
                    │            │            │
                    ▼            ▼            ▼
              ┌─────────┐  ┌─────────┐  ┌─────────┐
              │Accepted │  │Rejected │  │ (Invalid│
              │         │  │         │  │  trans)  │
              └────┬────┘  └─────────┘  └─────────┘
                   │
                   │ Collect
                   ▼
              ┌─────────┐
              │Completed│
              └─────────┘
```

**Valid Transitions:**
- Pending → Accepted
- Pending → Rejected
- Accepted → Completed

**Invalid Transitions (TC-REP-007):**
- Accepted → Accepted ❌
- Rejected → Accepted ❌
- Completed → Any ❌

## 🚀 Quick Start

### 1. Prerequisites
```bash
# Start backend
cd Waste-Recycling-Platform/backend
dotnet run

# Verify health
curl http://localhost:8080/api/health
```

### 2. Run Test Sequence

Thứ tự chạy test (có dependencies):

```
1. TC-AUTH-004: Login Citizen (lấy token)
   ↓
2. TC-REP-001: Create report (tạo report ID)
   ↓
3. TC-REP-003: Get report by ID (dùng report ID từ #2)
   ↓
4. TC-AUTH-xxx: Login Enterprise (lấy enterprise token)
   ↓
5. TC-REP-005: Accept report
   ↓
6. TC-REP-007: Try accept again (should fail)
```

### 3. Run with Newman

```bash
# Run Reports folder only
newman run Waste-Recycling-Platform/postman/WastePlatform.professional.postman_collection.json \
  -e Waste-Recycling-Platform/postman/WastePlatform.professional.postman_environment.json \
  --folder "03 - Reports" \
  --reporters cli,junit \
  --reporter-junit-export test-results/reports-tests.xml
```

## 📝 Test Data Requirements

### Images cho TC-REP-001 và TC-REP-008

Tạo thư mục `test-assets/`:

```
test-assets/
├── valid-image.jpg        (JPG, < 5MB) ✅
├── valid-image.png        (PNG, < 5MB) ✅
├── large-image.jpg        (> 10MB) ❌ TC-REP-008
├── document.pdf           (PDF) ❌ TC-REP-008
├── corrupted.jpg          (Broken) ❌ TC-REP-008
└── script.php.jpg         (Malicious) ❌ TC-REP-008
```

### Categories cần có

```sql
-- Kiểm tra categories trong DB
SELECT * FROM waste_categories;
-- Cần ít nhất 1 category với id = 1
```

## 📊 Test Execution Tracking

Cập nhật sau khi chạy từng test:

| Date | Tester | TC Run | Pass | Fail | Notes |
|------|--------|--------|------|------|-------|
| ⬜ | ⬜ | ⬜ | ⬜ | ⬜ | ⬜ |

## 🐛 Bug Tracking

Nếu phát hiện lỗi, tạo file trong `docs/bugs/`:

```markdown
# BUG-REP-001: [Mô tả ngắn]

**Related TC**: TC-REP-xxx
**Severity**: High/Medium/Low
**Status**: Open/In Progress/Fixed

## Description
...

## Steps to Reproduce
...

## Expected vs Actual
...

## Screenshots
...
```

## 🔗 Dependencies

### Test Cases liên quan
- **Auth Module**: Cần login trước khi test reports
  - TC-AUTH-004: Login Citizen
  - TC-AUTH-007: Login Enterprise (cho accept/reject)

### Postman Variables cần thiết
| Variable | Set By | Used By |
|----------|--------|---------|
| `citizenToken` | TC-AUTH-004 | TC-REP-001, 002, 003 |
| `enterpriseToken` | TC-AUTH-xxx | TC-REP-005, 006 |
| `reportId` | TC-REP-001 | TC-REP-003, 005, 006, 007 |

## 🎯 Success Criteria

Module pass khi:
- [ ] All 8 test cases executed
- [ ] At least 80% pass rate (7/8)
- [ ] Critical test (TC-REP-001, 004, 005) must pass
- [ ] State transitions work correctly
- [ ] Image validation secure

## 📚 Resources

- **Jira Epic**: [WRP-BE-TESTS-002](jira-link)
- **API Docs**: `Waste-Recycling-Platform/docs/openapi.yaml`
- **Postman Collection**: `postman/WastePlatform.professional.postman_collection.json`
- **Database Schema**: `db/migration_mysql.sql`

---

**Last Updated**: 2026-05-26
**Module Owner**: Nguyễn Minh Phụng
