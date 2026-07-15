#!/usr/bin/env bash
# Deploy the AutoTech interview SLM to an Azure VM.
#
# Prerequisites: Azure CLI (az) logged in with `az login`.
#
# Usage:
#   ./azure/deploy.sh [resource-group] [vm-name] [location]
#
# Creates a Standard_D4s_v5 (4 vCPU / 16 GiB) Ubuntu 22.04 VM. The
# cloud-init script clones this repo, trains the model on the VM,
# runs the 30 tokens/sec benchmark gate, and starts the API as a
# systemd service on port 8000.

set -euo pipefail

RESOURCE_GROUP="${1:-autotech-slm-rg}"
VM_NAME="${2:-autotech-slm-vm}"
LOCATION="${3:-eastus}"
VM_SIZE="Standard_D4s_v5"
ADMIN_USER="azureuser"
REPO_URL="${REPO_URL:-$(git config --get remote.origin.url | sed 's#git@github.com:#https://github.com/#')}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "==> Resource group: $RESOURCE_GROUP ($LOCATION)"
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none

echo "==> Rendering cloud-init with repo URL: $REPO_URL"
CLOUD_INIT="$(mktemp)"
sed "s#__REPO_URL__#${REPO_URL}#g" "$SCRIPT_DIR/cloud-init.yaml" > "$CLOUD_INIT"

echo "==> Creating VM: $VM_NAME ($VM_SIZE)"
az vm create \
  --resource-group "$RESOURCE_GROUP" \
  --name "$VM_NAME" \
  --image Ubuntu2204 \
  --size "$VM_SIZE" \
  --admin-username "$ADMIN_USER" \
  --generate-ssh-keys \
  --custom-data "$CLOUD_INIT" \
  --public-ip-sku Standard \
  --output none

echo "==> Opening port 8000 for the API"
az vm open-port --resource-group "$RESOURCE_GROUP" --name "$VM_NAME" --port 8000 --output none

PUBLIC_IP="$(az vm show -d --resource-group "$RESOURCE_GROUP" --name "$VM_NAME" \
  --query publicIps --output tsv)"

rm -f "$CLOUD_INIT"

cat <<EOF

Deployment started. Cloud-init will install dependencies, train the
model, run the throughput benchmark, and start the API service.
This takes roughly 10-15 minutes after the VM boots.

  VM public IP:  $PUBLIC_IP
  SSH:           ssh $ADMIN_USER@$PUBLIC_IP
  Watch setup:   ssh $ADMIN_USER@$PUBLIC_IP 'sudo tail -f /var/log/cloud-init-output.log'
  Benchmark log: ssh $ADMIN_USER@$PUBLIC_IP 'cat /var/log/slm-benchmark.log'

Test the API once setup completes:

  curl http://$PUBLIC_IP:8000/health
  curl -X POST http://$PUBLIC_IP:8000/generate \\
    -H 'Content-Type: application/json' \\
    -d '{"question": "How do you diagnose a parasitic battery drain?"}'

Tear down when finished:

  az group delete --name $RESOURCE_GROUP --yes --no-wait
EOF
