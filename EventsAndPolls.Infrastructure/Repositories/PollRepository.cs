using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;
using EventsAndPolls.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsAndPolls.Infrastructure.Repositories;

public class PollRepository : IPollRepository
{
     private readonly AppDbContext _context;

     public PollRepository(AppDbContext context)
     {
          _context = context;
     }

     public async Task<Poll?> GetByIdAsync(int id)
     {
          return await _context.Polls
              .Include(p => p.Options)
              .Include(p => p.Votes)
              .FirstOrDefaultAsync(p => p.Id == id);
     }

     public async Task<IEnumerable<Poll>> GetAllAsync()
     {
          return await _context.Polls.ToListAsync();
     }

     public async Task AddAsync(Poll entity)
     {
          await _context.Polls.AddAsync(entity);
          await _context.SaveChangesAsync();
     }

     public async Task UpdateAsync(Poll entity)
     {
          _context.Polls.Update(entity);
          await _context.SaveChangesAsync();
     }

     public async Task DeleteAsync(int id)
     {
          var entity = await GetByIdAsync(id);
          if (entity != null)
          {
               _context.Polls.Remove(entity);
               await _context.SaveChangesAsync();
          }
     }

     public async Task<IEnumerable<Poll>> GetPollsByEventAsync(int eventId)
     {
          return await _context.Polls
              .Where(p => p.EventId == eventId)
              .Include(p => p.Options)
              .Include(p => p.Votes)
              .ToListAsync();
     }
}