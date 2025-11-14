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

# Use ng build directly instead of npm run build:prod to avoid cross-env issues
# The build-vercel.js script already sets production: true in environment.ts
echo "Running: ng build --configuration production"

# Check if ng is available locally (from node_modules)
if [ -f "node_modules/.bin/ng" ]; then
  echo "Using local Angular CLI: node_modules/.bin/ng"
  ./node_modules/.bin/ng build --configuration production
elif command -v ng &> /dev/null; then
  echo "Using global Angular CLI: ng"
  ng build --configuration production
else
  echo "Using npx to run Angular CLI"
  npx --yes @angular/cli build --configuration production
fi

if [ $? -ne 0 ]; then
  echo "❌ ERROR: Angular build failed"
  echo "Build error details above"
  exit 1
fi

echo ""
echo "✅ Build completed successfully!"
echo "Output directory: dist/sdms-b2c-webapp"

