# Build and Test Instructions

## Prerequisites

1. **Node.js 18+** and npm installed
2. **.NET 8 SDK** installed
3. **PostgreSQL** running (or Docker with postgres container)

## Step 1: Install Dependencies

### B2C Web App (Angular)
```powershell
cd SDMSApps\SDMS.B2CWebApp\ClientApp
npm install
```

### Authentication Web App (if needed)
```powershell
cd SDMSApps\SDMS.AuthenticationWebApp
dotnet restore
```

## Step 2: Start Authentication Server

The B2C app depends on the Authentication server running.

```powershell
cd SDMSApps\SDMS.AuthenticationWebApp
dotnet run
```

The authentication server will start on:
- **HTTPS**: https://localhost:7001
- **HTTP**: http://localhost:5000

Wait for the server to fully start before proceeding.

## Step 3: Build B2C Web App

```powershell
cd SDMSApps\SDMS.B2CWebApp\ClientApp
npm run build
```

If build succeeds, you should see output in the `dist/sdms-b2c-webapp` folder.

## Step 4: Run B2C Web App (Development Server)

```powershell
cd SDMSApps\SDMS.B2CWebApp\ClientApp
npm start
```

The app will be available at: **http://localhost:4200**

## Step 5: Test in Browser

1. Open browser to `http://localhost:4200`
2. You should see the new UI layout with:
   - Header at top with menu buttons
   - Left sidebar (collapsible)
   - Center canvas showing test component
   - Right sidebar (collapsible)
   - Footer at bottom

### Test Scenarios:

1. **Sidebar Collapse/Expand**
   - Click the menu icon in header to collapse/expand left sidebar
   - Click the more_vert icon in header to collapse/expand right sidebar
   - Verify smooth animations and no UI overlap

2. **Component Loading**
   - Click any menu button in header → should load test component
   - Click any menu item in left sidebar → should load test component
   - Click any quick action in right sidebar → should load test component

3. **Responsive Design**
   - Resize browser window
   - Test on mobile viewport (F12 → Toggle device toolbar)
   - Verify sidebars adapt properly

4. **Authentication Flow** (if authentication server is running)
   - Click Login button in test component
   - Should redirect to authentication server
   - After login, should return to B2C app

## Troubleshooting

### Build Errors

If you see TypeScript errors:
1. Check that all component files exist in `src/app/Components/`
2. Verify imports in `app.module.ts` are correct
3. Run `npm install` to ensure all dependencies are installed

### Port Already in Use

If port 4200 is already in use:
```powershell
# Find process using port 4200
netstat -ano | findstr :4200

# Kill the process (replace PID with actual process ID)
taskkill /PID <PID> /F
```

### Authentication Server Not Running

If authentication fails:
1. Verify authentication server is running on https://localhost:7001
2. Check `appsettings.json` has correct authentication URLs
3. Verify database is running and migrations are applied

## Quick Start Script

You can use the provided PowerShell script:

```powershell
cd SDMSApps\SDMS.B2CWebApp\ClientApp
.\build-and-run.ps1
```

Note: You may need to set execution policy:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

