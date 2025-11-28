using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Models.Responses;

namespace SDMS.AuthenticationWebApp.Services;

/// <summary>
/// Service interface for user operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets user information by user ID
    /// </summary>
    Task<UserInfoResponse?> GetUserInfoAsync(string userId);

    /// <summary>
    /// Gets user information by email
    /// </summary>
    Task<UserInfoResponse?> GetUserInfoByEmailAsync(string email);

    /// <summary>
    /// Updates user last login date
    /// </summary>
    Task UpdateLastLoginDateAsync(string userId);
}

