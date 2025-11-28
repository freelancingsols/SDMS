namespace SDMS.AuthenticationWebApp.Models.Requests;

/// <summary>
/// Request model for password hash verification (for testing/debugging)
/// </summary>
public class VerifyPasswordHashRequest
{
    public string PasswordHash { get; set; } = string.Empty;
}

