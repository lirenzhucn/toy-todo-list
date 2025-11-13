namespace TodoBackend.Core.Entities
{
    public class TodoItem
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public bool IsComplete { get; set; } = false;
        public DateTime? ScheduledDateTime { get; set; }
        public DateTime? DueDateTime { get; set; }
    }
}