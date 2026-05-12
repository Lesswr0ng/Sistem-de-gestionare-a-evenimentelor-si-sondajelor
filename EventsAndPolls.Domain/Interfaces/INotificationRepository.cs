using EventsAndPolls.Domain.Entities;

namespace EventsAndPolls.Domain.Interfaces;

public interface INotificationRepository
{
     Task AddAsync(Notification notification);
     Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false);
     Task<int> GetUnreadCountAsync(string userId);
     Task MarkAsReadAsync(int notificationId, string userId);
     Task MarkAllAsReadAsync(string userId);
}