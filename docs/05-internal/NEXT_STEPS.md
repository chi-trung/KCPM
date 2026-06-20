# Next Steps - KCPM Verification Cleanup

> **Cập nhật:** 13/06/2026 — Session 9

## ✅ Đã hoàn thành (Session 1-9)

- [x] Deploy full-stack: Frontend (Vercel) + Backend (Render) + DB (Aiven MySQL)
- [x] 11 GitHub Actions workflows hoạt động đầy đủ
- [x] Fix CORS cho Vercel frontend (`*.vercel.app`)
- [x] Fix DB auto-migration (`EnsureCreated()`) cho cloud deploy
- [x] Fix Auth error handling (catch-all exception)
- [x] Fix Deploy Hook (accept HTTP 202)
- [x] Tạo DEPLOYMENT_GUIDE.md (892 dòng, kiến trúc + diagrams)
- [x] Tạo CI_CD_WORKFLOWS.md (450 dòng, chi tiết 11 workflows)
- [x] Seed data production: 5 categories, 8 accounts, enterprise/collector profiles
- [x] Cập nhật FINAL_REPORT.md v5.0 → v6.0 (451 tests, 11 workflows)
- [x] Tạo DEMO.md (696 dòng, kịch bản demo cho thầy)
- [x] Fix SonarCloud Quality Gate (xóa hardcoded secrets, exclusions)
- [x] Fix KIEM-26: Image validation (already fixed, status updated)
- [x] Fix KIEM-29: Max 5 images validation (code fix applied)
- [x] Fix CreateUserCommand hardcoded password → BCrypt

## Priority 1 - Làm bài để hiểu và để demo

1. Đọc `docs/DEMO.md` — kịch bản demo chi tiết 11 workflows.
2. Dùng `docs/TESTING_STRATEGY.md` để giải thích chiến lược.
3. Cập nhật `docs/TRACEABILITY_MATRIX.md` mỗi khi thêm Jira/test case.
4. Dùng DEPLOYMENT_GUIDE.md cho phần deploy architecture.

## Priority 2 - Nợ kỹ thuật còn lại

- Fix KIEM-28: Include taskId in report accept response.
- Bỏ `--no-lint` khỏi build hoặc thêm script lint/typecheck riêng.
- Chuẩn hóa UTF-8 cho README và E2E text.

## Priority 3 - Báo cáo cuối

Báo cáo nên có các mục:

1. Project overview và client-server architecture.
2. Testing strategy (6 kỹ thuật Ch.4).
3. Test levels và test types (451 xUnit + 19 E2E + 74 Postman).
4. Static testing bằng SonarCloud.
5. CI/CD deploy server (11 workflows).
6. Deployment architecture (Vercel + Render + Aiven).
7. Traceability matrix.
8. Defect management (4 bugs, 3 fixed).
