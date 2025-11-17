# Todo Task Management

Full-stack Todo application with Vue.js frontend and .NET 9 backend,
containerized with Docker for easy deployment. Features user authentication
task management with scheduling capabilities.

## Important AI Usage Note

Much of the code in this repo is written with AI coding assistance tools.
The primary tools used are Kimi CLI and Claude Code.

The tools were used under interactive supervision and the code generated was
carefully reviewed by the author.

.NET is an unfamiliar stack for me, who has primarily worked as a Python
backend developer in web development context. Vue.js has also not been within my
job's purview recently. The use of AI tools allowed me to produce a
feature-rich application with high-quality code, within a reasonable amount of
time.

## Quick Start

```bash
# Build and run the entire application
docker-compose up --build

# Access the application
open http://localhost:8080  # Application will be available at this address
```

To try without docker, see the [Design Choices/Development](#development).

## Design Choices

### Features

- User registration and authentication
- Create, read, update, delete todo items
- Schedule todos with scheduled and due dates
- Mark todos as complete/incomplete
- Filter todos by status (all, incomplete, complete, overdue)
- Docker containerization
- Health checks and monitoring
- Clean architecture with separation of concerns

### Architecture

- **Frontend**: Vue.js 3 with TypeScript, Element Plus UI
- **Backend**: .NET 9 Web API with Entity Framework Core and Clean Architecture
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
dotnet build
cd src/TodoBackend.API
dotnet ef database update  # First time setup
dotnet run
```

**Frontend Development**

```bash
cd TodoFrontend
npm install
npm run generate-api  # Generate API client from OpenAPI spec
npm run dev
```

### Production Deployment

The application is production-ready with:

- Multi-stage Docker builds for optimized images
- Non-root user execution in container for security
- Health checks and monitoring
- Volume persistence for database
- Single container deployment

## Future Work

- Separate deployment for frontend and backend (for separate scalability)
- Cookie auth instead of JWT
- Additional authentication features (password reset, email verification, OAuth support, etc.)
- Real-time notifications
- Advanced filtering and search capabilities
- Integration with other tools like calendar apps
