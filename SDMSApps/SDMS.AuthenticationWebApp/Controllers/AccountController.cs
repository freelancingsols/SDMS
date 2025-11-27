using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using SDMS.AuthenticationWebApp.Middleware;
using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Models.Common;
using SDMS.AuthenticationWebApp.Models.Requests;
using SDMS.AuthenticationWebApp.Models.Responses;
using SDMS.AuthenticationWebApp.Repositories;
using SDMS.AuthenticationWebApp.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;
using OpenIddictConstants = OpenIddict.Abstractions.OpenIddictConstants;

namespace SDMS.AuthenticationWebApp.Controllers;

[ApiController]
[Route("account")]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IExternalAuthService _externalAuthService;
    private readonly ITokenService _tokenService;
    private readonly IUserService _userService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IExternalAuthService externalAuthService,
        ITokenService tokenService,
        IUserService userService,
        IUserRepository userRepository,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _externalAuthService = externalAuthService;
        _tokenService = tokenService;
        _userService = userService;
        _userRepository = userRepository;
        _logger = logger;
    }

    [HttpPost("login")]
    [ValidateRequest]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            ApplicationUser? user = null;

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
                user = await _userRepository.GetByEmailAsync(request.Email);
                if (user != null)
                {
                    var isValidPassword = await _userManager.CheckPasswordAsync(user, request.Password);
                    if (isValidPassword)
                    {
                        // Sign the user in using Identity
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        user.LastLoginDate = DateTime.UtcNow;
                        await _userManager.UpdateAsync(user);
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

            // Update last login date
            await _userService.UpdateLastLoginDateAsync(user.Id);

            // User is now signed in via SignInManager
            // Return success - the Angular app will handle the OAuth flow redirect
            // The cookie is set, so when initCodeFlow() redirects to /connect/authorize, 
            // the user will be authenticated

            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            var loginResponse = new LoginResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                ExternalProvider = user.ExternalProvider,
                Success = true,
                Message = "Authentication successful. User signed in."
            };

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(loginResponse, "Login successful", correlationId));
        }
        catch (Exception ex)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            _logger.LogError(ex, "Error during login. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during login", correlationId: correlationId));
        }
    }

    [HttpPost("register")]
    [ValidateRequest]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        try
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { error = "Email and password are required" });
            }

            // Use repository to check if user exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "User with this email already exists",
                    correlationId: correlationId));
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
            var registerResponse = new RegisterResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                Message = "Registration successful"
            };

            return Ok(ApiResponse<RegisterResponse>.SuccessResponse(registerResponse, "Registration successful", correlationId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during registration", correlationId: correlationId));
        }
    }

    // Accept both cookie and Bearer token authentication
    // Note: OpenIddict validation scheme name is "OpenIddict.Validation.AspNetCore"
    [Authorize(AuthenticationSchemes = "Identity.Application,OpenIddict.Validation.AspNetCore")]
    [HttpGet("userinfo")]
    public async Task<IActionResult> UserInfo()
    {
        try
        {
            // Try to get user ID from Bearer token first (OpenIddict)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue(OpenIddictConstants.Claims.Subject);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var userInfo = await _userService.GetUserInfoAsync(userId);
            if (userInfo == null)
            {
                var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
                return NotFound(ApiResponse<object>.ErrorResponse("User not found", correlationId: correlationId));
            }

            var correlationId2 = HttpContext.Items["CorrelationId"]?.ToString();
            return Ok(ApiResponse<UserInfoResponse>.SuccessResponse(userInfo, "User information retrieved successfully", correlationId2));
        }
        catch (Exception ex)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
            _logger.LogError(ex, "Error retrieving user info. CorrelationId: {CorrelationId}", correlationId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user information", correlationId: correlationId));
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
                Email = user.Email,
                StoredHash = storedHash,
                ProvidedHash = request.PasswordHash,
                HashMatches = hashMatches,
                PasswordMatches = passwordMatches,
                VerificationResult = verificationResult.ToString()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying password hash");
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }
}


