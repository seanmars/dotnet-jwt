using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
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

    public Task<ApplicationUser?> GetUserByName(string userName, CancellationToken cancellationToken = default)
    {
        userName = _userManager.NormalizeName(userName);
        return _userManager.Users
            .Where(u => u.NormalizedUserName == userName)
            .Include(u => u.Claims)
            .Include(u => u.UserRoles!)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<(IdentityResult, ApplicationUser?)> ValidUserAsync(string userName, string password,
        CancellationToken cancellationToken = default)
    {
        var user = await GetUserByName(userName, cancellationToken);
        if (user == null)
        {
            return (IdentityResult.Failed(new IdentityError { Description = "User not found" }), null);
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            return (IdentityResult.Failed(new IdentityError { Description = "Password is incorrect" }), null);
        }

        return (IdentityResult.Success, user);
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

    private async Task<List<Claim>> GetClaimsAsync(ApplicationUser? user,
        bool includeUserClaim,
        bool includeRoleClaim,
        params string[] filters)
    {
        if (user == null)
        {
            return new List<Claim>();
        }

        var claims = new List<Claim>();
        if (includeUserClaim)
        {
            claims.AddRange(await _userManager.GetClaimsAsync(user));
        }

        if (includeRoleClaim)
        {
            var applicationRoles = user.UserRoles!
                .Select(userRole => userRole.Role!)
                .ToList();

            foreach (var role in applicationRoles)
            {
                claims.AddRange(await _roleManager.GetClaimsAsync(role));
            }
        }

        var resultClaims = (filters.Length == 0
                ? claims
                : claims.Where(x => filters.Contains(x.Type)))
            .ToList();

        return resultClaims;
    }

    private async Task<List<Claim>> GetClaimsAsync(string userName,
        bool includeUserClaim,
        bool includeRoleClaim,
        CancellationToken cancellationToken,
        params string[] filters)
    {
        var user = await GetUserByName(userName, cancellationToken);
        if (user == null)
        {
            return new List<Claim>();
        }

        return await GetClaimsAsync(user, includeUserClaim, includeRoleClaim, filters);
    }

    public Task<List<Claim>> GetClaimsExcludeRoleClaimAsync(ApplicationUser? user, params string[] filters)
    {
        return GetClaimsAsync(user, true, false, filters);
    }

    public Task<List<Claim>> GetClaimsExcludeRoleClaimAsync(string userName, params string[] filters)
    {
        return GetClaimsAsync(userName, true, false, default, filters);
    }

    public Task<List<Claim>> GetClaimsExcludeRoleClaimAsync(string userName, CancellationToken cancellationToken,
        params string[] filters)
    {
        return GetClaimsAsync(userName, true, false, cancellationToken, filters);
    }

    public Task<List<Claim>> GetClaimsIncludeRoleClaimAsync(ApplicationUser? user, params string[] filters)
    {
        return GetClaimsAsync(user, true, true, filters);
    }

    public Task<List<Claim>> GetClaimsIncludeRoleClaimAsync(string userName, params string[] filters)
    {
        return GetClaimsAsync(userName, true, true, default, filters);
    }

    public Task<List<Claim>> GetClaimsIncludeRoleClaimAsync(string userName, CancellationToken cancellationToken,
        params string[] filters)
    {
        return GetClaimsAsync(userName, true, true, cancellationToken, filters);
    }

    public Task<List<Claim>> GetClaimsOnlyRoleAsync(ApplicationUser? user, params string[] filters)
    {
        return GetClaimsAsync(user, false, true, filters);
    }

    public Task<List<Claim>> GetClaimsOnlyRoleAsync(string userName, params string[] filters)
    {
        return GetClaimsAsync(userName, false, true, default, filters);
    }

    public Task<List<Claim>> GetClaimsOnlyRoleAsync(string userName, CancellationToken cancellationToken,
        params string[] filters)
    {
        return GetClaimsAsync(userName, false, true, cancellationToken, filters);
    }

    public async Task<string> GenerateUserJti(ApplicationUser? user)
    {
        if (user == null)
        {
            return string.Empty;
        }

        var claims = user.Claims?
            .Where(uc => uc.ClaimType == JwtRegisteredClaimNames.Jti)
            .Select(uc => uc.ToClaim())
            .ToList();

        if (claims is { Count: > 0 })
        {
            _ = await _userManager.RemoveClaimsAsync(user, claims);
        }

        var jti = Guid.NewGuid().ToString();
        var result = await _userManager.AddClaimAsync(user, new Claim(JwtRegisteredClaimNames.Jti, jti));

        return result.Succeeded ? jti : string.Empty;
    }

    public async Task<bool> ValidUserJtiAsync(string userName, string jti, CancellationToken cancellationToken = default)
    {
        var user = await GetUserByName(userName, cancellationToken);
        if (user?.Claims == null)
        {
            return false;
        }

        var validJti = user.Claims
            .Any(uc => uc.ClaimType == JwtRegisteredClaimNames.Jti && uc.ClaimValue == jti);

        return validJti;
    }
}