# Test Authentication Flow for SDMS Authentication Web App
# Tests the complete login and token flow

$baseUrl = "https://sdms-production.up.railway.app"
$username = "admin@sdms.com"
$password = "Admin@123"
$clientId = "sdms_frontend"
$clientSecret = "sdms_frontend_secret"

Write-Host "=== SDMS Authentication Flow Test ===" -ForegroundColor Cyan
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Test 1: Ping endpoint
Write-Host "1. Testing /ping endpoint..." -ForegroundColor Green
try {
    $pingResponse = Invoke-WebRequest -Uri "$baseUrl/ping" -Method GET -Headers @{"Accept"="application/json"}
    Write-Host "   Status: $($pingResponse.StatusCode)" -ForegroundColor Green
    Write-Host "   Response: $($pingResponse.Content)" -ForegroundColor Gray
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 2: Health check
Write-Host "2. Testing /health endpoint..." -ForegroundColor Green
try {
    $healthResponse = Invoke-WebRequest -Uri "$baseUrl/health" -Method GET -Headers @{"Accept"="application/json"}
    Write-Host "   Status: $($healthResponse.StatusCode)" -ForegroundColor Green
    Write-Host "   Response: $($healthResponse.Content)" -ForegroundColor Gray
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 3: Login endpoint
Write-Host "3. Testing /account/login endpoint..." -ForegroundColor Green
try {
    $loginBody = @{
        email = $username
        password = $password
    } | ConvertTo-Json
    
    $loginResponse = Invoke-WebRequest -Uri "$baseUrl/account/login" -Method POST -ContentType "application/json" -Body $loginBody
    Write-Host "   Status: $($loginResponse.StatusCode)" -ForegroundColor Green
    $loginData = $loginResponse.Content | ConvertFrom-Json
    Write-Host "   User ID: $($loginData.userId)" -ForegroundColor Gray
    Write-Host "   Email: $($loginData.email)" -ForegroundColor Gray
    Write-Host "   Display Name: $($loginData.displayName)" -ForegroundColor Gray
    Write-Host "   Message: $($loginData.message)" -ForegroundColor Gray
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   Response: $responseBody" -ForegroundColor Red
    }
}
Write-Host ""

# Test 4: Get access token using password grant
Write-Host "4. Testing /connect/token endpoint (password grant)..." -ForegroundColor Green
try {
    # Password grant requires form-urlencoded format
    $tokenParams = @{
        grant_type = "password"
        username = $username
        password = $password
        client_id = $clientId
        client_secret = $clientSecret
        scope = "openid profile email roles"
    }
    
    $tokenBody = ($tokenParams.GetEnumerator() | ForEach-Object { "$($_.Key)=$([System.Web.HttpUtility]::UrlEncode($_.Value))" }) -join "&"
    
    $tokenResponse = Invoke-WebRequest -Uri "$baseUrl/connect/token" -Method POST -ContentType "application/x-www-form-urlencoded" -Body $tokenBody
    Write-Host "   Status: $($tokenResponse.StatusCode)" -ForegroundColor Green
    $tokenData = $tokenResponse.Content | ConvertFrom-Json
    Write-Host "   Access Token: $($tokenData.access_token.Substring(0, [Math]::Min(50, $tokenData.access_token.Length)))..." -ForegroundColor Gray
    Write-Host "   Token Type: $($tokenData.token_type)" -ForegroundColor Gray
    Write-Host "   Expires In: $($tokenData.expires_in) seconds" -ForegroundColor Gray
    if ($tokenData.refresh_token) {
        Write-Host "   Refresh Token: $($tokenData.refresh_token.Substring(0, [Math]::Min(50, $tokenData.refresh_token.Length)))..." -ForegroundColor Gray
    }
    
    $accessToken = $tokenData.access_token
} catch {
    Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "   Response: $responseBody" -ForegroundColor Red
    }
    $accessToken = $null
}
Write-Host ""

# Test 5: Get user info with access token
if ($accessToken) {
    Write-Host "5. Testing /account/userinfo endpoint..." -ForegroundColor Green
    try {
        $userInfoResponse = Invoke-WebRequest -Uri "$baseUrl/account/userinfo" -Method GET -Headers @{
            "Authorization" = "Bearer $accessToken"
            "Accept" = "application/json"
        }
        Write-Host "   Status: $($userInfoResponse.StatusCode)" -ForegroundColor Green
        $userInfoData = $userInfoResponse.Content | ConvertFrom-Json
        Write-Host "   User Info:" -ForegroundColor Gray
        $userInfoData | ConvertTo-Json -Depth 5 | Write-Host -ForegroundColor Gray
    } catch {
        Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "   Response: $responseBody" -ForegroundColor Red
        }
    }
    Write-Host ""
}

# Test 6: OpenIddict userinfo endpoint
if ($accessToken) {
    Write-Host "6. Testing /connect/userinfo endpoint..." -ForegroundColor Green
    try {
        $oidcUserInfoResponse = Invoke-WebRequest -Uri "$baseUrl/connect/userinfo" -Method GET -Headers @{
            "Authorization" = "Bearer $accessToken"
            "Accept" = "application/json"
        }
        Write-Host "   Status: $($oidcUserInfoResponse.StatusCode)" -ForegroundColor Green
        $oidcUserInfoData = $oidcUserInfoResponse.Content | ConvertFrom-Json
        Write-Host "   OIDC User Info:" -ForegroundColor Gray
        $oidcUserInfoData | ConvertTo-Json -Depth 5 | Write-Host -ForegroundColor Gray
    } catch {
        Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "   Response: $responseBody" -ForegroundColor Red
        }
    }
    Write-Host ""
}

Write-Host "=== Test Complete ===" -ForegroundColor Cyan

