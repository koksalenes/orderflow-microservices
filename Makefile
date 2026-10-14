SOLUTION := OrderFlow.slnx
COMPOSE  := docker compose --profile infra
COMPOSE_ALL := docker compose --profile infra --profile apps

.DEFAULT_GOAL := help

.PHONY: help env infra-up infra-down infra-reset infra-ps infra-logs up down ps logs images build test format

help: ## Show available targets
	@grep -E '^[a-zA-Z_-]+:.*?## ' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  %-12s %s\n", $$1, $$2}'

env: ## Create .env from .env.example if it is missing or empty
	@test -s .env || (cp .env.example .env && echo "Created .env from .env.example; change the passwords.")

infra-up: env ## Start the local infrastructure and wait until it is healthy
	$(COMPOSE) up -d --wait

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
