# Vicinia Service Discovery Script

Write-Host "Searching for running Vicinia services..." -ForegroundColor Green
Write-Host ""

# Service names to search for
$serviceNames = @(
    "Vicinia.ApiGateway",
    "Vicinia.UserService", 
    "Vicinia.ScoringService",
    "Vicinia.GeocodingService",
    "Vicinia.PoiService",
    "Vicinia.HistoryService",
    "Vicinia.LoggingService"
)

$foundServices = @()

foreach ($serviceName in $serviceNames) {
    $processes = Get-Process -Name $serviceName -ErrorAction SilentlyContinue
    
    if ($processes.Count -gt 0) {
        foreach ($process in $processes) {
            $foundServices += @{
                "Name" = $serviceName
                "PID" = $process.Id
                "StartTime" = $process.StartTime
                "Memory" = [math]::Round($process.WorkingSet64 / 1MB, 2)
                "CPU" = $process.CPU
            }
        }
    }
}

if ($foundServices.Count -eq 0) {
    Write-Host "No running Vicinia services found." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To start services for debugging:" -ForegroundColor Cyan
    Write-Host "  ./scripts/debug-service.ps1 -Service <service-name>" -ForegroundColor White
    Write-Host "  ./scripts/start-dev.ps1" -ForegroundColor White
    Write-Host ""
    Write-Host "Available services:" -ForegroundColor Cyan
    Write-Host "  api-gateway, user-service, scoring-service, geocoding-service" -ForegroundColor White
    Write-Host "  poi-service, history-service, logging-service" -ForegroundColor White
} else {
    Write-Host "Found $($foundServices.Count) running service(s):" -ForegroundColor Green
    Write-Host ""
    
    # Display services in a table format
    $foundServices | Format-Table -Property @(
        @{Name="Service"; Expression={$_.Name}},
        @{Name="PID"; Expression={$_.PID}},
        @{Name="Start Time"; Expression={$_.StartTime.ToString("HH:mm:ss")}},
        @{Name="Memory (MB)"; Expression={$_.Memory}},
        @{Name="CPU Time"; Expression={$_.CPU}}
    ) -AutoSize
    
    Write-Host ""
    Write-Host "To attach debugger to a specific service:" -ForegroundColor Cyan
    Write-Host "  ./scripts/debug-service.ps1 -Service <service-name> -Attach" -ForegroundColor White
    Write-Host ""
    Write-Host "Or use VS Code:" -ForegroundColor Cyan
    Write-Host "  1. Open VS Code" -ForegroundColor White
    Write-Host "  2. Go to Run and Debug (Ctrl+Shift+D)" -ForegroundColor White
    Write-Host "  3. Select 'Attach to <Service Name>'" -ForegroundColor White
    Write-Host "  4. Start debugging (F5)" -ForegroundColor White
}

Write-Host ""
Write-Host "Checking ports in use by Vicinia services..." -ForegroundColor Green

# Check which ports are in use
$viciniaPorts = @(5000, 5002, 5003, 5004, 5005, 5006, 5007)
$portMapping = @{
    5000 = "API Gateway"
    5002 = "User Service"
    5003 = "Scoring Service"
    5004 = "Geocoding Service"
    5005 = "POI Service"
    5006 = "History Service"
    5007 = "Logging Service"
}

$usedPorts = @()

foreach ($port in $viciniaPorts) {
    $connections = netstat -ano | Select-String ":$port\s" | Select-String "LISTENING"
    
    if ($connections) {
        foreach ($connection in $connections) {
            $parts = $connection -split '\s+'
            $pid = $parts[-1]
            
            try {
                $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                $processName = $process.ProcessName
            } catch {
                $processName = "Unknown"
            }
            
            $usedPorts += @{
                "Port" = $port
                "Service" = $portMapping[$port]
                "PID" = $pid
                "Process" = $processName
            }
        }
    }
}

if ($usedPorts.Count -gt 0) {
    Write-Host "Ports in use by Vicinia services:" -ForegroundColor Green
    $usedPorts | Format-Table -Property @(
        @{Name="Port"; Expression={$_.Port}},
        @{Name="Service"; Expression={$_.Service}},
        @{Name="PID"; Expression={$_.PID}},
        @{Name="Process"; Expression={$_.Process}}
    ) -AutoSize
} else {
    Write-Host "No Vicinia service ports are currently in use." -ForegroundColor Yellow
} 