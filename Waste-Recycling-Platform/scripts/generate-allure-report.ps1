param(
    [string]$ProjectPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.csproj",
    [string]$RunSettingsPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.runsettings",
    [string]$ResultsDirectory = ".\TestResults",
    [string]$AllureResultsPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\bin\Release\net8.0\allure-results",
    [string]$AllureReportPath = ".\TestResults\backend-allure-report",
    [string]$HistoryPath = ".\TestResults\backend-allure-history"
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

function Copy-DirectoryContent {
    param(
        [string]$Source,
        [string]$Destination
    )

    if (-not (Test-Path $Source)) {
        return
    }

    if (-not (Test-Path $Destination)) {
        New-Item -ItemType Directory -Path $Destination -Force | Out-Null
    }

    Get-ChildItem -Path $Source -Force | ForEach-Object {
        Copy-Item -Path $_.FullName -Destination $Destination -Recurse -Force
    }
}

function Write-AllureMetadata {
    param(
        [string]$ResultsPath,
        [string]$ProjectPath
    )

    if (-not (Test-Path $ResultsPath)) {
        New-Item -ItemType Directory -Path $ResultsPath -Force | Out-Null
    }

    $metadata = @{
        environmentProperties = @(
            "Environment=Local Windows workspace"
            "Branch=$(git branch --show-current)"
            "OS=$([System.Environment]::OSVersion.VersionString)"
            "DotNet=$(& $dotnet --version)"
            "TestProject=WastePlatform.Tests"
        )
    }

    $metadata.environmentProperties | Set-Content -Path (Join-Path $ResultsPath 'environment.properties') -Encoding utf8

    $executor = @{
        name = 'Local PowerShell runner'
        type = 'local'
        url = 'file:///C:/Users/gnurt/Desktop/KCPM'
        buildOrder = 1
        buildName = 'Local backend Allure run'
        reportName = 'WastePlatform Backend Allure Report'
        reportUrl = 'file:///C:/Users/gnurt/Desktop/KCPM/TestResults/backend-allure-report/index.html'
        branch = (git branch --show-current)
    } | ConvertTo-Json -Depth 5

    Set-Content -Path (Join-Path $ResultsPath 'executor.json') -Value $executor -Encoding utf8
}

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

if (Test-Path $HistoryPath) {
    Copy-DirectoryContent -Source $HistoryPath -Destination (Join-Path $AllureResultsPath 'history')
}

Write-AllureMetadata -ResultsPath $AllureResultsPath -ProjectPath $ProjectPath

Write-Host "Generating HTML report..."
& $allure generate $AllureResultsPath --clean -o $AllureReportPath

if (Test-Path (Join-Path $AllureReportPath 'history')) {
    if (Test-Path $HistoryPath) {
        Remove-Item -Path $HistoryPath -Recurse -Force
    }

    Copy-DirectoryContent -Source (Join-Path $AllureReportPath 'history') -Destination $HistoryPath
}

Write-Host "Allure HTML report generated at: $AllureReportPath\index.html"