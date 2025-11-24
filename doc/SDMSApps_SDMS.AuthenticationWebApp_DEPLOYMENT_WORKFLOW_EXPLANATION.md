# Deploy Authentication WebApp to Railway - Workflow Explanation

## Overview

This GitHub Actions workflow (`deploy-auth-railway.yml`) automatically deploys the Authentication WebApp to Railway after successful CI runs. It handles environment variable synchronization, builds the application, and deploys to production.

---

## Workflow Triggers

### 1. **Automatic Trigger (workflow_run)**
- **When:** After the CI workflow "CI - Authentication WebApp (Build, Test, and Lint)" completes
- **Conditions:**
  - CI workflow must have succeeded (`conclusion == 'success'`)
  - CI workflow must have run on `release` or `Release` branch
- **Purpose:** Ensures only tested, production-ready code is deployed

### 2. **Manual Trigger (workflow_dispatch)**
- **When:** Manually triggered from GitHub Actions UI
- **Conditions:** Can be run from any branch
- **Purpose:** Allows emergency deployments or testing deployments

---

## Concurrency Control

```yaml
concurrency:
  group: deploy-auth-railway-${{ github.ref }}
  cancel-in-progress: true
```

- **Purpose:** Prevents multiple deployments from running simultaneously
- **Behavior:** If a new deployment starts, it cancels any in-progress deployment for the same branch
- **Benefit:** Saves GitHub Actions minutes and prevents deployment conflicts

---

## Workflow Structure

### Job: `deploy`
- **Runner:** `ubuntu-latest`
- **Timeout:** 30 minutes (prevents hanging deployments)
- **Conditional Execution:** Only runs if:
  - CI succeeded AND ran on release branch (for automatic triggers), OR
  - Manually triggered (for workflow_dispatch)

---

## Step-by-Step Breakdown

### Step 1: Deployment Summary
**Lines 32-56**

Displays deployment information:
- Application name
- Target platform (Railway)
- Trigger type (automatic or manual)
- CI workflow details (if automatic)
- Branch and commit information

**Purpose:** Provides visibility into what's being deployed

---

### Step 2: Verify CI Status and Application
**Lines 58-105**
**Condition:** Only runs for `workflow_run` triggers

Validates:
1. **CI Workflow Name:** Ensures it matches "Authentication WebApp"
2. **Branch:** Verifies CI ran on `release` or `Release` branch
3. **CI Conclusion:** Ensures CI succeeded

**Failure Behavior:** Exits with error if validation fails

**Purpose:** Safety check to prevent deploying untested or wrong code

---

### Step 3: Verify Which Application Triggered Deployment
**Lines 107-130**
**Condition:** Only runs for `workflow_run` triggers

Double-checks that the correct CI workflow triggered this deployment.

**Purpose:** Additional safety verification

---

### Step 4: Checkout Code
**Lines 132-138**

- Uses the commit SHA from the CI workflow (for automatic triggers)
- Uses current branch/ref (for manual triggers)
- `fetch-depth: 1` - Only fetches the specific commit (saves time)

---

### Step 5: Setup .NET
**Lines 140-143**

Installs .NET 8.0.x SDK for building the .NET application.

---

### Step 6: Setup Node.js
**Lines 145-150**

- Installs Node.js 18
- Enables npm caching for faster builds
- Configures cache path for ClientApp dependencies

---

### Step 7: Install Angular Dependencies
**Lines 152-156**

- Runs `npm ci` (faster, reliable installs)
- Falls back to `npm install` if needed
- Uses `--legacy-peer-deps` flag for compatibility

---

### Step 8: Build Angular App
**Lines 158-165**
**CRITICAL STEP**

**Environment Variables Used:**
- `SDMS_AuthenticationWebApp_url` - Production URL
- `SDMS_AuthenticationWebApp_clientid` - Client ID

**Purpose:** 
- Builds the Angular frontend with production configuration
- Generates `appsettings.json` with production URLs
- Validates that the build works with production values

**Note:** This is a validation build. Railway will rebuild during actual deployment.

---

### Step 9: Restore .NET Dependencies
**Lines 167-169**

Restores NuGet packages for the .NET backend.

---

### Step 10: Build .NET App
**Lines 171-173**

Builds the .NET application in Release configuration.

---

### Step 11: Publish .NET App
**Lines 175-177**

Publishes the .NET application to `./publish` directory.

---

### Step 12: Install Railway CLI
**Lines 179-180**

Installs Railway CLI tool for deployment operations.

---

### Step 13: Sync Railway Environment Variables
**Lines 182-625**
**MOST COMPLEX STEP - CRITICAL FOR DEPLOYMENT**

#### 13.1: Load Variables from GitHub
**Lines 183-213**

Loads environment variables from GitHub Variables and Secrets:

**Secrets (sensitive data):**
- `RAILWAY_TOKEN` - Railway API authentication
- `SDMS_AuthenticationWebApp_ConnectionString` - Database connection
- `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientSecret`
- `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientSecret`
- `SDMS_AuthenticationWebApp_WebhookSecret`
- `logging_loki_token` - Grafana Loki API token

**Variables (non-sensitive):**
- Railway IDs (`RAILWAY_SERVICE_ID`, `RAILWAY_PROJECT_ID`, `RAILWAY_ENVIRONMENT_ID`)
- Application URLs and configuration
- External auth settings
- `logging_loki_url` and `logging_loki_user`

#### 13.2: Validate Required Variables
**Lines 242-267**

Checks that critical Railway configuration variables are set:
- `RAILWAY_TOKEN`
- `RAILWAY_PROJECT_ID`
- `RAILWAY_SERVICE_ID`
- `RAILWAY_ENVIRONMENT_ID`

**Failure:** Exits with error if any are missing

#### 13.3: Helper Functions
**Lines 272-380**

**`escape_json()`** - Escapes special characters for JSON/GraphQL

**`get_railway_variables()`** - Fetches current Railway variables via GraphQL API
- Uses Railway's GraphQL API endpoint
- Returns all existing environment variables

**`get_railway_var_value()`** - Extracts a specific variable value from Railway response

**`set_railway_env()`** - Sets/updates a Railway variable via GraphQL API
- Handles INSERT (new variable) and UPDATE (existing variable)
- Handles Railway's asynchronous processing (504/502/503 responses)
- Returns success/failure status

#### 13.4: Define Variables to Sync
**Lines 387-414**

Creates an associative array (`VARS_TO_SYNC`) mapping variable names to their GitHub values:

**Critical Variables:**
- `SDMS_AuthenticationWebApp_url` - Used during Railway build
- `SDMS_AuthenticationWebApp_clientid` - Used during Railway build
- `SDMS_AuthenticationWebApp_ConnectionString` - Database connection

**Application Configuration:**
- URLs, ports, authentication settings
- External auth providers (Google, Auth0)
- Redirect URIs

**Logging Configuration:**
- `logging_loki_url`
- `logging_loki_user`
- `logging_loki_token`

#### 13.5: Display Variable Status
**Lines 420-445**

Logs the status of critical variables:
- Shows which variables are set/not set
- Provides previews (masked for security)
- Special logging for Loki configuration

#### 13.6: Sync Process
**Lines 476-569**

For each variable in `VARS_TO_SYNC`:

1. **Check if variable is set in GitHub:**
   - If empty: Skip with appropriate warning
   - Critical variables get special warnings
   - Loki variables get specific guidance

2. **Compare with Railway:**
   - Fetch current value from Railway
   - Compare GitHub value vs Railway value

3. **Take Action:**
   - **INSERT:** Variable doesn't exist in Railway → Create it
   - **UPDATE:** Variable exists but value differs → Update it
   - **SKIP:** Variable exists and value matches → No action needed

4. **Track Results:**
   - Track inserted, updated, unchanged, failed, and skipped variables

#### 13.7: Summary and Error Handling
**Lines 571-625**

**Displays Summary:**
- Count of inserted variables
- Count of updated variables
- Count of unchanged variables
- Count of skipped variables
- Count of failed variables

**Lists Details:**
- Shows which variables were inserted/updated/skipped/failed

**Error Handling:**
- **Critical Failure:** If `SDMS_AuthenticationWebApp_ConnectionString` fails → Deployment stops
- **Non-Critical Failures:** Deployment continues with warning
- **All Success:** Proceeds to next step

**Key Features:**
- Masks sensitive values in logs (tokens, secrets, connection strings)
- Provides helpful error messages with links to Railway dashboard
- Handles Railway's asynchronous API responses gracefully

---

### Step 14: Verify Critical Variables in Railway
**Lines 627-691**

**Purpose:** Double-checks that critical variables are actually set in Railway after sync

**Process:**
1. Waits 3 seconds for Railway to propagate variables
2. Fetches variables from Railway via GraphQL API
3. Verifies critical variables:
   - `SDMS_AuthenticationWebApp_url`
   - `SDMS_AuthenticationWebApp_clientid`
4. Validates values:
   - Must be set (not empty)
   - Must not be localhost (production check)

**Failure:** Exits with error if critical variables are missing or incorrect

**Success:** Proceeds to deployment

---

### Step 15: Deploy to Railway
**Lines 693-711**

**Command:** `railway up --service $SERVICE_ID`

**Process:**
1. Railway CLI triggers deployment
2. Railway builds the application using:
   - `nixpacks.toml` (build configuration)
   - Environment variables synced in previous step
3. Railway runs `npm run build:prod` in ClientApp
4. Railway builds and publishes .NET application
5. Railway deploys to production

**Retry Logic:**
- Attempts deployment up to 3 times
- Waits 10 seconds between retries
- Fails if all 3 attempts fail

**Success:** Deployment complete ✅

---

## Key Features

### 1. **Safety Checks**
- Multiple validation steps prevent deploying bad code
- Branch verification ensures only release branch deploys
- CI success verification ensures code is tested

### 2. **Environment Variable Management**
- Automatic synchronization from GitHub to Railway
- Comparison logic prevents unnecessary updates
- Detailed logging for troubleshooting
- Handles missing optional variables gracefully

### 3. **Error Handling**
- Validates required variables before proceeding
- Provides clear error messages
- Links to Railway dashboard for manual fixes
- Distinguishes critical vs non-critical failures

### 4. **Security**
- Masks sensitive values in logs
- Uses GitHub Secrets for sensitive data
- Uses GitHub Variables for non-sensitive configuration

### 5. **Efficiency**
- Concurrency control prevents duplicate deployments
- Caching for npm dependencies
- Shallow git fetch (saves time)
- Timeout prevents hanging deployments

---

## Variable Flow

```
GitHub Variables/Secrets
    ↓
GitHub Actions Workflow (this file)
    ↓
    ├─→ Step 8: Build Angular (validation)
    │   └─→ Uses: SDMS_AuthenticationWebApp_url, clientid
    │
    └─→ Step 13: Sync to Railway
        └─→ All variables synced to Railway
            └─→ Step 15: Railway builds using synced variables
                └─→ Deployed app uses Railway environment variables
```

---

## Critical Variables

These variables **MUST** be set in GitHub for deployment to succeed:

1. **Railway Configuration:**
   - `RAILWAY_TOKEN` (Secret)
   - `RAILWAY_PROJECT_ID` (Variable)
   - `RAILWAY_SERVICE_ID` (Variable)
   - `RAILWAY_ENVIRONMENT_ID` (Variable)

2. **Application Build:**
   - `SDMS_AuthenticationWebApp_url` (Variable) - **CRITICAL**
   - `SDMS_AuthenticationWebApp_clientid` (Variable) - **CRITICAL**

3. **Database:**
   - `SDMS_AuthenticationWebApp_ConnectionString` (Secret) - **CRITICAL**

---

## Optional Variables

These variables are synced but optional (app works without them):

- `logging_loki_*` - Grafana Loki logging (all 3 must be set together)
- External auth providers (Google, Auth0)
- Server URLs and ports
- Redirect URIs

---

## Troubleshooting

### Deployment Fails: "Missing required configuration"
- **Cause:** GitHub Variables not set
- **Fix:** Set required variables in GitHub → Settings → Secrets and variables → Actions

### Deployment Fails: "CRITICAL ERROR: Required variables are missing"
- **Cause:** Variables not synced to Railway
- **Fix:** Check sync step logs, verify Railway API access, check variable names

### Build Uses Localhost URLs
- **Cause:** `SDMS_AuthenticationWebApp_url` not set or incorrect
- **Fix:** Verify variable is set in GitHub and synced to Railway

### Logs Not Appearing in Grafana Loki
- **Cause:** `logging_loki_*` variables not set
- **Fix:** Set all three variables in GitHub (url, user in Variables; token in Secrets)

---

## Summary

This workflow provides:
- ✅ **Automated deployment** after successful CI
- ✅ **Environment variable synchronization** from GitHub to Railway
- ✅ **Safety checks** to prevent bad deployments
- ✅ **Detailed logging** for troubleshooting
- ✅ **Error handling** with clear messages
- ✅ **Manual deployment** option for emergencies

The workflow ensures that production deployments are:
- Tested (CI must succeed)
- From release branch
- Configured correctly (variables synced)
- Built with production URLs (not localhost)

