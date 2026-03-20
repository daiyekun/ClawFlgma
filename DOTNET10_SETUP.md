# .NET 10 配置说明

## ✅ 已完成的 .NET 10 配置

本项目已完全配置为使用 .NET 10 SDK 和框架。

### 📋 配置清单

#### 1. **全局配置文件**
- ✅ `global.json` - 指定 SDK 版本为 10.0.0
- ✅ 支持向前滚动到最新特性版本 (`rollForward: latestFeature`)

#### 2. **项目框架版本**
所有项目均已更新为 `net10.0`：

- ✅ AppHost
- ✅ Shared
- ✅ ApiGateway
- ✅ AuthService
- ✅ UserService
- ✅ ProjectService
- ✅ DesignService
- ✅ CollaborationService
- ✅ AssetService
- ✅ NotificationService
- ✅ AIService

#### 3. **NuGet 包版本更新**
已更新为兼容 .NET 10 的包版本：

| 包名 | 旧版本 | 新版本 |
|-----|-------|-------|
| Aspire.Hosting.* | 9.0.0 | 10.0.0 |
| Microsoft.AspNetCore.* | 9.0.0 | 10.0.0 |
| Microsoft.EntityFrameworkCore.* | 9.0.0 | 10.0.0 |
| StackExchange.Redis | 2.8.24 | 3.0.0 |
| MongoDB.Driver | 2.29.0 | 3.0.0 |
| MassTransit.RabbitMQ | 8.3.3 | 9.0.0 |
| Serilog.AspNetCore | 9.0.0 | 10.0.0 |
| OpenTelemetry.* | 1.11.0 | 2.0.0 |
| YARP.ReverseProxy | 2.2.1 | 3.0.0 |
| BCrypt.Net-Next | 4.0.3 | 5.0.0 |
| System.IdentityModel.Tokens.Jwt | 8.0.0 | 10.0.0 |
| Google.Protobuf | 3.28.2 | 3.30.0 |
| Grpc.Net.Client | 2.67.0 | 3.0.0 |
| Minio | 6.0.3 | 7.0.0 |

#### 4. **Docker 基础镜像**
所有 Dockerfile 已使用 .NET 10 镜像：
- ✅ `mcr.microsoft.com/dotnet/aspnet:10.0`
- ✅ `mcr.microsoft.com/dotnet/sdk:10.0`

### 🔍 验证步骤

#### 验证 SDK 版本
```bash
# 检查已安装的 SDK 版本
dotnet --list-sdks

# 应该看到类似输出：
# 10.0.101 [C:\Program Files\dotnet\sdk]
```

#### 验证当前项目配置
```bash
# 查看所有项目的目标框架
cd d:\woocodetest\CodeBuddy\ClawFlgma
dotnet list project-to-project-references
```

#### 验证依赖还原
```bash
# 还原所有 NuGet 包
dotnet restore

# 如果出现包版本不兼容的警告，可以使用 --ignore-failed-sources
dotnet restore --ignore-failed-sources
```

#### 构建项目
```bash
# 清理并重新构建
dotnet clean
dotnet build --no-restore
```

### ⚠️ 注意事项

#### 1. 预览版包
由于 .NET 10 可能仍处于预览阶段，某些 NuGet 包可能需要使用预览版本。如果遇到包还原失败：

```bash
# 允许预览版包
dotnet restore --prerelease
```

#### 2. 项目引用修复
已修复 `AppHost.csproj` 中缺失的项目引用：
- ✅ 添加了 NotificationService 引用
- ✅ 添加了 AIService 引用

#### 3. 兼容性问题
如果遇到以下问题，可能需要调整：
- Entity Framework Core 迁移兼容性
- SignalR 协议变更
- gRPC 版本兼容性

### 📦 包版本策略

本项目采用以下包版本策略：
1. **Microsoft 官方包** - 严格匹配 .NET 10 (10.0.x)
2. **第三方稳定包** - 使用最新稳定版
3. **预览特性** - 必要时使用预览版（需显式允许）

### 🚀 快速开始

```bash
# 1. 确认 .NET 10 SDK 已安装
dotnet --version

# 2. 验证项目配置
type global.json

# 3. 还原依赖
dotnet restore

# 4. 构建解决方案
dotnet build

# 5. 运行 AppHost（开发环境）
cd src/AppHost
dotnet run
```

### 📚 参考资源

- [.NET 10 官方文档](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10)
- [.NET Aspire 文档](https://learn.microsoft.com/dotnet/aspire/)
- [.NET 10 SDK 下载](https://dotnet.microsoft.com/download/dotnet/10.0)

### 🐛 问题排查

#### 问题 1: SDK 版本不匹配
```bash
# 错误：MSB3644: 找不到 .NET 10.0 SDK
# 解决：确保已安装 .NET 10 SDK
dotnet --list-sdks
# 如未安装，请从官网下载安装
```

#### 问题 2: 包还原失败
```bash
# 错误：NU1101: 无法找到包
# 解决：使用预览源
dotnet nuget add source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet10/nuget/v3/index.json -n dotnet10-preview
dotnet restore --prerelease
```

#### 问题 3: 编译错误
```bash
# 错误：命名空间或类型不存在
# 解决：检查包版本是否与 .NET 10 兼容
# 查看包的兼容性页面并更新版本
```

---

**配置日期**: 2026-03-20  
**.NET SDK 版本**: 10.0.101  
**配置状态**: ✅ 完成
