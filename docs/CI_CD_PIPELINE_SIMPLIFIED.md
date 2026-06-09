# Simplified CI/CD Pipeline

## 1. Muc tieu

Pipeline nay thay the cach nhin cu qua roi bang mot duong chay chinh, de demo va de giai thich:

```text
Jira issue -> Branch/Commit/PR -> Quality Gate -> Deploy Server -> Post-deploy Verification -> Evidence
```

Pipeline khong phu thuoc vao cac script Jira sync phuc tap. Jira duoc dung de trace task, khong phai de che giau logic automation.

## 2. Luong chinh

1. Tao Jira issue cho feature/bug/test task.
2. Tao branch co Jira key, vi du `feature/KIEM-16-enterprise-task-tests`.
3. Commit co Jira key, vi du `KIEM-16: add enterprise task API tests`.
4. Tao PR co Jira key trong title.
5. GitHub Actions chay quality gate:
   - Jira key enforcement.
   - Backend xUnit.
   - Postman/Newman API smoke.
   - Frontend CodeceptJS E2E.
   - SonarCloud static analysis.
6. Neu quality gate pass, deploy server bang Docker Compose.
7. Sau deploy, pipeline goi backend health endpoint `/api/health`.
8. GitHub Actions luu log va artifact lam evidence.

## 3. Workflow roles

| Workflow | Vai tro | Co phai phan chinh khong? |
|---|---|---|
| `jira-key-enforcement.yml` | Dam bao PR/commit co Jira key de traceability | Yes |
| `backend-tests.yml` | Chay xUnit, coverage, Allure backend | Yes |
| `postman-smoke.yml` | Chay API smoke/regression bang Newman | Yes |
| `frontend-e2e.yml` | Chay E2E UI bang CodeceptJS | Yes |
| `sonar.yml` | Static analysis theo kiem thu tinh | Yes |
| `deploy-server.yml` | Deploy khi backend quality gate pass va health check sau deploy | Yes |
| `allure-gh-pages.yml` | Publish Allure report tong hop | Helpful |
| `postman-weekly-report.yml` | Bao cao lap lai theo tuan | Optional |
| Jira owner/name sync scripts | Gan owner/name cho report | Optional/Future improvement |

## 4. Quality gate de giai thich voi thay

Quality gate la cua chan chat luong truoc deploy:

- Verification: code co dung dac ta ky thuat/API/logic khong?
  - xUnit.
  - Postman/Newman.
  - SonarCloud.
- Validation: he thong co dap ung luong nguoi dung khong?
  - CodeceptJS E2E.
  - Post-deploy smoke.

## 5. Evidence can nop/demo

Moi dot nop bai nen co cac link/screenshot sau:

- Jira issue link.
- PR link.
- GitHub Actions run link.
- Backend xUnit artifact.
- Postman/Newman result.
- E2E output/screenshot on failure.
- SonarCloud summary.
- Deploy workflow log co `/api/health` OK.
- Traceability Matrix row tuong ung.

## 6. Phan nao dang co the gay roi

Khong nen dua cac phan sau vao cau chuyen chinh neu chua that su on dinh:

- Auto sync owner/name theo Jira issue.
- Auto transition Jira phuc tap.
- Per-owner Allure report.
- Weekly report neu noi dung lap lai voi Allure Pages.

Neu bi hoi, tra loi ro:

> Nhom em tach Jira automation nang cao thanh future improvement. Pipeline chinh hien tai dung Jira key de traceability va dung GitHub Actions artifact lam bang chung kiem chung.

## 7. Checklist moi lan lam task

- [ ] Jira issue co mo ta requirement/test objective.
- [ ] Branch/commit/PR co Jira key.
- [ ] Co test case ID trong `TRACEABILITY_MATRIX.md`.
- [ ] Co xUnit/API/E2E/manual evidence phu hop.
- [ ] Pipeline pass.
- [ ] Neu deploy, `/api/health` pass sau deploy.
- [ ] Neu fail, tao defect voi buoc tai tao va expected/actual result.
