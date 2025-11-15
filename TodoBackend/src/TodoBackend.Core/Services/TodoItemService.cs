using TodoBackend.Core.Entities;
using TodoBackend.Core.Interfaces;

namespace TodoBackend.Core.Services
{
    public class TodoItemService : ITodoItemService
    {
        private readonly ITodoItemRepository _todoItemRepository;

        public TodoItemService(ITodoItemRepository todoItemRepository)
        {
            _todoItemRepository = todoItemRepository;
        }

        public async Task<IEnumerable<TodoItem>> GetAllTodoItemsAsync(
            DateTime? scheduledDateTimeFrom = null,
            DateTime? scheduledDateTimeTo = null,
            DateTime? dueDateTimeFrom = null,
            DateTime? dueDateTimeTo = null)
        {
            return await _todoItemRepository.GetAllAsync(
                scheduledDateTimeFrom,
                scheduledDateTimeTo,
                dueDateTimeFrom,
                dueDateTimeTo);
        }

        public async Task<TodoItem?> GetTodoItemByIdAsync(int id)
        {
            return await _todoItemRepository.GetByIdAsync(id);
        }

        public async Task<TodoItem> CreateTodoItemAsync(TodoItem todoItem)
        {
            return await _todoItemRepository.AddAsync(todoItem);
        }

        public async Task<TodoItem> UpdateTodoItemAsync(int id, TodoItem todoItem)
        {
            var existingItem = await _todoItemRepository.GetByIdAsync(id);
            if (existingItem == null)
            {
                throw new KeyNotFoundException($"TodoItem with id {id} not found.");
            }

            existingItem.Title = todoItem.Title;
            existingItem.Description = todoItem.Description;
            existingItem.IsComplete = todoItem.IsComplete;
            existingItem.ScheduledDateTime = todoItem.ScheduledDateTime;
            existingItem.DueDateTime = todoItem.DueDateTime;

            await _todoItemRepository.UpdateAsync(existingItem);
            return existingItem;
        }

        public async Task<TodoItem?> DeleteTodoItemAsync(int id)
        {
            var todoItem = await _todoItemRepository.GetByIdAsync(id);
            if (todoItem == null)
            {
                return null;
            }

            await _todoItemRepository.DeleteAsync(todoItem);
            return todoItem;
        }
    }
}
