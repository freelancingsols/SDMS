# SDMS.B2CWebApp - Vercel Deployment Checklist

Use this checklist to ensure a successful deployment to Vercel.

## Pre-Deployment Setup

### 1. GitHub Secrets Configuration

Go to your GitHub repository → Settings → Secrets and variables → Actions → Secrets

Add the following secrets:

#### Application Configuration Secrets
- [ ] `SDMS_B2CWebApp_url` - Your Vercel app URL (e.g., `https://your-app.vercel.app`)
- [ ] `SDMS_AuthenticationWebApp_url` - Authentication server URL (e.g., `https://your-auth.railway.app`)
- [ ] `SDMS_AuthenticationWebApp_clientid` - OAuth client ID (e.g., `sdms_frontend`)
- [ ] `SDMS_AuthenticationWebApp_redirectUri` - OAuth redirect URI (e.g., `https://your-app.vercel.app/auth-callback`)
- [ ] `SDMS_AuthenticationWebApp_scope` - OAuth scope (e.g., `openid profile email roles api`)

#### Vercel Deployment Secrets
- [ ] `VERCEL_TOKEN` - Vercel authentication token
- [ ] `VERCEL_ORG_ID` - Vercel organization ID
- [ ] `VERCEL_PROJECT_ID` - Vercel project ID

### 2. Getting Vercel Credentials

#### Vercel Token
1. Go to [Vercel Dashboard](https://vercel.com/dashboard)
2. Go to Settings → Tokens
3. Click "Create Token"
4. Give it a name (e.g., "GitHub Actions Deployment")
5. Set expiration (recommended: No expiration or long expiration)
6. Copy the token → Add to GitHub Secret: `VERCEL_TOKEN`

#### Vercel Organization ID
1. Go to your Vercel Dashboard
2. Click on your organization/team name
3. Go to Settings → General
4. Copy "Organization ID" → Add to GitHub Secret: `VERCEL_ORG_ID`

#### Vercel Project ID
1. Go to your Vercel Dashboard
2. Select your project (or create a new one)
3. Go to Settings → General
4. Copy "Project ID" → Add to GitHub Secret: `VERCEL_PROJECT_ID`

**Note:** If you haven't created a Vercel project yet:
1. Go to Vercel Dashboard
2. Click "Add New" → "Project"
3. Import your GitHub repository (optional, we'll deploy via GitHub Actions)
4. Get the Project ID from Settings → General

## Deployment Process

### Automatic Deployment (Recommended)

The deployment happens automatically when you push to the `release` branch:

1. [ ] Push your code to the `release` branch
2. [ ] GitHub Actions workflow triggers automatically
3. [ ] Check workflow status in GitHub → Actions tab
4. [ ] Verify deployment in Vercel Dashboard

### Manual Deployment Trigger

You can also manually trigger the deployment:

1. [ ] Go to GitHub → Actions tab
2. [ ] Select "Deploy B2C WebApp to Vercel" workflow
3. [ ] Click "Run workflow"
4. [ ] Select branch (usually `release`)
5. [ ] Click "Run workflow" button

## Verification Steps

After deployment, verify the following:

### 1. Vercel Deployment
- [ ] Deployment shows as "Ready" in Vercel Dashboard
- [ ] Deployment URL is accessible (e.g., `https://your-app.vercel.app`)
- [ ] No build errors in Vercel logs

### 2. Application Configuration
- [ ] Open browser DevTools → Network tab
- [ ] Navigate to your app
- [ ] Check that `/assets/appsettings.json` loads correctly
- [ ] Verify the configuration values match your GitHub secrets

### 3. Authentication Flow
- [ ] Test login flow
- [ ] Verify redirect URI matches your configuration
- [ ] Test logout flow
- [ ] Verify OAuth callback works

### 4. Production Build
- [ ] Check that `environment.production` is `true` in browser console
- [ ] Verify service worker is registered (if enabled)
- [ ] Check that Angular optimizations are applied (minified code, etc.)

## Troubleshooting

### Build Fails in GitHub Actions

**Issue:** Build step fails
- [ ] Check Node.js version (should be 18)
- [ ] Verify `package.json` is correct
- [ ] Check for dependency conflicts
- [ ] Review build logs in GitHub Actions

**Issue:** Environment variables not found
- [ ] Verify all secrets are set in GitHub Secrets
- [ ] Check secret names match exactly (case-sensitive)
- [ ] Ensure secrets are in the correct repository

### Deployment Fails in Vercel

**Issue:** Vercel deployment fails
- [ ] Verify `VERCEL_TOKEN` is valid and not expired
- [ ] Check `VERCEL_ORG_ID` and `VERCEL_PROJECT_ID` are correct
- [ ] Ensure Vercel project exists
- [ ] Check Vercel logs for specific errors

**Issue:** App loads but configuration is wrong
- [ ] Verify GitHub secrets have correct values
- [ ] Check that `appsettings.json` was updated during build
- [ ] Review GitHub Actions logs for configuration update step

### Application Issues

**Issue:** Authentication doesn't work
- [ ] Verify `SDMS_AuthenticationWebApp_url` points to correct auth server
- [ ] Check `SDMS_AuthenticationWebApp_redirectUri` matches your Vercel URL
- [ ] Verify OAuth client ID is correct
- [ ] Check CORS settings on authentication server

**Issue:** CORS errors
- [ ] Verify authentication server allows your Vercel domain
- [ ] Check `SDMS_AuthenticationWebApp_url` is correct
- [ ] Review authentication server CORS configuration

## Post-Deployment

### 1. Update Documentation
- [ ] Update any documentation with the new production URL
- [ ] Update authentication server CORS settings if needed
- [ ] Update any external references to the app URL

### 2. Monitor
- [ ] Set up Vercel analytics (optional)
- [ ] Monitor error logs in Vercel Dashboard
- [ ] Set up uptime monitoring (optional)

### 3. Security
- [ ] Verify HTTPS is enforced (Vercel does this automatically)
- [ ] Review security headers in `vercel.json`
- [ ] Ensure sensitive data is not exposed in client-side code

## Rollback Procedure

If you need to rollback:

1. [ ] Go to Vercel Dashboard → Your Project → Deployments
2. [ ] Find the previous working deployment
3. [ ] Click "..." → "Promote to Production"
4. [ ] Verify the rollback worked

Or revert in GitHub:

1. [ ] Revert the commit in GitHub
2. [ ] Push to `release` branch
3. [ ] GitHub Actions will deploy the previous version

## Quick Reference

### GitHub Secrets Required
```
SDMS_B2CWebApp_url
SDMS_AuthenticationWebApp_url
SDMS_AuthenticationWebApp_clientid
SDMS_AuthenticationWebApp_redirectUri
SDMS_AuthenticationWebApp_scope
VERCEL_TOKEN
VERCEL_ORG_ID
VERCEL_PROJECT_ID
```

### Workflow File
`.github/workflows/deploy-b2c-vercel.yml`

### Configuration Files
- `vercel.json` - Vercel deployment configuration
- `ClientApp/src/assets/appsettings.json` - Runtime configuration (updated during build)
- `ClientApp/src/environments/environment.ts` - Angular environment (updated during build)

### Build Output
`ClientApp/dist/sdms-b2c-webapp/`

## Support

If you encounter issues:
1. Check GitHub Actions logs
2. Check Vercel deployment logs
3. Review browser console for client-side errors
4. Verify all secrets are correctly set
5. Check that authentication server is accessible

