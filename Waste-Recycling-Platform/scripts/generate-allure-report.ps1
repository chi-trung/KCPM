param(
    [string]$ProjectPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.csproj",
    [string]$RunSettingsPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.runsettings",
    [string]$ResultsDirectory = ".\TestResults",
    [string]$AllureResultsPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\bin\Release\net8.0\allure-results",
    [string]$AllureReportPath = ".\TestResults\backend-allure-report"
)

$ErrorActionPreference = "Stop"

function Get-DotNetCommand {
    $dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($dotnet) {
        return $dotnet.Source
    }

    $fallback = "C:\Program Files\dotnet\dotnet.exe"
    if (Test-Path $fallback) {
        return $fallback
    }

    throw "dotnet was not found. Install the .NET 8 SDK or add it to PATH."
}

function Get-AllureCommand {
    $allure = Get-Command allure -ErrorAction SilentlyContinue
    if ($allure) {
        return $allure.Source
    }

    throw "allure was not found. Install it first, for example: npm install -g allure-commandline"
}

$dotnet = Get-DotNetCommand
$allure = Get-AllureCommand

Write-Host "Running backend tests with Allure enabled..."
& $dotnet test $ProjectPath `
    --configuration Release `
    --settings $RunSettingsPath `
    --logger "trx;LogFileName=backend-tests.trx" `
    --collect:"XPlat Code Coverage" `
    --results-directory $ResultsDirectory

if (-not (Test-Path $AllureResultsPath)) {
    throw "Allure results were not found at $AllureResultsPath"
}

# Add environment and executor metadata so Allure Overview shows details
$envFile = Join-Path $AllureResultsPath "environment.properties"
Write-Host "Writing environment metadata to $envFile"
if ($env:GITHUB_REF_NAME) { $branch = $env:GITHUB_REF_NAME } else { $branch = "local" }
"Branch=$branch" | Out-File -FilePath $envFile -Encoding utf8
"OS=$(Get-CimInstance Win32_OperatingSystem | Select-Object -ExpandProperty Caption)" | Out-File -FilePath $envFile -Encoding utf8 -Append
"DotNet=$(& $dotnet --version)" | Out-File -FilePath $envFile -Encoding utf8 -Append

# executor.json for CI metadata (kept minimal)
$executorFile = Join-Path $AllureResultsPath "executor.json"
$executor = @{
    name = "Local PowerShell runner"
    type = "local"
    url = ""
}
$executor | ConvertTo-Json | Out-File -FilePath $executorFile -Encoding utf8

# Preserve history if present
$historySrc = Join-Path $ResultsDirectory "allure-history"
$historyDst = Join-Path $AllureResultsPath "history"
if (Test-Path $historySrc) {
    Write-Host "Copying existing history from $historySrc to $historyDst"
    if (Test-Path $historyDst) { Remove-Item -Recurse -Force $historyDst }
    Copy-Item -Recurse -Force $historySrc $historyDst
}

Write-Host "Generating HTML report..."
& $allure generate $AllureResultsPath --clean -o $AllureReportPath

# After generate, persist history back to ResultsDirectory for next runs
$generatedHistory = Join-Path $AllureReportPath "history"
if (Test-Path $generatedHistory) {
    Write-Host "Persisting generated history to $historySrc"
    if (Test-Path $historySrc) { Remove-Item -Recurse -Force $historySrc }
    Copy-Item -Recurse -Force $generatedHistory $historySrc
}

Write-Host "Allure HTML report generated at: $AllureReportPath\index.html"