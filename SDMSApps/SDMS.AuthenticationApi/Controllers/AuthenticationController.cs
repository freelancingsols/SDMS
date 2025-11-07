using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SDMS.AuthenticationApi.Attributes;
using SDMS.AuthenticationApi.Extensions;
using SDMS.AuthenticationApi.Models;
using SDMS.AuthenticationApi.Models.Request;
using SDMS.Common.Infra.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SDMS.AuthenticationApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IEventService _events;
        private readonly IConfiguration _configuration;
        public AuthenticationController(
            UserManager<User> userManager, SignInManager<User> signInManager,
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IAuthenticationSchemeProvider schemeProvider,
            IEventService events,
            IConfiguration configuration
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _clientStore = clientStore;
            _schemeProvider = schemeProvider;
            _events = events;
            _configuration = configuration;

        }
        [SecurityHeaders]
        [AllowAnonymous]
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            return await Task.FromResult(Ok(new { a = 111 }));
        }
        [SecurityHeaders]
        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] Login request)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);
            if (ModelState.IsValid)
            {
                // validate username/password
                var result = await _signInManager.PasswordSignInAsync(request.Username, request.Password, request.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByNameAsync(request.Username);
                    if (user != null)
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName));
                        // only set explicit expiration here if user chooses "remember me". 
                        // otherwise we rely upon expiration configured in cookie middleware.
                        AuthenticationProperties props = null;
                        if (AccountOptions.AllowRememberLogin && request.RememberLogin)
                        {
                            props = new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                            };
                        };

                        // issue authentication cookie with subject ID and username
                        var issuer = new IdentityServerUser(user.Id.ToString())
                        {
                            DisplayName = request.Username
                        };

                        await HttpContext.SignInAsync(issuer, props);
                        // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
                        if (!(_interaction.IsValidReturnUrl(request.ReturnUrl) || Url.IsLocalUrl(request.ReturnUrl)))
                        {
                            return Ok(new BaseResult<object>() { IsError = true });
                        }
                        if (context != null)
                        {
                            //if (context.IsNativeClient())
                            //{
                            //    return Ok(new BaseResult<object>(){});
                            //    // The client is native, so this change in how to
                            //    // return the response is for better UX for the end user.
                            //    //return this.LoadingPage("Redirect", request.ReturnUrl);
                            //}
                            return Ok(new BaseResult<string>() { Result = request.ReturnUrl });
                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                            //return Redirect(request.ReturnUrl);
                        }
                        else
                        {
                            return Ok(new BaseResult<string>() { Result = request.ReturnUrl });
                        }
                        //// request for a local page
                        //if (_interaction.IsValidReturnUrl(request.ReturnUrl) ||Url.IsLocalUrl(request.ReturnUrl))
                        //{
                        //    return Ok(new BaseResult<object>() { });
                        //    //return Redirect(request.ReturnUrl);
                        //}
                        //else if (string.IsNullOrEmpty(request.ReturnUrl))
                        //{
                        //    return Ok(new BaseResult<object>() { });
                        //    //return Redirect("~/");
                        //}
                        //else
                        //{
                        //    return Unauthorized(new BaseResult<object>() {IsError=true,Message= "invalid return URL" });
                        //    // user might have clicked on a malicious link - should be logged
                        //    //throw new Exception("invalid return URL");
                        //}
                    }
                    //return Unauthorized(new BaseResult<object>() { IsError = true, Message = "invalid return URL" });
                }
                else if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, AccountOptions.LockedOutAccount);
                }
                else
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(request.Username, "invalid credentials", clientId: context?.Client.ClientId));
                    ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                }
            }

            return Unauthorized();//invalid model error
        }
        [SecurityHeaders]
        [AllowAnonymous]
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] Register request)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(request.ReturnUrl);
            if (ModelState.IsValid)
            {
                // validate username/password
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    UserName = request.Username,
                    Email = request.Username
                };
                var userCreateresult = await _userManager.CreateAsync(user, request.Password);
                //check user created 
                var signInResult = await _signInManager.PasswordSignInAsync(request.Username, request.Password, request.RememberLogin, lockoutOnFailure: true);
                if (signInResult.Succeeded)
                {
                    var claims = new Claim[]{
                            new Claim(JwtClaimTypes.Name, request.FirstName+" "+request.LastName),
                            new Claim(JwtClaimTypes.GivenName,request.FirstName),
                            new Claim(JwtClaimTypes.FamilyName, request.LastName),
                            //new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        };
                    var claimResult = await _userManager.AddClaimsAsync(user, claims);
                    if (!claimResult.Succeeded)
                    {
                        //log error
                    }
                    user = await _userManager.FindByNameAsync(request.Username);
                    if (user != null) 
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName));

                        // make sure the returnUrl is still valid, and if so redirect back to authorize endpoint or a local page
                        if (!(_interaction.IsValidReturnUrl(request.ReturnUrl) || Url.IsLocalUrl(request.ReturnUrl)))
                        {
                            return Ok(new BaseResult<object>() { IsError = true });
                        }
                        // only set explicit expiration here if user chooses "remember me". 
                        // otherwise we rely upon expiration configured in cookie middleware.
                        AuthenticationProperties props = null;
                        if (AccountOptions.AllowRememberLogin && request.RememberLogin)
                        {
                            props = new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                            };
                        };

                        // issue authentication cookie with subject ID and username
                        var isuser = new IdentityServerUser(user.Id.ToString())
                        {
                            DisplayName = request.Username
                        };

                        await HttpContext.SignInAsync(isuser, props);

                        if (context != null)
                        {
                            //if (context.IsNativeClient())
                            //{
                            //    return Ok(new BaseResult<object>(){});
                            //    // The client is native, so this change in how to
                            //    // return the response is for better UX for the end user.
                            //    //return this.LoadingPage("Redirect", request.ReturnUrl);
                            //}
                            return Ok(new BaseResult<string>() { Result = request.ReturnUrl });
                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                            //return Redirect(request.ReturnUrl);
                        }
                        else
                        {
                            return Ok(new BaseResult<string>() { Result = request.ReturnUrl });
                        }

                        //// request for a local page
                        //if (_interaction.IsValidReturnUrl(request.ReturnUrl) || Url.IsLocalUrl(request.ReturnUrl))
                        //{
                        //    return Ok(new BaseResult<object>() { });
                        //    //return Redirect(request.ReturnUrl);
                        //}
                        //else if (string.IsNullOrEmpty(request.ReturnUrl))
                        //{
                        //    return Ok(new BaseResult<object>() { });
                        //    //return Redirect("~/");
                        //}
                        //else
                        //{
                        //    return Unauthorized(new BaseResult<object>() { IsError = true, Message = "invalid return URL" });
                        //    // user might have clicked on a malicious link - should be logged
                        //    //throw new Exception("invalid return URL");
                        //}
                    }
                    //return Unauthorized(new BaseResult<object>() { IsError = true, Message = "invalid return URL" });
                }
                else
                {
                    await _events.RaiseAsync(new UserLoginFailureEvent(request.Username, "invalid credentials", clientId: context?.Client.ClientId));
                    ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                }
            }

            return Unauthorized();//invalid model error
        }
        /// <summary>
        /// initiate roundtrip to external authentication provider
        /// </summary>
        [HttpPost]
        [SecurityHeaders]
        [AllowAnonymous]
        public IActionResult ChallengeLogin([FromBody] ExternalAuthData externalAuthRequest)
        {
            if (string.IsNullOrEmpty(externalAuthRequest.ReturnUrl)) externalAuthRequest.ReturnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(externalAuthRequest.ReturnUrl) == false && _interaction.IsValidReturnUrl(externalAuthRequest.ReturnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    { "returnUrl", externalAuthRequest.ReturnUrl },
                    { "scheme", externalAuthRequest.Scheme },
                }
            };

            return Challenge(props, externalAuthRequest.Scheme);

        }
        [HttpPost]
        [SecurityHeaders]
        [AllowAnonymous]
        public IActionResult ChallengeRegister([FromBody] ExternalAuthData externalAuthRequest)
        {
            Register request = null;////todo 
            if (string.IsNullOrEmpty(externalAuthRequest.ReturnUrl)) externalAuthRequest.ReturnUrl = "~/";

            // validate returnUrl - either it is a valid OIDC URL or back to a local page
            if (Url.IsLocalUrl(externalAuthRequest.ReturnUrl) == false && _interaction.IsValidReturnUrl(externalAuthRequest.ReturnUrl) == false)
            {
                // user might have clicked on a malicious link - should be logged
                throw new Exception("invalid return URL");
            }

            // start challenge and roundtrip the return URL and scheme 
            var props = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Callback)),
                Items =
                {
                    { "returnUrl", externalAuthRequest.ReturnUrl },
                    { "scheme", externalAuthRequest.Scheme },
                },
                Parameters = { { "ReturnRequestParams", request } }
            };

            return Challenge(props, externalAuthRequest.Scheme);

        }

        /// <summary>
        /// Post processing of external authentication
        /// </summary>
        [HttpGet]
        [Route("Callback")]
        [SecurityHeaders]
        [AllowAnonymous]
        public async Task<IActionResult> Callback()
        {
            // read external identity from the temporary cookie
            var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            if (result?.Succeeded != true)
            {
                throw new Exception("External authentication error");
            }

            //if (_logger.IsEnabled(LogLevel.Debug))
            //{
            //    var externalClaims = result.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
            //    _logger.LogDebug("External claims: {@claims}", externalClaims);
            //}

            // lookup our user and external provider info
            var (user, provider, providerUserId, claims) = await FindUserFromExternalProvider(result);
            if (user == null && result.Properties.Parameters.ContainsKey("ReturnRequestParams"))
            {
                // this might be where you might initiate a custom workflow for user registration
                // in this sample we don't show how that would be done, as our sample implementation
                // simply auto-provisions new external user
                user = await AutoProvisionUser(provider, providerUserId, claims, result.Properties.GetParameter<Register>("ReturnRequestParams"));
            }

            // this allows us to collect any additional claims or properties
            // for the specific protocols used and store them in the local auth cookie.
            // this is typically used to store data needed for signout from those protocols.
            var additionalLocalClaims = new List<Claim>();
            var localSignInProps = new AuthenticationProperties();
            ProcessLoginCallback(result, additionalLocalClaims, localSignInProps);

            // issue authentication cookie for user
            var isuser = new IdentityServerUser(user.Id.ToString())
            {
                DisplayName = user.UserName,
                IdentityProvider = provider,
                AdditionalClaims = additionalLocalClaims
            };

            await HttpContext.SignInAsync(isuser, localSignInProps);

            // delete temporary cookie used during external authentication
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

            // retrieve return URL
            var returnUrl = result.Properties.Items["returnUrl"] ?? "~/";

            // check if external login is in the context of an OIDC request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            await _events.RaiseAsync(new UserLoginSuccessEvent(provider, providerUserId, user.Id.ToString(), user.UserName, true, context?.Client.ClientId));

            if (context != null)
            {
                if (context.IsNativeClient())
                {
                    // The client is native, so this change in how to
                    // return the response is for better UX for the end user.
                    return this.LoadingPage("Redirect", returnUrl);
                }
            }

            return Redirect(returnUrl);
        }

        private async Task<(User user, string provider, string providerUserId, IEnumerable<Claim> claims)> FindUserFromExternalProvider(AuthenticateResult result)
        {
            var externalUser = result.Principal;

            // try to determine the unique id of the external user (issued by the provider)
            // the most common claim type for that are the sub claim and the NameIdentifier
            // depending on the external provider, some other claim type might be used
            var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                              externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                              throw new Exception("Unknown userid");

            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            var claims = externalUser.Claims.ToList();
            claims.Remove(userIdClaim);

            var provider = result.Properties.Items["scheme"];
            var providerUserId = userIdClaim.Value;

            // find external user
            var user = await _userManager.FindByIdAsync(userIdClaim.Value);

            return (user, provider, providerUserId, claims);
        }

        private async Task<User> AutoProvisionUser(string provider, string providerUserId, IEnumerable<Claim> claims, Register request)
        {
            //add user to identity
            var user = new User
            {
                Id = Guid.Parse(providerUserId),
                UserName = request.Username,
            };
            var userCreateresult = _userManager.CreateAsync(user).Result;
            //check user created 
            var claimResult = _userManager.AddClaimsAsync(user, new Claim[]{
                            new Claim(JwtClaimTypes.Name, request.FirstName+" "+request.LastName),
                            new Claim(JwtClaimTypes.GivenName,request.FirstName),
                            new Claim(JwtClaimTypes.FamilyName, request.LastName)
                        }).Result;
            user = await _userManager.FindByNameAsync(request.Username);
            //var signInResult = await _signInManager.PasswordSignInAsync(request.Username, request.Password, request.RememberLogin, lockoutOnFailure: true);
            //if (signInResult.Succeeded)
            //{
            //}
            return user;
        }

        // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
        // this will be different for WS-Fed, SAML2p or other protocols
        private void ProcessLoginCallback(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
        {
            // if the external system sent a session id claim, copy it over
            // so we can use it for single sign-out
            var sid = externalResult.Principal.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
            if (sid != null)
            {
                localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
            }

            // if the external provider issued an id_token, we'll keep it for signout
            var idToken = externalResult.Properties.GetTokenValue("id_token");
            if (idToken != null)
            {
                localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
            }
        }
        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout(string logoutId)
        {
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            //bool showSignoutPrompt = true;

            //if (context?.ShowSignoutPrompt == false)
            //{
            //    // it's safe to automatically sign-out
            //    showSignoutPrompt = false;
            //}

            if (User?.Identity.IsAuthenticated == true)
            {
                // delete local authentication cookie
                await HttpContext.SignOutAsync();
            }

            // no external signout supported for now (see \Quickstart\Account\AccountController.cs TriggerExternalSignout)
            return Ok(new BaseResult<object>() { });
            //return Ok(new
            //{
            //    showSignoutPrompt,
            //    ClientName = string.IsNullOrEmpty(context?.ClientName) ? context?.ClientId : context?.ClientName,
            //    context?.PostLogoutRedirectUri,
            //    context?.SignOutIFrameUrl,
            //    logoutId
            //});
        }
    }
}
