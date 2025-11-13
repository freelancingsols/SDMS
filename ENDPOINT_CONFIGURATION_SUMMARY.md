# Endpoint Configuration Summary

## ✅ All Configurations Updated

### 1. Authentication WebApp (Railway)

#### CORS Configuration
- **File**: `SDMSApps/SDMS.AuthenticationWebApp/Program.cs`
- **Changes**:
  - ✅ Added `SDMS_B2CWebApp_url` to CORS allowed origins
  - ✅ Dynamically includes both `FrontendUrl` and `B2CWebApp_url`
  - ✅ Supports localhost (dev) and production URLs

#### AppSettings Configuration
- **Files Updated**:
  - ✅ `appsettings.json` - Added `SDMS_B2CWebApp_url`
  - ✅ `appsettings.Development.json` - Already had `SDMS_B2CWebApp_url`
  - ✅ `appsettings.Production.json` - Added `SDMS_B2CWebApp_url` and `FrontendUrl` with production values

#### OpenIddict Client Configuration
- **File**: `SDMSApps/SDMS.AuthenticationWebApp/Program.cs`
- **Status**: ✅ Already configured
  - Includes B2C production redirect URIs
  - Includes localhost redirect URIs for development

#### Deployment Configuration
- **File**: `.github/workflows/deploy-auth-railway.yml`
- **Status**: ✅ Already configured
  - Includes `SDMS_B2CWebApp_url` in environment variables sync
  - Syncs to Railway environment variables

### 2. B2C WebApp (Vercel)

#### Vercel Configuration
- **File**: `SDMSApps/SDMS.B2CWebApp/vercel.json`
- **Changes**:
  - ✅ Added CORS headers:
    - `Access-Control-Allow-Origin: *`
    - `Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS`
    - `Access-Control-Allow-Headers: Content-Type, Authorization`

#### AppSettings Configuration
- **File**: `SDMSApps/SDMS.B2CWebApp/appsettings.json`
- **Status**: ✅ Already configured with all required settings

#### Deployment Configuration
- **File**: `.github/workflows/deploy-b2c-vercel.yml`
- **Status**: ✅ Already configured
  - Syncs all required environment variables to Vercel
  - Includes production and preview environments

## 📋 Configuration Checklist

### Authentication WebApp (Railway)

#### Required GitHub Variables
- [x] `SDMS_AuthenticationWebApp_FrontendUrl` = `https://sdms-pi.vercel.app`
- [x] `SDMS_B2CWebApp_url` = `https://sdms-pi.vercel.app`
- [x] `SDMS_AuthenticationWebApp_LoginUrl` = `/login`
- [x] `SDMS_AuthenticationWebApp_LogoutUrl` = `/logout`
- [x] `SDMS_AuthenticationWebApp_ErrorUrl` = `/login`
- [x] `SDMS_AuthenticationWebApp_ReturnUrlParameter` = `ReturnUrl`
- [x] `SDMS_AuthenticationWebApp_ServerPort` = (empty or port number)
- [x] `SDMS_AuthenticationWebApp_ServerUrls` = (empty or URLs)
- [x] `SDMS_AuthenticationWebApp_SigningKeyPath` = `signing-key.pem`
- [x] `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientId` = (if using Google)
- [x] `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_Domain` = (if using Auth0)
- [x] `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientId` = (if using Auth0)
- [x] `SDMS_AuthenticationWebApp_ExternalAuth_RedirectUri` = `https://sdms-pi.vercel.app/auth-callback`

#### Required GitHub Secrets
- [x] `SDMS_AuthenticationWebApp_ConnectionString` = (PostgreSQL connection string)
- [x] `SDMS_AuthenticationWebApp_ExternalAuth_Google_ClientSecret` = (if using Google)
- [x] `SDMS_AuthenticationWebApp_ExternalAuth_Auth0_ClientSecret` = (if using Auth0)
- [x] `SDMS_AuthenticationWebApp_WebhookSecret` = (if using webhooks)
- [x] `RAILWAY_TOKEN` = (Railway project token)
- [x] `RAILWAY_SERVICE_ID` = (Railway service ID)
- [x] `RAILWAY_PROJECT_ID` = (Railway project ID)
- [x] `RAILWAY_ENVIRONMENT_ID` = (Railway environment ID)

### B2C WebApp (Vercel)

#### Required GitHub Variables
- [x] `SDMS_B2CWebApp_url` = `https://sdms-pi.vercel.app`
- [x] `SDMS_AuthenticationWebApp_url` = `https://sdms-production.up.railway.app`
- [x] `SDMS_AuthenticationWebApp_clientid` = `sdms_frontend`
- [x] `SDMS_AuthenticationWebApp_redirectUri` = `https://sdms-pi.vercel.app/auth-callback`
- [x] `SDMS_AuthenticationWebApp_scope` = `openid profile email roles api`
- [x] `VERCEL_ORG_ID` = (Vercel organization/team ID)
- [x] `VERCEL_PROJECT_ID` = (Vercel project ID)

#### Required GitHub Secrets
- [x] `VERCEL_TOKEN` = (Vercel team token)

## 🔗 Endpoint URLs

### Production URLs
- **Auth Server**: `https://sdms-production.up.railway.app`
- **B2C App**: `https://sdms-pi.vercel.app`

### Key Endpoints

#### Authentication Server
- **Discovery**: `https://sdms-production.up.railway.app/.well-known/openid-configuration`
- **Authorization**: `https://sdms-production.up.railway.app/connect/authorize`
- **Token**: `https://sdms-production.up.railway.app/connect/token`
- **UserInfo**: `https://sdms-production.up.railway.app/connect/userinfo`
- **Logout**: `https://sdms-production.up.railway.app/connect/logout`
- **Login API**: `https://sdms-production.up.railway.app/account/login`
- **Register API**: `https://sdms-production.up.railway.app/account/register`
- **UserInfo API**: `https://sdms-production.up.railway.app/account/userinfo`

#### B2C App
- **Base URL**: `https://sdms-pi.vercel.app`
- **Auth Callback**: `https://sdms-pi.vercel.app/auth-callback`
- **Silent Refresh**: `https://sdms-pi.vercel.app/silent-refresh.html`

## 🔒 CORS Configuration

### Authentication Server CORS
- **Allowed Origins**:
  - `http://localhost:4200` (dev)
  - `https://localhost:4200` (dev)
  - `https://sdms-pi.vercel.app` (production B2C)
  - `SDMS_AuthenticationWebApp_FrontendUrl` (from config)
  - `SDMS_B2CWebApp_url` (from config)
- **Allowed Methods**: All (`AllowAnyMethod()`)
- **Allowed Headers**: All (`AllowAnyHeader()`)
- **Credentials**: Enabled (`AllowCredentials()`)

### B2C App CORS Headers
- **Access-Control-Allow-Origin**: `*`
- **Access-Control-Allow-Methods**: `GET, POST, PUT, DELETE, OPTIONS`
- **Access-Control-Allow-Headers**: `Content-Type, Authorization`

## ✅ Verification Steps

1. **Verify CORS**:
   - Test B2C app can make requests to auth server
   - Check browser console for CORS errors
   - Verify preflight OPTIONS requests succeed

2. **Verify OpenID Configuration**:
   - Access: `https://sdms-production.up.railway.app/.well-known/openid-configuration`
   - Should return valid OpenID discovery document

3. **Verify Authentication Flow**:
   - B2C app should redirect to auth server
   - Auth server should accept redirect from B2C URL
   - Token exchange should work
   - UserInfo endpoint should return user data

4. **Verify Environment Variables**:
   - Check Railway dashboard for all variables
   - Check Vercel dashboard for all variables
   - Verify values match GitHub variables/secrets

## 📝 Notes

- All configurations support both development (localhost) and production environments
- CORS is configured to allow requests from B2C app to auth server
- OpenIddict client includes both localhost and production redirect URIs
- All deployment workflows sync environment variables automatically
- B2C app uses Vercel environment variables (set during build)
- Auth app uses Railway environment variables (set via deployment workflow)

