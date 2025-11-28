using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using SDMS.AuthenticationWebApp.Configuration;
using SDMS.AuthenticationWebApp.Constants;
using SDMS.AuthenticationWebApp.Data;
using SDMS.AuthenticationWebApp.Middleware;
using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Models.Requests;
using SDMS.AuthenticationWebApp.Repositories;
using SDMS.AuthenticationWebApp.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.FileProviders;
using System.Net;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

// Configure Serilog early, before creating the builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting SDMS Authentication Web App");

    var builder = WebApplication.CreateBuilder(args);

    // Configuration loading order (highest to lowest priority):
    // 1. Environment Variables (loaded here - highest priority)
    // 2. appsettings.json (loaded automatically by CreateBuilder - base/default values with local development values)
    //
    // Note: We use a single appsettings.json file with local development values.
    // Production values are set via environment variables at runtime, which override the default values in appsettings.json.
    builder.Configuration.AddEnvironmentVariables();

    // Configure Serilog with GrafanaLoki sink
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        // Get Loki configuration from environment variables or configuration
        var lokiUrl = Environment.GetEnvironmentVariable("logging_loki_url")
            ?? context.Configuration["logging_loki_url"];

        var lokiUser = Environment.GetEnvironmentVariable("logging_loki_user")
            ?? context.Configuration["logging_loki_user"];

        var lokiToken = Environment.GetEnvironmentVariable("logging_loki_token")
            ?? context.Configuration["logging_loki_token"];

        // Configure Serilog
        // Minimum level is Information for application code
        // Microsoft and System namespaces are set to Warning to reduce noise from framework logs
        // CRITICAL: Error and Fatal levels should ALWAYS be captured regardless of namespace
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            // Ensure application namespace logs are captured at Information level (includes Error, Warning, Fatal)
            .MinimumLevel.Override("SDMS.AuthenticationWebApp", LogEventLevel.Information)
            // CRITICAL: Ensure Error and Fatal logs are ALWAYS captured for all namespaces
            .MinimumLevel.Override("SDMS.AuthenticationWebApp.Controllers", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "sdms.authenticationwebapp");

        // Enable Serilog self-logging only in development to diagnose issues
        if (context.HostingEnvironment.IsDevelopment())
        {
            Serilog.Debugging.SelfLog.Enable(msg =>
            {
                Console.WriteLine($"[Serilog SelfLog] {msg}");
            });
        }

        // Add GrafanaLoki sink if configuration is provided
        if (!string.IsNullOrWhiteSpace(lokiUrl) && !string.IsNullOrWhiteSpace(lokiUser) && !string.IsNullOrWhiteSpace(lokiToken))
        {
            try
            {
                // Normalize Loki URL - remove trailing slashes and ensure proper format
                var normalizedLokiUrl = lokiUrl.TrimEnd('/');

                // Check if this is a Grafana Cloud URL (contains grafana.net)
                var isGrafanaCloud = normalizedLokiUrl.Contains("grafana.net", StringComparison.OrdinalIgnoreCase);

                // IMPORTANT: Serilog.Sinks.Grafana.Loki automatically appends /loki/api/v1/push to the URL
                // So we must provide ONLY the base URL, not the full path
                // If we provide the full URL, the library will append /loki/api/v1/push again, causing 404 errors
                if (isGrafanaCloud)
                {
                    // For Grafana Cloud, extract just the base URL (scheme + host)
                    // Remove any existing path including /loki/api/v1/push
                    var uri = new Uri(normalizedLokiUrl);
                    normalizedLokiUrl = $"{uri.Scheme}://{uri.Host}";
                }
                else
                {
                    // For self-hosted Loki, also extract just the base URL
                    // Remove any existing path
                    var uri = new Uri(normalizedLokiUrl);
                    normalizedLokiUrl = $"{uri.Scheme}://{uri.Host}";
                    if (uri.Port != 80 && uri.Port != 443 && uri.Port != -1)
                    {
                        normalizedLokiUrl += $":{uri.Port}";
                    }
                }

                var envName = context.HostingEnvironment.EnvironmentName?.ToLowerInvariant() ?? "unknown";

                // For Grafana Cloud, the Login should be the instance ID (usually a number)
                // and Password should be the API token
                // Try both formats: Login/Password and also check if we need to use different auth
                var lokiCredentials = new LokiCredentials
                {
                    Login = lokiUser,
                    Password = lokiToken
                };

                // Configure GrafanaLoki sink with additional options for better reliability
                // CRITICAL: Use Information level to capture all application logs including Errors
                configuration.WriteTo.GrafanaLoki(
                    normalizedLokiUrl,
                    credentials: lokiCredentials,
                    labels: new[]
                    {
                        new LokiLabel { Key = "app", Value = "sdms-authenticationwebapp" },
                        new LokiLabel { Key = "environment", Value = envName },
                        new LokiLabel { Key = "service", Value = "authentication" }
                    },
                    restrictedToMinimumLevel: LogEventLevel.Information, // Captures Information, Warning, Error, Fatal
                    queueLimit: 50000, // Increased queue limit to handle bursts
                    batchPostingLimit: 200, // Increased batch size for better throughput
                    period: TimeSpan.FromSeconds(2) // Flush interval - balance between latency and efficiency
                );
                
                // Log successful Loki configuration (this will test if Loki is working)
                Console.WriteLine($"[Loki] GrafanaLoki sink configured successfully. URL: {normalizedLokiUrl}, Environment: {envName}");
            }
            catch (Exception ex)
            {
                // Log error but don't fail - app should still start with console logging
                Console.WriteLine($"[Loki] Error configuring GrafanaLoki sink: {ex.Message}");
                Console.WriteLine($"[Loki] Stack trace: {ex.StackTrace}");
            }
        }
        else
        {
            // Only log missing configuration in development
            if (context.HostingEnvironment.IsDevelopment())
            {
                Console.WriteLine("[Loki] GrafanaLoki sink not configured - missing configuration values (logging_loki_url, logging_loki_user, or logging_loki_token)");
            }
        }

        // Always write to console
        configuration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            restrictedToMinimumLevel: LogEventLevel.Information
        );
    });

    // Configure server URLs from configuration
    // Priority: Environment Variable (PORT) > Configuration (SDMS_AuthenticationWebApp_ServerPort) > Configuration (SDMS_AuthenticationWebApp_ServerUrls) > Default
    var port = Environment.GetEnvironmentVariable("PORT")
        ?? builder.Configuration[ConfigurationKeys.ServerPort];
    var urls = builder.Configuration[ConfigurationKeys.ServerUrls];

    if (!string.IsNullOrEmpty(port))
    {
        // Use PORT from environment variable or configuration
        // Railway and other platforms provide PORT environment variable
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
    }
    else if (!string.IsNullOrEmpty(urls))
    {
        // Use URLs from configuration (supports multiple URLs separated by semicolon)
        builder.WebHost.UseUrls(urls.Split(';', StringSplitOptions.RemoveEmptyEntries));
    }
    // If neither is set, ASP.NET Core will use defaults from launchSettings.json or default ports

    // Add services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SDMS Authentication API", Version = "v1" });
    });

    // Database - Get connection string from deployment configuration FIRST
    // This is needed for both DbContext and health checks
    var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));
    var configLogger = loggerFactory.CreateLogger("DeploymentConfiguration");
    var connectionString = DeploymentConfiguration.GetDatabaseConnectionString(builder.Configuration, configLogger);

    // Add health checks for Railway and other platforms
    // Include database connection check to verify database connectivity
    builder.Services.AddHealthChecks()
        .AddNpgSql(
            connectionString: connectionString,
            name: "database",
            failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
            tags: new[] { "db", "sql", "postgresql" });

    // Configure ForwardedHeaders for reverse proxy support (Railway, etc.)
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
            | ForwardedHeaders.XForwardedProto
            | ForwardedHeaders.XForwardedHost;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // Configure DbContext with the connection string (already retrieved above for health checks)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
        options.UseOpenIddict();
    });

    // Configure DataProtection for persistent key storage
    // Store keys in PostgreSQL database for Railway/container deployments
    // This ensures keys persist across container restarts and deployments
    // NOTE: Must be configured AFTER DbContext registration
    builder.Services.AddDataProtection()
        .PersistKeysToDbContext<ApplicationDbContext>()
        .SetApplicationName("SDMS.AuthenticationWebApp");

    // Configure Identity and Authentication using extension methods
    builder.Services.AddAuthenticationConfiguration(builder.Configuration);

    // Configure OpenIddict using extension method
    builder.Services.AddOpenIddictConfiguration(builder.Configuration);

    // Authorization - configure to redirect to login for unauthorized requests
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("RequireAuthentication", policy =>
        {
            policy.RequireAuthenticatedUser();
        });
    });

    // Memory cache for caching
    builder.Services.AddMemoryCache();

    // Services
    builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<ICacheService, MemoryCacheService>();
    builder.Services.AddScoped<IUserService, UserService>();
    
    // Repositories
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    
    builder.Services.AddHttpClient();

    // FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

    // Configure CORS using extension method
    builder.Services.AddCorsConfiguration(builder.Configuration);

    var app = builder.Build();

    // Initialize database BEFORE any middleware runs
    // This ensures DataProtectionKeys table exists before DataProtection tries to access it
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Ensure database is created (creates all tables including DataProtectionKeys)
        await context.Database.EnsureCreatedAsync();

        // Explicitly ensure DataProtectionKeys table exists with correct schema
        // Entity Framework Core expects: Id (int), FriendlyName (string), Xml (string)
        try
        {
            await context.Database.ExecuteSqlRawAsync(@"
            CREATE TABLE IF NOT EXISTS ""DataProtectionKeys""
            (
                ""Id"" SERIAL PRIMARY KEY,
                ""FriendlyName"" TEXT NULL,
                ""Xml"" TEXT NULL
            );
        ");
        }
        catch (Exception ex)
        {
            // Table might already exist or there might be a permission issue
            // Log but don't fail - EnsureCreatedAsync should have handled it
            Console.WriteLine($"Note: DataProtectionKeys table creation: {ex.Message}");
        }
    }

    // Configure HTTP pipeline
    // Global exception handler must be first to catch all exceptions
    app.UseGlobalExceptionHandler();
    
    // Security headers middleware (early in pipeline)
    app.UseSecurityHeaders(options =>
    {
        options.EnableContentSecurityPolicy = true;
        options.EnableStrictTransportSecurity = !app.Environment.IsDevelopment();
        options.EnableXContentTypeOptions = true;
        options.EnableXFrameOptions = true;
        options.EnableXssProtection = true;
        options.EnableReferrerPolicy = true;
        options.EnablePermissionsPolicy = true;
        options.RemoveServerHeader = true;
    });
    
    // ForwardedHeaders must be early to handle reverse proxy headers correctly (Railway, etc.)
    app.UseForwardedHeaders();
    
    // Correlation ID middleware for distributed tracing
    app.UseCorrelationId();
    
    // Rate limiting middleware
    app.UseRateLimiting(options =>
    {
        options.MaxRequestsPerWindow = 100;
        options.Window = TimeSpan.FromMinutes(1);
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Only use HTTPS redirection in development
    // In production behind a reverse proxy (like Railway), the proxy handles HTTPS termination
    // Railway terminates SSL at the proxy level, so HTTPS redirection is not needed
    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    // In production (Railway), skip HTTPS redirection as the proxy handles SSL/TLS

    app.UseCors();

    // Serve static files from Angular build output BEFORE routing
    // This ensures static files are checked before endpoint routing
    // Angular 17+ builds to a 'browser' subdirectory
    var angularDistPath = Path.Combine(builder.Environment.ContentRootPath, "ClientApp", "dist", "sdms-auth-client", "browser");
    var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");

    var fileProviders = new List<IFileProvider>();

    // Only add Angular dist file provider if directory exists
    if (Directory.Exists(angularDistPath))
    {
        fileProviders.Add(new PhysicalFileProvider(angularDistPath));
        Console.WriteLine($"✓ Angular dist directory found at: {angularDistPath}");
    }
    else
    {
        // Log warning to console (will be logged properly after app is built)
        Console.WriteLine($"✗ Warning: Angular dist directory not found at {angularDistPath}. Angular app will not be served.");
    }

    // Only add wwwroot file provider if directory exists
    if (Directory.Exists(wwwrootPath))
    {
        fileProviders.Add(new PhysicalFileProvider(wwwrootPath));
    }

    if (fileProviders.Count > 0)
    {
        var fileProvider = new CompositeFileProvider(fileProviders);

        // Serve static files from Angular build output
        // This comes BEFORE UseRouting so files are checked first
        app.UseDefaultFiles(new DefaultFilesOptions()
        {
            FileProvider = fileProvider,
            DefaultFileNames = new List<string>() { "index.html" }
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider
        });
    }
    else
    {
        Console.WriteLine("✗ No file providers configured. Static files will not be served.");
    }

    app.UseRouting();
    

    // Map health check and ping endpoints BEFORE authentication/authorization
    // This allows Railway and other platforms to check if the container is healthy
    app.MapHealthChecks("/health").AllowAnonymous();
    app.MapGet("/ping", () => Results.Ok(new
    {
        status = "ok",
        message = "pong",
        timestamp = DateTime.UtcNow
    })).AllowAnonymous();
    
    // Diagnostic endpoint to test logging to Loki
    app.MapGet("/test-logs", (ILogger<Program> logger) =>
    {
        logger.LogError("TEST ERROR: This is a test error log from /test-logs endpoint");
        logger.LogWarning("TEST WARNING: This is a test warning log from /test-logs endpoint");
        logger.LogInformation("TEST INFO: This is a test information log from /test-logs endpoint");
        
        // Also use Serilog directly
        Log.Error("TEST ERROR (Serilog): This is a test error log from /test-logs endpoint using Serilog directly");
        Log.Warning("TEST WARNING (Serilog): This is a test warning log from /test-logs endpoint using Serilog directly");
        Log.Information("TEST INFO (Serilog): This is a test information log from /test-logs endpoint using Serilog directly");
        
        return Results.Ok(new
        {
            message = "Test logs sent. Check Loki for: TEST ERROR, TEST WARNING, TEST INFO",
            timestamp = DateTime.UtcNow
        });
    }).AllowAnonymous();

    // Note: OpenIddict automatically exposes /.well-known/openid-configuration
    // No explicit mapping needed - OpenIddict middleware handles it
    // CORS is already configured above and will apply to all endpoints including well-known

    app.UseAuthentication();
    app.UseAuthorization();

    // Map controllers - OpenIddict endpoints are handled automatically by middleware
    app.MapControllers();

    // SPA fallback: serve index.html for all routes that don't match controllers or other endpoints
    // MapFallbackToFile automatically excludes routes matched by MapControllers, MapHealthChecks, etc.
    // Explicitly exclude API routes to ensure they're not caught by the fallback
    if (fileProviders.Count > 0)
    {
        var fallbackFileProvider = new CompositeFileProvider(fileProviders);
        app.MapFallbackToFile("index.html", new StaticFileOptions
        {
            FileProvider = fallbackFileProvider,
            RequestPath = "" // Only match routes that don't start with /api, /account, /connect, etc.
        }).ExcludeFromDescription(); // Exclude from API documentation
    }

    // Initialize OpenIddict and create default data
    using (var scope = app.Services.CreateScope())
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        await OpenIddictClientInitialization.InitializeAsync(
            scope.ServiceProvider,
            builder.Configuration,
            logger);
    }

    // Start the application
    Log.Information("SDMS Authentication Web App started successfully");
    
    // Test Loki logging on startup - this helps verify Loki is working
    var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
    startupLogger.LogError("TEST: This is a test error log to verify Loki is working. If you see this in Loki, logging is configured correctly.");
    startupLogger.LogWarning("TEST: This is a test warning log to verify Loki is working.");
    startupLogger.LogInformation("TEST: This is a test information log to verify Loki is working.");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SDMS Authentication Web App terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

