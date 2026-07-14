# eShopLegacyMVC Migration Summary

## Status: BUILD SUCCEEDED — 0 Errors

`dotnet build eShopLegacyMVC.sln` exits with code 0. All three projects build cleanly.

---

## Migration Scope

The `eShopLegacyMVC` project (`src/eShopLegacyMVC/`) was migrated from .NET Framework 4.7.2 to **net10.0**.

---

## Changes Made

### Project File (`eShopLegacyMVC.csproj`)
- Replaced legacy XML-style `.csproj` (targeting `net472`) with SDK-style project targeting `net10.0`
- Removed all legacy framework references: `System.Web.*`, `Autofac.Mvc5`, `Autofac.Integration.WebApi`, `EntityFramework` (EF6), `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.WebApi`, `System.Web.Optimization`, `WebGrease`, `Antlr`, ApplicationInsights packages
- Added modern packages: `Autofac 8.3.0`, `Autofac.Extensions.DependencyInjection 10.0.0`, `Microsoft.EntityFrameworkCore.SqlServer 9.0.6`, `log4net 2.0.17`, `Newtonsoft.Json 13.0.3`
- Added `GenerateAssemblyInfo=false` to prevent duplicate attribute conflicts with legacy `Properties/AssemblyInfo.cs`
- Excluded legacy framework files: `Global.asax`, `Global.asax.cs`, `Web.config`, `App_Start/BundleConfig.cs`, `App_Start/FilterConfig.cs`, `App_Start/RouteConfig.cs`, `App_Start/WebApiConfig.cs`

### Startup (`Program.cs` — new file)
- Replaced `Global.asax.cs` + `App_Start/*.cs` with ASP.NET Core `Program.cs`
- Autofac registered via `AutofacServiceProviderFactory`
- EF Core `DbContext` registered via `services.AddDbContext<CatalogDBContext>`
- Routes configured using `MapControllerRoute` (replaces `RouteConfig.cs` and `WebApiConfig.cs`)
- Static files served from content root (Content/, Scripts/, Images/, Pics/, fonts/)
- Database initialization on startup when not using mock data

### Configuration (`appsettings.json` — new file)
- Replaced `Web.config` with `appsettings.json`
- `UseMockData: true` (default safe value)
- `UseCustomizationData: false`
- Connection string for SQL Server (localdb)

### Data Access (`Models/CatalogDBContext.cs`)
- Migrated from EF6 `DbContext` to EF Core
- `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)`
- `EntityTypeConfiguration<T>` → `EntityTypeBuilder<T>` (inline Fluent API)
- `.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)` → `.ValueGeneratedNever()`
- `.HasRequired<>().WithMany().HasForeignKey()` → `.HasOne<>().WithMany().HasForeignKey()`
- Constructor takes `DbContextOptions<CatalogDBContext>` (EF Core pattern)

### Data Access (`Models/CatalogItemHiLoGenerator.cs`)
- Removed `using System.Web`
- `db.Database.SqlQuery<Int64>()` → `db.Database.SqlQueryRaw<long>()` (EF Core)
- Added `using Microsoft.EntityFrameworkCore`

### Data Access (`Models/Infrastructure/CatalogDBInitializer.cs`)
- Replaced EF6 `CreateDatabaseIfNotExists<CatalogDBContext>` base class with plain class with `Seed()` method
- `ConfigurationManager.AppSettings["UseCustomizationData"]` → `IConfiguration.GetValue<bool>("UseCustomizationData")`
- `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`
- `context.Database.SqlQuery<Int64>()` → `context.Database.SqlQueryRaw<long>()`
- `context.Database.ExecuteSqlCommand()` → `context.Database.ExecuteSqlRaw()`
- Added seeding guard: returns early if data already exists
- Added `using Microsoft.EntityFrameworkCore`

### Service Layer (`Services/CatalogService.cs`)
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- Return type `CatalogItem` → `CatalogItem?` (nullable-aware)

### Controllers (`Controllers/CatalogController.cs`)
- `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`
- `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
- `HttpNotFound()` → `NotFound()`
- `[Bind(Include = "...")]` → `[Bind("...")]` (ASP.NET Core syntax)
- `this.Request.Url.Scheme` removed; `Url.RouteUrl()` used directly
- Added null check before `RemoveCatalogItem`

### Controllers (`Controllers/PicController.cs`)
- `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`
- `Server.MapPath("~/Pics")` → `IWebHostEnvironment.ContentRootPath + "/Pics"`
- `IWebHostEnvironment` injected via constructor
- `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
- `HttpNotFound()` → `NotFound()`
- `switch` → `switch expression` (modern C# pattern)

### Controllers (`Controllers/WebApi/BrandsController.cs`)
- Replaced `ApiController` (System.Web.Http) with ASP.NET Core `ControllerBase`
- Removed `using System.Web.Http`, `using System.Runtime.Remoting.Messaging`
- `IHttpActionResult` → `IActionResult` / `ActionResult<T>`
- `ResponseMessage(new HttpResponseMessage(...))` → `NotFound()` / `Ok()`
- Added `[ApiController]` and `[Route("api/[controller]")]` attributes

### Controllers (`Controllers/WebApi/FilesController.cs`)
- Replaced `ApiController` with ASP.NET Core `ControllerBase`
- `BinaryFormatter` (removed in .NET 9+) replaced with `System.Text.Json`
- `HttpResponseMessage` replaced with `IActionResult`

### Controllers (`Controllers/Api/CatalogController.cs`)
- `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`
- Inherits from `ControllerBase`
- `Json(...)` → `Ok(new {...})`

### DI Module (`Modules/ApplicationModule.cs`)
- Removed `Autofac.Integration.Mvc` dependency
- Standard Autofac module compatible with ASP.NET Core

### Views
- `Views/_ViewImports.cshtml` — new file adding Tag Helper support
- `Views/Shared/_Layout.cshtml` — replaced `@Styles.Render(...)` / `@Scripts.Render(...)` bundles with direct `<link>` and `<script>` tags; removed `HttpContext.Current.Session` usage
- `Views/Catalog/Create.cshtml` and `Edit.cshtml` — replaced `@Scripts.Render("~/bundles/jqueryval")` with direct script tags

### Models (`Models/CatalogBrand.cs`, `Models/CatalogType.cs`)
- Removed `using System.Web`
- Added nullable-safe string defaults

### Utilities (`eShopLegacy.Utilities`)
- Target framework updated to `net10.0`
- `BinaryFormatter` (removed in .NET 9+) replaced with `System.Text.Json`
- Added `GenerateAssemblyInfo=false` to avoid conflict with legacy `AssemblyInfo.cs`

---

## Next Steps

- The `eShopPorted` project still references `Autofac.Mvc5`, `Microsoft.AspNet.Mvc`, `WebGrease` and other legacy packages that produce NU1701 compatibility warnings. Those are pre-existing issues in the `eShopPorted` project and were not part of the `eShopLegacyMVC` migration scope.
- The `log4net 2.0.17` package has a known moderate severity vulnerability (GHSA-4f7c-pmjv-c25w). Consider upgrading to `log4net 2.0.18+` or replacing with `Microsoft.Extensions.Logging` when available.
- The `PicController` serves images from `ContentRootPath/Pics/`. Ensure the `Pics/` directory is present in the deployment or configure it appropriately.
- The `CatalogDBInitializer` uses raw SQL (`SELECT NEXT VALUE FOR ...`) for HiLo sequences. Ensure the database is SQL Server or compatible.
- When `UseMockData=false`, the app expects SQL Server. For production, set `UseMockData: false` and configure the connection string in environment variables or secrets.
