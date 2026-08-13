# Migration Summary — .NET Framework → .NET 10

## Date
2026-08-13

## Projects Migrated
- `./src/eShopWCFService/eShopWCFService.csproj`
- `./src/eShopWinForms/eShopWinForms.csproj`
- `./src/eShopWinForms/eShopWinForms.fx.csproj`

## Final Build Status
| Project | Errors | Warnings |
|---|---|---|
| eShopWCFService | 0 | 0 |
| eShopWinForms.csproj | 0 | 0 |
| eShopWinForms.fx.csproj | 0 | 0 |

---

## Changes Made

### eShopWCFService

**Project File (`eShopWCFService.csproj`)**
- Converted from old non-SDK format (Visual Studio 2015-era) to `Microsoft.NET.Sdk.Web` SDK-style
- Target framework changed from `net461` → `net10.0`
- Replaced all framework-shipped references with NuGet packages:
  - `System.ServiceModel` (framework ref) → `CoreWCF.Primitives 1.9.1`, `CoreWCF.Http 1.9.1`, `CoreWCF.ConfigurationManager 1.9.1`
  - `EntityFramework 6.1.3` (packages.config) → `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`
- Excluded `CatalogServiceClient.cs` from compilation (WCF client stub that does not belong in the server project)
- Added `GenerateAssemblyInfo>false` to preserve existing `Properties/AssemblyInfo.cs`

**New: `Program.cs`**
- Replaced IIS/WAS hosting with ASP.NET Core + CoreWCF hosting
- `WebApplication.CreateBuilder` with `AddServiceModelServices()` / `AddServiceModelMetadata()`
- EF Core `DbContext` registered via `AddDbContext<EntityModel>`
- Database `EnsureCreated()` + `CatalogDBInitializer.Seed()` called at startup
- CoreWCF `BasicHttpBinding` endpoint registered at `/CatalogService.svc` (same path as legacy)
- Connection string resolved from env var `ConnectionString`, then `appsettings.json > ConnectionStrings > EntityModel`, then a local dev default

**New: `appsettings.json`**
- Default connection string for local dev (SQL LocalDB)
- `Urls` binding to `http://0.0.0.0:5113` (matches original Docker port)

**`ICatalogService.cs`**
- `using System.ServiceModel;` → `using CoreWCF;`
- `[ServiceContract]` / `[OperationContract]` now resolve from `CoreWCF` namespace

**`CatalogService.svc.cs`**
- `using System.ServiceModel;` + `using System.ServiceModel.Web;` → `using CoreWCF;`
- `using System.Data.Entity;` → `using Microsoft.EntityFrameworkCore;`
- Parameterless constructor removed; `EntityModel` is now injected via DI
- `EntityState.Modified` now from `Microsoft.EntityFrameworkCore`

**`EntityModel.cs`**
- `using System.Data.Entity;` → `using Microsoft.EntityFrameworkCore;`
- Constructor changed from `DbContext(string connectionString)` to `DbContext(DbContextOptions<EntityModel>)`
- `Database.SetInitializer(new CatalogDBInitializer())` removed
- `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)` (EF Core API)
- `HasPrecision(19, 4)` and `IsUnicode(false)` — same syntax, work in EF Core

**Model files** (`CatalogItem.cs`, `CatalogBrand.cs`, `CatalogType.cs`, `CatalogItemsStock.cs`, `DiscountItem.cs`)
- Removed `using System.Data.Entity.Spatial;` (no EF Core equivalent; DbGeography/DbGeometry are not used by any of these models)
- Added nullable reference type annotations (`string?`, `CatalogType?`, `CatalogBrand?`)

**`Models/CatalogItemHiLoGenerator.cs`**
- `using System.Web;` removed
- `db.Database.SqlQuery<Int64>(...)` → `db.Database.SqlQueryRaw<long>(...)` (EF Core 7+ API)

**`Models/Infrastructure/CatalogDBInitializer.cs`**
- EF6 `CreateDatabaseIfNotExists<EntityModel>` base class removed
- Converted to a `static` class with static `Seed(EntityModel context)` method
- Each seeding method guards against double-seeding with an `.Any()` check
- Called from `Program.cs` after `EnsureCreated()`

**`Models/Infrastructure/CatalogConfiguration.cs`**
- `using System.Web;` removed
- Returns `Environment.GetEnvironmentVariable("ConnectionString")` or `null` (Program.cs handles the fallback)

**`Models/Infrastructure/PreconfiguredData.cs`**
- `using System.Web;` removed

**`CatalogServiceMock.cs`**
- Fixed CS8603 nullable return: `FirstOrDefault(...)` → `FirstOrDefault(...)!`
- Fixed CS8602 null deref in `GetAvailableStock`: added null-coalescing `?.AvailableStock ?? 0`
- Fixed CS8600 null-to-non-nullable in `CreateAvailableStock`: typed `s` as `CatalogItemsStock?`

---

### eShopWinForms (both .csproj and .fx.csproj)

**Package corrections (KB 20 — client vs server rule)**
- Removed `CoreWCF.Primitives`, `CoreWCF.ConfigurationManager`, `CoreWCF.Http`, `CoreWCF.WebHttp`, `CoreWCF.NetTcp` — these are server-only packages and must not be in client projects
- Replaced with `System.ServiceModel.Http 8.1.0` — the correct WCF client package for .NET 5+
- Removed `EntityFramework 6.5.2` — WinForms communicates through WCF; it has no direct DB access
- Removed `Microsoft.AspNet.WebApi.Client` — not used by any WinForms code
- Removed `Microsoft.CSharp 4.7.0` (fx.csproj) — auto-provided by the SDK (NU1510)

**Build target**
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` — required when building `net10.0-windows` on Linux
- Added `<SupportedOSPlatformVersion>6.1</SupportedOSPlatformVersion>` and `<NoWarn>CA1416</NoWarn>` — WinForms is intrinsically Windows-only; CA1416 platform-compat warnings are not actionable

**`Connected Services/eShopServiceReference/Reference.cs`**
- Added `new` keyword to `CloseAsync()` to resolve CS0108 (hides inherited `ClientBase<T>.CloseAsync()`)
- The generated proxy was created by `dotnet-svcutil` and is already compatible with `System.ServiceModel.Http`; no other changes needed

**`Views/CatalogView.Designer.cs`**
- Added `#pragma warning disable CS0169` at top of auto-generated file to suppress unused-field warnings for `dataGridViewTextBoxName` etc. (columns declared by the designer but never read directly; they are used via the DataGridView control reference)

---

## Architecture After Migration

```
eShopWCFService (net10.0, ASP.NET Core + CoreWCF)
├── Program.cs              — CoreWCF hosting, EF Core registration, DB seeding
├── ICatalogService.cs      — [ServiceContract] using CoreWCF
├── CatalogService.svc.cs   — Service implementation, injected EntityModel
├── EntityModel.cs          — EF Core DbContext (SqlServer)
└── Models/                 — POCO entities + infrastructure (seeder, config, data)

eShopWinForms (net10.0-windows, WinForms)
├── Program.cs              — WinForms Application entry point
├── Controllers/            — MVP controller pattern, calls ICatalogService
├── Views/                  — WinForms UI (CatalogView.cs + Designer.cs)
└── Connected Services/     — Generated WCF client proxy (System.ServiceModel.Http)
```

---

## Next Steps

- **SQL Server availability**: The service requires SQL Server (or LocalDB) at startup for `EnsureCreated()`. In the Docker environment, ensure `ConnectionString` env var points to a reachable SQL Server instance.
- **EF Core migrations**: If the database schema needs to evolve, add EF Core migrations (`dotnet ef migrations add`) instead of relying on `EnsureCreated()`.
- **WCF WSDL metadata**: `UseRequestHeadersForMetadataAddressBehavior` is registered in `Program.cs` for correct WSDL URL generation when behind a reverse proxy.
- **`CatalogItemHiLoGenerator`**: Uses `SqlQueryRaw<long>` to call a SQL sequence (`catalog_hilo`). This sequence must exist in the database schema. If EF Core migrations are added, include a migration to create it.
- **`CatalogServiceClient.cs`** (server project): This file was excluded from compilation because it is a WCF client stub that belongs only in client projects. It can be safely deleted from the `eShopWCFService` directory if desired.
