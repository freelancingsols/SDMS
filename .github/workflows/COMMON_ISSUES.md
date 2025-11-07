# Common GitHub Actions Workflow Issues

## Issue 1: Missing Dependencies in angular.json

**Error:** `Cannot find module '@angular/material'` or `Cannot find module 'font-awesome'`

**Cause:** `angular.json` references packages that don't exist in `package.json`

**Solution:** Already fixed - removed references to:
- `@angular/material` (not in package.json)
- `font-awesome` (not in package.json)  
- `oidc-client` (replaced with `angular-oauth2-oidc`)

## Issue 2: Lint Builder Error

**Error:** `Builder '@angular-devkit/build-angular:tslint' is not found`

**Cause:** Angular 18 doesn't use tslint, but the angular.json still references it

**Solution:** The lint step in workflows uses `continue-on-error: true`, so it won't fail the build. However, if you want to fix it:

1. Install ESLint (if not already):
   ```bash
   ng add @angular-eslint/schematics
   ```

2. Or update angular.json lint builder to use ESLint

**Current Status:** Workflows will skip lint if it fails (non-blocking)

## Issue 3: Build Output Path Mismatch

**Error:** Artifacts not found at expected path

**Cause:** `angular.json` output path was `dist/browser`, but workflows expect `dist/sdms-b2c-webapp`

**Solution:** Already fixed - updated `outputPath` to `dist/sdms-b2c-webapp`

## Issue 4: Missing build-env.js

**Error:** `Cannot find module 'build-env.js'`

**Solution:** The file exists. If it fails, check:
- File exists at: `SDMSApps/SDMS.B2CWebApp/ClientApp/build-env.js`
- File has execute permissions
- Node.js can read the file

## Issue 5: Test Script Issues

**Error:** `npm ERR! missing script: test:ci`

**Current Status:** The script exists in package.json. If it fails:
- Check `karma.conf.ci.js` exists
- Verify ChromeHeadless is available in CI

## Issue 6: Angular 18 Configuration Issues

**Error:** Build fails with Angular 18 specific errors

**Potential Issues:**
- `angular.json` still uses Angular 8 format
- Missing polyfills configuration
- Builder compatibility

**Fixes Applied:**
- ✅ Removed non-existent package references
- ✅ Updated output path
- ✅ Fixed polyfills import path

## How to Share Error Details

To get help, please share:

1. **Workflow Run URL** from GitHub Actions
2. **Failed Step Name** (e.g., "Build Angular app (Production)")
3. **Error Message** (copy from logs)
4. **Branch Name** where it failed

## Quick Fixes to Try

### Fix 1: Update angular.json for Angular 18

If build fails, try updating angular.json to use Angular 18 format:

```json
{
  "projects": {
    "WebApp": {
      "architect": {
        "build": {
          "builder": "@angular-devkit/build-angular:application",
          "options": {
            "outputPath": "dist/sdms-b2c-webapp",
            "index": "src/index.html",
            "browser": "src/main.ts"
          }
        }
      }
    }
  }
}
```

### Fix 2: Remove Polyfills Reference

If polyfills error occurs, update angular.json:

```json
// Remove "polyfills" from build options
// Angular 18 doesn't require separate polyfills file
```

### Fix 3: Update Lint Configuration

If lint fails, either:
- Remove lint step temporarily
- Or install ESLint: `ng add @angular-eslint/schematics`

## Testing Workflows Locally

You can test workflows using `act` (GitHub Actions local runner):

```bash
# Install act
# Windows: choco install act-cli
# Or download from: https://github.com/nektos/act

# List workflows
act -l

# Run specific workflow
act -W .github/workflows/ci-b2c-webapp.yml
```

## Next Steps

1. **Check GitHub Actions logs** for specific error messages
2. **Share the error** so we can provide targeted fixes
3. **Test locally** to reproduce the issue
4. **Review this guide** for common solutions

