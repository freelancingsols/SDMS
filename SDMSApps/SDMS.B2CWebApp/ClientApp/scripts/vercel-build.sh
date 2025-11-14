#!/bin/bash
set -e

# Vercel build script
# This script is called by vercel.json buildCommand from root directory (SDMSApps/SDMS.B2CWebApp)
# Note: installCommand already runs npm install, so we just need to prepare and build

echo "🚀 Starting Vercel build process..."
echo "Current directory: $(pwd)"
echo ""

# Navigate to ClientApp directory
if [ ! -d "ClientApp" ]; then
  echo "❌ ERROR: ClientApp directory not found in $(pwd)"
  echo "Contents of current directory:"
  ls -la
  exit 1
fi

cd ClientApp || { echo "❌ ERROR: Failed to change to ClientApp directory"; exit 1; }
echo "✅ Changed to ClientApp directory: $(pwd)"
echo ""

# Verify package.json exists
if [ ! -f "package.json" ]; then
  echo "❌ ERROR: package.json not found in $(pwd)"
  exit 1
fi
echo "✅ Found package.json"
echo ""

# Verify node_modules exists (should be installed by installCommand)
if [ ! -d "node_modules" ]; then
  echo "⚠️  WARNING: node_modules not found, installing dependencies..."
  npm install --legacy-peer-deps
else
  echo "✅ Found node_modules"
fi
echo ""

# Run build preparation script (updates appsettings.json from env vars)
echo "📝 Running build preparation script..."
if [ ! -f "scripts/build-vercel.js" ]; then
  echo "❌ ERROR: scripts/build-vercel.js not found"
  exit 1
fi

node scripts/build-vercel.js
if [ $? -ne 0 ]; then
  echo "❌ ERROR: Build preparation script failed"
  exit 1
fi
echo "✅ Build preparation completed"
echo ""

# Build with production configuration
echo "🔨 Building for production..."
echo "Setting NODE_OPTIONS..."
export NODE_OPTIONS="--max-old-space-size=4096"
echo "NODE_OPTIONS=$NODE_OPTIONS"
echo ""

# Ensure Angular CLI is available
echo "🔍 Checking for Angular CLI..."
if [ ! -f "node_modules/.bin/ng" ]; then
  echo "⚠️  Angular CLI not found in node_modules, installing..."
  npm install --save-dev @angular/cli@latest --legacy-peer-deps
  if [ $? -ne 0 ]; then
    echo "❌ ERROR: Failed to install Angular CLI"
    exit 1
  fi
  echo "✅ Angular CLI installed"
else
  echo "✅ Angular CLI found in node_modules"
fi
echo ""

# Verify Angular CLI is executable
if [ ! -x "node_modules/.bin/ng" ]; then
  echo "⚠️  Angular CLI not executable, fixing permissions..."
  chmod +x node_modules/.bin/ng
fi

# Use ng build directly instead of npm run build:prod to avoid cross-env issues
# The build-vercel.js script already sets production: true in environment.ts
echo "Running: ng build --configuration production"
echo "Using local Angular CLI: node_modules/.bin/ng"
echo ""

# Always use local Angular CLI (should be available after installCommand + our check)
./node_modules/.bin/ng build --configuration production

if [ $? -ne 0 ]; then
  echo "❌ ERROR: Angular build failed"
  echo "Build error details above"
  echo ""
  echo "Debugging information:"
  echo "  Current directory: $(pwd)"
  echo "  Node version: $(node --version)"
  echo "  NPM version: $(npm --version)"
  echo "  Angular CLI path: $(ls -la node_modules/.bin/ng 2>/dev/null || echo 'NOT FOUND')"
  echo "  node_modules/.bin contents:"
  ls -la node_modules/.bin/ | head -20 || echo "  (node_modules/.bin not found)"
  exit 1
fi

echo ""
echo "✅ Build completed successfully!"
echo "Output directory: dist/sdms-b2c-webapp"

