# Instructions for Renaming Projects from SDSM to SDMS

## Overview
This document provides step-by-step instructions to rename all projects in the solution from "SDSM" to "SDMS".

## Step 1: Backup Your Solution
Before starting, make sure you have a backup of your solution or commit your changes to version control.

## Step 2: Close Visual Studio
Close Visual Studio completely before proceeding with renaming.

## Step 3: Run PowerShell Scripts

### Option A: Automated Scripts (Recommended)

1. **Run RENAME_PROJECTS.ps1** to rename folders and files:
   ```powershell
   cd SDSMApps
   .\RENAME_PROJECTS.ps1
   ```

2. **Run UPDATE_NAMESPACES.ps1** to update all code references:
   ```powershell
   cd SDSMApps
   .\UPDATE_NAMESPACES.ps1
   ```

### Option B: Manual Renaming

If you prefer to do it manually or the scripts don't work:

1. **Rename Solution File:**
   - Rename `SDSMApps.sln` to `SDMSApps.sln`

2. **Rename Project Folders:**
   - Rename each folder from `SDSM.*` to `SDMS.*`
   - Example: `SDSM.EndUserWebApp` → `SDMS.EndUserWebApp`

3. **Rename .csproj Files:**
   - Inside each renamed folder, rename the .csproj file
   - Example: `SDSM.EndUserWebApp.csproj` → `SDMS.EndUserWebApp.csproj`

## Step 4: Update Solution File
The solution file has already been updated, but verify that all project paths are correct.

## Step 5: Update All Code Files

### In Visual Studio (Recommended):

1. Open the solution in Visual Studio
2. Use Find & Replace (Ctrl+Shift+H):
   - Find: `namespace SDSM.`
   - Replace: `namespace SDMS.`
   - Scope: Entire Solution
   - Replace All

3. Repeat for:
   - `using SDSM.` → `using SDMS.`
   - `SDSM.Common.Infra` → `SDMS.Common.Infra`
   - `SDSM.Models` → `SDMS.Models`
   - `SDSM.ViewModels` → `SDMS.ViewModels`
   - `SDSM.BL.Common` → `SDMS.BL.Common`
   - `SDSM.DL.MySql` → `SDMS.DL.MySql`
   - `SDSM.DL.MongoDB` → `SDMS.DL.MongoDB`
   - `SDSM.ContentManagementApi` → `SDMS.ContentManagementApi`
   - `SDSM.AuthenticationApi` → `SDMS.AuthenticationApi`
   - `SDSM.CatalogApi` → `SDMS.CatalogApi`
   - `SDSM.GatewayApi` → `SDMS.GatewayApi`
   - And all other SDSM.* references

## Step 6: Update Configuration Files

### Update appsettings.json files:
- Replace all `"SDSM.` with `"SDMS.`
- Replace all `sdsm.` with `sdms.`

### Update Client IDs in AuthenticationApi:
- `sdsm.enduser.web.app` → `sdms.enduser.web.app`
- `sdsm.b2b.web.app` → `sdms.b2b.web.app`
- `sdsm.backoffice.web.app` → `sdms.backoffice.web.app`
- `sdsm.deliverypartner.web.app` → `sdms.deliverypartner.web.app`
- `sdsm.vendor.web.app` → `sdms.vendor.web.app`
- `sdsm.gateway.api` → `sdms.gateway.api`
- `sdsm.contentmanagement.api` → `sdms.contentmanagement.api`

### Update Ocelot Configuration:
- Update all route configurations in `appsettings.Ocelot.json`

## Step 7: Update Angular/TypeScript Files

Update references in:
- `package.json` files
- TypeScript files (`.ts`)
- Angular component files
- Service files

## Step 8: Update Package References

All `.csproj` files that reference other projects need to be updated:
- Change `PackageReference Include="SDSM.*"` to `PackageReference Include="SDMS.*"`

## Step 9: Rebuild Solution

1. Clean Solution (Build → Clean Solution)
2. Rebuild Solution (Build → Rebuild Solution)
3. Fix any remaining compilation errors

## Step 10: Update Database Connection Strings

If you have hardcoded database names with "SDSM" or "sdsm", update them:
- Check `appsettings.json` files
- Check `Startup.cs` files
- Update connection strings

## Step 11: Update NuGet Packages

If you're using local NuGet packages:
1. Rebuild projects to generate new packages
2. Update package references in consuming projects
3. Clear NuGet cache if needed

## Step 12: Test the Solution

1. Run all projects
2. Test authentication
3. Test API endpoints
4. Test frontend applications
5. Verify all services are working

## Common Issues and Solutions

### Issue: Project not found after renaming
**Solution:** Remove and re-add projects to the solution

### Issue: Namespace errors
**Solution:** Use Visual Studio's "Resolve" feature (Ctrl+.) or manually update using statements

### Issue: Package reference errors
**Solution:** Rebuild the referenced projects first, then rebuild dependent projects

### Issue: Configuration errors
**Solution:** Check all `appsettings.json` files for remaining SDSM references

## Verification Checklist

- [ ] Solution file renamed
- [ ] All project folders renamed
- [ ] All .csproj files renamed and updated
- [ ] All namespaces updated in code files
- [ ] All using statements updated
- [ ] All configuration files updated
- [ ] All client IDs updated
- [ ] All package references updated
- [ ] Solution builds successfully
- [ ] All projects run without errors
- [ ] Authentication works
- [ ] API endpoints accessible
- [ ] Frontend applications work

## Notes

- The PowerShell scripts attempt to automate most of the process
- Manual verification is still recommended
- Test thoroughly after renaming
- Consider doing this in a separate branch for version control
- Keep a backup of the original solution

## Completed Steps

✅ Solution file updated (SDSMApps.sln)
✅ Core .csproj files updated (Common.Infra, Models, ViewModels, etc.)
✅ PowerShell scripts created for automation

## Next Steps

1. Run the PowerShell scripts or follow manual steps
2. Update remaining .csproj files for API projects
3. Update all code files with namespace changes
4. Update configuration files
5. Test and verify everything works

