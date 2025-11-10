import { Injectable, Inject } from '@angular/core';
import { BehaviorSubject, concat, Observable, of } from 'rxjs';
import { filter, map, take, tap } from 'rxjs/operators';
import { OAuthService } from 'angular-oauth2-oidc';

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
  constructor(
    @Inject('BASE_URL') private baseUrl: string,
    private oauthService: OAuthService
  ) {
  }
  // By default pop ups are disabled because they don't work properly on Edge.
  // If you want to enable pop up authentication simply set this flag to false.

  private userSubject: BehaviorSubject<IUser | null> = new BehaviorSubject<IUser | null>(null);

  public isAuthenticated(): Observable<boolean> {
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
      // Try silent refresh first
      if (this.oauthService.hasValidAccessToken()) {
        await this.loadUserProfile();
        return this.success(state);
      }

      // If silent refresh fails, try code flow
      try {
        this.oauthService.initCodeFlow();
        return this.redirect();
      } catch (redirectError) {
        console.log('Redirect authentication error: ', redirectError);
        return this.error(String(redirectError));
      }
    } catch (error) {
      console.log('Authentication error: ', error);
      return this.error(String(error));
    }
  }

  public async completeSignIn(_url: string, _callbackAction: string): Promise<IAuthenticationResult> {
    try {
      // OAuth callback is handled by angular-oauth2-oidc automatically
      await this.oauthService.loadDiscoveryDocumentAndTryLogin();
      await this.loadUserProfile();
      return this.success(null);
    } catch (error) {
      console.log('There was an error signing in: ', error);
      return this.error('There was an error signing in.');
    }
  }

  public async signOut(state: any): Promise<IAuthenticationResult> {
    try {
      this.oauthService.logOut();
      this.userSubject.next(null);
      return this.success(state);
    } catch (error) {
      console.log('Signout error: ', error);
      return this.error(String(error));
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
        const claims = this.oauthService.getIdentityClaims();
        if (claims) {
          const user: IUser = {
            name: claims['name'] || claims['sub'] || ''
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
            name: claims['name'] || claims['sub'] || ''
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
