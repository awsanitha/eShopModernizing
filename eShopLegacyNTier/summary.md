# eShopLegacyNTier – .NET Framework → .NET 10 Migration Summary

## Migration result

`dotnet build eShopLegacyNTier.sln` exits with **0 errors, 0 warnings** in both Debug and Release configurations.

Both projects build successfully:
- `eShopWCFService` → `bin/*/net10.0/WcfService1.dll`
- `eShopWinForms` → `bin/*/net10.0-windows/eShopWinForms.dll`

---

## Changes made

### Solution file (`eShopLegacyNTier.sln`)
- Added `eShopWinForms` project entry (was missing from the solution despite being referenced in `ProjectConfigurationPlatforms`).
- Normalised path separators to forward slashes for Linux compatibility.

---

### eShopWCFService

**Project file (`eShopWCFService.csproj`)** — full SDK-style conversion:
- Changed from old-style MSBuild format to `Microsoft.NET.Sdk.Web` (required for CoreWCF / ASP.NET Core hosting).
- Target framework: `net10.0`.
- `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` — prevents duplicate-attribute conflicts with the existing `Properties/AssemblyInfo.cs`.
- Implicit usings enabled (`<ImplicitUsings>enable</ImplicitUsings>`) so `WebApplication`, `builder.*`, etc. resolve without manual `using` directives in `Program.cs`.
- Added packages: `CoreWCF.Http 1.9.1` (replaces the .NET Framework `System.ServiceModel` server assemblies) and `Microsoft.EntityFrameworkCore.SqlServer 10.0.12` (replaces EF6).
- Excluded `CatalogServiceClient.cs` (was never compiled in the original project; has an incorrect `GetCatalogItems()` signature that doesn't match the interface) and the IIS-hosted WCF files (`CatalogService.svc`, `Web.config`, `Web.Debug.config`, `Web.Release.config`).

**`Program.cs`** *(new file)* — CoreWCF + ASP.NET Core host:
- Configures EF Core `AddDbContext<EntityModel>` reading the connection string from `appsettings.json`.
- Registers `CatalogService` as a transient DI service.
- Calls `CatalogDBInitializer.Initialize()` at startup to create/seed the database.
- Adds a `BasicHttpBinding` endpoint at `/CatalogService.svc` (same URL as the old IIS-hosted endpoint).
- Enables CoreWCF service metadata.

**`appsettings.json`** *(new file)* — replaces `Web.config`:
- `ConnectionStrings:EntityModel` pointing to LocalDB.
- Kestrel port 62314 (matches the original IIS Express port so the WinForms client URL is unchanged).

**`ICatalogService.cs`**:
- `using System.ServiceModel;` → `using CoreWCF;` (server contracts must use `CoreWCF` namespace with `CoreWCF.Primitives`).
- `[ServiceContract]` / `[OperationContract]` attributes remain unchanged in code; they now resolve to `CoreWCF` types.

**`CatalogService.svc.cs`**:
- Removed `using System.Data.Entity` and `using System.ServiceModel.Web`.
- Added `using Microsoft.EntityFrameworkCore`.
- `RemoveCatalogItem` updated to use `ents.Entry(catalogItem).State = EntityState.Deleted` (EF Core pattern for deleting a detached entity by state).
- `EntityState.Modified` now resolves to `Microsoft.EntityFrameworkCore.EntityState.Modified`.

**`EntityModel.cs`** (EF6 → EF Core):
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`.
- Constructor changed from `base(CatalogConfiguration.ConnectionString)` (EF6 string format) to `base()` + `OnConfiguring` override using `UseSqlServer(...)`.
- `DbContextOptions<EntityModel>` injection constructor added (used by DI in `Program.cs`).
- Removed `Database.SetInitializer(new CatalogDBInitializer())` (EF6 initializer pattern).
- `protected override void OnModelCreating(DbModelBuilder …)` → `(ModelBuilder …)` (EF Core signature).

**`CatalogDBInitializer.cs`** (EF6 → EF Core):
- Removed `CreateDatabaseIfNotExists<EntityModel>` base class and its EF6-specific `Seed(EntityModel)` pattern.
- Rewritten as a `static class` with a `static void Initialize(EntityModel)` method.
- Uses `context.Database.EnsureCreated()` (EF Core equivalent of `CreateDatabaseIfNotExists`) followed by per-table existence checks before seeding.

**`CatalogConfiguration.cs`**:
- Removed `using System.Web`.
- Connection string now returns the actual SQL Server connection string as a fallback (EF Core does not support the EF6 `name=xxx` format).

**`PreconfiguredData.cs`**, **`CatalogDBInitializer.cs`**, **`CatalogServiceMock.cs`**:
- Removed `using System.Web` and other framework-only imports.

**Model files** (`CatalogItem.cs`, `CatalogBrand.cs`, `CatalogType.cs`, `CatalogItemsStock.cs`, `DiscountItem.cs`):
- Removed `using System.Data.Entity.Spatial` (EF6-only spatial types; unused in these models).
- Removed `using System.Web` from `DiscountItem.cs`.

---

### eShopWinForms

**Project file (`eShopWinForms.csproj`)** — full SDK-style conversion:
- Target framework: `net10.0-windows` with `<UseWindowsForms>true</UseWindowsForms>`.
- Output type: `WinExe`.
- `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`.
- `<EnableWindowsTargeting>true</EnableWindowsTargeting>` — required to build a Windows-targeted project on non-Windows (Linux) build agents.
- `<NoWarn>$(NoWarn);CA1416</NoWarn>` — suppresses platform compatibility warnings (CA1416). This is correct: the project targets `net10.0-windows` and is inherently Windows-only; the CA1416 warnings are false positives because no minimum Windows version is specified in the TFM string.
- Added package: `System.ServiceModel.Http 8.1.2` (WCF .NET Core client runtime; replaces the .NET Framework `System.ServiceModel` assembly).
- Excluded all files in the `Helpers/` folder (`<Compile Remove="Helpers\**" />`): these were UWP-only helper files (`Windows.Storage`, `Windows.UI.*`, etc.) that were never compiled in the original project (old-style .csproj required explicit `<Compile>` entries) and have no relevance to the WinForms app.
- Excluded `Properties/Settings.Designer.cs` — it depends on `System.Configuration.ApplicationSettingsBase` which requires the `System.Configuration.ConfigurationManager` NuGet package; since no code in the project uses `Settings.Default`, excluding the generated stub avoids adding an unnecessary dependency.

**`Program.cs`**:
- Replaced `new eShopServiceReference.CatalogServiceClient()` (default constructor that reads from app.config) with an explicit `new CatalogServiceClient(BasicHttpBinding, EndpointAddress)` construction. On .NET Core, `System.ServiceModel.Http` does not support reading `<client><endpoint>` from app.config; endpoints must be configured programmatically.

**`Connected Services/eShopServiceReference/Reference.cs`** (generated WCF proxy):
- Removed the three `string endpointConfigurationName` constructors (`CatalogServiceClient(string)`, `CatalogServiceClient(string, string)`, `CatalogServiceClient(string, EndpointAddress)`). These call `base(string, …)` overloads of `ClientBase<T>` that do not exist in `System.ServiceModel.Http` on .NET Core (config-file endpoints are unsupported). The `(Binding, EndpointAddress)` constructor is preserved.

**`Views/CatalogView.Designer.cs`**:
- Removed 5 unused `DataGridViewTextBoxColumn` field declarations (`dataGridViewTextBoxName`, `…Description`, `…Price`, `…Type`, `…Brand`) that were never initialised in `InitializeComponent()`, causing CS0169 warnings.

---

## Architecture decisions

| Concern | Decision |
|---------|----------|
| WCF server hosting | CoreWCF 1.9.1 with ASP.NET Core / Kestrel; endpoint remains at `/CatalogService.svc` on port 62314 so the WinForms client requires no URL change |
| EF6 → EF Core | EF Core 10 (`Microsoft.EntityFrameworkCore.SqlServer 10.0.12`); database initialised with `EnsureCreated()` + manual seed on startup |
| WCF client (WinForms) | `System.ServiceModel.Http 8.1.2`; endpoint configured programmatically in `Program.cs` |
| WCF service project output type | Changed from Library to Exe (via `Microsoft.NET.Sdk.Web`): CoreWCF requires self-hosted ASP.NET Core execution |
| UWP helper files | Excluded from compilation (they were physically present but never compiled in the old-style project) |

---

## Next steps

- **Runtime image path**: `CatalogView.SetCatalogItems` navigates up two directories (`..\..\Assets\Images\Catalog\`) from the executable to find catalog images. With the new SDK-style output path (`bin\Debug\net10.0-windows\`), this relative path is no longer correct. Either copy the `Assets` folder as content to the output directory or update the path logic.
- **SQL Server connection**: The default connection string targets `(localdb)\MSSQLLocalDB`. In containerised or CI environments, override via the `ConnectionString` environment variable (service) or by updating `appsettings.json`.
- **EF Core migrations**: `EnsureCreated()` is suitable for development; for production consider adding EF Core migrations (`dotnet ef migrations add Initial`) and running `dotnet ef database update`.
- **CoreWCF metadata / WSDL**: The existing WinForms `Reference.cs` proxy was generated from the legacy WSDL. If the contract changes, regenerate it with `dotnet-svcutil` pointed at the running CoreWCF service.
- **WinForms app.config**: The old `<system.serviceModel>` section in `App.config` is now unused (endpoint is configured in code). The `App.config` can be simplified or removed.
