# Build Instructions for SDMS.B2CWebApp

## Prerequisites
- Node.js (v18 or later)
- npm or yarn
- Angular CLI 18.0.0

## Build Commands

### Development Build
```bash
npm run build
```

This will:
- Build with development configuration (default, no optimization, source maps enabled)
- Output to `dist/sdms-b2c-webapp`

### Production Build
```bash
npm run build:prod
```

This will:
- Set production flag to true
- Build with production configuration (optimized, minified, no source maps)
- Enable service worker
- Output to `dist/sdms-b2c-webapp`

## Configuration Details

### Development Configuration
- Optimization: Disabled
- Source Maps: Enabled
- Output Hashing: None
- Named Chunks: Enabled
- Vendor Chunk: Enabled

### Production Configuration
- Optimization: Enabled (scripts, styles, fonts)
- Source Maps: Disabled
- Output Hashing: All
- Named Chunks: Disabled
- Vendor Chunk: Disabled
- Service Worker: Enabled
- Budgets: Configured (2MB initial, 5MB max error)

## Fixed Issues

1. ✅ Removed deprecated `buildOptimizer` option
2. ✅ Updated `optimization` to object format (Angular 18)
3. ✅ Removed deprecated `extractCss` option
4. ✅ Added proper development configuration
5. ✅ Set default configuration to development
6. ✅ Fixed `defaultProject` warning (property doesn't exist in config)

## Troubleshooting

If you encounter build errors:

1. Clear Angular cache:
   ```bash
   rm -rf .angular
   rm -rf node_modules/.cache
   ```

2. Reinstall dependencies:
   ```bash
   rm -rf node_modules
   npm install
   ```

3. Verify Angular CLI version:
   ```bash
   ng version
   ```

4. Check for TypeScript errors:
   ```bash
   npx tsc --noEmit
   ```

## Notes

- The `defaultProject` warning is harmless and can be ignored
- Build output is in `dist/sdms-b2c-webapp`
- Configuration flow: CI/CD sets env vars → CI/CD updates appsettings.json (file exists in source) → Build → Runtime loads appsettings.json
- No separate script files: CI/CD uses inline commands to update the file
- Simple and clean: File exists in source → CI/CD updates it → Build → Runtime reads it

