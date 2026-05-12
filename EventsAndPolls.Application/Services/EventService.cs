using EventsAndPolls.Application.DTOs.Requests;
using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;

namespace EventsAndPolls.Application.Services;

public class EventService : IEventService
{
     private readonly IEventRepository _eventRepository;

     public EventService(IEventRepository eventRepository)
     {
          _eventRepository = eventRepository;
     }

     public async Task<EventDto> CreateEventAsync(CreateEventDto createDto, string organizerId)
     {
          if (createDto.StartDate >= createDto.EndDate)
               throw new ArgumentException("Start date must be before end date");

          var @event = Event.Create(
              createDto.Title,
              createDto.Description,
              createDto.StartDate,
              createDto.EndDate,
              createDto.Location,
              createDto.MaxParticipants,
              organizerId);

          await _eventRepository.AddAsync(@event);
          return MapToDto(@event);
     }

     public async Task<EventDto?> UpdateEventAsync(int id, UpdateEventDto updateDto, string organizerId)
     {
          var @event = await _eventRepository.GetByIdAsync(id);
          if (@event == null) return null;

          // Only the organizer who created it can update it
          if (@event.OrganizerId != organizerId)
               throw new UnauthorizedAccessException("You can only edit your own events");

          if (updateDto.StartDate >= updateDto.EndDate)
               throw new ArgumentException("Start date must be before end date");

          @event.Update(
              updateDto.Title,
              updateDto.Description,
              updateDto.StartDate,
              updateDto.EndDate,
              updateDto.Location,
              updateDto.MaxParticipants);

          await _eventRepository.UpdateAsync(@event);
          return MapToDto(@event);
     }

     public async Task<EventDto?> GetEventByIdAsync(int id)
     {
          var @event = await _eventRepository.GetByIdAsync(id);
          return @event == null ? null : MapToDto(@event);
     }

     public async Task<IEnumerable<EventDto>> GetUpcomingEventsAsync()
     {
          var events = await _eventRepository.GetUpcomingEventsAsync();
          return events.Select(MapToDto);
     }

     public async Task<IEnumerable<EventDto>> GetAllEventsAsync()
     {
          var events = await _eventRepository.GetAllAsync();
          return events.Select(MapToDto);
     }

     public async Task DeleteEventAsync(int id, string organizerId)
     {
          var @event = await _eventRepository.GetByIdAsync(id);
          if (@event == null) return;

          if (@event.OrganizerId != organizerId)
               throw new UnauthorizedAccessException("You can only delete your own events");

          await _eventRepository.DeleteAsync(id);
     }

     private EventDto MapToDto(Event @event) => new()
     {
          Id = @event.Id,
          Title = @event.Title,
          Description = @event.Description,
          StartDate = @event.StartDate,
          EndDate = @event.EndDate,
          Location = @event.Location,
          MaxParticipants = @event.MaxParticipants,
          PollCount = @event.Polls?.Count ?? 0,
          CreatedAt = @event.CreatedAt,
          IsActive = @event.IsActive,
          OrganizerId = @event.OrganizerId
     };
}