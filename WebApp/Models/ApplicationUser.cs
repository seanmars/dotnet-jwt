using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class ApplicationUser : IdentityUser<int>
{
    public virtual ICollection<ApplicationUserClaim>? Claims { get; set; }
    public virtual ICollection<ApplicationUserLogin>? Logins { get; set; }
    public virtual ICollection<ApplicationUserToken>? Tokens { get; set; }
    public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
}