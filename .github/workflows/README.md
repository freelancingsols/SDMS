# GitHub Actions Workflows

All CI/CD workflows are located at the repository root level in `.github/workflows/` directory.

## Workflow Files

### B2C WebApp (SDMS.EndUserWebApp)

1. **`ci-b2c-webapp.yml`** - CI workflow for B2C WebApp
   - Runs on: push/PR to main, master, develop, release branches
   - **Manual trigger:** Yes (via GitHub Actions UI)
   - Jobs: Lint, Build (Dev), Build (Prod), Test, CI Complete
   - Triggers only when files in `SDMSApps/SDMS.EndUserWebApp/` are changed

2. **`deploy-b2c-vercel.yml`** - Deployment workflow for B2C WebApp
   - Runs on: push to `release` branch
   - **Manual trigger:** Yes (via GitHub Actions UI)
   - Deploys to: Vercel
   - Triggers only when files in `SDMSApps/SDMS.EndUserWebApp/` are changed

### Authentication WebApp (SDMS.AuthenticationWebApp)

1. **`ci-authentication-webapp.yml`** - CI workflow for Authentication WebApp
   - Runs on: push/PR to main, master, develop, release branches
   - **Manual trigger:** Yes (via GitHub Actions UI)
   - **Build Order:** Node/Angular first, then .NET
   - **Package Management:** Uses `package.json` and generates `package-lock.json`
   - Jobs: Lint (.NET format), Build (Dev), Build (Prod), Test (.NET + Angular), CI Complete
   - Triggers only when files in `SDMSApps/SDMS.AuthenticationWebApp/` are changed

2. **`deploy-auth-railway.yml`** - Deployment workflow for Authentication WebApp
   - Runs on: push to `release` branch
   - **Manual trigger:** Yes (via GitHub Actions UI)
   - **Build Order:** Node/Angular first, then .NET
   - **Package Management:** Uses `package.json` and generates `package-lock.json`
   - Deploys to: Railway
   - Triggers only when files in `SDMSApps/SDMS.AuthenticationWebApp/` are changed

### B2B WebApp (SDMS.B2BWebApp)

1. **`ci-b2b-webapp.yml`** - CI workflow for B2B WebApp (Frontend Only)
   - Runs on: push/PR to main, master, develop, release branches
   - **Manual trigger:** Yes (via GitHub Actions UI)
   - **Build Type:** Frontend only (no .NET build)
   - **Package Management:** Uses `package.json` and generates `package-lock.json`
   - Jobs: Lint, Build (Dev), Build (Prod), Test, CI Complete
   - Triggers only when files in `SDMSApps/SDMS.B2BWebApp/` are changed
   - **Note:** Only builds `ClientApp` folder, no .NET build required

## Workflow Structure

```
.github/
└── workflows/
    ├── ci-b2c-webapp.yml              # B2C WebApp CI
    ├── ci-b2b-webapp.yml              # B2B WebApp CI (Frontend Only)
    ├── ci-authentication-webapp.yml    # Authentication WebApp CI
    ├── deploy-b2c-vercel.yml           # B2C WebApp Deployment
    └── deploy-auth-railway.yml        # Authentication WebApp Deployment
```

## Path Filters

Each workflow uses `paths` filter to only trigger when relevant project files change:

- B2C WebApp workflows: `SDMSApps/SDMS.EndUserWebApp/**`
- B2B WebApp workflows: `SDMSApps/SDMS.B2BWebApp/**`
- Authentication WebApp workflows: `SDMSApps/SDMS.AuthenticationWebApp/**`

This ensures workflows only run when their respective project files are modified.

## Build Order and Package Management

### Package Management
All workflows now:
- Use `package.json` for npm cache dependency path
- Generate `package-lock.json` automatically if it doesn't exist
- Use `npm install` instead of `npm ci` to ensure lock file is created

### Build Order

**AuthenticationWebApp:**
1. Install Angular dependencies (generates package-lock.json)
2. Build Angular/Node code first
3. Restore .NET dependencies
4. Build .NET code

**B2BWebApp:**
- Frontend only - no .NET build
- Only builds `ClientApp` folder

**B2C WebApp:**
- Frontend only - no .NET build

## Viewing and Running Workflows in GitHub

After pushing these files to your repository, you can view them in:
- **GitHub Repository** → **Actions** tab
- Each workflow will appear as a separate workflow with its own run history

### Manual Trigger (Run on Click)

All workflows support **manual triggering** via the GitHub Actions UI:

1. Go to **GitHub Repository** → **Actions** tab
2. Select the workflow you want to run (e.g., "CI - B2C WebApp")
3. Click **"Run workflow"** button on the right
4. Select the branch to run from
5. Click **"Run workflow"** to start

This is useful for:
- Testing workflows without pushing code
- Re-running failed workflows
- Running CI checks on demand
- Deploying manually when needed

## Notes

- All workflows are at the repository root level (not in project subdirectories)
- Workflows are separated by project name
- Each project has its own CI and deployment workflows
- Workflows use path filters to avoid unnecessary runs

