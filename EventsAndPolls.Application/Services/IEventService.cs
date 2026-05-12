using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;

namespace EventsAndPolls.Application.Services;

public interface IEventService
{
     Task<EventDto> CreateEventAsync(CreateEventDto createDto, string organizerId);
     Task<EventDto?> UpdateEventAsync(int id, UpdateEventDto updateDto, string organizerId);
     Task<EventDto?> GetEventByIdAsync(int id);
     Task<IEnumerable<EventDto>> GetUpcomingEventsAsync();
     Task<IEnumerable<EventDto>> GetAllEventsAsync();
     Task DeleteEventAsync(int id, string organizerId);
}