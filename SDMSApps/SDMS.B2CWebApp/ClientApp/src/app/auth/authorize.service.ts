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
      // OAuth callback is handled by angular-oauth2-oidc automatically
      // Check if we already have a valid token (prevent multiple processing)
      if (this.oauthService.hasValidAccessToken()) {
        await this.loadUserProfile();
        return this.success(null);
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
      // tryLoginCodeFlow() automatically reads from window.location and exchanges the code for tokens
      try {
        console.log('Processing OAuth callback, current URL:', window.location.href);
        await this.oauthService.tryLoginCodeFlow();
        
        // Wait a bit for token to be stored
        await new Promise(resolve => setTimeout(resolve, 200));
        
        // Check if we got a valid token
        const hasToken = this.oauthService.hasValidAccessToken();
        console.log('Has valid access token after tryLoginCodeFlow:', hasToken);
        
        if (hasToken) {
          const token = this.oauthService.getAccessToken();
          console.log('Access token received, length:', token?.length || 0);
          await this.loadUserProfile();
          return this.success(null);
        } else {
          // Check if there's an error in the URL
          const urlParams = new URLSearchParams(window.location.search);
          const error = urlParams.get('error');
          const errorDescription = urlParams.get('error_description');
          if (error) {
            console.error('OAuth error in URL:', error, errorDescription);
            return this.error(`OAuth error: ${error} - ${errorDescription || ''}`);
          }
          
          // Check identity claims as fallback
          const claims = this.oauthService.getIdentityClaims();
          console.log('Identity claims:', claims);
          
          return this.error('No access token received after login. Check browser console for details.');
        }
      } catch (loginError) {
        console.error('Error in tryLoginCodeFlow:', loginError);
        if (loginError && typeof loginError === 'object') {
          console.error('Login error details:', JSON.stringify(loginError, null, 2));
        }
        return this.error('Failed to process OAuth callback: ' + (loginError instanceof Error ? loginError.message : String(loginError)));
      }
    } catch (error) {
      console.error('There was an error signing in: ', error);
      // Log full error details
      if (error && typeof error === 'object') {
        console.error('Full error object:', JSON.stringify(error, null, 2));
      }
      // Don't throw error to prevent redirect loops - just return error result
      const errorMessage = error instanceof Error ? error.message : (error && typeof error === 'object' ? JSON.stringify(error) : String(error));
      return this.error('There was an error signing in: ' + errorMessage);
    }
  }

  public async signOut(state: any): Promise<IAuthenticationResult> {
    try {
      if (this.popUpDisabled) {
        // Use redirect logout
        this.oauthService.logOut();
        this.userSubject.next(null);
        return this.success(state);
      }

      // Popup logout not directly supported, use redirect
      this.oauthService.logOut();
      this.userSubject.next(null);
      return this.success(state);
    } catch (popupSignOutError) {
      console.log('Signout error: ', popupSignOutError);
      try {
        this.oauthService.logOut();
        this.userSubject.next(null);
        return this.success(state);
      } catch (redirectSignOutError) {
        console.log('Redirect signout error: ', redirectSignOutError);
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
