# eShopLegacyNTier Migration Summary

## Migration: .NET Framework 4.5–4.8 → net10.0

**Build Result:** ✅ `dotnet build eShopLegacyNTier.sln` — **0 errors, 0 blocking warnings**

---

## Projects Migrated

### 1. eShopWCFService (WCF Service)
**Before:** Legacy csproj (ToolsVersion="15.0"), targeting .NET Framework 4.6.1, EF6, System.Web

**After:** SDK-style `Microsoft.NET.Sdk.Web`, targeting `net10.0`, EF Core 9.x, CoreWCF

#### Changes Made
- **eShopWCFService.csproj** — Rewrote to SDK-style. Replaced EF6 with EF Core 9.0.7 packages. Added CoreWCF.Primitives + CoreWCF.Http. Removed legacy `<Reference>`, `<HintPath>`, `<Import>` blocks. Excluded legacy `.svc` file and WCF client proxy from compilation.
- **ICatalogService.cs** — Changed `using System.ServiceModel;` → `using CoreWCF;`. Note: CoreWCF exposes `[ServiceContract]`/`[OperationContract]` under the `CoreWCF` namespace, not `System.ServiceModel`.
- **CatalogService.svc.cs** — Removed `System.Data.Entity`, `System.ServiceModel`, `System.ServiceModel.Web` usings. Replaced with `Microsoft.EntityFrameworkCore`. Changed `EntityState.Modified` to EF Core equivalent (same API, different assembly).
- **EntityModel.cs** — Migrated EF6 `DbContext` → EF Core. Added `DbContextOptions<EntityModel>` constructor. Added `GetDefaultOptions()` static helper for direct instantiation. Removed `Database.SetInitializer` (EF6 only). Kept `OnModelCreating` with EF Core-compatible fluent API.
- **CatalogDBInitializer.cs** — Replaced EF6 `CreateDatabaseIfNotExists<T>` initializer pattern with EF Core `context.Database.EnsureCreated()` + conditional seeding.
- **CatalogConfiguration.cs** — Removed `System.Web` using. Uses inline default connection string + environment variable override.
- **PreconfiguredData.cs** — Removed `System.Web` using.
- **Models/CatalogItem.cs, CatalogBrand.cs, CatalogType.cs, CatalogItemsStock.cs, DiscountItem.cs** — Removed `System.Data.Entity.Spatial` using (EF6-only, not needed). Added nullable annotations for string properties.
- **CatalogServiceClient.cs** — Excluded from compilation (WCF client proxy does not belong in the server project; eShopWinForms has its own generated client in `Connected Services/eShopServiceReference/Reference.cs`).
- **Program.cs** (new) — Created ASP.NET Core entry point hosting CoreWCF. Wires up `AddServiceModelServices()`, `AddDbContext<EntityModel>()`, registers `CatalogService`, exposes endpoint at `/CatalogService.svc` with `BasicHttpBinding`. Seeds database on startup.
- **Properties/AssemblyInfo.cs** — Deleted (SDK-style auto-generates assembly info).
- **packages.config** — Deleted (replaced with inline PackageReference).

### 2. eShopWinForms (Windows Forms Client)
**Before:** Already SDK-style targeting `net10.0-windows`, but missing `EnableWindowsTargeting`, had UWP helper files in project, had incompatible EF6/AspNetWebApi packages, and used CoreWCF packages (server-side) instead of System.ServiceModel client packages.

**After:** Clean net10.0-windows WinForms app using System.ServiceModel client packages.

#### Changes Made
- **eShopWinForms.csproj** — Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` (required on Linux build agents). Replaced CoreWCF server packages + EF6 + Microsoft.AspNet.WebApi.Client with `System.ServiceModel.Primitives` 8.1.0 and `System.ServiceModel.Http` 8.1.0 (WCF client packages). Removed `EntityFramework`, `Microsoft.AspNet.WebApi.Client`. Added `<Compile Remove>` exclusions for UWP-only helpers.
- **Connected Services/eShopServiceReference/Reference.cs** — Removed `ClientBase<T>` constructors that take string `endpointConfigurationName` parameters — these are not available in `System.ServiceModel.Primitives` 8.x for .NET (no app.config binding resolution at runtime on modern .NET). Kept the default constructor and the `(Binding, EndpointAddress)` constructor.
- **Helpers/DependencyObjectExtensions.cs, SettingsStorageExtensions.cs, UploadImageHelper.cs, NotificationsHelper.cs, ResourceExtensions.cs** — Excluded from compilation. These are UWP-specific helpers (namespace `eShop.UWP.Helpers`) that reference `Windows.UI.Xaml`, `Windows.Storage`, `Windows.UI.Notifications`, and `Microsoft.Toolkit.Uwp.Notifications` — none of which are available or needed in a WinForms application. They are not referenced by any WinForms code.

---

## Key Migration Decisions

| Decision | Rationale |
|----------|-----------|
| EF Core 9.0.7 (not 6.5.x) | EF6 works on net10.0 but EF Core is the recommended modern path |
| CoreWCF 1.9.1 (server) + System.ServiceModel.Http 8.1.0 (client) | CoreWCF for server hosting; System.ServiceModel.* NuGet packages for WCF client proxies |
| CoreWCF namespace (not System.ServiceModel) | CoreWCF exposes service-side attributes under `CoreWCF` namespace |
| Exclude UWP helpers | Not referenced by WinForms code; UWP APIs unavailable in WinForms context |
| Remove CatalogServiceClient from server project | Server project has no need for a WCF client proxy |
| Remove string-based ClientBase constructors | `ClientBase(string endpointConfigurationName)` not available in dotnet/wcf |

---

## Next Steps

- **App.config WCF client config**: The WinForms app still has an `App.config` with `<system.serviceModel>` binding config. On .NET 10, this is read by `System.ServiceModel.Http` via the `ConfigurationManager` compatibility layer if `System.ServiceModel.Primitives` supports it — but the recommended approach is to configure the endpoint programmatically in `Program.cs` using a `BasicHttpBinding` and `EndpointAddress`. Consider updating `Program.cs` in eShopWinForms to configure the WCF endpoint explicitly rather than relying on app.config.
- **Connection string**: `CatalogConfiguration` uses `(localdb)\MSSQLLocalDB` as the default. Ensure SQL Server LocalDB is available in the target environment, or set the `ConnectionString` environment variable.
- **CoreWCF DataContractSerializer**: The `[DataContract]`/`[DataMember]` attributes on model classes use `System.Runtime.Serialization`. CoreWCF uses the same serializer — no changes needed.
- **EF Core migrations**: No EF Core migrations have been created. The current setup uses `EnsureCreated()` for database initialization. For production, set up proper EF Core migrations with `dotnet ef migrations add`.
