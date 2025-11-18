// Build-time environment variable replacement script
// This script replaces environment variables in appsettings.json during build
// CI/CD will set these environment variables during deployment
// Uses SDMS_* naming convention for all configuration keys
//
// BREAKING CHANGE: No hardcoded defaults. Configuration must be provided via:
// 1. Environment Variables (process.env.SDMS_*) - HIGHEST PRIORITY
// 2. appsettings.json file - Fallback for local development
// 3. Error if missing - No hardcoded defaults

const fs = require('fs');
const path = require('path');

// Generate appsettings.json for runtime loading
// Read from root appsettings.json as template and replace values from environment variables
const rootAppSettingsPath = path.join(__dirname, '..', 'appsettings.json');
let appSettingsConfig = {};

// Try to read from root appsettings.json first
if (fs.existsSync(rootAppSettingsPath)) {
  try {
    const rootAppSettings = JSON.parse(fs.readFileSync(rootAppSettingsPath, 'utf8'));
    
    // Use values from environment variables if provided (CI/CD deployment), otherwise use appsettings.json values
    appSettingsConfig.SDMS_AuthenticationWebApp_url = process.env.SDMS_AuthenticationWebApp_url || rootAppSettings.SDMS_AuthenticationWebApp_url;
    appSettingsConfig.SDMS_AuthenticationWebApp_clientid = process.env.SDMS_AuthenticationWebApp_clientid || rootAppSettings.SDMS_AuthenticationWebApp_clientid;
    
    console.log('Loaded appsettings from root appsettings.json (with environment variable overrides)');
  } catch (error) {
    console.warn('Could not read root appsettings.json, using environment variables only:', error);
    // Use environment variables only
    appSettingsConfig.SDMS_AuthenticationWebApp_url = process.env.SDMS_AuthenticationWebApp_url;
    appSettingsConfig.SDMS_AuthenticationWebApp_clientid = process.env.SDMS_AuthenticationWebApp_clientid;
  }
} else {
  // Use environment variables only
  console.warn('Root appsettings.json not found, using environment variables only');
  appSettingsConfig.SDMS_AuthenticationWebApp_url = process.env.SDMS_AuthenticationWebApp_url;
  appSettingsConfig.SDMS_AuthenticationWebApp_clientid = process.env.SDMS_AuthenticationWebApp_clientid;
}

// Validate required configuration
const missing = [];
if (!appSettingsConfig.SDMS_AuthenticationWebApp_url) missing.push('SDMS_AuthenticationWebApp_url');
if (!appSettingsConfig.SDMS_AuthenticationWebApp_clientid) missing.push('SDMS_AuthenticationWebApp_clientid');

if (missing.length > 0) {
  console.error('❌ ERROR: Missing required configuration!');
  console.error('   Missing:', missing.join(', '));
  console.error('   Ensure appsettings.json exists with these values or set environment variables (SDMS_*).');
  console.error('   BREAKING CHANGE: No hardcoded defaults. Configuration is required.');
  process.exit(1);
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

