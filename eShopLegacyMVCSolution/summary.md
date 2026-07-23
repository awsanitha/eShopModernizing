# eShopLegacyMVC Migration Summary

## Status: ✅ Build Succeeds — 0 Errors

`dotnet build eShopLegacyMVC.sln` → **Build succeeded. 4 Warning(s). 0 Error(s).**

---

## Changes Made

### 1. `eShopLegacy.Utilities`
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to the `.csproj`  
  **Fix:** Duplicate assembly attribute CS0579 errors caused by legacy `Properties/AssemblyInfo.cs` conflicting with SDK auto-generation.

### 2. `src/eShopLegacyMVC` — Full migration from .NET Framework 4.7.2 to net10.0

**Project file:**
- Replaced old-format `.csproj` (MSBuild-style with `Microsoft.WebApplication.targets`) with SDK-style `<Project Sdk="Microsoft.NET.Sdk.Web">` targeting `net10.0`
- Added modern package references: EF Core 10, Autofac 4.9.1, log4net
- Excluded legacy files from compilation: `Global.asax.cs`, `Properties/AssemblyInfo.cs`, `App_Start/BundleConfig.cs`, `FilterConfig.cs`, `RouteConfig.cs`, `WebApiConfig.cs`

**New files added:**
- `Program.cs` — Replaces Global.asax application startup using `WebHost.CreateDefaultBuilder` + `Startup`
- `Startup.cs` — ASP.NET Core startup (services registration, pipeline configuration, static file serving)
- `appsettings.json` — Migrated from `Web.config` (connection strings, UseMockData, UseCustomizationData)
- `appsettings.Development.json` — Dev override with mock data enabled
- `Views/_ViewImports.cshtml` — Replaces `Views/Web.config` namespace declarations

**Code migrations:**
- `Models/CatalogDBContext.cs` — EF6 `DbContext` + `EntityTypeConfiguration<T>` → EF Core `DbContext` + `EntityTypeBuilder<T>`, `HasRequired` → `HasOne`, `HasDatabaseGeneratedOption(None)` → `ValueGeneratedNever()`
- `Models/CatalogItemHiLoGenerator.cs` — `db.Database.SqlQuery<T>` → `db.Database.SqlQueryRaw<T>` (EF Core)
- `Models/Infrastructure/CatalogDBInitializer.cs` — EF6 `CreateDatabaseIfNotExists<T>` + `System.Web.Hosting.HostingEnvironment` → EF Core `EnsureCreated()` + `IWebHostEnvironment`, `ExecuteSqlCommand` → `ExecuteSqlRaw`, `SqlQuery<T>` → `SqlQueryRaw<T>`
- `Models/CatalogBrand.cs`, `CatalogType.cs` — Removed `System.Web` imports
- `Controllers/CatalogController.cs` — `System.Web.Mvc.Controller` → `Microsoft.AspNetCore.Mvc.Controller`, `HttpStatusCodeResult` → `BadRequest()`/`NotFound()`
- `Controllers/PicController.cs` — `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`, `Server.MapPath` → `IWebHostEnvironment.ContentRootPath`
- `Controllers/WebApi/BrandsController.cs`, `FilesController.cs` — `System.Web.Http.ApiController` → `Microsoft.AspNetCore.Mvc.ControllerBase` with `[ApiController]` attribute
- `Controllers/Api/CatalogController.cs` — `System.Web.Mvc.Controller` → `ControllerBase`
- `Services/CatalogService.cs` — EF6 `System.Data.Entity.EntityState` → EF Core `Microsoft.EntityFrameworkCore.EntityState`
- `Services/CatalogServiceMock.cs` — Nullable return type alignment
- `Services/ICatalogService.cs` — Nullable return type for `FindCatalogItem`
- `Modules/ApplicationModule.cs` — Updated DI registration (removed EF6 CatalogDBContext from Autofac, handled via ASP.NET Core DI)
- `Views/Shared/_Layout.cshtml` — Replaced `@Styles.Render()`/`@Scripts.Render()` with CDN links for Bootstrap/jQuery, removed `HttpContext.Current.Session`
- `Views/Catalog/Create.cshtml`, `Edit.cshtml` — Removed `@Scripts.Render("~/bundles/jqueryval")`

### 3. `eShopPorted` — Package cleanup and code quality fixes

**Package updates:**
- Removed: `Autofac.Mvc5 4.0.2`, `WebGrease 1.6.0`, `Antlr4 4.6.6`, `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.Razor`, `Microsoft.AspNet.WebPages`, `Microsoft.Web.Infrastructure`, `Microsoft.CSharp`
- These were .NET Framework TFM-fallback packages no longer needed in net10.0

**Code fixes:**
- `Controllers/PicController.cs` — `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`, `Server.MapPath` → `IWebHostEnvironment`
- `Startup.cs` — Fully qualified `Microsoft.AspNetCore.Hosting.IHostingEnvironment` to resolve ambiguity; updated to use `UseEndpoints`

---

## Remaining Warnings (non-blocking)

| Code | Project | Description |
|------|---------|-------------|
| NU1902 | eShopLegacyMVC | log4net 2.0.10 moderate vulnerability advisory |
| NU1902 | eShopPorted | log4net 2.0.10 moderate vulnerability advisory |

## Next Steps

- **log4net upgrade**: Update log4net to 2.0.17+ to resolve the NU1902 security advisory. (Requires internet access to download newer NuGet package.)
- **WebHost deprecation**: Both projects use the `WebHost.CreateDefaultBuilder` + `Startup` pattern which is marked as obsolete in .NET 8+. Consider migrating to the minimal hosting model (`WebApplication.CreateBuilder`) when upgrading Autofac to version 7.x or later (which supports `UseServiceProviderFactory` natively). This requires `Autofac.Extensions.DependencyInjection >= 7.0`.
- **EF Core migrations**: The migrated `src/eShopLegacyMVC` project uses `EnsureCreated()` for database setup (matches EF6 behavior). For production, add proper EF Core migrations and remove `EnsureCreated`.
- **Session migration**: The legacy `Global.asax.cs` stored `MachineName` and `SessionStartTime` in session; these are now displayed as static properties in the layout. If session state is needed elsewhere, add `services.AddSession()` and `app.UseSession()` back.
- **IHostingEnvironment**: `IHostingEnvironment` is deprecated; both projects should eventually move to `IWebHostEnvironment`. The src/eShopLegacyMVC project already uses `IWebHostEnvironment` correctly; eShopPorted still uses the deprecated type in Startup.cs via the fully-qualified name.
