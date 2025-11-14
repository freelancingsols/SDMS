# Authentication Flow Test Plan

## Prerequisites

1. **Database Connection**: Ensure the database is accessible
   - Connection String: `Host=ep-summer-thunder-a87epsr7-pooler.eastus2.azure.neon.tech;Port=5432;Database=neondb;Username=neondb_owner;Password=npg_9wDBEnMgl4eN;SSL Mode=Require;Channel Binding=Require`
   - Test User: `admin@sdms.com` / `Admin@123`

2. **Start Authentication Server**:
   ```bash
   cd SDMSApps/SDMS.AuthenticationWebApp
   dotnet run --urls "https://localhost:7001;http://localhost:5000"
   ```
   - Should be accessible at: `https://localhost:7001` or `http://localhost:5000`
   - OpenID Discovery: `https://localhost:7001/.well-known/openid-configuration`

3. **Start B2C WebApp**:
   ```bash
   cd SDMSApps/SDMS.B2CWebApp/ClientApp
   npm start
   ```
   - Should be accessible at: `http://localhost:4200`

## Test Cases

### Test Case 1: OpenID Discovery Document Loading
**Objective**: Verify the discovery document loads successfully

**Steps**:
1. Open browser DevTools (F12)
2. Navigate to: `http://localhost:4200`
3. Check Console for:
   - Discovery document load success
   - No CORS errors
   - No "Unknown Error" messages

**Expected Result**:
- ✅ Discovery document loads successfully
- ✅ No CORS errors in console
- ✅ OAuth service configured correctly

**Actual Result**: [TO BE FILLED]

---

### Test Case 2: Initial Page Load (Unauthenticated)
**Objective**: Verify unauthenticated users are redirected to login

**Steps**:
1. Clear browser storage (localStorage, sessionStorage, cookies)
2. Navigate to: `http://localhost:4200/test`
3. Observe redirect behavior

**Expected Result**:
- ✅ User redirected to `/login?returnUrl=/test`
- ✅ Login page displays correctly

**Actual Result**: [TO BE FILLED]

---

### Test Case 3: Login Flow - OAuth Authorization
**Objective**: Verify login redirects to authentication server

**Steps**:
1. Navigate to: `http://localhost:4200/login`
2. Observe redirect
3. Check Network tab for authorization request

**Expected Result**:
- ✅ Redirects to: `https://localhost:7001/connect/authorize?...`
- ✅ Authorization request includes:
   - `client_id=sdms_frontend`
   - `redirect_uri=http://localhost:4200/auth-callback`
   - `response_type=code`
   - `scope=openid profile email roles api offline_access`
   - `code_challenge` (PKCE)

**Actual Result**: [TO BE FILLED]

---

### Test Case 4: Login with Valid Credentials
**Objective**: Verify successful login with admin credentials

**Steps**:
1. Navigate to: `http://localhost:4200/login`
2. Wait for redirect to authentication server
3. Enter credentials:
   - Username: `admin@sdms.com`
   - Password: `Admin@123`
4. Click Login/Submit
5. Observe redirect back to B2C app

**Expected Result**:
- ✅ Redirects to: `http://localhost:4200/auth-callback?code=...&state=...`
- ✅ OAuth callback processes successfully
- ✅ User redirected to `/test` (or returnUrl)
- ✅ User profile loaded
- ✅ Access token stored in localStorage

**Actual Result**: [TO BE FILLED]

---

### Test Case 5: OAuth Callback Processing
**Objective**: Verify callback URL processes authorization code

**Steps**:
1. After login, check Network tab for:
   - Token exchange request to `/connect/token`
   - UserInfo request to `/connect/userinfo`
2. Check Console for:
   - Token received messages
   - User profile loaded messages
3. Check Application tab → Local Storage for tokens

**Expected Result**:
- ✅ Token exchange successful (200 OK)
- ✅ Access token received
- ✅ Refresh token received (if offline_access scope)
- ✅ UserInfo endpoint returns user profile
- ✅ Tokens stored in localStorage

**Actual Result**: [TO BE FILLED]

---

### Test Case 6: User Profile Loading
**Objective**: Verify user profile is loaded after authentication

**Steps**:
1. After successful login
2. Check Console for user profile data
3. Check if username displays in UI (if applicable)

**Expected Result**:
- ✅ User profile loaded: `{ name: "admin@sdms.com" or "Administrator" }`
- ✅ `getUser()` observable emits user data
- ✅ User info displayed in UI

**Actual Result**: [TO BE FILLED]

---

### Test Case 7: Authenticated Route Access
**Objective**: Verify authenticated users can access protected routes

**Steps**:
1. After successful login
2. Navigate to: `http://localhost:4200/test`
3. Verify page loads without redirect

**Expected Result**:
- ✅ Test component loads
- ✅ No redirect to login
- ✅ `AuthorizeGuard` allows access

**Actual Result**: [TO BE FILLED]

---

### Test Case 8: Token Refresh
**Objective**: Verify access token can be refreshed

**Steps**:
1. After successful login
2. Wait for token to expire (or manually trigger refresh)
3. Check Network tab for refresh token request
4. Verify new access token received

**Expected Result**:
- ✅ Refresh token request sent to `/connect/token`
- ✅ New access token received
- ✅ User remains authenticated

**Actual Result**: [TO BE FILLED]

---

### Test Case 9: Logout Flow
**Objective**: Verify logout clears session and redirects correctly

**Steps**:
1. While authenticated, trigger logout
2. Check Network tab for logout request
3. Verify redirect behavior
4. Check localStorage/sessionStorage cleared

**Expected Result**:
- ✅ Logout request sent to `/connect/logout`
- ✅ Tokens cleared from storage
- ✅ User redirected to login page
- ✅ `isAuthenticated()` returns false

**Actual Result**: [TO BE FILLED]

---

### Test Case 10: Invalid Credentials
**Objective**: Verify error handling for invalid credentials

**Steps**:
1. Navigate to: `http://localhost:4200/login`
2. Enter invalid credentials (e.g., wrong password)
3. Submit login form

**Expected Result**:
- ✅ Error message displayed
- ✅ User remains on login page
- ✅ No redirect to callback

**Actual Result**: [TO BE FILLED]

---

### Test Case 11: CORS Configuration
**Objective**: Verify CORS headers are present

**Steps**:
1. Open DevTools → Network tab
2. Make request to: `https://localhost:7001/.well-known/openid-configuration`
3. Check Response Headers

**Expected Result**:
- ✅ `Access-Control-Allow-Origin: http://localhost:4200`
- ✅ `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`
- ✅ `Access-Control-Allow-Headers: Content-Type, Authorization`
- ✅ `Access-Control-Allow-Credentials: true`

**Actual Result**: [TO BE FILLED]

---

## Common Issues and Fixes

### Issue 1: CORS Error
**Symptoms**: `Access to XMLHttpRequest blocked by CORS policy`

**Fix**: 
- Verify `SDMS_AuthenticationWebApp_FrontendUrl` in auth server config
- Check CORS middleware is configured correctly
- Ensure `*.vercel.app` wildcard is working

### Issue 2: Discovery Document Not Loading
**Symptoms**: `Unknown Error` or `Http failure response`

**Fix**:
- Check authentication server is running
- Verify URL in `app-settings.ts` matches server URL
- Check SSL certificate (if using HTTPS)
- Verify retry logic is working

### Issue 3: Login Redirect Loop
**Symptoms**: Continuous redirects between login and callback

**Fix**:
- Check redirect URI matches exactly in both apps
- Verify `code_challenge` and `code_verifier` (PKCE) are working
- Check state parameter is preserved

### Issue 4: Token Not Received
**Symptoms**: Login succeeds but no token in storage

**Fix**:
- Check token endpoint is accessible
- Verify client_id matches
- Check PKCE code verifier matches challenge
- Verify redirect_uri matches exactly

---

## Test Results Summary

| Test Case | Status | Notes |
|-----------|--------|-------|
| TC1: Discovery Document | ⬜ | |
| TC2: Unauthenticated Redirect | ⬜ | |
| TC3: OAuth Authorization | ⬜ | |
| TC4: Valid Login | ⬜ | |
| TC5: Callback Processing | ⬜ | |
| TC6: User Profile | ⬜ | |
| TC7: Authenticated Access | ⬜ | |
| TC8: Token Refresh | ⬜ | |
| TC9: Logout | ⬜ | |
| TC10: Invalid Credentials | ⬜ | |
| TC11: CORS | ⬜ | |

**Legend**: ✅ Pass | ❌ Fail | ⚠️ Partial | ⬜ Not Tested

---

## Next Steps After Testing

1. Document all failures
2. Fix identified issues
3. Re-test failed cases
4. Update this document with results

