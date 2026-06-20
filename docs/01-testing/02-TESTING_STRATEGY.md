# Testing Strategy - Waste Recycling Platform

## 1. Muc tieu

Tai lieu nay chuan hoa cach kiem chung cho do an Waste Recycling Platform theo huong pipeline automation deploy server, nhung giu nguyen tac: don gian, chay that, trace duoc va giai thich duoc.

Project duoc trinh bay theo mo hinh client-server:

- Client: Next.js frontend.
- Server: ASP.NET Core Web API.
- Database: MySQL.
- Deployment: Docker Compose tren server.
- Automation evidence: GitHub Actions, xUnit, Postman/Newman, CodeceptJS, SonarCloud, Allure artifacts.

## 2. Co so kien thuc ap dung

Tai lieu nay bam theo cac chuong mon Kiem thu phan mem trong folder `docs-kcpm`:

- Chuong 1 - Tong quan: kiem thu can thiet vi phan mem co rui ro, bug va anh huong den chat luong; test khong chung minh phan mem het loi, chi giam rui ro.
- Chuong 2 - Testing trong vong doi phat trien PM: phan biet Verification va Validation; test duoc dat vao tung muc Unit, Integration, System va Acceptance.
- Chuong 3 - Kiem thu tinh: review tai lieu, review code, static analysis bang SonarCloud, khong can thuc thi chuong trinh.
- Chuong 4 - Ky thuat thiet ke test: phan tich dieu kien test, thiet ke test case, xay dung test script; uu tien black-box, boundary value, equivalence partitioning, decision table, state transition va error guessing.
- Chuong 5 - Loi phan mem: tap trung cac nhom loi UI, error handling, boundary, data handling, control flow, race condition, environment, version/control va documentation.
- Chuong 6 - Quan ly kiem thu: can scope, test plan, nguoi phu trach, moi truong test, milestone, defect tracking va cau hinh phien ban.
- Chuong 7 - Cong cu: cong cu chi ho tro hoat dong test lap lai; tool phai phuc vu muc tieu kiem chung, khong phai them tool cho dep.

## 3. Chien luoc kiem thu chinh

| Muc kiem thu | Muc tieu | Cong cu | Evidence |
|---|---|---|---|
| Static Testing | Phat hien code smell, bug pattern, duplicated code, maintainability issue truoc khi chay chuong trinh | SonarCloud, code review, PR review | SonarCloud result, PR review |
| Unit Testing | Kiem tra domain logic, command/query handler, service logic rieng le | xUnit, FluentAssertions, Moq | TRX, coverage, Allure xUnit |
| Integration/API Testing | Kiem tra backend API, database interaction, auth, status code, response payload | Postman/Newman, WebApplicationFactory | Newman report, Allure Postman, GitHub artifact |
| Frontend E2E Testing | Kiem tra luong nguoi dung tren client va routing UI | CodeceptJS + Playwright | Screenshot on failure, Allure E2E artifact |
| Deployment Verification | Xac nhan ban deploy tren server song va tra loi API health | GitHub Actions, Docker Compose, curl health check | Deploy workflow log |
| Regression Evidence | Luu lai ket qua qua tung lan push/PR/deploy | GitHub Actions artifacts, Allure report | Run link, artifact link |

## 4. Quality gate toi thieu

Mot thay doi chi duoc xem la san sang deploy khi dat cac dieu kien sau:

1. PR hoac commit co Jira key dung format, vi du `KIEM-16: add enterprise task tests`.
2. Backend xUnit pass.
3. Postman API smoke pass.
4. Frontend E2E smoke pass voi cac flow quan trong.
5. SonarCloud khong co blocker/critical issue moi.
6. Deploy server thanh cong.
7. Post-deploy health check `/api/health` tra ve OK.

## 5. Scope chinh va phan optional

### Phan chinh de demo voi thay

- Jira key traceability.
- GitHub PR/commit history.
- xUnit backend test.
- Postman/Newman API test.
- CodeceptJS E2E test.
- SonarCloud static analysis.
- Deploy server qua GitHub Actions.
- Post-deploy health check.
- Allure/GitHub Actions artifacts.

### Phan optional / future improvement

- Jira owner/name sync tu dong.
- Per-owner Allure report.
- Weekly report tu dong neu trung voi Allure Pages.
- Auto transition/comment Jira neu workflow chua on dinh.

Cac phan optional khong duoc dung lam bang chung chinh neu ben trong con workaround hoac khong giai thich duoc ro.

## 6. Cach noi trong bao cao

Cau chuyen nen trinh bay ngan gon:

> He thong ap dung pipeline kiem chung theo mo hinh client-server. Moi thay doi duoc gan Jira key, sau do GitHub Actions tu dong chay static testing, unit testing, API testing, E2E testing. Neu quality gate pass, he thong moi duoc deploy len server bang Docker Compose va duoc kiem chung sau deploy bang health check. Ket qua duoc luu thanh evidence trong GitHub Actions va Allure.

## 7. Viec can lam tiep

- Bo sung E2E nghiep vu that: citizen report, enterprise assign collector, collector complete task.
- Them traceability cho tung feature chinh.
- Giam phu thuoc vao Jira sync nang cao.
- Sua cac known limitation ve secret demo, password fix cung va encoding tieng Viet neu kip.
