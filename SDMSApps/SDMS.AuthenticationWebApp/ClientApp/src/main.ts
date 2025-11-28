import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { importProvidersFrom } from '@angular/core';
import { OAuthModule } from 'angular-oauth2-oidc';

import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { AuthService } from './app/services/auth.service';
import { AppSettings } from './app/config/app-settings';
import { loadAppSettingsBeforeBootstrap } from './app/services/app-settings-loader.service';

// Load appsettings before bootstrap
loadAppSettingsBeforeBootstrap()
  .then(() => {
    bootstrapApplication(AppComponent, {
      providers: [
        provideRouter(routes),
        provideHttpClient(withInterceptorsFromDi()),
        importProvidersFrom(
          OAuthModule.forRoot({
            resourceServer: {
              allowedUrls: [AppSettings.SDMS_AuthenticationWebApp_url],
              sendAccessToken: true
            }
          })
        ),
        AuthService
      ]
    }).catch(err => console.error(err));
  })
  .catch(err => {
    console.error('Failed to load appsettings:', err);
    // Still bootstrap to show error in UI
    bootstrapApplication(AppComponent, {
      providers: [
        provideRouter(routes),
        provideHttpClient(withInterceptorsFromDi()),
        importProvidersFrom(
          OAuthModule.forRoot({
            resourceServer: {
              allowedUrls: [],
              sendAccessToken: true
            }
          })
        ),
        AuthService
      ]
    }).catch(bootstrapErr => console.error('Bootstrap error:', bootstrapErr));
  });

