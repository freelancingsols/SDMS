# SDMS B2C WebApp - Deployment Guide

## Deployment Platform: Vercel

This Angular frontend application is configured to deploy on Vercel.

## Configuration Setup

### 1. GitHub Secrets

Add the following secrets in your GitHub repository (Settings → Secrets and variables → Actions):

- `API_URL` - Backend API URL (e.g., `https://your-api.railway.app`)
- `AUTH_SERVER` - Authentication server URL (e.g., `https://your-auth.railway.app`)
- `CLIENT_ID` - OAuth client ID (e.g., `sdms_frontend`)
- `VERCEL_TOKEN` - Vercel authentication token
- `VERCEL_ORG_ID` - Vercel organization ID
- `VERCEL_PROJECT_ID` - Vercel project ID

### 2. Getting Vercel Credentials

1. Go to [Vercel Dashboard](https://vercel.com/dashboard)
2. Go to Settings → Tokens to create a new token
3. Copy the token to `VERCEL_TOKEN` secret
4. Go to your project settings to find `VERCEL_ORG_ID` and `VERCEL_PROJECT_ID`

### 3. Environment Variables

The application uses environment variables that are injected at build time:

- `API_URL` - Backend API endpoint
- `AUTH_SERVER` - Authentication server endpoint  
- `CLIENT_ID` - OAuth client identifier

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

