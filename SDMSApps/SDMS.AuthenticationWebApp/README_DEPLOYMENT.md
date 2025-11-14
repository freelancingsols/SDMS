# SDMS Authentication WebApp - Deployment Guide

## Deployment Platform: Railway

This .NET 8.0 backend application is configured to deploy on Railway.

## Configuration Setup

### 1. Railway Configuration Secrets

Add the following secrets in your GitHub repository (Settings → Secrets and variables → Actions):

- `RAILWAY_TOKEN` - Railway authentication token
- `RAILWAY_PROJECT_ID` - Railway project ID
- `RAILWAY_SERVICE_ID` - Railway service ID

### 2. Application Configuration Secrets

Add the following secrets in your GitHub repository (Settings → Secrets and variables → Actions):

#### Connection String
- `SDMS_AuthenticationWebApp_ConnectionString` - PostgreSQL connection string (preferred)
  - Format: `Host=host;Database=db;Username=user;Password=pass;Port=5432`
  - Note: If not set, the application will fall back to `POSTGRES_CONNECTION` (Railway automatic env var)

#### Frontend Configuration
- `SDMS_AuthenticationWebApp_FrontendUrl` - Frontend application URL (for CORS)

#### Authentication URLs
- `SDMS_AuthenticationWebApp_LoginUrl` - Login page URL (default: `/login`)
- `SDMS_AuthenticationWebApp_LogoutUrl` - Logout page URL (default: `/logout`)
- `SDMS_AuthenticationWebApp_ErrorUrl` - Error page URL (default: `/login`)
- `SDMS_AuthenticationWebApp_ReturnUrlParameter` - Return URL parameter name (default: `ReturnUrl`)

#### External Auth (Google)
- `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId` - Google OAuth Client ID
- `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientSecret` - Google OAuth Client Secret

#### External Auth (Auth0) - Optional
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_Domain` - Auth0 domain
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientId` - Auth0 Client ID
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientSecret` - Auth0 Client Secret

#### External Auth (Redirect URI)
- `SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri` - OAuth redirect URI

#### Webhook
- `SDMS_AuthenticationWebApp_WebhookSecret` - Webhook secret for external user sync (optional)

#### Signing Key
- `SDMS_AuthenticationWebApp_SigningKeyPath` - Path to signing key file (default: `signing-key.pem`)

**Note:** The GitHub Actions workflow automatically sets these secrets as Railway environment variables during deployment. You don't need to set them manually in Railway dashboard.

### 3. Getting Railway Credentials

1. Install Railway CLI: `npm install -g @railway/cli`
2. Login: `railway login`
3. Get token: `railway whoami` or create token in Railway dashboard
4. Get service ID from your Railway project settings

### 4. Railway Setup Steps

1. Create a new project in Railway
2. Add PostgreSQL database (Railway plugin)
3. Set environment variables in Railway dashboard
4. Connect your GitHub repository
5. Railway will auto-deploy on push to main branch

## Configuration Priority

The application loads configuration in this order:
1. Environment variables (from Railway, set by GitHub Actions from GitHub Variables/Secrets) - Highest Priority
2. `appsettings.json` - Single file with local development values (localhost URLs, local database, etc.)
3. Hardcoded defaults - Fallback

**All configuration keys use the `SDMS_AuthenticationWebApp_` prefix for consistency.**

**Note**: We use a single `appsettings.json` file with local development values. Production values are set via environment variables at runtime, which override the values in appsettings.json.

## Alternative Platforms

### Render (Free Alternative)
- Free tier available
- PostgreSQL included
- GitHub integration
- Environment variables support
- Similar to Railway

### Fly.io (Free Alternative)
- Free tier available
- PostgreSQL support
- GitHub integration
- Global edge deployment

### Azure App Service (Free Tier)
- Free tier available
- PostgreSQL support via Azure Database
- GitHub Actions integration
- Environment variables support

## Configuration Files

- `appsettings.json` - Single configuration file with local development values (localhost URLs, local database, etc.)
- `railway.json` - Railway deployment configuration
- `.github/workflows/deploy-auth-railway.yml` - GitHub Actions workflow

**Note**: 
- Single `appsettings.json` file contains local development values
- Production values are set via environment variables (synced from GitHub Variables/Secrets to Railway) at runtime
- Environment variables override values in appsettings.json

## Database Migration

After deployment, run migrations:

```bash
railway run dotnet ef database update
```

Or set up automatic migrations in Railway startup command.

