using Microsoft.EntityFrameworkCore;
using TodoBackend.Core.Entities;
using TodoBackend.Core.Interfaces;
using TodoBackend.Infrastructure.Data;

namespace TodoBackend.Infrastructure.Repositories
{
    public class TodoItemRepository : ITodoItemRepository
    {
        private readonly TodoContext _context;

        public TodoItemRepository(TodoContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TodoItem>> GetAllAsync(
            DateTime? scheduledDateTimeFrom = null,
            DateTime? scheduledDateTimeTo = null,
            DateTime? dueDateTimeFrom = null,
            DateTime? dueDateTimeTo = null)
        {
            var query = _context.TodoItems.AsQueryable();

            if (scheduledDateTimeFrom.HasValue)
            {
                query = query.Where(t => t.ScheduledDateTime >= scheduledDateTimeFrom.Value);
            }

            if (scheduledDateTimeTo.HasValue)
            {
                query = query.Where(t => t.ScheduledDateTime <= scheduledDateTimeTo.Value);
            }

            if (dueDateTimeFrom.HasValue)
            {
                query = query.Where(t => t.DueDateTime >= dueDateTimeFrom.Value);
            }

            if (dueDateTimeTo.HasValue)
            {
                query = query.Where(t => t.DueDateTime <= dueDateTimeTo.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<TodoItem?> GetByIdAsync(int id)
        {
            return await _context.TodoItems.FindAsync(id);
        }

        public async Task<TodoItem> AddAsync(TodoItem todoItem)
        {
            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
            return todoItem;
        }

        public async Task UpdateAsync(TodoItem todoItem)
        {
            _context.TodoItems.Update(todoItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TodoItem todoItem)
        {
            _context.TodoItems.Remove(todoItem);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TodoItems.AnyAsync(t => t.Id == id);
        }
    }
}