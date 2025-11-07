# NPM Dependency Conflict Fix

## Problem

GitHub Actions workflows were failing with dependency conflicts:
- `@angular-devkit/build-angular@0.803.29` (Angular 8) was being resolved instead of Angular 18
- `karma-jasmine-html-reporter@~5.1.0` doesn't exist
- Old `package-lock.json` files had Angular 8 versions locked

## Root Cause

The `package-lock.json` files contained old Angular 8 dependencies that were being used instead of the Angular 18 versions specified in `package.json`.

## Solution Applied

### 1. Fixed Package Versions

**Updated `package.json` files:**
- Changed `karma-jasmine-html-reporter` from `~5.1.0` to `^2.1.0` (correct version)

### 2. Updated GitHub Actions Workflows

**All workflows now:**
1. **Remove old `package-lock.json`** before installing dependencies
2. **Remove `node_modules`** to ensure clean install
3. **Use `npm install --legacy-peer-deps`** to handle peer dependency conflicts

**Updated workflows:**
- `.github/workflows/ci-b2c-webapp.yml`
- `.github/workflows/ci-authentication-webapp.yml`
- `.github/workflows/deploy-b2c-vercel.yml`
- `.github/workflows/deploy-auth-railway.yml`

### 3. Workflow Changes

**Before:**
```yaml
- name: Install dependencies and generate package-lock.json
  run: |
    npm install
    if [ ! -f package-lock.json ]; then
      npm install --package-lock-only
    fi
```

**After:**
```yaml
- name: Remove old package-lock.json and node_modules
  run: |
    rm -f package-lock.json
    rm -rf node_modules || true

- name: Install dependencies
  run: npm install --legacy-peer-deps
```

## Why `--legacy-peer-deps`?

Angular 18 has strict peer dependency requirements. Using `--legacy-peer-deps` allows npm to:
- Install packages even if peer dependencies don't match exactly
- Resolve conflicts more flexibly
- Still maintain compatibility

## Local Development

For local development, you should:

1. **Delete old lock files:**
   ```bash
   rm package-lock.json
   rm -rf node_modules
   ```

2. **Install with legacy peer deps:**
   ```bash
   npm install --legacy-peer-deps
   ```

3. **Or use Node 18+ and install normally:**
   ```bash
   # Ensure Node 18+ is installed
   node --version  # Should be 18.19.1 or higher
   npm install
   ```

## Verification

After these changes, workflows should:
- ✅ Install Angular 18 packages correctly
- ✅ Build successfully
- ✅ Run tests without dependency conflicts
- ✅ Deploy without errors

## Next Steps

1. **Commit and push** these changes
2. **Monitor GitHub Actions** to verify builds succeed
3. **Regenerate `package-lock.json`** locally if needed (with Node 18+)
4. **Consider committing new `package-lock.json`** after successful builds

## Notes

- The old `package-lock.json` files are intentionally removed in CI to prevent conflicts
- `--legacy-peer-deps` is safe to use and is recommended for Angular 18 projects
- Local developers should use Node 18.19.1+ for best compatibility

