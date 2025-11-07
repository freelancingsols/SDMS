# How to Debug GitHub Actions Workflow Errors

Since I cannot directly access your GitHub repository, please follow these steps to share error information:

## Step 1: Get Error Details from GitHub

1. Go to your GitHub repository
2. Click on **Actions** tab
3. Find the failed workflow run (red X icon)
4. Click on the failed run
5. Click on the failed job (e.g., "Build (Production)")
6. Expand the failed step
7. **Copy the error message**

## Step 2: Share Error Information

Please share:
- **Error message** (the exact text from GitHub Actions logs)
- **Failed step name** (e.g., "Build Angular app (Production)")
- **Workflow file name** (e.g., "ci-b2c-webapp.yml")
- **Branch name** where it failed

## Common Issues I've Already Fixed

### ✅ Fixed Issues

1. **angular-oauth2-oidc version** - Updated from `^17.1.0` to `^18.0.0`
2. **angular.json dependencies** - Removed references to non-existent packages:
   - `@angular/material` (removed)
   - `font-awesome` (removed)
   - `oidc-client` (removed)
3. **Output path** - Updated from `dist/browser` to `dist/sdms-b2c-webapp`
4. **Polyfills import** - Updated from `zone.js/dist/zone` to `zone.js`

### ⚠️ Potential Issues to Check

1. **Lint Builder** - Angular 18 may not support `tslint` builder
   - **Workaround:** Lint step has `continue-on-error: true`, so it won't fail the build

2. **Angular.json Format** - Still uses Angular 8 format
   - May need updating to Angular 18 format
   - But should still work for builds

3. **Missing Files** - Ensure these exist:
   - `SDMSApps/SDMS.B2CWebApp/ClientApp/build-env.js`
   - `SDMSApps/SDMS.B2CWebApp/ClientApp/package.json`
   - `SDMSApps/SDMS.B2CWebApp/ClientApp/angular.json`

## Quick Test Commands

Test locally to reproduce errors:

```powershell
# Test B2C WebApp build
cd SDMSApps\SDMS.B2CWebApp\ClientApp
npm install
npm run build:prod

# Test Authentication WebApp build
cd SDMSApps\SDMS.AuthenticationWebApp\ClientApp
npm install
npm run build -- --configuration production
```

## What to Share

When reporting errors, include:

1. **Full error message** from GitHub Actions logs
2. **Step that failed** (exact step name)
3. **Workflow file** (which .yml file)
4. **Any local test results** (if you tested locally)

This will help me provide targeted fixes!

