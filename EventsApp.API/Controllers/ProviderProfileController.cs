using EventsApp.API.Extensions;
using EventsApp.Application.DTOs.Providers;
using EventsApp.Application.Services;
using EventsApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/providers/me")]                         // /api/providers/me (נפרד מ-ProvidersController של האדמין)
[Authorize(Roles = nameof(UserRole.Provider))]      // ספקים בלבד, בכל סטטוס
public class ProviderProfileController : ControllerBase
{
    private readonly IProviderService _providers;
    public ProviderProfileController(IProviderService providers) { _providers = providers; }    // הזרקה

    [HttpGet]                                       // GET /api/providers/me
    public async Task<ActionResult<ProviderDto>> GetMine(CancellationToken ct)
        => Ok(await _providers.GetMineAsync(User.GetUserId(), ct));     // הפרופיל של המחובר

    [HttpPut]                                       // PUT /api/providers/me
    public async Task<ActionResult<ProviderDto>> UpdateMine([FromBody] UpdateProviderProfileDto dto, CancellationToken ct)
        => Ok(await _providers.UpdateMineAsync(User.GetUserId(), dto, ct));     // עדכון הפרופיל של המחובר

    [HttpGet("bookings")]                           // GET /api/providers/me/bookings
    public async Task<ActionResult<List<BookingDto>>> GetBookings(CancellationToken ct)
        => Ok(await _providers.GetBookingsAsync(User.GetUserId(), ct));     // הסעיפים ששובצו לי
}
