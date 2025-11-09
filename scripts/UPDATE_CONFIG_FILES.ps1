# PowerShell Script to Update Configuration Files (sdsm -> sdms)
# Run this script from the SDMSApps directory

$ErrorActionPreference = "Stop"

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Updating Configuration Files: sdsm -> sdms" -ForegroundColor Cyan
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

# File types to process
$fileExtensions = @("*.json", "*.ts", "*.js", "*.html", "*.cshtml")

foreach ($extension in $fileExtensions) {
    Write-Host "Processing $extension files..." -ForegroundColor Yellow
    $files = Get-ChildItem -Path $baseDir -Filter $extension -Recurse -File | Where-Object { 
        $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\|\\dist\\|\\wwwroot\\lib\\"
    }
    
    foreach ($file in $files) {
        $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
        $originalContent = $content
        $fileReplacements = 0
        
        # Replace patterns
        $replacements = @(
            @{Pattern = '"sdsm\.'; Replacement = '"sdms.'},
            @{Pattern = "'sdsm\."; Replacement = "'sdms."},
            @{Pattern = "sdsm\.enduser\.web\.app"; Replacement = "sdms.enduser.web.app"},
            @{Pattern = "sdsm\.b2b\.web\.app"; Replacement = "sdms.b2b.web.app"},
            @{Pattern = "sdsm\.backoffice\.web\.app"; Replacement = "sdms.backoffice.web.app"},
            @{Pattern = "sdsm\.deliverypartner\.web\.app"; Replacement = "sdms.deliverypartner.web.app"},
            @{Pattern = "sdsm\.vendor\.web\.app"; Replacement = "sdms.vendor.web.app"},
            @{Pattern = "sdsm\.gateway\.api"; Replacement = "sdms.gateway.api"},
            @{Pattern = "sdsm\.contentmanagement\.api"; Replacement = "sdms.contentmanagement.api"},
            @{Pattern = "sdsm\.app\.extranet"; Replacement = "sdms.app.extranet"},
            @{Pattern = "sdsm\.app\.b2b"; Replacement = "sdms.app.b2b"},
            @{Pattern = "sdsm\.api\.api\."; Replacement = "sdms.api.api."}
        )
        
        foreach ($replacement in $replacements) {
            if ($content -match $replacement.Pattern) {
                $matches = [regex]::Matches($content, $replacement.Pattern).Count
                $content = $content -replace $replacement.Pattern, $replacement.Replacement
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
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Configuration Files Update Complete!" -ForegroundColor Green
Write-Host "  Files updated: $filesUpdated" -ForegroundColor Yellow
Write-Host "  Total replacements: $totalReplacements" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan

