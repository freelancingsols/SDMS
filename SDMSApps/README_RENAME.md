# How to Rename Folders and Files to See Changes in Cursor

## The Problem
The file contents have been updated (namespaces, assembly names), but the physical folder names and file names are still `SDSM.*`. That's why you don't see changes in Cursor's file explorer.

## Solution: Run the Renaming Script

### Option 1: PowerShell (Recommended)

1. **Open PowerShell** as Administrator
2. **Navigate to the SDSMApps folder:**
   ```powershell
   cd "F:\wp\2\SDMS\SDSMApps"
   ```

3. **First, test what will be renamed (safe preview):**
   ```powershell
   .\RENAME_ALL.ps1 -WhatIf
   ```

4. **If the preview looks good, run the actual renaming:**
   ```powershell
   .\RENAME_ALL.ps1
   ```

### Option 2: Manual Renaming in Windows Explorer

If you prefer to do it manually:

1. **Close Visual Studio/Cursor** (important!)
2. **Open Windows Explorer** and navigate to `F:\wp\2\SDMS\SDSMApps`
3. **Rename each folder** from `SDSM.*` to `SDMS.*`:
   - `SDSM.EndUserWebApp` → `SDMS.EndUserWebApp`
   - `SDSM.B2BWebApp` → `SDMS.B2BWebApp`
   - `SDSM.BackOfficeWebApp` → `SDMS.BackOfficeWebApp`
   - `SDSM.DeliveryPartnerWebApp` → `SDMS.DeliveryPartnerWebApp`
   - `SDSM.VendorWebApp` → `SDMS.VendorWebApp`
   - `SDSM.GatewayApi` → `SDMS.GatewayApi`
   - `SDSM.CatalogApi` → `SDMS.CatalogApi`
   - `SDSM.AuthenticationApi` → `SDMS.AuthenticationApi`
   - `SDSM.DL.MySql` → `SDMS.DL.MySql`
   - `SDSM.DL.MongoDB` → `SDMS.DL.MongoDB`
   - `SDSM.BL.Common` → `SDMS.BL.Common`
   - `SDSM.Models` → `SDMS.Models`
   - `SDSM.ViewModels` → `SDMS.ViewModels`
   - `SDSM.Common.Infra` → `SDMS.Common.Infra`
   - `SDSM.ContentManagementApi` → `SDMS.ContentManagementApi`
   - `SDSM.ContentManagementApi.BL` → `SDMS.ContentManagementApi.BL`

4. **Rename the solution file:**
   - `SDSMApps.sln` → `SDMSApps.sln`

5. **Inside each renamed folder, rename the .csproj file:**
   - `SDSM.EndUserWebApp.csproj` → `SDMS.EndUserWebApp.csproj`
   - And so on for each project...

### Option 3: Using Cursor's Terminal

1. **Open Terminal in Cursor** (View → Terminal or Ctrl+`)
2. **Navigate to SDSMApps:**
   ```powershell
   cd SDSMApps
   ```

3. **Run the script:**
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\RENAME_ALL.ps1
   ```

## After Renaming

1. **Reload Cursor/VS Code** - Close and reopen the workspace
2. **The file explorer will now show the new folder names**
3. **Next, update code files** (namespaces):
   - Run `UPDATE_NAMESPACES.ps1` script, OR
   - Use Find & Replace in Cursor/Visual Studio

## Verification

After running the script, you should see:
- ✅ All folders renamed to `SDMS.*`
- ✅ Solution file renamed to `SDMSApps.sln`
- ✅ All .csproj files renamed to `SDMS.*.csproj`

## Troubleshooting

### If you get "Execution Policy" error:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### If folders are locked:
- Close Visual Studio/Cursor completely
- Close any file explorers showing those folders
- Try again

### If script fails partway:
- The script will skip files that already exist
- You can run it again safely
- Check which folders were renamed and continue manually if needed

