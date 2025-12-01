namespace SDMS.GatewayApi.Configuration;

/// <summary>
/// Handles Gateway API configuration from environment variables
/// Follows the same pattern as SDMS.AuthenticationWebApp
/// </summary>
public static class GatewayConfiguration
{
    /// <summary>
    /// Gets configuration value from environment variable or appsettings.json
    /// Priority: Environment Variable > appsettings.json > defaultValue
    /// </summary>
    public static string GetConfigurationValue(IConfiguration configuration, string key, string defaultValue = "")
    {
        // Environment variables have highest priority
        var envValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(envValue))
        {
            return envValue;
        }

        // Fall back to appsettings.json
        var configValue = configuration[key];
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            return configValue;
        }

        // Use default value if provided
        return defaultValue;
    }

    /// <summary>
    /// Gets authentication authority URL from environment variable or configuration
    /// </summary>
    public static string GetAuthenticationAuthority(IConfiguration configuration)
    {
        return GetConfigurationValue(
            configuration,
            "SDMS_GatewayApi_Authentication_Authority",
            configuration["Authentication:Authority"] ?? "https://localhost:7001"
        );
    }

    /// <summary>
    /// Gets authentication audience from environment variable or configuration
    /// </summary>
    public static string GetAuthenticationAudience(IConfiguration configuration)
    {
        return GetConfigurationValue(
            configuration,
            "SDMS_GatewayApi_Authentication_Audience",
            configuration["Authentication:Audience"] ?? "api"
        );
    }

    /// <summary>
    /// Gets CORS allowed origins from environment variable or configuration
    /// </summary>
    public static string[] GetCorsAllowedOrigins(IConfiguration configuration)
    {
        var envOrigins = Environment.GetEnvironmentVariable("SDMS_GatewayApi_Cors_AllowedOrigins");
        if (!string.IsNullOrWhiteSpace(envOrigins))
        {
            return envOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(o => o.Trim())
                .ToArray();
        }

        var configOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (configOrigins != null && configOrigins.Length > 0)
        {
            return configOrigins;
        }

        // Default for development
        return new[] { "http://localhost:4200", "https://localhost:4200" };
    }

    /// <summary>
    /// Gets service URL for a specific service from environment variable
    /// </summary>
    public static string GetServiceUrl(IConfiguration configuration, string serviceName, string defaultUrl)
    {
        var envKey = $"SDMS_GatewayApi_Service_{serviceName}_Url";
        return GetConfigurationValue(configuration, envKey, defaultUrl);
    }
}

