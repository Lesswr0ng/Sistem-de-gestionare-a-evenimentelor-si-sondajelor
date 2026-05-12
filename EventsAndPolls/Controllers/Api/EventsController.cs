using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.Services;
using EventsAndPolls.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
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

    // Public — anyone can view events
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        try
        {
            var events = await _eventService.GetUpcomingEventsAsync();
            return Ok(events);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all events");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetEvent(int id)
    {
        try
        {
            var @event = await _eventService.GetEventByIdAsync(id);
            if (@event == null) return NotFound(new { error = $"Event {id} not found" });
            return Ok(@event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpGet("{id}/polls")]
    public async Task<IActionResult> GetEventPolls(int id)
    {
        try
        {
            var polls = await _pollService.GetPollsByEventAsync(id);
            return Ok(polls);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting polls for event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    // Organizer only — must be logged in and have Organizer role
    [HttpPost]
    [Authorize(Roles = Roles.Organizer)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
    {
        try
        {
            // Use real authenticated user ID
            var organizerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(organizerId))
                return Unauthorized(new { error = "User not authenticated" });

            var @event = await _eventService.CreateEventAsync(dto, organizerId);
            return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, @event);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Organizer)]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto updateDto)
    {
        try
        {
               if (id != updateDto.Id)
                    return BadRequest(new { error = "ID mismatch" });
            var organizerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var updated = await _eventService.UpdateEventAsync(id, updateDto, organizerId);
            if (updated == null) return NotFound(new { error = $"Event {id} not found or not yours" });
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

     [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Organizer)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        try
        {
            var organizerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _eventService.DeleteEventAsync(id, organizerId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }
}
