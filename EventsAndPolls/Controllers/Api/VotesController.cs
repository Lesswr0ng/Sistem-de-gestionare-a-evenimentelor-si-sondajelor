using EventsAndPolls.Application.Services;
using EventsAndPolls.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class VotesController : ControllerBase
{
     private readonly IVoteService _voteService;
     private readonly ILogger<VotesController> _logger;

     public VotesController(IVoteService voteService, ILogger<VotesController> logger)
     {
          _voteService = voteService;
          _logger = logger;
     }

     // GET: api/votes/poll/{pollId}
     [HttpGet("poll/{pollId}")]
     public async Task<ActionResult<IEnumerable<VoteViewModel>>> GetVotesByPoll(int pollId)
     {
          try
          {
               var votes = await _voteService.GetVotesByPollAsync(pollId);

               var result = votes.Select(v => new VoteViewModel
               {
                    Id = v.Id,
                    UserId = v.UserId,
                    PollId = v.PollId,
                    SelectedOptionIds = new List<int> { v.PollOptionId },
                    CreatedAt = v.CreatedAt
               }).ToList();

               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting votes for poll {PollId}", pollId);
               return StatusCode(500, new { error = "An error occurred while retrieving votes" });
          }
     }

     // GET: api/votes/user/{userId}
     [HttpGet("user/{userId}")]
     public async Task<ActionResult<IEnumerable<VoteViewModel>>> GetVotesByUser(string userId)
     {
          try
          {
               var votes = await _voteService.GetVotesByUserAsync(userId);

               var result = votes.Select(v => new VoteViewModel
               {
                    Id = v.Id,
                    UserId = v.UserId,
                    PollId = v.PollId,
                    SelectedOptionIds = new List<int> { v.PollOptionId },
                    CreatedAt = v.CreatedAt
               }).ToList();

               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting votes for user {UserId}", userId);
               return StatusCode(500, new { error = "An error occurred while retrieving votes" });
          }
     }

     // DELETE: api/votes/{id}
     [HttpDelete("{id}")]
     public async Task<IActionResult> DeleteVote(int id)
     {
          try
          {
               await _voteService.DeleteVoteAsync(id);
               return Ok(new { message = "Vote deleted successfully" });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error deleting vote {VoteId}", id);
               return StatusCode(500, new { error = "An error occurred while deleting the vote" });
          }
     }
}