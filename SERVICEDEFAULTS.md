# ServiceDefaults 项目说明

## 概述

`ClawFlgma.ServiceDefaults` 是 `.NET Aspire` 提供的共享配置项目，为所有微服务提供统一的默认配置，包括日志、健康检查、OpenTelemetry 遥测、服务发现和 HTTP 客户端弹性。

## 项目结构

```
src/Server/ServiceDefaults/
├── ClawFlgma.ServiceDefaults.csproj    # 项目文件
└── Extensions.cs                        # 扩展方法实现
```

## 项目配置

### ClawFlgma.ServiceDefaults.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsAspireSharedProject>true</IsAspireSharedProject>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />

    <PackageReference Include="Microsoft.Extensions.Http.Resilience" Version="10.1.0" />
    <PackageReference Include="Microsoft.Extensions.ServiceDiscovery" Version="10.1.0" />
    <PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.14.0" />
    <PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.14.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.14.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.14.0" />
    <PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.14.0" />
  </ItemGroup>
</Project>
```

**关键配置说明：**
- `TargetFramework`: `net10.0` - 使用 .NET 10.0 框架
- `IsAspireSharedProject`: `true` - 标记为 Aspire 共享项目
- `FrameworkReference`: 引用 `Microsoft.AspNetCore.App` 框架

## 核心功能

### 1. AddServiceDefaults()
为服务添加默认配置，包括：
- OpenTelemetry 配置
- 健康检查配置
- 服务发现
- HTTP 客户端弹性配置

### 2. ConfigureOpenTelemetry()
配置 OpenTelemetry 遥测：
- **日志**：结构化日志收集
- **追踪**：分布式追踪（Tracing）
- **指标**：应用性能指标（Metrics）
- **OTLP Exporter**：将数据发送到 OpenTelemetry Collector

### 3. AddDefaultHealthChecks()
添加健康检查端点：
- `/health`：所有健康检查都必须通过
- `/alive`：标记为 "live" 的检查必须通过

### 4. MapDefaultEndpoints()
在 Web 应用中映射默认端点

## 如何使用

### 1. 添加项目引用

在每个服务的 `.csproj` 文件中添加：

```xml
<ItemGroup>
  <ProjectReference Include="..\..\ServiceDefaults\ClawFlgma.ServiceDefaults.csproj" />
</ItemGroup>
```

**注意**：服务项目位于 `src/Server/Services/` 目录，因此引用路径为 `..\..\ServiceDefaults\`。

### 2. 在 Program.cs 中使用

```csharp
var builder = WebApplication.CreateBuilder(args);

// 添加服务默认配置
builder.AddServiceDefaults();

// 添加服务特定配置
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// 映射默认端点
app.MapDefaultEndpoints();

app.Run();
```

## 集成的功能

### OpenTelemetry 遥测

所有服务自动启用：
- ✅ 应用程序追踪
- ✅ HTTP 客户端追踪
- ✅ ASP.NET Core 请求追踪
- ✅ 运行时指标收集
- ✅ 自定义指标和日志
- ✅ OTLP 导出器

### 健康检查

自动配置健康检查端点：
- `GET /health` - 完整健康状态
- `GET /alive` - 存活状态检查

### 服务发现

自动启用服务发现功能，支持：
- 服务名称解析
- 负载均衡
- 服务实例发现

### HTTP 客户端弹性

自动为所有 HTTP 客户端添加弹性策略：
- 重试机制
- 超时控制
- 断路器模式
- 速率限制

## 环境变量配置

### OTLP Exporter

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

### 服务发现

```bash
Services__0__Name=authservice
Services__0__Address=http://authservice:8080
```

## 集成的服务

以下服务已集成 ServiceDefaults（位于 `src/Server/Services/` 目录）：

1. ✅ **AuthService** - `src/Server/Services/AuthService/ClawFlgma.AuthService.csproj`
2. ✅ **UserService** - `src/Server/Services/UserService/`
3. ✅ **ProjectService** - `src/Server/Services/ProjectService/`
4. ✅ **DesignService** - `src/Server/Services/DesignService/`
5. ✅ **CollaborationService** - `src/Server/Services/CollaborationService/`
6. ✅ **AssetService** - `src/Server/Services/AssetService/`
7. ✅ **NotificationService** - `src/Server/Services/NotificationService/`
8. ✅ **AIService** - `src/Server/Services/AIService/`
9. ✅ **ApiGateway** - `src/Server/Gateway/ClawFlgma.ApiGateway/`

## 项目布局

```
ClawFlgma/
├── src/
│   ├── Client/                    # 客户端应用
│   └── Server/
│       ├── ServiceDefaults/       # 共享配置项目
│       │   ├── ClawFlgma.ServiceDefaults.csproj
│       │   └── Extensions.cs
│       ├── Services/              # 微服务
│       │   ├── AuthService/
│       │   ├── UserService/
│       │   ├── ProjectService/
│       │   ├── DesignService/
│       │   ├── CollaborationService/
│       │   ├── AssetService/
│       │   ├── NotificationService/
│       │   └── AIService/
│       └── Gateway/               # API 网关
│           └── ClawFlgma.ApiGateway/
└── SERVICEDEFAULTS.md             # 本文档
```

## 与 AppHost 的集成

`ServiceDefaults` 与 `AppHost` 中的 OpenTelemetry Collector 配合工作：

```csharp
var otlpCollector = builder.AddOtlpExporter("otlp")
    .WithEndpoint(serviceBinding);
```

所有服务的遥测数据通过 OTLP 协议发送到 Collector，然后转发到：
- Prometheus (指标)
- Grafana (可视化)
- 其他后端存储

## 最佳实践

1. **始终调用 AddServiceDefaults()**：在所有服务中尽早调用
2. **使用健康检查端点**：确保容器编排系统可以监控服务状态
3. **配置 OTLP Exporter**：确保遥测数据正确发送到 Collector
4. **自定义健康检查**：为服务添加特定的健康检查逻辑
5. **利用弹性策略**：HTTP 客户端自动获得弹性，无需额外配置

## 故障排查

### 健康检查失败
检查服务是否正确配置了 `AddServiceDefaults()` 和 `MapDefaultEndpoints()`。

### 遥测数据未显示
验证 OTLP Exporter 端点配置：`OTEL_EXPORTER_OTLP_ENDPOINT`

### 服务发现不工作
确保 AppHost 正确配置了服务端点和服务发现。

## 相关文档

- [.NET Aspire 文档](https://learn.microsoft.com/dotnet/aspire)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [ASP.NET Core 健康检查](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)
