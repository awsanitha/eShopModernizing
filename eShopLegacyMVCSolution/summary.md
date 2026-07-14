# eShopLegacyMVC Migration Summary

## Migration Status: ✅ COMPLETE — 0 Compilation Errors

**Build Result:** `dotnet build eShopLegacyMVC.sln` exits with code 0, zero errors, 23 warnings (non-blocking nullable/style suggestions).

---

## What Was Migrated

### Target Framework
All projects now target modern .NET:
- **eShopLegacyMVC** → `net10.0` (was .NET Framework 4.7.2)
- **eShopPorted** → `net10.0` (already targeted net10.0 but had incompatible packages)
- **eShopLegacy.Utilities** → `netstandard2.0` (already correct, fixed AssemblyInfo conflict)

---

### eShopLegacyMVC (Primary Migration)

**Project File (eShopLegacyMVC.csproj)**
- Completely replaced legacy verbose XML format with SDK-style `<Project Sdk="Microsoft.NET.Sdk.Web">`
- Removed all legacy `<Reference>` entries, legacy NuGet packages (Autofac.Mvc5, WebGrease, Antlr, Microsoft.AspNet.*, System.Web.*)
- Added modern packages: Autofac 8.x, Autofac.Extensions.DependencyInjection, EF Core 10.0, log4net 2.0.17, Newtonsoft.Json 13.x
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to avoid CS0579 duplicate assembly attribute errors with existing Properties/AssemblyInfo.cs

**Startup/Hosting**
- Created `Program.cs` using the minimal hosting model (`WebApplication.CreateBuilder`)
- Replaced `Global.asax.cs` with empty stub
- Migrated `Web.config` settings to `appsettings.json` / `appsettings.Development.json`
- Integrated Autofac using `AutofacServiceProviderFactory` (modern pattern)

**Controllers**
- `Controllers/CatalogController.cs`: `System.Web.Mvc.Controller` → `Microsoft.AspNetCore.Mvc.Controller`, action returns changed to `IActionResult`, HTTP status codes updated (`HttpStatusCodeResult` → `BadRequest()`, `HttpNotFound()` → `NotFound()`)
- `Controllers/PicController.cs`: Replaced `Server.MapPath` with `IWebHostEnvironment.ContentRootPath` injection
- `Controllers/Api/CatalogController.cs`: Migrated to `ControllerBase` with `[ApiController]`
- `Controllers/WebApi/BrandsController.cs`: Migrated from `ApiController` (System.Web.Http) to `ControllerBase` (ASP.NET Core)
- `Controllers/WebApi/FilesController.cs`: Same Web API migration; removed `System.Runtime.Remoting.Messaging` dependency

**Models**
- `Models/CatalogDBContext.cs`: EF6 `DbContext` → EF Core `DbContext`; replaced `EntityTypeConfiguration<T>` (EF6) with `EntityTypeBuilder<T>` (EF Core); replaced `DbModelBuilder` with `ModelBuilder`; constructor now takes `DbContextOptions<CatalogDBContext>`
- `Models/CatalogItemHiLoGenerator.cs`: Replaced `db.Database.SqlQuery<Int64>()` (EF6) with `db.Database.SqlQueryRaw<long>()` (EF Core)
- `Models/CatalogBrand.cs`, `Models/CatalogType.cs`: Removed `using System.Web` imports
- `Models/Infrastructure/CatalogDBInitializer.cs`: Replaced EF6 `CreateDatabaseIfNotExists<T>` with plain class; replaced `HostingEnvironment.ApplicationPhysicalPath` (System.Web.Hosting) with `IHostEnvironment.ContentRootPath`; replaced `context.Database.ExecuteSqlCommand` with `context.Database.ExecuteSqlRaw`; replaced `ConfigurationManager.AppSettings` with `IConfiguration`
- `Models/Infrastructure/PreconfiguredData.cs`: Removed `using System.Web`

**Services**
- `Services/CatalogService.cs`: Updated EF Core imports; removed EF6-specific patterns
- `Services/ICatalogService.cs`: Updated return types to be nullable-compatible
- `Services/CatalogServiceMock.cs`: Removed `System.Web` imports

**App_Start (emptied)**
- `App_Start/BundleConfig.cs`, `RouteConfig.cs`, `FilterConfig.cs`, `WebApiConfig.cs`: Replaced with empty stubs (logic moved to Program.cs)

**Views**
- `Views/Shared/_Layout.cshtml`: Replaced `@Scripts.Render`/`@Styles.Render` (System.Web.Optimization) with CDN links; removed `HttpContext.Current.Session` reference
- `Views/Catalog/Create.cshtml`, `Views/Catalog/Edit.cshtml`: Replaced `@Scripts.Render("~/bundles/jqueryval")` with CDN script tags
- Created `Views/_ViewImports.cshtml` with ASP.NET Core namespace imports and tag helper registration

---

### eShopPorted

**Project File (eShopPorted.csproj)**
- Removed incompatible legacy packages: Autofac.Mvc5, WebGrease, Antlr, Microsoft.AspNet.Mvc, Microsoft.Web.Infrastructure
- Added modern packages: Autofac 8.x, Autofac.Extensions.DependencyInjection, EF Core 10.0, Newtonsoft.Json 13.x
- Removed `Microsoft.CSharp` (unnecessary in .NET 10)

**Startup/Hosting**
- Replaced `Program.cs` (old `IWebHostBuilder`/`WebHost.CreateDefaultBuilder`) with modern `WebApplication.CreateBuilder` minimal API pattern
- Replaced `Startup.cs` (old `IHostingEnvironment`, `app.UseMvc()`, `IServiceProvider` return) with empty stub
- Updated `_Layout.cshtml` to remove reference to deprecated `eShopPorted.Startup.StartTime`

**Controllers**
- `Controllers/PicController.cs`: Migrated from `System.Web.Mvc.Controller` to `Microsoft.AspNetCore.Mvc.Controller`; replaced `HttpStatusCodeResult(HttpStatusCode.BadRequest)` with `BadRequest()`, `HttpNotFound()` with `NotFound()`; injected `IWebHostEnvironment` for path resolution

---

### eShopLegacy.Utilities

**Project File**
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to prevent CS0579 duplicate attribute errors with the legacy `Properties/AssemblyInfo.cs`
- Added `<LangVersion>latest</LangVersion>` to enable C# 8+ syntax in netstandard2.0
- Added `System.Text.Json` package reference (required since netstandard2.0 doesn't include it built-in)

**Serializing.cs**
- Replaced `BinaryFormatter` (removed in .NET 9) with `System.Text.Json.JsonSerializer`

---

## Next Steps (Non-blocking)

1. **Nullable warnings (CS8618/CS8603)**: 23 remaining nullable reference warnings in `eShopPorted`. These can be fixed by adding null-forgiving operators (`!`) or making properties nullable where appropriate.
2. **MVC1000 warning**: `eShopPorted/Views/Catalog/Index.cshtml` uses `@Html.Partial()` — consider migrating to `<partial>` tag helper or `@await Html.PartialAsync()`.
3. **EF Core Migrations**: The `eShopPorted` project has existing migrations targeting EF 3.x. These may need to be regenerated for EF Core 10.0. Run `dotnet ef migrations add InitialMigration` after configuring a database.
4. **Static file setup**: The `eShopLegacyMVC` project serves static files from `Content/`, `Scripts/`, `Images/`, `Pics/`, `fonts/` folders. These should be moved to a `wwwroot/` directory for proper ASP.NET Core static file serving. At runtime, `app.UseStaticFiles()` will look in `wwwroot/` by default.
5. **Log4net configuration**: The `log4Net.xml` file is referenced in `Properties/AssemblyInfo.cs` and `Program.cs`. Ensure it is copied to the output directory or use a proper logger configuration for production.
6. **Database connection strings**: Update `appsettings.json` and `appsettings.Development.json` with real connection strings for your environment.
7. **BinaryFormatter replacement**: The `Serializing` class in `eShopLegacy.Utilities` now uses `System.Text.Json`. Consumers should verify the new JSON-based serialization is compatible with their use case.
