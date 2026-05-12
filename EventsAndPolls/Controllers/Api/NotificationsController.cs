using EventsAndPolls.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventsAndPolls.Controllers.Api;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
     private readonly INotificationService _notificationService;
     private readonly ILogger<NotificationsController> _logger;

     public NotificationsController(
         INotificationService notificationService,
         ILogger<NotificationsController> logger)
     {
          _notificationService = notificationService;
          _logger = logger;
     }

     // GET api/notifications — get current user's notifications
     [HttpGet]
     public async Task<IActionResult> GetNotifications()
     {
          try
          {
               var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
               if (string.IsNullOrEmpty(userId)) return Unauthorized();

               var notifications = await _notificationService.GetUserNotificationsAsync(userId);
               return Ok(notifications);
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting notifications");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     // GET api/notifications/unread-count
     [HttpGet("unread-count")]
     public async Task<IActionResult> GetUnreadCount()
     {
          try
          {
               var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
               if (string.IsNullOrEmpty(userId)) return Unauthorized();

               var count = await _notificationService.GetUnreadCountAsync(userId);
               return Ok(new { count });
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error getting unread count");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     // POST api/notifications/{id}/read
     [HttpPost("{id}/read")]
     public async Task<IActionResult> MarkAsRead(int id)
     {
          try
          {
               var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
               if (string.IsNullOrEmpty(userId)) return Unauthorized();

               await _notificationService.MarkAsReadAsync(id, userId);
               return Ok();
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error marking notification as read");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }

     // POST api/notifications/read-all
     [HttpPost("read-all")]
     public async Task<IActionResult> MarkAllAsRead()
     {
          try
          {
               var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
               if (string.IsNullOrEmpty(userId)) return Unauthorized();

               await _notificationService.MarkAllAsReadAsync(userId);
               return Ok();
          }
          catch (Exception ex)
          {
               _logger.LogError(ex, "Error marking all as read");
               return StatusCode(500, new { error = "An error occurred" });
          }
     }
}