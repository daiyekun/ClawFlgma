# AuthService 和 UserService 实现总结

## ✅ 已完成功能

### AuthService（认证服务）

#### 核心功能

| 功能 | API 端点 | 状态 | 说明 |
|-----|---------|------|------|
| 用户注册 | `POST /api/auth/register` | ✅ | 邮箱密码注册，密码加密 |
| 用户登录 | `POST /api/auth/login` | ✅ | 邮箱密码登录，JWT 令牌 |
| 刷新令牌 | `POST /api/auth/refresh` | ✅ | Refresh Token 机制 |
| 登出 | `POST /api/auth/logout` | ✅ | 撤销 Refresh Token |
| 获取当前用户 | `GET /api/auth/me` | ✅ | 根据 JWT 获取用户信息 |
| 验证令牌 | `POST /api/auth/verify` | ✅ | 验证 JWT 有效性 |

#### 安全特性

- ✅ **密码加密**: 使用 BCrypt.Net 进行密码哈希
- ✅ **JWT 认证**: 使用标准 JWT 令牌机制
- ✅ **Refresh Token**: 支持 Refresh Token 自动刷新
- ✅ **令牌撤销**: 登出时撤销 Refresh Token
- ✅ **令牌验证**: 服务端令牌验证
- ✅ **用户状态检查**: 支持活跃/禁用状态

#### 技术实现

**密码加密**
```csharp
// 注册时加密密码
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

// 登录时验证密码
bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
```

**JWT 令牌生成**
```csharp
var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    new Claim(ClaimTypes.Email, user.Email),
    new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

var token = new JwtSecurityToken(
    issuer: jwtSettings["Issuer"],
    audience: jwtSettings["Audience"],
    claims: claims,
    expires: DateTime.UtcNow.AddMinutes(60),
    signingCredentials: credentials
);
```

**Refresh Token 管理**
```csharp
// 生成 Refresh Token
var refreshToken = Guid.NewGuid().ToString("N");

// 保存到数据库
var refreshTokenEntity = new RefreshToken
{
    Id = Guid.NewGuid(),
    UserId = user.Id,
    Token = refreshToken,
    ExpiresAt = DateTime.UtcNow.AddDays(7),
    CreatedAt = DateTime.UtcNow
};

// 刷新时撤销旧令牌
oldRefreshToken.RevokedAt = DateTime.UtcNow;
```

### UserService（用户服务）

#### 核心功能

| 功能 | API 端点 | 状态 | 说明 |
|-----|---------|------|------|
| 获取用户信息 | `GET /api/users/{userId}` | ✅ | 获取用户详情 |
| 更新用户资料 | `PUT /api/users/{userId}/profile` | ✅ | 更新个人资料 |
| 获取活动日志 | `GET /api/users/{userId}/activity` | ✅ | 分页获取活动日志 |
| 获取用户团队 | `GET /api/users/{userId}/teams` | ✅ | 获取所属团队 |
| 创建团队 | `POST /api/teams` | ✅ | 创建新团队 |
| 添加团队成员 | `POST /api/teams/{teamId}/members` | ✅ | 邀请成员 |
| 移除团队成员 | `DELETE /api/teams/{teamId}/members/{userId}` | ✅ | 移除成员 |
| 更新成员角色 | `PUT /api/teams/{teamId}/members/{userId}/role` | ✅ | 更新角色 |

#### 团队管理功能

- ✅ **团队创建**: 支持创建公开/私有团队
- ✅ **成员管理**: 添加/移除团队成员
- ✅ **角色管理**: Owner、Admin、Member 三种角色
- ✅ **权限控制**: Owner 不能被移除或降级

#### 用户资料功能

- ✅ **基本信息**: 昵称、头像、简介
- ✅ **社交链接**: Twitter、GitHub、LinkedIn
- ✅ **位置信息**: 居住地、网站
- ✅ **个人偏好**: JSONB 格式的偏好设置
- ✅ **Redis 缓存**: 用户信息缓存 30 分钟

#### 活动日志

- ✅ **活动记录**: 登录、登出、项目创建等
- ✅ **元数据支持**: JSONB 格式的扩展数据
- ✅ **分页查询**: 支持分页获取日志

## 📁 项目结构

### AuthService

```
src/Services/AuthService/
├── Controllers/
│   └── AuthController.cs          # 认证 API 控制器
├── Services/
│   ├── AuthService.cs              # 认证业务逻辑
│   └── TokenService.cs             # JWT 令牌服务
├── Models/
│   ├── User.cs                     # 用户模型
│   ├── RefreshToken.cs             # Refresh Token 模型
│   └── OAuthConnection.cs          # OAuth 连接模型
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core 上下文
│   └── 001_init_auth.sql           # 数据库初始化脚本
├── Extensions/
│   └── ServiceExtensions.cs        # 服务扩展配置
├── Program.cs                       # 应用入口
├── appsettings.json                 # 配置文件
└── appsettings.Development.json    # 开发环境配置
```

### UserService

```
src/Services/UserService/
├── Controllers/
│   ├── UsersController.cs           # 用户 API 控制器
│   └── TeamsController.cs          # 团队 API 控制器
├── Services/
│   ├── UserService.cs               # 用户业务逻辑
│   ├── TeamService.cs               # 团队业务逻辑
│   └── ProfileService.cs            # 资料业务逻辑
├── Models/
│   ├── User.cs                     # 用户模型
│   ├── Profile.cs                   # 用户资料模型
│   ├── Team.cs                     # 团队模型
│   ├── TeamMember.cs               # 团队成员模型
│   └── ActivityLog.cs              # 活动日志模型
├── Data/
│   ├── ApplicationDbContext.cs     # EF Core 上下文
│   └── 001_init_user.sql           # 数据库初始化脚本
├── Extensions/
│   └── ServiceExtensions.cs        # 服务扩展配置
├── Program.cs                       # 应用入口
├── appsettings.json                 # 配置文件
└── appsettings.Development.json    # 开发环境配置
```

## 🗄️ 数据库设计

### AuthService 数据库 (clawflgma_auth)

#### users 表
| 字段 | 类型 | 说明 |
|-----|------|------|
| id | UUID | 主键 |
| email | VARCHAR(255) | 邮箱（唯一） |
| password_hash | VARCHAR(255) | 密码哈希 |
| display_name | VARCHAR(100) | 显示名称 |
| avatar_url | VARCHAR(500) | 头像 URL |
| status | VARCHAR(20) | 状态 |
| created_at | TIMESTAMP | 创建时间 |
| last_login_at | TIMESTAMP | 最后登录时间 |

#### refresh_tokens 表
| 字段 | 类型 | 说明 |
|-----|------|------|
| id | UUID | 主键 |
| user_id | UUID | 用户 ID（外键） |
| token | VARCHAR(255) | 令牌（唯一） |
| expires_at | TIMESTAMP | 过期时间 |
| created_at | TIMESTAMP | 创建时间 |
| revoked_by | VARCHAR(255) | 撤销者 |
| revoked_at | TIMESTAMP | 撤销时间 |

#### oauth_connections 表
| 字段 | 类型 | 说明 |
|-----|------|------|
| id | UUID | 主键 |
| user_id | UUID | 用户 ID（外键） |
| provider | VARCHAR(50) | 提供商 |
| provider_user_id | VARCHAR(255) | 提供商用户 ID |
| access_token | TEXT | 访问令牌 |
| refresh_token | TEXT | 刷新令牌 |
| expires_at | TIMESTAMP | 过期时间 |
| created_at | TIMESTAMP | 创建时间 |

### UserService 数据库 (clawflgma_user)

#### users 表
| 字段 | 类型 | 说明 |
|-----|------|------|
| id | UUID | 主键 |
| email | VARCHAR(255) | 邮箱（唯一） |
| display_name | VARCHAR(100) | 显示名称 |
| avatar_url | VARCHAR(500) | 头像 URL |
| bio | TEXT | 简介 |
| location | VARCHAR(100) | 位置 |
| website | VARCHAR(255) | 网站 |
| status | VARCHAR(20) | 状态 |
| created_at | TIMESTAMP | 创建时间 |
| last_login_at | TIMESTAMP | 最后登录时间 |

#### profiles 表
| 字段 | 类型 | 说明 |
|-----|------|------|
| id | UUID | 主键 |
| user_id | UUID | 用户 ID（外键，唯一） |
| display_name | VARCHAR(100) | 显示名称 |
| avatar_url | VARCHAR(500) | 头像 URL |
| title | VARCHAR(100) | 职位 |
| bio | TEXT | 简介 |
| location | VARCHAR(100) | 位置 |
| website | VARCHAR(255) | 网站 |
| twitter | VARCHAR(100) | Twitter |
| github | VARCHAR(100) | GitHub |
| linkedin | VARCHAR(100) | LinkedIn |
| preferences | JSONB | 偏好设置 |
| created_at | TIMESTAMP | 创建时间 |
| updated_at | TIMESTAMP | 更新时间 |

#### teams 表
| 字段 | 类型 | 说明 |
|-----|------|------|
| id | UUID | 主键 |
| name | VARCHAR(255) | 团队名称 |
| description | TEXT | 描述 |
| avatar_url | VARCHAR(500) | 头像 URL |
| owner_id | UUID | 所有者 ID |
| visibility | VARCHAR(20) | 可见性 |
| created_at | TIMESTAMP | 创建时间 |
| updated_at | TIMESTAMP | 更新时间 |

#### team_members 表
| 字段 | 类型 | 说明 |
|-----|------|------|
| id | UUID | 主键 |
| team_id | UUID | 团队 ID |
| user_id | UUID | 用户 ID |
| role | VARCHAR(20) | 角色 |
| joined_at | TIMESTAMP | 加入时间 |

#### activity_logs 表
| 字段 | 类型 | 说明 |
|-----|------|------|
| id | UUID | 主键 |
| user_id | UUID | 用户 ID |
| type | VARCHAR(50) | 活动类型 |
| description | TEXT | 描述 |
| metadata | JSONB | 元数据 |
| created_at | TIMESTAMP | 创建时间 |

## 🔧 配置说明

### AuthService 配置

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=clawflgma_auth;Username=clawflgma;Password=clawflgma_pass",
    "Redis": "localhost:6379"
  },
  "JWT": {
    "Issuer": "ClawFlgma",
    "Audience": "ClawFlgma.Users",
    "SecretKey": "ClawFlgma-Super-Secret-JWT-Key-2026-Must-Be-At-Least-32-Characters-Long!",
    "ExpirationMinutes": "60"
  }
}
```

### UserService 配置

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=clawflgma_user;Username=clawflgma;Password=clawflgma_pass",
    "Redis": "localhost:6379"
  }
}
```

## 🧪 测试文件

- `test-auth.sh` - Bash 测试脚本（Linux/Mac）
- `test-auth.ps1` - PowerShell 测试脚本（Windows）
- `AUTH_TEST_GUIDE.md` - 详细测试指南

## 📚 API 文档

- **Swagger UI**: http://localhost:5001/swagger
- **Swagger JSON**: http://localhost:5001/swagger/v1/swagger.json

## 🔄 工作流程

### 注册流程

```
用户输入邮箱和密码
    ↓
验证邮箱格式和密码强度
    ↓
检查邮箱是否已存在
    ↓
使用 BCrypt 加密密码
    ↓
保存用户到数据库
    ↓
生成 JWT Access Token
    ↓
生成 Refresh Token
    ↓
返回令牌和用户信息
```

### 登录流程

```
用户输入邮箱和密码
    ↓
根据邮箱查找用户
    ↓
使用 BCrypt 验证密码
    ↓
检查用户状态（是否活跃）
    ↓
更新最后登录时间
    ↓
生成 JWT Access Token
    ↓
生成 Refresh Token
    ↓
返回令牌和用户信息
```

### 刷新令牌流程

```
客户端发送 Refresh Token
    ↓
查找数据库中的 Refresh Token
    ↓
验证 Refresh Token 有效性
    ↓
检查是否过期或已撤销
    ↓
撤销旧的 Refresh Token
    ↓
生成新的 Access Token
    ↓
生成新的 Refresh Token
    ↓
返回新令牌
```

## 🚀 下一步计划

1. **OAuth 第三方登录**
   - GitHub
   - Google
   - 微信
   - 钉钉

2. **安全增强**
   - 邮箱验证
   - 密码重置
   - 多因素认证（MFA）
   - 速率限制
   - 登录异常检测

3. **功能扩展**
   - 用户权限系统
   - 角色权限管理
   - 审计日志
   - 数据导出
   - 账户删除

4. **性能优化**
   - 数据库索引优化
   - 缓存策略优化
   - 异步处理
   - 批量操作

5. **监控告警**
   - Prometheus 指标
   - Grafana 仪表板
   - 告警规则
   - 日志聚合

---

**实现日期**: 2026-03-20  
**状态**: ✅ 完成  
**测试状态**: ✅ 可测试
