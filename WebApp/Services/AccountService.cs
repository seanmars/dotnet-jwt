using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Constants;
using WebApp.Data;
using WebApp.Models;
using WebApp.Models.ViewModels;

namespace WebApp.Services;

public class AccountService
{
    private readonly ILogger<AccountService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public AccountService(ILogger<AccountService> logger,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IdentityResult> CreateUser(string email, string userName, string password,
        bool emailConfirmed = true, string? role = default)
    {
        var user = new ApplicationUser
        {
            Email = email,
            NormalizedEmail = _userManager.NormalizeEmail(email),
            EmailConfirmed = emailConfirmed,
            UserName = userName,
            NormalizedUserName = _userManager.NormalizeName(userName)
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return result;
        }

        if (role != null)
        {
            await _userManager.AddToRoleAsync(user, role);
        }

        return result;
    }

    public async Task<IdentityResult?> AddUserToRole(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            return null;
        }

        return await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<IdentityResult> ValidUser(string userName, string password)
    {
        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "UserNotFound",
                Description = "User not found"
            });
        }

        var result = await _userManager.CheckPasswordAsync(user, password);
        if (!result)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidPassword",
                Description = "Invalid password"
            });
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> CreateRole(string roleName, IEnumerable<string> permissions)
    {
        var existedRole = await _roleManager.FindByNameAsync(roleName);
        if (existedRole != null)
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "RoleExisted",
                Description = "Role existed"
            });
        }

        var role = new ApplicationRole
        {
            Name = roleName,
            NormalizedName = _roleManager.NormalizeKey(roleName)
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            return result;
        }

        foreach (var permission in permissions)
        {
            var claimResult = await _roleManager.AddClaimAsync(role, new Claim(RolePermissionClaim.ClaimName, permission));
            if (!claimResult.Succeeded)
            {
                _logger.LogWarning("Failed to add permission {Permission} to role {Role}", permission, roleName);
            }
        }

        return result;
    }

    public async Task<IList<RoleViewModel>> GetAllRole(bool includeSuperAdmin = false)
    {
        var roles = await _roleManager.Roles
            .Where(x => includeSuperAdmin || x.Name != ConstantData.DefaultRole.SuperAdminRole)
            .ToListAsync();

        if (!roles.Any())
        {
            return ImmutableArray<RoleViewModel>.Empty;
        }

        var viewModels = roles
            .Select(role => new RoleViewModel()
            {
                Id = role.Id,
                Name = role.Name,
            })
            .ToList();

        return viewModels;
    }

    public async Task<ApplicationRole?> FindRoleById(int roleId, CancellationToken stoppingToken = default)
    {
        var role = await _roleManager.Roles
            .Where(r => r.Id == roleId)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(stoppingToken);

        return role;
    }
}