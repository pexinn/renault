# Walkthrough - Renault Lead Integration Microservice

I have completed the implementation of the Renault Lead Integration microservice. The solution is structured into five projects following the Barigui pattern and targets .NET 9 (due to local environment availability).

## Project Structure
- `MsRenault.API`: WebAPI and Workers (Ingestion, Salesforce Consumer, Feedback Consumer).
- `MsRenault.Aplicacao`: Business logic and external API clients.
- `MsRenault.Dominio`: DTOs, entities, constants, and mapping logic.
- `MsRenault.Infra.Dados`: MongoDB and API implementation.
- `MsRenault.Infra.Mensageria`: RabbitMQ implementation with OTel tracing.

## Key Features Implemented

### 1. Authentication (US01)
- `RenaultAuthService` handles OAuth 2.0.
- Automatic token renewal (90-day cycle) with thread-safe locking.
- Tracing of authentication calls.

### 2. Lead Ingestion (US02 & US03)
- `RenaultIngestionWorker` polls the Renault API every 15 minutes (configurable).
- Raw payloads are stored in MongoDB for audit.
- Leads are published to RabbitMQ with OTel context propagation.

### 3. Salesforce Integration (US04)
- `RabbitMqLeadConsumer` processes leads and sends them to Salesforce.
- Mapping logic handles Renault-specific taxonomies.
- Dead Letter Queue (DLQ) implemented for resilience.
- MongoDB records are updated with the Salesforce ID.

### 4. Journey Feedback (Phase 2 & 3 - US06, US07, US08, US09)
- `LeadController` provides a webhook for Salesforce events.
- `RabbitMqFeedbackConsumer` handles:
    - Funnel updates (Status change).
    - Prospection updates.
    - Sales & Delivery updates.
    - Creation of exclusive dealer leads (Phase 3).

### 5. OpenTelemetry from Scratch (US05)
- Custom OTel configuration implemented without external Barigui packages.
- Support for Grpc OTLP exporter.
- Instrumentation for AspNetCore, HttpClient, MongoDB, and RabbitMQ.
- Custom spans for workers and consumers.

## Documentation
- `README.md` created with architecture overview, endpoint documentation, and setup instructions.
- Swagger UI/OpenAPI document available at `/openapi/v1.json` (Development).

## Verification
- **Build**: Successfully built the entire solution (`dotnet build MsRenault.sln`).
- **Structure**: Verified project references and NuGet dependencies.
- **Mapping**: Implemented `LeadMapper` with the required De/Para logic and UTC date formatting.

> [!NOTE]
> The project was created using .NET 9 as target framework because the local SDK version is 9.0.312. If .NET 10 is required, the `TargetFramework` can be easily updated in the `.csproj` files once the SDK is available.
