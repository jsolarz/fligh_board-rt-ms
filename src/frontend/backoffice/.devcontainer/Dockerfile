# Use Node.js 20 LTS as base image
FROM node:20-bullseye

# Set working directory
WORKDIR /workspace

# Create non-root user (already exists as 'node')
USER root

# Install additional development tools
RUN apt-get update && apt-get install -y \
    git \
    curl \
    wget \
    vim \
    nano \
    tree \
    jq \
    && rm -rf /var/lib/apt/lists/*

# Install global npm packages for React development
RUN npm install -g \
    @types/node \
    typescript \
    create-react-app \
    npm-check-updates \
    serve

# Set up proper ownership
RUN chown -R node:node /workspace

# Switch back to node user for security
USER node

# Set Node.js environment
ENV NODE_ENV=development
ENV CHOKIDAR_USEPOLLING=true
ENV FAST_REFRESH=true

# Expose port for the development server
EXPOSE 3001

# Default command
CMD ["npm", "start"]
