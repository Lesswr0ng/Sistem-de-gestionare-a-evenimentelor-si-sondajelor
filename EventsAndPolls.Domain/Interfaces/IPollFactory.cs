using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Interfaces;

public interface IPoll
{
     string GetPollType();
     string GetPollDescription();
     bool ValidateVote(List<int> selectedOptionIds);
}

public class SingleChoicePoll : Poll, IPoll
{
     public SingleChoicePoll(string question, int eventId) : base(question, eventId, false) { }
     public string GetPollType() => "Alege o singură opțiune";
     public string GetPollDescription() => "Poți selecta DOAR o opțiune din listă";
     public bool ValidateVote(List<int> selectedOptionIds) => selectedOptionIds.Count == 1;
}

public class MultipleChoicePoll : Poll, IPoll
{
     public MultipleChoicePoll(string question, int eventId) : base(question, eventId, true) { }
     public string GetPollType() => "Alege mai multe opțiuni";
     public string GetPollDescription() => "Poți selecta MULTIPLE opțiuni din listă";
     public bool ValidateVote(List<int> selectedOptionIds) => selectedOptionIds.Count >= 1;
}

public abstract class PollCreator
{
     public abstract IPoll CreatePoll(string question, int eventId);

     public IPoll CreateAndSetupPoll(string question, int eventId, List<string> options)
     {
          var poll = CreatePoll(question, eventId);

          foreach (var option in options)
          {
               if (poll is Poll concretePoll)
               {
                    concretePoll.AddOption(option);
               }
          }

          Console.WriteLine($"✅ Poll created using Factory Method: {poll.GetPollType()}");
          return poll;
     }
}
public class SingleChoicePollCreator : PollCreator
{
     public override IPoll CreatePoll(string question, int eventId)
     {
          return new SingleChoicePoll(question, eventId);
     }
}

public class MultipleChoicePollCreator : PollCreator
{
     public override IPoll CreatePoll(string question, int eventId)
     {
          return new MultipleChoicePoll(question, eventId);
     }
}
