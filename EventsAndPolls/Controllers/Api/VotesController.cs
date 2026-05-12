using EventsAndPolls.Application.ChainOfResponsibility;
using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.Services;
using EventsAndPolls.Application.ViewModels;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class VotesController : ControllerBase
{
     private readonly IVoteService _voteService;
     private readonly IPollService _pollService;
     private readonly IVoteRepository _voteRepository;
     private readonly ILoggerFactory _loggerFactory;
     private readonly ILogger<VotesController> _logger;

     public VotesController(
         IVoteService voteService,
         IPollService pollService,
         IVoteRepository voteRepository,
         ILoggerFactory loggerFactory,
         ILogger<VotesController> logger)
     {
          _voteService = voteService;
          _pollService = pollService;
          _voteRepository = voteRepository;
          _loggerFactory = loggerFactory;
          _logger = logger;
     }

     // POST: api/votes  — Chain of Responsibility guards this before the service touches it
     [HttpPost]
     [Authorize]
     public async Task<IActionResult> CastVote([FromBody] CastVoteDto dto)
     {
          try
          {
               var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
               if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "You must be logged in to vote" });

               // Build and run the chain — throws if any handler blocks the request
               var chain = VoteHandlerChainFactory.Build(_pollService, _voteRepository, _loggerFactory);
               var request = new VoteRequest { Dto = dto, UserId = userId };
               await chain.HandleAsync(request);

               // Chain passed — cast the vote
               var result = await _pollService.CastVoteAsync(dto, userId);
               return Ok(result);
          }
          catch (InvalidOperationException ex)
          {
               return BadRequest(new { error = ex.Message });
          }
          catch (ArgumentException ex)
          {
               return NotFound(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error casting vote on poll {PollId}", dto.PollId);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpGet("poll/{pollId}")]
     public async Task<IActionResult> GetVotesByPoll(int pollId)
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
               });
               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting votes for poll {PollId}", pollId);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpGet("user/{userId}")]
     public async Task<IActionResult> GetVotesByUser(string userId)
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
               });
               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting votes for user {UserId}", userId);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }
}