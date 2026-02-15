using EventsAndPolls.Application.Services;
using EventsAndPolls.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class PollsController : ControllerBase
{
     private readonly IPollService _pollService;
     private readonly IVoteService _voteService;
     private readonly ILogger<PollsController> _logger;

     public PollsController(
         IPollService pollService,
         IVoteService voteService,
         ILogger<PollsController> logger)
     {
          _pollService = pollService;
          _voteService = voteService;
          _logger = logger;
     }

     // GET: api/polls/{id}
     [HttpGet("{id}")]
     public async Task<ActionResult<PollViewModel>> GetPoll(int id)
     {
          try
          {
               var poll = await _pollService.GetPollByIdAsync(id);

               if (poll == null)
                    return NotFound(new { error = $"Poll with ID {id} not found" });

               var result = new PollViewModel
               {
                    Id = poll.Id,
                    Question = poll.Question,
                    EventId = poll.EventId,
                    IsActive = poll.IsActive,
                    AllowMultipleChoices = poll.AllowMultipleChoices,
                    TotalVotes = poll.Votes?.Count ?? 0,
                    Options = poll.Options?.Select(o => new PollOptionViewModel
                    {
                         Id = o.Id,
                         Text = o.Text,
                         VoteCount = poll.Votes?.Count(v => v.PollOptionId == o.Id) ?? 0
                    }).ToList() ?? new List<PollOptionViewModel>()
               };

               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred while retrieving the poll" });
          }
     }

     // POST: api/polls
     [HttpPost]
     public async Task<ActionResult<PollViewModel>> CreatePoll([FromBody] CreatePollViewModel model)
     {
          try
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               var createdPoll = await _pollService.CreatePollAsync(
                   model.EventId,
                   model.Question,
                   model.Options,
                   model.AllowMultipleChoices);

               var result = new PollViewModel
               {
                    Id = createdPoll.Id,
                    Question = createdPoll.Question,
                    EventId = createdPoll.EventId,
                    IsActive = createdPoll.IsActive,
                    AllowMultipleChoices = createdPoll.AllowMultipleChoices,
                    Options = createdPoll.Options?.Select(o => new PollOptionViewModel
                    {
                         Id = o.Id,
                         Text = o.Text
                    }).ToList() ?? new List<PollOptionViewModel>()
               };

               return CreatedAtAction(nameof(GetPoll), new { id = createdPoll.Id }, result);
          }
          catch (ArgumentException ex)
          {
               return BadRequest(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error creating poll");
               return StatusCode(500, new { error = "An error occurred while creating the poll" });
          }
     }

     // POST: api/polls/{id}/votes
     [HttpPost("{id}/votes")]
     public async Task<IActionResult> CastVote(int id, [FromBody] VoteViewModel model)
     {
          try
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               if (model.SelectedOptionIds == null || !model.SelectedOptionIds.Any())
                    return BadRequest(new { error = "At least one option must be selected" });

               var userId = "voter-user"; // In real app, get from authentication

               await _pollService.VoteAsync(id, userId, model.SelectedOptionIds);

               return Ok(new { message = "Vote recorded successfully" });
          }
          catch (InvalidOperationException ex)
          {
               return BadRequest(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error casting vote for poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred while casting the vote" });
          }
     }

     // GET: api/polls/{id}/results
     [HttpGet("{id}/results")]
     public async Task<ActionResult<PollResultsViewModel>> GetPollResults(int id)
     {
          try
          {
               var poll = await _pollService.GetPollByIdAsync(id);

               if (poll == null)
                    return NotFound(new { error = $"Poll with ID {id} not found" });

               var totalVotes = poll.Votes?.Count ?? 0;

               var results = new PollResultsViewModel
               {
                    PollId = poll.Id,
                    Question = poll.Question,
                    TotalVotes = totalVotes,
                    OptionResults = poll.Options?.Select(o => new PollOptionResultViewModel
                    {
                         OptionId = o.Id,
                         Text = o.Text,
                         VoteCount = poll.Votes?.Count(v => v.PollOptionId == o.Id) ?? 0,
                         Percentage = totalVotes > 0
                            ? Math.Round((poll.Votes?.Count(v => v.PollOptionId == o.Id) ?? 0) * 100.0 / totalVotes, 2)
                            : 0
                    }).OrderByDescending(r => r.VoteCount).ToList() ?? new List<PollOptionResultViewModel>()
               };

               return Ok(results);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting results for poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred while retrieving poll results" });
          }
     }

     // DELETE: api/polls/{id}
     [HttpDelete("{id}")]
     public async Task<IActionResult> DeletePoll(int id)
     {
          try
          {
               await _pollService.DeletePollAsync(id);
               return Ok(new { message = "Poll deleted successfully" });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error deleting poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred while deleting the poll" });
          }
     }
}