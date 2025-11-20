using System.Security.Claims;
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
            if (string.IsNullOrEmpty(postLogoutRedirectUri))
            {
                postLogoutRedirectUri = Request.Form["post_logout_redirect_uri"].ToString();
            }
            
            string? idTokenHint = Request.Query["id_token_hint"].ToString();
            if (string.IsNullOrEmpty(idTokenHint))
            {
                idTokenHint = Request.Form["id_token_hint"].ToString();
            }
            
            string? clientId = Request.Query["client_id"].ToString();
            if (string.IsNullOrEmpty(clientId))
            {
                clientId = Request.Form["client_id"].ToString();
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
                    var isAllowed = allowedUris.Any(uri => 
                        uri.ToString().Equals(redirectUri.ToString(), StringComparison.OrdinalIgnoreCase) ||
                        uri.ToString().Equals(redirectUri.GetLeftPart(UriPartial.Authority) + "/", StringComparison.OrdinalIgnoreCase));
                    
                    if (!isAllowed)
                    {
                        _logger.LogWarning("Post-logout redirect URI not allowed for client: {PostLogoutRedirectUri}", postLogoutRedirectUri);
                        return BadRequest(new { error = "invalid_request", error_description = "Post-logout redirect URI not allowed" });
                    }
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

