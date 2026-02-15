using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;
using EventsAndPolls.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsAndPolls.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
     private readonly AppDbContext _context;

     public EventRepository(AppDbContext context)
     {
          _context = context;
     }

     public async Task<Event?> GetByIdAsync(int id)
     {
          return await _context.Events.FindAsync(id);
     }

     public async Task<IEnumerable<Event>> GetAllAsync()
     {
          return await _context.Events.ToListAsync();
     }

     public async Task AddAsync(Event entity)
     {
          await _context.Events.AddAsync(entity);
          await _context.SaveChangesAsync();
     }

     public async Task UpdateAsync(Event entity)
     {
          _context.Events.Update(entity);
          await _context.SaveChangesAsync();
     }

     public async Task DeleteAsync(int id)
     {
          var entity = await GetByIdAsync(id);
          if (entity != null)
          {
               _context.Events.Remove(entity);
               await _context.SaveChangesAsync();
          }
     }

     public async Task<IEnumerable<Event>> GetUpcomingEventsAsync()
     {
          return await _context.Events
              .Where(e => e.StartDate > DateTime.UtcNow && e.IsActive)
              .OrderBy(e => e.StartDate)
              .ToListAsync();
     }
}