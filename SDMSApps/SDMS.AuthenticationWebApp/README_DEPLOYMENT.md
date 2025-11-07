# SDMS Authentication WebApp - Deployment Guide

## Deployment Platform: Railway

This .NET 8.0 backend application is configured to deploy on Railway.

## Configuration Setup

### 1. GitHub Secrets

Add the following secrets in your GitHub repository (Settings → Secrets and variables → Actions):

- `RAILWAY_TOKEN` - Railway authentication token
- `RAILWAY_SERVICE_ID` - Railway service ID

### 2. Railway Environment Variables

Set the following environment variables in Railway dashboard:

#### Database
- `POSTGRES_CONNECTION` - PostgreSQL connection string
  - Format: `Host=host;Database=db;Username=user;Password=pass;Port=5432`
  - Or use Railway's PostgreSQL plugin which auto-generates this

#### Authentication
- `Authentication:LoginUrl` - Login page URL (default: `/login`)
- `Authentication:LogoutUrl` - Logout page URL (default: `/logout`)
- `Authentication:ErrorUrl` - Error page URL (default: `/login`)

#### External Auth
- `ExternalAuth:Google:ClientId` - Google OAuth Client ID
- `ExternalAuth:Google:ClientSecret` - Google OAuth Client Secret
- `ExternalAuth:Auth0:Domain` - Auth0 domain (if using)
- `ExternalAuth:Auth0:ClientId` - Auth0 Client ID
- `ExternalAuth:Auth0:ClientSecret` - Auth0 Client Secret

#### Frontend
- `Frontend:Url` - Frontend application URL (for CORS)

#### Webhook
- `Webhook:Secret` - Webhook secret for external user sync

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
1. Environment variables (highest priority)
2. `appsettings.Production.json`
3. `appsettings.json` (default/fallback)

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

- `appsettings.json` - Development configuration
- `appsettings.Production.json` - Production configuration template
- `railway.json` - Railway deployment configuration
- `.github/workflows/deploy-railway.yml` - GitHub Actions workflow

## Database Migration

After deployment, run migrations:

```bash
railway run dotnet ef database update
```

Or set up automatic migrations in Railway startup command.

