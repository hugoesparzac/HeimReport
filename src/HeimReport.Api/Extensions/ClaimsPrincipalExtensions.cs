using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HeimReport.Api.Enums;

namespace HeimReport.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (value is null || !int.TryParse(value, out var id))
        {
            throw new InvalidOperationException("User id claim not found or invalid in the current principal.");
        }

        return id;
    }

    public static SystemRole GetSystemRole(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.Role);

        if (value is null || !Enum.TryParse<SystemRole>(value, out var role))
        {
            throw new InvalidOperationException("Role claim not found or invalid in the current principal.");
        }

        return role;
    }
}