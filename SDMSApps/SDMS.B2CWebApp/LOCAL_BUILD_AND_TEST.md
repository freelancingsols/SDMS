# Local Build and Test Guide

## Quick Start

### Option 1: Using Command Prompt (Recommended to avoid PowerShell execution policy issues)

1. **Open Command Prompt (cmd.exe)** - NOT PowerShell

2. **Start Authentication Server** (in first terminal):
```cmd
cd F:\wp\2\SDMS\SDMSApps\SDMS.AuthenticationWebApp
dotnet run
```
Wait for: "Now listening on: https://localhost:7001"

3. **Build and Run B2C App** (in second terminal):
```cmd
cd F:\wp\2\SDMS\SDMSApps\SDMS.B2CWebApp\ClientApp
npm run build
npm start
```
Wait for: "Angular Live Development Server is listening on localhost:4200"

4. **Open Browser**: Navigate to `http://localhost:4200`

### Option 2: Using PowerShell (if execution policy allows)

1. **Set Execution Policy** (run once as Administrator):
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

2. **Start Authentication Server**:
```powershell
cd SDMSApps\SDMS.AuthenticationWebApp
dotnet run
```

3. **Build and Run B2C App**:
```powershell
cd SDMSApps\SDMS.B2CWebApp\ClientApp
npm run build
npm start
```

## What to Test

### 1. UI Layout Verification
- ✅ Header at top with brand logo and menu buttons
- ✅ Left sidebar with menu items (collapsible)
- ✅ Center canvas showing test component
- ✅ Right sidebar with quick actions (collapsible)
- ✅ Footer at bottom

### 2. Sidebar Functionality
- Click **menu icon** (☰) in header → Left sidebar should collapse/expand
- Click **more_vert icon** (⋮) in header → Right sidebar should collapse/expand
- Verify smooth animations
- Verify no UI overlap when collapsed

### 3. Component Loading
- Click **Dashboard** button in header → Test component loads in center
- Click **Products** button in header → Test component loads in center
- Click **Orders** button in header → Test component loads in center
- Click **Profile** button in header → Test component loads in center
- Click any item in **left sidebar** → Test component loads in center
- Click any item in **right sidebar** → Test component loads in center

### 4. Test Component Features
- Should display "Test Component" card
- Should show user information (if logged in)
- Login button should redirect to authentication
- Logout button should clear session

### 5. Responsive Design
- Resize browser window
- Open DevTools (F12) → Toggle device toolbar
- Test on mobile viewport (375x667)
- Verify sidebars adapt properly on small screens

### 6. Authentication Flow (if auth server running)
- Click **Login** in test component
- Should redirect to authentication server
- After login, should return to B2C app
- User name should appear in test component

## Expected Build Output

When build succeeds, you should see:
```
✔ Browser application bundle generation complete.
✔ Copying assets complete.
✔ Index html generation complete.

Initial chunk files   | Names         |  Raw size
...
```

Output directory: `ClientApp/dist/sdms-b2c-webapp/`

## Troubleshooting

### Build Errors

**Error: "File is not a module"**
- Solution: All component files are correctly created. This might be a TypeScript cache issue.
- Try: Delete `node_modules/.cache` and rebuild

**Error: "Cannot find module '@angular/material/...'"**
- Solution: Run `npm install` to ensure all dependencies are installed

**Error: "Port 4200 already in use"**
- Solution: 
```cmd
netstat -ano | findstr :4200
taskkill /PID <PID> /F
```

### Runtime Errors

**Error: "Cannot GET /"**
- Solution: Make sure you're accessing `http://localhost:4200` (not https)

**Error: Authentication not working**
- Solution: Verify authentication server is running on https://localhost:7001
- Check browser console for CORS errors
- Verify `appsettings.json` has correct URLs

**Error: Material Icons not showing**
- Solution: Check network tab - icons should load from Google Fonts
- Verify internet connection

## Verification Checklist

After successful build and run:

- [ ] Application builds without errors
- [ ] Dev server starts on port 4200
- [ ] UI loads with all components visible
- [ ] Header displays correctly
- [ ] Left sidebar displays and collapses
- [ ] Right sidebar displays and collapses
- [ ] Center canvas shows test component
- [ ] Footer displays correctly
- [ ] All menu buttons work
- [ ] Test component loads from all sources
- [ ] Responsive design works
- [ ] No console errors in browser
- [ ] Material icons display correctly
- [ ] Smooth animations work
- [ ] Authentication flow works (if auth server running)

## Files Created/Modified

### New Components:
- `Components/header-new/` - Header component
- `Components/footer-new/` - Footer component
- `Components/left-sidebar/` - Left sidebar component
- `Components/right-sidebar/` - Right sidebar component
- `Components/center-canvas/` - Center canvas component

### Modified Files:
- `app.module.ts` - Added new component declarations
- `framework-body/` - Updated to use new layout
- `test/` - Enhanced with Material Design
- `styles.css` - Updated global styles

### Removed:
- All image files from `assets/` folder (kept only icons and config files)

## Next Steps

Once everything is working:

1. **Add More Components**: Create new components to load in center canvas
2. **Add Routing**: Implement proper routing for different components
3. **Add State Management**: Implement component state persistence
4. **Add Animations**: Enhance with page transition animations
5. **Add Services**: Create services for data management

