/**
 * Static AppSettings class that holds application configuration
 * 
 * CONFIGURATION FLOW:
 * 1. Build: CI/CD/Vercel reads process.env.SDMS_* → updates src/assets/appsettings.json
 * 2. Runtime: loadAppSettingsBeforeBootstrap() reads /assets/appsettings.json → AppSettings.initialize()
 * 3. Usage: Services/components import and use AppSettings directly
 * 
 * CONFIGURATION PRIORITY:
 * 1. Environment Variables (process.env.SDMS_*) - HIGHEST PRIORITY
 * 2. appsettings.json file - Fallback for local development
 * 3. Error if missing - No hardcoded defaults
 * 
 * HOW IT WORKS:
 * - src/assets/appsettings.json exists in source (template with localhost values for local dev)
 * - CI/CD/Vercel: Sets environment variables from GitHub secrets/Vercel env vars
 * - Build: CI/CD/Vercel reads process.env → Updates appsettings.json file in place
 * - Runtime: loadAppSettingsBeforeBootstrap() → Reads appsettings.json → Initializes AppSettings
 * 
 * Usage example:
 * ```typescript
 * import { AppSettings } from './config/app-settings';
 * 
 * const apiUrl = AppSettings.SDMS_AuthenticationWebApp_url;
 * const clientId = AppSettings.SDMS_AuthenticationWebApp_clientid;
 * const redirectUri = AppSettings.SDMS_AuthenticationWebApp_redirectUri;
 * const postLogoutRedirectUri = AppSettings.SDMS_AuthenticationWebApp_postLogoutRedirectUri;
 * ```
 * 
 * Note: AppSettings is initialized before Angular bootstrap in main.ts
 * You can use it anywhere in the application by importing and accessing the static properties
 * DO NOT use app.config.ts - use AppSettings directly
 * 
 * BREAKING CHANGE: All hardcoded URL defaults removed. Configuration now required.
 */
export class AppSettings {
  private static _sdmsB2CWebAppUrl: string | undefined;
  private static _sdmsAuthenticationWebAppUrl: string | undefined;
  private static _sdmsAuthenticationWebAppClientId: string | undefined;
  private static _sdmsAuthenticationWebAppRedirectUri: string | undefined;
  private static _sdmsAuthenticationWebAppPostLogoutRedirectUri: string | undefined;
  private static _sdmsAuthenticationWebAppScope: string | undefined;

  // Getters (throw error if not initialized)
  static get SDMS_B2CWebApp_url(): string {
    if (!this._sdmsB2CWebAppUrl) {
      throw new Error('SDMS_B2CWebApp_url is not configured. Ensure appsettings.json exists or environment variables are set.');
    }
    return this._sdmsB2CWebAppUrl;
  }

  static get SDMS_AuthenticationWebApp_url(): string {
    if (!this._sdmsAuthenticationWebAppUrl) {
      throw new Error('SDMS_AuthenticationWebApp_url is not configured. Ensure appsettings.json exists or environment variables are set.');
    }
    return this._sdmsAuthenticationWebAppUrl;
  }

  static get SDMS_AuthenticationWebApp_clientid(): string {
    if (!this._sdmsAuthenticationWebAppClientId) {
      throw new Error('SDMS_AuthenticationWebApp_clientid is not configured. Ensure appsettings.json exists or environment variables are set.');
    }
    return this._sdmsAuthenticationWebAppClientId;
  }

  static get SDMS_AuthenticationWebApp_redirectUri(): string {
    // Generate from B2CWebApp_url if not explicitly set
    if (!this._sdmsAuthenticationWebAppRedirectUri) {
      if (!this._sdmsB2CWebAppUrl) {
        throw new Error('SDMS_B2CWebApp_url is not configured. Cannot generate redirectUri. Ensure appsettings.json exists or environment variables are set.');
      }
      // Generate redirectUri from B2CWebApp_url
      return `${this._sdmsB2CWebAppUrl}/auth-callback`;
    }
    return this._sdmsAuthenticationWebAppRedirectUri;
  }

  static get SDMS_AuthenticationWebApp_postLogoutRedirectUri(): string {
    // Generate from B2CWebApp_url if not explicitly set
    if (!this._sdmsAuthenticationWebAppPostLogoutRedirectUri) {
      if (!this._sdmsB2CWebAppUrl) {
        throw new Error('SDMS_B2CWebApp_url is not configured. Cannot generate postLogoutRedirectUri. Ensure appsettings.json exists or environment variables are set.');
      }
      // Generate postLogoutRedirectUri from B2CWebApp_url (landing page)
      // Remove trailing slash to match database format (OpenIddict does exact matching)
      // The auth app will accept both versions, but this ensures consistency
      return this._sdmsB2CWebAppUrl.endsWith('/') 
        ? this._sdmsB2CWebAppUrl.slice(0, -1) 
        : this._sdmsB2CWebAppUrl;
    }
    // Also normalize explicitly set value to remove trailing slash for consistency
    return this._sdmsAuthenticationWebAppPostLogoutRedirectUri.endsWith('/')
      ? this._sdmsAuthenticationWebAppPostLogoutRedirectUri.slice(0, -1)
      : this._sdmsAuthenticationWebAppPostLogoutRedirectUri;
  }

  static get SDMS_AuthenticationWebApp_scope(): string {
    if (!this._sdmsAuthenticationWebAppScope) {
      throw new Error('SDMS_AuthenticationWebApp_scope is not configured. Ensure appsettings.json exists or environment variables are set.');
    }
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

  static set SDMS_AuthenticationWebApp_postLogoutRedirectUri(value: string) {
    this._sdmsAuthenticationWebAppPostLogoutRedirectUri = value;
  }

  static set SDMS_AuthenticationWebApp_scope(value: string) {
    this._sdmsAuthenticationWebAppScope = value;
  }

  /**
   * Initialize AppSettings from a configuration object
   * Throws error if required configuration is missing
   */
  static initialize(config: {
    SDMS_B2CWebApp_url?: string;
    SDMS_AuthenticationWebApp_url?: string;
    SDMS_AuthenticationWebApp_clientid?: string;
    SDMS_AuthenticationWebApp_redirectUri?: string;
    SDMS_AuthenticationWebApp_postLogoutRedirectUri?: string;
    SDMS_AuthenticationWebApp_scope?: string;
  }): void {
    const missing: string[] = [];
    
    if (!config.SDMS_B2CWebApp_url) {
      missing.push('SDMS_B2CWebApp_url');
    } else {
      this._sdmsB2CWebAppUrl = config.SDMS_B2CWebApp_url;
    }
    
    if (!config.SDMS_AuthenticationWebApp_url) {
      missing.push('SDMS_AuthenticationWebApp_url');
    } else {
      this._sdmsAuthenticationWebAppUrl = config.SDMS_AuthenticationWebApp_url;
    }
    
    if (!config.SDMS_AuthenticationWebApp_clientid) {
      missing.push('SDMS_AuthenticationWebApp_clientid');
    } else {
      this._sdmsAuthenticationWebAppClientId = config.SDMS_AuthenticationWebApp_clientid;
    }
    
    // redirectUri is optional - will be generated from B2CWebApp_url if not provided
    if (config.SDMS_AuthenticationWebApp_redirectUri) {
      this._sdmsAuthenticationWebAppRedirectUri = config.SDMS_AuthenticationWebApp_redirectUri;
    }
    
    // postLogoutRedirectUri is optional - will be generated from B2CWebApp_url if not provided
    if (config.SDMS_AuthenticationWebApp_postLogoutRedirectUri) {
      this._sdmsAuthenticationWebAppPostLogoutRedirectUri = config.SDMS_AuthenticationWebApp_postLogoutRedirectUri;
    }
    
    if (!config.SDMS_AuthenticationWebApp_scope) {
      missing.push('SDMS_AuthenticationWebApp_scope');
    } else {
      this._sdmsAuthenticationWebAppScope = config.SDMS_AuthenticationWebApp_scope;
    }
    
    if (missing.length > 0) {
      throw new Error(
        `Missing required configuration: ${missing.join(', ')}. ` +
        `Ensure appsettings.json exists with these values or set environment variables (SDMS_*).`
      );
    }
  }
}

