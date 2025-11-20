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
                        _logger.LogInformation("Extracted clientId from id_token_hint: {ClientId}", clientId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to extract clientId from id_token_hint");
                }
            }

            // If still no clientId, use default
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = "sdms_frontend"; // Default client ID
                _logger.LogInformation("Using default clientId: {ClientId}", clientId);
            }

            _logger.LogInformation("Logout request received. PostLogoutRedirectUri: {PostLogoutRedirectUri}, IdTokenHint: {IdTokenHint}, ClientId: {ClientId}",
                postLogoutRedirectUri ?? "none", idTokenHint != null ? "present" : "absent", clientId ?? "none");

            // If user is authenticated, sign them out
            if (User?.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirst(Claims.Subject)?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? User.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation("Signing out user: {UserId}", userId);
                }

                try
                {
                    // Sign out from Identity (clears authentication cookie)
                    // This will sign out from the Identity.Application scheme
                    await _signInManager.SignOutAsync();
                    _logger.LogInformation("User signed out successfully from Identity");
                }
                catch (Exception signOutEx)
                {
                    _logger.LogWarning(signOutEx, "Error signing out from Identity, continuing with logout");
                    // Continue with logout even if sign out fails
                }
            }
            else
            {
                _logger.LogInformation("User is not authenticated, skipping sign out");
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
                _logger.LogWarning("No post-logout redirect URI specified, using default");
                // Try to get from configuration or use a sensible default
                // For now, return success - the client will handle navigation
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
                _logger.LogWarning("Invalid post-logout redirect URI: {PostLogoutRedirectUri}", postLogoutRedirectUri);
                return BadRequest(new { error = "invalid_request", error_description = "Invalid post-logout redirect URI" });
            }

            // Check if the redirect URI is allowed for this client
            if (!string.IsNullOrEmpty(clientId))
            {
                var client = await _applicationManager.FindByClientIdAsync(clientId);
                if (client != null)
                {
                    var allowedUris = await _applicationManager.GetPostLogoutRedirectUrisAsync(client);
                    
                    // Log for debugging
                    _logger.LogInformation("Validating post-logout redirect URI. Requested: {RequestedUri}, ClientId: {ClientId}, Allowed URIs Count: {Count}",
                        postLogoutRedirectUri, clientId, allowedUris.Count());
                    
                    if (allowedUris.Any())
                    {
                        _logger.LogInformation("Allowed post-logout redirect URIs: {AllowedUris}",
                            string.Join(", ", allowedUris.Select(u => u.ToString())));
                    }
                    
                    // Normalize the requested URI for comparison
                    // Strategy: Always ensure trailing slash, lowercase, trim whitespace
                    var requestedUriString = redirectUri.ToString().Trim();
                    var normalizedRedirectUri = requestedUriString.TrimEnd('/').ToLowerInvariant();
                    if (!normalizedRedirectUri.EndsWith("/"))
                    {
                        normalizedRedirectUri += "/";
                    }
                    
                    _logger.LogInformation("Normalized requested URI: {NormalizedUri}", normalizedRedirectUri);
                    
                    var isAllowed = false;
                    string? matchedUriString = null;
                    
                    foreach (var allowedUri in allowedUris)
                    {
                        // allowedUri is already a Uri object, convert to string for comparison
                        var allowedUriString = allowedUri.ToString().Trim();
                        var normalizedAllowed = allowedUriString.TrimEnd('/').ToLowerInvariant();
                        if (!normalizedAllowed.EndsWith("/"))
                        {
                            normalizedAllowed += "/";
                        }
                        
                        _logger.LogInformation("Comparing: Requested='{Requested}', Allowed='{Allowed}'",
                            normalizedRedirectUri, normalizedAllowed);
                        
                        // Strategy 1: Exact match (case-insensitive, normalized)
                        if (normalizedAllowed.Equals(normalizedRedirectUri, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            _logger.LogInformation("✅ Exact match found: {MatchedUri}", allowedUriString);
                            break;
                        }
                        
                        // Strategy 2: Authority match (scheme + host + port + /)
                        var requestedAuthority = redirectUri.GetLeftPart(UriPartial.Authority).ToLowerInvariant() + "/";
                        if (normalizedAllowed.Equals(requestedAuthority, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            _logger.LogInformation("✅ Authority match found: {MatchedUri}", allowedUriString);
                            break;
                        }
                        
                        // Strategy 3: Prefix match (requested starts with allowed, after removing trailing slash)
                        var allowedPrefix = normalizedAllowed.TrimEnd('/');
                        if (!string.IsNullOrEmpty(allowedPrefix) && 
                            normalizedRedirectUri.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            _logger.LogInformation("✅ Prefix match found: {MatchedUri}", allowedUriString);
                            break;
                        }
                    }
                    
                    if (!isAllowed)
                    {
                        _logger.LogWarning("❌ Post-logout redirect URI not allowed for client. Requested: '{RequestedUri}', Normalized: '{NormalizedUri}', ClientId: {ClientId}, Allowed URIs: {AllowedUris}", 
                            postLogoutRedirectUri, normalizedRedirectUri, clientId, string.Join(", ", allowedUris.Select(u => u.ToString())));
                        return BadRequest(new { 
                            error = "invalid_request", 
                            error_description = $"The specified 'post_logout_redirect_uri' is invalid. Allowed URIs: {string.Join(", ", allowedUris.Select(u => u.ToString()))}",
                            error_uri = "https://documentation.openiddict.com/errors/ID2052"
                        });
                    }
                    
                    _logger.LogInformation("✅ Post-logout redirect URI validated successfully. Matched: {MatchedUri}", matchedUriString ?? "null");
                }
                else
                {
                    _logger.LogWarning("Client not found: {ClientId}", clientId);
                    // Continue with logout even if client not found (for backward compatibility)
                }
            }

            _logger.LogInformation("Redirecting to post-logout URI: {PostLogoutRedirectUri}", postLogoutRedirectUri);
            
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

