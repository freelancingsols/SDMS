import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { OAuthService } from 'angular-oauth2-oidc';
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface UserInfo {
  userId: string;
  email: string;
  displayName?: string;
  externalProvider?: string;
  profilePictureUrl?: string;
  lastLoginDate?: string;
  roles?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private userInfoSubject = new BehaviorSubject<UserInfo | null>(null);
  public userInfo$ = this.userInfoSubject.asObservable();
  private apiUrl = environment.apiUrl;

  constructor(
    private oauthService: OAuthService,
    private http: HttpClient,
    private router: Router
  ) {
    this.configureOAuth();
    this.loadUserProfile();
  }

  private configureOAuth() {
    // Normalize issuer URL - ensure it ends with a slash to match discovery document
    let issuerUrl = environment.authServer;
    if (!issuerUrl.endsWith('/')) {
      issuerUrl = issuerUrl + '/';
    }

    this.oauthService.configure({
      issuer: issuerUrl,
      redirectUri: window.location.origin + '/auth-callback',
      clientId: environment.clientId,
      responseType: 'code',
      scope: 'openid profile email roles',
      requireHttps: false, // Set to true in production
      showDebugInformation: true,
      strictDiscoveryDocumentValidation: false,
      useSilentRefresh: false,
      disableAtHashCheck: true
      // PKCE is enabled by default for code flow in angular-oauth2-oidc v18
    });

    this.oauthService.loadDiscoveryDocumentAndTryLogin().then(() => {
      if (this.oauthService.hasValidAccessToken()) {
        this.loadUserProfile();
      }
    });
  }

  async loginWithExternalProvider(provider: 'auth0' | 'google'): Promise<void> {
    // Initiate OAuth flow with PKCE
    this.oauthService.initCodeFlow();
  }

  async loginWithExternalProviderDirect(provider: 'auth0' | 'google'): Promise<void> {
    // Alternative: Direct redirect to authorization endpoint
    const authUrl = `${environment.authServer}/connect/authorize?` +
      `client_id=${environment.clientId}&` +
      `response_type=code&` +
      `scope=openid profile email roles&` +
      `redirect_uri=${encodeURIComponent(window.location.origin + '/auth-callback')}&` +
      `state=${provider}`;

    window.location.href = authUrl;
  }

  async loginWithEmail(email: string, password: string): Promise<boolean> {
    try {
      // Use withCredentials to ensure cookies are sent and received
      const response = await this.http.post<any>(
        `${this.apiUrl}/account/login`,
        { email, password },
        { withCredentials: true }
      ).toPromise();

      if (response && response.success) {
        // Wait a bit to ensure cookie is set
        await new Promise(resolve => setTimeout(resolve, 100));
        
        // DO NOT initiate a new OAuth flow here!
        // The login component will redirect back to the ReturnUrl (which is /connect/authorize with original OAuth params)
        // OpenIddict will handle the authorization flow and redirect to the correct redirect_uri
        // Calling initCodeFlow() here would create a NEW OAuth flow with the wrong redirect_uri
        return true;
      }
      return false;
    } catch (error) {
      console.error('Login error:', error);
      return false;
    }
  }

  async register(email: string, password: string, displayName?: string): Promise<boolean> {
    try {
      const response = await this.http.post<any>(`${this.apiUrl}/account/register`, {
        email,
        password,
        displayName
      }).toPromise();

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
        const headers = new HttpHeaders({
          'Authorization': `Bearer ${token}`
        });

        const userInfo = await this.http.get<UserInfo>(`${this.apiUrl}/account/userinfo`, { headers }).toPromise();
        if (userInfo) {
          this.userInfoSubject.next(userInfo);
        }
      } catch (error) {
        console.error('Error loading user profile:', error);
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
    this.oauthService.logOut();
    this.userInfoSubject.next(null);
    this.router.navigate(['/login']);
  }

  getUserInfo(): UserInfo | null {
    return this.userInfoSubject.value;
  }
}

