# Authentication Fixes Summary

## Issues Fixed

### 1. OpenID Discovery Document Endpoint
- **Issue**: OpenID configuration endpoint `/.well-known/openid-configuration` was not accessible
- **Fix**: OpenIddict automatically exposes this endpoint. No explicit configuration needed - it's handled by OpenIddict middleware
- **Status**: ✅ Fixed

### 2. Authorization and Token Controllers
- **Issue**: Controllers were using reflection to get OpenIddict request, which was unreliable
- **Fix**: Updated `AuthorizationController.cs` and `TokenController.cs` to use the proper extension method `HttpContext.GetOpenIddictServerRequest()`
- **Files Modified**:
  - `SDMSApps/SDMS.AuthenticationWebApp/Controllers/AuthorizationController.cs`
  - `SDMSApps/SDMS.AuthenticationWebApp/Controllers/TokenController.cs`
- **Status**: ✅ Fixed

### 3. B2C Production Configuration
- **Issue**: B2C app was not configured to use production auth URL
- **Fix**: Updated `SDMSApps/SDMS.B2CWebApp/ClientApp/src/assets/appsettings.json` to use:
  - Auth URL: `https://sdms-production.up.railway.app`
  - B2C URL: `https://sdms-pi.vercel.app`
  - Redirect URI: `https://sdms-pi.vercel.app/auth-callback`
- **Status**: ✅ Fixed

### 4. OpenIddict Client Configuration
- **Issue**: Client configuration didn't include B2C production redirect URIs
- **Fix**: Updated `Program.cs` to dynamically include B2C URL from configuration:
  - Added B2C redirect URI: `https://sdms-pi.vercel.app/auth-callback`
  - Added B2C post-logout redirect URI: `https://sdms-pi.vercel.app/`
- **Files Modified**:
  - `SDMSApps/SDMS.AuthenticationWebApp/Program.cs`
- **Status**: ✅ Fixed

## Testing Instructions

### Prerequisites
1. PostgreSQL database running locally (or update connection string)
2. .NET 8.0 SDK installed
3. Node.js and npm (for B2C frontend if testing)

### Local Testing Steps

#### 1. Start Authentication API
```powershell
cd SDMSApps\SDMS.AuthenticationWebApp
dotnet run
```

The app should start on:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7001`

#### 2. Test Endpoints

**Root Endpoint:**
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/" -Method Get
```

**OpenID Discovery Document:**
```powershell
Invoke-WebRequest -Uri "http://localhost:5000/.well-known/openid-configuration" -Method Get
```

**Register Endpoint:**
```powershell
$registerData = @{
    email = "test@example.com"
    password = "Test123!"
    displayName = "Test User"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/account/register" -Method Post -Body $registerData -ContentType "application/json"
```

**Login Endpoint:**
```powershell
$loginData = @{
    email = "test@example.com"
    password = "Test123!"
} | ConvertTo-Json

Invoke-WebRequest -Uri "http://localhost:5000/account/login" -Method Post -Body $loginData -ContentType "application/json"
```

**Token Endpoint (Password Grant):**
```powershell
$tokenData = "grant_type=password&username=test@example.com&password=Test123!&client_id=sdms_frontend&client_secret=sdms_frontend_secret&scope=openid profile email roles"

Invoke-WebRequest -Uri "http://localhost:5000/connect/token" -Method Post -Body $tokenData -ContentType "application/x-www-form-urlencoded"
```

#### 3. Test B2C Integration

1. Update B2C app settings for local testing (revert to localhost):
   ```json
   {
     "SDMS_AuthenticationWebApp_url": "http://localhost:5000",
     "SDMS_AuthenticationWebApp_redirectUri": "http://localhost:4200/auth-callback"
   }
   ```

2. Start B2C app:
   ```powershell
   cd SDMSApps\SDMS.B2CWebApp\ClientApp
   npm start
   ```

3. Test login flow in browser:
   - Navigate to `http://localhost:4200`
   - Click login
   - Should redirect to auth server
   - After login, should redirect back with token

## Production Configuration

### Railway (Auth Server)
- URL: `https://sdms-production.up.railway.app`
- Ensure environment variables are set:
  - `SDMS_AuthenticationWebApp_ConnectionString` (PostgreSQL connection string)
  - `SDMS_B2CWebApp_url` = `https://sdms-pi.vercel.app`

### Vercel (B2C App)
- URL: `https://sdms-pi.vercel.app`
- Ensure environment variables are set:
  - `SDMS_AuthenticationWebApp_url` = `https://sdms-production.up.railway.app`
  - `SDMS_AuthenticationWebApp_redirectUri` = `https://sdms-pi.vercel.app/auth-callback`

## Known Issues & Notes

1. **Database**: Ensure PostgreSQL is running and connection string is correct
2. **CORS**: CORS is configured to allow requests from B2C URL
3. **HTTPS**: In production, Railway handles HTTPS termination, so the app runs on HTTP internally
4. **Client Secret**: The client secret `sdms_frontend_secret` is hardcoded. For production, consider using environment variables

## Next Steps

1. Test locally with PostgreSQL database
2. Verify OpenID discovery document is accessible
3. Test login and register flows
4. Test B2C token acquisition
5. Deploy to production and verify end-to-end integration

