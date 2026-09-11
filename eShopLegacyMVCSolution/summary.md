# .NET Framework → .NET 10 Migration Summary

## Final Build Status
✅ `dotnet build eShopLegacyMVC.sln` exits with code 0  
✅ Zero compilation errors  
✅ Zero warnings  
✅ All 3 projects build cleanly for Release and Debug configurations

---

## Projects Migrated

### 1. `eShopLegacyMVC` (src/eShopLegacyMVC)
**Before:** .NET Framework 4.7.2, non-SDK-style `.csproj`, ASP.NET MVC 5, Entity Framework 6, System.Web  
**After:** .NET 10 SDK-style `.csproj`, ASP.NET Core MVC, Entity Framework Core 10.0.12

### 2. `eShopPorted` (eShopPorted)
**Before:** `net10.0;netstandard2.0` (incorrect for a web app), old Autofac 4.x pattern, stale packages  
**After:** `net10.0` only, Autofac 9.0/8.0 with `UseServiceProviderFactory`, clean .NET 10 web app

### 3. `eShopLegacy.Utilities` (eShopLegacy.Utilities)
**Before:** `netstandard2.0` only, `BinaryFormatter` (removed in .NET 9+)  
**After:** `net10.0;netstandard2.0` multi-targeted, `System.Text.Json` serialization

---

## Key Changes Per Project

### eShopLegacy.Utilities
| File | Change |
|------|--------|
| `eShopLegacy.Utilities.csproj` | Added `net10.0` to `<TargetFrameworks>`, added `System.Text.Json` package for netstandard2.0 target, added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to prevent duplicate attribute errors |
| `Serializing.cs` | Replaced `BinaryFormatter` (removed in .NET 9+) with `System.Text.Json.JsonSerializer` |

### eShopPorted
| File | Change |
|------|--------|
| `eShopPorted.csproj` | Removed `netstandard2.0` TFM (web app should be single TFM), removed legacy packages (`Autofac.Mvc5`, `WebGrease`, `Antlr4`), updated `Autofac` to 8.0.0 / `Autofac.Extensions.DependencyInjection` to 9.0.0, updated `log4net` to 3.4.0 |
| `Program.cs` | Rewritten to use .NET 10 minimal hosting model with `WebApplication.CreateBuilder`, Autofac `UseServiceProviderFactory(new AutofacServiceProviderFactory())` |
| `Startup.cs` | Deleted — replaced by minimal hosting in `Program.cs` |
| `AppInfo.cs` | New file — holds `StartTime` static property previously on `Startup` class |
| `Controllers/PicController.cs` | Replaced `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`, replaced `HttpStatusCodeResult`/`HttpNotFound()` with `BadRequest()`/`NotFound()`, injected `IWebHostEnvironment` for physical path resolution |
| `Models/Infrastructure/PreconfiguredData.cs` | Removed unused `using System.Web` |
| `Views/Shared/_Layout.cshtml` | Updated `eShopPorted.Startup.StartTime` reference to `eShopPorted.AppInfo.StartTime` |
| `Views/Catalog/Index.cshtml` | Replaced `@Html.Partial(...)` with `@await Html.PartialAsync(...)` (MVC1000 fix) |

### eShopLegacyMVC (Primary Migration Target)
| File | Change |
|------|--------|
| `eShopLegacyMVC.csproj` | Full replacement: non-SDK format → `Microsoft.NET.Sdk.Web` targeting `net10.0`, removed all legacy packages/references, added EF Core 10, Autofac 9/8, log4net 3.4.0 |
| `Program.cs` | **New file** — replaces `Global.asax` application lifecycle, configures Autofac, registers MVC/session/DbContext, configures middleware pipeline |
| `appsettings.json` | **New file** — migrated connection strings and app settings from `Web.config` |
| `appsettings.Development.json` | **New file** — development overrides (debug logging, mock data) |
| `Global.asax.cs` | Stripped `HttpApplication` and all `System.Web.*` dependencies; kept `ActivityIdHelper` helper class |
| `App_Start/BundleConfig.cs` | Replaced with empty stub (bundling removed; static files served directly) |
| `App_Start/FilterConfig.cs` | Replaced with empty stub (global filters configured in Program.cs) |
| `App_Start/RouteConfig.cs` | Replaced with empty stub (routing configured in Program.cs) |
| `App_Start/WebApiConfig.cs` | Replaced with empty stub (API routing via `MapControllers()`) |
| `Controllers/CatalogController.cs` | Replaced `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`, replaced `HttpStatusCodeResult`/`HttpNotFound()` with `BadRequest()`/`NotFound()`, updated URL generation using `Request.Scheme` |
| `Controllers/PicController.cs` | Replaced `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`, injected `IWebHostEnvironment`, pictures served from `ContentRootPath/Pics/` |
| `Controllers/Api/CatalogController.cs` | Replaced `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc` |
| `Controllers/WebApi/FilesController.cs` | Replaced `System.Web.Http.ApiController` → `ControllerBase`, fixed return types to `IActionResult` |
| `Controllers/WebApi/BrandsController.cs` | Replaced `System.Web.Http.ApiController` → `ControllerBase`, removed `System.Runtime.Remoting.Messaging`, fixed return types |
| `Models/CatalogDBContext.cs` | Full EF Core rewrite: `DbContext` constructor now takes `DbContextOptions<T>`, replaced `EntityTypeConfiguration<T>` with `EntityTypeBuilder<T>`, replaced EF6 fluent APIs (`HasRequired`, `HasDatabaseGeneratedOption`) with EF Core equivalents (`HasOne`, `ValueGeneratedNever`) |
| `Models/CatalogItemHiLoGenerator.cs` | Replaced EF6 `Database.SqlQuery<T>` with EF Core `Database.SqlQueryRaw<T>` |
| `Models/Infrastructure/CatalogDBInitializer.cs` | Full rewrite: removed `CreateDatabaseIfNotExists<T>` inheritance, replaced `System.Web.Hosting.HostingEnvironment` with `IWebHostEnvironment`, replaced `ConfigurationManager` with `IConfiguration`, replaced EF6 `Database.ExecuteSqlCommand`/`SqlQuery` with EF Core `ExecuteSqlRaw`/`SqlQueryRaw`, added `Initialize(IServiceProvider)` method |
| `Models/Infrastructure/PreconfiguredData.cs` | Removed unused `using System.Web` |
| `Models/CatalogBrand.cs` | Removed unused `System.Web` and other unnecessary usings |
| `Models/CatalogType.cs` | Removed unused `System.Web` and other unnecessary usings |
| `Models/CatalogItem.cs` | No changes needed (uses only BCL types) |
| `Modules/ApplicationModule.cs` | Removed `CatalogDBContext` registration (handled by `services.AddDbContext<>()` in Program.cs), kept service/generator registrations, made `CatalogDBInitializer` conditional on non-mock mode |
| `Services/CatalogService.cs` | Replaced `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`, added `.ToList()` to `GetCatalogTypes()`/`GetCatalogBrands()` |
| `Services/CatalogServiceMock.cs` | Removed unused `using System.Web` |
| `Views/Shared/_Layout.cshtml` | Replaced `@Styles.Render(...)` and `@Scripts.Render(...)` bundle helpers with direct CSS/JS link and script tags; removed `HttpContext.Current.Session` reference |
| `Views/Catalog/Create.cshtml` | Replaced `@Scripts.Render("~/bundles/jqueryval")` with direct script tags |
| `Views/Catalog/Edit.cshtml` | Replaced `@Scripts.Render("~/bundles/jqueryval")` with direct script tags |
| `Views/Catalog/Index.cshtml` | Replaced `@Html.Partial(...)` with `@await Html.PartialAsync(...)` (MVC1000 fix) |

---

## Package Version Changes

| Package | Before | After |
|---------|--------|-------|
| Target Framework | `net472` (non-SDK) | `net10.0` |
| `Autofac` | 6.1.0 | 8.0.0 |
| `Autofac.Extensions.DependencyInjection` | 4.4.0 | 9.0.0 |
| `Autofac.Mvc5` | 4.0.2 | **Removed** (not for ASP.NET Core) |
| `EntityFramework` (EF6) | 6.2.0 | **Removed** |
| `Microsoft.EntityFrameworkCore` | N/A | 10.0.12 |
| `Microsoft.EntityFrameworkCore.SqlServer` | N/A | 10.0.12 |
| `Microsoft.EntityFrameworkCore.Design` | N/A | 10.0.12 |
| `log4net` | 2.0.10 | 3.4.0 |
| `Newtonsoft.Json` | 12.0.1 | 13.0.4 |
| `System.Web.Mvc` (Microsoft.AspNet.Mvc) | 5.2.7 | **Removed** (built into ASP.NET Core) |
| `System.Web.Http` (WebApi) | 5.2.7 | **Removed** (built into ASP.NET Core) |
| `System.Web.Optimization` (WebGrease) | 1.6.0 | **Removed** |
| `Microsoft.ApplicationInsights.*` | 2.9.1 | **Removed** |
| `System.Text.Json` | N/A | 10.0.12 (netstandard2.0 only in Utilities) |

---

## Architecture Changes

### Startup / Application Lifecycle
- `Global.asax` / `MvcApplication : HttpApplication` → `Program.cs` with minimal hosting (`WebApplication.CreateBuilder`)
- `Web.config` (connection strings, appSettings) → `appsettings.json` / `appsettings.Development.json`
- `App_Start/` files → All replaced by Program.cs configuration

### Dependency Injection
- `Autofac.Integration.Mvc` + `AutofacDependencyResolver` → `Autofac.Extensions.DependencyInjection` with `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
- `Autofac.Integration.WebApi` + `AutofacWebApiDependencyResolver` → same factory pattern

### Data Access
- Entity Framework 6 → Entity Framework Core 10
- `Database.SetInitializer<T>` → `Database.EnsureCreated()` in `CatalogDBInitializer.Initialize()`
- `Database.SqlQuery<T>` → `Database.SqlQueryRaw<T>` (EF Core 7+)
- `Database.ExecuteSqlCommand` → `Database.ExecuteSqlRaw`
- `EntityTypeConfiguration<T>` fluent API → `EntityTypeBuilder<T>` with EF Core methods

### Static Files / Bundling
- `System.Web.Optimization.BundleTable` → Removed; CSS/JS referenced directly in views
- `@Styles.Render(...)` / `@Scripts.Render(...)` → Direct `<link>` and `<script>` tags in `_Layout.cshtml`

### Serialization
- `System.Runtime.Serialization.Formatters.Binary.BinaryFormatter` (removed in .NET 9) → `System.Text.Json.JsonSerializer`

---

## Next Steps

1. **Static files location** — The `Content/`, `Scripts/`, `Images/`, `Pics/` directories in `eShopLegacyMVC` are outside the standard ASP.NET Core `wwwroot/` folder. The `UseStaticFiles()` call serves from `wwwroot/` by default. Consider:
   - Moving static files into a `wwwroot/` subdirectory, OR
   - Adding additional `UseStaticFiles` middleware with `PhysicalFileProvider` for each directory

2. **Database migrations** — The migrated project uses `Database.EnsureCreated()` for seeding, which does not play well with EF Core Migrations. Consider creating proper EF Core migrations to replace the SQL sequence scripts.

3. **Session usage** — The legacy `_Layout.cshtml` displayed session data (MachineName, SessionStartTime). This was simplified to just show `Environment.MachineName`. If session-based tracking is needed, add proper session middleware and update the view.

4. **log4net configuration** — The `Properties/AssemblyInfo.cs` has `[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml")]`. This is called at startup in `Program.cs` via `log4net.Config.XmlConfigurator.Configure()`. Verify the `log4Net.xml` configuration file is copied to the output directory (mark as `Content` / `CopyToOutputDirectory`).

5. **ApplicationInsights** — ApplicationInsights telemetry was removed. If telemetry is needed, add `Microsoft.ApplicationInsights.AspNetCore` package and configure in `Program.cs`.
