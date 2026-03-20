# AuthService 和 UserService 测试指南

## 📋 概述

本文档说明如何测试 AuthService（认证服务）和 UserService（用户服务）的基本功能。

## 🚀 快速启动

### 1. 启动 PostgreSQL 数据库

```bash
# 使用 Docker Compose
docker-compose up -d postgres

# 或者手动启动
docker run -d --name clawflgma-postgres \
  -e POSTGRES_USER=clawflgma \
  -e POSTGRES_PASSWORD=clawflgma_pass \
  -e POSTGRES_DB=clawflgma \
  -p 5432:5432 \
  postgres:16
```

### 2. 启动 Redis

```bash
# 使用 Docker Compose
docker-compose up -d redis

# 或者手动启动
docker run -d --name clawflgma-redis \
  -p 6379:6379 \
  redis:7-alpine
```

### 3. 初始化数据库

```bash
# 初始化 AuthService 数据库
docker exec -i clawflgma-postgres psql -U clawflgma < src/Services/AuthService/Data/001_init_auth.sql

# 初始化 UserService 数据库
docker exec -i clawflgma-postgres psql -U clawflgma < src/Services/UserService/Data/001_init_user.sql
```

### 4. 运行 AuthService

```bash
cd src/Services/AuthService
dotnet run
```

服务将在 `http://localhost:5001` 启动。

### 5. 运行 UserService

```bash
cd src/Services/UserService
dotnet run
```

服务将在 `http://localhost:5002` 启动。

### 6. 访问 Swagger 文档

- AuthService Swagger: http://localhost:5001/swagger
- UserService Swagger: http://localhost:5002/swagger

## 🧪 功能测试

### 方式 1: 使用自动化测试脚本

#### PowerShell (Windows)

```powershell
# 确保已安装 PowerShell
pwsh.exe test-auth.ps1
```

#### Bash (Linux/Mac)

```bash
# 确保已安装 jq
sudo apt-get install jq  # Debian/Ubuntu
brew install jq          # macOS

# 运行测试
chmod +x test-auth.sh
./test-auth.sh
```

### 方式 2: 手动测试 (curl)

#### 1. 注册新用户

```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Test123456",
    "displayName": "Test User"
  }'
```

**响应示例：**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "testuser@example.com",
    "displayName": "Test User",
    "avatarUrl": null,
    "status": "Active"
  }
}
```

#### 2. 用户登录

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Test123456"
  }'
```

**响应示例：**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4e5f6...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "testuser@example.com",
    "displayName": "Test User",
    "avatarUrl": null,
    "status": "Active"
  }
}
```

#### 3. 获取当前用户信息

```bash
# 需要先登录获取 accessToken
ACCESS_TOKEN="your-access-token-here"

curl -X GET http://localhost:5001/api/auth/me \
  -H "Authorization: Bearer $ACCESS_TOKEN"
```

**响应示例：**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "email": "testuser@example.com",
  "displayName": "Test User",
  "avatarUrl": null,
  "status": "Active"
}
```

#### 4. 刷新令牌

```bash
curl -X POST http://localhost:5001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your-refresh-token-here"
  }'
```

**响应示例：**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "b2c3d4e5f6a7...",
  "tokenType": "Bearer",
  "expiresIn": 3600,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "testuser@example.com",
    "displayName": "Test User",
    "avatarUrl": null,
    "status": "Active"
  }
}
```

#### 5. 验证令牌

```bash
curl -X POST http://localhost:5001/api/auth/verify \
  -H "Content-Type: application/json" \
  -d '{
    "token": "your-access-token-here"
  }'
```

**响应示例：**
```json
{
  "valid": true,
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "testuser@example.com"
}
```

#### 6. 登出

```bash
curl -X POST http://localhost:5001/api/auth/logout \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "your-refresh-token-here"
  }'
```

**响应示例：**
```json
{
  "message": "登出成功"
}
```

## 👤 UserService 测试

### 获取用户信息

```bash
curl -X GET http://localhost:5002/api/users/{userId}
```

### 更新用户资料

```bash
curl -X PUT http://localhost:5002/api/users/{userId}/profile \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "Updated Name",
    "bio": "Software Engineer",
    "location": "Beijing",
    "website": "https://example.com"
  }'
```

### 获取用户活动日志

```bash
curl -X GET "http://localhost:5002/api/users/{userId}/activity?page=1&pageSize=20"
```

### 创建团队

```bash
curl -X POST http://localhost:5002/api/teams \
  -H "Content-Type: application/json" \
  -H "userId: {userId}" \
  -d '{
    "name": "My Team",
    "description": "A great team",
    "visibility": "Private"
  }'
```

### 获取用户的团队

```bash
curl -X GET http://localhost:5002/api/users/{userId}/teams
```

## 🔐 JWT 配置

JWT 配置位于 `src/Services/AuthService/appsettings.json`：

```json
{
  "JWT": {
    "Issuer": "ClawFlgma",
    "Audience": "ClawFlgma.Users",
    "SecretKey": "ClawFlgma-Super-Secret-JWT-Key-2026-Must-Be-At-Least-32-Characters-Long!",
    "ExpirationMinutes": "60"
  }
}
```

### 生产环境注意事项

⚠️ **重要**：在生产环境中，请务必：

1. 修改 `SecretKey` 为强随机密钥（至少32个字符）
2. 使用环境变量或密钥管理服务存储密钥
3. 设置合理的 `ExpirationMinutes`（建议15-60分钟）
4. 启用 HTTPS
5. 配置正确的 CORS 策略

## 📊 数据库结构

### AuthService 数据库 (clawflgma_auth)

- **users**: 用户基本信息
- **refresh_tokens**: 刷新令牌
- **oauth_connections**: OAuth 第三方登录连接

### UserService 数据库 (clawflgma_user)

- **users**: 用户扩展信息
- **profiles**: 用户详细资料
- **teams**: 团队信息
- **team_members**: 团队成员
- **activity_logs**: 用户活动日志

## 🐛 故障排查

### 问题 1: 无法连接到数据库

**错误信息**：`Npgsql.PostgresException: could not connect to server`

**解决方案**：
```bash
# 检查 PostgreSQL 是否运行
docker ps | grep postgres

# 查看 PostgreSQL 日志
docker logs clawflgma-postgres

# 检查连接字符串
# 确认 appsettings.json 中的 ConnectionStrings 配置正确
```

### 问题 2: 无法连接到 Redis

**错误信息**：`StackExchange.Redis.RedisConnectionException`

**解决方案**：
```bash
# 检查 Redis 是否运行
docker ps | grep redis

# 测试 Redis 连接
docker exec -it clawflgma-redis redis-cli ping
```

### 问题 3: 令牌验证失败

**错误信息**：`System.IdentityModel.Tokens.SecurityTokenInvalidAudienceException`

**解决方案**：
- 检查 JWT 配置中的 `Issuer` 和 `Audience` 是否匹配
- 确认 `SecretKey` 在所有服务中一致
- 检查令牌是否已过期

### 问题 4: CORS 错误

**错误信息**：浏览器控制台显示 CORS 相关错误

**解决方案**：
- 在 `appsettings.json` 中添加前端地址到 `AllowedOrigins`
- 或者在 Program.cs 中配置 CORS 策略

## 📝 下一步

1. 完善错误处理和异常管理
2. 添加单元测试和集成测试
3. 实现邮箱验证功能
4. 添加密码重置功能
5. 实现 OAuth 第三方登录（GitHub、Google、微信等）
6. 添加速率限制防止暴力破解
7. 实现多因素认证（MFA）

## 📚 相关文档

- [架构设计文档](docs/architecture/microservices-design.md)
- [API 规范](docs/architecture/api-specification.md)
- [部署指南](DEPLOYMENT.md)

---

**最后更新**: 2026-03-20
