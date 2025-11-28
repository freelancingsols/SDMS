using System.Security.Claims;
using SDMS.AuthenticationWebApp.Models;

namespace SDMS.AuthenticationWebApp.Services;

/// <summary>
/// Service interface for token operations
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Creates a claims principal for the specified user
    /// </summary>
    Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user, string clientId = "sdms_frontend");

    /// <summary>
    /// Generates an access token for the specified user
    /// </summary>
    Task<string> GenerateAccessTokenAsync(ApplicationUser user, string clientId = "sdms_frontend");
}

