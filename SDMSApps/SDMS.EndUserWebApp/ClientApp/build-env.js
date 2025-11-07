// Build-time environment variable replacement script
// This script replaces environment variables in environment.prod.ts during build

const fs = require('fs');
const path = require('path');

const envFile = path.join(__dirname, 'src', 'environments', 'environment.prod.ts');

// Read environment variables
const apiUrl = process.env.API_URL || 'https://localhost:7001';
const authServer = process.env.AUTH_SERVER || 'https://localhost:7001';
const clientId = process.env.CLIENT_ID || 'sdms_frontend';

// Read the environment file
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
console.log('Environment variables injected successfully');

