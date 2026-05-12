namespace EventsAndPolls.Application.Observer;

// The event data published by the subject
public record PollCreatedEvent(int PollId, string Question, int EventId, DateTime CreatedAt);
public record VoteCastEvent(int PollId, int OptionId, string UserId, int TotalVotes, DateTime VotedAt);
public record PollClosedEvent(int PollId, string Question, int TotalVotes, DateTime ClosedAt);

// Observer interface — any subscriber implements this
public interface IPollObserver
{
     Task OnPollCreatedAsync(PollCreatedEvent e);
     Task OnVoteCastAsync(VoteCastEvent e);
     Task OnPollClosedAsync(PollClosedEvent e);
}

// Subject interface — the thing being observed
public interface IPollEventPublisher
{
     void Subscribe(IPollObserver observer);
     void Unsubscribe(IPollObserver observer);
     Task NotifyPollCreatedAsync(PollCreatedEvent e);
     Task NotifyVoteCastAsync(VoteCastEvent e);
     Task NotifyPollClosedAsync(PollClosedEvent e);
}