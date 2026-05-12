using EventsAndPolls.Application.DTOs.Responses;
using EventsAndPolls.Application.Notifications;
using EventsAndPolls.Domain.Entities;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EventsAndPolls.Application.Services;

public interface INotificationService
{
     Task NotifyPollCreatedAsync(PollDto poll, string eventTitle, List<string> participantUserIds);
     Task NotifyVoteCastAsync(PollDto poll, string voterUserId, string organizerUserId);
     Task NotifyPollClosedAsync(PollDto poll, string eventTitle, List<string> participantUserIds, string organizerUserId);
     Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId);
     Task<int> GetUnreadCountAsync(string userId);
     Task MarkAsReadAsync(int notificationId, string userId);
     Task MarkAllAsReadAsync(string userId);
}

public class NotificationDto
{
     public int Id { get; set; }
     public string Title { get; set; } = string.Empty;
     public string Body { get; set; } = string.Empty;
     public bool IsRead { get; set; }
     public DateTime CreatedAt { get; set; }
     public int? RelatedPollId { get; set; }
     public int? RelatedEventId { get; set; }
     public string Channel { get; set; } = string.Empty;
}

public class NotificationService : INotificationService
{
     private readonly INotificationRepository _repository;
     private readonly UserManager<ApplicationUser> _userManager;
     private readonly ILogger<NotificationService> _logger;

     // The two factories — selected based on recipient role
     private readonly INotificationFactory _basicFactory;
     private readonly INotificationFactory _detailedFactory;

     public NotificationService(
         INotificationRepository repository,
         UserManager<ApplicationUser> userManager,
         ILogger<NotificationService> logger)
     {
          _repository = repository;
          _userManager = userManager;
          _logger = logger;
          _basicFactory = new BasicNotificationFactory();
          _detailedFactory = new DetailedNotificationFactory();
     }

     // ── Factory selection ─────────────────────────────────────────────────
     // This is the key decision point — which factory to use per recipient

     private async Task<INotificationFactory> SelectFactoryAsync(string userId)
     {
          var user = await _userManager.FindByIdAsync(userId);
          if (user == null) return _basicFactory;

          var roles = await _userManager.GetRolesAsync(user);
          return roles.Contains("Organizer") ? _detailedFactory : _basicFactory;
     }

     private async Task<NotificationContext> BuildContextAsync(
         string userId, PollDto poll, string eventTitle, int totalVotes = 0)
     {
          var user = await _userManager.FindByIdAsync(userId);
          return new NotificationContext
          {
               RecipientUserId = userId,
               RecipientEmail = user?.Email ?? string.Empty,
               RecipientName = (user as ApplicationUser)?.DisplayName ?? user?.Email ?? "Utilizator",
               PollQuestion = poll.Question,
               PollId = poll.Id,
               EventTitle = eventTitle,
               EventId = poll.EventId,
               TotalVotes = totalVotes,
               OptionResults = poll.Options.Select(o => new OptionResult
               {
                    Text = o.Text,
                    VoteCount = o.VoteCount,
                    Percentage = o.Percentage
               }).ToList()
          };
     }

     // ── Persist in-app notification to DB ─────────────────────────────────

     private async Task PersistInAppAsync(IInAppNotification notification, NotificationType type)
     {
          var entity = new Notification(
              userId: notification.RecipientUserId,
              title: notification.Title,
              body: notification.Body,
              type: type,
              channel: Domain.Entities.NotificationChannel.InApp,
              relatedPollId: notification.RelatedPollId,
              relatedEventId: notification.RelatedEventId);

          await _repository.AddAsync(entity);
          _logger.LogInformation(
              "[Notification] InApp saved for user {UserId}: {Title}",
              notification.RecipientUserId, notification.Title);
     }

     // ── Simulate email sending (log only — no real SMTP) ─────────────────

     private void SimulateEmail(IEmailNotification email, NotificationType type)
     {
          _logger.LogInformation(
              "[Notification:Email] To: {Email} | Subject: {Subject} | Type: {Type}",
              email.RecipientEmail, email.Subject, type);
          // Real implementation: await _emailSender.SendAsync(email.RecipientEmail, email.Subject, email.Body);
     }

     // ── Public methods ────────────────────────────────────────────────────

     public async Task NotifyPollCreatedAsync(
         PollDto poll, string eventTitle, List<string> participantUserIds)
     {
          foreach (var userId in participantUserIds)
          {
               try
               {
                    var factory = await SelectFactoryAsync(userId);
                    var ctx = await BuildContextAsync(userId, poll, eventTitle);

                    var inApp = factory.CreatePollCreatedInApp(ctx);
                    var email = factory.CreatePollCreatedEmail(ctx);

                    await PersistInAppAsync(inApp, NotificationType.PollCreated);
                    SimulateEmail(email, NotificationType.PollCreated);
               }
               catch (Exception ex)
               {
                    _logger.LogError(ex, "Failed to notify user {UserId} of poll creation", userId);
               }
          }
     }

     public async Task NotifyVoteCastAsync(
         PollDto poll, string voterUserId, string organizerUserId)
     {
          // Notify the voter (Basic factory — confirms their vote)
          try
          {
               var voterCtx = await BuildContextAsync(voterUserId, poll, string.Empty, poll.TotalVotes);
               var voterInApp = _basicFactory.CreateVoteCastInApp(voterCtx);
               var voterEmail = _basicFactory.CreateVoteCastEmail(voterCtx);

               await PersistInAppAsync(voterInApp, NotificationType.VoteCast);
               SimulateEmail(voterEmail, NotificationType.VoteCast);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Failed to notify voter {UserId}", voterUserId);
          }

          // Notify the organizer (Detailed factory — full stats)
          if (!string.IsNullOrEmpty(organizerUserId) && organizerUserId != voterUserId)
          {
               try
               {
                    var orgCtx = await BuildContextAsync(organizerUserId, poll, string.Empty, poll.TotalVotes);
                    var orgInApp = _detailedFactory.CreateVoteCastInApp(orgCtx);
                    var orgEmail = _detailedFactory.CreateVoteCastEmail(orgCtx);

                    await PersistInAppAsync(orgInApp, NotificationType.VoteCast);
                    SimulateEmail(orgEmail, NotificationType.VoteCast);
               }
               catch (Exception ex)
               {
                    _logger.LogError(ex, "Failed to notify organizer {UserId}", organizerUserId);
               }
          }
     }

     public async Task NotifyPollClosedAsync(
         PollDto poll, string eventTitle, List<string> participantUserIds, string organizerUserId)
     {
          foreach (var userId in participantUserIds)
          {
               try
               {
                    var factory = await SelectFactoryAsync(userId);
                    var ctx = await BuildContextAsync(userId, poll, eventTitle, poll.TotalVotes);

                    var inApp = factory.CreatePollClosedInApp(ctx);
                    var email = factory.CreatePollClosedEmail(ctx);

                    await PersistInAppAsync(inApp, NotificationType.PollClosed);
                    SimulateEmail(email, NotificationType.PollClosed);
               }
               catch (Exception ex)
               {
                    _logger.LogError(ex, "Failed to notify user {UserId} of poll close", userId);
               }
          }
     }

     public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId)
     {
          var notifications = await _repository.GetByUserIdAsync(userId);
          return notifications.Select(n => new NotificationDto
          {
               Id = n.Id,
               Title = n.Title,
               Body = n.Body,
               IsRead = n.IsRead,
               CreatedAt = n.CreatedAt,
               RelatedPollId = n.RelatedPollId,
               RelatedEventId = n.RelatedEventId,
               Channel = n.Channel.ToString()
          });
     }

     public Task<int> GetUnreadCountAsync(string userId) =>
         _repository.GetUnreadCountAsync(userId);

     public Task MarkAsReadAsync(int notificationId, string userId) =>
         _repository.MarkAsReadAsync(notificationId, userId);

     public Task MarkAllAsReadAsync(string userId) =>
         _repository.MarkAllAsReadAsync(userId);
}