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
                    
                    // Log for debugging - BEFORE normalization
                    _logger.LogInformation("=== POST-LOGOUT REDIRECT URI VALIDATION ===");
                    _logger.LogInformation("Requested URI (BEFORE normalization): '{RequestedUri}'", postLogoutRedirectUri);
                    _logger.LogInformation("Requested URI (from redirectUri object): '{RequestedUriObject}'", redirectUri.ToString());
                    _logger.LogInformation("ClientId: {ClientId}, Allowed URIs Count: {Count}", clientId, allowedUris.Count());
                    
                    if (allowedUris.Any())
                    {
                        _logger.LogInformation("Allowed post-logout redirect URIs from DB (BEFORE normalization):");
                        foreach (var uriItem in allowedUris)
                        {
                            var uriString = uriItem.ToString();
                            // Try to parse as Uri to get detailed info for logging
                            if (Uri.TryCreate(uriString, UriKind.Absolute, out var parsedUri))
                            {
                                var portDisplay = parsedUri.Port == -1 ? "default" : parsedUri.Port.ToString();
                                _logger.LogInformation("  - '{UriString}' (Scheme: {Scheme}, Host: {Host}, Port: {Port}, Path: {Path})", 
                                    uriString, parsedUri.Scheme, parsedUri.Host, portDisplay, parsedUri.AbsolutePath);
                            }
                            else
                            {
                                _logger.LogInformation("  - '{UriString}' (raw string from DB)", uriString);
                            }
                        }
                    }
                    
                    // Normalize the requested URI for comparison
                    // Strategy: Always ensure trailing slash, lowercase, trim whitespace
                    var requestedUriString = redirectUri.ToString().Trim();
                    var normalizedRedirectUri = requestedUriString.TrimEnd('/').ToLowerInvariant();
                    if (!normalizedRedirectUri.EndsWith("/"))
                    {
                        normalizedRedirectUri += "/";
                    }
                    
                    _logger.LogInformation("Requested URI (AFTER normalization): '{NormalizedUri}'", normalizedRedirectUri);
                    _logger.LogInformation("--- Starting comparison loop ---");
                    
                    var isAllowed = false;
                    string? matchedUriString = null;
                    
                    foreach (var allowedUri in allowedUris)
                    {
                        // allowedUri is a Uri object from OpenIddict, convert to string for comparison
                        var allowedUriString = allowedUri.ToString().Trim();
                        var normalizedAllowed = allowedUriString.TrimEnd('/').ToLowerInvariant();
                        if (!normalizedAllowed.EndsWith("/"))
                        {
                            normalizedAllowed += "/";
                        }
                        
                        _logger.LogInformation("Comparing:");
                        _logger.LogInformation("  Allowed URI (BEFORE normalization): '{AllowedUriBefore}'", allowedUriString);
                        _logger.LogInformation("  Allowed URI (AFTER normalization): '{AllowedUriAfter}'", normalizedAllowed);
                        _logger.LogInformation("  Requested URI (AFTER normalization): '{RequestedUriAfter}'", normalizedRedirectUri);
                        _logger.LogInformation("  Exact match check: {ExactMatch}", normalizedAllowed.Equals(normalizedRedirectUri, StringComparison.OrdinalIgnoreCase));
                        
                        // Strategy 1: Exact match (case-insensitive, normalized)
                        var exactMatch = normalizedAllowed.Equals(normalizedRedirectUri, StringComparison.OrdinalIgnoreCase);
                        _logger.LogInformation("  Strategy 1 (Exact match): {ExactMatch}", exactMatch);
                        if (exactMatch)
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            _logger.LogInformation("  ✅ EXACT MATCH FOUND: {MatchedUri}", allowedUriString);
                            break;
                        }
                        
                        // Strategy 2: Authority match (scheme + host + port + /)
                        var requestedAuthority = redirectUri.GetLeftPart(UriPartial.Authority).ToLowerInvariant() + "/";
                        var authorityMatch = normalizedAllowed.Equals(requestedAuthority, StringComparison.OrdinalIgnoreCase);
                        _logger.LogInformation("  Strategy 2 (Authority match):");
                        _logger.LogInformation("    Requested authority: '{RequestedAuthority}'", requestedAuthority);
                        _logger.LogInformation("    Allowed normalized: '{AllowedNormalized}'", normalizedAllowed);
                        _logger.LogInformation("    Match: {AuthorityMatch}", authorityMatch);
                        if (authorityMatch)
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            _logger.LogInformation("  ✅ AUTHORITY MATCH FOUND: {MatchedUri}", allowedUriString);
                            break;
                        }
                        
                        // Strategy 3: Prefix match (requested starts with allowed, after removing trailing slash)
                        var allowedPrefix = normalizedAllowed.TrimEnd('/');
                        var prefixMatch = !string.IsNullOrEmpty(allowedPrefix) && 
                                         normalizedRedirectUri.StartsWith(allowedPrefix, StringComparison.OrdinalIgnoreCase);
                        _logger.LogInformation("  Strategy 3 (Prefix match):");
                        _logger.LogInformation("    Allowed prefix (after trim): '{AllowedPrefix}'", allowedPrefix);
                        _logger.LogInformation("    Requested starts with prefix: {PrefixMatch}", prefixMatch);
                        if (prefixMatch)
                        {
                            isAllowed = true;
                            matchedUriString = allowedUriString;
                            _logger.LogInformation("  ✅ PREFIX MATCH FOUND: {MatchedUri}", allowedUriString);
                            break;
                        }
                        
                        _logger.LogInformation("  ❌ No match for this allowed URI");
                    }
                    
                    // Strategy 4: Allow localhost for development (any port)
                    // This provides flexibility during local development when URIs might not be in config
                    if (!isAllowed)
                    {
                        var redirectUriHost = redirectUri.Host.ToLowerInvariant();
                        if (redirectUriHost == "localhost" || redirectUriHost == "127.0.0.1")
                        {
                            isAllowed = true;
                            matchedUriString = redirectUri.ToString();
                            _logger.LogInformation("✅ Localhost match found: {MatchedUri}", matchedUriString);
                        }
                    }
                    
                    if (!isAllowed)
                    {
                        _logger.LogWarning("=== VALIDATION FAILED ===");
                        _logger.LogWarning("❌ Post-logout redirect URI not allowed for client.");
                        _logger.LogWarning("Requested URI (original): '{RequestedUri}'", postLogoutRedirectUri);
                        _logger.LogWarning("Requested URI (normalized): '{NormalizedUri}'", normalizedRedirectUri);
                        _logger.LogWarning("ClientId: {ClientId}", clientId);
                        _logger.LogWarning("Allowed URIs from DB: {AllowedUris}", string.Join(", ", allowedUris.Select(u => u.ToString())));
                        return BadRequest(new { 
                            error = "invalid_request", 
                            error_description = $"The specified 'post_logout_redirect_uri' is invalid. Allowed URIs: {string.Join(", ", allowedUris.Select(u => u.ToString()))}",
                            error_uri = "https://documentation.openiddict.com/errors/ID2052"
                        });
                    }
                    
                    _logger.LogInformation("=== VALIDATION SUCCESS ===");
                    _logger.LogInformation("✅ Post-logout redirect URI validated successfully.");
                    _logger.LogInformation("Matched URI: {MatchedUri}", matchedUriString ?? "null");
                    _logger.LogInformation("Requested URI (original): '{RequestedUri}'", postLogoutRedirectUri);
                    _logger.LogInformation("Requested URI (normalized): '{NormalizedUri}'", normalizedRedirectUri);
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

