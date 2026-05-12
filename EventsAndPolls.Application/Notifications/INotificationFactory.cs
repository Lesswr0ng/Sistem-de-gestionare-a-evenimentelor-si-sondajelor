namespace EventsAndPolls.Application.Notifications;

// ── Product interfaces ────────────────────────────────────────────────────
// These are the two products in the family.
// Every factory must create both consistently.

public interface IEmailNotification
{
     string Subject { get; }
     string Body { get; }
     string RecipientEmail { get; }
     NotificationChannel Channel => NotificationChannel.Email;
}

public interface IInAppNotification
{
     string Title { get; }
     string Body { get; }
     string RecipientUserId { get; }
     int? RelatedPollId { get; }
     int? RelatedEventId { get; }
     NotificationChannel Channel => NotificationChannel.InApp;
}

// ── Abstract Factory ──────────────────────────────────────────────────────
// Defines the contract for creating a FAMILY of related notifications.
// Each concrete factory creates both products consistently for one user type.

public interface INotificationFactory
{
     // Triggered when an organizer creates a new poll
     IEmailNotification CreatePollCreatedEmail(NotificationContext ctx);
     IInAppNotification CreatePollCreatedInApp(NotificationContext ctx);

     // Triggered when a user casts a vote
     IEmailNotification CreateVoteCastEmail(NotificationContext ctx);
     IInAppNotification CreateVoteCastInApp(NotificationContext ctx);

     // Triggered when a poll is closed
     IEmailNotification CreatePollClosedEmail(NotificationContext ctx);
     IInAppNotification CreatePollClosedInApp(NotificationContext ctx);
}

// ── Notification Context ──────────────────────────────────────────────────
// Carries all the data needed to build any notification.
// Passed to every factory method.

public class NotificationContext
{
     public string RecipientUserId { get; set; } = string.Empty;
     public string RecipientEmail { get; set; } = string.Empty;
     public string RecipientName { get; set; } = string.Empty;
     public string PollQuestion { get; set; } = string.Empty;
     public int PollId { get; set; }
     public string EventTitle { get; set; } = string.Empty;
     public int EventId { get; set; }
     public int TotalVotes { get; set; }
     public string OrganizerName { get; set; } = string.Empty;

     // Per-option results for organizer detailed notifications
     public List<OptionResult> OptionResults { get; set; } = new();
}

public class OptionResult
{
     public string Text { get; set; } = string.Empty;
     public int VoteCount { get; set; }
     public decimal Percentage { get; set; }
}

public enum NotificationChannel { InApp, Email }