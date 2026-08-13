# Migration Summary — eShopLegacyNTier → net10.0

## Final build result
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## What was migrated

### Solution (`eShopLegacyNTier.sln`)
- Added `eShopWinForms` project entry (it existed on disk but was missing from the solution file).

---

### `eShopWCFService` — .NET Framework 4.6.1 WCF web service → net10.0 CoreWCF web app

**Project file (`eShopWCFService.csproj`)**
- Replaced legacy MSBuild-style project file with SDK-style `Microsoft.NET.Sdk.Web`.
- Target framework: `net10.0`.
- Excluded legacy files (`CatalogServiceClient.cs`, `CatalogService.svc.cs`, `CatalogService.svc`, Web transform files) that have no equivalent in CoreWCF hosting.
- Added packages:
  - `CoreWCF.Primitives 1.9.1` / `CoreWCF.Http 1.9.1` — WCF server hosting
  - `Microsoft.EntityFrameworkCore 10.0.0` / `.SqlServer 10.0.0` / `.Design 10.0.0` — EF Core

**New `Program.cs`** (replaces `Global.asax` + IIS/WAS hosting)
- ASP.NET Core host with CoreWCF middleware.
- Registers `EntityModel` via `AddDbContext<EntityModel>` with the connection string from `appsettings.json` (or `ConnectionString` env var override).
- Registers `CatalogService` as `AddScoped<CatalogService>()`.
- Calls `db.Database.EnsureCreated()` + `CatalogDBInitializer.Seed()` at startup.
- Exposes the service at `/CatalogService.svc` via `BasicHttpBinding` (same URL path as legacy).

**New `appsettings.json`** (replaces `Web.config`)
- Connection string `EntityModel` pointing to `(localdb)\MSSQLLocalDB;Initial Catalog=eShopDatabase`.

**`ICatalogService.cs`**
- Changed `using System.ServiceModel` → `using CoreWCF` (server-side attributes now from CoreWCF namespace).

**`CatalogService.cs`** (new file, replaces `CatalogService.svc.cs`)
- Removed `System.Data.Entity` → `Microsoft.EntityFrameworkCore` for `EntityState`.
- Constructor now takes `EntityModel` via DI (no default constructor needed).
- `Dispose()` is a no-op — EF Core context lifetime managed by DI scope.

**`EntityModel.cs`**
- Changed `DbContext` base constructor from `base(connectionString)` to `base(DbContextOptions<EntityModel>)` (EF Core pattern).
- Changed `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)`.
- Removed `Database.SetInitializer(...)` call (EF6 API).
- Namespaced all `DbSet` properties with `= null!` for nullable compatibility.

**`CatalogDBInitializer.cs`**
- Replaced EF6 `CreateDatabaseIfNotExists<EntityModel>` inheritance with a static `Seed(EntityModel)` method.
- Seed checks `Any()` before inserting to prevent duplicates on restart.

**`CatalogConfiguration.cs`**
- Removed `System.Web` dependency; now exposes `ConnectionStringName` and `EnvironmentOverride` for use in `Program.cs`.

**Entity model files** (`CatalogItem`, `CatalogBrand`, `CatalogType`, `CatalogItemsStock`, `DiscountItem`)
- Removed `using System.Data.Entity.Spatial` (no spatial types were used).
- Removed `using System.Web`.
- Added nullable reference type annotations.

**`CatalogServiceMock.cs`**
- Fixed return type mismatch: `GetCatalogTypes()` / `GetCatalogBrands()` now return `List<T>` matching `ICatalogService`.
- Removed unused imports.

---

### `eShopWinForms` — .NET Framework 4.7 WinForms app → net10.0-windows

**Project file (`eShopWinForms.csproj`)**
- Replaced legacy MSBuild-style project file with SDK-style `Microsoft.NET.Sdk`.
- Target framework: `net10.0-windows` (`<UseWindowsForms>true</UseWindowsForms>`).
- `<EnableWindowsTargeting>true</EnableWindowsTargeting>` for cross-compilation on Linux.
- Excluded entire `Helpers\**` directory — those helpers are leftover UWP code (`Windows.Storage`, `Windows.UI.Xaml`, etc.) that was never used by the WinForms layer and cannot compile against a WinForms target.
- Added packages:
  - `System.ServiceModel.Primitives 8.1.0` / `System.ServiceModel.Http 8.1.0` — WCF client
  - `System.Configuration.ConfigurationManager 10.0.0` — `ApplicationSettingsBase` + `ConfigurationManager.AppSettings`
- Suppressed `CA1416` (Windows-only platform guard warnings — intentional for a Windows-only app) and `CS0169` (auto-generated designer fields).

**`Program.cs`**
- Constructs `CatalogServiceClient` with explicit `BasicHttpBinding` + `EndpointAddress` (config-name constructors are not supported in `System.ServiceModel` NuGet packages on .NET 10).
- Service URL sourced from `App.config` `<appSettings>` key `ServiceUrl`.

**`App.config`**
- Removed `<system.serviceModel>` block (not read by .NET 10 WCF client packages).
- Added `<appSettings><add key="ServiceUrl" .../></appSettings>`.

**`Connected Services/eShopServiceReference/Reference.cs`**
- Fixed three `ClientBase<T>` constructors that took `string endpointConfigurationName` (not available in NuGet `System.ServiceModel`) — replaced with stubs that throw `NotSupportedException`. The `(Binding, EndpointAddress)` constructor is unchanged and is the one used at runtime.

**`Controllers/ICatalogView.cs`**
- Events declared as nullable (`event ViewHandler<ICatalogView>?`) to match the `CatalogView` implementation.

**`Views/CatalogView.cs`**
- Made `_controller` field and events nullable.
- Used null-conditional `?.Invoke(...)` for event firing (safe against unsubscribed events).
- Added null guard before `listBox1.SelectedItem.ToString()`.
- Fixed CS8625 by using `null!` for `BindingSource` `dataMember` parameter (null is the correct value here, `null!` satisfies the nullable checker).

---

## Key architectural decisions

| Area | Decision |
|------|----------|
| WCF server | Migrated to **CoreWCF 1.9.1** with `Microsoft.NET.Sdk.Web` + ASP.NET Core host. Same SOAP endpoint path (`/CatalogService.svc`) preserved. |
| EF6 | Migrated to **EF Core 10.0.0**. `CreateDatabaseIfNotExists` replaced with `EnsureCreated()` + seed-on-startup pattern. |
| WCF client | Used **System.ServiceModel.Http 8.1.0** NuGet packages. Config-file endpoint discovery replaced with programmatic `BasicHttpBinding` + `EndpointAddress`. |
| UWP helpers | Excluded from build (leftover files, never referenced by WinForms code). |
| Nullable reference types | Enabled project-wide via `<Nullable>enable</Nullable>`; all CS818x/CS86xx warnings resolved. |

---

## Next steps

- **Database connectivity**: The `ConnectionString` setting targets `(localdb)\MSSQLLocalDB` which is Windows-only. For Linux/container deployments, set the `ConnectionString` environment variable to a SQL Server or Azure SQL connection string.
- **EF Core migrations**: No code-first migrations exist yet. Run `dotnet ef migrations add InitialCreate` and `dotnet ef database update` to create a proper migration history if needed (currently relying on `EnsureCreated` which is fine for dev/test but not recommended for production schema management).
- **CoreWCF endpoint path**: The service is exposed at `/CatalogService.svc`. Update the `App.config ServiceUrl` in `eShopWinForms` and any other clients if the host/port changes.
- **HTTPS**: The legacy `Web.config` had `basicHttpsBinding`. To re-enable HTTPS in CoreWCF, configure `BasicHttpsBinding` and a certificate in `Program.cs`.
