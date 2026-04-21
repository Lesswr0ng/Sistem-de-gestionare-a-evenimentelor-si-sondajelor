using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Services;
using EventsAndPolls.Domain.Composite;
using EventsAndPolls.Domain.Interfaces;
using EventsAndPolls.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class PollsController : ControllerBase
{
     private readonly IPollService _pollService;
     private readonly ILogger<PollsController> _logger;
     private readonly IPollRepository _pollRepository;

     public PollsController(IPollService pollService, ILogger<PollsController> logger, IPollRepository pollRepository)
     {
          _pollService = pollService;
          _logger = logger;
          _pollRepository = pollRepository;
     }

     [HttpGet("{id}")]
     public async Task<ActionResult<PollDto>> GetPoll(int id)
     {
          try
          {
               var poll = await _pollService.GetPollByIdAsync(id);
               if (poll == null)
                    return NotFound(new { error = $"Poll with ID {id} not found" });

               return Ok(poll);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpPost]
     public async Task<ActionResult<PollDto>> CreatePoll([FromBody] CreatePollDto createDto)
     {
          try
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               var result = await _pollService.CreatePollAsync(createDto);
               return CreatedAtAction(nameof(GetPoll), new { id = result.Id }, result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error creating poll");
               return StatusCode(500, new { error = ex.Message });
          }
     }

     [HttpPost("{id}/votes")]
     public async Task<ActionResult<VoteResultDto>> CastVote(int id, [FromBody] CastVoteDto voteDto)
     {
          try
          {
               if (id != voteDto.PollId)
                    return BadRequest(new { error = "Poll ID mismatch" });

               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               // In real app, get from authentication
               var userId = User.Identity?.Name ?? "anonymous-user";

               var result = await _pollService.CastVoteAsync(voteDto, userId);
               return Ok(result);
          }
          catch (ArgumentException ex)
          {
               return NotFound(new { error = ex.Message });
          }
          catch (InvalidOperationException ex)
          {
               return BadRequest(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error casting vote for poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpGet("{id}/results")]
     public async Task<ActionResult<PollDto>> GetPollResults(int id)
     {
          try
          {
               var results = await _pollService.GetPollResultsAsync(id);
               return Ok(results);
          }
          catch (ArgumentException ex)
          {
               return NotFound(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting results for poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

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
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpPost("clone")]
     public async Task<ActionResult<PollDto>> ClonePoll([FromBody] ClonePollDto cloneDto)
     {
          try
          {
               var result = await _pollService.ClonePollAsync(cloneDto);
               return CreatedAtAction(nameof(GetPoll), new { id = result.Id }, result);
          }
          catch (Exception ex)
          {
               return BadRequest(new { error = ex.Message });
          }
     }
     [HttpGet("{id}/tree")]
     public async Task<IActionResult> GetPollTree(int id)
     {
          try
          {
               var poll = await _pollRepository.GetByIdAsync(id);
               if (poll == null)
                    return NotFound(new { error = $"Poll with ID {id} not found" });

               var tree = PollTreeBuilder.BuildTree(poll);

               // Return a structured JSON representation of the composite tree
               var result = BuildTreeJson(tree);
               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error building tree for poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     // Recursively serialize the composite tree to an anonymous object
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

          // Leaf node
          return new
          {
               type = "option",
               id = node.Id,
               text = node.DisplayText
          };
     }
}