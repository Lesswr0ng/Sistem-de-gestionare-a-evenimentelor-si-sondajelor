using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Services;

public class VoteService : IVoteService
{
     private readonly IVoteRepository _voteRepository;
     private readonly ILogger<VoteService> _logger;

     public VoteService(IVoteRepository voteRepository, ILogger<VoteService> logger)
     {
          _voteRepository = voteRepository;
          _logger = logger;
     }

     public async Task<IEnumerable<Vote>> GetVotesByPollAsync(int pollId)
     {
          try
          {
               return await _voteRepository.GetVotesByPollIdAsync(pollId);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting votes for poll {PollId}", pollId);
               throw;
          }
     }

     public async Task<IEnumerable<Vote>> GetVotesByUserAsync(string userId)
     {
          try
          {
               return await _voteRepository.GetVotesByUserIdAsync(userId);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting votes for user {UserId}", userId);
               throw;
          }
     }

     public async Task<int> GetVoteCountForPollAsync(int pollId)
     {
          try
          {
               return await _voteRepository.GetVoteCountAsync(pollId);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting vote count for poll {PollId}", pollId);
               throw;
          }
     }

     public async Task<bool> HasUserVotedAsync(int pollId, string userId)
     {
          try
          {
               return await _voteRepository.HasUserVotedAsync(pollId, userId);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error checking if user voted in poll {PollId}", pollId);
               throw;
          }
     }

     public async Task<IEnumerable<Vote>> GetRecentVotesAsync(int pollId, int count = 10)
     {
          try
          {
               var allVotes = await _voteRepository.GetAllAsync();
               return allVotes
                   .Where(v => v.PollId == pollId)
                   .OrderByDescending(v => v.CreatedAt)
                   .Take(count);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting recent votes for poll {PollId}", pollId);
               throw;
          }
     }
}