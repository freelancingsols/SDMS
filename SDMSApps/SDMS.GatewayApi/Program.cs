using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using SDMS.GatewayApi.Configuration;
using Serilog;
using Serilog.Events;

// Configure Serilog early, before creating the builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SDMS Gateway API");

    var builder = WebApplication.CreateBuilder(args);

    // Configuration loading order (highest to lowest priority):
    // 1. Environment Variables (loaded here - highest priority)
    // 2. appsettings.json (loaded automatically by CreateBuilder - base/default values)
    builder.Configuration.AddEnvironmentVariables();

    // Configure Serilog with full configuration
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        // Get Loki configuration from environment variables or configuration
        var lokiUrl = Environment.GetEnvironmentVariable("logging_loki_url")
            ?? context.Configuration["logging_loki_url"];

        var lokiUser = Environment.GetEnvironmentVariable("logging_loki_user")
            ?? context.Configuration["logging_loki_user"];

        var lokiToken = Environment.GetEnvironmentVariable("logging_loki_token")
            ?? context.Configuration["logging_loki_token"];

        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("SDMS.GatewayApi", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "sdms.gatewayapi")
            .WriteTo.Console();

        // Add file logging
        configuration.WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7);

        // Add GrafanaLoki sink if configuration is provided
        if (!string.IsNullOrWhiteSpace(lokiUrl) && !string.IsNullOrWhiteSpace(lokiUser) && !string.IsNullOrWhiteSpace(lokiToken))
        {
            try
            {
                var normalizedLokiUrl = lokiUrl.TrimEnd('/');
                var uri = new Uri(normalizedLokiUrl);
                normalizedLokiUrl = $"{uri.Scheme}://{uri.Host}";
                if (uri.Port != 80 && uri.Port != 443 && uri.Port != -1)
                {
                    normalizedLokiUrl += $":{uri.Port}";
                }

                // Note: GrafanaLoki sink requires Serilog.Sinks.Grafana.Loki package
                // Uncomment and install package if needed:
                // configuration.WriteTo.GrafanaLoki(...);
                Log.Information("GrafanaLoki URL configured but sink not enabled (package not installed)");

                Log.Information("GrafanaLoki logging configured");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to configure GrafanaLoki logging, continuing without it");
            }
        }
    });

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "SDMS Gateway API",
            Version = "v1",
            Description = "API Gateway for SDMS microservices architecture"
        });
    });

    // CORS Configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

        options.AddPolicy("Production", policy =>
        {
            var allowedOrigins = GatewayConfiguration.GetCorsAllowedOrigins(builder.Configuration);
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

    // Authentication Configuration - Load from environment variables or appsettings
    var authenticationAuthority = GatewayConfiguration.GetAuthenticationAuthority(builder.Configuration);
    var authenticationAudience = GatewayConfiguration.GetAuthenticationAudience(builder.Configuration);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authenticationAuthority;
            options.Audience = authenticationAudience;
            options.RequireHttpsMetadata = builder.Environment.IsProduction();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    // Ocelot Configuration
    builder.Configuration.AddJsonFile("appsettings.Ocelot.json", optional: false, reloadOnChange: true);
    builder.Services.AddOcelot(builder.Configuration)
        .AddPolly();

    // Rate Limiting (if needed in future)
    // builder.Services.AddRateLimiter(options => { ... });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "SDMS Gateway API v1");
            c.RoutePrefix = string.Empty; // Swagger UI at root
        });
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();

    // CORS
    if (app.Environment.IsDevelopment())
    {
        app.UseCors("AllowAll");
    }
    else
    {
        app.UseCors("Production");
    }

    app.UseAuthentication();
    app.UseAuthorization();

    // Health Checks
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready")
    });
    app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapControllers();

    // Ocelot must be last in the pipeline
    await app.UseOcelot();

    Log.Information("SDMS Gateway API started successfully");
    Log.Information("Authentication Authority: {Authority}", authenticationAuthority);
    Log.Information("Authentication Audience: {Audience}", authenticationAudience);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SDMS Gateway API failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
