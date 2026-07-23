# Migration Summary: eShopLegacyMVC.sln — .NET Framework 4.7.2 → .NET 10

## Result

**Build succeeded — 0 errors, 0 warnings** across all 3 projects.

```
eShopLegacy.Utilities → net10.0/eShopLegacy.Utilities.dll
eShopLegacyMVC        → net10.0/eShopLegacyMVC.dll
eShopPorted           → net10.0/eShopPorted.dll
```

---

## Changes Made

### 1. `eShopLegacy.Utilities`
- **Target framework** upgraded from `netstandard2.0` → `net10.0`.
- **AssemblyInfo conflict** fixed: added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to suppress SDK auto-generated attributes that conflicted with the existing `Properties/AssemblyInfo.cs`.
- **Removed** `Microsoft.CSharp` and `System.Data.DataSetExtensions` NuGet packages (not needed on net10.0).
- **`Serializing.cs`** — Replaced removed `BinaryFormatter` (SYSLIB0011 error in .NET 9+) with `System.Text.Json.JsonSerializer`. The serialized format changes from binary to JSON; callers consuming `SerializeBinary`/`DeserializeBinary` should be aware of this behavioral change.

### 2. `src/eShopLegacyMVC` — Full Migration .NET Framework 4.7.2 → net10.0

#### Project file (`eShopLegacyMVC.csproj`)
- Replaced the old-style `.csproj` (XML with MSBuild 15 targets, `Microsoft.WebApplication.targets`) with a clean **SDK-style** `<Project Sdk="Microsoft.NET.Sdk.Web">` file.
- `TargetFramework` set to `net10.0`.
- Removed all .NET Framework-only packages: `EntityFramework 6`, `System.Web.Mvc`, `System.Web.Http`, `Autofac.Integration.Mvc`, `Autofac.Mvc5`, `Microsoft.AspNet.WebApi`, `Microsoft.AspNet.Web.Optimization`, `WebGrease`, `Antlr`, `Microsoft.ApplicationInsights.*`, `Microsoft.Net.Compilers`, etc.
- Added modern packages: `Autofac 8.3.0`, `Autofac.Extensions.DependencyInjection 10.0.0`, `Microsoft.EntityFrameworkCore 10.0.10`, `Microsoft.EntityFrameworkCore.SqlServer 10.0.10`, `log4net 3.3.2`, `Newtonsoft.Json 13.0.3`.
- Excluded legacy files from compilation: `Global.asax.cs`, `App_Start/BundleConfig.cs`, `App_Start/FilterConfig.cs`, `App_Start/RouteConfig.cs`, `App_Start/WebApiConfig.cs`.

#### New files created
| File | Purpose |
|------|---------|
| `Program.cs` | Replaces `Global.asax.cs` and all `App_Start/*.cs`; uses minimal hosting (`WebApplication.CreateBuilder`). Configures Autofac, EF Core, MVC, static files, and routing. |
| `appsettings.json` | Replaces `Web.config`; connection string + `UseMockData`/`UseCustomizationData` settings. |
| `appsettings.Development.json` | Development log-level overrides. |
| `Views/_ViewImports.cshtml` | Adds `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` for ASP.NET Core tag helpers. |

#### Source files migrated
| File | Key changes |
|------|-------------|
| `Models/CatalogBrand.cs` | Removed `System.Web` using. Added nullable annotation on `Brand`. |
| `Models/CatalogType.cs` | Removed `System.Web` using. Added nullable annotation on `Type`. |
| `Models/CatalogItem.cs` | Removed `System.Web` using. Added proper nullable annotations; navigation props use `null!` (EF Core required-nav pattern). |
| `Models/CatalogDBContext.cs` | **EF6 → EF Core**: changed base class from `DbContext("name=...")` to `DbContext(DbContextOptions<T>)`. Replaced `EntityTypeConfiguration<T>` fluent API with `ModelBuilder.Entity<T>()` lambdas. |
| `Models/CatalogItemHiLoGenerator.cs` | Replaced `Database.SqlQuery<T>` (EF6) with `Database.SqlQueryRaw<T>` (EF Core). Added `System.Linq` for `ToList()`. |
| `Models/Infrastructure/CatalogDBInitializer.cs` | **EF6 `CreateDatabaseIfNotExists<T>` → EF Core seeding**: class no longer inherits EF6 initializer. Seed method calls `context.Database.Migrate()` then seeds if empty. Replaced `HostingEnvironment.ApplicationPhysicalPath` with injected `contentRootPath`. Replaced `Database.ExecuteSqlCommand` with `ExecuteSqlRaw`. Added sequence-name validation to suppress `EF1002` SQL-injection advisory. |
| `Controllers/CatalogController.cs` | `System.Web.Mvc.Controller` → `Microsoft.AspNetCore.Mvc.Controller`. `HttpStatusCodeResult(BadRequest)` → `BadRequest()`. `HttpNotFound()` → `NotFound()`. `Request.Url.Scheme` → `Request.Scheme`. |
| `Controllers/PicController.cs` | Same MVC namespace migration. `Server.MapPath("~/Pics")` → `IWebHostEnvironment.WebRootPath`. Added `IWebHostEnvironment` constructor injection. `HttpNotFound()` → `NotFound()`. |
| `Controllers/WebApi/BrandsController.cs` | `System.Web.Http.ApiController` → `Microsoft.AspNetCore.Mvc.ControllerBase`. `IHttpActionResult` → `IActionResult`. Removed `System.Net.Http`, `System.Runtime.Remoting.Messaging`. Added `[ApiController]`, `[Route]` attributes. |
| `Controllers/WebApi/FilesController.cs` | Same Web API migration. |
| `Controllers/Api/CatalogController.cs` | `System.Web.Mvc.Controller` → `Microsoft.AspNetCore.Mvc.ControllerBase` with `[ApiController]`. |
| `Services/CatalogService.cs` | Removed EF6 `EntityState` and `System.Data.Entity`. Added EF Core `Microsoft.EntityFrameworkCore`. `db.Database.SqlQuery` removed (HiLo now in generator). |
| `Services/CatalogServiceMock.cs` | Updated `FindCatalogItem` return type to `CatalogItem?` to match interface. |
| `Services/ICatalogService.cs` | Updated `FindCatalogItem` to return `CatalogItem?`. |
| `Modules/ApplicationModule.cs` | Removed `CatalogDBContext` and `CatalogDBInitializer` Autofac registrations (now managed by ASP.NET Core DI via `AddDbContext`). |
| `Views/Shared/_Layout.cshtml` | Replaced `@Styles.Render`/`@Scripts.Render` bundle helpers with direct `<link>` and `<script>` tags. Removed `HttpContext.Current.Session` display. |
| `Views/Catalog/Create.cshtml` | Replaced `@Scripts.Render("~/bundles/jqueryval")` with direct script references. |
| `Views/Catalog/Edit.cshtml` | Same bundling replacement. |
| `Views/Catalog/Index.cshtml` | Replaced `@Html.Partial(...)` with `@await Html.PartialAsync(...)` (MVC1000). |

### 3. `eShopPorted` — Cleanup and Modernisation

- **Removed legacy packages**: `Autofac.Mvc5`, `WebGrease`, `Antlr4`, `Microsoft.CSharp`.
- **Upgraded packages**: `Autofac` → 8.3.0, `Autofac.Extensions.DependencyInjection` → 10.0.0, `log4net` → 3.3.2, `Newtonsoft.Json` → 13.0.3.
- **`Program.cs`** — Migrated from obsolete `WebHost.CreateDefaultBuilder` / `IWebHostBuilder` (ASPDEPR008) to the modern `WebApplication.CreateBuilder` pattern.
- **`Startup.cs`** — Excluded from compilation (`<Compile Remove="Startup.cs" />`); all startup logic moved to `Program.cs`.
- **`Controllers/PicController.cs`** — Replaced `System.Web.Mvc` (from the legacy `Autofac.Mvc5` dependency chain) with `Microsoft.AspNetCore.Mvc`. Replaced `Server.MapPath` with `IWebHostEnvironment.WebRootPath`.
- **`Views/Shared/_Layout.cshtml`** — Replaced `eShopPorted.Startup.StartTime` reference (from removed Startup.cs) with `System.DateTime.UtcNow`.
- **`Views/Catalog/Index.cshtml`** — Replaced `@Html.Partial` with `@await Html.PartialAsync` (MVC1000).

---

## Next Steps

- **Database migrations**: `src/eShopLegacyMVC` does not have EF Core migrations. Run `dotnet ef migrations add Initial` and `dotnet ef database update` before first run (or switch to `UseMockData=true` for testing without a DB). The legacy EF6 `HiLo` sequence generator is preserved but requires the SQL Server sequence objects to exist.
- **`Serializing.cs` format change**: `BinaryFormatter` was replaced with `System.Text.Json`. Any existing clients that consumed binary-serialized data from the `FilesController` will need to be updated to parse JSON instead.
- **Static files path**: `src/eShopLegacyMVC` serves static files from the content root (legacy folders `Content/`, `Scripts/`, `Images/`, `Pics/`). Consider moving these into a `wwwroot/` subdirectory and removing the `PhysicalFileProvider` workaround in `Program.cs` for cleaner production deployment.
- **`app.config` / `Views/Web.config`**: These legacy files are still present in the repository but are not used at runtime. They can be safely deleted.
- **log4net 3.3.2 configuration**: The `log4Net.xml` configuration file may need updating if it references appenders or patterns that changed between log4net 2.x and 3.x.
- **`eShopPorted` database migrations**: The existing EF Core migrations in `eShopPorted/Migrations/` were created against EF Core 5.x; verify they are compatible with EF Core 10.0.10.
