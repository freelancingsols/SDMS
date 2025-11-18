# Fix for "localhost:7001" Error in Deployed Version

## Problem
The deployed Angular ClientApp is trying to connect to `https://localhost:7001` instead of the production Railway URL, causing `ERR_CONNECTION_REFUSED` errors.

## Root Cause
The `build-env.js` script runs during the Angular build process and needs environment variables to generate the correct `appsettings.json`. If these variables aren't set in Railway BEFORE the build, it falls back to the localhost values from the root `appsettings.json`.

## Solution

### Step 1: Set Environment Variables in Railway

**CRITICAL:** These must be set BEFORE deploying/redeploying:

1. Go to Railway Dashboard → Your AuthenticationWebApp Service → **Variables** tab
2. Add these variables:

```
SDMS_AuthenticationWebApp_url = https://your-railway-url.railway.app
SDMS_AuthenticationWebApp_clientid = sdms_frontend
```

**Important:** 
- Replace `https://your-railway-url.railway.app` with your actual Railway service URL
- These variables are used DURING the build process, not just at runtime
- If you set them after the build, the app will still have localhost URLs

### Step 2: Verify Variables Are Set

Check that the variables are visible in Railway dashboard → Variables tab.

### Step 3: Redeploy

After setting the variables, trigger a new deployment:
- Railway will automatically redeploy if auto-deploy is enabled
- Or manually trigger a deployment from Railway dashboard

### Step 4: Verify Build Logs

Check the Railway build logs. You should see:
```
📋 Configuration Source:
  SDMS_AuthenticationWebApp_url: ✅ Environment Variable
  SDMS_AuthenticationWebApp_clientid: ✅ Environment Variable
```

If you see:
```
⚠️  WARNING: Using localhost URL in build!
```

Then the environment variables are not set correctly.

## Prevention

The build script now:
1. ✅ **Logs** which source is being used (env var vs fallback)
2. ✅ **Warns** if localhost is detected in production-like environment
3. ✅ **Fails the build** if localhost is detected in production (prevents bad deployments)

## Additional Required Variables

Also ensure these are set in Railway:

- `SDMS_B2CWebApp_url` - Your B2C WebApp URL (for CORS)
- `SDMS_AuthenticationWebApp_RedirectUris` - Redirect URIs
- `SDMS_AuthenticationWebApp_PostLogoutRedirectUris` - Post-logout redirect URIs
- `SDMS_AuthenticationWebApp_ConnectionString` - Database connection (or Railway auto-sets `POSTGRES_CONNECTION`)

## Verification

After redeploying, check the browser console. The app should:
- ✅ Load `appsettings.json` successfully
- ✅ Use the Railway URL (not localhost)
- ✅ Connect to the API successfully

If you still see localhost URLs, the build didn't use the environment variables. Check:
1. Variables are set in Railway BEFORE build
2. Build logs show environment variables are available
3. Redeploy after setting variables

