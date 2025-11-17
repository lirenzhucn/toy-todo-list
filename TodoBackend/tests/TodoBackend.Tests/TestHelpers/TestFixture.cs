using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoBackend.Infrastructure.Data;

namespace TodoBackend.Tests.TestHelpers
{
    public class TestFixture : WebApplicationFactory<TodoBackend.API.Program>
    {
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

                // Remove TodoContext registration if it exists
                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(TodoContext));

                if (contextDescriptor != null)
                {
                    services.Remove(contextDescriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<TodoContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                    options.UseInternalServiceProvider(null); // Reset internal service provider
                });

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
    }
}