# Migration Summary — .NET Framework → net10.0

## Status: ✅ BUILD SUCCEEDED — Zero Errors, Zero Warnings

All three upgraded projects compile cleanly against net10.0 / net10.0-windows:

| Project | Target | Result |
|---|---|---|
| `src/eShopWCFService/eShopWCFService.csproj` | `net10.0` | ✅ 0 errors, 0 warnings |
| `src/eShopWinForms/eShopWinForms.csproj` | `net10.0-windows` | ✅ 0 errors, 0 warnings |
| `src/eShopWinForms/eShopWinForms.fx.csproj` | `net10.0-windows` | ✅ 0 errors, 0 warnings |

---

## Changes Made

### eShopWCFService

**Project file (`eShopWCFService.csproj`)**
- Replaced legacy non-SDK `<Project ToolsVersion="15.0" xmlns=...>` with SDK-style `<Project Sdk="Microsoft.NET.Sdk.Web">`
- Targeting `net10.0`
- Replaced old `<Reference Include="EntityFramework" />` (EF6, via packages.config / HintPath) with EF Core 10 PackageReference: `Microsoft.EntityFrameworkCore.SqlServer` + `Microsoft.EntityFrameworkCore.Design`
- Replaced all `<Reference Include="System.ServiceModel*" />` framework assemblies with `CoreWCF.Primitives`, `CoreWCF.Http`, `CoreWCF.ConfigurationManager` (CoreWCF v1.9.1)
- Removed all legacy `<Reference Include="System.Web*" />` and `<Reference Include="System.Data.Entity*" />` framework references
- Removed `packages.config` dependency entirely (SDK-style PackageReference used instead)
- Excluded `CatalogServiceClient.cs` from compilation (legacy dead code left in server project, was never compiled in original project; incompatible System.Web dependency)
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to keep existing `Properties/AssemblyInfo.cs`

**Program.cs (new)**
- Replaces `Web.config`'s `<system.serviceModel>` hosting with ASP.NET Core + CoreWCF pipeline
- Wires up `DbContext` via `AddDbContext<EntityModel>` using connection string from `appsettings.json` or `ConnectionString` environment variable
- Registers `CatalogService` as a scoped service for CoreWCF DI injection
- Calls `db.Database.EnsureCreated()` + `CatalogDBInitializer.Seed()` at startup
- Exposes `/CatalogService.svc` endpoint with `BasicHttpBinding`
- Enables service metadata (`httpGetEnabled`)

**appsettings.json / appsettings.Development.json (new)**
- Migrated connection string from `Web.config` `<connectionStrings>` section
- Default: `(localdb)\MSSQLLocalDB;Initial Catalog=eShopDatabase`

**EntityModel.cs**
- Migrated `System.Data.Entity.DbContext` (EF6) → `Microsoft.EntityFrameworkCore.DbContext` (EF Core 10)
- Constructor changed from `base(CatalogConfiguration.ConnectionString)` → `DbContextOptions<EntityModel>` pattern for DI compatibility
- `OnModelCreating` parameter changed from `DbModelBuilder` → `ModelBuilder`
- EF Core fluent API preserved: `HasPrecision(19,4)`, `IsUnicode(false)`, table mapping
- `Database.SetInitializer` call removed (EF Core has no initializer concept; seeding moved to Program.cs)

**CatalogService.svc.cs**
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- `using System.ServiceModel` → `using CoreWCF`
- Parameterless constructor removed; only DI constructor `(EntityModel ents)` retained
- `EntityState.Modified` continues to work (same type in EF Core)
- Nullable annotations added throughout (`?`, `!`)

**ICatalogService.cs**
- `using System.ServiceModel` → `using CoreWCF`
- `[ServiceContract]` / `[OperationContract]` attributes now resolve from `CoreWCF` namespace

**Model files (CatalogBrand, CatalogItem, CatalogType, CatalogItemsStock, DiscountItem)**
- Removed `using System.Data.Entity.Spatial` (unused in all models — no DbGeography/DbGeometry types)
- Removed `using System.Web` from DiscountItem.cs
- Added nullable annotations (`string?`, `CatalogType?`, `CatalogBrand?`) for EF Core nullable-reference compatibility

**CatalogItemHiLoGenerator.cs**
- `db.Database.SqlQuery<Int64>(...)` (EF6) → `db.Database.SqlQueryRaw<long>(...)` (EF Core 7+)
- Removed `using System.Web`

**CatalogConfiguration.cs**
- Removed `using System.Web`
- Renamed `ConnectionString` property to `ConnectionStringKey` (returns config key name for use with `IConfiguration`)
- Added `EnvironmentConnectionString` property that returns the env variable override

**CatalogDBInitializer.cs**
- Removed inheritance from `CreateDatabaseIfNotExists<EntityModel>` (EF6 concept, no EF Core equivalent)
- Converted to a `static` class with a `Seed(EntityModel context)` method
- Added guards (`if (!context.X.Any())`) so seeding is idempotent and only runs on first use
- Removed `using System.Data.Entity` and `using System.Web`

**PreconfiguredData.cs**
- Removed `using System.Web` and `using System.Linq`

**CatalogServiceMock.cs**
- Fixed three nullable warnings: added `!` null-forgiving operators on `FirstOrDefault()` calls where the original code assumed non-null results

---

### eShopWinForms (eShopWinForms.csproj)

**Package changes**
- Removed: `EntityFramework` 6.5.2 — WinForms project does not use EF directly; it communicates via WCF service
- Removed: `Microsoft.AspNet.WebApi.Client` — not used by the WinForms application  
- Removed: all `CoreWCF.*` packages — client projects must NOT use CoreWCF (it is server-only)
- Added: `System.ServiceModel.Primitives` 8.1.0 — WCF client runtime
- Added: `System.ServiceModel.Http` 8.1.0 — provides `BasicHttpBinding` for WCF client

**Project properties**
- Removed `<UseWPF>true</UseWPF>` — app is WinForms only
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` — required when building Windows-targeted project on Linux CI
- Added `<NoWarn>CA1416;CS0169</NoWarn>` — suppresses false-positive platform-compatibility warnings on Windows-only project, and unused-field warnings in auto-generated Designer.cs

**App.config**
- Removed `<configSections>` for EntityFramework (would throw `TypeLoadException` at runtime without the EF6 DLL)
- Removed `<startup>` element referencing .NET Framework 4.7.1
- Removed `<entityFramework>` config section
- Retained `<system.serviceModel>` client endpoint configuration and DPI-awareness setting

**Connected Services / Reference.cs**
- Added `new` keyword to `CloseAsync()` override to fix `CS0108` hide-without-new warning

---

### eShopWinForms.fx.csproj

Same package corrections as `eShopWinForms.csproj`:
- Removed `EntityFramework`, `Microsoft.AspNet.WebApi.Client`, `Microsoft.CSharp`, `System.ComponentModel.Annotations`, `System.Data.DataSetExtensions`, and all `CoreWCF.*` packages
- Added `System.ServiceModel.Primitives` 8.1.0, `System.ServiceModel.Http` 8.1.0, `System.Configuration.ConfigurationManager` 10.0.0
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` and `<NoWarn>CA1416;CS0169</NoWarn>`
- Removed deployment/ClickOnce-era metadata properties not relevant to SDK-style .NET 10 builds

---

## Architecture Notes

**WCF Server (eShopWCFService)**
The service is now hosted as an ASP.NET Core application using CoreWCF. The `CatalogService.svc` file and `Web.config` `<system.serviceModel>` section have been fully replaced by `Program.cs`. The endpoint path `/CatalogService.svc` is preserved for backward compatibility with existing clients.

**EF Core**
`EntityModel` now requires `DbContextOptions<EntityModel>` (injected by ASP.NET Core DI). The EF6 `Database.SetInitializer` / `CreateDatabaseIfNotExists` pattern is replaced by `EnsureCreated()` + `CatalogDBInitializer.Seed()` at startup.

**WCF Client (eShopWinForms)**
The generated `Reference.cs` proxy (`CatalogServiceClient`) uses `System.ServiceModel.ClientBase<T>` from the `System.ServiceModel.*` NuGet packages — the correct choice for .NET 10 WCF clients. The endpoint URL is hardcoded in the generated proxy (`http://localhost:5113/CatalogService.svc`), matching the original App.config.

---

## Next Steps

- **Database migration**: If targeting a pre-existing schema, run `dotnet ef migrations add InitialCreate` and `dotnet ef database update` instead of relying solely on `EnsureCreated()`.
- **Connection string**: For production/container deployments, set the `ConnectionString` environment variable to override the `appsettings.json` default.
- **HTTPS**: The CoreWCF endpoint currently uses plain HTTP. For production, add `BasicHttpsBinding` and configure TLS termination.
- **Dockerfile**: The WCF service Dockerfile may need updating to base image `mcr.microsoft.com/dotnet/aspnet:10.0` (was `aspnet:4.6.1`).
- **`CatalogItemHiLoGenerator`**: The `SqlQueryRaw<long>` call requires SQL Server's `catalog_hilo` sequence to exist. Ensure the sequence is created in a migration.
- **`CatalogServiceClient.cs`** in eShopWCFService: This file was excluded from compilation. It can be deleted if the code is confirmed unnecessary.
