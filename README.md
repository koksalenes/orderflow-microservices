<div align="center">

# OrderFlow

**An event-driven order fulfillment platform built with .NET 10 microservices**

[![.NET](https://img.shields.io/badge/.NET-10.0_LTS-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-18-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-4.1-FF6600?logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com/)
[![Redis](https://img.shields.io/badge/Redis-7.2-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![Keycloak](https://img.shields.io/badge/Keycloak-26-4D4D4D?logo=keycloak&logoColor=white)](https://www.keycloak.org/)
[![YARP](https://img.shields.io/badge/YARP-2.3-512BD4?logo=dotnet&logoColor=white)](https://dotnet.github.io/yarp/)
[![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-OTLP-425CC7?logo=opentelemetry&logoColor=white)](https://opentelemetry.io/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://docs.docker.com/compose/)

</div>

---

OrderFlow is a **case project** that models how an online order moves through a distributed system. A customer places an order; stock is reserved, the payment is taken from a wallet, and the customer is notified in real time. If any step fails, earlier steps are compensated automatically.

Each step is owned by an independent service with its own database. The services cooperate through **asynchronous messaging** for state changes and **gRPC** for the few reads that must happen synchronously.

> **Status:** the platform skeleton is in place: services, gateway, identity provider, messaging and storage infrastructure, observability, and containerization. Business features are implemented next, one service at a time.

## Architecture

```mermaid
flowchart LR
    client([Client]) --> gateway[YARP Gateway<br/>routing · rate limiting]
    gateway --> products[Products API]
    gateway --> inventory[Inventory API]
    gateway --> orders[Orders API]
    gateway --> payments[Payments API]
    gateway --> notifications[Notifications API<br/>SignalR]

    orders -. gRPC .-> products
    orders -. gRPC .-> inventory

    subgraph messaging [RabbitMQ]
        bus{{events & commands}}
    end

    products & inventory & orders & payments & notifications <--> bus

    keycloak[(Keycloak<br/>OIDC / JWT)] -.-> gateway
    aspire[[Aspire Dashboard<br/>traces · metrics · logs]]
```

Every service runs as two containers: an **API** for HTTP traffic and a **Worker** for background processing (outbox publishing, message consumers, saga steps). Both share the same application and infrastructure code; only the host differs.

## Services

| Service | Responsibility | Highlights |
|---|---|---|
| **Products** | Product catalog and prices | Read-heavy, cached in Redis; serves prices over gRPC |
| **Inventory** | Stock levels and reservations | Optimistic concurrency to prevent overselling |
| **Orders** | Order lifecycle | **Saga orchestrator** for the fulfillment flow, idempotent order placement |
| **Payments** | Wallet balance, fund reservations, refunds | **Event-sourced** ledger with snapshots and projections |
| **Notifications** | Email and live updates | Real-time push with **SignalR**; email via SMTP |

Each service owns a separate PostgreSQL database. No service reads another service's data.

## Order fulfillment flow

```mermaid
sequenceDiagram
    participant C as Client
    participant O as Orders
    participant I as Inventory
    participant P as Payments
    participant N as Notifications

    C->>O: Place order
    O->>I: ReserveStock
    I-->>O: StockReserved
    O->>P: ReserveFunds
    P-->>O: FundsReserved
    O-)I: OrderConfirmed
    O-)P: OrderConfirmed
    O-)N: OrderConfirmed
    N-->>C: "Your order is confirmed" (SignalR)

    Note over O,P: On failure or timeout, the saga releases<br/>reserved stock and funds (compensation)
```

## Key patterns

| Pattern | Purpose |
|---|---|
| **Clean Architecture** | Domain, Application, Infrastructure, and host layers per service, with dependencies pointing inward |
| **CQRS** | EF Core for the write side, Dapper for reads; handlers injected directly, no mediator |
| **Transactional Outbox & Inbox** | Messages are stored in the same transaction as the state change and consumed idempotently |
| **Saga (orchestration + choreography)** | Orders coordinates the decision phase; the outcome event fans out to every service |
| **Event Sourcing** | Payments stores every money movement as an event; balances are projections |
| **Database per service** | Services evolve and scale independently |
| **API Gateway** | YARP-based single entry point with routing, rate limiting, and health-aware load balancing |
| **Distributed tracing** | One trace id from the gateway through every service, returned as `X-Request-Id` |
| **RFC 9457 ProblemDetails** | Every error has the same shape with a stable error `code` |

## Tech stack

| Area | Technology |
|---|---|
| Runtime | .NET 10 (LTS), C# 14, Minimal APIs |
| Persistence | PostgreSQL 18, EF Core 10, Dapper |
| Messaging | RabbitMQ 4.1 |
| Caching | Redis 7.2 |
| Identity | Keycloak 26 (OIDC, JWT) |
| Gateway | YARP 2.3 (ASP.NET Core reverse proxy) |
| Observability | Serilog, OpenTelemetry, Aspire Dashboard |
| Containers | Docker, chiseled .NET runtime images (non-root, no shell) |
| Quality | Roslyn + SonarAnalyzer (warnings as errors), CSharpier, Husky.Net git hooks |

## Getting started

**Requirements:** .NET SDK 10.0.401, Docker with Compose v2, `make`.

```bash
make env    # creates .env from .env.example — change the passwords before the first start
make up     # builds the service images and starts everything; waits until all containers are healthy
make ps     # lists containers and their health
```

| Endpoint | URL |
|---|---|
| Gateway (all APIs under `/api/v1/<resource>`) | http://localhost:8000 |
| Aspire Dashboard (traces, metrics, logs) | http://localhost:18888 |
| RabbitMQ management | http://localhost:15672 |
| Keycloak admin console | http://localhost:8080 |
| Mailpit (captured emails) | http://localhost:8025 |

### Everyday commands

| Command | What it does |
|---|---|
| `make up` | Build images and start infrastructure, services, and gateway |
| `make down` | Stop and remove the containers (data volumes are kept) |
| `make logs s=orders-api` | Follow the logs of one container |
| `make infra-up` | Start only the infrastructure, to run a service from the IDE |
| `make infra-reset` | Remove the containers **and all data volumes** |
| `make build` / `make test` | Build the solution / run the tests |
| `make` | List every target |

Run a single service from the IDE with `make infra-up` and its `http` launch profile (Products API on port 5101, then sequential ports per host). Set its connection string first:

```bash
dotnet user-secrets set "Database:ConnectionString" \
  "Host=localhost;Port=5432;Database=orders_db;Username=<user>;Password=<password>" \
  --project src/Services/Orders/OrderFlow.Orders.Api
```

## Repository layout

```
src/
├── Shared/                  Building blocks shared by every service (domain primitives, host defaults, web defaults)
├── Gateway/                 YARP API gateway
└── Services/
    └── <Service>/           Products · Inventory · Orders · Payments · Notifications
        ├── *.Contracts      Events, commands, and gRPC contracts the service exposes
        ├── *.Domain
        ├── *.Application
        ├── *.Infrastructure
        ├── *.Api
        └── *.Worker
deploy/                      Dockerfile and Keycloak configuration
scripts/                     Database initialization
```
