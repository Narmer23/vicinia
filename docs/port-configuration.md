# Port Configuration Guide

This document explains how the Vicinia application handles port configuration for different environments.

## Service Ports

### Development Mode (dotnet run)
When running services locally with `dotnet run`, the following ports are used:

| Service | HTTP Port | HTTPS Port |
|---------|-----------|------------|
| API Gateway | 5000 | 5001 |
| User Service | 5002 | 7002 |
| Scoring Service | 5003 | 7003 |
| Geocoding Service | 5004 | 7004 |
| POI Service | 5005 | 7005 |
| History Service | 5006 | 7006 |
| Logging Service | 5007 | 7007 |

### Docker Mode (docker-compose)
When running services in Docker containers, the following ports are exposed:

| Service | External Port | Internal Port |
|---------|---------------|---------------|
| API Gateway | 5000 | 80 |
| User Service | 5002 | 80 |
| Scoring Service | 5003 | 80 |
| Geocoding Service | 5004 | 80 |
| POI Service | 5005 | 80 |
| History Service | 5006 | 80 |
| Logging Service | 5007 | 80 |

## Configuration System

### Frontend Configuration

The frontend uses a comprehensive configuration system located in `frontend/src/config/index.ts` that automatically detects the environment and provides URLs for all services:

#### Development Configuration
```typescript
const developmentConfig: AppConfig = {
  api: {
    baseUrl: 'http://localhost:5000',
    timeout: 10000
  },
  services: {
    geocodingService: 'http://localhost:5004',
    poiService: 'http://localhost:5005',
    scoringService: 'http://localhost:5003',
    userService: 'http://localhost:5002',
    historyService: 'http://localhost:5006',
    loggingService: 'http://localhost:5007'
  },
  environment: 'development',
  isDevelopment: true,
  isDocker: false
};
```

#### Docker Configuration
```typescript
const dockerConfig: AppConfig = {
  api: {
    baseUrl: 'http://api-gateway:80',
    timeout: 10000
  },
  services: {
    geocodingService: 'http://geocoding-service:80',
    poiService: 'http://poi-service:80',
    scoringService: 'http://scoring-service:80',
    userService: 'http://user-service:80',
    historyService: 'http://history-service:80',
    loggingService: 'http://logging-service:80'
  },
  environment: 'docker',
  isDevelopment: false,
  isDocker: true
};
```

#### Production Configuration
```typescript
const productionConfig: AppConfig = {
  api: {
    baseUrl: process.env.VUE_APP_API_BASE_URL || 'http://localhost:5000',
    timeout: 15000
  },
  services: {
    geocodingService: process.env.VUE_APP_GEOCODING_SERVICE_URL || 'http://localhost:5004',
    poiService: process.env.VUE_APP_POI_SERVICE_URL || 'http://localhost:5005',
    scoringService: process.env.VUE_APP_SCORING_SERVICE_URL || 'http://localhost:5003',
    userService: process.env.VUE_APP_USER_SERVICE_URL || 'http://localhost:5002',
    historyService: process.env.VUE_APP_HISTORY_SERVICE_URL || 'http://localhost:5006',
    loggingService: process.env.VUE_APP_LOGGING_SERVICE_URL || 'http://localhost:5007'
  },
  environment: 'production',
  isDevelopment: false,
  isDocker: false
};
```

The environment is determined by:
- `NODE_ENV` environment variable
- `VUE_APP_IS_DOCKER` environment variable (set to `true` in Docker builds)

### Backend Configuration

Each backend service includes a `ServiceUrlResolver` that automatically determines the correct URLs for inter-service communication:

- **Development**: Uses localhost URLs with specific ports
- **Docker**: Uses internal Docker network service names

#### Service URL Resolution

The `ServiceUrlResolver` checks the `appsettings.json` configuration:

```json
{
  "Services": {
    "UserService": {
      "Development": "http://localhost:5002",
      "Docker": "http://user-service:80"
    }
  }
}
```

If a service is not configured, it falls back to default port mapping.

## Environment Detection

### Frontend
- **Development**: `npm run dev` → Uses development config
- **Docker**: `VUE_APP_IS_DOCKER=true` → Uses Docker config
- **Production**: `NODE_ENV=production` → Uses production config

### Backend
- **Development**: `ASPNETCORE_ENVIRONMENT=Development` → Uses localhost URLs
- **Docker**: `ASPNETCORE_ENVIRONMENT=Production` → Uses Docker service names

## Usage Examples

### Frontend API Calls
```typescript
import { api } from 'src/boot/axios'
import { servicesConfig } from 'src/config'

// Using the main API (goes through API Gateway)
const response = await api.get('/api/users')

// Direct service calls (when needed)
const geocodingResponse = await fetch(`${servicesConfig.geocodingService}/api/geocoding/geocode?address=${address}`)
const poiResponse = await fetch(`${servicesConfig.poiService}/api/poi/search`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(data)
})
const scoringResponse = await fetch(`${servicesConfig.scoringService}/api/scoring/calculate`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(data)
})
```

### Backend Service Communication
```csharp
public class MyService
{
    private readonly IServiceUrlResolver _serviceUrlResolver;
    
    public MyService(IServiceUrlResolver serviceUrlResolver)
    {
        _serviceUrlResolver = serviceUrlResolver;
    }
    
    public async Task CallOtherService()
    {
        var userServiceUrl = _serviceUrlResolver.GetServiceUrl("UserService");
        // Automatically resolves to correct URL based on environment
    }
}
```

## No Hardcoded URLs

✅ **Frontend**: All hardcoded localhost URLs have been removed from Vue components
✅ **Backend**: All hardcoded URLs have been replaced with the ServiceUrlResolver
✅ **Configuration**: All URLs are now managed through the configuration system
✅ **Environment Awareness**: Automatic detection and switching between environments

## Starting the Application

### Development Mode
```powershell
# Run the development startup script
./scripts/start-dev.ps1
```

This will start all services on their development ports.

### Docker Mode
```powershell
# Start all services in Docker
docker-compose up -d
```

This will start all services in Docker containers with the configured port mappings.

## Health Checks

Each service exposes a health check endpoint at `/health`:

- API Gateway: `http://localhost:5000/health`
- User Service: `http://localhost:5002/health`
- Scoring Service: `http://localhost:5003/health`
- Geocoding Service: `http://localhost:5004/health`
- POI Service: `http://localhost:5005/health`
- History Service: `http://localhost:5006/health`
- Logging Service: `http://localhost:5007/health`

## Troubleshooting

### Port Conflicts
If you encounter port conflicts:

1. Check which ports are in use:
   ```powershell
   netstat -an | findstr LISTENING
   ```

2. Stop conflicting services or modify the port configuration in the respective `launchSettings.json` files.

### Service Communication Issues
If services can't communicate:

1. Verify the environment is correctly detected
2. Check the service URLs in the configuration
3. Ensure all required services are running
4. Check Docker network connectivity (if using Docker)

### Frontend API Issues
If the frontend can't reach the API:

1. Verify the correct configuration is loaded
2. Check the browser console for configuration details
3. Ensure the API Gateway is running and accessible
4. Check CORS configuration if needed

### Configuration Debugging
To debug configuration issues:

1. Check the browser console for the loaded configuration:
   ```javascript
   console.log('Environment:', import.meta.env.NODE_ENV)
   console.log('Is Docker:', import.meta.env.VUE_APP_IS_DOCKER)
   console.log('Config:', import('src/config').then(c => console.log(c.config)))
   ```

2. Verify environment variables are set correctly in Docker:
   ```bash
   docker exec -it vicinia-frontend env | grep VUE_APP
   ``` 