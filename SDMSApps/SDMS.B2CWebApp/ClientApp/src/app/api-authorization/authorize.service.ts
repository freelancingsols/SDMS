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
      sessionChecksEnabled: true,
      
      disableAtHashCheck: true
    });

    // Setup automatic silent refresh if enabled
    if (enableSilentRefresh) {
      this.oauthService.setupAutomaticSilentRefresh();
    }

    // Load discovery document and try to login automatically if token exists
    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
      if (this.oauthService.hasValidAccessToken()) {
        this.loadUserProfile().catch(err => console.error('Error loading user profile on init:', err));
      }
    }).catch(err => {
      console.warn('Error loading discovery document:', err);
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
      // Load discovery document first
      await this.oauthService.loadDiscoveryDocument();
      
      // Try to process the callback URL
      await this.oauthService.tryLoginCodeFlow();
      
      // Check if we got a valid token
      if (this.oauthService.hasValidAccessToken()) {
        await this.loadUserProfile();
        return this.success(null);
      } else {
        return this.error('No access token received after login.');
      }
    } catch (error) {
      console.log('There was an error signing in: ', error);
      return this.error('There was an error signing in: ' + (error instanceof Error ? error.message : String(error)));
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
}
