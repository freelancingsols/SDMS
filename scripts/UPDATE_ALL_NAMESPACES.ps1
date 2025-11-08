# PowerShell Script to Update All Namespace References from SDSM to SDMS
# Run this script from the SDMSApps directory

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Updating Namespaces: SDSM -> SDMS" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Get the base directory
$baseDir = Get-Location
if (-not (Test-Path (Join-Path $baseDir "SDMSApps.sln"))) {
    Write-Host "ERROR: This script must be run from the SDMSApps directory!" -ForegroundColor Red
    exit 1
}

Write-Host "Base directory: $baseDir" -ForegroundColor Green
Write-Host ""

$filesUpdated = 0
$totalReplacements = 0

# Get all C# files
$csFiles = Get-ChildItem -Path $baseDir -Filter "*.cs" -Recurse -File | Where-Object { 
    $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\"
}

Write-Host "Found $($csFiles.Count) C# files to process..." -ForegroundColor Yellow
Write-Host ""

# Patterns to replace
$replacements = @(
    @{Pattern = "namespace SDSM\.Common\.Infra"; Replacement = "namespace SDMS.Common.Infra"},
    @{Pattern = "namespace SDSM\.Models"; Replacement = "namespace SDMS.Models"},
    @{Pattern = "namespace SDSM\.ViewModels"; Replacement = "namespace SDMS.ViewModels"},
    @{Pattern = "namespace SDSM\.BL\.Common"; Replacement = "namespace SDMS.BL.Common"},
    @{Pattern = "namespace SDSM\.DL\.MySql"; Replacement = "namespace SDMS.DL.MySql"},
    @{Pattern = "namespace SDSM\.DL\.MongoDB"; Replacement = "namespace SDMS.DL.MongoDB"},
    @{Pattern = "namespace SDSM\.ContentManagementApi"; Replacement = "namespace SDMS.ContentManagementApi"},
    @{Pattern = "namespace SDSM\.AuthenticationApi"; Replacement = "namespace SDMS.AuthenticationApi"},
    @{Pattern = "namespace SDSM\.CatalogApi"; Replacement = "namespace SDMS.CatalogApi"},
    @{Pattern = "namespace SDSM\.GatewayApi"; Replacement = "namespace SDMS.GatewayApi"},
    @{Pattern = "namespace SDSM\.EndUserWebApp"; Replacement = "namespace SDMS.EndUserWebApp"},
    @{Pattern = "namespace SDSM\.VendorWebApp"; Replacement = "namespace SDMS.VendorWebApp"},
    @{Pattern = "namespace SDSM\.B2BWebApp"; Replacement = "namespace SDMS.B2BWebApp"},
    @{Pattern = "namespace SDSM\.BackOfficeWebApp"; Replacement = "namespace SDMS.BackOfficeWebApp"},
    @{Pattern = "namespace SDSM\.DeliveryPartnerWebApp"; Replacement = "namespace SDMS.DeliveryPartnerWebApp"},
    
    # Using statements
    @{Pattern = "using SDSM\.Common\.Infra"; Replacement = "using SDMS.Common.Infra"},
    @{Pattern = "using SDSM\.Models"; Replacement = "using SDMS.Models"},
    @{Pattern = "using SDSM\.ViewModels"; Replacement = "using SDMS.ViewModels"},
    @{Pattern = "using SDSM\.BL\.Common"; Replacement = "using SDMS.BL.Common"},
    @{Pattern = "using SDSM\.DL\.MySql"; Replacement = "using SDMS.DL.MySql"},
    @{Pattern = "using SDSM\.DL\.MongoDB"; Replacement = "using SDMS.DL.MongoDB"},
    @{Pattern = "using SDSM\.ContentManagementApi"; Replacement = "using SDMS.ContentManagementApi"},
    @{Pattern = "using SDSM\.AuthenticationApi"; Replacement = "using SDMS.AuthenticationApi"},
    @{Pattern = "using SDSM\.CatalogApi"; Replacement = "using SDMS.CatalogApi"},
    @{Pattern = "using SDSM\.GatewayApi"; Replacement = "using SDMS.GatewayApi"},
    @{Pattern = "using SDSM\.EndUserWebApp"; Replacement = "using SDMS.EndUserWebApp"},
    @{Pattern = "using SDSM\.VendorWebApp"; Replacement = "using SDMS.VendorWebApp"},
    @{Pattern = "using SDSM\.B2BWebApp"; Replacement = "using SDMS.B2BWebApp"},
    @{Pattern = "using SDSM\.BackOfficeWebApp"; Replacement = "using SDMS.BackOfficeWebApp"},
    @{Pattern = "using SDSM\.DeliveryPartnerWebApp"; Replacement = "using SDMS.DeliveryPartnerWebApp"}
)

foreach ($file in $csFiles) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $fileReplacements = 0
    
    foreach ($replacement in $replacements) {
        if ($content -match $replacement.Pattern) {
            $content = $content -replace $replacement.Pattern, $replacement.Replacement
            $matches = [regex]::Matches($originalContent, $replacement.Pattern).Count
            $fileReplacements += $matches
        }
    }
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        $relativePath = $file.FullName.Replace($baseDir, "").TrimStart('\')
        Write-Host "  Updated: $relativePath ($fileReplacements replacements)" -ForegroundColor Green
        $filesUpdated++
        $totalReplacements += $fileReplacements
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Namespace Update Complete!" -ForegroundColor Green
Write-Host "  Files updated: $filesUpdated" -ForegroundColor Yellow
Write-Host "  Total replacements: $totalReplacements" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Update configuration files (appsettings.json, etc.)" -ForegroundColor White
Write-Host "2. Update client IDs in AuthenticationApi" -ForegroundColor White
Write-Host "3. Rebuild the solution" -ForegroundColor White

