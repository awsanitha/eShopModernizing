# eShopLegacyNTier — .NET Framework → .NET 10 Migration Summary

## Final Build Status

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

Both projects build cleanly targeting **net10.0** / **net10.0-windows** on .NET SDK 10.0.401.

---

## Projects Migrated

### 1. eShopWCFService (net10.0, Microsoft.NET.Sdk.Web)
**Original:** .NET Framework 4.6.1, WCF service hosted via IIS/.svc files, Entity Framework 6

**Key changes:**
- Converted from old-style `.csproj` (ToolsVersion 15) to SDK-style `Microsoft.NET.Sdk.Web`
- **WCF → CoreWCF:** Added `CoreWCF.Http` 1.9.1; updated `[ServiceContract]`/`[OperationContract]` attributes in `ICatalogService.cs` from `using System.ServiceModel` → `using CoreWCF`
- Added `Program.cs` for CoreWCF / Kestrel hosting (replaces IIS + `.svc` file activation)
- Added `appsettings.json` with SQL connection string
- **EF6 → EF Core 10:** Replaced `System.Data.Entity.DbContext` with `Microsoft.EntityFrameworkCore.DbContext`; converted `EntityModel` to accept `DbContextOptions<EntityModel>` via DI
- Removed `CatalogDBInitializer : CreateDatabaseIfNotExists<T>` (EF6 pattern); replaced with `HasData()` seeding in `EntityModel.OnModelCreating` and `db.Database.EnsureCreated()` at startup
- Added explicit key values to `PreconfiguredData.GetPreconfiguredDiscountItems()` (required by EF Core `HasData`)
- Removed `System.Data.Entity.Spatial` imports from all model files (no DbGeography/DbGeometry usage)
- Removed `System.Web` imports from all files (CatalogConfiguration, PreconfiguredData, DiscountItem, CatalogDBInitializer)
- Excluded `CatalogServiceClient.cs` from server build (legacy client proxy mixing server/client concerns; incompatible with CoreWCF namespace)
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to prevent conflicts with existing `Properties/AssemblyInfo.cs`

### 2. eShopWinForms (net10.0-windows, Microsoft.NET.Sdk + UseWindowsForms)
**Original:** .NET Framework 4.7, Windows Forms, WCF client reference

**Key changes:**
- Converted from old-style `.csproj` to SDK-style `Microsoft.NET.Sdk` targeting `net10.0-windows`
- Added `<UseWindowsForms>true</UseWindowsForms>` and `<EnableWindowsTargeting>true</EnableWindowsTargeting>`
- **WCF client:** Added `System.ServiceModel.Http` 8.1.0 and `System.ServiceModel.Primitives` 8.1.0 NuGet packages (replaces framework-built-in `System.ServiceModel`)
- **Updated `Reference.cs`:** Removed config-name constructors (`base(string endpointConfigurationName)`) that are not supported by `System.ServiceModel.Primitives` on .NET 10; kept only the explicit `Binding + EndpointAddress` constructor
- **Updated `Program.cs`:** Replaced config-based `new CatalogServiceClient()` with explicit `new CatalogServiceClient(new BasicHttpBinding(), new EndpointAddress(url))`; service endpoint URL is a constant (`http://localhost:5000/CatalogService.svc`)
- **Excluded UWP helper files** from compilation (not referenced by WinForms code; use Windows Runtime APIs unavailable on WinForms):
  - `Helpers/SettingsStorageExtensions.cs`
  - `Helpers/NotificationsHelper.cs`
  - `Helpers/DependencyObjectExtensions.cs`
  - `Helpers/ResourceExtensions.cs`
  - `Helpers/UploadImageHelper.cs`
- Removed `System.Configuration.ConfigurationManager` NuGet reference (auto-provided by .NET 10 SDK)
- Added `[assembly: SupportedOSPlatform("windows")]` to `Properties/AssemblyInfo.cs` to silence CA1416 platform-compatibility warnings (correct: this is a Windows-only WinForms app)
- Added `<NoWarn>CS0169</NoWarn>` to suppress unused-field warnings in auto-generated `CatalogView.Designer.cs`
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to prevent conflicts with existing `Properties/AssemblyInfo.cs`
- Added `Newtonsoft.Json` 13.0.3 (used by `Helpers/Json.cs`)

---

## Package Versions Used

| Package | Version | Project |
|---------|---------|---------|
| CoreWCF.Http | 1.9.1 | eShopWCFService |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.0 | eShopWCFService |
| Microsoft.EntityFrameworkCore.Design | 10.0.0 | eShopWCFService |
| System.ServiceModel.Http | 8.1.0 | eShopWinForms |
| System.ServiceModel.Primitives | 8.1.0 | eShopWinForms |
| Newtonsoft.Json | 13.0.3 | eShopWinForms |

---

## Next Steps

1. **Service endpoint URL** — `Program.cs` in `eShopWinForms` contains a hardcoded constant `http://localhost:5000/CatalogService.svc`. In production, this should be read from `appsettings.json` or an environment variable via `Microsoft.Extensions.Configuration`.

2. **Database setup** — `eShopWCFService` uses `Database.EnsureCreated()` at startup for development convenience. For production, add proper EF Core migrations (`dotnet ef migrations add InitialCreate`, `dotnet ef database update`).

3. **Service port** — CoreWCF in Program.cs uses Kestrel's default port (5000 HTTP). The original service ran on port 62314. Align the port in `eShopWinForms/Program.cs` `ServiceEndpointUrl` with the deployed service URL or configure via environment/config.

4. **WCF WSDL compatibility** — The CoreWCF BasicHttpBinding endpoint at `/CatalogService.svc` should be SOAP-wire-compatible with the original .svc endpoint. The WinForms `Reference.cs` proxy contract is unchanged. Smoke-test with a running service after deployment.

5. **UWP helper files** — `Helpers/SettingsStorageExtensions.cs`, `NotificationsHelper.cs`, `DependencyObjectExtensions.cs`, `ResourceExtensions.cs`, and `UploadImageHelper.cs` are excluded from the WinForms build. They appear to be UWP project artifacts included by mistake. If WinForms equivalents are needed, rewrite them using `System.IO`, `System.Windows.Forms` notification APIs, etc.

6. **EF Core migrations directory** — No migrations are currently committed. Run `dotnet ef migrations add InitialCreate --project src/eShopWCFService` from the solution root to generate the initial migration before first deployment to a non-development environment.
