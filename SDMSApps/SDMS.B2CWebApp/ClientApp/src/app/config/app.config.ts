import { AppSettings } from './app-settings';

export interface AppConfig {
  apiUrl: string;
  authServer: string;
  clientId: string;
  redirectUri: string;
  scope: string;
}

export const appConfig: AppConfig = {
  apiUrl: AppSettings.SDMS_AuthenticationWebApp_url,
  authServer: AppSettings.SDMS_AuthenticationWebApp_url,
  clientId: AppSettings.SDMS_AuthenticationWebApp_clientid,
  redirectUri: AppSettings.SDMS_AuthenticationWebApp_redirectUri,
  scope: AppSettings.SDMS_AuthenticationWebApp_scope
};

