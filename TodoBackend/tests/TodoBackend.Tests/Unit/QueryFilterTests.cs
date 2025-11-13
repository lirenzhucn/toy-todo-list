using FluentAssertions;
using TodoBackend.Core.Entities;

namespace TodoBackend.Tests.Tests.Unit
{
    /// Unit tests for query filtering functionality on TodoItem objects.
    /// Tests various date-time filtering scenarios including:
    /// - Filtering by ScheduledDateTime (from/to ranges)
    /// - Filtering by DueDateTime (from/to ranges)
    /// - Combining multiple date filters
    /// - Handling null date values
    /// - Edge cases (no matches, no filters applied)
    public class QueryFilterTests
    {
        private readonly List<TodoItem> _testData;

        public QueryFilterTests()
        {
            _testData = new List<TodoItem>
            {
                new TodoItem
                {
                    Id = 1,
                    Title = "Todo 1",
                    ScheduledDateTime = DateTime.Parse("2024-01-15T10:00:00"),
                    DueDateTime = DateTime.Parse("2024-01-20T17:00:00")
                },
                new TodoItem
                {
                    Id = 2,
                    Title = "Todo 2",
                    ScheduledDateTime = DateTime.Parse("2024-02-15T14:00:00"),
                    DueDateTime = DateTime.Parse("2024-02-25T18:00:00")
                },
                new TodoItem
                {
                    Id = 3,
                    Title = "Todo 3",
                    ScheduledDateTime = null,
                    DueDateTime = null
                }
            };
        }

        [Fact]
        public void QueryFilter_ShouldFilterByScheduledDateTimeFrom()
        {
            // Arrange
            var fromDate = DateTime.Parse("2024-01-20T00:00:00");

            // Act
            var result = _testData.Where(t => t.ScheduledDateTime >= fromDate).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainSingle(t => t.Id == 2);
        }

        [Fact]
        public void QueryFilter_ShouldFilterByScheduledDateTimeTo()
        {
            // Arrange
            var toDate = DateTime.Parse("2024-01-20T00:00:00");

            // Act
            var result = _testData.Where(t => t.ScheduledDateTime <= toDate).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainSingle(t => t.Id == 1);
        }

        [Fact]
        public void QueryFilter_ShouldFilterByDueDateTimeFrom()
        {
            // Arrange
            var fromDate = DateTime.Parse("2024-02-20T00:00:00");

            // Act
            var result = _testData.Where(t => t.DueDateTime >= fromDate).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainSingle(t => t.Id == 2);
        }

        [Fact]
        public void QueryFilter_ShouldFilterByDueDateTimeTo()
        {
            // Arrange
            var toDate = DateTime.Parse("2024-01-25T00:00:00");

            // Act
            var result = _testData.Where(t => t.DueDateTime <= toDate).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainSingle(t => t.Id == 1);
        }

        [Fact]
        public void QueryFilter_ShouldFilterByBothScheduledAndDueDateTime()
        {
            // Arrange
            var scheduledFromDate = DateTime.Parse("2024-01-01T00:00:00");
            var dueFromDate = DateTime.Parse("2024-02-01T00:00:00");

            // Act
            var result = _testData
                .Where(t => t.ScheduledDateTime >= scheduledFromDate)
                .Where(t => t.DueDateTime >= dueFromDate)
                .ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainSingle(t => t.Id == 2);
        }

        [Fact]
        public void QueryFilter_ShouldHandleNullValuesCorrectly()
        {
            // Act - Query for items with null ScheduledDateTime
            var result = _testData.Where(t => t.ScheduledDateTime == null).ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainSingle(t => t.Id == 3);
        }

        [Fact]
        public void QueryFilter_ShouldReturnEmpty_WhenNoMatches()
        {
            // Arrange
            var futureDate = DateTime.Parse("2025-01-01T00:00:00");

            // Act
            var result = _testData.Where(t => t.ScheduledDateTime >= futureDate).ToList();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void QueryFilter_ShouldHandleMultipleFilters()
        {
            // Arrange
            var scheduledFrom = DateTime.Parse("2024-01-01T00:00:00");
            var scheduledTo = DateTime.Parse("2024-02-01T00:00:00");
            var dueFrom = DateTime.Parse("2024-01-15T00:00:00");
            var dueTo = DateTime.Parse("2024-02-15T00:00:00");

            // Act
            var result = _testData
                .Where(t => t.ScheduledDateTime >= scheduledFrom && t.ScheduledDateTime <= scheduledTo)
                .Where(t => t.DueDateTime >= dueFrom && t.DueDateTime <= dueTo)
                .ToList();

            // Assert
            result.Should().HaveCount(1);
            result.Should().ContainSingle(t => t.Id == 1);
        }

        [Fact]
        public void QueryFilter_ShouldReturnAllItems_WhenNoFiltersApplied()
        {
            // Act
            var result = _testData.ToList();

            // Assert
            result.Should().HaveCount(3);
            result.Should().Contain(t => t.Id == 1);
            result.Should().Contain(t => t.Id == 2);
            result.Should().Contain(t => t.Id == 3);
        }
    }
}
