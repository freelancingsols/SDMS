using SDMS.AuthenticationWebApp.Models;

namespace SDMS.AuthenticationWebApp.Services;

public interface IExternalAuthService
{
    Task<(bool Success, ApplicationUser? User, string? Error)> AuthenticateWithAuth0Async(string idToken, string? code);
    Task<(bool Success, ApplicationUser? User, string? Error)> AuthenticateWithGoogleAsync(string idToken, string? code);
    Task<(bool Success, ApplicationUser? User, string? Error)> AuthenticateWithProviderAsync(string provider, string? idToken, string? code);
}

