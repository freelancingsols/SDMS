/**
 * BREAKING CHANGE: Environment file no longer contains hardcoded URLs.
 * Configuration is now loaded from appsettings.json via AppSettings.
 * 
 * This file is kept for backward compatibility but should not be used directly.
 * Use AppSettings instead:
 * - AppSettings.SDMS_AuthenticationWebApp_url
 * - AppSettings.SDMS_AuthenticationWebApp_clientid
 */
export const environment = {
  production: true
  // Removed: authServer, clientId, apiUrl - use AppSettings instead
} as const;

