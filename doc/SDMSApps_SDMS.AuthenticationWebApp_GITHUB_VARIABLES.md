# GitHub Variables Setup for AuthenticationWebApp

## Overview

The AuthenticationWebApp uses GitHub Variables to store non-sensitive configuration values that are automatically synced to Railway and Vercel during deployment.

## Required GitHub Variables

Add the following variables in your GitHub repository (Settings → Secrets and variables → Actions → Variables):

### Railway Configuration
- `RAILWAY_PROJECT_ID` - Railway project ID
- `RAILWAY_SERVICE_ID` - Railway service ID
- `RAILWAY_ENVIRONMENT_ID` - Railway environment ID (optional)

### Server Configuration
- `SDMS_AuthenticationWebApp_ServerPort` - Server port (optional, Railway provides `PORT` automatically)
- `SDMS_AuthenticationWebApp_ServerUrls` - Server URLs (optional, semicolon-separated)
  - Example: `https://your-auth-domain.com;http://your-auth-domain.com`

### Frontend Configuration
- `SDMS_B2CWebApp_url` - B2C WebApp URL (used for CORS and redirect URIs)
  - Example: `https://your-frontend-domain.com`
- `SDMS_AuthenticationWebApp_url` - Authentication WebApp URL
  - Example: `https://your-auth-domain.railway.app`
- `SDMS_AuthenticationWebApp_clientid` - OpenIddict client ID
  - Example: `sdms_frontend`

### Authentication URLs
- `SDMS_AuthenticationWebApp_LoginUrl` - Login page URL (default: `/login`)
- `SDMS_AuthenticationWebApp_LogoutUrl` - Logout page URL (default: `/logout`)
- `SDMS_AuthenticationWebApp_ErrorUrl` - Error page URL (default: `/login`)
- `SDMS_AuthenticationWebApp_ReturnUrlParameter` - Return URL parameter name (default: `ReturnUrl`)

### External Authentication (Google)
- `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId` - Google OAuth Client ID
  - Example: `your-google-client-id.apps.googleusercontent.com`

### External Authentication (Auth0) - Optional
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_Domain` - Auth0 domain
  - Example: `your-tenant.auth0.com`
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientId` - Auth0 Client ID

### External Authentication (Redirect URI)
- `SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri` - OAuth redirect URI
  - Example: `https://your-frontend-domain.com/auth-callback`

### Redirect URIs
- `SDMS_AuthenticationWebApp_RedirectUris` - Comma-separated list of redirect URIs
  - Example: `https://your-frontend-domain.com/auth-callback,https://your-b2c-domain.vercel.app/auth-callback`
- `SDMS_AuthenticationWebApp_PostLogoutRedirectUris` - Comma-separated list of post-logout redirect URIs
  - Example: `https://your-frontend-domain.com/,https://your-b2c-domain.vercel.app/`

### Signing Key
- `SDMS_AuthenticationWebApp_SigningKeyPath` - Path to signing key file (default: `signing-key.pem`)

### Logging Configuration (Grafana Loki)
- `logging_loki_url` - Grafana Loki endpoint URL
  - Example: `https://your-loki-instance.com/loki/api/v1/push`
- `logging_loki_user` - Grafana Loki username
  - Example: `your-loki-username`

### Vercel Configuration (for B2C WebApp)
- `VERCEL_ORG_ID` - Vercel organization ID
- `VERCEL_PROJECT_ID` - Vercel project ID

## How It Works

1. **GitHub Variables** → Stored in GitHub repository (Settings → Secrets and variables → Actions → Variables)
2. **GitHub Actions Workflow** → Reads variables and syncs them to Railway/Vercel environment variables
3. **Railway/Vercel** → Environment variables are available to the application
4. **ASP.NET Core** → Reads environment variables via `AddEnvironmentVariables()`
5. **Application** → Uses configuration via `IConfiguration`

## Configuration Priority

1. **Environment Variables** (from Railway/Vercel, synced from GitHub Variables) - Highest Priority
2. **appsettings.json** - Single file with local development values (localhost URLs, local database, etc.)
3. **Hardcoded defaults** - Fallback

**Note**: We use a single `appsettings.json` file with local development values. Production values are set via environment variables at runtime, which override the values in appsettings.json.

## Setting Up GitHub Variables

1. Go to your GitHub repository
2. Navigate to **Settings** → **Secrets and variables** → **Actions**
3. Click on the **Variables** tab
4. Click **New repository variable**
5. Add each variable with the exact name listed above
6. Set the value for each variable
7. Click **Add variable**

## Railway Environment Variables

The GitHub Actions workflow automatically syncs these variables to Railway:

```bash
# Railway Configuration
RAILWAY_PROJECT_ID
RAILWAY_SERVICE_ID
RAILWAY_ENVIRONMENT_ID

# Server Configuration
SDMS_AuthenticationWebApp_ServerPort
SDMS_AuthenticationWebApp_ServerUrls

# Frontend Configuration
SDMS_B2CWebApp_url
SDMS_AuthenticationWebApp_url
SDMS_AuthenticationWebApp_clientid

# Authentication URLs
SDMS_AuthenticationWebApp_LoginUrl
SDMS_AuthenticationWebApp_LogoutUrl
SDMS_AuthenticationWebApp_ErrorUrl
SDMS_AuthenticationWebApp_ReturnUrlParameter

# External Authentication
SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId
SDMS_AuthenticationWebApp_ExternalAuth_Auth0_Domain
SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientId
SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri

# Redirect URIs
SDMS_AuthenticationWebApp_RedirectUris
SDMS_AuthenticationWebApp_PostLogoutRedirectUris

# Signing Key
SDMS_AuthenticationWebApp_SigningKeyPath

# Logging Configuration (Grafana Loki)
logging_loki_url
logging_loki_user
logging_loki_token
```

## Vercel Environment Variables

For B2C WebApp deployment, these variables are synced to Vercel:

```bash
# Vercel Configuration
VERCEL_ORG_ID
VERCEL_PROJECT_ID

# Frontend Configuration
SDMS_B2CWebApp_url
SDMS_AuthenticationWebApp_url
SDMS_AuthenticationWebApp_clientid
```

## Verification

After deployment, verify that environment variables are set correctly:

1. Check Railway dashboard → Your service → Variables
2. Verify all `SDMS_AuthenticationWebApp_*` and `logging_loki_*` variables are present
3. Check application logs for configuration loading
4. Test the application to ensure it's using the correct configuration

## Troubleshooting

### Environment variables not set
- Verify GitHub variables are set correctly
- Check GitHub Actions workflow logs for errors
- Verify Railway service ID and project ID are correct

### Configuration not loading
- Check Railway environment variables in dashboard
- Verify variable names match exactly (case-sensitive)
- Check application logs for configuration errors

### Missing configuration values
- Verify all required variables are set in GitHub
- Check that variables are not empty
- Verify Railway environment variables are synced

## Notes

- All configuration keys use consistent naming conventions
- Environment variables take precedence over `appsettings.json` values
- Railway's `PORT` environment variable is automatically used if `SDMS_AuthenticationWebApp_ServerPort` is not set
- Logging configuration is optional - if not set, logs will only go to console

