using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.Entities;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class TodoItemsGetTests : BaseIntegrationTest
    {
        public TodoItemsGetTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task GetAllTodoItems_ShouldReturnAllItems_WhenNoQueryParameters()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().HaveCount(3);
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 1");
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 2");
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 3");
        }

        [Fact]
        public async Task GetTodoItems_ShouldFilterByScheduledDateTimeFrom()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?scheduledDateTimeFrom=2024-01-20T00:00:00");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().HaveCount(1);
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 2");
        }

        [Fact]
        public async Task GetTodoItems_ShouldFilterByScheduledDateTimeTo()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?scheduledDateTimeTo=2024-01-20T00:00:00");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().HaveCount(1);
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 1");
        }

        [Fact]
        public async Task GetTodoItems_ShouldFilterByScheduledDateTimeRange()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?scheduledDateTimeFrom=2024-01-01T00:00:00&scheduledDateTimeTo=2024-02-01T00:00:00");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().HaveCount(1);
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 1");
        }

        [Fact]
        public async Task GetTodoItems_ShouldFilterByDueDateTimeFrom()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?dueDateTimeFrom=2024-02-20T00:00:00");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().HaveCount(1);
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 2");
        }

        [Fact]
        public async Task GetTodoItems_ShouldFilterByDueDateTimeTo()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?dueDateTimeTo=2024-01-25T00:00:00");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().HaveCount(1);
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 1");
        }

        [Fact]
        public async Task GetTodoItems_ShouldFilterByDueDateTimeRange()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?dueDateTimeFrom=2024-01-15T00:00:00&dueDateTimeTo=2024-02-15T00:00:00");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().HaveCount(1);
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 1");
        }

        [Fact]
        public async Task GetTodoItems_ShouldFilterByBothScheduledAndDueDateTime()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?scheduledDateTimeFrom=2024-02-01T00:00:00&dueDateTimeFrom=2024-02-20T00:00:00");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().HaveCount(1);
            todoItems.Should().ContainSingle(t => t.Title == "Test Todo 2");
        }

        [Fact]
        public async Task GetTodoItems_ShouldReturnEmptyList_WhenNoItemsMatchFilters()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?scheduledDateTimeFrom=2025-01-01T00:00:00");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var todoItems = await response.Content.ReadFromJsonAsync<List<TodoItem>>();
            todoItems.Should().NotBeNull();
            todoItems.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTodoItems_ShouldHandleInvalidDateFormat()
        {
            // Act
            var response = await _client.GetAsync("/api/todoitems?scheduledDateTimeFrom=invalid-date");

            // Assert - ASP.NET Core returns 400 for invalid date formats
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}