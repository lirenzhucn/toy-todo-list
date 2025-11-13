using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoBackend.Infrastructure.Data;
using TodoBackend.Core.Entities;

namespace TodoBackend.Tests.TestHelpers
{
    public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        protected readonly HttpClient _client;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly TodoContext _dbContext;
        private bool _disposed = false;

        protected BaseIntegrationTest(CustomWebApplicationFactory fixture)
        {
            _client = fixture.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _serviceProvider = fixture.Services;
            _dbContext = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<TodoContext>();
            
            // Seed initial data
            SeedDatabase().GetAwaiter().GetResult();
        }

        protected async Task SeedDatabase()
        {
            // Clear existing data
            _dbContext.TodoItems.RemoveRange(_dbContext.TodoItems);
            await _dbContext.SaveChangesAsync();

            // Seed test data
            var todoItems = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = 1,
                    Title = "Test Todo 1",
                    Description = "Description for test todo 1",
                    IsComplete = false,
                    ScheduledDateTime = DateTime.Parse("2024-01-15T10:00:00"),
                    DueDateTime = DateTime.Parse("2024-01-20T17:00:00")
                },
                new TodoItem
                {
                    Id = 2,
                    Title = "Test Todo 2",
                    Description = "Description for test todo 2",
                    IsComplete = true,
                    ScheduledDateTime = DateTime.Parse("2024-02-15T14:00:00"),
                    DueDateTime = DateTime.Parse("2024-02-25T18:00:00")
                },
                new TodoItem
                {
                    Id = 3,
                    Title = "Test Todo 3",
                    Description = "Description for test todo 3",
                    IsComplete = false,
                    ScheduledDateTime = null,
                    DueDateTime = null
                }
            };

            _dbContext.TodoItems.AddRange(todoItems);
            await _dbContext.SaveChangesAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _client?.Dispose();
                    _dbContext?.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}