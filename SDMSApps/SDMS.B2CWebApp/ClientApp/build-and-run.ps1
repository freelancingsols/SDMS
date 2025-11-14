# Build and Run Script for B2C Web App
Write-Host "Building B2C Web App..." -ForegroundColor Green

# Check if node_modules exists
if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies..." -ForegroundColor Yellow
    npm install
}

# Build the application
Write-Host "Building application..." -ForegroundColor Green
npm run build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build successful! Starting dev server..." -ForegroundColor Green
    npm start
} else {
    Write-Host "Build failed! Please check errors above." -ForegroundColor Red
    exit 1
}

