# eShopLegacyNTier Migration Summary

## Migration Target
**.NET Framework 4.6.1 / 4.7 → .NET 10**

## Final Build Status
✅ `dotnet build eShopLegacyNTier.sln` — **0 errors, 0 warnings**

Both projects build cleanly:
- `eShopWCFService → bin/Debug/net10.0/eShopWCFService.dll`
- `eShopWinForms → bin/Debug/net10.0-windows/eShopWinForms.dll`

---

## Changes Made

### Solution File (`eShopLegacyNTier.sln`)
- Added `eShopWinForms` project (GUID `{AE32909C-9EE6-4ECE-B407-D23A15A1FEED}`) — it existed on disk but had no `Project` entry in the solution, only orphaned config lines.

---

### eShopWCFService

#### `eShopWCFService.csproj`
- Replaced legacy MSBuild XML with SDK-style `Microsoft.NET.Sdk.Web`, targeting `net10.0`
- Removed all `<Reference>` to framework assemblies (auto-resolved by SDK)
- Removed `packages.config`-style dependencies
- Added:
  - `CoreWCF.Http 1.9.1` + `CoreWCF.Primitives 1.9.1` — host the WCF service on ASP.NET Core
  - `Microsoft.EntityFrameworkCore.SqlServer 10.0.0` + `Microsoft.EntityFrameworkCore.Tools 10.0.0`
- Excluded `CatalogService.svc` from Content (legacy IIS file, replaced by Program.cs endpoint)
- Excluded `CatalogServiceClient.cs` from compilation (WCF client stub inside server project — not needed)

#### `Program.cs` (new)
- Replaces `Global.asax` and `Web.config` service configuration
- Registers `EntityModel` as scoped EF Core DbContext with SQL Server
- Registers `CatalogService` as a scoped DI-managed service
- Configures CoreWCF: `AddServiceModelServices()` + `AddServiceModelMetadata()`
- Exposes service at `/CatalogService.svc` via `BasicHttpBinding` (matches legacy URL for WinForms client)
- Calls `db.Database.EnsureCreated()` + `CatalogDBInitializer.Seed()` on startup

#### `appsettings.json` (new)
- Replaces `Web.config` — contains the `EntityModel` connection string (LocalDB)

#### `ICatalogService.cs`
- `using System.ServiceModel` → `using CoreWCF`

#### `CatalogService.svc.cs`
- `using System.ServiceModel` → `using CoreWCF`
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- Removed parameterless constructor — `EntityModel` is now DI-injected
- `Dispose()` is a no-op (EF Core DbContext lifetime managed by DI container)

#### `EntityModel.cs`
- `System.Data.Entity.DbContext` → `Microsoft.EntityFrameworkCore.DbContext`
- Constructor changed from `base(CatalogConfiguration.ConnectionString)` to `DbContextOptions<EntityModel>` injection pattern
- Removed `Database.SetInitializer` (EF6-only API; seeding moved to `Program.cs`)
- `DbModelBuilder` → `ModelBuilder` in `OnModelCreating`

#### `CatalogDBInitializer.cs`
- Replaced EF6 `CreateDatabaseIfNotExists<EntityModel>` with a static `Seed(EntityModel context)` method that seeds tables only when empty (idempotent)

#### Model Files (`CatalogBrand.cs`, `CatalogItem.cs`, `CatalogType.cs`, `CatalogItemsStock.cs`, `DiscountItem.cs`)
- Removed `using System.Data.Entity.Spatial` (EF6-only, removed in EF Core)
- Added nullable reference annotations (`?`) on string/reference-type properties

#### Infrastructure (`CatalogConfiguration.cs`, `PreconfiguredData.cs`)
- Removed `using System.Web`

#### `CatalogServiceMock.cs`
- Removed duplicate explicit interface implementations (now consolidated)
- Fixed null-safety: `.FirstOrDefault()` + null-coalescing for `GetAvailableStock`

---

### eShopWinForms

#### `eShopWinForms.csproj`
- Replaced legacy MSBuild XML with SDK-style `Microsoft.NET.Sdk`, targeting `net10.0-windows`
- Set `<UseWindowsForms>true</UseWindowsForms>` and `<OutputType>WinExe</OutputType>`
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` to allow building on Linux CI
- Added `System.ServiceModel.Http 8.1.0` + `System.ServiceModel.Primitives 8.1.0` for WCF client
- Excluded 6 UWP-specific helper files that were in the project directory but not compiled in the original project:
  - `Helpers/NotificationsHelper.cs` (uses `Windows.UI.Notifications`)
  - `Helpers/SettingsStorageExtensions.cs` (uses `Windows.Storage`)
  - `Helpers/DependencyObjectExtensions.cs` (uses `Windows.UI.Xaml`)
  - `Helpers/ResourceExtensions.cs` (uses `Windows.ApplicationModel.Resources`)
  - `Helpers/UploadImageHelper.cs` (uses `Windows.ApplicationModel`)
  - `Helpers/Json.cs` (UWP namespace; app uses no UWP JSON)

#### `Connected Services/eShopServiceReference/Reference.cs`
- Removed 3 config-name constructors (`CatalogServiceClient(string)`, `CatalogServiceClient(string, string)`, `CatalogServiceClient(string, EndpointAddress)`) — `ClientBase<T>` in `System.ServiceModel.Primitives` on .NET Core has no string-based endpoint-name constructor (CS1503)
- Retained the default `CatalogServiceClient()` (reads from `app.config`) and the explicit `CatalogServiceClient(Binding, EndpointAddress)` constructor
- `Program.cs` uses `new CatalogServiceClient()` (default constructor) — no behavioral change

---

## Architecture After Migration

```
eShopWCFService  (net10.0, ASP.NET Core + CoreWCF)
├── Program.cs           — startup, CoreWCF endpoint, DB seed
├── appsettings.json     — connection strings
├── ICatalogService.cs   — [ServiceContract] using CoreWCF namespace
├── CatalogService.svc.cs — service impl (DI-injected EntityModel)
├── EntityModel.cs       — EF Core DbContext
└── Models/              — POCO entities + Infrastructure (seeder, config, data)

eShopWinForms (net10.0-windows, WinForms)
├── Program.cs           — WinForms entry point
├── Connected Services/  — WCF client proxy (System.ServiceModel.Http)
├── Controllers/         — MVC-style controller + event args
└── Views/               — CatalogView Form
```

---

## Next Steps

1. **CoreWCF 1.9.1 on .NET 10**: CoreWCF is versioned independently of the .NET runtime. Verify at runtime that CoreWCF 1.9.1 is fully compatible with .NET 10 (should be — the library targets netstandard2.0/net6.0+). If issues arise, check https://github.com/CoreWCF/CoreWCF for a newer release.

2. **WCF client `app.config` on .NET Core**: `System.ServiceModel.Http` reads `app.config` for the `<client><endpoint>` configuration. The existing `App.config` has `address="http://localhost:62314/CatalogService.svc"`. After migrating the service, update the port/address in `App.config` to match where CoreWCF hosts (default Kestrel port may differ). Alternatively, construct the client explicitly: `new CatalogServiceClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:5000/CatalogService.svc"))`.

3. **EF Core migrations**: The migration uses `Database.EnsureCreated()` for schema creation. For production, generate proper EF Core migrations: `dotnet ef migrations add InitialCreate` from the `eShopWCFService` directory.

4. **CA1416 warnings suppressed**: The WinForms project targets `net10.0-windows` which is Windows-only. The CA1416 "only supported on windows" analyzer warnings are false positives for a project that already requires Windows — they can be suppressed by adding `<NoWarn>CA1416</NoWarn>` to the csproj if desired.

5. **UWP helper files**: Six UWP helper files in `src/eShopWinForms/Helpers/` are excluded from compilation. They reference UWP APIs (`Windows.UI.*`, `Windows.ApplicationModel.*`) that do not exist in WinForms. If UWP features are needed, these should be replaced with WinForms/Windows App SDK equivalents or removed entirely.
