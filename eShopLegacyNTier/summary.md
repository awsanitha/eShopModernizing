# eShopLegacyNTier Migration Summary

## Status: COMPLETE — `dotnet build` exits with 0 errors

Both projects build cleanly targeting `net10.0` / `net10.0-windows`.

---

## Changes Made

### eShopWCFService (WCF service — server)

**Project file (`eShopWCFService.csproj`)**
- Replaced legacy-format `.csproj` (ToolsVersion 15.0, targeting net461) with SDK-style `Microsoft.NET.Sdk.Web`
- Targets `net10.0`
- Replaced `EntityFramework 6.1.3` + framework `System.ServiceModel` references with:
  - `CoreWCF.Primitives 1.9.1` and `CoreWCF.Http 1.9.1` (server-side WCF)
  - `Microsoft.EntityFrameworkCore 10.0.0` and `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`
- Set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to preserve legacy `Properties/AssemblyInfo.cs`
- Deleted `packages.config`

**WCF contract (`ICatalogService.cs`)**
- Changed `using System.ServiceModel;` → `using CoreWCF;`
- `[ServiceContract]` / `[OperationContract]` now resolve from `CoreWCF` namespace (server-side requirement)

**WCF implementation (`CatalogService.svc.cs`)**
- Removed `System.Web` and `System.Data.Entity` usings
- Added `using Microsoft.EntityFrameworkCore;` (for `EntityState.Modified`)
- `EntityState` now resolves from EF Core

**Entity model (`EntityModel.cs`)**
- Replaced EF6 `DbContext` constructor `base("name=...")` pattern with EF Core pattern using `OnConfiguring`
- Removed `Database.SetInitializer(...)` call (EF6 only)
- Added `DbContextOptions<EntityModel>` constructor for DI compatibility
- Changed `using System.Data.Entity;` → `using Microsoft.EntityFrameworkCore;`

**Database initializer (`CatalogDBInitializer.cs`)**
- Replaced EF6 `CreateDatabaseIfNotExists<T>` base class with static `Initialize(EntityModel)` method
- Uses `context.Database.EnsureCreated()` + existence checks before seeding

**Model classes (`CatalogItem`, `CatalogBrand`, `CatalogType`, `CatalogItemsStock`, `DiscountItem`)**
- Removed `using System.Data.Entity.Spatial;` (type not present in EF Core)
- Removed `using System.Web;`

**Infrastructure (`CatalogConfiguration.cs`, `PreconfiguredData.cs`)**
- Removed `using System.Web;`
- `CatalogConfiguration`: now returns raw connection string instead of EF6 `"name=..."` format

**Client stub (`CatalogServiceClient.cs`)**
- Removed `using System.Web;`
- Simplified to a plain `ICatalogService` stub (not a WCF proxy — the real WCF client proxy lives in eShopWinForms)

**New file: `Program.cs`**
- Added ASP.NET Core + CoreWCF host entry point
- Registers `CatalogService` via `AddServiceModelServices()` with a `BasicHttpBinding` endpoint at `/CatalogService.svc`
- Enables `ServiceMetadataBehavior.HttpGetEnabled` in development

---

### eShopWinForms (WinForms client)

**Project file (`eShopWinForms.csproj`)**
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` to allow building on non-Windows (Linux) hosts
- Replaced `CoreWCF.*` packages (incorrectly placed by a prior partial migration) with:
  - `System.ServiceModel.Http 8.1.2` and `System.ServiceModel.Primitives 8.1.2` (client-side WCF)
- Removed `EntityFramework 6.5.2` (WinForms project does not access the database directly)
- Removed `Microsoft.AspNet.WebApi.Client` (unused)
- Excluded UWP-specific helper files from compilation (see below)

**UWP helper files excluded**
The following files used UWP-only APIs (`Windows.UI.Xaml`, `Windows.Storage`, `Windows.UI.Notifications`, `Windows.ApplicationModel.Resources`) that do not exist in WinForms on .NET 10. They are excluded via `<Compile Remove="..." />` in the project file and are not used by any active WinForms code:
- `Helpers/DependencyObjectExtensions.cs`
- `Helpers/NotificationsHelper.cs`
- `Helpers/ResourceExtensions.cs`
- `Helpers/UploadImageHelper.cs`
- `Helpers/SettingsStorageExtensions.cs`
- `Helpers/Json.cs`

**WCF proxy (`Connected Services/eShopServiceReference/Reference.cs`)**
- Fixed `CatalogServiceClient` constructors: the `string endpointConfigurationName` overloads use `ClientBase<T>(string)` which does not exist in `System.ServiceModel.Primitives` on modern .NET. Changed those overloads to use `BasicHttpBinding` + `EndpointAddress` instead (per KB guide 22-wcf-to-corewcf-migration.md).

---

## Next Steps (non-blocking)

- **Nullable warnings**: The WCF service code has ~20 CS8618/CS8603 nullable warnings (e.g. non-nullable reference properties on EF Core entities). These are warnings only and do not block the build. Recommended fix: mark string/reference properties as nullable (`string?`) or add `= null!` initializers on entity classes.
- **NU1510 warnings**: `Microsoft.CSharp` in eShopWinForms and `Microsoft.Extensions.Configuration.*` in eShopWCFService will be auto-pruned in a future SDK version; remove them to clean up.
- **WCF service endpoint URL**: The `CatalogServiceClient()` default constructor in Reference.cs uses no explicit endpoint. The `App.config` in eShopWinForms configures the endpoint as `http://localhost:62314/CatalogService.svc`. At runtime, the client will attempt to read this from config or you can pass the URL explicitly via the `(Binding, EndpointAddress)` constructor.
- **EF Core migrations**: No EF Core migration baseline has been created. Run `dotnet ef migrations add InitialCreate` from the `eShopWCFService` directory when a database is available.
- **EF Core database provider**: The current configuration uses `UseSqlServer` with a `localdb` connection string. For Linux/container deployment, consider switching to `Microsoft.EntityFrameworkCore.Sqlite` or updating the connection string to point to a SQL Server container.
- **Legacy `.svc` file**: `CatalogService.svc` is still present but no longer used (CoreWCF registers endpoints in `Program.cs`). It can be deleted.
