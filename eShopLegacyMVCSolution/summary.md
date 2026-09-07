# Migration Summary: eShopLegacyMVC.sln → .NET 10

## Result
**Build succeeded: 0 errors, 0 warnings** across all 3 projects.

## Projects Migrated

| Project | From | To |
|---------|------|----|
| `src/eShopLegacyMVC` | .NET Framework 4.7.2 | net10.0 |
| `eShopPorted` | net10.0 (broken) | net10.0 (fixed) |
| `eShopLegacy.Utilities` | netstandard2.0 (broken) | netstandard2.0 (fixed) |

---

## Changes by Project

### `src/eShopLegacyMVC` (main migration)

**Project File (`eShopLegacyMVC.csproj`)**
- Replaced legacy MSBuild-format `.csproj` with SDK-style `Microsoft.NET.Sdk.Web`
- Target framework: `net10.0`
- Removed all legacy packages: `EntityFramework` (EF6), `Autofac.Mvc5`, `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.WebApi.*`, `System.Web.Optimization`, `WebGrease`, `Antlr`, `log4net`, `Microsoft.ApplicationInsights.*`, `Microsoft.CodeDom.Providers.DotNetCompilerPlatform`, `Microsoft.Web.Infrastructure`
- Added modern packages: `Autofac 8.0.0`, `Autofac.Extensions.DependencyInjection 9.0.0`, `Microsoft.EntityFrameworkCore 10.0.11`, `Microsoft.EntityFrameworkCore.SqlServer 10.0.11`, `Newtonsoft.Json 13.0.4`

**New Files Created**
- `Program.cs` — ASP.NET Core entry point replacing `Global.asax`. Sets up Autofac via `AutofacServiceProviderFactory`, EF Core, static files, routing, and database seeding.
- `appsettings.json` / `appsettings.Development.json` — migrated from `Web.config`
- `Views/_ViewImports.cshtml` — ASP.NET Core tag helper imports

**Application Lifecycle (Global.asax.cs → Program.cs)**
- `Global.asax.cs` stubbed out (logic moved to `Program.cs`)
- All `App_Start/*.cs` files stubbed (BundleConfig, RouteConfig, WebApiConfig, FilterConfig — logic moved to `Program.cs`)
- Route configuration uses ASP.NET Core endpoint routing via `app.MapControllerRoute`
- Session removed (not needed; machine name now shown via `Environment.MachineName`)

**MVC / ASP.NET Core**
- `Properties/AssemblyInfo.cs` — removed `log4net.Config.XmlConfigurator` attribute
- All controllers migrated from `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`
- `log4net` replaced by `ILogger<T>` (structured logging with message templates)
- `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
- `HttpNotFound()` → `NotFound()`
- `PicController` now uses `IWebHostEnvironment` to resolve the `Pics` folder path
- `Controllers/Api/CatalogController.cs` — updated to `Microsoft.AspNetCore.Mvc`
- `Controllers/WebApi/FilesController.cs` — migrated from `ApiController` (System.Web.Http) to `ControllerBase`
- `Controllers/WebApi/BrandsController.cs` — migrated from `ApiController`, removed `System.Runtime.Remoting.Messaging`
- `Views/Shared/_Layout.cshtml` — replaced `@Styles.Render`/`@Scripts.Render` (System.Web.Optimization) with CDN links
- `Views/Catalog/Create.cshtml` / `Edit.cshtml` — replaced `@Scripts.Render("~/bundles/jqueryval")` with CDN script tags
- `Views/Catalog/Index.cshtml` — replaced `@Html.Partial` with `<partial>` tag helper (MVC1000 fix)

**Entity Framework 6 → EF Core 10**
- `Models/CatalogDBContext.cs` — complete rewrite: `DbContext(string)` → `DbContext(DbContextOptions<T>)`, `DbModelBuilder` → `ModelBuilder`, `EntityTypeConfiguration<T>` → inline `entity` lambda configuration, `HasDatabaseGeneratedOption(None)` → `ValueGeneratedNever()`, `HasRequired` → `HasOne...WithMany`
- `Models/CatalogItemHiLoGenerator.cs` — `db.Database.SqlQuery<Int64>` → `db.Database.SqlQuery<long>` (EF Core 7+ API)
- `Models/Infrastructure/CatalogDBInitializer.cs` — complete rewrite: removed EF6 `CreateDatabaseIfNotExists<T>` base class; converted to static `SeedAsync` method accepting `CatalogDBContext`, `IWebHostEnvironment`, `IConfiguration`; replaced `ConfigurationManager.AppSettings` → `IConfiguration.GetValue<bool>`, `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`, `AppDomain.CurrentDomain.BaseDirectory` → `IWebHostEnvironment.ContentRootPath`, `ExecuteSqlCommand` → `ExecuteSqlRawAsync`, `Database.SetInitializer` → called in `Program.cs` startup scope
- `Services/CatalogService.cs` — updated `System.Data.Entity` → `Microsoft.EntityFrameworkCore`

**Models — Nullable Reference Types**
- `Models/CatalogItem.cs` — added default initializers to `Name`, `Description`, `PictureFileName`, `PictureUri`; navigation properties `CatalogType` and `CatalogBrand` set to `null!` (always loaded via `.Include()`)
- `Models/CatalogBrand.cs` — removed `System.Web`; added `= string.Empty` default for `Brand`
- `Models/CatalogType.cs` — removed `System.Web`; added `= string.Empty` default for `Type`
- `Models/Infrastructure/PreconfiguredData.cs` — removed `System.Web` import

**DI — Autofac**
- `Modules/ApplicationModule.cs` — updated to register `CatalogItemHiLoGenerator` only in the non-mock branch
- `Program.cs` uses `AutofacServiceProviderFactory` with `ConfigureContainer<ContainerBuilder>` modern pattern

---

### `eShopPorted` (pre-existing partially-migrated project — fixed)

**Project File (`eShopPorted.csproj`)**
- Removed: `Autofac.Mvc5 4.0.2`, `Autofac 4.9.1`, `Autofac.Extensions.DependencyInjection 4.4.0`, `log4net 2.0.10`, `WebGrease 1.6.0`, `Antlr4 4.6.6`, `Microsoft.CSharp`
- Switched from Autofac to built-in ASP.NET Core DI

**Program.cs**
- Replaced `Microsoft.AspNetCore.WebHost.CreateDefaultBuilder` (removed in .NET 6) with `WebApplication.CreateBuilder`
- Services registered directly using built-in DI

**Startup.cs** — stubbed (all logic moved to `Program.cs`)

**Modules/ApplicationModule.cs** — stubbed (DI moved to `Program.cs`)

**Controllers/PicController.cs**
- Replaced `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`
- `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`; `HttpNotFound()` → `NotFound()`
- Uses `IWebHostEnvironment` to resolve picture path

**Controllers/CatalogController.cs** — `log4net` replaced by `ILogger<CatalogController>`

**Models/Infrastructure/PreconfiguredData.cs** — removed `System.Web` import

**Views/_ViewImports.cshtml** — created (tag helpers)

**Views/Shared/_Layout.cshtml** — removed `eShopPorted.Startup.StartTime` reference (Startup class no longer has that static property)

---

### `eShopLegacy.Utilities`

**eShopLegacy.Utilities.csproj**
- Added `<LangVersion>latest</LangVersion>`, `<Nullable>enable</Nullable>`, `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` (fix CS0579 duplicate attribute errors)
- Added `System.Text.Json 8.0.5` package (needed for netstandard2.0 target)

**Serializing.cs**
- Replaced `BinaryFormatter` (SYSLIB0011 — disabled/error in .NET 7+) with `System.Text.Json.JsonSerializer`
- `SerializeBinary` serializes to JSON stream; `DeserializeBinary` deserializes from JSON stream
- Note: this changes the binary wire format to JSON. Any persisted binary data will need re-serialization. The `FilesController` serializes to a byte stream for HTTP responses — callers will now receive JSON instead of binary.

---

## Next Steps

- **Database migrations**: EF Core migrations have not been created for the migrated `eShopLegacyMVC` project. Run `dotnet ef migrations add InitialCreate` after verifying connection strings, or use `context.Database.EnsureCreated()` (already in the seeder for development).
- **Autofac version**: `eShopLegacyMVC` uses Autofac 8.0.0. Confirm this package resolves correctly in the target environment's NuGet feed. If unavailable, downgrade to 6.x with `UseServiceProviderFactory` support.
- **BinaryFormatter serialization change**: The `FilesController` API endpoint in both projects now returns JSON instead of binary. Any API consumers of `/api/Files` need to be updated to parse JSON.
- **Static files**: The legacy `Content/`, `Scripts/`, `Pics/`, `Images/`, `fonts/` directories in `eShopLegacyMVC` are served outside of `wwwroot`. The `Program.cs` adds a static files provider for `Pics/`. Add similar providers for other folders if needed, or move them to `wwwroot`.
- **Session**: The ASP.NET Framework session (`Session["MachineName"]`) has been removed. The footer now shows `Environment.MachineName`. If session is required for other features, add `builder.Services.AddSession()` and `app.UseSession()` to `Program.cs`.
- **Application Insights**: Removed from the migration. Consider replacing with `Microsoft.ApplicationInsights.AspNetCore` or OpenTelemetry if telemetry is required.
- **Integration testing**: Run end-to-end tests against the migrated `eShopLegacyMVC` to verify all routes, CRUD operations, and picture serving work correctly.
