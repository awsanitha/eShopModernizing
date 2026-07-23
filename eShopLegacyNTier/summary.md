# eShopLegacyNTier Migration Summary
## .NET Framework 4.6.1 / 4.7 → .NET 10

**Build status:** ✅ `dotnet build eShopLegacyNTier.sln` — 0 errors, 0 warnings

---

## Projects Migrated

### eShopWCFService (net4.6.1 → net10.0)
WCF web service hosting the catalog data via Entity Framework.

### eShopWinForms (net4.7 → net10.0-windows)
Windows Forms desktop client consuming the WCF service.

---

## Changes Made

### Solution File (`eShopLegacyNTier.sln`)
- Added `eShopWinForms` project declaration — the project existed on disk but was missing from the solution `Project(...)` section (its GUID was referenced in configuration but not declared).

### eShopWCFService

#### `eShopWCFService.csproj`
- Replaced legacy XML-heavy csproj with SDK-style (`Microsoft.NET.Sdk.Web`)
- Target framework: `net10.0`
- Package upgrades:
  - `EntityFramework 6.1.3` → `Microsoft.EntityFrameworkCore 9.0.0` + `Microsoft.EntityFrameworkCore.SqlServer 9.0.0`
  - `System.ServiceModel` (framework) → `CoreWCF.Http 1.9.1` + `CoreWCF.Primitives 1.9.1`
- `CatalogServiceClient.cs` excluded from compilation (`<Compile Remove="CatalogServiceClient.cs" />`): this was dead code in the server project — a client proxy that was never used server-side and had broken implementations (wrong method signatures, stale `System.Web` dependency).
- `CatalogService.svc` excluded from Content (routing is done in `Program.cs`)

#### `Program.cs` (new)
- Created ASP.NET Core + CoreWCF host using `WebApplication.CreateBuilder`
- Registers `EntityModel` via `AddDbContext` with EF Core SQL Server provider
- Configures CoreWCF endpoint: `BasicHttpBinding` at `/CatalogService.svc` (preserves original URL)
- Exposes service metadata (`ServiceMetadataBehavior.HttpGetEnabled = true`)
- Runs EF Core `EnsureCreated()` + `CatalogDBSeeder.Seed()` on startup

#### `appsettings.json` (new)
- Migrated connection string from `Web.config` `<connectionStrings>` section
- Kestrel configured to listen on port 62314 (matches original IIS Express port)

#### `ICatalogService.cs`
- `using System.ServiceModel` → `using CoreWCF`
- `[ServiceContract]` / `[OperationContract]` now resolved from `CoreWCF` namespace (server-side rule)

#### `CatalogService.svc.cs`
- Removed `using System.ServiceModel.Web`, `using System.Data.Entity`
- Added `using Microsoft.EntityFrameworkCore` for `EntityState.Modified`
- Constructor changed from parameterless to DI-injected `(EntityModel ents)` — aligns with ASP.NET Core DI

#### `EntityModel.cs`
- `System.Data.Entity.DbContext` → `Microsoft.EntityFrameworkCore.DbContext`
- Constructor: `base(connectionString)` (EF6) → `base(DbContextOptions<EntityModel>)` (EF Core DI)
- `Database.SetInitializer(...)` removed — EF Core does not use initializers
- `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)` (EF Core)
- `IsUnicode(false)` and `HasPrecision(19, 4)` retained — both valid in EF Core

#### `CatalogDBInitializer.cs`
- Replaced EF6 `CreateDatabaseIfNotExists<EntityModel>` pattern with a static `CatalogDBSeeder` helper
- Uses idempotent `if (!context.X.Any()) seed()` pattern to avoid duplicate seeding
- Called from `Program.cs` inside a scoped service scope after `EnsureCreated()`

#### `Models/CatalogBrand.cs`, `CatalogItem.cs`, `CatalogType.cs`, `CatalogItemsStock.cs`
- Removed `using System.Data.Entity.Spatial` (EF6-only, not used by any property)
- Added nullable annotations (`string?`, `CatalogType?`, `CatalogBrand?`) for EF Core / C# 10+ compatibility

#### `Models/DiscountItem.cs`
- Removed `using System.Web` (not used by the class)
- Added explicit `Id` values to `PreconfiguredData.GetPreconfiguredDiscountItems()` so seeder can insert deterministically

#### `Models/Infrastructure/CatalogConfiguration.cs`, `PreconfiguredData.cs`
- Removed `using System.Web` (not used by either class)

#### `CatalogServiceMock.cs`
- Fixed CS8603 / CS8602 / CS8600 nullable reference warnings
- Fixed null-dereference in `GetAvailableStock` (used `?.AvailableStock ?? 0` pattern)

### eShopWinForms

#### `eShopWinForms.csproj`
- Replaced legacy XML csproj with SDK-style (`Microsoft.NET.Sdk`)
- Target framework: `net10.0-windows` with `<UseWindowsForms>true</UseWindowsForms>`
- `<EnableWindowsTargeting>true</EnableWindowsTargeting>` to allow cross-compile on Linux CI
- `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to avoid CS0579 duplicate attribute conflict with `Properties/AssemblyInfo.cs`
- Package additions:
  - `System.ServiceModel.Http 8.1.0` — WCF client transport
  - `System.ServiceModel.Primitives 8.1.0` — WCF client contracts
  - `Newtonsoft.Json 13.0.3` — retained for future use
- Packages removed (built into `net10.0-windows` platform):
  - `System.Drawing.Common` — unnecessary, included in platform
  - `System.Configuration.ConfigurationManager` — unnecessary, included in platform
- Excluded all UWP helper files from compilation (`Helpers/*.cs`) — these use `Windows.Storage`, `Windows.UI.Xaml`, and `Windows.ApplicationModel` APIs that are not available in WinForms on .NET 10. They have namespace `eShop.UWP.Helpers` and were never referenced by any WinForms code.

#### `Program.cs`
- Changed WCF client instantiation from config-based (no-arg constructor) to explicit `BasicHttpBinding` + `EndpointAddress`:
  ```csharp
  var binding = new BasicHttpBinding();
  var endpoint = new EndpointAddress("http://localhost:62314/CatalogService.svc");
  ICatalogService service = new eShopServiceReference.CatalogServiceClient(binding, endpoint);
  ```
- Reason: `System.ServiceModel.Http` NuGet does not support `app.config`-based endpoint resolution reliably on .NET 10.

#### `Connected Services/eShopServiceReference/Reference.cs`
- Removed three unsupported constructors from `CatalogServiceClient` that called `base(string endpointConfigurationName)` overloads — these do not exist in `System.ServiceModel.Primitives` NuGet's `ClientBase<T>`.
- Retained: no-arg constructor and `(Binding, EndpointAddress)` constructor.

#### `App.config`
- Retained for reference; the `<system.serviceModel>` client configuration is no longer used at runtime (replaced by explicit binding in `Program.cs`).

---

## Architecture After Migration

```
┌─────────────────────────────────────────────────────┐
│  eShopWinForms (net10.0-windows / WinForms)          │
│  WCF client via System.ServiceModel.Http 8.1.0      │
│  BasicHttpBinding → http://localhost:62314/...svc   │
└────────────────────────┬────────────────────────────┘
                         │ SOAP / basicHttp
┌────────────────────────▼────────────────────────────┐
│  eShopWCFService (net10.0 / ASP.NET Core + CoreWCF) │
│  CoreWCF 1.9.1 — BasicHttpBinding endpoint          │
│  EF Core 9.0 — SQL Server (LocalDB by default)      │
│  Database auto-created + seeded on first startup    │
└─────────────────────────────────────────────────────┘
```

---

## Next Steps

- **CoreWCF — GHSA-xjr9-gg9q-jx3v vulnerability in old 1.6/1.7 packages** — already resolved by upgrading to 1.9.1.
- **Database migrations**: The migration uses `EnsureCreated()` for simplicity (matches the original EF6 `CreateDatabaseIfNotExists` behavior). For production, replace with proper EF Core migrations (`dotnet ef migrations add InitialCreate && dotnet ef database update`).
- **WinForms Helpers/**: The six UWP helper files (`UploadImageHelper.cs`, `SettingsStorageExtensions.cs`, `DependencyObjectExtensions.cs`, `ResourceExtensions.cs`, `Singleton.cs`, `Json.cs`, `NotificationsHelper.cs`) appear to be copied from a UWP companion project. They are excluded from compilation. If any UWP-style functionality is needed in the WinForms client, these should be rewritten using WinForms/Win32 equivalents.
- **CatalogServiceClient.cs** in eShopWCFService is excluded from compilation. It was dead code containing a broken WCF client proxy inside the server project. If a programmatic client is needed within the service project, consider a dedicated client project or using `HttpClient` directly.
- **Service endpoint URL**: Hardcoded to `http://localhost:62314/CatalogService.svc` in WinForms `Program.cs`. For production, read this from configuration (e.g., `appsettings.json` or environment variable).
