# Create Entity Framework migrations for Vicinia services
Write-Host "Creating Entity Framework migrations for Vicinia services..." -ForegroundColor Green

# Set the working directory to the backend folder
Set-Location "D:\Dev\vicinia\backend"

# Create migrations for User Service
Write-Host "Creating migration for User Service..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate --project "src\Vicinia.UserService" --startup-project "src\Vicinia.UserService"

# Create migrations for History Service
Write-Host "Creating migration for History Service..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate --project "src\Vicinia.HistoryService" --startup-project "src\Vicinia.HistoryService"

# Create migrations for Logging Service
Write-Host "Creating migration for Logging Service..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate --project "src\Vicinia.LoggingService" --startup-project "src\Vicinia.LoggingService"

Write-Host "Migrations created successfully!" -ForegroundColor Green
Write-Host "To apply migrations to the database, run: docker-compose up postgres" -ForegroundColor Cyan
Write-Host "Then run: dotnet ef database update --project src\Vicinia.UserService --startup-project src\Vicinia.UserService" -ForegroundColor Cyan 