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

## 0007 — User-facing API messages are German

**Decision.** Code, comments and documentation are English, but the `detail` of a `ProblemDetails`
and every validation message are German.

**Why.** The UI is German and the HTTP interceptor puts these messages straight into a snackbar.
Translating them a second time in the frontend would mean maintaining the same catalogue twice for
an app that has exactly one locale.

## 0008 — Entity ids are `string`, stored as `ObjectId`

**Decision.** `Category.Id` is a `string` in the domain; the class map serialises it as an
`ObjectId` and lets MongoDB generate it on insert.

**Why.** Keeps `MongoDB.Bson.ObjectId` out of the domain and out of the API contract, while the
database still gets the compact, index-friendly native type. Ids that are not parsable as an
`ObjectId` are treated as "not found" by the repository instead of throwing.

## 0009 — A category's type cannot be changed

**Decision.** `PUT /api/categories/{id}` updates name, icon and colour only.

**Why.** Transactions must match the type of their category (business rule 1) and budgets only
exist for expense categories. Allowing the type to flip would silently invalidate existing
transactions and budgets, and repairing that would need a cascade that this app does not warrant.
Creating a second category and moving the transactions is the explicit alternative.

## 0010 — Icon and colour are validated by format, the choices live in the UI

**Decision.** The API accepts any Material Symbol name (`^[a-z][a-z0-9_]{0,39}$`) and any hex
triplet (`#RRGGBB`); the fixed sets the user can pick from are defined in the frontend.

**Why.** A fixed palette is a presentation concern. Duplicating the exact list on both sides would
mean two places to change whenever a colour is added, without making the data any safer — the
format check already rules out anything that would break rendering.

## 0011 — Duplicate names are prevented by a case-insensitive unique index

**Decision.** A unique index on `(name, type)` with collation `de`, strength 2. The repository
translates the resulting duplicate-key error into a `ConflictException` (HTTP 409).

**Why.** The index is the only check that cannot race, so there is no "does it exist?" query before
the insert: one round trip, and the database is the single source of truth. Strength 2 ignores case
and accents, so "Miete" and "miete" are recognised as the same category.

## 0012 — Seeding only fills an empty collection

**Decision.** `MongoInitializer` inserts the five default categories when the collection contains
no documents at all, and does nothing otherwise.

**Why.** It is idempotent across restarts and, unlike a per-category upsert, it does not resurrect
a default category that the user deliberately deleted.

## 0013 — Startup fails if MongoDB is unreachable

**Decision.** Index creation and seeding run in an `IHostedService` before the server starts
listening. If MongoDB is down, the host fails to start.

**Why.** An API that answers requests without its indexes is worse than one that is visibly down.
`docker compose` handles the ordering with a health check on the database.

## 0014 — One `ProblemDetails` shape for every invalid input

**Decision.** A global action filter runs the FluentValidation validators, and
`ApiBehaviorOptions.InvalidModelStateResponseFactory` is overridden so that model-binding failures
produce the same `ValidationProblemDetails` with the same German title. Error keys are camelCased to
match the JSON payload and the Angular form controls. `AllowInputFormatterExceptionMessages` is
disabled so that deserializer messages do not leak internal type names.

**Why.** The frontend maps `errors` onto form controls by key; it should not have to deal with two
different error formats depending on whether a value failed to parse or failed a rule.

## 0015 — `PUT` returns the updated resource

**Decision.** `PUT /api/categories/{id}` responds `200 OK` with the updated representation,
`DELETE` responds `204 No Content`.

**Why.** The client needs the stored result (the name is trimmed, the colour is uppercased) to
refresh its list without a second `GET`.

