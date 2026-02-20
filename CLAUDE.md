# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**IshTop** — AI-powered Telegram job matching platform for the Uzbekistan market. Telegram-first, mobile-optimized. Starts with IT/programmers, expands to SMM, Design, Sales, Call Center, Logistics, Accounting, and remote international jobs.

## System Architecture (Three Components)

1. **User Bot** (`src/IshTop.Bot`) — Telegram bot with AI onboarding in Uzbek/Russian, structured profile collection, smart job matching with inline actions (Apply/Save/Hide/More like this), saved jobs, notification toggle.
2. **Parser** (`src/IshTop.Parser`) — WTelegramClient-based channel monitor that detects job posts, filters spam via AI, extracts structured job data with OpenAI.
3. **Admin Panel** (`admin/`) — Next.js 14 web app with JWT auth, dashboard analytics, user/job/channel management.

## Tech Stack

- **Backend:** .NET 8, Clean Architecture (Modular Monolith), MediatR CQRS, FluentValidation
- **Database:** PostgreSQL 16 + pgvector (vector similarity search for job matching)
- **Cache:** Redis 7
- **Queue:** RabbitMQ 3 (via MassTransit — wired but no consumers yet; prepared for future async processing)
- **AI:** OpenAI API (gpt-4o-mini for parsing, text-embedding-3-small for matching)
- **Bot:** Telegram.Bot 22.3.0 (polling-based)
- **Parser:** WTelegramClient 4.2.3 (MTProto Telegram client)
- **Admin:** Next.js 14 + TypeScript + Tailwind CSS

## Solution Structure

```
IshTop.sln
├── src/
│   ├── IshTop.Domain/           # Entities, enums, interfaces (zero dependencies)
│   ├── IshTop.Application/      # CQRS commands/queries, DTOs, validators (MediatR)
│   ├── IshTop.Infrastructure/   # EF Core, OpenAI, Redis, pgvector repos
│   ├── IshTop.Bot/              # Telegram user bot (BackgroundService)
│   ├── IshTop.Parser/           # Telegram channel parser (BackgroundService)
│   ├── IshTop.Api/              # REST API for admin panel (ASP.NET Core)
│   └── IshTop.Shared/           # Constants, localized bot messages
├── admin/                       # Next.js admin panel
├── tests/
│   ├── IshTop.Domain.Tests/
│   └── IshTop.Application.Tests/
└── docker-compose.yml           # PostgreSQL+pgvector, Redis, RabbitMQ
```

## Build & Run Commands

```bash
# Build entire .NET solution
dotnet build

# Run infrastructure
docker-compose up -d

# Run individual services
dotnet run --project src/IshTop.Bot
dotnet run --project src/IshTop.Parser
dotnet run --project src/IshTop.Api      # Swagger: http://localhost:5000/swagger

# Admin panel
cd admin && npm run dev                  # http://localhost:3000

# Tests
dotnet test

# Run a single test class or method
dotnet test --filter "FullyQualifiedName~ClassName"
dotnet test tests/IshTop.Domain.Tests    # Single project

# Note: test projects are currently scaffolded stubs (UnitTest1.cs placeholder only)
# Tests need to be written as features are implemented
```

## Database Migrations (EF Core)

```bash
# Add a new migration (run from solution root)
dotnet ef migrations add <MigrationName> --project src/IshTop.Infrastructure --startup-project src/IshTop.Api

# Apply migrations
dotnet ef database update --project src/IshTop.Infrastructure --startup-project src/IshTop.Api

# Revert last migration
dotnet ef migrations remove --project src/IshTop.Infrastructure --startup-project src/IshTop.Api
```

## Dependency Flow (Clean Architecture)

Domain ← Application ← Infrastructure ← Bot/Parser/Api

- **Domain** has no project dependencies (only Pgvector, MediatR.Contracts)
- **Application** depends on Domain
- **Infrastructure** depends on Application + Domain
- **Bot/Parser/Api** depend on Application + Infrastructure

## Key Patterns

### CQRS with MediatR
All business logic flows through MediatR in the Application layer. Handlers return `Result<T>` / `Result` (success/failure pattern) from `IshTop.Application/Common/Models/Result.cs`. Access the value via `.Value` (not `.Data`). FluentValidation validators auto-trigger via `ValidationBehavior<,>` pipeline behavior registered in `IshTop.Application/DependencyInjection.cs`.

### Repository + UnitOfWork
`IUnitOfWork` (in `IshTop.Domain/Interfaces/`) provides `Users`, `Jobs`, `Channels` repos + `SaveChangesAsync`. The `AppDbContext` auto-sets `UpdatedAt` on every `SaveChangesAsync` call.

### Embedding-Based Job Matching
User profiles and jobs get `text-embedding-3-small` vectors (stored as pgvector `float4`) in `UserProfile.Embedding` and `Job.Embedding`. Matching uses cosine distance via `IJobRepository.GetMatchingJobsAsync`. Duplicate detection uses a 0.95 similarity threshold (`IsDuplicateAsync`). Embeddings are cached in Redis for 7 days.

### Bot Onboarding State Machine
`User.OnboardingStep` enum tracks progress through 9 steps:
`Language → TechStack → Experience → Salary → WorkType → City → EnglishLevel → Confirmation → Completed`

`OnboardingService` in `IshTop.Bot/Services/` has `HandleStepAsync` (send UI) and `ProcessAnswerAsync` (parse input, advance step). On `Confirmation → yes`, calls `UpdateProfileCommand` and sets `UserState.Active`.

### Callback Data Conventions (Bot)
Inline keyboard callbacks follow the pattern `prefix:value`:
- `lang:uz` / `lang:ru`
- `exp:Junior`, `work:Remote`, `city:Toshkent`, `eng:Intermediate`
- `confirm:yes` / `confirm:no`
- `apply:{JobId}`, `save:{JobId}`, `hide:{JobId}`, `similar:{JobId}`
- `toggle_notif`, `restart_profile`

### Domain Events
`IshTop.Domain/Events/` contains MediatR `INotification` records: `ProfileCompletedEvent(Guid UserId)` and `JobCreatedEvent(Guid JobId)`. These are defined but handlers are not yet wired — they're intended for future async processing (e.g., via RabbitMQ).

### AI Prompts
All OpenAI system prompts are in `IshTop.Infrastructure/AI/PromptTemplates.cs`. Bot messages (Uzbek/Russian) are in `IshTop.Shared/Constants/BotMessages.cs`.

## Configuration

**Bot** (`src/IshTop.Bot/appsettings.json`):
- `ConnectionStrings:PostgreSQL`, `ConnectionStrings:Redis`
- `Telegram:BotToken`
- `OpenAI:ApiKey`
- `Jwt:Key/Issuer/Audience`

**Parser** (`src/IshTop.Parser/appsettings.json`):
- `ConnectionStrings:PostgreSQL`, `ConnectionStrings:Redis`
- `Telegram:ApiId`, `Telegram:ApiHash`, `Telegram:PhoneNumber` (MTProto — prompts for verification code on first run)
- `OpenAI:ApiKey`

**API** (`src/IshTop.Api/appsettings.json`):
- All of the above plus `Jwt:Key/Issuer/Audience`
- CORS is configured to allow `http://localhost:3000`

## Language Support

- Primary: Uzbek (Latin script). Bot messages in `IshTop.Shared/Constants/BotMessages.cs`
- Secondary: Russian (full translations available)
- User selects language during onboarding; all subsequent messages use their preference
