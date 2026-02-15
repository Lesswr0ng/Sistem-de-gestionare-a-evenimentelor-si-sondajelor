using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;

namespace EventsAndPolls.Application.Services;

public class PollService : IPollService
{
     private readonly IPollRepository _pollRepository;
     private readonly IVoteRepository _voteRepository;

     public PollService(IPollRepository pollRepository, IVoteRepository voteRepository)
     {
          _pollRepository = pollRepository;
          _voteRepository = voteRepository;
     }

     public async Task<Poll> CreatePollAsync(int eventId, string question, List<string> options, bool allowMultipleChoices = false)
     {
          var poll = Poll.Create(question, eventId, null, allowMultipleChoices);

          foreach (var optionText in options)
          {
               poll.AddOption(optionText);
          }

          await _pollRepository.AddAsync(poll);
          return poll;
     }

     public async Task<Poll?> GetPollByIdAsync(int id)
     {
          return await _pollRepository.GetByIdAsync(id);
     }

     public async Task VoteAsync(int pollId, string userId, List<int> optionIds)
     {
          // Check if user already voted
          var hasVoted = await _voteRepository.HasUserVotedAsync(pollId, userId);
          if (hasVoted)
               throw new Exception("User already voted");

          // Get poll
          var poll = await _pollRepository.GetByIdAsync(pollId);
          if (poll == null || !poll.IsActive)
               throw new Exception("Poll not found or inactive");

          // Validate single/multiple choice
          if (!poll.AllowMultipleChoices && optionIds.Count > 1)
               throw new Exception("This poll allows only single choice");

          // Create votes
          foreach (var optionId in optionIds)
          {
               var vote = new Vote(userId, pollId, optionId);
               await _voteRepository.AddAsync(vote);
          }
     }

     public async Task<Dictionary<int, int>> GetPollResultsAsync(int pollId)
     {
          var poll = await _pollRepository.GetByIdAsync(pollId);
          if (poll == null)
               return new Dictionary<int, int>();

          var results = new Dictionary<int, int>();
          foreach (var option in poll.Options)
          {
               // In real app, you'd query the database
               results[option.Id] = 0; // Placeholder
          }

          return results;
     }
     public async Task DeletePollAsync(int id)
     {
          await _pollRepository.DeleteAsync(id);
     }

     public async Task<IEnumerable<Poll>> GetPollsByEventAsync(int eventId)
     {
          return await _pollRepository.GetPollsByEventAsync(eventId);
     }
}
