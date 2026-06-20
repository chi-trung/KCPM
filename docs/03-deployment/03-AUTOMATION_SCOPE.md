# Automation Scope and Known Limitations

## 1. Ly do co tai lieu nay

Project da co nhieu automation: Jira, GitHub Actions, Postman, Allure, Sonar, CodeceptJS va deploy server. Tuy nhien, khong phai automation nao cung nen dua vao phan chinh cua bai nop.

Nguyen tac cua project tu thoi diem nay:

> Automation nao chay that, giai thich duoc va tao evidence ro thi dua vao pipeline chinh. Automation nao con workaround hoac phuc tap kho giai thich thi de optional/future improvement.

## 2. Pipeline chinh

Cac phan sau duoc xem la official evidence:

- Jira key trong branch/commit/PR.
- Backend xUnit result.
- Postman/Newman API result.
- CodeceptJS E2E result.
- SonarCloud static analysis.
- Deploy server workflow.
- Post-deploy `/api/health` check.
- GitHub Actions artifacts va Allure report.

## 3. Optional/future improvement

Cac phan sau khong dung lam bang chung chinh neu chua on dinh:

- Jira owner/name sync.
- Auto map assignee display name neu phai hard-code/manual fallback.
- Per-owner Allure report.
- Auto comment/transition Jira neu token/permission/transition name khong on dinh.

## 4. Cach giai thich ve Jira sync

Neu duoc hoi ve Jira owner/name sync, noi thang:

> Nhom em co thu nghiem sync owner/name tu Jira de chia report theo thanh vien. Tuy nhien API/permission mapping cua Jira khong on dinh trong moi truong lop, nen phan nay duoc xem la experimental. Pipeline chinh khong phu thuoc vao no; chung em dung Jira key de trace issue -> commit -> PR -> test evidence.

Cach noi nay trung thuc va khong lam hong bai, vi trong kiem thu phan mem, evidence phai tai lap va giai thich duoc.

## 5. Known limitations can sua sau

| Limitation | Rui ro | Huong xu ly |
|---|---|---|
| Frontend E2E moi chu yeu la smoke public page | Chua validation du luong nghiep vu client-server | Them E2E citizen/enterprise/collector/admin |
| Secret demo trong config | Bi hoi ve security/configuration management | Chuyen sang `.env.example` va GitHub Secrets |
| Mot so comment/text bi loi encoding | Bao cao/demo kem chuyen nghiep | Chuan hoa UTF-8 |
| Password tao user admin con fix cung | Rui ro security va logic auth | Dung BCrypt va temporary password/reset flow |
| Sonar chua gan quality gate ro trong deploy | Static testing chua chan deploy | Them quality gate neu token/project da on dinh |

## 6. Definition of Done moi

Mot task duoc xem la done khi:

- Co Jira key.
- Co code/PR lien quan.
- Co test case hoac evidence tuong ung.
- CI pass.
- Neu task anh huong deploy, server health check pass.
- Neu co bug, defect duoc ghi voi reproduction steps, actual result, expected result va severity.
