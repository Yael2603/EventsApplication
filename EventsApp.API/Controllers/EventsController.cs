using EventsApp.API.Extensions;
using EventsApp.Application.DTOs.Common;
using EventsApp.Application.DTOs.Events;
using EventsApp.Application.Services;
using EventsApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/events")]                               // /api/events
[Authorize]                                         // ברירת מחדל: כל נקודה דורשת טוקן
public class EventsController : ControllerBase
{
    private readonly IEventService _events;
    public EventsController(IEventService events) { _events = events; }     // הזרקה

    [HttpPost]                                      // POST /api/events
    [Authorize(Roles = nameof(UserRole.Client))]    // רק לקוח; אחרת 403
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventDto dto, CancellationToken ct)
    {
        var result = await _events.CreateAsync(User.GetUserId(), dto, ct);  // המשתמש מהטוקן הוא בעל האירוע
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);   // 201 + כותרת Location לאירוע החדש
    }

    [HttpGet("my")]                                 // GET /api/events/my
    [Authorize(Roles = nameof(UserRole.Client))]    // רק לקוח
    public async Task<ActionResult<List<EventDto>>> GetMine(CancellationToken ct)
        => Ok(await _events.GetMineAsync(User.GetUserId(), ct));            // האירועים של המחובר

    [HttpGet("{id:int}")]                           // GET /api/events/5; :int מבדיל מהכתובת "my"
    public async Task<ActionResult<EventDto>> GetById([FromRoute] int id, CancellationToken ct)
        => Ok(await _events.GetByIdAsync(id, User.GetUserId(), User.IsInRole(nameof(UserRole.Admin)), ct));   // בעלות נבדקת בשירות

    [HttpGet]                                       // GET /api/events?page=1&pageSize=10
    [Authorize(Roles = nameof(UserRole.Admin))]     // רק אדמין
    public async Task<ActionResult<PagedResultDto<EventDto>>> GetAll([FromQuery] PaginationQueryDto query, CancellationToken ct)
        => Ok(await _events.GetPageAsync(query, ct));   // עמוד אחד של אירועים + נתוני עימוד

    [HttpPut("{id:int}")]                           // PUT /api/events/5
    [Authorize(Roles = nameof(UserRole.Client))]    // לקוח בלבד (הבעלות בשירות)
    public async Task<ActionResult<EventDto>> Update([FromRoute] int id, [FromBody] UpdateEventDto dto, CancellationToken ct)
        => Ok(await _events.UpdateAsync(User.GetUserId(), id, dto, ct));            // 200

    [HttpPost("{id:int}/items")]                    // POST /api/events/5/items
    [Authorize(Roles = nameof(UserRole.Client))]    // לקוח בלבד
    public async Task<ActionResult<EventItemDto>> AddItem([FromRoute] int id, [FromBody] CreateEventItemDto dto, CancellationToken ct)
        => Created($"/api/events/{id}", await _events.AddItemAsync(User.GetUserId(), id, dto, ct));     // 201 + Location לאירוע שהסעיף נוסף אליו
}
