# Apply Entity Framework migrations for Vicinia services
Write-Host "Applying Entity Framework migrations for Vicinia services..." -ForegroundColor Green

# Set the working directory to the backend folder
Set-Location "D:\Dev\vicinia\backend"

# Wait for PostgreSQL to be ready
Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Apply migrations for User Service
Write-Host "Applying migration for User Service..." -ForegroundColor Yellow
dotnet ef database update --project "src\Vicinia.UserService" --startup-project "src\Vicinia.UserService"

# Apply migrations for History Service
Write-Host "Applying migration for History Service..." -ForegroundColor Yellow
dotnet ef database update --project "src\Vicinia.HistoryService" --startup-project "src\Vicinia.HistoryService"

# Apply migrations for Logging Service
Write-Host "Applying migration for Logging Service..." -ForegroundColor Yellow
dotnet ef database update --project "src\Vicinia.LoggingService" --startup-project "src\Vicinia.LoggingService"

Write-Host "All migrations applied successfully!" -ForegroundColor Green 