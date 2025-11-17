using Microsoft.AspNetCore.Identity;

namespace TodoBackend.Infrastructure.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}