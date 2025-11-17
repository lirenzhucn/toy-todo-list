using TodoBackend.Core.Entities;

namespace TodoBackend.Core.Interfaces
{
    public interface ITodoItemRepository
    {
        Task<IEnumerable<TodoItem>> GetAllAsync(string userId, DateTime? scheduledDateTimeFrom = null, DateTime? scheduledDateTimeTo = null, DateTime? dueDateTimeFrom = null, DateTime? dueDateTimeTo = null);
        Task<TodoItem?> GetByIdAsync(int id, string userId);
        Task<TodoItem> AddAsync(TodoItem todoItem);
        Task UpdateAsync(TodoItem todoItem);
        Task DeleteAsync(TodoItem todoItem);
        Task<bool> ExistsAsync(int id, string userId);
    }
}