import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, concat, Observable, of, firstValueFrom } from 'rxjs';
import { filter, map, take, tap } from 'rxjs/operators';
import { OAuthService } from 'angular-oauth2-oidc';
import { AppSettings } from '../config/app-settings';
import { environment } from '../../environments/environment';

export type IAuthenticationResult =
  SuccessAuthenticationResult |
  FailureAuthenticationResult |
  RedirectAuthenticationResult;

export interface SuccessAuthenticationResult {
  status: AuthenticationResultStatus.Success;
  state: any;
}

export interface FailureAuthenticationResult {
  status: AuthenticationResultStatus.Fail;
  message: string;
}

export interface RedirectAuthenticationResult {
  status: AuthenticationResultStatus.Redirect;
}

export enum AuthenticationResultStatus {
  Success,
  Redirect,
  Fail
}

export interface IUser {
  name?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthorizeService {
  private apiUrl = AppSettings.SDMS_AuthenticationWebApp_url;
  private popUpDisabled = true; // By default pop ups are disabled because they don't work properly on Edge.
  private userSubject: BehaviorSubject<IUser | null> = new BehaviorSubject<IUser | null>(null);
  private oauthConfigured = false;

  constructor(
    private oauthService: OAuthService,
    private http: HttpClient
  ) {
    this.configureOAuth();
  }

  private configureOAuth() {
    if (this.oauthConfigured) {
      return;
    }

    // Determine if we should use silent refresh
    const enableSilentRefresh = true;

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
      scope: AppSettings.SDMS_AuthenticationWebApp_scope + ' offline_access',
      requireHttps: environment.production,
      showDebugInformation: !environment.production,
      strictDiscoveryDocumentValidation: false,
      
      // Silent refresh configuration
      useSilentRefresh: enableSilentRefresh,
      silentRefreshRedirectUri: window.location.origin + '/silent-refresh.html',
      silentRefreshTimeout: 5000,
      
      // Token refresh settings
      timeoutFactor: 0.75,
      sessionChecksEnabled: false, // Disabled because OpenIddict doesn't provide check_session_iframe endpoint
      
      disableAtHashCheck: true,
      
      // Skip JWKS validation if it fails (for development)
      skipIssuerCheck: false,
      skipSubjectCheck: false
    });

    // Setup automatic silent refresh if enabled
    if (enableSilentRefresh) {
      this.oauthService.setupAutomaticSilentRefresh();
    }

    // Load discovery document and try to login automatically if token exists
    // Use a retry mechanism for better reliability
    this.loadDiscoveryDocumentWithRetry(3).then(() => {
      if (this.oauthService.hasValidAccessToken()) {
        this.loadUserProfile().catch(err => console.error('Error loading user profile on init:', err));
      }
    }).catch(err => {
      // Log the full error for debugging
      console.warn('Error loading discovery document after retries:', err);
      if (err instanceof Error) {
        console.warn('Error details:', err.message, err.stack);
      } else if (err && typeof err === 'object') {
        // Handle OAuthErrorEvent
        console.warn('OAuth error details:', JSON.stringify(err, null, 2));
        if ('reason' in err) {
          console.warn('Error reason:', (err as any).reason);
        }
        if ('params' in err) {
          console.warn('Error params:', (err as any).params);
        }
      }
      // Try to manually set discovery document URL as fallback
      this.tryManualDiscoveryDocument();
    });

    // Listen for token events
    this.oauthService.events.subscribe(event => {
      if (event.type === 'token_received' || event.type === 'token_refreshed') {
        this.loadUserProfile();
      } else if (event.type === 'logout') {
        this.userSubject.next(null);
      }
    });

    this.oauthConfigured = true;
  }

  public isAuthenticated(): Observable<boolean> {
    // Check if we have a valid access token
    const hasToken = this.oauthService.hasValidAccessToken();
    if (hasToken) {
      // If we have a token but no user loaded, try to load it
      if (!this.userSubject.value) {
        this.loadUserProfile().catch(err => console.error('Error loading user profile:', err));
      }
      return of(true);
    }
    // If no token, check if user is in storage
    return this.getUser().pipe(map(u => !!u));
  }

  public getUser(): Observable<IUser | null> {
    return concat(
      this.userSubject.pipe(take(1), filter(u => !!u)),
      this.getUserFromStorage().pipe(filter(u => !!u), tap(u => this.userSubject.next(u))),
      this.userSubject.asObservable());
  }

  public getAccessToken(): Observable<string> {
    const token = this.oauthService.getAccessToken();
    return of(token || '');
  }

  // We try to authenticate the user in three different ways:
  // 1) We try to see if we can authenticate the user silently. This happens
  //    when the user is already logged in on the IdP and is done using a hidden iframe
  //    on the client.
  // 2) We try to authenticate the user using a PopUp Window. This might fail if there is a
  //    Pop-Up blocker or the user has disabled PopUps.
  // 3) If the two methods above fail, we redirect the browser to the IdP to perform a traditional
  //    redirect flow.
  public async signIn(state: any): Promise<IAuthenticationResult> {
    try {
      // Try silent refresh first (equivalent to signinSilent)
      if (this.oauthService.hasValidAccessToken()) {
        await this.loadUserProfile();
        return this.success(state);
      }

      // Try silent refresh
      try {
        await this.oauthService.loadDiscoveryDocument();
        await this.oauthService.tryLoginCodeFlow();
        if (this.oauthService.hasValidAccessToken()) {
          await this.loadUserProfile();
          return this.success(state);
        }
      } catch (silentError) {
        console.log('Silent authentication error: ', silentError);
      }

      // User might not be authenticated, fallback to popup authentication
      try {
        if (this.popUpDisabled) {
          throw new Error('Popup disabled. Change \'authorize.service.ts:AuthorizeService.popupDisabled\' to false to enable it.');
        }
        
        // Popup authentication - OAuthService doesn't have direct popup support
        // We'll use redirect flow instead
        throw new Error('Popup authentication not supported, using redirect');
      } catch (popupError: unknown) {
        const errorMessage = popupError instanceof Error ? popupError.message : String(popupError);
        if (errorMessage === 'Popup window closed' || errorMessage.includes('Popup')) {
          // Fall through to redirect
        } else if (!this.popUpDisabled) {
          console.log('Popup authentication error: ', popupError);
        }

        // PopUps might be blocked by the user, fallback to redirect
        try {
          await this.oauthService.loadDiscoveryDocument();
          this.oauthService.initCodeFlow();
          return this.redirect();
        } catch (redirectError) {
          console.log('Redirect authentication error: ', redirectError);
          return this.error(String(redirectError));
        }
      }
    } catch (error) {
      console.log('Authentication error: ', error);
      return this.error(String(error));
    }
  }

  public async completeSignIn(_url: string, _callbackAction: string): Promise<IAuthenticationResult> {
    try {
      // Parse the original URL FIRST (before angular-oauth2-oidc processes it)
      // The _url parameter contains the original callback URL with code/state
      const originalUrl = _url || window.location.href;
      const urlObj = new URL(originalUrl);
      const urlParams = urlObj.searchParams;
      const hasCode = urlParams.has('code');
      const hasState = urlParams.has('state');
      const hasIss = urlParams.has('iss');
      
      console.log('completeSignIn - Original URL params:', { hasCode, hasState, hasIss });
      
      // Check if this is a logout callback (has 'iss' but no 'code' or 'state')
      // Logout callbacks should not be processed as login
      if (hasIss && !hasCode && !hasState) {
        // Check if we have tokens stored - if yes, this might be a logout callback after tokens were cleared
        // If no tokens, definitely a logout callback
        const hasStoredTokens = !!sessionStorage.getItem('access_token') || !!sessionStorage.getItem('id_token');
        if (!hasStoredTokens) {
          console.log('Detected logout callback in completeSignIn (no code/state, no tokens), skipping login processing');
          window.history.replaceState({}, document.title, '/');
          return this.success({ returnUrl: '/' });
        }
      }
      
      // Check if we already have a valid token (prevent multiple processing)
      // This should be checked AFTER determining if it's a logout callback
      if (this.oauthService.hasValidAccessToken()) {
        console.log('Already have valid access token, loading user profile');
        await this.loadUserProfile();
        return this.success(null);
      }
      
      // If we have code/state in the original URL, this is a login callback - process it
      if (!hasCode || !hasState) {
        // No code/state in original URL - check if tokens exist in storage
        // If tokens exist but OAuth service says invalid, try to load user profile anyway
        const hasStoredTokens = !!sessionStorage.getItem('access_token') || !!sessionStorage.getItem('id_token');
        if (hasStoredTokens) {
          console.log('Tokens exist in storage but OAuth service reports invalid - attempting to load user profile');
          try {
            await this.loadUserProfile();
            return this.success(null);
          } catch (error) {
            console.log('Failed to load user profile with stored tokens:', error);
            // Continue to process as if no tokens
          }
        } else {
          // No tokens and no code/state - this is likely a logout callback
          console.log('No code/state in URL and no stored tokens - treating as logout callback');
          window.history.replaceState({}, document.title, '/');
          return this.success({ returnUrl: '/' });
        }
      }

      // Load discovery document first
      try {
        await this.oauthService.loadDiscoveryDocument();
      } catch (discoveryError) {
        console.error('Error loading discovery document in completeSignIn:', discoveryError);
        if (discoveryError && typeof discoveryError === 'object') {
          console.error('Discovery error details:', JSON.stringify(discoveryError, null, 2));
        }
        return this.error('Failed to load discovery document. Please check the authentication server is running.');
      }
      
      // Try to process the callback URL
      // IMPORTANT: Restore the original URL temporarily because tryLoginCodeFlow() reads from window.location.href
      // The OAuth library may have already modified the URL to remove code/state
      try {
        console.log('Processing OAuth callback, original URL:', originalUrl);
        console.log('Processing OAuth callback, current URL:', window.location.href);
        
        // Temporarily restore the original URL so tryLoginCodeFlow can extract code/state
        const currentUrl = window.location.href;
        window.history.replaceState({}, document.title, originalUrl);
        
        try {
          // Now tryLoginCodeFlow can read the code/state from window.location.href
          await this.oauthService.tryLoginCodeFlow();
        } finally {
          // Restore the current URL after processing
          window.history.replaceState({}, document.title, currentUrl);
        }
        
        // Wait longer for token to be stored and validated by OAuth service
        // The OAuth service needs time to process the token exchange response
        await new Promise(resolve => setTimeout(resolve, 500));
        
        // Check for stored tokens FIRST (before checking hasValidAccessToken)
        // This is more reliable because tokens might be stored but not yet validated
        const accessToken = sessionStorage.getItem('access_token');
        const idToken = sessionStorage.getItem('id_token');
        const hasStoredTokens = !!accessToken || !!idToken;
        
        console.log('After tryLoginCodeFlow - Stored tokens check:', {
          hasAccessToken: !!accessToken,
          hasIdToken: !!idToken,
          accessTokenLength: accessToken?.length || 0,
          idTokenLength: idToken?.length || 0
        });
        
        // Check if we got a valid token via OAuth service
        const hasValidToken = this.oauthService.hasValidAccessToken();
        console.log('Has valid access token after tryLoginCodeFlow:', hasValidToken);
        
        // If we have stored tokens OR valid token, try to load user profile
        if (hasStoredTokens || hasValidToken) {
          console.log('Tokens found (stored or valid) - loading user profile');
          try {
            await this.loadUserProfile();
            // Clear any OAuth callback parameters from URL after successful login
            window.history.replaceState({}, document.title, '/test');
            return this.success(null);
          } catch (profileError) {
            console.log('Failed to load user profile, but tokens exist:', profileError);
            // Even if profile load fails, if we have tokens, authentication succeeded
            // Clear URL and return success
            window.history.replaceState({}, document.title, '/test');
            return this.success(null);
          }
        }
        
        // Check if there's an error in the URL
        const urlParams = new URLSearchParams(window.location.search);
        const error = urlParams.get('error');
        const errorDescription = urlParams.get('error_description');
        if (error) {
          console.error('OAuth error in URL:', error, errorDescription);
          return this.error(`OAuth error: ${error} - ${errorDescription || ''}`);
        }
        
        // Check identity claims as final fallback
        const claims = this.oauthService.getIdentityClaims();
        console.log('Identity claims:', claims);
        
        if (claims) {
          // We have identity claims - try to load profile
          try {
            await this.loadUserProfile();
            window.history.replaceState({}, document.title, '/test');
            return this.success(null);
          } catch (profileError) {
            console.log('Failed to load user profile with identity claims:', profileError);
            // Still return success if we have claims
            window.history.replaceState({}, document.title, '/test');
            return this.success(null);
          }
        }
        
        // Only show error if we truly have no tokens, no claims, and no error in URL
        console.error('No tokens, no claims, and no error in URL - authentication may have failed');
        return this.error('No access token received after login. Check browser console for details.');
      } catch (loginError) {
        console.error('Error in tryLoginCodeFlow:', loginError);
        
        // Extract meaningful error message from various error formats
        let errorMessage = 'Failed to process OAuth callback';
        
        if (loginError instanceof Error) {
          errorMessage += ': ' + loginError.message;
        } else if (loginError && typeof loginError === 'object') {
          // Try to extract error message from common error object properties
          const errorObj = loginError as any;
          if (errorObj.message) {
            errorMessage += ': ' + errorObj.message;
          } else if (errorObj.error) {
            errorMessage += ': ' + errorObj.error;
            if (errorObj.error_description) {
              errorMessage += ' - ' + errorObj.error_description;
            }
          } else if (errorObj.error_description) {
            errorMessage += ': ' + errorObj.error_description;
          } else {
            // Fallback: try to stringify the error object
            try {
              const errorStr = JSON.stringify(loginError);
              if (errorStr && errorStr !== '{}') {
                errorMessage += ': ' + errorStr;
              }
            } catch (e) {
              errorMessage += '. Check browser console for details.';
            }
          }
          
          // Log full error details for debugging
          console.error('Login error details:', JSON.stringify(loginError, null, 2));
        } else if (loginError) {
          errorMessage += ': ' + String(loginError);
        } else {
          errorMessage += '. Check browser console for details.';
        }
        
        return this.error(errorMessage);
      }
    } catch (error) {
      console.error('There was an error signing in: ', error);
      
      // Extract meaningful error message from various error formats
      let errorMessage = 'There was an error signing in';
      
      if (error instanceof Error) {
        errorMessage += ': ' + error.message;
      } else if (error && typeof error === 'object') {
        // Try to extract error message from common error object properties
        const errorObj = error as any;
        if (errorObj.message) {
          errorMessage += ': ' + errorObj.message;
        } else if (errorObj.error) {
          errorMessage += ': ' + errorObj.error;
          if (errorObj.error_description) {
            errorMessage += ' - ' + errorObj.error_description;
          }
        } else if (errorObj.error_description) {
          errorMessage += ': ' + errorObj.error_description;
        } else {
          // Fallback: try to stringify the error object
          try {
            const errorStr = JSON.stringify(error);
            if (errorStr && errorStr !== '{}') {
              errorMessage += ': ' + errorStr;
            } else {
              errorMessage += '. Check browser console for details.';
            }
          } catch (e) {
            errorMessage += '. Check browser console for details.';
          }
        }
        
        // Log full error details for debugging
        console.error('Full error object:', JSON.stringify(error, null, 2));
      } else if (error) {
        errorMessage += ': ' + String(error);
      } else {
        errorMessage += '. Check browser console for details.';
      }
      
      // Don't throw error to prevent redirect loops - just return error result
      return this.error(errorMessage);
    }
  }

  public async signOut(state: any): Promise<IAuthenticationResult> {
    try {
      // Clear user subject first to prevent getUserFromStorage from repopulating
      this.userSubject.next(null);
      
      // Manually clear OAuth-related storage FIRST before calling logOut()
      // This prevents logOut() from redirecting with tokens still in storage
      // angular-oauth2-oidc stores tokens in sessionStorage with these exact keys
      try {
        const oauthKeys = [
          'access_token',
          'access_token_stored_at',
          'id_token',
          'id_token_stored_at',
          'id_token_expires_at',
          'id_token_claims_obj',
          'refresh_token',
          'nonce',
          'PKCE_verifier',
          'session_state',
          'granted_scopes',
          'expires_at'
        ];
        
        // Clear all OAuth keys from sessionStorage - do this synchronously
        oauthKeys.forEach(key => {
          try {
            sessionStorage.removeItem(key);
          } catch (e) {
            // Ignore errors for individual keys
          }
        });
        
        // Also clear any keys that might match patterns - collect first, then remove
        const sessionKeysToRemove: string[] = [];
        for (let i = sessionStorage.length - 1; i >= 0; i--) {
          const key = sessionStorage.key(i);
          if (key && (
            key.startsWith('oauth_') || 
            key.startsWith('oidc_') ||
            oauthKeys.includes(key)
          )) {
            sessionKeysToRemove.push(key);
          }
        }
        sessionKeysToRemove.forEach(key => {
          try {
            sessionStorage.removeItem(key);
          } catch (e) {
            // Ignore errors
          }
        });
        
        // Clear localStorage as well - collect first, then remove
        const localKeysToRemove: string[] = [];
        for (let i = localStorage.length - 1; i >= 0; i--) {
          const key = localStorage.key(i);
          if (key && (
            key.startsWith('oauth_') || 
            key.startsWith('oidc_') ||
            oauthKeys.includes(key)
          )) {
            localKeysToRemove.push(key);
          }
        }
        localKeysToRemove.forEach(key => {
          try {
            localStorage.removeItem(key);
          } catch (e) {
            // Ignore errors
          }
        });
        
        // Force clear OAuth service internal state by trying to access and clear
        try {
          // Clear any cached tokens in OAuth service
          if (this.oauthService.getAccessToken()) {
            // Token still exists - force clear by removing from storage again
            sessionStorage.removeItem('access_token');
            sessionStorage.removeItem('id_token');
            sessionStorage.removeItem('refresh_token');
          }
        } catch (e) {
          // Ignore
        }
      } catch (storageError) {
        console.log('Error clearing storage:', storageError);
      }
      
      // Don't call oauthService.logOut() - it will redirect to /connect/logout
      // which then redirects back to /auth-callback, causing a login loop
      // We've already cleared all storage, so logOut() is not needed
      // The OAuth service's internal state will be cleared when tokens are removed from storage
      
      return this.success(state);
    } catch (popupSignOutError) {
      console.log('Signout error: ', popupSignOutError);
      try {
        // Ensure user is cleared even on error
        this.userSubject.next(null);
        // Try to clear storage again
        try {
          const oauthKeys = ['access_token', 'id_token', 'refresh_token', 'nonce', 'PKCE_verifier', 'session_state', 'granted_scopes', 'expires_at'];
          oauthKeys.forEach(key => {
            try {
              sessionStorage.removeItem(key);
              localStorage.removeItem(key);
            } catch (e) {}
          });
        } catch (e) {}
        return this.success(state);
      } catch (redirectSignOutError) {
        console.log('Redirect signout error: ', redirectSignOutError);
        // Still clear user on error
        this.userSubject.next(null);
        return this.error(String(redirectSignOutError));
      }
    }
  }

  public async completeSignOut(_url: string): Promise<IAuthenticationResult> {
    try {
      this.oauthService.logOut();
      this.userSubject.next(null);
      return this.success(null);
    } catch (error) {
      console.log(`There was an error trying to log out '${error}'.`);
      return this.error(String(error));
    }
  }

  private error(message: string): IAuthenticationResult {
    return { status: AuthenticationResultStatus.Fail, message };
  }

  private success(state: any): IAuthenticationResult {
    return { status: AuthenticationResultStatus.Success, state };
  }

  private redirect(): IAuthenticationResult {
    return { status: AuthenticationResultStatus.Redirect };
  }

  private async loadUserProfile(): Promise<void> {
    if (this.oauthService.hasValidAccessToken()) {
      try {
        // Try to get user info from API first (more complete user data)
        try {
          const token = this.oauthService.getAccessToken();
          if (token) {
            const headers = new HttpHeaders({
              'Authorization': `Bearer ${token}`
            });
            const userInfo = await firstValueFrom(
              this.http.get<any>(`${this.apiUrl}/account/userinfo`, { headers })
            );
            if (userInfo) {
              const user: IUser = {
                name: userInfo.displayName || userInfo.email || userInfo.name || ''
              };
              this.userSubject.next(user);
              return;
            }
          }
        } catch (apiError) {
          // If API call fails, fall back to identity claims
          console.warn('Could not fetch user info from API, using identity claims:', apiError);
        }

        // Fallback to identity claims from token
        const claims = this.oauthService.getIdentityClaims();
        if (claims) {
          const user: IUser = {
            name: claims['name'] || claims['email'] || claims['sub'] || ''
          };
          this.userSubject.next(user);
        }
      } catch (error) {
        console.error('Error loading user profile:', error);
      }
    }
  }

  private getUserFromStorage(): Observable<IUser | null> {
    // If userSubject is null, we're in a logout state - don't read from storage
    if (this.userSubject.value === null) {
      return of(null);
    }
    
    if (this.oauthService.hasValidAccessToken()) {
      try {
        const claims = this.oauthService.getIdentityClaims();
        if (claims) {
          const user: IUser = {
            name: claims['name'] || claims['email'] || claims['sub'] || ''
          };
          return of(user);
        }
      } catch (error) {
        console.error('Error getting user from storage:', error);
      }
    }
    return of(null);
  }

  private async loadDiscoveryDocumentWithRetry(maxRetries: number = 3): Promise<void> {
    let lastError: any = null;
    for (let attempt = 1; attempt <= maxRetries; attempt++) {
      try {
        await this.oauthService.loadDiscoveryDocumentAndTryLogin();
        return; // Success
      } catch (error) {
        lastError = error;
        if (attempt < maxRetries) {
          // Wait before retry (exponential backoff)
          const delay = Math.min(1000 * Math.pow(2, attempt - 1), 5000);
          console.warn(`Discovery document load attempt ${attempt} failed, retrying in ${delay}ms...`);
          await new Promise(resolve => setTimeout(resolve, delay));
        }
      }
    }
    throw lastError;
  }

  private tryManualDiscoveryDocument(): void {
    try {
      // Manually set discovery document URL if automatic discovery fails
      const issuerUrl = AppSettings.SDMS_AuthenticationWebApp_url.endsWith('/') 
        ? AppSettings.SDMS_AuthenticationWebApp_url 
        : AppSettings.SDMS_AuthenticationWebApp_url + '/';
      const discoveryUrl = `${issuerUrl}.well-known/openid-configuration`;
      
      console.log('Attempting to manually load discovery document from:', discoveryUrl);
      
      // Try to fetch discovery document manually with retry
      let retryCount = 0;
      const maxRetries = 2;
      
      const fetchDiscovery = () => {
        this.http.get(discoveryUrl).subscribe({
          next: () => {
            console.log('Successfully fetched discovery document manually');
            // Try to reload discovery document in OAuth service
            // The OAuth service should handle the configuration
            this.oauthService.loadDiscoveryDocument(discoveryUrl).then(() => {
              console.log('OAuth service configured with manual discovery document');
            }).catch(err => {
              console.error('Error loading discovery document in OAuth service:', err);
            });
          },
          error: (err) => {
            retryCount++;
            if (retryCount < maxRetries) {
              console.warn(`Manual discovery fetch attempt ${retryCount} failed, retrying...`);
              setTimeout(fetchDiscovery, 2000);
            } else {
              console.error('Failed to manually fetch discovery document after retries:', err);
            }
          }
        });
      };
      
      fetchDiscovery();
    } catch (error) {
      console.error('Error in tryManualDiscoveryDocument:', error);
    }
  }
}
