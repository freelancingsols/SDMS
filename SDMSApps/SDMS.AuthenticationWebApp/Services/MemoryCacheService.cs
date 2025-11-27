using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace SDMS.AuthenticationWebApp.Services;

/// <summary>
/// In-memory cache service implementation
/// For production, use distributed cache (Redis, etc.)
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(
        IMemoryCache memoryCache,
        ILogger<MemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var cachedValue))
            {
                if (cachedValue is T typedValue)
                {
                    return Task.FromResult<T?>(typedValue);
                }

                // Try to deserialize if stored as string
                if (cachedValue is string jsonString)
                {
                    var deserialized = JsonSerializer.Deserialize<T>(jsonString);
                    return Task.FromResult(deserialized);
                }
            }

            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return Task.FromResult<T?>(null);
        }
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15),
                SlidingExpiration = expiration.HasValue ? null : TimeSpan.FromMinutes(5)
            };

            _memoryCache.Set(key, value, options);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            return Task.CompletedTask;
        }
    }

    public Task RemoveByPatternAsync(string pattern)
    {
        // Memory cache doesn't support pattern matching
        // In production with distributed cache (Redis), this would use SCAN or similar
        _logger.LogWarning("Pattern-based cache removal not supported in memory cache. Pattern: {Pattern}", pattern);
        return Task.CompletedTask;
    }
}

