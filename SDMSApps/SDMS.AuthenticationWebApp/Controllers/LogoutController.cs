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
                    
                    if (!string.IsNullOrEmpty(clientId))
                    {
                        _logger.LogDebug("Extracted clientId from id_token_hint: {ClientId}", clientId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Failed to extract clientId from id_token_hint");
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
                catch (Exception signOutEx)
                {
                    _logger.LogWarning(signOutEx, "Error signing out from Identity, continuing with logout");
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
            
            // Validate the redirect URI
            if (!Uri.TryCreate(postLogoutRedirectUri, UriKind.Absolute, out var redirectUri))
            {
                _logger.LogWarning("Invalid post-logout redirect URI format: {PostLogoutRedirectUri}", postLogoutRedirectUri);
                return BadRequest(new { error = "invalid_request", error_description = "Invalid post-logout redirect URI" });
            }

            // Check if the redirect URI is allowed for this client
            if (!string.IsNullOrEmpty(clientId))
            {
                var client = await _applicationManager.FindByClientIdAsync(clientId);
                if (client != null)
                {
                    var allowedUris = await _applicationManager.GetPostLogoutRedirectUrisAsync(client);
                    
                    // Normalize the requested URI for comparison
                    // Strategy: Always ensure trailing slash, lowercase, trim whitespace
                    var requestedUriString = redirectUri.ToString().Trim();
                    var normalizedRedirectUri = requestedUriString.TrimEnd('/').ToLowerInvariant();
                    if (!normalizedRedirectUri.EndsWith("/"))
                    {
                        normalizedRedirectUri += "/";
                    }
                    
                    var isAllowed = false;
                    string? matchedUriString = null;
                    string? matchStrategy = null;
                    
                    foreach (var allowedUri in allowedUris)
                    {
                        // allowedUri is a Uri object from OpenIddict, convert to string for comparison
                        var allowedUriString = allowedUri.ToString().Trim();
                        var normalizedAllowed = allowedUriString.TrimEnd('/').ToLowerInvariant();
                        if (!normalizedAllowed.EndsWith("/"))
                        {
                            normalizedAllowed += "/";
                        }
                        
                        // Strategy 1: Exact match (case-insensitive, normalized)
                        if (normalizedAllowed.Equals(normalizedRedirectUri, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            matchStrategy = "exact";
                            break;
                        }
                        
                        // Strategy 2: Authority match (scheme + host + port + /)
                        var requestedAuthority = redirectUri.GetLeftPart(UriPartial.Authority).ToLowerInvariant() + "/";
                        if (normalizedAllowed.Equals(requestedAuthority, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            matchStrategy = "authority";
                            break;
                        }
                        
                        // Strategy 3: Prefix match (requested starts with allowed, after removing trailing slash)
                        var allowedPrefix = normalizedAllowed.TrimEnd('/');
                        if (!string.IsNullOrEmpty(allowedPrefix) && 
                            normalizedRedirectUri.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            matchStrategy = "prefix";
                            break;
                        }
                    }
                    
                    // Strategy 4: Allow localhost for development (any port)
                    if (!isAllowed)
                    {
                        var redirectUriHost = redirectUri.Host.ToLowerInvariant();
                        if (redirectUriHost == "localhost" || redirectUriHost == "127.0.0.1")
                        {
                            isAllowed = true;
                            matchedUriString = redirectUri.ToString();
                            matchStrategy = "localhost";
                        }
                    }
                    
                    if (!isAllowed)
                    {
                        // Only log detailed info on failure
                        _logger.LogWarning("Post-logout redirect URI validation failed. Requested: '{RequestedUri}' (normalized: '{NormalizedUri}'), ClientId: {ClientId}, Allowed: [{AllowedUris}]", 
                            postLogoutRedirectUri, normalizedRedirectUri, clientId, string.Join(", ", allowedUris.Select(u => u.ToString())));
                        return BadRequest(new { 
                            error = "invalid_request", 
                            error_description = $"The specified 'post_logout_redirect_uri' is invalid. Allowed URIs: {string.Join(", ", allowedUris.Select(u => u.ToString()))}",
                            error_uri = "https://documentation.openiddict.com/errors/ID2052"
                        });
                    }
                    
                    // Single log for success case
                    _logger.LogInformation("Post-logout redirect URI validated. Requested: '{RequestedUri}', Matched: '{MatchedUri}' ({Strategy})", 
                        postLogoutRedirectUri, matchedUriString, matchStrategy);
                }
                else
                {
                    _logger.LogDebug("Client not found: {ClientId}, continuing with logout", clientId);
                    // Continue with logout even if client not found (for backward compatibility)
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

