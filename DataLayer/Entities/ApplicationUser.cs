using Microsoft.AspNetCore.Identity;

namespace DataLayer.Entities;

public class ApplicationUser : IdentityUser
{
    public int? Employeeid { get; set; }

    public virtual Employee? Employee { get; set; }
}
