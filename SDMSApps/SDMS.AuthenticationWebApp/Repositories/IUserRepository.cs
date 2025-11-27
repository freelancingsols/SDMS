using SDMS.AuthenticationWebApp.Models;

namespace SDMS.AuthenticationWebApp.Repositories;

/// <summary>
/// Repository interface for user data access
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    Task<ApplicationUser?> GetByIdAsync(string userId);

    /// <summary>
    /// Gets a user by email
    /// </summary>
    Task<ApplicationUser?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets a user by username
    /// </summary>
    Task<ApplicationUser?> GetByUsernameAsync(string username);

    /// <summary>
    /// Creates a new user
    /// </summary>
    Task<ApplicationUser> CreateAsync(ApplicationUser user);

    /// <summary>
    /// Updates an existing user
    /// </summary>
    Task UpdateAsync(ApplicationUser user);

    /// <summary>
    /// Checks if a user exists by email
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);
}

