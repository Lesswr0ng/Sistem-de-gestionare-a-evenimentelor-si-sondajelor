using Microsoft.AspNetCore.Identity;

namespace EventsAndPolls.Domain.Entities;

public class ApplicationUser : IdentityUser
{
     public string DisplayName { get; set; } = string.Empty;
     public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

     // Navigation — events created by this user
     public virtual ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
}
