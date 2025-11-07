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

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
var connectionString = builder.Configuration[ConfigurationKeys.PostgresConnection] 
    ?? builder.Configuration[ConfigurationKeys.DefaultConnection]
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
var loginUrl = builder.Configuration[ConfigurationKeys.AuthenticationLoginUrl] ?? "/login";
var logoutUrl = builder.Configuration[ConfigurationKeys.AuthenticationLogoutUrl] ?? "/logout";
var errorUrl = builder.Configuration[ConfigurationKeys.AuthenticationErrorUrl] ?? "/login";
var returnUrlParameter = builder.Configuration[ConfigurationKeys.AuthenticationReturnUrlParameter] ?? "ReturnUrl";

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
        options.GetClaimsFromUserInfoEndpoint = true;
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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://localhost:4200",
            builder.Configuration[ConfigurationKeys.FrontendUrl] ?? "http://localhost:4200")
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
app.UseAuthentication();
app.UseAuthorization();

// Map controllers - OpenIddict endpoints are handled automatically by middleware
app.MapControllers();

// Serve static files from Angular build output
var angularDistPath = Path.Combine(builder.Environment.ContentRootPath, "ClientApp", "dist", "sdms-auth-client");
var wwwrootPath = Path.Combine(builder.Environment.ContentRootPath, "wwwroot");

var fileProvider = new CompositeFileProvider(
    new List<IFileProvider>()
    {
        new PhysicalFileProvider(angularDistPath),
        new PhysicalFileProvider(wwwrootPath)
    }
);

// SPA fallback: redirect non-API routes to index.html
app.Use(async (HttpContext context, Func<Task> next) =>
{
    await next.Invoke();
    if (context.Response.StatusCode == (int)HttpStatusCode.NotFound 
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
                Permissions.Endpoints.Userinfo,
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

app.Run("https://localhost:7001");

