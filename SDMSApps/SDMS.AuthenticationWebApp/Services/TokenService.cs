using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenIddict.Abstractions;
using SDMS.AuthenticationWebApp.Models;

namespace SDMS.AuthenticationWebApp.Services;

public class TokenService
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<TokenService> logger)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(ApplicationUser user, string clientId = "sdms_frontend")
    {
        var identity = new ClaimsIdentity(
            authenticationType: "Bearer",
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);

        // Add standard claims
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
        identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? ""));
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email ?? ""));
        
        if (!string.IsNullOrEmpty(user.DisplayName))
        {
            identity.AddClaim(new Claim("display_name", user.DisplayName));
        }

        // Add roles
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, role));
        }

        // Add custom claims
        if (!string.IsNullOrEmpty(user.ExternalProvider))
        {
            identity.AddClaim(new Claim("external_provider", user.ExternalProvider));
        }

        return new ClaimsPrincipal(identity);
    }

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user, string clientId = "sdms_frontend")
    {
        var principal = await CreateClaimsPrincipalAsync(user, clientId);
        
        // Note: OpenIddict will handle token generation through its endpoints
        // This is a helper for programmatic token creation if needed
        return string.Empty; // Actual token generation handled by OpenIddict middleware
    }

    public static RSA? GetOrCreateSigningKey(IConfiguration configuration, ILogger logger)
    {
        var keyPath = configuration["Auth:SigningKeyPath"] ?? "signing-key.pem";
        
        if (File.Exists(keyPath))
        {
            try
            {
                var keyContent = File.ReadAllText(keyPath);
                var rsa = RSA.Create();
                rsa.ImportFromPem(keyContent);
                logger.LogInformation("Loaded signing key from {Path}", keyPath);
                return rsa;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to load signing key from {Path}, generating new key", keyPath);
            }
        }

        // Generate new key
        var newRsa = RSA.Create(2048);
        var privateKeyPem = newRsa.ExportRSAPrivateKeyPem();
        
        try
        {
            File.WriteAllText(keyPath, privateKeyPem);
            logger.LogInformation("Generated new signing key and saved to {Path}", keyPath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to save signing key to {Path}: {Error}", keyPath, ex.Message);
        }

        return newRsa;
    }
}

