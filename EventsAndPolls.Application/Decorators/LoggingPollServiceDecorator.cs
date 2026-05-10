using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Services;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Decorators;

public class LoggingPollServiceDecorator : IPollService
{
     private readonly IPollService _inner;
     private readonly ILogger<LoggingPollServiceDecorator> _logger;

     public LoggingPollServiceDecorator(IPollService inner, ILogger<LoggingPollServiceDecorator> logger)
     {
          _inner = inner;
          _logger = logger;
     }

     public async Task<PollDto> CreatePollAsync(CreatePollDto dto)
     {
          _logger.LogInformation("[Decorator:Logging] CreatePollAsync called — EventId: {EventId}, Question: {Question}",
              dto.EventId, dto.Question);

          var result = await _inner.CreatePollAsync(dto);

          _logger.LogInformation("[Decorator:Logging] CreatePollAsync completed — PollId: {PollId}", result.Id);
          return result;
     }

     public async Task<PollDto?> GetPollByIdAsync(int id)
     {
          _logger.LogInformation("[Decorator:Logging] GetPollByIdAsync called — PollId: {PollId}", id);

          var result = await _inner.GetPollByIdAsync(id);

          if (result == null)
               _logger.LogWarning("[Decorator:Logging] GetPollByIdAsync — Poll {PollId} not found", id);
          else
               _logger.LogInformation("[Decorator:Logging] GetPollByIdAsync completed — Poll: {Question}", result.Question);

          return result;
     }

     public async Task<IEnumerable<PollDto>> GetPollsByEventAsync(int eventId)
     {
          _logger.LogInformation("[Decorator:Logging] GetPollsByEventAsync called — EventId: {EventId}", eventId);

          var result = await _inner.GetPollsByEventAsync(eventId);
          var list = result.ToList();

          _logger.LogInformation("[Decorator:Logging] GetPollsByEventAsync completed — {Count} polls found", list.Count);
          return list;
     }

     public async Task<VoteResultDto> CastVoteAsync(CastVoteDto dto, string userId)
     {
          _logger.LogInformation("[Decorator:Logging] CastVoteAsync called — PollId: {PollId}, UserId: {UserId}, Options: [{Options}]",
              dto.PollId, userId, string.Join(", ", dto.SelectedOptionIds));

          var result = await _inner.CastVoteAsync(dto, userId);

          _logger.LogInformation("[Decorator:Logging] CastVoteAsync completed — Success: {Success}, TotalVotes: {TotalVotes}",
              result.Success, result.TotalVotes);

          return result;
     }

     public async Task<PollDto> GetPollResultsAsync(int pollId)
     {
          _logger.LogInformation("[Decorator:Logging] GetPollResultsAsync called — PollId: {PollId}", pollId);

          var result = await _inner.GetPollResultsAsync(pollId);

          _logger.LogInformation("[Decorator:Logging] GetPollResultsAsync completed — TotalVotes: {TotalVotes}", result.TotalVotes);
          return result;
     }

     public async Task DeletePollAsync(int id)
     {
          _logger.LogInformation("[Decorator:Logging] DeletePollAsync called — PollId: {PollId}", id);
          await _inner.DeletePollAsync(id);
          _logger.LogInformation("[Decorator:Logging] DeletePollAsync completed — PollId: {PollId}", id);
     }

     public async Task<PollDto> ClonePollAsync(ClonePollDto dto)
     {
          _logger.LogInformation("[Decorator:Logging] ClonePollAsync called — SourcePollId: {SourcePollId}, DeepClone: {DeepClone}",
              dto.SourcePollId, dto.DeepClone);

          var result = await _inner.ClonePollAsync(dto);

          _logger.LogInformation("[Decorator:Logging] ClonePollAsync completed — NewPollId: {NewPollId}", result.Id);
          return result;
     }
}