using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.Entities;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class TodoItemsDeleteTests : BaseIntegrationTest
    {
        public TodoItemsDeleteTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task DeleteTodoItem_ShouldReturnOkWithDeletedItem_WhenItemExists()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/todoitems/{1}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var deletedItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            deletedItem.Should().NotBeNull();
            deletedItem!.Id.Should().Be(1);
            deletedItem.Title.Should().Be("Test Todo 1");

            // Verify the item was actually deleted
            var getResponse = await _client.GetAsync($"/api/todoitems/{1}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/todoitems/{999}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldReduceTotalCount()
        {
            // Arrange - Get initial count
            var initialResponse = await _client.GetAsync("/api/todoitems");
            initialResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var initialItems = await initialResponse.Content.ReadFromJsonAsync<List<TodoItem>>();
            var initialCount = initialItems!.Count;

            // Act - Delete an item
            var deleteResponse = await _client.DeleteAsync($"/api/todoitems/{2}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert - Verify count decreased
            var finalResponse = await _client.GetAsync("/api/todoitems");
            var finalItems = await finalResponse.Content.ReadFromJsonAsync<List<TodoItem>>();
            finalItems!.Count.Should().Be(initialCount - 1);

            // Verify the specific item was deleted
            finalItems.Should().NotContain(t => t.Id == 2);
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldAllowReusingDeletedId()
        {
            // Arrange - Delete an item
            var deleteResponse = await _client.DeleteAsync($"/api/todoitems/{3}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act - Create a new item (database should assign a new ID)
            var newTodoItem = new TodoItem
            {
                Title = "New Item After Delete",
                Description = "This item was created after deletion",
                IsComplete = false
            };

            var createResponse = await _client.PostAsJsonAsync("/api/todoitems", newTodoItem);

            // Assert
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var createdItem = await createResponse.Content.ReadFromJsonAsync<TodoItem>();
            createdItem!.Id.Should().BeGreaterThan(3); // New ID should be greater than deleted ID
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldReturnDeletedItemData()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/todoitems/{1}");

            // Assert - Verify all fields of deleted item are returned
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var deletedItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            deletedItem.Should().NotBeNull();
            deletedItem!.Id.Should().Be(1);
            deletedItem.Title.Should().Be("Test Todo 1");
            deletedItem.Description.Should().Be("Description for test todo 1");
            deletedItem.IsComplete.Should().BeFalse();
            deletedItem.ScheduledDateTime.Should().Be(DateTime.Parse("2024-01-15T10:00:00"));
            deletedItem.DueDateTime.Should().Be(DateTime.Parse("2024-01-20T17:00:00"));
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldHandleInvalidId()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/todoitems/{-1}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteTodoItem_ShouldHandleZeroId()
        {
            // Act
            var response = await _client.DeleteAsync($"/api/todoitems/{0}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}