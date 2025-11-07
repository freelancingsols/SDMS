# Quick Fix: Run Workflow Button Not Appearing

## The Problem

The "Run workflow" button doesn't appear because:
1. ❌ Workflow files are not committed to git
2. ❌ Workflows need to be on the **default branch** (`master`)

## Quick Solution

### Step 1: Commit Workflow Files

```bash
# Add workflow files
git add .github/workflows/*.yml

# Commit them
git commit -m "Add GitHub Actions workflows with workflow_dispatch"

# Check current branch
git branch
```

### Step 2: Push to Default Branch

**Option A: If you're on `master` branch:**
```bash
git push origin master
```

**Option B: If you're on `stage` branch (current):**
```bash
# Switch to master
git checkout master

# Merge your changes
git merge stage

# Push to GitHub
git push origin master
```

### Step 3: Verify on GitHub

1. Go to: `https://github.com/YOUR_USERNAME/YOUR_REPO/actions`
2. Click on a workflow name (e.g., "CI - B2C WebApp")
3. **"Run workflow" button should appear** (top right, above runs list)

## Why This Happens

GitHub Actions only shows the "Run workflow" button when:
- ✅ Workflow files are committed and pushed
- ✅ Workflows are in the **default branch** (usually `master` or `main`)
- ✅ Workflow has `workflow_dispatch:` trigger (✅ all ours have this)

## Current Status

- ✅ Workflow files exist locally
- ✅ All workflows have `workflow_dispatch`
- ❌ Files not yet committed to git
- ❌ Need to push to `master` branch

## After Pushing

Once you push to `master`:
1. Go to GitHub → Actions tab
2. Click on any workflow name
3. **"Run workflow"** button will appear
4. Click it → Select branch → Run!

