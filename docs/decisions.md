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

## 0016 — Dates are stored as `yyyy-MM-dd` strings

**Decision.** `Transaction.Date` is a `DateOnly` and is serialised with
`DateOnlySerializer(BsonType.String)`, so a booking is stored as `"2025-03-04"`.

**Why.** There is no time and no time zone to get wrong, and the value is readable in `mongosh`.
ISO-8601 dates are fixed width, so the lexicographic order is the chronological order and range
queries still use the index — verified with `explain()`, which reports index bounds
`["2025-03-01", "2025-04-01")` on `ix_transactions_date`.

**Alternative considered.** UTC midnight `DateTime`. It would enable MongoDB's date aggregation
operators, which this app does not use, in exchange for a value that silently shifts as soon as
anything converts it to a local time.

## 0017 — Money is stored as `Decimal128`

**Decision.** `Amount` and `Limit` are `decimal` in C# and are mapped with
`DecimalSerializer(BsonType.Decimal128)`. The OpenAPI document reports them as `format: decimal`.

**Why.** Binary floating point cannot represent amounts like `0.1` exactly, and the errors
accumulate in exactly the sums this app is about. The default OpenAPI format for `decimal` is
`double`, which would advertise the opposite of what happens, so a schema transformer corrects it.

## 0018 — The half-open month range lives in `MonthRange`

**Decision.** Every month-scoped endpoint takes a mandatory `year` and `month`, turns them into a
`MonthRange`, and queries `[Start, EndExclusive)` (business rule 4). `MonthRange.Of` validates the
bounds and is the only place the range is defined.

**Why.** Off-by-one errors at month boundaries are the classic bug in this kind of app, and the
rule is easier to trust when it exists once and has its own unit tests (December rollover, leap-year
February). Making the month mandatory is honest: every screen is month-scoped, and an unbounded
listing has no caller.

## 0019 — Create and update share one transaction contract

**Decision.** `POST` and `PUT /api/transactions` both take `TransactionRequest`.

**Why.** Both operations accept exactly the same fields. Two identical records would only be
duplication. Categories are different, and therefore keep separate contracts: the type can be set on
creation but not changed afterwards (0009).

## 0020 — Transactions and budgets embed their category

**Decision.** `TransactionResponse` and `BudgetResponse` contain the full `CategoryResponse` rather
than a bare `categoryId`.

**Why.** Every list that shows a transaction also shows the category's name, icon and colour, so the
alternative is a join on the client for every screen. The categories are a handful of documents, so
the service reads them once per request and maps in memory — no `$lookup`, no N+1.

## 0021 — `PUT /api/budgets` always answers `200 OK`

**Decision.** The upsert returns `200` whether it created or replaced the limit, instead of `201`
plus a `Location` header on first write.

**Why.** There is no `GET /api/budgets/{id}` to point a `Location` at — budgets are always read per
month — so a `201` would advertise a URL that does not exist.

## 0022 — The budget upsert is a single `findOneAndUpdate`

**Decision.** `BudgetRepository.UpsertAsync` runs one `findOneAndUpdate` with `IsUpsert` against the
`(categoryId, year, month)` equality filter and returns the document after the write.

**Why.** Read-then-write would race with itself and needs two round trips. The equality filter
fields are applied automatically when the upsert inserts, so only the limit has to be written, and
the unique index on the same three fields guarantees there is never a second budget for a month.

## 0023 — Domain rule violations carry the field they belong to

**Decision.** `ValidationException` takes a property name alongside its message, and the exception
handler renders it as an `errors` entry, producing the same body as a failed `FluentValidation` rule.

**Why.** Rules such as "the type must match the category" can only be checked against stored state,
but from the client's perspective they are still field errors. Giving them the same shape lets the
Angular form map every 400 the same way, whether the value failed a format rule or a business rule.

## 0024 — The monthly summary is computed by a pure calculator

**Decision.** `MonthlySummaryCalculator` is a static class that takes the month, its transactions,
its budgets and the categories, and returns the finished response. `SummaryService` only loads those
four things and calls it.

**Why.** This is the part of the app with real arithmetic — totals, percentages, budget overruns,
cumulative daily expenses — and it is the part most worth testing. With no I/O and no clock in the
way, every case (empty month, ties, exact limit, leap-year February) is a plain function call.

## 0025 — The budget fields of a comparison are null together

**Decision.** In `budgetComparison`, `limit`, `remaining` and `overBy` are all null exactly when the
category has no budget for the month. `isOverBudget` is a plain `false` in that case, and `spent` is
always a number.

**Why.** Business rule 6 puts categories without a budget in the same list as budgeted ones, so the
client needs a single unambiguous check. One null test on `limit` decides between showing a progress
bar and showing "Budget erstellen"; a zero limit would be indistinguishable from a real limit of 0.

## 0026 — The summary is ordered deterministically, without the server's locale

**Decision.** Breakdowns are sorted by amount descending and ties alphabetically by name;
`budgetComparison` is sorted by category name. All name comparisons use
`StringComparer.InvariantCulture`.

**Why.** Business rule 5 asks for alphabetical tie-breaking, and the top expense category is simply
the first entry of the already-sorted expense breakdown — one ordering rule instead of two. Pinning
the comparer means the output does not change with the container's locale, which also keeps the
tests honest.

## 0027 — `dailyExpenses` covers every day of the month

**Decision.** The API returns one entry per calendar day, including days with no expenses, and the
cumulative total carries across them. Cutting the series off at today for the current month is left
to the chart.

**Why.** A chart needs a continuous x-axis, and building the gaps client-side would duplicate the
leap-year and month-length logic that the server already has. What "today" is depends on the user's
time zone, so the server has no business deciding it.

## 0028 — Percentages are rounded to two decimals and need not add up to 100

**Decision.** `percentage` is `amount / total * 100`, rounded to two decimals; a total of zero
yields zero rather than an error.

**Why.** Rounded shares can add up to 99.99 or 100.01, and forcing the last one to absorb the
difference would make a category's percentage depend on its position in the list. The UI shows these
as a rough share next to the exact amount, so the exactness belongs on the amount.

## 0029 — The Bruno collection is an ordered, self-cleaning run

**Decision.** `api-collection/bruno` is a `.bru` collection with folders sequenced Health →
Categories → Transactions → Budgets → Summaries → Cleanup. Requests capture ids into runtime
variables, carry assertions on status and payload, and the Cleanup folder removes everything the run
created.

**Why.** It doubles as an executable smoke test: `bru run --env Local -r` exercises all four
resources and every business rule end to end, and because it cleans up after itself it can be run
repeatedly against the same database. Writing it as `.bru` text files keeps the diffs reviewable.

## 0030 — Angular 22 needs neither Zone.js nor `@angular/animations`

**Decision.** The app runs zoneless (no `zone.js` dependency, change detection driven by signals)
and without `provideAnimations`/`provideAnimationsAsync`.

**Why.** `ng new` no longer scaffolds Zone.js by default, which suits a signals-first app. Angular
Material 22's peer dependencies no longer include `@angular/animations` either — its components now
animate with plain CSS — so adding the package back would just be dead weight.

## 0031 — Fonts and icons are bundled via `@fontsource`, not Google Fonts

**Decision.** `@fontsource/roboto` (300/400/500) and `@fontsource/material-symbols-outlined` are
npm dependencies imported from `styles.scss`; `index.html` has no `fonts.googleapis.com` links. A
small `.material-symbols-outlined` utility class defines the ligature-font rules that Google's own
stylesheet would otherwise provide, and `MAT_ICON_DEFAULT_OPTIONS` points every `mat-icon` at it.

**Why.** `docker compose up` must work offline: a build step or a container with no internet access
should not depend on fetching fonts from Google at request time.

## 0032 — The selected month lives in the router, not in a stored signal

**Decision.** `MonthStateService` derives `selected` from the root route's `?month=YYYY-MM` query
parameter (via `toSignal` on `router.routerState.root.queryParamMap`) instead of holding its own
`signal<MonthValue>`. `next()`/`previous()`/`set()` all go through `router.navigate`.

**Why.** The brief requires the month to survive in the URL (shareable links, reload, back button).
Keeping a separate signal in sync with the URL is two sources of truth that can drift; reading the
URL directly means there is exactly one.

## 0033 — Categories keep their own signal-based store, not a shared one

**Decision.** `CategoryStore` is the only per-feature state service so far: a `signal<Category[]>`
plus `loading`/`loadError`, loaded once and patched in place by `create`/`update`/`delete`.

**Why.** Matches "state via signal-based services, no NgRx" from the brief. A category list is
short-lived, single-collection state — a signal service is the whole solution; introducing a
shared/global store now would be speculative for a feature this small.

## 0034 — The error interceptor skips the toast for 400 validation errors

**Decision.** The functional `errorInterceptor` shows a snackbar for every failed request except a
400 whose body has an `errors` object.

**Why.** Those errors are field-level and a form maps them onto the matching control right next to
where the user is looking; a toast repeating the same message would just be noise. A 409 (e.g. a
duplicate category name) has no `errors` object, so it still gets the toast — and the category form
additionally puts it under the Name field, since the brief asks for that conflict to be shown clearly.

## 0035 — A category's type is a `mat-radio-group`, not a `mat-button-toggle-group`

**Decision.** The create/edit dialog picks Income vs. Expense with `mat-radio-group`.

**Why.** `mat-button-toggle-group` in the installed Angular Material 22 build clips its own height
to a single line box (an upstream rendering bug found while verifying the dialog live: the group's
`.mat-button-toggle-button` children measured 40px tall but the group container measured under 17px,
cutting the labels off), while `mat-radio-group` renders correctly and is just as suited to a
two-option mutually exclusive choice.

## 0036 — Routing only exists for what is built

**Decision.** `app.routes.ts` currently only defines `/categories` (lazy-loaded) and redirects
everything else there. Dashboard, transactions and budgets get their routes in the phases that build
them.

**Why.** The brief rules out placeholder implementations. A `/dashboard` route pointing at an empty
stub page would be exactly that; better to grow the route table alongside the features and keep the
default redirect honest about what currently exists.

