using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Entities;

public class Event : BaseEntity
{
     public string Title { get; private set; } = string.Empty;
     public string Description { get; private set; } = string.Empty;
     public DateTime StartDate { get; private set; }
     public DateTime EndDate { get; private set; }
     public string Location { get; private set; } = string.Empty;
     public int MaxParticipants { get; private set; }
     public bool IsActive { get; private set; } = true;
     public string OrganizerId { get; private set; } = string.Empty;

     public virtual ICollection<Poll> Polls { get; private set; } = new List<Poll>();

     private Event() { }

     public static Event Create(string title, string description, DateTime startDate,
                                DateTime endDate, string location, int maxParticipants, string organizerId)
     {
          return new Event
          {
               Title = title,
               Description = description,
               StartDate = startDate,
               EndDate = endDate,
               Location = location,
               MaxParticipants = maxParticipants,
               OrganizerId = organizerId
          };
     }

     public void Update(string title, string description, DateTime startDate,
                       DateTime endDate, string location, int maxParticipants)
     {
          Title = title;
          Description = description;
          StartDate = startDate;
          EndDate = endDate;
          Location = location;
          MaxParticipants = maxParticipants;
     }

     public void Deactivate() => IsActive = false;
}