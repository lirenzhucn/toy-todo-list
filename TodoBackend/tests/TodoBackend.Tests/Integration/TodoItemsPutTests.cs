using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TodoBackend.Core.Entities;
using TodoBackend.Tests.TestHelpers;

namespace TodoBackend.Tests.Tests.Integration
{
    public class TodoItemsPutTests : BaseIntegrationTest
    {
        public TodoItemsPutTests(CustomWebApplicationFactory fixture) : base(fixture) { }

        [Fact]
        public async Task UpdateTodoItem_ShouldReturnNoContent_WhenItemExists()
        {
            // Arrange
            var updatedTodoItem = new TodoItem
            {
                Id = 1,
                Title = "Updated Todo Item",
                Description = "This todo item has been updated",
                IsComplete = true,
                ScheduledDateTime = DateTime.Parse("2024-01-16T11:00:00"),
                DueDateTime = DateTime.Parse("2024-01-21T18:00:00")
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todoitems/{1}", updatedTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the response contains the updated item
            var updatedItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            updatedItem.Should().NotBeNull();
            updatedItem!.Title.Should().Be(updatedTodoItem.Title);
            updatedItem.Description.Should().Be(updatedTodoItem.Description);
            updatedItem.IsComplete.Should().Be(updatedTodoItem.IsComplete);
            updatedItem.ScheduledDateTime.Should().Be(updatedTodoItem.ScheduledDateTime);
            updatedItem.DueDateTime.Should().Be(updatedTodoItem.DueDateTime);

            // Verify the item was actually updated
            var getResponse = await _client.GetAsync($"/api/todoitems/{1}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var retrievedItem = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
            retrievedItem.Should().NotBeNull();
            retrievedItem!.Title.Should().Be(updatedTodoItem.Title);
            retrievedItem.Description.Should().Be(updatedTodoItem.Description);
            retrievedItem.IsComplete.Should().BeTrue();
            retrievedItem.ScheduledDateTime.Should().Be(updatedTodoItem.ScheduledDateTime);
            retrievedItem.DueDateTime.Should().Be(updatedTodoItem.DueDateTime);
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldReturnNotFound_WhenItemDoesNotExist()
        {
            // Arrange
            var updatedTodoItem = new TodoItem
            {
                Id = 999,
                Title = "Non-existent Todo Item",
                Description = "This item doesn't exist",
                IsComplete = false
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todoitems/{999}", updatedTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldUpdateOnlySpecifiedFields()
        {
            // Arrange - Get original item
            var originalResponse = await _client.GetAsync($"/api/todoitems/{2}");
            originalResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var originalItem = await originalResponse.Content.ReadFromJsonAsync<TodoItem>();

            // Arrange - Create update with only some fields changed
            var updatedTodoItem = new TodoItem
            {
                Id = 2,
                Title = "Only Title Updated",
                Description = originalItem!.Description, // Keep original description
                IsComplete = originalItem.IsComplete, // Keep original status
                ScheduledDateTime = originalItem.ScheduledDateTime, // Keep original scheduled time
                DueDateTime = originalItem.DueDateTime // Keep original due time
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todoitems/{2}", updatedTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the response contains the updated item
            var updatedItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            updatedItem.Should().NotBeNull();
            updatedItem!.Title.Should().Be("Only Title Updated");
            updatedItem.Description.Should().Be(originalItem.Description);
            updatedItem.IsComplete.Should().Be(originalItem.IsComplete);
            updatedItem.ScheduledDateTime.Should().Be(originalItem.ScheduledDateTime);
            updatedItem.DueDateTime.Should().Be(originalItem.DueDateTime);

            // Verify only the title was updated
            var getResponse = await _client.GetAsync($"/api/todoitems/{2}");
            var retrievedItem = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
            retrievedItem!.Title.Should().Be("Only Title Updated");
            retrievedItem.Description.Should().Be(originalItem.Description);
            retrievedItem.IsComplete.Should().Be(originalItem.IsComplete);
            retrievedItem.ScheduledDateTime.Should().Be(originalItem.ScheduledDateTime);
            retrievedItem.DueDateTime.Should().Be(originalItem.DueDateTime);
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldAllowSettingNullableFieldsToNull()
        {
            // Arrange
            var updatedTodoItem = new TodoItem
            {
                Id = 1,
                Title = "Todo with null dates",
                Description = "This todo has null date fields",
                IsComplete = false,
                ScheduledDateTime = null,
                DueDateTime = null
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/todoitems/{1}", updatedTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the response contains the updated item with null dates
            var updatedItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            updatedItem.Should().NotBeNull();
            updatedItem!.Title.Should().Be(updatedTodoItem.Title);
            updatedItem.Description.Should().Be(updatedTodoItem.Description);
            updatedItem.IsComplete.Should().Be(updatedTodoItem.IsComplete);
            updatedItem.ScheduledDateTime.Should().BeNull();
            updatedItem.DueDateTime.Should().BeNull();

            // Verify the fields were set to null
            var getResponse = await _client.GetAsync($"/api/todoitems/{1}");
            var retrievedItem = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
            retrievedItem!.ScheduledDateTime.Should().BeNull();
            retrievedItem.DueDateTime.Should().BeNull();
        }

        [Fact]
        public async Task UpdateTodoItem_ShouldAllowCompleteStatusChange()
        {
            // Arrange - Verify original status
            var originalResponse = await _client.GetAsync($"/api/todoitems/{1}");
            var originalItem = await originalResponse.Content.ReadFromJsonAsync<TodoItem>();
            originalItem!.IsComplete.Should().BeFalse();

            // Act - Update to complete
            var updatedTodoItem = new TodoItem
            {
                Id = 1,
                Title = originalItem.Title,
                Description = originalItem.Description,
                IsComplete = true,
                ScheduledDateTime = originalItem.ScheduledDateTime,
                DueDateTime = originalItem.DueDateTime
            };

            var response = await _client.PutAsJsonAsync($"/api/todoitems/{1}", updatedTodoItem);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the response contains the updated item with completed status
            var updatedItem = await response.Content.ReadFromJsonAsync<TodoItem>();
            updatedItem.Should().NotBeNull();
            updatedItem!.Title.Should().Be(originalItem.Title);
            updatedItem.Description.Should().Be(originalItem.Description);
            updatedItem.IsComplete.Should().BeTrue();
            updatedItem.ScheduledDateTime.Should().Be(originalItem.ScheduledDateTime);
            updatedItem.DueDateTime.Should().Be(originalItem.DueDateTime);

            // Verify the status was updated
            var getResponse = await _client.GetAsync($"/api/todoitems/{1}");
            var retrievedItem = await getResponse.Content.ReadFromJsonAsync<TodoItem>();
            retrievedItem!.IsComplete.Should().BeTrue();
        }
    }
}