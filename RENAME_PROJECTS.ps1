# PowerShell Script to Rename All Projects from SDSM to SDMS
# Run this script from the SDSMApps directory

$ErrorActionPreference = "Stop"

Write-Host "Starting project renaming from SDSM to SDMS..." -ForegroundColor Green

# Get the base directory (SDSMApps)
$baseDir = $PSScriptRoot
if (-not $baseDir) {
    $baseDir = Get-Location
}

Write-Host "Base directory: $baseDir" -ForegroundColor Yellow

# List of folders to rename
$foldersToRename = @(
    "SDSM.EndUserWebApp",
    "SDSM.B2BWebApp",
    "SDSM.BackOfficeWebApp",
    "SDSM.DeliveryPartnerWebApp",
    "SDSM.VendorWebApp",
    "SDSM.GatewayApi",
    "SDSM.CatalogApi",
    "SDSM.AuthenticationApi",
    "SDSM.DL.MySql",
    "SDSM.DL.MongoDB",
    "SDSM.BL.Common",
    "SDSM.Models",
    "SDSM.ViewModels",
    "SDSM.Common.Infra",
    "SDSM.ContentManagementApi",
    "SDSM.ContentManagementApi.BL"
)

# Rename folders
foreach ($folder in $foldersToRename) {
    $oldPath = Join-Path $baseDir $folder
    $newFolder = $folder -replace "SDSM", "SDMS"
    $newPath = Join-Path $baseDir $newFolder
    
    if (Test-Path $oldPath) {
        Write-Host "Renaming folder: $folder -> $newFolder" -ForegroundColor Cyan
        Rename-Item -Path $oldPath -NewName $newFolder -Force
    } else {
        Write-Host "Folder not found: $oldPath" -ForegroundColor Red
    }
}

# Rename solution file
$solutionFile = Join-Path $baseDir "SDSMApps.sln"
if (Test-Path $solutionFile) {
    Write-Host "Renaming solution file: SDSMApps.sln -> SDMSApps.sln" -ForegroundColor Cyan
    Rename-Item -Path $solutionFile -NewName "SDMSApps.sln" -Force
}

# Rename .csproj files inside renamed folders
$newFolders = $foldersToRename | ForEach-Object { $_ -replace "SDSM", "SDMS" }
foreach ($folder in $newFolders) {
    $folderPath = Join-Path $baseDir $folder
    if (Test-Path $folderPath) {
        $csprojFiles = Get-ChildItem -Path $folderPath -Filter "*.csproj" -Recurse
        foreach ($csprojFile in $csprojFiles) {
            $oldName = $csprojFile.Name
            $newName = $oldName -replace "SDSM", "SDMS"
            if ($oldName -ne $newName) {
                Write-Host "Renaming .csproj: $oldName -> $newName" -ForegroundColor Cyan
                Rename-Item -Path $csprojFile.FullName -NewName $newName -Force
            }
        }
    }
}

Write-Host "`nFolder and file renaming completed!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Update all namespace references in code files (use Find & Replace in Visual Studio)" -ForegroundColor White
Write-Host "2. Update all 'using SDSM' statements to 'using SDMS'" -ForegroundColor White
Write-Host "3. Update project references in .csproj files" -ForegroundColor White
Write-Host "4. Update configuration files (appsettings.json, etc.)" -ForegroundColor White
Write-Host "5. Update any string literals containing 'SDSM' or 'sdsm'" -ForegroundColor White

