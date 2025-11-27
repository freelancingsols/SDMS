using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using SDMS.AuthenticationWebApp.Constants;
using SDMS.AuthenticationWebApp.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SDMS.AuthenticationWebApp.Configuration;

/// <summary>
/// Configuration helper for OpenIddict setup
/// </summary>
public static class OpenIddictConfiguration
{
    /// <summary>
    /// Configures OpenIddict services
    /// </summary>
    public static IServiceCollection AddOpenIddictConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
            })
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("/connect/token");
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetUserinfoEndpointUris("/connect/userinfo");
                options.SetLogoutEndpointUris("/connect/logout");
                options.SetIntrospectionEndpointUris("/connect/introspect");

                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                options.AllowRefreshTokenFlow();
                options.AllowClientCredentialsFlow();
                options.AllowPasswordFlow(); // Allow password grant for testing and API access

                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "api", "offline_access");

                // Signing and encryption - use development certificates for now
                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableUserinfoEndpointPassthrough()
                    .EnableLogoutEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }
}

