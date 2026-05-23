# Backend Testing Playbook

This repository now has a backend testing setup designed to be easy to explain in class and easy to prove in GitHub.

## Stack

- Unit tests: xUnit
- Assertions: FluentAssertions
- Mocking: Moq
- Coverage: coverlet.collector
- HTML test reports: Allure.Xunit + Allure CLI
- CI: GitHub Actions workflow at `.github/workflows/backend-tests.yml`
- Postman CI smoke: GitHub Actions workflow at `.github/workflows/postman-smoke.yml`
- Manual API smoke tests: Postman collection in `postman/WastePlatform.postman_collection.json`

## Linking GitHub, Jira, and Postman

Use the same Jira key everywhere:

- Branch name: `feature/WASTE-TEST-001-unit-tests`
- Commit message: `WASTE-TEST-001: add backend unit tests`
- GitHub PR title: `WASTE-TEST-001 backend test suite`
- Postman collection folder/request names: `[WASTE-TEST-001] ...`

That gives you a clean trace from Jira issue to GitHub commit to Postman request.

## What is already included

- A real backend test project at `backend/tests/WastePlatform.Tests`
- Sample unit tests for domain state transitions and an application command handler
- A GitHub Actions workflow that runs on push and pull request
- A Postman collection with health and auth smoke requests
- Allure-enabled xUnit reporting for backend test evidence

## Suggested next steps

1. Add controller tests with `WebApplicationFactory`.
2. Add integration tests with Testcontainers if you want DB-backed proof.
3. Add a PR template requiring Jira key and Postman evidence.

## How To Export Allure Report From xUnit

Use this flow when you need a real Allure report for class submission or weekly evidence.

If you want a one-command local runner on Windows, use:

```powershell
.\Waste-Recycling-Platform\scripts\generate-allure-report.ps1
```

### 1. Run the backend tests

Run the xUnit test project with the Allure runsettings enabled so the test adapter writes result files:

```powershell
dotnet test .\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.csproj `
	--configuration Release `
	--settings .\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.runsettings `
	--logger "trx;LogFileName=backend-tests.trx" `
	--collect:"XPlat Code Coverage" `
	--results-directory .\TestResults
```

### 2. Find the Allure results folder

After the test run, Allure xUnit writes raw result files to the test output folder, typically:

- `Waste-Recycling-Platform/backend/tests/WastePlatform.Tests/bin/Release/net8.0/allure-results`

If the folder is empty, verify that `WastePlatform.Tests.runsettings` still contains:

```xml
<RunSettings>
	<xUnit>
		<ReporterSwitch>allure</ReporterSwitch>
	</xUnit>
</RunSettings>
```

### 3. Generate the HTML report

Use Allure CLI to convert the raw results into a browsable HTML report:

```powershell
allure generate .\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\bin\Release\net8.0\allure-results --clean -o .\TestResults\backend-allure-report
```

### 4. Open or submit the report

- Open `TestResults/backend-allure-report/index.html` in a browser.
- Upload `TestResults/backend-allure-report` as a GitHub Actions artifact.
- Keep the raw `allure-results` folder and the `.trx` file as backup evidence.

### 5. Optional PDF export

Allure itself produces HTML, not PDF. If your teacher requires PDF, open the HTML report in a browser and use Print to PDF.

### 6. CI behavior in this repo

The GitHub Actions workflows already do the same steps automatically:

- run the xUnit tests,
- collect Allure results,
- generate the HTML report,
- upload both raw results and the generated report as artifacts.

That means your weekly report should point to the artifact or the generated HTML, not to a handwritten `.md` file.

## Install Allure CLI On Windows

If `allure` is not found, install it before running the script:

```powershell
npm install -g allure-commandline
```

Then reopen the terminal and run the script again.