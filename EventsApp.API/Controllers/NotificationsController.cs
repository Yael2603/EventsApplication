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

    [HttpPatch("{id:int}")]                         // PATCH /api/notifications/4  גוף: { "isRead": true }
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateNotificationDto dto, CancellationToken ct)
    {
        await _notifications.SetReadAsync(User.GetUserId(), id, dto.IsRead!.Value, ct);    // מעדכן את מצב הקריאה ([Required] מבטיח שיש ערך)
        return NoContent();                                                 // 204
    }
}
