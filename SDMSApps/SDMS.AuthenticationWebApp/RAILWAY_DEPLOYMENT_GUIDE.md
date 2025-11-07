# Railway Deployment Guide - Step by Step

This guide will walk you through deploying SDMS.AuthenticationWebApp to Railway.

## Quick Start (5 Minutes)

1. **Sign up:** Go to [railway.app](https://railway.app) and sign up with GitHub
2. **New Project:** Click "New Project" → "Deploy from GitHub repo"
3. **Select Repo:** Choose your repository and `SDMS.AuthenticationWebApp` folder
4. **Add Database:** Click "New" → "Database" → "Add PostgreSQL"
5. **Set Variables:** Go to Variables tab and add:
   - `Frontend__Url` = Your Vercel frontend URL
   - `Authentication__LoginUrl` = `/login`
6. **Deploy:** Railway will auto-deploy! Get your URL from the service dashboard

## Prerequisites

- GitHub account with the repository
- Railway account (free tier available)
- PostgreSQL database (Railway provides this)

## Step 1: Create Railway Account

1. Go to [railway.app](https://railway.app)
2. Click **"Start a New Project"**
3. Sign up with **GitHub** (recommended for easy integration)
4. Authorize Railway to access your GitHub repositories

## Step 2: Create New Project

1. In Railway dashboard, click **"New Project"**
2. Select **"Deploy from GitHub repo"**
3. Choose your repository: `SDMS/SDMSApps` (or your repo name)
4. Select the project: **SDMS.AuthenticationWebApp**
5. Railway will automatically detect it's a .NET project

## Step 3: Add PostgreSQL Database

1. In your Railway project, click **"New"** button
2. Select **"Database"** → **"Add PostgreSQL"**
3. Railway will automatically:
   - Create a PostgreSQL database
   - Generate connection string
   - Set `POSTGRES_CONNECTION` environment variable automatically

**Note:** The connection string is automatically available as `DATABASE_URL` or `POSTGRES_URL` in Railway.

## Step 4: Configure Environment Variables

1. Click on your **service** (the AuthenticationWebApp service)
2. Go to **"Variables"** tab
3. Add the following environment variables:

### Required Variables

| Variable Name | Description | Example Value |
|--------------|-------------|---------------|
| `POSTGRES_CONNECTION` | Database connection (if not auto-set) | Check Railway PostgreSQL service for connection string |
| `Authentication__LoginUrl` | Login page URL | `/login` |
| `Authentication__LogoutUrl` | Logout page URL | `/logout` |
| `Authentication__ErrorUrl` | Error page URL | `/login` |
| `Frontend__Url` | Frontend URL (for CORS) | `https://your-frontend.vercel.app` |

### Optional Variables (External Auth)

| Variable Name | Description |
|--------------|-------------|
| `ExternalAuth__Google__ClientId` | Google OAuth Client ID |
| `ExternalAuth__Google__ClientSecret` | Google OAuth Client Secret |
| `ExternalAuth__Auth0__Domain` | Auth0 domain |
| `ExternalAuth__Auth0__ClientId` | Auth0 Client ID |
| `ExternalAuth__Auth0__ClientSecret` | Auth0 Client Secret |
| `Webhook__Secret` | Webhook secret for external user sync |

**Important:** Use double underscore `__` for nested configuration keys (e.g., `Authentication__LoginUrl`)

## Step 5: Configure Deployment Method

### Option A: Railway Auto-Deploy (Recommended - Easiest)

Railway can automatically deploy when you push to GitHub. This is the simplest method:

1. In Railway project, go to **Settings** → **GitHub**
2. Enable **"Auto Deploy"** from GitHub
3. Select branch: **`release`** (or your preferred branch)
4. Railway will automatically:
   - Detect pushes to the selected branch
   - Build the application
   - Deploy to production
   - No GitHub Actions needed!

**This is the recommended method** - it's simpler and Railway handles everything automatically.

### Option B: GitHub Actions Deployment

If you prefer to use GitHub Actions for more control:

If you want to use the GitHub Actions workflow:

1. **Get Railway Token:**
   - Go to Railway dashboard
   - Click your profile → **Settings** → **Tokens**
   - Click **"New Token"**
   - Give it a name (e.g., "GitHub Actions")
   - Copy the token

2. **Get Project ID:**
   - In Railway project, go to **Settings** → **General**
   - Find **"Project ID"** and copy it

3. **Get Service ID:**
   - Click on your service (AuthenticationWebApp)
   - Go to **Settings** → **General**
   - Find **"Service ID"** and copy it

4. **Add GitHub Secrets:**
   - Go to your GitHub repository
   - **Settings** → **Secrets and variables** → **Actions**
   - Add these secrets:
     - `RAILWAY_TOKEN` = Your Railway token
     - `RAILWAY_PROJECT_ID` = Your project ID
     - `RAILWAY_SERVICE_ID` = Your service ID

## Step 6: Configure Build Settings

Railway will auto-detect .NET, but you can customize:

1. In Railway service, go to **Settings** → **Build & Deploy**
2. **Build Command:** (Leave empty - Railway auto-detects)
3. **Start Command:** `dotnet SDMS.AuthenticationWebApp.dll`
4. **Root Directory:** Leave as root (`.`)

## Step 7: Build Angular Frontend

Since the app has an Angular frontend, Railway needs to build it:

1. Railway will automatically:
   - Detect `ClientApp/package.json`
   - Run `npm install` in ClientApp
   - Build Angular app
   - Then build .NET app

2. If build fails, check:
   - Node.js version (should be 18)
   - npm dependencies are installed
   - Angular build completes successfully

## Step 8: Deploy

### Method 1: Railway Auto-Deploy (Easiest - Recommended)
1. Push to `release` branch
2. Railway automatically detects the push
3. Railway builds and deploys automatically
4. No additional configuration needed!

### Method 2: GitHub Actions Deployment
1. Push to `release` branch
2. GitHub Actions workflow will:
   - Build the application
   - Deploy using Railway CLI
3. Requires Railway token setup (see Step 5, Option B)

### Method 3: Manual Deployment via Railway CLI
```bash
# Install Railway CLI
npm install -g @railway/cli

# Login
railway login

# Link to project
railway link

# Deploy
railway up
```

## Step 9: Verify Deployment

1. Check Railway **Deployments** tab
2. Wait for build to complete (usually 3-5 minutes)
3. Click on the deployment to see logs
4. Once deployed, Railway provides a URL like: `https://your-app.railway.app`

## Step 10: Update Frontend Configuration

After deployment, update your frontend (B2CWebApp) to point to the Railway URL:

1. In GitHub Secrets, update:
   - `API_URL` = `https://your-app.railway.app`
   - `AUTH_SERVER` = `https://your-app.railway.app`

2. Or update in Vercel environment variables

## Step 11: Run Database Migrations

After first deployment, run migrations:

### Option A: Via Railway CLI
```bash
railway run dotnet ef database update
```

### Option B: Via Railway Dashboard
1. Go to your service
2. Click **"Deployments"** → **"View Logs"**
3. Or use Railway's **"Shell"** feature to run commands

### Option C: Automatic Migration
Add to Railway start command:
```bash
dotnet ef database update && dotnet SDMS.AuthenticationWebApp.dll
```

## Troubleshooting

### Build Fails
- **Check logs** in Railway dashboard
- **Verify Node.js 18** is available
- **Check Angular build** completes before .NET build
- **Verify all dependencies** are in package.json

### Database Connection Fails
- **Check POSTGRES_CONNECTION** environment variable
- **Verify PostgreSQL service** is running
- **Check connection string format**

### Application Crashes
- **Check application logs** in Railway
- **Verify all environment variables** are set
- **Check port binding** (Railway uses PORT environment variable)

### CORS Errors
- **Update Frontend__Url** to your actual frontend URL
- **Check CORS configuration** in Program.cs

## Railway Free Tier Limits

- **$5 credit** per month
- **500 hours** of usage
- **PostgreSQL** included
- **Custom domains** available
- **Automatic HTTPS**

## Monitoring

1. **View Logs:** Railway dashboard → Your service → **Logs**
2. **Metrics:** Railway dashboard → Your service → **Metrics**
3. **Deployments:** Railway dashboard → **Deployments** tab

## Next Steps

1. ✅ Deploy to Railway
2. ✅ Update frontend to use Railway URL
3. ✅ Test authentication flow
4. ✅ Set up custom domain (optional)
5. ✅ Configure monitoring alerts

## Quick Reference

**Railway Dashboard:** https://railway.app/dashboard  
**Documentation:** https://docs.railway.app  
**Support:** support@railway.app

## Alternative: Render.com

If Railway doesn't work for you, Render.com is a similar free alternative:
- Free tier available
- PostgreSQL included
- GitHub integration
- Similar setup process

