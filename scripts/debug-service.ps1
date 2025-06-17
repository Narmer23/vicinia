# Vicinia Service Debugging Script

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("api-gateway", "user-service", "scoring-service", "geocoding-service", "poi-service", "history-service", "logging-service")]
    [string]$Service,
    
    [Parameter(Mandatory=$false)]
    [switch]$Attach,
    
    [Parameter(Mandatory=$false)]
    [switch]$Watch,
    
    [Parameter(Mandatory=$false)]
    [switch]$Verbose
)

# Service configuration
$services = @{
    "api-gateway" = @{
        "name" = "API Gateway"
        "project" = "Vicinia.ApiGateway"
        "port" = 5000
        "processName" = "Vicinia.ApiGateway"
    }
    "user-service" = @{
        "name" = "User Service"
        "project" = "Vicinia.UserService"
        "port" = 5002
        "processName" = "Vicinia.UserService"
    }
    "scoring-service" = @{
        "name" = "Scoring Service"
        "project" = "Vicinia.ScoringService"
        "port" = 5003
        "processName" = "Vicinia.ScoringService"
    }
    "geocoding-service" = @{
        "name" = "Geocoding Service"
        "project" = "Vicinia.GeocodingService"
        "port" = 5004
        "processName" = "Vicinia.GeocodingService"
    }
    "poi-service" = @{
        "name" = "POI Service"
        "project" = "Vicinia.PoiService"
        "port" = 5005
        "processName" = "Vicinia.PoiService"
    }
    "history-service" = @{
        "name" = "History Service"
        "project" = "Vicinia.HistoryService"
        "port" = 5006
        "processName" = "Vicinia.HistoryService"
    }
    "logging-service" = @{
        "name" = "Logging Service"
        "project" = "Vicinia.LoggingService"
        "port" = 5007
        "processName" = "Vicinia.LoggingService"
    }
}

$selectedService = $services[$Service]
$projectPath = "backend/src/$($selectedService.project)"

Write-Host "Debugging $($selectedService.name)..." -ForegroundColor Green

# Check if project exists
if (-not (Test-Path $projectPath)) {
    Write-Host "Error: Project not found at $projectPath" -ForegroundColor Red
    exit 1
}

# Change to backend directory
Set-Location backend

if ($Attach) {
    Write-Host "Attaching debugger to running $($selectedService.name)..." -ForegroundColor Yellow
    
    # Find running process
    $processes = Get-Process -Name $selectedService.processName -ErrorAction SilentlyContinue
    
    if ($processes.Count -eq 0) {
        Write-Host "No running $($selectedService.name) process found." -ForegroundColor Red
        Write-Host "Please start the service first using: dotnet run --project src/$($selectedService.project)" -ForegroundColor Yellow
        exit 1
    }
    
    if ($processes.Count -gt 1) {
        Write-Host "Multiple $($selectedService.name) processes found:" -ForegroundColor Yellow
        for ($i = 0; $i -lt $processes.Count; $i++) {
            Write-Host "  $i: PID $($processes[$i].Id) - Started: $($processes[$i].StartTime)" -ForegroundColor White
        }
        $choice = Read-Host "Select process (0-$($processes.Count-1))"
        $selectedProcess = $processes[[int]$choice]
    } else {
        $selectedProcess = $processes[0]
    }
    
    Write-Host "Attaching to process PID: $($selectedProcess.Id)" -ForegroundColor Green
    
    # Launch VS Code with attach configuration
    $launchConfig = "Attach to $($selectedService.name)"
    code --new-window --goto "$PSScriptRoot/../.vscode/launch.json:1"
    
    Write-Host "VS Code opened. Please select '$launchConfig' from the debug menu and start debugging." -ForegroundColor Cyan
    
} else {
    # Start service with debugging
    Write-Host "Starting $($selectedService.name) in debug mode..." -ForegroundColor Yellow
    
    if ($Verbose) {
        Write-Host "Project Path: $projectPath" -ForegroundColor Gray
        Write-Host "Port: $($selectedService.port)" -ForegroundColor Gray
        Write-Host "Process Name: $($selectedService.processName)" -ForegroundColor Gray
    }
    
    # Set environment variables for debugging
    $env:ASPNETCORE_ENVIRONMENT = "Development"
    $env:ASPNETCORE_URLS = "http://localhost:$($selectedService.port)"
    
    if ($Watch) {
        Write-Host "Starting with watch mode (auto-restart on changes)..." -ForegroundColor Cyan
        dotnet watch run --project "src/$($selectedService.project)"
    } else {
        Write-Host "Starting in debug mode..." -ForegroundColor Cyan
        Write-Host "Service will be available at: http://localhost:$($selectedService.port)" -ForegroundColor Green
        Write-Host "Swagger UI: http://localhost:$($selectedService.port)/swagger" -ForegroundColor Green
        Write-Host "Health Check: http://localhost:$($selectedService.port)/health" -ForegroundColor Green
        Write-Host ""
        Write-Host "To attach a debugger:" -ForegroundColor Yellow
        Write-Host "  1. Open VS Code" -ForegroundColor White
        Write-Host "  2. Go to Run and Debug (Ctrl+Shift+D)" -ForegroundColor White
        Write-Host "  3. Select 'Attach to $($selectedService.name)'" -ForegroundColor White
        Write-Host "  4. Start debugging (F5)" -ForegroundColor White
        Write-Host ""
        Write-Host "Press Ctrl+C to stop the service" -ForegroundColor Yellow
        
        dotnet run --project "src/$($selectedService.project)"
    }
}

# Return to original directory
Set-Location .. 