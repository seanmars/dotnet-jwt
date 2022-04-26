using Microsoft.AspNetCore.Mvc;

namespace WebApp.Authorizations
{
    public class RolePermissionFilterAttribute : TypeFilterAttribute
    {
        public RolePermissionFilterAttribute(params string[] permissions)
            : base(typeof(RolePermissionAuthorizationFilter))
        {
            Arguments = new object[] { new RolePermissionRequirement(permissions) };
            Order = int.MinValue;
        }
    }
}