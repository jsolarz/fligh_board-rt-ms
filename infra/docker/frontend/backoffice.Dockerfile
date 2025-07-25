# Backoffice Frontend Dockerfile
# Build stage
FROM node:18-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
RUN npm ci --only=production

# Copy source code
COPY . .

# Build the application
RUN npm run build

# Production stage
FROM nginx:alpine AS production
WORKDIR /usr/share/nginx/html

# Remove default nginx files
RUN rm -rf ./*

# Copy built application
COPY --from=build /app/build .

# Copy nginx configuration
COPY ../../../infra/docker/nginx/backoffice.nginx.conf /etc/nginx/conf.d/default.conf

# Create non-root user
RUN addgroup -g 1001 -S flightboard && \
    adduser -S -D -H -u 1001 -h /usr/share/nginx/html -s /sbin/nologin -G flightboard -g flightboard flightboard

# Set proper permissions
RUN chown -R flightboard:flightboard /usr/share/nginx/html && \
    chown -R flightboard:flightboard /var/cache/nginx && \
    chown -R flightboard:flightboard /var/log/nginx && \
    chown -R flightboard:flightboard /etc/nginx/conf.d
    
RUN touch /var/run/nginx.pid && \
    chown -R flightboard:flightboard /var/run/nginx.pid

# Switch to non-root user
USER flightboard

# Expose port
EXPOSE 3001

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:3001/ || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
