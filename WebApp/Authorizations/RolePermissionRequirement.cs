using Microsoft.AspNetCore.Authorization;

namespace WebApp.Authorizations
{
    public class RolePermissionRequirement : IAuthorizationRequirement
    {
        public string[] Permissions { get; }

        public RolePermissionRequirement(params string[] permissions)
        {
            Permissions = permissions;
        }
    }
}