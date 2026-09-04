# =============================================================================
# Setup Azure para FIAP.Agro - Fase 5 (AKS)
# =============================================================================
# Roda ESTE ARQUIVO no PowerShell, uma seção por vez, depois de:
#   1) az login
#   2) az account show -o table  (confirmar que está na subscription certa)
#
# Cada seção é uma etapa. Rode uma por vez.
# =============================================================================

# --- Variaveis (ajuste se quiser mudar os nomes) ---
$RG       = "rg-fiap-agro"
$LOCATION = "brazilsouth"
$ACR      = "acrfiapagro$(Get-Random -Minimum 10000 -Maximum 99999)"
$AKS      = "aks-fiap-agro"

Write-Host "Sera usado:" -ForegroundColor Cyan
Write-Host "  RG       = $RG"
Write-Host "  LOCATION = $LOCATION"
Write-Host "  ACR      = $ACR"
Write-Host "  AKS      = $AKS"
Write-Host ""

# =============================================================================
# ETAPA 1 - Criar Resource Group
# =============================================================================
Write-Host "=== ETAPA 1: Criando Resource Group ===" -ForegroundColor Yellow
az group create -n $RG -l $LOCATION -o table

# =============================================================================
# ETAPA 2 - Criar ACR (Azure Container Registry)
# =============================================================================
Write-Host ""
Write-Host "=== ETAPA 2: Criando ACR (Basic SKU) ===" -ForegroundColor Yellow
az acr create -n $ACR -g $RG --sku Basic -o table

# =============================================================================
# ETAPA 3 - Criar AKS (Kubernetes cluster)
# Demora ~5-10 minutos!
# =============================================================================
Write-Host ""
Write-Host "=== ETAPA 3: Criando AKS (~5-10 min) ===" -ForegroundColor Yellow
Write-Host "Aguarde... AKS demora pra criar."
az aks create `
    -n $AKS `
    -g $RG `
    --node-count 1 `
    --node-vm-size Standard_D2s_v6 `
    --tier free `
    --generate-ssh-keys `
    --attach-acr $ACR `
    --enable-managed-identity `
    -o table

# =============================================================================
# ETAPA 4 - Configurar kubectl local
# =============================================================================
Write-Host ""
Write-Host "=== ETAPA 4: Configurando kubectl ===" -ForegroundColor Yellow
az aks get-credentials -n $AKS -g $RG --overwrite-existing
kubectl get nodes

# =============================================================================
# ETAPA 5 - Criar Service Principal para GitHub Actions
# =============================================================================
Write-Host ""
Write-Host "=== ETAPA 5: Criando Service Principal ===" -ForegroundColor Yellow
$SUB_ID = az account show --query id -o tsv
Write-Host "Subscription ID: $SUB_ID"

$SP_JSON = az ad sp create-for-rbac `
    --name "sp-fiap-agro-github" `
    --role Contributor `
    --scopes "/subscriptions/$SUB_ID/resourceGroups/$RG" `
    --sdk-auth

Write-Host ""
Write-Host "=============================================================" -ForegroundColor Green
Write-Host "COPIE o JSON abaixo (INTEIRO, das chaves { até } inclusive)" -ForegroundColor Green
Write-Host "Este JSON sera o valor do secret AZURE_CREDENTIALS no GitHub:" -ForegroundColor Green
Write-Host "=============================================================" -ForegroundColor Green
Write-Host $SP_JSON

# =============================================================================
# ETAPA 6 - Exibir valores para os outros secrets GitHub
# =============================================================================
Write-Host ""
Write-Host "=============================================================" -ForegroundColor Green
Write-Host "OUTROS SECRETS que voce precisa criar no GitHub:" -ForegroundColor Green
Write-Host "=============================================================" -ForegroundColor Green
Write-Host "  ACR_NAME         = $ACR"
Write-Host "  RESOURCE_GROUP   = $RG"
Write-Host "  AKS_CLUSTER_NAME = $AKS"
Write-Host ""
Write-Host "Va em: https://github.com/MarceloDev1/FIAP-Agro/settings/secrets/actions"
Write-Host "e cadastre esses 4 secrets."
