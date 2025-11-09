using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SDMS.AuthenticationWebApp.Constants;
using SDMS.AuthenticationWebApp.Data;
using SDMS.AuthenticationWebApp.Models;

namespace SDMS.AuthenticationWebApp.Services;

public class ExternalAuthService : IExternalAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ExternalAuthService> _logger;
    private readonly HttpClient _httpClient;

    public ExternalAuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<ExternalAuthService> logger,
        HttpClient httpClient)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<(bool Success, ApplicationUser? User, string? Error)> AuthenticateWithProviderAsync(
        string provider, string? idToken, string? code)
    {
        try
        {
            return provider.ToLower() switch
            {
                "auth0" => await AuthenticateWithAuth0Async(idToken ?? "", code),
                "google" => await AuthenticateWithGoogleAsync(idToken ?? "", code),
                _ => (false, null, $"Unsupported provider: {provider}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with provider {Provider}", provider);
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool Success, ApplicationUser? User, string? Error)> AuthenticateWithAuth0Async(
        string idToken, string? code)
    {
        try
        {
            var auth0Domain = _configuration[ConfigurationKeys.ExternalAuthAuth0Domain];
            var auth0ClientId = _configuration[ConfigurationKeys.ExternalAuthAuth0ClientId];
            
            if (string.IsNullOrEmpty(auth0Domain) || string.IsNullOrEmpty(auth0ClientId))
            {
                _logger.LogWarning("Auth0 configuration is missing");
                return (false, null, "Auth0 is not configured");
            }

            // If code is provided, exchange it for tokens
            if (!string.IsNullOrEmpty(code))
            {
                var tokenResponse = await ExchangeCodeForTokensAsync("auth0", code);
                if (!tokenResponse.Success)
                {
                    return (false, null, tokenResponse.Error ?? "Failed to exchange code");
                }
                idToken = tokenResponse.IdToken ?? idToken;
            }

            if (string.IsNullOrEmpty(idToken))
            {
                return (false, null, "ID token is required");
            }

            // Validate and decode JWT
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{auth0Domain}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                _httpClient);

            var discoveryDocument = await configurationManager.GetConfigurationAsync();
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = auth0Domain,
                ValidateAudience = true,
                ValidAudience = auth0ClientId,
                ValidateLifetime = true,
                IssuerSigningKeys = discoveryDocument.SigningKeys,
                RequireExpirationTime = true,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(idToken, validationParameters, out _);
            var jwt = tokenHandler.ReadJwtToken(idToken);

            // Extract user info
            var email = principal.FindFirst(ClaimTypes.Email)?.Value 
                ?? jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value 
                ?? jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var picture = jwt.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return (false, null, "Email claim not found in token");
            }

            // Find or create user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    DisplayName = name,
                    ExternalProvider = "Auth0",
                    ExternalId = externalId,
                    ProfilePictureUrl = picture,
                    LastLoginDate = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                _logger.LogInformation("Created new user from Auth0: {Email}", email);
            }
            else
            {
                // Update existing user
                user.LastLoginDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(name)) user.DisplayName = name;
                if (!string.IsNullOrEmpty(picture)) user.ProfilePictureUrl = picture;
                if (user.ExternalProvider != "Auth0")
                {
                    user.ExternalProvider = "Auth0";
                    user.ExternalId = externalId;
                }
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Updated user from Auth0: {Email}", email);
            }

            return (true, user, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with Auth0");
            return (false, null, $"Auth0 authentication failed: {ex.Message}");
        }
    }

    public async Task<(bool Success, ApplicationUser? User, string? Error)> AuthenticateWithGoogleAsync(
        string idToken, string? code)
    {
        try
        {
            var googleClientId = _configuration[ConfigurationKeys.ExternalAuthGoogleClientId];
            var googleClientSecret = _configuration[ConfigurationKeys.ExternalAuthGoogleClientSecret];
            
            if (string.IsNullOrEmpty(googleClientId))
            {
                _logger.LogWarning("Google configuration is missing");
                return (false, null, "Google is not configured");
            }

            // If code is provided, exchange it for tokens
            if (!string.IsNullOrEmpty(code))
            {
                var tokenResponse = await ExchangeCodeForTokensAsync("google", code);
                if (!tokenResponse.Success)
                {
                    return (false, null, tokenResponse.Error ?? "Failed to exchange code");
                }
                idToken = tokenResponse.IdToken ?? idToken;
            }

            if (string.IsNullOrEmpty(idToken))
            {
                return (false, null, "ID token is required");
            }

            // Validate Google ID token
            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://accounts.google.com/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                _httpClient);

            var discoveryDocument = await configurationManager.GetConfigurationAsync();
            
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://accounts.google.com", "accounts.google.com" },
                ValidateAudience = true,
                ValidAudience = googleClientId,
                ValidateLifetime = true,
                IssuerSigningKeys = discoveryDocument.SigningKeys,
                RequireExpirationTime = true,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(idToken, validationParameters, out _);
            var jwt = tokenHandler.ReadJwtToken(idToken);

            // Extract user info
            var email = principal.FindFirst(ClaimTypes.Email)?.Value 
                ?? jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var name = principal.FindFirst(ClaimTypes.Name)?.Value 
                ?? jwt.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var externalId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                ?? jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var picture = jwt.Claims.FirstOrDefault(c => c.Type == "picture")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return (false, null, "Email claim not found in token");
            }

            // Find or create user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    DisplayName = name,
                    ExternalProvider = "Google",
                    ExternalId = externalId,
                    ProfilePictureUrl = picture,
                    LastLoginDate = DateTime.UtcNow
                };
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return (false, null, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
                _logger.LogInformation("Created new user from Google: {Email}", email);
            }
            else
            {
                // Update existing user
                user.LastLoginDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(name)) user.DisplayName = name;
                if (!string.IsNullOrEmpty(picture)) user.ProfilePictureUrl = picture;
                if (user.ExternalProvider != "Google")
                {
                    user.ExternalProvider = "Google";
                    user.ExternalId = externalId;
                }
                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Updated user from Google: {Email}", email);
            }

            return (true, user, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authenticating with Google");
            return (false, null, $"Google authentication failed: {ex.Message}");
        }
    }

    private async Task<(bool Success, string? IdToken, string? AccessToken, string? Error)> ExchangeCodeForTokensAsync(
        string provider, string code)
    {
        try
        {
            var redirectUri = _configuration["ExternalAuth:RedirectUri"] ?? "http://localhost:4200/auth-callback";
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
            });

            string tokenEndpoint;
            if (provider == "auth0")
            {
                var domain = _configuration[ConfigurationKeys.ExternalAuthAuth0Domain];
                var clientId = _configuration[ConfigurationKeys.ExternalAuthAuth0ClientId];
                var clientSecret = _configuration[ConfigurationKeys.ExternalAuthAuth0ClientSecret];
                tokenEndpoint = $"{domain}/oauth/token";
                content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", clientId ?? ""),
                    new KeyValuePair<string, string>("client_secret", clientSecret ?? ""),
                });
            }
            else if (provider == "google")
            {
                var clientId = _configuration[ConfigurationKeys.ExternalAuthGoogleClientId];
                var clientSecret = _configuration[ConfigurationKeys.ExternalAuthGoogleClientSecret];
                tokenEndpoint = "https://oauth2.googleapis.com/token";
                content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("client_id", clientId ?? ""),
                    new KeyValuePair<string, string>("client_secret", clientSecret ?? ""),
                });
            }
            else
            {
                return (false, null, null, $"Unsupported provider: {provider}");
            }

            var response = await _httpClient.PostAsync(tokenEndpoint, content);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Token exchange failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return (false, null, null, $"Token exchange failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(responseContent);
            
            var idToken = tokenResponse?.ContainsKey("id_token") == true 
                ? tokenResponse["id_token"]?.ToString() 
                : null;
            var accessToken = tokenResponse?.ContainsKey("access_token") == true 
                ? tokenResponse["access_token"]?.ToString() 
                : null;

            return (true, idToken, accessToken, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for tokens");
            return (false, null, null, ex.Message);
        }
    }
}

