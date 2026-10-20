using EventsApp.Application.DTOs.Users;

namespace EventsApp.Application.Services;

public interface IUserService
{
    Task<UserDto> GetByIdAsync(int id, CancellationToken ct);
}
