using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApp.Authorizations
{
    public class RolePermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IAuthorizationService _authService;
        private readonly RolePermissionRequirement _requirement;

        public RolePermissionAuthorizationFilter(IAuthorizationService authService, RolePermissionRequirement requirement)
        {
            _authService = authService;
            _requirement = requirement;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var ok = await _authService.AuthorizeAsync(context.HttpContext.User, null, _requirement);
            if (!ok.Succeeded)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}