# Backend Service Debugging Guide

This guide explains how to debug individual backend services in the Vicinia application using various methods and tools.

## Available Services

| Service | Port | Project Path | Process Name |
|---------|------|--------------|--------------|
| API Gateway | 5000 | `backend/src/Vicinia.ApiGateway` | `Vicinia.ApiGateway` |
| User Service | 5002 | `backend/src/Vicinia.UserService` | `Vicinia.UserService` |
| Scoring Service | 5003 | `backend/src/Vicinia.ScoringService` | `Vicinia.ScoringService` |
| Geocoding Service | 5004 | `backend/src/Vicinia.GeocodingService` | `Vicinia.GeocodingService` |
| POI Service | 5005 | `backend/src/Vicinia.PoiService` | `Vicinia.PoiService` |
| History Service | 5006 | `backend/src/Vicinia.HistoryService` | `Vicinia.HistoryService` |
| Logging Service | 5007 | `backend/src/Vicinia.LoggingService` | `Vicinia.LoggingService` |

## Method 1: VS Code Debugging (Recommended)

### Prerequisites
- Visual Studio Code with C# extension installed
- .NET 9.0 SDK installed

### Launch Configuration
VS Code launch configurations are available in `.vscode/launch.json`:

#### Direct Launch (Start and Debug)
- **Debug API Gateway**: Launches API Gateway with debugger attached
- **Debug User Service**: Launches User Service with debugger attached
- **Debug Scoring Service**: Launches Scoring Service with debugger attached
- **Debug Geocoding Service**: Launches Geocoding Service with debugger attached
- **Debug POI Service**: Launches POI Service with debugger attached
- **Debug History Service**: Launches History Service with debugger attached
- **Debug Logging Service**: Launches Logging Service with debugger attached

#### Attach to Running Process
- **Attach to API Gateway**: Attaches debugger to running API Gateway
- **Attach to User Service**: Attaches debugger to running User Service
- **Attach to Scoring Service**: Attaches debugger to running Scoring Service
- **Attach to Geocoding Service**: Attaches debugger to running Geocoding Service
- **Attach to POI Service**: Attaches debugger to running POI Service
- **Attach to History Service**: Attaches debugger to running History Service
- **Attach to Logging Service**: Attaches debugger to running Logging Service

### Steps to Debug

1. **Open VS Code** in the project root directory
2. **Go to Run and Debug** (Ctrl+Shift+D)
3. **Select the desired configuration** from the dropdown
4. **Set breakpoints** in your code
5. **Start debugging** (F5)

### Example: Debugging Scoring Service
```bash
# 1. Open VS Code
code .

# 2. Set breakpoints in ScoringService code
# 3. Select "Debug Scoring Service" from Run and Debug
# 4. Press F5 to start debugging
# 5. Service will be available at http://localhost:5003
```

## Method 2: PowerShell Scripts

### Find Running Services
```powershell
# Find all running Vicinia services and their PIDs
./scripts/find-services.ps1
```

This script will:
- List all running Vicinia services
- Show process IDs, start times, and memory usage
- Display which ports are in use
- Provide instructions for attaching debuggers

### Debug Individual Service
```powershell
# Start a specific service in debug mode
./scripts/debug-service.ps1 -Service scoring-service

# Start with watch mode (auto-restart on changes)
./scripts/debug-service.ps1 -Service scoring-service -Watch

# Attach debugger to running service
./scripts/debug-service.ps1 -Service scoring-service -Attach

# Verbose output with additional information
./scripts/debug-service.ps1 -Service scoring-service -Verbose
```

### Available Service Names
- `api-gateway`
- `user-service`
- `scoring-service`
- `geocoding-service`
- `poi-service`
- `history-service`
- `logging-service`

## Method 3: Command Line

### Direct dotnet Commands
```bash
# Navigate to backend directory
cd backend

# Start service in debug mode
dotnet run --project src/Vicinia.ScoringService

# Start with watch mode
dotnet watch run --project src/Vicinia.ScoringService

# Build specific service
dotnet build src/Vicinia.ScoringService

# Build all services
dotnet build Vicinia.sln
```

### Environment Variables
```bash
# Set environment for debugging
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5003"

# Start service
dotnet run --project src/Vicinia.ScoringService
```

## Method 4: Docker Debugging

### Debug Services Running in Docker
```bash
# Start services in Docker
docker-compose up -d

# Find container ID
docker ps

# Attach to container for debugging
docker exec -it vicinia-scoring-service /bin/bash

# View logs
docker logs vicinia-scoring-service

# View logs with follow
docker logs -f vicinia-scoring-service
```

### Remote Debugging with Docker
For advanced Docker debugging, you can expose debugging ports:

```yaml
# Add to docker-compose.yml for debugging
services:
  scoring-service:
    ports:
      - "5003:80"
      - "5008:5008"  # Debug port
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - DOTNET_USE_POLLING_FILE_WATCHER=1
```

## Method 5: Visual Studio

### Open Solution in Visual Studio
```bash
# Open the solution file
start backend/Vicinia.sln
```

### Set Startup Project
1. Right-click on the desired service project
2. Select "Set as Startup Project"
3. Press F5 to start debugging

## Debugging Tips

### 1. Set Breakpoints
- **Line Breakpoints**: Click in the gutter next to line numbers
- **Conditional Breakpoints**: Right-click breakpoint → Edit Breakpoint
- **Function Breakpoints**: Debug → New Breakpoint → Function Breakpoint

### 2. Use Debug Console
- **Variables**: Inspect local variables and watch expressions
- **Call Stack**: Navigate through the call stack
- **Breakpoints**: Manage all breakpoints

### 3. Hot Reload
```bash
# Use watch mode for automatic restarts
dotnet watch run --project src/Vicinia.ScoringService
```

### 4. Logging
All services use Serilog for structured logging:
```csharp
_logger.LogInformation("Processing request for user {UserId}", userId);
_logger.LogError(ex, "Error processing request");
```

### 5. Health Checks
Each service exposes a health check endpoint:
- API Gateway: `http://localhost:5000/health`
- User Service: `http://localhost:5002/health`
- Scoring Service: `http://localhost:5003/health`
- etc.

### 6. Swagger UI
Most services expose Swagger documentation:
- API Gateway: `http://localhost:5000/swagger`
- User Service: `http://localhost:5002/swagger`
- Scoring Service: `http://localhost:5003/swagger`
- etc.

## Troubleshooting

### Common Issues

#### 1. Port Already in Use
```powershell
# Check what's using the port
netstat -ano | findstr :5003

# Kill the process
taskkill /PID <process-id> /F
```

#### 2. Service Won't Start
```bash
# Check if .NET SDK is installed
dotnet --version

# Restore packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

#### 3. Debugger Won't Attach
```powershell
# Find the process
./scripts/find-services.ps1

# Make sure the service is running
dotnet run --project src/Vicinia.ScoringService
```

#### 4. Breakpoints Not Hit
- Ensure you're running in Debug configuration
- Check that the source files match the running code
- Verify the service is actually running the code you're debugging

### Performance Debugging

#### Memory Usage
```powershell
# Monitor memory usage
Get-Process -Name "Vicinia.ScoringService" | Select-Object ProcessName, WorkingSet64, CPU
```

#### CPU Usage
```powershell
# Monitor CPU usage over time
while ($true) {
    $process = Get-Process -Name "Vicinia.ScoringService" -ErrorAction SilentlyContinue
    if ($process) {
        Write-Host "$(Get-Date): CPU: $($process.CPU), Memory: $([math]::Round($process.WorkingSet64 / 1MB, 2)) MB"
    }
    Start-Sleep -Seconds 5
}
```

## Advanced Debugging

### Remote Debugging
For debugging services running on different machines:

1. **Configure remote debugging** in launch.json
2. **Set up port forwarding** if needed
3. **Use remote attach** configuration

### Multi-Service Debugging
To debug multiple services simultaneously:

1. **Start services** using the development script
2. **Attach debuggers** to each service
3. **Set breakpoints** in all services
4. **Test the full flow**

### Database Debugging
For services with database access:

```csharp
// Add logging to see SQL queries
services.AddDbContext<MyDbContext>(options =>
    options.UseNpgsql(connectionString)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());
```

## Best Practices

1. **Use meaningful breakpoints** - Don't set too many at once
2. **Log important information** - Use structured logging
3. **Test with realistic data** - Use production-like test data
4. **Monitor performance** - Watch memory and CPU usage
5. **Use conditional breakpoints** - Only break when specific conditions are met
6. **Keep debugging sessions short** - Restart services regularly

## Quick Reference

### Start Debugging
```powershell
# VS Code
code . → Ctrl+Shift+D → Select configuration → F5

# PowerShell
./scripts/debug-service.ps1 -Service scoring-service

# Command Line
cd backend
dotnet run --project src/Vicinia.ScoringService
```

### Find Services
```powershell
./scripts/find-services.ps1
```

### Health Checks
```bash
curl http://localhost:5003/health
```

### Swagger UI
```
http://localhost:5003/swagger
``` 