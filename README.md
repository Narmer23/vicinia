# Vicinia - Location Scoring Application

A scalable, maintainable web and mobile application that scores locations in Lombardy, Italy, based on proximity to Points of Interest (POIs) like schools, hospitals, supermarkets, and more.

## 🎯 Purpose

The application provides location-based scoring by analyzing proximity to essential amenities, helping users make informed decisions about locations in the Lombardy region.

## 🏗️ Architecture Overview

### Frontend
- **Framework**: Quasar Framework (Vue.js 3)
- **Map**: Leaflet with OpenStreetMap
- **UI**: Material Design components
- **Languages**: English and Italian support

### Backend Microservices
- **API Gateway**: Entry point for all client requests
- **User Service**: Authentication and user management
- **Scoring Service**: Core business logic for location scoring
- **Geocoding Service**: Address to coordinates conversion
- **POI Service**: Points of Interest data management
- **History Service**: Search history tracking
- **Logging Service**: Centralized logging

### Data Layer
- **Database**: PostgreSQL with separate databases per service
- **ORM**: Entity Framework Core
- **Caching**: Redis
- **Containerization**: Docker & Docker Compose

## 🚀 Features

### Core Functionality
- **Address Search**: Manual address input with geocoding
- **Transportation Modes**: Car and walking distance calculations
- **Dynamic Scoring**: 1-10 score based on POI proximity
- **Real-time POI Data**: Integration with Regione Lombardia OpenData APIs
- **Interactive Maps**: Map-first interface with POI markers

### User Features
- **Authentication**: Registration and login with email/password
- **Search History**: Store and retrieve user search history
- **Multilingual**: English and Italian support
- **Responsive Design**: Works on web and mobile devices

## 📁 Project Structure

```
vicinia/
├── frontend/                 # Quasar Framework frontend
├── backend/                  # .NET microservices
│   ├── src/
│   │   ├── Vicinia.ApiGateway/
│   │   ├── Vicinia.UserService/
│   │   ├── Vicinia.ScoringService/
│   │   ├── Vicinia.GeocodingService/
│   │   ├── Vicinia.PoiService/
│   │   ├── Vicinia.HistoryService/
│   │   └── Vicinia.LoggingService/
│   └── tests/
├── infrastructure/           # Docker and deployment configs
├── docs/                     # Documentation
└── scripts/                  # Build and deployment scripts
```

## 🛠️ Prerequisites

- .NET 9 SDK
- Node.js 18+ and npm
- Docker and Docker Compose
- PostgreSQL (or use Docker container)
- Git

## 🚀 Quick Start

### 1. Clone the Repository
```bash
git clone <repository-url>
cd vicinia
```

### 2. Start Infrastructure
```bash
docker-compose up -d postgres redis
```

### 3. Run Backend Services
```bash
cd backend
dotnet restore
dotnet run --project src/Vicinia.ApiGateway
```

### 4. Run Frontend
```bash
cd frontend
npm install
npm run dev
```

### 5. Access the Application
- Frontend: http://localhost:9000
- API Gateway: http://localhost:5000
- Swagger Documentation: http://localhost:5000/swagger

## 🔧 Development

### Backend Development
```bash
cd backend
dotnet restore
dotnet build
dotnet test
```

### Frontend Development
```bash
cd frontend
npm install
npm run dev          # Development server
npm run build        # Production build
npm run build:pwa    # PWA build
```

### Database Migrations
```bash
cd backend/src/Vicinia.UserService
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 🐳 Docker Deployment

### Development
```bash
docker-compose up -d
```

### Production
```bash
docker-compose -f docker-compose.prod.yml up -d
```

## 📊 API Documentation

The API documentation is available via Swagger UI at:
- Development: http://localhost:5000/swagger
- Production: https://api.vicinia.com/swagger

## 🌍 Internationalization

The application supports:
- **English** (default)
- **Italian** (auto-detected based on browser settings)

## 🔒 Security

- JWT-based authentication
- HTTPS enforcement in production
- Input validation and sanitization
- Rate limiting on API endpoints
- CORS configuration

## 📈 Monitoring & Logging

- Structured logging with Serilog
- Health check endpoints for all services
- Application Insights integration ready
- Error tracking and monitoring

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the GitHub repository
- Check the documentation in the `/docs` folder
- Review the API documentation via Swagger

## 🔮 Future Enhancements

- GPS support for mobile devices
- Custom scoring per POI type
- POI sharing via links/images
- Optional POI caching
- Third-party routing API integration
- Advanced analytics and reporting 