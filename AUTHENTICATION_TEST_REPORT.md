# Authentication Test Report - B2C App
**Date:** 2025-01-13  
**Tester:** Auto (AI Assistant)  
**Environment:** Local Development

## Executive Summary

Comprehensive testing of authentication scenarios in the B2C application revealed several issues preventing complete end-to-end testing. The authentication server is running correctly, but the B2C app failed to start, preventing full scenario testing.

## Application Status

### ✅ Authentication Server (SDMS.AuthenticationWebApp)
- **Status:** ✅ RUNNING
- **URL:** https://localhost:7001
- **HTTP URL:** http://localhost:5000
- **Process ID:** 8812
- **OpenID Configuration:** ✅ Accessible and valid
- **Endpoints Verified:**
  - `/well-known/openid-configuration` - ✅ Working
  - Authorization endpoint: `https://localhost:7001/connect/authorize`
  - Token endpoint: `https://localhost:7001/connect/token`
  - UserInfo endpoint: `https://localhost:7001/connect/userinfo`
  - Logout endpoint: `https://localhost:7001/connect/logout`

### ❌ B2C App (SDMS.B2CWebApp)
- **Status:** ❌ NOT RUNNING
- **Expected URL:** http://localhost:4200
- **Issue:** Application failed to start
- **Root Cause:** PowerShell execution policy preventing npm commands
- **Attempted Solutions:**
  1. Started via PowerShell background process - Failed
  2. Started via cmd.exe - Status unknown (may be compiling)

## Test Scenarios

### Scenario 1: Direct Access to Protected Route Without Authentication
**Status:** ❌ NOT TESTED (B2C app not running)

**Expected Behavior:**
1. User navigates to `/test` route
2. `AuthorizeGuard` checks authentication status
3. User is redirected to `/login?returnUrl=/test`
4. Login component initiates OAuth flow

**Code Path:**
- Route: `/test` (protected by `AuthorizeGuard`)
- Guard: `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/auth/authorize.guard.ts`
- Service: `AuthorizeService.isAuthenticated()`
- Redirect: `Router.navigate(['/login'], { queryParams: { returnUrl: state.url } })`

**Issues Found:**
- Cannot verify guard behavior without running app

---

### Scenario 2: Login Flow Initiation from B2C App
**Status:** ❌ NOT TESTED (B2C app not running)

**Expected Behavior:**
1. User clicks "Login" button or navigates to `/login`
2. `LoginComponent.ngOnInit()` is called
3. `LoginComponent.login()` calls `AuthorizeService.signIn()`
4. OAuth service redirects to: `https://localhost:7001/connect/authorize?client_id=sdms_frontend&redirect_uri=http://localhost:4200/auth-callback&response_type=code&scope=openid profile email roles api offline_access&state=...&code_challenge=...&code_challenge_method=S256`

**Code Path:**
- Component: `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/auth/login/login.component.ts`
- Service: `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/auth/authorize.service.ts`
- OAuth Config:
  - Issuer: `https://localhost:7001/`
  - Client ID: `sdms_frontend`
  - Redirect URI: `http://localhost:4200/auth-callback`
  - Scope: `openid profile email roles api offline_access`

**Issues Found:**
- Cannot verify OAuth redirect without running app
- Need to verify PKCE (code challenge) is being generated correctly

---

### Scenario 3: OAuth Authorization Code Flow
**Status:** ⚠️ PARTIALLY TESTED

**Expected Behavior:**
1. User is redirected to auth server authorization endpoint
2. User sees login page at `https://localhost:7001/login`
3. User enters credentials: `adminx@sdms.com` / `adminx@123`
4. Auth server validates credentials
5. Auth server redirects back to: `http://localhost:4200/auth-callback?code=...&state=...&iss=https://localhost:7001/`

**Test Results:**
- ✅ Auth server login page accessible
- ✅ Login form present with email/password fields
- ✅ External auth buttons present (Auth0, Google)
- ❌ Cannot test full flow without B2C app callback handler

**Issues Found:**
- Need to verify callback URL matches configured redirect URI
- Need to verify state parameter validation
- Need to verify code exchange for tokens

---

### Scenario 4: OAuth Callback Processing
**Status:** ❌ NOT TESTED (B2C app not running)

**Expected Behavior:**
1. B2C app receives callback at `/auth-callback?code=...&state=...`
2. `LoginComponent.processLoginCallback()` is called
3. `AuthorizeService.completeSignIn()` exchanges code for tokens
4. Tokens stored in sessionStorage
5. User redirected to original route (`/test`)

**Code Path:**
- Route: `/auth-callback` (handled by `LoginComponent`)
- Service: `AuthorizeService.completeSignIn(url, callbackAction)`
- OAuth Service: `OAuthService.tryLoginCodeFlow()`

**Issues Found:**
- Cannot verify token exchange
- Cannot verify token storage
- Cannot verify redirect after successful login

---

### Scenario 5: Successful Authentication and Access to Protected Route
**Status:** ❌ NOT TESTED (B2C app not running)

**Expected Behavior:**
1. User successfully authenticated
2. Tokens stored in sessionStorage
3. User redirected to `/test`
4. `AuthorizeGuard` checks `AuthorizeService.isAuthenticated()`
5. Guard returns `true`, allowing access
6. Test component displays username: `adminx@sdms.com`

**Code Path:**
- Guard: `AuthorizeGuard.canActivate()`
- Service: `AuthorizeService.isAuthenticated()` checks OAuth service
- Component: `TestComponent` displays user info

**Issues Found:**
- Cannot verify guard allows access
- Cannot verify user info display
- Cannot verify API calls with Bearer token

---

### Scenario 6: UserInfo API Call
**Status:** ⚠️ PARTIALLY TESTED

**Expected Behavior:**
1. After authentication, B2C app calls `https://localhost:7001/account/userinfo`
2. Request includes `Authorization: Bearer <access_token>` header
3. Auth server validates token
4. Returns user information JSON

**Test Results:**
- ✅ Auth server endpoint exists: `/account/userinfo`
- ✅ Endpoint configured with `[Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]`
- ❌ Cannot test with actual Bearer token without B2C app

**Previous Issues (From Code Review):**
- ❌ **FIXED:** Endpoint was returning 404 - Fixed by excluding `/account` from SPA fallback
- ❌ **FIXED:** Endpoint was returning 401 - Fixed by adding explicit authentication scheme

**Current Status:**
- Endpoint should work correctly with proper Bearer token

---

### Scenario 7: Logout Flow
**Status:** ❌ NOT TESTED (B2C app not running)

**Expected Behavior:**
1. User clicks "Logout" button in test component
2. `TestComponent.logout()` calls `AuthorizeService.signOut()`
3. OAuth service clears tokens from sessionStorage
4. OAuth service redirects to: `https://localhost:7001/connect/logout?post_logout_redirect_uri=http://localhost:4200/&id_token_hint=...`
5. Auth server processes logout
6. User redirected back to B2C app home page
7. User navigated to `/login` page

**Code Path:**
- Component: `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/Components/test/test.component.ts`
- Method: `logout()`
- Service: `AuthorizeService.signOut({ returnUrl: '/' })`

**Issues Found:**
- Cannot verify token clearing
- Cannot verify logout redirect
- Cannot verify post-logout redirect URI

---

### Scenario 8: Token Refresh
**Status:** ❌ NOT TESTED (B2C app not running)

**Expected Behavior:**
1. Access token expires (typically 1 hour)
2. OAuth service automatically refreshes using refresh token
3. New access token obtained
4. User session continues without interruption

**Configuration:**
- Silent refresh enabled: `useSilentRefresh: true`
- Silent refresh URI: `window.location.origin + '/silent-refresh.html'`
- Timeout factor: `0.75` (refresh at 75% of token lifetime)

**Issues Found:**
- Cannot verify automatic refresh
- Need to verify refresh token is stored correctly
- Need to verify refresh endpoint works

---

### Scenario 9: Session Persistence
**Status:** ❌ NOT TESTED (B2C app not running)

**Expected Behavior:**
1. User authenticates and closes browser
2. User reopens browser and navigates to `/test`
3. If refresh token valid, user automatically authenticated
4. If refresh token expired, user redirected to login

**Issues Found:**
- Cannot verify session persistence
- Need to verify token storage mechanism (sessionStorage vs localStorage)

---

### Scenario 10: Unauthorized API Access
**Status:** ⚠️ PARTIALLY TESTED

**Expected Behavior:**
1. User makes API call without token or with invalid token
2. Auth server returns 401 Unauthorized
3. B2C app handles error gracefully

**Test Results:**
- ✅ Auth server returns 401 for `/account/userinfo` without token
- ❌ Cannot test B2C app error handling

---

### Scenario 11: Invalid/Expired Token Handling
**Status:** ❌ NOT TESTED (B2C app not running)

**Expected Behavior:**
1. User has expired access token
2. API call made with expired token
3. Auth server returns 401
4. B2C app attempts token refresh
5. If refresh succeeds, retry API call
6. If refresh fails, redirect to login

**Issues Found:**
- Cannot verify error handling
- Cannot verify automatic retry logic

---

### Scenario 12: CORS Configuration
**Status:** ⚠️ PARTIALLY VERIFIED

**Expected Behavior:**
1. B2C app (http://localhost:4200) can make requests to auth server (https://localhost:7001)
2. CORS headers allow requests from B2C app origin

**Code Review:**
- CORS configured in `Program.cs` to allow `http://localhost:4200`
- Need to verify in actual requests

**Issues Found:**
- Cannot verify CORS headers without B2C app making requests

---

## Critical Issues Found

### 1. B2C App Not Starting
**Severity:** 🔴 CRITICAL  
**Impact:** Prevents all end-to-end testing

**Root Cause:**
- PowerShell execution policy blocking npm commands
- Application may need manual start or policy change

**Recommendation:**
1. Change PowerShell execution policy: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`
2. Or start app manually: `cd SDMSApps\SDMS.B2CWebApp\ClientApp && npm start`
3. Or use cmd.exe: `cmd /c "cd /d F:\wp\2\SDMS\SDMSApps\SDMS.B2CWebApp\ClientApp && npm start"`

### 2. Cannot Verify OAuth Flow End-to-End
**Severity:** 🟡 HIGH  
**Impact:** Cannot confirm complete authentication flow works

**Missing Verification:**
- OAuth authorization code flow initiation
- Callback processing and token exchange
- Token storage and retrieval
- Automatic token refresh
- Logout flow

### 3. Cannot Verify Route Protection
**Severity:** 🟡 HIGH  
**Impact:** Cannot confirm protected routes work correctly

**Missing Verification:**
- `AuthorizeGuard` redirect behavior
- Route protection after logout
- Route access after successful login

---

## Configuration Verification

### Authentication Server Configuration
✅ **Verified:**
- OpenID Connect discovery document accessible
- All required endpoints present
- PKCE support enabled
- Authorization code flow supported
- Refresh token flow supported

### B2C App Configuration
⚠️ **From Code Review:**
- OAuth service configured correctly
- Redirect URI: `http://localhost:4200/auth-callback`
- Client ID: `sdms_frontend`
- Scope: `openid profile email roles api offline_access`
- Silent refresh enabled

**Need to Verify:**
- Actual OAuth service initialization
- Discovery document loading
- Token storage mechanism

---

## Test Credentials

**Username:** `adminx@sdms.com`  
**Password:** `adminx@123`

**Note:** These credentials are different from the default `admin@sdms.com` / `Admin@123`

---

## Recommendations

### Immediate Actions Required:
1. **Fix B2C App Startup Issue**
   - Resolve PowerShell execution policy
   - Verify npm dependencies installed
   - Check for compilation errors

2. **Complete End-to-End Testing**
   - Test all scenarios once B2C app is running
   - Verify OAuth flow works correctly
   - Test error handling

3. **Add Integration Tests**
   - Automated tests for authentication flow
   - Tests for token refresh
   - Tests for error scenarios

### Code Improvements:
1. **Error Handling**
   - Add better error messages for failed authentication
   - Handle network errors gracefully
   - Provide user feedback for all error states

2. **Logging**
   - Add comprehensive logging for authentication flow
   - Log OAuth errors with details
   - Log token refresh attempts

3. **Testing**
   - Add unit tests for `AuthorizeService`
   - Add unit tests for `AuthorizeGuard`
   - Add E2E tests for authentication flow

---

## Next Steps

1. Resolve B2C app startup issue
2. Re-run all test scenarios
3. Document any additional issues found
4. Fix identified issues
5. Re-test after fixes

---

## Appendix: Code References

### Key Files:
- `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/auth/authorize.service.ts` - OAuth service
- `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/auth/authorize.guard.ts` - Route guard
- `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/auth/login/login.component.ts` - Login component
- `SDMSApps/SDMS.B2CWebApp/ClientApp/src/app/Components/test/test.component.ts` - Test component
- `SDMSApps/SDMS.AuthenticationWebApp/Program.cs` - Auth server configuration
- `SDMSApps/SDMS.AuthenticationWebApp/Controllers/AccountController.cs` - UserInfo endpoint

### Configuration Files:
- `SDMSApps/SDMS.B2CWebApp/ClientApp/src/assets/appsettings.json` - B2C app settings
- `SDMSApps/SDMS.AuthenticationWebApp/appsettings.json` - Auth server settings

