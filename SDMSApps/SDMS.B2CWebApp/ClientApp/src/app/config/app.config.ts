import { environment } from '../../environments/environment';

export interface AppConfig {
  apiUrl: string;
  authServer: string;
  clientId: string;
}

export const appConfig: AppConfig = {
  apiUrl: environment.apiUrl,
  authServer: environment.authServer,
  clientId: environment.clientId
};

