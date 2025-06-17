# Build stage
FROM node:18-alpine as build-stage

WORKDIR /app

# Copy entire project first
COPY . .

# Install dependencies
RUN npm ci

# Set environment variable for Docker build
ENV VUE_APP_IS_DOCKER=true

# Build the app
RUN npm run build

# Production stage
FROM nginx:alpine as production-stage

# Copy built files from build stage
COPY --from=build-stage /app/dist/spa /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"] 