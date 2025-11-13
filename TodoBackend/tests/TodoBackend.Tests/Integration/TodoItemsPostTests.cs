using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.Entities;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class TodoItemsPostTests : BaseIntegrationTest
    {
        public TodoItemsPostTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task CreateTodoItem_ShouldReturnCreated_WhenValidItem()
        {
            // Arrange
            var newTodoItem = new TodoItem
            {
                Title = "New Todo Item",
                Description = "This is a new todo item",
                IsComplete = false,
                ScheduledDateTime = DateTime.Parse("2024-03-01T10:00:00"),
                DueDateTime = DateTime.Parse("2024-03-15T17:00:00")
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todoitems", newTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            createdItem.Should().NotBeNull();
            createdItem!.Id.Should().BeGreaterThan(0);
            createdItem.Title.Should().Be(newTodoItem.Title);
            createdItem.Description.Should().Be(newTodoItem.Description);
            createdItem.IsComplete.Should().Be(newTodoItem.IsComplete);
            createdItem.ScheduledDateTime.Should().Be(newTodoItem.ScheduledDateTime);
            createdItem.DueDateTime.Should().Be(newTodoItem.DueDateTime);

            // Verify Location header
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Be($"/api/todoitems/{createdItem.Id}");

            // Verify the item was actually created in the database
            var getResponse = await _client.GetAsync($"/api/todoitems/{createdItem.Id}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CreateTodoItem_ShouldReturnCreated_WhenMinimalValidItem()
        {
            // Arrange
            var newTodoItem = new TodoItem
            {
                Title = "Minimal Todo Item",
                Description = null,
                IsComplete = false,
                ScheduledDateTime = null,
                DueDateTime = null
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todoitems", newTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            createdItem.Should().NotBeNull();
            createdItem!.Id.Should().BeGreaterThan(0);
            createdItem.Title.Should().Be(newTodoItem.Title);
            createdItem.Description.Should().BeNull();
            createdItem.IsComplete.Should().BeFalse();
            createdItem.ScheduledDateTime.Should().BeNull();
            createdItem.DueDateTime.Should().BeNull();
        }

        [Fact]
        public async Task CreateTodoItem_ShouldReturnCreated_WhenCompleteItem()
        {
            // Arrange
            var newTodoItem = new TodoItem
            {
                Title = "Complete Todo Item",
                Description = "This todo is already complete",
                IsComplete = true,
                ScheduledDateTime = DateTime.Parse("2024-02-01T09:00:00"),
                DueDateTime = DateTime.Parse("2024-02-10T18:00:00")
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/todoitems", newTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            createdItem.Should().NotBeNull();
            createdItem!.Title.Should().Be(newTodoItem.Title);
            createdItem.IsComplete.Should().BeTrue();
        }

        [Fact]
        public async Task CreateTodoItem_ShouldIncrementId()
        {
            // Arrange
            var newTodoItem = new TodoItem
            {
                Title = "ID Test Item",
                Description = "Testing ID increment",
                IsComplete = false
            };

            // Act - Create first item
            var response1 = await _client.PostAsJsonAsync("/api/todoitems", newTodoItem);
            response1.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem1 = await response1.Content.ReadFromJsonAsync<TodoItem>();

            // Act - Create second item
            var response2 = await _client.PostAsJsonAsync("/api/todoitems", newTodoItem);
            response2.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem2 = await response2.Content.ReadFromJsonAsync<TodoItem>();

            // Assert
            createdItem2!.Id.Should().BeGreaterThan(createdItem1!.Id);
        }
    }
}