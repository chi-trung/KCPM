# Next Steps - KCPM Verification Cleanup

## Priority 1 - Lam bai de hieu va de demo

1. Doc `docs/TESTING_STRATEGY.md` truoc khi trinh bay.
2. Dung `docs/CI_CD_PIPELINE_SIMPLIFIED.md` lam cau chuyen chinh.
3. Cap nhat `docs/TRACEABILITY_MATRIX.md` moi khi them Jira/test case.
4. Khi thuyet trinh, khong dua Jira owner/name sync lam phan chinh.

## Priority 2 - Them test co gia tri

Them 3 E2E flow:

- `TC-E2E-REPORT-001`: Citizen login va tao waste report.
- `TC-E2E-TASK-001`: Enterprise login va assign collector.
- `TC-E2E-COLLECTOR-001`: Collector login va complete task.

Moi flow nen co:

- Preconditions.
- Test data.
- Steps.
- Expected result.
- Automation file mapping.
- Evidence link.

## Priority 3 - Don no ky thuat de tranh bi hoi kho

- Bo `--no-lint` khoi build hoac them script lint/typecheck rieng.
- Chuyen secret demo thanh placeholder.
- Sua password fix cung trong create user.
- Chuan hoa UTF-8 cho README va E2E text.
- Tach Jira automation phuc tap thanh experimental.

## Priority 4 - Bao cao cuoi

Bao cao nen co cac muc:

1. Project overview va client-server architecture.
2. Testing strategy.
3. Test levels va test types.
4. Static testing bang SonarCloud.
5. Unit testing bang xUnit.
6. API/integration testing bang Postman/Newman.
7. E2E testing bang CodeceptJS.
8. CI/CD deploy server.
9. Traceability matrix.
10. Defect management va known limitations.
