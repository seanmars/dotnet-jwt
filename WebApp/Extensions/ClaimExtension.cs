using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace WebApp.Extensions;

public static class ClaimExtension
{
    public static string? GetUserName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public static string? GetUserJti(this ClaimsPrincipal user)
    {
        return user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
    }
}