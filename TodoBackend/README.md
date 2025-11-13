# TodoBackend - Clean Architecture

This project has been reorganized following clean architecture principles with a clear separation of concerns across multiple layers.

## Solution Structure

```
TodoBackend/
├── src/
│   ├── TodoBackend.API/          # Web API layer
│   ├── TodoBackend.Core/         # Domain/Business layer
│   └── TodoBackend.Infrastructure/ # Data access layer
├── tests/
│   └── TodoBackend.Tests/        # Unit and Integration tests
└── TodoBackend.sln
```

## Projects

### TodoBackend.API
The Web API layer responsible for handling HTTP requests and responses.
- **Program.cs**: Minimal API configuration and endpoint definitions
- **Controllers/**: (Available for future expansion to MVC pattern)
- **Middleware/**: Custom middleware components
- **Filters/**: Action filters and model validation
- **appsettings.json**: Application configuration

### TodoBackend.Core
The domain and business logic layer containing:
- **Entities/**: Domain models (TodoItem)
- **Interfaces/**: Repository and service contracts
- **Services/**: Business logic implementation
- **Exceptions/**: Custom exception types

### TodoBackend.Infrastructure
The data access and external services layer containing:
- **Data/**: Entity Framework DbContext and migrations
- **Repositories/**: Repository pattern implementations

### TodoBackend.Tests
Test project containing:
- **UnitTests/**: Unit tests for business logic
- **IntegrationTests/**: Integration tests for API endpoints
- **TestHelpers/**: Shared testing utilities and fixtures

## Key Features

- **Clean Architecture**: Clear separation of concerns with dependency inversion
- **Repository Pattern**: Data access abstraction
- **Service Layer**: Business logic encapsulation
- **Entity Framework Core**: ORM with SQLite database
- **Comprehensive Testing**: Unit and integration tests
- **Minimal API**: Modern ASP.NET Core minimal API approach
- **OpenAPI/Swagger**: API documentation support

## API Endpoints

- `GET /api/todoitems` - Get all todo items (with optional date filtering)
- `GET /api/todoitems/{id}` - Get specific todo item by ID
- `POST /api/todoitems` - Create new todo item
- `PUT /api/todoitems/{id}` - Update existing todo item
- `DELETE /api/todoitems/{id}` - Delete todo item (returns deleted item)

## Getting Started

1. **Build the solution**:
   ```bash
   cd TodoBackend
   dotnet build
   ```

2. **Apply database migrations**:
   ```bash
   cd src/TodoBackend.API
   dotnet ef database update
   ```

3. **Run the API**:
   ```bash
   dotnet run
   ```

4. **Run tests**:
   ```bash
   cd ../..
   dotnet test
   ```

## Technologies Used

- .NET 9.0
- ASP.NET Core Minimal API
- Entity Framework Core 9.0.10
- SQLite Database
- xUnit Testing Framework
- FluentAssertions
- Moq (for mocking)

## Architecture Benefits

1. **Maintainability**: Each layer has a specific responsibility
2. **Testability**: Clear separation enables comprehensive testing
3. **Flexibility**: Easy to swap implementations (e.g., different database providers)
4. **Scalability**: Clean boundaries support future growth
5. **Developer Experience**: Clear structure makes the codebase easier to understand