using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.DTOs;
using TodoBackend.Core.Entities;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class AuthenticationFlowTests : BaseIntegrationTest
    {
        public AuthenticationFlowTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task FullAuthenticationFlow_ShouldWorkEndToEnd()
        {
            // 1. Register a new user
            var registerRequest = new RegisterRequest
            {
                Email = "fullflow@example.com",
                UserName = "fullflow",
                Password = "FullFlow123!"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            authResponse.UserName.Should().Be("fullflow");
            authResponse.Email.Should().Be("fullflow@example.com");

            var token = authResponse.Token;

            // 2. Create authenticated client with the token
            var authenticatedClient = _fixture.CreateClient();
            authenticatedClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // 3. Get todo items (should be empty for new user)
            var getResponse = await authenticatedClient.GetAsync("/api/todoitems");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await getResponse.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().BeEmpty();

            // 4. Create a new todo item
            var newTodoItem = new TodoItem
            {
                Title = "Full Flow Test Todo",
                Description = "This is a test todo item from full flow test",
                IsComplete = false
            };

            var createResponse = await authenticatedClient.PostAsJsonAsync("/api/todoitems", newTodoItem);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
            createdItem.Should().NotBeNull();
            createdItem!.Id.Should().BeGreaterThan(0);
            createdItem.Title.Should().Be(newTodoItem.Title);

            var todoId = createdItem.Id;

            // 5. Get specific todo item
            var getByIdResponse = await authenticatedClient.GetAsync($"/api/todoitems/{todoId}");
            getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var retrievedItem = await getByIdResponse.Content.ReadFromJsonAsync<TodoItem>();
            retrievedItem.Should().NotBeNull();
            retrievedItem!.Id.Should().Be(todoId);
            retrievedItem.Title.Should().Be(newTodoItem.Title);

            // 6. Update the todo item
            var updatedTodoItem = new TodoItem
            {
                Id = todoId,
                Title = "Updated Full Flow Test Todo",
                Description = "This is an updated test todo item",
                IsComplete = true
            };

            var updateResponse = await authenticatedClient.PutAsJsonAsync($"/api/todoitems/{todoId}", updatedTodoItem);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 7. Get all todo items after update
            var getAllAfterUpdateResponse = await authenticatedClient.GetAsync("/api/todoitems");
            getAllAfterUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var allItemsAfterUpdate = await getAllAfterUpdateResponse.Content.ReadFromJsonAsync<List<TodoItem>>();
            allItemsAfterUpdate.Should().NotBeNull();
            allItemsAfterUpdate.Should().HaveCount(1);
            allItemsAfterUpdate![0].Title.Should().Be("Updated Full Flow Test Todo");
            allItemsAfterUpdate[0].IsComplete.Should().BeTrue();

            // 8. Delete the todo item
            var deleteResponse = await authenticatedClient.DeleteAsync($"/api/todoitems/{todoId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // 9. Verify deletion
            var getAfterDeleteResponse = await authenticatedClient.GetAsync($"/api/todoitems/{todoId}");
            getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UnauthorizedAccess_ShouldFailForAllEndpoints()
        {
            // Test all endpoints without authentication
            var unauthenticatedClient = _fixture.CreateClient();

            // GET /api/todoitems
            var getAllResponse = await unauthenticatedClient.GetAsync("/api/todoitems");
            getAllResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // GET /api/todoitems/{id}
            var getByIdResponse = await unauthenticatedClient.GetAsync("/api/todoitems/1");
            getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // POST /api/todoitems
            var newItem = new TodoItem { Title = "Test", Description = "Test", IsComplete = false };
            var postResponse = await unauthenticatedClient.PostAsJsonAsync("/api/todoitems", newItem);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // PUT /api/todoitems/{id}
            var putResponse = await unauthenticatedClient.PutAsJsonAsync("/api/todoitems/1", newItem);
            putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // DELETE /api/todoitems/{id}
            var deleteResponse = await unauthenticatedClient.DeleteAsync("/api/todoitems/1");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task InvalidToken_ShouldFailForAllEndpoints()
        {
            // Test all endpoints with invalid token
            var invalidTokenClient = _fixture.CreateClient();
            invalidTokenClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

            // GET /api/todoitems
            var getAllResponse = await invalidTokenClient.GetAsync("/api/todoitems");
            getAllResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // GET /api/todoitems/{id}
            var getByIdResponse = await invalidTokenClient.GetAsync("/api/todoitems/1");
            getByIdResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // POST /api/todoitems
            var newItem = new TodoItem { Title = "Test", Description = "Test", IsComplete = false };
            var postResponse = await invalidTokenClient.PostAsJsonAsync("/api/todoitems", newItem);
            postResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // PUT /api/todoitems/{id}
            var putResponse = await invalidTokenClient.PutAsJsonAsync("/api/todoitems/1", newItem);
            putResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            // DELETE /api/todoitems/{id}
            var deleteResponse = await invalidTokenClient.DeleteAsync("/api/todoitems/1");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task UserIsolation_ShouldPreventAccessToOtherUsersItems()
        {
            // Register first user and create a todo item
            var firstUserRequest = new RegisterRequest
            {
                Email = "firstuser@example.com",
                UserName = "firstuser",
                Password = "FirstUser123!"
            };

            var firstUserRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", firstUserRequest);
            var firstUserAuthResponse = await firstUserRegisterResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var firstUserToken = firstUserAuthResponse!.Token;

            var firstUserClient = _fixture.CreateClient();
            firstUserClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", firstUserToken);

            // Create todo item for first user
            var firstUserTodo = new TodoItem
            {
                Title = "First User Todo",
                Description = "This belongs to first user",
                IsComplete = false
            };

            var createResponse = await firstUserClient.PostAsJsonAsync("/api/todoitems", firstUserTodo);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
            var firstUserItemId = createdItem!.Id;

            // Register second user
            var secondUserRequest = new RegisterRequest
            {
                Email = "seconduser@example.com",
                UserName = "seconduser",
                Password = "SecondUser123!"
            };

            var secondUserRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", secondUserRequest);
            var secondUserAuthResponse = await secondUserRegisterResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var secondUserToken = secondUserAuthResponse!.Token;

            var secondUserClient = _fixture.CreateClient();
            secondUserClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secondUserToken);

            // Second user should not be able to access first user's item
            var getResponse = await secondUserClient.GetAsync($"/api/todoitems/{firstUserItemId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Second user should not be able to update first user's item
            var updateItem = new TodoItem
            {
                Id = firstUserItemId,
                Title = "Hacked Todo",
                Description = "This should not be updated",
                IsComplete = true
            };

            var updateResponse = await secondUserClient.PutAsJsonAsync($"/api/todoitems/{firstUserItemId}", updateItem);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Second user should not be able to delete first user's item
            var deleteResponse = await secondUserClient.DeleteAsync($"/api/todoitems/{firstUserItemId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Verify first user still has their item
            var firstUserItemsResponse = await firstUserClient.GetAsync("/api/todoitems");
            var firstUserItems = await firstUserItemsResponse.Content.ReadFromJsonAsync<List<TodoItem>>();
            firstUserItems.Should().HaveCount(1);
            firstUserItems![0].Title.Should().Be("First User Todo");

            // Verify second user has no items
            var secondUserItemsResponse = await secondUserClient.GetAsync("/api/todoitems");
            var secondUserItems = await secondUserItemsResponse.Content.ReadFromJsonAsync<List<TodoItem>>();
            secondUserItems.Should().BeEmpty();
        }
    }
}