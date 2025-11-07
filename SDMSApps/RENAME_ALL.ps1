# PowerShell Script to Rename All Projects from SDSM to SDMS
# Run this script from the SDSMApps directory
# IMPORTANT: Close Visual Studio before running this script!

param(
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  SDMS Project Renaming Script" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Get the base directory (should be SDSMApps)
$baseDir = Get-Location
if (-not (Test-Path (Join-Path $baseDir "SDSMApps.sln"))) {
    Write-Host "ERROR: This script must be run from the SDSMApps directory!" -ForegroundColor Red
    Write-Host "Current directory: $baseDir" -ForegroundColor Yellow
    exit 1
}

Write-Host "Base directory: $baseDir" -ForegroundColor Green
Write-Host ""

if ($WhatIf) {
    Write-Host "WHAT-IF MODE: No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

# List of folders to rename (old name -> new name)
$foldersToRename = @(
    @{Old = "SDSM.EndUserWebApp"; New = "SDMS.EndUserWebApp"},
    @{Old = "SDSM.B2BWebApp"; New = "SDMS.B2BWebApp"},
    @{Old = "SDSM.BackOfficeWebApp"; New = "SDMS.BackOfficeWebApp"},
    @{Old = "SDSM.DeliveryPartnerWebApp"; New = "SDMS.DeliveryPartnerWebApp"},
    @{Old = "SDSM.VendorWebApp"; New = "SDMS.VendorWebApp"},
    @{Old = "SDSM.GatewayApi"; New = "SDMS.GatewayApi"},
    @{Old = "SDSM.CatalogApi"; New = "SDMS.CatalogApi"},
    @{Old = "SDSM.AuthenticationApi"; New = "SDMS.AuthenticationApi"},
    @{Old = "SDSM.DL.MySql"; New = "SDMS.DL.MySql"},
    @{Old = "SDSM.DL.MongoDB"; New = "SDMS.DL.MongoDB"},
    @{Old = "SDSM.BL.Common"; New = "SDMS.BL.Common"},
    @{Old = "SDSM.Models"; New = "SDMS.Models"},
    @{Old = "SDSM.ViewModels"; New = "SDMS.ViewModels"},
    @{Old = "SDSM.Common.Infra"; New = "SDMS.Common.Infra"},
    @{Old = "SDSM.ContentManagementApi"; New = "SDMS.ContentManagementApi"},
    @{Old = "SDSM.ContentManagementApi.BL"; New = "SDMS.ContentManagementApi.BL"}
)

Write-Host "Step 1: Renaming folders..." -ForegroundColor Cyan
Write-Host ""

$renamedFolders = @()

foreach ($folder in $foldersToRename) {
    $oldPath = Join-Path $baseDir $folder.Old
    $newPath = Join-Path $baseDir $folder.New
    
    if (Test-Path $oldPath) {
        if (Test-Path $newPath) {
            Write-Host "  SKIP: $($folder.New) already exists" -ForegroundColor Yellow
        } else {
            Write-Host "  RENAME: $($folder.Old) -> $($folder.New)" -ForegroundColor Green
            if (-not $WhatIf) {
                try {
                    Rename-Item -Path $oldPath -NewName $folder.New -Force
                    $renamedFolders += $folder
                } catch {
                    Write-Host "    ERROR: $_" -ForegroundColor Red
                }
            }
        }
    } else {
        Write-Host "  NOT FOUND: $($folder.Old)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Step 2: Renaming solution file..." -ForegroundColor Cyan
Write-Host ""

$solutionFile = Join-Path $baseDir "SDSMApps.sln"
$newSolutionFile = Join-Path $baseDir "SDMSApps.sln"

if (Test-Path $solutionFile) {
    if (Test-Path $newSolutionFile) {
        Write-Host "  SKIP: SDMSApps.sln already exists" -ForegroundColor Yellow
    } else {
        Write-Host "  RENAME: SDSMApps.sln -> SDMSApps.sln" -ForegroundColor Green
        if (-not $WhatIf) {
            try {
                Rename-Item -Path $solutionFile -NewName "SDMSApps.sln" -Force
            } catch {
                Write-Host "    ERROR: $_" -ForegroundColor Red
            }
        }
    }
} else {
    Write-Host "  NOT FOUND: SDSMApps.sln" -ForegroundColor Red
}

Write-Host ""
Write-Host "Step 3: Renaming .csproj files..." -ForegroundColor Cyan
Write-Host ""

# Update folder list with new names
$updatedFolders = if ($WhatIf) { $foldersToRename } else { 
    $foldersToRename | Where-Object { $renamedFolders -contains $_ } | ForEach-Object { @{Old = $_.New; New = $_.New} }
    $foldersToRename | Where-Object { $renamedFolders -notcontains $_ } | ForEach-Object { @{Old = $_.Old; New = $_.New} }
}

foreach ($folder in $updatedFolders) {
    $folderPath = Join-Path $baseDir $folder.New
    if (Test-Path $folderPath) {
        $csprojFiles = Get-ChildItem -Path $folderPath -Filter "*.csproj" -File
        foreach ($csprojFile in $csprojFiles) {
            $oldName = $csprojFile.Name
            $newName = $oldName -replace "SDSM", "SDMS"
            if ($oldName -ne $newName) {
                $oldPath = $csprojFile.FullName
                $newPath = Join-Path $csprojFile.DirectoryName $newName
                
                if (Test-Path $newPath) {
                    Write-Host "  SKIP: $newName already exists in $($folder.New)" -ForegroundColor Yellow
                } else {
                    Write-Host "  RENAME: $oldName -> $newName (in $($folder.New))" -ForegroundColor Green
                    if (-not $WhatIf) {
                        try {
                            Rename-Item -Path $oldPath -NewName $newName -Force
                        } catch {
                            Write-Host "    ERROR: $_" -ForegroundColor Red
                        }
                    }
                }
            } else {
                Write-Host "  SKIP: $oldName already has correct name" -ForegroundColor Yellow
            }
        }
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
if ($WhatIf) {
    Write-Host "  WHAT-IF mode completed. No changes made." -ForegroundColor Yellow
    Write-Host "  Run without -WhatIf to apply changes." -ForegroundColor Yellow
} else {
    Write-Host "  Folder and file renaming completed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "  Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Run UPDATE_NAMESPACES.ps1 to update code files" -ForegroundColor White
    Write-Host "  2. Or use Visual Studio Find & Replace to update namespaces" -ForegroundColor White
    Write-Host "  3. Update configuration files (appsettings.json, etc.)" -ForegroundColor White
    Write-Host "  4. Rebuild the solution" -ForegroundColor White
}
Write-Host "================================================" -ForegroundColor Cyan

