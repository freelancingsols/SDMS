/**
 * Static AppSettings class that holds application configuration
 * 
 * CONFIGURATION FLOW (Simple):
 * 1. Build: CI/CD/Vercel reads process.env → updates src/assets/appsettings.json (file already exists in source)
 * 2. Runtime: loadAppSettingsBeforeBootstrap() reads /assets/appsettings.json → AppSettings.initialize()
 * 3. Usage: Services/components import and use AppSettings directly
 * 
 * CONFIGURATION SOURCES:
 * 1. Environment variables (process.env.SDMS_*) - Set by CI/CD/Vercel, override file values at build time
 * 2. src/assets/appsettings.json - Template file (committed to repo) with default values
 * 3. Default hardcoded values - Fallback if file not found
 * 
 * HOW IT WORKS:
 * - src/assets/appsettings.json exists in source (template with defaults)
 * - CI/CD/Vercel: Sets environment variables from GitHub secrets
 * - Build: CI/CD/Vercel reads process.env → Updates appsettings.json file in place (inline command, no separate script)
 * - Runtime: loadAppSettingsBeforeBootstrap() → Reads appsettings.json → Initializes AppSettings
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
 * DO NOT use app.config.ts - use AppSettings directly
 */
export class AppSettings {
  private static _sdmsB2CWebAppUrl: string = 'http://localhost:4200';
  private static _sdmsAuthenticationWebAppUrl: string = 'http://localhost:5000';
  private static _sdmsAuthenticationWebAppClientId: string = 'sdms_frontend';
  private static _sdmsAuthenticationWebAppRedirectUri: string = 'http://localhost:4200/auth-callback';
  private static _sdmsAuthenticationWebAppScope: string = 'openid profile email roles api';

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

  /**
   * Initialize AppSettings from a configuration object
   */
  static initialize(config: {
    SDMS_B2CWebApp_url?: string;
    SDMS_AuthenticationWebApp_url?: string;
    SDMS_AuthenticationWebApp_clientid?: string;
    SDMS_AuthenticationWebApp_redirectUri?: string;
    SDMS_AuthenticationWebApp_scope?: string;
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
  }
}

