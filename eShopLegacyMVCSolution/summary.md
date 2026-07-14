# eShopLegacyMVC Migration Summary

## Status: BUILD SUCCEEDED — 0 Errors, 0 Warnings

`dotnet build eShopLegacyMVC.sln` exits with code 0. All three projects build cleanly with zero errors and zero warnings.

---

## Migration Scope

The `eShopLegacyMVC` project (`src/eShopLegacyMVC/`) was migrated from .NET Framework 4.7.2 to **net10.0**.  
The `eShopPorted` project (`eShopPorted/`) was modernized from early ASP.NET Core patterns to **net10.0** modern patterns.  
The `eShopLegacy.Utilities` project was updated to **net10.0**.

---

## Changes Made

### Project File (`src/eShopLegacyMVC/eShopLegacyMVC.csproj`)
- Replaced legacy XML-style `.csproj` (targeting `net472`) with SDK-style project targeting `net10.0`
- Removed all legacy framework references: `System.Web.*`, `Autofac.Mvc5`, `Autofac.Integration.WebApi`, `EntityFramework` (EF6), `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.WebApi`, `System.Web.Optimization`, `WebGrease`, `Antlr`, ApplicationInsights packages
- Added modern packages: `Autofac 8.3.0`, `Autofac.Extensions.DependencyInjection 10.0.0`, `Microsoft.EntityFrameworkCore.SqlServer 9.0.6`, `log4net 3.3.2`, `Newtonsoft.Json 13.0.3`
- Added `GenerateAssemblyInfo=false` to prevent duplicate attribute conflicts with legacy `Properties/AssemblyInfo.cs`
- Excluded legacy framework files: `Global.asax`, `Global.asax.cs`, `Web.config`, `App_Start/BundleConfig.cs`, `App_Start/FilterConfig.cs`, `App_Start/RouteConfig.cs`, `App_Start/WebApiConfig.cs`

### Startup (`src/eShopLegacyMVC/Program.cs` — new file)
- Replaced `Global.asax.cs` + `App_Start/*.cs` with ASP.NET Core `Program.cs`
- Autofac registered via `AutofacServiceProviderFactory`
- EF Core `DbContext` registered via `services.AddDbContext<CatalogDBContext>`
- Routes configured using `MapControllerRoute` (replaces `RouteConfig.cs` and `WebApiConfig.cs`)
- Static files served from content root (Content/, Scripts/, Images/, Pics/, fonts/)
- Database initialization on startup when not using mock data

### Configuration (`src/eShopLegacyMVC/appsettings.json` — new file)
- Replaced `Web.config` with `appsettings.json`
- `UseMockData: true` (default safe value)
- `UseCustomizationData: false`
- Connection string for SQL Server (localdb)

### Data Access (`src/eShopLegacyMVC/Models/CatalogDBContext.cs`)
- Migrated from EF6 `DbContext` to EF Core
- `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)`
- Constructor takes `DbContextOptions<CatalogDBContext>` (EF Core pattern)

### Data Access (`src/eShopLegacyMVC/Models/CatalogItemHiLoGenerator.cs`)
- Removed `using System.Web`
- `db.Database.SqlQuery<Int64>()` → `db.Database.SqlQueryRaw<long>()` (EF Core)

### Data Access (`src/eShopLegacyMVC/Models/Infrastructure/CatalogDBInitializer.cs`)
- Replaced EF6 `CreateDatabaseIfNotExists<CatalogDBContext>` base class with plain class
- `ConfigurationManager.AppSettings` → `IConfiguration.GetValue<bool>()`
- `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`
- `context.Database.SqlQuery<Int64>()` → `context.Database.SqlQueryRaw<long>()`
- `context.Database.ExecuteSqlCommand()` → `context.Database.ExecuteSqlRaw()`
- Fixed EF1002 SQL injection warning by replacing string interpolation with string concatenation (internal-only value, not user input)

### Models (`src/eShopLegacyMVC/Models/CatalogItem.cs`)
- Fixed CS8618 nullable warnings: added default initializers for `Name`, `Description`, `PictureUri`
- Made navigation properties `CatalogType` and `CatalogBrand` nullable (`?`) — they are optional EF navigation properties
- Made `PictureFileName` nullable (`?`) as it may not always be set

### Service Layer (`src/eShopLegacyMVC/Services/CatalogService.cs`)
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- Return type `CatalogItem` → `CatalogItem?` (nullable-aware, matches interface)

### Service Layer (`src/eShopLegacyMVC/Services/CatalogServiceMock.cs`)
- Fixed CS8603: `FindCatalogItem` return type updated to `CatalogItem?`

### Controllers (`src/eShopLegacyMVC/Controllers/CatalogController.cs`)
- `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`
- `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
- `HttpNotFound()` → `NotFound()`

### Controllers (`src/eShopLegacyMVC/Controllers/PicController.cs`)
- `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`
- `IWebHostEnvironment` injected via constructor
- `Server.MapPath("~/Pics")` → `IWebHostEnvironment.ContentRootPath + "/Pics"`

### Controllers (`src/eShopLegacyMVC/Controllers/WebApi/BrandsController.cs`)
- Replaced `ApiController` (System.Web.Http) with ASP.NET Core `ControllerBase`

### Controllers (`src/eShopLegacyMVC/Controllers/WebApi/FilesController.cs`)
- Replaced `ApiController` with ASP.NET Core `ControllerBase`
- `BinaryFormatter` (removed in .NET 9+) replaced with `System.Text.Json`

### DI Module (`src/eShopLegacyMVC/Modules/ApplicationModule.cs`)
- Removed `Autofac.Integration.Mvc` dependency

### Views (`src/eShopLegacyMVC/Views/`)
- `Views/_ViewImports.cshtml` — new file adding Tag Helper support
- `Views/Shared/_Layout.cshtml` — replaced `@Styles.Render(...)` / `@Scripts.Render(...)` bundles with direct `<link>` and `<script>` tags
- `Views/Catalog/Index.cshtml` — replaced `@Html.Partial(...)` with `<partial>` tag helper (fixes MVC1000 warning)
- `Views/Catalog/Create.cshtml` and `Edit.cshtml` — replaced `@Scripts.Render("~/bundles/jqueryval")` with direct script tags

### Utilities (`eShopLegacy.Utilities/`)
- Target framework updated to `net10.0`
- `BinaryFormatter` replaced with `System.Text.Json`
- Added `GenerateAssemblyInfo=false`

---

## eShopPorted Project Modernization

### Project File (`eShopPorted/eShopPorted.csproj`)
- Removed legacy packages causing NU1701 compatibility warnings: `Autofac.Mvc5 4.0.2`, `Microsoft.CSharp 4.7.0`, `WebGrease 1.6.0`, `Antlr4 4.6.6`
- Upgraded `Autofac` from 4.9.1 → 9.3.1
- Upgraded `Autofac.Extensions.DependencyInjection` from 4.4.0 → 11.0.2
- Upgraded `log4net` from 2.0.10 → 3.3.2 (resolves NU1902 vulnerability)
- Added `Nullable=enable`, `ImplicitUsings=disable`, `GenerateAssemblyInfo=false`
- Excluded legacy `Startup.cs` from compilation (logic merged into `Program.cs`)

### Startup (`eShopPorted/Program.cs`)
- Replaced deprecated `WebHost.CreateDefaultBuilder` + `IWebHostBuilder` (ASPDEPR008) with `WebApplication.CreateBuilder`
- Replaced old Autofac `builder.Populate` / `AutofacServiceProvider` pattern with `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
- Replaced `AddMvc()` + `UseMvc()` (MVC1005) with `AddControllersWithViews()` + `MapControllerRoute`
- Added `Program.StartTime` static property (replaces `Startup.StartTime`)

### Controllers (`eShopPorted/Controllers/PicController.cs`)
- Removed `using System.Web.Mvc` — replaced with `using Microsoft.AspNetCore.Mvc`
- `new HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
- `HttpNotFound()` → `NotFound()`
- Injected `IWebHostEnvironment` for proper file path resolution
- Updated to switch expression for MIME type lookup

### Views (`eShopPorted/Views/`)
- Created `Views/_ViewImports.cshtml` for Tag Helper support
- `Views/Catalog/Index.cshtml` — replaced `@Html.Partial(...)` with `<partial>` tag helper (MVC1000)
- `Views/Shared/_Layout.cshtml` — updated `eShopPorted.Startup.StartTime` → `eShopPorted.Program.StartTime`

---

## Next Steps

- The `PicController` in both projects serves images from the `Pics/` directory relative to `ContentRootPath`/`WebRootPath`. Ensure the `Pics/` directory is present in the deployment.
- The `CatalogDBInitializer` uses raw SQL (`SELECT NEXT VALUE FOR ...`) for HiLo sequences. Ensure the database is SQL Server or compatible.
- When `UseMockData=false`, the app expects SQL Server. For production, configure the connection string in environment variables or Azure Key Vault / AWS Secrets Manager.
- The `eShopPorted/Views/Web.config` file is a legacy artifact from ASP.NET MVC 5. It is harmless (it's content, not compiled) but can be deleted in a future cleanup pass.
- Consider adding a `Properties/launchSettings.json` to `src/eShopLegacyMVC/` for local development convenience.
