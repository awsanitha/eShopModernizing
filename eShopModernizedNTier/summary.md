# Migration Summary — .NET Framework → net10.0

## Final Build Status

| Project | Target | Errors | Warnings |
|---|---|---|---|
| `eShopWCFService` | `net10.0` | 0 | 0 |
| `eShopWinForms` | `net10.0-windows` | 0 | 0 |
| `eShopWinForms.fx` | `net10.0-windows` | 0 | 0 |

---

## Changes Made

### eShopWCFService

**eShopWCFService.csproj**
- Converted from legacy XML-style `.csproj` (ToolsVersion 15.0) to SDK-style using `Microsoft.NET.Sdk.Web`
- Target: `net10.0`
- Replaced EF6 references (`EntityFramework.6.1.3` HintPath) with EF Core 10 packages:
  - `Microsoft.EntityFrameworkCore` 10.0.0
  - `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0
  - `Microsoft.EntityFrameworkCore.Design` 10.0.0 (private/build-only)
- Replaced WCF framework references with CoreWCF NuGet packages:
  - `CoreWCF.Primitives` 1.9.1
  - `CoreWCF.Http` 1.9.1
  - `CoreWCF.ConfigurationManager` 1.9.1
- Set `GenerateAssemblyInfo=false` to avoid CS0579 duplicates with existing `Properties/AssemblyInfo.cs`
- Excluded `CatalogServiceClient.cs` from compilation — it is a broken client-side proxy stub that does not belong in the server project and cannot satisfy the `ICatalogService` interface contract

**ICatalogService.cs**
- Changed `using System.ServiceModel;` → `using CoreWCF;` (KB-20: server contracts use CoreWCF namespace)
- `[ServiceContract]` / `[OperationContract]` now resolve from `CoreWCF.*`

**EntityModel.cs** (EF6 → EF Core)
- `using System.Data.Entity;` → `using Microsoft.EntityFrameworkCore;`
- Removed `using System.ComponentModel.DataAnnotations.Schema;` duplicate (already in entity classes)
- `DbContext` constructor pattern updated:
  - Parameterless constructor uses `OnConfiguring` for non-DI scenarios
  - Added DI constructor `EntityModel(DbContextOptions<EntityModel> options)`
- Removed `Database.SetInitializer(new CatalogDBInitializer())` — EF Core has no equivalent; initialization is done from `Program.cs` at startup
- `OnModelCreating` updated to EF Core `ModelBuilder` API (same intent, syntax compatible)
- Added explicit column type annotations for `date` columns (`CatalogItemsStock.Date`, `DiscountItem.Start/End`) since EF Core does not auto-map `DateTime` to `date`

**CatalogService.svc.cs**
- Removed `using System.Data.Entity;` → added `using Microsoft.EntityFrameworkCore;` for `EntityState.Modified`
- Removed `using System.ServiceModel;`, `using System.ServiceModel.Web;`, `using System.Web;`
- Added nullable annotations to suppress CS8600

**Models/*.cs** (all entity classes)
- Removed `using System.Data.Entity.Spatial;` (removed in EF Core — `DbGeography`/`DbGeometry` types don't exist)
- Added nullable annotations for string and navigation properties

**Models/DiscountItem.cs**
- Removed `using System.Web;`

**Models/CatalogItemHiLoGenerator.cs**
- Removed `using System.Web;`
- Replaced EF6 `db.Database.SqlQuery<Int64>(sql)` with direct ADO.NET via `db.Database.GetDbConnection()` — most reliable cross-version approach for sequence queries
- Added `using Microsoft.EntityFrameworkCore;` for `GetDbConnection()` extension method

**Models/Infrastructure/CatalogConfiguration.cs**
- Removed `using System.Web;`
- Replaced EF6-specific `"name=EntityModel"` connection-string reference with a real SQL Server connection string fallback
- Environment variable `ConnectionString` override preserved for container deployments

**Models/Infrastructure/CatalogDBInitializer.cs**
- Replaced EF6 `CreateDatabaseIfNotExists<EntityModel>` base class with a static EF Core initializer
- Calls `context.Database.EnsureCreated()` for database creation (equivalent behavior)
- Seeding now checks `Any()` before inserting to be idempotent

**Models/Infrastructure/PreconfiguredData.cs**
- Removed `using System.Web;`

**CatalogServiceMock.cs**
- Removed duplicate explicit-interface implementations for `GetCatalogBrands()` / `GetCatalogTypes()` — consolidated to single public implementation returning `List<T>` to match the interface contract cleanly
- Removed `using System.Web;`
- Fixed null-safe `GetAvailableStock` fallback

**Program.cs** (new — replaces Global.asax + Web.config service hosting)
- CoreWCF hosting setup via `builder.Services.AddServiceModelServices()`
- EF Core DbContext registration via `builder.Services.AddDbContext<EntityModel>()`
- Database initialization at startup (EnsureCreated + seed) inside a DI scope
- `BasicHttpBinding` endpoint at `/CatalogService.svc` matching the original WCF endpoint path
- ServiceMetadataBehavior with `HttpGetEnabled=true` (mirrors `<serviceMetadata httpGetEnabled="true">` from Web.config)

**appsettings.json** (new — replaces Web.config connection strings)
- Connection string `EntityModel` matching the original Web.config value

---

### eShopWinForms (both .csproj files)

Both `eShopWinForms.csproj` and `eShopWinForms.fx.csproj`:

**Package fixes (per KB-20 WCF client/server classification rule)**
- **Removed** `CoreWCF.Primitives`, `CoreWCF.ConfigurationManager`, `CoreWCF.Http`, `CoreWCF.WebHttp`, `CoreWCF.NetTcp` — CoreWCF is server-only; these should never be in a WCF client project
- **Removed** `EntityFramework 6.5.2` — WinForms client does not use EF; it consumes the catalog data exclusively via the WCF service reference
- **Removed** `Microsoft.AspNet.WebApi.Client` — not used by any WinForms code
- **Added** `System.ServiceModel.Primitives` 8.1.0 — provides `ClientBase<T>`, service contract attributes for the client proxy in `Reference.cs`
- **Added** `System.ServiceModel.Http` 8.1.0 — provides `BasicHttpBinding` used by `Reference.cs` `GetDefaultBinding()`

**Property additions**
- `EnableWindowsTargeting=true` — required to compile `net10.0-windows` projects on Linux CI agents
- `NoWarn=CA1416;CS0108;CS0169`:
  - CA1416: Windows platform-compatibility analyzer fires false positives when cross-compiling Windows desktop apps on Linux; the entire project is `net10.0-windows` and is Windows-only by design
  - CS0108: hiding-without-new in auto-generated `Reference.cs` (`CloseAsync` shadows `ClientBase<T>.CloseAsync`)
  - CS0169: unused fields in auto-generated `CatalogView.Designer.cs`
- `GenerateAssemblyInfo=false` (fx.csproj) — avoids CS0579 duplicates with designer-generated assembly attributes

**eShopWinForms.fx.csproj only**
- Removed explicit `Microsoft.CSharp 4.7.0` reference (NU1510 — already included transitively by Windows Desktop SDK)

**App.config**
- Removed `<configSections>` EF6 registration, `<entityFramework>` configuration block, `<startup>` .NET Framework version declaration, and `<system.serviceModel>` client endpoint (WCF endpoint is now hardcoded in `Reference.cs` `GetDefaultEndpointAddress()` and does not use config-file-based endpoint resolution)
- Retained `<System.Windows.Forms.ApplicationConfigurationSection>` with `DpiAwareness=PerMonitorV2` (valid WinForms .NET 5+ setting)

---

## Behavioral Notes

- **Database initialization**: EF Core's `EnsureCreated()` creates the database schema only if the database does not exist. It does not run migrations. For existing databases, no schema changes are made. This mirrors the EF6 `CreateDatabaseIfNotExists` behavior.
- **Connection string resolution order**: `appsettings.json` → `ConnectionString` environment variable → hardcoded fallback in `CatalogConfiguration`. This supports both local development and Docker container deployments.
- **WCF endpoint URL**: `/CatalogService.svc` — preserved from original Web.config service mapping so existing WinForms client proxy endpoints remain functional.
- **`CatalogServiceClient.cs` in server project**: Excluded from compilation. It was an incomplete client proxy stub (mismatched `GetCatalogItems()` signature, `System.Web` dependency) that had no callers in the server-side code. The authoritative client proxy is `Connected Services/eShopServiceReference/Reference.cs` in the WinForms project.

---

## Next Steps

- **EF Core Migrations**: The project currently uses `EnsureCreated()` which does not support schema migrations. For production use, consider running `dotnet ef migrations add InitialCreate` and replacing `EnsureCreated()` with `context.Database.Migrate()` in Program.cs.
- **CoreWCF service lifetime**: `CatalogService` is registered as `Transient`. If the service accumulates per-request state, consider `Scoped`. Monitor for any transaction boundary issues with the `EntityModel` DbContext (which is `Scoped` by default via `AddDbContext`).
- **`CatalogItemHiLoGenerator`**: The HiLo sequence (`catalog_hilo`) must exist in the SQL Server database before it can be used. Ensure the sequence is created during initial schema setup or migration.
- **Linux deployment**: The WCF service (`eShopWCFService`) targets `net10.0` and runs on Linux. The WinForms client (`eShopWinForms`, `eShopWinForms.fx`) targets `net10.0-windows` and must be deployed and run on Windows.
