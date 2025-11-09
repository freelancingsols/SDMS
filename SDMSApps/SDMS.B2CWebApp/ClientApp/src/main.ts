import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import { loadAppSettingsBeforeBootstrap } from './app/services/app-settings-loader.service';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

const providers = [
  { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] }
];

if (environment.production) {
  enableProdMode();
}

// Load appsettings before bootstrapping Angular application
document.addEventListener('DOMContentLoaded', async () => {
  try {
    // Load appsettings from appsettings.json (generated at build time from environment variables)
    await loadAppSettingsBeforeBootstrap();
    
    // Bootstrap Angular application after settings are loaded
    platformBrowserDynamic(providers).bootstrapModule(AppModule)
      .catch(err => console.error(err));
  } catch (error) {
    console.error('Error loading appsettings before bootstrap:', error);
    // Still bootstrap even if loading settings fails
    platformBrowserDynamic(providers).bootstrapModule(AppModule)
      .catch(err => console.error(err));
  }
});
