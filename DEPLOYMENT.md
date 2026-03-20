# ClawFlgma 部署指南

## 环境准备

### 开发环境

- .NET 10 SDK
- Docker Desktop
- Git
- Visual Studio 2022 或 VS Code

### 生产环境

- Kubernetes 1.28+
- Helm 3.8+
- 容器镜像仓库 (Docker Hub / Harbor / AWS ECR)
- PostgreSQL 17+
- MongoDB 7+
- Redis 7+

## 本地开发部署

### 1. 启动基础设施

```bash
docker-compose up -d postgres redis rabbitmq mongodb minio
```

### 2. 运行 AppHost

```bash
cd src/AppHost
dotnet run
```

所有微服务将自动启动并连接到基础设施。

### 3. 验证部署

访问以下端点验证服务状态：

- API Gateway: http://localhost:8000
- API Gateway Health: http://localhost:8000/health
- Auth Service: http://localhost:5001/health
- Design Service: http://localhost:5004/health

## Docker Compose 部署

### 完整部署

```bash
docker-compose up -d
```

### 查看日志

```bash
docker-compose logs -f auth-service
docker-compose logs -f design-service
```

### 停止服务

```bash
docker-compose down
```

## Kubernetes 部署

### 前置步骤

1. **准备镜像仓库**
   ```bash
   docker login your-registry.com
   ```

2. **修改镜像地址**
   编辑 `deploy/kubernetes/deployments/*.yaml` 中的镜像地址

### 自动部署

```bash
chmod +x deploy/scripts/deploy-all.sh
./deploy/scripts/deploy-all.sh
```

### 手动部署

#### 1. 创建命名空间

```bash
kubectl apply -f deploy/kubernetes/namespace.yaml
```

#### 2. 配置密钥

```bash
# 编辑 secrets.yaml 中的实际值
vim deploy/kubernetes/secrets.yaml
kubectl apply -f deploy/kubernetes/secrets.yaml
```

#### 3. 部署基础设施

```bash
kubectl apply -f deploy/kubernetes/infrastructure/
```

等待基础设施就绪：

```bash
kubectl wait --for=condition=ready pod -l app=postgres -n clawflgma --timeout=300s
kubectl wait --for=condition=ready pod -l app=mongodb -n clawflgma --timeout=300s
```

#### 4. 部署微服务

```bash
kubectl apply -f deploy/kubernetes/deployments/
```

#### 5. 配置 Ingress

```bash
kubectl apply -f deploy/kubernetes/ingress.yaml
```

#### 6. 验证部署

```bash
# 查看所有 Pod
kubectl get pods -n clawflgma

# 查看服务
kubectl get svc -n clawflgma

# 查看日志
kubectl logs -f deployment/auth-service -n clawflgma
```

### 扩容服务

```bash
kubectl scale deployment auth-service --replicas=5 -n clawflgma
```

### 滚动更新

```bash
# 更新镜像
kubectl set image deployment/auth-service \
  auth-service=your-registry.com/clawflgma/auth-service:v1.1.0 \
  -n clawflgma

# 查看滚动更新状态
kubectl rollout status deployment/auth-service -n clawflgma

# 回滚
kubectl rollout undo deployment/auth-service -n clawflgma
```

## 监控配置

### Prometheus

```bash
kubectl apply -f deploy/kubernetes/monitoring/prometheus.yaml
```

访问 Prometheus: http://prometheus.clawflgma.com

### Grafana

```bash
kubectl apply -f deploy/kubernetes/monitoring/grafana.yaml
```

访问 Grafana: http://grafana.clawflgma.com (admin/admin)

### 查看监控数据

```bash
# 查看 Prometheus targets
kubectl port-forward -n clawflgma svc/prometheus 9090:9090

# 查看 Grafana
kubectl port-forward -n clawflgma svc/grafana 3000:3000
```

## 数据库初始化

### PostgreSQL

```bash
# 连接到 PostgreSQL
kubectl exec -it -n clawflgma postgres-0 -- psql -U clawflgma -d clawflgma

# 运行迁移脚本
\i /docker-entrypoint-initdb.d/001_init.sql
```

### MongoDB

```bash
# 连接到 MongoDB
kubectl exec -it -n clawflgma mongodb-0 -- mongosh -u clawflgma -p clawflgma_pass --authenticationDatabase admin

# 创建索引
use clawflgma
db.design_documents.createIndex({ "projectId": 1, "version": -1 })
```

## 故障排查

### Pod 无法启动

```bash
# 查看 Pod 状态
kubectl describe pod <pod-name> -n clawflgma

# 查看日志
kubectl logs <pod-name> -n clawflgma

# 查看事件
kubectl get events -n clawflgma --sort-by=.metadata.creationTimestamp
```

### 连接数据库失败

检查密钥配置：

```bash
kubectl get secret clawflgma-secrets -n clawflgma -o yaml
```

验证数据库连接：

```bash
kubectl exec -it -n clawflgma <pod-name> -- curl http://postgres:5432
```

### 资源不足

调整资源限制：

```yaml
resources:
  requests:
    memory: "512Mi"
    cpu: "250m"
  limits:
    memory: "1Gi"
    cpu: "500m"
```

## 安全加固

### 1. 使用 Secret 管理密钥

不要将敏感信息硬编码在代码中，使用 Kubernetes Secrets：

```bash
kubectl create secret generic api-keys \
  --from-literal=jwt-secret-key='your-secret-key' \
  -n clawflgma
```

### 2. 启用 RBAC

```bash
kubectl apply -f deploy/kubernetes/security/rbac.yaml
```

### 3. 配置 NetworkPolicy

```bash
kubectl apply -f deploy/kubernetes/security/network-policy.yaml
```

### 4. 启用 Pod Security Policy

```bash
kubectl apply -f deploy/kubernetes/security/psp.yaml
```

## 备份与恢复

### 数据库备份

```bash
# PostgreSQL
kubectl exec -n clawflgma postgres-0 -- pg_dump -U clawflgma clawflgma > backup.sql

# MongoDB
kubectl exec -n clawflgma mongodb-0 -- mongodump --archive=/data/backup.archive
kubectl cp clawflgma/mongodb-0:/data/backup.archive ./backup.archive
```

### 恢复数据

```bash
# PostgreSQL
kubectl exec -i -n clawflgma postgres-0 -- psql -U clawflgma clawflgma < backup.sql

# MongoDB
kubectl cp ./backup.archive clawflgma/mongodb-0:/data/restore.archive
kubectl exec -n clawflgma mongodb-0 -- mongorestore --archive=/data/restore.archive
```

## 性能优化

### 1. 调整 HPA 参数

```yaml
autoscaling/v2:
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
  targetMemoryUtilizationPercentage: 80
```

### 2. 配置资源限制

根据实际负载调整 CPU 和内存限制。

### 3. 启用缓存

确保 Redis 正确配置并使用。

## 常见问题

### Q: 如何查看服务日志？

```bash
kubectl logs -f deployment/<service-name> -n clawflgma
```

### Q: 如何进入 Pod 调试？

```bash
kubectl exec -it <pod-name> -n clawflgma -- /bin/bash
```

### Q: 如何重启服务？

```bash
kubectl rollout restart deployment/<service-name> -n clawflgma
```

### Q: 如何查看资源使用情况？

```bash
kubectl top pods -n clawflgma
kubectl top nodes
```

## 更多信息

- [架构设计文档](docs/architecture/microservices-design.md)
- [API 文档](docs/architecture/api-specification.md)
- [.NET Aspire 文档](https://learn.microsoft.com/dotnet/aspire/)
- [Kubernetes 文档](https://kubernetes.io/docs/)
