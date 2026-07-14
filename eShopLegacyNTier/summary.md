# eShopLegacyNTier Migration Summary

## Migration Result
✅ `dotnet build eShopLegacyNTier.sln` — **0 errors**, 14 warnings (all NuGet vulnerability advisories for CoreWCF.Primitives 1.6.0, not compilation errors)

## Target Framework
- **eShopWCFService**: `net10.0` (ASP.NET Core web host via `Microsoft.NET.Sdk.Web`)
- **eShopWinForms**: `net10.0-windows` (Windows Forms, `EnableWindowsTargeting=true` for cross-platform build)

---

## Changes Made

### Solution File (`eShopLegacyNTier.sln`)
- Added missing `eShopWinForms` project entry (it existed on disk but was not declared in the solution)

### eShopWCFService Project

**`eShopWCFService.csproj`** — Converted from legacy MSBuild format to SDK-style:
- Target: `net10.0` with `Microsoft.NET.Sdk.Web`
- Packages: `CoreWCF.Primitives 1.6.0`, `CoreWCF.Http 1.6.0`, `EntityFramework 6.5.1`
- Excluded `CatalogService.svc` (legacy IIS hosting) and `CatalogServiceClient.cs` (WCF client stub in server project)
- `GenerateAssemblyInfo=false` to avoid CS0579 duplicate attribute conflicts with legacy `AssemblyInfo.cs`

**`Program.cs`** — New file replacing IIS/SVC hosting:
- ASP.NET Core + CoreWCF minimal hosting
- Registers `CatalogService` via DI, exposes endpoint at `/CatalogService.svc` on `BasicHttpBinding`
- ServiceMetadata enabled for HTTP GET

**`ICatalogService.cs`** — Server contract:
- `using System.ServiceModel;` → `using CoreWCF;` (server contracts use CoreWCF namespace)

**`CatalogService.svc.cs`** — Service implementation:
- `using System.ServiceModel.Web;` removed
- `using CoreWCF;` added (kept `System.Data.Entity` for EF6)

**`EntityModel.cs`** — Removed unused `System.Data.Entity.Spatial` import

**`Models/CatalogItem.cs`, `CatalogBrand.cs`, `CatalogType.cs`, `CatalogItemsStock.cs`** — Removed `System.Data.Entity.Spatial` import

**`Models/DiscountItem.cs`** — Removed `System.Web` import

**`Models/Infrastructure/CatalogConfiguration.cs`** — Removed `System.Web` import

**`Models/Infrastructure/PreconfiguredData.cs`** — Removed `System.Web` import; added explicit `Id` values to `DiscountItem` seed records

**`Models/Infrastructure/CatalogDBInitializer.cs`** — Removed `System.Web` import

### eShopWinForms Project

**`eShopWinForms.csproj`** — Converted to SDK-style:
- Target: `net10.0-windows`, `UseWindowsForms=true`, `OutputType=WinExe`
- `EnableWindowsTargeting=true` for building on Linux CI
- Packages: `System.ServiceModel.Http 8.1.0`, `Newtonsoft.Json 13.0.3`
- Excluded all `Helpers/*.cs` files (UWP/WinRT APIs: `Windows.Storage`, `Windows.UI.Xaml`, etc. — not compiled and not supported on WinForms)
- `GenerateAssemblyInfo=false`

**`Program.cs`** — Updated WCF client construction:
- Replaced config-name constructor (`new CatalogServiceClient("BasicHttpBinding_ICatalogService")`) with explicit binding + address constructor: `new CatalogServiceClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:62314/CatalogService.svc"))`
- Config-name constructors are not supported in `System.ServiceModel.Http` on .NET 10

**`Connected Services/eShopServiceReference/Reference.cs`** — Removed unsupported constructors:
- Removed `CatalogServiceClient(string endpointConfigurationName)` 
- Removed `CatalogServiceClient(string endpointConfigurationName, string remoteAddress)`
- Removed `CatalogServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress)`
- Kept parameterless ctor and `CatalogServiceClient(Binding, EndpointAddress)`

---

## Key Migration Decisions

| Decision | Rationale |
|---|---|
| **EF6 6.5.1 (not EF Core)** | The `EntityModel`/`DbContext` uses EF6-specific APIs (`Database.SetInitializer`, `CreateDatabaseIfNotExists<T>`, `DbModelBuilder`). Migrating to EF Core would require rewriting the initializer, seeding, and fluent config. EF6 6.5.1 runs on .NET 10 and is the safer path per migration guidance. |
| **CoreWCF server** | WCF server hosting (`System.ServiceModel`) does not run on .NET 10; CoreWCF is the official port. |
| **System.ServiceModel.Http client** | WCF client (`ClientBase<T>`) available via the modern `System.ServiceModel.Http` NuGet package; source unchanged except removing config-name constructors. |
| **Exclude UWP Helpers** | Files in `eShopWinForms/Helpers/` use `Windows.Storage`, `Windows.UI.Xaml`, `Windows.UI.Notifications` (UWP/WinRT APIs not available on WinForms). These were not in the original `<Compile>` items of the old project file and are excluded. |

---

## Next Steps

- **CoreWCF.Primitives 1.6.0 vulnerability warnings**: Consider upgrading to a later patch version if one becomes available without CVEs. The warnings do not block the build.
- **EF6 → EF Core (future work)**: The `CatalogDBInitializer` / `CreateDatabaseIfNotExists<T>` pattern is EF6-only. A future migration to EF Core 10 would require converting to `DbContext.Database.EnsureCreated()` + `HasData()` seeding in `OnModelCreating`, and updating `DbModelBuilder` → `ModelBuilder`.
- **appsettings.json for connection string**: The connection string is still read from the environment variable `ConnectionString` or falls back to `"name=EntityModel"` (EF6 config-file format). For a production deployment, add an `appsettings.json` with the `ConnectionStrings:EntityModel` key and update `CatalogConfiguration` to use `IConfiguration`.
- **WinForms runtime on Linux**: `net10.0-windows` WinForms apps require Windows at runtime. The `EnableWindowsTargeting=true` flag allows compilation on Linux but the app cannot run there.
- **CatalogServiceClient.cs (WCF service project)**: This file was a WCF client stub living inside the server project. It has been excluded from compilation. If a server-to-service call pattern is needed in future, implement it as a separate client project or use `HttpClient`.
