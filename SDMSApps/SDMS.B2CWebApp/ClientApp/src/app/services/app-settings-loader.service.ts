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
   * Values are replaced during build from environment variables (Vercel)
   */
  async loadAppSettings(): Promise<void> {
    try {
      let config: AppSettingsConfig;
      try {
        // Try assets/appsettings.json first
        try {
          config = await this.http.get<AppSettingsConfig>('/assets/appsettings.json').toPromise();
          if (config) {
            console.log('AppSettings loaded from assets/appsettings.json');
            AppSettings.initialize(config);
            return;
          }
        } catch (error1) {
          // Try root appsettings.json as fallback
          try {
            config = await this.http.get<AppSettingsConfig>('/appsettings.json').toPromise();
            if (config) {
              console.log('AppSettings loaded from root appsettings.json');
              AppSettings.initialize(config);
              return;
            }
          } catch (error2) {
            throw error2;
          }
        }
      } catch (error) {
        console.warn('Could not load appsettings.json, using hardcoded defaults:', error);
        // Use default values already set in AppSettings
        config = {
          SDMS_B2CWebApp_url: 'http://localhost:4200',
          SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
          SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
          SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
          SDMS_AuthenticationWebApp_scope: 'openid profile email roles api'
        };
        AppSettings.initialize(config);
      }
    } catch (error) {
      console.error('Error loading appsettings:', error);
      // Use default values already set in AppSettings
    }
  }
}

/**
 * Standalone function to load appsettings before Angular bootstrap
 * This can be called from main.ts without Angular DI
 * Loads configuration from appsettings.json (values replaced during build from environment variables)
 */
export async function loadAppSettingsBeforeBootstrap(): Promise<void> {
  return new Promise<void>((resolve, reject) => {
    try {
      // Try assets/appsettings.json first
      fetch('/assets/appsettings.json')
        .then(response => {
          if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
          }
          return response.json();
        })
        .then((config: AppSettingsConfig) => {
          console.log('AppSettings loaded from assets/appsettings.json');
          AppSettings.initialize(config);
          resolve();
        })
        .catch(error => {
          console.warn('Could not load assets/appsettings.json, trying root appsettings.json:', error);
          // Try root appsettings.json as fallback
          fetch('/appsettings.json')
            .then(response => {
              if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
              }
              return response.json();
            })
            .then((config: AppSettingsConfig) => {
              console.log('AppSettings loaded from root appsettings.json');
              AppSettings.initialize(config);
              resolve();
            })
            .catch(error2 => {
              console.warn('Could not load appsettings.json, using default values:', error2);
              // Use default values already set in AppSettings
              const defaultConfig: AppSettingsConfig = {
                SDMS_B2CWebApp_url: 'http://localhost:4200',
                SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
                SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
                SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
                SDMS_AuthenticationWebApp_scope: 'openid profile email roles api'
              };
              AppSettings.initialize(defaultConfig);
              resolve();
            });
        });
    } catch (error) {
      console.error('Error loading appsettings:', error);
      // Continue with default values already set in AppSettings
      resolve();
    }
  });
}

