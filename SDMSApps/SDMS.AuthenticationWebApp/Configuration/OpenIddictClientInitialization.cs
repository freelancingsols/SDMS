using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using SDMS.AuthenticationWebApp.Constants;
using SDMS.AuthenticationWebApp.Data;
using SDMS.AuthenticationWebApp.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SDMS.AuthenticationWebApp.Configuration;

/// <summary>
/// Helper class for initializing OpenIddict client configuration
/// </summary>
public static class OpenIddictClientInitialization
{
    /// <summary>
    /// Initializes OpenIddict client and default data
    /// </summary>
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        ILogger logger)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var applicationManager = serviceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Get B2C URL from configuration
        var b2cUrlForClient = configuration["SDMS_B2CWebApp_url"]
            ?? throw new InvalidOperationException("Missing required configuration: SDMS_B2CWebApp_url. Set in appsettings.json or environment variable.");

        // Parse redirect URIs
        var defaultRedirectUris = new HashSet<Uri>();
        var defaultPostLogoutRedirectUris = new HashSet<Uri>();

        // Add B2C URL redirect URIs (required)
        if (!string.IsNullOrEmpty(b2cUrlForClient))
        {
            var normalizedB2cUrl = b2cUrlForClient.TrimEnd('/');
            defaultRedirectUris.Add(new Uri($"{normalizedB2cUrl}/auth-callback"));
            defaultPostLogoutRedirectUris.Add(new Uri(normalizedB2cUrl));
        }

        // Get redirect URIs from configuration
        var redirectUrisConfig = configuration[ConfigurationKeys.RedirectUris];
        var redirectUris = ParseUrisFromConfig(redirectUrisConfig, defaultRedirectUris);

        // Get post-logout redirect URIs from configuration
        var postLogoutRedirectUrisConfig = configuration[ConfigurationKeys.PostLogoutRedirectUris];
        var postLogoutRedirectUris = ParseUrisFromConfig(postLogoutRedirectUrisConfig, defaultPostLogoutRedirectUris);

        // Validate that we have at least one redirect URI
        if (redirectUris.Count == 0)
        {
            throw new InvalidOperationException(
                $"No redirect URIs configured. Set {ConfigurationKeys.RedirectUris} in appsettings.json or environment variable, " +
                "or ensure SDMS_B2CWebApp_url is set to generate default redirect URI.");
        }

        if (postLogoutRedirectUris.Count == 0)
        {
            throw new InvalidOperationException(
                $"No post-logout redirect URIs configured. Set {ConfigurationKeys.PostLogoutRedirectUris} in appsettings.json or environment variable, " +
                "or ensure SDMS_B2CWebApp_url is set to generate default post-logout redirect URI.");
        }

        // Create or update OpenIddict client
        var clientDescriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = "sdms_frontend",
            ClientType = ClientTypes.Public,
            DisplayName = "SDMS Frontend Application",
            ConsentType = ConsentTypes.Implicit,
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Logout,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.GrantTypes.Password,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + "api",
                Permissions.Prefixes.Scope + "offline_access",
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        // Add redirect URIs
        foreach (var uri in redirectUris)
        {
            clientDescriptor.RedirectUris.Add(uri);
        }

        // Add post-logout redirect URIs
        logger.LogInformation("Adding {Count} post-logout redirect URI(s) to client: {Uris}",
            postLogoutRedirectUris.Count,
            string.Join(", ", postLogoutRedirectUris.Select(u => u.ToString())));

        foreach (var uri in postLogoutRedirectUris)
        {
            clientDescriptor.PostLogoutRedirectUris.Add(uri);
        }

        var existingClient = await applicationManager.FindByClientIdAsync("sdms_frontend");
        if (existingClient == null)
        {
            await applicationManager.CreateAsync(clientDescriptor);
            Console.WriteLine("Created OpenIddict client: sdms_frontend");
        }
        else
        {
            await applicationManager.UpdateAsync(existingClient, clientDescriptor);
            Console.WriteLine("Updated OpenIddict client: sdms_frontend (updated to Public client type with latest permissions and scopes)");
        }

        // Create default roles
        if (!await roleManager.RoleExistsAsync("Administrator"))
        {
            await roleManager.CreateAsync(new IdentityRole("Administrator"));
        }

        // Create default admin user if not exists
        if (!userManager.Users.Any())
        {
            var adminUser = new ApplicationUser
            {
                UserName = "admin@sdms.com",
                Email = "admin@sdms.com",
                EmailConfirmed = true,
                DisplayName = "Administrator"
            };
            await userManager.CreateAsync(adminUser, "Admin@123");
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
    }

    private static HashSet<Uri> ParseUrisFromConfig(string? configValue, HashSet<Uri> defaultUris)
    {
        var uris = new HashSet<Uri>();

        if (!string.IsNullOrWhiteSpace(configValue))
        {
            var uriStrings = configValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var uriString in uriStrings)
            {
                if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
                {
                    uris.Add(uri);

                    // For root URIs, add both versions (with and without trailing slash)
                    if (uri.AbsolutePath == "/")
                    {
                        if (uriString.EndsWith("/"))
                        {
                            var withoutSlash = uriString.TrimEnd('/');
                            if (Uri.TryCreate(withoutSlash, UriKind.Absolute, out var uriWithoutSlash))
                            {
                                uris.Add(uriWithoutSlash);
                            }
                        }
                        else
                        {
                            var withSlash = uriString + "/";
                            if (Uri.TryCreate(withSlash, UriKind.Absolute, out var uriWithSlash))
                            {
                                uris.Add(uriWithSlash);
                            }
                        }
                    }
                }
            }
        }

        if (uris.Count == 0)
        {
            uris = defaultUris;
        }
        else
        {
            foreach (var defaultUri in defaultUris.ToList())
            {
                uris.Add(defaultUri);

                if (defaultUri.AbsolutePath == "/")
                {
                    var defaultUriString = defaultUri.ToString();
                    if (defaultUriString.EndsWith("/"))
                    {
                        var withoutSlash = defaultUriString.TrimEnd('/');
                        if (Uri.TryCreate(withoutSlash, UriKind.Absolute, out var uriWithoutSlash))
                        {
                            uris.Add(uriWithoutSlash);
                        }
                    }
                    else
                    {
                        var withSlash = defaultUriString + "/";
                        if (Uri.TryCreate(withSlash, UriKind.Absolute, out var uriWithSlash))
                        {
                            uris.Add(uriWithSlash);
                        }
                    }
                }
            }
        }

        return uris;
    }
}

