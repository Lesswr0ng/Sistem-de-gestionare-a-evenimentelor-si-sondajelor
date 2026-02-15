using EventsAndPolls.Application.Services;
using EventsAndPolls.Application.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
     private readonly IEventService _eventService;
     private readonly IPollService _pollService;
     private readonly ILogger<EventsController> _logger;

     public EventsController(
         IEventService eventService,
         IPollService pollService,
         ILogger<EventsController> logger)
     {
          _eventService = eventService;
          _pollService = pollService;
          _logger = logger;
     }

     // GET: api/events
     [HttpGet]
     public async Task<ActionResult<IEnumerable<EventViewModel>>> GetEvents()
     {
          try
          {
               var events = await _eventService.GetUpcomingEventsAsync();

               var result = events.Select(e => new EventViewModel
               {
                    Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Location = e.Location,
                    MaxParticipants = e.MaxParticipants,
                    PollCount = e.Polls?.Count ?? 0,
                    CreatedAt = e.CreatedAt
               }).ToList();

               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting events");
               return StatusCode(500, new { error = "An error occurred while retrieving events" });
          }
     }

     // GET: api/events/{id}
     [HttpGet("{id}")]
     public async Task<ActionResult<EventViewModel>> GetEvent(int id)
     {
          try
          {
               var @event = await _eventService.GetEventByIdAsync(id);

               if (@event == null)
                    return NotFound(new { error = $"Event with ID {id} not found" });

               var result = new EventViewModel
               {
                    Id = @event.Id,
                    Title = @event.Title,
                    Description = @event.Description,
                    StartDate = @event.StartDate,
                    EndDate = @event.EndDate,
                    Location = @event.Location,
                    MaxParticipants = @event.MaxParticipants,
                    PollCount = @event.Polls?.Count ?? 0,
                    CreatedAt = @event.CreatedAt
               };

               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting event {EventId}", id);
               return StatusCode(500, new { error = "An error occurred while retrieving the event" });
          }
     }

     // POST: api/events
     [HttpPost]
     public async Task<ActionResult<EventViewModel>> CreateEvent([FromBody] CreateEventViewModel model)
     {
          try
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               // Validate dates
               if (model.StartDate >= model.EndDate)
                    return BadRequest(new { error = "Start date must be before end date" });

               if (model.StartDate < DateTime.UtcNow)
                    return BadRequest(new { error = "Start date cannot be in the past" });

               var userId = "system-user"; // In real app, get from authentication

               var createdEvent = await _eventService.CreateEventAsync(
                   model.Title,
                   model.Description,
                   model.StartDate,
                   model.EndDate,
                   model.Location,
                   model.MaxParticipants,
                   userId);

               var result = new EventViewModel
               {
                    Id = createdEvent.Id,
                    Title = createdEvent.Title,
                    Description = createdEvent.Description,
                    StartDate = createdEvent.StartDate,
                    EndDate = createdEvent.EndDate,
                    Location = createdEvent.Location,
                    MaxParticipants = createdEvent.MaxParticipants,
                    CreatedAt = createdEvent.CreatedAt
               };

               return CreatedAtAction(nameof(GetEvent), new { id = createdEvent.Id }, result);
          }
          catch (ArgumentException ex)
          {
               return BadRequest(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error creating event");
               return StatusCode(500, new { error = "An error occurred while creating the event" });
          }
     }

     // PUT: api/events/{id}
     [HttpPut("{id}")]
     public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventViewModel model)
     {
          try
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               // Validate dates
               if (model.StartDate >= model.EndDate)
                    return BadRequest(new { error = "Start date must be before end date" });

               await _eventService.UpdateEventAsync(
                   id,
                   model.Title,
                   model.Description,
                   model.StartDate,
                   model.EndDate,
                   model.Location,
                   model.MaxParticipants);

               return Ok(new { message = "Event updated successfully" });
          }
          catch (ArgumentException ex)
          {
               return BadRequest(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error updating event {EventId}", id);
               return StatusCode(500, new { error = "An error occurred while updating the event" });
          }
     }

     // DELETE: api/events/{id}
     [HttpDelete("{id}")]
     public async Task<IActionResult> DeleteEvent(int id)
     {
          try
          {
               await _eventService.DeleteEventAsync(id);
               return Ok(new { message = "Event deleted successfully" });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error deleting event {EventId}", id);
               return StatusCode(500, new { error = "An error occurred while deleting the event" });
          }
     }

     // GET: api/events/{id}/polls
     [HttpGet("{id}/polls")]
     public async Task<ActionResult<IEnumerable<PollViewModel>>> GetEventPolls(int id)
     {
          try
          {
               var polls = await _pollService.GetPollsByEventAsync(id);

               var result = polls.Select(p => new PollViewModel
               {
                    Id = p.Id,
                    Question = p.Question,
                    EventId = p.EventId,
                    IsActive = p.IsActive,
                    AllowMultipleChoices = p.AllowMultipleChoices,
                    TotalVotes = p.Votes?.Count ?? 0,
                    Options = p.Options?.Select(o => new PollOptionViewModel
                    {
                         Id = o.Id,
                         Text = o.Text,
                         VoteCount = p.Votes?.Count(v => v.PollOptionId == o.Id) ?? 0
                    }).ToList() ?? new List<PollOptionViewModel>()
               }).ToList();

               return Ok(result);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting polls for event {EventId}", id);
               return StatusCode(500, new { error = "An error occurred while retrieving polls" });
          }
     }
}