using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Entities;

public class PollOption : BaseEntity
{
     public string Text { get; private set; } = string.Empty;
     public int PollId { get; private set; }

     // Navigation
     public virtual Poll Poll { get; private set; } = null!;

     public PollOption(string text, int pollId)
     {
          Text = text;
          PollId = pollId;
     }

     private PollOption() { }
}