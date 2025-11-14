# Ezra Take-Home Test (Todo Task Management)

Full-stack Todo application with Vue.js frontend and .NET Core backend,
containerized with Docker for easy deployment.

## Quick Start

### Using Docker (Recommended)

```bash
# Build and run the entire application
docker-compose up --build

# Access the application
open http://localhost:8080  # or manually navigate to this address on your browser
```

### Manual Setup

See individual README files in `TodoFrontend/` and `TodoBackend/` directories.

## Design Choices

### Features

- Create, read, update, delete todo items
- Schedule todos with due dates
- Mark todos as complete/incomplete
- Docker containerization
- Health checks and monitoring

### Architecture

- **Frontend**: Vue.js 3 with TypeScript, Element Plus UI
- **Backend**: .NET 9 Web API with Entity Framework Core
- **Database**: SQLite
- **Containerization**: Docker with multi-stage builds

### Project Structure

```
├── TodoFrontend/          # Vue.js frontend application
├── TodoBackend/           # .NET Core Web API
├── Dockerfile            # Multi-stage Docker build
├── docker-compose.yml    # Docker Compose configuration
```

### Development

In a dev setup, please start the backend server first. The `vite.config.ts` of
the frontend has a proxy setup so that the frontend code can call the backend
API without needing CORS enabled.

**Backend Development**

```bash
cd TodoBackend
dotnet run --project src/TodoBackend.API/TodoBackend.API.csproj
```

**Frontend Development**

```bash
cd TodoFrontend
npm install
npm run dev
```

### Production Deployment

The application is production-ready with:

- Multi-stage Docker builds for optimized images
- Non-root user execution for security
- Health checks and monitoring
- Volume persistence for database
- Single container deployment

## Future Work

- User management and auth
- Separate deployment for frontend and backend
