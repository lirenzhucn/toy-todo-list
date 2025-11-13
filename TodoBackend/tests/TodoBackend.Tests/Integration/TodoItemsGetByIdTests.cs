using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.Entities;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class TodoItemsGetByIdTests : BaseIntegrationTest
    {
        public TodoItemsGetByIdTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task GetTodoItemById_ShouldReturnItem_WhenItemExists()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            todoItem.Should().NotBeNull();
            todoItem!.Id.Should().Be(1);
            todoItem.Title.Should().Be("Test Todo 1");
            todoItem.Description.Should().Be("Description for test todo 1");
            todoItem.IsComplete.Should().BeFalse();
            todoItem.ScheduledDateTime.Should().Be(DateTime.Parse("2024-01-15T10:00:00"));
            todoItem.DueDateTime.Should().Be(DateTime.Parse("2024-01-20T17:00:00"));
        }

        [Fact]
        public async Task GetTodoItemById_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetTodoItemById_ShouldReturnNotFound_WhenIdIsInvalid()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems/-1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetTodoItemById_ShouldReturnNotFound_WhenIdIsZero()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems/0");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}