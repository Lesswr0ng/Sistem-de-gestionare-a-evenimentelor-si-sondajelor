using EventsAndPolls.Application.Command;
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
     private readonly PollCommandInvoker _invoker;
     private readonly ILoggerFactory _loggerFactory;
     private readonly INotificationService _notificationService;

     public PollsController(
         IPollService pollService,
         IPollRepository pollRepository,
         PollCommandInvoker invoker,
         ILoggerFactory loggerFactory,
         ILogger<PollsController> logger,
         INotificationService notificationService)
     {
          _pollService = pollService;
          _pollRepository = pollRepository;
          _logger = logger;
          _invoker = invoker;
          _loggerFactory = loggerFactory;
          _notificationService = notificationService;
     }

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
               return Ok(BuildTreeJson(tree));
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error building tree for poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpPost]
     [Authorize(Roles = Roles.Organizer)]
     public async Task<IActionResult> CreatePoll([FromBody] CreatePollDto dto)
     {
          try
          {
               var cmd = new CreatePollCommand(
                   _pollService, dto,
                   _loggerFactory.CreateLogger<CreatePollCommand>());

               await _invoker.ExecuteAsync(cmd);

               var scopeFactory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
               var capturedOrganizerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
               var capturedPollId = cmd.Result!.Id;
               var capturedEventId = cmd.Result!.EventId;

               _ = Task.Run(async () =>
               {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    try
                    {
                         var pollService = scope.ServiceProvider.GetRequiredService<IPollService>();
                         var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                         var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                         var poll = await pollService.GetPollByIdAsync(capturedPollId);
                         var @event = await eventService.GetEventByIdAsync(capturedEventId);

                         if (poll != null)
                              await notificationService.NotifyPollCreatedAsync(
                                  poll, @event?.Title ?? string.Empty,
                                  new List<string> { capturedOrganizerId });
                    }
                    catch (Exception ex)
                    {
                         _logger.LogError(ex, "Notification error after poll creation");
                    }
               });

               return CreatedAtAction(nameof(GetPoll), new { id = cmd.Result!.Id }, cmd.Result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error creating poll");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpPut("{id}")]
     [Authorize(Roles = Roles.Organizer)]
     public async Task<IActionResult> UpdatePoll(int id, [FromBody] UpdatePollDto dto)
     {
          var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

          try
          {
               dto.Id = id;
               var poll = await _pollService.UpdatePollAsync(dto);

               if (!poll.IsActive)
               {
                    var scopeFactory = HttpContext.RequestServices
                        .GetRequiredService<IServiceScopeFactory>();
                    var capturedUserId = userId;
                    var capturedPollId = id;

                    _ = Task.Run(async () =>
                    {
                         // Create a new scope with its own DbContext — not the disposed one
                         await using var scope = scopeFactory.CreateAsyncScope();
                         try
                         {
                              var pollService = scope.ServiceProvider.GetRequiredService<IPollService>();
                              var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                              var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                              var freshPoll = await pollService.GetPollResultsAsync(capturedPollId);
                              var @event = await eventService.GetEventByIdAsync(freshPoll.EventId);
                              var organizerId = @event?.OrganizerId ?? string.Empty;

                              await notificationService.NotifyVoteCastAsync(freshPoll, capturedUserId, organizerId);
                         }
                         catch (Exception ex)
                         {
                              _logger.LogError(ex, "Notification error after vote");
                         }
                    });
               }

               return Ok(poll);
          }
          catch (ArgumentException ex)
          {
               return NotFound(new { error = ex.Message });
          }
          catch (InvalidOperationException ex)
          {
               // Integrity violation — e.g. trying to delete option with votes
               return BadRequest(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error updating poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     // Command: DeletePoll — snapshots then deletes; undo recreates it
     [HttpDelete("{id}")]
     [Authorize(Roles = Roles.Organizer)]
     public async Task<IActionResult> DeletePoll(int id)
     {
          try
          {
               var cmd = new DeletePollCommand(
                   _pollService, id,
                   _loggerFactory.CreateLogger<DeletePollCommand>());

               await _invoker.ExecuteAsync(cmd);

               return NoContent();
          }
          catch (ArgumentException ex)
          {
               return NotFound(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error deleting poll {PollId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpPost("clone")]
     [Authorize(Roles = Roles.Organizer)]
     public async Task<IActionResult> ClonePoll([FromBody] ClonePollDto dto)
     {
          try
          {
               var cmd = new ClonePollCommand(
                   _pollService, dto,
                   _loggerFactory.CreateLogger<ClonePollCommand>());

               await _invoker.ExecuteAsync(cmd);

               return Ok(cmd.Result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error cloning poll");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     // Undo the last command — testable via Swagger
     [HttpPost("undo")]
     [Authorize(Roles = Roles.Organizer)]
     public async Task<IActionResult> Undo()
     {
          try
          {
               if (_invoker.HistoryDepth == 0)
                    return BadRequest(new { error = "Nothing to undo" });

               var before = _invoker.HistoryDepth;
               await _invoker.UndoLastAsync();

               return Ok(new
               {
                    message = "Last action undone",
                    historyDepth = _invoker.HistoryDepth
               });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error during undo");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpGet("undo/history")]
     [Authorize(Roles = Roles.Organizer)]
     public IActionResult GetUndoHistory()
     {
          return Ok(new
          {
               historyDepth = _invoker.HistoryDepth,
               commands = _invoker.HistoryNames
          });
     }

     [HttpPost("{id}/votes")]
     [Authorize]
     public async Task<IActionResult> CastVote(int id, [FromBody] CastVoteDto dto)
     {
          try
          {
               var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
               if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "You must be logged in to vote" });

               dto.PollId = id;
               var result = await _pollService.CastVoteAsync(dto, userId);

               var scopeFactory = HttpContext.RequestServices
                   .GetRequiredService<IServiceScopeFactory>();
               var capturedUserId = userId;
               var capturedPollId = id;

               _ = Task.Run(async () =>
               {
                    // Create a new scope with its own DbContext — not the disposed one
                    await using var scope = scopeFactory.CreateAsyncScope();
                    try
                    {
                         var pollService = scope.ServiceProvider.GetRequiredService<IPollService>();
                         var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                         var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                         var freshPoll = await pollService.GetPollResultsAsync(capturedPollId);
                         var @event = await eventService.GetEventByIdAsync(freshPoll.EventId);
                         var organizerId = @event?.OrganizerId ?? string.Empty;

                         await notificationService.NotifyVoteCastAsync(freshPoll, capturedUserId, organizerId);
                    }
                    catch (Exception ex)
                    {
                         _logger.LogError(ex, "Notification error after vote");
                    }
               });

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
               return new
               {
                    type = "group",
                    id = group.Id,
                    text = group.DisplayText,
                    children = group.Children.Select(BuildTreeJson).ToList()
               };
          return new { type = "option", id = node.Id, text = node.DisplayText };
     }
}