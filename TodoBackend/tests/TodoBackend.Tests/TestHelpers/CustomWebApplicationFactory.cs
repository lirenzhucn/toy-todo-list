using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoBackend.Infrastructure.Data;
using TodoBackend.Core.Entities;

namespace TodoBackend.Tests.TestHelpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<TodoBackend.API.Program>
    {
        private readonly SqliteConnection _connection;

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

                // Ensure the database is created
                dbContext.Database.EnsureCreated();
            });

            builder.UseEnvironment("Testing");
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