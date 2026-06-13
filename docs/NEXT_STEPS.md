# Next Steps - KCPM Verification Cleanup

> **Cập nhật:** 13/06/2026 — Session 7

## ✅ Đã hoàn thành (Session 1-7)

- [x] Deploy full-stack: Frontend (Vercel) + Backend (Render) + DB (Aiven MySQL)
- [x] 11 GitHub Actions workflows hoạt động đầy đủ
- [x] Fix CORS cho Vercel frontend (`*.vercel.app`)
- [x] Fix DB auto-migration (`EnsureCreated()`) cho cloud deploy
- [x] Fix Auth error handling (catch-all exception)
- [x] Fix Deploy Hook (accept HTTP 202)
- [x] Tạo DEPLOYMENT_GUIDE.md (892 dòng, kiến trúc + diagrams)
- [x] Tạo CI_CD_WORKFLOWS.md (450 dòng, chi tiết 11 workflows)
- [x] Seed data production: 5 categories, 8 accounts, enterprise/collector profiles
- [x] Cập nhật FINAL_REPORT.md v5.0 (11 workflows)

## Priority 1 - Làm bài để hiểu và để demo

1. Đọc `docs/TESTING_STRATEGY.md` trước khi trình bày.
2. Dùng `docs/CI_CD_PIPELINE_SIMPLIFIED.md` làm câu chuyện chính.
3. Cập nhật `docs/TRACEABILITY_MATRIX.md` mỗi khi thêm Jira/test case.
4. Khi thuyết trình, không đưa Jira owner/name sync làm phần chính.
5. **Mới:** Dùng DEPLOYMENT_GUIDE.md cho phần deploy architecture.

## Priority 2 - Thêm test có giá trị

Thêm 3 E2E flow:

- `TC-E2E-REPORT-001`: Citizen login và tạo waste report.
- `TC-E2E-TASK-001`: Enterprise login và assign collector.
- `TC-E2E-COLLECTOR-001`: Collector login và complete task.

Mỗi flow nên có:

- Preconditions.
- Test data.
- Steps.
- Expected result.
- Automation file mapping.
- Evidence link.

## Priority 3 - Dọn nợ kỹ thuật để tránh bị hỏi khó

- Bỏ `--no-lint` khỏi build hoặc thêm script lint/typecheck riêng.
- Chuyển secret demo thành placeholder.
- Sửa password fix cứng trong create user.
- Chuẩn hóa UTF-8 cho README và E2E text.
- Tách Jira automation phức tạp thành experimental.

## Priority 4 - Báo cáo cuối

Báo cáo nên có các mục:

1. Project overview và client-server architecture.
2. Testing strategy.
3. Test levels và test types.
4. Static testing bằng SonarCloud.
5. Unit testing bằng xUnit.
6. API/integration testing bằng Postman/Newman.
7. E2E testing bằng CodeceptJS.
8. CI/CD deploy server (11 workflows).
9. Deployment architecture (Vercel + Render + Aiven).
10. Traceability matrix.
11. Defect management và known limitations.
