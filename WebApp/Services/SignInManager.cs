using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using WebApp.Constants;
using WebApp.Data;
using WebApp.Helpers;

namespace WebApp.Services;

public class SignInManager
{
    private readonly ILogger<SignInManager> _logger;

    private readonly JwtHelper _jwtHelper;
    private readonly AccountService _accountService;

    public SignInManager(ILogger<SignInManager> logger,
        JwtHelper jwtHelper,
        AccountService accountService
    )
    {
        _logger = logger;
        _jwtHelper = jwtHelper;
        _accountService = accountService;
    }

    public async Task<(IdentityResult Result, string? Token)> SignInAsync(string userName, string password)
    {
        var (result, user) = await _accountService.ValidUserAsync(userName, password);
        if (!result.Succeeded)
        {
            return (result, null);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, ConstantData.DefaultRole.Member),
        };

        var permissionClaims = await _accountService.GetClaimsOnlyRoleAsync(
            user, RolePermissionClaim.ClaimName);

        claims.AddRange(permissionClaims);

        var token = _jwtHelper.GenerateToken(userName, claims);

        return (result, token);
    }
}