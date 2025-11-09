// Build-time environment variable replacement script
// This script replaces environment variables in appsettings.json during build
// Vercel will set these environment variables during deployment
// Uses SDMS_* naming convention for all configuration keys

const fs = require('fs');
const path = require('path');

// Generate appsettings.json for runtime loading
// Read from root appsettings.json as template and replace values from environment variables
const rootAppSettingsPath = path.join(__dirname, '..', 'appsettings.json');
let appSettingsConfig = {
  SDMS_B2CWebApp_url: 'http://localhost:4200',
  SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
  SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
  SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
  SDMS_AuthenticationWebApp_scope: 'openid profile email roles api'
};

// Try to read from root appsettings.json
if (fs.existsSync(rootAppSettingsPath)) {
  try {
    const rootAppSettings = JSON.parse(fs.readFileSync(rootAppSettingsPath, 'utf8'));
    
    // Use values from environment variables if provided (Vercel deployment), otherwise use appsettings.json values
    appSettingsConfig.SDMS_B2CWebApp_url = process.env.SDMS_B2CWebApp_url || rootAppSettings.SDMS_B2CWebApp_url || appSettingsConfig.SDMS_B2CWebApp_url;
    appSettingsConfig.SDMS_AuthenticationWebApp_url = process.env.SDMS_AuthenticationWebApp_url || rootAppSettings.SDMS_AuthenticationWebApp_url || appSettingsConfig.SDMS_AuthenticationWebApp_url;
    appSettingsConfig.SDMS_AuthenticationWebApp_clientid = process.env.SDMS_AuthenticationWebApp_clientid || rootAppSettings.SDMS_AuthenticationWebApp_clientid || appSettingsConfig.SDMS_AuthenticationWebApp_clientid;
    appSettingsConfig.SDMS_AuthenticationWebApp_redirectUri = process.env.SDMS_AuthenticationWebApp_redirectUri || rootAppSettings.SDMS_AuthenticationWebApp_redirectUri || appSettingsConfig.SDMS_AuthenticationWebApp_redirectUri;
    appSettingsConfig.SDMS_AuthenticationWebApp_scope = process.env.SDMS_AuthenticationWebApp_scope || rootAppSettings.SDMS_AuthenticationWebApp_scope || appSettingsConfig.SDMS_AuthenticationWebApp_scope;
    
    console.log('Loaded appsettings from root appsettings.json (with environment variable overrides)');
  } catch (error) {
    console.warn('Could not read root appsettings.json, using environment variables or defaults:', error);
    // Use environment variables or defaults
    appSettingsConfig.SDMS_B2CWebApp_url = process.env.SDMS_B2CWebApp_url || appSettingsConfig.SDMS_B2CWebApp_url;
    appSettingsConfig.SDMS_AuthenticationWebApp_url = process.env.SDMS_AuthenticationWebApp_url || appSettingsConfig.SDMS_AuthenticationWebApp_url;
    appSettingsConfig.SDMS_AuthenticationWebApp_clientid = process.env.SDMS_AuthenticationWebApp_clientid || appSettingsConfig.SDMS_AuthenticationWebApp_clientid;
    appSettingsConfig.SDMS_AuthenticationWebApp_redirectUri = process.env.SDMS_AuthenticationWebApp_redirectUri || appSettingsConfig.SDMS_AuthenticationWebApp_redirectUri;
    appSettingsConfig.SDMS_AuthenticationWebApp_scope = process.env.SDMS_AuthenticationWebApp_scope || appSettingsConfig.SDMS_AuthenticationWebApp_scope;
  }
} else {
  // Use environment variables or defaults
  console.warn('Root appsettings.json not found, using environment variables or hardcoded defaults');
  appSettingsConfig.SDMS_B2CWebApp_url = process.env.SDMS_B2CWebApp_url || appSettingsConfig.SDMS_B2CWebApp_url;
  appSettingsConfig.SDMS_AuthenticationWebApp_url = process.env.SDMS_AuthenticationWebApp_url || appSettingsConfig.SDMS_AuthenticationWebApp_url;
  appSettingsConfig.SDMS_AuthenticationWebApp_clientid = process.env.SDMS_AuthenticationWebApp_clientid || appSettingsConfig.SDMS_AuthenticationWebApp_clientid;
  appSettingsConfig.SDMS_AuthenticationWebApp_redirectUri = process.env.SDMS_AuthenticationWebApp_redirectUri || appSettingsConfig.SDMS_AuthenticationWebApp_redirectUri;
  appSettingsConfig.SDMS_AuthenticationWebApp_scope = process.env.SDMS_AuthenticationWebApp_scope || appSettingsConfig.SDMS_AuthenticationWebApp_scope;
}

// Create appsettings.json in src/assets so it gets copied to dist
const appSettingsPath = path.join(__dirname, 'src', 'assets', 'appsettings.json');
const appSettingsDir = path.dirname(appSettingsPath);

// Ensure directory exists
if (!fs.existsSync(appSettingsDir)) {
  fs.mkdirSync(appSettingsDir, { recursive: true });
}

// Write appsettings.json (only contains the config values, not Logging, etc.)
fs.writeFileSync(appSettingsPath, JSON.stringify(appSettingsConfig, null, 2), 'utf8');
console.log('appsettings.json generated in src/assets');

console.log('Build environment setup completed successfully');
console.log('  - AppSettings:', JSON.stringify(appSettingsConfig, null, 2));

