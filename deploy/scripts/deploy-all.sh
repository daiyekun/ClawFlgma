#!/bin/bash

# ClawFlgma Kubernetes Deployment Script

set -e

echo "🚀 Starting ClawFlgma deployment..."

# Variables
NAMESPACE="clawflgma"
REGISTRY="your-registry.com"  # Change to your registry

# Create namespace
echo "📦 Creating namespace: $NAMESPACE"
kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

# Apply secrets (update with actual secrets)
echo "🔐 Applying secrets..."
kubectl apply -f deploy/kubernetes/secrets.yaml

# Apply configmaps
echo "⚙️ Applying configmaps..."
kubectl apply -f deploy/kubernetes/configmaps.yaml

# Build and push images (uncomment when ready)
# echo "🏗️ Building and pushing images..."
# for service in auth user project design collab asset notification ai gateway; do
#   docker build -f src/Services/${service}Service/Dockerfile -t $REGISTRY/clawflgma/${service}-service:latest .
#   docker push $REGISTRY/clawflgma/${service}-service:latest
# done

# Deploy infrastructure
echo "🌐 Deploying infrastructure services..."
kubectl apply -f deploy/kubernetes/infrastructure/postgres.yaml
kubectl apply -f deploy/kubernetes/infrastructure/redis.yaml
kubectl apply -f deploy/kubernetes/infrastructure/rabbitmq.yaml
kubectl apply -f deploy/kubernetes/infrastructure/mongodb.yaml
kubectl apply -f deploy/kubernetes/infrastructure/minio.yaml

# Wait for infrastructure to be ready
echo "⏳ Waiting for infrastructure to be ready..."
kubectl wait --for=condition=ready pod -l app=postgres -n $NAMESPACE --timeout=300s
kubectl wait --for=condition=ready pod -l app=redis -n $NAMESPACE --timeout=300s
kubectl wait --for=condition=ready pod -l app=mongodb -n $NAMESPACE --timeout=300s

# Deploy microservices
echo "🔧 Deploying microservices..."
kubectl apply -f deploy/kubernetes/deployments/auth-service.yaml
kubectl apply -f deploy/kubernetes/deployments/user-service.yaml
kubectl apply -f deploy/kubernetes/deployments/project-service.yaml
kubectl apply -f deploy/kubernetes/deployments/design-service.yaml
kubectl apply -f deploy/kubernetes/deployments/collab-service.yaml
kubectl apply -f deploy/kubernetes/deployments/asset-service.yaml
kubectl apply -f deploy/kubernetes/deployments/notification-service.yaml
kubectl apply -f deploy/kubernetes/deployments/ai-service.yaml
kubectl apply -f deploy/kubernetes/deployments/api-gateway.yaml

# Deploy monitoring
echo "📊 Deploying monitoring stack..."
kubectl apply -f deploy/kubernetes/monitoring/prometheus.yaml
kubectl apply -f deploy/kubernetes/monitoring/grafana.yaml

# Deploy ingress
echo "🌍 Configuring ingress..."
kubectl apply -f deploy/kubernetes/ingress.yaml

# Wait for all pods to be ready
echo "⏳ Waiting for all pods to be ready..."
kubectl wait --for=condition=ready pod -l app -n $NAMESPACE --timeout=600s

echo "✅ Deployment completed successfully!"
echo ""
echo "📋 Service URLs:"
kubectl get svc -n $NAMESPACE
echo ""
echo "🌐 Ingress endpoints:"
kubectl get ingress -n $NAMESPACE
