# Migration Summary: eShopLegacyNTier → .NET 10

## Result
`dotnet build eShopLegacyNTier.sln` exits with **0 errors, 0 warnings**.

Both projects build successfully:
- `eShopWCFService` → `net10.0`
- `eShopWinForms` → `net10.0-windows`

---

## Changes Made

### Solution File (`eShopLegacyNTier.sln`)
- Added `eShopWinForms` as a proper `Project(...)` block (it had build configurations but was missing the declaration).

### eShopWCFService (WCF + EF6 → CoreWCF + EF Core)

**Project file (`eShopWCFService.csproj`)**
- Converted from legacy non-SDK to `Microsoft.NET.Sdk.Web` SDK-style
- Target: `net10.0`
- Replaced `EntityFramework 6.1.3` → `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`
- Replaced `System.ServiceModel` framework assemblies → `CoreWCF.Http 1.9.1` + `CoreWCF.Primitives 1.9.1`
- Added `System.Security.Cryptography.Pkcs 10.0.14` to override vulnerable transitive dependency
- `GenerateAssemblyInfo=false` to prevent conflict with existing `Properties/AssemblyInfo.cs`
- `ImplicitUsings=enable` for ASP.NET Core host boilerplate in `Program.cs`
- Excluded `CatalogServiceClient.cs` (leftover stub with mismatched interface; incompatible server-side WCF client code)
- Excluded `Web.config`, transform files, and `CatalogService.svc` content

**New: `Program.cs`**
- Created ASP.NET Core host with CoreWCF middleware
- `AddDbContext<EntityModel>` with SQL Server provider; connection string sourced from env var → `appsettings.json` fallback
- `AddScoped<CatalogService>()` for DI
- `UseServiceModel` configures `BasicHttpBinding` endpoint at `/CatalogService.svc`
- Seeds the database at startup via `CatalogDBInitializer.Seed()`

**New: `appsettings.json`**
- Connection string `EntityModel` pointing to LocalDB

**`ICatalogService.cs`**
- Changed `using System.ServiceModel` → `using CoreWCF` for server-side contract attributes

**`CatalogService.svc.cs`**
- Changed `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- Removed `using System.ServiceModel` and `using System.ServiceModel.Web`
- Removed parameterless constructor (called `new EntityModel()` which is no longer valid without DI options; DI uses the `CatalogService(EntityModel)` constructor)

**`EntityModel.cs`**
- Changed `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- Constructor changed from `base(connectionString)` pattern to `DbContextOptions<EntityModel>` for DI
- `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)` (EF Core parameter type)
- Removed `Database.SetInitializer()` call (EF6-only API)

**`Models/Infrastructure/CatalogDBInitializer.cs`**
- Replaced `CreateDatabaseIfNotExists<EntityModel>` base class (EF6-only) with a static `Seed(EntityModel context)` method
- Uses `context.Database.EnsureCreated()` + idempotent seed checks

**`Models/Infrastructure/CatalogConfiguration.cs`**
- Removed `using System.Web` and unused `configConnectionName` field
- Kept env-variable override logic (used by `Program.cs`)

**`Models/Infrastructure/PreconfiguredData.cs`**
- Removed `using System.Web` (unused)

**All Model files (`CatalogBrand`, `CatalogItem`, `CatalogType`, `CatalogItemsStock`, `DiscountItem`)**
- Removed `using System.Data.Entity.Spatial` (not available in EF Core)
- Removed `using System.Web` from `DiscountItem`

**`CatalogServiceMock.cs`**
- Removed unused imports (`System.Web`, `System.Text`, `System.Threading.Tasks`)

---

### eShopWinForms (WinForms client)

**Project file (`eShopWinForms.csproj`)**
- Converted from legacy non-SDK to `Microsoft.NET.Sdk` SDK-style with `<UseWindowsForms>true</UseWindowsForms>`
- Target: `net10.0-windows`
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` for Linux cross-compilation
- Removed `EntityFramework 6.1.3` (unused in WinForms client)
- Removed `Microsoft.AspNet.WebApi.Client` (unused)
- Replaced framework-shipped `System.ServiceModel` → `System.ServiceModel.Primitives 8.1.0` + `System.ServiceModel.Http 8.1.0`
- Upgraded `Newtonsoft.Json` from 6.0.4 → 13.0.3
- Excluded `Helpers\**` (UWP/WinRT helper stubs from a template that reference `Windows.UI`, `Windows.Storage` etc., not applicable to WinForms)

**`Program.cs`**
- Replaced `new CatalogServiceClient()` (config-file endpoint, unsupported on .NET 10 `System.ServiceModel`) with explicit `BasicHttpBinding` + `EndpointAddress` constructor

**`Connected Services/eShopServiceReference/Reference.cs`**
- Removed config-name string constructors (`CatalogServiceClient(string)`, `CatalogServiceClient(string, string)`, `CatalogServiceClient(string, EndpointAddress)`) which are not supported on `System.ServiceModel.Primitives`
- Kept only the `(Binding, EndpointAddress)` constructor

---

## Next Steps
- **CoreWCF metadata endpoint**: The `ServiceMetadataBehavior.HttpGetEnabled = true` is set but WSDL at `?wsdl` should be verified at runtime.
- **WinForms on Linux runtime**: `net10.0-windows` compiles on Linux but the WinForms app can only run on Windows.
- **Database connectivity**: LocalDB is Windows-only; for Linux/container deployment, update the connection string to a full SQL Server instance.
- **EF Core migrations**: The `EnsureCreated()` approach in `CatalogDBInitializer` creates the schema from scratch. For production, consider using EF Core Migrations (`dotnet ef migrations add`).
- **`CatalogServiceClient.cs`** (in eShopWCFService): This stub file was excluded from compilation as it had stale, mismatched method signatures. It should be deleted or moved to a dedicated test/integration project.
- **Helpers in eShopWinForms**: The UWP helper files (`DependencyObjectExtensions.cs`, `NotificationsHelper.cs`, etc.) are excluded from build. They should either be deleted or replaced with WinForms-appropriate equivalents if the functionality is needed.
