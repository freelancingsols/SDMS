# GitHub Secrets Setup Guide

## Quick Reference: Required Secrets

### SDMS.B2CWebApp (Frontend - Vercel)

Go to: **Repository Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Description | Example Value |
|------------|-------------|---------------|
| `API_URL` | Backend API endpoint | `https://your-app.railway.app` |
| `AUTH_SERVER` | Authentication server URL | `https://your-auth.railway.app` |
| `CLIENT_ID` | OAuth client ID | `sdms_frontend` |
| `VERCEL_TOKEN` | Vercel authentication token | Get from Vercel dashboard |
| `VERCEL_ORG_ID` | Vercel organization ID | Get from Vercel project settings |
| `VERCEL_PROJECT_ID` | Vercel project ID | Get from Vercel project settings |

### SDMS.AuthenticationWebApp (Backend - Railway)

Go to: **Repository Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Description | Example Value |
|------------|-------------|---------------|
| `RAILWAY_TOKEN` | Railway authentication token | Get from Railway dashboard |
| `RAILWAY_PROJECT_ID` | Railway project ID | Get from Railway project settings |
| `RAILWAY_SERVICE_ID` | Railway service ID | Get from Railway service settings |

## How to Get Vercel Credentials

1. **VERCEL_TOKEN**:
   - Go to [Vercel Dashboard](https://vercel.com/dashboard)
   - Settings → Tokens
   - Create new token
   - Copy and paste as secret

2. **VERCEL_ORG_ID** and **VERCEL_PROJECT_ID**:
   - Go to your project in Vercel
   - Settings → General
   - Find "Organization ID" and "Project ID"
   - Copy to respective secrets

## How to Get Railway Credentials

1. **RAILWAY_TOKEN**:
   - Go to [Railway Dashboard](https://railway.app)
   - Click your profile → Settings → Tokens
   - Create new token
   - Copy and paste as secret

2. **RAILWAY_PROJECT_ID**:
   - Go to your Railway project
   - Settings → General
   - Find "Project ID"
   - Copy to secret

3. **RAILWAY_SERVICE_ID**:
   - Go to your Railway service
   - Settings → General
   - Find "Service ID"
   - Copy to secret

## Railway Environment Variables (Set in Railway Dashboard)

**Important:** These are set in Railway dashboard, NOT GitHub Secrets.

Go to: **Railway Project → Your Service → Variables**

| Variable Name | Description | Example |
|--------------|-------------|---------|
| `POSTGRES_CONNECTION` | Database connection (auto-generated) | Auto-set by Railway PostgreSQL plugin |
| `Authentication__LoginUrl` | Login page URL | `/login` |
| `Authentication__LogoutUrl` | Logout page URL | `/logout` |
| `Authentication__ErrorUrl` | Error page URL | `/login` |
| `ExternalAuth__Google__ClientId` | Google OAuth Client ID | Your Google client ID |
| `ExternalAuth__Google__ClientSecret` | Google OAuth Secret | Your Google secret |
| `Frontend__Url` | Frontend URL (for CORS) | `https://your-app.vercel.app` |
| `Webhook__Secret` | Webhook secret | Your webhook secret |

**Note:** Use double underscore `__` for nested configuration (e.g., `Authentication__LoginUrl`)

## Testing Secrets

After setting secrets, you can test by:
1. Pushing to main branch
2. Checking GitHub Actions workflow run
3. Verifying deployment succeeds

## Security Best Practices

1. ✅ Never commit secrets to code
2. ✅ Use different secrets for dev/staging/production
3. ✅ Rotate secrets regularly
4. ✅ Use least privilege principle
5. ✅ Enable 2FA on GitHub and deployment platforms

