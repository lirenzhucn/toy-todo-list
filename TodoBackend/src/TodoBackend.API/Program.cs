using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TodoBackend.Core.Entities;
using TodoBackend.Infrastructure.Entities;
using TodoBackend.Core.Interfaces;
using TodoBackend.Core.Services;
using TodoBackend.Core.DTOs;
using TodoBackend.Infrastructure.Data;
using TodoBackend.Infrastructure.Repositories;
using TodoBackend.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure SQLite database connection
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("TodoBackend.API")));

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<TodoContext>()
.AddDefaultTokenProviders();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// Register application services
builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
builder.Services.AddScoped<ITodoItemService, TodoItemService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

var app = builder.Build();

// Apply database migrations on startup (skip in test environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.UseDefaultFiles();   // looks for index.html
app.UseStaticFiles();    // serves everything under wwwroot
app.MapFallbackToFile("index.html");

// Health check endpoint for Docker
app.MapGet("/health", () => Results.Ok("Healthy"));

// Authentication endpoints
app.MapPost("/api/auth/register", async (RegisterRequest request, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(request);
    return result != null
        ? Results.Ok(result)
        : Results.BadRequest(new { message = "Registration failed. Username or email may already exist." });
});

app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService) =>
{
    var result = await authService.LoginAsync(request);
    return result != null
        ? Results.Ok(result)
        : Results.Unauthorized();
});

// Define API endpoints (CRUD operations)

// GET all todo items with optional date range filtering
app.MapGet("/api/todoitems", async (
    DateTime? scheduledDateTimeFrom,
    DateTime? scheduledDateTimeTo,
    DateTime? dueDateTimeFrom,
    DateTime? dueDateTimeTo,
    ITodoItemService todoItemService,
    System.Security.Claims.ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("userId")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var todoItems = await todoItemService.GetAllTodoItemsAsync(
        userId,
        scheduledDateTimeFrom,
        scheduledDateTimeTo,
        dueDateTimeFrom,
        dueDateTimeTo);

    return Results.Ok(todoItems);
}).RequireAuthorization();

// GET a specific todo item by ID
app.MapGet("/api/todoitems/{id}", async (int id, ITodoItemService todoItemService, System.Security.Claims.ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("userId")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var todoItem = await todoItemService.GetTodoItemByIdAsync(id, userId);
    return todoItem is not null ? Results.Ok(todoItem) : Results.NotFound();
}).RequireAuthorization();

// POST (Create) a new todo item
app.MapPost("/api/todoitems", async (TodoItem todoItem, ITodoItemService todoItemService, System.Security.Claims.ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("userId")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var createdItem = await todoItemService.CreateTodoItemAsync(todoItem, userId);
    return Results.Created($"/api/todoitems/{createdItem.Id}", createdItem);
}).RequireAuthorization();

// PUT (Modify) an existing todo item
app.MapPut("/api/todoitems/{id}", async (int id, TodoItem inputTodoItem, ITodoItemService todoItemService, System.Security.Claims.ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("userId")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    try
    {
        var updatedItem = await todoItemService.UpdateTodoItemAsync(id, inputTodoItem, userId);
        return updatedItem is not null ? Results.Ok(updatedItem) : Results.NotFound();
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
}).RequireAuthorization();

// DELETE a todo item
app.MapDelete("/api/todoitems/{id}", async (int id, ITodoItemService todoItemService, System.Security.Claims.ClaimsPrincipal user) =>
{
    var userId = user.FindFirst("userId")?.Value;
    if (string.IsNullOrEmpty(userId))
    {
        return Results.Unauthorized();
    }

    var deletedItem = await todoItemService.DeleteTodoItemAsync(id, userId);
    return deletedItem != null ? Results.Ok(deletedItem) : Results.NotFound();
}).RequireAuthorization();

app.Run();

// Make the Program class accessible for testing
public partial class Program { }

// Make the Program class accessible for testing with full namespace
namespace TodoBackend.API
{
    public partial class Program { }
}
