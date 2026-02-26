#!/usr/bin/env bash

# Builda todas as imagens Docker do FIAP.Agro e aplica manifests do Kubernetes.
# Uso:
#   ./build-and-deploy.sh                # usa tag 1.0
#   ./build-and-deploy.sh 2.0            # usa tag 2.0

set -e

# Sempre trabalhar a partir da raiz do repositório
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]:-$0}")" && pwd)"
cd "$SCRIPT_DIR"

TAG="${1:-1.0}"

echo "Building images with tag ${TAG}..."

docker build -f FIAP.Agro.Identity/Dockerfile   -t fiap-agro/identity:${TAG}      .
docker build -f FIAP.Agro.Property/Dockerfile   -t fiap-agro/property:${TAG}      .
docker build -f FIAP.Agro.Ingestion/Dockerfile  -t fiap-agro/ingestion:${TAG}     .
docker build -f FIAP.Agro.Alert/Dockerfile      -t fiap-agro/alert-api:${TAG}     .
docker build -f AlertWorker/Dockerfile          -t fiap-agro/alert-worker:${TAG}  .

echo
echo "Docker images built:"
docker images "fiap-agro/*:${TAG}" || true

# Lista de manifests do Kubernetes (pasta k8s/)
MANIFESTS=(
  "k8s/00-namespace.yaml"
  "k8s/01-sqlserver.yaml"
  "k8s/02-rabbitmq.yaml"
  "k8s/03-jwt-secret.yaml"
  "k8s/10-identity.yaml"
  "k8s/11-property.yaml"
  "k8s/12-ingestion.yaml"
  "k8s/13-alertapi.yaml"
  "k8s/14-alertworker.yaml"
)

echo
echo "Applying manifests (se existirem) no cluster Kubernetes..."

for f in "${MANIFESTS[@]}"; do
  if [ -f "$f" ]; then
    echo "  - kubectl apply -f $f"
    kubectl apply -f "$f"
  else
    echo "  - (ignorando, arquivo não encontrado: $f)"
  fi
done

echo
echo "Concluído. Verifique os recursos com:"
echo "  kubectl get all -n fiap-agro"

