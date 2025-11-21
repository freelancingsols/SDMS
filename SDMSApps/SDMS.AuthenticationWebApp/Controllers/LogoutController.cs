using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SDMS.AuthenticationWebApp.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SDMS.AuthenticationWebApp.Controllers;

[ApiController]
public class LogoutController : ControllerBase
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly ILogger<LogoutController> _logger;

    public LogoutController(
        SignInManager<ApplicationUser> signInManager,
        IOpenIddictApplicationManager applicationManager,
        ILogger<LogoutController> logger)
    {
        _signInManager = signInManager;
        _applicationManager = applicationManager;
        _logger = logger;
    }

    [HttpPost("~/connect/logout")]
    [HttpGet("~/connect/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Read logout parameters directly from query/form parameters
            // OpenIddict logout endpoint uses standard OAuth2/OIDC parameters
            string? postLogoutRedirectUri = Request.Query["post_logout_redirect_uri"].ToString();
            if (string.IsNullOrEmpty(postLogoutRedirectUri) && Request.HasFormContentType)
            {
                postLogoutRedirectUri = Request.Form["post_logout_redirect_uri"].ToString();
            }
            
            string? idTokenHint = Request.Query["id_token_hint"].ToString();
            if (string.IsNullOrEmpty(idTokenHint) && Request.HasFormContentType)
            {
                idTokenHint = Request.Form["id_token_hint"].ToString();
            }
            
            string? clientId = Request.Query["client_id"].ToString();
            if (string.IsNullOrEmpty(clientId) && Request.HasFormContentType)
            {
                clientId = Request.Form["client_id"].ToString();
            }

            // If clientId is not provided, try to extract it from id_token_hint
            if (string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(idTokenHint))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken = tokenHandler.ReadJwtToken(idTokenHint);
                    
                    // Try to get clientId from "azp" (authorized party) or "aud" (audience) claim
                    clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == "azp")?.Value
                        ?? jwtToken.Claims.FirstOrDefault(c => c.Type == "aud")?.Value;
                }
                catch
                {
                    // Ignore token parsing errors
                }
            }

            // If still no clientId, use default
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = "sdms_frontend"; // Default client ID
            }

            // If user is authenticated, sign them out
            if (User?.Identity?.IsAuthenticated == true)
            {
                try
                {
                    // Sign out from Identity (clears authentication cookie)
                    await _signInManager.SignOutAsync();
                }
                catch
                {
                    // Continue with logout even if sign out fails
                }
            }

            // If no post-logout redirect URI is specified, try to get it from the client configuration
            if (string.IsNullOrEmpty(postLogoutRedirectUri) && !string.IsNullOrEmpty(clientId))
            {
                var client = await _applicationManager.FindByClientIdAsync(clientId);
                if (client != null)
                {
                    var postLogoutRedirectUris = await _applicationManager.GetPostLogoutRedirectUrisAsync(client);
                    if (postLogoutRedirectUris.Any())
                    {
                        postLogoutRedirectUri = postLogoutRedirectUris.First().ToString();
                    }
                }
            }

            // If still no redirect URI, use a default (landing page)
            if (string.IsNullOrEmpty(postLogoutRedirectUri))
            {
                // Return success - the client will handle navigation
                return Ok(new { 
                    logged_out = true,
                    message = "Logout successful"
                });
            }

            // URL decode the post-logout redirect URI if needed
            try
            {
                postLogoutRedirectUri = Uri.UnescapeDataString(postLogoutRedirectUri);
            }
            catch
            {
                // If decoding fails, use as-is
            }
            
            // Log request post logout URI before normalization
            var requestUriBeforeNormalization = postLogoutRedirectUri;
            
            // Validate the redirect URI
            if (!Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out var redirectUri))
            {
                return BadRequest(new { error = "invalid_request", error_description = "Invalid post-logout redirect URI" });
            }

            // Check if the redirect URI is allowed for this client
            if (!string.IsNullOrEmpty(clientId))
            {
                var client = await _applicationManager.FindByClientIdAsync(clientId);
                if (client != null)
                {
                    var allowedUris = await _applicationManager.GetPostLogoutRedirectUrisAsync(client);
                    
                    // Log DB post logout URIs before normalization
                    var dbUrisBeforeNormalization = string.Join(", ", allowedUris.Select(u => u.ToString()));
                    
                    // Normalize the requested URI for comparison
                    var requestedUriString = redirectUri.ToString().Trim();
                    var normalizedRedirectUri = requestedUriString.TrimEnd('/').ToLowerInvariant();
                    if (!normalizedRedirectUri.EndsWith("/"))
                    {
                        normalizedRedirectUri += "/";
                    }
                    
                    // Log request post logout URI before normalization
                    _logger.LogInformation("Request post logout URI before normalization: {RequestUriBefore}", requestUriBeforeNormalization);
                    
                    // Log request post logout URI after normalization
                    _logger.LogInformation("Request post logout URI after normalization: {RequestUriAfter}", normalizedRedirectUri);
                    
                    // Log DB post logout URIs before normalization
                    _logger.LogInformation("DB post logout URIs before normalization: {DbUrisBefore}", dbUrisBeforeNormalization);
                    
                    var isAllowed = false;
                    var dbUrisAfterNormalization = new List<string>();
                    
                    foreach (var allowedUri in allowedUris)
                    {
                        var allowedUriString = allowedUri.ToString().Trim();
                        var normalizedAllowed = allowedUriString.TrimEnd('/').ToLowerInvariant();
                        if (!normalizedAllowed.EndsWith("/"))
                        {
                            normalizedAllowed += "/";
                        }
                        
                        dbUrisAfterNormalization.Add(normalizedAllowed);
                        
                        // Strategy 1: Exact match
                        if (normalizedAllowed.Equals(normalizedRedirectUri, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            break;
                        }
                        
                        // Strategy 2: Authority match
                        var requestedAuthority = redirectUri.GetLeftPart(UriPartial.Authority).ToLowerInvariant() + "/";
                        if (normalizedAllowed.Equals(requestedAuthority, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            break;
                        }
                        
                        // Strategy 3: Prefix match
                        var allowedPrefix = normalizedAllowed.TrimEnd('/');
                        if (!string.IsNullOrEmpty(allowedPrefix) && 
                            normalizedRedirectUri.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            break;
                        }
                    }
                    
                    // Log DB post logout URIs after normalization
                    _logger.LogInformation("DB post logout URIs after normalization: {DbUrisAfter}", string.Join(", ", dbUrisAfterNormalization));
                    
                    // Strategy 4: Allow localhost for development (any port)
                    if (!isAllowed)
                    {
                        var redirectUriHost = redirectUri.Host.ToLowerInvariant();
                        if (redirectUriHost == "localhost" || redirectUriHost == "127.0.0.1")
                        {
                            isAllowed = true;
                        }
                    }
                    
                    if (!isAllowed)
                    {
                        var allowedUrisList = string.Join(", ", allowedUris.Select(u => u.ToString()));
                        var fullErrorMessage = $"The specified 'post_logout_redirect_uri' is invalid. Allowed URIs: {allowedUrisList}";
                        _logger.LogError("Post-logout redirect URI validation failed. Requested: {RequestUriBefore} (normalized: {RequestUriAfter}), ClientId: {ClientId}, Allowed URIs: {AllowedUris}", 
                            requestUriBeforeNormalization, normalizedRedirectUri, clientId, allowedUrisList);
                        
                        return StatusCode(400, new { 
                            error = "invalid_request", 
                            error_description = fullErrorMessage,
                            error_uri = "https://documentation.openiddict.com/errors/ID2052"
                        });
                    }
                }
            }
            
            // Redirect to the post-logout URI
            return Redirect(postLogoutRedirectUri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "server_error", error_description = "An error occurred during logout" });
        }
    }
}

