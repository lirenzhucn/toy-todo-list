using TodoBackend.Core.Entities;

namespace TodoBackend.Core.Interfaces
{
    public interface ITodoItemService
    {
        Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync(DateTime? scheduledDateTimeFrom = null, DateTime? scheduledDateTimeTo = null, DateTime? dueDateTimeFrom = null, DateTime? dueDateTimeTo = null);
        Task<TodoItem?> GetTodoItemByIdAsync(int id);
        Task<TodoItem> CreateTodoItemAsync(TodoItem todoItem);
        Task<TodoItem> UpdateTodoItemAsync(int id, TodoItem todoItem);
        Task<TodoItem?> DeleteTodoItemAsync(int id);
    }
}
