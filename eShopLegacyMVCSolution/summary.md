# Migration Summary: eShopLegacyMVC (.NET Framework 4.7.2 → .NET 10)

## Build Result
`dotnet build eShopLegacyMVC.sln` — **Build succeeded. 0 errors, 0 warnings** (eShopLegacyMVC project).

---

## Changes Made

### Project File (`src/eShopLegacyMVC/eShopLegacyMVC.csproj`)
- Replaced legacy verbose MSBuild .csproj with SDK-style `Microsoft.NET.Sdk.Web`
- Changed `TargetFramework` from `v4.7.2` → `net10.0`
- Replaced all legacy package references (`EntityFramework`, `Microsoft.AspNet.Mvc`, `Autofac.Mvc5`, etc.)
- New packages: `Autofac 8.0.0`, `Autofac.Extensions.DependencyInjection 9.0.0`, `log4net 3.4.0`, `Microsoft.EntityFrameworkCore.SqlServer 10.0.12`
- `GenerateAssemblyInfo=false` to preserve `Properties/AssemblyInfo.cs`

### Program.cs (new — replaces Global.asax)
- Replaced `Global.asax` / `MvcApplication : HttpApplication` with minimal hosting via `WebApplication.CreateBuilder`
- Autofac integrated via `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
- `ApplicationModule` registered via `ConfigureContainer<ContainerBuilder>`
- EF Core `CatalogDBContext` registered via `AddDbContext<CatalogDBContext>`
- Session middleware added; session initialization (MachineName, SessionStartTime) via inline middleware
- Static file middleware configured for `Content/`, `Scripts/`, `Images/`, `fonts/` directories
- DB seeding called at startup via `CatalogDBInitializer.Seed()`
- log4net configured via `XmlConfigurator.Configure()` (replaces `[assembly: XmlConfigurator]` attribute)

### Configuration (`appsettings.json`)
- Migrated `Web.config` connection string and appSettings to `appsettings.json`
- `appsettings.Development.json` sets `UseMockData=true` for dev

### Models
- `CatalogDBContext`: Migrated from EF6 (`System.Data.Entity`) to EF Core (`Microsoft.EntityFrameworkCore`); constructor now accepts `DbContextOptions<CatalogDBContext>`; fluent API unchanged except `DatabaseGeneratedOption.None` → `ValueGeneratedNever()`
- `CatalogItemHiLoGenerator`: Replaced `db.Database.SqlQuery<Int64>()` (EF6) with raw ADO.NET `GetDbConnection().CreateCommand().ExecuteScalar()`
- `CatalogDBInitializer`: Removed `CreateDatabaseIfNotExists<T>` base class; now a plain service with `Seed(CatalogDBContext)` method; `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`; `ExecuteSqlCommand` → `ExecuteSqlRaw`; `IWebHostEnvironment` and `IConfiguration` injected via constructor
- `CatalogItem`, `CatalogBrand`, `CatalogType`, `PreconfiguredData`: Removed `System.Web` usings
- `PaginatedItemsViewModel`: Removed `System`, `System.Collections.Generic` (covered by implicit usings)

### Services
- `CatalogService`: Changed `Include("CatalogBrand")` string-based (EF6) to lambda-based `Include(c => c.CatalogBrand)` (EF Core)
- `CatalogServiceMock`, `ICatalogService`: Removed `System.Web` usings; `FindCatalogItem` return type updated to `CatalogItem?`

### Modules
- `ApplicationModule`: Removed `CatalogDBContext` registration (now registered via `AddDbContext`); removed legacy Autofac MVC/WebApi integration imports

### Controllers
- `CatalogController`: Updated to `Microsoft.AspNetCore.Mvc.Controller`; replaced `System.Web.Mvc` with ASP.NET Core equivalents
- `PicController`: `Server.MapPath("~/Pics")` → `IWebHostEnvironment.ContentRootPath`; route pattern preserved with `[HttpGet(..., Name = GetPicRouteName)]`
- `BrandsController`, `FilesController`: Migrated from `ApiController` (Web API 2) to `ControllerBase` + `[ApiController]`; `FilesController` returns JSON instead of binary (BinaryFormatter removed)
- `Controllers/Api/CatalogController`: Removed `System.Web.Http` dependency

### App_Start Files (stubbed)
- `BundleConfig.cs`, `FilterConfig.cs`, `RouteConfig.cs`, `WebApiConfig.cs`: Replaced with empty class stubs (logic moved to `Program.cs`)
- `Global.asax.cs`: Replaced with empty namespace declaration

### Views
- `Views/_ViewImports.cshtml`: Created with `@addTagHelper` and common `@using` directives
- `Views/Shared/_Layout.cshtml`: Replaced `@Styles.Render()`/`@Scripts.Render()` with direct `<link>`/`<script>` tags; `HttpContext.Current.Session` → `Context.Session.GetString()`
- `Views/Catalog/Create.cshtml`, `Edit.cshtml`: Replaced `@Scripts.Render("~/bundles/jqueryval")` with direct `<script>` tags

### Properties/AssemblyInfo.cs
- Removed `[assembly: log4net.Config.XmlConfigurator]` attribute (doesn't work in .NET Core; configured in `Program.cs`)

### eShopLegacy.Utilities
- `Serializing.cs`: Replaced `BinaryFormatter` (removed in .NET 9+) with `XmlSerializer`
- `eShopLegacy.Utilities.csproj`: Added `<LangVersion>latest</LangVersion>` and `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`

---

## Next Steps

1. **eShopPorted project warnings**: The `eShopPorted` project has 40 warnings from legacy packages (`Autofac.Mvc5`, `Microsoft.AspNet.Mvc`, `WebGrease`, `Antlr`, etc.) — these are pre-existing and not in scope for this migration.
2. **FilesController behavior change**: `FilesController.Get()` now returns JSON instead of binary-serialized data (BinaryFormatter was removed in .NET 9+). Any API clients consuming binary data from `GET /api/files` will need updating.
3. **Static files**: CSS/JS/images are served from project root subdirectories via custom `StaticFileOptions`. Consider moving them to `wwwroot/` for conventional ASP.NET Core layout.
4. **log4net 3.4.0**: Upgraded from 2.0.17 to resolve moderate vulnerability GHSA-4f7c-pmjv-c25w.
5. **Database migrations**: The EF Core migration has not been set up (no `dotnet ef migrations add`). `EnsureCreated()` is used for development. For production, add proper EF Core migrations.
6. **Session storage**: Uses in-memory distributed cache (`AddDistributedMemoryCache`). For multi-server deployments, use Redis or SQL Server session store.
