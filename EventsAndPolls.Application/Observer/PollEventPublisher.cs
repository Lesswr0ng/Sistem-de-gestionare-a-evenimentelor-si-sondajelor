using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Observer;

// The concrete subject — holds the subscriber list and fires notifications
public class PollEventPublisher : IPollEventPublisher
{
     private readonly List<IPollObserver> _observers = new();
     private readonly ILogger<PollEventPublisher> _logger;

     public PollEventPublisher(ILogger<PollEventPublisher> logger)
     {
          _logger = logger;
     }

     public void Subscribe(IPollObserver observer)
     {
          if (!_observers.Contains(observer))
          {
               _observers.Add(observer);
               _logger.LogInformation("[Observer] Subscribed: {Observer}", observer.GetType().Name);
          }
     }

     public void Unsubscribe(IPollObserver observer)
     {
          _observers.Remove(observer);
          _logger.LogInformation("[Observer] Unsubscribed: {Observer}", observer.GetType().Name);
     }

     public async Task NotifyPollCreatedAsync(PollCreatedEvent e)
     {
          _logger.LogInformation("[Observer] Publishing PollCreatedEvent for Poll {PollId}", e.PollId);
          foreach (var observer in _observers)
               await observer.OnPollCreatedAsync(e);
     }

     public async Task NotifyVoteCastAsync(VoteCastEvent e)
     {
          _logger.LogInformation("[Observer] Publishing VoteCastEvent for Poll {PollId}", e.PollId);
          foreach (var observer in _observers)
               await observer.OnVoteCastAsync(e);
     }

     public async Task NotifyPollClosedAsync(PollClosedEvent e)
     {
          _logger.LogInformation("[Observer] Publishing PollClosedEvent for Poll {PollId}", e.PollId);
          foreach (var observer in _observers)
               await observer.OnPollClosedAsync(e);
     }
}