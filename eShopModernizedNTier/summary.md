# Migration Summary: eShopModernizedNTier – .NET Framework 4.6.1 → .NET 10

## Result
`dotnet build` exits successfully with **0 errors, 0 warnings**.

---

## Changes Made

### eShopWCFService (server project)

| File | Change |
|---|---|
| `eShopWCFService.csproj` | Rewrote from old-style .csproj to SDK-style `Microsoft.NET.Sdk.Web`, targeting `net10.0`. Replaced EF6 + System.ServiceModel references with `CoreWCF.Http 1.9.1`, `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`, and `Microsoft.EntityFrameworkCore.Tools 10.0.0`. |
| `Program.cs` *(new)* | ASP.NET Core entry point: registers EF Core `DbContext`, CoreWCF service, seeds the database, and exposes the WCF endpoint at `/CatalogService.svc` via `BasicHttpBinding`. |
| `appsettings.json` *(new)* | Replaces `Web.config` for connection string and Kestrel URL (`http://0.0.0.0:5113`). |
| `EntityModel.cs` | Migrated from EF6 `DbContext` (string connection constructor + `Database.SetInitializer`) to EF Core `DbContext(DbContextOptions<EntityModel>)`. `OnModelCreating` updated for EF Core `ModelBuilder` (same fluent API; `HasPrecision`/`IsUnicode` supported). Added `OnConfiguring` fallback for design-time tools. |
| `ICatalogService.cs` | Replaced `using System.ServiceModel` with `using CoreWCF` (server contracts must use the `CoreWCF` namespace on .NET 10). |
| `CatalogService.svc.cs` | Replaced `using System.Data.Entity` / `System.ServiceModel` with `using CoreWCF` / `using Microsoft.EntityFrameworkCore`. Removed `new EntityModel()` default constructor (now DI-injected). `Dispose()` no longer disposes `EntityModel` (managed by the DI scope). |
| `CatalogDBInitializer.cs` | Replaced EF6's `CreateDatabaseIfNotExists<T>` pattern with a static `CatalogDBInitializer.Initialize(EntityModel)` method that calls `EnsureCreated()` then seeds if the database is empty. Called from `Program.cs` on startup. |
| `CatalogConfiguration.cs` | Removed `using System.Web`. |
| `PreconfiguredData.cs` | Removed `using System.Web`. |
| `DiscountItem.cs` | Removed `using System.Web`. |
| `CatalogBrand.cs`, `CatalogType.cs`, `CatalogItem.cs`, `CatalogItemsStock.cs` | Removed `using System.Data.Entity.Spatial` (unused EF6 spatial type import). |
| `CatalogServiceMock.cs` | Removed unused `System.Text` / `System.Threading.Tasks` usings. |
| `CatalogServiceClient.cs` | Cleared dead-code WCF client proxy that did not belong in the server project (broken interface implementation, `System.Web` dependency). The proper WCF client proxy lives in `eShopWinForms/Connected Services/eShopServiceReference/Reference.cs`. |
| `Models/CatalogItemHiLoGenerator.cs` | Replaced EF6 `Database.SqlQuery<Int64>()` with EF Core 8+ `Database.SqlQueryRaw<long>()`. Removed `using System.Web`. |

### eShopWinForms (WinForms client project)

| File | Change |
|---|---|
| `eShopWinForms.csproj` | Replaced incorrect **server-side** `CoreWCF.*` packages with the **client-side** `System.ServiceModel.Http 8.1.0` package (WCF client for .NET). Removed unused `EntityFramework`, `Microsoft.AspNet.WebApi.Client`. Added `EnableWindowsTargeting=true` for cross-OS compilation. Removed unnecessary `UseWPF`. Kept `System.Configuration.ConfigurationManager` for `App.config` support. |
| `App.config` | Removed EF6 `configSections` / `entityFramework` config block (EF6 no longer referenced). Kept WCF `<system.serviceModel>` client endpoint config. |

---

## Architecture after migration

```
eShopWCFService  (net10.0, CoreWCF HTTP server, EF Core 10 / SQL Server)
  ↑ BasicHttpBinding HTTP on port 5113
eShopWinForms    (net10.0-windows, WinForms, System.ServiceModel.Http WCF client)
```

- The WCF service is now an ASP.NET Core / Kestrel application. No IIS or `.svc` file activation needed.
- `CatalogService.svc` and `Web.config` are inert legacy artefacts; they can be deleted in a follow-up.
- Database seeding runs once at startup via `EnsureCreated()` + `CatalogDBInitializer.Initialize()`.

---

## Next Steps

- The `.svc` file and `Web.config` in `eShopWCFService` are no longer used and can be deleted in a clean-up pass.
- `CatalogItemHiLoGenerator` is unused (commented out in `ApplicationModule.cs`). Consider removing it or wiring it up if the Hi-Lo ID generation pattern is still required.
- Add EF Core migrations (`dotnet ef migrations add InitialCreate`) to manage schema changes in production instead of relying on `EnsureCreated()`.
- The `docker-compose.dcproj` and Dockerfiles reference `http://localhost:5200` for the old IIS-hosted service; update to port `5113` or configure via environment variable (`ConnectionString`) for container deployments.
- Consider adding `[SupportedOSPlatform("windows")]` attribute suppression in `eShopWinForms` if CA1416 warnings reappear with stricter analyzer settings.
