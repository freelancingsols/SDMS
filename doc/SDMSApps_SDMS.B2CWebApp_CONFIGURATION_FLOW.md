# SDMS B2C WebApp - Configuration Loading Flow

This document explains how application configuration is loaded and used in the SDMS B2C WebApp.

## Overview

The configuration system uses a **simple 3-step approach**:
1. **CI/CD/Vercel**: Reads GitHub secrets → Sets environment variables → Updates `src/assets/appsettings.json` (file already exists in source)
2. **Build**: Angular build copies `src/assets/appsettings.json` → `dist/assets/appsettings.json`
3. **Runtime**: `loadAppSettingsBeforeBootstrap()` reads `/assets/appsettings.json` → Initializes `AppSettings`

---

## Configuration Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          STEP 1: CI/CD/Vercel                           │
└─────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐
│  GitHub Secrets  │  (SDMS_AuthenticationWebApp_url, etc.)
└────────┬─────────┘
         │
         │ Sets environment variables
         │
         ▼
┌─────────────────────────────────────┐
│  Environment Variables              │
│  process.env.SDMS_*                 │
│  (Available during build)           │
└────────┬────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────┐
│                          STEP 2: Build Time                             │
└─────────────────────────────────────────────────────────────────────────┘

         │
         │ CI/CD/Vercel updates file
         │ (inline node command, no separate script)
         │
         ▼
┌─────────────────────────────────────┐
│  CI/CD/Vercel                       │
│  Reads process.env.SDMS_*           │
│  Updates src/assets/appsettings.json│
│  (file already exists in source)    │
└────────┬────────────────────────────┘
         │
         │ Updates file in place
         │
         ▼
┌─────────────────────────────────────┐
│  src/assets/appsettings.json        │
│  {                                  │
│    "SDMS_B2CWebApp_url": "...",    │
│    "SDMS_AuthenticationWebApp_...": │
│  }                                  │
│  (Updated with env vars)            │
└────────┬────────────────────────────┘
         │
         │ Angular build copies to dist
         │
         ▼
┌─────────────────────────────────────┐
│  dist/sdms-b2c-webapp/              │
│    assets/appsettings.json          │  (Deployed file)
└─────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────┐
│                          STEP 3: Runtime (Browser)                      │
└─────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐
│   Browser        │  User opens application
│   (index.html)   │
└────────┬─────────┘
         │
         │ Loads
         │
         ▼
┌──────────────────┐
│   main.ts        │  DOMContentLoaded event
│                  │  ┌─────────────────────────────┐
│                  │  │ await loadAppSettingsBefore │
│                  │  │ Bootstrap()                 │
│                  │  └───────────┬─────────────────┘
└──────────────────┘              │
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────┐
│  loadAppSettingsBeforeBootstrap()                           │
│  (app-settings-loader.service.ts)                           │
│                                                              │
│  1. Try fetch('/assets/appsettings.json')                   │
│     │                                                        │
│     ├─► Success ────────────────────┐                       │
│     │                                │                       │
│     └─► Fail                         │                       │
│         │                            │                       │
│         2. Use hardcoded defaults    │                       │
│            │                         │                       │
└────────────┼─────────────────────────┼───────────────────────┘
             │                         │
             │                         │
             ▼                         ▼
            ┌───────────────────────────────────────────┐
            │  AppSettings.initialize(config)           │
            │  (app-settings.ts)                        │
            │                                            │
            │  Sets static properties:                  │
            │  - _sdmsB2CWebAppUrl                      │
            │  - _sdmsAuthenticationWebAppUrl           │
            │  - _sdmsAuthenticationWebAppClientId      │
            │  - etc.                                   │
            └──────────────┬────────────────────────────┘
                           │
                           │ Configuration ready
                           │
                           ▼
            ┌───────────────────────────────────────────┐
            │  platformBrowserDynamic().bootstrapModule │
            │  (Angular Bootstrap)                      │
            └───────────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────┐
│                          USAGE PHASE                                     │
└─────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐
│  Services/       │
│  Components      │
│                  │
│  Example:        │
│  auth.service.ts │
└────────┬─────────┘
         │
         │ import { AppSettings } from './config/app-settings';
         │
         │ const apiUrl = AppSettings.SDMS_AuthenticationWebApp_url;
         │ const clientId = AppSettings.SDMS_AuthenticationWebApp_clientid;
         │
         ▼
┌──────────────────┐
│  AppSettings     │  Static class with initialized values
│  (app-settings.ts)│
│                  │
│  Properties:     │
│  - SDMS_B2CWebApp_url
│  - SDMS_AuthenticationWebApp_url
│  - SDMS_AuthenticationWebApp_clientid
│  - etc.
└──────────────────┘
```

---

## Detailed Configuration Priority Chain

### Build-Time Priority (in CI/CD/Vercel - inline update)

```
Priority Order (Highest to Lowest):
┌─────────────────────────────────────────────────────────────┐
│ 1. Environment Variables (process.env.SDMS_*)              │
│    - Set by CI/CD/Vercel from GitHub Secrets              │
│    - Available during build time                           │
│                                                             │
│ 2. src/assets/appsettings.json (existing file in source)   │
│    - Template file with default values                     │
│    - Updated in place by CI/CD with env vars               │
└─────────────────────────────────────────────────────────────┘
```

### Runtime Priority (in `loadAppSettingsBeforeBootstrap`)

```
Priority Order (Highest to Lowest):
┌─────────────────────────────────────────────────────────────┐
│ 1. /assets/appsettings.json                                 │
│    - Updated at build time from environment variables       │
│    - File exists in source, updated in place by CI/CD       │
│    - Contains values from process.env.SDMS_* (overrides)    │
│                                                             │
│ 2. Hardcoded defaults (in loadAppSettingsBeforeBootstrap)   │
│    - Used if appsettings.json not found                     │
│    - Same defaults as src/assets/appsettings.json           │
└─────────────────────────────────────────────────────────────┘
```

---

## Configuration Keys (SDMS_* Naming Convention)

All configuration keys follow the `SDMS_*` naming convention:

- `SDMS_B2CWebApp_url` - B2C WebApp URL
- `SDMS_AuthenticationWebApp_url` - Authentication server URL
- `SDMS_AuthenticationWebApp_clientid` - OAuth client ID
- `SDMS_AuthenticationWebApp_redirectUri` - OAuth redirect URI
- `SDMS_AuthenticationWebApp_scope` - OAuth scope

---

## Step-by-Step Process

### Phase 1: Build-Time (CI/CD or Local Build)

1. **GitHub Actions Workflow** (`.github/workflows/deploy-b2c-vercel.yml`)
   - Sets environment variables from GitHub Secrets:
     ```yaml
     env:
       SDMS_B2CWebApp_url: ${{ secrets.SDMS_B2CWebApp_url }}
       SDMS_AuthenticationWebApp_url: ${{ secrets.SDMS_AuthenticationWebApp_url }}
       # ... etc
     ```

2. **Update appsettings.json** (CI/CD/Vercel)
   - CI/CD/Vercel reads `process.env.SDMS_*` (from GitHub secrets/Vercel env vars)
   - Updates `src/assets/appsettings.json` in place (file already exists in source)
   - Uses inline node command (no separate script file needed)
   - File values are overridden by env vars (env vars take precedence)

3. **Angular Build**
   - Compiles Angular application
   - Copies `src/assets/appsettings.json` → `dist/sdms-b2c-webapp/assets/appsettings.json`

### Phase 2: Runtime (Browser)

1. **HTML Loads**
   - Browser loads `index.html`
   - Triggers `DOMContentLoaded` event

2. **Main.ts Execution**
   - `main.ts` listens for `DOMContentLoaded`
   - Calls `loadAppSettingsBeforeBootstrap()`
   - Waits for configuration to load (async/await)

3. **Configuration Loading** (`loadAppSettingsBeforeBootstrap`)
   - Attempts to fetch `/assets/appsettings.json`
   - If fails, tries `/appsettings.json`
   - If both fail, uses hardcoded defaults
   - Calls `AppSettings.initialize(config)` with loaded values

4. **AppSettings Initialization**
   - Sets static private properties in `AppSettings` class
   - Configuration is now available throughout the application

5. **Angular Bootstrap**
   - After configuration is loaded, Angular bootstraps
   - Services and components can now use `AppSettings`

### Phase 3: Usage (Application Code)

1. **Service/Component**
   ```typescript
   import { AppSettings } from './config/app-settings';
   
   // Access configuration
   const apiUrl = AppSettings.SDMS_AuthenticationWebApp_url;
   const clientId = AppSettings.SDMS_AuthenticationWebApp_clientid;
   ```

2. **AppSettings Class**
   - Returns initialized static property values
   - Values are set during bootstrap phase
   - Available immediately (no async needed)

---

## File Structure

```
SDMSApps/SDMS.B2CWebApp/
├── ClientApp/
│   ├── src/
│   │   ├── assets/
│   │   │   └── appsettings.json              # Template file (exists in source, updated at build)
│   │   ├── environments/
│   │   │   └── environment.ts                # Single environment file (production flag only)
│   │   ├── app/
│   │   │   ├── config/
│   │   │   │   └── app-settings.ts           # AppSettings class
│   │   │   └── services/
│   │   │       └── app-settings-loader.service.ts  # Loader service
│   │   └── main.ts                           # Bootstrap entry point
│   └── dist/sdms-b2c-webapp/
│       └── assets/
│           └── appsettings.json              # Deployed (runtime)
└── .github/workflows/
    └── deploy-b2c-vercel.yml                 # CI/CD workflow
```

---

## Example: Complete Flow

### Scenario: Production Deployment

1. **Developer pushes to `release` branch**
   - Triggers GitHub Actions workflow

2. **GitHub Actions:**
   ```yaml
   env:
     SDMS_AuthenticationWebApp_url: ${{ secrets.SDMS_AuthenticationWebApp_url }}
     # ... other secrets
   run: npm run build:prod
   ```

3. **Update appsettings.json** (CI/CD/Vercel):
   ```bash
   # CI/CD reads process.env and updates existing file
   node -e "
   const fs = require('fs');
   const existing = JSON.parse(fs.readFileSync('src/assets/appsettings.json', 'utf8'));
   const updated = {
     SDMS_B2CWebApp_url: process.env.SDMS_B2CWebApp_url || existing.SDMS_B2CWebApp_url,
     // ... etc (env vars override file values)
   };
   fs.writeFileSync('src/assets/appsettings.json', JSON.stringify(updated, null, 2));
   "
   ```

4. **Angular Build:**
   - Compiles application
   - Copies `src/assets/appsettings.json` to `dist/`

5. **Deployment:**
   - Vercel deploys `dist/` folder
   - `assets/appsettings.json` is served at `/assets/appsettings.json`

6. **Browser (User):**
   - Loads `index.html`
   - `main.ts` executes
   - Fetches `/assets/appsettings.json`
   - Initializes `AppSettings` with production values
   - Bootstraps Angular
   - Application uses production configuration

---

## Key Benefits

1. **Environment-Specific Configuration**
   - Different values for dev/staging/production
   - No code changes needed between environments

2. **Security**
   - Sensitive values stored in GitHub Secrets
   - Not committed to repository
   - Injected at build-time

3. **Single Source of Truth**
   - `AppSettings` class provides consistent access
   - No need to pass configuration through dependency injection
   - Available immediately (synchronous access)

4. **Fallback Chain**
   - Multiple fallback levels ensure application always has configuration
   - Graceful degradation if files are missing

5. **Build-Time Optimization**
   - Configuration is baked into the build
   - No runtime environment variable lookup needed
   - Faster application startup

---

## Troubleshooting

### Configuration Not Loading

1. **Check environment variables**
   - Verify environment variables are set in GitHub Secrets
   - Check Vercel dashboard for environment variables
   - Verify `process.env.SDMS_*` are available during build

2. **Check appsettings.json update** (CI/CD)
   - Look for "Updated appsettings.json" message in CI/CD logs
   - Verify `src/assets/appsettings.json` is updated with env vars
   - Check file contains correct values (env vars should override defaults)

3. **Check browser console**
   - Look for "AppSettings loaded from /assets/appsettings.json" message
   - Check for fetch errors
   - Verify AppSettings values are correct

4. **Verify appsettings.json exists**
   - Check `dist/assets/appsettings.json` after build
   - Verify file is being served correctly

5. **Check GitHub Secrets**
   - Verify secrets are set in GitHub repository
   - Verify secret names match workflow file

### Using Default Values

If you see default values (localhost:4200, etc.), it means:
- Environment variables were not set during build, OR
- appsettings.json files were not found at runtime

Check the priority chain above to determine which source should be used.

---

## Summary

The configuration system uses a **simple 3-step approach**:

1. **CI/CD/Vercel**: Reads GitHub secrets → Sets environment variables → Updates `src/assets/appsettings.json` (file exists in source, updated in place)
2. **Build**: Angular build copies `src/assets/appsettings.json` → `dist/assets/appsettings.json`
3. **Runtime**: `loadAppSettingsBeforeBootstrap()` reads `/assets/appsettings.json` → `AppSettings.initialize()`
4. **Usage**: Services/Components → `AppSettings` static properties

**Key Benefits**: 
- No separate script files needed (inline commands in CI/CD)
- No separate environment files (single environment.ts for production flag only)
- File exists in source (easy to see defaults)
- Simple: CI/CD updates files → Build → Runtime reads file

**Simplified Architecture**:
- `environment.ts` - Single file, only contains `production` flag (for Angular optimizations)
- `appsettings.json` - Contains all application configuration (API URLs, etc.)
- No file replacements needed - files updated at build time by CI/CD
- Configuration loaded from `appsettings.json` at runtime via `AppSettings` class

This ensures configuration is available immediately when the application starts, with proper fallback mechanisms and environment-specific values.

