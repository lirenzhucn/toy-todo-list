using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoBackend.Infrastructure.Data;
using TodoBackend.Infrastructure.Entities;
using TodoBackend.Core.Entities;
using TodoBackend.Core.DTOs;

namespace TodoBackend.Tests.TestHelpers
{
    public abstract class BaseIntegrationTest : IClassFixture<CustomWebApplicationFactory>, IDisposable
    {
        protected readonly HttpClient _client;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly TodoContext _dbContext;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly ApplicationUser _testUser;
        protected readonly CustomWebApplicationFactory _fixture;
        protected string _authToken = string.Empty;
        private bool _disposed = false;

        protected BaseIntegrationTest(CustomWebApplicationFactory fixture)
        {
            _fixture = fixture;
            _client = fixture.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            _serviceProvider = fixture.Services;
            var scope = _serviceProvider.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<TodoContext>();
            _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            _testUser = fixture.TestUser ?? throw new InvalidOperationException("Test user not found");

            // Get authentication token for test user
            GetAuthTokenAsync().GetAwaiter().GetResult();

            // Seed initial data
            SeedDatabase().GetAwaiter().GetResult();
        }

        private async Task GetAuthTokenAsync()
        {
            var loginRequest = new LoginRequest
            {
                UserName = _testUser.UserName!,
                Password = "Test123!"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            if (response.IsSuccessStatusCode)
            {
                var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
                _authToken = authResponse?.Token ?? string.Empty;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
            }
        }

        protected async Task SeedDatabase()
        {
            // Clear existing data
            _dbContext.TodoItems.RemoveRange(_dbContext.TodoItems);
            await _dbContext.SaveChangesAsync();

            // Seed test data with user association
            var todoItems = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = 1,
                    Title = "Test Todo 1",
                    Description = "Description for test todo 1",
                    IsComplete = false,
                    ScheduledDateTime = DateTime.Parse("2024-01-15T10:00:00"),
                    DueDateTime = DateTime.Parse("2024-01-20T17:00:00"),
                    UserId = _testUser.Id
                },
                new TodoItem
                {
                    Id = 2,
                    Title = "Test Todo 2",
                    Description = "Description for test todo 2",
                    IsComplete = true,
                    ScheduledDateTime = DateTime.Parse("2024-02-15T14:00:00"),
                    DueDateTime = DateTime.Parse("2024-02-25T18:00:00"),
                    UserId = _testUser.Id
                },
                new TodoItem
                {
                    Id = 3,
                    Title = "Test Todo 3",
                    Description = "Description for test todo 3",
                    IsComplete = false,
                    ScheduledDateTime = null,
                    DueDateTime = null,
                    UserId = _testUser.Id
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