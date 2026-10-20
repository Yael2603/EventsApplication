using EventsApp.Application.DTOs.Providers;
using EventsApp.Application.Services;
using EventsApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/providers")]                            // /api/providers
[Authorize(Roles = nameof(UserRole.Admin))]         // כל הקונטרולר: אדמין בלבד
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providers;
    public ProvidersController(IProviderService providers) { _providers = providers; }  // הזרקה

    [HttpGet]                                       // GET /api/providers?status=Pending
    public async Task<ActionResult<List<ProviderDto>>> Get([FromQuery] ProviderStatus? status, CancellationToken ct)
        => Ok(await _providers.GetAsync(status, ct));       // 200 עם הרשימה

    [HttpPut("{id:int}/status")]                    // PUT /api/providers/3/status
    public async Task<ActionResult<ProviderDto>> UpdateStatus(int id, UpdateProviderStatusDto dto, CancellationToken ct)
        => Ok(await _providers.UpdateStatusAsync(id, dto, ct));     // 200 עם הספק המעודכן
}
