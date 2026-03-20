# ClawFlgma 项目结构说明

## 📁 完整目录结构

```
ClawFlgma/
├── src/
│   ├── AppHost/                          # .NET Aspire 编排主机
│   │   ├── AppHost.csproj
│   │   └── Program.cs                    # 服务编排配置
│   │
│   ├── Shared/                           # 共享库
│   │   ├── Shared.csproj
│   │   ├── Events/                       # 事件定义
│   │   │   ├── DesignEvents.cs           # 设计相关事件
│   │   │   └── CollaborationEvents.cs    # 协作相关事件
│   │   ├── Models/                       # 数据模型
│   │   │   ├── DesignModels.cs           # 设计文档模型
│   │   │   ├── CollaborationModels.cs    # 协作模型
│   │   │   └── UserModels.cs            # 用户模型
│   │   └── DTOs/                         # 数据传输对象
│   │       └── CommonDTOs.cs           # 通用DTO
│   │
│   ├── Services/                         # 微服务
│   │   ├── AuthService/                  # 认证服务 (端口: 5001)
│   │   │   ├── AuthService.csproj
│   │   │   ├── Program.cs
│   │   │   ├── Dockerfile
│   │   │   ├── appsettings.json
│   │   │   ├── Data/
│   │   │   │   └── ApplicationDbContext.cs
│   │   │   ├── Models/
│   │   │   │   └── User.cs
│   │   │   ├── Services/
│   │   │   │   ├── AuthService.cs
│   │   │   │   └── TokenService.cs
│   │   │   ├── Extensions/
│   │   │   │   └── ServiceExtensions.cs
│   │   │   └── Migrations/
│   │   │       └── 001_initial.sql
│   │   │
│   │   ├── UserService/                   # 用户服务 (端口: 5002)
│   │   ├── ProjectService/               # 项目服务 (端口: 5003)
│   │   ├── DesignService/                # 设计服务 (端口: 5004)
│   │   │   ├── DesignService.csproj
│   │   │   ├── Program.cs
│   │   │   ├── Dockerfile
│   │   │   ├── Data/
│   │   │   │   └── MongoDBContext.cs
│   │   │   ├── Models/
│   │   │   │   └── DesignDocument.cs
│   │   │   ├── Services/
│   │   │   │   └── DesignService.cs
│   │   │   └── Protos/
│   │   │       └── design.proto
│   │   │
│   │   ├── CollaborationService/         # 协作服务 (端口: 5005)
│   │   │   ├── CollaborationService.csproj
│   │   │   ├── Program.cs
│   │   │   ├── Dockerfile
│   │   │   ├── Hubs/
│   │   │   │   └── CollaborationHub.cs  # SignalR Hub
│   │   │   ├── Services/
│   │   │   │   ├── CRDTService.cs       # 冲突解决
│   │   │   │   └── CollaborationService.cs
│   │   │   └── Models/
│   │   │       └── CollaborationOperation.cs
│   │   │
│   │   ├── AssetService/                  # 资源服务 (端口: 5006)
│   │   ├── NotificationService/           # 通知服务 (端口: 5007)
│   │   └── AIService/                    # AI服务 (端口: 5008)
│   │
│   └── Gateway/
│       └── ApiGateway/                   # API网关 (端口: 8000)
│           ├── ApiGateway.csproj
│           ├── Program.cs
│           └── Dockerfile
│
├── docs/                                 # 文档
│   ├── architecture/                     # 架构文档
│   │   ├── microservices-design.md       # 微服务架构设计
│   │   ├── data-flow.md                  # 数据流设计
│   │   ├── api-specification.md          # API规范
│   │   └── deployment-guide.md           # 部署指南
│   └── Figma功能分析.md                  # Figma功能分析
│
├── deploy/                               # 部署配置
│   ├── docker/                           # Docker配置
│   ├── kubernetes/                       # K8s配置
│   │   ├── namespace.yaml
│   │   ├── configmaps.yaml
│   │   ├── secrets.yaml
│   │   ├── ingress.yaml
│   │   ├── deployments/                  # 各服务部署配置
│   │   │   ├── auth-service.yaml
│   │   │   ├── design-service.yaml
│   │   │   ├── collab-service.yaml
│   │   │   └── ...
│   │   ├── infrastructure/               # 基础设施配置
│   │   │   ├── postgres.yaml
│   │   │   ├── redis.yaml
│   │   │   ├── rabbitmq.yaml
│   │   │   ├── mongodb.yaml
│   │   │   └── minio.yaml
│   │   ├── security/                     # 安全配置
│   │   │   ├── rbac.yaml
│   │   │   ├── network-policy.yaml
│   │   │   └── psp.yaml
│   │   └── monitoring/                   # 监控配置
│   │       ├── prometheus.yaml
│   │       └── grafana.yaml
│   └── scripts/                          # 部署脚本
│       └── deploy-all.sh
│
├── ClawFlgma.sln                         # Visual Studio解决方案
├── docker-compose.yml                    # Docker Compose配置
├── .gitignore
├── .gitlab-ci.yml                       # GitLab CI/CD配置
├── github-ci-cd.yml                     # GitHub Actions配置
├── README.md                            # 项目说明
├── DEPLOYMENT.md                        # 部署指南
└── PROJECT_STRUCTURE.md                 # 本文件
```

## 🏗️ 架构层次

### 1. 编排层 (AppHost)
- 使用 .NET Aspire 编排所有微服务和基础设施
- 自动配置服务发现和依赖注入
- 统一管理配置和环境变量

### 2. 共享层 (Shared)
- 定义事件契约
- 共享数据模型
- 通用DTO和工具类

### 3. 服务层 (Services)
- 每个微服务独立部署和扩展
- 服务间通过 gRPC 和消息队列通信
- SignalR 提供实时通信能力

### 4. 基础设施层
- PostgreSQL: 关系型数据存储
- MongoDB: 文档型数据存储
- Redis: 缓存和会话管理
- RabbitMQ: 异步消息队列
- MinIO: 对象存储

### 5. 网关层 (ApiGateway)
- 统一入口
- 路由转发
- 负载均衡
- 认证授权

## 🔄 通信机制

### 同步通信 (gRPC)
- DesignService ↔ CollaborationService
- 高性能、低延迟
- 适用于服务间高频调用

### 异步通信 (RabbitMQ + MassTransit)
- DesignService → NotificationService
- ProjectService → NotificationService
- 事件驱动、解耦服务

### 实时通信 (SignalR)
- 客户端 ↔ CollaborationService
- WebSocket长连接
- 多人实时协作

## 📊 数据流

```
客户端请求
    ↓
API Gateway (YARP)
    ↓
AuthService (认证)
    ↓
UserService/ProjectService/DesignService (业务逻辑)
    ↓
数据库 (PostgreSQL/MongoDB/Redis)
    ↓
响应返回
```

## 🚀 快速启动

### 本地开发
```bash
# 1. 启动基础设施
docker-compose up -d postgres redis rabbitmq mongodb minio

# 2. 运行 AppHost
cd src/AppHost
dotnet run
```

### Docker Compose
```bash
docker-compose up -d
```

### Kubernetes
```bash
# 部署所有服务
./deploy/scripts/deploy-all.sh
```

## 📝 下一步开发建议

1. **完善微服务实现**
   - 补全各服务的业务逻辑
   - 实现gRPC接口
   - 添加SignalR Hub

2. **前端开发**
   - 创建 React + TypeScript 项目
   - 实现画布渲染引擎
   - 集成SignalR实时协作

3. **测试覆盖**
   - 编写单元测试
   - 编写集成测试
   - 配置测试流水线

4. **监控告警**
   - 配置 Prometheus + Grafana
   - 设置告警规则
   - 实现健康检查

5. **安全加固**
   - 配置 RBAC
   - 实现审计日志
   - 加强数据加密

## 🔧 技术栈总结

| 技术领域 | 选型 | 版本 |
|---------|------|------|
| 运行时 | .NET | 10.0 |
| 云原生框架 | .NET Aspire | 9.0 |
| 通信协议 | gRPC | 2.67.0 |
| 实时通信 | SignalR | 8.0.0 |
| 消息队列 | RabbitMQ | 3.x |
| ORM | Entity Framework Core | 10.0 |
| 文档数据库 | MongoDB | 7.x |
| 关系数据库 | PostgreSQL | 17 |
| 缓存 | Redis | 7.x |
| 对象存储 | MinIO | Latest |
| 容器编排 | Kubernetes | 1.28+ |
| 监控 | OpenTelemetry | 1.11.0 |
