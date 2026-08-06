# Migration Summary: .NET Framework 4.7.2 → .NET 10

## Result
**✅ Build succeeded — 0 errors, 0 warnings (Debug and Release)**

All three projects in `eShopLegacyMVC.sln` now target modern .NET:

| Project | Before | After |
|---------|--------|-------|
| `eShopLegacyMVC` | .NET Framework 4.7.2 (old `.csproj`) | `net10.0` (SDK-style) |
| `eShopPorted` | `net10.0` (broken — legacy packages, deprecated APIs) | `net10.0` (fully fixed) |
| `eShopLegacy.Utilities` | `netstandard2.0` (CS0579 duplicate attributes) | `netstandard2.0` (fixed) |

---

## Changes Made

### eShopLegacy.Utilities
- **`eShopLegacy.Utilities.csproj`**: Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to fix duplicate assembly attribute errors (`CS0579`). Removed unused `Microsoft.CSharp` and `System.Data.DataSetExtensions` packages. Added `System.Text.Json` and set `<LangVersion>latest</LangVersion>`.
- **`Serializing.cs`**: Replaced `BinaryFormatter` (removed in .NET 9) with `System.Text.Json` serialization. Behavioral change: binary wire format → JSON; any consumer expecting binary format would need updating (noted in Next Steps).

### eShopLegacyMVC (src/eShopLegacyMVC)
- **`eShopLegacyMVC.csproj`**: Complete rewrite from legacy XML format to SDK-style targeting `net10.0`. Excluded legacy files (`Global.asax.cs`, `Properties/AssemblyInfo.cs`, `App_Start/*.cs`, `Web.config`, `Views/Web.config`) from compilation/content. Replaced all .NET Framework NuGet packages with ASP.NET Core equivalents.
- **`Program.cs`** (new): ASP.NET Core minimal hosting entry point replacing `Global.asax`. Registers MVC, EF Core DbContext, session, and services using built-in DI. Initializes DB on startup.
- **`appsettings.json`** / **`appsettings.Development.json`** (new): Replaces `Web.config` `<appSettings>` and `<connectionStrings>`. Development overrides `UseMockData=true`.
- **`Models/CatalogDBContext.cs`**: EF6 → EF Core. Constructor now takes `DbContextOptions<CatalogDBContext>` for DI. `DbModelBuilder` → `ModelBuilder`. EF6 fluent API → EF Core fluent API.
- **`Models/CatalogItem.cs`**: Added proper nullable annotations. Navigation properties use `= null!` initializer (non-nullable reference type with deferred assignment).
- **`Models/CatalogBrand.cs`**, **`Models/CatalogType.cs`**: Removed `System.Web` usings. Added string default initializers.
- **`Models/CatalogItemHiLoGenerator.cs`**: Replaced `db.Database.SqlQuery<long>()` (EF6) with `db.Database.SqlQueryRaw<long>()` (EF Core).
- **`Models/Infrastructure/PreconfiguredData.cs`**: Removed `System.Web` usings.
- **`Models/Infrastructure/CatalogDBInitializer.cs`**: Full rewrite — replaced `CreateDatabaseIfNotExists<T>`, `HostingEnvironment.ApplicationPhysicalPath`, `Database.ExecuteSqlCommand()`, and `Database.SqlQuery<long>()` with EF Core equivalents (`EnsureCreated`, `AppDomain.CurrentDomain.BaseDirectory`, `ExecuteSqlRaw`, `SqlQueryRaw`).
- **`Controllers/CatalogController.cs`**: `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`. `HttpStatusCodeResult`/`HttpNotFound()` → `BadRequest()`/`NotFound()`. `log4net` → `ILogger<T>` (constructor injected).
- **`Controllers/PicController.cs`**: Added `IWebHostEnvironment` injection to replace `Server.MapPath`. `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`. `log4net` → `ILogger<T>`.
- **`Controllers/Api/CatalogController.cs`**: `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`.
- **`Controllers/WebApi/FilesController.cs`**: `System.Web.Http.ApiController` → `Microsoft.AspNetCore.Mvc.ControllerBase`. `HttpResponseMessage` → `IActionResult`.
- **`Controllers/WebApi/BrandsController.cs`**: Same as FilesController. Removed `System.Runtime.Remoting.Messaging` (removed in .NET Core). Simplified HTTP response returns.
- **`Services/CatalogService.cs`**: `System.Data.Entity` → `Microsoft.EntityFrameworkCore`. `EntityState.Modified` now from EF Core namespace.
- **`Services/CatalogServiceMock.cs`**: Added null-forgiving operator `!` for `FirstOrDefault()` return.
- **`Modules/ApplicationModule.cs`**: Replaced with empty stub. DI registration moved to `Program.cs` using built-in ASP.NET Core DI.
- **`Views/_ViewImports.cshtml`** (new): Registers `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` and common namespace `@using` directives.
- **`Views/Shared/_Layout.cshtml`**: Replaced `Styles.Render()` / `Scripts.Render()` (System.Web.Optimization bundling) with direct `<link>` and `<script>` tags. Replaced `HttpContext.Current.Session["..."]` with `Context.Session.GetString("...")`.
- **`Views/Catalog/Create.cshtml`**, **`Views/Catalog/Edit.cshtml`**: Replaced `@Scripts.Render("~/bundles/jqueryval")` with direct script includes.
- **`Views/Catalog/Index.cshtml`**: Replaced `@Html.Partial(...)` with `<partial name="..." model="..." />` tag helper (fixes MVC1000 deadlock warning).

### eShopPorted
- **`eShopPorted.csproj`**: Removed incompatible `.NET Framework` packages (`Autofac.Mvc5 4.0.2`, `Microsoft.AspNet.Mvc`, `Microsoft.AspNet.Razor`, `Microsoft.AspNet.WebPages`, `Microsoft.Web.Infrastructure`, `WebGrease`, `Antlr`, `Microsoft.CSharp`). Upgraded `Autofac` from 4.9.1 → 8.2.1 and `Autofac.Extensions.DependencyInjection` from 4.4.0 → 10.0.0. Removed `log4net`.
- **`Program.cs`**: Replaced old `WebHost.CreateDefaultBuilder().UseStartup<Startup>()` pattern with .NET 6+ `WebApplication.CreateBuilder()` + `UseServiceProviderFactory(new AutofacServiceProviderFactory())`.
- **`Startup.cs`**: `IHostingEnvironment` → `IWebHostEnvironment`. Removed `IServiceProvider` return from `ConfigureServices` (deprecated Autofac integration pattern). Added `ConfigureContainer(ContainerBuilder)` method for Autofac. `app.UseMvc()` → `app.UseRouting()` + `app.UseEndpoints()`.
- **`Controllers/CatalogController.cs`**: Replaced `log4net` with `ILogger<CatalogController>`.
- **`Controllers/PicController.cs`**: Replaced `System.Web.Mvc` with `Microsoft.AspNetCore.Mvc`. Added `IWebHostEnvironment`. Replaced `log4net` with `ILogger<PicController>`. Fixed `Server.MapPath` with environment-aware path resolution.
- **`Models/Infrastructure/PreconfiguredData.cs`**: Removed `System.Web` usings.
- **`Views/_ViewImports.cshtml`** (new): Added tag helper registration.
- **`Views/Catalog/Index.cshtml`**: Replaced `@Html.Partial()` with `<partial>` tag helper (fixes MVC1000).

---

## Architecture Notes

### DI Strategy
- `eShopLegacyMVC`: Uses **built-in ASP.NET Core DI** (no Autofac). Simpler and more idiomatic for .NET 10.
- `eShopPorted`: Retains **Autofac** DI using the modern `AutofacServiceProviderFactory` integration pattern.

### Logging
Both projects migrated from `log4net` (vulnerable CVE) to `Microsoft.Extensions.Logging` (`ILogger<T>` injected via constructor). This eliminates the NU1902 advisory for `log4net`.

### BinaryFormatter
`Serializing.cs` in `eShopLegacy.Utilities` now uses `System.Text.Json` instead of `BinaryFormatter` (removed in .NET 9). The wire format changed from binary to JSON.

---

## Next Steps

1. **BinaryFormatter format change**: Any client that calls the `/api/files` endpoint and expects a binary stream will receive JSON instead. Clients should be updated to handle JSON.

2. **eShopLegacyMVC static files**: The `Content/`, `Scripts/`, `Images/`, and `Pics/` folders in `src/eShopLegacyMVC` serve as `wwwroot` equivalents. For production, consider moving them to a proper `wwwroot/` directory and configuring `UseStaticFiles()` with a `FileProvider` pointing to those paths, or restructure to use standard wwwroot layout.

3. **Database initialization**: `CatalogDBInitializer` uses `context.Database.EnsureCreated()` which does not run EF Core migrations. For production, run `dotnet ef database update` with proper migration files.

4. **log4Net.xml**: The `log4Net.xml` config file is still present in `eShopLegacyMVC` but is no longer used (logging is now via `Microsoft.Extensions.Logging`). It can be deleted.

5. **HTTPS**: Both applications have `UseHttpsRedirection()`. In development containers without certificates, this may need to be removed or conditioned on environment.

6. **Session in eShopLegacyMVC**: The `_Layout.cshtml` reads `MachineName` and `SessionStartTime` from session. The `Program.cs` sets the machine name but `SessionStartTime` is not set. Add session initialization middleware or middleware to set it on session start.

7. **eShopPorted Autofac version**: Upgraded to Autofac 8.2.1. Test module registration patterns for any breaking changes between Autofac 4.x and 8.x.
