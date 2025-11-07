# How to See and Use the "Run workflow" Button

## Why the Button Might Not Appear

The "Run workflow" button appears in GitHub Actions when:

1. ✅ **Workflow file is committed and pushed** to the repository
2. ✅ **Workflow has `workflow_dispatch` trigger** (all our workflows have this)
3. ✅ **You're viewing the workflow on GitHub** (not locally)
4. ✅ **You have write permissions** to the repository
5. ✅ **Workflow file is in `.github/workflows/` directory**

## Step-by-Step: Finding the Run Workflow Button

### Step 1: Push Workflow Files to GitHub

First, make sure the workflow files are committed and pushed:

```bash
git add .github/workflows/
git commit -m "Add GitHub Actions workflows"
git push origin main  # or your branch name
```

### Step 2: Go to GitHub Actions

1. Open your repository on **GitHub.com**
2. Click on the **"Actions"** tab (top navigation)
3. You should see a list of workflows on the left sidebar

### Step 3: Select a Workflow

1. Click on a workflow name (e.g., "CI - B2C WebApp")
2. You'll see the workflow runs history
3. **Look for the "Run workflow" button** on the right side, above the runs list

### Step 4: Run the Workflow

1. Click **"Run workflow"** dropdown button
2. Select the branch (usually `main` or `master`)
3. Optionally add inputs (if the workflow accepts them)
4. Click **"Run workflow"** green button

## Visual Guide

```
GitHub Repository
└── Actions Tab
    └── Left Sidebar (Workflow List)
        ├── CI - B2C WebApp
        ├── CI - Authentication WebApp
        ├── Deploy B2C WebApp to Vercel
        └── Deploy Authentication WebApp to Railway
            └── [Run workflow] ← Button appears here
```

## Troubleshooting

### Button Still Not Appearing?

#### Check 1: Workflow File Location
- ✅ Must be in `.github/workflows/` directory
- ✅ File must have `.yml` or `.yaml` extension
- ✅ File must be in the repository root

#### Check 2: Workflow Syntax
Verify the workflow has `workflow_dispatch`:

```yaml
on:
  workflow_dispatch:  # ← This must be present
```

#### Check 3: Default Branch
- The workflow must exist in the **default branch** (usually `main` or `master`)
- If you created it in a feature branch, merge it first

#### Check 4: Permissions
- You need **write access** to the repository
- If it's an organization repo, check your permissions

#### Check 5: GitHub UI Location
The button appears:
- ✅ In the workflow's detail page (click on workflow name)
- ✅ Above the list of workflow runs
- ❌ NOT in the general Actions page
- ❌ NOT in a specific run's page

### Still Not Working?

1. **Refresh the page** (Ctrl+F5 or Cmd+Shift+R)
2. **Check if workflow appears in the list** - if not, there might be a syntax error
3. **Check GitHub Actions tab** for any error messages
4. **Verify the file is actually on GitHub** - check the repository file browser

## Alternative: Trigger via GitHub CLI

If the button doesn't appear, you can trigger workflows via CLI:

```bash
# Install GitHub CLI
# Windows: choco install gh
# Mac: brew install gh
# Linux: See https://cli.github.com/

# Authenticate
gh auth login

# Run workflow
gh workflow run "CI - B2C WebApp" --ref main
```

## Workflows That Support Manual Run

All these workflows have `workflow_dispatch`:

- ✅ `ci-b2c-webapp.yml` - CI - B2C WebApp
- ✅ `ci-authentication-webapp.yml` - CI - Authentication WebApp  
- ✅ `deploy-b2c-vercel.yml` - Deploy B2C WebApp to Vercel
- ✅ `deploy-auth-railway.yml` - Deploy Authentication WebApp to Railway
- ❌ `error-handler.yml` - Does NOT have `workflow_dispatch` (runs automatically on failures)

## Quick Test

To verify workflows are working:

1. **Push a commit** to trigger automatic runs
2. **Check Actions tab** - you should see runs appearing
3. **Once a run appears**, the workflow is recognized
4. **Then the "Run workflow" button should appear**

## Common Issues

### Issue: "No workflows found"
- **Cause:** Workflow files not pushed to GitHub
- **Fix:** Commit and push `.github/workflows/*.yml` files

### Issue: Workflow appears but no "Run workflow" button
- **Cause:** Missing `workflow_dispatch` trigger
- **Fix:** Add `workflow_dispatch:` to the `on:` section

### Issue: Button appears but workflow doesn't run
- **Cause:** Workflow might have errors or missing dependencies
- **Fix:** Check the workflow run logs for errors

## Need Help?

If the button still doesn't appear after:
1. ✅ Committing and pushing workflow files
2. ✅ Checking the workflow is in `.github/workflows/`
3. ✅ Verifying `workflow_dispatch` is present
4. ✅ Refreshing the GitHub page

Then:
- Check GitHub's status page for outages
- Verify your repository permissions
- Try accessing from a different browser
- Contact GitHub support

