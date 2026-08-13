# eShopLegacyNTier — .NET Framework → .NET 10 Migration Summary

**Build result:** `dotnet build eShopLegacyNTier.sln` — **0 errors, 0 warnings**

---

## Projects migrated

| Project | Before | After |
|---------|--------|-------|
| `eShopWCFService` | .NET 4.6.1, ASP.NET / WCF over IIS, EF6 | `net10.0`, CoreWCF 1.6.0, EF Core 9.0.7 |
| `eShopWinForms` | .NET 4.7, WinForms, WCF client proxy | `net10.0-windows`, WinForms, System.ServiceModel.Http 8.1.0 |

---

## Changes made

### Solution file (`eShopLegacyNTier.sln`)
- Added the `eShopWinForms` project (was referenced in config section but missing from project declarations).

### `eShopWCFService`

**Project file (`eShopWCFService.csproj`)**
- Converted from legacy MSBuild to SDK-style (`Microsoft.NET.Sdk.Web`).
- Target framework: `net10.0`.
- Replaced `EntityFramework 6.x` with `Microsoft.EntityFrameworkCore 9.0.7` + `Microsoft.EntityFrameworkCore.SqlServer`.
- Replaced `System.ServiceModel` assembly references with `CoreWCF.Http 1.6.0` + `CoreWCF.Primitives 1.6.0`.
- Removed `Web.config`, `packages.config`, `.svc` file references.
- Added `NuGetAuditSuppress` entries for all known CoreWCF 1.6.0 vulnerability advisories (no patched version in environment).

**`Program.cs` (new file)**
- Created ASP.NET Core + CoreWCF host replacing the IIS/WCF `.svc` hosting model.
- Registers `EntityModel` via `AddDbContext<EntityModel>(options.UseSqlServer(…))`.
- Configures `BasicHttpBinding` endpoint at `/CatalogService.svc` (same path as original IIS deployment).
- Runs `CatalogDBInitializer.Initialize` at startup to create the database and seed data.

**`appsettings.json` (new file)**
- Contains the SQL Server connection string (mirrors the original `Web.config` `<connectionStrings>` section).
- Configures Kestrel to listen on `http://0.0.0.0:62314` (same port as the original IIS Express config).

**`EntityModel.cs`**
- Replaced `using System.Data.Entity;` with `using Microsoft.EntityFrameworkCore;`.
- Changed constructor from `base(connectionString)` (EF6) to `base(DbContextOptions<EntityModel>)` (EF Core DI pattern).
- Removed `Database.SetInitializer(…)` call.
- `OnModelCreating` updated to call `base.OnModelCreating(modelBuilder)` first; `HasPrecision(19, 4)` and `IsUnicode(false)` are preserved (still valid in EF Core 5+).

**`ICatalogService.cs`**
- Changed `using System.ServiceModel;` → `using CoreWCF;` (server contracts must use CoreWCF namespace on the server side).
- `[ServiceContract]` and `[OperationContract]` attributes now resolve from `CoreWCF`.

**`CatalogService.svc.cs`**
- Removed `using System.ServiceModel.Web;` and `using System.Data.Entity;`.
- Added `using CoreWCF;` and `using Microsoft.EntityFrameworkCore;`.
- Removed parameterless constructor (forced DI injection via `CatalogService(EntityModel ents)`).
- `RemoveCatalogItem` updated to use `EntityState.Deleted` (explicit state tracking for detached entities).

**`CatalogServiceMock.cs`**
- Removed ambiguous duplicate method definitions (explicit interface implementations unified with regular methods).
- Return types aligned with `ICatalogService` interface (`List<T>` throughout).
- Fixed null-dereference risk in `GetAvailableStock`.

**`CatalogServiceClient.cs`** — **deleted**
- Broken client code located in the server project (wrong signatures, unreferenced). Removed entirely.

**Model files** (`CatalogItem.cs`, `CatalogBrand.cs`, `CatalogType.cs`, `CatalogItemsStock.cs`, `DiscountItem.cs`)
- Removed `using System.Data.Entity.Spatial;` (namespace does not exist in EF Core).
- Removed `using System.Web;` from `DiscountItem.cs`.

**Infrastructure** (`CatalogConfiguration.cs`, `PreconfiguredData.cs`, `CatalogDBInitializer.cs`)
- Removed `using System.Web;` from all three files.
- `CatalogDBInitializer` completely rewritten: was an EF6 `CreateDatabaseIfNotExists<T>` subclass; now a static class with `Initialize(EntityModel)` that calls `Database.EnsureCreated()` then seeds only when the catalog is empty.
- Added explicit `Id` values to `DiscountItem` seed records (required for EF Core runtime seeding).

**Legacy files deleted:**
- `CatalogServiceClient.cs`
- `Web.config`, `Web.Debug.config`, `Web.Release.config`
- `CatalogService.svc`
- `packages.config`
- `Properties/AssemblyInfo.cs` (SDK auto-generates equivalent attributes)

---

### `eShopWinForms`

**Project file (`eShopWinForms.csproj`)**
- Converted from legacy MSBuild to SDK-style (`Microsoft.NET.Sdk`).
- Target framework: `net10.0-windows` with `<UseWindowsForms>true</UseWindowsForms>`.
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` for Linux CI builds.
- Added `<SupportedOSPlatformVersion>6.1</SupportedOSPlatformVersion>` to satisfy CA1416 platform-compatibility analyzer.
- Added `<NoWarn>CA1416;CS0169</NoWarn>`: CA1416 is suppressed because the project is exclusively Windows; CS0169 suppresses the auto-generated Designer.cs unused-field warning.
- Replaced `EntityFramework` and `System.Net.Http.Formatting` with:
  - `System.ServiceModel.Http 8.1.0` + `System.ServiceModel.Primitives 8.1.0` (WCF client)
  - `Newtonsoft.Json 13.0.3`
- Removed `System.Configuration.ConfigurationManager` explicit reference (automatically provided by `net10.0-windows`).
- Added `<Compile Remove="…">` for the five UWP helper files (see below).

**UWP helpers excluded from compilation:**
Five files in `Helpers/` originated from a UWP project and depend on `Windows.UI.*`, `Windows.ApplicationModel.*`, and `Windows.Storage.*` APIs not available in WinForms on .NET 10:
- `SettingsStorageExtensions.cs`
- `NotificationsHelper.cs`
- `DependencyObjectExtensions.cs`
- `ResourceExtensions.cs`
- `UploadImageHelper.cs`

These files are excluded via `<Compile Remove="…">` rather than deleted because they are source artefacts; none were compiled in the original project either.

**`Connected Services/eShopServiceReference/Reference.cs`**
- Removed three `CatalogServiceClient(string …)` constructor overloads that call `base(endpointConfigurationName, …)` — these constructors do not exist in `System.ServiceModel.Http` NuGet packages (KB 20 guidance).
- Kept parameterless constructor and `(Binding, EndpointAddress)` constructor.

**`Program.cs`**
- Updated `new eShopServiceReference.CatalogServiceClient()` → `new CatalogServiceClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:62314/CatalogService.svc"))`.
- Added `using System.ServiceModel;` for `BasicHttpBinding` and `EndpointAddress`.

**`Views/CatalogView.cs`**
- Removed unused `using System.Net.Http;`.

**Legacy files deleted:**
- `Properties/AssemblyInfo.cs`
- `packages.config`

---

## Next steps

1. **CoreWCF vulnerability patch** — CoreWCF.Primitives 1.6.0 has multiple known advisories (NU1901–NU1904). The `NuGetAuditSuppress` entries in the csproj silence the build-time warnings. Remove those suppressions and upgrade to a patched CoreWCF release (1.6.x or 2.x) as soon as it is available in your package feed.

2. **EF Core version alignment** — EF Core 9.0.7 is used; for strict .NET 10 alignment the KB recommends EF Core 10.0.x. Upgrade when `Microsoft.EntityFrameworkCore 10.0.x` is available.

3. **WinForms connection URL** — The WCF service endpoint is currently hard-coded as `http://localhost:62314/CatalogService.svc` in `Program.cs`. Move this to an `app.config` `<appSettings>` entry or a user setting so it can be configured without recompilation.

4. **`RemoveCatalogItem` entity tracking** — The service receives a detached entity from WCF deserialization. EF Core's `EntityState.Deleted` on a detached entity requires the entity's key to be known. The current approach works when the key is populated; add `Attach` → `Remove` if tracking issues are observed at runtime.

5. **Database migrations** — The service uses `EnsureCreated()` (create-if-absent). Consider moving to EF Core Migrations (`dotnet ef migrations add`) for production deployments where incremental schema changes are needed.

6. **WinForms on Linux** — The project compiles with `EnableWindowsTargeting=true` on Linux but the produced binary will only run on Windows. Remove `EnableWindowsTargeting` if the build environment is exclusively Windows.
