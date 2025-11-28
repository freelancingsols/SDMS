namespace SDMS.AuthenticationWebApp.Models.Responses;

/// <summary>
/// Response model for registration operation
/// </summary>
public class RegisterResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Message { get; set; } = string.Empty;
}

