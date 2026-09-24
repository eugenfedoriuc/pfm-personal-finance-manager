# Personal Finance Manager (PFM)

A small single-user personal finance manager: record income and expenses, organise them in
categories, set a monthly spending limit per category, and review a monthly summary.

- Backend: ASP.NET Core 10 Web API + MongoDB
- Frontend: Angular + Angular Material (German UI, locale `de-AT`)
- Deployment: `docker compose up --build`

## Quick start

**Prerequisite:** Docker Desktop (or Docker Engine + Compose) — nothing else.

```sh
docker compose up --build
```

Open **http://localhost** once the containers report healthy (`docker compose ps`). All
configuration (Mongo connection string, database name, CORS origin) is already baked into
`docker-compose.yml`, so there is no `.env` file or manual setup step. To stop everything:
`docker compose down` (add `-v` to also drop the MongoDB volume and start from an empty database).

## Local development (without Docker)

Run MongoDB (`docker compose up mongo` is the easiest way to get just the database), then:

```sh
# Backend — http://localhost:5150, Swagger UI at /swagger (Development only)
cd backend && dotnet run --project src/Pfm.Api

# Frontend — http://localhost:4200, proxies /api to the backend (see proxy.conf.json)
cd frontend && npm install && npm start
```

Tests: `dotnet test` in `backend/`, `npx ng test` in `frontend/`. The Bruno collection in
[api-collection/bruno](api-collection/bruno) is a self-cleaning smoke test for the API
(`bru run --env Local -r`).

Documentation is written as the project is built. See [docs/decisions.md](docs/decisions.md)
for the design decisions taken along the way.
