# Vercel Team Deployment Setup Guide

## Problem: "Not a member of this team" Error

If you're seeing this error during deployment:
```
@saurabhmagdum is attempting to deploy a commit to the freelancingsols' projects team on Vercel, but is not a member of this team.
```

This means the Vercel token being used doesn't have access to the team/project.

## Solution: Use a Team Token (Not Personal Token)

### Step 1: Create a Team Token

1. **Go to Vercel Dashboard**: https://vercel.com/dashboard
2. **IMPORTANT:** Select your **TEAM** from the top dropdown (not your personal account)
   - Look for "freelancingsols" or your team name in the dropdown
3. Go to **Settings** → **Tokens** (in Team Settings, not Personal Settings)
4. Click **"Create Token"**
5. Configure:
   - **Name:** "GitHub Actions Deployment" (or any name)
   - **Expiration:** No expiration (or long expiration)
6. **Copy the token** - you won't see it again!

### Step 2: Verify Team Settings

1. Make sure you're in the **Team** view (not Personal)
2. Go to **Settings** → **General**
3. Note the **Team ID** or **Organization ID**
4. This is what you'll use for `VERCEL_ORG_ID`

### Step 3: Get Project ID

1. Still in **Team** view
2. Select your project (or create one)
3. Go to **Settings** → **General**
4. Copy the **Project ID**

### Step 4: Add to GitHub Secrets

Go to: **GitHub Repository → Settings → Secrets and variables → Actions → Secrets**

Add:
- `VERCEL_TOKEN` = The **team token** you created (not personal token)
- `VERCEL_ORG_ID` = Your **team ID** (from Team Settings → General)
- `VERCEL_PROJECT_ID` = Your **project ID** (from Project Settings → General)

## How to Tell if You're Using the Wrong Token

### Signs you're using a personal token:
- ❌ Token created from Personal Settings
- ❌ Deployment fails with "not a member of this team"
- ❌ ORG_ID is your personal account ID

### Signs you're using the correct team token:
- ✅ Token created from Team Settings
- ✅ Deployment succeeds
- ✅ ORG_ID matches your team ID

## Alternative Solutions

### Option 1: Make Repository Public (Free)
If this is an open-source project:
1. Go to GitHub Repository → Settings → General
2. Scroll to "Danger Zone"
3. Change repository visibility to **Public**
4. Vercel allows free collaboration on public repos

### Option 2: Upgrade to Vercel Pro
If you need private repository:
1. Go to Vercel Dashboard → Team Settings → Billing
2. Upgrade to **Pro** plan
3. Add team members in Team Settings → Members
4. Then use team token as described above

### Option 3: Use Personal Account (Not Recommended)
If you're the only one deploying:
1. Create token from **Personal Settings** (not Team)
2. Use your **personal account ID** for `VERCEL_ORG_ID`
3. Create project in your personal account
4. **Note:** This won't work for team deployments

## Verification Checklist

Before deploying, verify:

- [ ] Token was created from **Team Settings** (not Personal Settings)
- [ ] `VERCEL_ORG_ID` matches your **Team ID** (not personal account ID)
- [ ] `VERCEL_PROJECT_ID` is from a project in the **team** (not personal)
- [ ] All three secrets are added to GitHub Secrets
- [ ] You're a member of the Vercel team (if using team deployment)

## Testing the Token

You can test if your token works:

```bash
# Test token (replace with your token)
curl -H "Authorization: Bearer YOUR_TOKEN" https://api.vercel.com/v2/teams

# Should return your teams if token is valid
```

## Common Mistakes

1. **Using Personal Token for Team Project**
   - ❌ Created token from Personal Settings
   - ✅ Must create from Team Settings

2. **Wrong ORG_ID**
   - ❌ Using personal account ID
   - ✅ Must use team/organization ID

3. **Project in Wrong Account**
   - ❌ Project created in personal account
   - ✅ Project must be in team account

4. **Not a Team Member**
   - ❌ User not added to team
   - ✅ Must be added to team (requires Pro plan for private repos)

## Still Having Issues?

1. **Check Vercel Dashboard:**
   - Verify you can see the team and project in dashboard
   - Verify you have access to team settings

2. **Verify GitHub Secrets:**
   - Double-check secret names (case-sensitive)
   - Verify values are correct (no extra spaces)

3. **Check Vercel Logs:**
   - Go to Vercel Dashboard → Your Project → Deployments
   - Check deployment logs for specific errors

4. **Contact Team Owner:**
   - If you're not the team owner, ask them to:
     - Add you to the team
     - Create a team token for you
     - Share the ORG_ID and PROJECT_ID







