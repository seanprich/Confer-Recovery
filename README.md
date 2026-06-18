# Confer — Self-Hosted A/V Server

Privacy-first audio/video meeting server for non-profit chapter organizations. Designed to run on hardware you own (Raspberry Pi 5, mini PC, or any Linux server) with no cloud media dependency.

Members' meeting audio and video never leave your network. All media is routed through a [LiveKit](https://livekit.io) SFU that you control.

Licensed under the **GNU General Public License v3.0**. No warranty. Verifiable by reading the source.

---

## What it does

- Issues short-lived, room-scoped LiveKit tokens so members can join A/V sessions
- Authenticates members with BCrypt-hashed passwords and JWT bearer tokens
- Enforces five access roles: `Listener`, `Presenter`, `Host`, `ChapterAdmin`, `OrgAdmin`
- Keeps an immutable audit trail of join/leave events — no media content is logged
- Encrypts LiveKit API secrets at rest using ASP.NET Data Protection
- Exposes local-only telemetry: Prometheus metrics + pre-built Grafana dashboard

---

## Stack

| Layer | Technology |
|---|---|
| API server | ASP.NET Core 10 Web API |
| Database | MongoDB 8 |
| Media SFU | LiveKit (self-hosted, not included here) |
| Telemetry | OpenTelemetry → Prometheus → Grafana |
| Container runtime | Docker + Docker Compose |

---

## Prerequisites

- **Docker** and **Docker Compose** (for production / quick start)
- **[.NET 10 SDK](https://dotnet.microsoft.com/download)** (for local development only)
- A running **LiveKit SFU** instance per chapter (see [LiveKit self-hosting docs](https://docs.livekit.io/home/self-hosting/))

---

## Quick start

```bash
git clone git@github.com:seanprich/Confer-Recovery.git
cd Confer-Recovery

cp .env.example .env
# Edit .env — set JWT_SECRET_KEY to at least 32 random characters
```

```bash
docker compose up -d
```

That starts four containers on a private bridge network:

| Service | Port | Purpose |
|---|---|---|
| `server` | `8080` | ASP.NET Core API |
| `mongo` | — | MongoDB (internal only) |
| `prometheus` | `9090` | Metrics storage |
| `grafana` | `3000` | Dashboard (anonymous viewer, login: admin) |

Check liveness: `curl http://localhost:8080/healthz`

---

## Configuration

Copy `.env.example` to `.env` and set values before starting.

| Variable | Required | Description |
|---|---|---|
| `JWT_SECRET_KEY` | Yes | Minimum 32 random characters. Used to sign API bearer tokens. |
| `GRAFANA_ADMIN_PASSWORD` | No | Grafana admin password (default: `changeme`). Change before exposing to any network. |
| `CORS_ALLOWED_ORIGINS` | No | Comma-separated list of allowed origins. Leave empty to allow all (localhost only recommended). |

LiveKit credentials are stored **per chapter** via the `PUT /api/chapters/{id}/sfu` endpoint after setup. They are encrypted at rest using ASP.NET Data Protection.

---

## API

All endpoints except `/api/auth/login`, `/metrics`, and `/healthz*` require a `Bearer` token.

### Authentication

```
POST /api/auth/login
```

Returns a JWT valid for 60 minutes.

### Members

```
GET    /api/members?chapterId={id}     # ChapterAdmin, OrgAdmin
GET    /api/members/{id}               # Own record, or admin
POST   /api/members                    # ChapterAdmin, OrgAdmin
PUT    /api/members/{id}/status        # ChapterAdmin, OrgAdmin
PUT    /api/members/{id}/role          # OrgAdmin
```

### Chapters

```
GET    /api/chapters
GET    /api/chapters/{id}
POST   /api/chapters                   # OrgAdmin
PUT    /api/chapters/{id}/sfu          # OrgAdmin — sets LiveKit URL + credentials
PUT    /api/chapters/{id}/status       # OrgAdmin
```

### Rooms

```
GET    /api/rooms?chapterId={id}
GET    /api/rooms/{id}
POST   /api/rooms                      # Host, ChapterAdmin, OrgAdmin
POST   /api/rooms/{id}/start           # Host, ChapterAdmin, OrgAdmin
POST   /api/rooms/{id}/end             # Host, ChapterAdmin, OrgAdmin
POST   /api/rooms/{id}/join            # Any authenticated member who has acknowledged consent
```

`POST /api/rooms/{id}/join` returns a short-lived LiveKit token and SFU URL. Members must have acknowledged the consent notice before a token is issued.

### Observability

```
GET /metrics      # Prometheus scrape endpoint (no auth)
GET /healthz      # Liveness probe
GET /healthz/ready  # Readiness probe (includes MongoDB ping)
```

---

## Roles

| Role | Can do |
|---|---|
| `Listener` | Join rooms as audio-only |
| `Presenter` | Join rooms with video publish rights |
| `Host` | Create and end rooms |
| `ChapterAdmin` | Manage members and rooms within a chapter |
| `OrgAdmin` | Full access across all chapters |

---

## Monitoring

Grafana loads automatically at `http://localhost:3000` with a pre-built dashboard showing:

- Active rooms and session counts
- HTTP request rate and latency (P50 / P95 / P99)
- Token issuance by role
- Auth success / failure rates
- Room session duration distribution
- .NET runtime GC heap size

Prometheus retains 30 days of metrics locally. No data leaves the host.

---

## Development

```bash
# Run MongoDB for local dev
docker compose up -d mongo

# Run the API
cd src/Server
dotnet run
```

The API starts at `http://localhost:5000` (or as configured in `launchSettings.json`).

### Running tests

```bash
dotnet test
```

49 tests — unit tests use NSubstitute mocks; integration tests use EphemeralMongo6 (embedded MongoDB, no external dependency).

---

## Project structure

```
src/Server/          # ASP.NET Core 10 API
  Controllers/       # Auth, Members, Chapters, Rooms
  Services/          # Business logic
  Repositories/      # MongoDB data access
  Models/            # Domain entities
  Telemetry/         # OpenTelemetry metrics + health checks
  Middleware/        # Audit logging middleware
test/Tests/          # xUnit tests
infra/
  prometheus/        # Prometheus scrape config
  grafana/           # Auto-provisioned datasource + dashboard
docs/
  executive-summary.html  # Printable privacy overview for stakeholders
```

---

## License

GNU General Public License v3.0 — see [https://www.gnu.org/licenses/gpl-3.0.html](https://www.gnu.org/licenses/gpl-3.0.html)

This software is distributed without any warranty. You are free to run, study, modify, and distribute it under the terms of the GPL.
