# Implementation Plan - Renault Lead Integration Microservice

Create a new .NET 10 microservice to integrate Renault leads with Salesforce using MongoDB, RabbitMQ, and OpenTelemetry.

## Proposed Changes

### [Component] Project Structure
Setup the solution with a clean separation of concerns, following the patterns found in other Barigui microservices.

#### [NEW] MsRenault.sln
#### [NEW] MsRenault.API
- Entry point for the application.
- Configuration for DI, OpenTelemetry, and Hosted Services.
- `Program.cs`.

#### [NEW] MsRenault.Aplicacao
- Application services and use cases.
- Ingestion logic and transformation.

#### [NEW] MsRenault.Dominio
- Domain models (Renault, Salesforce).
- Interfaces for repositories and services.
- Mapping logic.

#### [NEW] MsRenault.Infra.Dados
- MongoDB implementation and raw storage logic.

#### [NEW] MsRenault.Infra.Mensageria
- RabbitMQ implementation for publishing and consuming leads.

---

### [Component] US01: Authentication
- Implement `RenaultAuthService` using `HttpClient`.
- Cache the `accessToken` and monitor the 90-day expiration.
- Log failures to OpenTelemetry.

### [Component] US02 & US03: Ingestion and Messaging
- `RenaultIngestionWorker` runs periodically.
- Fetch leads from Renault API (handling 250 limit and 2-day range).
- Save raw data to MongoDB.
- Publish each lead to `renault.leads.received` in RabbitMQ.

### [Component] US04: Salesforce Integration
- Consume from `renault.leads.received`.
- Map Renault fields to Salesforce `Lead` object.
- Send to Salesforce API.
- Update MongoDB status.
- Implement DLQ for resilience.

### [Component] Phase 2 & 3: Journey Feedback and Exclusive Leads
- Implement endpoints/workers for `/funnel`, `/prospection`, `/sales`, `/delivery`.
- Implement logic to identified leads born in Salesforce and send to Renault via `/create`.

### [Component] US05: OpenTelemetry (From Scratch)
- Setup `AddOpenTelemetry()` from scratch in `MsRenault.API`.
- Configure Trace and Metric exporters (OTLP).
- Instrument standard libraries: `HttpClient`, `AspNetCore`, `MongoDB.Driver`, and `RabbitMQ`.
- Add custom activity sources for granular tracing.
- Add custom tags (`renault.bir`, `renault.leadReferenceId`) for traceability.

### [Component] Project Documentation
- Create `README.md` with:
    - Architecture overview.
    - Endpoint documentation (internal/external).
    - Request/Response payloads.
    - Setup instructions (MongoDB, RabbitMQ, Environment Variables).

## Verification Plan

### Automated Tests
- **Unit Tests**: Test mappers (Renault -> Salesforce) to ensure field alignment.
- **Integration Tests**: Mock Renault/Salesforce APIs to verify worker flow.

### Manual Verification
1. Start RabbitMQ and MongoDB locally (Docker).
2. Run the application and monitor logs.
3. Check MongoDB for saved raw payloads.
4. Check RabbitMQ Management UI for published messages.
5. Verify OpenTelemetry traces in a compatible backend (e.g., Aspire Dashboard or Jaeger).
