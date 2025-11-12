using System.Collections.Immutable;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SDMS.AuthenticationWebApp.Constants;
using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SDMS.AuthenticationWebApp.Controllers;

// Wrapper class to read OpenIddict request parameters from HTTP request
// Used when the request object isn't available in HttpContext.Items
internal class OpenIddictRequestWrapper
{
    private readonly HttpRequest _request;
    
    public OpenIddictRequestWrapper(HttpRequest request)
    {
        _request = request;
    }
    
    public string? ClientId => _request.Query["client_id"].ToString() ?? _request.Form["client_id"].ToString();
    public string? ResponseType => _request.Query["response_type"].ToString() ?? _request.Form["response_type"].ToString();
    public string? RedirectUri => _request.Query["redirect_uri"].ToString() ?? _request.Form["redirect_uri"].ToString();
    public string? Scope => _request.Query["scope"].ToString() ?? _request.Form["scope"].ToString();
    public string? State => _request.Query["state"].ToString() ?? _request.Form["state"].ToString();
    
    public IEnumerable<string> GetScopes()
    {
        var scope = Scope;
        return string.IsNullOrEmpty(scope) 
            ? Array.Empty<string>() 
            : scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
    
    public bool HasPrompt(string prompt)
    {
        var requestPrompt = _request.Query["prompt"].ToString() ?? _request.Form["prompt"].ToString();
        return requestPrompt == prompt;
    }
}

public class AuthorizationController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;

    public AuthorizationController(
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

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
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
        
        // If still not found, create a wrapper that reads from the HTTP request directly
        // This happens when passthrough is enabled but the request object isn't in Items
        if (requestObj == null)
        {
            var wrapper = new OpenIddictRequestWrapper(Request);
            if (!string.IsNullOrEmpty(wrapper.ClientId))
            {
                requestObj = wrapper;
            }
        }
        
        if (requestObj == null)
        {
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");
        }
        
        // Cast to the OpenIddict request - the actual type is internal, so we use it via the interface
        // The request object implements methods we need, so we'll access them directly
        var request = requestObj;

        // Retrieve the user principal stored in the authentication cookie.
        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

        // If the user can't be extracted, redirect them to the login page.
        if (!result.Succeeded || result.Principal == null)
        {
            // Get configured login URL from configuration
            var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var loginUrl = configuration[ConfigurationKeys.LoginUrl] ?? "/login";
            var returnUrlParameter = configuration[ConfigurationKeys.ReturnUrlParameter] ?? "ReturnUrl";
            
            // Build return URL with all query parameters
            var returnUrl = Request.PathBase + Request.Path + QueryString.Create(
                Request.HasFormContentType ? Request.Form.ToList() : Request.Query.ToList());
            
            // Build the full redirect URL - ensure it's relative to the current request
            var redirectUrl = $"{loginUrl}?{returnUrlParameter}={Uri.EscapeDataString(returnUrl)}";
            
            // Log the redirect for debugging
            var logger = HttpContext.RequestServices.GetService<ILogger<AuthorizationController>>();
            logger?.LogInformation("Redirecting unauthenticated user to login: {RedirectUrl}", redirectUrl);
            
            // Use LocalRedirect to ensure it's a relative URL within the same application
            // This prevents open redirect vulnerabilities and ensures the Angular app handles it
            return LocalRedirect(redirectUrl);
        }

        // Retrieve the profile of the logged-in user.
        var user = await _userManager.GetUserAsync(result.Principal) ??
            throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        string clientId;
        if (request is OpenIddictRequestWrapper wrapper1)
        {
            clientId = wrapper1.ClientId ?? string.Empty;
        }
        else
        {
            var clientIdProp = request.GetType().GetProperty("ClientId", BindingFlags.Public | BindingFlags.Instance);
            clientId = clientIdProp?.GetValue(request)?.ToString() ?? string.Empty;
        }
        var application = await _applicationManager.FindByClientIdAsync(clientId) ??
            throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        IEnumerable<string> scopes;
        if (request is OpenIddictRequestWrapper wrapper2)
        {
            scopes = wrapper2.GetScopes();
        }
        else
        {
            var getScopesMethod = request.GetType().GetMethod("GetScopes", BindingFlags.Public | BindingFlags.Instance);
            scopes = getScopesMethod != null ? (IEnumerable<string>)getScopesMethod.Invoke(request, null)! : Array.Empty<string>();
        }
        var scopesArray = scopes.ToImmutableArray();
        var authorizationsList = new List<object>();
        await foreach (var authorization in _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application, CancellationToken.None) ?? string.Empty,
            status: Statuses.Valid,
            type: AuthorizationTypes.Permanent,
            scopes: scopesArray))
        {
            authorizationsList.Add(authorization);
        }
        var authorizations = authorizationsList;

        switch (await _applicationManager.GetConsentTypeAsync(application, CancellationToken.None))
        {
            // If the consent is external (e.g., when the user is redirected to an external website),
            // immediately return an error if no authorization can be found in the database.
            case ConsentTypes.External when !authorizations.Any():
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged-in user is not allowed to access this client application."
                    }));

            // If the consent is implicit or if an authorization was found,
            // return an authorization response without displaying the consent form.
            case ConsentTypes.Implicit:
            case ConsentTypes.External when authorizations.Any():
            case ConsentTypes.Explicit when authorizations.Any():
                bool hasConsentPrompt;
                if (request is OpenIddictRequestWrapper wrapper3)
                {
                    hasConsentPrompt = wrapper3.HasPrompt(Prompts.Consent);
                }
                else
                {
                    var hasPromptMethod = request.GetType().GetMethod("HasPrompt", BindingFlags.Public | BindingFlags.Instance);
                    hasConsentPrompt = hasPromptMethod != null && (bool)hasPromptMethod.Invoke(request, new object[] { Prompts.Consent })!;
                }
                if (hasConsentPrompt)
                {
                    // Need to show consent form - this case should be handled elsewhere
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "User consent is required."
                        }));
                }
                // Create the claims-based identity that will be used by OpenIddict to generate tokens.
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
                if (request is OpenIddictRequestWrapper wrapper4)
                {
                    scopes2 = wrapper4.GetScopes();
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

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            default:
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string?>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Interactive user consent is required."
                    }));
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
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

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

