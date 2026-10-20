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
    public async Task<ActionResult<DeletionRequestDto>> Create(int eventId, CreateDeletionRequestDto dto, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _requests.CreateAsync(User.GetUserId(), eventId, dto, ct));  // 201

    [HttpGet("api/deletion-requests")]              // GET /api/deletion-requests?status=Pending
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<ActionResult<List<DeletionRequestDto>>> Get([FromQuery] DeletionRequestStatus? status, CancellationToken ct)
        => Ok(await _requests.GetAsync(status, ct));

    [HttpPut("api/deletion-requests/{id:int}/approve")]     // PUT /api/deletion-requests/3/approve
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        await _requests.ApproveAsync(id, ct);       // מוחק את האירוע ומשחרר את היום
        return NoContent();                         // 204: הצליח, אין מה להחזיר (האירוע והבקשה כבר לא קיימים)
    }

    [HttpPut("api/deletion-requests/{id:int}/reject")]      // PUT /api/deletion-requests/3/reject
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<ActionResult<DeletionRequestDto>> Reject(int id, RejectDeletionRequestDto dto, CancellationToken ct)
        => Ok(await _requests.RejectAsync(id, dto, ct));
}
