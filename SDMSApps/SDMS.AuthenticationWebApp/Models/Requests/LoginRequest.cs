namespace SDMS.AuthenticationWebApp.Models.Requests;

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    public string? Provider { get; set; }
    public string? IdToken { get; set; }
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

