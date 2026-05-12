namespace EventsAndPolls.Application.Notifications;

// Concrete Factory 2 — DetailedNotificationFactory
// Creates rich, data-heavy notifications for organizers.
// They get full breakdowns — vote counts per option, percentages, analytics.
// Same interface as BasicNotificationFactory — the switch is invisible to the caller.

public class DetailedNotificationFactory : INotificationFactory
{
     // ── Poll Created ──────────────────────────────────────────────────────

     public IEmailNotification CreatePollCreatedEmail(NotificationContext ctx) =>
         new DetailedEmail(
             recipient: ctx.RecipientEmail,
             subject: $"[Organizator] Sondaj publicat: {ctx.PollQuestion}",
             body: $"Salut {ctx.RecipientName},\n\n" +
                   $"Sondajul tău a fost publicat cu succes în evenimentul \"{ctx.EventTitle}\".\n\n" +
                   $"📋 Întrebare: {ctx.PollQuestion}\n" +
                   $"🔗 Link direct: /Polls/Details?id={ctx.PollId}\n" +
                   $"✏️  Editare: /Polls/Edit?id={ctx.PollId}\n\n" +
                   $"Participanții la eveniment au fost notificați.\n\n" +
                   $"EventHub — Panou Organizator");

     public IInAppNotification CreatePollCreatedInApp(NotificationContext ctx) =>
         new DetailedInApp(
             userId: ctx.RecipientUserId,
             title: "✅ Sondaj publicat",
             body: $"Sondajul \"{ctx.PollQuestion}\" este acum activ în \"{ctx.EventTitle}\". " +
                   $"Participanții pot vota.",
             pollId: ctx.PollId,
             eventId: ctx.EventId);

     // ── Vote Cast ─────────────────────────────────────────────────────────
     // Organizer gets notified every time someone votes — with running total

     public IEmailNotification CreateVoteCastEmail(NotificationContext ctx) =>
         new DetailedEmail(
             recipient: ctx.RecipientEmail,
             subject: $"[Organizator] Vot nou — {ctx.PollQuestion}",
             body: $"Salut {ctx.RecipientName},\n\n" +
                   $"Un nou vot a fost înregistrat pentru sondajul \"{ctx.PollQuestion}\".\n\n" +
                   $"📊 Statistici curente:\n" +
                   $"   Total voturi: {ctx.TotalVotes}\n\n" +
                   BuildOptionBreakdown(ctx.OptionResults) +
                   $"\n🔗 Vezi toate rezultatele: /Polls/Details?id={ctx.PollId}\n\n" +
                   $"EventHub — Panou Organizator");

     public IInAppNotification CreateVoteCastInApp(NotificationContext ctx) =>
         new DetailedInApp(
             userId: ctx.RecipientUserId,
             title: $"📊 Vot nou — {ctx.TotalVotes} total",
             body: $"Sondaj: \"{ctx.PollQuestion}\" — {ctx.TotalVotes} voturi înregistrate. " +
                   $"Opțiunea lider: {GetLeadingOption(ctx.OptionResults)}",
             pollId: ctx.PollId,
             eventId: ctx.EventId);

     // ── Poll Closed ───────────────────────────────────────────────────────
     // Organizer gets full results breakdown

     public IEmailNotification CreatePollClosedEmail(NotificationContext ctx) =>
         new DetailedEmail(
             recipient: ctx.RecipientEmail,
             subject: $"[Organizator] Sondaj închis — Rezultate finale: {ctx.PollQuestion}",
             body: $"Salut {ctx.RecipientName},\n\n" +
                   $"Sondajul \"{ctx.PollQuestion}\" s-a închis.\n\n" +
                   $"📊 REZULTATE FINALE:\n" +
                   $"   Total voturi: {ctx.TotalVotes}\n\n" +
                   BuildOptionBreakdown(ctx.OptionResults) +
                   $"\n🏆 Câștigător: {GetLeadingOption(ctx.OptionResults)}\n\n" +
                   $"🔗 Raport complet: /Polls/Details?id={ctx.PollId}\n" +
                   $"📥 Export JSON: /api/export/poll/{ctx.PollId}?format=json\n" +
                   $"📥 Export Text: /api/export/poll/{ctx.PollId}?format=txt\n\n" +
                   $"EventHub — Panou Organizator");

     public IInAppNotification CreatePollClosedInApp(NotificationContext ctx) =>
         new DetailedInApp(
             userId: ctx.RecipientUserId,
             title: "🔒 Sondaj închis — Rezultate finale",
             body: $"\"{ctx.PollQuestion}\" — {ctx.TotalVotes} voturi. " +
                   $"Câștigător: {GetLeadingOption(ctx.OptionResults)}. " +
                   $"Exportează rezultatele din pagina sondajului.",
             pollId: ctx.PollId,
             eventId: ctx.EventId);

     // ── Helpers ───────────────────────────────────────────────────────────

     private static string BuildOptionBreakdown(List<OptionResult> results)
     {
          if (!results.Any()) return string.Empty;
          var lines = results.Select(r =>
              $"   • {r.Text}: {r.VoteCount} voturi ({r.Percentage:F1}%)");
          return string.Join("\n", lines) + "\n";
     }

     private static string GetLeadingOption(List<OptionResult> results)
     {
          if (!results.Any()) return "N/A";
          var leader = results.MaxBy(r => r.VoteCount);
          return leader != null ? $"{leader.Text} ({leader.Percentage:F1}%)" : "N/A";
     }
}

// ── Concrete Products for Detailed Factory ────────────────────────────────

internal class DetailedEmail : IEmailNotification
{
     public string Subject { get; }
     public string Body { get; }
     public string RecipientEmail { get; }

     public DetailedEmail(string recipient, string subject, string body)
     {
          RecipientEmail = recipient;
          Subject = subject;
          Body = body;
     }
}

internal class DetailedInApp : IInAppNotification
{
     public string Title { get; }
     public string Body { get; }
     public string RecipientUserId { get; }
     public int? RelatedPollId { get; }
     public int? RelatedEventId { get; }

     public DetailedInApp(string userId, string title, string body, int? pollId, int? eventId)
     {
          RecipientUserId = userId;
          Title = title;
          Body = body;
          RelatedPollId = pollId;
          RelatedEventId = eventId;
     }
}