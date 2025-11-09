# SDMS B2C WebApp - Configuration Loading Flow

This document explains how application configuration is loaded and used in the SDMS B2C WebApp.

## Overview

The configuration system uses a two-phase approach:
1. **Build-time**: Environment variables are injected into `appsettings.json` during the build process
2. **Runtime**: Configuration is loaded from `appsettings.json` before Angular bootstrap and stored in the `AppSettings` static class

---

## Configuration Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          BUILD-TIME PHASE                                │
└─────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐
│  GitHub Secrets  │  (SDMS_AuthenticationWebApp_url, etc.)
│  or Vercel Env   │
└────────┬─────────┘
         │
         │ Environment Variables
         │
         ▼
┌──────────────────┐
│   build-env.js   │  Reads process.env.SDMS_*
│   (Node.js)      │  Reads appsettings.json (template)
└────────┬─────────┘
         │
         │ Generates
         │
         ▼
┌─────────────────────────────────────┐
│  src/assets/appsettings.json        │  (Generated file)
│  {                                  │
│    "SDMS_B2CWebApp_url": "...",    │
│    "SDMS_AuthenticationWebApp_...": │
│  }                                  │
└────────┬────────────────────────────┘
         │
         │ Copied to dist during Angular build
         │
         ▼
┌─────────────────────────────────────┐
│  dist/sdms-b2c-webapp/              │
│    assets/appsettings.json          │  (Deployed file)
└─────────────────────────────────────┘


┌─────────────────────────────────────────────────────────────────────────┐
│                          RUNTIME PHASE                                   │
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
│         2. Try fetch('/appsettings.json')                    │
│            │                                                 │
│            ├─► Success ──────────────┼──────────────────┐   │
│            │                          │                  │   │
│            └─► Fail                   │                  │   │
│                │                      │                  │   │
│                3. Use hardcoded defaults                  │   │
│                   │                    │                  │   │
└───────────────────┼────────────────────┼──────────────────┼───┘
                    │                    │                  │
                    │                    │                  │
                    ▼                    ▼                  ▼
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

### Build-Time Priority (in `build-env.js`)

```
Priority Order (Highest to Lowest):
┌─────────────────────────────────────────────────────────────┐
│ 1. Environment Variables (process.env.SDMS_*)              │
│    - From GitHub Secrets (CI/CD)                           │
│    - From Vercel Environment Variables                     │
│    - From local shell environment                          │
│                                                             │
│ 2. Root appsettings.json values                            │
│    - SDMSApps/SDMS.B2CWebApp/appsettings.json             │
│                                                             │
│ 3. Hardcoded defaults (in build-env.js)                    │
│    - Fallback values for local development                 │
└─────────────────────────────────────────────────────────────┘
```

### Runtime Priority (in `loadAppSettingsBeforeBootstrap`)

```
Priority Order (Highest to Lowest):
┌─────────────────────────────────────────────────────────────┐
│ 1. /assets/appsettings.json                                 │
│    - Generated during build                                 │
│    - Contains values from build-time environment variables  │
│                                                             │
│ 2. /appsettings.json (root)                                 │
│    - Fallback if assets/appsettings.json not found         │
│                                                             │
│ 3. Hardcoded defaults (in AppSettings class)                │
│    - Used if JSON files cannot be loaded                    │
│    - Same defaults as build-env.js                          │
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

2. **Build Script** (`build-env.js`)
   - Runs before Angular build: `node build-env.js`
   - Reads environment variables: `process.env.SDMS_*`
   - Reads template: `appsettings.json` (root)
   - Merges values (environment vars override template)
   - Generates: `src/assets/appsettings.json`

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
├── appsettings.json                          # Template (root)
├── ClientApp/
│   ├── build-env.js                          # Build-time script
│   ├── src/
│   │   ├── assets/
│   │   │   └── appsettings.json              # Generated (build-time)
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
   run: node build-env.js && npm run build:prod
   ```

3. **build-env.js:**
   ```javascript
   // Reads: process.env.SDMS_AuthenticationWebApp_url = "https://prod-auth.example.com"
   // Reads: appsettings.json (template)
   // Generates: src/assets/appsettings.json with production values
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

1. **Check build-env.js output**
   - Look for console logs during build
   - Verify environment variables are set

2. **Check browser console**
   - Look for "AppSettings loaded from..." messages
   - Check for fetch errors

3. **Verify appsettings.json exists**
   - Check `dist/assets/appsettings.json` after build
   - Verify file is being served correctly

4. **Check GitHub Secrets**
   - Verify secrets are set in GitHub repository
   - Verify secret names match workflow file

### Using Default Values

If you see default values (localhost:4200, etc.), it means:
- Environment variables were not set during build, OR
- appsettings.json files were not found at runtime

Check the priority chain above to determine which source should be used.

---

## Summary

The configuration system uses a **build-time injection** and **runtime loading** approach:

1. **Build-time**: Environment variables → `build-env.js` → `assets/appsettings.json`
2. **Runtime**: `main.ts` → `loadAppSettingsBeforeBootstrap()` → `AppSettings.initialize()`
3. **Usage**: Services/Components → `AppSettings` static properties

This ensures configuration is available immediately when the application starts, with proper fallback mechanisms and environment-specific values.

