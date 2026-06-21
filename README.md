# EconomyViewerWeb

REST API backend for the **Economy Viewer** application — a tool for browsing and managing Minecraft server economy price lists.

Migrated from the desktop WPF app ([EconomyViewerCore](https://github.com/aquaprogit/EconomyViewerCore)).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Git

## Solution structure

```
EconomyViewerWeb/
├── src/
│   ├── EconomyViewerWeb.Api/            # ASP.NET Core Web API entry point
│   ├── EconomyViewerWeb.Application/    # Use cases, services, DTOs
│   ├── EconomyViewerWeb.Domain/         # Entities, domain rules
│   └── EconomyViewerWeb.Infrastructure/ # EF Core, forum sync, external services
└── tests/
    └── EconomyViewerWeb.Tests/          # Unit tests
```

## Quick start

```bash
git clone https://github.com/aquaprogit/EconomyViewerWeb.git
cd EconomyViewerWeb
dotnet restore
dotnet build
dotnet run --project src/EconomyViewerWeb.Api
```

API runs at `http://localhost:5232` (see `launchSettings.json`).

## Endpoints (scaffold)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Built-in health check |
| GET | `/api/health` | API health status JSON |
| GET | `/openapi/v1.json` | OpenAPI spec (Development only) |

## Configuration

Key settings in `src/EconomyViewerWeb.Api/appsettings.json`:

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | SQLite database path (default: `economy.db`) |
| `ForumSync:EconomyForumUrl` | SimpleMinecraft economy forum URL for sync |

## Development workflow

1. Pick an issue from the [Kanban board](https://github.com/users/aquaprogit/projects/6)
2. Create a feature branch from `master`
3. Implement, add tests, open a PR

Suggested order: **#1** (done) → **#2** (EF Core) → **#3–#4** (domain) → **#5+** (API endpoints)

## Related

- [GitHub Issues](https://github.com/aquaprogit/EconomyViewerWeb/issues)
- [Project Board](https://github.com/users/aquaprogit/projects/6)
