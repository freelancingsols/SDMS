import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { OAuthService } from 'angular-oauth2-oidc';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { AppSettings } from '../config/app-settings';
import { environment } from '../../environments/environment';
import { UserInfo, AuthenticationProvider } from '../interfaces/auth.interface';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private userInfoSubject = new BehaviorSubject<UserInfo | null>(null);
  public userInfo$ = this.userInfoSubject.asObservable();
  private apiUrl = AppSettings.SDMS_AuthenticationWebApp_url;

  constructor(
    private oauthService: OAuthService,
    private http: HttpClient
  ) {
    this.configureOAuth();
    this.loadUserProfile();
  }

  private configureOAuth() {
    // Determine if we should use silent refresh
    // Silent refresh requires a refresh token, which is obtained from authorization code or password grant
    const enableSilentRefresh = true; // Set to false to disable silent refresh

    // Normalize issuer URL - ensure it ends with a slash to match discovery document
    let issuerUrl = AppSettings.SDMS_AuthenticationWebApp_url;
    if (!issuerUrl.endsWith('/')) {
      issuerUrl = issuerUrl + '/';
    }

    this.oauthService.configure({
      issuer: issuerUrl,
      redirectUri: AppSettings.SDMS_AuthenticationWebApp_redirectUri,
      clientId: AppSettings.SDMS_AuthenticationWebApp_clientid,
      responseType: 'code',
      scope: AppSettings.SDMS_AuthenticationWebApp_scope + ' offline_access', // Add offline_access for refresh tokens
      requireHttps: environment.production, // Require HTTPS in production for security
      showDebugInformation: !environment.production, // Only show debug info in development
      strictDiscoveryDocumentValidation: false,
      
      // Silent refresh configuration
      useSilentRefresh: enableSilentRefresh,
      silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
      silentRefreshTimeout: 5000, // 5 seconds timeout for silent refresh
      
      // Token refresh settings
      timeoutFactor: 0.75, // Refresh token when 75% of lifetime has passed (default: 0.75)
      sessionChecksEnabled: true, // Check if user session is still valid
      
      disableAtHashCheck: true
    });

    // Setup automatic silent refresh if enabled
    if (enableSilentRefresh) {
      this.oauthService.setupAutomaticSilentRefresh();
    }

    // CRITICAL: Check if there's a code in the URL before auto-processing
    // If there's a code, only load discovery document (don't auto-process)
    // The code will be processed by completeSignIn() to prevent duplicate exchanges
    const urlParams = new URLSearchParams(window.location.search);
    const hasCode = urlParams.has('code');
    
    if (hasCode) {
      console.log('Authorization code detected in URL - loading discovery document only (no auto-login)');
      this.oauthService.loadDiscoveryDocument().then(() => {
        // Don't auto-process - let completeSignIn() handle it
      });
    } else {
      // No code in URL - safe to use loadDiscoveryDocumentAndTryLogin for silent refresh
      this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
        if (this.oauthService.hasValidAccessToken()) {
          this.loadUserProfile();
        }
      });
    }
  }

  async loginWithExternalProvider(_provider: AuthenticationProvider): Promise<void> {
    // Initiate OAuth flow with PKCE
    this.oauthService.initCodeFlow();
  }

  /**
   * Login with email/password using Authorization Code Flow (redirects to login page)
   * This is the standard OAuth 2.0 flow recommended for SPAs.
   */
  async loginWithEmail(email: string, password: string): Promise<boolean> {
    try {
      const response = await firstValueFrom(
        this.http.post<any>(`${this.apiUrl}/account/login`, {
          email,
          password
        })
      );

      if (response) {
        // After successful backend login, initiate OpenIddict OAuth flow to get tokens
        await this.oauthService.loadDiscoveryDocument();
        this.oauthService.initCodeFlow();
        return true;
      }
      return false;
    } catch (error) {
      console.error('Login error:', error);
      return false;
    }
  }

  /**
   * Login with email/password using Password Grant Flow (NO REDIRECT)
   * This allows direct login without redirecting to a login page.
   * 
   * SECURITY NOTE: Password grant exposes credentials in the client.
   * - Use only in trusted environments (internal apps, admin panels)
   * - For public-facing apps, prefer loginWithEmail() (Authorization Code Flow)
   * - Client secret is exposed in client code (not ideal for public clients)
   * 
   * @param email User email
   * @param password User password
   * @returns true if login successful, false otherwise
   */
  async loginWithEmailDirect(email: string, password: string): Promise<boolean> {
    try {
      // Prepare form data for password grant
      const formData = new URLSearchParams();
      formData.set('grant_type', 'password');
      formData.set('username', email);
      formData.set('password', password);
      formData.set('client_id', AppSettings.SDMS_AuthenticationWebApp_clientid);
      // SECURITY NOTE: Password grant with client secret in client code is not secure
      // For production, either:
      // 1. Use authorization code flow with PKCE (recommended for SPAs)
      // 2. Use a public client (no secret) if your auth server supports it
      // 3. Move authentication to a backend service
      // For now, this is acceptable only for development/testing
      formData.set('scope', AppSettings.SDMS_AuthenticationWebApp_scope + ' offline_access');

      const response = await firstValueFrom(
        this.http.post<any>(
          `${this.apiUrl}/connect/token`,
          formData.toString(),
          {
            headers: {
              'Content-Type': 'application/x-www-form-urlencoded'
            }
          }
        )
      );

      if (response && response.access_token) {
        // Store tokens using OAuthService's built-in methods
        // The library handles token storage internally
        // We need to manually set the tokens since we're using password grant
        if (response.access_token) {
          // Use the library's internal storage mechanism
          // Note: This is a workaround for password grant flow
          // Ideally, use authorization code flow with PKCE for better security
          const storage = sessionStorage;
          storage.setItem('access_token', response.access_token);
          
          if (response.refresh_token) {
            storage.setItem('refresh_token', response.refresh_token);
          }
          
          if (response.expires_in) {
            const now = Math.floor(new Date().getTime() / 1000);
            const expiresAt = now + response.expires_in;
            storage.setItem('access_token_stored_at', now.toString());
            storage.setItem('access_token_expires_at', expiresAt.toString());
          }
          
          if (response.id_token) {
            storage.setItem('id_token', response.id_token);
          }
          
          if (response.token_type) {
            storage.setItem('token_type', response.token_type);
          }
          
          if (response.scope) {
            storage.setItem('scope', response.scope);
          }
          
          // Trigger token received event to notify the library
          // Note: This is a workaround for password grant flow
          // The events property is an Observable, but we need to emit to it for manual token storage
          (this.oauthService.events as any).next({ 
            type: 'token_received',
            info: {
              access_token: response.access_token,
              id_token: response.id_token,
              refresh_token: response.refresh_token,
              expires_in: response.expires_in
            }
          });
        }

        // Load user profile
        await this.loadUserProfile();
        return true;
      }
      return false;
    } catch (error: unknown) {
      console.error('Direct login error:', error);
      if (error && typeof error === 'object' && 'error' in error) {
        const httpError = error as { error?: { error_description?: string } };
        if (httpError.error) {
          console.error('Error details:', httpError.error);
          if (httpError.error.error_description) {
            console.error('Error description:', httpError.error.error_description);
          }
        }
      }
      return false;
    }
  }

  /**
   * Refresh the access token using the refresh token
   * This is called automatically by silent refresh, but can also be called manually
   * 
   * @returns Promise<boolean> true if refresh successful, false otherwise
   */
  async refreshToken(): Promise<boolean> {
    try {
      // Check if we have a refresh token
      const refreshToken = this.oauthService.getRefreshToken();
      if (!refreshToken) {
        console.warn('No refresh token available');
        return false;
      }

      // Use OAuthService's refresh token method
      await this.oauthService.refreshToken();
      
      // Reload user profile after token refresh
      await this.loadUserProfile();
      return true;
    } catch (error) {
      console.error('Token refresh error:', error);
      return false;
    }
  }

  /**
   * Get refresh token from storage
   * @returns Refresh token string or null
   */
  getRefreshToken(): string | null {
    return this.oauthService.getRefreshToken();
  }

  async register(email: string, password: string, displayName?: string): Promise<boolean> {
    try {
      const response = await firstValueFrom(
        this.http.post<any>(`${this.apiUrl}/account/register`, {
          email,
          password,
          displayName
        })
      );

      return response != null;
    } catch (error) {
      console.error('Registration error:', error);
      return false;
    }
  }

  async loadUserProfile(): Promise<void> {
    if (this.oauthService.hasValidAccessToken()) {
      try {
        const token = this.oauthService.getAccessToken();
        if (!token) {
          console.warn('No access token available for user profile');
          return;
        }
        
        const headers = new HttpHeaders({
          'Authorization': `Bearer ${token}`
        });

        const userInfo = await firstValueFrom(
          this.http.get<UserInfo>(`${this.apiUrl}/account/userinfo`, { headers })
        );
        if (userInfo) {
          this.userInfoSubject.next(userInfo);
        }
      } catch (error: unknown) {
        console.error('Error loading user profile:', error);
        // Clear user info on error to prevent stale data
        this.userInfoSubject.next(null);
      }
    }
  }

  getAccessToken(): string | null {
    return this.oauthService.getAccessToken();
  }

  isAuthenticated(): boolean {
    return this.oauthService.hasValidAccessToken();
  }

  async logout(): Promise<void> {
    // IMPORTANT: Get the ID token BEFORE clearing tokens (needed for logout request)
    const idToken = this.oauthService.getIdToken();
    
    // Set logout flag to prevent auto-login
    sessionStorage.setItem('_logout_flag', 'true');
    
    // Clear user info first
    this.userInfoSubject.next(null);
    
    // Get the post-logout redirect URI from AppSettings (same pattern as redirectUri)
    let postLogoutRedirectUri = AppSettings.SDMS_AuthenticationWebApp_postLogoutRedirectUri;
    
    // Normalize postLogoutRedirectUri: remove trailing slash for root URIs
    // This ensures it matches the database format (OpenIddict does exact matching)
    // AppSettings.getter should already normalize it, but do it here as a safety check
    if (postLogoutRedirectUri && postLogoutRedirectUri.endsWith('/')) {
      try {
        const url = new URL(postLogoutRedirectUri);
        // If the path is just "/", remove the trailing slash
        if (url.pathname === '/') {
          postLogoutRedirectUri = postLogoutRedirectUri.slice(0, -1);
        }
      } catch {
        // If URL parsing fails, just remove trailing slash
        postLogoutRedirectUri = postLogoutRedirectUri.endsWith('/') 
          ? postLogoutRedirectUri.slice(0, -1) 
          : postLogoutRedirectUri;
      }
    }
    
    // Get the auth server URL from configuration
    const authServerUrl = AppSettings.SDMS_AuthenticationWebApp_url;
    const logoutUrl = authServerUrl.endsWith('/') 
      ? `${authServerUrl}connect/logout`
      : `${authServerUrl}/connect/logout`;
    
    // Build the logout URL with proper parameters
    const logoutParams = new URLSearchParams();
    logoutParams.set('post_logout_redirect_uri', postLogoutRedirectUri);
    if (idToken) {
      logoutParams.set('id_token_hint', idToken);
    }
    
    const fullLogoutUrl = `${logoutUrl}?${logoutParams.toString()}`;
    
    // Clear local tokens first
    this.clearLocalTokens();
    
    // Redirect to auth server logout endpoint
    // The auth server will invalidate the session and redirect back
    window.location.href = fullLogoutUrl;
  }
  
  private clearLocalTokens(): void {
    // Clear all OAuth tokens from storage
    const oauthKeys = [
      'access_token',
      'access_token_stored_at',
      'access_token_expires_at',
      'id_token',
      'id_token_stored_at',
      'id_token_expires_at',
      'id_token_claims_obj',
      'refresh_token',
      'nonce',
      'PKCE_verifier',
      'session_state',
      'granted_scopes',
      'expires_at',
      'token_type',
      'scope'
    ];
    
    oauthKeys.forEach(key => {
      try {
        sessionStorage.removeItem(key);
        localStorage.removeItem(key);
      } catch (e) {
        // Ignore errors
      }
    });
    
    // Clear any OAuth-prefixed keys
    for (let i = sessionStorage.length - 1; i >= 0; i--) {
      const key = sessionStorage.key(i);
      if (key && (key.startsWith('oauth_') || key.startsWith('oidc_'))) {
        sessionStorage.removeItem(key);
      }
    }
    
    for (let i = localStorage.length - 1; i >= 0; i--) {
      const key = localStorage.key(i);
      if (key && (key.startsWith('oauth_') || key.startsWith('oidc_'))) {
        localStorage.removeItem(key);
      }
    }
  }

  getUserInfo(): UserInfo | null {
    return this.userInfoSubject.value;
  }
}

