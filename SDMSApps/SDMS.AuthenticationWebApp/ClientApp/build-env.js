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
    const envUrl = process.env.SDMS_AuthenticationWebApp_url;
    const envClientId = process.env.SDMS_AuthenticationWebApp_clientid;
    
    appSettingsConfig.SDMS_AuthenticationWebApp_url = envUrl || rootAppSettings.SDMS_AuthenticationWebApp_url;
    appSettingsConfig.SDMS_AuthenticationWebApp_clientid = envClientId || rootAppSettings.SDMS_AuthenticationWebApp_clientid;
    
    // Log what was used (for debugging)
    console.log('📋 Configuration Source:');
    console.log(`  SDMS_AuthenticationWebApp_url: ${envUrl ? '✅ Environment Variable' : '⚠️  Fallback to appsettings.json'}`);
    console.log(`  SDMS_AuthenticationWebApp_clientid: ${envClientId ? '✅ Environment Variable' : '⚠️  Fallback to appsettings.json'}`);
    if (envUrl) {
      console.log(`  ✅ Using Railway URL: ${envUrl}`);
    } else {
      console.log(`  ⚠️  Using fallback URL: ${appSettingsConfig.SDMS_AuthenticationWebApp_url}`);
    }
    
    // Warn if using localhost in what appears to be a production build
    if (appSettingsConfig.SDMS_AuthenticationWebApp_url && appSettingsConfig.SDMS_AuthenticationWebApp_url.includes('localhost')) {
      console.warn('⚠️  WARNING: Using localhost URL in build!');
      console.warn('   This should only happen in local development.');
      console.warn('   For production, ensure SDMS_AuthenticationWebApp_url environment variable is set.');
    }
    
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
  console.error('');
  console.error('   For Railway deployment:');
  console.error('   1. Go to Railway dashboard → Your service → Variables');
  console.error('   2. Add: SDMS_AuthenticationWebApp_url = https://your-railway-url.railway.app');
  console.error('   3. Add: SDMS_AuthenticationWebApp_clientid = sdms_frontend');
  console.error('   4. Redeploy after setting variables');
  process.exit(1);
}

// Final validation: Check if using localhost in production environment
// Railway/Vercel builds: Only fail if we're CERTAIN it's a deployment build AND using localhost
// Railway sets RAILWAY_ENVIRONMENT, RAILWAY_PROJECT_ID, or PORT during builds
const isRailwayBuild = !!(process.env.RAILWAY_ENVIRONMENT || 
                          process.env.RAILWAY_PROJECT_ID || 
                          (process.env.PORT && !process.env.NODE_ENV));
const isVercelBuild = !!process.env.VERCEL;
const isCI = !!process.env.CI;
// GitHub Actions CI is just for validation - Railway will rebuild with correct variables
// Only consider it production if we're actually in Railway/Vercel, not just GitHub Actions CI
const isProductionBuild = (isRailwayBuild || isVercelBuild) && 
                          process.env.NODE_ENV !== 'development';
const isGitHubActionsCI = isCI && !isRailwayBuild && !isVercelBuild;

// Only fail if:
// 1. We're in a production build environment (Railway/Vercel, NOT GitHub Actions CI)
// 2. AND the URL is localhost
// 3. AND the environment variable was NOT set (meaning it truly fell back to appsettings.json)
const envVarWasSet = !!process.env.SDMS_AuthenticationWebApp_url;
const isLocalhost = appSettingsConfig.SDMS_AuthenticationWebApp_url && 
                    appSettingsConfig.SDMS_AuthenticationWebApp_url.includes('localhost');

if (isGitHubActionsCI && isLocalhost && !envVarWasSet) {
  // GitHub Actions CI is just for validation - Railway will rebuild with correct variables
  // Warn but don't fail - this is expected if GitHub Variables aren't set for validation build
  console.warn('⚠️  WARNING: Using localhost URL in GitHub Actions CI build (validation only)');
  console.warn('   This is OK - Railway will rebuild with correct variables from Railway environment.');
  console.warn('   For this validation build, GitHub Variables should be set, but Railway build will use Railway variables.');
  console.warn('');
  console.warn('   To fix this warning (optional):');
  console.warn('   - Set SDMS_AuthenticationWebApp_url in GitHub Variables for validation builds');
  console.warn('   - Or ignore this warning - Railway build will use Railway variables');
} else if (isProductionBuild && isLocalhost && !envVarWasSet) {
  // This is a REAL production build (Railway/Vercel) - fail if localhost
  console.error('❌ ERROR: localhost URL detected in production build!');
  console.error('   URL:', appSettingsConfig.SDMS_AuthenticationWebApp_url);
  console.error('   Environment: Railway/Vercel detected');
  console.error('   Issue: SDMS_AuthenticationWebApp_url environment variable was not set during build');
  console.error('');
  console.error('   Debug Info:');
  console.error('     RAILWAY_ENVIRONMENT:', process.env.RAILWAY_ENVIRONMENT || 'not set');
  console.error('     RAILWAY_PROJECT_ID:', process.env.RAILWAY_PROJECT_ID || 'not set');
  console.error('     PORT:', process.env.PORT || 'not set');
  console.error('     NODE_ENV:', process.env.NODE_ENV || 'not set');
  console.error('     CI:', process.env.CI || 'not set');
  console.error('');
  console.error('   Solution:');
  console.error('   1. For Railway: Set SDMS_AuthenticationWebApp_url in Railway dashboard → Variables tab');
  console.error('   2. For CI/CD: Ensure GitHub Variables are set and synced to Railway before build');
  console.error('   3. The deployment workflow should sync variables, but verify they exist in Railway');
  console.error('   4. Variables must be set in Railway BEFORE the build starts');
  process.exit(1);
} else if (isProductionBuild && isLocalhost && envVarWasSet) {
  // Environment variable was set but still contains localhost - this is a configuration error
  console.error('❌ ERROR: SDMS_AuthenticationWebApp_url environment variable is set to localhost!');
  console.error('   URL:', appSettingsConfig.SDMS_AuthenticationWebApp_url);
  console.error('   This should be set to your production Railway URL (e.g., https://your-app.railway.app)');
  console.error('   Update the environment variable in Railway dashboard → Variables tab');
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

