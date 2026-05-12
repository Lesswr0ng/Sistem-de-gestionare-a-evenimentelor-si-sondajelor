namespace EventsAndPolls.Domain.Entities;

public enum NotificationType
{
     PollCreated,
     VoteCast,
     PollClosed,
     EventCreated
}

public enum NotificationChannel
{
     InApp,
     Email
}

// Domain entity stored in DB for in-app notifications
public class Notification : BaseEntity
{
     public string UserId { get; private set; } = string.Empty;
     public string Title { get; private set; } = string.Empty;
     public string Body { get; private set; } = string.Empty;
     public NotificationType Type { get; private set; }
     public NotificationChannel Channel { get; private set; }
     public bool IsRead { get; private set; }
     public int? RelatedPollId { get; private set; }
     public int? RelatedEventId { get; private set; }

     private Notification() { }

     public Notification(
         string userId,
         string title,
         string body,
         NotificationType type,
         NotificationChannel channel,
         int? relatedPollId = null,
         int? relatedEventId = null)
     {
          UserId = userId;
          Title = title;
          Body = body;
          Type = type;
          Channel = channel;
          IsRead = false;
          RelatedPollId = relatedPollId;
          RelatedEventId = relatedEventId;
     }

     public void MarkAsRead() => IsRead = true;
}