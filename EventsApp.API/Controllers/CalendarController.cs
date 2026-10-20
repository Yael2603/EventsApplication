using EventsApp.Application.DTOs.Calendar;
using EventsApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]                                     // התנהגות API (ולידציה אוטומטית ו-400)
[Route("api/calendar")]                             // /api/calendar
[Authorize]                                         // דורש טוקן (כל תפקיד)
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendar;
    public CalendarController(ICalendarService calendar) { _calendar = calendar; }   // הזרקה

    [HttpGet]                                       // GET /api/calendar?from=2026-12-01&to=2026-12-31
    public async Task<ActionResult<List<CalendarDayDto>>> Get(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)  // שני התאריכים אופציונליים, מתוך ה-URL
        => Ok(await _calendar.GetDaysAsync(from, to, ct));                           // 200 עם הימים
}
