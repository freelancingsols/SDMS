using System.Collections.Immutable;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SDMS.AuthenticationWebApp.Controllers;

[ApiController]
public class TokenController : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public TokenController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        TokenService tokenService)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        // Get the OpenIddict request - try multiple ways to access it
        object? requestObj = null;
        if (HttpContext.Items.TryGetValue("openiddict-server-request", out var item))
        {
            requestObj = item;
        }
        else if (HttpContext.Items.TryGetValue("openiddict_request", out item))
        {
            requestObj = item;
        }
        
        if (requestObj == null)
        {
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        }
        
        // Use reflection to access the OpenIddict request methods
        // The request type is internal, so we access it via reflection
        var request = requestObj;

        var isAuthCodeMethod = request.GetType().GetMethod("IsAuthorizationCodeGrantType", BindingFlags.Public | BindingFlags.Instance);
        var isRefreshTokenMethod = request.GetType().GetMethod("IsRefreshTokenGrantType", BindingFlags.Public | BindingFlags.Instance);
        var isAuthCode = isAuthCodeMethod != null && (bool)isAuthCodeMethod.Invoke(request, null)!;
        var isRefreshToken = isRefreshTokenMethod != null && (bool)isRefreshTokenMethod.Invoke(request, null)!;
        
        if (isAuthCode || isRefreshToken)
        {
            // Retrieve the claims principal stored in the authorization code/refresh token.
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            var user = await _userManager.FindByIdAsync(result.Principal?.GetClaim(Claims.Subject) ?? string.Empty);
            if (user == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                    }));
            }

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    }));
            }

            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Add the claims that will be persisted in the tokens.
            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                    .SetClaims(Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

            // Set the list of scopes granted to the client application.
            var getScopesMethod2 = request.GetType().GetMethod("GetScopes", BindingFlags.Public | BindingFlags.Instance);
            var scopes2 = getScopesMethod2 != null ? (IEnumerable<string>)getScopesMethod2.Invoke(request, null)! : Array.Empty<string>();
            identity.SetScopes(scopes2);
            identity.SetResources(await GetResourcesAsync(scopes2));
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var isPasswordMethod = request.GetType().GetMethod("IsPasswordGrantType", BindingFlags.Public | BindingFlags.Instance);
        var isPassword = isPasswordMethod != null && (bool)isPasswordMethod.Invoke(request, null)!;
        
        if (isPassword)
        {
            // Password grant type - validate username/email and password
            // Try to find user by username first, then by email (since users can login with email)
            var usernameProp = request.GetType().GetProperty("Username", BindingFlags.Public | BindingFlags.Instance);
            var username = usernameProp?.GetValue(request)?.ToString() ?? string.Empty;
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                // Try finding by email if username lookup fails
                user = await _userManager.FindByEmailAsync(username);
            }
            
            if (user == null)
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid."
                    }));
            }

            // Validate the password
            var passwordProp = request.GetType().GetProperty("Password", BindingFlags.Public | BindingFlags.Instance);
            var password = passwordProp?.GetValue(request)?.ToString() ?? string.Empty;
            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid."
                    }));
            }

            // Ensure the user is still allowed to sign in.
            if (!await _signInManager.CanSignInAsync(user))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                    }));
            }

            // Update last login date
            user.LastLoginDate = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // Create a new ClaimsIdentity containing the claims that will be used to create tokens.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Add the claims that will be persisted in the tokens.
            identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user) ?? string.Empty)
                    .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user) ?? string.Empty)
                    .SetClaims(Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

            // Set the list of scopes granted to the client application.
            var getScopesMethod2 = request.GetType().GetMethod("GetScopes", BindingFlags.Public | BindingFlags.Instance);
            var scopes2 = getScopesMethod2 != null ? (IEnumerable<string>)getScopesMethod2.Invoke(request, null)! : Array.Empty<string>();
            identity.SetScopes(scopes2);
            identity.SetResources(await GetResourcesAsync(scopes2));
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var isClientCredentialsMethod = request.GetType().GetMethod("IsClientCredentialsGrantType", BindingFlags.Public | BindingFlags.Instance);
        var isClientCredentials = isClientCredentialsMethod != null && (bool)isClientCredentialsMethod.Invoke(request, null)!;
        
        if (isClientCredentials)
        {
            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            var clientIdProp2 = request.GetType().GetProperty("ClientId", BindingFlags.Public | BindingFlags.Instance);
            var clientId2 = clientIdProp2?.GetValue(request)?.ToString() ?? string.Empty;
            var application = await _applicationManager.FindByClientIdAsync(clientId2) ??
                throw new InvalidOperationException("The application details cannot be found in the database.");

            // Create a new ClaimsIdentity containing the claims that
            // will be used to create an id_token, a token or a code.
            var identity = new ClaimsIdentity(
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            // Add the claims that will be persisted in the tokens.
            identity.SetClaim(Claims.Subject, await _applicationManager.GetClientIdAsync(application, CancellationToken.None) ?? string.Empty);
            identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application, CancellationToken.None) ?? string.Empty);

            // Set the list of scopes granted to the client application.
            var getScopesMethod2 = request.GetType().GetMethod("GetScopes", BindingFlags.Public | BindingFlags.Instance);
            var scopes2 = getScopesMethod2 != null ? (IEnumerable<string>)getScopesMethod2.Invoke(request, null)! : Array.Empty<string>();
            identity.SetScopes(scopes2);
            identity.SetResources(await GetResourcesAsync(scopes2));
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    private async Task<IEnumerable<string>> GetResourcesAsync(IEnumerable<string> scopes)
    {
        var resources = new List<string>();
        foreach (var scope in scopes)
        {
            var scopeEntity = await _scopeManager.FindByNameAsync(scope);
            if (scopeEntity != null)
            {
                resources.AddRange(await _scopeManager.GetResourcesAsync(scopeEntity));
            }
        }
        return resources.Distinct();
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name:
            case Claims.Subject:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            case Claims.Email:
            case Claims.Role:
                yield return Destinations.AccessToken;
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}

