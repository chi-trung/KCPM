# 📁 Tài Liệu Dự Án KCPM — Waste Recycling Platform

> **Cập nhật:** 2026-06-20

---

## Cấu trúc thư mục

```
docs/
├── README.md                              ← Bạn đang đọc file này
│
├── 01-testing/                            ← 📋 Kiểm thử (Testing)
│   ├── 01-TEST_PLAN.md                    ← Kế hoạch kiểm thử (v3.0)
│   ├── 02-TESTING_STRATEGY.md             ← Chiến lược kiểm thử tổng thể
│   ├── 03-TRACEABILITY_MATRIX.md          ← Ma trận truy vết (Req → TC → Code → CI)
│   ├── 04-TEST_ACCOUNTS.md               ← Tài khoản test + URLs production
│   ├── 05-WHITEBOX_ANALYSIS.md            ← Phân tích whitebox: CFG, V(G), Coverage
│   ├── reports/                           ← Báo cáo kiểm thử từng module
│   │   ├── REPORT_REPORTS_MODULE.md       ← Module Báo cáo rác (KIEM-5)
│   │   ├── REPORT_COMPLAINTS_COLLECTION.md← Module Khiếu nại + Thu gom (KIEM-67)
│   │   ├── REPORT_ADMIN_CITIZEN.md        ← Module Admin + Citizen
│   │   └── REPORT_CATEGORY_NOTIFICATIONS.md← Module Danh mục + Thông báo
│   └── bugs/                              ← Bug reports
│       └── BUG-REP-001.md                 ← BUG: Thiếu validation max 5 ảnh
│
├── 02-course/                             ← 🎓 Bài nộp môn học
│   ├── 01-CHAPTERS_1_TO_4.md              ← Áp dụng kiến thức Chương 1-4 vào project
│   ├── 02-FINAL_REPORT.md                 ← Báo cáo tổng kết (v6.0)
│   └── 03-DEMO_SCRIPT.md                  ← Kịch bản demo CI/CD cho thầy
│
├── 03-deployment/                         ← 🚀 CI/CD & Hạ tầng
│   ├── 01-CI_CD_WORKFLOWS.md              ← Chi tiết 9 GitHub Actions workflows
│   ├── 02-DEPLOYMENT_GUIDE.md             ← Hướng dẫn triển khai (Vercel + Render + Aiven)
│   ├── 03-AUTOMATION_SCOPE.md             ← Phạm vi automation & giới hạn
│   └── 04-JIRA_AUTOMATION.md              ← Hướng dẫn tạo Jira issue tự động
│
├── 04-team/                               ← 👥 Quy trình nhóm
│   ├── 01-WORKFLOW_GUIDE.md               ← Hướng dẫn quy trình: Sprint, Git, Jira
│   └── 02-CI_CD_SKILL.md                  ← Kỹ năng CI/CD cho thành viên
│
└── 05-internal/                           ← 📝 Ghi chú nội bộ (không nộp)
    ├── HISTORY_CHAT.md                    ← Lịch sử chat dev sessions
    └── NEXT_STEPS.md                      ← Checklist việc cần làm
```

---

## 🔗 Liên kết nhanh theo chủ đề

### Thầy muốn xem gì? → Đọc file nào

| Thầy hỏi | File |
|-----------|------|
| "Test Plan ở đâu?" | [`01-testing/01-TEST_PLAN.md`](01-testing/01-TEST_PLAN.md) |
| "Whitebox testing?" | [`01-testing/05-WHITEBOX_ANALYSIS.md`](01-testing/05-WHITEBOX_ANALYSIS.md) |
| "Áp dụng Chương 1-4?" | [`02-course/01-CHAPTERS_1_TO_4.md`](02-course/01-CHAPTERS_1_TO_4.md) |
| "Báo cáo tổng kết?" | [`02-course/02-FINAL_REPORT.md`](02-course/02-FINAL_REPORT.md) |
| "Demo CI/CD?" | [`02-course/03-DEMO_SCRIPT.md`](02-course/03-DEMO_SCRIPT.md) |
| "Bug reports?" | [`01-testing/bugs/BUG-REP-001.md`](01-testing/bugs/BUG-REP-001.md) |
| "Traceability?" | [`01-testing/03-TRACEABILITY_MATRIX.md`](01-testing/03-TRACEABILITY_MATRIX.md) |
| "Kết quả test module X?" | [`01-testing/reports/`](01-testing/reports/) |

---

## 📊 Tổng quan dự án

| Metric | Giá trị |
|--------|---------|
| Backend tests | **451+** (57 files, xUnit) |
| Whitebox tests | **43** (3 methods, CFG + V(G)) |
| E2E tests | **19** scenarios (10 files, CodeceptJS) |
| Frontend tests | **27** files (React Testing Library) |
| Postman tests | **74** requests, **128** assertions |
| CI/CD workflows | **9** GitHub Actions |
| SonarCloud coverage | **79.3%** |
| Jira issues | **36** (3 sprints) |

---

## 📚 Mapping Chương 1-4

| Chương | Nội dung | File tham chiếu |
|--------|----------|-----------------|
| **Ch.1** | Error/Fault/Failure, 7 Principles, Test Process | [`02-course/01-CHAPTERS_1_TO_4.md`](02-course/01-CHAPTERS_1_TO_4.md#chương-1--tổng-quan-về-kiểm-thử) |
| **Ch.2** | V-Model, 4 Test Levels, Regression, V&V | [`02-course/01-CHAPTERS_1_TO_4.md`](02-course/01-CHAPTERS_1_TO_4.md#chương-2--testing-trong-vòng-đời-phát-triển-pm) |
| **Ch.3** | Static Analysis (SonarCloud), Code Review (PR) | [`02-course/01-CHAPTERS_1_TO_4.md`](02-course/01-CHAPTERS_1_TO_4.md#chương-3--các-kỹ-thuật-kiểm-thử-tĩnh) |
| **Ch.4** | EP, BVA, State Transition, Decision Table, CFG, V(G), Coverage | [`02-course/01-CHAPTERS_1_TO_4.md`](02-course/01-CHAPTERS_1_TO_4.md#chương-4--các-kỹ-thuật-thiết-kế-test) |
