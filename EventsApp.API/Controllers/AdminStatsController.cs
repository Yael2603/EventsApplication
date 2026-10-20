using EventsApp.Application.DTOs.Stats;
using EventsApp.Application.Services;
using EventsApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/admin/stats")]                          // /api/admin/stats
[Authorize(Roles = nameof(UserRole.Admin))]         // אדמין בלבד
public class AdminStatsController : ControllerBase
{
    private readonly IStatsService _stats;
    public AdminStatsController(IStatsService stats) { _stats = stats; }    // הזרקה

    [HttpGet]                                       // GET /api/admin/stats
    public async Task<ActionResult<AdminStatsDto>> Get(CancellationToken ct)
        => Ok(await _stats.GetAsync(ct));           // 200 עם המספרים
}
