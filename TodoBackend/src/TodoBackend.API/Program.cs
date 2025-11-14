using Microsoft.EntityFrameworkCore;
using TodoBackend.Core.Entities;
using TodoBackend.Core.Interfaces;
using TodoBackend.Core.Services;
using TodoBackend.Infrastructure.Data;
using TodoBackend.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Configure SQLite database connection
builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddScoped<ITodoItemRepository, TodoItemRepository>();
builder.Services.AddScoped<ITodoItemService, TodoItemService>();

var app = builder.Build();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();   // looks for index.html
app.UseStaticFiles();    // serves everything under wwwroot
app.MapFallbackToFile("index.html");

// Health check endpoint for Docker
app.MapGet("/health", () => Results.Ok("Healthy"));

// Define API endpoints (CRUD operations)

// GET all todo items with optional date range filtering
app.MapGet("/api/todoitems", async (
    DateTime? scheduledDateTimeFrom,
    DateTime? scheduledDateTimeTo,
    DateTime? dueDateTimeFrom,
    DateTime? dueDateTimeTo,
    ITodoItemService todoItemService) =>
{
    var todoItems = await todoItemService.GetAllTodoItemsAsync(
        scheduledDateTimeFrom,
        scheduledDateTimeTo,
        dueDateTimeFrom,
        dueDateTimeTo);

    return Results.Ok(todoItems);
});

// GET a specific todo item by ID
app.MapGet("/api/todoitems/{id}", async (int id, ITodoItemService todoItemService) =>
{
    var todoItem = await todoItemService.GetTodoItemByIdAsync(id);
    return todoItem is not null ? Results.Ok(todoItem) : Results.NotFound();
});

// POST (Create) a new todo item
app.MapPost("/api/todoitems", async (TodoItem todoItem, ITodoItemService todoItemService) =>
{
    var createdItem = await todoItemService.CreateTodoItemAsync(todoItem);
    return Results.Created($"/api/todoitems/{createdItem.Id}", createdItem);
});

// PUT (Modify) an existing todo item
app.MapPut("/api/todoitems/{id}", async (int id, TodoItem inputTodoItem, ITodoItemService todoItemService) =>
{
    try
    {
        await todoItemService.UpdateTodoItemAsync(id, inputTodoItem);
        return Results.NoContent();
    }
    catch (KeyNotFoundException)
    {
        return Results.NotFound();
    }
});

// DELETE a todo item
app.MapDelete("/api/todoitems/{id}", async (int id, ITodoItemService todoItemService) =>
{
    var deletedItem = await todoItemService.DeleteTodoItemAsync(id);
    return deletedItem != null ? Results.Ok(deletedItem) : Results.NotFound();
});

app.Run();

// Make the Program class accessible for testing
public partial class Program { }

// Make the Program class accessible for testing with full namespace
namespace TodoBackend.API
{
    public partial class Program { }
}
