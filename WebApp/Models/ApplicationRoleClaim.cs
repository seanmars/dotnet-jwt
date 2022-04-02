using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class ApplicationRoleClaim : IdentityRoleClaim<int>
{
    public virtual ApplicationRole Role { get; set; }
}