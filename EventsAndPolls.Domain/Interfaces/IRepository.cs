using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Interfaces;

// Generic Repository Pattern
public interface IRepository<T> where T : BaseEntity
{
     Task<T?> GetByIdAsync(int id);
     Task<IEnumerable<T>> GetAllAsync();
     Task AddAsync(T entity);
     Task UpdateAsync(T entity);
     Task DeleteAsync(int id);
}

// Specific interfaces (Interface Segregation Principle)
public interface IEventRepository : IRepository<Event>
{
     Task<IEnumerable<Event>> GetUpcomingEventsAsync();
}

public interface IPollRepository : IRepository<Poll>
{
     Task<IEnumerable<Poll>> GetPollsByEventAsync(int eventId);
}

public interface IVoteRepository : IRepository<Vote>
{
     Task<bool> HasUserVotedAsync(int pollId, string userId);
     Task<int> GetVoteCountAsync(int pollId);
}
