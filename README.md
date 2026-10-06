# SEO Lead Platform

An AI-powered client acquisition platform for SEO and digital marketing agencies.

The platform helps an agency:

1. Discover and manage prospect businesses
2. Run an SEO audit on each prospect's website
3. Score the opportunity (explainable, deterministic scoring)
4. Generate personalized outreach drafts
5. Track qualified leads through a sales pipeline
6. Share client-facing SEO reports via secure tokenized links

The MVP focuses on the vertical slice: **prospect → audit → opportunity score → outreach → lead**.

---

## Architecture

Clean architecture with four backend projects and one React SPA.

```
src/
  SeoLeadPlatform.Domain/            entities, enums, value objects
  SeoLeadPlatform.Application/       services, DTOs, validators, interfaces
  SeoLeadPlatform.Infrastructure/    EF Core, Postgres, JWT, SEO analyzers, background jobs
  SeoLeadPlatform.Api/               controllers, middleware, DI

tests/
  SeoLeadPlatform.UnitTests/         opportunity scoring, validation, SSRF protection
  SeoLeadPlatform.IntegrationTests/  API contract tests

web/                                 React + TypeScript + Vite + Tailwind
```

### Backend

- .NET 10 / ASP.NET Core Web API + Controllers
- Entity Framework Core 9 + Npgsql (PostgreSQL)
- FluentValidation for request validation
- Serilog for structured logging
- Swashbuckle for OpenAPI/Swagger
- JWT bearer authentication with BCrypt password hashing
- Background `SeoAuditWorker` (BackgroundService) with in-memory channel queue
- SSRF-safe HTTP client that refuses private/loopback addresses

### Frontend

- React 18 + TypeScript + Vite
- React Router, TanStack Query, React Hook Form, Zod
- Tailwind CSS (design system in `src/index.css`)
- Feature-based folders under `src/features/`

---

## Getting started

### Prerequisites

- .NET 10 SDK (preview)
- Node.js 20+
- Docker (optional, for local Postgres or full stack)

### Environment variables

Copy `.env.example` to `.env` and fill in secrets:

```
POSTGRES_DB=seoleadplatform
POSTGRES_USER=seolead
POSTGRES_PASSWORD=seolead
JWT_SIGNING_KEY=change-me-in-production-please-use-a-long-random-string
```

The API also honours these settings from `appsettings.json` and env vars:

- `ConnectionStrings__Postgres`
- `Jwt__SigningKey` **(required)**
- `Jwt__Issuer`, `Jwt__Audience`
- `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, …
- `Database__AutoMigrate` (default `true`)
- `USE_INMEMORY_DB` (integration tests only)

### Run with Docker Compose

```bash
docker compose up --build
```

- API → http://localhost:5080 (Swagger at `/swagger`)
- Web → http://localhost:5173
- Postgres → localhost:5432

### Run locally without Docker

1. Start Postgres (any local instance) and update `ConnectionStrings:Postgres`.
2. Backend:
   ```bash
   dotnet run --project src/SeoLeadPlatform.Api
   ```
   The API applies migrations on startup when `Database:AutoMigrate=true`.
3. Frontend:
   ```bash
   cd web
   npm install
   npm run dev
   ```

### Run tests

```bash
dotnet test SeoLeadPlatform.slnx
cd web && npm run build
```

### Add a database migration

```bash
dotnet ef migrations add <Name> \
  --project src/SeoLeadPlatform.Infrastructure \
  --startup-project src/SeoLeadPlatform.Api \
  --output-dir Persistence/Migrations
```

---

## Product principles

- Every screen answers **"what should the salesperson do next?"**
- No fabricated data: metrics come from observed audit signals or user input
- Opportunity scoring is deterministic and explainable — the UI shows each factor's contribution
- Outreach messages are always saved as drafts and require human approval before being marked as sent
- Website analysis is designed to be SSRF-safe: private/loopback IPs and non-HTTP schemes are refused

## Key endpoints

```
POST   /api/auth/register
POST   /api/auth/login
GET    /api/auth/me

GET    /api/agency
PUT    /api/agency
POST   /api/agency/services

GET    /api/prospects
POST   /api/prospects
GET    /api/prospects/{id}
DELETE /api/prospects/{id}
POST   /api/prospects/import
POST   /api/prospects/{id}/audit
GET    /api/prospects/{id}/audit
POST   /api/prospects/{id}/qualify
POST   /api/prospects/{id}/outreach
GET    /api/prospects/{id}/outreach
GET    /api/prospects/{id}/activities

POST   /api/prospects/{id}/contacts
PUT    /api/contacts/{id}
DELETE /api/contacts/{id}

PATCH  /api/leads/{id}/status
PUT    /api/leads/{id}

PUT    /api/outreach/{id}
POST   /api/outreach/{id}/approve
POST   /api/outreach/{id}/sent

POST   /api/proposals/generate/{prospectId}
GET    /api/proposals/prospect/{prospectId}
PUT    /api/proposals/{id}
POST   /api/proposals/{id}/send
GET    /api/proposals/public/{token}

GET    /api/dashboard/summary

POST   /api/reports/audits/{auditId}/token
GET    /api/reports/public/{token}
```

## End-to-end funnel workflow

The complete client acquisition workflow supported today:

1. **Find prospects** — add manually or bulk-import via CSV/paste
2. **Analyse** — run SEO audit (technical, on-page, content, performance, local)
3. **Score** — explainable opportunity score with weighted factors
4. **Decision makers** — add contacts and mark decision maker
5. **Outreach** — AI-drafted email, edit, approve, mark sent (auto-advances lead to Contacted)
6. **Track** — sales pipeline board, activity timeline, notes, follow-up dates
7. **Meeting → Proposal** — AI-generated proposal from audit + services + prospect
8. **Send proposal** — publishes a tokenized public URL and advances lead to ProposalSent
9. **Close** — mark Won/Lost, pipeline value tracked on dashboard

## What's next

Phase 1 and Phase 2 are complete. Phase 3 is partially delivered — proposal generation, activity timeline, contacts CRUD and bulk import are shipped. Remaining Phase 3 items (contact enrichment, email delivery, automated discovery, notifications) plug in via the abstractions already in place:

- `IProspectDiscoveryProvider` — implement to plug in a directory or discovery API
- `ILeadIntelligenceService` — currently heuristic; swap for an LLM-backed implementation
- `ISeoAuditProvider` — the website-based provider is the reference implementation; add Lighthouse/PageSpeed, Search Console, etc. as additional providers

Contributions and providers should preserve the "no fabricated data" and explainability guarantees.
