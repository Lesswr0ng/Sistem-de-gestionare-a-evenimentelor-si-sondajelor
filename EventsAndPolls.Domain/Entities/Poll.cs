using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Entities;

public class Poll : BaseEntity
{
     public static PollBuilder CreateBuilder(string question, int eventId)
     {
          return new PollBuilder(question, eventId);
     }
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

     public Poll(string question, int eventId, bool allowMultipleChoices)
     {
          Question = question;
          EventId = eventId;
          AllowMultipleChoices = allowMultipleChoices;
          IsActive = true;
          CreatedAt = DateTime.UtcNow;
     }
     // Shallow Clone - copiază doar proprietățile de bază, fără relații
     public Poll Clone()
     {
          return new Poll
          {
               Question = this.Question,
               EventId = this.EventId, // Poate fi schimbat pentru alt eveniment
               AllowMultipleChoices = this.AllowMultipleChoices,
               ClosesAt = this.ClosesAt,
               IsActive = true, // Sondajul nou e activ
               CreatedAt = DateTime.UtcNow
          };
     }

     // Deep Clone - copiază tot, inclusiv opțiunile
     public Poll DeepClone()
     {
          var clonedPoll = new Poll
          {
               Question = this.Question,
               EventId = this.EventId,
               AllowMultipleChoices = this.AllowMultipleChoices,
               ClosesAt = this.ClosesAt,
               IsActive = true,
               CreatedAt = DateTime.UtcNow
          };

          // Copiază și opțiunile
          foreach (var option in this.Options)
          {
               clonedPoll.AddOption(option.Text);
          }

          return clonedPoll;
     }

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
     public virtual bool ValidateVote(List<int> selectedOptionIds)
     {
          if (!AllowMultipleChoices && selectedOptionIds.Count > 1)
               return false;

          return selectedOptionIds.Count > 0 &&
                 selectedOptionIds.All(id => Options.Any(o => o.Id == id));
     }

     public void AddOption(string text)
     {
          Options.Add(new PollOption(text, Id));
     }
     public void SetEventId(int newEventId)
     {
          EventId = newEventId;
     }

     public void SetQuestion(string newQuestion)
     {
          Question = newQuestion;
     }
}

public class PollBuilder
{
     private readonly string _question;
     private readonly int _eventId;
     private readonly List<string> _options = new();
     private bool _allowMultipleChoices;
     private DateTime? _closesAt;

     public PollBuilder(string question, int eventId)
     {
          _question = question;
          _eventId = eventId;
     }

     public PollBuilder WithOptions(List<string> options)
     {
          _options.AddRange(options);
          return this;
     }

     public PollBuilder AllowMultipleSelections(bool allow = true)
     {
          _allowMultipleChoices = allow;
          return this;
     }

     public PollBuilder ClosesAt(DateTime? closesAt)
     {
          _closesAt = closesAt;
          return this;
     }

     public Poll Build()
     {
          var poll = Poll.Create(_question, _eventId, _closesAt, _allowMultipleChoices);

          foreach (var option in _options)
          {
               poll.AddOption(option);
          }

          return poll;
     }
}