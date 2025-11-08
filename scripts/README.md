# Scripts Directory

This directory contains PowerShell scripts used for project maintenance and automation.

## Scripts

### RENAME_PROJECTS.ps1
Renames project folders and files from `SDSM.*` to `SDMS.*`.

**Usage:**
```powershell
.\RENAME_PROJECTS.ps1
```

### UPDATE_NAMESPACES.ps1
Updates all namespace references in code files from `SDSM.*` to `SDMS.*`.

**Usage:**
```powershell
.\UPDATE_NAMESPACES.ps1
```

### RENAME_ALL.ps1
Complete renaming script that performs all renaming operations.

**Usage:**
```powershell
.\RENAME_ALL.ps1
```

### UPDATE_ALL_NAMESPACES.ps1
Updates all namespaces across the entire solution.

**Usage:**
```powershell
.\UPDATE_ALL_NAMESPACES.ps1
```

### UPDATE_CONFIG_FILES.ps1
Updates configuration files (appsettings.json, etc.) to use new naming.

**Usage:**
```powershell
.\UPDATE_CONFIG_FILES.ps1
```

## Notes

- These scripts were used during the project renaming process (SDSM → SDMS)
- Most renaming has been completed
- Scripts are kept for reference and future use
- Always review scripts before running them
- Back up your work before running scripts

## Execution Policy

If you encounter execution policy errors, run:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

**Last Updated:** 2024

