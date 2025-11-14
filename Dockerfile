# Multi-stage Dockerfile for Full-Stack Todo Application
# Builds both frontend and backend in a single container

# Stage 1: Build Frontend
FROM node:22-alpine AS frontend-build
WORKDIR /app/TodoFrontend

# Copy frontend package files
COPY TodoFrontend/package*.json ./
COPY ./openapi.json ../
RUN npm ci

# Copy frontend source code
COPY TodoFrontend/ ./

# Generate API client from OpenAPI spec
RUN npm run generate-api

# Create fake backend directory to make sure build passes
RUN mkdir -p /app/TodoBackend/src/TodoBackend.API/wwwroot

# Build frontend application
RUN npm run build

# Stage 2: Build Backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy solution and project files
COPY TodoBackend/TodoBackend.sln ./
COPY TodoBackend/src/TodoBackend.API/TodoBackend.API.csproj src/TodoBackend.API/
COPY TodoBackend/src/TodoBackend.Core/TodoBackend.Core.csproj src/TodoBackend.Core/
COPY TodoBackend/src/TodoBackend.Infrastructure/TodoBackend.Infrastructure.csproj src/TodoBackend.Infrastructure/
COPY TodoBackend/tests/TodoBackend.Tests/TodoBackend.Tests.csproj tests/TodoBackend.Tests/

# Restore dependencies
RUN dotnet restore "TodoBackend.sln"

# Copy source code
COPY TodoBackend/src/ src/
COPY TodoBackend/tests/ tests/

# Build the application
WORKDIR "/src/src/TodoBackend.API"
RUN dotnet build "TodoBackend.API.csproj" -c Release -o /app/build

# Stage 3: Publish Backend
FROM backend-build AS backend-publish
RUN dotnet publish "TodoBackend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install SQLite tools for database operations
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -g 1001 appuser && useradd -u 1001 -g appuser -s /bin/bash -m appuser

# Copy published backend application
COPY --from=backend-publish /app/publish .

# Copy frontend build files to wwwroot
COPY --from=frontend-build /app/TodoFrontend/dist ./wwwroot

# Copy entrypoint script
COPY TodoBackend/docker-entrypoint.sh /app/
RUN chmod +x /app/docker-entrypoint.sh

# Create directory for SQLite database
RUN mkdir -p /app/data && chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Environment variables for ASP.NET Core
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Start the application
ENTRYPOINT ["/app/docker-entrypoint.sh", "dotnet", "TodoBackend.API.dll"]
