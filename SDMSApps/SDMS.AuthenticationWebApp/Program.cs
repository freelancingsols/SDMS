using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.Server.AspNetCore;
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

// Load configuration from environment variables (for Railway/deployment)
// Environment variables take precedence over appsettings.json
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

// Database - Load from environment variable
// Priority: SDMS_AuthenticationWebApp_ConnectionString > POSTGRES_CONNECTION (Railway) > Default
var connectionString = builder.Configuration[ConfigurationKeys.ConnectionString]
    ?? builder.Configuration[ConfigurationKeys.PostgresConnection]
    ?? "Host=localhost;Database=sdms_auth;Username=postgres;Password=postgres";

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
var loginUrl = builder.Configuration[ConfigurationKeys.LoginUrl] ?? "/login";
var logoutUrl = builder.Configuration[ConfigurationKeys.LogoutUrl] ?? "/logout";
var errorUrl = builder.Configuration[ConfigurationKeys.ErrorUrl] ?? "/login";
var returnUrlParameter = builder.Configuration[ConfigurationKeys.ReturnUrlParameter] ?? "ReturnUrl";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = loginUrl;
    options.LogoutPath = logoutUrl;
    options.AccessDeniedPath = errorUrl;
    options.ReturnUrlParameter = returnUrlParameter;
})
.AddCookie(IdentityConstants.ExternalScheme)
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();
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
app.Use(async (HttpContext context, Func<Task> next) =>
{
    await next.Invoke();
    if (context.Response.StatusCode == (int)HttpStatusCode.NotFound 
        && context.Request.Path.Value != null
        && !context.Request.Path.Value.StartsWith("/api")
        && !context.Request.Path.Value.StartsWith("/connect")
        && !context.Request.Path.Value.StartsWith("/swagger"))
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

