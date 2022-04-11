using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class ApplicationUserToken : IdentityUserToken<int>
{
    public virtual ApplicationUser? User { get; set; }
}