# Migration Summary: eShopLegacyMVC.sln → .NET 10

## Result
`dotnet build eShopLegacyMVC.sln` → **Build succeeded. 0 Error(s). 0 Warning(s).**

All three projects now target `net10.0`.

---

## Changes Made

### eShopLegacy.Utilities
- **csproj**: Changed `TargetFramework` from `netstandard2.0` → `net10.0`, added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to suppress CS0579 duplicate attribute errors from the legacy `Properties/AssemblyInfo.cs`, removed unnecessary `Microsoft.CSharp` and `System.Data.DataSetExtensions` packages.
- **Serializing.cs**: Replaced removed `BinaryFormatter` (eliminated in .NET 9) with `System.Text.Json.JsonSerializer`. Behavioral note: serialized format changed from binary to JSON; callers receiving the stream (FilesController) are not affected as they only pipe bytes to the HTTP response.

### eShopLegacyMVC (src/eShopLegacyMVC)
Full .NET Framework 4.7.2 → .NET 10 conversion.

#### Project file
Replaced old non-SDK `.csproj` with an SDK-style `Microsoft.NET.Sdk.Web` project targeting `net10.0`. Excluded legacy files (`Global.asax`, `Web.config`, `App_Start/*`, `Properties/AssemblyInfo.cs`) from compilation and removed all .NET Framework / System.Web packages.

New dependencies:
- `Autofac 8.3.0` + `Autofac.Extensions.DependencyInjection 10.0.0`
- `log4net 3.3.2`
- `Microsoft.EntityFrameworkCore 10.0.10` + SqlServer + Design
- `Newtonsoft.Json 13.0.4`

#### Application startup
Added `Program.cs` (minimal hosting model replacing `Global.asax`/`MvcApplication`) and `appsettings.json` / `appsettings.Development.json` (replacing `Web.config` app settings and connection strings).

#### Models
- **CatalogDBContext.cs**: Migrated from EF6 (`System.Data.Entity.DbContext`) to EF Core (`Microsoft.EntityFrameworkCore.DbContext`). Replaced `EntityTypeConfiguration<T>` with `EntityTypeBuilder<T>`. Constructor now takes `DbContextOptions<CatalogDBContext>` (injected).
- **CatalogItem.cs**: Added nullable annotations (`string? Description`, `string? PictureFileName`, `string? PictureUri`); required navigation properties use `= null!` (EF Core convention for properties always populated via `.Include()`).
- **CatalogBrand.cs**, **CatalogType.cs**: Removed `using System.Web;`.
- **CatalogItemHiLoGenerator.cs**: Replaced `db.Database.SqlQuery<Int64>` (EF6) with `db.Database.SqlQueryRaw<long>(...).AsEnumerable().Single()` (EF Core).
- **CatalogDBInitializer.cs**: Replaced `CreateDatabaseIfNotExists<T>` (EF6) with an injectable class calling `context.Database.EnsureCreated()`. Replaced `HostingEnvironment.ApplicationPhysicalPath` with `IHostEnvironment.ContentRootPath`, `ConfigurationManager.AppSettings` with `IConfiguration`, `context.Database.ExecuteSqlCommand` with `context.Database.ExecuteSqlRaw`.
- **PreconfiguredData.cs**: Removed `using System.Web;`.

#### Services
- **ICatalogService.cs**: `FindCatalogItem` return type marked `CatalogItem?`.
- **CatalogService.cs**: `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`.
- **CatalogServiceMock.cs**: Removed `using System.Web;`.

#### Controllers
- **CatalogController.cs**: `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`; `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`; `HttpNotFound()` → `NotFound()`; `Request.Url.Scheme` → `Request.Scheme`; logger uses `typeof(CatalogController)`.
- **PicController.cs**: `using System.Web.Mvc` → ASP.NET Core MVC; injected `IWebHostEnvironment` to replace `Server.MapPath("~/Pics")`; `HttpStatusCodeResult`/`HttpNotFound()` replaced with `BadRequest()`/`NotFound()`.
- **BrandsController.cs** (WebApi): `ApiController` (System.Web.Http) → `ControllerBase` (ASP.NET Core); `IHttpActionResult` → `IActionResult`; added `[ApiController]` + `[Route]` attributes; removed `System.Runtime.Remoting.Messaging` import.
- **FilesController.cs** (WebApi): Same Web API migration; returns `File(stream, ...)` instead of `HttpResponseMessage`.
- **Api/CatalogController.cs**: `using System.Web.Mvc` → ASP.NET Core.

#### Modules
- **ApplicationModule.cs**: Removed `Autofac.Integration.Mvc` / `Autofac.Integration.WebApi` imports; uses plain `Autofac` (no framework-specific integrations needed).

#### Views
- **Views/_ViewImports.cshtml**: Added (required for ASP.NET Core Razor views).
- **Views/Shared/_Layout.cshtml**: Replaced `@Styles.Render(...)` / `@Scripts.Render(...)` (System.Web.Optimization) with direct `<link>` and `<script>` tags; removed `HttpContext.Current.Session` footer reference.
- **Views/Catalog/Create.cshtml**, **Edit.cshtml**: Replaced `@Scripts.Render("~/bundles/jqueryval")` with direct script tags.
- **Views/Catalog/Index.cshtml**: Replaced `@Html.Partial("CatalogTable", ...)` with `<partial>` tag helper (fixes MVC1000 deadlock warning).

### eShopPorted
Already partially migrated; completed the following:

- **eShopPorted.csproj**: Removed incompatible legacy packages (`Autofac.Mvc5`, `WebGrease`, `Antlr4`, `Microsoft.CSharp`). Updated `Autofac` → 8.3.0, `Autofac.Extensions.DependencyInjection` → 10.0.0, `log4net` → 3.3.2.
- **Program.cs**: Migrated from `WebHost.CreateDefaultBuilder` (ASP.NET Core 2.x pattern) to `Host.CreateDefaultBuilder` with `UseServiceProviderFactory(new AutofacServiceProviderFactory())` (Autofac 8.x pattern).
- **Startup.cs**: Changed `ConfigureServices` signature from `IServiceProvider` return (removed in ASP.NET Core 3+) to `void`. Added `ConfigureContainer(ContainerBuilder)` for Autofac module registration. Replaced obsolete `IHostingEnvironment` with `IWebHostEnvironment`. Updated `app.UseMvc()` to `app.UseRouting()` + `app.UseEndpoints()`.
- **Controllers/PicController.cs**: Replaced `System.Web.Mvc` with ASP.NET Core MVC; injected `IWebHostEnvironment`; replaced `HttpStatusCodeResult`/`HttpNotFound()` with `BadRequest()`/`NotFound()`.
- **Models/Infrastructure/PreconfiguredData.cs**: Removed `using System.Web;`.
- **Views/Catalog/Index.cshtml**: Replaced `Html.Partial` with `<partial>` tag helper.

---

## Next Steps
- The `eShopLegacyMVC` project's `CatalogDBInitializer` now uses `EnsureCreated()` + seed logic instead of EF Migrations. Consider adding proper EF Core migrations (`dotnet ef migrations add Initial`) for production use.
- `SqlQueryRaw` in `CatalogDBInitializer.GetSequenceIdFromSelectedDBSequence` and `CatalogItemHiLoGenerator` uses string interpolation with hardcoded constant sequence names (not user input) — no actual SQL injection risk, but EF1002 is suppressed with a pragma. Consider replacing with a safer formulated approach if the sequence name ever becomes variable.
- `eShopPorted` already has EF Core Migrations in the `Migrations/` folder; `eShopLegacyMVC` does not — it uses `EnsureCreated()` as a drop-in replacement for the EF6 `CreateDatabaseIfNotExists` initializer.
- Static assets in `eShopLegacyMVC` are referenced from `~/Content/`, `~/Scripts/`, `~/Images/` paths — these should be served from the `wwwroot/` folder. Ensure the static files are moved or symlinked there before production deployment.
