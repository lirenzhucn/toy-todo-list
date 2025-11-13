using FluentAssertions;
using TodoBackend.Core.Entities;

namespace TodoBackend.Tests.Tests.Unit
{
    public class TodoItemTests
    {
        [Fact]
        public void TodoItem_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var todoItem = new TodoItem();

            // Assert
            todoItem.Id.Should().Be(0);
            todoItem.Title.Should().BeNull();
            todoItem.Description.Should().BeNull();
            todoItem.IsComplete.Should().BeFalse();
            todoItem.ScheduledDateTime.Should().BeNull();
            todoItem.DueDateTime.Should().BeNull();
        }

        [Fact]
        public void TodoItem_ShouldAllowSettingAllProperties()
        {
            // Arrange
            var testDate = DateTime.Parse("2024-01-15T10:00:00");
            var dueDate = DateTime.Parse("2024-01-20T17:00:00");

            // Act
            var todoItem = new TodoItem
            {
                Id = 1,
                Title = "Test Todo",
                Description = "Test Description",
                IsComplete = true,
                ScheduledDateTime = testDate,
                DueDateTime = dueDate
            };

            // Assert
            todoItem.Id.Should().Be(1);
            todoItem.Title.Should().Be("Test Todo");
            todoItem.Description.Should().Be("Test Description");
            todoItem.IsComplete.Should().BeTrue();
            todoItem.ScheduledDateTime.Should().Be(testDate);
            todoItem.DueDateTime.Should().Be(dueDate);
        }

        [Fact]
        public void TodoItem_ShouldAllowNullableFields()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Title = "Todo with null dates",
                ScheduledDateTime = null,
                DueDateTime = null
            };

            // Act & Assert
            todoItem.ScheduledDateTime.Should().BeNull();
            todoItem.DueDateTime.Should().BeNull();
        }

        [Fact]
        public void TodoItem_ShouldAllowEmptyTitle()
        {
            // Arrange
            var todoItem = new TodoItem
            {
                Title = string.Empty,
                Description = "Test Description"
            };

            // Act & Assert
            todoItem.Title.Should().Be(string.Empty);
            todoItem.Description.Should().Be("Test Description");
        }

        [Fact]
        public void TodoItem_ShouldAllowLongTitleAndDescription()
        {
            // Arrange
            var longTitle = new string('A', 1000);
            var longDescription = new string('B', 5000);

            // Act
            var todoItem = new TodoItem
            {
                Title = longTitle,
                Description = longDescription
            };

            // Assert
            todoItem.Title.Should().Be(longTitle);
            todoItem.Description.Should().Be(longDescription);
        }
    }
}