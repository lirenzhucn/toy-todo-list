using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TodoBackend.Core.DTOs;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class PasswordValidationTests : BaseIntegrationTest
    {
        public PasswordValidationTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task Register_ShouldReturnDetailedPasswordErrors_WhenPasswordIsTooShort()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "shortpwd@example.com",
                UserName = "shortpwduser",
                Password = "123"  // Too short, minimum is 6
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Parse the response to check for specific error messages
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            root.GetProperty("message").GetString().Should().Be("Registration failed.");
            root.GetProperty("errors").EnumerateArray().Should().Contain(error =>
                error.GetString()!.Contains("Passwords must be at least 6 characters."));
        }

        [Fact]
        public async Task Register_ShouldReturnDetailedPasswordErrors_WhenPasswordMissingDigit()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "nodigit@example.com",
                UserName = "nodigituser",
                Password = "Password"  // Missing digit
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Parse the response to check for specific error messages
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            root.GetProperty("message").GetString().Should().Be("Registration failed.");
            root.GetProperty("errors").EnumerateArray().Should().Contain(error =>
                error.GetString()!.Contains("Passwords must have at least one digit ('0'-'9')."));
        }

        [Fact]
        public async Task Register_ShouldReturnDetailedPasswordErrors_WhenPasswordMissingUppercase()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "noupper@example.com",
                UserName = "noupperuser",
                Password = "password123"  // Missing uppercase
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Parse the response to check for specific error messages
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            root.GetProperty("message").GetString().Should().Be("Registration failed.");
            root.GetProperty("errors").EnumerateArray().Should().Contain(error =>
                error.GetString()!.Contains("Passwords must have at least one uppercase ('A'-'Z')."));
        }

        [Fact]
        public async Task Register_ShouldReturnMultiplePasswordErrors_WhenPasswordHasMultipleIssues()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "multierror@example.com",
                UserName = "multierroruser",
                Password = "weak"  // Too short, no digit, no uppercase
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Parse the response to check for specific error messages
            using var doc = JsonDocument.Parse(responseContent);
            var root = doc.RootElement;

            root.GetProperty("message").GetString().Should().Be("Registration failed.");
            var errors = root.GetProperty("errors").EnumerateArray().Select(e => e.GetString()).ToList();

            errors.Should().Contain(error => error!.Contains("Passwords must be at least 6 characters."));
            errors.Should().Contain(error => error!.Contains("Passwords must have at least one digit"));
            errors.Should().Contain(error => error!.Contains("Passwords must have at least one uppercase"));
        }

        [Fact]
        public async Task Register_ShouldReturnSuccess_WhenPasswordMeetsAllRequirements()
        {
            // Arrange
            var registerRequest = new RegisterRequest
            {
                Email = "success@example.com",
                UserName = "successuser",
                Password = "Valid123"  // Meets all requirements: 6+ chars, digit, uppercase
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            authResponse.Should().NotBeNull();
            authResponse!.UserName.Should().Be("successuser");
            authResponse.Email.Should().Be("success@example.com");
            authResponse.Token.Should().NotBeNullOrEmpty();
        }
    }
}
