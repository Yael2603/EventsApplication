using EventsApp.API.Extensions;
using EventsApp.Application.DTOs.Notifications;
using EventsApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/notifications")]                        // /api/notifications
[Authorize]                                         // כל משתמש מחובר (כל תפקיד)
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notifications;
    public NotificationsController(INotificationService notifications) { _notifications = notifications; }  // הזרקה

    [HttpGet]                                       // GET /api/notifications
    public async Task<ActionResult<List<NotificationDto>>> GetMine(CancellationToken ct)
        => Ok(await _notifications.GetMineAsync(User.GetUserId(), ct));     // ההתראות של המחובר

    [HttpPut("{id:int}/read")]                      // PUT /api/notifications/4/read
    public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
    {
        await _notifications.MarkReadAsync(User.GetUserId(), id, ct);       // מסמן כנקראה
        return NoContent();                                                 // 204
    }
}
