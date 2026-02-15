using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Application.Services;

public interface IPollService
{
     Task<Poll> CreatePollAsync(int eventId, string question, List<string> options, bool allowMultipleChoices = false);
     Task<Poll?> GetPollByIdAsync(int id);
     Task VoteAsync(int pollId, string userId, List<int> optionIds);
     Task<Dictionary<int, int>> GetPollResultsAsync(int pollId);
     Task DeletePollAsync(int id);
     Task<IEnumerable<Poll>> GetPollsByEventAsync(int eventId);
}