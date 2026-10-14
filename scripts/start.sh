#!/usr/bin/env bash
# Runs OrderFlow from the published images on GHCR: no .NET SDK, Node, or image build needed.
#   ./scripts/start.sh            latest images (main branch)
#   ./scripts/start.sh v1.2.0     a release tag
set -euo pipefail
cd "$(dirname "$0")/.."

./scripts/env.sh

set -a
# shellcheck disable=SC1091
. ./.env
set +a

export ORDERFLOW_IMAGE_PREFIX="${ORDERFLOW_RELEASE_PREFIX:-ghcr.io/koksalenes/orderflow-microservices}"
export ORDERFLOW_IMAGE_TAG="${1:-${ORDERFLOW_RELEASE_TAG:-latest}}"

echo "Pulling ${ORDERFLOW_IMAGE_PREFIX}/*:${ORDERFLOW_IMAGE_TAG}"
docker compose --profile infra --profile apps pull
docker compose --profile infra --profile apps up -d --no-build --wait

cat <<MESSAGE

OrderFlow is running (${ORDERFLOW_IMAGE_TAG}):
  Storefront         http://localhost:${WEB_PORT:-3000}
  Emails (Mailpit)   http://localhost:${MAILPIT_UI_PORT:-8025}
  Traces (Aspire)    http://localhost:${ASPIRE_DASHBOARD_PORT:-18888}
  RabbitMQ           http://localhost:${RABBITMQ_MANAGEMENT_PORT:-15672}
  Keycloak admin     http://localhost:${KEYCLOAK_PORT:-8080}
Stop with: docker compose --profile infra --profile apps down
MESSAGE
