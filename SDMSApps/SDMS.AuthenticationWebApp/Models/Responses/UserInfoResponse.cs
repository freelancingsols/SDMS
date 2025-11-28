namespace SDMS.AuthenticationWebApp.Models.Responses;

/// <summary>
/// Response model for user information
/// </summary>
public class UserInfoResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ExternalProvider { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
}

