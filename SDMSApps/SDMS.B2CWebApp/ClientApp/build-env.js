// Build-time environment variable replacement script
// This script replaces environment variables in environment.prod.ts and index.html during build

const fs = require('fs');
const path = require('path');

const envFile = path.join(__dirname, 'src', 'environments', 'environment.prod.ts');
const indexPath = path.join(__dirname, 'src', 'index.html');

// Read environment variables with fallbacks
const apiUrl = process.env.API_URL || process.env.SDMS_AuthenticationWebApp_url || 'https://localhost:7001';
const authServer = process.env.AUTH_SERVER || process.env.SDMS_AuthenticationWebApp_url || 'https://localhost:7001';
const clientId = process.env.CLIENT_ID || process.env.SDMS_AuthenticationWebApp_clientid || 'sdms_frontend';
const githubSecretsUrl = process.env.GITHUB_SECRETS_URL || '';

// Update environment.prod.ts
if (fs.existsSync(envFile)) {
  let content = fs.readFileSync(envFile, 'utf8');

  // Replace placeholders or update values
  content = content.replace(
    /apiUrl:\s*['"`].*?['"`]/,
    `apiUrl: '${apiUrl}'`
  );
  content = content.replace(
    /authServer:\s*['"`].*?['"`]/,
    `authServer: '${authServer}'`
  );
  content = content.replace(
    /clientId:\s*['"`].*?['"`]/,
    `clientId: '${clientId}'`
  );

  // Write back
  fs.writeFileSync(envFile, content, 'utf8');
  console.log('Environment variables injected into environment.prod.ts');
}

// Update index.html with GitHub secrets URL in meta tag and window variable
if (fs.existsSync(indexPath)) {
  let htmlContent = fs.readFileSync(indexPath, 'utf8');

  // Update meta tag for github-secrets-url
  if (githubSecretsUrl) {
    htmlContent = htmlContent.replace(
      /<meta\s+name="github-secrets-url"\s+content="[^"]*">/,
      `<meta name="github-secrets-url" content="${githubSecretsUrl}">`
    );

    // Also inject as a window variable before app-root
    const windowVarScript = `\n  <script>
    window.__GITHUB_SECRETS_URL__ = '${githubSecretsUrl}';
  </script>`;
    
    // Insert before </head> or before <app-root>
    if (htmlContent.includes('</head>')) {
      htmlContent = htmlContent.replace('</head>', `${windowVarScript}\n</head>`);
    } else if (htmlContent.includes('<app-root>')) {
      htmlContent = htmlContent.replace('<app-root>', `${windowVarScript}\n  <app-root>`);
    }

    console.log('GitHub secrets URL injected into index.html');
  }

  // Write back
  fs.writeFileSync(indexPath, htmlContent, 'utf8');
}

// Generate appsettings.json for runtime loading
// Read from root appsettings.json as template and only replace GITHUB_SECRETS_URL
const rootAppSettingsPath = path.join(__dirname, '..', 'appsettings.json');
let appSettingsConfig = {
  SDMS_B2CWebApp_url: 'http://localhost:4200',
  SDMS_AuthenticationWebApp_url: 'https://localhost:7001',
  SDMS_AuthenticationWebApp_clientid: 'sdms_frontend',
  SDMS_AuthenticationWebApp_redirectUri: 'http://localhost:4200/auth-callback',
  SDMS_AuthenticationWebApp_scope: 'openid profile email roles api',
  GITHUB_SECRETS_URL: ''
};

// Try to read from root appsettings.json
if (fs.existsSync(rootAppSettingsPath)) {
  try {
    const rootAppSettings = JSON.parse(fs.readFileSync(rootAppSettingsPath, 'utf8'));
    // Extract only the config values we need (ignore Logging, AllowedHosts, Deployment, etc.)
    if (rootAppSettings.SDMS_B2CWebApp_url) {
      appSettingsConfig.SDMS_B2CWebApp_url = rootAppSettings.SDMS_B2CWebApp_url;
    }
    if (rootAppSettings.SDMS_AuthenticationWebApp_url) {
      appSettingsConfig.SDMS_AuthenticationWebApp_url = rootAppSettings.SDMS_AuthenticationWebApp_url;
    }
    if (rootAppSettings.SDMS_AuthenticationWebApp_clientid) {
      appSettingsConfig.SDMS_AuthenticationWebApp_clientid = rootAppSettings.SDMS_AuthenticationWebApp_clientid;
    }
    if (rootAppSettings.SDMS_AuthenticationWebApp_redirectUri) {
      appSettingsConfig.SDMS_AuthenticationWebApp_redirectUri = rootAppSettings.SDMS_AuthenticationWebApp_redirectUri;
    }
    if (rootAppSettings.SDMS_AuthenticationWebApp_scope) {
      appSettingsConfig.SDMS_AuthenticationWebApp_scope = rootAppSettings.SDMS_AuthenticationWebApp_scope;
    }
    // Only replace GITHUB_SECRETS_URL if provided via environment variable (Vercel will set this)
    appSettingsConfig.GITHUB_SECRETS_URL = githubSecretsUrl || rootAppSettings.GITHUB_SECRETS_URL || '';
    console.log('Loaded appsettings from root appsettings.json');
  } catch (error) {
    console.warn('Could not read root appsettings.json, using defaults:', error);
    appSettingsConfig.GITHUB_SECRETS_URL = githubSecretsUrl || '';
  }
} else {
  // Use defaults and set GITHUB_SECRETS_URL from environment
  appSettingsConfig.GITHUB_SECRETS_URL = githubSecretsUrl || '';
  console.warn('Root appsettings.json not found, using hardcoded defaults');
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
console.log('  - API URL:', apiUrl);
console.log('  - Auth Server:', authServer);
console.log('  - Client ID:', clientId);
console.log('  - GitHub Secrets URL:', githubSecretsUrl || '(not set)');
console.log('  - AppSettings:', JSON.stringify(appSettingsConfig, null, 2));

