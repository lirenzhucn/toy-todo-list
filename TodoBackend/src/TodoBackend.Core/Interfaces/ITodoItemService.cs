using TodoBackend.Core.Entities;

namespace TodoBackend.Core.Interfaces
{
    public interface ITodoItemService
    {
        Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync(string userId, DateTime? scheduledDateTimeFrom = null, DateTime? scheduledDateTimeTo = null, DateTime? dueDateTimeFrom = null, DateTime? dueDateTimeTo = null);
        Task<TodoItem?> GetTodoItemByIdAsync(int id, string userId);
        Task<TodoItem> CreateTodoItemAsync(TodoItem todoItem, string userId);
        Task<TodoItem> UpdateTodoItemAsync(int id, TodoItem todoItem, string userId);
        Task<TodoItem?> DeleteTodoItemAsync(int id, string userId);
    }
}
