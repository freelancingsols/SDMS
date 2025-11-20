# SDMS B2C WebApp - Deployment Guide

## Deployment Platform: Vercel

This Angular frontend application is configured to deploy on Vercel.

## Configuration Setup

### 1. GitHub Secrets

Add the following secrets in your GitHub repository (Settings → Secrets and variables → Actions → Secrets):

**Application Configuration Secrets:**
- `SDMS_B2CWebApp_url` - B2C WebApp URL (e.g., `https://your-app.vercel.app`)
- `SDMS_AuthenticationWebApp_url` - Authentication server URL (e.g., `https://your-auth.railway.app`)
- `SDMS_AuthenticationWebApp_clientid` - OAuth client ID (e.g., `sdms_frontend`)
- `SDMS_AuthenticationWebApp_redirectUri` - OAuth redirect URI (e.g., `https://your-app.vercel.app/auth-callback`)
- `SDMS_AuthenticationWebApp_scope` - OAuth scope (e.g., `openid profile email roles api`)

**Vercel Deployment Secrets:**
- `VERCEL_TOKEN` - Vercel authentication token
- `VERCEL_ORG_ID` - Vercel organization ID
- `VERCEL_PROJECT_ID` - Vercel project ID

### 2. Getting Vercel Credentials

#### Vercel Token (IMPORTANT: Must be a Team Token)

**For Team Deployments:**
1. Go to [Vercel Dashboard](https://vercel.com/dashboard)
2. **Select your team** (not your personal account) from the top dropdown
3. Go to **Settings → Tokens** (Team Settings)
4. Click **"Create Token"**
5. Give it a name (e.g., "GitHub Actions Deployment")
6. Set expiration (recommended: No expiration or long expiration)
7. **IMPORTANT:** Make sure you're creating the token under the **team** (not personal account)
8. Copy the token → Add to GitHub Secret: `VERCEL_TOKEN`

**⚠️ Common Issue:** If you get "not a member of this team" error, you're using a personal token. You MUST use a team token.

#### Vercel Organization ID
1. Go to your Vercel Dashboard
2. **Select your team** from the top dropdown
3. Go to **Settings → General** (Team Settings)
4. Copy **"Team ID"** or **"Organization ID"** → Add to GitHub Secret: `VERCEL_ORG_ID`

#### Vercel Project ID
1. Go to your Vercel Dashboard
2. **Select your team** from the top dropdown
3. Select your project (or create a new one)
4. Go to **Settings → General**
5. Copy **"Project ID"** → Add to GitHub Secret: `VERCEL_PROJECT_ID`

**Note:** If you haven't created a Vercel project yet:
1. Make sure you're in the correct team
2. Click "Add New" → "Project"
3. You can skip importing (we'll deploy via GitHub Actions)
4. Get the Project ID from Settings → General

### 3. How Environment Variables Work

The GitHub Actions workflow automatically:
1. Reads secrets from GitHub Secrets
2. Updates `src/assets/appsettings.json` with the secret values at build time
3. Sets the production flag in `src/environments/environment.ts`
4. Builds the Angular app with the updated configuration
5. Deploys to Vercel

**Note:** The secrets are loaded from GitHub Secrets during the CI/CD pipeline and injected into the appsettings.json file before the Angular build. This ensures sensitive configuration is not committed to the repository.

### 4. Manual Deployment

If you prefer manual deployment:

```bash
cd ClientApp
npm install
npm run build
# Deploy the dist/sdms-b2c-webapp folder to Vercel
```

## Alternative Platforms

### Netlify (Free Alternative)
- Similar to Vercel
- Free tier available
- GitHub integration
- Environment variables support

### Cloudflare Pages (Free Alternative)
- Free tier available
- Fast CDN
- GitHub integration
- Environment variables support

## Configuration Files

- `appsettings.json` - Application configuration
- `vercel.json` - Vercel deployment configuration
- `.github/workflows/deploy-vercel.yml` - GitHub Actions workflow

