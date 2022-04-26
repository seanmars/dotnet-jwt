using Microsoft.AspNetCore.Authorization;
using WebApp.Constants;
using WebApp.Data;

namespace WebApp.Authorizations
{
    public class RolePermissionAuthorizationHandler : AuthorizationHandler<RolePermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolePermissionRequirement requirement)
        {
            var isSuperAdmin = context.User.IsInRole(ConstantData.DefaultRole.SuperAdminRole);
            if (isSuperAdmin)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Default role if no any permission
            if (!requirement.Permissions.Any())
            {
                if (context.User.IsInRole(ConstantData.DefaultRole.Member))
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            var hasPermission = requirement.Permissions
                .Any(permission => context.User.HasClaim(RolePermissionClaim.ClaimName, permission));

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }

            return Task.CompletedTask;
        }
    }
}