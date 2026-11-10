# Migration Summary: eShopLegacyNTier — .NET Framework → .NET 10

## Status: ✅ BUILD SUCCEEDED — 0 errors, 0 warnings

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Projects migrated

| Project | Original TFM | New TFM | Output |
|---------|-------------|---------|--------|
| eShopWCFService | net461 (web app) | net10.0 (ASP.NET Core + CoreWCF) | WcfService1.dll |
| eShopWinForms | net47 (WinExe) | net10.0-windows (WinForms) | eShopWinForms.dll |

---

## Changes made

### Solution file (`eShopLegacyNTier.sln`)
- Added `eShopWinForms` project back to the solution (it was present in `ProjectConfigurationPlatforms` but missing as a `Project` entry).

---

### eShopWCFService

#### Project file (eShopWCFService.csproj)
- Replaced legacy verbose MSBuild format with **SDK-style** `Microsoft.NET.Sdk.Web`
- Target: `net10.0`
- **EF6 → EF Core 10**: Replaced `EntityFramework` 6.1.3 with `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0
- **WCF → CoreWCF**: Replaced framework `System.ServiceModel` with `CoreWCF.Http` 1.9.1 and `CoreWCF.Primitives` 1.9.1
- Excluded `CatalogServiceClient.cs` and `Properties/AssemblyInfo.cs` (SDK auto-generates)
- Pinned `System.Security.Cryptography.Pkcs` 10.0.14 to resolve transitive vulnerability from CoreWCF 1.9.1

#### New files
- **`Program.cs`** — ASP.NET Core host setup with CoreWCF service registration, EF Core DbContext DI, database initialization
- **`appsettings.json`** — Connection string for SQL Server (replaces `Web.config`)

#### EntityModel.cs (EF6 → EF Core)
- Constructor changed from `base(CatalogConfiguration.ConnectionString)` to `DbContextOptions<EntityModel>` injection
- Removed `Database.SetInitializer(new CatalogDBInitializer())` (EF6-only API)
- Updated `OnModelCreating` to EF Core fluent API style (lambda delegates)
- Removed `using System.Data.Entity`; added `using Microsoft.EntityFrameworkCore`

#### ICatalogService.cs
- Replaced `using System.ServiceModel` → `using CoreWCF` (server-side service contracts use CoreWCF namespace)
- `[ServiceContract]` and `[OperationContract]` now resolve from `CoreWCF`

#### CatalogService.svc.cs
- Replaced `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- Replaced `using System.ServiceModel` → `using CoreWCF`
- Updated constructor to use injected `EntityModel` only (DI-driven, no parameterless constructor)
- Added nullable annotations (`?`, `!`) for C# nullable reference types

#### CatalogDBInitializer.cs
- Removed `EF6 CreateDatabaseIfNotExists<T>` base class
- Converted to `static` class with `SeedIfEmpty(EntityModel context)` method (checks before inserting)
- Removed `using System.Web` and `using System.Data.Entity`

#### Model files (CatalogItem, CatalogBrand, CatalogType, CatalogItemsStock, DiscountItem)
- Removed `using System.Data.Entity.Spatial` from all model files (not in EF Core)
- Removed `using System.Web` from DiscountItem.cs
- Added nullable annotations to string and reference-type properties

#### CatalogServiceMock.cs, PreconfiguredData.cs, CatalogConfiguration.cs
- Removed `using System.Web` (not needed, no HttpContext usage)
- Added nullable safety improvements

---

### eShopWinForms

#### Project file (eShopWinForms.csproj)
- Replaced legacy verbose MSBuild format with **SDK-style** `Microsoft.NET.Sdk`
- Target: `net10.0-windows` (WinForms requires Windows TFM)
- Added `<UseWindowsForms>true</UseWindowsForms>`
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` (allows building on Linux CI)
- **WCF client**: Replaced framework `System.ServiceModel` with `System.ServiceModel.Http` 8.1.0 + `System.ServiceModel.Primitives` 8.1.0
- Added `Newtonsoft.Json` 13.0.3 for Helpers/Json.cs
- Excluded UWP-specific helper files from compilation (never compiled originally, not referenced):
  - `Helpers/SettingsStorageExtensions.cs` (uses `Windows.Storage`)
  - `Helpers/NotificationsHelper.cs` (uses `Windows.UI.Notifications`)
  - `Helpers/UploadImageHelper.cs` (uses `Windows.ApplicationModel`)
  - `Helpers/DependencyObjectExtensions.cs` (uses `Windows.UI.Xaml`)
  - `Helpers/ResourceExtensions.cs` (uses `Windows.ApplicationModel.Resources`)

#### Connected Services Reference.cs
- Replaced all `base(endpointConfigurationName)` string-based constructors (not supported on .NET 10 `System.ServiceModel.Primitives`) with:
  - New parameterless constructor using `new BasicHttpBinding(), new EndpointAddress(DefaultEndpointAddress)`
  - `DefaultEndpointAddress` reads from `CATALOG_SERVICE_URL` environment variable, falls back to `http://localhost:62314/CatalogService.svc`

#### CatalogView.cs
- Made `_controller` and event fields nullable (`CatalogController?`, event delegates as `?`)
- Replaced `filterChanged.Invoke(...)` with null-safe `filterChanged?.Invoke(...)` pattern
- Fixed `BindingSource` calls to use `string.Empty` instead of `null` for dataMember parameter
- Added null guard on `listBox1.SelectedItem` in `searchAvailabilityButton_Click`

#### ICatalogView.cs
- Event types made nullable (`event ViewHandler<ICatalogView>?`) to match CatalogView implementation

#### CatalogController.cs
- Added nullable annotation for `DiscountItem?` return values
- Used null-safe `.?` on Brand and Type string properties

#### CatalogView.Designer.cs
- Added `#pragma warning disable CS0169` / `#pragma warning restore CS0169` around 5 stale `DataGridViewTextBoxColumn` fields that were never assigned (pre-existing designer artifacts)

#### Helpers/Json.cs
- Updated `ToObjectAsync<T>` return type to `T?` to reflect nullable deserialization result

#### Program.cs
- Removed unused `eShopWinForms.eShopServiceReference` alias-qualified reference

---

## Architectural decisions

| Decision | Rationale |
|----------|-----------|
| WCF server → CoreWCF (not REST API) | Preserves existing SOAP contract; WinForms client can connect without changes at the protocol level |
| EF6 → EF Core 10 | EF6 has no .NET 10 support; EF Core 10 is the matching version for net10.0 |
| `net10.0-windows` for WinForms | WinForms requires Windows TFM; `EnableWindowsTargeting=true` allows CI builds on Linux |
| DB initialization via `EnsureCreated` + `SeedIfEmpty` | Replaces EF6 `CreateDatabaseIfNotExists` initializer with equivalent EF Core pattern |
| UWP helper files excluded | These files (`eShop.UWP.Helpers` namespace) were never in the original .csproj compile list and use UWP APIs unavailable in WinForms |

---

## Next steps

- **SQL Server availability**: The app requires SQL Server (LocalDB by default). Update `appsettings.json` in `eShopWCFService` with the production connection string before deploying.
- **EF Core migrations**: Consider running `dotnet ef migrations add InitialCreate` to generate a formal migration history if the database schema will evolve.
- **WinForms runtime**: `eShopWinForms` targets `net10.0-windows` and can only run on Windows. The build is cross-platform, but the binary requires Windows at runtime.
- **UWP helper files**: The 5 excluded helper files (`Helpers/*.cs` using `Windows.*` APIs) are not used by the WinForms application. They can be safely deleted if they are not needed for a future UWP port.
- **CoreWCF service endpoint URL**: The WinForms client defaults to `http://localhost:62314/CatalogService.svc`. Set the `CATALOG_SERVICE_URL` environment variable to point at the actual service host in non-local environments.
- **System.ServiceModel.Http version**: Currently pinned at 8.1.0 (latest stable). Watch for 10.x releases aligned with .NET 10 if behavior differences are observed.
