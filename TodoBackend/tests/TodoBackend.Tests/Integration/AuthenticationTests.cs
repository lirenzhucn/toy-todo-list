using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.DTOs;
using TodoBackend.Core.Entities;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class AuthenticationTests : BaseIntegrationTest
    {
        public AuthenticationTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task Register_ShouldReturnSuccess_WhenValidData()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "newuser@example.com",
                UserName = "newuser",
                Password = "NewPass123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            authResponse.UserName.Should().Be("newuser");
            authResponse.Email.Should().Be("newuser@example.com");
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUsernameAlreadyExists()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "different@example.com",
                UserName = "testuser", // This already exists from test setup
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com", // This already exists from test setup
                UserName = "differentuser",
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ShouldReturnSuccess_WhenValidCredentials()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                UserName = "testuser",
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            authResponse.Should().NotBeNull();
            authResponse!.Token.Should().NotBeNullOrEmpty();
            authResponse.UserName.Should().Be("testuser");
            authResponse.Email.Should().Be("test@example.com");
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenInvalidPassword()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                UserName = "testuser",
                Password = "WrongPassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                UserName = "nonexistentuser",
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task TodoItems_ShouldRequireAuthentication()
        {
            // Arrange - Create unauthenticated client
            var unauthenticatedClient = _fixture.CreateClient();

            // Act
            var response = await unauthenticatedClient.GetAsync("/api/todoitems");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task TodoItems_ShouldAllowAccess_WithValidToken()
        {
            // Act - Using authenticated client from base class
            var response = await _client.GetAsync("/api/todoitems");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task TodoItems_ShouldRejectInvalidToken()
        {
            // Arrange - Create client with invalid token
            var invalidTokenClient = _fixture.CreateClient();
            invalidTokenClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid-token");

            // Act
            var response = await invalidTokenClient.GetAsync("/api/todoitems");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task TodoItems_ShouldIsolateUserData()
        {
            // Arrange - Create a second user and their todo item
            var registerRequest = new RegisterRequest
            {
                Email = "seconduser@example.com",
                UserName = "seconduser",
                Password = "SecondPass123!"
            };

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var secondUserToken = authResponse!.Token;

            // Create a client for the second user
            var secondUserClient = _fixture.CreateClient();
            secondUserClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secondUserToken);

            // Create a todo item for the second user
            var newTodoItem = new TodoItem
            {
                Title = "Second User Todo",
                Description = "This belongs to second user",
                IsComplete = false
            };

            var createResponse = await secondUserClient.PostAsJsonAsync("/api/todoitems", newTodoItem);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Act - First user tries to get all todo items
            var firstUserTodos = await _client.GetFromJsonAsync<List<TodoItem>>("/api/todoitems");
            
            // Act - Second user tries to get all todo items
            var secondUserTodos = await secondUserClient.GetFromJsonAsync<List<TodoItem>>("/api/todoitems");

            // Assert - Users should only see their own items
            firstUserTodos.Should().NotBeNull();
            secondUserTodos.Should().NotBeNull();
            firstUserTodos!.Count.Should().Be(3); // From seed data
            secondUserTodos!.Count.Should().Be(1); // Only the item they created
            firstUserTodos.Should().NotContain(t => t.Title == "Second User Todo");
            secondUserTodos.Should().ContainSingle(t => t.Title == "Second User Todo");
        }
    }
}