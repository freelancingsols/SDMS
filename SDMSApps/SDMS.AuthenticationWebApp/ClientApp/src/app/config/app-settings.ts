/**
 * Static AppSettings class that holds application configuration
 * 
 * CONFIGURATION FLOW:
 * 1. Build: CI/CD reads process.env.SDMS_* → updates src/assets/appsettings.json
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
 * - CI/CD: Sets environment variables from GitHub secrets
 * - Build: CI/CD reads process.env → Updates appsettings.json file in place
 * - Runtime: loadAppSettingsBeforeBootstrap() → Reads appsettings.json → Initializes AppSettings
 * 
 * Usage example:
 * ```typescript
 * import { AppSettings } from './config/app-settings';
 * 
 * const authServer = AppSettings.SDMS_AuthenticationWebApp_url;
 * const clientId = AppSettings.SDMS_AuthenticationWebApp_clientid;
 * ```
 * 
 * Note: AppSettings is initialized before Angular bootstrap in main.ts
 * You can use it anywhere in the application by importing and accessing the static properties
 * 
 * BREAKING CHANGE: All hardcoded URL defaults removed. Configuration now required.
 */
export class AppSettings {
  private static _sdmsAuthenticationWebAppUrl: string | undefined;
  private static _sdmsAuthenticationWebAppClientId: string | undefined;

  // Getters (throw error if not initialized)
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

  // Setters (used during initialization)
  static set SDMS_AuthenticationWebApp_url(value: string) {
    this._sdmsAuthenticationWebAppUrl = value;
  }

  static set SDMS_AuthenticationWebApp_clientid(value: string) {
    this._sdmsAuthenticationWebAppClientId = value;
  }

  /**
   * Initialize AppSettings from a configuration object
   * Throws error if required configuration is missing
   */
  static initialize(config: {
    SDMS_AuthenticationWebApp_url?: string;
    SDMS_AuthenticationWebApp_clientid?: string;
  }): void {
    const missing: string[] = [];
    
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
    
    if (missing.length > 0) {
      throw new Error(
        `Missing required configuration: ${missing.join(', ')}. ` +
        `Ensure appsettings.json exists with these values or set environment variables (SDMS_*).`
      );
    }
  }
}

