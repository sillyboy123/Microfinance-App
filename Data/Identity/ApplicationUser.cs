using System;
using Microsoft.AspNetCore.Identity;

namespace FinPlus.Data.Identity;

public class ApplicationUser : IdentityUser
{
    public DateTime RegisteredAt { get; set; }

    public ApplicationUser() : base()
    {
        RegisteredAt = DateTime.UtcNow;
    }
}
