# Flight Board System

## Project Structure

This project consists of:
- **Backend API**: .NET Core Web API (`src/FlightBoard.Api`)
- **Consumer Frontend**: React TypeScript app for public flight display (`src/frontend/flight-board-consumer`)
- **Backoffice Frontend**: React TypeScript app for flight management (`src/frontend/flight-board-backoffice`)

## Setup Instructions

### Prerequisites
- .NET 9.0 SDK
- Node.js (v16+)
- npm or yarn

### Backend API Setup
```bash
cd src
dotnet build
dotnet run --project FlightBoard.Api
```
The API will start on `https://localhost:7000` and `http://localhost:5000`

### Frontend Setup

#### Consumer App (Public Flight Board)
```bash
cd src/frontend/flight-board-consumer
npm install
npm start
```
Runs on `http://localhost:3000`

#### Backoffice App (Admin Interface)
```bash
cd src/frontend/flight-board-backoffice
npm install
npm start
```
Runs on `http://localhost:3001` (if 3000 is taken)

## Development

Follow the implementation guide (`implementation_guide_streamlined.md`) for step-by-step development instructions.

### Current Status
- [x] Project structure created
- [x] .NET solution builds successfully
- [x] React apps created with TypeScript
- [ ] Database foundation (Step 2)
- [ ] Basic API endpoints (Step 3)
- [ ] Frontend integration (Step 4)

## Next Steps
1. Setup Entity Framework and database (Step 2)
2. Create Flight entity and initial migration
3. Build basic CRUD API endpoints
4. Setup Tailwind CSS in frontend apps
5. Create basic flight display interface
