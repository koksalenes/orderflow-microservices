#!/usr/bin/env bash
# Runs OrderFlow from the published images on GHCR: no .NET SDK, Node, or image build needed.
#   ./scripts/start.sh            latest images (main branch)
#   ./scripts/start.sh v1.2.0     a release tag
set -euo pipefail
cd "$(dirname "$0")/.."

project="${COMPOSE_PROJECT_NAME:-orderflow}"
if [ ! -f .env ] && docker volume inspect "${project}_postgres-data" >/dev/null 2>&1; then
    cat >&2 <<MESSAGE
A database volume (${project}_postgres-data) already exists on this machine, but there is no .env here.
A new .env would get new random passwords that do not match that database, and Keycloak and the services would fail to connect.

Choose one:
  - Keep the existing data: copy the .env that created it (e.g. from your other OrderFlow folder) into this folder, then run this script again.
  - Start fresh (DELETES all OrderFlow data on this machine):
      docker compose --profile infra --profile apps down -v
      ./scripts/start.sh
MESSAGE
    exit 1
fi

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
