# Data model — implementation notes

The project statement (`IP Information - Project Statement.pdf`, page 3) defines the data model.
This page records where the implementation differs from it, and why. The C# model and the
EF Core migrations in `src/IpInformation.Infrastructure/Persistence` are the source of truth.

## Countries

| Column | Statement | Implementation | Why |
|---|---|---|---|
| `Name` | `varchar(50)` | `nvarchar(100)` | IP2C returns names up to 58 characters (e.g. `United Kingdom of Great Britain and Northern Ireland (the)`), and Unicode covers names outside Latin-1. |
| `TwoLetterCode` | `char(2)` | `char(2)`, **unique index** | The IP lookup finds a country by this code, so there must be one row per code. |

All other columns match the statement.

## IpAddresses

Matches the statement: `IP varchar(15)` unique, `CountryId` foreign key to `Countries.Id`
(delete restricted), `CreatedAt`/`UpdatedAt` `datetime2` defaulting to `SYSUTCDATETIME()`.

All dates are stored and read as UTC.

## Seed data

Sample data is a plain SQL script, [`seed-data.sql`](seed-data.sql), run manually (e.g. in SSMS)
after the migrations have been applied. Its only purpose is that the database has data to look at
before the application has been used; the application never runs it.

- 9 countries and 28 IP addresses (4 for Greece, 3 for each other country), mirroring the sample report.
- Country names and codes are exactly what IP2C returned for these IPs (2026-11-21).
- Safe to run more than once: it only inserts the countries and IPs that are missing.
  Ids come from `IDENTITY` and dates from the column defaults.

The script is outside the application, so it does not affect the rule that the application accesses
the database only through EF Core. After it runs, the rows are ordinary data: the hourly refresh job
may move them to another country, or delete them when IP2C no longer knows them, like any other IP.

Run it in SSMS, or from the command line:

```powershell
sqlcmd -S .\SQLEXPRESS -E -i docs/seed-data.sql
```
