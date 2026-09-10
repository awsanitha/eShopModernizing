# Migration Summary: .NET Framework 4.7.2 → .NET 10

## Result

`dotnet build eShopLegacyMVC.sln` → **Build succeeded. 0 Error(s). 0 Warning(s).**

All 4 output assemblies built cleanly:
- `eShopLegacy.Utilities` (net10.0 + netstandard2.0)
- `eShopLegacyMVC` (net10.0)
- `eShopPorted` (net10.0)

---

## Changes Made

### eShopLegacyMVC (main migration target)

**Project file (`eShopLegacyMVC.csproj`)**
- Replaced old-style `.NET Framework 4.7.2` XML project file with SDK-style `Microsoft.NET.Sdk.Web`, targeting `net10.0`
- Solution file updated to use SDK project type GUID (`9A19103F-...`)
- Replaced all legacy packages (EF6, ASP.NET MVC 5, WebGrease, Autofac.Mvc5, ApplicationInsights, etc.) with modern equivalents:
  - `Microsoft.EntityFrameworkCore` + `.SqlServer` 10.0.12 (replaced EF6)
  - `Autofac` 8.3.0 + `Autofac.Extensions.DependencyInjection` 10.0.0 (replaced Autofac.Mvc5)
  - `log4net` 3.4.0 (vulnerability fix over 2.0.17/3.0.x)
  - `Newtonsoft.Json` 13.0.4
  - Enabled `<Nullable>enable</Nullable>`

**New files created**
- `Program.cs` — .NET hosting model with `AutofacServiceProviderFactory`
- `Startup.cs` — replaces `Global.asax` and `App_Start/*.cs`; uses `ConfigureContainer<ContainerBuilder>` for Autofac
- `appsettings.json` / `appsettings.Development.json` — migrated from `Web.config`
- `Views/_ViewImports.cshtml` — replaces `Views/Web.config` namespace imports
- `wwwroot/` — static files (Content/, Scripts/, Images/, Pics/, fonts/) moved here

**Files removed**
- `Global.asax` / `Global.asax.cs` — replaced by `Program.cs` + `Startup.cs`
- `App_Start/BundleConfig.cs` — bundling removed; direct `<link>` / `<script>` tags in `_Layout.cshtml`
- `App_Start/RouteConfig.cs`, `WebApiConfig.cs`, `FilterConfig.cs` — consolidated into `Startup.cs`
- `Properties/AssemblyInfo.cs` — handled by SDK auto-generation

**Source file migrations**

| File | Change |
|------|--------|
| `Models/CatalogDBContext.cs` | EF6 (`System.Data.Entity`) → EF Core (`Microsoft.EntityFrameworkCore`); `DbModelBuilder` → `ModelBuilder`; `EntityTypeConfiguration<T>` → `EntityTypeBuilder<T>`; constructor now accepts `DbContextOptions<T>` |
| `Models/CatalogItemHiLoGenerator.cs` | `db.Database.SqlQuery<Int64>` → `db.Database.SqlQuery<long>(...)` (EF Core 7+ raw SQL queries) |
| `Models/Infrastructure/CatalogDBInitializer.cs` | Removed `CreateDatabaseIfNotExists<T>` base; converted to injectable service with `Seed(CatalogDBContext)` method; `HostingEnvironment.ApplicationPhysicalPath` → `IHostEnvironment.ContentRootPath`; `ConfigurationManager.AppSettings` → `IConfiguration`; `Database.ExecuteSqlCommand` → `Database.ExecuteSqlRaw`; `GetSequenceIdFromSelectedDBSequence` uses EF Core `SqlQuery<long>` |
| `Models/CatalogBrand.cs`, `CatalogType.cs`, `Models/Infrastructure/PreconfiguredData.cs` | Removed `using System.Web` |
| `Models/CatalogItem.cs` | Added nullable annotations; navigation properties use `= null!` (always loaded via `Include`) |
| `Services/CatalogService.cs` | `System.Data.Entity` → `Microsoft.EntityFrameworkCore` |
| `Services/ICatalogService.cs` | `FindCatalogItem` returns `CatalogItem?` |
| `Controllers/CatalogController.cs` | `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`; `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`; `HttpNotFound()` → `NotFound()`; `new SelectList()` uses `Microsoft.AspNetCore.Mvc.Rendering`; `Request.Url.Scheme` → `Request.Scheme` |
| `Controllers/PicController.cs` | `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`; `Server.MapPath("~/Pics")` → `IWebHostEnvironment.WebRootPath + "/Pics"` |
| `Controllers/WebApi/BrandsController.cs` | `System.Web.Http.ApiController` → `ControllerBase`; `IHttpActionResult` → `IActionResult`; removed `System.Runtime.Remoting.Messaging` (no .NET Core equivalent) |
| `Controllers/WebApi/FilesController.cs` | `ApiController` → `ControllerBase`; `HttpResponseMessage` → `File(stream, contentType)` |
| `Controllers/Api/CatalogController.cs` | `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc` |
| `Views/Shared/_Layout.cshtml` | `@Styles.Render()`/`@Scripts.Render()` → direct `<link>`/`<script>` tags; `HttpContext.Current.Session["..."]` → `Context.Session.GetString("...")` |
| `Views/Catalog/Create.cshtml`, `Edit.cshtml` | `@Scripts.Render("~/bundles/jqueryval")` → direct `<script>` tags |
| `Views/Catalog/Index.cshtml` | `@Html.Partial(...)` → `<partial>` tag helper (avoids MVC1000 deadlock warning) |
| `Modules/ApplicationModule.cs` | Removed `CatalogDBContext` registration (now handled by `services.AddDbContext<>` in Startup) |

### eShopPorted (existing ported project — fixes applied)

- `eShopPorted.csproj`: Changed `<TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>` → `<TargetFramework>net10.0</TargetFramework>` (web apps cannot multi-target); removed legacy packages (Autofac.Mvc5, WebGrease, Antlr4); upgraded log4net to 3.4.0; Newtonsoft.Json to 13.0.4
- `Program.cs`: Updated to `Host.CreateDefaultBuilder` with `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
- `Startup.cs`: `IHostingEnvironment` → `IWebHostEnvironment`; `IServiceProvider` return type → `void`; added `ConfigureContainer<ContainerBuilder>` for Autofac registrations
- `Controllers/PicController.cs`: `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`; `Server.MapPath` → `IWebHostEnvironment`; `HttpStatusCodeResult`/`HttpNotFound()` → proper ASP.NET Core results
- `Models/Infrastructure/PreconfiguredData.cs`: Removed `using System.Web`
- `Views/Catalog/Index.cshtml`: `@Html.Partial(...)` → `<partial>` tag helper

### eShopLegacy.Utilities (library)

- `eShopLegacy.Utilities.csproj`: Added `net10.0` to `TargetFrameworks` (now `net10.0;netstandard2.0`); added `GenerateAssemblyInfo=false` to prevent duplicate attribute errors with `Properties/AssemblyInfo.cs`; added `System.Text.Json` NuGet reference (conditional on `netstandard2.0`)
- `Serializing.cs`: Replaced `BinaryFormatter` (removed in .NET 5+, security concern) with `System.Text.Json`-based JSON serialization; used `using(var reader = ...)` (C# 7.3 compatible) instead of `using var` declaration for `netstandard2.0` compatibility

---

## Next Steps

- **log4net configuration**: The `log4Net.xml` file is still present in the project root but is not automatically wired up. ASP.NET Core log4net integration (`log4net.Config.XmlConfigurator`) should be configured in `Program.cs` if log4net-specific appenders are needed. Currently the app will start without log4net output unless configured.
- **Session data types**: `Context.Session.GetString("MachineName")` and `GetString("SessionStartTime")` are used in `_Layout.cshtml`. The original code set these in `Global.asax Session_Start`. If session tracking is needed, add a middleware or action filter to set `HttpContext.Session.SetString("MachineName", Environment.MachineName)` at request start.
- **Database migrations**: The migrated `CatalogDBInitializer` uses `EnsureCreated()`. For production, consider using EF Core migrations (`dotnet ef migrations add Initial` → `dotnet ef database update`) instead of `EnsureCreated`.
- **CatalogItemHiLoGenerator**: The `SqlQuery<long>` with raw SQL string (`$"SELECT NEXT VALUE FOR catalog_hilo"`) uses EF Core's SQL query API. Ensure the SQL Server sequence objects exist before use.
- **BinaryFormatter replacement in FilesController**: The `/api/files` endpoint now returns JSON bytes via `Serializing.SerializeBinary`. Any existing callers that expected BinaryFormatter-format binary data will need updating; the wire format has changed from binary to JSON UTF-8.
- **Static files**: Static files were copied to `wwwroot/`. Consider reviewing if `Setup/` and legacy XML config files should be cleaned from the project.
