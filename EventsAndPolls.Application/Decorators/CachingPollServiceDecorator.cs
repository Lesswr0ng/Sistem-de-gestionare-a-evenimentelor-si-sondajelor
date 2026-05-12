using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Decorators;

public class CachingPollServiceDecorator : IPollService
{
     private readonly IPollService _inner;
     private readonly IMemoryCache _cache;
     private readonly ILogger<CachingPollServiceDecorator> _logger;

     private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

     private static string PollCacheKey(int pollId) => $"poll_{pollId}";
     private static string EventPollsCacheKey(int eventId) => $"event_polls_{eventId}";

     public CachingPollServiceDecorator(
         IPollService inner,
         IMemoryCache cache,
         ILogger<CachingPollServiceDecorator> logger)
     {
          _inner = inner;
          _cache = cache;
          _logger = logger;
     }

     public async Task<PollDto?> GetPollByIdAsync(int id)
     {
          var key = PollCacheKey(id);

          if (_cache.TryGetValue(key, out PollDto? cached))
          {
               _logger.LogInformation("[Decorator:Cache] HIT — GetPollByIdAsync PollId: {PollId}", id);
               return cached;
          }

          _logger.LogInformation("[Decorator:Cache] MISS — GetPollByIdAsync PollId: {PollId}", id);
          var result = await _inner.GetPollByIdAsync(id);

          if (result != null)
               _cache.Set(key, result, CacheDuration);

          return result;
     }

     public async Task<PollDto> UpdatePollAsync(UpdatePollDto dto)
     {
          var result = await _inner.UpdatePollAsync(dto);

          // Invalidate cache for this poll and its event's list
          _cache.Remove(PollCacheKey(dto.Id));
          _cache.Remove(EventPollsCacheKey(result.EventId));
          _logger.LogInformation("[Decorator:Cache] INVALIDATED — poll {PollId} after update", dto.Id);

          return result;
     }

     public async Task<IEnumerable<PollDto>> GetPollsByEventAsync(int eventId)
     {
          var key = EventPollsCacheKey(eventId);

          if (_cache.TryGetValue(key, out IEnumerable<PollDto>? cached))
          {
               _logger.LogInformation("[Decorator:Cache] HIT — GetPollsByEventAsync EventId: {EventId}", eventId);
               return cached!;
          }

          _logger.LogInformation("[Decorator:Cache] MISS — GetPollsByEventAsync EventId: {EventId}", eventId);
          var result = await _inner.GetPollsByEventAsync(eventId);
          var list = result.ToList();

          _cache.Set(key, (IEnumerable<PollDto>)list, CacheDuration);
          return list;
     }

     // NOT cached — vote counts change on every vote
     public async Task<PollDto> GetPollResultsAsync(int pollId)
     {
          _logger.LogInformation("[Decorator:Cache] SKIP — GetPollResultsAsync not cached (live vote counts)");
          return await _inner.GetPollResultsAsync(pollId);
     }

     public async Task<VoteResultDto> CastVoteAsync(CastVoteDto dto, string userId)
     {
          var poll = await _inner.GetPollByIdAsync(dto.PollId);

          var result = await _inner.CastVoteAsync(dto, userId);

          _cache.Remove(PollCacheKey(dto.PollId));
          if (poll != null)
               _cache.Remove(EventPollsCacheKey(poll.EventId));

          _logger.LogInformation("[Decorator:Cache] INVALIDATED — poll {PollId} and its event polls cache", dto.PollId);

          return result;
     }

     public async Task<PollDto> CreatePollAsync(CreatePollDto dto)
     {
          var result = await _inner.CreatePollAsync(dto);
          _cache.Remove(EventPollsCacheKey(dto.EventId));
          _logger.LogInformation("[Decorator:Cache] INVALIDATED — event polls cache for EventId: {EventId}", dto.EventId);
          return result;
     }

     public async Task DeletePollAsync(int id)
     {
          var poll = await _inner.GetPollByIdAsync(id);
          await _inner.DeletePollAsync(id);

          _cache.Remove(PollCacheKey(id));
          if (poll != null)
               _cache.Remove(EventPollsCacheKey(poll.EventId));

          _logger.LogInformation("[Decorator:Cache] INVALIDATED — poll {PollId} removed from cache", id);
     }

     public async Task<PollDto> ClonePollAsync(ClonePollDto dto)
     {
          var result = await _inner.ClonePollAsync(dto);
          _cache.Remove(EventPollsCacheKey(result.EventId));
          return result;
     }
}