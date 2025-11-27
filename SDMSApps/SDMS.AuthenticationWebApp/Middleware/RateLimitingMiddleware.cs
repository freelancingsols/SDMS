using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using SDMS.AuthenticationWebApp.Models.Common;

namespace SDMS.AuthenticationWebApp.Middleware;

/// <summary>
/// Rate limiting middleware to prevent abuse
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;

    // In-memory store for rate limiting (use distributed cache in production)
    private static readonly ConcurrentDictionary<string, RateLimitInfo> _rateLimitStore = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for health checks and static files
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/ping") ||
            context.Request.Path.StartsWithSegments("/assets"))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);
        var endpoint = $"{context.Request.Method}:{context.Request.Path}";

        // Check rate limit
        if (!IsWithinRateLimit(clientId, endpoint))
        {
            var correlationId = context.Items["CorrelationId"]?.ToString();
            _logger.LogWarning(
                "Rate limit exceeded for client: {ClientId}, endpoint: {Endpoint}, CorrelationId: {CorrelationId}",
                clientId, endpoint, correlationId);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = ApiResponse.ErrorResponse(
                "Rate limit exceeded. Please try again later.",
                correlationId: correlationId);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Add rate limit headers
            var rateLimitInfo = GetRateLimitInfo(clientId, endpoint);
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequestsPerWindow.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = 
                Math.Max(0, _options.MaxRequestsPerWindow - rateLimitInfo.RequestCount).ToString();
            context.Response.Headers["X-RateLimit-Reset"] = 
                rateLimitInfo.WindowResetTime.ToUnixTimeSeconds().ToString();

            await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
            return;
        }

        // Update rate limit tracking
        UpdateRateLimit(clientId, endpoint);

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get authenticated user ID first
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private bool IsWithinRateLimit(string clientId, string endpoint)
    {
        var key = $"{clientId}:{endpoint}";
        var now = DateTimeOffset.UtcNow;

        if (_rateLimitStore.TryGetValue(key, out var info))
        {
            // Check if window has expired
            if (now >= info.WindowResetTime)
            {
                // Window expired, reset
                _rateLimitStore.TryRemove(key, out _);
                return true;
            }

            // Check if limit exceeded
            return info.RequestCount < _options.MaxRequestsPerWindow;
        }

        return true;
    }

    private void UpdateRateLimit(string clientId, string endpoint)
    {
        var key = $"{clientId}:{endpoint}";
        var now = DateTimeOffset.UtcNow;

        _rateLimitStore.AddOrUpdate(
            key,
            new RateLimitInfo
            {
                RequestCount = 1,
                WindowStartTime = now,
                WindowResetTime = now.Add(_options.Window)
            },
            (k, existing) =>
            {
                // Check if window expired
                if (now >= existing.WindowResetTime)
                {
                    return new RateLimitInfo
                    {
                        RequestCount = 1,
                        WindowStartTime = now,
                        WindowResetTime = now.Add(_options.Window)
                    };
                }

                // Increment count
                existing.RequestCount++;
                return existing;
            });
    }

    private RateLimitInfo GetRateLimitInfo(string clientId, string endpoint)
    {
        var key = $"{clientId}:{endpoint}";
        if (_rateLimitStore.TryGetValue(key, out var info))
        {
            return info;
        }

        return new RateLimitInfo
        {
            RequestCount = 0,
            WindowStartTime = DateTimeOffset.UtcNow,
            WindowResetTime = DateTimeOffset.UtcNow.Add(_options.Window)
        };
    }

    private class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTimeOffset WindowStartTime { get; set; }
        public DateTimeOffset WindowResetTime { get; set; }
    }
}

/// <summary>
/// Rate limiting options
/// </summary>
public class RateLimitOptions
{
    public int MaxRequestsPerWindow { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}

/// <summary>
/// Extension method to register rate limiting middleware
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder builder,
        Action<RateLimitOptions>? configure = null)
    {
        var options = new RateLimitOptions();
        configure?.Invoke(options);

        return builder.UseMiddleware<RateLimitingMiddleware>(options);
    }
}

