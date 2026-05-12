using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;
using EventsAndPolls.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsAndPolls.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
     private readonly AppDbContext _context;

     public NotificationRepository(AppDbContext context)
     {
          _context = context;
     }

     public async Task AddAsync(Notification notification)
     {
          await _context.Notifications.AddAsync(notification);
          await _context.SaveChangesAsync();
     }

     public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false)
     {
          var query = _context.Notifications
              .Where(n => n.UserId == userId);

          if (unreadOnly)
               query = query.Where(n => !n.IsRead);

          return await query
              .OrderByDescending(n => n.CreatedAt)
              .Take(50) // cap at 50 most recent
              .ToListAsync();
     }

     public async Task<int> GetUnreadCountAsync(string userId)
     {
          return await _context.Notifications
              .CountAsync(n => n.UserId == userId && !n.IsRead);
     }

     public async Task MarkAsReadAsync(int notificationId, string userId)
     {
          var notification = await _context.Notifications
              .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

          if (notification != null)
          {
               notification.MarkAsRead();
               await _context.SaveChangesAsync();
          }
     }

     public async Task MarkAllAsReadAsync(string userId)
     {
          var unread = await _context.Notifications
              .Where(n => n.UserId == userId && !n.IsRead)
              .ToListAsync();

          foreach (var n in unread)
               n.MarkAsRead();

          await _context.SaveChangesAsync();
     }
}