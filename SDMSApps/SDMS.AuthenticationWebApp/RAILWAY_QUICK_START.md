# Railway Quick Start Guide

## 🚀 Deploy in 5 Minutes

### 1. Sign Up & Create Project
- Go to [railway.app](https://railway.app)
- Sign up with GitHub
- Click **"New Project"** → **"Deploy from GitHub repo"**
- Select your repository and `SDMS.AuthenticationWebApp` folder

### 2. Add Database
- Click **"New"** → **"Database"** → **"Add PostgreSQL"**
- Railway automatically sets `POSTGRES_CONNECTION` environment variable

### 3. Set Environment Variables
Go to your service → **Variables** tab:

```
Frontend__Url = https://your-frontend.vercel.app
Authentication__LoginUrl = /login
Authentication__LogoutUrl = /logout
Authentication__ErrorUrl = /login
```

### 4. Enable Auto-Deploy
- Go to **Settings** → **GitHub**
- Enable **"Auto Deploy"**
- Select branch: **`release`**

### 5. Deploy!
- Push to `release` branch
- Railway automatically builds and deploys
- Get your URL from the service dashboard

## 📋 Environment Variables Reference

### Required
- `POSTGRES_CONNECTION` (auto-set by Railway)
- `Frontend__Url` - Your Vercel frontend URL
- `Authentication__LoginUrl` - Login page path
- `Authentication__LogoutUrl` - Logout page path
- `Authentication__ErrorUrl` - Error page path

### Optional (External Auth)
- `ExternalAuth__Google__ClientId`
- `ExternalAuth__Google__ClientSecret`
- `ExternalAuth__Auth0__Domain`
- `ExternalAuth__Auth0__ClientId`
- `ExternalAuth__Auth0__ClientSecret`
- `Webhook__Secret`

**Note:** Use `__` (double underscore) for nested keys!

## 🔧 How It Works

1. **Railway detects** your .NET project automatically
2. **Builds Angular** frontend first (from `ClientApp/`)
3. **Builds .NET** backend
4. **Runs migrations** (if configured)
5. **Deploys** to production

## 📍 Important URLs

After deployment, Railway provides:
- **Service URL:** `https://your-app.railway.app`
- **Database:** Automatically connected
- **Logs:** Available in Railway dashboard

## 🔄 Update Frontend

After Railway deployment, update your frontend (B2CWebApp):

1. Go to GitHub Secrets or Vercel Environment Variables
2. Update:
   - `API_URL` = `https://your-app.railway.app`
   - `AUTH_SERVER` = `https://your-app.railway.app`
3. Redeploy frontend

## ✅ Verify Deployment

1. Check Railway dashboard → **Deployments** tab
2. Wait for build to complete (3-5 minutes)
3. Click service URL to test
4. Check logs for any errors

## 🐛 Common Issues

### Build Fails
- Check if Angular build completes
- Verify Node.js 18 is available
- Check build logs in Railway

### Database Connection Fails
- Verify `POSTGRES_CONNECTION` is set
- Check PostgreSQL service is running

### App Crashes
- Check application logs
- Verify all environment variables are set
- Check PORT variable (Railway sets this automatically)

## 📚 More Help

- **Full Guide:** See `RAILWAY_DEPLOYMENT_GUIDE.md`
- **Checklist:** See `RAILWAY_CHECKLIST.md`
- **Railway Docs:** https://docs.railway.app
- **Support:** support@railway.app

## 🎯 Next Steps

1. ✅ Deploy to Railway
2. ✅ Update frontend environment variables
3. ✅ Test authentication flow
4. ✅ Set up custom domain (optional)
5. ✅ Configure monitoring

---

**That's it!** Railway handles HTTPS, scaling, and monitoring automatically. 🎉

