namespace SDMS.AuthenticationWebApp.Middleware;

/// <summary>
/// Middleware to add security headers to responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(RequestDelegate next, SecurityHeadersOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers
        if (_options.EnableContentSecurityPolicy)
        {
            context.Response.Headers["Content-Security-Policy"] = 
                "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self' https:; frame-ancestors 'none';";
        }

        if (_options.EnableStrictTransportSecurity)
        {
            context.Response.Headers["Strict-Transport-Security"] = 
                $"max-age={_options.HstsMaxAge}; includeSubDomains";
        }

        if (_options.EnableXContentTypeOptions)
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        }

        if (_options.EnableXFrameOptions)
        {
            context.Response.Headers["X-Frame-Options"] = "DENY";
        }

        if (_options.EnableXssProtection)
        {
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
        }

        if (_options.EnableReferrerPolicy)
        {
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        }

        if (_options.EnablePermissionsPolicy)
        {
            context.Response.Headers["Permissions-Policy"] = 
                "geolocation=(), microphone=(), camera=()";
        }

        // Remove server header
        if (_options.RemoveServerHeader)
        {
            context.Response.Headers.Remove("Server");
        }

        await _next(context);
    }
}

/// <summary>
/// Security headers options
/// </summary>
public class SecurityHeadersOptions
{
    public bool EnableContentSecurityPolicy { get; set; } = true;
    public bool EnableStrictTransportSecurity { get; set; } = true;
    public bool EnableXContentTypeOptions { get; set; } = true;
    public bool EnableXFrameOptions { get; set; } = true;
    public bool EnableXssProtection { get; set; } = true;
    public bool EnableReferrerPolicy { get; set; } = true;
    public bool EnablePermissionsPolicy { get; set; } = true;
    public bool RemoveServerHeader { get; set; } = true;
    public int HstsMaxAge { get; set; } = 31536000; // 1 year
}

/// <summary>
/// Extension method to register security headers middleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(
        this IApplicationBuilder builder,
        Action<SecurityHeadersOptions>? configure = null)
    {
        var options = new SecurityHeadersOptions();
        configure?.Invoke(options);

        return builder.UseMiddleware<SecurityHeadersMiddleware>(options);
    }
}

