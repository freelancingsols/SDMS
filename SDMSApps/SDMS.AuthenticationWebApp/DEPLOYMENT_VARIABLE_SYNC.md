# Deployment Variable Sync Flow

## Overview
This document explains how environment variables flow from GitHub Variables/Secrets → Railway/Vercel during deployment.

---

## Flow Diagram

```
GitHub Variables/Secrets
    ↓
GitHub Actions Workflow
    ↓
    ├─→ Build Step (uses GitHub vars for local build validation)
    │   └─→ Generates appsettings.json with production URLs
    │
    └─→ Sync Step (syncs GitHub vars to Railway/Vercel)
        └─→ Railway/Vercel now has variables for their build
            └─→ Railway/Vercel builds using their variables
                └─→ Deployed app uses correct production URLs
```

---

## Railway Deployment Flow

### File: `.github/workflows/deploy-auth-railway.yml`

**Step 1: Build Angular App (Local - for validation)**
- **Location:** Line 159-166
- **Uses:** GitHub Variables (`vars.SDMS_AuthenticationWebApp_url`, `vars.SDMS_AuthenticationWebApp_clientid`)
- **Purpose:** Validates build works with production URLs before deploying
- **Output:** Local build artifacts (not used in deployment, Railway rebuilds)

**Step 2: Sync Variables to Railway**
- **Location:** Line 183-487
- **Reads from:** GitHub Variables/Secrets
- **Writes to:** Railway environment variables
- **Critical Variables Synced:**
  - ✅ `SDMS_AuthenticationWebApp_url` - **CRITICAL** (used during Railway build)
  - ✅ `SDMS_AuthenticationWebApp_clientid` - **CRITICAL** (used during Railway build)
  - ✅ `SDMS_B2CWebApp_url`
  - ✅ `SDMS_AuthenticationWebApp_ConnectionString`
  - ✅ All other configuration variables
  - ❌ `SDMS_AuthenticationWebApp_FrontendUrl` - **REMOVED** (obsolete)

**Step 3: Deploy to Railway**
- **Location:** Line 497-503
- **Command:** `railway up --service $SERVICE_ID`
- **What happens:**
  1. Railway triggers build using `nixpacks.toml`
  2. `nixpacks.toml` runs `npm run build:prod` in ClientApp
  3. `build:prod` runs `build-env.js` which reads Railway environment variables
  4. `build-env.js` generates `appsettings.json` with production URLs
  5. Angular build completes with correct URLs
  6. .NET app builds and publishes
  7. Deployment completes

---

## Vercel Deployment Flow

### File: `.github/workflows/deploy-b2c-vercel.yml`

**Step 1: Sync Variables to Vercel**
- **Location:** Line 266-333
- **Reads from:** GitHub Variables
- **Writes to:** Vercel environment variables (production + preview)
- **Variables Synced:**
  - ✅ `SDMS_B2CWebApp_url`
  - ✅ `SDMS_AuthenticationWebApp_url`
  - ✅ `SDMS_AuthenticationWebApp_clientid`
  - ✅ `SDMS_AuthenticationWebApp_redirectUri`
  - ✅ `SDMS_AuthenticationWebApp_scope`

**Step 2: Build Angular App**
- **Location:** Line 340-342
- **Uses:** Vercel environment variables (set in Step 1)
- **Command:** `npm run build` (which runs `build-env.js`)
- **Output:** Built Angular app with correct production URLs

**Step 3: Deploy to Vercel**
- **Location:** After build
- **What happens:**
  1. Vercel builds using environment variables from Step 1
  2. `build-env.js` reads Vercel env vars
  3. Generates `appsettings.json` with production URLs
  4. Angular build completes
  5. Vercel deploys the built app

---

## Critical Variables for Build

These variables **MUST** be set in GitHub Variables and will be synced to Railway/Vercel:

### AuthenticationWebApp (Railway)
- ✅ `SDMS_AuthenticationWebApp_url` - **CRITICAL** (used during Angular build)
- ✅ `SDMS_AuthenticationWebApp_clientid` - **CRITICAL** (used during Angular build)

### B2CWebApp (Vercel)
- ✅ `SDMS_B2CWebApp_url` - **CRITICAL** (used during Angular build)
- ✅ `SDMS_AuthenticationWebApp_url` - **CRITICAL** (used during Angular build)
- ✅ `SDMS_AuthenticationWebApp_clientid` - **CRITICAL** (used during Angular build)
- ✅ `SDMS_AuthenticationWebApp_redirectUri` - Optional (generated in code if not set)
- ✅ `SDMS_AuthenticationWebApp_scope` - **CRITICAL**

---

## Variable Sync Logic

### Railway Sync (`.github/workflows/deploy-auth-railway.yml`)

**How it works:**
1. Reads GitHub Variables/Secrets
2. Fetches current Railway variables via GraphQL API
3. Compares GitHub values with Railway values
4. **INSERT** if variable doesn't exist in Railway
5. **UPDATE** if variable exists but value is different
6. **SKIP** if variable exists and value is the same

**Variables synced:**
- All `SDMS_AuthenticationWebApp_*` variables from GitHub Variables
- All secrets from GitHub Secrets
- **Removed:** `SDMS_AuthenticationWebApp_FrontendUrl` (obsolete)

### Vercel Sync (`.github/workflows/deploy-b2c-vercel.yml`)

**How it works:**
1. Reads GitHub Variables
2. Uses Vercel CLI to sync variables
3. Removes old variable, adds new one (ensures update)
4. Syncs to both `production` and `preview` environments

**Variables synced:**
- All `SDMS_*` variables needed for B2C WebApp build

---

## Updated Variables (After Our Changes)

### ✅ Added to Railway Sync
- `SDMS_AuthenticationWebApp_url` - **NEW** (was missing, now synced)
- `SDMS_AuthenticationWebApp_clientid` - **NEW** (was missing, now synced)

### ❌ Removed from Railway Sync
- `SDMS_AuthenticationWebApp_FrontendUrl` - **REMOVED** (obsolete, replaced with `SDMS_B2CWebApp_url`)

### ✅ Updated Build Steps
- Railway deployment: Now passes `SDMS_AuthenticationWebApp_url` and `clientid` to Angular build
- Uses `npm run build:prod` (which runs `build-env.js`)

---

## Verification Checklist

After deployment, verify:

- [ ] **GitHub Variables are set:**
  - `SDMS_AuthenticationWebApp_url` = Production Railway URL
  - `SDMS_AuthenticationWebApp_clientid` = `sdms_frontend`
  - `SDMS_B2CWebApp_url` = Production Vercel URL

- [ ] **Railway Variables are synced:**
  - Check Railway dashboard → Variables tab
  - Should see `SDMS_AuthenticationWebApp_url` with production URL
  - Should see `SDMS_AuthenticationWebApp_clientid`

- [ ] **Build logs show correct URLs:**
  - Railway build logs should show: `SDMS_AuthenticationWebApp_url=✅ Environment Variable`
  - Should NOT show: `⚠️ WARNING: Using localhost URL in build!`

- [ ] **Deployed app works:**
  - No `ERR_CONNECTION_REFUSED` errors
  - App connects to production Railway URL (not localhost)

---

## Troubleshooting

### Issue: Deployed app still uses localhost URLs

**Cause:** Variables not synced to Railway before build, or build didn't use environment variables.

**Fix:**
1. Check Railway Variables tab - ensure `SDMS_AuthenticationWebApp_url` is set
2. Check GitHub Variables - ensure they're set correctly
3. Redeploy - variables are synced before Railway build
4. Check Railway build logs - should show environment variables being used

### Issue: Build fails with "Missing required configuration"

**Cause:** GitHub Variables not set.

**Fix:**
1. Go to GitHub → Settings → Secrets and variables → Actions → Variables
2. Add missing variables
3. Redeploy

---

## Summary

✅ **Railway Deployment:**
- Reads from GitHub Variables/Secrets
- Syncs to Railway BEFORE build
- Railway build uses synced variables
- ✅ Now includes `SDMS_AuthenticationWebApp_url` and `clientid`
- ❌ Removed obsolete `FrontendUrl`

✅ **Vercel Deployment:**
- Reads from GitHub Variables
- Syncs to Vercel BEFORE build
- Vercel build uses synced variables
- ✅ Already correct

✅ **Build Process:**
- Both workflows now pass critical variables to Angular build
- `build-env.js` reads environment variables during build
- Generates `appsettings.json` with production URLs
- ✅ Build fails if localhost detected in production

