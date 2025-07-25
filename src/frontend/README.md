# Frontend Applications

**React TypeScript Frontend Applications**

This folder contains two distinct React applications with different themes and purposes, both built with modern React, TypeScript, and state management.

## Applications

### consumer/
- **Theme**: Cyberpunk/futuristic styling (Blade Runner inspired)
- **Purpose**: Public flight information display
- **Users**: Passengers, general public
- **Features**: Real-time flight board, search/filtering, responsive design
- **Styling**: Neon colors, holographic effects, cyberpunk aesthetics

### backoffice/
- **Theme**: Retro BBS terminal styling (80s/90s bulletin board system)
- **Purpose**: Administrative flight management interface  
- **Users**: Airport operators, administrators
- **Features**: Flight CRUD operations, management tools, admin controls
- **Styling**: Green-on-black terminal, monospace fonts, retro computing

## Technology Stack

### Core Technologies
- **React 18** - Modern React with hooks and concurrent features
- **TypeScript** - Type-safe development with strict typing
- **Vite** - Fast build tool and development server
- **TanStack Query** - Server state management and caching

### State Management
- **Redux Toolkit** - Global state management
- **React Query** - Server state and caching
- **React Hook Form** - Form state and validation

### Real-time Communication
- **@microsoft/signalr** - Real-time updates from backend
- **SignalR integration** - Live flight status updates
- **Connection management** - Automatic reconnection and error handling

### Styling & UI
- **Tailwind CSS** - Utility-first CSS framework
- **Custom CSS** - Theme-specific styling and animations
- **Responsive design** - Mobile-first responsive layouts

## Shared Features

- **JWT Authentication** - Token-based authentication
- **Real-time updates** - Live data via SignalR
- **Search & filtering** - Advanced flight search capabilities
- **Error handling** - Comprehensive error states and retry logic
- **Loading states** - Smooth loading indicators and skeletons
- **Type safety** - Full TypeScript coverage

## Development

Each application has its own:
- **Package.json** - Independent dependency management
- **Development server** - Separate ports (3000/3001)
- **Build configuration** - Optimized production builds
- **Environment configuration** - Environment-specific settings
