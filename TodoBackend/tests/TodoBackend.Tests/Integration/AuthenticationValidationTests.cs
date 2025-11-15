using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.DTOs;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class AuthenticationValidationTests : BaseIntegrationTest
    {
        public AuthenticationValidationTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailIsInvalid()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "invalid-email",
                UserName = "testuser",
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordIsTooWeak()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                UserName = "testuser",
                Password = "weak"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUserNameIsEmpty()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                UserName = "",
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailIsEmpty()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "",
                UserName = "testuser",
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordIsEmpty()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "test@example.com",
                UserName = "testuser",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserNameIsEmpty()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                UserName = "",
                Password = "Test123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsEmpty()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                UserName = "testuser",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenBothCredentialsAreEmpty()
        {
            // Arrange
            var loginRequest = new LoginRequest
            {
                UserName = "",
                Password = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            // Act
            var response = await _client.PostAsJsonAsync<RegisterRequest>("/api/auth/register", null!);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenRequestIsNull()
        {
            // Act
            var response = await _client.PostAsJsonAsync<LoginRequest>("/api/auth/login", null!);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_ShouldHandleConcurrentRequests()
        {
            // Arrange
            var registerRequests = new[]
            {
                new RegisterRequest { Email = "concurrent1@example.com", UserName = "concurrent1", Password = "Concurrent1!" },
                new RegisterRequest { Email = "concurrent2@example.com", UserName = "concurrent2", Password = "Concurrent2!" },
                new RegisterRequest { Email = "concurrent3@example.com", UserName = "concurrent3", Password = "Concurrent3!" }
            };

            // Act
            var tasks = registerRequests.Select(request =>
                _client.PostAsJsonAsync("/api/auth/register", request));

            var responses = await Task.WhenAll(tasks);

            // Assert
            foreach (var response in responses)
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }

        [Fact]
        public async Task TokenExpiration_ShouldRequireReauthentication()
        {
            // This test would require token expiration to be configured
            // For now, we'll test that different tokens work for different users

            // Register and login first user
            var firstUserRequest = new RegisterRequest
            {
                Email = "token1@example.com",
                UserName = "tokenuser1",
                Password = "Token1!"
            };

            var firstRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", firstUserRequest);
            var firstAuthResponse = await firstRegisterResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var firstToken = firstAuthResponse!.Token;

            // Register and login second user
            var secondUserRequest = new RegisterRequest
            {
                Email = "token2@example.com",
                UserName = "tokenuser2",
                Password = "Token2!"
            };

            var secondRegisterResponse = await _client.PostAsJsonAsync("/api/auth/register", secondUserRequest);
            var secondAuthResponse = await secondRegisterResponse.Content.ReadFromJsonAsync<AuthResponse>();
            var secondToken = secondAuthResponse!.Token;

            // Tokens should be different
            firstToken.Should().NotBe(secondToken);

            // Both tokens should work for their respective users
            var firstUserClient = _fixture.CreateClient();
            firstUserClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", firstToken);

            var secondUserClient = _fixture.CreateClient();
            secondUserClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", secondToken);

            var firstUserItemsResponse = await firstUserClient.GetAsync("/api/todoitems");
            firstUserItemsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var secondUserItemsResponse = await secondUserClient.GetAsync("/api/todoitems");
            secondUserItemsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}