using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SDMS.AuthenticationWebApp.Data;
using SDMS.AuthenticationWebApp.Models;

namespace SDMS.AuthenticationWebApp.Repositories;

/// <summary>
/// Repository implementation for user data access
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string userId)
    {
        try
        {
            return await _userManager.FindByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        try
        {
            return await _userManager.FindByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<ApplicationUser?> GetByUsernameAsync(string username)
    {
        try
        {
            return await _userManager.FindByNameAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username: {Username}", username);
            return null;
        }
    }

    public async Task<ApplicationUser> CreateAsync(ApplicationUser user)
    {
        try
        {
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", user.Email);
            throw;
        }
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        try
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user exists by email: {Email}", email);
            return false;
        }
    }
}

