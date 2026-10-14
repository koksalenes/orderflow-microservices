SOLUTION := OrderFlow.slnx
COMPOSE  := docker compose --profile infra
COMPOSE_ALL := docker compose --profile infra --profile apps

.DEFAULT_GOAL := help

.PHONY: web-dev web-build theme-dev theme-build keycloak-realm-reset help env infra-up infra-down infra-reset infra-ps infra-logs up down ps logs images build test format

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  %-12s %s\n", $$1, $$2}'

env: ## Create .env and add keys missing from .env.example (random values for *_SECRET); existing values are kept
	@touch .env
	@[ ! -s .env ] || [ -z "$$(tail -c1 .env)" ] || echo >> .env
	@while IFS= read -r line || [ -n "$$line" ]; do \
		case "$$line" in ''|\#*) continue ;; esac; \
		key=$${line%%=*}; \
		if ! grep -q "^$$key=" .env; then \
			case "$$key" in *_SECRET) value=$$(openssl rand -hex 32) ;; *) value=$${line#*=} ;; esac; \
			echo "$$key=$$value" >> .env; \
			echo "Added $$key to .env"; \
		fi; \
	done < .env.example

infra-up: env ## Start the local infrastructure and wait until it is healthy
	$(COMPOSE) up -d --build --wait

infra-down: ## Stop the local infrastructure (data is kept)
	$(COMPOSE) down

infra-reset: ## Stop the infrastructure and DELETE all volumes (databases, queues, cache)
	$(COMPOSE) down -v

infra-ps: ## List infrastructure containers and their health
	$(COMPOSE) ps

infra-logs: ## Follow infrastructure logs (e.g. make infra-logs s=keycloak)
	$(COMPOSE) logs -f $(s)

up: env ## Build the service images and start infrastructure + services + gateway
	$(COMPOSE_ALL) up -d --build --wait

down: ## Stop infrastructure, services, and gateway (data is kept)
	$(COMPOSE_ALL) down

ps: ## List every container and its health
	$(COMPOSE_ALL) ps

logs: ## Follow logs (e.g. make logs s=orders-api)
	$(COMPOSE_ALL) logs -f $(s)

images: ## Build the service images without starting them
	$(COMPOSE_ALL) build

build: ## Build the solution
	dotnet build $(SOLUTION)

test: ## Run all tests
	dotnet test $(SOLUTION)

format: ## Format the code with CSharpier
	dotnet csharpier format .

web-dev: ## Run the React app with hot reload on http://localhost:3000 against a local BFF on :5111 (stops the bff container, which also uses :3000)
	-@$(COMPOSE_ALL) stop bff >/dev/null 2>&1
	cd web/app && npm install && npm run dev

web-build: ## Type-check and build the React app
	cd web/app && npm install && npm run build

theme-dev: ## Preview the login theme with mock data
	cd web/keycloak-theme && npm install && npm run dev

theme-build: ## Build the login theme jar locally
	cd web/keycloak-theme && npm install && npm run build-keycloak-theme

keycloak-realm-reset: ## DELETE the orderflow realm (and its users) and re-import it from deploy/keycloak/import
	$(COMPOSE) exec keycloak bash -c '/opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080 --realm master --user "$$KC_BOOTSTRAP_ADMIN_USERNAME" --password "$$KC_BOOTSTRAP_ADMIN_PASSWORD" && /opt/keycloak/bin/kcadm.sh delete realms/orderflow'
	$(COMPOSE) restart keycloak
