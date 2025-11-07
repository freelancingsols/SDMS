# Project Renaming Summary - SDSM to SDMS

## Status: In Progress

This document summarizes the progress of renaming all projects from "SDSM" to "SDMS".

## Completed Tasks

### ✅ Solution File
- [x] Updated `SDSMApps.sln` with new project names and paths
- [ ] **Action Required:** Rename solution file from `SDSMApps.sln` to `SDMSApps.sln` (manual step)

### ✅ Core Library Projects (.csproj files updated)
- [x] `SDSM.Common.Infra` → `SDMS.Common.Infra`
- [x] `SDSM.Models` → `SDMS.Models`
- [x] `SDSM.ViewModels` → `SDMS.ViewModels`
- [x] `SDSM.BL.Common` → `SDMS.BL.Common`
- [x] `SDSM.DL.MySql` → `SDMS.DL.MySql`
- [x] `SDSM.DL.MongoDB` → `SDMS.DL.MongoDB`
- [x] `SDSM.ContentManagementApi.BL` → `SDMS.ContentManagementApi.BL`

### ✅ API Projects (.csproj files updated)
- [x] `SDSM.AuthenticationApi` → `SDMS.AuthenticationApi`
- [x] `SDSM.CatalogApi` → `SDMS.CatalogApi`
- [x] `SDSM.GatewayApi` → `SDMS.GatewayApi`
- [x] `SDSM.ContentManagementApi` → `SDMS.ContentManagementApi`

### ✅ Web Application Projects (.csproj files updated)
- [x] `SDSM.EndUserWebApp` → `SDMS.EndUserWebApp`
- [x] `SDSM.VendorWebApp` → `SDMS.VendorWebApp`
- [ ] `SDSM.B2BWebApp` → `SDMS.B2BWebApp` (pending)
- [ ] `SDSM.BackOfficeWebApp` → `SDMS.BackOfficeWebApp` (pending)
- [ ] `SDSM.DeliveryPartnerWebApp` → `SDMS.DeliveryPartnerWebApp` (pending)

### ✅ Package References Updated
- [x] All PackageReference entries updated in core projects
- [x] All AssemblyName and RootNamespace properties updated
- [x] All StartupObject properties updated

## Remaining Tasks

### 🔄 Folder and File Renaming
**Action Required:** Run the PowerShell script or manually rename:
- [ ] Rename solution file: `SDSMApps.sln` → `SDMSApps.sln`
- [ ] Rename all project folders from `SDSM.*` to `SDMS.*`
- [ ] Rename all `.csproj` files from `SDSM.*.csproj` to `SDMS.*.csproj`
- [ ] Update remaining web app .csproj files (B2BWebApp, BackOfficeWebApp, DeliveryPartnerWebApp)

### 📝 Code Files (Namespace Updates)
**Action Required:** Update all C# code files:
- [ ] Replace `namespace SDSM.` with `namespace SDMS.` in all `.cs` files
- [ ] Replace `using SDSM.` with `using SDMS.` in all `.cs` files
- [ ] Update all type references (e.g., `SDSM.Common.Infra` → `SDMS.Common.Infra`)
- [ ] **Use the UPDATE_NAMESPACES.ps1 script or Visual Studio Find & Replace**

### 📋 Configuration Files
**Action Required:** Update configuration files:
- [ ] Update `appsettings.json` files
- [ ] Update `appsettings.Development.json` files
- [ ] Update `appsettings.Ocelot.json`
- [ ] Update client IDs in `StaticDataHelper.cs`:
  - `sdsm.enduser.web.app` → `sdms.enduser.web.app`
  - `sdsm.b2b.web.app` → `sdms.b2b.web.app`
  - `sdsm.backoffice.web.app` → `sdms.backoffice.web.app`
  - `sdsm.deliverypartner.web.app` → `sdms.deliverypartner.web.app`
  - `sdsm.vendor.web.app` → `sdms.vendor.web.app`
  - `sdsm.gateway.api` → `sdms.gateway.api`
  - `sdsm.contentmanagement.api` → `sdms.contentmanagement.api`

### 🌐 Frontend Files (Angular/TypeScript)
**Action Required:** Update frontend code:
- [ ] Update TypeScript files (`.ts`)
- [ ] Update Angular component files
- [ ] Update service files
- [ ] Update configuration in `authorize.service.ts` (client_id, authority)
- [ ] Update `package.json` files if they reference project names

### 🔍 Verification Steps
After completing the renaming:
- [ ] Clean and rebuild solution
- [ ] Verify all projects compile without errors
- [ ] Test authentication
- [ ] Test API endpoints
- [ ] Test frontend applications
- [ ] Verify Gateway routing works
- [ ] Check all service references

## Tools Created

1. **RENAME_PROJECTS.ps1** - PowerShell script to rename folders and files
2. **UPDATE_NAMESPACES.ps1** - PowerShell script to update namespaces in code files
3. **RENAME_INSTRUCTIONS.md** - Detailed step-by-step instructions

## Quick Start

### Option 1: Automated (Recommended)
```powershell
cd SDSMApps
.\RENAME_PROJECTS.ps1
.\UPDATE_NAMESPACES.ps1
```

### Option 2: Manual (Visual Studio)
1. Open solution in Visual Studio
2. Use Find & Replace (Ctrl+Shift+H):
   - Find: `namespace SDSM.`
   - Replace: `namespace SDMS.`
   - Scope: Entire Solution
3. Repeat for `using SDSM.` → `using SDMS.`
4. Manually rename folders and files
5. Update configuration files

## Important Notes

⚠️ **Before Starting:**
- Backup your solution or commit to version control
- Close Visual Studio before renaming folders/files
- Test thoroughly after renaming

⚠️ **After Renaming:**
- You may need to remove and re-add projects to the solution
- Rebuild all projects
- Update NuGet package references if using local packages
- Clear NuGet cache if needed

## Files Modified

### Solution File
- `SDSMApps/SDSMApps.sln` - Updated with new project names

### Core Projects (.csproj)
- `SDSM.Common.Infra/SDSM.Common.Infra.csproj`
- `SDSM.Models/SDSM.Models.csproj`
- `SDSM.ViewModels/SDSM.ViewModels.csproj`
- `SDSM.BL.Common/SDSM.BL.Common.csproj`
- `SDSM.DL.MySql/SDSM.DL.MySql.csproj`
- `SDSM.DL.MongoDB/SDSM.DL.MongoDB.csproj`
- `SDSM.ContentManagementApi.BL/SDSM.ContentManagementApi.BL.csproj`

### API Projects (.csproj)
- `SDSM.AuthenticationApi/SDSM.AuthenticationApi.csproj`
- `SDSM.CatalogApi/SDSM.CatalogApi.csproj`
- `SDSM.GatewayApi/SDSM.GatewayApi.csproj`
- `SDSM.ContentManagementApi/SDSM.ContentManagementApi.csproj`

### Web App Projects (.csproj)
- `SDSM.EndUserWebApp/SDSM.EndUserWebApp.csproj`
- `SDSM.VendorWebApp/SDSM.VendorWebApp.csproj`

## Next Steps

1. **Run the PowerShell scripts** or follow manual steps
2. **Update remaining .csproj files** for B2BWebApp, BackOfficeWebApp, DeliveryPartnerWebApp
3. **Update all code files** with namespace changes (use scripts or Visual Studio)
4. **Update configuration files** (appsettings.json, client IDs, etc.)
5. **Test the solution** - rebuild and verify everything works
6. **Update documentation** - update any documentation referencing SDSM

## Support

If you encounter issues:
1. Check the `RENAME_INSTRUCTIONS.md` for detailed steps
2. Verify all .csproj files are updated
3. Check that all namespaces are updated in code files
4. Ensure configuration files are updated
5. Clean and rebuild the solution

---

**Last Updated:** [Current Date]
**Status:** Core project files updated. Folder/file renaming and code updates pending.

