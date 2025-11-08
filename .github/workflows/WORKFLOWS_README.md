# GitHub Actions Workflows Documentation

This document consolidates all GitHub Actions workflows documentation for the SDMS project.

## Table of Contents

1. [Workflow Overview](#workflow-overview)
2. [CI Workflows](#ci-workflows)
3. [Deployment Workflows](#deployment-workflows)
4. [Error Handler Workflow](#error-handler-workflow)
5. [Troubleshooting](#troubleshooting)
6. [Common Issues](#common-issues)
7. [How to Run Workflows](#how-to-run-workflows)

---

## Workflow Overview

All CI/CD workflows are located at the repository root level in `.github/workflows/` directory.

### Workflow Files

```
.github/workflows/
├── ci-b2c-webapp.yml              # B2C WebApp CI
├── ci-authentication-webapp.yml   # Authentication WebApp CI
├── deploy-b2c-vercel.yml          # B2C WebApp Deployment
├── deploy-auth-railway.yml        # Authentication WebApp Deployment
└── error-handler.yml              # Error Handler and Reporter
```

### Path Filters

Each workflow uses `paths` filter to only trigger when relevant project files change:
- B2C WebApp workflows: `SDMSApps/SDMS.B2CWebApp/**`
- Authentication WebApp workflows: `SDMSApps/SDMS.AuthenticationWebApp/**`

---

## CI Workflows

### B2C WebApp (SDMS.B2CWebApp)

**File:** `.github/workflows/ci-b2c-webapp.yml`

**Triggers:**
- Push to: main, master, develop, release branches
- Pull requests to: main, master, develop, release branches
- Manual trigger: Yes (via GitHub Actions UI)
- Path filter: `SDMSApps/SDMS.B2CWebApp/**`

**Jobs:**
1. **Lint** - Runs Angular linting
2. **Build (Development)** - Builds Angular app in development mode
3. **Build (Production)** - Builds Angular app in production mode with environment variables
4. **Test** - Runs unit tests with code coverage
5. **CI Complete** - Aggregates results from all jobs

### Authentication WebApp (SDMS.AuthenticationWebApp)

**File:** `.github/workflows/ci-authentication-webapp.yml`

**Triggers:**
- Push to: main, master, develop, release branches
- Pull requests to: main, master, develop, release branches
- Manual trigger: Yes (via GitHub Actions UI)
- Path filter: `SDMSApps/SDMS.AuthenticationWebApp/**`

**Jobs:**
1. **Lint** - Runs .NET format check
2. **Build (Development)** - Builds Angular frontend and .NET backend in Debug mode
3. **Build (Production)** - Builds Angular frontend and .NET backend in Release mode
4. **Test** - Runs .NET tests (if any) and Angular tests (if any)
5. **CI Complete** - Aggregates results from all jobs

**Build Order:**
1. Install Angular dependencies (generates package-lock.json)
2. Build Angular/Node code first
3. Restore .NET dependencies
4. Build .NET code

---

## Deployment Workflows

### B2C WebApp → Vercel

**File:** `.github/workflows/deploy-b2c-vercel.yml`

**Triggers:**
- Push to: `release` branch
- Manual trigger: Yes (via GitHub Actions UI)
- Path filter: `SDMSApps/SDMS.B2CWebApp/**`

**Deployment:**
- Platform: Vercel
- Build: Production build with environment variables
- Artifacts: Uploaded for 7 days

**Required Secrets:**
- `VERCEL_TOKEN`
- `VERCEL_ORG_ID`
- `VERCEL_PROJECT_ID`
- `API_URL`
- `AUTH_SERVER`
- `CLIENT_ID`

### Authentication WebApp → Railway

**File:** `.github/workflows/deploy-auth-railway.yml`

**Triggers:**
- Push to: `release` branch
- Manual trigger: Yes (via GitHub Actions UI)
- Path filter: `SDMSApps/SDMS.AuthenticationWebApp/**`

**Deployment:**
- Platform: Railway
- Build: Production build (.NET + Angular)
- Database: PostgreSQL (auto-configured by Railway)

**Required Secrets:**
- `RAILWAY_TOKEN`
- `RAILWAY_PROJECT_ID`
- `RAILWAY_SERVICE_ID`

**Railway Environment Variables (Set in Railway Dashboard):**
- `POSTGRES_CONNECTION` (auto-generated)
- `Authentication__LoginUrl`
- `Authentication__LogoutUrl`
- `Authentication__ErrorUrl`
- `ExternalAuth__Google__ClientId`
- `ExternalAuth__Google__ClientSecret`
- `Frontend__Url`
- `Webhook__Secret`

---

## Error Handler Workflow

**File:** `.github/workflows/error-handler.yml`

**Purpose:** Automatically detects, analyzes, and reports build failures in CI/CD pipelines.

**Triggers:**
- Automatically when any monitored workflow fails
- Manual trigger via `workflow_dispatch`

**Monitored Workflows:**
- `CI - B2C WebApp (Build, Test, and Lint)`
- `CI - Authentication WebApp (Build, Test, and Lint)`
- `Deploy B2C WebApp to Vercel`
- `Deploy Authentication WebApp to Railway`

**Features:**
- Comprehensive error extraction (C#, TypeScript, NPM, Build config, Tests, Linter)
- Comparison with last successful build
- Detailed error reports with file paths and line numbers
- Fix suggestions for Cursor AI
- GitHub issue creation
- Artifact uploads (30-day retention)

**For detailed documentation, see:** `.github/workflows/ERROR_HANDLER_README.md`

---

## Troubleshooting

### Common Issues

#### Issue 1: Build Script Not Found

**Error:** `npm ERR! missing script: build:prod`

**Solution:** Check if `build:prod` script exists in `package.json`. If not, update the workflow to use the correct script name.

**Fix:** Update workflow to use:
```yaml
run: npm run build -- --configuration production
```

#### Issue 2: build-env.js Not Found

**Error:** `node: cannot find module 'build-env.js'`

**Solution:** Ensure `build-env.js` exists in `ClientApp` directory. If missing, create it or remove the step.

**Fix:** Either:
1. Create the `build-env.js` file
2. Or remove the `node build-env.js` step if not needed

#### Issue 3: Path Not Found

**Error:** `Error: ENOENT: no such file or directory`

**Solution:** Verify the folder structure matches the workflow paths. The folder should be:
- `SDMSApps/SDMS.B2CWebApp/ClientApp/`
- `SDMSApps/SDMS.AuthenticationWebApp/ClientApp/`

#### Issue 4: Package.json Not Found

**Error:** `npm ERR! path /path/to/package.json`

**Solution:** Ensure `package.json` exists in the `ClientApp` directory.

#### Issue 5: Angular Build Fails

**Error:** `An unhandled exception occurred`

**Solution:** Check for:
- Missing dependencies in `package.json`
- Angular version compatibility
- TypeScript errors

**Fix:** Run `npm install` locally first to identify issues.

#### Issue 6: Test Script Fails

**Error:** `npm ERR! missing script: test:ci`

**Solution:** Ensure `test:ci` script exists in `package.json` or update workflow to use correct script.

#### Issue 7: Cache Dependency Path Error

**Error:** `Some specified paths were not resolved`

**Solution:** The `package.json` file must exist before the cache step runs. The workflow should create it if missing.

#### Issue 8: .NET Build Fails

**Error:** `error NETSDK1045: The current .NET SDK does not support targeting .NET 8.0`

**Solution:** 
- Install .NET 8.0 SDK locally
- Or rely on GitHub Actions (workflows use .NET 8.0)

#### Issue 9: Angular Configuration Error

**Error:** `Schema validation failed` or `buildOptimizer option cannot be used without aot`

**Solution:** Remove deprecated properties from `angular.json`:
- Remove `aot` property (AOT is always enabled in Angular 18)
- Remove `extractCss` property (deprecated in Angular 18)
- Remove `defaultProject` property (deprecated)

---

## How to Debug Workflow Issues

### Step 1: Check Workflow Logs

1. Go to GitHub → **Actions** tab
2. Click on the failed workflow run
3. Click on the failed job
4. Expand each step to see the error message
5. Copy the error message

### Step 2: Test Locally

Run the same commands locally to reproduce the error:

```powershell
# For B2C WebApp
cd SDMSApps\SDMS.B2CWebApp\ClientApp
npm install
npm run build:prod

# For Authentication WebApp
cd SDMSApps\SDMS.AuthenticationWebApp\ClientApp
npm install
npm run build -- --configuration production
```

### Step 3: Verify File Structure

Ensure these files exist:
- `SDMSApps/SDMS.B2CWebApp/ClientApp/package.json`
- `SDMSApps/SDMS.B2CWebApp/ClientApp/build-env.js` (if used)
- `SDMSApps/SDMS.B2CWebApp/ClientApp/angular.json`
- `SDMSApps/SDMS.AuthenticationWebApp/ClientApp/package.json`
- `SDMSApps/SDMS.AuthenticationWebApp/ClientApp/angular.json`

### Step 4: Check Package.json Scripts

Verify these scripts exist:
- `build` - Development build
- `build:prod` - Production build (or `build --configuration production`)
- `test:ci` - CI test script
- `lint` - Linting script

### Step 5: Verify Environment Variables

Check if required environment variables are set:
- GitHub Secrets for deployment
- Railway environment variables
- Vercel environment variables

---

## How to Run Workflows

### Manual Trigger

1. Go to GitHub → **Actions** tab
2. Select the workflow you want to run
3. Click **Run workflow** button
4. Select branch and click **Run workflow**

### Automatic Trigger

Workflows automatically run on:
- Push to main, master, develop, release branches
- Pull requests to main, master, develop, release branches
- Workflow failures (for error handler)

### Branch Strategy

- **main/master/develop** - CI runs (lint, build, test) but no deployment
- **release/Release** - CI runs + Deployment to production
- **Pull Requests** - CI runs to validate changes

---

## Build Order and Package Management

### Package Management

All workflows now:
- Use `package.json` for npm cache dependency path
- Generate `package-lock.json` automatically if it doesn't exist
- Use `npm install --legacy-peer-deps` to handle peer dependency issues
- Remove old `package-lock.json` and `node_modules` before install

### Build Order

**AuthenticationWebApp:**
1. Remove old package-lock.json and node_modules
2. Install Angular dependencies (generates package-lock.json)
3. Build Angular/Node code first
4. Restore .NET dependencies
5. Build .NET code

**B2C WebApp:**
- Frontend only - no .NET build
- Remove old package-lock.json and node_modules
- Install dependencies
- Build Angular app

---

## Viewing Workflow Results

After pushing these files to your repository, you can view them in:
- **GitHub Repository** → **Actions** tab
- Click on a workflow run to see detailed logs
- Download workflow artifacts
- View workflow summaries

---

## Security Best Practices

1. ✅ Never commit secrets to code
2. ✅ Use different secrets for dev/staging/production
3. ✅ Rotate secrets regularly
4. ✅ Use least privilege principle
5. ✅ Enable 2FA on GitHub and deployment platforms
6. ✅ Use GitHub Secrets for sensitive data
7. ✅ Review workflow permissions regularly

---

## Quick Reference

### Required GitHub Secrets

#### For B2CWebApp (Vercel)
- `API_URL`
- `AUTH_SERVER`
- `CLIENT_ID`
- `VERCEL_TOKEN`
- `VERCEL_ORG_ID`
- `VERCEL_PROJECT_ID`

#### For AuthenticationWebApp (Railway)
- `RAILWAY_TOKEN`
- `RAILWAY_PROJECT_ID`
- `RAILWAY_SERVICE_ID`

### Required Railway Environment Variables

Set these in Railway dashboard:
- `POSTGRES_CONNECTION` (auto-generated)
- `Authentication__LoginUrl`
- `Authentication__LogoutUrl`
- `Authentication__ErrorUrl`
- `ExternalAuth__Google__ClientId`
- `ExternalAuth__Google__ClientSecret`
- `Frontend__Url`
- `Webhook__Secret`

---

## Documentation Files

This consolidated document replaces the following individual documentation files:
- `.github/workflows/README.md`
- `.github/workflows/TROUBLESHOOTING.md`
- `.github/workflows/COMMON_ISSUES.md`
- `.github/workflows/DEBUG_WORKFLOWS.md`
- `.github/workflows/HOW_TO_RUN_WORKFLOWS.md`
- `.github/workflows/FIX_RUN_WORKFLOW_BUTTON.md`
- `.github/workflows/NPM_DEPENDENCY_FIX.md`
- `.github/workflows/QUICK_FIX.md`
- `.github/workflows/AUTOMATED_ERROR_FIXING.md`

All workflow documentation has been consolidated into this single document for easier reference and maintenance.

---

**Last Updated:** 2024
**Status:** ✅ All workflows configured and tested

