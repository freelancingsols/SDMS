# Next Steps After Renaming Projects to SDMS

## ✅ Completed
- [x] Folders renamed from `SDSM.*` to `SDMS.*`
- [x] Solution file renamed to `SDMSApps.sln`
- [x] .csproj files renamed
- [x] .csproj file contents updated (AssemblyName, RootNamespace)

## 🔄 Next Critical Step: Update Namespaces in Code Files

Now you need to update all the code files to use the new `SDMS.*` namespaces instead of `SDSM.*`.

### Option 1: Using PowerShell Script (Automated)

If you have the `UPDATE_NAMESPACES.ps1` script:

```powershell
cd SDMSApps  # or SDSMApps if that's what it's called now
.\UPDATE_NAMESPACES.ps1
```

### Option 2: Using Cursor/VS Code Find & Replace (Recommended)

1. **Open the solution/folder in Cursor**
2. **Press `Ctrl+Shift+H`** to open Find & Replace
3. **Enable "Use Regular Expression"** (Regex mode)
4. **Update namespaces:**

   **a) Update namespace declarations:**
   - Find: `namespace SDSM\.`
   - Replace: `namespace SDMS.`
   - Files to include: `*.cs`
   - Click "Replace All"

   **b) Update using statements:**
   - Find: `using SDSM\.`
   - Replace: `using SDMS.`
   - Files to include: `*.cs`
   - Click "Replace All"

   **c) Update type references:**
   - Find: `SDSM\.Common\.Infra`
   - Replace: `SDMS.Common.Infra`
   - Files to include: `*.cs`
   - Click "Replace All"
   
   - Repeat for:
     - `SDSM.Models` → `SDMS.Models`
     - `SDSM.ViewModels` → `SDMS.ViewModels`
     - `SDSM.BL.Common` → `SDMS.BL.Common`
     - `SDSM.DL.MySql` → `SDMS.DL.MySql`
     - `SDSM.DL.MongoDB` → `SDMS.DL.MongoDB`
     - `SDSM.AuthenticationApi` → `SDMS.AuthenticationApi`
     - `SDSM.CatalogApi` → `SDMS.CatalogApi`
     - `SDSM.GatewayApi` → `SDMS.GatewayApi`
     - `SDSM.ContentManagementApi` → `SDMS.ContentManagementApi`
     - `SDSM.EndUserWebApp` → `SDMS.EndUserWebApp`
     - `SDSM.VendorWebApp` → `SDMS.VendorWebApp`
     - `SDSM.B2BWebApp` → `SDMS.B2BWebApp`
     - `SDSM.BackOfficeWebApp` → `SDMS.BackOfficeWebApp`
     - `SDSM.DeliveryPartnerWebApp` → `SDMS.DeliveryPartnerWebApp`

### Option 3: Manual Update (If needed)

If you encounter issues, you can manually update key files:

1. **Program.cs files** - Update namespace and using statements
2. **Startup.cs files** - Update namespace and using statements
3. **Controller files** - Update namespace and using statements
4. **All .cs files** - Update namespace declarations

## Step 3: Update Configuration Files

### Update appsettings.json files

Search for and replace in all `appsettings.json` and `appsettings.Development.json`:
- `"SDSM.` → `"SDMS.`
- `sdsm.` → `sdms.`

### Update Authentication Client IDs

In `SDMS.AuthenticationApi/Helper/StaticDataHelper.cs`:

Update:
- `sdsm.enduser.web.app` → `sdms.enduser.web.app`
- `sdsm.b2b.web.app` → `sdms.b2b.web.app`
- `sdsm.backoffice.web.app` → `sdms.backoffice.web.app`
- `sdsm.deliverypartner.web.app` → `sdms.deliverypartner.web.app`
- `sdsm.vendor.web.app` → `sdms.vendor.web.app`
- `sdsm.gateway.api` → `sdms.gateway.api`
- `sdsm.contentmanagement.api` → `sdms.contentmanagement.api`

### Update Ocelot Configuration

In `SDMS.GatewayApi/appsettings.Ocelot.json`:
- Update any references to `SDSM` or `sdsm` to `SDMS` or `sdms`

### Update Angular/TypeScript Files

In frontend ClientApp folders, update:
- `authorize.service.ts` - Update `client_id` and `authority` values
- Any configuration files referencing the old names

## Step 4: Rebuild Solution

1. **Clean Solution:**
   - In Visual Studio: Build → Clean Solution
   - Or in terminal: `dotnet clean`

2. **Restore NuGet Packages:**
   ```powershell
   dotnet restore
   ```

3. **Rebuild Solution:**
   - In Visual Studio: Build → Rebuild Solution
   - Or in terminal: `dotnet build`

## Step 5: Verify Everything Works

1. **Check for Compilation Errors:**
   - All projects should build without errors
   - Fix any namespace or reference issues

2. **Test Authentication:**
   - Verify Identity Server works
   - Test login/logout

3. **Test API Endpoints:**
   - Verify Gateway API routes work
   - Test individual API services

4. **Test Frontend Apps:**
   - Verify Angular apps compile
   - Test authentication flow

## Common Issues and Solutions

### Issue: "The type or namespace name 'SDSM' could not be found"
**Solution:** Update the using statement to `SDMS.*`

### Issue: Project references broken
**Solution:** 
1. Remove and re-add project references
2. Or update the .csproj file to use the new project names

### Issue: NuGet package not found
**Solution:**
1. Rebuild the referenced projects first
2. Then rebuild dependent projects
3. Or update package references if using local NuGet packages

### Issue: Configuration errors
**Solution:** Check all `appsettings.json` files for remaining `SDSM` or `sdsm` references

## Verification Checklist

- [ ] All `namespace SDSM.*` changed to `namespace SDMS.*`
- [ ] All `using SDSM.*` changed to `using SDMS.*`
- [ ] All configuration files updated
- [ ] Client IDs updated in AuthenticationApi
- [ ] Solution builds without errors
- [ ] All projects compile successfully
- [ ] Authentication works
- [ ] API endpoints accessible
- [ ] Frontend applications work

## Quick Find & Replace Commands

If using VS Code/Cursor, here are the exact regex patterns:

1. **Namespaces:**
   - Find: `(namespace\s+)SDSM\.`
   - Replace: `$1SDMS.`

2. **Using statements:**
   - Find: `(using\s+)SDSM\.`
   - Replace: `$1SDMS.`

3. **Full type references:**
   - Find: `SDSM\.([A-Za-z]+)\.([A-Za-z]+)`
   - Replace: `SDMS.$1.$2`

## After Completion

Once everything is updated and working:
1. ✅ Commit changes to version control
2. ✅ Update any documentation
3. ✅ Update deployment scripts if needed
4. ✅ Notify team members of the change

---

**Status:** Folder/file renaming complete ✅  
**Next:** Update namespaces in code files 🔄

