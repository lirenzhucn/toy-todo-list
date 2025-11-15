using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoBackend.Infrastructure.Data;
using TodoBackend.Infrastructure.Entities;
using TodoBackend.Core.Entities;

namespace TodoBackend.Tests.TestHelpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TodoBackend.API.Program>
    {
        private readonly SqliteConnection _connection;
        private UserManager<ApplicationUser>? _userManager;
        public ApplicationUser? TestUser { get; private set; }
        public string? TestUserToken { get; private set; }

        public CustomWebApplicationFactory()
        {
            // Create in-memory SQLite connection
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TodoContext>));
                
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add SQLite with in-memory database
                services.AddDbContext<TodoContext>(options =>
                    options.UseSqlite(_connection));

                // Build the service provider
                var serviceProvider = services.BuildServiceProvider();

                // Create a scope to obtain a reference of the db contexts
                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<TodoContext>();

                // Ensure the database is created and apply migrations
                dbContext.Database.Migrate();

                // Create test user
                _userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                CreateTestUserAsync().GetAwaiter().GetResult();
            });

            builder.UseEnvironment("Testing");
        }

        private async Task CreateTestUserAsync()
        {
            if (_userManager == null) return;

            // Check if test user already exists
            var existingUser = await _userManager.FindByNameAsync("testuser");
            if (existingUser != null)
            {
                TestUser = existingUser;
                return;
            }

            // Create test user
            var testUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(testUser, "Test123!");
            if (result.Succeeded)
            {
                TestUser = testUser;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection?.Dispose();
            }
        }
    }
}