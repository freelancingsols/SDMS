# Authentication Implementation Guide

## Summary

This guide explains the authentication types, flows, and implementations available in the SDMS authentication system.

---

## 🔐 Authentication Types Currently Used

### 1. **Authorization Code Flow with PKCE** (Primary - Current Implementation)
- **Status**: ✅ Currently implemented and active
- **Flow**: User redirects to login page → authenticates → returns with authorization code → exchanges for tokens
- **Security**: ⭐⭐⭐⭐⭐ Highest security (uses PKCE)
- **User Experience**: ⚠️ Requires redirect to login page
- **Use Case**: Recommended for all public-facing SPAs

### 2. **Refresh Token Flow** (Supported)
- **Status**: ✅ Backend supports it, ✅ Frontend configured for automatic refresh
- **Flow**: Uses refresh token to get new access token without user interaction
- **Security**: ⭐⭐⭐⭐ High security (tokens only, no credentials)
- **User Experience**: ✅ Silent, automatic
- **Use Case**: Automatic token renewal (enabled by silent refresh)

### 3. **Password Grant Flow** (Available for Direct Login)
- **Status**: ✅ Backend supports it, ✅ Frontend implementation added
- **Flow**: Direct login with username/password → returns tokens immediately
- **Security**: ⭐⭐⭐ Medium security (credentials in client code)
- **User Experience**: ✅ No redirect, stays on page
- **Use Case**: Internal apps, admin panels, trusted environments

### 4. **Client Credentials Flow** (For APIs)
- **Status**: ✅ Backend supports it
- **Flow**: Machine-to-machine authentication
- **Security**: ⭐⭐⭐⭐ High security (no user context)
- **Use Case**: API-to-API communication, service accounts

---

## 🔄 Silent Token Refresh

### How It Works

1. **Automatic Refresh**: When access token is about to expire (75% of lifetime), the system automatically refreshes it
2. **Uses Refresh Token**: Refresh token is exchanged for a new access token
3. **Hidden Iframe**: Uses a hidden iframe to avoid page reload
4. **Transparent**: User doesn't notice the refresh happening

### Implementation

**Enabled in `auth.service.ts`:**
```typescript
useSilentRefresh: true,
silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
timeoutFactor: 0.75, // Refresh when 75% of lifetime has passed
sessionChecksEnabled: true
```

**Automatic Setup:**
```typescript
this.oauthService.setupAutomaticSilentRefresh();
```

### Requirements

1. ✅ Refresh token must be obtained (from authorization code or password grant)
2. ✅ `offline_access` scope must be requested
3. ✅ `silent-refresh.html` file must exist in assets

### Manual Refresh

You can also manually refresh tokens:
```typescript
await authService.refreshToken();
```

---

## 🚀 Login Without Redirect (Direct Login)

### Method: Password Grant Flow

**New Method Added**: `loginWithEmailDirect(email, password)`

### Usage

```typescript
// In your component
import { AuthService } from './services/auth.service';

constructor(private authService: AuthService) {}

async login() {
  const success = await this.authService.loginWithEmailDirect(
    'user@example.com',
    'password123'
  );
  
  if (success) {
    // User is logged in, tokens are stored
    // No redirect happened!
    this.router.navigate(['/dashboard']);
  }
}
```

### How It Works

1. Sends username/password directly to `/connect/token` endpoint
2. Server validates credentials and returns tokens
3. Tokens are stored in browser storage
4. User profile is loaded automatically
5. **No redirect occurs** - user stays on the same page

### Security Considerations

⚠️ **Important Security Notes:**

1. **Client Secret Exposure**: The client secret is exposed in client code (not ideal for public clients)
2. **Credential Exposure**: Username/password are sent from client (less secure than authorization code flow)
3. **Recommendations**:
   - Use only in trusted environments (internal apps, admin panels)
   - For public-facing apps, prefer `loginWithEmail()` (Authorization Code Flow)
   - Consider creating a public client (no secret) for password grant if needed

### Comparison: Redirect vs Direct Login

| Feature | Authorization Code (Redirect) | Password Grant (Direct) |
|---------|------------------------------|-------------------------|
| **Security** | ✅ More secure (PKCE) | ⚠️ Less secure |
| **User Experience** | ❌ Redirects to login page | ✅ No redirect |
| **Refresh Tokens** | ✅ Supported | ✅ Supported |
| **Silent Refresh** | ✅ Supported | ✅ Supported |
| **Best For** | Public-facing apps | Internal/admin apps |

---

## 📋 Available Methods in AuthService

### Login Methods

1. **`loginWithEmail(email, password)`** - Authorization Code Flow (redirects)
   - Standard OAuth 2.0 flow
   - Redirects to login page
   - Recommended for public apps

2. **`loginWithEmailDirect(email, password)`** - Password Grant (no redirect)
   - Direct login without redirect
   - Returns tokens immediately
   - Use for internal/admin apps

3. **`loginWithExternalProvider(provider)`** - External OAuth (redirects)
   - Google, Auth0, etc.
   - Redirects to external provider

### Token Management

1. **`refreshToken()`** - Manual token refresh
   - Refreshes access token using refresh token
   - Called automatically by silent refresh

2. **`getRefreshToken()`** - Get refresh token
   - Returns current refresh token

3. **`getAccessToken()`** - Get access token
   - Returns current access token

### User Management

1. **`loadUserProfile()`** - Load user info
   - Fetches user profile from server
   - Called automatically after login

2. **`isAuthenticated()`** - Check authentication
   - Returns true if user is authenticated

3. **`getUserInfo()`** - Get user info
   - Returns current user information

4. **`logout()`** - Logout user
   - Clears tokens and redirects to login

---

## 🔧 Configuration

### Enable/Disable Silent Refresh

In `auth.service.ts`:
```typescript
const enableSilentRefresh = true; // Set to false to disable
```

### Configure Token Refresh Timing

```typescript
timeoutFactor: 0.75, // Refresh when 75% of lifetime has passed (default: 0.75)
```

### Configure Silent Refresh Timeout

```typescript
silentRefreshTimeout: 5000, // 5 seconds timeout for silent refresh
```

---

## 🧪 Testing

### Test Direct Login (Password Grant)

```typescript
// Test direct login
const success = await authService.loginWithEmailDirect(
  'admin@sdms.com',
  'Admin@123'
);

console.log('Login successful:', success);
console.log('Access token:', authService.getAccessToken());
console.log('Refresh token:', authService.getRefreshToken());
console.log('User info:', authService.getUserInfo());
```

### Test Silent Refresh

1. Login using either method
2. Wait for token to expire (or manually trigger refresh)
3. Check console for refresh events
4. Verify new access token is obtained

### Test Manual Refresh

```typescript
// Manually refresh token
const refreshed = await authService.refreshToken();
console.log('Token refreshed:', refreshed);
```

---

## 📝 Files Modified

1. **`auth.service.ts`** - Added silent refresh and direct login
2. **`silent-refresh.html`** - Created for silent refresh iframe
3. **`AUTHENTICATION_FLOWS.md`** - Documentation of all flows
4. **`AUTHENTICATION_IMPLEMENTATION_GUIDE.md`** - This guide

---

## 🎯 Recommendations

### For Production

1. **Public-Facing Apps**: Use Authorization Code Flow (`loginWithEmail()`)
2. **Internal/Admin Apps**: Can use Password Grant (`loginWithEmailDirect()`)
3. **Enable Silent Refresh**: Always enable for better UX
4. **Use HTTPS**: Always use HTTPS in production
5. **Secure Token Storage**: Consider httpOnly cookies for sensitive apps

### Security Best Practices

1. **Never expose client secrets** in public client code (consider public client for password grant)
2. **Use Authorization Code Flow** for public-facing applications
3. **Enable silent refresh** to avoid token expiration issues
4. **Validate tokens** on the server side
5. **Use secure storage** for tokens (consider httpOnly cookies)

---

## 🚨 Troubleshooting

### Silent Refresh Not Working

1. Check if refresh token is available: `authService.getRefreshToken()`
2. Verify `offline_access` scope is requested
3. Check browser console for errors
4. Verify `silent-refresh.html` exists in assets

### Direct Login Not Working

1. Check if password grant is enabled in backend
2. Verify client ID and secret are correct
3. Check browser console for errors
4. Verify user credentials are correct

### Token Refresh Failing

1. Check if refresh token is valid
2. Verify refresh token hasn't expired
3. Check server logs for errors
4. Verify refresh token flow is enabled in backend

---

## 📚 Additional Resources

- [OAuth 2.0 Specification](https://oauth.net/2/)
- [OpenID Connect Specification](https://openid.net/connect/)
- [angular-oauth2-oidc Documentation](https://github.com/manfredsteyer/angular-oauth2-oidc)
- [OpenIddict Documentation](https://documentation.openiddict.com/)

---

## ✅ Next Steps

1. ✅ Silent refresh is enabled
2. ✅ Direct login (password grant) is implemented
3. ⚠️ Test both methods in your application
4. ⚠️ Configure client secret management (consider public client for password grant)
5. ⚠️ Enable HTTPS in production
6. ⚠️ Test token refresh functionality
7. ⚠️ Monitor token expiration and refresh events

