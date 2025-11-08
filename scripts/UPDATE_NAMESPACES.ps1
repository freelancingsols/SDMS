# PowerShell Script to Update All Namespace References from SDSM to SDMS
# This script updates all C# code files, configuration files, and JSON files

$ErrorActionPreference = "Stop"

Write-Host "Starting namespace and reference updates from SDSM to SDMS..." -ForegroundColor Green

# Get the base directory (SDSMApps)
$baseDir = $PSScriptRoot
if (-not $baseDir) {
    $baseDir = Get-Location
}

Write-Host "Base directory: $baseDir" -ForegroundColor Yellow

# File types to process
$fileExtensions = @("*.cs", "*.csproj", "*.json", "*.ts", "*.js", "*.html", "*.cshtml")

# Patterns to replace
$replacements = @{
    "namespace SDSM\." = "namespace SDMS."
    "using SDSM\." = "using SDMS."
    "SDSM\.Common\.Infra" = "SDMS.Common.Infra"
    "SDSM\.Models" = "SDMS.Models"
    "SDSM\.ViewModels" = "SDMS.ViewModels"
    "SDSM\.BL\.Common" = "SDMS.BL.Common"
    "SDSM\.DL\.MySql" = "SDMS.DL.MySql"
    "SDSM\.DL\.MongoDB" = "SDMS.DL.MongoDB"
    "SDSM\.ContentManagementApi" = "SDMS.ContentManagementApi"
    "SDSM\.AuthenticationApi" = "SDMS.AuthenticationApi"
    "SDSM\.CatalogApi" = "SDMS.CatalogApi"
    "SDSM\.GatewayApi" = "SDMS.GatewayApi"
    "SDSM\.EndUserWebApp" = "SDMS.EndUserWebApp"
    "SDSM\.VendorWebApp" = "SDMS.VendorWebApp"
    "SDSM\.B2BWebApp" = "SDMS.B2BWebApp"
    "SDSM\.BackOfficeWebApp" = "SDMS.BackOfficeWebApp"
    "SDSM\.DeliveryPartnerWebApp" = "SDMS.DeliveryPartnerWebApp"
    # String literals (case-sensitive patterns)
    '"SDSM\.' = '"SDMS.'
    "'SDSM\." = "'SDMS."
    "sdsm\." = "sdms."
    "SDSMApps" = "SDMSApps"
}

# Process each file type
foreach ($extension in $fileExtensions) {
    Write-Host "`nProcessing $extension files..." -ForegroundColor Cyan
    $files = Get-ChildItem -Path $baseDir -Filter $extension -Recurse -File | Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\|\\dist\\" }
    
    foreach ($file in $files) {
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        $originalContent = $content
        $modified = $false
        
        foreach ($pattern in $replacements.Keys) {
            if ($content -match $pattern) {
                $content = $content -replace $pattern, $replacements[$pattern]
                $modified = $true
            }
        }
        
        if ($modified) {
            Write-Host "  Updated: $($file.FullName.Replace($baseDir, ''))" -ForegroundColor Green
            Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        }
    }
}

# Special handling for appsettings.json and configuration files
Write-Host "`nProcessing configuration files..." -ForegroundColor Cyan
$configFiles = @("appsettings.json", "appsettings.Development.json", "appsettings.Ocelot.json")
foreach ($configFile in $configFiles) {
    $files = Get-ChildItem -Path $baseDir -Filter $configFile -Recurse -File | Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }
    foreach ($file in $files) {
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        $originalContent = $content
        
        # Replace SDSM references in JSON
        $content = $content -replace '"SDSM\.', '"SDMS.'
        $content = $content -replace 'SDSM\.', 'SDMS.'
        $content = $content -replace 'sdsm\.', 'sdms.'
        
        if ($content -ne $originalContent) {
            Write-Host "  Updated config: $($file.FullName.Replace($baseDir, ''))" -ForegroundColor Green
            Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        }
    }
}

Write-Host "`nNamespace and reference updates completed!" -ForegroundColor Green
Write-Host "`nPlease review the changes and test the solution." -ForegroundColor Yellow

