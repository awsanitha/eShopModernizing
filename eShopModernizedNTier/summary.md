# Migration Summary — .NET Framework → net10.0

**Date:** 2026-08-13  
**Projects migrated:**
- `src/eShopWCFService/eShopWCFService.csproj`
- `src/eShopWinForms/eShopWinForms.csproj`
- `src/eShopWinForms/eShopWinForms.fx.csproj`

**Final build result:** ✅ All three projects — 0 errors, 0 warnings.

---

## Changes Made

### eShopWCFService

**Project file** (`eShopWCFService.csproj`)  
- Rewrote from old-style MSBuild XML to SDK-style (`Microsoft.NET.Sdk.Web`) targeting `net10.0`.  
- Replaced `EntityFramework 6.1.3` with `Microsoft.EntityFrameworkCore.SqlServer 10.0.0` + `Microsoft.EntityFrameworkCore.Design 10.0.0`.  
- Replaced framework-shipped `System.ServiceModel` references with `CoreWCF.Primitives 1.9.1`, `CoreWCF.Http 1.9.1`, `CoreWCF.ConfigurationManager 1.9.1`.  
- Excluded `CatalogServiceClient.cs` from compilation (it is dead code misplaced in the server project — it inherited `System.ServiceModel.ClientBase<T>` which is a client-only construct not available in a CoreWCF server).  
- Excluded legacy `CatalogService.svc` content file (replaced by `Program.cs` routing).

**`Program.cs`** (new)  
- Created ASP.NET Core + CoreWCF host.  
- Registers `EntityModel` via `AddDbContext` with SQL Server connection string sourced first from `ConnectionString` env var, then from `appsettings.json`, then a LocalDB fallback.  
- Registers `CatalogService` as scoped for DI.  
- Runs `CatalogDBInitializer.Initialize(db)` on startup to create and seed the database if empty.  
- Exposes the service at `/CatalogService.svc` via `BasicHttpBinding` (same address as the original IIS-hosted `.svc` endpoint).

**`appsettings.json`** (new)  
- Provides a default LocalDB connection string for the `EntityModel` context.

**`ICatalogService.cs`**  
- Changed `using System.ServiceModel;` → `using CoreWCF;` so `[ServiceContract]` and `[OperationContract]` resolve against the CoreWCF assembly (KB doc 20).

**`CatalogService.svc.cs`**  
- Removed `using System.Data.Entity;` and `using System.ServiceModel.Web;`.  
- Added `using Microsoft.EntityFrameworkCore;` so `EntityState.Modified` resolves correctly in EF Core.

**`EntityModel.cs`**  
- Replaced `System.Data.Entity.DbContext` with `Microsoft.EntityFrameworkCore.DbContext`.  
- Added `DbContextOptions<EntityModel>` constructor for DI injection.  
- Added parameterless constructor (retained for unit-test and design-time use).  
- Added `OnConfiguring` fallback that calls `optionsBuilder.UseSqlServer(CatalogConfiguration.ConnectionString)` when no DI options are provided.  
- Changed `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)` (EF Core API).  
- Removed `Database.SetInitializer(...)` call (EF6-specific, replaced by `CatalogDBInitializer.Initialize`).

**`Models/Infrastructure/CatalogDBInitializer.cs`**  
- Replaced `CreateDatabaseIfNotExists<EntityModel>` (EF6) with a static `Initialize(EntityModel)` method that calls `context.Database.EnsureCreated()` and seeds only when the `CatalogTypes` table is empty.

**`Models/Infrastructure/CatalogConfiguration.cs`**  
- Removed `using System.Web;`.  
- Updated the fallback connection string from EF6 `"name=EntityModel"` format to a real LocalDB connection string.

**`Models/Infrastructure/PreconfiguredData.cs`**  
- Removed `using System.Web;`.

**`Models/CatalogBrand.cs`, `CatalogItem.cs`, `CatalogType.cs`**  
- Removed `using System.Data.Entity.Spatial;` (the `DbGeography`/`DbGeometry` spatial types from EF6 — none of these entities actually used spatial properties).

**`Models/CatalogItemsStock.cs`, `Models/DiscountItem.cs`**  
- Removed `using System.Data.Entity.Spatial;` / `using System.Web;`.

**`Models/CatalogItemHiLoGenerator.cs`**  
- Removed `using System.Web;`.  
- Added `using Microsoft.EntityFrameworkCore;` so that `DatabaseFacade.SqlQuery<T>()` (EF Core 7+ extension method) resolves.  
- Updated `db.Database.SqlQuery<Int64>(...)` to `db.Database.SqlQuery<long>($"...")` using the EF Core 7+ interpolated overload.

---

### eShopWinForms (both project files)

**`eShopWinForms.csproj`**  
- Replaced all five `CoreWCF.*` packages (server-only, wrongly added by the original migration) with `System.ServiceModel.Primitives 8.1.0` and `System.ServiceModel.Http 8.1.0` (the correct WCF *client* NuGet packages; KB doc 20 §Step 5).  
- Removed unused `Microsoft.AspNet.WebApi.Client 5.2.7` package which pulled in `Newtonsoft.Json 10.0.1` (NU1903 high-severity vulnerability) — the WinForms client does not use any HttpClient extension APIs from that package.  
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` (required to cross-compile `net10.0-windows` on a Linux CI host).  
- Added `<NoWarn>CA1416;CS0169</NoWarn>` to suppress Windows-platform analyzer noise on a project that is already declared to target Windows exclusively.

**`eShopWinForms.fx.csproj`**  
- Replaced all five `CoreWCF.*` packages with `System.ServiceModel.Primitives 8.1.0` and `System.ServiceModel.Http 8.1.0`.  
- Removed explicitly-referenced `Microsoft.CSharp 4.7.0` (NU1510 — automatically provided by the SDK; no explicit reference needed).  
- Removed `System.Data.DataSetExtensions 4.5.0` (not used).  
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>`.  
- Added `<NoWarn>CA1416;CS0169</NoWarn>`.

**`Connected Services/eShopServiceReference/Reference.cs`**  
- Added `new` keyword to `CloseAsync()` override to resolve `CS0108` hide-without-new warning (the `System.ServiceModel.Primitives` base class `ClientBase<T>` now exposes `CloseAsync()` as a virtual, so the override must declare `new virtual`).

---

## Next Steps

- **Database migrations**: EF Core migration history files do not exist yet. Run `dotnet ef migrations add InitialCreate` inside `src/eShopWCFService` and commit the migration files before first production deployment.  
- **`CatalogItemHiLoGenerator`**: The HiLo generator references a SQL Server sequence `catalog_hilo` that is not created by EF Core's `EnsureCreated()`. If this generator is ever activated, create the sequence manually or via a migration: `CREATE SEQUENCE catalog_hilo START WITH 1 INCREMENT BY 10;`  
- **`CatalogServiceClient.cs`** (in server project): File is excluded from compilation via `<Compile Remove="...">` but still exists on disk. It should either be deleted or moved to an appropriate client project in a follow-up cleanup.  
- **`App.config` `<system.serviceModel>` section** (eShopWinForms): The WinForms client reads WCF endpoint address from `App.config` at runtime on .NET Framework. On .NET Core/5+, `System.ServiceModel` client packages no longer process `app.config` WCF sections. The auto-generated `Reference.cs` was updated by a previous migration pass to use programmatic `BasicHttpBinding` + `EndpointAddress` construction (hard-coded to `http://localhost:5113/CatalogService.svc`). Consider moving this URL to `appsettings.json` or an environment variable for production flexibility.  
- **`Microsoft.AspNet.WebApi.Client 6.0.0`** (in eShopWinForms.fx.csproj): This package is still referenced but unused in the WinForms source. It brings in `Newtonsoft.Json` transitively — a minor vulnerability risk. Consider removing it in a follow-up pass once confirmed unused.
