using Microsoft.AspNetCore.Identity;
using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Models.Responses;
using SDMS.AuthenticationWebApp.Repositories;

namespace SDMS.AuthenticationWebApp.Services;

/// <summary>
/// Service for user operations with caching
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserService> _logger;
    private const string UserInfoCachePrefix = "userinfo:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public UserService(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        ICacheService cacheService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UserInfoResponse?> GetUserInfoAsync(string userId)
    {
        try
        {
            var cacheKey = $"{UserInfoCachePrefix}{userId}";
            
            // Try to get from cache first
            var cachedUserInfo = await _cacheService.GetAsync<UserInfoResponse>(cacheKey);
            if (cachedUserInfo != null)
            {
                _logger.LogDebug("User info retrieved from cache for userId: {UserId}", userId);
                return cachedUserInfo;
            }

            // Cache miss - get from database using repository
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var userInfo = new UserInfoResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                ExternalProvider = user.ExternalProvider,
                ProfilePictureUrl = user.ProfilePictureUrl,
                LastLoginDate = user.LastLoginDate,
                Roles = await _userManager.GetRolesAsync(user)
            };

            // Cache the result
            await _cacheService.SetAsync(cacheKey, userInfo, CacheExpiration);

            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for userId: {UserId}", userId);
            return null;
        }
    }

    public async Task<UserInfoResponse?> GetUserInfoByEmailAsync(string email)
    {
        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                return null;
            }

            return await GetUserInfoAsync(user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for email: {Email}", email);
            return null;
        }
    }

    public async Task UpdateLastLoginDateAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.LastLoginDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);

                // Invalidate cache
                var cacheKey = $"{UserInfoCachePrefix}{userId}";
                await _cacheService.RemoveAsync(cacheKey);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login date for userId: {UserId}", userId);
        }
    }
}

