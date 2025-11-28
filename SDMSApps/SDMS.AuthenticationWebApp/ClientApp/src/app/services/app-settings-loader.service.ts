import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { AppSettings } from '../config/app-settings';

export interface AppSettingsConfig {
  SDMS_AuthenticationWebApp_url?: string;
  SDMS_AuthenticationWebApp_clientid?: string;
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
   * 
   * BREAKING CHANGE: No hardcoded defaults. Configuration must be provided via appsettings.json or env vars.
   */
  async loadAppSettings(): Promise<void> {
    try {
      // Try assets/appsettings.json first
      try {
        const config = await firstValueFrom(
          this.http.get<AppSettingsConfig>('/assets/appsettings.json')
        );
        if (config) {
          console.log('✓ AppSettings loaded from /assets/appsettings.json');
          AppSettings.initialize(config);
          return;
        }
      } catch (error1) {
        // Try root appsettings.json as fallback
        try {
          const rootConfig = await firstValueFrom(
            this.http.get<any>('/appsettings.json')
          );
          if (rootConfig) {
            console.log('✓ AppSettings loaded from /appsettings.json');
            const appConfig: AppSettingsConfig = {
              SDMS_AuthenticationWebApp_url: rootConfig.SDMS_AuthenticationWebApp_url,
              SDMS_AuthenticationWebApp_clientid: rootConfig.SDMS_AuthenticationWebApp_clientid
            };
            AppSettings.initialize(appConfig);
            return;
          }
        } catch (error2) {
          throw new Error(
            'Could not load appsettings.json files. ' +
            'Ensure /assets/appsettings.json or /appsettings.json exists with required configuration. ' +
            'For local development, create appsettings.json with localhost values.'
          );
        }
      }
    } catch (error) {
      console.error('Error loading appsettings:', error);
      throw error;
    }
  }
}

/**
 * Standalone function to load appsettings before Angular bootstrap
 * This can be called from main.ts without Angular DI
 * 
 * Configuration Priority:
 * 1. Environment Variables (process.env.SDMS_*) - HIGHEST PRIORITY
 * 2. appsettings.json file - Fallback for local development
 * 3. Error if missing - No hardcoded defaults
 * 
 * Note: appsettings.json is updated at build time by CI/CD
 * which reads environment variables and updates the file before Angular build.
 * 
 * BREAKING CHANGE: No hardcoded defaults. Configuration must be provided.
 */
export async function loadAppSettingsBeforeBootstrap(): Promise<void> {
  return new Promise<void>((resolve, reject) => {
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
        AppSettings.initialize(config);
        resolve();
      })
      .catch((error) => {
        const errorMessage = 
          'Could not load /assets/appsettings.json. ' +
          'Ensure appsettings.json exists with required configuration. ' +
          'For local development, create appsettings.json with localhost values. ' +
          'For production, ensure environment variables (SDMS_*) are set in CI/CD.';
        console.error(errorMessage, error);
        reject(new Error(errorMessage));
      });
  });
}

