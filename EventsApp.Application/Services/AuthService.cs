using AutoMapper;
using EventsApp.Application.Abstractions;
using EventsApp.Application.DTOs.Auth;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly ICategoryRepository _categories;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenGenerator _tokens;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository users,
        ICategoryRepository categories,
        IPasswordHasher hasher,
        ITokenGenerator tokens,
        IMapper mapper)
    {
        _users = users;
        _categories = categories;
        _hasher = hasher;
        _tokens = tokens;
        _mapper = mapper;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken ct)
    {
        dto.Email = dto.Email.Trim().ToLowerInvariant();

        if (await _users.EmailExistsAsync(dto.Email, ct))
            throw new ConflictException("האימייל כבר רשום במערכת");

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = _hasher.Hash(dto.Password);
        user.Role = UserRole.Client;
        user.CreatedAtUtc = DateTime.UtcNow;

        await _users.AddAsync(user, ct);
        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> RegisterProviderAsync(RegisterProviderRequestDto dto, CancellationToken ct)
    {
        dto.Email = dto.Email.Trim().ToLowerInvariant();

        if (await _users.EmailExistsAsync(dto.Email, ct))
            throw new ConflictException("האימייל כבר רשום במערכת");

        if (!await _categories.ExistsAsync(dto.CategoryId, ct))
            throw new BadRequestException("הקטגוריה שנבחרה אינה קיימת");

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = _hasher.Hash(dto.Password);
        user.Role = UserRole.Provider;
        user.CreatedAtUtc = DateTime.UtcNow;

        var provider = _mapper.Map<Provider>(dto);
        provider.Status = ProviderStatus.Pending;
        provider.AcceptedTermsAt = DateTime.UtcNow;
        user.Provider = provider;

        await _users.AddAsync(user, ct);
        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken ct)
    {
        var email = dto.Email.Trim().ToLowerInvariant();

        var user = await _users.GetByEmailAsync(email, ct);

        if (user is null || !_hasher.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedException("אימייל או סיסמה שגויים");

        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user)
    {
        var response = _mapper.Map<AuthResponseDto>(user);
        response.Token = _tokens.Generate(user);
        return response;
    }
}
