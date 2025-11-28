using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SDMS.AuthenticationWebApp.Configuration;

/// <summary>
/// Configuration helper for CORS setup
/// </summary>
public static class CorsConfiguration
{
    /// <summary>
    /// Configures CORS services
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var b2cUrl = configuration["SDMS_B2CWebApp_url"]
            ?? throw new InvalidOperationException("Missing required configuration: SDMS_B2CWebApp_url. Set in appsettings.json or environment variable.");

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                var origins = new List<string>
                {
                    "http://localhost:4200",
                    "https://localhost:4200"
                };

                // Add B2C URL if not already in list
                if (!string.IsNullOrEmpty(b2cUrl) && !origins.Contains(b2cUrl))
                {
                    origins.Add(b2cUrl);
                }

                // Use SetIsOriginAllowed to allow configured origins, Vercel preview deployments, and localhost
                policy.SetIsOriginAllowed(origin =>
                {
                    // Allow configured origins
                    if (origins.Contains(origin))
                    {
                        return true;
                    }

                    // Allow any Vercel preview deployment (*.vercel.app)
                    if (origin.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // Allow localhost for development (any port)
                    if (origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase) ||
                        origin.StartsWith("https://localhost:", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    return false;
                })
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });

        return services;
    }
}

