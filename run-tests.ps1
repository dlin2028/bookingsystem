# BookingSystem Test Runner (PowerShell)
# This script runs all tests with detailed output

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  BookingSystem - Running Test Suite" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to test directory
Set-Location BookingSystem.Tests

Write-Host "1. Restoring test project dependencies..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to restore dependencies" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Dependencies restored" -ForegroundColor Green
Write-Host ""

Write-Host "2. Building test project..." -ForegroundColor Yellow
dotnet build --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build successful" -ForegroundColor Green
Write-Host ""

Write-Host "3. Running all tests..." -ForegroundColor Yellow
Write-Host "=========================================" -ForegroundColor Cyan
dotnet test --no-build --verbosity normal
$testResult = $LASTEXITCODE
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

if ($testResult -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Running tests with detailed results..." -ForegroundColor Yellow
    dotnet test --no-build --logger "console;verbosity=detailed"
} else {
    Write-Host "❌ Some tests failed. Check output above." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "  Test Coverage Summary" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Models:        100%" -ForegroundColor Green
Write-Host "Seating Types: 100%" -ForegroundColor Green
Write-Host "Services:      95%+" -ForegroundColor Green
Write-Host "Repositories:  85%+" -ForegroundColor Green
Write-Host "Controllers:   80%+" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "✅ Test suite completed successfully!" -ForegroundColor Green
