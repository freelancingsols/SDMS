# Authentication Types and Flows

## Current Authentication Implementation

### 1. **Authorization Code Flow with PKCE** (Currently Used)
- **Flow Type**: Authorization Code with Proof Key for Code Exchange (PKCE)
- **How it works**: 
  1. User is redirected to authorization server
  2. User authenticates
  3. Authorization server returns authorization code
  4. Frontend exchanges code for access token + refresh token
  5. Tokens are stored in browser storage
- **Pros**: Most secure for browser-based apps, supports refresh tokens
- **Cons**: Requires redirect to login page
- **Status**: ✅ Currently implemented in frontend

### 2. **Refresh Token Flow** (Supported, Not Fully Utilized)
- **Flow Type**: Refresh Token Grant
- **How it works**: 
  1. Client sends refresh token to `/connect/token`
  2. Server validates refresh token
  3. Server returns new access token + refresh token
- **Pros**: Silent token renewal without user interaction
- **Cons**: Requires initial authorization code flow
- **Status**: ✅ Backend supports it, ⚠️ Frontend not configured for automatic refresh

### 3. **Password Grant Flow** (Supported, Available for Direct Login)
- **Flow Type**: Resource Owner Password Credentials (ROPC)
- **How it works**: 
  1. Client sends username/password directly to `/connect/token`
  2. Server validates credentials
  3. Server returns access token + refresh token immediately
- **Pros**: No redirect, direct login, works in SPA without popups
- **Cons**: Less secure (credentials in client), not recommended for public clients
- **Status**: ✅ Backend supports it, ⚠️ Frontend not using it for direct login

### 4. **Client Credentials Flow** (For Machine-to-Machine)
- **Flow Type**: Client Credentials Grant
- **How it works**: 
  1. Client sends client_id + client_secret to `/connect/token`
  2. Server validates client credentials
  3. Server returns access token
- **Pros**: No user interaction, perfect for APIs/services
- **Cons**: No user context, no refresh tokens
- **Status**: ✅ Backend supports it, 🔒 Not for user authentication

---

## Available Authentication Options

### Option 1: Authorization Code Flow (Current - Redirects to Login)
```typescript
// Current implementation
this.oauthService.initCodeFlow(); // Redirects to login page
```

### Option 2: Password Grant (Direct Login - No Redirect)
```typescript
// Direct login without redirect
const tokenResponse = await this.http.post('/connect/token', {
  grant_type: 'password',
  username: 'user@example.com',
  password: 'password123',
  client_id: 'sdms_frontend',
  client_secret: 'sdms_frontend_secret',
  scope: 'openid profile email roles'
}).toPromise();
```

### Option 3: Silent Refresh (Automatic Token Renewal)
```typescript
// Enable silent refresh in OAuth configuration
this.oauthService.configure({
  useSilentRefresh: true,
  silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
  // ... other config
});
```

---

## How to Implement Silent Token Refresh

### Step 1: Enable Silent Refresh in Frontend

Update `auth.service.ts`:

```typescript
private configureOAuth() {
  this.oauthService.configure({
    issuer: AppSettings.SDMS_AuthenticationWebApp_url,
    redirectUri: AppSettings.SDMS_AuthenticationWebApp_redirectUri,
    clientId: AppSettings.SDMS_AuthenticationWebApp_clientid,
    responseType: 'code',
    scope: AppSettings.SDMS_AuthenticationWebApp_scope,
    requireHttps: false,
    showDebugInformation: true,
    strictDiscoveryDocumentValidation: false,
    
    // Enable silent refresh
    useSilentRefresh: true,
    silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
    silentRefreshTimeout: 5000,
    
    // Token refresh settings
    timeoutFactor: 0.75, // Refresh token when 75% of lifetime has passed
    sessionChecksEnabled: true,
    
    disableAtHashCheck: true
  });

  // Setup automatic token refresh
  this.oauthService.setupAutomaticSilentRefresh();
  
  this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
    if (this.oauthService.hasValidAccessToken()) {
      this.loadUserProfile();
    }
  });
}
```

### Step 2: Create Silent Refresh HTML File

Create `wwwroot/silent-refresh.html`:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Silent Refresh</title>
</head>
<body>
    <script src="assets/js/oauth2-silent-refresh.js"></script>
</body>
</html>
```

### Step 3: How Silent Refresh Works

1. **Automatic**: `angular-oauth2-oidc` automatically refreshes tokens before expiration
2. **Uses Refresh Token**: When access token is about to expire, it uses the refresh token
3. **Hidden Iframe**: Uses a hidden iframe to avoid page reload
4. **Transparent**: User doesn't notice the refresh happening

---

## How to Login Without Redirecting (Direct Login)

### Method 1: Use Password Grant Flow (Recommended for SPA)

Update `auth.service.ts` to add direct login method:

```typescript
async loginWithEmailDirect(email: string, password: string): Promise<boolean> {
  try {
    const formData = new URLSearchParams();
    formData.set('grant_type', 'password');
    formData.set('username', email);
    formData.set('password', password);
    formData.set('client_id', AppSettings.SDMS_AuthenticationWebApp_clientid);
    formData.set('client_secret', 'sdms_frontend_secret');
    formData.set('scope', 'openid profile email roles offline_access');

    const response = await this.http.post<any>(
      `${AppSettings.SDMS_AuthenticationWebApp_url}/connect/token`,
      formData.toString(),
      {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded'
        }
      }
    ).toPromise();

    if (response && response.access_token) {
      // Store tokens manually
      this.oauthService.storeAccessToken(response.access_token);
      if (response.refresh_token) {
        this.oauthService.storeRefreshToken(response.refresh_token);
      }
      
      // Load user profile
      await this.loadUserProfile();
      return true;
    }
    return false;
  } catch (error) {
    console.error('Direct login error:', error);
    return false;
  }
}
```

### Method 2: Use Authorization Code Flow in Popup (Alternative)

```typescript
async loginWithPopup(): Promise<boolean> {
  try {
    // Open authorization in popup
    const popup = window.open(
      `${this.oauthService.issuer}/connect/authorize?` +
      `client_id=${this.oauthService.clientId}&` +
      `response_type=code&` +
      `scope=openid profile email roles&` +
      `redirect_uri=${encodeURIComponent(this.oauthService.redirectUri)}`,
      'oauth',
      'width=500,height=600'
    );

    // Wait for popup to close and handle callback
    // This requires additional implementation
    return true;
  } catch (error) {
    console.error('Popup login error:', error);
    return false;
  }
}
```

---

## Comparison: Redirect vs Direct Login

| Feature | Authorization Code (Redirect) | Password Grant (Direct) |
|---------|------------------------------|-------------------------|
| **Security** | ✅ More secure (PKCE) | ⚠️ Less secure (credentials in client) |
| **User Experience** | ❌ Redirects to login page | ✅ No redirect, stays on page |
| **Refresh Tokens** | ✅ Supported | ✅ Supported |
| **Silent Refresh** | ✅ Supported | ✅ Supported |
| **Mobile Apps** | ✅ Recommended | ⚠️ Can be used |
| **SPA Best Practice** | ✅ Recommended by OAuth 2.1 | ⚠️ Not recommended but works |

---

## Recommendations

### For Production SPA:
1. **Primary**: Use **Authorization Code Flow with PKCE** (current implementation)
2. **Enhancement**: Enable **silent refresh** for automatic token renewal
3. **Fallback**: Use **Password Grant** only for trusted environments or admin panels

### For Better UX:
1. Enable silent refresh to avoid token expiration issues
2. Use password grant for direct login in admin/internal apps
3. Keep authorization code flow for public-facing applications

### Security Considerations:
1. **Password Grant**: Only use in trusted environments (internal apps, admin panels)
2. **Client Secret**: Never expose client secret in public client apps
3. **HTTPS**: Always use HTTPS in production
4. **Token Storage**: Use secure storage (httpOnly cookies recommended for sensitive apps)

---

## Implementation Steps

1. ✅ Backend already supports all flows
2. ⚠️ Frontend needs silent refresh configuration
3. ⚠️ Frontend needs password grant implementation for direct login
4. ⚠️ Create silent-refresh.html file
5. ⚠️ Update auth service with new methods

