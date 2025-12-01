using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SDMS.GatewayApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var health = await _healthCheckService.CheckHealthAsync();
        
        return health.Status == HealthStatus.Healthy
            ? Ok(new { status = "Healthy", timestamp = DateTime.UtcNow })
            : StatusCode(503, new { status = "Unhealthy", timestamp = DateTime.UtcNow });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready()
    {
        var health = await _healthCheckService.CheckHealthAsync(check => check.Tags.Contains("ready"));
        
        return health.Status == HealthStatus.Healthy
            ? Ok(new { status = "Ready", timestamp = DateTime.UtcNow })
            : StatusCode(503, new { status = "Not Ready", timestamp = DateTime.UtcNow });
    }

    [HttpGet("live")]
    public IActionResult Live()
    {
        return Ok(new { status = "Alive", timestamp = DateTime.UtcNow });
    }
}

