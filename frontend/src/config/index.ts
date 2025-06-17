export interface ApiConfig {
  baseUrl: string;
  timeout: number;
}

export interface ServiceConfig {
  geocodingService: string;
  poiService: string;
  scoringService: string;
  userService: string;
  historyService: string;
  loggingService: string;
}

export interface AppConfig {
  api: ApiConfig;
  services: ServiceConfig;
  environment: string;
  isDevelopment: boolean;
  isDocker: boolean;
}

// Development configuration (when running with npm run dev)
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

// Docker configuration (when running in Docker containers)
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

// Production configuration (when deployed)
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

// Determine which configuration to use
function getConfig(): AppConfig {
  const env = process.env.NODE_ENV;
  const isDocker = process.env.VUE_APP_IS_DOCKER === 'true';
  
  if (isDocker) {
    return dockerConfig;
  }
  
  switch (env) {
    case 'development':
      return developmentConfig;
    case 'production':
      return productionConfig;
    default:
      return developmentConfig;
  }
}

export const config = getConfig();

// Export individual configs for direct access
export const apiConfig = config.api;
export const servicesConfig = config.services;
export const isDevelopment = config.isDevelopment;
export const isDocker = config.isDocker;
export const environment = config.environment; 