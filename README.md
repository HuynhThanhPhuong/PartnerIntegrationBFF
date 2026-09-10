# Partner Integration BFF

A .NET 8 Backend-for-Frontend (BFF) microservice that receives transaction data from third-party partners, validates it, verifies the partner against an external service, and reliably publishes it to a message queue for downstream legacy systems.

## Architecture

This solution follows **Clean Architecture** with a strict, compiler-enforced dependency rule: dependencies only point inward.

```
API  ─────────────┐
                   ▼
Infrastructure ──► Application ──► Domain
```

| Project | Responsibility |
|---|---|
| `Domain` | Pure business rules. No dependency on any other project. `Transaction` and `Money` are self-validating: they can only be constructed via a `Create` factory method that enforces invariants (amount > 0, valid currency, required fields), throwing `ValidationException` on violation. |
| `Application` | Use-case orchestration (`TransactionService`). Defines DTOs (`TransactionRequest`/`TransactionResponse`) for the API boundary, and interfaces (`IPartnerVerification`, `ITransactionPublisher`) that Infrastructure implements — this is the Dependency Inversion Principle: Application never references Infrastructure directly. |
| `Infrastructure` | Technical implementations: `PartnerVerificationClient` (HTTP + Polly resilience) and `RabbitMqPublisher` (message publishing). This is the layer most likely to change (swap RabbitMQ for Kafka, REST for gRPC) without touching business logic. |
| `API` | Composition root. Wires up DI, hosts the HTTP endpoint, Global Exception Handler, and API Key authentication middleware. |
| `PartnerVerificationAPI` | A **mock** third-party service built only for this exercise, simulating the real Partner Verification API a partner company would expose (30% random `TimeoutException`, 70% success). In a real project this would not exist in our codebase — we would just call the partner's real endpoint. |
| `PartnerIntegrationBFF.Tests` | xUnit tests covering Domain validation rules and the resilience/retry pipeline. |

### Why this DTO ↔ Entity flow

```
HTTP Request (JSON)
      │
      ▼
TransactionRequest (DTO, all fields nullable — distinguishes "missing field" from "invalid value")
      │
      ▼
Transaction.Create(...)  (Domain factory — enforces business rules)
      │
      ▼
Transaction (Entity, guaranteed valid)
      │
      ├──► IPartnerVerification.VerifyAsync(...)
      └──► ITransactionPublisher.PublishAsync(...)
      │
      ▼
TransactionResponse (DTO — controls exactly what is exposed back to the caller)
```

DTOs exist only at the boundary (HTTP in/out). Internally, the Domain entity is passed directly between Application and Infrastructure — no re-wrapping needed once the data is validated.

## Resilience Strategy (Requirement 2)

`PartnerVerificationClient` calls the mock Partner Verification API through an `HttpClient` configured with **`Microsoft.Extensions.Http.Resilience`**'s standard resilience handler (`AddStandardResilienceHandler()`), which bundles:
- **Retry** with exponential backoff on 5xx/transient failures.
- **Circuit breaker** to fail fast if the dependency is persistently down.
- **Timeout** (per-attempt and total).

If all retry attempts are exhausted, `VerifyAsync` catches the failure and returns `false` rather than letting an exception crash the incoming request — the caller receives a clean `400 Bad Request` ("Partner verification failed"), never an unhandled `500`.

This is verified by `PartnerVerificationClientResilienceTests`, which replaces the real network call with a scripted `FakeHttpMessageHandler` to deterministically test both the "recovers after transient failures" and "fails gracefully after exhausting retries" scenarios — without depending on the real mock API's random behavior.

## Messaging

`RabbitMqPublisher` publishes a verified transaction to the durable queue `partner.transactions` (`RabbitMQ.Client` v7, async API). A dedicated `TransactionMessage` contract is used for the wire format instead of serializing the Domain entity directly, keeping the queue schema decoupled from internal Domain changes. The publisher is registered as a **Singleton** because the underlying connection is meant to be opened once and reused — not per-request.

## Security

The endpoint is protected by a custom **API Key** middleware (`X-Api-Key` header), matching the reality of B2B partner integrations (a static credential per partner, not an end-user login — OAuth/JWT would be over-engineered here).

**Known limitation**: an API key alone only proves "who is calling", not "was the traffic safe from eavesdropping". It provides real protection only when served over **HTTPS** — the local dev setup here uses HTTP for convenience, but production would require enforcing HTTPS.

## Error Handling

A centralized `GlobalExceptionHandler` (`IExceptionHandler`, .NET 8) maps `ValidationException` → `400` and any unhandled exception → `500`, with a consistent JSON response shape. Controllers no longer need per-action try/catch blocks.

## How to Run

### Option A — Docker Compose (recommended)

```bash
docker compose up --build
```

This starts 3 containers: `api` (port 5032), `partnerverificationapi` (port 5131), and `rabbitmq` (5672 / management UI on 15672).

> `docker-compose.override.yml` (gitignored, not included in this repo) is used locally to set `ASPNETCORE_ENVIRONMENT=Development` so Swagger UI is available. Without it, the stack runs in Production mode (Swagger disabled) — the correct default for real deployments.

### Option B — Run locally via Visual Studio / CLI

Requires RabbitMQ running locally (`docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4.3-management-alpine`), then run `PartnerVerificationAPI` and `API` as separate startup projects (Solution → Set Startup Projects → Multiple startup projects).

## How to Test

### Manually (Swagger)

1. Open `http://localhost:5032/swagger`.
2. Click **Authorize** and enter the API key: `super-secret-partner-key-2026`.
3. `POST /api/v1/partner/transactions` with:
   ```json
   {
     "partnerId": "P-1001",
     "transactionReference": "TXN-99823",
     "amount": 250.00,
     "currency": "USD",
     "timestamp": "2024-05-10T14:30:00Z"
   }
   ```
4. Expect `200 OK` with `"status": "Verified"` (retries mask most of the mock API's 30% failure rate).
5. Check `http://localhost:15672` (guest/guest) → Queues → `partner.transactions` to confirm the message was published.

### Automated

```bash
dotnet test PartnerIntegrationBFF.Tests
```

Covers: `Money`/`Transaction` validation rules (Domain), and the resilience/retry pipeline (Infrastructure) using a fake `HttpMessageHandler`.

## Known Limitations / Future Improvements

- API Key auth needs HTTPS in front of it to be meaningful; consider per-partner keys with rotation and expiry instead of one shared static key.
- `RabbitMqPublisher` reuses a single `IChannel` across concurrent requests; `IChannel` is not guaranteed thread-safe for concurrent publish — a channel-per-publish (on top of the shared, safe-to-share `IConnection`) would be more robust under load.
- No idempotency check on `transactionReference` — a retried/duplicated partner request could be published twice.
- `docker-compose.yml` is intended for local development; production would deploy via Kubernetes/managed services with environment variables injected by the deployment pipeline, not via compose files.
