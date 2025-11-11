# SDMS.B2CWebApp - Vercel Deployment Setup Summary

## ✅ Changes Made

### 1. Updated GitHub Actions Workflow
**File:** `.github/workflows/deploy-b2c-vercel.yml`

**Changes:**
- ✅ Changed from `vars` to `secrets` for environment variables (matching AuthenticationWebApp pattern)
- ✅ Updated step name to "Update configuration files from GitHub secrets"
- ✅ Fixed Vercel deployment configuration:
  - Changed `working-directory` to `SDMSApps/SDMS.B2CWebApp` (root of project)
  - Added `vercel-config-path` to explicitly reference `vercel.json`
  - Added `vercel-args: '--prod'` for production deployment
  - Changed `VERCEL_ORG_ID` and `VERCEL_PROJECT_ID` from `vars` to `secrets`

### 2. Updated Documentation
**Files:**
- `README_DEPLOYMENT.md` - Updated to reflect use of GitHub Secrets
- `DEPLOYMENT_CHECKLIST.md` - Created comprehensive deployment checklist

## 📋 Required GitHub Secrets

All secrets should be added in: **GitHub Repository → Settings → Secrets and variables → Actions → Secrets**

### Application Configuration (5 secrets)
1. `SDMS_B2CWebApp_url` - Your Vercel app URL
2. `SDMS_AuthenticationWebApp_url` - Authentication server URL
3. `SDMS_AuthenticationWebApp_clientid` - OAuth client ID
4. `SDMS_AuthenticationWebApp_redirectUri` - OAuth redirect URI
5. `SDMS_AuthenticationWebApp_scope` - OAuth scope

### Vercel Deployment (3 secrets)
6. `VERCEL_TOKEN` - Vercel authentication token
7. `VERCEL_ORG_ID` - Vercel organization ID
8. `VERCEL_PROJECT_ID` - Vercel project ID

## 🔄 Deployment Flow

1. **Trigger:** Push to `release` branch or manual workflow dispatch
2. **Build Process:**
   - Checkout code
   - Setup Node.js 18
   - Install dependencies
   - **Load secrets from GitHub Secrets**
   - **Update `src/assets/appsettings.json` with secret values**
   - **Set production flag in `src/environments/environment.ts`**
   - Build Angular app (production)
3. **Deploy:**
   - Deploy built files to Vercel using `vercel.json` configuration
   - Deploy to production environment

## 📁 Key Files

### Configuration Files
- `.github/workflows/deploy-b2c-vercel.yml` - GitHub Actions workflow
- `vercel.json` - Vercel deployment configuration
- `ClientApp/src/assets/appsettings.json` - Runtime config (updated during build)
- `ClientApp/src/environments/environment.ts` - Angular environment (updated during build)

### Documentation
- `README_DEPLOYMENT.md` - Deployment guide
- `DEPLOYMENT_CHECKLIST.md` - Step-by-step checklist
- `DEPLOYMENT_SETUP_SUMMARY.md` - This file

## 🚀 How to Deploy

### First Time Setup
1. Add all 8 GitHub secrets (see list above)
2. Get Vercel credentials (token, org ID, project ID)
3. Create Vercel project (if not exists)
4. Push to `release` branch or trigger workflow manually

### Subsequent Deployments
- Push to `release` branch → Automatic deployment
- Or manually trigger workflow from GitHub Actions tab

## 🔍 Verification

After deployment, verify:
1. ✅ Vercel deployment shows "Ready"
2. ✅ App URL is accessible
3. ✅ `/assets/appsettings.json` loads with correct values
4. ✅ Authentication flow works
5. ✅ Production build is active (check browser console)

## 📝 Notes

- **Secrets vs Vars:** All configuration now uses GitHub Secrets (not Variables) for better security
- **Build Time Injection:** Configuration is injected at build time, not runtime
- **Production Flag:** Automatically set to `true` during deployment
- **Vercel Config:** Uses `vercel.json` for deployment settings (output directory, headers, rewrites)

## 🔗 Related Documentation

- See `DEPLOYMENT_CHECKLIST.md` for detailed step-by-step instructions
- See `README_DEPLOYMENT.md` for general deployment information
- See `REFACTORING_SUMMARY.md` for code improvements made

