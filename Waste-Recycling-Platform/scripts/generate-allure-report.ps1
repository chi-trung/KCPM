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

Write-Host "Generating HTML report..."
& $allure generate $AllureResultsPath --clean -o $AllureReportPath

Write-Host "Allure HTML report generated at: $AllureReportPath\index.html"