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
          // Validate dates
          if (createDto.StartDate >= createDto.EndDate)
               throw new ArgumentException("Start date must be before end date");

          if (createDto.StartDate < DateTime.UtcNow)
               throw new ArgumentException("Start date cannot be in the past");

          // Create domain entity
          var @event = Event.Create(
              createDto.Title,
              createDto.Description,
              createDto.StartDate,
              createDto.EndDate,
              createDto.Location,
              createDto.MaxParticipants,
              organizerId);

          // Save
          await _eventRepository.AddAsync(@event);

          // Return DTO
          return MapToDto(@event);
     }

     public async Task<EventDto> UpdateEventAsync(UpdateEventDto updateDto)
     {
          var @event = await _eventRepository.GetByIdAsync(updateDto.Id);
          if (@event == null)
               throw new ArgumentException($"Event with ID {updateDto.Id} not found");

          // Validate dates
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

     public async Task DeleteEventAsync(int id)
     {
          await _eventRepository.DeleteAsync(id);
     }

     // Mapping method (in real app, use AutoMapper)
     private EventDto MapToDto(Event @event)
     {
          return new EventDto
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
               IsActive = @event.IsActive
          };
     }
}