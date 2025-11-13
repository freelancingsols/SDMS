# SDMS AuthenticationWebApp - AppSettings Architecture

## Overview

The AuthenticationWebApp uses **ASP.NET Core's built-in configuration system** with a hierarchical approach that supports multiple configuration sources.

## Configuration Flow

```
1. Environment Variables (Highest Priority)
   ↓
2. appsettings.json (Single file with local development values)
   ↓
3. Hardcoded defaults (Fallback)
```

## Configuration Files

### 1. `appsettings.json` (Single Configuration File)
- Contains local development values (localhost URLs, local database, etc.)
- Committed to source control
- Used as base template for all environments
- **Production values are set via environment variables at runtime**
- Environment variables override values in this file

### Note: Single File Approach
- **We use only `appsettings.json` file**
- Contains local development values (localhost)
- Production values are loaded from environment variables at runtime
- Environment variables have the highest priority and override appsettings.json
- No separate development or production files needed

## How It Works

### 1. Configuration Loading in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// Load configuration from environment variables (for Railway/deployment)
// Environment variables take precedence over appsettings.json
builder.Configuration.AddEnvironmentVariables();
```

**Key Points:**
- `WebApplication.CreateBuilder(args)` automatically loads:
  - `appsettings.json` (always loaded - contains local development values)
- `AddEnvironmentVariables()` adds environment variables with **highest priority**
- Environment variables override values from appsettings.json
- Environment variables use the same key names as in appsettings.json (e.g., `SDMS_AuthenticationWebApp_FrontendUrl`)
- **Single file approach**: Only `appsettings.json` is used, no environment-specific files

### 2. Configuration Access

#### Using `IConfiguration` (Dependency Injection)

```csharp
public class ExternalAuthService
{
    private readonly IConfiguration _configuration;
    
    public ExternalAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public void SomeMethod()
    {
        var frontendUrl = _configuration["Frontend:Url"];
        var googleClientId = _configuration["ExternalAuth:Google:ClientId"];
    }
}
```

#### Using Configuration Keys Constants

```csharp
// Constants/ConfigurationKeys.cs
public static class ConfigurationKeys
{
    public const string FrontendUrl = "Frontend:Url";
    public const string ExternalAuthGoogleClientId = "ExternalAuth:Google:ClientId";
    // ...
}

// Usage in Program.cs
var frontendUrl = builder.Configuration[ConfigurationKeys.FrontendUrl];
var loginUrl = builder.Configuration[ConfigurationKeys.AuthenticationLoginUrl] ?? "/login";
```

### 3. Configuration Priority Example

```csharp
// Priority: Environment Variable > appsettings.{Environment}.json > appsettings.json
var port = Environment.GetEnvironmentVariable("PORT") 
    ?? builder.Configuration[ConfigurationKeys.ServerPort];
var urls = builder.Configuration[ConfigurationKeys.ServerUrls];
```

## Configuration Structure

### `appsettings.json` Structure

All configuration keys use the `SDMS_AuthenticationWebApp_` prefix for consistency:

```json
{
  "SDMS_AuthenticationWebApp_ConnectionString": "Host=localhost;Database=sdms_auth;...",
  "SDMS_AuthenticationWebApp_FrontendUrl": "http://localhost:4200",
  "SDMS_AuthenticationWebApp_LoginUrl": "/login",
  "SDMS_AuthenticationWebApp_LogoutUrl": "/logout",
  "SDMS_AuthenticationWebApp_ErrorUrl": "/login",
  "SDMS_AuthenticationWebApp_ReturnUrlParameter": "ReturnUrl",
  "SDMS_AuthenticationWebApp_ServerPort": "",
  "SDMS_AuthenticationWebApp_ServerUrls": "https://localhost:7001;http://localhost:5000",
  "SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId": "",
  "SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientSecret": "",
  "SDMS_AuthenticationWebApp_ExternalAuth_Auth0_Domain": "",
  "SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientId": "",
  "SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientSecret": "",
  "SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri": "http://localhost:4200/auth-callback",
  "SDMS_AuthenticationWebApp_WebhookSecret": "",
  "SDMS_AuthenticationWebApp_SigningKeyPath": "signing-key.pem"
}
```

## Environment Variables

### How Environment Variables Map to Configuration

Environment variables use the same key names as in `appsettings.json` (with `SDMS_AuthenticationWebApp_` prefix):

| Environment Variable | Maps To | Priority |
|---------------------|---------|----------|
| `SDMS_AuthenticationWebApp_ConnectionString` | `appsettings.json → SDMS_AuthenticationWebApp_ConnectionString` | Highest (preferred) |
| `POSTGRES_CONNECTION` | Railway automatic env var | Fallback (if `SDMS_AuthenticationWebApp_ConnectionString` not set) |
| `SDMS_AuthenticationWebApp_FrontendUrl` | `appsettings.json → SDMS_AuthenticationWebApp_FrontendUrl` | - |
| `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId` | `appsettings.json → SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId` | - |

### Setting Environment Variables

#### Railway (Deployment Platform)
- Set in Railway dashboard → Environment Variables
- Automatically available to the application

#### Local Development
```bash
# Windows PowerShell
$env:SDMS_AuthenticationWebApp_FrontendUrl = "http://localhost:4200"
$env:SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId = "your-client-id"

# Linux/Mac
export SDMS_AuthenticationWebApp_FrontendUrl="http://localhost:4200"
export SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId="your-client-id"
```

## Key Differences from B2CWebApp

| Aspect | AuthenticationWebApp (.NET) | B2CWebApp (Angular) |
|--------|----------------------------|---------------------|
| **Configuration System** | ASP.NET Core built-in `IConfiguration` | Custom `AppSettings` class |
| **Configuration Files** | `appsettings.json`, `appsettings.{Environment}.json` | Single `appsettings.json` |
| **Environment Variables** | Automatic via `AddEnvironmentVariables()` | Manual update at build time |
| **Runtime Loading** | Built into ASP.NET Core | Custom `loadAppSettingsBeforeBootstrap()` |
| **Configuration Access** | Dependency injection (`IConfiguration`) | Static class (`AppSettings`) |
| **Priority** | Env vars > appsettings.{Env}.json > appsettings.json | Env vars > appsettings.json > defaults |
| **Build-Time Updates** | Not needed (runtime loading) | Required (update file before build) |

## Usage Examples

### 1. In Controllers

```csharp
public class AuthorizationController : Controller
{
    private readonly IConfiguration _configuration;
    
    public AuthorizationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public IActionResult SomeAction()
    {
        var frontendUrl = _configuration[ConfigurationKeys.FrontendUrl];
        // ...
    }
}
```

### 2. In Services

```csharp
using SDMS.AuthenticationWebApp.Constants;

public class ExternalAuthService : IExternalAuthService
{
    private readonly IConfiguration _configuration;
    
    public ExternalAuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task AuthenticateWithGoogleAsync()
    {
        var clientId = _configuration[ConfigurationKeys.ExternalAuthGoogleClientId];
        var clientSecret = _configuration[ConfigurationKeys.ExternalAuthGoogleClientSecret];
        // ...
    }
}
```

### 3. In Program.cs (Startup)

```csharp
using SDMS.AuthenticationWebApp.Constants;

// Read configuration
var loginUrl = builder.Configuration[ConfigurationKeys.LoginUrl] ?? "/login";
var frontendUrl = builder.Configuration[ConfigurationKeys.FrontendUrl] ?? "http://localhost:4200";

// Use configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(frontendUrl)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

## Advantages of This Approach

1. **Built-in Support**: ASP.NET Core handles configuration automatically
2. **Multiple Sources**: Supports JSON files, environment variables, command-line args, etc.
3. **Environment-Specific**: Automatic loading based on `ASPNETCORE_ENVIRONMENT`
4. **Type Safety**: Can use strongly-typed configuration classes
5. **Dependency Injection**: `IConfiguration` is automatically available
6. **Runtime Updates**: Configuration can be reloaded at runtime (with `IOptionsSnapshot`)

## Configuration Best Practices

1. **Use Constants**: Define configuration keys in `ConfigurationKeys` class
2. **Provide Defaults**: Always provide fallback values using `??` operator
3. **Environment Variables**: Use environment variables for sensitive data (secrets, connection strings)
4. **Single Configuration File**: Use only `appsettings.json` with default values. Production values come from environment variables.
5. **Don't Commit Secrets**: Never commit sensitive data to source control

## Summary

AuthenticationWebApp uses **ASP.NET Core's standard configuration system**, which:
- Automatically loads configuration files based on environment
- Supports environment variables with highest priority
- Provides `IConfiguration` via dependency injection
- Requires no build-time scripts (configuration loaded at runtime)
- Supports multiple configuration sources (JSON, env vars, command-line, etc.)

This is the **standard .NET approach** and is more powerful and flexible than the Angular approach, but requires a server-side runtime environment.

