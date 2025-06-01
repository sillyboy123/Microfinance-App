using System;
using Microsoft.AspNetCore.Identity;

namespace MicrofinanceApp.Data.Identity;

public class ApplicationUser : IdentityUser
{
    public DateTime RegisteredAt { get; set; }

    public ApplicationUser() : base()
    {
        RegisteredAt = DateTime.UtcNow;
    }
}
