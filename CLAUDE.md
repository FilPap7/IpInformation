# IpInformation — project rules

Scope: this repository only. The global `CLAUDE.md` still applies; where they conflict, this file wins.

## Data access

- ALL database access goes through EF Core, including the report endpoint (task 3).
- NEVER use raw SQL: no `FromSql*`, `SqlQuery*`, `ExecuteSql*`, Dapper or ADO.NET commands.

## Use cases

- Task 1 (IP information lookup) is a command, `ResolveIpInformationCommand`: on a cache/DB miss it calls IP2C and persists the result, so it changes state.
- Queries never change state (global rule); the report (task 3) is a query.

## Packages

- MediatR is pinned to 12.5.0 (Apache-2.0). NEVER upgrade to v13+ (RPL-1.5 / commercial license).

## Database seeding

- Seed data is a manual SQL script, `docs/seed-data.sql`, run by the user in SSMS after the migrations. It is demo data only; the application never runs it. See `docs/data-model.md`.

## Integration tests

- Until further notice, tests that need a real database run on the local SQL Server Express, not Testcontainers
  (exception to the global rule; Docker is not available yet).
- They use their own database, `IpInformation_Tests`, which the test fixture migrates automatically and empties
  before each test. The fixture refuses any database whose name does not end with `_Tests`.
- Override the connection string with the `IPINFORMATION_TEST_DATABASE` environment variable (e.g. for a container later).
- They carry `[Trait("Category", "Database")]`: skip them with `dotnet test --filter "Category!=Database"`.

## Git history

- One commit per step (global rule).
- Squash + force push on `main` only when we change a decision that is already pushed, so that `main` reads as a meaningful history. It is an exception, never a habit.
- When rewriting: explain the commands first, wait for approval, use `--force-with-lease`.
