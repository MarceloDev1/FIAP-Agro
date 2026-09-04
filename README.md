# FIAP.Agro - Fase 5 (Kubernetes/AKS)

Solução de microsserviços em .NET 8 implantada em Azure Kubernetes Service (AKS)
com pipeline CI/CD via GitHub Actions.

## Arquitetura

5 microsserviços:

| Serviço | Tipo | Porta | Descrição |
|---|---|---|---|
| `identity-api` | Web API | 8080 | Autenticação e JWT |
| `property-api` | Web API | 8080 | Cadastro de propriedades |
| `ingestion-api` | Web API | 8080 | Recebe leituras de sensores → publica no RabbitMQ |
| `alert-api` | Web API | 8080 | Consulta alertas gerados |
| `alert-worker` | Background service | - | Consome fila RabbitMQ e persiste alertas |

Infraestrutura:

- **SQL Server 2022** (com PVC de 5Gi)
- **RabbitMQ 3 Management**
- **Namespace** `fiap-agro`
- **Secret** `jwt-secret` (chaves JWT compartilhadas)
- **Secret** `sqlserver-secret` (senha SA)

## Fluxo do Dockerfile

Cada microsserviço tem seu próprio Dockerfile em multi-stage build:

```dockerfile
# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish FIAP.Agro.Identity/FIAP.Agro.Identity.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "FIAP.Agro.Identity.dll"]
```

- Etapa `build` usa a imagem `sdk:8.0` (mais pesada, ~800MB) só pra compilar
- Etapa `runtime` usa `aspnet:8.0` (~200MB), copia só o `publish/`
- Resultado final: imagem de ~100MB (verificado com `docker images`)

## Pipeline CI/CD

Arquivo: [.github/workflows/azure-kubernetes-service.yml](.github/workflows/azure-kubernetes-service.yml)

Trigger: `push` em `main` ou `workflow_dispatch` manual.

Etapas:

1. **Checkout** do código
2. **Azure login** via Service Principal
3. **Build de 5 imagens no ACR** (`az acr build`) — build acontece na Azure, não no runner
4. **Autenticação no AKS** via kubelogin
5. **Dry-run validation** de todos os manifests
6. **Deploy via `Azure/k8s-deploy@v4`** — faz substitute do nome da imagem e aplica no cluster
7. **Rollout status** — aguarda cada deployment ficar Ready

## Pré-requisitos para deploy

### Recursos na Azure

```bash
RG=rg-fiap-agro
LOCATION=brazilsouth
ACR=acrfiapagro$RANDOM   # nome único
AKS=aks-fiap-agro

# Resource Group
az group create -n $RG -l $LOCATION

# ACR (SKU Basic - mais barato)
az acr create -n $ACR -g $RG --sku Basic

# AKS (tier free - control plane gratuito)
az aks create \
  -n $AKS -g $RG \
  --node-count 1 \
  --node-vm-size Standard_B2s \
  --tier free \
  --generate-ssh-keys \
  --attach-acr $ACR

# Salvar contexto kubectl local
az aks get-credentials -n $AKS -g $RG
```

### Service Principal (para o GitHub Actions logar na Azure)

```bash
SUB_ID=$(az account show --query id -o tsv)

az ad sp create-for-rbac \
  --name "sp-fiap-agro-github" \
  --role Contributor \
  --scopes /subscriptions/$SUB_ID/resourceGroups/$RG \
  --sdk-auth
```

O JSON de saída vai virar o secret `AZURE_CREDENTIALS` do GitHub.

### Secrets do GitHub (Settings → Secrets and variables → Actions)

| Nome | Valor |
|---|---|
| `AZURE_CREDENTIALS` | JSON completo retornado pelo `az ad sp create-for-rbac` |
| `ACR_NAME` | Nome do ACR (ex.: `acrfiapagro12345`) |
| `RESOURCE_GROUP` | `rg-fiap-agro` |
| `AKS_CLUSTER_NAME` | `aks-fiap-agro` |

## Deploy

Basta fazer push em `main`:

```bash
git push origin main
```

Acompanhar em: `https://github.com/<usuário>/FIAP-Agro/actions`

## Validação pós-deploy

```bash
# Listar recursos criados
kubectl get all -n fiap-agro

# Ver pods rodando
kubectl get pods -n fiap-agro

# Logs de um serviço
kubectl logs -n fiap-agro deploy/identity-api --tail=50

# Obter IP externo (se algum Service for LoadBalancer)
kubectl get svc -n fiap-agro
```

## Cleanup (para não gastar crédito Azure)

```bash
az group delete -n rg-fiap-agro --yes --no-wait
```

## Rodar local com Kind (opcional)

Requer Docker Desktop + Kind + máquina com pelo menos 16GB de RAM.

```bash
# Criar cluster com port-forward das APIs
kind create cluster --config=kind-config.yaml

# Buildar imagens
./build-and-deploy.sh

# Carregar imagens no cluster
kind load docker-image fiap-agro/identity:1.0 --name fiap-agro
kind load docker-image fiap-agro/property:1.0 --name fiap-agro
kind load docker-image fiap-agro/ingestion:1.0 --name fiap-agro
kind load docker-image fiap-agro/alert-api:1.0 --name fiap-agro
kind load docker-image fiap-agro/alert-worker:1.0 --name fiap-agro

# Aplicar manifests
kubectl apply -f k8s/
```

APIs ficam expostas em:
- `http://localhost:8080` (identity)
- `http://localhost:8081` (property)
- `http://localhost:8082` (ingestion)
- `http://localhost:8083` (alert)
- `http://localhost:15672` (RabbitMQ mgmt, guest/guest)
