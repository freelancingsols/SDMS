# B2C WebApp Configuration Summary

## ✅ All B2C App Configurations Updated

### 1. AppSettings Files

#### Root AppSettings
- **File**: `SDMSApps/SDMS.B2CWebApp/appsettings.json`
- **Status**: ✅ Updated with production values
  - `SDMS_B2CWebApp_url`: `https://sdms-pi.vercel.app`
  - `SDMS_AuthenticationWebApp_url`: `https://sdms-production.up.railway.app`
  - `SDMS_AuthenticationWebApp_redirectUri`: `https://sdms-pi.vercel.app/auth-callback`

#### Client App AppSettings
- **File**: `SDMSApps/SDMS.B2CWebApp/ClientApp/src/assets/appsettings.json`
- **Status**: ✅ Updated with production values
  - This file is used at runtime by the Angular app
  - Updated during build by `build-vercel.js` script from environment variables

### 2. Vercel Configuration

#### Vercel.json
- **File**: `SDMSApps/SDMS.B2CWebApp/vercel.json`
- **Status**: ✅ Updated with CORS headers
  - `Access-Control-Allow-Origin: *`
  - `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`
  - `Access-Control-Allow-Headers: Content-Type, Authorization`
  - Security headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection)

### 3. Build Configuration

#### Build Script
- **File**: `SDMSApps/SDMS.B2CWebApp/ClientApp/scripts/build-vercel.js`
- **Status**: ✅ Configured
  - Reads environment variables from `process.env`
  - Updates `appsettings.json` during build
  - Sets production flag in `environment.ts`
  - Uses production defaults when `NODE_ENV === 'production'`

#### Vercel Build Script
- **File**: `SDMSApps/SDMS.B2CWebApp/ClientApp/scripts/vercel-build.sh`
- **Status**: ✅ Configured
  - Calls `build-vercel.js` to update appsettings
  - Runs production build

### 4. Deployment Workflow

#### GitHub Actions Workflow
- **File**: `.github/workflows/deploy-b2c-vercel.yml`
- **Status**: ✅ Fully configured
  - Syncs environment variables to Vercel (production and preview)
  - Validates all required variables
  - Builds and deploys to Vercel

## 📋 Required GitHub Variables for B2C App

All variables should be set in: **GitHub Repository → Settings → Secrets and variables → Actions → Variables**

### Application Configuration Variables
1. ✅ `SDMS_B2CWebApp_url` = `https://sdms-pi.vercel.app`
2. ✅ `SDMS_AuthenticationWebApp_url` = `https://sdms-production.up.railway.app`
3. ✅ `SDMS_AuthenticationWebApp_clientid` = `sdms_frontend`
4. ✅ `SDMS_AuthenticationWebApp_redirectUri` = `https://sdms-pi.vercel.app/auth-callback`
5. ✅ `SDMS_AuthenticationWebApp_scope` = `openid profile email roles api`

### Vercel Deployment Variables
6. ✅ `VERCEL_ORG_ID` = (Vercel organization/team ID)
7. ✅ `VERCEL_PROJECT_ID` = (Vercel project ID)

### Vercel Deployment Secrets
8. ✅ `VERCEL_TOKEN` = (Vercel team token - must be team token, not personal)

## 🔄 Configuration Flow

### Production Deployment
1. **GitHub Variables** → Set in GitHub repository
2. **Deployment Workflow** → Syncs variables to Vercel environment variables
3. **Vercel Build** → Reads environment variables from Vercel
4. **Build Script** → Updates `appsettings.json` with environment variable values
5. **Angular Build** → Builds with production configuration
6. **Runtime** → Angular app reads `appsettings.json` and initializes `AppSettings`

### Local Development
1. **Default Values** → Build script uses localhost defaults
2. **Manual Override** → Developers can edit `appsettings.json` for local testing
3. **ng serve** → Uses values from `appsettings.json`

## 🔗 Endpoint Configuration

### B2C App Endpoints
- **Base URL**: `https://sdms-pi.vercel.app`
- **Auth Callback**: `https://sdms-pi.vercel.app/auth-callback`
- **Silent Refresh**: `https://sdms-pi.vercel.app/silent-refresh.html`

### Authentication Server Endpoints (Used by B2C)
- **Discovery**: `https://sdms-production.up.railway.app/.well-known/openid-configuration`
- **Authorization**: `https://sdms-production.up.railway.app/connect/authorize`
- **Token**: `https://sdms-production.up.railway.app/connect/token`
- **UserInfo**: `https://sdms-production.up.railway.app/connect/userinfo`
- **Login API**: `https://sdms-production.up.railway.app/account/login`
- **Register API**: `https://sdms-production.up.railway.app/account/register`
- **UserInfo API**: `https://sdms-production.up.railway.app/account/userinfo`

## ✅ Verification Checklist

### Pre-Deployment
- [x] All GitHub variables are set
- [x] Vercel token is a team token (not personal)
- [x] Vercel project is linked
- [x] Root directory is set to `SDMSApps/SDMS.B2CWebApp`

### Post-Deployment
- [ ] B2C app is accessible at `https://sdms-pi.vercel.app`
- [ ] `/assets/appsettings.json` loads with correct production values
- [ ] Authentication flow works (redirects to auth server)
- [ ] Token acquisition works
- [ ] UserInfo endpoint returns user data
- [ ] No CORS errors in browser console

## 📝 Notes

- **Environment Variables**: The deployment workflow syncs GitHub variables to Vercel environment variables
- **Build Time**: Configuration is injected at build time, not runtime
- **Local Development**: For local dev, the build script uses localhost defaults
- **Production**: Production values come from Vercel environment variables (synced from GitHub)
- **CORS**: B2C app has CORS headers configured in `vercel.json`
- **Security**: Security headers are configured in `vercel.json`

## 🔍 Troubleshooting

### Issue: B2C app uses localhost URLs in production
**Solution**: 
1. Check Vercel environment variables are set
2. Verify GitHub variables are synced to Vercel
3. Check build logs for environment variable values
4. Redeploy after setting variables

### Issue: CORS errors
**Solution**:
1. Verify auth server CORS includes B2C URL
2. Check `vercel.json` has CORS headers
3. Verify redirect URI matches B2C URL

### Issue: Authentication flow fails
**Solution**:
1. Check `SDMS_AuthenticationWebApp_url` is correct
2. Verify `SDMS_AuthenticationWebApp_redirectUri` matches B2C URL
3. Check OpenID discovery document is accessible
4. Verify OpenIddict client includes B2C redirect URI

