# BookingHub — CRM Booking API

Production-oriented multi-tenant CRM and booking backend built with ASP.NET Core and .NET 10.

> Current implementation progress: approximately **99%** of the planned backend architecture and core platform scope.

## What is already implemented

The project currently includes:

- multi-tenant organization domain model;
- users and organization memberships;
- organization roles and membership lifecycle;
- customers;
- employees;
- services and employee-service assignments;
- employee working hours and time-off;
- booking aggregate and booking lifecycle;
- booking conflict detection;
- availability engine;
- PostgreSQL persistence with Entity Framework Core;
- PostgreSQL migrations;
- database-level double-booking protection with a GiST exclusion constraint;
- complete booking lifecycle application use cases;
- booking create/get/list/confirm/reschedule/cancel/complete/no-show HTTP endpoints;
- centralized Problem Details error handling;
- password hashing;
- JWT access token generation and validation;
- tenant-aware JWT claims;
- login use case and `/api/auth/login` endpoint;
- opaque refresh tokens with SHA-256 hashing and rotation;
- `/api/auth/refresh` session renewal endpoint;
- tenant-aware authorization policy for organization routes;
- protected booking endpoint with `401`/`403` tenant enforcement;
- role-based booking management policy for Owner/Admin/Manager/Receptionist;
- employee management API with tenant-safe read/write flows;
- employee working-hours and time-off management;
- employee schedule role policies and overlap protection;
- service management API with tenant-safe CRUD and lifecycle;
- employee-service assignment management with duplicate protection;
- Transactional Outbox stored atomically with booking changes;
- RabbitMQ topic exchange publishing from the Worker;
- publisher confirms before outbox messages are marked processed;
- retry tracking for failed outbox messages;
- authenticated SignalR booking hub;
- RabbitMQ-to-SignalR realtime booking event bridge;
- organization-scoped SignalR groups;
- PostgreSQL Inbox deduplication for RabbitMQ realtime events;
- persistent per-user booking notifications;
- idempotent RabbitMQ notification consumer;
- notification read/unread API;
- Dapper booking summary reporting read model;
- currency-safe completed-revenue aggregation;
- asynchronous booking CSV export jobs;
- concurrent-safe export claiming with PostgreSQL `FOR UPDATE SKIP LOCKED`;
- retry/reclaim handling for stale export jobs;
- shared file-storage abstraction with local Docker volume implementation;
- Dockerfiles for API and Worker;
- Docker Compose local stack for PostgreSQL, RabbitMQ, API and Worker;
- GitHub Actions CI for restore, build, tests, compose validation and container builds;
- unit tests, API integration tests and PostgreSQL integration tests with Testcontainers.

## Architecture

The solution follows Clean Architecture principles and starts as a modular monolith with a separate worker process.

```text
BookingHub.Api
    ↓
BookingHub.Application
    ↓
BookingHub.Domain

BookingHub.Infrastructure
    ↑ implements Application abstractions

BookingHub.Worker
    → background processing host
```

Projects:

- `BookingHub.Domain` — entities, aggregates, invariants and domain services.
- `BookingHub.Application` — use cases, orchestration and abstractions.
- `BookingHub.Infrastructure` — EF Core, PostgreSQL, repositories, JWT and other external implementations.
- `BookingHub.Api` — HTTP API, authentication pipeline and composition root.
- `BookingHub.Worker` — background processing host.
- `BookingHub.Domain.UnitTests` — domain tests.
- `BookingHub.Application.UnitTests` — application use-case tests.
- `BookingHub.Infrastructure.IntegrationTests` — real PostgreSQL integration tests.
- `BookingHub.Api.IntegrationTests` — ASP.NET Core HTTP integration tests.
- `BookingHub.ArchitectureTests` — architecture rules.

## Booking model

A booking contains:

- organization;
- customer;
- employee;
- service;
- UTC start/end interval;
- service price snapshot;
- currency;
- notes;
- lifecycle status.

Current statuses:

```text
Pending
  ├─ Confirmed
  │    ├─ Completed
  │    ├─ NoShow
  │    └─ Cancelled
  └─ Cancelled
```

## Availability

Availability is calculated from:

```text
Working hours
- Time off
- Existing Pending/Confirmed bookings
= Available slot
```

Working hours use local organization time.

Concrete bookings and time-off intervals are stored as UTC timestamps.

Time intervals use half-open semantics:

```text
[start, end)
```

Therefore `10:00–11:00` does not conflict with `11:00–12:00`.

## Double-booking protection

Double booking is prevented at two levels.

### Application/domain check

The availability engine detects overlapping bookings before persistence so the API can return a meaningful business error.

### PostgreSQL constraint

PostgreSQL is the final concurrency guarantee.

The booking table uses a GiST exclusion constraint based on:

```text
OrganizationId
EmployeeId
tstzrange(StartsAtUtc, EndsAtUtc, '[)')
```

Only `Pending` and `Confirmed` bookings block the slot.

This prevents race conditions where two concurrent requests both see the same slot as available.

## Authentication

Login endpoint:

```http
POST /api/auth/login
```

Login requires:

- email;
- password;
- organization ID.

The flow validates:

1. active user;
2. password;
3. active organization membership;
4. active organization;
5. JWT creation.

JWT access tokens currently contain:

- `sub` — user ID;
- `email`;
- `organization_id`;
- role;
- `jti`.

Access-token lifetime is currently configured to 15 minutes.

The signing key in `appsettings.json` is a **development-only placeholder**. Production deployment must supply secrets through environment variables or a secret-management system.

Refresh tokens are opaque random values. Only their SHA-256 hashes are stored in PostgreSQL. Refresh performs token rotation: the previous token is revoked and replaced by a new token.

Organization-scoped booking routes require authentication and verify that the JWT `organization_id` claim matches the `{organizationId}` route value.

## Current API

### Login

```http
POST /api/auth/login
```

### Refresh session

```http
POST /api/auth/refresh
```

### Booking lifecycle

```http
POST /api/organizations/{organizationId}/bookings
GET  /api/organizations/{organizationId}/bookings
GET  /api/organizations/{organizationId}/bookings/{bookingId}
POST /api/organizations/{organizationId}/bookings/{bookingId}/confirm
POST /api/organizations/{organizationId}/bookings/{bookingId}/reschedule
POST /api/organizations/{organizationId}/bookings/{bookingId}/cancel
POST /api/organizations/{organizationId}/bookings/{bookingId}/complete
POST /api/organizations/{organizationId}/bookings/{bookingId}/no-show
```

Requires a valid Bearer access token for the same organization. Booking creation is currently allowed for `Owner`, `Admin`, `Manager`, and `Receptionist`; `Employee` receives `403 Forbidden`.

Booking creation and rescheduling execute tenant ownership, employee/service and availability checks. Rescheduling preserves the booking's original duration and excludes the booking itself from overlap detection.

### Booking CSV exports

```http
POST /api/organizations/{organizationId}/reports/bookings/exports
GET  /api/organizations/{organizationId}/reports/bookings/exports/{exportId}
GET  /api/organizations/{organizationId}/reports/bookings/exports/{exportId}/download
```

Export creation returns `202 Accepted`. The Worker claims pending jobs, reads booking data through Dapper, generates CSV asynchronously and stores the file through `IExportFileStorage`. Export status progresses through `Pending`, `Processing`, `Completed` or `Failed`. Only the requesting user in the same organization can query or download the export.

## PostgreSQL

Default local development connection:

```text
Host=localhost
Port=5432
Database=bookinghub
Username=bookinghub
Password=bookinghub
```

Entity Framework Core is used for domain persistence.

PostgreSQL is the source of truth.

## Running the project

Requirements:

- .NET 10 SDK;
- Docker Desktop for PostgreSQL integration tests;
- PostgreSQL when running the API against a local database.

### Run the full local stack with Docker Compose

```powershell
docker compose up --build
```

Local endpoints:

```text
API:               http://localhost:8080
API health:        http://localhost:8080/health
RabbitMQ UI:       http://localhost:15672
RabbitMQ AMQP:     localhost:5672
PostgreSQL:        localhost:5432
```

RabbitMQ local credentials are `bookinghub` / `bookinghub`.

The Compose API enables `Database__MigrateOnStartup=true`, so EF Core migrations are applied on local container startup. Production deployments should keep this disabled and apply migrations as a separate deployment step.

Stop the stack:

```powershell
docker compose down
```

Remove local database/RabbitMQ volumes too:

```powershell
docker compose down -v
```

### Build without containers

Restore and build:

```powershell
dotnet restore
dotnet build
```

## Tests

Domain tests:

```powershell
dotnet test tests/BookingHub.Domain.UnitTests
```

Application tests:

```powershell
dotnet test tests/BookingHub.Application.UnitTests
```

Infrastructure integration tests:

```powershell
dotnet test tests/BookingHub.Infrastructure.IntegrationTests
```

Docker Desktop must be running for infrastructure integration tests because Testcontainers starts a real PostgreSQL container.

API integration tests:

```powershell
dotnet test tests/BookingHub.Api.IntegrationTests
```

## Technology stack

Implemented now:

- .NET 10;
- ASP.NET Core;
- C#;
- Entity Framework Core;
- PostgreSQL;
- Npgsql;
- JWT Bearer authentication;
- ASP.NET Core password hashing;
- xUnit;
- NSubstitute;
- Testcontainers;
- WebApplicationFactory;
- OpenAPI.

Implemented now:

- RabbitMQ;
- Transactional Outbox;
- SignalR;
- Docker Compose;
- GitHub Actions CI;

Planned as the project grows:

- Redis;
- Quartz.NET;
- Elasticsearch;
- MinIO/S3 abstraction;
- Polly;
- OpenTelemetry;
- Prometheus/Grafana;
- distributed tracing.

## Design principles

- PostgreSQL is the source of truth.
- Tenant isolation is explicit.
- Domain rules stay in the Domain layer.
- Application orchestrates use cases.
- Infrastructure implements external concerns.
- API controllers stay thin.
- Technologies are added only when they solve a concrete problem.
- The system remains a modular monolith until there is a real reason to split services.

## Next development steps

The immediate next work is:

1. add OpenTelemetry metrics and distributed tracing;
2. add production secret/configuration hardening;
3. add deployment-specific health/readiness and operational dashboards;
4. add Redis/search only where justified by measured needs;
5. replace local export storage with MinIO/S3 when deployment requirements justify it.
