using EventsApp.Application.DTOs.Auth;

namespace EventsApp.Application.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct);
    Task<AuthResponseDto> RegisterProviderAsync(RegisterProviderRequestDto dto, CancellationToken ct);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct);
}
