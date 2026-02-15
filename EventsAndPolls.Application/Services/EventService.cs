using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;

namespace EventsAndPolls.Application.Services;

public class EventService : IEventService
{
     private readonly IEventRepository _eventRepository;
     private readonly IPollRepository _pollRepository;

     public EventService(IEventRepository eventRepository, IPollRepository pollRepository)
     {
          _eventRepository = eventRepository;
          _pollRepository = pollRepository;
     }

     public async Task<Event> CreateEventAsync(string title, string description, DateTime startDate,
                                               DateTime endDate, string location, int maxParticipants, string organizerId)
     {
          var @event = Event.Create(title, description, startDate, endDate, location, maxParticipants, organizerId);
          await _eventRepository.AddAsync(@event);
          return @event;
     }

     public async Task<Event?> GetEventByIdAsync(int id)
     {
          return await _eventRepository.GetByIdAsync(id);
     }

     public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
     {
          return await _eventRepository.GetUpcomingEventsAsync();
     }

     public async Task AddPollToEventAsync(int eventId, string question, List<string> options, bool allowMultipleChoices = false)
     {
          var @event = await _eventRepository.GetByIdAsync(eventId);
          if (@event == null)
               throw new Exception("Event not found");

          var poll = Poll.Create(question, eventId, null, allowMultipleChoices);

          foreach (var optionText in options)
          {
               poll.AddOption(optionText);
          }

          await _pollRepository.AddAsync(poll);
     }
     public async Task UpdateEventAsync(int id, string title, string description, DateTime startDate,
                                  DateTime endDate, string location, int maxParticipants)
     {
          var @event = await _eventRepository.GetByIdAsync(id);
          if (@event == null)
               throw new ArgumentException($"Event with ID {id} not found");

          @event.Update(title, description, startDate, endDate, location, maxParticipants);
          await _eventRepository.UpdateAsync(@event);
     }

     public async Task DeleteEventAsync(int id)
     {
          await _eventRepository.DeleteAsync(id);
     }
}