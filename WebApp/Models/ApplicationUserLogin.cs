using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class ApplicationUserLogin : IdentityUserLogin<int>
{
    public virtual ApplicationUser User { get; set; }
}