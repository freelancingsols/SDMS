# Derived URLs from Parent URLs - Complete List

This document lists all places in the codebase where URLs are generated/derived from parent URL configuration values.

---

## 1. B2C WebApp - AppSettings (TypeScript)

**File:** `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/config/app-settings.ts`

**Line:** 64-73

**Derived URL:**
```typescript
static get SDMS_AuthenticationWebApp_redirectUri(): string {
  // Generate from B2CWebApp_url if not explicitly set
  if (!this._sdmsAuthenticationWebAppRedirectUri) {
    if (!this._sdmsB2CWebAppUrl) {
      throw new Error('SDMS_B2CWebApp_url is not configured...');
    }
    // Generate redirectUri from B2CWebApp_url
    return `${this._sdmsB2CWebAppUrl}/auth-callback`;
  }
  return this._sdmsAuthenticationWebAppRedirectUri;
}
```

**Parent URL:** `SDMS_B2CWebApp_url`  
**Derived URL:** `{SDMS_B2CWebApp_url}/auth-callback`  
**Purpose:** OAuth redirect URI for authentication callback  
**Status:** ✅ Code generation (optional - can be overridden)

---

## 2. Auth WebApp - Program.cs (C#)

**File:** `SDMSApps/SDMS.AuthenticationWebApp/Program.cs`

**Lines:** 475-476

**Derived URLs:**
```csharp
// Add B2C URL redirect URIs (required)
if (!string.IsNullOrEmpty(b2cUrlForClient))
{
    defaultRedirectUris.Add(new Uri($"{b2cUrlForClient}/auth-callback"));
    defaultPostLogoutRedirectUris.Add(new Uri($"{b2cUrlForClient}/"));
}
```

**Parent URL:** `SDMS_B2CWebApp_url` (from `b2cUrlForClient`)  
**Derived URLs:**
- `{SDMS_B2CWebApp_url}/auth-callback` - OAuth redirect URI
- `{SDMS_B2CWebApp_url}/` - Post-logout redirect URI

**Purpose:** Default redirect URIs for OpenIddict client configuration  
**Status:** ✅ Code generation (used as defaults if not explicitly configured)

---

## 3. Auth WebApp ClientApp - auth.service.ts (TypeScript)

**File:** `SDMSApps/SDMS.AuthenticationWebApp/ClientApp/src/app/services/auth.service.ts`

**Lines:** 44, 74

**Derived URLs:**
```typescript
// Line 44
redirectUri: window.location.origin + '/auth-callback',

// Line 74
`redirect_uri=${encodeURIComponent(window.location.origin + '/auth-callback')}&`
```

**Parent URL:** `window.location.origin` (runtime browser origin)  
**Derived URL:** `{window.location.origin}/auth-callback`  
**Purpose:** OAuth redirect URI for authentication callback  
**Status:** ✅ Runtime generation (uses current browser origin)

---

## 4. Auth WebApp ClientApp - auth.service.ts (TypeScript)

**File:** `SDMSApps/SDMS.AuthenticationWebApp/ClientApp/src/app/services/auth.service.ts`

**Line:** 70

**Derived URL:**
```typescript
const authUrl = `${AppSettings.SDMS_AuthenticationWebApp_url}/connect/authorize?` +
  `client_id=${AppSettings.SDMS_AuthenticationWebApp_clientid}&` +
  `response_type=code&` +
  `scope=openid profile email roles&` +
  `redirect_uri=${encodeURIComponent(window.location.origin + '/auth-callback')}&` +
  `state=${provider}`;
```

**Parent URL:** `AppSettings.SDMS_AuthenticationWebApp_url`  
**Derived URL:** `{SDMS_AuthenticationWebApp_url}/connect/authorize`  
**Purpose:** OAuth authorization endpoint  
**Status:** ✅ Code generation (API endpoint construction)

---

## 5. B2C WebApp - auth.service.ts (TypeScript)

**File:** `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/services/auth.service.ts`

**Line:** 53

**Derived URL:**
```typescript
silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
```

**Parent URL:** `window.location.origin` (runtime browser origin)  
**Derived URL:** `{window.location.origin}/silent-refresh.html`  
**Purpose:** Silent refresh callback URI for token renewal  
**Status:** ✅ Runtime generation (uses current browser origin)

---

## 6. B2C WebApp - authorize.service.ts (TypeScript)

**File:** `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/auth/authorize.service.ts`

**Line:** 80

**Derived URL:**
```typescript
silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
```

**Parent URL:** `window.location.origin` (runtime browser origin)  
**Derived URL:** `{window.location.origin}/silent-refresh.html`  
**Purpose:** Silent refresh callback URI for token renewal  
**Status:** ✅ Runtime generation (uses current browser origin)

---

## 7. B2C WebApp - authorize.service.ts (TypeScript)

**File:** `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/auth/authorize.service.ts`

**Line:** 737-739

**Derived URL:**
```typescript
const issuerUrl = AppSettings.SDMS_AuthenticationWebApp_url.endsWith('/') 
  ? AppSettings.SDMS_AuthenticationWebApp_url 
  : AppSettings.SDMS_AuthenticationWebApp_url + '/';
```

**Parent URL:** `AppSettings.SDMS_AuthenticationWebApp_url`  
**Derived URL:** `{SDMS_AuthenticationWebApp_url}/` (ensures trailing slash)  
**Purpose:** OAuth issuer URL normalization  
**Status:** ✅ Code generation (URL normalization)

---

## 8. B2C WebApp - login-callback-silent.html (JavaScript)

**File:** `SDMSApps/SDMS.B2CWebApp/ClientApp/src/assets/login-callback-silent.html`

**Line:** 10

**Derived URL:**
```javascript
var silentRedirectUri = window.location.origin + '/assets/login-callback-silent.html';
```

**Parent URL:** `window.location.origin` (runtime browser origin)  
**Derived URL:** `{window.location.origin}/assets/login-callback-silent.html`  
**Purpose:** Silent callback URI for OIDC client  
**Status:** ✅ Runtime generation (uses current browser origin)

---

## 9. Auth WebApp ClientApp - auth.service.ts (TypeScript)

**File:** `SDMSApps/SDMS.AuthenticationWebApp/ClientApp/src/app/services/auth.service.ts`

**Line:** 38-40

**Derived URL:**
```typescript
if (!issuerUrl.endsWith('/')) {
  issuerUrl = issuerUrl + '/';
}
```

**Parent URL:** `AppSettings.SDMS_AuthenticationWebApp_url` (from `issuerUrl`)  
**Derived URL:** `{SDMS_AuthenticationWebApp_url}/` (ensures trailing slash)  
**Purpose:** OAuth issuer URL normalization  
**Status:** ✅ Code generation (URL normalization)

---

## Summary Table

| # | File | Parent URL | Derived URL | Type | Status |
|---|------|------------|-------------|------|--------|
| 1 | `B2CWebApp/ClientApp/src/app/config/app-settings.ts` | `SDMS_B2CWebApp_url` | `{B2CWebApp_url}/auth-callback` | Config-based | ✅ Optional override |
| 2 | `AuthenticationWebApp/Program.cs` | `SDMS_B2CWebApp_url` | `{B2CWebApp_url}/auth-callback`<br>`{B2CWebApp_url}/` | Config-based | ✅ Default generation |
| 3 | `AuthenticationWebApp/ClientApp/auth.service.ts` | `window.location.origin` | `{origin}/auth-callback` | Runtime | ✅ Runtime |
| 4 | `AuthenticationWebApp/ClientApp/auth.service.ts` | `SDMS_AuthenticationWebApp_url` | `{url}/connect/authorize` | Config-based | ✅ API endpoint |
| 5 | `B2CWebApp/ClientApp/auth.service.ts` | `window.location.origin` | `{origin}/silent-refresh.html` | Runtime | ✅ Runtime |
| 6 | `B2CWebApp/ClientApp/authorize.service.ts` | `window.location.origin` | `{origin}/silent-refresh.html` | Runtime | ✅ Runtime |
| 7 | `B2CWebApp/ClientApp/authorize.service.ts` | `SDMS_AuthenticationWebApp_url` | `{url}/` (trailing slash) | Config-based | ✅ Normalization |
| 8 | `B2CWebApp/ClientApp/login-callback-silent.html` | `window.location.origin` | `{origin}/assets/login-callback-silent.html` | Runtime | ✅ Runtime |
| 9 | `AuthenticationWebApp/ClientApp/auth.service.ts` | `SDMS_AuthenticationWebApp_url` | `{url}/` (trailing slash) | Config-based | ✅ Normalization |

---

## Categories

### 1. Configuration-Based Generation (From Config Values)
- **#1**: `redirectUri` from `B2CWebApp_url` (optional override)
- **#2**: Redirect URIs from `B2CWebApp_url` (defaults)
- **#4**: Authorization endpoint from `AuthenticationWebApp_url`
- **#7, #9**: URL normalization (trailing slash)

### 2. Runtime Generation (From Browser Origin)
- **#3**: `redirectUri` from `window.location.origin`
- **#5, #6**: `silentRefreshRedirectUri` from `window.location.origin`
- **#8**: Silent callback URI from `window.location.origin`

---

## Notes

1. **Configuration-based generation** (#1, #2) allows optional override - if explicit value is provided, it's used; otherwise, it's generated from parent URL.

2. **Runtime generation** (#3, #5, #6, #8) uses `window.location.origin` to dynamically determine the current deployment URL, making it work across different environments without configuration.

3. **URL normalization** (#7, #9) ensures trailing slashes for OAuth issuer URLs, which is required by some OAuth libraries.

4. **API endpoint construction** (#4) builds OAuth endpoints from base authentication server URL.

---

## Recommendations

All derived URL generation is intentional and serves specific purposes:
- ✅ **Runtime URLs** (from `window.location.origin`) - Correct approach for client-side apps
- ✅ **Optional config generation** (#1) - Allows flexibility while reducing configuration burden
- ✅ **Default generation** (#2) - Provides sensible defaults while allowing explicit override
- ✅ **URL normalization** (#7, #9) - Required for OAuth library compatibility

No changes needed - all URL derivation is appropriate and well-implemented.

