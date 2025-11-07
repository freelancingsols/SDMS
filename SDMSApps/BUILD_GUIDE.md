# Local Build Guide

This guide explains how to build projects locally and resolve common build issues.

## Prerequisites

### Required SDKs

- **.NET SDK 8.0** - Required for `SDMS.AuthenticationWebApp`
- **.NET SDK 3.1** - Required for `SDMS.B2BWebApp` and other older projects
- **Node.js 18.x** - Required for Angular frontend builds
- **npm** - Comes with Node.js

### Check Your SDK Versions

```powershell
# Check .NET SDK versions
dotnet --list-sdks

# Check Node.js version
node --version

# Check npm version
npm --version
```

## Building Projects

### SDMS.AuthenticationWebApp

**Requirements:**
- .NET SDK 8.0
- Node.js 18.x

**Build Steps:**

```powershell
cd SDMSApps\SDMS.AuthenticationWebApp

# Restore .NET dependencies
dotnet restore

# Install Angular dependencies
cd ClientApp
npm install
cd ..

# Build Angular app (optional, done automatically during .NET build)
cd ClientApp
npm run build
cd ..

# Build .NET project
dotnet build

# Or build in Release mode
dotnet build --configuration Release
```

**Note:** If you don't have .NET 8.0 SDK installed locally, you can:
- Install .NET 8.0 SDK from https://dotnet.microsoft.com/download
- Or rely on GitHub Actions for builds (workflows will work fine)

### SDMS.B2BWebApp

**Requirements:**
- .NET SDK 3.1
- Node.js (any recent version)

**Build Steps:**

```powershell
cd SDMSApps\SDMS.B2BWebApp

# Build .NET project (automatically installs npm dependencies)
dotnet build
```

**Note:** The build process will automatically run `npm install` if `node_modules` doesn't exist.

### SDMS.EndUserWebApp (B2C WebApp)

**Requirements:**
- Node.js 18.x
- No .NET build required (frontend-only project)

**Build Steps:**

```powershell
cd SDMSApps\SDMS.EndUserWebApp\ClientApp

# Install dependencies
npm install

# Build for development
npm run build

# Build for production
npm run build:prod
```

## Common Build Issues

### Issue: .NET SDK Version Mismatch

**Error:**
```
error NETSDK1045: The current .NET SDK does not support targeting .NET 8.0
```

**Solution:**
- Install .NET 8.0 SDK from https://dotnet.microsoft.com/download
- Or use GitHub Actions for builds

### Issue: npm Dependencies Not Found

**Error:**
```
npm ERR! code ENOENT
npm ERR! syscall open
npm ERR! path package.json
```

**Solution:**
- Make sure you're in the correct directory (`ClientApp` folder)
- Run `npm install` in the `ClientApp` directory

### Issue: Angular Build Fails

**Error:**
```
Error: Cannot find module '@angular/...'
```

**Solution:**
```powershell
cd ClientApp
rm -rf node_modules package-lock.json  # On Windows: Remove-Item -Recurse -Force node_modules,package-lock.json
npm install
```

### Issue: Cache Dependency Path Error in GitHub Actions

**Error:**
```
Error: Some specified paths were not resolved, unable to cache dependencies
```

**Solution:**
- This is already fixed in workflows
- Workflows use `package.json` instead of `package-lock.json` for AuthenticationWebApp
- Make sure `package.json` exists in the ClientApp directory

## GitHub Actions Workflows

All workflows are configured to:
- Use .NET 8.0 SDK (for AuthenticationWebApp)
- Use Node.js 18.x
- Build Angular apps with production configuration
- Cache npm dependencies

### Workflow Files Location

All workflows are in `.github/workflows/` at repository root:
- `ci-authentication-webapp.yml`
- `ci-b2c-webapp.yml`
- `deploy-auth-railway.yml`
- `deploy-b2c-vercel.yml`

### Manual Trigger

You can manually trigger workflows from GitHub Actions UI:
1. Go to **Actions** tab
2. Select workflow
3. Click **"Run workflow"**

## Build Output Locations

### AuthenticationWebApp
- .NET: `bin/Debug/net8.0/` or `bin/Release/net8.0/`
- Angular: `ClientApp/dist/sdms-auth-client/`

### B2BWebApp
- .NET: `bin/Debug/netcoreapp3.1/` or `bin/Release/netcoreapp3.1/`
- Angular: `ClientApp/dist/browser/`

### B2C WebApp (EndUserWebApp)
- Angular: `ClientApp/dist/sdms-b2c-webapp/`

## Troubleshooting

### Clean Build

If you encounter build issues, try a clean build:

```powershell
# For .NET projects
dotnet clean
dotnet restore
dotnet build

# For Angular projects
cd ClientApp
Remove-Item -Recurse -Force node_modules,dist
npm install
npm run build
```

### Check for Missing Files

Make sure these files exist:
- `package.json` in ClientApp directory
- `.csproj` file in project root
- `angular.json` in ClientApp directory (for Angular projects)

## Next Steps

After successful local build:
1. Test the application locally
2. Commit changes
3. Push to trigger GitHub Actions
4. Check workflow status in GitHub Actions tab

## Getting Help

If you encounter build issues:
1. Check the error message carefully
2. Verify SDK versions match requirements
3. Try a clean build (see above)
4. Check GitHub Actions logs for CI/CD issues
5. Review workflow files in `.github/workflows/`

