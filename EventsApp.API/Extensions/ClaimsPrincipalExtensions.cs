using System.Security.Claims;
using EventsApp.Application.Exceptions;

namespace EventsApp.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id)
            ? id
            : throw new UnauthorizedException("טוקן לא תקין");
    }
}
