using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Application.Services;

public interface IEventService
{
     Task<Event> CreateEventAsync(string title, string description, DateTime startDate,
                                  DateTime endDate, string location, int maxParticipants, string organizerId);

     Task<Event?> GetEventByIdAsync(int id);
     Task<IEnumerable<Event>> GetUpcomingEventsAsync();

     Task AddPollToEventAsync(int eventId, string question, List<string> options, bool allowMultipleChoices = false);
     Task UpdateEventAsync(int id, string title, string description, DateTime startDate,
                     DateTime endDate, string location, int maxParticipants);
     Task DeleteEventAsync(int id);
}
