/**
 * Build script for Vercel deployment
 * 
 * This script:
 * 1. Reads environment variables from process.env (set by Vercel)
 * 2. Updates src/assets/appsettings.json with environment variable values
 * 3. Updates src/environments/environment.ts to set production: true
 * 4. Provides logging to help debug environment variable issues
 */

const fs = require('fs');
const path = require('path');

console.log('🚀 Starting Vercel build preparation...');
console.log('');

// Paths
const appSettingsPath = path.join(__dirname, '..', 'src', 'assets', 'appsettings.json');
const appSettingsDir = path.dirname(appSettingsPath);
const environmentPath = path.join(__dirname, '..', 'src', 'environments', 'environment.ts');

// Environment variables to check (with fallback defaults)
const envVars = {
  SDMS_B2CWebApp_url: process.env.SDMS_B2CWebApp_url,
  SDMS_AuthenticationWebApp_url: process.env.SDMS_AuthenticationWebApp_url,
  SDMS_AuthenticationWebApp_clientid: process.env.SDMS_AuthenticationWebApp_clientid,
  SDMS_AuthenticationWebApp_redirectUri: process.env.SDMS_AuthenticationWebApp_redirectUri,
  SDMS_AuthenticationWebApp_scope: process.env.SDMS_AuthenticationWebApp_scope
};

// Default values (fallback)
const defaults = {
  SDMS_B2CWebApp_url: 'http://localhost:4200',
  SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
  SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
  SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
  SDMS_AuthenticationWebApp_scope: 'openid profile email roles api'
};

console.log('📋 Environment Variables Status:');
console.log('');
console.log('🔍 Checking process.env for SDMS_* variables...');
console.log('   VERCEL_ENV:', process.env.VERCEL_ENV || 'not set');
console.log('   VERCEL: ', process.env.VERCEL || 'not set');
console.log('');

// Check which environment variables are available
let hasEnvVars = false;
const appSettings = {};

for (const [key, value] of Object.entries(envVars)) {
  // Check raw value from process.env
  const rawValue = process.env[key];
  if (rawValue !== undefined) {
    console.log(`  🔍 ${key}: found in process.env (type: ${typeof rawValue}, length: ${rawValue ? rawValue.length : 0})`);
  } else {
    console.log(`  🔍 ${key}: NOT found in process.env`);
  }
  
  if (value && value.trim().length > 0) {
    console.log(`  ✅ ${key}: present and valid (${value.length} chars)`);
    // Show first and last few characters for verification (not full value for security)
    const preview = value.length > 20 
      ? `${value.substring(0, 10)}...${value.substring(value.length - 10)}`
      : value.substring(0, Math.min(20, value.length));
    console.log(`     Preview: ${preview}`);
    appSettings[key] = value.trim();
    hasEnvVars = true;
  } else {
    console.log(`  ⚠️  ${key}: not set or empty, using default: ${defaults[key]}`);
    appSettings[key] = defaults[key];
  }
}

console.log('');
if (hasEnvVars) {
  console.log('✅ Found environment variables from Vercel');
  console.log('   These values will be used in the build');
} else {
  console.log('⚠️  WARNING: No environment variables found, using defaults!');
  console.log('   This means the application will use localhost URLs.');
  console.log('');
  console.log('   To fix this:');
  console.log('   1. Go to Vercel Dashboard → Your Project → Settings → Environment Variables');
  console.log('   2. Make sure these variables are set for "Production" environment:');
  Object.keys(envVars).forEach(key => {
    console.log(`      - ${key}`);
  });
  console.log('   3. The GitHub Actions workflow should sync these, but verify they exist in Vercel');
  console.log('   4. Redeploy after setting the variables');
}

console.log('');
console.log('📝 Updating appsettings.json...');

// Ensure assets directory exists
if (!fs.existsSync(appSettingsDir)) {
  fs.mkdirSync(appSettingsDir, { recursive: true });
}

// Write appsettings.json
try {
  fs.writeFileSync(appSettingsPath, JSON.stringify(appSettings, null, 2), 'utf8');
  console.log(`✅ Updated ${appSettingsPath}`);
  console.log('   Content:', JSON.stringify(appSettings, null, 2));
} catch (error) {
  console.error(`❌ Failed to write appsettings.json: ${error.message}`);
  process.exit(1);
}

console.log('');
console.log('📝 Updating environment.ts to set production: true...');

// Update environment.ts to set production: true
try {
  let envContent = fs.readFileSync(environmentPath, 'utf8');
  envContent = envContent.replace(/production:\s*(true|false)/g, 'production: true');
  fs.writeFileSync(environmentPath, envContent, 'utf8');
  console.log(`✅ Updated ${environmentPath}`);
} catch (error) {
  console.error(`❌ Failed to update environment.ts: ${error.message}`);
  process.exit(1);
}

console.log('');
console.log('✅ Build preparation complete!');
console.log('');

