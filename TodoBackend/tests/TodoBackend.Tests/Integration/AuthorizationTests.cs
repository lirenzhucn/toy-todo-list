using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.DTOs;
using TodoBackend.Core.Entities;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class AuthorizationTests : BaseIntegrationTest
    {
        public AuthorizationTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task GetTodoItemById_ShouldRequireAuthentication()
        {
            // Arrange - Create unauthenticated client
            var unauthenticatedClient = new CustomWebApplicationFactory().CreateClient();

            // Act
            var response = await unauthenticatedClient.GetAsync("/api/todoitems/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task CreateTodoItem_ShouldRequireAuthentication()
        {
            // Arrange - Create unauthenticated client
            var unauthenticatedClient = new CustomWebApplicationFactory().CreateClient();
            var newTodoItem = new TodoItem
            {
                Title = "Unauthorized Todo",
                Description = "This should not be created",
                IsComplete = false
            };

            // Act
            var response = await unauthenticatedClient.PostAsJsonAsync("/api/todoitems", newTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldRequireAuthentication()
        {
            // Arrange - Create unauthenticated client
            var unauthenticatedClient = new CustomWebApplicationFactory().CreateClient();
            var updatedTodoItem = new TodoItem
            {
                Id = 1,
                Title = "Updated Todo",
                Description = "This should not be updated",
                IsComplete = true
            };

            // Act
            var response = await unauthenticatedClient.PutAsJsonAsync("/api/todoitems/1", updatedTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldRequireAuthentication()
        {
            // Arrange - Create unauthenticated client
            var unauthenticatedClient = new CustomWebApplicationFactory().CreateClient();

            // Act
            var response = await unauthenticatedClient.DeleteAsync("/api/todoitems/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetTodoItemById_ShouldReturnNotFound_WhenItemBelongsToDifferentUser()
        {
            // Arrange - Create a second user and their todo item
            var registerRequest = new RegisterRequest
            {
                Email = "differentuser@example.com",
                UserName = "differentuser",
                Password = "DifferentPass123!"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var differentUserToken = authResponse!.Token;

            // Create a client for the different user using the same factory to share database context
            var differentUserClient = _fixture.CreateClient();
            differentUserClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", differentUserToken);

            // Create a todo item for the different user
            var newTodoItem = new TodoItem
            {
                Title = "Different User Todo",
                Description = "This belongs to different user",
                IsComplete = false
            };

            var createResponse = await differentUserClient.PostAsJsonAsync("/api/todoitems", newTodoItem);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
            var differentUserItemId = createdItem!.Id;

            // Act - First user tries to access the different user's item
            var response = await _client.GetAsync($"/api/todoitems/{differentUserItemId}");

            // Assert - Should return NotFound because the item belongs to a different user
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldReturnNotFound_WhenItemBelongsToDifferentUser()
        {
            // Arrange - Create a second user and their todo item
            var registerRequest = new RegisterRequest
            {
                Email = "updateuser@example.com",
                UserName = "updateuser",
                Password = "UpdatePass123!"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var differentUserToken = authResponse!.Token;

            // Create a client for the different user using the same factory to share database context
            var differentUserClient = _fixture.CreateClient();
            differentUserClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", differentUserToken);

            // Create a todo item for the different user
            var newTodoItem = new TodoItem
            {
                Title = "Update Test Todo",
                Description = "This will be updated by different user",
                IsComplete = false
            };

            var createResponse = await differentUserClient.PostAsJsonAsync("/api/todoitems", newTodoItem);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
            var differentUserItemId = createdItem!.Id;

            // Act - First user tries to update the different user's item
            var updatedItem = new TodoItem
            {
                Id = differentUserItemId,
                Title = "Hacked Todo",
                Description = "This should not be updated",
                IsComplete = true
            };

            var response = await _client.PutAsJsonAsync($"/api/todoitems/{differentUserItemId}", updatedItem);

            // Assert - Should return NotFound because the item belongs to a different user
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldReturnNotFound_WhenItemBelongsToDifferentUser()
        {
            // Arrange - Create a second user and their todo item
            var registerRequest = new RegisterRequest
            {
                Email = "deleteuser@example.com",
                UserName = "deleteuser",
                Password = "DeletePass123!"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var differentUserToken = authResponse!.Token;

            // Create a client for the different user using the same factory to share database context
            var differentUserClient = _fixture.CreateClient();
            differentUserClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", differentUserToken);

            // Create a todo item for the different user
            var newTodoItem = new TodoItem
            {
                Title = "Delete Test Todo",
                Description = "This will be deleted by different user",
                IsComplete = false
            };

            var createResponse = await differentUserClient.PostAsJsonAsync("/api/todoitems", newTodoItem);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
            var differentUserItemId = createdItem!.Id;

            // Act - First user tries to delete the different user's item
            var response = await _client.DeleteAsync($"/api/todoitems/{differentUserItemId}");

            // Assert - Should return NotFound because the item belongs to a different user
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task AllTodoEndpoints_ShouldReturnUnauthorized_WithExpiredToken()
        {
            // This test would require token expiration to be set very low
            // For now, we'll test with an obviously invalid token format
            var invalidTokenClient = new CustomWebApplicationFactory().CreateClient();
            invalidTokenClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.invalid.token");

            // Test GET
            var getResponse = await invalidTokenClient.GetAsync("/api/todoitems");
            getResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Test GET by ID
            var getByIdResponse = await invalidTokenClient.GetAsync("/api/todoitems/1");
            getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Test POST
            var newItem = new TodoItem { Title = "Test", Description = "Test", IsComplete = false };
            var postResponse = await invalidTokenClient.PostAsJsonAsync("/api/todoitems", newItem);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Test PUT
            var putResponse = await invalidTokenClient.PutAsJsonAsync("/api/todoitems/1", newItem);
            putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // Test DELETE
            var deleteResponse = await invalidTokenClient.DeleteAsync("/api/todoitems/1");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}