namespace SDMS.AuthenticationWebApp.Models.Responses;

/// <summary>
/// Response model for login operation
/// </summary>
public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? ExternalProvider { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

