using EventsApp.API.Extensions;
using EventsApp.Application.DTOs.Candidacies;
using EventsApp.Application.Services;
using EventsApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/candidacies")]                          // /api/candidacies
[Authorize(Roles = nameof(UserRole.Provider))]      // ספקים בלבד
public class CandidaciesController : ControllerBase
{
    private readonly ICandidacyService _candidacies;
    public CandidaciesController(ICandidacyService candidacies) { _candidacies = candidacies; }  // הזרקה

    [HttpPost]                                      // POST /api/candidacies
    public async Task<ActionResult<CandidacyDto>> Apply([FromBody] CreateCandidacyDto dto, CancellationToken ct)
    {
        var result = await _candidacies.ApplyAsync(User.GetUserId(), dto, ct);  // הספק מהטוקן
        return StatusCode(StatusCodes.Status201Created, result);                // 201 (אין כתובת "קריאה בודדת" למועמדות)
    }

    [HttpGet("my")]                                 // GET /api/candidacies/my
    public async Task<ActionResult<List<MyCandidacyDto>>> GetMine(CancellationToken ct)
        => Ok(await _candidacies.GetMineAsync(User.GetUserId(), ct));       // המועמדויות שלי

    [HttpDelete("{id:int}")]                        // DELETE /api/candidacies/3
    public async Task<IActionResult> Withdraw([FromRoute] int id, CancellationToken ct)
    {
        await _candidacies.WithdrawAsync(User.GetUserId(), id, ct);         // מבטל (או זורק שגיאה)
        return NoContent();                                                 // 204
    }
}
