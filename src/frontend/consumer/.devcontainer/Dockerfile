# Use Node.js 20 LTS
FROM node:20-bullseye

# Install additional tools
RUN apt-get update && apt-get install -y \
    git \
    curl \
    wget \
    && rm -rf /var/lib/apt/lists/*

# Create app directory
WORKDIR /workspace

# Copy package files
COPY src/frontend/consumer/package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY src/frontend/consumer/ ./

# Build the app for production
RUN npm run build

# Expose port
EXPOSE 3000

# Start the application
CMD ["npm", "start"]
