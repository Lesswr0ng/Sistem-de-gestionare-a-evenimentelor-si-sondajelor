using System.Text.RegularExpressions;
using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Entities;

public class PollOption : BaseEntity
{
     public string Text { get; set; } = string.Empty;
     public int PollId { get; private set; }

     public int? GroupId { get; private set; }
     public virtual PollOptionGroup? Group { get; private set; }

     // Navigation
     public virtual Poll Poll { get; set; } = null!;

     public PollOption(string text, int pollId, int? groupId = null)
     {
          Text = text;
          PollId = pollId;
          GroupId = groupId;
     }

     public PollOption() { }
}