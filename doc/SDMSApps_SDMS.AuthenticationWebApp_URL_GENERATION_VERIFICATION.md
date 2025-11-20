# AuthenticationWebApp - URL Generation Verification

## ✅ Code Status: UPDATED

### 1. Backend (C#) - Program.cs

**Location:** `SDMSApps/SDMS.AuthenticationWebApp/Program.cs`

#### ✅ URL Generation from Parent URLs:

**Line 262-263:** CORS Configuration
```csharp
var b2cUrl = builder.Configuration["SDMS_B2CWebApp_url"] 
    ?? throw new InvalidOperationException(...);
```
- **Parent URL:** `SDMS_B2CWebApp_url`
- **Usage:** CORS allowed origins
- **Status:** ✅ Updated - Uses `SDMS_B2CWebApp_url` (no FrontendUrl)

**Lines 475-476:** OpenIddict Redirect URIs
```csharp
defaultRedirectUris.Add(new Uri($"{b2cUrlForClient}/auth-callback"));
defaultPostLogoutRedirectUris.Add(new Uri($"{b2cUrlForClient}/"));
```
- **Parent URL:** `SDMS_B2CWebApp_url` (from `b2cUrlForClient`)
- **Derived URLs:**
  - `{SDMS_B2CWebApp_url}/auth-callback` - OAuth redirect URI
  - `{SDMS_B2CWebApp_url}/` - Post-logout redirect URI
- **Status:** ✅ Updated - Generates from `SDMS_B2CWebApp_url`

**Lines 480-485:** Configuration Override
```csharp
var redirectUrisConfig = builder.Configuration[ConfigurationKeys.RedirectUris];
var redirectUris = ParseUrisFromConfig(redirectUrisConfig, defaultRedirectUris);
```
- **Status:** ✅ Updated - Uses generated defaults if config not provided

#### ✅ ExternalAuthService.cs

**Lines 288-292:** External Auth Redirect URI
```csharp
var redirectUri = _configuration["ExternalAuth:RedirectUri"]
    ?? throw new InvalidOperationException(...);
```
- **Status:** ✅ Updated - Requires explicit configuration (no fallback generation)

---

### 2. Frontend (TypeScript) - ClientApp

**Location:** `SDMSApps/SDMS.AuthenticationWebApp/ClientApp/src/app/services/auth.service.ts`

#### ✅ URL Generation from Parent URLs:

**Line 44:** OAuth Redirect URI
```typescript
redirectUri: window.location.origin + '/auth-callback',
```
- **Parent URL:** `window.location.origin` (runtime browser origin)
- **Derived URL:** `{window.location.origin}/auth-callback`
- **Status:** ✅ Updated - Runtime generation (correct approach)

**Line 70:** Authorization Endpoint
```typescript
const authUrl = `${AppSettings.SDMS_AuthenticationWebApp_url}/connect/authorize?` +
```
- **Parent URL:** `AppSettings.SDMS_AuthenticationWebApp_url`
- **Derived URL:** `{SDMS_AuthenticationWebApp_url}/connect/authorize`
- **Status:** ✅ Updated - API endpoint construction

**Line 74:** Redirect URI in Query String
```typescript
`redirect_uri=${encodeURIComponent(window.location.origin + '/auth-callback')}&`
```
- **Parent URL:** `window.location.origin`
- **Derived URL:** `{window.location.origin}/auth-callback`
- **Status:** ✅ Updated - Runtime generation

**Lines 38-40:** Issuer URL Normalization
```typescript
if (!issuerUrl.endsWith('/')) {
  issuerUrl = issuerUrl + '/';
}
```
- **Parent URL:** `AppSettings.SDMS_AuthenticationWebApp_url`
- **Derived URL:** `{SDMS_AuthenticationWebApp_url}/` (trailing slash)
- **Status:** ✅ Updated - URL normalization

---

## ✅ Configuration Status: UPDATED

### appsettings.json

**File:** `SDMSApps/SDMS.AuthenticationWebApp/appsettings.json`

**Status:** ✅ Updated
- ✅ Uses `SDMS_B2CWebApp_url` (line 11)
- ✅ No `SDMS_AuthenticationWebApp_FrontendUrl` (removed)
- ✅ `SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri` explicitly set (line 25)
- ✅ `SDMS_AuthenticationWebApp_RedirectUris` explicitly set (line 28)
- ✅ `SDMS_AuthenticationWebApp_PostLogoutRedirectUris` explicitly set (line 29)

---

## ⚠️ Deployment Documentation Status: NEEDS UPDATE

### RAILWAY_CHECKLIST.md

**File:** `SDMSApps/SDMS.AuthenticationWebApp/RAILWAY_CHECKLIST.md`

**Line 30:** ❌ **FIXED** - Was referencing `SDMS_AuthenticationWebApp_FrontendUrl`
- **Before:** `SDMS_AuthenticationWebApp_FrontendUrl`
- **After:** `SDMS_B2CWebApp_url` ✅

**Status:** ✅ **NOW UPDATED**

---

### GITHUB_SECRETS_SETUP.md

**File:** `SDMSApps/SDMS.AuthenticationWebApp/GITHUB_SECRETS_SETUP.md`

**Status:** ✅ Updated
- ✅ Line 18: Uses `SDMS_B2CWebApp_url`
- ✅ Line 41-42: Documents `SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri`
- ✅ No references to `FrontendUrl`

---

### GITHUB_VARIABLES.txt

**File:** `SDMSApps/GITHUB_VARIABLES.txt`

**Status:** ✅ Updated
- ✅ Line 31: `SDMS_B2CWebApp_url` defined
- ✅ Line 37-38: `SDMS_AuthenticationWebApp_redirectUri` with note about code generation
- ✅ Line 42-44: `SDMS_AuthenticationWebApp_RedirectUris` and `PostLogoutRedirectUris`

---

## Summary of URL Generation

### Backend (C#) - Program.cs

| Parent URL | Derived URL | Purpose | Status |
|------------|-------------|---------|--------|
| `SDMS_B2CWebApp_url` | `{B2CWebApp_url}/auth-callback` | OAuth redirect URI (default) | ✅ Generated in code |
| `SDMS_B2CWebApp_url` | `{B2CWebApp_url}/` | Post-logout redirect URI (default) | ✅ Generated in code |
| `SDMS_B2CWebApp_url` | `{B2CWebApp_url}` | CORS allowed origin | ✅ Used directly |

### Frontend (TypeScript) - ClientApp

| Parent URL | Derived URL | Purpose | Status |
|------------|-------------|---------|--------|
| `window.location.origin` | `{origin}/auth-callback` | OAuth redirect URI | ✅ Runtime generation |
| `SDMS_AuthenticationWebApp_url` | `{url}/connect/authorize` | Authorization endpoint | ✅ API endpoint |
| `SDMS_AuthenticationWebApp_url` | `{url}/` | Issuer URL (normalized) | ✅ URL normalization |

---

## ✅ Verification Checklist

- [x] **Code Updated:** All URL generation uses `SDMS_B2CWebApp_url` (no FrontendUrl)
- [x] **No Fallbacks:** ExternalAuthService requires explicit configuration
- [x] **appsettings.json:** Updated with correct keys
- [x] **RAILWAY_CHECKLIST.md:** ✅ Fixed - Now uses `SDMS_B2CWebApp_url`
- [x] **GITHUB_SECRETS_SETUP.md:** Updated correctly
- [x] **GITHUB_VARIABLES.txt:** Updated correctly
- [x] **Runtime URLs:** Uses `window.location.origin` (correct approach)

---

## ✅ Final Status

**Code:** ✅ **FULLY UPDATED**
- All URL generation from parent URLs is implemented correctly
- No obsolete `FrontendUrl` references in code
- Proper use of `SDMS_B2CWebApp_url` for generation

**Configuration:** ✅ **FULLY UPDATED**
- `appsettings.json` uses correct keys
- No obsolete configuration keys

**Deployment Documentation:** ✅ **FULLY UPDATED**
- All deployment files updated to use `SDMS_B2CWebApp_url`
- No references to obsolete `FrontendUrl`

---

## Generated URLs Summary

### From `SDMS_B2CWebApp_url`:
1. `{SDMS_B2CWebApp_url}/auth-callback` - OAuth redirect URI (default in Program.cs)
2. `{SDMS_B2CWebApp_url}/` - Post-logout redirect URI (default in Program.cs)
3. `{SDMS_B2CWebApp_url}` - CORS allowed origin (used directly)

### From `window.location.origin` (Runtime):
1. `{origin}/auth-callback` - OAuth redirect URI (ClientApp auth.service.ts)

### From `SDMS_AuthenticationWebApp_url`:
1. `{url}/connect/authorize` - Authorization endpoint (ClientApp auth.service.ts)
2. `{url}/` - Issuer URL normalization (ClientApp auth.service.ts)

---

**All URL generation is properly implemented and deployment configuration is updated!** ✅

