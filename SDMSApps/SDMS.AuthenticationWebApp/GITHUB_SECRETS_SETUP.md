# GitHub Secrets Setup for AuthenticationWebApp

## Overview

The AuthenticationWebApp uses GitHub Secrets to store sensitive configuration values that are automatically loaded as environment variables in Railway during deployment.

## Required GitHub Secrets

Add the following secrets in your GitHub repository (Settings → Secrets and variables → Actions):

### Connection String
- `SDMS_AuthenticationWebApp_ConnectionString` - PostgreSQL connection string (preferred)
  - Format: `Host=host;Database=db;Username=user;Password=pass;Port=5432`
  - Note: If not set, the application will fall back to `POSTGRES_CONNECTION` (Railway automatic env var)
  - Priority: `SDMS_AuthenticationWebApp_ConnectionString` > `POSTGRES_CONNECTION` > Default

### Frontend Configuration
- `SDMS_AuthenticationWebApp_FrontendUrl` - Frontend application URL (for CORS)
  - Example: `https://your-frontend-domain.com`

### Authentication URLs
- `SDMS_AuthenticationWebApp_LoginUrl` - Login page URL (default: `/login`)
- `SDMS_AuthenticationWebApp_LogoutUrl` - Logout page URL (default: `/logout`)
- `SDMS_AuthenticationWebApp_ErrorUrl` - Error page URL (default: `/login`)
- `SDMS_AuthenticationWebApp_ReturnUrlParameter` - Return URL parameter name (default: `ReturnUrl`)

### Server Configuration
- `SDMS_AuthenticationWebApp_ServerPort` - Server port (optional, Railway provides `PORT` automatically)
- `SDMS_AuthenticationWebApp_ServerUrls` - Server URLs (optional, semicolon-separated)

### External Authentication (Google)
- `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId` - Google OAuth Client ID
- `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientSecret` - Google OAuth Client Secret

### External Authentication (Auth0) - Optional
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_Domain` - Auth0 domain
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientId` - Auth0 Client ID
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientSecret` - Auth0 Client Secret

### External Authentication (Redirect URI)
- `SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri` - OAuth redirect URI
  - Example: `https://your-frontend-domain.com/auth-callback`

### Webhook
- `SDMS_AuthenticationWebApp_WebhookSecret` - Webhook secret for external user sync (optional)

### Signing Key
- `SDMS_AuthenticationWebApp_SigningKeyPath` - Path to signing key file (default: `signing-key.pem`)

### Railway Configuration
- `RAILWAY_TOKEN` - Railway authentication token
- `RAILWAY_PROJECT_ID` - Railway project ID
- `RAILWAY_SERVICE_ID` - Railway service ID

## How It Works

1. **GitHub Secrets** → Stored securely in GitHub repository
2. **GitHub Actions Workflow** → Reads secrets and sets them as Railway environment variables
3. **Railway** → Environment variables are available to the application
4. **ASP.NET Core** → Reads environment variables via `AddEnvironmentVariables()`
5. **Application** → Uses configuration via `IConfiguration` or `ConfigurationKeys`

## Configuration Priority

1. **Environment Variables** (from Railway, set by GitHub Actions) - Highest Priority
2. **appsettings.json** - Single file with local development values (localhost URLs, local database, etc.)
3. **Hardcoded defaults** - Fallback

**Note**: We use a single `appsettings.json` file with local development values. Production values are set via environment variables at runtime, which override the values in appsettings.json.

## Setting Up GitHub Secrets

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Add each secret with the exact name listed above
5. Set the value for each secret
6. Click **Add secret**

## Railway Environment Variables

The GitHub Actions workflow automatically sets these environment variables in Railway:

```bash
# Connection String
SDMS_AuthenticationWebApp_ConnectionString

# Frontend Configuration
SDMS_AuthenticationWebApp_FrontendUrl

# Authentication URLs
SDMS_AuthenticationWebApp_LoginUrl
SDMS_AuthenticationWebApp_LogoutUrl
SDMS_AuthenticationWebApp_ErrorUrl
SDMS_AuthenticationWebApp_ReturnUrlParameter

# Server Configuration
SDMS_AuthenticationWebApp_ServerPort
SDMS_AuthenticationWebApp_ServerUrls

# External Authentication (Google)
SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId
SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientSecret

# External Authentication (Auth0)
SDMS_AuthenticationWebApp_ExternalAuth_Auth0_Domain
SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientId
SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientSecret

# External Authentication (Redirect URI)
SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri

# Webhook
SDMS_AuthenticationWebApp_WebhookSecret

# Signing Key
SDMS_AuthenticationWebApp_SigningKeyPath
```

## Verification

After deployment, verify that environment variables are set correctly:

1. Check Railway dashboard → Your service → Variables
2. Verify all `SDMS_AuthenticationWebApp_*` variables are present
3. Check application logs for configuration loading
4. Test the application to ensure it's using the correct configuration

## Troubleshooting

### Environment variables not set
- Verify GitHub secrets are set correctly
- Check GitHub Actions workflow logs for errors
- Verify Railway service ID and project ID are correct

### Configuration not loading
- Check Railway environment variables in dashboard
- Verify variable names match exactly (case-sensitive)
- Check application logs for configuration errors

### Missing configuration values
- Verify all required secrets are set in GitHub
- Check that secrets are not empty
- Verify Railway environment variables are set

## Notes

- All configuration keys use the `SDMS_AuthenticationWebApp_` prefix for consistency
- Environment variables take precedence over `appsettings.json` values
- Railway's `POSTGRES_CONNECTION` can be used as an alternative to `SDMS_AuthenticationWebApp_ConnectionString`
- Railway's `PORT` environment variable is automatically used if `SDMS_AuthenticationWebApp_ServerPort` is not set

