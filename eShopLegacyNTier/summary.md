# eShopLegacyNTier — .NET Framework → .NET 10 Migration Summary

## Result

`dotnet build eShopLegacyNTier.sln` → **Build succeeded. 0 Error(s). 0 Warning(s).**

---

## Projects Migrated

| Project | Before | After |
|---|---|---|
| eShopWCFService | .NET 4.6.1 · old-style csproj · EF6 · WCF/IIS | net10.0 · SDK-style · EF Core 10 · CoreWCF 1.9.1 |
| eShopWinForms | .NET 4.7 · old-style csproj · WCF client via App.config | net10.0-windows · SDK-style · System.ServiceModel.Http 8.1.0 |

The solution file was also updated to include `eShopWinForms` as a proper Project entry (it was only in the configuration section).

---

## Changes Made

### Solution File (`eShopLegacyNTier.sln`)
- Added `eShopWinForms` Project entry (it existed only in the configuration section).

### `eShopWCFService/eShopWCFService.csproj`
- Replaced entire old-style csproj with SDK-style targeting `net10.0` using `Microsoft.NET.Sdk.Web`.
- Replaced EF6 NuGet references with `Microsoft.EntityFrameworkCore 10.0.0` + `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`.
- Replaced `System.ServiceModel` framework references with `CoreWCF.Http 1.9.1` + `CoreWCF.Primitives 1.9.1`.
- Added explicit `System.Security.Cryptography.Pkcs 10.0.14` to override the vulnerable transitive dependency pulled in by CoreWCF.
- Excluded: `CatalogService.svc`, `Web.config`, `Web.Debug.config`, `Web.Release.config`, `packages.config`, `CatalogServiceClient.cs` (misplaced WCF client proxy).
- Added `GenerateAssemblyInfo=false` to avoid conflicts with existing `Properties/AssemblyInfo.cs`.

### `eShopWCFService/Program.cs` *(new)*
- Created ASP.NET Core host file that bootstraps CoreWCF, registers the EF Core DbContext and `CatalogService` via DI, runs `EnsureCreated` + `CatalogDBInitializer.Seed`, and registers the SOAP endpoint at `/CatalogService.svc`.

### `eShopWCFService/appsettings.json` *(new)*
- Added JSON configuration containing the SQL Server connection string (previously in `Web.config`).

### `eShopWCFService/ICatalogService.cs`
- Changed `using System.ServiceModel;` → `using CoreWCF;` (server contracts must use the `CoreWCF` namespace on the server side).

### `eShopWCFService/CatalogService.svc.cs`
- Removed `using System.Data.Entity;`, `using System.ServiceModel;`, `using System.ServiceModel.Web;`.
- Added `using Microsoft.EntityFrameworkCore;` for `EntityState`.
- Removed the no-argument constructor (replaced by DI constructor injection in CoreWCF).

### `eShopWCFService/EntityModel.cs`
- Replaced EF6 `DbContext` constructor `base(connectionString)` + `Database.SetInitializer(...)` with EF Core `DbContextOptions<EntityModel>` pattern.
- Changed `DbModelBuilder` → `ModelBuilder` in `OnModelCreating`.
- Removed `using System.Data.Entity;` and related EF6 using statements.

### `eShopWCFService/Models/Infrastructure/CatalogConfiguration.cs`
- Removed `using System.Web;`.
- Changed connection string helper to return the raw SQL Server connection string directly (EF Core takes the full connection string rather than the EF6 `name=…` reference).

### `eShopWCFService/Models/Infrastructure/CatalogDBInitializer.cs`
- Replaced EF6 `CreateDatabaseIfNotExists<EntityModel>` initializer with a static `CatalogDBInitializer.Seed(EntityModel context)` method that idempotently seeds each table.

### `eShopWCFService/Models/Infrastructure/PreconfiguredData.cs`
- Removed `using System.Web;`.

### `eShopWCFService/Models/CatalogItem.cs`, `CatalogBrand.cs`, `CatalogType.cs`, `CatalogItemsStock.cs`
- Removed `using System.Data.Entity.Spatial;` (not needed; spatial type was never actually used in these models).

### `eShopWCFService/Models/DiscountItem.cs`
- Removed `using System.Web;`.

### `eShopWinForms/eShopWinForms.csproj`
- Replaced old-style csproj with SDK-style targeting `net10.0-windows` with `UseWindowsForms=true` and `EnableWindowsTargeting=true`.
- Added `System.ServiceModel.Http 8.1.0` + `System.ServiceModel.Primitives 8.1.0` (WCF client NuGet packages for .NET 10).
- Added `Newtonsoft.Json 13.0.3` (used by the Json helper retained in the project).
- Excluded all 7 UWP-specific Helpers files (`UploadImageHelper.cs`, `SettingsStorageExtensions.cs`, `DependencyObjectExtensions.cs`, `NotificationsHelper.cs`, `ResourceExtensions.cs`, `Singleton.cs`, `Json.cs`) — these use `Windows.Storage`, `Windows.UI`, and `Windows.ApplicationModel` UWP APIs that do not compile under WinForms on .NET 10, and none of them are referenced by any WinForms code.
- Suppressed `CA1416` (Windows-platform false-positives for a Windows-only WinForms app) and `CS0169` (unused fields in auto-generated Designer code).
- Added `GenerateAssemblyInfo=false`.

### `eShopWinForms/Program.cs`
- Replaced the default `CatalogServiceClient()` (config-file based) constructor call with `new CatalogServiceClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:62314/CatalogService.svc"))`. The System.ServiceModel NuGet client packages on .NET 10 do not support config-file endpoints.

### `eShopWinForms/Connected Services/eShopServiceReference/Reference.cs`
- Removed the three string-based `CatalogServiceClient` constructor overloads (`string endpointConfigurationName`, `string + string`, `string + EndpointAddress`) that call `base(string)` — those overloads do not exist in the `System.ServiceModel` NuGet client packages.

---

## Architecture After Migration

```
eShopWCFService (net10.0, Kestrel + CoreWCF)
  └── Exposes SOAP endpoint: http://localhost:<port>/CatalogService.svc
  └── EF Core 10 → SQL Server (LocalDB by default)
  └── Seeded on first start via CatalogDBInitializer

eShopWinForms (net10.0-windows, WinForms)
  └── Connects to SOAP service via System.ServiceModel.Http
  └── Uses explicit BasicHttpBinding + EndpointAddress
```

---

## Next Steps

1. **CoreWCF service URL configuration**: The WinForms client hard-codes `http://localhost:62314/CatalogService.svc`. Consider reading this from `appsettings.json` or an environment variable in a follow-up pass.
2. **Connection string**: The SQL Server connection string defaults to LocalDB (`(localdb)\MSSQLLocalDB`). For production, inject via the `ConnectionString` environment variable or update `appsettings.json`.
3. **HTTPS / TLS**: The CoreWCF service currently runs HTTP only. Add `HttpsBinding` and TLS configuration for production deployments.
4. **Docker/Linux**: `eShopWinForms` is Windows-only (WinForms). `eShopWCFService` can be containerized on Linux (CoreWCF + EF Core are cross-platform). See `19-linux-containerization.md`.
5. **Database migrations**: EF Core `EnsureCreated` is used for simplicity. For production, generate proper EF Core migrations with `dotnet ef migrations add`.
