# IpInformation

A REST API that returns the country of an IPv4 address, using the free [IP2C](https://ip2c.org) web service.
Results are cached in memory and stored in SQL Server, and a background job refreshes every stored IP every hour.

Built with .NET 10, Clean Architecture, CQRS (MediatR) and EF Core code-first.

## Features

| | What it does |
|---|---|
| **IP lookup** | Returns the country of an IP. Looks in the cache first, then the database, and only then calls IP2C; a new result is stored in the database and the cache. |
| **Hourly refresh** | Re-checks every stored IP against IP2C in batches of 100. When the country of an IP changed, the database is updated and its cache entry is invalidated. |
| **Country report** | How many IPs each country has and when an IP of it was last updated, optionally filtered by two-letter country codes. A single EF Core LINQ query, no raw SQL. |

## Architecture

```
src/
  IpInformation.Domain          Aggregates (Country, IpAddress), their rules, domain events. No dependencies.
  IpInformation.Application     Use cases as MediatR commands/queries, validators, interfaces for what they need.
  IpInformation.Infrastructure  EF Core (SQL Server), repositories, cache, IP2C client, the hourly job.
  IpInformation.Api             Minimal API endpoints, error handling, composition root.
tests/
  IpInformation.Domain.Tests        Every domain rule, no mocks.
  IpInformation.Application.Tests   Handlers and validators, with hand-written fakes.
  IpInformation.IntegrationTests    Infrastructure and API pieces; database tests on a real SQL Server.
```

Dependencies point inward only: Domain ← Application ← Infrastructure ← Api.

- **CQRS with MediatR.** The IP lookup is a command (on a miss it stores the result); the report is a query.
- **Validation in the pipeline.** A MediatR behavior runs the FluentValidation validator of each request
  and returns a validation failure before the handler runs.
- **Result pattern.** Expected failures (invalid input, not found) are returned as `Result` values and mapped
  centrally to [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) responses. Exceptions are for unexpected failures only.
- **Domain events.** When an IP moves to another country or is removed, the aggregate raises an event.
  The DbContext dispatches it **after** a successful save, and its handler invalidates the cache entry.

How an IP lookup flows:

```
GET /api/ip-information/8.8.8.8
  -> validation (IPv4?)                       invalid -> 400
  -> cache          hit -> return
  -> database       hit -> cache it -> return
  -> IP2C           unknown -> 404, unavailable -> 503
     -> store country (if new) and IP -> cache it -> return
```

## Getting started

**Prerequisites:** [.NET SDK 10.0.301](https://dotnet.microsoft.com/download) or a later 10.0 feature band,
and SQL Server (the instructions use a local SQL Server Express, `.\SQLEXPRESS`).

```powershell
# 1. Restore the local tools (dotnet-ef)
dotnet tool restore

# 2. Store the connection string in user-secrets (never in appsettings.json)
dotnet user-secrets set "ConnectionStrings:Database" "Server=.\SQLEXPRESS;Database=IpInformation;Trusted_Connection=True;TrustServerCertificate=True" --project src/IpInformation.Api

# 3. Create the database from the migrations
dotnet ef database update --project src/IpInformation.Infrastructure --startup-project src/IpInformation.Api

# 4. (Optional) Load sample data: run docs/seed-data.sql in SSMS, or
sqlcmd -S .\SQLEXPRESS -E -i docs/seed-data.sql

# 5. Trust the HTTPS development certificate (once per machine)
dotnet dev-certs https --trust

# 6. Run
dotnet run --project src/IpInformation.Api --launch-profile https
```

Open **https://localhost:7256/swagger** to try the API. On startup the hourly job runs its first cycle;
the console shows `IP refresh cycle finished: N processed, ...`.

If the app fails with *"Unable to configure HTTPS endpoint"*, step 5 is missing.

## API

### `GET /api/ip-information/{ip}`

```http
GET https://localhost:7256/api/ip-information/8.8.8.8
```

```json
{
  "countryName": "United States of America (the)",
  "twoLetterCode": "US",
  "threeLetterCode": "USA"
}
```

### `GET /api/reports/countries`

Repeat `twoLetterCodes` to filter; omit it for all countries.

```http
GET https://localhost:7256/api/reports/countries?twoLetterCodes=GR&twoLetterCodes=US
```

```json
[
  { "countryName": "Greece", "addressesCount": 4, "lastAddressUpdated": "2026-11-21T10:15:00Z" },
  { "countryName": "United States of America (the)", "addressesCount": 3, "lastAddressUpdated": "2026-11-21T10:15:00Z" }
]
```

### Errors

Every error is a ProblemDetails response with a machine-readable `code`:

| Status | `code` | When |
|---|---|---|
| 400 | `IpAddress.InvalidIp` | The IP is not a dotted-decimal IPv4 address. |
| 400 | `Country.InvalidTwoLetterCode` | A report filter code is not two Latin letters. |
| 404 | `IpInformation.NotFound` | IP2C does not know the IP. |
| 503 | `Ip2c.Unavailable` | IP2C could not be reached or timed out. |
| 500 | — | Unexpected error (no internal details are returned). |

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The IP must be a valid IPv4 address in dotted-decimal form, e.g. 44.255.255.254.",
  "code": "IpAddress.InvalidIp"
}
```

[`src/IpInformation.Api/IpInformation.Api.http`](src/IpInformation.Api/IpInformation.Api.http) has ready-made requests for Visual Studio / VS Code.

## Configuration

| Setting | Default | Where |
|---|---|---|
| `ConnectionStrings:Database` | — (required) | user-secrets locally, environment variables elsewhere |
| `Ip2c:BaseAddress` | `https://ip2c.org/` | `appsettings.json` |
| `Ip2c:Timeout` | `00:00:10` | `appsettings.json` |
| `IpAddressRefresh:Interval` | `01:00:00` | `appsettings.json` |
| `IpAddressRefresh:BatchSize` | `100` | `appsettings.json` |

Durations are `hh:mm:ss`. Any setting can be overridden by an environment variable, e.g.
`IpAddressRefresh__Interval=00:01:00` for a refresh every minute. Invalid values stop the application at startup.

## Tests

```powershell
dotnet test                                   # everything
dotnet test --filter "Category!=Database"     # without SQL Server
dotnet test --filter "Category=Database"      # only the database tests
```

Database tests run against their own database, **`IpInformation_Tests`**, on `.\SQLEXPRESS`. The test fixture
creates and migrates it automatically and empties it before each test; it refuses any database whose name does
not end with `_Tests`, so it can never touch real data. To use another server, set the
`IPINFORMATION_TEST_DATABASE` environment variable to its connection string.

## Design decisions

- **The cache never expires.** Entries are removed only when the hourly job finds that an IP changed or was removed,
  so the cache always agrees with the database. A size limit (100,000 IPs) keeps memory bounded.
- **IPs that IP2C no longer knows are deleted** by the refresh job. This matches the lookup, which never stores an IP
  that IP2C does not know. When IP2C fails or times out for an IP, the IP is skipped and retried in the next cycle.
- **Batches use keyset pagination** (`Id > lastId ORDER BY Id`), and each batch runs with a fresh DbContext,
  so a cycle stays correct when rows change and memory does not grow with the number of IPs.
- **The data model follows the project statement**, with two changes (country name `nvarchar(100)`,
  unique `TwoLetterCode`). See [`docs/data-model.md`](docs/data-model.md).
- **Seed data is a plain SQL script** run by hand: it exists only so a new database has something to show.

## Roadmap

- Structured logging with a correlation ID per request.
- A front-end for the lookup and the report.
- Docker Compose (API + SQL Server) and Testcontainers for the database tests.
