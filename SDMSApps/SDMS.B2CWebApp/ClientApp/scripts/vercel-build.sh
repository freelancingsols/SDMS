#!/bin/bash
set -e

# Vercel build script
# This script is called by vercel.json buildCommand from root directory (SDMSApps/SDMS.B2CWebApp)
# Note: installCommand already runs npm install, so we just need to prepare and build

# Navigate to ClientApp directory
cd ClientApp || { echo "Error: ClientApp directory not found"; exit 1; }

# Run build preparation script (updates appsettings.json from env vars)
echo "Running build preparation script..."
node scripts/build-vercel.js

# Build with production configuration
echo "Building for production..."
export NODE_OPTIONS=--max-old-space-size=4096
npm run build:prod

echo "✅ Build completed successfully!"

