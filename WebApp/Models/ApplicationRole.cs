using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class ApplicationRole : IdentityRole<int>
{
    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    public virtual ICollection<ApplicationRoleClaim> RoleClaims { get; set; }
}