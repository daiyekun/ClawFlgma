# .NET Aspire AppHost 完整指南

## 📋 概述

AppHost 是 .NET Aspire 的分布式应用编排器，负责管理和启动所有微服务、基础设施组件以及监控工具。

## 🏗️ 架构组件

### 基础设施 (Infrastructure)

| 组件 | 端口 | 说明 |
|-------|-------|------|
| PostgreSQL | 5432 | 关系型数据库 |
| PgAdmin | 5050 | PostgreSQL 管理界面 |
| Redis | 6379 | 缓存和会话存储 |
| Redis Commander | 8082 | Redis 管理界面 |
| RabbitMQ | 5672 | 消息队列 |
| RabbitMQ Management | 15672 | RabbitMQ 管理界面 |
| MongoDB | 27017 | 文档数据库 |
| Mongo Express | 8081 | MongoDB 管理界面 |
| MinIO | 9000 (API), 9001 (Console) | 对象存储 |

### 微服务 (Microservices)

| 服务 | 端口 | 数据库 | 说明 |
|-----|-------|-------|------|
| Auth Service | 5001 | PostgreSQL | 认证授权 |
| User Service | 5002 | PostgreSQL | 用户管理 |
| Project Service | 5003 | PostgreSQL, RabbitMQ | 项目管理 |
| Design Service | 5004 | MongoDB, RabbitMQ | 设计文档 |
| Collaboration Service | 5005 | MongoDB, RabbitMQ, Redis | 实时协作 |
| Asset Service | 5006 | PostgreSQL, MinIO | 资源管理 |
| Notification Service | 5007 | PostgreSQL, RabbitMQ | 消息通知 |
| AI Service | 5008 | MongoDB | AI 辅助 |

### 网关和监控 (Gateway & Monitoring)

| 组件 | 端口 | 说明 |
|-------|-------|------|
| API Gateway | 8000 | 统一 API 网关 |
| Grafana | 3000 | 监控仪表板 |
| Prometheus | 9090 | 指标采集 |

## 🚀 快速启动

### 前置要求

- ✅ .NET 10 SDK
- ✅ Docker Desktop 或 Podman
- ✅ 至少 8GB RAM
- ✅ 至少 20GB 磁盘空间

### 步骤 1: 拉取基础镜像

```bash
docker pull postgres:16-alpine
docker pull redis:7-alpine
docker pull rabbitmq:4-management
docker pull mongo:8
docker pull minio/minio:latest
docker pull grafana/grafana:latest
docker pull prom/prometheus:latest
```

### 步骤 2: 初始化数据库

```bash
# 方式 1: 使用 psql
docker exec -it clawflgma-postgres psql -U clawflgma < src/AppHost/init-databases.sql

# 方式 2: 连接到 PostgreSQL 后执行
psql -h localhost -p 5432 -U clawflgma -f src/AppHost/init-databases.sql
```

### 步骤 3: 运行 AppHost

```bash
cd src/AppHost
dotnet run
```

AppHost 会自动启动所有配置的服务和基础设施。

### 步骤 4: 访问服务

#### 管理界面

| 服务 | URL | 用户名/密码 |
|-----|------|------------|
| API Gateway | http://localhost:8000 | - |
| Grafana | http://localhost:3000 | admin/admin |
| Prometheus | http://localhost:9090 | - |
| PgAdmin | http://localhost:5050 | clawflgma/clawflgma_pass |
| Redis Commander | http://localhost:8082 | - |
| RabbitMQ Management | http://localhost:15672 | clawflgma/clawflgma_pass |
| Mongo Express | http://localhost:8081 | - |
| MinIO Console | http://localhost:9001 | clawflgma/clawflgma_pass |

#### 服务 Swagger 文档

| 服务 | Swagger URL |
|-----|-----------|
| Auth Service | http://localhost:5001/swagger |
| User Service | http://localhost:5002/swagger |
| Project Service | http://localhost:5003/swagger |
| Design Service | http://localhost:5004/swagger |
| Collaboration Service | http://localhost:5005/swagger |
| Asset Service | http://localhost:5006/swagger |
| Notification Service | http://localhost:5007/swagger |
| AI Service | http://localhost:5008/swagger |

## 📊 监控配置

### Grafana 仪表板

1. 访问 http://localhost:3000
2. 使用 admin/admin 登录
3. 添加 Prometheus 数据源：
   - Name: Prometheus
   - URL: http://prometheus:9090
4. 导入预配置的仪表板（待创建）

### Prometheus 指标

Prometheus 已配置为采集以下指标：

- **服务健康检查**: 每 15 秒
- **基础设施状态**: 每 30 秒
- **API 性能**: 响应时间、错误率
- **系统资源**: CPU、内存使用率
- **队列状态**: RabbitMQ 队列积压

### 告警规则

已配置的告警规则（位于 `alert_rules.yml`）：

#### 服务健康
- 服务宕机超过 2 分钟 → Critical
- PostgreSQL 不可用 → Critical
- Redis 不可用 → Critical
- RabbitMQ 不可用 → Critical

#### 性能
- CPU 使用率 > 80% → Warning
- 内存使用率 > 80% → Warning
- API 错误率 > 5% → Warning
- P95 响应时间 > 1 秒 → Warning

#### 队列
- RabbitMQ 队列积压 > 1000 条 → Warning

## 🔧 配置说明

### 数据库连接字符串

所有服务的数据库连接已配置为：

```bash
# PostgreSQL
Host=postgres;Port=5432;Database={db_name};Username=clawflgma;Password=clawflgma_pass

# MongoDB
mongodb://clawflgma:clawflgma_pass@mongodb:27017/{db_name}?authSource=admin

# Redis
redis:6379
```

### 服务依赖关系

```
Auth Service → PostgreSQL, Redis
User Service → PostgreSQL, Redis
Project Service → PostgreSQL, Redis, RabbitMQ
Design Service → MongoDB, Redis, RabbitMQ
Collaboration Service → MongoDB, Redis, RabbitMQ
Asset Service → PostgreSQL, MinIO
Notification Service → PostgreSQL, RabbitMQ
AI Service → MongoDB
API Gateway → All Services
```

### 环境变量

所有服务都配置了以下环境变量：

```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:{port}
```

服务会根据依赖关系自动注入连接字符串。

## 🗄️ 数据库架构

### 数据库列表

| 数据库名称 | 用途 | 服务 |
|-----------|------|-----|
| clawflgma_auth | 认证数据 | Auth Service |
| clawflgma_user | 用户数据 | User Service |
| clawflgma_project | 项目数据 | Project Service |
| clawflgma_asset | 资源数据 | Asset Service |
| clawflgma_notification | 通知数据 | Notification Service |
| clawflgma_design | 设计数据 | Design Service (MongoDB) |
| clawflgma_collab | 协作数据 | Collaboration Service (MongoDB) |
| clawflgma_ai | AI 数据 | AI Service (MongoDB) |

### 初始化脚本

完整的数据库初始化脚本位于：
- `src/AppHost/init-databases.sql`

该脚本会：
1. 创建所有数据库
2. 创建所有表结构
3. 创建索引
4. 插入测试数据

## 🧪 测试

### 健康检查

所有服务都配置了健康检查端点：

```bash
# 检查各个服务
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
curl http://localhost:5005/health
curl http://localhost:5006/health
curl http://localhost:5007/health
curl http://localhost:5008/health
curl http://localhost:8000/health
```

### 端到端测试

```bash
# 1. 注册用户
curl -X POST http://localhost:8000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123456","displayName":"Test User"}'

# 2. 登录
curl -X POST http://localhost:8000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123456"}'

# 3. 使用令牌访问受保护资源
curl -X GET http://localhost:8000/api/auth/me \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## 🐛 故障排查

### 问题 1: 端口被占用

**错误**：`System.IO.IOException: Failed to bind to address`

**解决方案**：
```bash
# 检查端口占用
netstat -ano | findstr :5001
netstat -ano | findstr :8000

# 停止占用端口的进程
taskkill /PID <pid> /F
```

### 问题 2: 数据库连接失败

**错误**：`Npgsql.PostgresException: could not connect to server`

**解决方案**：
```bash
# 检查 PostgreSQL 是否运行
docker ps | grep postgres

# 查看 PostgreSQL 日志
docker logs clawflgma-postgres

# 检查数据库是否创建
docker exec -it clawflgma-postgres psql -U clawflgma -l
```

### 问题 3: 服务启动失败

**错误**：`System.InvalidOperationException: Unable to resolve service`

**解决方案**：
```bash
# 检查 AppHost 日志
cd src/AppHost
dotnet run

# 查看详细错误
dotnet run --verbose
```

### 问题 4: OpenTelemetry 导出失败

**错误**：`OpenTelemetry.ExportException: Failed to export metrics`

**解决方案**：
```bash
# 确认 Prometheus 正在运行
curl http://localhost:9090/api/v1/targets

# 检查 Prometheus 配置
cat src/AppHost/prometheus.yml
```

## 📝 配置文件

| 文件 | 说明 |
|------|------|
| `src/AppHost/Program.cs` | 主配置文件 |
| `src/AppHost/AppHost.csproj` | 项目文件 |
| `src/AppHost/prometheus.yml` | Prometheus 配置 |
| `src/AppHost/alert_rules.yml` | 告警规则 |
| `src/AppHost/init-databases.sql` | 数据库初始化 |

## 🎯 下一步

1. **完善服务实现**
   - 添加业务逻辑到各服务
   - 实现服务间通信
   - 添加更多 API 端点

2. **配置监控**
   - 创建 Grafana 仪表板
   - 配置告警通知
   - 设置日志聚合

3. **安全加固**
   - 配置 TLS/HTTPS
   - 设置服务间认证
   - 实现访问控制

4. **性能优化**
   - 配置资源限制
   - 优化数据库查询
   - 实现缓存策略

5. **开发前端**
   - 集成 API Gateway
   - 实现认证流程
   - 构建用户界面

## 📚 相关文档

- [.NET Aspire 文档](https://learn.microsoft.com/dotnet/aspire/)
- [架构设计文档](docs/architecture/microservices-design.md)
- [AuthService 测试指南](AUTH_TEST_GUIDE.md)
- [部署指南](DEPLOYMENT.md)

---

**最后更新**: 2026-03-20  
**状态**: ✅ 完整配置完成
