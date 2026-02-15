namespace EventsAndPolls.Domain.Entities;

public class Vote : BaseEntity
{
     public string UserId { get; private set; } = string.Empty;
     public int PollId { get; private set; }
     public int PollOptionId { get; private set; }

     // Navigation
     public virtual Poll Poll { get; private set; } = null!;
     public virtual PollOption PollOption { get; private set; } = null!;

     public Vote(string userId, int pollId, int pollOptionId)
     {
          UserId = userId;
          PollId = pollId;
          PollOptionId = pollOptionId;
     }

     private Vote() { }
}
