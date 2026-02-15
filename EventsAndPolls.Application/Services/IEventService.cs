using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;

namespace EventsAndPolls.Application.Services;

public interface IEventService
{
     Task<EventDto> CreateEventAsync(CreateEventDto createDto, string organizerId);
     Task<EventDto> UpdateEventAsync(UpdateEventDto updateDto);
     Task<EventDto?> GetEventByIdAsync(int id);
     Task<IEnumerable<EventDto>> GetUpcomingEventsAsync();
     Task DeleteEventAsync(int id);
}
