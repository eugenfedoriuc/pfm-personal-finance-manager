# Design decisions

Short records of decisions taken while building the PFM assignment, including the
reasoning and the alternatives that were considered.

## 0001 — Repository layout: single repo, `backend/` and `frontend/` side by side

**Decision.** One Git repository containing `backend/` (.NET solution), `frontend/`
(Angular app), `docs/`, `api-collection/` and a root `docker-compose.yml`.

**Why.** The two parts are deployed together and reviewed together. A single repository
keeps the one command that starts everything (`docker compose up --build`) at the root and
avoids cross-repository version coupling for a project of this size.

## 0002 — Backend structure: four projects following the dependency rule

**Decision.** `Pfm.Domain` ← `Pfm.Application` ← `Pfm.Infrastructure`, with `Pfm.Api` as the
composition root referencing Application and Infrastructure.

**Why.** It makes the dependency direction explicit and compiler-enforced: the domain model
and the business calculations cannot accidentally take a dependency on MongoDB or ASP.NET
Core, which is what keeps them unit-testable without infrastructure. The cost is four
projects instead of one, which is acceptable because the boundaries are the point of the
exercise.

**Alternative considered.** A single project with folders. Cheaper to navigate, but the
layering would only be a convention and nothing would prevent it from eroding.

## 0003 — Central package management

**Decision.** All NuGet versions live in `backend/Directory.Packages.props`; shared compiler
settings live in `backend/Directory.Build.props`.

**Why.** One place to see and update every dependency version, and no chance of two projects
silently resolving different versions of the same package.

## 0004 — `TreatWarningsAsErrors` is enabled

**Decision.** Compiler warnings fail the build across all backend projects.

**Why.** Nullable-reference warnings in particular are only useful if they cannot be ignored.
`CS1591` (missing XML comment) is excluded, because XML documentation is generated to feed
the OpenAPI document, not to force a comment on every member.

## 0005 — Swagger UI via `Swashbuckle.AspNetCore.SwaggerUI`

**Decision.** The OpenAPI document is produced by the built-in `Microsoft.AspNetCore.OpenApi`
package; only the Swagger UI web assets are taken from Swashbuckle. The UI is served in the
Development environment only.

**Why.** ASP.NET Core 10 generates the OpenAPI document itself, so the full Swashbuckle
generator is redundant. Exposing an API explorer in production is an unnecessary information
disclosure for an app that has no authentication.

## 0006 — xUnit with NSubstitute, no assertion library

**Decision.** Plain xUnit assertions; NSubstitute for the few repository fakes.

**Why.** The brief rules out libraries that moved to a commercial licence (FluentAssertions,
AutoMapper, MediatR). NSubstitute is MIT-licensed and only used where hand-writing a fake
repository would add noise without adding clarity.
