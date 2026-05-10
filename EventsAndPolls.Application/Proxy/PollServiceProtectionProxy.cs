using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Services;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Proxy;

public class PollServiceProtectionProxy : IPollService
{
     private readonly IPollService _real;
     private readonly IVoteRepository _voteRepository;
     private readonly ILogger<PollServiceProtectionProxy> _logger;

     public PollServiceProtectionProxy(
         IPollService real,
         IVoteRepository voteRepository,
         ILogger<PollServiceProtectionProxy> logger)
     {
          _real = real;
          _voteRepository = voteRepository;
          _logger = logger;
     }

     public async Task<VoteResultDto> CastVoteAsync(CastVoteDto dto, string userId)
     {
          _logger.LogInformation("[Proxy:Protection] Checking access for CastVoteAsync — PollId: {PollId}, UserId: {UserId}",
              dto.PollId, userId);

          // Guard 1 — poll must exist
          var poll = await _real.GetPollByIdAsync(dto.PollId);
          if (poll == null)
          {
               _logger.LogWarning("[Proxy:Protection] ACCESS DENIED — Poll {PollId} does not exist", dto.PollId);
               throw new ArgumentException($"Poll {dto.PollId} does not exist");
          }

          // Guard 2 — poll must be active
          if (!poll.IsActive)
          {
               _logger.LogWarning("[Proxy:Protection] ACCESS DENIED — Poll {PollId} is inactive", dto.PollId);
               throw new InvalidOperationException("Cannot vote on an inactive poll");
          }

          // Guard 3 — poll must not be past its closing time
          if (poll.ClosesAt.HasValue && poll.ClosesAt.Value < DateTime.UtcNow)
          {
               _logger.LogWarning("[Proxy:Protection] ACCESS DENIED — Poll {PollId} closed at {ClosesAt}",
                   dto.PollId, poll.ClosesAt);
               throw new InvalidOperationException($"This poll closed on {poll.ClosesAt.Value:yyyy-MM-dd HH:mm} UTC");
          }

          // Guard 4 — user must not have already voted
          var hasVoted = await _voteRepository.HasUserVotedAsync(dto.PollId, userId);
          if (hasVoted)
          {
               _logger.LogWarning("[Proxy:Protection] ACCESS DENIED — User {UserId} already voted on Poll {PollId}",
                   userId, dto.PollId);
               throw new InvalidOperationException("You have already voted on this poll");
          }

          // Guard 5 — multiple choice validation
          if (!poll.AllowMultipleChoices && dto.SelectedOptionIds.Count > 1)
               throw new InvalidOperationException("This poll only allows a single choice");

          _logger.LogInformation("[Proxy:Protection] ACCESS GRANTED — delegating to real service");

          // All guards passed — delegate to the real service
          return await _real.CastVoteAsync(dto, userId);
     }

     public Task<PollDto> CreatePollAsync(CreatePollDto dto) =>
         _real.CreatePollAsync(dto);

     public Task<PollDto?> GetPollByIdAsync(int id) =>
         _real.GetPollByIdAsync(id);

     public Task<IEnumerable<PollDto>> GetPollsByEventAsync(int eventId) =>
         _real.GetPollsByEventAsync(eventId);

     public Task<PollDto> GetPollResultsAsync(int pollId) =>
         _real.GetPollResultsAsync(pollId);

     public Task DeletePollAsync(int id) =>
         _real.DeletePollAsync(id);

     public Task<PollDto> ClonePollAsync(ClonePollDto dto) =>
         _real.ClonePollAsync(dto);
}
