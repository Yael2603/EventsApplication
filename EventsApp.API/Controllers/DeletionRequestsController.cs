using EventsApp.API.Extensions;
using EventsApp.Application.DTOs.DeletionRequests;
using EventsApp.Application.Services;
using EventsApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]                                     // בלי Authorize ברמת הקונטרולר: לקוח ואדמין שניהם משתמשים בו
public class DeletionRequestsController : ControllerBase
{
    private readonly IDeletionRequestService _requests;
    public DeletionRequestsController(IDeletionRequestService requests) { _requests = requests; }   // הזרקה

    [HttpPost("api/events/{eventId:int}/deletion-request")]     // POST /api/events/5/deletion-request
    [Authorize(Roles = nameof(UserRole.Client))]    // לקוח בלבד (הבעלות נבדקת בשירות)
    public async Task<ActionResult<DeletionRequestDto>> Create([FromRoute] int eventId, [FromBody] CreateDeletionRequestDto dto, CancellationToken ct)
        => Created($"/api/events/{eventId}", await _requests.CreateAsync(User.GetUserId(), eventId, dto, ct));  // 201 + Location לאירוע שעליו הוגשה הבקשה

    [HttpGet("api/deletion-requests")]              // GET /api/deletion-requests?status=Pending
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<ActionResult<List<DeletionRequestDto>>> Get([FromQuery] DeletionRequestStatus? status, CancellationToken ct)
        => Ok(await _requests.GetAsync(status, ct));

    [HttpPatch("api/deletion-requests/{id:int}")]   // PATCH /api/deletion-requests/3  גוף: { "decision": "Approve" או "Reject", "adminNote": "..." }
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<IActionResult> Resolve([FromRoute] int id, [FromBody] ResolveDeletionRequestDto dto, CancellationToken ct)
    {
        if (dto.Decision == DeletionDecision.Approve)   // אישור
        {
            await _requests.ApproveAsync(id, ct);   // מוחק את האירוע ומשחרר את היום
            return NoContent();                     // 204: הצליח, אין מה להחזיר (האירוע והבקשה כבר לא קיימים)
        }

        var rejected = await _requests.RejectAsync(id,                  // דחייה
            new RejectDeletionRequestDto { AdminNote = dto.AdminNote }, ct);   // מעביר את הערת האדמין לשירות
        return Ok(rejected);                        // 200 עם הבקשה המעודכנת (סטטוס Rejected)
    }
}
