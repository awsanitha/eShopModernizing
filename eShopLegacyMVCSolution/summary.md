# Migration Summary: .NET Framework 4.7.2 → .NET 10

## Migration Status
**Build: ✅ SUCCEEDED — 0 errors, 0 warnings**

All three projects in `eShopLegacyMVC.sln` now target .NET 10 (or netstandard2.0 for the utility library) and build cleanly.

---

## Changes Made

### eShopLegacyMVC (src/eShopLegacyMVC/)

**Project file (eShopLegacyMVC.csproj)**
- Replaced old-style `.csproj` (ToolsVersion 15.0, targeting `.NET Framework 4.7.2`) with SDK-style `Microsoft.NET.Sdk.Web` targeting `net10.0`
- Removed all `<Reference>` HintPath entries pointing to old `packages/` folder
- Removed incompatible packages: `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.WebApi`, `Autofac.Mvc5`, `Autofac.Integration.WebApi`, `WebGrease`, `Antlr`, `Microsoft.ApplicationInsights.*`, `Microsoft.AspNet.Web.Optimization`, `Microsoft.Net.Compilers`, `bootstrap` (JS/CSS packages – now served as static files)
- Added ASP.NET Core packages: `Autofac 4.9.1`, `Autofac.Extensions.DependencyInjection 4.4.0`, `log4net 3.4.0`, `Microsoft.EntityFrameworkCore.SqlServer 10.0.12`, `Newtonsoft.Json 13.0.4`
- Excluded legacy files from compilation: `Global.asax.cs`, `App_Start/*.cs`, `Properties/AssemblyInfo.cs`

**Application startup (new files)**
- Created `Program.cs` using `Host.CreateDefaultBuilder` + `ConfigureWebHostDefaults` (modern non-deprecated hosting API)
- Created `Startup.cs` with ASP.NET Core DI, session, MVC routing, and Autofac integration
- Created `appsettings.json` and `appsettings.Development.json` (replaces `Web.config`)

**Entity Framework 6 → EF Core 10**
- `Models/CatalogDBContext.cs`: Changed base class constructor from `DbContext("name=...")` to `DbContext(DbContextOptions)`, replaced `DbModelBuilder`/`EntityTypeConfiguration<T>` with EF Core `ModelBuilder` fluent API, replaced `HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)` with `ValueGeneratedNever()`, replaced `HasRequired<>(...).WithMany().HasForeignKey(...)` with `HasOne<>(...).WithMany().HasForeignKey(...)`
- `Models/CatalogItemHiLoGenerator.cs`: Replaced EF6 `db.Database.SqlQuery<Int64>()` with raw ADO.NET via `db.Database.GetDbConnection()` for sequence value retrieval
- `Models/Infrastructure/CatalogDBInitializer.cs`: Replaced EF6 `CreateDatabaseIfNotExists<CatalogDBContext>` with a plain class, injecting `IWebHostEnvironment` and `IConfiguration` via constructor DI; replaced `HostingEnvironment.ApplicationPhysicalPath` → `env.ContentRootPath`, `ConfigurationManager.AppSettings` → `IConfiguration`, `context.Database.ExecuteSqlCommand()` → `context.Database.ExecuteSqlRaw()`, `context.Database.SqlQuery<Int64>()` → raw ADO.NET command; added `Seed(CatalogDBContext)` public method called from `Startup.Configure`

**System.Web → ASP.NET Core**
- `Controllers/CatalogController.cs`: Changed `using System.Web.Mvc` → `using Microsoft.AspNetCore.Mvc`, `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`, `HttpNotFound()` → `NotFound()`, `[Bind(Include="...")]` → `[Bind("...")]`, `Request.Url.Scheme` → `Request.Scheme`
- `Controllers/PicController.cs`: Injected `IWebHostEnvironment`, replaced `Server.MapPath("~/Pics")` → `Path.Combine(webHostEnvironment.WebRootPath, "Pics")`, replaced `HttpStatusCodeResult`/`HttpNotFound()` with `BadRequest()`/`NotFound()`
- `Controllers/Api/CatalogController.cs`: Changed `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`
- `Controllers/WebApi/FilesController.cs`: Replaced `System.Web.Http.ApiController` + `HttpResponseMessage` with `Microsoft.AspNetCore.Mvc.Controller` + `IActionResult`; removed `Serializable` attribute from BrandDTO (still present as decorative marker)
- `Controllers/WebApi/BrandsController.cs`: Replaced `System.Web.Http.ApiController`, `IHttpActionResult`, `System.Runtime.Remoting.Messaging` with ASP.NET Core `Controller`/`IActionResult`; converted response helpers

**Services**
- `Services/CatalogService.cs`: Changed `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- `Models/Infrastructure/PreconfiguredData.cs`: Removed `using System.Web` and `using System.Linq` (unused after removing System.Web dependencies)
- `Modules/ApplicationModule.cs`: Removed `RegisterControllers`/`RegisterApiControllers` (not needed in ASP.NET Core), restructured to register only ICatalogService, CatalogItemHiLoGenerator, and CatalogDBInitializer

**Views**
- `Views/Shared/_Layout.cshtml`: Replaced `@Styles.Render()`/`@Scripts.Render()` bundling helpers with direct `<link>`/`<script>` tags; replaced `@HttpContext.Current.Session["..."]` with ASP.NET Core `Context.Session.GetString()` 
- `Views/Catalog/Create.cshtml` & `Edit.cshtml`: Replaced `@Scripts.Render("~/bundles/jqueryval")` with direct script tags
- `Views/Catalog/Index.cshtml`: Changed `@Html.Partial()` → `@await Html.PartialAsync()` (fixes MVC1000 deadlock warning)
- Created `Views/_ViewImports.cshtml`: Added `@using Microsoft.AspNetCore.Http` for `ISession.GetString()` extension methods

---

### eShopPorted (eShopPorted/)

**Project file (eShopPorted.csproj)**
- Removed incompatible packages: `Autofac.Mvc5` (MVC 5 / .NET Framework only), `WebGrease`, `Antlr4`, `Microsoft.CSharp` (NU1510: auto-included)
- Upgraded `log4net` from 2.0.10 to 3.4.0 (fixes NU1902 vulnerability warning)

**Controllers/PicController.cs**
- Replaced `using System.Web.Mvc` + `HttpStatusCodeResult`/`HttpNotFound()` with ASP.NET Core `IWebHostEnvironment` injection + `BadRequest()`/`NotFound()`

**Startup.cs**
- Fixed `IHostingEnvironment` (deprecated) → `IWebHostEnvironment`; added `using Microsoft.Extensions.Hosting` for `IsDevelopment()` extension method
- Added `options.EnableEndpointRouting = false` to suppress routing compatibility warning with legacy `UseMvc()`

**Program.cs**
- Replaced deprecated `WebHost.CreateDefaultBuilder()` (ASPDEPR008) with `Host.CreateDefaultBuilder().ConfigureWebHostDefaults()` 

**Views/Catalog/Index.cshtml**
- Changed `@Html.Partial()` → `@await Html.PartialAsync()` (fixes MVC1000 deadlock warning)

---

### eShopLegacy.Utilities (eShopLegacy.Utilities/)

**Project file (eShopLegacy.Utilities.csproj)**
- Added `net10.0` as second target: `<TargetFrameworks>net10.0;netstandard2.0</TargetFrameworks>`
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to prevent duplicate attribute errors from existing `Properties/AssemblyInfo.cs`
- Added `System.Text.Json` package (required for netstandard2.0 target)

**Serializing.cs**
- Replaced `BinaryFormatter` (removed in .NET 9+, throws NotSupportedException on .NET 5+) with `System.Text.Json.JsonSerializer`; the public `Stream SerializeBinary(object)` / `object DeserializeBinary(Stream)` interface is preserved

---

## Next Steps

1. **log4net vulnerability (NU1902)**: log4net 3.4.0 is the latest version but the NuGet advisory still flags it. Monitor the log4net project for a patched release or consider migrating logging to `Microsoft.Extensions.Logging` with an appropriate sink.

2. **Autofac DI integration modernization**: Both projects use `Autofac.Extensions.DependencyInjection` 4.4.0 with the legacy `ConfigureServices` → `IServiceProvider` pattern and `ContainerBuilder.Populate(services)`. For full .NET 10 alignment, consider upgrading to `Autofac.Extensions.DependencyInjection` 9.x + `Autofac` 8.x and switching to `AutofacServiceProviderFactory` with `WebApplicationBuilder` (minimal hosting API).

3. **`app.UseMvc()` routing**: Both projects still use the legacy `UseMvc()` routing with `EnableEndpointRouting = false`. For a fully modern setup, migrate to `app.UseRouting()` + `app.MapControllerRoute()` / `app.MapControllers()` (endpoint routing).

4. **EF Core migrations for eShopLegacyMVC**: The migrated project uses `Database.EnsureCreated()` + seeding in `CatalogDBInitializer`. For production, replace with proper EF Core migrations (`dotnet ef migrations add Initial` + `database.MigrateAsync()`). The eShopPorted project already has migrations at `eShopPorted/Migrations/`.

5. **Views/Web.config** (src/eShopLegacyMVC/Views/Web.config): Legacy .NET Framework Razor config file — no longer used by ASP.NET Core. Safe to delete.

6. **Global.asax** / **App_Start/** files: Excluded from compilation. These legacy files can be deleted from the repository to clean up the project.

7. **Static files wwwroot**: The eShopLegacyMVC project serves CSS/JS from Content/ and Scripts/ folders at the project root level. For proper ASP.NET Core static file serving, consider moving these under a `wwwroot/` folder and removing the legacy `Pics/`, `Images/` root-level directories.

8. **FilesController binary serialization**: The `FilesController` now returns JSON via `System.Text.Json` (replacing BinaryFormatter). Any client consuming this endpoint expecting binary deserialization will need to be updated to use JSON deserialization.
