# Fix: "Run workflow" Button Not Appearing

## Quick Checklist

If the "Run workflow" button is not showing in GitHub Actions, check these:

### ✅ 1. Workflow File Location
- Must be in `.github/workflows/` directory at repository root
- File must have `.yml` or `.yaml` extension
- ✅ All our workflows are in the correct location

### ✅ 2. workflow_dispatch Trigger
- The workflow must have `workflow_dispatch:` in the `on:` section
- ✅ All our workflows have this configured

### ✅ 3. File Committed and Pushed
- The workflow file must be committed to the repository
- Must be pushed to GitHub (not just local)
- **Check:** Go to your repository on GitHub and verify the file exists at `.github/workflows/`

### ✅ 4. Permissions
- You need write access to the repository
- For organization repos, you might need specific permissions

### ✅ 5. Syntax Errors
- YAML syntax errors prevent workflows from being recognized
- Check for indentation issues, missing colons, etc.

## How to Verify

### Step 1: Check File Exists on GitHub

1. Go to your GitHub repository
2. Navigate to `.github/workflows/` folder
3. Verify these files exist:
   - `ci-b2c-webapp.yml`
   - `ci-authentication-webapp.yml`
   - `deploy-b2c-vercel.yml`
   - `deploy-auth-railway.yml`
   - `error-handler.yml`

### Step 2: Check workflow_dispatch is Present

Open each workflow file and verify it has:

```yaml
on:
  push:
    branches: [...]
  workflow_dispatch:  # ← This must be present
```

### Step 3: Check Actions Tab

1. Go to **GitHub Repository** → **Actions** tab
2. You should see all workflows listed on the left sidebar
3. Click on a workflow name (e.g., "CI - B2C WebApp")
4. Look for **"Run workflow"** button on the right side

## Common Issues and Fixes

### Issue 1: Workflow Not Showing in Actions Tab

**Cause:** File not committed/pushed or syntax error

**Fix:**
```bash
# Verify file exists locally
ls .github/workflows/

# Commit and push
git add .github/workflows/
git commit -m "Add GitHub Actions workflows"
git push
```

### Issue 2: "Run workflow" Button Missing

**Cause:** Missing `workflow_dispatch` trigger

**Fix:** Add to workflow file:
```yaml
on:
  push:
    branches: [...]
  workflow_dispatch:  # Add this line
```

### Issue 3: Button Grayed Out

**Cause:** No default branch selected or permission issue

**Fix:**
- Make sure you're on a branch that exists on GitHub
- Check you have write permissions
- Try refreshing the page

### Issue 4: Workflow Shows But Can't Run

**Cause:** YAML syntax error

**Fix:**
- Use a YAML validator
- Check GitHub Actions tab for error messages
- Fix indentation (must use spaces, not tabs)

## Verification Steps

### Test 1: Check Workflow Syntax

```bash
# Install act (optional, for local testing)
# Or use online YAML validator: https://www.yamllint.com/
```

### Test 2: Verify on GitHub

1. Go to: `https://github.com/YOUR_USERNAME/YOUR_REPO/actions`
2. Click on a workflow name
3. Look for **"Run workflow"** dropdown button
4. If missing, check the workflow file has `workflow_dispatch`

### Test 3: Check File Content

```bash
# Verify workflow_dispatch exists
grep -r "workflow_dispatch" .github/workflows/
```

Should show:
```
.github/workflows/ci-b2c-webapp.yml:  workflow_dispatch:
.github/workflows/ci-authentication-webapp.yml:  workflow_dispatch:
.github/workflows/deploy-b2c-webapp.yml:  workflow_dispatch:
.github/workflows/deploy-auth-railway.yml:  workflow_dispatch:
```

## Current Status of Our Workflows

✅ **ci-b2c-webapp.yml** - Has `workflow_dispatch`
✅ **ci-authentication-webapp.yml** - Has `workflow_dispatch`
✅ **deploy-b2c-vercel.yml** - Has `workflow_dispatch`
✅ **deploy-auth-railway.yml** - Has `workflow_dispatch`
✅ **error-handler.yml** - Now has `workflow_dispatch` (for manual testing)

## Where to Find "Run workflow" Button

1. **GitHub Repository** → **Actions** tab
2. Click on workflow name in left sidebar (e.g., "CI - B2C WebApp")
3. Look for **"Run workflow"** button on the right side, above the workflow runs list
4. Click the dropdown to select branch
5. Click **"Run workflow"** button

## Screenshot Location

The button appears here:
```
┌─────────────────────────────────────┐
│  Actions  │  [Workflow Name]        │
├─────────────────────────────────────┤
│           │  [Run workflow ▼]  ← HERE│
│ Workflows │                          │
│  • CI...  │  [Recent runs...]       │
│  • Deploy │                          │
└─────────────────────────────────────┘
```

## Still Not Working?

If the button still doesn't appear:

1. **Check GitHub Status:** https://www.githubstatus.com/
2. **Clear Browser Cache:** Hard refresh (Ctrl+F5)
3. **Try Different Browser:** Sometimes browser extensions interfere
4. **Check Repository Settings:** Settings → Actions → Workflow permissions
5. **Verify Branch:** Make sure you're viewing the branch where workflows are committed

## Quick Fix Command

If workflows aren't showing, re-commit them:

```bash
git add .github/workflows/
git commit -m "Fix: Ensure all workflows have workflow_dispatch"
git push
```

Then wait a few seconds and refresh the GitHub Actions page.
