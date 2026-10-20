using AutoMapper;
using EventsApp.Application.DTOs.Users;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;

namespace EventsApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IMapper _mapper;

    public UserService(IUserRepository users, IMapper mapper)
    {
        _users = users;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(id, ct)
                   ?? throw new NotFoundException("המשתמש לא נמצא");
        return _mapper.Map<UserDto>(user);
    }
}
