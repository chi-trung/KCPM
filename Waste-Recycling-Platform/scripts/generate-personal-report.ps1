$ErrorActionPreference = "Stop"

# 1. Đường dẫn thư mục
$rootDir = "$PSScriptRoot\.."
$resultsDir = "$rootDir\TestResults\personal-allure-results"

# Xóa thư mục kết quả cũ nếu có
if (Test-Path $resultsDir) {
    Remove-Item -Path $resultsDir -Recurse -Force
}
New-Item -ItemType Directory -Path $resultsDir | Out-Null

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "1. BẮT ĐẦU CHẠY API TEST (POSTMAN)" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan

# Chạy Newman nhưng chỉ lọc 4 folder của bạn
$postmanCollection = "$rootDir\postman\WastePlatform API - Professional QA Suite.postman_collection.json"
$postmanEnv = "$rootDir\postman\WastePlatform.professional.postman_environment.json"

newman run "$postmanCollection" -e "$postmanEnv" `
    --folder "01 - Auth" `
    --folder "07 - Collector API" `
    --folder "08 - Collector Tasks" `
    --folder "09 - Enterprise API" `
    -r allure `
    --reporter-allure-export "$resultsDir"

Write-Host "================================================" -ForegroundColor Green
Write-Host "2. BẮT ĐẦU CHẠY UNIT TEST (C# xUnit)" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green

# Thiết lập biến môi trường để ép Allure.Xunit ghi kết quả vào chung 1 thư mục với Postman
$env:ALLURE_RESULTS_DIR = $resultsDir

# Chạy dotnet test, dùng bộ lọc FullyQualifiedName để chỉ lấy các file chứa từ khóa Auth, Collector, Enterprise
# Bạn có thể sửa chuỗi bộ lọc này nếu tên file của bạn khác nhé.
$xunitProject = "$rootDir\backend\tests\WastePlatform.Tests\WastePlatform.Tests.csproj"
$runSettings = "$rootDir\backend\tests\WastePlatform.Tests\WastePlatform.Tests.runsettings"
$xunitAllureDir = "$rootDir\backend\tests\WastePlatform.Tests\bin\Release\net8.0\allure-results"

# Dọn dẹp thư mục kết quả cũ của xUnit để tránh bị trộn lẫn test của người khác
if (Test-Path $xunitAllureDir) {
    Remove-Item -Path "$xunitAllureDir\*" -Recurse -Force
}

dotnet test "$xunitProject" `
    --configuration Release `
    --settings "$runSettings" `
    --filter "FullyQualifiedName~Auth|FullyQualifiedName~Collector|FullyQualifiedName~Enterprise|FullyQualifiedName~Notification"

# Copy xUnit results to the shared results directory
# Copy xUnit results to the shared results directory
if (Test-Path $xunitAllureDir) {
    Copy-Item -Path "$xunitAllureDir\*" -Destination "$resultsDir" -Force -Recurse
}

Write-Host "================================================" -ForegroundColor Yellow
Write-Host "3. TẠO BÁO CÁO ALLURE GỘP CHUNG VÀ MỞ LÊN" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Yellow

# Dùng allure CLI để mở báo cáo lên web (từ web này bạn có thể Print to PDF)
allure serve $resultsDir
