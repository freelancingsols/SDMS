import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AppSettings } from '../config/app-settings';

export interface AppSettingsConfig {
  SDMS_B2CWebApp_url?: string;
  SDMS_AuthenticationWebApp_url?: string;
  SDMS_AuthenticationWebApp_clientid?: string;
  SDMS_AuthenticationWebApp_redirectUri?: string;
  SDMS_AuthenticationWebApp_scope?: string;
  GITHUB_SECRETS_URL?: string;
}

/**
 * Service to load appsettings from GitHub secrets
 * This service is used before Angular bootstrap to load configuration
 */
@Injectable({
  providedIn: 'root'
})
export class AppSettingsLoaderService {
  constructor(private http: HttpClient) {}

  /**
   * Load appsettings from GitHub secrets URL
   * 
   * Flow:
   * 1. Load appsettings.json to get GITHUB_SECRETS_URL and defaults
   * 2. If GITHUB_SECRETS_URL is set, fetch SDMS_* configs from GitHub secrets
   * 3. If GitHub secrets fetch fails or GITHUB_SECRETS_URL is empty, use defaults from appsettings.json
   */
  async loadAppSettings(): Promise<void> {
    try {
      // Step 1: Load appsettings.json to get GITHUB_SECRETS_URL and defaults
      let defaultConfig: AppSettingsConfig;
      try {
        // Try assets/appsettings.json first
        try {
          defaultConfig = await this.http.get<AppSettingsConfig>('/assets/appsettings.json').toPromise();
          if (defaultConfig) {
            console.log('appsettings.json loaded from assets/appsettings.json');
          }
        } catch (error1) {
          // Try root appsettings.json as fallback
          try {
            defaultConfig = await this.http.get<AppSettingsConfig>('/appsettings.json').toPromise();
            if (defaultConfig) {
              console.log('appsettings.json loaded from root appsettings.json');
            }
          } catch (error2) {
            throw error2;
          }
        }
      } catch (error) {
        console.warn('Could not load appsettings.json, using hardcoded defaults:', error);
        defaultConfig = {
          SDMS_B2CWebApp_url: 'http://localhost:4200',
          SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
          SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
          SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
          SDMS_AuthenticationWebApp_scope: 'openid profile email roles api',
          GITHUB_SECRETS_URL: ''
        };
      }

      const githubSecretsUrl = defaultConfig.GITHUB_SECRETS_URL || this.getGitHubSecretsUrl();

      // Step 2: If GITHUB_SECRETS_URL is set, fetch SDMS_* configs from GitHub secrets
      if (githubSecretsUrl && githubSecretsUrl.trim() !== '') {
        console.log('Loading SDMS_* configs from GitHub secrets:', githubSecretsUrl);
        try {
          const secretsConfig = await this.http.get<AppSettingsConfig>(githubSecretsUrl).toPromise();
          if (secretsConfig) {
            // Merge GitHub secrets config with defaults (secrets take precedence)
            const mergedConfig: AppSettingsConfig = {
              ...defaultConfig,
              ...secretsConfig,
              // Keep GITHUB_SECRETS_URL from appsettings.json
              GITHUB_SECRETS_URL: defaultConfig.GITHUB_SECRETS_URL || githubSecretsUrl
            };
            AppSettings.initialize(mergedConfig);
            console.log('AppSettings loaded successfully from GitHub secrets');
            return;
          }
        } catch (error) {
          console.warn('Failed to load from GitHub secrets, using defaults from appsettings.json:', error);
        }
      }

      // Step 3: No GITHUB_SECRETS_URL or fetch failed, use defaults from appsettings.json
      console.log('Using defaults from appsettings.json');
      AppSettings.initialize(defaultConfig);
    } catch (error) {
      console.error('Error loading appsettings:', error);
      // Use default values already set in AppSettings
    }
  }

  /**
   * Get GitHub secrets URL from environment variables or build-time config
   */
  private getGitHubSecretsUrl(): string {
    // Check for environment variable (set during Vercel deployment)
    if ((window as any).__GITHUB_SECRETS_URL__) {
      return (window as any).__GITHUB_SECRETS_URL__;
    }

    // Check for process.env (available during build)
    if (typeof process !== 'undefined' && process.env && process.env['GITHUB_SECRETS_URL']) {
      return process.env['GITHUB_SECRETS_URL'];
    }

    // Check for meta tag in index.html
    const metaTag = document.querySelector('meta[name="github-secrets-url"]');
    if (metaTag) {
      return metaTag.getAttribute('content') || '';
    }

    return '';
  }

}

/**
 * Standalone function to load appsettings before Angular bootstrap
 * This can be called from main.ts without Angular DI
 * 
 * Flow:
 * 1. Load appsettings.json to get GITHUB_SECRETS_URL
 * 2. If GITHUB_SECRETS_URL is set, fetch SDMS_* configs from GitHub secrets
 * 3. If GitHub secrets fetch fails or GITHUB_SECRETS_URL is empty, use defaults from appsettings.json
 */
export async function loadAppSettingsBeforeBootstrap(): Promise<void> {
  return new Promise<void>((resolve, reject) => {
    try {
      // Step 1: Load appsettings.json to get GITHUB_SECRETS_URL and defaults
      loadFromLocalAppSettings((defaultConfig: AppSettingsConfig) => {
        const githubSecretsUrl = defaultConfig.GITHUB_SECRETS_URL || getGitHubSecretsUrlStatic();
        
        // Step 2: If GITHUB_SECRETS_URL is set, fetch configs from GitHub secrets
        if (githubSecretsUrl && githubSecretsUrl.trim() !== '') {
          console.log('Loading SDMS_* configs from GitHub secrets:', githubSecretsUrl);
          fetch(githubSecretsUrl)
            .then(response => {
              if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
              }
              return response.json();
            })
            .then((secretsConfig: AppSettingsConfig) => {
              // Merge GitHub secrets config with defaults (secrets take precedence)
              const mergedConfig: AppSettingsConfig = {
                ...defaultConfig,
                ...secretsConfig,
                // Keep GITHUB_SECRETS_URL from appsettings.json
                GITHUB_SECRETS_URL: defaultConfig.GITHUB_SECRETS_URL || githubSecretsUrl
              };
              AppSettings.initialize(mergedConfig);
              console.log('AppSettings loaded successfully from GitHub secrets');
              resolve();
            })
            .catch(error => {
              console.warn('Failed to load from GitHub secrets, using defaults from appsettings.json:', error);
              // Use defaults from appsettings.json
              AppSettings.initialize(defaultConfig);
              resolve();
            });
        } else {
          // Step 3: No GITHUB_SECRETS_URL, use defaults from appsettings.json
          console.log('No GitHub secrets URL found, using defaults from appsettings.json');
          AppSettings.initialize(defaultConfig);
          resolve();
        }
      }, reject);
    } catch (error) {
      console.error('Error loading appsettings:', error);
      // Continue with default values already set in AppSettings
      resolve();
    }
  });
}

function getGitHubSecretsUrlStatic(): string {
  // Check for window variable (injected during build)
  if (typeof window !== 'undefined' && (window as any).__GITHUB_SECRETS_URL__) {
    return (window as any).__GITHUB_SECRETS_URL__;
  }

  // Check for meta tag
  if (typeof document !== 'undefined') {
    const metaTag = document.querySelector('meta[name="github-secrets-url"]');
    if (metaTag) {
      return metaTag.getAttribute('content') || '';
    }
  }

  return '';
}

function loadFromLocalAppSettings(
  resolve: (config: AppSettingsConfig) => void, 
  reject: (error: any) => void
): void {
  // Try assets/appsettings.json first
  fetch('/assets/appsettings.json')
    .then(response => {
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      return response.json();
    })
    .then((config: AppSettingsConfig) => {
      console.log('appsettings.json loaded from assets/appsettings.json');
      resolve(config);
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
          console.log('appsettings.json loaded from root appsettings.json');
          resolve(config);
        })
        .catch(error2 => {
          console.warn('Could not load appsettings.json, using default values:', error2);
          // Return default config with empty GITHUB_SECRETS_URL
          const defaultConfig: AppSettingsConfig = {
            SDMS_B2CWebApp_url: 'http://localhost:4200',
            SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
            SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
            SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
            SDMS_AuthenticationWebApp_scope: 'openid profile email roles api',
            GITHUB_SECRETS_URL: ''
          };
          resolve(defaultConfig);
        });
    });
}

