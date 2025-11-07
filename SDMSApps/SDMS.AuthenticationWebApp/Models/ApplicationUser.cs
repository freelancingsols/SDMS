using Microsoft.AspNetCore.Identity;

namespace SDMS.AuthenticationWebApp.Models;

public class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ExternalId { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

