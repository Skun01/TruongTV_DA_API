# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Scope

This `learning/` repository is the backend API in the larger TachoProject monorepo.
Tech stack: .NET 8, ASP.NET Core Web API, EF Core (Npgsql/PostgreSQL), FluentValidation, Serilog, JWT auth.

## Build, run, and development commands

Run commands from `learning/`.

- Restore dependencies:
  - `dotnet restore Learning.sln`
- Build all projects:
  - `dotnet build Learning.sln`
- Run API (default profile):
  - `dotnet run --project API/API.csproj`
- Run API with hot reload:
  - `dotnet watch --project API/API.csproj run`

### Tests

- Run all tests in solution:
  - `dotnet test Learning.sln`
- Run a single test (when test projects exist):
  - `dotnet test Learning.sln --filter "FullyQualifiedName~<TestNameFragment>"`

Note: at the time this file was written, no `*Test*.csproj` was found in this repo.

### EF Core migrations (AppDbContext)

- Add migration:
  - `dotnet ef migrations add <MigrationName> --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --context AppDbContext`
- Update database:
  - `dotnet ef database update --project Infrastructure/Infrastructure.csproj --startup-project API/API.csproj --context AppDbContext`

## Architecture (big picture)

The codebase follows Clean Architecture with 4 projects in `Learning.sln`:

- `Domain`: entities/enums/constants only (no dependencies on other layers).
- `Application`: DTOs, service/repository interfaces, mapping extensions, helper utilities, shared response model.
- `Infrastructure`: `AppDbContext`, EF configurations/migrations, repository implementations, external service integrations, service implementations, DI wiring.
- `API`: controllers, validators, middleware, DI bootstrap, auth/cors/swagger/configuration binding.

Reference: `BACKEND_ORGANIZATION_NOTE.md` is the canonical backend organization guideline.

## Request/response and error contract (important)

API business responses use a unified `ApiResponse<T>` envelope (`Application/Common/ApiResponse.cs`) with fields:
`code`, `success`, `message`, `data`, `metaData`.

Project convention (also documented in `README.md` and `API.md`): business failures/validation errors are often returned as HTTP 200 with `success=false` and business `code`/`message`. Do not assume HTTP status alone indicates business success/failure.

Model validation is customized in `API/Extensions/ApiBehaviorExtension.cs` to return validation errors in this same envelope.

## Dependency injection and runtime composition

- Entry point: `API/Program.cs`.
- Service/repository registrations: `Infrastructure/DependencyInjection.cs`.
- Auth policy and JWT setup: `API/Extensions/AuthenticationConfigurationExtension.cs`.
- Options binding: `API/Extensions/OptionSettingsExtension.cs`.
- Global exception middleware: `API/Extensions/ExceptionHandlingExtension.cs`.

## Data access pattern

- `Infrastructure/Persistence/AppDbContext.cs` defines all DbSets.
- Repository + Unit of Work pattern is central:
  - Interface: `Application/IRepositories/IUnitOfWork.cs`
  - Implementation: `Infrastructure/Repositories/UnitOfWork.cs`
- Services orchestrate business flow and call repositories via UoW; mapping/helper logic should stay in `Application/Mappings` and `Application/Helper` (not inline in services).

## Feature/module map

The backend is organized around domain modules (not a single monolithic service). Current major areas include:

- Auth + user profile
- Cards (vocabulary/grammar/kanji), notes, sentences, media uploads
- Decks and learning progress/SRS
- Shadowing
- JLPT exam system (exam, section/group/question, exam session, AI-generated questions)

When adding features, follow existing module separation across all layers (`Domain` entities, `Application` DTOs/interfaces/mappings, `Infrastructure` repos/services, `API` controllers/validators).

## Configuration and local dependencies

Configured in `API/appsettings.json` and bound via options classes.
Common local dependencies used by features:

- PostgreSQL (`ConnectionStrings:DefaultConnection`)
- Voicevox service (`VoicevoxConfig`)
- Cloudinary (`Cloudinary`)
- JWT/email/app settings
- AI provider settings (`AiGeneration`)

When implementing features that depend on these integrations, verify required settings are present before runtime testing.
