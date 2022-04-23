using Microsoft.AspNetCore.Identity;
using WebApp.Models;

namespace WebApp.Services;

public class SignInManager
{
    private readonly ILogger<SignInManager> _logger;

    public UserManager<ApplicationUser> UserManager { get; set; }
    public IUserClaimsPrincipalFactory<ApplicationUser> ClaimsFactory { get; set; }
    public RoleManager<ApplicationRole> RoleManager { get; set; }
    public JwtService Jwt { get; set; }

    public SignInManager(ILogger<SignInManager> logger,
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        RoleManager<ApplicationRole> roleManager,
        JwtService jwt
    )
    {
        _logger = logger;
        UserManager = userManager;
        RoleManager = roleManager;
        Jwt = jwt;
        ClaimsFactory = claimsFactory;
    }

    public async Task<(IdentityResult Result, string? Token)> SignInAsync(string userName, string password)
    {
        var user = await UserManager.FindByNameAsync(userName);
        if (user == null)
        {
            return (IdentityResult.Failed(new IdentityError { Description = "User not found" }), null);
        }

        var result = await UserManager.CheckPasswordAsync(user, password);
        if (!result)
        {
            return (IdentityResult.Failed(new IdentityError { Description = "Password is incorrect" }), null);
        }

        // TODO: using claims factory to create principal and add jwt token

        var token = Jwt.GenerateToken(user.NormalizedUserName);

        return (IdentityResult.Success, token);
    }
}