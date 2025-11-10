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
using SDMS.AuthenticationWebApp.Configuration;
using SDMS.AuthenticationWebApp.Constants;
using SDMS.AuthenticationWebApp.Data;
using SDMS.AuthenticationWebApp.Models;
using SDMS.AuthenticationWebApp.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Microsoft.Extensions.FileProviders;
using System.Net;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Load configuration from environment variables
// Environment variables take precedence over appsettings.json
// This allows configuration to be set via environment variables in any deployment platform
builder.Configuration.AddEnvironmentVariables();

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

// Configure DataProtection for persistent key storage
// For Railway/container deployments, consider using a volume or database storage
// For now, use the app directory (Railway can mount a volume if persistence is needed)
try
{
    var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
    if (!Directory.Exists(dataProtectionKeysPath))
    {
        Directory.CreateDirectory(dataProtectionKeysPath);
    }
    
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
        .SetApplicationName("SDMS.AuthenticationWebApp");
}
catch (Exception ex)
{
    // If we can't create the directory, DataProtection will use default in-memory storage
    // This is acceptable for single-instance deployments but keys will be lost on restart
    Console.WriteLine($"Warning: Could not configure DataProtection key storage: {ex.Message}");
    Console.WriteLine("DataProtection will use default storage (keys may be lost on restart)");
}

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

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// OpenIddict Configuration
// Note: OpenIddict doesn't have a direct UserInteraction.LoginUrl like IdentityServer4.
// Instead, the login URL is configured via cookie authentication (see below).
// When /connect/authorize is called without authentication, AuthorizationController
// redirects to the login page configured in Authentication:LoginUrl (default: "/login").
builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");
        options.SetAuthorizationEndpointUris("/connect/authorize");
        options.SetUserinfoEndpointUris("/connect/userinfo");
        options.SetLogoutEndpointUris("/connect/logout");
        options.SetIntrospectionEndpointUris("/connect/introspect");

        options.AllowAuthorizationCodeFlow()
            .RequireProofKeyForCodeExchange();

        options.AllowRefreshTokenFlow();
        options.AllowClientCredentialsFlow();
        options.AllowPasswordFlow(); // Allow password grant for testing and API access

        options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles, "api");

        // Signing and encryption - use development certificates for now
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserinfoEndpointPassthrough()
            .EnableLogoutEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// Authentication - configure login interaction similar to IdentityServer4 UserInteraction
// Note: AddIdentity already registers Identity.Application and Identity.External schemes
// We only need to configure authentication defaults and add external authentication providers
var loginUrl = builder.Configuration[ConfigurationKeys.LoginUrl] ?? "/login";
var logoutUrl = builder.Configuration[ConfigurationKeys.LogoutUrl] ?? "/logout";
var errorUrl = builder.Configuration[ConfigurationKeys.ErrorUrl] ?? "/login";
var returnUrlParameter = builder.Configuration[ConfigurationKeys.ReturnUrlParameter] ?? "ReturnUrl";

// Configure authentication defaults
// AddIdentity already registers Identity.Application and Identity.External schemes
// Do NOT add them again here to avoid duplicate scheme registration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddGoogle(options =>
{
    var clientId = builder.Configuration[ConfigurationKeys.ExternalAuthGoogleClientId];
    var clientSecret = builder.Configuration[ConfigurationKeys.ExternalAuthGoogleClientSecret];
    
    if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
    {
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.SignInScheme = IdentityConstants.ExternalScheme;
        options.SaveTokens = true;
        // GetClaimsFromUserInfoEndpoint is automatically enabled in ASP.NET Core 8.0
        // No need to set it explicitly
    }
});

// Authorization - configure to redirect to login for unauthorized requests
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthentication", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// Configure authentication options
// Redirect to login only when user is unauthorized or token is invalid
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = loginUrl;
    options.LogoutPath = logoutUrl;
    options.AccessDeniedPath = errorUrl;
    options.ReturnUrlParameter = returnUrlParameter;
    
    // Only redirect to login when user is unauthorized or token is invalid
    // OnRedirectToLogin is only called when authentication is required but user is not authenticated
    options.Events.OnRedirectToLogin = context =>
    {
        // For API calls, always return 401 instead of redirecting
        if (context.Request.Path.StartsWithSegments("/api") || 
            context.Request.Path.StartsWithSegments("/connect/token") ||
            context.Request.Path.StartsWithSegments("/connect/userinfo"))
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        }
        
        // For browser requests: redirect to login when unauthorized
        // (OnRedirectToLogin is only called when user is unauthorized or token is invalid)
        var returnUrl = context.Request.Path + context.Request.QueryString;
        context.Response.Redirect($"{loginUrl}?{returnUrlParameter}={Uri.EscapeDataString(returnUrl)}");
        return Task.CompletedTask;
    };
});

// Services
builder.Services.AddScoped<IExternalAuthService, ExternalAuthService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddHttpClient();

// CORS
var frontendUrl = builder.Configuration[ConfigurationKeys.FrontendUrl] ?? "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            frontendUrl)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Configure HTTP pipeline
// ForwardedHeaders must be first to handle reverse proxy headers correctly (Railway, etc.)
app.UseForwardedHeaders();

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
app.UseRouting();

// Map health check, ping, and root endpoints BEFORE authentication/authorization
// This allows Railway and other platforms to check if the container is healthy
app.MapHealthChecks("/health").AllowAnonymous();
app.MapGet("/ping", () => Results.Ok(new { 
    status = "ok",
    message = "pong",
    timestamp = DateTime.UtcNow
})).AllowAnonymous();
app.MapGet("/", () => Results.Json(new { 
    status = "ok", 
    service = "SDMS Authentication API", 
    version = "1.0",
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName
})).AllowAnonymous();

app.UseAuthentication();
app.UseAuthorization();

// Map controllers - OpenIddict endpoints are handled automatically by middleware
app.MapControllers();

// Serve static files from Angular build output
var angularDistPath = Path.Combine(builder.Environment.ContentRootPath, "ClientApp", "dist", "sdms-auth-client");
var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");

var fileProviders = new List<IFileProvider>();

// Only add Angular dist file provider if directory exists
if (Directory.Exists(angularDistPath))
{
    fileProviders.Add(new PhysicalFileProvider(angularDistPath));
}
else
{
    // Log warning to console (will be logged properly after app is built)
    Console.WriteLine($"Warning: Angular dist directory not found at {angularDistPath}. Angular app will not be served.");
}

// Only add wwwroot file provider if directory exists
if (Directory.Exists(wwwrootPath))
{
    fileProviders.Add(new PhysicalFileProvider(wwwrootPath));
}

var fileProvider = new CompositeFileProvider(fileProviders);

// SPA fallback: redirect non-API routes to index.html
// Exclude health check, root, and API endpoints from SPA fallback
app.Use(async (HttpContext context, Func<Task> next) =>
{
    await next.Invoke();
    var path = context.Request.Path.Value ?? "";
    if (context.Response.StatusCode == (int)HttpStatusCode.NotFound 
        && !path.StartsWith("/api")
        && !path.StartsWith("/connect")
        && !path.StartsWith("/swagger")
        && path != "/"
        && path != "/health"
        && path != "/ping")
    {
        context.Request.Path = new PathString("/index.html");
        await next.Invoke();
    }
});

app.UseDefaultFiles(new DefaultFilesOptions()
{
    FileProvider = fileProvider,
    DefaultFileNames = new List<string>() { "index.html" }
});

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider
});

// Initialize database and OpenIddict
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var applicationManager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
    
    await context.Database.EnsureCreatedAsync();

    // Create OpenIddict client
    if (await applicationManager.FindByClientIdAsync("sdms_frontend") == null)
    {
        await applicationManager.CreateAsync(new OpenIddictApplicationDescriptor
        {
            ClientId = "sdms_frontend",
            ClientSecret = "sdms_frontend_secret",
            DisplayName = "SDMS Frontend Application",
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Logout,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.GrantTypes.Password, // Allow password grant for API access
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
            },
            RedirectUris =
            {
                new Uri("http://localhost:4200/auth-callback"),
                new Uri("https://localhost:4200/auth-callback"),
            },
            PostLogoutRedirectUris =
            {
                new Uri("http://localhost:4200/"),
                new Uri("https://localhost:4200/"),
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        });
    }

    // Create default roles
    if (!await roleManager.RoleExistsAsync("Administrator"))
    {
        await roleManager.CreateAsync(new IdentityRole("Administrator"));
    }

    // Create default admin user if not exists
    if (!userManager.Users.Any())
    {
        var adminUser = new ApplicationUser
        {
            UserName = "admin@sdms.com",
            Email = "admin@sdms.com",
            EmailConfirmed = true,
            DisplayName = "Administrator"
        };
        await userManager.CreateAsync(adminUser, "Admin@123");
        await userManager.AddToRoleAsync(adminUser, "Administrator");
    }
}

// Start the application
app.Run();

