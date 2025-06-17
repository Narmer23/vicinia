# Vicinia Development Startup Script

Write-Host "Starting Vicinia Development Environment..." -ForegroundColor Green

# Check if Docker is running
Write-Host "Checking Docker status..." -ForegroundColor Yellow
try {
    docker version | Out-Null
    Write-Host "Docker is running" -ForegroundColor Green
} catch {
    Write-Host "Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    exit 1
}

# Start infrastructure services
Write-Host "Starting infrastructure services (PostgreSQL, Redis)..." -ForegroundColor Yellow
docker-compose up -d postgres redis

# Wait for services to be ready
Write-Host "Waiting for services to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Start backend services
Write-Host "Starting backend services..." -ForegroundColor Yellow
cd backend
dotnet restore
Start-Job -ScriptBlock { dotnet run --project src/Vicinia.ApiGateway }
Start-Job -ScriptBlock { dotnet run --project src/Vicinia.UserService }
Start-Job -ScriptBlock { dotnet run --project src/Vicinia.ScoringService }
Start-Job -ScriptBlock { dotnet run --project src/Vicinia.GeocodingService }
Start-Job -ScriptBlock { dotnet run --project src/Vicinia.PoiService }
Start-Job -ScriptBlock { dotnet run --project src/Vicinia.HistoryService }
Start-Job -ScriptBlock { dotnet run --project src/Vicinia.LoggingService }

# Start frontend
Write-Host "Starting frontend..." -ForegroundColor Yellow
cd ../frontend
npm run dev

Write-Host "Development environment started!" -ForegroundColor Green
Write-Host "Frontend: http://localhost:9000" -ForegroundColor Cyan
Write-Host "API Gateway: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Swagger: http://localhost:5000/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "Backend Service Ports:" -ForegroundColor Yellow
Write-Host "  API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "  User Service: http://localhost:5002" -ForegroundColor White
Write-Host "  Scoring Service: http://localhost:5003" -ForegroundColor White
Write-Host "  Geocoding Service: http://localhost:5004" -ForegroundColor White
Write-Host "  POI Service: http://localhost:5005" -ForegroundColor White
Write-Host "  History Service: http://localhost:5006" -ForegroundColor White
Write-Host "  Logging Service: http://localhost:5007" -ForegroundColor White
Write-Host ""
Write-Host "Health Checks:" -ForegroundColor Yellow
Write-Host "  API Gateway: http://localhost:5000/health" -ForegroundColor White
Write-Host "  User Service: http://localhost:5002/health" -ForegroundColor White
Write-Host "  Scoring Service: http://localhost:5003/health" -ForegroundColor White
Write-Host "  Geocoding Service: http://localhost:5004/health" -ForegroundColor White
Write-Host "  POI Service: http://localhost:5005/health" -ForegroundColor White
Write-Host "  History Service: http://localhost:5006/health" -ForegroundColor White
Write-Host "  Logging Service: http://localhost:5007/health" -ForegroundColor White 