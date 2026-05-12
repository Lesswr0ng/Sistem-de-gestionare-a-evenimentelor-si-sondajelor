using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Observer;

// Subscriber 1 — audit log: writes every domain event to the logger
public class AuditLogObserver : IPollObserver
{
     private readonly ILogger<AuditLogObserver> _logger;

     public AuditLogObserver(ILogger<AuditLogObserver> logger)
     {
          _logger = logger;
     }

     public Task OnPollCreatedAsync(PollCreatedEvent e)
     {
          _logger.LogInformation(
              "[AuditLog] Poll #{PollId} created — Question: \"{Question}\" | EventId: {EventId} | At: {At}",
              e.PollId, e.Question, e.EventId, e.CreatedAt);
          return Task.CompletedTask;
     }

     public Task OnVoteCastAsync(VoteCastEvent e)
     {
          _logger.LogInformation(
              "[AuditLog] Vote cast — Poll #{PollId} | Option #{OptionId} | User: {UserId} | TotalVotes: {Total} | At: {At}",
              e.PollId, e.OptionId, e.UserId, e.TotalVotes, e.VotedAt);
          return Task.CompletedTask;
     }

     public Task OnPollClosedAsync(PollClosedEvent e)
     {
          _logger.LogInformation(
              "[AuditLog] Poll #{PollId} closed — \"{Question}\" | Final vote count: {Total} | At: {At}",
              e.PollId, e.Question, e.TotalVotes, e.ClosedAt);
          return Task.CompletedTask;
     }
}

// Subscriber 2 — real-time stats: tracks vote counts in memory for fast dashboard reads
public class RealTimeStatsObserver : IPollObserver
{
     private readonly Dictionary<int, int> _voteCounts = new();
     private readonly ILogger<RealTimeStatsObserver> _logger;

     public RealTimeStatsObserver(ILogger<RealTimeStatsObserver> logger)
     {
          _logger = logger;
     }

     public Task OnPollCreatedAsync(PollCreatedEvent e)
     {
          _voteCounts[e.PollId] = 0;
          _logger.LogDebug("[RealTimeStats] Tracking started for Poll #{PollId}", e.PollId);
          return Task.CompletedTask;
     }

     public Task OnVoteCastAsync(VoteCastEvent e)
     {
          _voteCounts[e.PollId] = e.TotalVotes;
          _logger.LogDebug("[RealTimeStats] Poll #{PollId} now has {Total} votes", e.PollId, e.TotalVotes);
          return Task.CompletedTask;
     }

     public Task OnPollClosedAsync(PollClosedEvent e)
     {
          _logger.LogInformation(
              "[RealTimeStats] Poll #{PollId} finalized with {Total} total votes", e.PollId, e.TotalVotes);
          return Task.CompletedTask;
     }

     public int GetVoteCount(int pollId) =>
         _voteCounts.TryGetValue(pollId, out var count) ? count : 0;
}

// Subscriber 3 — notification observer: simulates sending alerts when polls close or hit milestones
public class NotificationObserver : IPollObserver
{
     private readonly ILogger<NotificationObserver> _logger;
     private const int MilestoneInterval = 10; // notify every 10 votes

     public NotificationObserver(ILogger<NotificationObserver> logger)
     {
          _logger = logger;
     }

     public Task OnPollCreatedAsync(PollCreatedEvent e)
     {
          _logger.LogInformation(
              "[Notification] 🆕 New poll available: \"{Question}\" (Poll #{PollId})", e.Question, e.PollId);
          return Task.CompletedTask;
     }

     public Task OnVoteCastAsync(VoteCastEvent e)
     {
          if (e.TotalVotes % MilestoneInterval == 0)
          {
               _logger.LogInformation(
                   "[Notification] 🎉 Poll #{PollId} reached {Total} votes!", e.PollId, e.TotalVotes);
          }
          return Task.CompletedTask;
     }

     public Task OnPollClosedAsync(PollClosedEvent e)
     {
          _logger.LogInformation(
              "[Notification] 🔒 Poll \"{Question}\" has closed. Final result: {Total} votes.",
              e.Question, e.TotalVotes);
          return Task.CompletedTask;
     }
}