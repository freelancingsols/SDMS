# CI/CD Workflows Documentation

## Overview

Both projects now have comprehensive CI/CD workflows that run on every push and pull request, with deployment only on the `release` branch.

## Workflow Structure

### CI Workflows (Run on all branches/PRs)

#### SDMS.B2CWebApp (Frontend)
**File:** `.github/workflows/ci.yml`

**Jobs:**
1. **Lint** - Runs Angular linting
2. **Build (Development)** - Builds Angular app in development mode
3. **Build (Production)** - Builds Angular app in production mode with environment variables
4. **Test** - Runs unit tests with code coverage
5. **CI Complete** - Aggregates results from all jobs

#### SDMS.AuthenticationWebApp (Backend)
**File:** `.github/workflows/ci.yml`

**Jobs:**
1. **Lint** - Runs .NET format check
2. **Build (Development)** - Builds Angular frontend and .NET backend in Debug mode
3. **Build (Production)** - Builds Angular frontend and .NET backend in Release mode
4. **Test** - Runs .NET tests (if any) and Angular tests (if any)
5. **CI Complete** - Aggregates results from all jobs

### Deployment Workflows (Run only on Release branch)

#### SDMS.B2CWebApp → Vercel
**File:** `.github/workflows/deploy-vercel.yml`
- Triggers on: `release` branch push
- Deploys to: Vercel

#### SDMS.AuthenticationWebApp → Railway
**File:** `.github/workflows/deploy-railway.yml`
- Triggers on: `release` branch push
- Deploys to: Railway

## Branch Strategy

- **main/master/develop** - CI runs (lint, build, test) but no deployment
- **release/Release** - CI runs + Deployment to production
- **Pull Requests** - CI runs to validate changes

## CI Jobs Details

### Lint Job
- **Frontend:** Angular CLI lint (`ng lint`)
- **Backend:** .NET format check (`dotnet format`)
- **Status:** Non-blocking (continue-on-error: true)

### Build Jobs

#### Development Build
- Builds without optimization
- Faster build time
- Includes source maps
- Artifacts retained for 1 day

#### Production Build
- Optimized build
- Minified code
- Environment variables injected
- Artifacts retained for 7 days

### Test Job
- **Frontend:** Karma + Jasmine tests with ChromeHeadless
- **Backend:** .NET test projects (if any)
- **Coverage:** Code coverage reports generated
- **Status:** Non-blocking (continue-on-error: true)
- **Artifacts:** Test results and coverage reports uploaded

## Running Locally

### Frontend (B2CWebApp)
```bash
cd ClientApp
npm install
npm run lint          # Run linting
npm run build         # Development build
npm run build:prod    # Production build
npm run test:ci       # Run tests in CI mode
```

### Backend (AuthenticationWebApp)
```bash
dotnet restore
dotnet build          # Development build
dotnet build -c Release # Production build
dotnet format         # Check code formatting
dotnet test           # Run tests (if test projects exist)

# With Angular
cd ClientApp
npm install
npm run build
```

## Workflow Triggers

### CI Workflows
- Push to: `main`, `master`, `develop`, `release`, `Release`
- Pull requests to: `main`, `master`, `develop`, `release`, `Release`

### Deployment Workflows
- Push to: `release`, `Release` branch only
- Manual trigger: `workflow_dispatch`

## Artifacts

All workflows upload artifacts:
- **Build artifacts:** Compiled applications
- **Test results:** Test output and coverage reports
- **Retention:** 1-7 days depending on type

## Status Badges

You can add status badges to your README:

```markdown
![CI](https://github.com/your-org/your-repo/workflows/CI%20-%20Build%2C%20Test%2C%20and%20Lint/badge.svg)
```

## Troubleshooting

### Tests Fail
- Check test output in workflow logs
- Verify test configuration files exist
- Check for missing dependencies

### Build Fails
- Verify all dependencies are installed
- Check for TypeScript/compilation errors
- Review build logs for specific errors

### Lint Fails
- Run `npm run lint:fix` locally to auto-fix issues
- Review linting rules in `tslint.json` or ESLint config

### Deployment Fails
- Verify GitHub Secrets are set correctly
- Check deployment platform credentials
- Review deployment logs

## Next Steps

1. Push code to trigger CI workflows
2. Create `release` branch when ready to deploy
3. Merge to `release` branch to trigger deployment
4. Monitor workflow runs in GitHub Actions tab

