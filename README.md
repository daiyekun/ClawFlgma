# ClawFlgma - 云原生设计协作平台

> 基于 .NET 10 + .NET Aspire 的微服务架构，对标 Figma 的在线设计协作平台

## 🎯 项目概述

ClawFlgma 是一款现代化的云原生设计协作平台，提供矢量设计、实时协作、原型制作和开发者对接等核心能力。

## 🏗️ 技术架构

### 技术栈

- **后端框架**: .NET 10 + .NET Aspire
- **架构模式**: 微服务架构
- **数据库**: PostgreSQL + MongoDB + Redis + MinIO
- **消息队列**: RabbitMQ + MassTransit
- **实时通信**: SignalR + gRPC
- **容器化**: Docker + Kubernetes
- **监控**: OpenTelemetry + Prometheus + Grafana

### 微服务列表

| 服务名称 | 端口 | 职责 |
|---------|------|------|
| Auth Service | 5001 | 认证授权、JWT管理 |
| User Service | 5002 | 用户管理、团队组织 |
| Project Service | 5003 | 项目管理、文件组织 |
| Design Service | 5004 | 设计文档、组件库 |
| Collaboration Service | 5005 | 实时协作、冲突解决 |
| Asset Service | 5006 | 资源管理、切图导出 |
| Notification Service | 5007 | 消息推送、通知管理 |
| AI Service | 5008 | 智能搜索、AI辅助 |
| API Gateway | 8000 | 统一网关、路由转发 |

## 📦 项目结构

```
ClawFlgma/
├── src/
│   ├── AppHost/                 # .NET Aspire 编排
│   ├── Shared/                  # 共享库
│   ├── Services/                # 微服务
│   │   ├── AuthService/
│   │   ├── UserService/
│   │   ├── ProjectService/
│   │   ├── DesignService/
│   │   ├── CollaborationService/
│   │   ├── AssetService/
│   │   ├── NotificationService/
│   │   └── AIService/
│   └── Gateway/
│       └── ApiGateway/
├── docs/
│   └── architecture/            # 架构文档
├── deploy/
│   ├── docker/                  # Docker 配置
│   ├── kubernetes/              # K8s 部署
│   └── scripts/                 # 部署脚本
├── ClawFlgma.sln
├── docker-compose.yml
└── README.md
```

## 🚀 快速开始

### 前置要求

- .NET 10 SDK
- Docker & Docker Compose
- Kubernetes (可选,生产环境)
- Node.js 18+ (前端)

### 本地开发

1. **克隆仓库**
   ```bash
   git clone https://github.com/your-org/ClawFlgma.git
   cd ClawFlgma
   ```

2. **启动基础设施**
   ```bash
   docker-compose up -d postgres redis rabbitmq mongodb minio
   ```

3. **运行应用**
   ```bash
   cd src/AppHost
   dotnet run
   ```

4. **访问服务**
   - API Gateway: http://localhost:8000
   - Swagger UI: http://localhost:8000/swagger
   - Prometheus: http://localhost:9090
   - Grafana: http://localhost:3000 (admin/admin)

### Docker Compose 部署

```bash
docker-compose up -d
```

### Kubernetes 部署

```bash
# 创建命名空间
kubectl apply -f deploy/kubernetes/namespace.yaml

# 部署所有服务
./deploy/scripts/deploy-all.sh
```

## 📚 文档

- [架构设计文档](docs/architecture/microservices-design.md)
- [数据流设计](docs/architecture/data-flow.md)
- [API 规范](docs/architecture/api-specification.md)
- [部署指南](docs/architecture/deployment-guide.md)

## 🔧 开发指南

### 创建新的微服务

1. 在 `src/Services/` 下创建新服务目录
2. 参考现有服务结构（如 AuthService）
3. 在 `AppHost/Program.cs` 中注册服务
4. 更新 `docker-compose.yml` 和 K8s 配置

### 数据库迁移

```bash
# PostgreSQL
dotnet ef migrations add InitialCreate --project src/Services/AuthService
dotnet ef database update --project src/Services/AuthService
```

### 运行测试

```bash
dotnet test
```

## 🤝 贡献指南

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 提交 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 🙏 致谢

- [Figma](https://www.figma.com/) - 设计灵感来源
- [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) - 云原生开发框架
- [Kubernetes](https://kubernetes.io/) - 容器编排平台

---

Made with ❤️ by ClawFlgma Team
