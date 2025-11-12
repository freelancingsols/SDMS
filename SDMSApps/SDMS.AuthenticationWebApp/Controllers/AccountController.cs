using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SDMS.AuthenticationWebApp.Controllers;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IExternalAuthService _externalAuthService;
    private readonly TokenService _tokenService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IExternalAuthService externalAuthService,
        TokenService tokenService,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _externalAuthService = externalAuthService;
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            ApplicationUser? user = null;
            bool externalAuthSuccess = false;

            // Try external authentication first if provider is specified
            if (!string.IsNullOrEmpty(request.Provider) && 
                (request.Provider == "auth0" || request.Provider == "google"))
            {
                try
                {
                    var (success, externalUser, error) = await _externalAuthService
                        .AuthenticateWithProviderAsync(request.Provider, request.IdToken, request.Code);
                    
                    if (success && externalUser != null)
                    {
                        user = externalUser;
                        externalAuthSuccess = true;
                        _logger.LogInformation("External authentication successful for {Provider}: {Email}", 
                            request.Provider, user.Email);
                    }
                    else
                    {
                        _logger.LogWarning("External authentication failed for {Provider}: {Error}", 
                            request.Provider, error);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during external authentication with {Provider}", request.Provider);
                    // Fall through to local authentication
                }
            }

            // Fallback to local authentication if external failed or not attempted
            if (user == null && !string.IsNullOrEmpty(request.Email) && !string.IsNullOrEmpty(request.Password))
            {
                if (externalAuthSuccess == false)
                {
                    _logger.LogInformation("Attempting local authentication fallback for {Email}", request.Email);
                }

                user = await _userManager.FindByEmailAsync(request.Email);
                if (user != null)
                {
                    var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
                    if (isValidPassword)
                    {
                        // Sign the user in using Identity
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        
                        user.LastLoginDate = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
                        _logger.LogInformation("Local authentication successful for {Email}", request.Email);
                    }
                    else
                    {
                        _logger.LogWarning("Invalid password for {Email}", request.Email);
                        return Unauthorized(new { error = "Invalid credentials" });
                    }
                }
                else
                {
                    _logger.LogWarning("User not found: {Email}", request.Email);
                    return Unauthorized(new { error = "Invalid credentials" });
                }
            }

            if (user == null)
            {
                return BadRequest(new { error = "Invalid login request" });
            }

            // User is now signed in via SignInManager
            // Return success - the Angular app will handle the OAuth flow redirect
            // The cookie is set, so when initCodeFlow() redirects to /connect/authorize, 
            // the user will be authenticated
            
            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                displayName = user.DisplayName,
                externalProvider = user.ExternalProvider,
                success = true,
                message = "Authentication successful. User signed in."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Email and password are required" });
            }

            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { error = "User with this email already exists" });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                DisplayName = request.DisplayName,
                EmailConfirmed = false // Require email confirmation in production
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { error = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            _logger.LogInformation("User registered: {Email}", request.Email);

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                displayName = user.DisplayName,
                message = "Registration successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [Authorize(AuthenticationSchemes = "Identity.Application")]
    [HttpGet("userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                displayName = user.DisplayName,
                externalProvider = user.ExternalProvider,
                profilePictureUrl = user.ProfilePictureUrl,
                lastLoginDate = user.LastLoginDate,
                roles = await _userManager.GetRolesAsync(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user info");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpPost("verify-password-hash")]
    public async Task<IActionResult> VerifyPasswordHash([FromBody] VerifyPasswordHashRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync("admin@sdms.com");
            if (user == null)
            {
                return NotFound(new { error = "User admin@sdms.com not found" });
            }

            // Get the stored password hash
            var storedHash = user.PasswordHash;
            
            // Verify the provided hash matches the stored hash
            bool hashMatches = storedHash == request.PasswordHash;
            
            // Also verify if the password "Admin@123" matches the stored hash
            var passwordHasher = _userManager.PasswordHasher;
            var verificationResult = passwordHasher.VerifyHashedPassword(user, storedHash ?? "", "Admin@123");
            bool passwordMatches = verificationResult == PasswordVerificationResult.Success;

            return Ok(new
            {
                email = user.Email,
                storedHash = storedHash,
                providedHash = request.PasswordHash,
                hashMatches = hashMatches,
                passwordMatches = passwordMatches,
                verificationResult = verificationResult.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password hash");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}

public class VerifyPasswordHashRequest
{
    public string PasswordHash { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string? Provider { get; set; }
    public string? IdToken { get; set; }
    public string? Code { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}

