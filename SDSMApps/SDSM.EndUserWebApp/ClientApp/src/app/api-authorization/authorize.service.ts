import { Injectable, Inject, SystemJsNgModuleLoader } from '@angular/core';
import { User, UserManager, WebStorageStateStore, UserManagerSettings } from 'oidc-client';
import { BehaviorSubject, concat, from, Observable } from 'rxjs';
import { filter, map, mergeMap, take, tap } from 'rxjs/operators';
import { ApplicationPaths, ApplicationName, LoginActions, LogoutActions, CallbackActions } from './api-authorization.constants';

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
  private baseUrl: string;
  constructor(@Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
  }
  // By default pop ups are disabled because they don't work properly on Edge.
  // If you want to enable pop up authentication simply set this flag to false.

  private popUpDisabled = true;
  private userManager: UserManager;
  private userSubject: BehaviorSubject<IUser | null> = new BehaviorSubject(null);

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
    return from(this.ensureUserManagerInitialized())
      .pipe(mergeMap(() => from(this.userManager.getUser())),
        map(user => user && user.access_token));
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
    let user: User = null;
    try {
      await this.ensureUserManagerInitialized(CallbackActions.Redirect);
      await this.userManager.signinSilent(this.createArguments()).then((user1)=>
      {
        setTimeout(function () {
          console.log('waiting');
        }, 50000);
        setTimeout(function () {
          console.log('waiting');
        }, 50000);
        console.log(user1);
        user = user1;
        setTimeout(function () {
          console.log('waiting');
        }, 50000);
        this.userSubject.next(user1 && user1.profile);
        window.location.href = '/test';
      });
      user = await this.userManager.getUser();
      this.userSubject.next(user.profile);
      return this.success(state);
    } catch (silentError) {
      // User might not be authenticated, fallback to popup authentication
      console.log('Silent authentication error: ', silentError);

      try {
        if (this.popUpDisabled) {
          throw new Error('Popup disabled. Change \'authorize.service.ts:AuthorizeService.popupDisabled\' to false to enable it.');
        }
        await this.ensureUserManagerInitialized(CallbackActions.PopUp);
        user = await this.userManager.signinPopup(this.createArguments());
        user = await this.userManager.getUser();
        this.userSubject.next(user.profile);
        return this.success(state);
      } catch (popupError) {
        if (popupError.message === 'Popup window closed') {
          // The user explicitly cancelled the login action by closing an opened popup.
          return this.error('The user closed the window.');
        } else if (!this.popUpDisabled) {
          console.log('Popup authentication error: ', popupError);
        }

        // PopUps might be blocked by the user, fallback to redirect
        try {
          await this.ensureUserManagerInitialized(CallbackActions.Redirect);
          let t = this.createArguments(state);
          await this.userManager.signinRedirect(this.createArguments(state));
          return this.redirect();
        } catch (redirectError) {
          console.log('Redirect authentication error: ', redirectError);
          return this.error(redirectError);
        }
      }
    }
  }

  public async completeSignIn(url: string, callbackAction: string): Promise<IAuthenticationResult> {
    try {
      let settings: UserManagerSettings = <UserManagerSettings>
        {
          //response_mode: 'query',
        };
      var userManager = new UserManager(settings);
      var user: User;
      window.location.hash = decodeURIComponent(window.location.hash);
      if (callbackAction === CallbackActions.PopUp) {
        
        await userManager.signinPopupCallback(url)
          .then(user1 => {
            setTimeout(function () {
              console.log('waiting');
            }, 50000);
            console.log(user1);
            user = user1;
            setTimeout(function () {
              console.log('waiting');
            }, 50000);
            this.userSubject.next(user1 && user1.profile);
            window.location.href = '/test';
          })
          .catch(error => {
            console.error(error);
          });

      }
      else if (callbackAction === CallbackActions.Redirect) {
        window.location.hash = decodeURIComponent(window.location.hash);
        await userManager.signinRedirectCallback(url)
          .then(user1 => {
            setTimeout(function () {
              console.log('waiting');
            }, 50000);
            console.log(user1);
            user = user1;
            setTimeout(function () {
              console.log('waiting');
            }, 50000);
            this.userSubject.next(user1 && user1.profile);
            window.location.href = '/test';
          })
          .catch(error => {
            console.error(error);
          });
      }

      //const user = await this.userManager.signinCallback(url);
      //user = await userManager.getUser();
      //this.userSubject.next(user && user.profile);
      return this.success(user && user.state);
    } catch (error) {
      console.log('There was an error signing in: ', error);
      return this.error('There was an error signing in.');
    }
  }

  public async signOut(state: any): Promise<IAuthenticationResult> {
    try {
      if (this.popUpDisabled) {
        throw new Error('Popup disabled. Change \'authorize.service.ts:AuthorizeService.popupDisabled\' to false to enable it.');
      }

      await this.ensureUserManagerInitialized();
      await this.userManager.signoutPopup(this.createArguments());
      this.userSubject.next(null);
      return this.success(state);
    } catch (popupSignOutError) {
      console.log('Popup signout error: ', popupSignOutError);
      try {
        await this.userManager.signoutRedirect(this.createArguments(state));
        return this.redirect();
      } catch (redirectSignOutError) {
        console.log('Redirect signout error: ', popupSignOutError);
        return this.error(redirectSignOutError);
      }
    }
  }

  public async completeSignOut(url: string): Promise<IAuthenticationResult> {
    await this.ensureUserManagerInitialized();
    try {
      const response = await this.userManager.signoutCallback(url);
      this.userSubject.next(null);
      return this.success(response && response.state);
    } catch (error) {
      console.log(`There was an error trying to log out '${error}'.`);
      return this.error(error);
    }
  }

  private createArguments(state?: any): any {
    return { useReplaceToNavigate: true, data: state };
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

  private async ensureUserManagerInitialized(callBackAction: string = null): Promise<void> {

    if (callBackAction === CallbackActions.PopUp) {
      if (this.userManager !== undefined && this.userManager.settings.redirect_uri === this.baseUrl + LoginActions.LoginCallback + '-' + CallbackActions.PopUp) {
        return;
      }
      let settings: UserManagerSettings = <UserManagerSettings>
        {
          automaticSilentRenew: true,
          includeIdTokenInSilentRenew: true,
          redirect_uri: this.baseUrl + LoginActions.LoginCallback + '-' + CallbackActions.PopUp,
          post_logout_redirect_uri: this.baseUrl + LogoutActions.LogoutCallback,
          authority: "http://localhost:5001",
          client_id: "sdsm.enduser.web.app",
          response_type: "id_token token",
          //response_type:"code",
          scope: "openid profile",
          loadUserInfo: true,
          silent_redirect_uri: this.baseUrl + 'assets/'+ LoginActions.LoginCallback + '-' + CallbackActions.Silent+'.html'
        };
      this.userManager = new UserManager(settings);

    }
    else if (callBackAction === CallbackActions.Redirect) {
      if (this.userManager !== undefined && this.userManager.settings.redirect_uri === this.baseUrl + LoginActions.LoginCallback + '-' + CallbackActions.Redirect) {
        return;
      }
      let settings: UserManagerSettings = <UserManagerSettings>
        {
          automaticSilentRenew: true,
          includeIdTokenInSilentRenew: true,
          redirect_uri: this.baseUrl + LoginActions.LoginCallback + '-' + CallbackActions.Redirect,
          post_logout_redirect_uri: this.baseUrl + LogoutActions.LogoutCallback,
          authority: "http://localhost:5001",
          client_id: "sdsm.enduser.web.app",
          response_type: "id_token token",
          //response_type:"code",
          scope: "openid profile",
          loadUserInfo: true,
          silent_redirect_uri: this.baseUrl + 'assets/'+LoginActions.LoginCallback + '-' + CallbackActions.Silent+'.html'
        };
      this.userManager = new UserManager(settings);

    }
    else if (!callBackAction) {
      if (this.userManager !== undefined && this.userManager.settings.redirect_uri === this.baseUrl + LoginActions.LoginCallback) {
        return;
      }

      //const response = await fetch(ApplicationPaths.ApiAuthorizationClientConfigurationUrl);
      // if (!response.ok) {
      //   throw new Error(`Could not load settings for '${ApplicationName}'`);
      // }

      // const settings: any = await response.json();
      let settings: UserManagerSettings = <UserManagerSettings>
        {
          automaticSilentRenew: true,
          includeIdTokenInSilentRenew: true,
          redirect_uri: this.baseUrl + LoginActions.LoginCallback,
          post_logout_redirect_uri: this.baseUrl + LogoutActions.LogoutCallback,
          authority: "http://localhost:5001",
          client_id: "sdsm.enduser.web.app",
          response_type: "id_token token",
          //response_type:"code",
          scope: "openid profile",
          loadUserInfo: true,
          silent_redirect_uri: this.baseUrl + LoginActions.LoginCallback + '-' + CallbackActions.Silent
        };
      this.userManager = new UserManager(settings);

    }
    this.userManager.events.addUserSignedOut(async () => {
      await this.userManager.removeUser();
      this.userSubject.next(null);
    });
  }

  private getUserFromStorage(): Observable<IUser> {
    return from(this.ensureUserManagerInitialized())
      .pipe(
        mergeMap(() => this.userManager.getUser()),
        map(u => u && u.profile));
  }
}
