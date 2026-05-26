param(
    [string]$ProjectPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.csproj",
    [string]$RunSettingsPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\WastePlatform.Tests.runsettings",
    [string]$ResultsDirectory = ".\TestResults",
    [string]$AllureResultsPath = ".\Waste-Recycling-Platform\backend\tests\WastePlatform.Tests\bin\Release\net8.0\allure-results",
    [string]$AllureReportPath = ".\TestResults\backend-allure-report",
    [string]$CategoriesPath = ".\Waste-Recycling-Platform\allure-categories.json",
    [ValidateSet('All', 'Owner', 'Both')]
    [string]$ReportMode = 'All',
    [ValidateSet('Html', 'Pdf', 'Both')]
    [string]$ExportFormat = 'Html',
    [string]$SelectedOwner = 'all'
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

function Get-BrowserCommand {
    $browser = Get-Command msedge.exe -ErrorAction SilentlyContinue
    if ($browser) {
        return $browser.Source
    }

    $browser = Get-Command chrome.exe -ErrorAction SilentlyContinue
    if ($browser) {
        return $browser.Source
    }

    $browserCandidates = @(
        "C:\Program Files\Microsoft\Edge\Application\msedge.exe",
        "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
        "C:\Program Files\Google\Chrome\Application\chrome.exe",
        "C:\Program Files (x86)\Google\Chrome\Application\chrome.exe"
    )

    foreach ($candidate in $browserCandidates) {
        if (Test-Path $candidate) {
            return $candidate
        }
    }

    throw "A browser executable was not found for PDF export. Install Microsoft Edge or Google Chrome."
}

function Export-HtmlToPdf {
    param(
        [Parameter(Mandatory = $true)]
        [string]$IndexHtmlPath,
        [Parameter(Mandatory = $true)]
        [string]$PdfPath
    )

    $browser = Get-BrowserCommand
    $resolvedHtml = (Resolve-Path $IndexHtmlPath).Path
    $pdfDirectory = Split-Path $PdfPath -Parent

    if ($pdfDirectory -and -not (Test-Path $pdfDirectory)) {
        New-Item -ItemType Directory -Force -Path $pdfDirectory | Out-Null
    }

    $uri = [System.Uri]::new($resolvedHtml).AbsoluteUri
    Write-Host "Exporting PDF: $PdfPath"
    & $browser `
        --headless `
        --disable-gpu `
        --no-first-run `
        --no-default-browser-check `
        --print-to-pdf-no-header `
        "--print-to-pdf=$PdfPath" `
        $uri

    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $PdfPath)) {
        throw "PDF export failed for $IndexHtmlPath"
    }
}

$dotnet = Get-DotNetCommand
$allure = Get-AllureCommand
$selectedReportMode = $ReportMode.ToLowerInvariant()
$selectedExportFormat = $ExportFormat.ToLowerInvariant()

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

if (Test-Path $CategoriesPath) {
    Write-Host "Copying categories from $CategoriesPath to $AllureResultsPath\categories.json"
    Copy-Item -Force $CategoriesPath (Join-Path $AllureResultsPath 'categories.json')
}

if ($selectedReportMode -in @('all', 'both')) {
    Write-Host "Generating HTML report..."
    & $allure generate $AllureResultsPath --clean -o $AllureReportPath

    if ($selectedExportFormat -in @('pdf', 'both')) {
        Export-HtmlToPdf -IndexHtmlPath (Join-Path $AllureReportPath 'index.html') -PdfPath (Join-Path $AllureReportPath 'report.pdf')
    }
}

if ($selectedReportMode -in @('owner', 'both')) {
    Write-Host "Generating per-owner reports..."
    $env:SELECTED_OWNER = $SelectedOwner
    & python .\Waste-Recycling-Platform\scripts\generate_per_owner_reports.py

    $ownerReportsRoot = Join-Path $ResultsDirectory 'owners'
    if ((Test-Path $ownerReportsRoot) -and ($selectedExportFormat -in @('pdf', 'both'))) {
        Get-ChildItem -Path $ownerReportsRoot -Directory | ForEach-Object {
            $ownerIndex = Join-Path $_.FullName 'index.html'
            if (Test-Path $ownerIndex) {
                Export-HtmlToPdf -IndexHtmlPath $ownerIndex -PdfPath (Join-Path $_.FullName 'report.pdf')
            }
        }
    }
}

if ($selectedReportMode -eq 'all') {
    Write-Host "Allure HTML report generated at: $AllureReportPath\index.html"
} elseif ($selectedReportMode -eq 'owner') {
    Write-Host "Per-owner Allure reports generated under: $ResultsDirectory\owners"
} else {
    Write-Host "Allure HTML report generated at: $AllureReportPath\index.html"
    Write-Host "Per-owner Allure reports generated under: $ResultsDirectory\owners"
}

# After generate, persist history back to ResultsDirectory for next runs
if ($selectedReportMode -in @('all', 'both')) {
    $generatedHistory = Join-Path $AllureReportPath "history"
    if (Test-Path $generatedHistory) {
        Write-Host "Persisting generated history to $historySrc"
        if (Test-Path $historySrc) { Remove-Item -Recurse -Force $historySrc }
        Copy-Item -Recurse -Force $generatedHistory $historySrc
    }
}