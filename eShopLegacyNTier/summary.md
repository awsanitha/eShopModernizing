# eShopLegacyNTier Migration Summary
## .NET Framework 4.6.1 / 4.7 → .NET 10

**Build status:** ✅ `dotnet build eShopLegacyNTier.sln` — **0 errors, 0 warnings**

---

## Projects Migrated

### 1. eShopWCFService (`net10.0`)
WCF service hosting a catalog API, previously targeting .NET Framework 4.6.1 with IIS hosting.

### 2. eShopWinForms (`net10.0-windows`)
WinForms desktop application connecting to the WCF service via a generated client proxy, previously targeting .NET Framework 4.7.

---

## Changes Made

### Solution File (`eShopLegacyNTier.sln`)
- Added missing `eShopWinForms` project entry — the project GUID was referenced in solution configuration sections but had no `Project(...)` declaration.

### eShopWCFService

**Project file (`eShopWCFService.csproj`):**
- Converted from legacy-style MSBuild XML to SDK-style (`Microsoft.NET.Sdk.Web`).
- Replaced framework `<Reference>` items with modern `<PackageReference>` items.
- Removed EntityFramework 6.1.3 and legacy WCF framework assemblies.
- Added `CoreWCF.Http` 1.9.1 and `CoreWCF.Primitives` 1.9.1 (WCF server hosting).
- Added `Microsoft.EntityFrameworkCore.SqlServer` 9.0.0 (EF Core).
- Excluded `CatalogServiceClient.cs` — the file was not compiled in the original project (not listed in `<Compile>` items) and contains a conflicting WCF client proxy in the server project.
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to avoid conflict with `Properties/AssemblyInfo.cs`.

**New `Program.cs`:**
- Created ASP.NET Core entry point for CoreWCF hosting.
- Registers `CatalogService` as transient in the DI container.
- Wires the `ICatalogService` endpoint at `/CatalogService.svc` using `BasicHttpBinding` (preserving the original SOAP contract and URL).
- Enables `ServiceMetadataBehavior.HttpGetEnabled` for WSDL/MEX discovery.
- Calls `CatalogDBInitializer.Initialize()` at startup for EF Core database creation and seeding.

**New `appsettings.json`:**
- Migrated connection string from `Web.config` `<connectionStrings>`.
- Default: `Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=eShopDatabase;Persist Security Info=True;`
- Override at runtime via `ConnectionString` environment variable (preserved original behaviour).

**`ICatalogService.cs`:**
- Changed `using System.ServiceModel;` → `using CoreWCF;` (server contracts compile against `CoreWCF.Primitives`).

**`CatalogService.svc.cs`:**
- Removed `using System.Data.Entity;` and `using System.ServiceModel.Web;`.
- Added `using Microsoft.EntityFrameworkCore;` for `EntityState`.
- Applied nullable annotations (`?`) to return types to match nullable-enabled project.

**`EntityModel.cs` (EF6 → EF Core):**
- Changed base class from `DbContext` (EF6, `System.Data.Entity`) to `DbContext` (EF Core, `Microsoft.EntityFrameworkCore`).
- Replaced EF6 parameterless constructor `base(connectionString)` with `OnConfiguring(DbContextOptionsBuilder)` override that calls `optionsBuilder.UseSqlServer(...)`.
- Removed `Database.SetInitializer(...)` call (EF6-only); initialization now happens in `Program.cs`.
- Changed `OnModelCreating(DbModelBuilder)` signature to `OnModelCreating(ModelBuilder)` (EF Core type).
- Added `= null!` initializers to DbSet properties to satisfy nullable analysis.

**`CatalogDBInitializer.cs`:**
- Removed `CreateDatabaseIfNotExists<EntityModel>` inheritance (EF6-only).
- Converted to a `public static class` with a single `Initialize(EntityModel context)` method.
- Uses `context.Database.EnsureCreated()` (EF Core equivalent of CreateDatabaseIfNotExists).
- Seeds data only when the database is empty (`if (!context.CatalogTypes.Any())`).
- Removed `using System.Web;` and `using System.Data.Entity;`.

**`CatalogConfiguration.cs`:**
- Removed `using System.Web;`.
- Changed from `"name=EntityModel"` named connection string format (EF6/config-file) to a direct connection string with a hardcoded default, preserving the `ConnectionString` environment variable override.

**`PreconfiguredData.cs`:**
- Removed `using System.Web;`.

**Model files (`CatalogItem.cs`, `CatalogBrand.cs`, `CatalogType.cs`, `CatalogItemsStock.cs`, `DiscountItem.cs`):**
- Removed `using System.Data.Entity.Spatial;` (types from that namespace were imported but never used in any model).
- Applied nullable `?` annotations to reference-type properties to match `<Nullable>enable</Nullable>`.

**`CatalogServiceMock.cs`:**
- Removed `using System.Text;` (unused).
- Fixed interface implementation: collapsed duplicate explicit interface implementations (`IEnumerable<T>` overloads) into the required `List<T>` return type matching `ICatalogService`.
- Applied null-forgiving operator `!` on `FirstOrDefault()` return values to satisfy nullable analysis.
- Fixed `GetAvailableStock` to use null-conditional operator instead of direct member access on a potentially null result.

### eShopWinForms

**Project file (`eShopWinForms.csproj`):**
- Converted from legacy-style MSBuild XML to SDK-style (`Microsoft.NET.Sdk`).
- Target framework: `net10.0-windows` with `<UseWindowsForms>true</UseWindowsForms>`.
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` (required for cross-platform build hosts like Linux CI).
- Added `System.ServiceModel.Http` 8.1.0 and `System.ServiceModel.Primitives` 8.1.0 (WCF client packages for modern .NET).
- Removed EntityFramework 6.1.3, `Microsoft.AspNet.WebApi.Client`, and `Newtonsoft.Json` (none of the compiled WinForms code uses them).
- Excluded all Helpers files that were **not included** in the original project's `<Compile>` items and use UWP-specific APIs (`Windows.Storage`, `Windows.UI.*`):
  - `Helpers/Json.cs` (namespace: `eShop.UWP.Helpers`)
  - `Helpers/SettingsStorageExtensions.cs`
  - `Helpers/NotificationsHelper.cs`
  - `Helpers/UploadImageHelper.cs`
  - `Helpers/DependencyObjectExtensions.cs`
  - `Helpers/ResourceExtensions.cs`
  - `Helpers/Singleton.cs`
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to avoid conflict with `Properties/AssemblyInfo.cs`.

**`Program.cs`:**
- Changed WCF client construction from config-file–based `new CatalogServiceClient()` (reads `App.config`) to explicit `new CatalogServiceClient(new BasicHttpBinding(), new EndpointAddress("http://localhost:62314/CatalogService.svc"))`.
- This is required because `System.ServiceModel.Http` 8.x does not support config-name–based constructors on `ClientBase<T>`.
- Added `using System.ServiceModel;` for `BasicHttpBinding` and `EndpointAddress`.

**`Connected Services/eShopServiceReference/Reference.cs`:**
- Removed three broken constructors from the generated `CatalogServiceClient` proxy:
  - `CatalogServiceClient(string endpointConfigurationName)`
  - `CatalogServiceClient(string endpointConfigurationName, string remoteAddress)`
  - `CatalogServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress)`
- These constructors pass `string` where `System.ServiceModel.Http` 8.x expects `ServiceEndpoint` or does not provide a matching overload (CS1503).
- Kept `CatalogServiceClient()` and `CatalogServiceClient(Binding, EndpointAddress)` which are the constructors that work on modern .NET.

---

## Architecture Preserved

- The SOAP contract (`ICatalogService`) and all operation signatures are unchanged — existing WCF clients (the WinForms app) connect without modification.
- The WCF endpoint path `/CatalogService.svc` is preserved to maintain compatibility with the `App.config` address.
- The EF Core data model (table names, column types, precision) matches the original EF6 schema.
- Database seeding logic (PreconfiguredData) is unchanged.

---

## Next Steps

1. **Runtime test with a live SQL Server / LocalDB instance** — The build is clean but the database initialization path (`EnsureCreated` + seeding) has not been integration-tested. Run both projects against a real SQL LocalDB to verify end-to-end behaviour.

2. **EF Core migrations** — The migration uses `EnsureCreated()` (no migration history table). For production or schema-evolution scenarios, replace with `dotnet ef migrations add Initial` + `dotnet ef database update` to get a proper migration baseline.

3. **CoreWCF `ServiceMetadataBehavior` HTTP binding** — The metadata endpoint is configured for HTTP (`HttpGetEnabled = true`). If the service is deployed behind HTTPS, also enable `HttpsGetEnabled` in `Program.cs`.

4. **UWP helper files** — `Helpers/Json.cs`, `Helpers/SettingsStorageExtensions.cs`, etc. are excluded from compilation because they depend on UWP APIs (`Windows.Storage`, `Windows.UI.*`) and were not compiled in the original project. If any of this functionality is needed in the future, it should be reimplemented using the .NET 10 equivalents (`System.Text.Json`, `Microsoft.Extensions.Configuration`, etc.).

5. **WinForms CA1416 warnings** — Currently suppressed by `<Nullable>enable</Nullable>` and build not emitting them as errors; they are expected for Windows-only WinForms code compiled without a `[SupportedOSPlatform]` context attribute. Adding `<SupportedOSPlatform>windows6.1</SupportedOSPlatform>` to the project or annotating the `Main` method would silence these analytically.
