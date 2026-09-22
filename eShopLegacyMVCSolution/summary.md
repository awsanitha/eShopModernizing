# eShopLegacyMVCSolution Migration Summary

## Migration: .NET Framework 4.7.2 → .NET 10.0

### Build status
`dotnet build eShopLegacyMVC.sln` → **Build succeeded, 0 errors.**

---

## What was changed

### eShopLegacy.Utilities (netstandard2.0)
- Replaced `BinaryFormatter` with `System.Text.Json` (`JsonSerializer`) in `Serializing.cs`. BinaryFormatter throws `PlatformNotSupportedException` at runtime on .NET 5+.
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to suppress CS0579 duplicate attribute conflicts.

### eShopLegacyMVC (net10.0)
- Rewrote `eShopLegacyMVC.csproj` as SDK-style targeting `net10.0`.
- Replaced `Global.asax` + `Application_Start` with `Program.cs` using `WebApplication.CreateBuilder`.
- Integrated Autofac 8.3.0 + Autofac.Extensions.DependencyInjection 10.0.0 via `AutofacServiceProviderFactory`.
- Migrated ASP.NET MVC 5 (`System.Web.Mvc`) → ASP.NET Core MVC (`Microsoft.AspNetCore.Mvc`) across all controllers.
- Migrated Web API 2 controllers (`ApiController`, `IHttpActionResult`) → `ControllerBase` / `IActionResult`.
- Migrated Entity Framework 6 to EF Core 10.0.12:
  - `CatalogDBContext`: `DbModelBuilder` → `ModelBuilder`, `EntityTypeConfiguration<T>` → `IEntityTypeConfiguration<T>` with `EntityTypeBuilder<T>`.
  - `HasRequired` → `HasOne`, `HasDatabaseGeneratedOption` → `ValueGeneratedNever`.
  - `CatalogDBInitializer`: removed EF6 `CreateDatabaseIfNotExists<T>`, replaced with `context.Database.EnsureCreated()`.
  - `CatalogItemHiLoGenerator`: `SqlQuery` → `SqlQueryRaw<long>`.
- Replaced `System.Web.Optimization` bundle helpers (`@Styles.Render`, `@Scripts.Render`) with direct `<link>` and `<script>` tags in Razor views.
- Replaced `ConfigurationManager.AppSettings` with `IConfiguration` / `appsettings.json`.
- Added `Views/_ViewImports.cshtml` with ASP.NET Core tag helper registrations.
- Replaced `@Html.Partial(...)` with `<partial>` tag helper in `Views/Catalog/Index.cshtml`.
- Stubbed `App_Start/BundleConfig.cs`, `FilterConfig.cs`, `RouteConfig.cs`, `WebApiConfig.cs` as empty namespaces.
- `PicController.cs`: injected `IWebHostEnvironment`, replaced `Server.MapPath` with `Path.Combine(env.ContentRootPath, "Pics")`.
- `HttpStatusCodeResult` / `HttpNotFound()` → `BadRequest()` / `NotFound()` throughout.

### eShopPorted (net10.0)
- Upgraded Autofac to 8.3.0 + Autofac.Extensions.DependencyInjection 10.0.0.
- Rewrote `Program.cs` using `WebApplication.CreateBuilder` + `AutofacServiceProviderFactory`; removed the old `IServiceProvider ConfigureServices` pattern.
- `Startup.cs` emptied (bootstrapping moved to Program.cs).
- `Startup.StartTime` extracted to `AppStartTime.StartTime` static class.
- Fixed `IHostingEnvironment` → `IWebHostEnvironment` in `Startup.cs`.
- Replaced `@Html.Partial(...)` with `<partial>` tag helper.

---

## Remaining warnings (non-blocking)

| Code | Location | Description |
|------|----------|-------------|
| NU1902 | All projects | `log4net` 3.0.0 has known moderate severity vulnerability GHSA-4f7c-pmjv-c25w |
| EF1003 | `CatalogDBInitializer.cs:305` | `SqlQueryRaw` with string concatenation — sequence name comes from a private constant, not user input, so SQL injection risk is negligible here |

---

## Next steps

### 1. log4net vulnerability (GHSA-4f7c-pmjv-c25w)
All versions of log4net including 3.0.0 (the latest) carry this advisory. There is no patched version available.
**Resolution**: Migrate logging to `Microsoft.Extensions.Logging` (+ a provider such as Serilog or NLog). This requires:
- Replacing `LogManager.GetLogger(typeof(X))` calls with `ILogger<X>` constructor injection.
- Removing the `log4net.Config.XmlConfigurator` assembly attribute from `AssemblyInfo.cs`.
- Replacing `log4Net.xml` config with `appsettings.json` logging configuration.

### 2. EF1003 in CatalogDBInitializer.cs
`GetSequenceIdFromSelectedDBSequence` uses `SqlQueryRaw` with string concatenation. The sequence name is a private constant, but EF Core still warns. Options:
- Suppress with `#pragma warning disable EF1003 / restore` around the call.
- Switch to parameterized `SqlQuery<long>` using FormattableString: `context.Database.SqlQuery<long>($"SELECT NEXT VALUE FOR {dBSequenceName}")`.

### 3. Session configuration
`AddSession()` is registered in `eShopLegacyMVC` but no distributed cache provider is configured. For production, add `AddDistributedMemoryCache()` (dev/single-node) or a Redis/SQL provider before `AddSession()`.

### 4. Database initialization on startup
`CatalogDBInitializer.Initialize()` must be called explicitly after the app is built. Consider calling it in a startup scope:
```csharp
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<CatalogDBInitializer>();
    initializer.Initialize(scope.ServiceProvider.GetRequiredService<CatalogDBContext>());
}
```
