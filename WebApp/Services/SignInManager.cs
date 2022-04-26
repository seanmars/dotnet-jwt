using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using WebApp.Data;
using WebApp.Models;

namespace WebApp.Services;

public class SignInManager
{
    private readonly ILogger<SignInManager> _logger;

    public UserManager<ApplicationUser> UserManager { get; set; }
    public RoleManager<ApplicationRole> RoleManager { get; set; }
    public AccountService AccountService { get; set; }
    public JwtService JwtService { get; set; }

    public SignInManager(ILogger<SignInManager> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        AccountService accountService,
        JwtService jwtService
    )
    {
        _logger = logger;
        UserManager = userManager;
        RoleManager = roleManager;
        AccountService = accountService;
        JwtService = jwtService;
    }

    public async Task<(IdentityResult Result, string? Token)> SignInAsync(string userName, string password)
    {
        var result = await AccountService.ValidUserAsync(userName, password);
        if (!result.Succeeded)
        {
            return (result, null);
        }

        // TODO: get all role permissions
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, ConstantData.DefaultRole.Member),
            new(ClaimTypes.Role, "admin"),
        };
        var token = JwtService.GenerateToken(userName, claims);

        return (result, token);
    }
}