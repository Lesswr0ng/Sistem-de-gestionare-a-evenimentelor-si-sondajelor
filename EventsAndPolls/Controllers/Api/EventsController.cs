using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
public class EventsController : ControllerBase
{
     private readonly IEventService _eventService;
     private readonly ILogger<EventsController> _logger;
     private readonly IPollService _pollService;

     public EventsController(IEventService eventService, ILogger<EventsController> logger, IPollService pollService)
     {
          _eventService = eventService;
          _logger = logger;
          _pollService = pollService;
     }

     [HttpGet]
     public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
     {
          try
          {
               var events = await _eventService.GetUpcomingEventsAsync();
               return Ok(events);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting events");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpGet("{id}")]
     public async Task<ActionResult<EventDto>> GetEvent(int id)
     {
          try
          {
               var @event = await _eventService.GetEventByIdAsync(id);
               if (@event == null)
                    return NotFound(new { error = $"Event with ID {id} not found" });

               return Ok(@event);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting event {EventId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpPost]
     public async Task<ActionResult<EventDto>> CreateEvent([FromBody] CreateEventDto createDto)
     {
          try
          {
               if (!ModelState.IsValid)
                    return BadRequest(ModelState);

               // In real app, get from authentication
               var organizerId = User.Identity?.Name ?? "system-user";

               var result = await _eventService.CreateEventAsync(createDto, organizerId);
               return CreatedAtAction(nameof(GetEvent), new { id = result.Id }, result);
          }
          catch (ArgumentException ex)
          {
               return BadRequest(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error creating event");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     [HttpPut("{id}")]
     public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto updateDto)
     {
          try
          {
               if (id != updateDto.Id)
                    return BadRequest(new { error = "ID mismatch" });

               var result = await _eventService.UpdateEventAsync(updateDto);
               return Ok(result);
          }
          catch (ArgumentException ex)
          {
               return NotFound(new { error = ex.Message });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error updating event {EventId}", id);
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

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
               return StatusCode(500, new { error = "An error occurred" });
          }
     }
     [HttpGet("{id}/polls")]
     public async Task<ActionResult<IEnumerable<PollDto>>> GetEventPolls(int id)
     {
          try
          {
               Console.WriteLine($"GET /api/events/{id}/polls called");

               // First check if event exists
               var eventExists = await _eventService.GetEventByIdAsync(id);
               if (eventExists == null)
               {
                    return NotFound(new { error = $"Event with ID {id} not found" });
               }

               // Get polls for this event
               var polls = await _pollService.GetPollsByEventAsync(id);

               return Ok(polls);
          }
          catch (Exception ex)
          {
               Console.WriteLine($"Error getting polls for event {id}: {ex.Message}");
               return StatusCode(500, new { error = ex.Message });
          }
     }
}