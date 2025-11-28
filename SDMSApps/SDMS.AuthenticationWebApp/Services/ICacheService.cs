namespace SDMS.AuthenticationWebApp.Services;

/// <summary>
/// Cache service interface for distributed caching
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets a value in cache
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;

    /// <summary>
    /// Removes a value from cache
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all values matching a pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
}

