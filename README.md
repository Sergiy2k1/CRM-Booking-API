# CRM Booking API

Production-oriented multi-tenant CRM and booking platform built with ASP.NET Core and .NET 10.

## Current status

The repository currently contains the base solution architecture only. Business features will be implemented incrementally.

## Architecture

The solution starts as a Clean Architecture-inspired modular monolith with a separate worker process:

- `BookingHub.Domain` — domain model and business invariants.
- `BookingHub.Application` — use cases and abstractions.
- `BookingHub.Infrastructure` — infrastructure implementations.
- `BookingHub.Api` — HTTP API and composition root.
- `BookingHub.Worker` — background processing host.

Tests are split into domain unit, application unit, infrastructure integration, API integration, and architecture test projects.

The next development step is the domain model and ER diagram.
