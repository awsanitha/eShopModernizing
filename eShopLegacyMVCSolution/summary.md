# Migration Summary: eShopLegacyMVCSolution → .NET 10

## Result

**`dotnet build eShopLegacyMVC.sln` exits with code 0 — ZERO compilation errors.**

All three projects build successfully targeting `net10.0`:
- `eShopLegacyMVC` (src/eShopLegacyMVC) — migrated from .NET Framework 4.7.2
- `eShopPorted` — modernized from old WebHost/Autofac pattern
- `eShopLegacy.Utilities` — multi-targeted net10.0 + netstandard2.0

---

## Changes Made

### eShopLegacy.Utilities

| File | Change |
|------|--------|
| `eShopLegacy.Utilities.csproj` | Added `net10.0` target; multi-targeted `net10.0;netstandard2.0`. Added `GenerateAssemblyInfo=false` to prevent duplicate attribute conflict with legacy `Properties/AssemblyInfo.cs`. Added `System.Text.Json` for netstandard2.0 target. |
| `Serializing.cs` | Replaced `BinaryFormatter` (removed in .NET 5+) with `System.Text.Json.JsonSerializer`. Serialized format changed from binary to JSON — see Next Steps. |

### eShopPorted

| File | Change |
|------|--------|
| `eShopPorted.csproj` | Removed `netstandard2.0` target (web app must single-target `net10.0`). Removed `Autofac`, `Autofac.Extensions.DependencyInjection`, `Autofac.Mvc5`, `WebGrease`, `Antlr4`. Updated `log4net` 2.0.10 → 2.0.17. |
| `Program.cs` | Rewritten to `WebApplication.CreateBuilder` (modern minimal hosting), replacing obsolete `WebHost.CreateDefaultBuilder` (ASPDEPR008). |
| `Startup.cs` | Emptied — all startup logic moved into `Program.cs`. Autofac dependency removed. |
| `Modules/ApplicationModule.cs` | Removed Autofac dependency; converted to documentation-only static class. DI registrations moved to `Program.cs`. |
| `Controllers/PicController.cs` | Replaced `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`. Replaced `Server.MapPath` with `IWebHostEnvironment.WebRootPath`. Fixed `HttpStatusCodeResult`/`HttpNotFound` → `BadRequest()`/`NotFound()`. |
| `Models/Infrastructure/PreconfiguredData.cs` | Removed `using System.Web`. |
| `Views/Catalog/Index.cshtml` | Replaced `@Html.Partial` with `@await Html.PartialAsync` (MVC1000 fix). |
| `Views/Shared/_Layout.cshtml` | Replaced `eShopPorted.Startup.StartTime` reference with `DateTime.UtcNow`. |
| `Views/_ViewImports.cshtml` | Created with `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`. |
| `Views/Web.config` | Cleared old System.Web.Mvc Razor host config (not used by ASP.NET Core). |

### eShopLegacyMVC (src/eShopLegacyMVC)

#### Project Infrastructure

| File | Change |
|------|--------|
| `eShopLegacyMVC.csproj` | **Full rewrite** from old-style → SDK-style `Microsoft.NET.Sdk.Web`, targeting `net10.0`. Removed all old framework references (`System.Web.Mvc`, `EntityFramework`, `Autofac.Mvc5`, `Autofac.WebApi2`, `Microsoft.AspNet.*`, `WebGrease`, `Antlr`, Application Insights packages). Added `Microsoft.EntityFrameworkCore.SqlServer 10.0.12`, `log4net 2.0.17`, `Newtonsoft.Json 13.0.4`. Excluded legacy files from compilation. |
| `Program.cs` | **New file** — top-level hosting with `WebApplication.CreateBuilder`. Configures: MVC, Session, EF Core, logging (log4net), static files from legacy content directories (Content, Scripts, Images, Pics, fonts), attribute and conventional routing, and DB initialization. |
| `appsettings.json` | **New file** — migrated settings from `Web.config` (connection strings, UseMockData, UseCustomizationData). |
| `appsettings.Development.json` | **New file** — development-specific logging settings. |
| `Global.asax.cs` | Excluded from compilation (replaced by `Program.cs`). |
| `App_Start/BundleConfig.cs` | Excluded from compilation (bundling replaced by CDN links in `_Layout.cshtml`). |
| `App_Start/FilterConfig.cs` | Excluded from compilation (filters configured in `Program.cs` if needed). |
| `App_Start/RouteConfig.cs` | Excluded from compilation (routing in `Program.cs`). |
| `App_Start/WebApiConfig.cs` | Excluded from compilation (API routing via attribute routing). |
| `Properties/AssemblyInfo.cs` | Excluded from compilation (SDK auto-generates these). |
| `Views/_ViewImports.cshtml` | **New file** — `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`. |

#### Code Migration

| File | Change |
|------|--------|
| `Models/CatalogDBContext.cs` | **EF6 → EF Core**: `DbContext(string connStr)` → `DbContext(DbContextOptions<T>)`. `DbModelBuilder` → `ModelBuilder`. `EntityTypeConfiguration<T>` → `EntityTypeBuilder<T>`. `HasDatabaseGeneratedOption(None)` → `ValueGeneratedNever()`. `HasRequired().WithMany().HasForeignKey()` → `HasOne().WithMany().HasForeignKey().IsRequired()`. |
| `Models/CatalogItemHiLoGenerator.cs` | **EF6 → EF Core**: `db.Database.SqlQuery<Int64>()` → `db.Database.SqlQueryRaw<long>()` (EF Core 7+). |
| `Models/Infrastructure/CatalogDBInitializer.cs` | **EF6 → EF Core**: Removed `CreateDatabaseIfNotExists<T>` base class. Added `Initialize(context, contentRootPath, useCustomizationData)` method. `EnsureCreated()` replaces old initializer pattern. `ExecuteSqlCommand` → `ExecuteSqlRaw`. `SqlQuery<Int64>` → `SqlQueryRaw<long>`. `HostingEnvironment.ApplicationPhysicalPath` → `contentRootPath` parameter. `ConfigurationManager.AppSettings` → parameter injection. |
| `Models/Infrastructure/PreconfiguredData.cs` | Removed `using System.Web`. |
| `Models/CatalogBrand.cs` | Removed `using System.Web`. |
| `Models/CatalogType.cs` | Removed `using System.Web`. |
| `Models/CatalogItemHiLoGenerator.cs` | Removed `using System.Web`. Added `using Microsoft.EntityFrameworkCore`. |
| `Services/CatalogService.cs` | `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`. |
| `Services/ICatalogService.cs` | Cleaned up unused imports. |
| `Modules/ApplicationModule.cs` | Replaced Autofac module with documentation-only static class. DI in `Program.cs`. |
| `Controllers/CatalogController.cs` | `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`. `HttpStatusCodeResult(BadRequest)` → `BadRequest()`. `HttpNotFound()` → `NotFound()`. `[Bind(Include="...")]` → `[Bind("...")]`. Updated `AddUriPlaceHolder` to use ASP.NET Core `Request.Scheme`. Added `using Microsoft.AspNetCore.Mvc.Rendering`. |
| `Controllers/PicController.cs` | `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`. Added `IWebHostEnvironment` injection. `Server.MapPath("~/Pics")` → `Path.Combine(_env.ContentRootPath, "Pics")`. `HttpStatusCodeResult(BadRequest)` → `BadRequest()`. `HttpNotFound()` → `NotFound()`. |
| `Controllers/WebApi/BrandsController.cs` | `ApiController` → `ControllerBase` with `[ApiController]`. `System.Web.Http` → `Microsoft.AspNetCore.Mvc`. `IHttpActionResult` → `IActionResult`. Removed `System.Runtime.Remoting.Messaging` (removed in .NET Core). |
| `Controllers/WebApi/FilesController.cs` | `ApiController` → `ControllerBase` with `[ApiController]`. `System.Web.Http` → `Microsoft.AspNetCore.Mvc`. `HttpResponseMessage + StreamContent` → `File(stream, contentType)`. |
| `Controllers/Api/CatalogController.cs` | `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`. Return type changed to `IActionResult`. |
| `Views/Shared/_Layout.cshtml` | Replaced `@Styles.Render` and `@Scripts.Render` (System.Web.Optimization) with CDN links. Replaced `HttpContext.Current.Session` with `System.Environment.MachineName`. |
| `Views/Catalog/Create.cshtml` | Replaced `@Scripts.Render("~/bundles/jqueryval")` with CDN links for jQuery Validate. |
| `Views/Catalog/Edit.cshtml` | Replaced `@Scripts.Render("~/bundles/jqueryval")` with CDN links for jQuery Validate. |
| `Views/Catalog/Index.cshtml` | Replaced `@Html.Partial` with `@await Html.PartialAsync` (MVC1000 fix). |

---

## Remaining Warnings (4 — all identical)

| Warning | Package | Reason | Action |
|---------|---------|--------|--------|
| NU1902 | log4net 2.0.17 | Moderate severity vulnerability (GHSA-4f7c-pmjv-c25w) | See Next Steps |

---

## Next Steps

1. **log4net vulnerability (NU1902)**: All available versions of log4net in the package cache (including the latest 2.0.17) are flagged by advisory GHSA-4f7c-pmjv-c25w. Options: (a) Replace log4net with `Microsoft.Extensions.Logging` + a modern provider (`Serilog`, `NLog`); (b) Accept the risk and add `<NuGetAuditSuppress>` if log4net configuration is only loaded from trusted local files; (c) Check for a later log4net version (≥ 2.0.16 fixed the underlying XXE issue but the NuGet advisory may not be cleared yet).

2. **Serializing.cs binary format change**: `BinaryFormatter` was replaced with `System.Text.Json`. Any existing clients (like `FilesController`) that consumed binary-serialized data will now receive JSON. If binary wire format compatibility is needed, document the format change and update any consumers.

3. **eShopLegacyMVC static file serving**: Static files (CSS, JS, images) are served from the project's legacy `Content/`, `Scripts/`, `Images/`, `Pics/`, and `fonts/` directories via additional `UseStaticFiles` middleware. For production, move these to a `wwwroot/` folder for standard ASP.NET Core static file serving.

4. **EF Core database initialization**: `CatalogDBInitializer` uses `context.Database.EnsureCreated()` for schema creation. For production, migrate to EF Core migrations (`dotnet ef migrations add Initial && dotnet ef database update`) for proper schema versioning. The HiLo sequence scripts (`dbo.catalog_hilo.Sequence.sql` etc.) are run via raw SQL — these may need adjustment for your target SQL Server version.

5. **Session support**: The original `Session_Start` that stored `MachineName` and `SessionStartTime` in `_Layout.cshtml` has been simplified to show `Environment.MachineName` and `DateTime.UtcNow` directly. If per-session tracking is required, re-add session middleware and store values in `CatalogController.OnActionExecuting`.

6. **Application Insights**: The `Microsoft.ApplicationInsights.*` packages from the original project were removed. If telemetry is needed, add `Microsoft.ApplicationInsights.AspNetCore` and configure it in `Program.cs`.

7. **Views/Web.config in eShopLegacyMVC**: The `Views/Web.config` was excluded in the csproj. In the SDK-style project it is not used; `_ViewImports.cshtml` handles global Razor configuration.
