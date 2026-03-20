# 认证服务测试脚本 (PowerShell)
# 用于测试 AuthService 的注册、登录、刷新令牌等功能

$ErrorActionPreference = "Stop"

$BASE_URL = "http://localhost:5001/api/auth"

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  AuthService 功能测试" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan
Write-Host ""

# 辅助函数：打印测试结果
function Print-Result {
    param(
        [string]$TestName,
        [bool]$Success,
        [string]$Message = ""
    )
    
    if ($Success) {
        Write-Host "✓ $TestName" -ForegroundColor Green
    } else {
        Write-Host "✗ $TestName" -ForegroundColor Red
        if ($Message) {
            Write-Host "  $Message" -ForegroundColor Red
        }
    }
}

# 测试1: 注册新用户
Write-Host "[测试1] 注册新用户" -ForegroundColor Yellow
$registerBody = @{
    email = "testuser@example.com"
    password = "Test123456"
    displayName = "Test User"
} | ConvertTo-Json

try {
    $registerResponse = Invoke-RestMethod -Uri "$BASE_URL/register" -Method Post -Body $registerBody -ContentType "application/json"
    Write-Host "响应: $($registerResponse | ConvertTo-Json -Depth 3)"
    
    $accessToken = $registerResponse.accessToken
    $refreshToken = $registerResponse.refreshToken
    
    if ($accessToken) {
        Print-Result "注册成功" $true
    } else {
        Print-Result "注册失败" $false
        exit 1
    }
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 测试2: 使用错误密码登录
Write-Host "[测试2] 使用错误密码登录" -ForegroundColor Yellow
$loginFailBody = @{
    email = "testuser@example.com"
    password = "WrongPassword"
} | ConvertTo-Json

try {
    $loginFailResponse = Invoke-RestMethod -Uri "$BASE_URL/login" -Method Post -Body $loginFailBody -ContentType "application/json" -ErrorAction SilentlyContinue
    Write-Host "响应: $($loginFailResponse | ConvertTo-Json)"
    
    if ($loginFailResponse.message) {
        Print-Result "正确拒绝了错误密码" $true
    } else {
        Print-Result "应该拒绝错误密码" $false
    }
} catch {
    Write-Host "正确拒绝了错误密码 (HTTP 401)" -ForegroundColor Green
}
Write-Host ""

# 测试3: 使用正确密码登录
Write-Host "[测试3] 使用正确密码登录" -ForegroundColor Yellow
$loginBody = @{
    email = "testuser@example.com"
    password = "Test123456"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$BASE_URL/login" -Method Post -Body $loginBody -ContentType "application/json"
    Write-Host "响应: $($loginResponse | ConvertTo-Json -Depth 3)"
    
    $accessToken = $loginResponse.accessToken
    $refreshToken = $loginResponse.refreshToken
    
    if ($accessToken) {
        Print-Result "登录成功" $true
    } else {
        Print-Result "登录失败" $false
        exit 1
    }
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
    exit 1
}
Write-Host ""

# 测试4: 获取当前用户信息
Write-Host "[测试4] 获取当前用户信息" -ForegroundColor Yellow
try {
    $meResponse = Invoke-RestMethod -Uri "$BASE_URL/me" -Method Get -Headers @{ Authorization = "Bearer $accessToken" }
    Write-Host "响应: $($meResponse | ConvertTo-Json)"
    
    if ($meResponse.email) {
        Print-Result "获取用户信息成功" $true
    } else {
        Print-Result "获取用户信息失败" $false
    }
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
}
Write-Host ""

# 测试5: 刷新令牌
Write-Host "[测试5] 刷新令牌" -ForegroundColor Yellow
$refreshBody = @{
    refreshToken = $refreshToken
} | ConvertTo-Json

try {
    $refreshResponse = Invoke-RestMethod -Uri "$BASE_URL/refresh" -Method Post -Body $refreshBody -ContentType "application/json"
    Write-Host "响应: $($refreshResponse | ConvertTo-Json -Depth 3)"
    
    $newAccessToken = $refreshResponse.accessToken
    $newRefreshToken = $refreshResponse.refreshToken
    
    if ($newAccessToken) {
        Print-Result "刷新令牌成功" $true
        # 更新 token
        $accessToken = $newAccessToken
        $refreshToken = $newRefreshToken
    } else {
        Print-Result "刷新令牌失败" $false
    }
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
}
Write-Host ""

# 测试6: 验证新令牌
Write-Host "[测试6] 验证新令牌" -ForegroundColor Yellow
$verifyBody = @{
    token = $accessToken
} | ConvertTo-Json

try {
    $verifyResponse = Invoke-RestMethod -Uri "$BASE_URL/verify" -Method Post -Body $verifyBody -ContentType "application/json"
    Write-Host "响应: $($verifyResponse | ConvertTo-Json)"
    
    if ($verifyResponse.valid) {
        Print-Result "令牌验证成功" $true
    } else {
        Print-Result "令牌验证失败" $false
    }
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
}
Write-Host ""

# 测试7: 登出
Write-Host "[测试7] 登出" -ForegroundColor Yellow
$logoutBody = @{
    refreshToken = $refreshToken
} | ConvertTo-Json

try {
    $logoutResponse = Invoke-RestMethod -Uri "$BASE_URL/logout" -Method Post -Body $logoutBody -ContentType "application/json" -Headers @{ Authorization = "Bearer $accessToken" }
    Write-Host "响应: $($logoutResponse | ConvertTo-Json)"
    
    if ($logoutResponse.message) {
        Print-Result "登出成功" $true
    } else {
        Print-Result "登出失败" $false
    }
} catch {
    Write-Host "错误: $_" -ForegroundColor Red
}
Write-Host ""

# 测试8: 尝试使用已撤销的 refresh token
Write-Host "[测试8] 尝试使用已撤销的 refresh token" -ForegroundColor Yellow
$refreshAgainBody = @{
    refreshToken = $refreshToken
} | ConvertTo-Json

try {
    $refreshAgainResponse = Invoke-RestMethod -Uri "$BASE_URL/refresh" -Method Post -Body $refreshAgainBody -ContentType "application/json" -ErrorAction SilentlyContinue
    Write-Host "响应: $($refreshAgainResponse | ConvertTo-Json)"
    
    if ($refreshAgainResponse.message) {
        Print-Result "正确拒绝了已撤销的 refresh token" $true
    } else {
        Print-Result "应该拒绝已撤销的 refresh token" $false
    }
} catch {
    Write-Host "正确拒绝了已撤销的 refresh token (HTTP 401)" -ForegroundColor Green
}
Write-Host ""

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  所有测试完成!" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan
