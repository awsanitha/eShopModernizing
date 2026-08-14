# Migration Summary: eShopLegacyNTier → .NET 10

## Build Status
✅ `dotnet build eShopLegacyNTier.sln` — **0 errors, 0 warnings**

## Projects Migrated

| Project | From | To |
|---------|------|----|
| eShopWCFService | .NET Framework 4.6.1 (WCF web project) | net10.0 (CoreWCF ASP.NET Core) |
| eShopWinForms | .NET Framework 4.7 (WinForms) | net10.0-windows (WinForms) |

---

## eShopWCFService Changes

### Project File (`eShopWCFService.csproj`)
- Replaced legacy MSBuild XML format with SDK-style project (`Microsoft.NET.Sdk.Web`)
- Removed all `<Reference>` GAC entries and `packages.config`
- Added NuGet packages:
  - `CoreWCF.Http` 1.9.1 and `CoreWCF.Primitives` 1.9.1 (WCF server hosting)
  - `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0 (EF Core)
  - `Microsoft.EntityFrameworkCore.Design` 10.0.0 (tooling)
- Excluded `CatalogService.svc`, `CatalogServiceClient.cs`, `Web.config`, `packages.config`

### New Files
- **`Program.cs`** – CoreWCF hosting entry point (replaces IIS/web.config hosting). Configures EF Core DbContext, CoreWCF service model, and seeds the database on startup.
- **`appsettings.json`** – Replaces `Web.config`. Contains connection string and logging config.

### Source Code Changes

| File | Change |
|------|--------|
| `ICatalogService.cs` | `using System.ServiceModel` → `using CoreWCF` (server contracts must use CoreWCF namespace) |
| `CatalogService.svc.cs` | Removed `System.ServiceModel.Web` and `System.Data.Entity` imports; added `Microsoft.EntityFrameworkCore`; removed parameterless constructor (DI injects EntityModel) |
| `EntityModel.cs` | Replaced EF6 `DbContext` with EF Core: `DbContextOptions<EntityModel>` constructor, `ModelBuilder` instead of `DbModelBuilder`, removed `Database.SetInitializer` call |
| `CatalogDBInitializer.cs` | Replaced EF6 `CreateDatabaseIfNotExists<T>` with a static `Seed(EntityModel)` method called from `Program.cs` after `EnsureCreated()` |
| `CatalogConfiguration.cs` | Removed unused `System.Web` import |
| `PreconfiguredData.cs` | Removed unused `System.Web` import |
| `DiscountItem.cs` | Removed `System.Web` import |
| `CatalogItem.cs` | Removed `System.Data.Entity.Spatial` import (never used) |
| `CatalogBrand.cs` | Removed `System.Data.Entity.Spatial` import |
| `CatalogType.cs` | Removed `System.Data.Entity.Spatial` import |
| `CatalogItemsStock.cs` | Removed `System.Data.Entity.Spatial` import |

### Removed Files
- `CatalogServiceClient.cs` — Excluded from compilation; this was dead code (a WCF client proxy placed inside the server project, with uncompilable references to `System.Web` and `System.ServiceModel.ClientBase<T>` which is not available in CoreWCF server projects)

---

## eShopWinForms Changes

### Project File (`eShopWinForms.csproj`)
- Replaced legacy MSBuild format with SDK-style project (`Microsoft.NET.Sdk`)
- Target: `net10.0-windows` with `<UseWindowsForms>true</UseWindowsForms>`
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` for cross-compilation on Linux CI
- Added NuGet packages:
  - `System.ServiceModel.Http` 8.1.0 (WCF client for .NET Core)
  - `System.ServiceModel.Primitives` 8.1.0
- Excluded `Helpers\**` (UWP-specific helper files in the wrong namespace that were never in the original compile list)
- Suppressed `CA1416` (Windows platform APIs — entire project is `net10.0-windows`) and `CS0169` (unused designer fields)

### Source Code Changes

| File | Change |
|------|--------|
| `Program.cs` | Replaced `new CatalogServiceClient()` (config-file based, not supported on .NET Core) with `new CatalogServiceClient(new BasicHttpBinding(), new EndpointAddress(url))`. Endpoint URL read from `ESHOP_SERVICE_URL` environment variable, falling back to `http://localhost:62314/CatalogService.svc` |
| `Connected Services/eShopServiceReference/Reference.cs` | Fixed the three config-name `base(endpointConfigurationName, ...)` constructors that do not exist in `System.ServiceModel.Primitives`. Replaced with `base()` or `base(new BasicHttpBinding(), new EndpointAddress(...))` equivalents |

### Solution File (`eShopLegacyNTier.sln`)
- Added missing `Project(...)` declaration for `eShopWinForms` (it was only in `GlobalSection` previously)

---

## Architecture After Migration

```
eShopLegacyNTier.sln
├── eShopWCFService (net10.0)
│   ├── Hosted by Kestrel via CoreWCF (replaces IIS + .svc file)
│   ├── BasicHttpBinding endpoint at /CatalogService.svc
│   ├── EF Core 10 + SQL Server (replaces EF6)
│   └── Database seeded via CatalogDBInitializer.Seed() on startup
│
└── eShopWinForms (net10.0-windows)
    ├── WCF client via System.ServiceModel.Http
    └── Endpoint URL configurable via ESHOP_SERVICE_URL env var
```

---

## Next Steps

- **Database migrations**: EF Core migrations have not been generated (`dotnet ef migrations add InitialCreate`). The current setup uses `EnsureCreated()` which creates the schema directly — acceptable for development but migrations should be added before production deployment.
- **Connection string for production**: Update `appsettings.json` or set the `ConnectionString` environment variable (checked first) with a real SQL Server connection string.
- **CoreWCF Primitives vulnerabilities (resolved)**: Upgraded from CoreWCF 1.6.0 (had known CVEs) to 1.9.1 (latest; vulnerabilities resolved). No further action needed.
- **WinForms on non-Windows**: The WinForms project targets `net10.0-windows`. It can be compiled on Linux CI (with `EnableWindowsTargeting=true`) but must be run on Windows.
- **`CatalogServiceClient.cs` (eShopWCFService)**: This file was excluded from the build (dead code). It can be deleted from the repository in a cleanup pass.
