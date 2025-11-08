/**
 * Static AppSettings class that holds application configuration
 * Values are loaded from GitHub secrets during application bootstrap
 * 
 * Usage example:
 * ```typescript
 * import { AppSettings } from './config/app-settings';
 * 
 * const apiUrl = AppSettings.SDMS_AuthenticationWebApp_url;
 * const clientId = AppSettings.SDMS_AuthenticationWebApp_clientid;
 * const redirectUri = AppSettings.SDMS_AuthenticationWebApp_redirectUri;
 * ```
 * 
 * Note: AppSettings is initialized before Angular bootstrap in main.ts
 * You can use it anywhere in the application by importing and accessing the static properties
 */
export class AppSettings {
  private static _sdmsB2CWebAppUrl: string = 'http://localhost:4200';
  private static _sdmsAuthenticationWebAppUrl: string = 'https://localhost:7001';
  private static _sdmsAuthenticationWebAppClientId: string = 'sdms_frontend';
  private static _sdmsAuthenticationWebAppRedirectUri: string = 'http://localhost:4200/auth-callback';
  private static _sdmsAuthenticationWebAppScope: string = 'openid profile email roles api';
  private static _githubSecretsUrl: string = '';

  // Getters
  static get SDMS_B2CWebApp_url(): string {
    return this._sdmsB2CWebAppUrl;
  }

  static get SDMS_AuthenticationWebApp_url(): string {
    return this._sdmsAuthenticationWebAppUrl;
  }

  static get SDMS_AuthenticationWebApp_clientid(): string {
    return this._sdmsAuthenticationWebAppClientId;
  }

  static get SDMS_AuthenticationWebApp_redirectUri(): string {
    return this._sdmsAuthenticationWebAppRedirectUri;
  }

  static get SDMS_AuthenticationWebApp_scope(): string {
    return this._sdmsAuthenticationWebAppScope;
  }

  static get GITHUB_SECRETS_URL(): string {
    return this._githubSecretsUrl;
  }

  // Setters (used during initialization)
  static set SDMS_B2CWebApp_url(value: string) {
    this._sdmsB2CWebAppUrl = value;
  }

  static set SDMS_AuthenticationWebApp_url(value: string) {
    this._sdmsAuthenticationWebAppUrl = value;
  }

  static set SDMS_AuthenticationWebApp_clientid(value: string) {
    this._sdmsAuthenticationWebAppClientId = value;
  }

  static set SDMS_AuthenticationWebApp_redirectUri(value: string) {
    this._sdmsAuthenticationWebAppRedirectUri = value;
  }

  static set SDMS_AuthenticationWebApp_scope(value: string) {
    this._sdmsAuthenticationWebAppScope = value;
  }

  static set GITHUB_SECRETS_URL(value: string) {
    this._githubSecretsUrl = value;
  }

  /**
   * Initialize AppSettings from a configuration object
   */
  static initialize(config: {
    SDMS_B2CWebApp_url?: string;
    SDMS_AuthenticationWebApp_url?: string;
    SDMS_AuthenticationWebApp_clientid?: string;
    SDMS_AuthenticationWebApp_redirectUri?: string;
    SDMS_AuthenticationWebApp_scope?: string;
    GITHUB_SECRETS_URL?: string;
  }): void {
    if (config.SDMS_B2CWebApp_url) {
      this._sdmsB2CWebAppUrl = config.SDMS_B2CWebApp_url;
    }
    if (config.SDMS_AuthenticationWebApp_url) {
      this._sdmsAuthenticationWebAppUrl = config.SDMS_AuthenticationWebApp_url;
    }
    if (config.SDMS_AuthenticationWebApp_clientid) {
      this._sdmsAuthenticationWebAppClientId = config.SDMS_AuthenticationWebApp_clientid;
    }
    if (config.SDMS_AuthenticationWebApp_redirectUri) {
      this._sdmsAuthenticationWebAppRedirectUri = config.SDMS_AuthenticationWebApp_redirectUri;
    }
    if (config.SDMS_AuthenticationWebApp_scope) {
      this._sdmsAuthenticationWebAppScope = config.SDMS_AuthenticationWebApp_scope;
    }
    if (config.GITHUB_SECRETS_URL) {
      this._githubSecretsUrl = config.GITHUB_SECRETS_URL;
    }
  }
}

