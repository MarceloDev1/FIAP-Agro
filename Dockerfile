- name: Build and push image to ACR
  run: |
    az acr build \
      --image ${{ env.AZURE_CONTAINER_REGISTRY }}.azurecr.io/${{ env.CONTAINER_NAME }}:${{ github.sha }} \
      --registry ${{ env.AZURE_CONTAINER_REGISTRY }} \
      --resource-group ${{ env.RESOURCE_GROUP }} \
      --file ./FIAP.Agro/Dockerfile \
      ./FIAP.Agro