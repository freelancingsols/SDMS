using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SDMS.AuthenticationWebApp.Models;
using System.Text.Json;

namespace SDMS.AuthenticationWebApp.Controllers;

[ApiController]
[Route("webhook")]
public class WebhookController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        ILogger<WebhookController> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("external-user")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalUserSync([FromBody] ExternalUserPayload payload)
    {
        try
        {
            // Validate webhook secret if configured
            var webhookSecret = _configuration["Webhook:Secret"];
            if (!string.IsNullOrEmpty(webhookSecret))
            {
                var providedSecret = Request.Headers["X-Webhook-Secret"].FirstOrDefault();
                if (providedSecret != webhookSecret)
                {
                    _logger.LogWarning("Invalid webhook secret");
                    return Unauthorized();
                }
            }

            if (string.IsNullOrEmpty(payload.Email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                // Create new user
                user = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = true,
                    DisplayName = payload.DisplayName,
                    ExternalProvider = payload.Provider,
                    ExternalId = payload.ExternalId,
                    ProfilePictureUrl = payload.ProfilePictureUrl,
                    LastLoginDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    _logger.LogError("Failed to create user from webhook: {Errors}", 
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(new { error = "Failed to create user" });
                }

                _logger.LogInformation("Created user from webhook: {Email}", payload.Email);
            }
            else
            {
                // Update existing user
                user.DisplayName = payload.DisplayName ?? user.DisplayName;
                user.ExternalProvider = payload.Provider ?? user.ExternalProvider;
                user.ExternalId = payload.ExternalId ?? user.ExternalId;
                user.ProfilePictureUrl = payload.ProfilePictureUrl ?? user.ProfilePictureUrl;
                user.LastLoginDate = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Updated user from webhook: {Email}", payload.Email);
            }

            return Ok(new
            {
                userId = user.Id,
                email = user.Email,
                message = "User synchronized successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public class ExternalUserPayload
{
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Provider { get; set; }
    public string? ExternalId { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

