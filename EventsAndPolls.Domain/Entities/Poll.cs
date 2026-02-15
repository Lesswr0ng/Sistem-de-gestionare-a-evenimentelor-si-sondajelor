using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Entities;

public class Poll : BaseEntity
{
     public string Question { get; private set; } = string.Empty;
     public int EventId { get; private set; }
     public bool IsActive { get; private set; } = true;
     public DateTime? ClosesAt { get; private set; }
     public bool AllowMultipleChoices { get; private set; }

     // Navigation properties
     public virtual Event Event { get; private set; } = null!;
     public virtual ICollection<PollOption> Options { get; private set; } = new List<PollOption>();
     public virtual ICollection<Vote> Votes { get; private set; } = new List<Vote>();

     private Poll() { }

     public static Poll Create(string question, int eventId, DateTime? closesAt = null, bool allowMultipleChoices = false)
     {
          return new Poll
          {
               Question = question,
               EventId = eventId,
               ClosesAt = closesAt,
               AllowMultipleChoices = allowMultipleChoices
          };
     }

     public void AddOption(string text)
     {
          Options.Add(new PollOption(text, Id));
     }

     public void Close() => IsActive = false;
}