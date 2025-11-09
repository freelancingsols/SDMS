import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppSettings } from '../config/app-settings';

export interface AppSettingsConfig {
  SDMS_B2CWebApp_url?: string;
  SDMS_AuthenticationWebApp_url?: string;
  SDMS_AuthenticationWebApp_clientid?: string;
  SDMS_AuthenticationWebApp_redirectUri?: string;
  SDMS_AuthenticationWebApp_scope?: string;
}

/**
 * Service to load appsettings from appsettings.json
 * This service is used before Angular bootstrap to load configuration
 */
@Injectable({
  providedIn: 'root'
})
export class AppSettingsLoaderService {
  constructor(private http: HttpClient) {}

  /**
   * Load appsettings from appsettings.json
   * This service method is available for use within Angular DI context
   * For bootstrap, use loadAppSettingsBeforeBootstrap() instead
   */
  async loadAppSettings(): Promise<void> {
    const defaultConfig: AppSettingsConfig = {
      SDMS_B2CWebApp_url: 'http://localhost:4200',
      SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
      SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
      SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
      SDMS_AuthenticationWebApp_scope: 'openid profile email roles api'
    };

    try {
      // Try assets/appsettings.json first
      try {
        const config = await this.http.get<AppSettingsConfig>('/assets/appsettings.json').toPromise();
        if (config) {
          console.log('✓ AppSettings loaded from /assets/appsettings.json');
          const mergedConfig = { ...defaultConfig, ...config };
          AppSettings.initialize(mergedConfig);
          return;
        }
      } catch (error1) {
        // Try root appsettings.json as fallback
        try {
          const rootConfig = await this.http.get<any>('/appsettings.json').toPromise();
          if (rootConfig) {
            console.log('✓ AppSettings loaded from /appsettings.json');
            const appConfig: AppSettingsConfig = {
              SDMS_B2CWebApp_url: rootConfig.SDMS_B2CWebApp_url,
              SDMS_AuthenticationWebApp_url: rootConfig.SDMS_AuthenticationWebApp_url,
              SDMS_AuthenticationWebApp_clientid: rootConfig.SDMS_AuthenticationWebApp_clientid,
              SDMS_AuthenticationWebApp_redirectUri: rootConfig.SDMS_AuthenticationWebApp_redirectUri,
              SDMS_AuthenticationWebApp_scope: rootConfig.SDMS_AuthenticationWebApp_scope
            };
            const mergedConfig = { ...defaultConfig, ...appConfig };
            AppSettings.initialize(mergedConfig);
            return;
          }
        } catch (error2) {
          console.warn('Could not load appsettings.json files, using defaults');
          AppSettings.initialize(defaultConfig);
        }
      }
    } catch (error) {
      console.error('Error loading appsettings:', error);
      AppSettings.initialize(defaultConfig);
    }
  }
}

/**
 * Standalone function to load appsettings before Angular bootstrap
 * This can be called from main.ts without Angular DI
 * 
 * Simple flow:
 * 1. Read from /assets/appsettings.json (updated at build time from env vars)
 * 2. Fallback to hardcoded defaults if file not found
 * 
 * Note: appsettings.json is updated at build time by CI/CD/Vercel
 * which reads environment variables and updates the file before Angular build.
 */
export async function loadAppSettingsBeforeBootstrap(): Promise<void> {
  return new Promise<void>((resolve) => {
    // Default configuration (used as fallback)
    const defaultConfig: AppSettingsConfig = {
      SDMS_B2CWebApp_url: 'http://localhost:4200',
      SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
      SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
      SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
      SDMS_AuthenticationWebApp_scope: 'openid profile email roles api'
    };

    // Load from assets/appsettings.json (updated at build time from env vars)
    fetch('/assets/appsettings.json')
      .then(response => {
        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }
        return response.json();
      })
      .then((config: AppSettingsConfig) => {
        console.log('✓ AppSettings loaded from /assets/appsettings.json');
        // Merge with defaults (config values take precedence)
        const mergedConfig: AppSettingsConfig = {
          ...defaultConfig,
          ...config
        };
        AppSettings.initialize(mergedConfig);
        resolve();
      })
      .catch(() => {
        // If file not found, use hardcoded defaults
        console.warn('Could not load /assets/appsettings.json, using hardcoded defaults');
        AppSettings.initialize(defaultConfig);
        resolve();
      });
  });
}

