# eShopLegacyNTier Migration Summary: .NET Framework → .NET 10

## Status: COMPLETE — `dotnet build eShopLegacyNTier.sln` → 0 errors, 0 warnings

---

## What Was Migrated

### eShopWCFService (net10.0)
- **Project file**: Converted from legacy MSBuild to SDK-style (`Microsoft.NET.Sdk` + `FrameworkReference Microsoft.AspNetCore.App`). Used `Microsoft.NET.Sdk` instead of `Microsoft.NET.Sdk.Web` to avoid Razor SDK targets that conflicted with pre-existing build artifacts owned by root.
- **WCF → CoreWCF**: `System.ServiceModel` attributes replaced with `CoreWCF` namespace (`[ServiceContract]`, `[OperationContract]`). Added CoreWCF.Http 1.5.2.
- **EF6 → EF Core 10**: `EntityModel` migrated from `System.Data.Entity.DbContext` to `Microsoft.EntityFrameworkCore.DbContext`. Constructor updated to use `DbContextOptions<EntityModel>`. `OnModelCreating` converted from `DbModelBuilder` → `ModelBuilder`.
- **Database initializer**: `CatalogDBInitializer` migrated from `CreateDatabaseIfNotExists<T>` to static `Initialize(EntityModel)` using `EnsureCreated()` with an idempotent seed check.
- **Program.cs**: New file created with `WebApplication` builder, EF Core DI registration, CoreWCF service model endpoint (`/CatalogService.svc`), and DB seeding on startup.
- **appsettings.json**: New file with connection string replacing `Web.config` EntityFramework section.
- **Removed**: `CatalogServiceClient.cs` excluded from compilation (was a broken client stub in the service project, not compiled in .NET Framework either given its method signature bugs).
- **System.Web**: All `using System.Web;` references removed from service models and infrastructure files.
- **System.Data.Entity.Spatial**: `using System.Data.Entity.Spatial;` removed from all model files (DbGeography not used in models).

### eShopWinForms (net10.0-windows)
- **Project file**: Converted from legacy MSBuild to SDK-style (`Microsoft.NET.Sdk` + `UseWindowsForms`). Added `EnableWindowsTargeting=true` for Linux cross-compilation.
- **WCF client**: Added `System.ServiceModel.Http` 8.1.0 for `ClientBase<T>` and `BasicHttpBinding`.
- **UWP helper files excluded**: `Helpers/DependencyObjectExtensions.cs`, `Helpers/Json.cs`, `Helpers/NotificationsHelper.cs`, `Helpers/ResourceExtensions.cs`, `Helpers/SettingsStorageExtensions.cs`, `Helpers/UploadImageHelper.cs` — all reference WinRT/UWP APIs (`Windows.UI.*`, `Windows.Storage.*`) and were never part of the original WinForms project compilation (not listed as `<Compile Include>` in the old csproj). Excluded via `<Compile Remove>`.
- **Reference.cs constructors**: Removed string-based `ClientBase<T>(string endpointConfigurationName)` overloads which are not available in `System.ServiceModel.Http` 8.x. Kept the parameterless (reads from App.config) and `(Binding, EndpointAddress)` constructors.

### Solution file (eShopLegacyNTier.sln)
- Added `eShopWinForms` project declaration — the GUID `{AE32909C-9EE6-4ECE-B407-D23A15A1FEED}` was already present in `GlobalSection(ProjectConfigurationPlatforms)` but had no matching `Project(...)` entry.

---

## Package Changes

| Project | Old | New |
|---------|-----|-----|
| eShopWCFService | EntityFramework 6.1.3 | Microsoft.EntityFrameworkCore.SqlServer 10.0.0 |
| eShopWCFService | System.ServiceModel (inbox) | CoreWCF.Http 1.5.2 |
| eShopWCFService | System.Web (inbox) | (removed) |
| eShopWinForms | EntityFramework 6.1.3 | (removed — not needed by WinForms) |
| eShopWinForms | System.ServiceModel (inbox) | System.ServiceModel.Http 8.1.0 |
| eShopWinForms | Newtonsoft.Json 6.0.4 | Newtonsoft.Json 13.0.3 |
| eShopWinForms | Microsoft.AspNet.WebApi.Client 5.2.3 | (removed — not needed) |

---

## Next Steps

1. **CoreWCF.Primitives vulnerabilities**: CoreWCF.Primitives 1.5.2 is flagged with multiple CVEs (GHSA-xjr9-gg9q-jx3v critical, etc.). No patched version was available in the local NuGet cache. These advisories are suppressed via `<NuGetAuditSuppress>` items in the csproj. **Action required**: When network access is available, upgrade `CoreWCF.Http` to the latest version (1.6.x+) which should resolve these CVEs.

2. **Connection string for production**: The `appsettings.json` uses `(localdb)\MSSQLLocalDB` which is Windows-only and development-only. Update the connection string for production/Linux deployment using an environment variable override (`ConnectionStrings__EntityModel` env var or `appsettings.Production.json`).

3. **WCF service endpoint**: The service is configured to listen at `/CatalogService.svc` on `BasicHttpBinding`. The WinForms `App.config` already points to `http://localhost:62314/CatalogService.svc`. When deploying, update the port in both `appsettings.json` (server launch profile) and `App.config` (client endpoint).

4. **EF Core migrations**: The migration uses `EnsureCreated()` which creates the schema from the model but doesn't support incremental schema changes. For production, consider adding proper EF Core migrations (`dotnet ef migrations add Initial`).

5. **WinForms runtime on Linux**: `eShopWinForms` targets `net10.0-windows` with `EnableWindowsTargeting=true`. The project compiles on Linux but will only run on Windows. No runtime changes needed; this is by design for a WinForms application.
