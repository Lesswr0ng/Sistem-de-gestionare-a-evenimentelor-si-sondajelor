using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.Services;
using EventsAndPolls.Domain.Composite;
using EventsAndPolls.Domain.Interfaces;
using EventsAndPolls.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class PollsController : ControllerBase
{
     private readonly IPollService _pollService;
     private readonly IPollRepository _pollRepository;
     private readonly ILogger<PollsController> _logger;

     public PollsController(
         IPollService pollService,
         IPollRepository pollRepository,
         ILogger<PollsController> logger)
     {
          _pollService = pollService;
          _pollRepository = pollRepository;
          _logger = logger;
     }

     // Public — anyone can view polls
     [HttpGet("{id}")]
     public async Task<IActionResult> GetPoll(int id)
     {
          try
          {
               var poll = await _pollService.GetPollByIdAsync(id);
               if (poll == null) return NotFound(new { error = $"Poll {id} not found" });
               return Ok(poll);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpGet("{id}/results")]
     public async Task<IActionResult> GetPollResults(int id)
     {
          try
          {
               var results = await _pollService.GetPollResultsAsync(id);
               return Ok(results);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting results for poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpGet("{id}/tree")]
     public async Task<IActionResult> GetPollTree(int id)
     {
          try
          {
               var poll = await _pollRepository.GetByIdAsync(id);
               if (poll == null) return NotFound(new { error = $"Poll {id} not found" });

               var tree = PollTreeBuilder.BuildTree(poll);
               var result = BuildTreeJson(tree);
               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error building tree for poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     // Organizer only
     [HttpPost]
     [Authorize(Roles = Roles.Organizer)]
     public async Task<IActionResult> CreatePoll([FromBody] CreatePollDto dto)
     {
          try
          {
               var poll = await _pollService.CreatePollAsync(dto);
               return CreatedAtAction(nameof(GetPoll), new { id = poll.Id }, poll);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error creating poll");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpPost("clone")]
     [Authorize(Roles = Roles.Organizer)]
     public async Task<IActionResult> ClonePoll([FromBody] ClonePollDto dto)
     {
          try
          {
               var poll = await _pollService.ClonePollAsync(dto);
               return Ok(poll);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error cloning poll");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpDelete("{id}")]
     [Authorize(Roles = Roles.Organizer)]
     public async Task<IActionResult> DeletePoll(int id)
     {
          try
          {
               await _pollService.DeletePollAsync(id);
               return NoContent();
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error deleting poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     // Any authenticated user can vote
     [HttpPost("{id}/votes")]
     [Authorize]
     public async Task<IActionResult> CastVote(int id, [FromBody] CastVoteDto dto)
     {
          try
          {
               // Real userId from authenticated user — no more "anonymous-user"
               var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
               if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "You must be logged in to vote" });

               dto.PollId = id;
               var result = await _pollService.CastVoteAsync(dto, userId);
               return Ok(result);
          }
          catch (InvalidOperationException ex)
          {
               return BadRequest(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error casting vote on poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     private object BuildTreeJson(IPollComponent node)
     {
          if (node is PollOptionGroup group)
          {
               return new
               {
                    type = "group",
                    id = group.Id,
                    text = group.DisplayText,
                    children = group.Children.Select(BuildTreeJson).ToList()
               };
          }
          return new { type = "option", id = node.Id, text = node.DisplayText };
     }
}