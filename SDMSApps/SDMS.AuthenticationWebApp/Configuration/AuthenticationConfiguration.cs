using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SDMS.AuthenticationWebApp.Constants;
using SDMS.AuthenticationWebApp.Data;
using SDMS.AuthenticationWebApp.Models;

namespace SDMS.AuthenticationWebApp.Configuration;

/// <summary>
/// Configuration helper for authentication setup
/// </summary>
public static class AuthenticationConfiguration
{
    /// <summary>
    /// Configures authentication services
    /// </summary>
    public static IServiceCollection AddAuthenticationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Identity configuration
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Get configuration values
        var loginUrl = configuration[ConfigurationKeys.LoginUrl]
            ?? throw new InvalidOperationException($"Missing required configuration: {ConfigurationKeys.LoginUrl}. Set in appsettings.json or environment variable.");
        var logoutUrl = configuration[ConfigurationKeys.LogoutUrl]
            ?? throw new InvalidOperationException($"Missing required configuration: {ConfigurationKeys.LogoutUrl}. Set in appsettings.json or environment variable.");
        var errorUrl = configuration[ConfigurationKeys.ErrorUrl]
            ?? throw new InvalidOperationException($"Missing required configuration: {ConfigurationKeys.ErrorUrl}. Set in appsettings.json or environment variable.");
        var returnUrlParameter = configuration[ConfigurationKeys.ReturnUrlParameter]
            ?? throw new InvalidOperationException($"Missing required configuration: {ConfigurationKeys.ReturnUrlParameter}. Set in appsettings.json or environment variable.");

        // Configure authentication defaults
        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        });

        // Add Google authentication if configured
        var googleClientId = configuration[ConfigurationKeys.ExternalAuthGoogleClientId];
        var googleClientSecret = configuration[ConfigurationKeys.ExternalAuthGoogleClientSecret];

        if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
        {
            authBuilder.AddGoogle(options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.SaveTokens = true;
            });
        }

        // Configure application cookie
        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = loginUrl;
            options.LogoutPath = logoutUrl;
            options.AccessDeniedPath = errorUrl;
            options.ReturnUrlParameter = returnUrlParameter;

            options.Events.OnRedirectToLogin = context =>
            {
                // For API calls, always return 401 instead of redirecting
                if (context.Request.Path.StartsWithSegments("/api") ||
                    context.Request.Path.StartsWithSegments("/account") ||
                    context.Request.Path.StartsWithSegments("/connect/token") ||
                    context.Request.Path.StartsWithSegments("/connect/userinfo"))
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                }

                // For browser requests: redirect to login when unauthorized
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"{loginUrl}?{returnUrlParameter}={Uri.EscapeDataString(returnUrl)}");
                return Task.CompletedTask;
            };
        });

        return services;
    }
}

