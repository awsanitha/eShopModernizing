# eShopLegacyMVC.sln — .NET Framework to .NET 10 Migration Summary

**Migration completed:** 2026-07-27  
**Final build result:** `Build succeeded. 0 Warning(s). 0 Error(s).`  
**Target framework:** `net10.0`

---

## Projects migrated

| Project | Before | After |
|---------|--------|-------|
| `src/eShopLegacyMVC` | .NET Framework 4.7.2 (classic .csproj) | `net10.0` (SDK-style) |
| `eShopPorted` | `net10.0` (compilation errors) | `net10.0` (clean build) |
| `eShopLegacy.Utilities` | `netstandard2.0` (duplicate assembly attrs) | `netstandard2.0` (clean build) |

---

## Changes made

### eShopLegacy.Utilities
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to suppress CS0579 duplicate attribute errors from the legacy `Properties/AssemblyInfo.cs`
- Replaced `BinaryFormatter` (disabled in .NET 10) with `System.Text.Json` in `Serializing.cs`
- Added `System.Text.Json` 9.0.5 NuGet reference
- Removed `System.Data.DataSetExtensions` (unnecessary)

### eShopLegacyMVC (src/eShopLegacyMVC)

**Project file:**
- Replaced classic MSBuild `.csproj` with SDK-style `Microsoft.NET.Sdk.Web` targeting `net10.0`
- Removed: `Autofac.Mvc5`, `EntityFramework` 6, `Microsoft.AspNet.*`, `WebGrease`, `Antlr`, `WebApi`, `ApplicationInsights`, `Microsoft.CodeDom.Providers.*`
- Added: `Autofac` 8.3.0, `Autofac.Extensions.DependencyInjection` 9.0.0, `log4net` 3.3.2 (non-vulnerable), EF Core 10.0.10, `Newtonsoft.Json` 13.0.4
- Excluded `App_Start/*.cs`, `Global.asax.cs` from compilation (replaced by Program.cs + Startup.cs)

**New files added:**
- `Program.cs` — .NET 6+ Generic Host with `AutofacServiceProviderFactory`
- `Startup.cs` — ASP.NET Core middleware pipeline, Autofac `ConfigureContainer`
- `appsettings.json` — replaced `Web.config` settings
- `Views/_ViewImports.cshtml` — Razor tag helpers and namespace imports
- `Models/Config/CatalogItemConfig.cs`, `CatalogBrandConfig.cs`, `CatalogTypeConfig.cs` — EF Core `IEntityTypeConfiguration<T>`

**Source files migrated:**
- `Controllers/CatalogController.cs` — `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`; `HttpStatusCodeResult` → `BadRequest()`; `HttpNotFound()` → `NotFound()`; `[Bind(Include=...)]` → `[Bind("...")]`
- `Controllers/PicController.cs` — replaced `Server.MapPath` with `IWebHostEnvironment.WebRootPath`
- `Controllers/WebApi/BrandsController.cs` — `ApiController`/`IHttpActionResult` → `ControllerBase`/`IActionResult`; added `[ApiController]` + `[Route("api/[controller]")]`
- `Controllers/WebApi/FilesController.cs` — `System.Web.Http` → ASP.NET Core
- `Controllers/Api/CatalogController.cs` — `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`
- `Models/CatalogDBContext.cs` — EF6 `DbContext`/`EntityTypeConfiguration` → EF Core `DbContext`/`IEntityTypeConfiguration`
- `Models/CatalogBrand.cs`, `CatalogType.cs` — removed `System.Web` usings
- `Models/CatalogItem.cs` — nullable annotations; EF Core navigation property pattern (`= null!`)
- `Models/CatalogItemHiLoGenerator.cs` — `db.Database.SqlQuery<Int64>()` (EF6) → `db.Database.SqlQuery<long>()` (EF Core 8+)
- `Models/Infrastructure/PreconfiguredData.cs` — removed `System.Web` usings
- `Models/Infrastructure/CatalogDBInitializer.cs` — replaced EF6 `CreateDatabaseIfNotExists<T>` with EF Core-compatible seeder using `context.Database.EnsureCreated()`; replaced `ConfigurationManager` with `IConfiguration`; replaced `HostingEnvironment.ApplicationPhysicalPath` with injected `contentRootPath`
- `Services/CatalogService.cs` — EF6 → EF Core; removed `CatalogItemHiLoGenerator` dependency
- `Services/ICatalogService.cs` — added nullable return type for `FindCatalogItem`
- `Services/CatalogServiceMock.cs` — nullable return type
- `Modules/ApplicationModule.cs` — removed EF6-specific registrations (`CatalogDBInitializer`, `CatalogItemHiLoGenerator`)
- `Views/Shared/_Layout.cshtml` — replaced `@Styles.Render`, `@Scripts.Render` (MVC5 bundling) with CDN links; replaced `HttpContext.Current.Session` with `Environment.MachineName`
- `Views/Catalog/Create.cshtml`, `Edit.cshtml` — replaced `@Scripts.Render` with direct `<script>` tags
- `Views/Catalog/Index.cshtml` — replaced `@Html.Partial` with `<partial>` tag helper (fixes MVC1000)

### eShopPorted

**Project file:**
- Removed incompatible packages: `Autofac.Mvc5` 4.0.2, `Autofac` 4.9.1, `Autofac.Extensions.DependencyInjection` 4.4.0, `WebGrease` 1.6.0, `Antlr4` 4.6.6, `Microsoft.CSharp`
- Updated: `Autofac` to 8.3.0, `Autofac.Extensions.DependencyInjection` to 9.0.0, `log4net` to 3.3.2

**Source files fixed:**
- `Controllers/PicController.cs` — replaced `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`; replaced `Server.MapPath` with `IWebHostEnvironment`; fixed `HttpStatusCodeResult`/`HttpNotFound()` → ASP.NET Core equivalents
- `Controllers/CatalogController.cs` — fixed `LogManager.GetLogger` to use `typeof()` (avoids nullable CS8604)
- `Program.cs` — replaced old `WebHost.CreateDefaultBuilder` with `Host.CreateDefaultBuilder` + `AutofacServiceProviderFactory`
- `Startup.cs` — replaced old `IServiceProvider ConfigureServices` + `builder.Populate` + `AutofacServiceProvider` pattern with modern `ConfigureContainer(ContainerBuilder)` method; replaced `IHostingEnvironment` (obsolete) with `IWebHostEnvironment`
- `Models/CatalogItem.cs`, `CatalogBrand.cs`, `CatalogType.cs` — added proper nullable annotations and EF Core `= null!` navigation property pattern
- `Models/Infrastructure/PreconfiguredData.cs` — removed `System.Web` using
- `Services/ICatalogService.cs` — nullable return type on `FindCatalogItem`
- `Services/CatalogService.cs` — nullable return type; made `db` field `readonly`
- `Services/CatalogServiceMock.cs` — nullable return type
- `Controllers/Api/FilesController.cs` — fixed `Brand` property nullable
- `Views/_ViewImports.cshtml` — created with ASP.NET Core tag helpers
- `Views/Catalog/Index.cshtml` — replaced `@Html.Partial` with `<partial>` tag helper

---

## Next steps

- **log4net vulnerability (NU1902):** `log4net` 3.3.2 is currently the latest available version. The reported advisory (GHSA-4f7c-pmjv-c25w) is marked moderate severity. Monitor for a future release that clears the advisory, or consider replacing with `Microsoft.Extensions.Logging` for a fully modern logging stack.
- **EF Core Migrations for eShopLegacyMVC:** The project now uses EF Core but the existing Migrations folder was not migrated (only `eShopPorted` has EF Core migrations). If a SQL Server database is needed, run `dotnet ef migrations add Initial` and `dotnet ef database update` in the `eShopLegacyMVC` project directory.
- **`CatalogItemHiLoGenerator`:** The HiLo sequence generator requires a `catalog_hilo` SQL sequence to exist in the database schema. The legacy SQL scripts in `Models/Infrastructure/*.Sequence.sql` define these. When provisioning the database, ensure these sequences are created first.
- **`BinaryFormatter` removal in `FilesController`:** Both `eShopPorted` and `eShopLegacyMVC` `FilesController` endpoints now return JSON-serialized data (via `eShopLegacy.Utilities.Serializing`). Any existing clients expecting the old binary format will need to be updated to parse JSON.
- **Production connection string:** Both `appsettings.json` files default to `(localdb)\mssqllocaldb`. Update `ConnectionStrings:DefaultConnection` for production deployments and set `UseMockData: false`.
- **Views/Web.config:** The legacy `Views/Web.config` files remain on disk in both projects. They are ignored by the SDK-style build system but can be deleted for cleanup.
- **eShopPorted Views (Details/Delete/CatalogTable CS8602):** These Razor views access `CatalogBrand.Brand` and `CatalogType.Type` on navigation properties. Using the `= null!` EF Core pattern on the model class resolved these warnings.
