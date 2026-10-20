using EventsApp.API.Extensions;
using EventsApp.Application.DTOs.Candidacies;
using EventsApp.Application.DTOs.Events;
using EventsApp.Application.Services;
using EventsApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/event-items")]                          // /api/event-items
[Authorize]                                         // ברירת מחדל: טוקן; כל נקודה מצמצמת תפקיד
public class EventItemsController : ControllerBase
{
    private readonly ICandidacyService _candidacies;
    private readonly IEventService _events;
    public EventItemsController(ICandidacyService candidacies, IEventService events)    // הזרקה
    {
        _candidacies = candidacies;
        _events = events;
    }

    [HttpGet("open")]                               // GET /api/event-items/open
    [Authorize(Roles = nameof(UserRole.Provider))]  // ספק בלבד
    public async Task<ActionResult<List<OpenItemDto>>> GetOpen(CancellationToken ct)
        => Ok(await _candidacies.GetOpenItemsAsync(User.GetUserId(), ct));      // הסעיפים הפתוחים של הספק

    [HttpGet("{itemId:int}/candidacies")]           // GET /api/event-items/7/candidacies
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<ActionResult<List<CandidacyDto>>> GetCandidacies(int itemId, CancellationToken ct)
        => Ok(await _candidacies.GetByItemAsync(itemId, ct));                   // המועמדים לסעיף

    [HttpPut("{itemId:int}/provider")]              // PUT /api/event-items/7/provider
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<ActionResult<EventItemDto>> AssignProvider(int itemId, AssignProviderDto dto, CancellationToken ct)
        => Ok(await _events.AssignProviderAsync(itemId, dto, ct));              // שיבוץ

    [HttpDelete("{itemId:int}")]                    // DELETE /api/event-items/7
    [Authorize(Roles = nameof(UserRole.Client))]    // לקוח בלבד
    public async Task<IActionResult> RemoveItem(int itemId, CancellationToken ct)
    {
        await _events.RemoveItemAsync(User.GetUserId(), itemId, ct);    // מסיר (או זורק שגיאה)
        return NoContent();                                             // 204
    }
}
