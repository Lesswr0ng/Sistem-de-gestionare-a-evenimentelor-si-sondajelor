using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Application.Services;

public interface IVoteService
{
     Task<IEnumerable<Vote>> GetVotesByPollAsync(int pollId);
     Task<IEnumerable<Vote>> GetVotesByUserAsync(string userId);
     Task<int> GetVoteCountForPollAsync(int pollId);
     Task<bool> HasUserVotedAsync(int pollId, string userId);
     Task<IEnumerable<Vote>> GetRecentVotesAsync(int pollId, int count = 10);
}