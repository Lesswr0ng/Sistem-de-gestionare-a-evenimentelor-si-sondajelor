using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;
using EventsAndPolls.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsAndPolls.Infrastructure.Repositories;

public class VoteRepository : IVoteRepository
{
     private readonly AppDbContext _context;

     public VoteRepository(AppDbContext context)
     {
          _context = context;
     }

     public async Task<Vote?> GetByIdAsync(int id)
     {
          return await _context.Votes.FindAsync(id);
     }

     public async Task<IEnumerable<Vote>> GetAllAsync()
     {
          return await _context.Votes.ToListAsync();
     }

     public async Task AddAsync(Vote entity)
     {
          await _context.Votes.AddAsync(entity);
          await _context.SaveChangesAsync();
     }

     public async Task UpdateAsync(Vote entity)
     {
          _context.Votes.Update(entity);
          await _context.SaveChangesAsync();
     }

     public async Task DeleteAsync(int id)
     {
          var entity = await GetByIdAsync(id);
          if (entity != null)
          {
               _context.Votes.Remove(entity);
               await _context.SaveChangesAsync();
          }
     }

     public async Task<bool> HasUserVotedAsync(int pollId, string userId)
     {
          return await _context.Votes
              .AnyAsync(v => v.PollId == pollId && v.UserId == userId);
     }

     public async Task<int> GetVoteCountAsync(int pollId)
     {
          return await _context.Votes
              .CountAsync(v => v.PollId == pollId);
     }
}
