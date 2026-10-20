using EventsApp.Application.DTOs.Auth;
using EventsApp.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto dto, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("register-provider")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> RegisterProvider([FromBody] RegisterProviderRequestDto dto, CancellationToken ct)
    {
        var result = await _auth.RegisterProviderAsync(dto, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto dto, CancellationToken ct)
    {
        var result = await _auth.LoginAsync(dto, ct);
        return Ok(result);
    }
}
