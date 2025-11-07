# Railway Deployment Checklist

Use this checklist to ensure a successful deployment.

## Pre-Deployment

- [ ] Railway account created (sign up at railway.app)
- [ ] GitHub repository is accessible
- [ ] Code is pushed to `release` branch (for auto-deploy)

## Step 1: Create Project

- [ ] Click "New Project" in Railway
- [ ] Select "Deploy from GitHub repo"
- [ ] Choose your repository
- [ ] Select `SDMS.AuthenticationWebApp` folder/service

## Step 2: Add PostgreSQL Database

- [ ] Click "New" → "Database" → "Add PostgreSQL"
- [ ] Database service created
- [ ] Connection string automatically available as `DATABASE_URL` or `POSTGRES_CONNECTION`

## Step 3: Configure Environment Variables

Go to your service → Variables tab and add:

### Required Variables
- [ ] `POSTGRES_CONNECTION` (usually auto-set by Railway)
- [ ] `Frontend__Url` = Your Vercel frontend URL (e.g., `https://your-app.vercel.app`)
- [ ] `Authentication__LoginUrl` = `/login`
- [ ] `Authentication__LogoutUrl` = `/logout`
- [ ] `Authentication__ErrorUrl` = `/login`

### Optional Variables (if using external auth)
- [ ] `ExternalAuth__Google__ClientId`
- [ ] `ExternalAuth__Google__ClientSecret`
- [ ] `ExternalAuth__Auth0__Domain`
- [ ] `ExternalAuth__Auth0__ClientId`
- [ ] `ExternalAuth__Auth0__ClientSecret`
- [ ] `Webhook__Secret`

**Note:** Use double underscore `__` for nested keys (e.g., `Frontend__Url`)

## Step 4: Build Configuration

- [ ] Railway auto-detected .NET project
- [ ] Build command is correct (auto-detected)
- [ ] Start command: `dotnet SDMS.AuthenticationWebApp.dll`
- [ ] Root directory: `.` (root)

## Step 5: Deploy

- [ ] Push to `release` branch (for auto-deploy)
- [ ] OR manually trigger deployment
- [ ] Build completes successfully
- [ ] No errors in build logs

## Step 6: Verify Deployment

- [ ] Deployment shows as "Active" in Railway dashboard
- [ ] Service URL is accessible (e.g., `https://your-app.railway.app`)
- [ ] Health check endpoint responds (`/` or `/health`)
- [ ] Application logs show no errors

## Step 7: Database Migration

- [ ] Database migrations run (via Railway CLI or manually)
- [ ] Database tables created successfully
- [ ] Default admin user created (if configured)

## Step 8: Test Application

- [ ] Frontend can connect to Railway API
- [ ] Authentication flow works
- [ ] Login page loads
- [ ] Token generation works
- [ ] CORS is configured correctly

## Step 9: Update Frontend

- [ ] Update frontend (B2CWebApp) environment variables:
  - [ ] `API_URL` = Railway service URL
  - [ ] `AUTH_SERVER` = Railway service URL
- [ ] Redeploy frontend to Vercel

## Step 10: Final Checks

- [ ] End-to-end authentication flow works
- [ ] API endpoints respond correctly
- [ ] Static files (Angular app) are served
- [ ] HTTPS is working (Railway provides automatically)
- [ ] Custom domain configured (optional)

## Troubleshooting

If deployment fails:

- [ ] Check build logs in Railway dashboard
- [ ] Verify all environment variables are set
- [ ] Check database connection string
- [ ] Verify Node.js 18 is available for Angular build
- [ ] Check application logs for runtime errors
- [ ] Verify PORT environment variable is set (Railway sets this automatically)

## Common Issues

### Build Fails
- **Solution:** Check if Angular build completes before .NET build
- **Solution:** Verify all npm dependencies are in package.json

### Database Connection Fails
- **Solution:** Check `POSTGRES_CONNECTION` environment variable
- **Solution:** Verify PostgreSQL service is running

### Application Crashes
- **Solution:** Check application logs
- **Solution:** Verify PORT environment variable (Railway sets this automatically)

### CORS Errors
- **Solution:** Update `Frontend__Url` to actual frontend URL
- **Solution:** Check CORS configuration in Program.cs

## Success Criteria

✅ Application is accessible via Railway URL  
✅ Database is connected and migrations run  
✅ Frontend can authenticate with backend  
✅ All API endpoints respond correctly  
✅ Static files are served correctly  

---

**Need Help?**
- Railway Docs: https://docs.railway.app
- Railway Support: support@railway.app
- Check logs in Railway dashboard → Your service → Logs

