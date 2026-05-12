namespace EventsAndPolls.Application.Notifications;

// Concrete Factory 1 — BasicNotificationFactory
// Creates simple, concise notifications for regular users (voters).
// They get minimal info — just what they need to know, nothing more.

public class BasicNotificationFactory : INotificationFactory
{
     // ── Poll Created ──────────────────────────────────────────────────────

     public IEmailNotification CreatePollCreatedEmail(NotificationContext ctx) =>
         new BasicEmail(
             recipient: ctx.RecipientEmail,
             subject: $"Sondaj nou: {ctx.PollQuestion}",
             body: $"Salut {ctx.RecipientName},\n\n" +
                   $"Un sondaj nou a fost creat în evenimentul \"{ctx.EventTitle}\".\n\n" +
                   $"Întrebare: {ctx.PollQuestion}\n\n" +
                   $"Votează acum la: /Polls/Details?id={ctx.PollId}\n\n" +
                   $"EventHub");

     public IInAppNotification CreatePollCreatedInApp(NotificationContext ctx) =>
         new BasicInApp(
             userId: ctx.RecipientUserId,
             title: "Sondaj nou disponibil",
             body: $"Sondaj nou în \"{ctx.EventTitle}\": {ctx.PollQuestion}",
             pollId: ctx.PollId,
             eventId: ctx.EventId);

     // ── Vote Cast ─────────────────────────────────────────────────────────

     public IEmailNotification CreateVoteCastEmail(NotificationContext ctx) =>
         new BasicEmail(
             recipient: ctx.RecipientEmail,
             subject: "Votul tău a fost înregistrat",
             body: $"Salut {ctx.RecipientName},\n\n" +
                   $"Votul tău pentru sondajul \"{ctx.PollQuestion}\" a fost înregistrat cu succes.\n\n" +
                   $"Mulțumim pentru participare!\n\n" +
                   $"EventHub");

     public IInAppNotification CreateVoteCastInApp(NotificationContext ctx) =>
         new BasicInApp(
             userId: ctx.RecipientUserId,
             title: "Vot înregistrat ✓",
             body: $"Votul tău pentru \"{ctx.PollQuestion}\" a fost înregistrat.",
             pollId: ctx.PollId,
             eventId: ctx.EventId);

     // ── Poll Closed ───────────────────────────────────────────────────────

     public IEmailNotification CreatePollClosedEmail(NotificationContext ctx) =>
         new BasicEmail(
             recipient: ctx.RecipientEmail,
             subject: $"Sondajul s-a închis: {ctx.PollQuestion}",
             body: $"Salut {ctx.RecipientName},\n\n" +
                   $"Sondajul \"{ctx.PollQuestion}\" s-a închis.\n" +
                   $"Total voturi: {ctx.TotalVotes}\n\n" +
                   $"Vezi rezultatele la: /Polls/Details?id={ctx.PollId}\n\n" +
                   $"EventHub");

     public IInAppNotification CreatePollClosedInApp(NotificationContext ctx) =>
         new BasicInApp(
             userId: ctx.RecipientUserId,
             title: "Sondaj închis",
             body: $"\"{ctx.PollQuestion}\" s-a închis cu {ctx.TotalVotes} voturi.",
             pollId: ctx.PollId,
             eventId: ctx.EventId);
}

// ── Concrete Products for Basic Factory ──────────────────────────────────

internal class BasicEmail : IEmailNotification
{
     public string Subject { get; }
     public string Body { get; }
     public string RecipientEmail { get; }

     public BasicEmail(string recipient, string subject, string body)
     {
          RecipientEmail = recipient;
          Subject = subject;
          Body = body;
     }
}

internal class BasicInApp : IInAppNotification
{
     public string Title { get; }
     public string Body { get; }
     public string RecipientUserId { get; }
     public int? RelatedPollId { get; }
     public int? RelatedEventId { get; }

     public BasicInApp(string userId, string title, string body, int? pollId, int? eventId)
     {
          RecipientUserId = userId;
          Title = title;
          Body = body;
          RelatedPollId = pollId;
          RelatedEventId = eventId;
     }
}