# Multi-stage Dockerfile for TodoBackend .NET Core Web API

# Stage 1: Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["TodoBackend.sln", "./"]
COPY ["src/TodoBackend.API/TodoBackend.API.csproj", "src/TodoBackend.API/"]
COPY ["src/TodoBackend.Core/TodoBackend.Core.csproj", "src/TodoBackend.Core/"]
COPY ["src/TodoBackend.Infrastructure/TodoBackend.Infrastructure.csproj", "src/TodoBackend.Infrastructure/"]
COPY ["tests/TodoBackend.Tests/TodoBackend.Tests.csproj", "tests/TodoBackend.Tests/"]

# Restore dependencies
RUN dotnet restore "TodoBackend.sln"

# Copy source code
COPY ["src/", "src/"]
COPY ["tests/", "tests/"]

# Build the application
WORKDIR "/src/src/TodoBackend.API"
RUN dotnet build "TodoBackend.API.csproj" -c Release -o /app/build

# Stage 2: Publish stage
FROM build AS publish
RUN dotnet publish "TodoBackend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install SQLite tools for database operations (optional, for migrations)
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -g 1001 appuser && useradd -u 1001 -g appuser -s /bin/bash -m appuser

# Copy published application
COPY --from=publish /app/publish .

# Copy entrypoint script
COPY docker-entrypoint.sh /app/
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
