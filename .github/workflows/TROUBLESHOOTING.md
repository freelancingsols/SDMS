# GitHub Actions Workflow Troubleshooting Guide

## Common Issues and Solutions

### Issue 1: Build Script Not Found

**Error:** `npm ERR! missing script: build:prod`

**Solution:** Check if `build:prod` script exists in `package.json`. If not, update the workflow to use the correct script name.

**Fix:** Update workflow to use:
```yaml
run: npm run build -- --configuration production
```

### Issue 2: build-env.js Not Found

**Error:** `node: cannot find module 'build-env.js'`

**Solution:** Ensure `build-env.js` exists in `ClientApp` directory. If missing, create it or remove the step.

**Fix:** Either:
1. Create the `build-env.js` file
2. Or remove the `node build-env.js` step if not needed

### Issue 3: Path Not Found

**Error:** `Error: ENOENT: no such file or directory`

**Solution:** Verify the folder structure matches the workflow paths. The folder should be:
- `SDMSApps/SDMS.B2CWebApp/ClientApp/`
- `SDMSApps/SDMS.AuthenticationWebApp/ClientApp/`

### Issue 4: Package.json Not Found

**Error:** `npm ERR! path /path/to/package.json`

**Solution:** Ensure `package.json` exists in the `ClientApp` directory.

### Issue 5: Angular Build Fails

**Error:** `An unhandled exception occurred`

**Solution:** Check for:
- Missing dependencies in `package.json`
- Angular version compatibility
- TypeScript errors

**Fix:** Run `npm install` locally first to identify issues.

### Issue 6: Test Script Fails

**Error:** `npm ERR! missing script: test:ci`

**Solution:** Ensure `test:ci` script exists in `package.json` or update workflow to use correct script.

### Issue 7: Cache Dependency Path Error

**Error:** `Some specified paths were not resolved`

**Solution:** The `package.json` file must exist before the cache step runs. The workflow should create it if missing.

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
- `build:prod` or `build --configuration production`
- `test:ci` or `test`
- `lint` (optional)

## Quick Fixes

### Fix Missing build:prod Script

If `build:prod` doesn't exist, update the workflow:

```yaml
# Instead of:
run: npm run build:prod

# Use:
run: npm run build -- --configuration production
```

### Fix Missing build-env.js

If `build-env.js` is missing, either:

1. **Create it** (if needed for environment variables)
2. **Remove the step** (if not needed):

```yaml
# Remove this line:
node build-env.js
```

### Fix Test Script

If `test:ci` doesn't exist, update to:

```yaml
run: npm test -- --watch=false --browsers=ChromeHeadless --code-coverage
```

## Workflow Validation

To validate workflow syntax, use:

```bash
# Install act (local GitHub Actions runner)
# Then test workflows locally
act -l
```

## Getting Help

When reporting issues, include:
1. **Error message** from GitHub Actions logs
2. **Failed step** name
3. **Workflow file** name
4. **Branch** where it failed
5. **Local test results** (if applicable)

## Common Error Messages

| Error | Likely Cause | Solution |
|-------|--------------|----------|
| `missing script: build:prod` | Script doesn't exist | Use `npm run build -- --configuration production` |
| `ENOENT: no such file` | Path incorrect | Verify folder structure |
| `cannot find module` | Missing file | Check if file exists |
| `Some specified paths were not resolved` | package.json missing | Ensure package.json exists |
| `npm ERR! code ELIFECYCLE` | Build failed | Check build logs for specific error |

