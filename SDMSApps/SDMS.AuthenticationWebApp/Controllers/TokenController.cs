using System.Collections.Immutable;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SDMS.AuthenticationWebApp.Controllers;

// Wrapper class to read OpenIddict request parameters from HTTP request for token endpoint
// Used when the request object isn't available in HttpContext.Items
internal class OpenIddictTokenRequestWrapper
{
    private readonly HttpRequest _request;
    
    public OpenIddictTokenRequestWrapper(HttpRequest request)
    {
        _request = request;
    }
    
    public string? ClientId => _request.Form["client_id"].ToString() ?? _request.Query["client_id"].ToString();
    public string? GrantType => _request.Form["grant_type"].ToString() ?? _request.Query["grant_type"].ToString();
    public string? Code => _request.Form["code"].ToString() ?? _request.Query["code"].ToString();
    public string? RedirectUri => _request.Form["redirect_uri"].ToString() ?? _request.Query["redirect_uri"].ToString();
    public string? CodeVerifier => _request.Form["code_verifier"].ToString() ?? _request.Query["code_verifier"].ToString();
    public string? Username => _request.Form["username"].ToString() ?? _request.Query["username"].ToString();
    public string? Password => _request.Form["password"].ToString() ?? _request.Query["password"].ToString();
    public string? Scope => _request.Form["scope"].ToString() ?? _request.Query["scope"].ToString();
    public string? RefreshToken => _request.Form["refresh_token"].ToString() ?? _request.Query["refresh_token"].ToString();
    
    public IEnumerable<string> GetScopes()
    {
        var scope = Scope;
        return string.IsNullOrEmpty(scope) 
            ? Array.Empty<string>() 
            : scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
    
    public bool IsAuthorizationCodeGrantType()
    {
        return GrantType == GrantTypes.AuthorizationCode;
    }
    
    public bool IsRefreshTokenGrantType()
    {
        return GrantType == GrantTypes.RefreshToken;
    }
    
    public bool IsPasswordGrantType()
    {
        return GrantType == GrantTypes.Password;
    }
    
    public bool IsClientCredentialsGrantType()
    {
        return GrantType == GrantTypes.ClientCredentials;
    }
}

[ApiController]
public class TokenController : ControllerBase
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public TokenController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        IOpenIddictScopeManager scopeManager,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    [HttpOptions("~/connect/token")]
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        // Handle CORS preflight requests
        if (Request.Method == "OPTIONS")
        {
            return Ok();
        }
        
        return await ExchangeInternal();
    }
    
    private async Task<IActionResult> ExchangeInternal()
    {
        // Get logger for error logging only
        var logger = HttpContext.RequestServices.GetRequiredService<ILogger<TokenController>>();
        
        // Declare variables outside try block for use in catch
        string? grantType = null;
        string? clientId = null;
        
        try
        {
        
        // Get the OpenIddict request from HttpContext.Items
        // With passthrough enabled, OpenIddict stores the request in HttpContext.Items
        object? requestObj = null;
        
        // Try all possible key names that OpenIddict might use
        var possibleKeys = new[]
        {
            "openiddict-server-request",
            "openiddict_request",
            "openiddict.server.request",
            "openiddict.server.request.feature"
        };
        
        foreach (var key in possibleKeys)
        {
            if (HttpContext.Items.TryGetValue(key, out var item) && item != null)
            {
                requestObj = item;
                break;
            }
        }
        
        // If still not found, try to find any item that has "OpenIddict" in its type name
        if (requestObj == null)
        {
            foreach (var item in HttpContext.Items.Values)
            {
                if (item != null && item.GetType().FullName?.Contains("OpenIddict") == true)
                {
                    requestObj = item;
                    break;
                }
            }
        }
        
        // If still not found, create a wrapper from the HTTP request directly
        if (requestObj == null)
        {
            // Try to read from form/query directly as fallback
            var requestWrapper = new OpenIddictTokenRequestWrapper(Request);
            if (!string.IsNullOrEmpty(requestWrapper.ClientId) || !string.IsNullOrEmpty(requestWrapper.GrantType))
            {
                requestObj = requestWrapper;
            }
        }
        
        if (requestObj == null)
        {
            // Log all form and query parameters for debugging
            var formParams = new Dictionary<string, string>();
            foreach (var key in Request.Form.Keys)
            {
                formParams[key] = Request.Form[key].ToString();
            }
            var queryParams = new Dictionary<string, string>();
            foreach (var key in Request.Query.Keys)
            {
                queryParams[key] = Request.Query[key].ToString();
            }
            
            logger.LogError("The OpenID Connect request cannot be retrieved. Form params: {FormParams}, Query params: {QueryParams}",
                System.Text.Json.JsonSerializer.Serialize(formParams), 
                System.Text.Json.JsonSerializer.Serialize(queryParams));
            
            // Check if OpenIddict set an error in the response
            if (Response.Headers.ContainsKey("WWW-Authenticate"))
            {
                // OpenIddict may have already set an error response
                return StatusCode(400, new
                {
                    error = Errors.InvalidRequest,
                    error_description = "The OpenID Connect request cannot be retrieved. Check server logs for details."
                });
            }
            
            return BadRequest(new
            {
                error = Errors.InvalidRequest,
                error_description = "The OpenID Connect request cannot be retrieved. This may indicate the request format is invalid or OpenIddict middleware did not process it."
            });
        }
        
        // Use reflection to access the OpenIddict request methods
        // The request type is internal, so we access it via reflection
        var request = requestObj;

        // Log request details for debugging
        if (request is OpenIddictTokenRequestWrapper wrapperLog)
        {
            grantType = wrapperLog.GrantType;
            clientId = wrapperLog.ClientId;
        }
        else
        {
            var grantTypeProp = request.GetType().GetProperty("GrantType", BindingFlags.Public | BindingFlags.Instance);
            var clientIdProp = request.GetType().GetProperty("ClientId", BindingFlags.Public | BindingFlags.Instance);
            grantType = grantTypeProp?.GetValue(request)?.ToString();
            clientId = clientIdProp?.GetValue(request)?.ToString();
        }
        // Check grant type - handle both wrapper and real OpenIddict request
        bool isAuthCode, isRefreshToken;
        if (request is OpenIddictTokenRequestWrapper wrapper)
        {
            isAuthCode = wrapper.IsAuthorizationCodeGrantType();
            isRefreshToken = wrapper.IsRefreshTokenGrantType();
        }
        else
        {
            var isAuthCodeMethod = request.GetType().GetMethod("IsAuthorizationCodeGrantType", BindingFlags.Public | BindingFlags.Instance);
            var isRefreshTokenMethod = request.GetType().GetMethod("IsRefreshTokenGrantType", BindingFlags.Public | BindingFlags.Instance);
            isAuthCode = isAuthCodeMethod != null && (bool)isAuthCodeMethod.Invoke(request, null)!;
            isRefreshToken = isRefreshTokenMethod != null && (bool)isRefreshTokenMethod.Invoke(request, null)!;
        }
        
        if (isAuthCode || isRefreshToken)
        {
            // Get code/refresh_token for logging
            string? codeOrToken = null;
            if (request is OpenIddictTokenRequestWrapper wrapperCode)
            {
                codeOrToken = isAuthCode ? wrapperCode.Code : wrapperCode.RefreshToken;
            }
            else
            {
                var codeProp = request.GetType().GetProperty("Code", BindingFlags.Public | BindingFlags.Instance);
                var refreshTokenProp = request.GetType().GetProperty("RefreshToken", BindingFlags.Public | BindingFlags.Instance);
                codeOrToken = isAuthCode 
                    ? codeProp?.GetValue(request)?.ToString() 
                    : refreshTokenProp?.GetValue(request)?.ToString();
            }
            
            // Retrieve the claims principal stored in the authorization code/refresh token.
            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
            if (result?.Principal == null)
            {
                logger.LogWarning("Authentication failed: No principal found for {GrantType}. Code/Token: {CodeToken}", 
                    isAuthCode ? "authorization_code" : "refresh_token",
                    codeOrToken?.Substring(0, Math.Min(20, codeOrToken?.Length ?? 0)) ?? "null");
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The authorization code or refresh token is invalid or has already been used."
                    }));
            }

            // Retrieve the user profile corresponding to the authorization code/refresh token.
            var subject = result.Principal.GetClaim(Claims.Subject);
            
            if (string.IsNullOrEmpty(subject))
            {
                logger.LogWarning("Token exchange failed: No subject claim found in principal");
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token does not contain a valid subject claim."
                    }));
            }
            
            var user = await _userManager.FindByIdAsync(subject);
            if (user == null)
            {
                logger.LogWarning("Token exchange failed: User not found for subject: {Subject}", subject);
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
            IEnumerable<string> scopes2;
            if (request is OpenIddictTokenRequestWrapper wrapperScopes1)
            {
                scopes2 = wrapperScopes1.GetScopes();
            }
            else
            {
                var getScopesMethod2 = request.GetType().GetMethod("GetScopes", BindingFlags.Public | BindingFlags.Instance);
                scopes2 = getScopesMethod2 != null ? (IEnumerable<string>)getScopesMethod2.Invoke(request, null)! : Array.Empty<string>();
            }
            identity.SetScopes(scopes2);
            identity.SetResources(await GetResourcesAsync(scopes2));
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Check if password grant type
        bool isPassword;
        if (request is OpenIddictTokenRequestWrapper wrapper3)
        {
            isPassword = wrapper3.IsPasswordGrantType();
        }
        else
        {
            var isPasswordMethod = request.GetType().GetMethod("IsPasswordGrantType", BindingFlags.Public | BindingFlags.Instance);
            isPassword = isPasswordMethod != null && (bool)isPasswordMethod.Invoke(request, null)!;
        }
        
        if (isPassword)
        {
            // Password grant type - validate username/email and password
            // Try to find user by username first, then by email (since users can login with email)
            string username, password;
            if (request is OpenIddictTokenRequestWrapper wrapper4)
            {
                username = wrapper4.Username ?? string.Empty;
                password = wrapper4.Password ?? string.Empty;
            }
            else
            {
                var usernameProp = request.GetType().GetProperty("Username", BindingFlags.Public | BindingFlags.Instance);
                username = usernameProp?.GetValue(request)?.ToString() ?? string.Empty;
                var passwordProp = request.GetType().GetProperty("Password", BindingFlags.Public | BindingFlags.Instance);
                password = passwordProp?.GetValue(request)?.ToString() ?? string.Empty;
            }
            
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
            IEnumerable<string> scopes3;
            if (request is OpenIddictTokenRequestWrapper wrapperScopes2)
            {
                scopes3 = wrapperScopes2.GetScopes();
            }
            else
            {
                var getScopesMethod2 = request.GetType().GetMethod("GetScopes", BindingFlags.Public | BindingFlags.Instance);
                scopes3 = getScopesMethod2 != null ? (IEnumerable<string>)getScopesMethod2.Invoke(request, null)! : Array.Empty<string>();
            }
            identity.SetScopes(scopes3);
            identity.SetResources(await GetResourcesAsync(scopes3));
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        // Check if client credentials grant type
        bool isClientCredentials;
        if (request is OpenIddictTokenRequestWrapper wrapper5)
        {
            isClientCredentials = wrapper5.IsClientCredentialsGrantType();
        }
        else
        {
            var isClientCredentialsMethod = request.GetType().GetMethod("IsClientCredentialsGrantType", BindingFlags.Public | BindingFlags.Instance);
            isClientCredentials = isClientCredentialsMethod != null && (bool)isClientCredentialsMethod.Invoke(request, null)!;
        }
        
        if (isClientCredentials)
        {
            // Note: the client credentials are automatically validated by OpenIddict:
            // if client_id or client_secret are invalid, this action won't be invoked.

            // Get client ID
            string clientId2;
            if (request is OpenIddictTokenRequestWrapper wrapper6)
            {
                clientId2 = wrapper6.ClientId ?? string.Empty;
            }
            else
            {
                var clientIdProp2 = request.GetType().GetProperty("ClientId", BindingFlags.Public | BindingFlags.Instance);
                clientId2 = clientIdProp2?.GetValue(request)?.ToString() ?? string.Empty;
            }
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
            IEnumerable<string> scopes4;
            if (request is OpenIddictTokenRequestWrapper wrapperScopes3)
            {
                scopes4 = wrapperScopes3.GetScopes();
            }
            else
            {
                var getScopesMethod2 = request.GetType().GetMethod("GetScopes", BindingFlags.Public | BindingFlags.Instance);
                scopes4 = getScopesMethod2 != null ? (IEnumerable<string>)getScopesMethod2.Invoke(request, null)! : Array.Empty<string>();
            }
            identity.SetScopes(scopes4);
            identity.SetResources(await GetResourcesAsync(scopes4));
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        logger.LogWarning("Unsupported grant type: {GrantType}", grantType ?? "unknown");
        return BadRequest(new
        {
            error = Errors.UnsupportedGrantType,
            error_description = $"The specified grant type '{grantType}' is not supported."
        });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during token exchange for GrantType={GrantType}, ClientId={ClientId}", grantType, clientId);
            return StatusCode(500, new
            {
                error = Errors.ServerError,
                error_description = "An unexpected internal server error occurred during token exchange."
            });
        }
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

