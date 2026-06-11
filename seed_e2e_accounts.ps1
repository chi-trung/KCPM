# seed_e2e_accounts.ps1
# ============================================================
# Script tạo test accounts cho E2E tests qua REST API
# Chạy script này khi backend đang chạy ở http://localhost:8080
#
# Usage:
#   .\seed_e2e_accounts.ps1
#   .\seed_e2e_accounts.ps1 -ApiUrl "http://localhost:8080"
#
# Accounts tạo:
#   enterprise@test.waste / password  (role: Enterprise)
#   collector@test.waste  / password  (role: Collector -- qua Admin API)
#   citizen@test.waste    / password  (role: Citizen)
# ============================================================

param(
    [string]$ApiUrl = "http://localhost:8080"
)

$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  E2E Test Account Seeder - KCPM           " -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Target API: $ApiUrl" -ForegroundColor Gray
Write-Host ""

# ── Helper: Register account ──────────────────────────────────────────────────
function Register-Account {
    param(
        [string]$Email,
        [string]$Password,
        [string]$FullName,
        [string]$Role
    )

    $body = @{
        email    = $Email
        password = $Password
        fullName = $FullName
        role     = $Role
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod `
            -Uri "$ApiUrl/api/auth/register" `
            -Method POST `
            -ContentType "application/json; charset=utf-8" `
            -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) `
            -ErrorAction Stop

        Write-Host "  [OK] $Email ($Role)" -ForegroundColor Green
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 409) {
            Write-Host "  [SKIP] $Email -- already exists (409)" -ForegroundColor Yellow
        } else {
            Write-Host "  [FAIL] $Email -- $($_.Exception.Message)" -ForegroundColor Red
        }
        return $null
    }
}

# ── Helper: Login and get token ───────────────────────────────────────────────
function Get-Token {
    param([string]$Email, [string]$Password)

    $body = @{
        email    = $Email
        password = $Password
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod `
            -Uri "$ApiUrl/api/auth/login" `
            -Method POST `
            -ContentType "application/json; charset=utf-8" `
            -Body ([System.Text.Encoding]::UTF8.GetBytes($body)) `
            -ErrorAction Stop

        return $response.token
    }
    catch {
        Write-Host "  [FAIL] Login $Email -- $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# ── Step 1: Check API health ──────────────────────────────────────────────────
Write-Host "[1/4] Checking API health..." -ForegroundColor White
try {
    $health = Invoke-RestMethod -Uri "$ApiUrl/api/health" -Method GET -ErrorAction Stop
    Write-Host "  [OK] API is healthy" -ForegroundColor Green
}
catch {
    Write-Host "  [WARN] /api/health returned error (may still work): $($_.Exception.Message)" -ForegroundColor Yellow
}

# ── Step 2: Create Citizen account ───────────────────────────────────────────
Write-Host ""
Write-Host "[2/4] Creating Citizen E2E account..." -ForegroundColor White
Register-Account `
    -Email    "citizen@test.waste" `
    -Password "password" `
    -FullName "E2E Citizen Test" `
    -Role     "citizen"

# ── Step 3: Create Enterprise account ────────────────────────────────────────
Write-Host ""
Write-Host "[3/4] Creating Enterprise E2E account..." -ForegroundColor White
Register-Account `
    -Email    "enterprise@test.waste" `
    -Password "password" `
    -FullName "E2E Enterprise Test" `
    -Role     "enterprise"

# ── Step 4: Login as Admin and create Collector via Admin API ─────────────────
Write-Host ""
Write-Host "[4/4] Creating Collector E2E account via Admin..." -ForegroundColor White
Write-Host "  Note: Collector role requires Admin privileges or direct DB seed." -ForegroundColor Gray
Write-Host "  Trying Admin login (admin@gmail.com / password)..." -ForegroundColor Gray

$adminToken = Get-Token -Email "admin@gmail.com" -Password "password"

if ($adminToken) {
    # Try to create collector via admin endpoint if available
    $collectorBody = @{
        email      = "collector@test.waste"
        password   = "password"
        fullName   = "E2E Collector Test"
        role       = "collector"
        phone      = "0911900001"
    } | ConvertTo-Json

    try {
        $collectorResponse = Invoke-RestMethod `
            -Uri "$ApiUrl/api/admin/users" `
            -Method POST `
            -ContentType "application/json; charset=utf-8" `
            -Headers @{ Authorization = "Bearer $adminToken" } `
            -Body ([System.Text.Encoding]::UTF8.GetBytes($collectorBody)) `
            -ErrorAction Stop

        Write-Host "  [OK] collector@test.waste (Collector) via Admin API" -ForegroundColor Green
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 409) {
            Write-Host "  [SKIP] collector@test.waste -- already exists" -ForegroundColor Yellow
        } else {
            Write-Host "  [INFO] Admin API not available, use SQL seed instead:" -ForegroundColor Yellow
            Write-Host "         Run: V9__e2e_test_accounts.sql against your MySQL DB" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "  [INFO] Admin login failed. Use SQL seed instead:" -ForegroundColor Yellow
    Write-Host "         File: db/migrations/V9__e2e_test_accounts.sql" -ForegroundColor Gray
}

# ── Summary ───────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Seed Summary" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Email                   | Password | Role" -ForegroundColor White
Write-Host "  ------------------------|----------|----------" -ForegroundColor Gray
Write-Host "  citizen@test.waste      | password | Citizen" -ForegroundColor Green
Write-Host "  enterprise@test.waste   | password | Enterprise" -ForegroundColor Green
Write-Host "  collector@test.waste    | password | Collector (via SQL or Admin)" -ForegroundColor Green
Write-Host ""
Write-Host "  These accounts are used by:" -ForegroundColor Gray
Write-Host "    - e2e/citizen_report_test.js (TC-E2E-002)" -ForegroundColor Gray
Write-Host "    - e2e/enterprise_assign_test.js (TC-E2E-003)" -ForegroundColor Gray
Write-Host "    - e2e/collector_task_test.js (TC-E2E-004)" -ForegroundColor Gray
Write-Host ""
Write-Host "  To verify accounts exist:" -ForegroundColor Gray
Write-Host "    curl $ApiUrl/api/auth/login -d '{""email"":""citizen@test.waste"",""password"":""password""}'" -ForegroundColor Gray
Write-Host ""
