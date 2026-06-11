using Microsoft.AspNetCore.Identity;
using HumanFlow.Domain.Security;

namespace HumanFlow.Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public PlatformRole PlatformRole { get; set; } = PlatformRole.None;
}

