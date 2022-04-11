﻿using Microsoft.AspNetCore.Identity;

namespace WebApp.Models;

public class ApplicationUserClaim : IdentityUserClaim<int>
{
    public virtual ApplicationUser? User { get; set; }
}