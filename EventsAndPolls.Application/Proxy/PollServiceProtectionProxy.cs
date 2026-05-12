using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Services;
using EventsAndPolls.Application.Strategy;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Proxy;

public class PollServiceProtectionProxy : IPollService
{
     private readonly IPollService _real;
     private readonly IVoteRepository _voteRepository;
     private readonly VoteValidator _validator;
     private readonly ILogger<PollServiceProtectionProxy> _logger;

     public PollServiceProtectionProxy(
         IPollService real,
         IVoteRepository voteRepository,
         VoteValidator validator,
         ILogger<PollServiceProtectionProxy> logger)
     {
          _real = real;
          _voteRepository = voteRepository;
          _validator = validator;
          _logger = logger;
     }

     public async Task<VoteResultDto> CastVoteAsync(CastVoteDto dto, string userId)
     {
          _logger.LogInformation("[Proxy] Validating vote — PollId: {PollId}, UserId: {UserId}", dto.PollId, userId);

          var poll = await _real.GetPollByIdAsync(dto.PollId);
          var hasVoted = poll != null && await _voteRepository.HasUserVotedAsync(dto.PollId, userId);

          // Build context and run all strategies
          var context = new VoteValidationContext(
              PollId: dto.PollId,
              PollIsActive: poll?.IsActive ?? false,
              PollClosesAt: poll?.ClosesAt,
              AllowMultipleChoices: poll?.AllowMultipleChoices ?? false,
              SelectedOptionIds: dto.SelectedOptionIds,
              ValidOptionIds: poll?.Options.Select(o => o.Id).ToList() ?? new(),
              UserHasAlreadyVoted: hasVoted,
              UserId: userId
          );

          var result = _validator.Validate(context);

          if (!result.IsValid)
          {
               _logger.LogWarning("[Proxy] ACCESS DENIED — {Reason}", result.ErrorMessage);
               throw new InvalidOperationException(result.ErrorMessage);
          }

          _logger.LogInformation("[Proxy] ACCESS GRANTED — delegating to real service");
          return await _real.CastVoteAsync(dto, userId);
     }

     public Task<PollDto> CreatePollAsync(CreatePollDto dto) => _real.CreatePollAsync(dto);
     public Task<PollDto?> GetPollByIdAsync(int id) => _real.GetPollByIdAsync(id);
     public Task<IEnumerable<PollDto>> GetPollsByEventAsync(int id) => _real.GetPollsByEventAsync(id);
     public Task<PollDto> GetPollResultsAsync(int pollId) => _real.GetPollResultsAsync(pollId);
     public Task DeletePollAsync(int id) => _real.DeletePollAsync(id);
     public Task<PollDto> ClonePollAsync(ClonePollDto dto) => _real.ClonePollAsync(dto);
     public Task<PollDto> UpdatePollAsync(UpdatePollDto dto) =>
    _real.UpdatePollAsync(dto);
}
