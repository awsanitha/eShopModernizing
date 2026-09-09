# eShopLegacyMVC → .NET 10 Migration Summary

## Migration Status: ✅ COMPLETE — `dotnet build eShopLegacyMVC.sln` succeeds with 0 errors

---

## Projects Migrated

### 1. `eShopLegacyMVC` (src/eShopLegacyMVC)
**Before:** .NET Framework 4.7.2, old-style `.csproj`, System.Web MVC, EF6, Autofac MVC5 integration  
**After:** .NET 10 (`net10.0`), SDK-style `.csproj` with `Microsoft.NET.Sdk.Web`

Changes made:
- **eShopLegacyMVC.csproj** — Replaced old-style XML project with SDK-style targeting `net10.0`. Removed all legacy Framework references (System.Web.*, ApplicationInsights, WebGrease, Antlr, Microsoft.AspNet.Mvc, etc.). Added: Autofac 8.1.0, Autofac.Extensions.DependencyInjection 9.0.0, EF Core 10.0.12, log4net 2.0.17, Newtonsoft.Json 13.0.4.
- **Program.cs** (new) — Replaces Global.asax. Uses minimal hosting (`WebApplication.CreateBuilder`), Autofac `AutofacServiceProviderFactory`, EF Core `AddDbContext`, attribute-routed controllers and default MVC route.
- **appsettings.json** (new) — Replaces Web.config. Contains connection string, `UseMockData`, `UseCustomizationData`.
- **appsettings.Development.json** (new) — Development overrides; `UseMockData: true` for local runs.
- **Global.asax.cs** — Excluded from compilation (replaced by Program.cs).
- **Properties/AssemblyInfo.cs** — Excluded from compilation (SDK auto-generates).
- **Controllers/CatalogController.cs** — `System.Web.Mvc.Controller` → `Microsoft.AspNetCore.Mvc.Controller`. `HttpStatusCodeResult` → `BadRequest()`/`NotFound()`. `HttpNotFound()` → `NotFound()`.
- **Controllers/PicController.cs** — Replaced `Server.MapPath` with `IWebHostEnvironment.ContentRootPath`. Removed `System.Web.Mvc`.
- **Controllers/Api/CatalogController.cs** — `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc.ControllerBase`.
- **Controllers/WebApi/FilesController.cs** — `System.Web.Http.ApiController` → `Microsoft.AspNetCore.Mvc.ControllerBase`. Added `[ApiController]`, `[Route]` attributes.
- **Controllers/WebApi/BrandsController.cs** — Removed `System.Web.Http`, `System.Runtime.Remoting.Messaging` (not available in .NET Core). Converted to `ControllerBase` with `[ApiController]`.
- **Models/CatalogDBContext.cs** — EF6 `DbContext` → EF Core `DbContext`. `DbModelBuilder` → `ModelBuilder`. `EntityTypeConfiguration<T>` → `EntityTypeBuilder<T>`. `HasRequired` → `HasOne`. `HasDatabaseGeneratedOption(None)` → `ValueGeneratedNever()`.
- **Models/CatalogItemHiLoGenerator.cs** — EF6 `db.Database.SqlQuery<Int64>` → EF Core `db.Database.SqlQueryRaw<long>`. Removed `System.Web`.
- **Services/CatalogService.cs** — Updated to EF Core (removed `System.Data.Entity` import). Added null-safe return type.
- **Services/CatalogServiceMock.cs** — Updated return types for nullable annotations.
- **Services/ICatalogService.cs** — Added nullable `CatalogItem?` return type.
- **Models/Infrastructure/CatalogDBInitializer.cs** — Replaced EF6 `CreateDatabaseIfNotExists<T>` with a plain seeder class using EF Core `context.Database.EnsureCreated()`. Removed `System.Web.Hosting.HostingEnvironment`; uses `IWebHostEnvironment.ContentRootPath` instead. Removed HiLo SQL sequences dependency for initial seed (uses sequential IDs).
- **Models/Infrastructure/PreconfiguredData.cs** — Removed `System.Web` import.
- **Modules/ApplicationModule.cs** — Removed old `Autofac.Integration.Mvc`/`Autofac.Integration.WebApi` registrations. Retained core service registrations.
- **App_Start/BundleConfig.cs** — Stubbed out (System.Web.Optimization removed).
- **App_Start/FilterConfig.cs** — Stubbed out (global filters configured in Program.cs).
- **App_Start/RouteConfig.cs** — Stubbed out (routes configured in Program.cs).
- **App_Start/WebApiConfig.cs** — Stubbed out (Web API routes configured in Program.cs).
- **Views/Shared/_Layout.cshtml** — Removed `@Styles.Render`/`@Scripts.Render` bundle references. Removed `HttpContext.Current.Session`. Uses CDN links for Bootstrap/jQuery.
- **Views/Catalog/Create.cshtml** — Replaced `@Scripts.Render("~/bundles/jqueryval")` with CDN script tags.
- **Views/Catalog/Edit.cshtml** — Same as Create.cshtml.

### 2. `eShopPorted` (eShopPorted/)
**Before:** `TargetFrameworks: net10.0;netstandard2.0` (invalid for web app), old Autofac 4.x, old .NET Core 2.x Startup pattern, System.Web.Mvc remnants  
**After:** `TargetFramework: net10.0` only

Changes made:
- **eShopPorted.csproj** — Removed `netstandard2.0` target (web apps cannot multi-target), upgraded Autofac to 8.1.0 / Autofac.Extensions.DependencyInjection to 9.0.0, removed legacy packages (Autofac.Mvc5, WebGrease, Antlr4, Microsoft.CSharp). Fixed Newtonsoft.Json to 13.0.4.
- **Program.cs** — Replaced old `WebHost.CreateDefaultBuilder` (.NET Core 2.x) with `WebApplication.CreateBuilder` (minimal hosting, .NET 6+). Updated to use `AutofacServiceProviderFactory`.
- **Startup.cs** — Replaced with empty comment file (functionality moved to Program.cs).
- **Controllers/PicController.cs** — Removed `System.Web.Mvc`. Added `IWebHostEnvironment` injection for path resolution.
- **Models/Infrastructure/PreconfiguredData.cs** — Removed `System.Web` import.
- **Views/Shared/_Layout.cshtml** — Removed reference to `eShopPorted.Startup.StartTime` (class no longer exists). Replaced with `System.Environment.MachineName`.

### 3. `eShopLegacy.Utilities` (eShopLegacy.Utilities/)
**Before:** `netstandard2.0`, BinaryFormatter (obsolete/dangerous), duplicate AssemblyInfo, C# 7.3 language version  
**After:** `netstandard2.0`, `LangVersion: latest`, `Nullable: enable`, `GenerateAssemblyInfo: false`

Changes made:
- **eShopLegacy.Utilities.csproj** — Added `<LangVersion>latest</LangVersion>`, `<Nullable>enable</Nullable>`, `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` (prevents duplicate attribute errors from Properties/AssemblyInfo.cs). Added `System.Text.Json 8.0.5`.
- **Serializing.cs** — Replaced `BinaryFormatter` (removed in .NET 8+, security-prohibited) with `System.Text.Json`. `SerializeBinary` now returns a UTF-8 JSON `MemoryStream`. `DeserializeBinary` parses JSON back to a dynamic `object`.

---

## Build Result

```
Build succeeded.
4 Warning(s) — all NU1902 (log4net 2.0.17 moderate advisory, same for both web projects)
0 Error(s)
```

---

## Next Steps

| Item | Reason | Action Required |
|------|---------|-----------------|
| **log4net vulnerability (NU1902)** | log4net 2.0.17 has a known moderate severity vulnerability (GHSA-4f7c-pmjv-c25w). No newer version was available in the local NuGet cache at migration time. | Upgrade to log4net ≥ 2.0.18 (or latest stable) when available from the package feed. Alternatively, migrate to Microsoft.Extensions.Logging or Serilog. |
| **CatalogDBInitializer no longer uses HiLo sequences** | The SQL-sequence-based HiLo generator was removed from the seeder and replaced with simple sequential IDs to avoid requiring the SQL sequences to exist before seeding. | If the production database requires HiLo sequences, restore the SQL script execution in the seeder and ensure the sequences exist before the application starts. |
| **Views/Web.config** | The legacy `Views/Web.config` (Razor 2 config) is still present but excluded from the build. | Can be safely deleted. |
| **BinaryFormatter → System.Text.Json (behavioral change)** | `Serializing.SerializeBinary` previously used `BinaryFormatter` (binary format). It now produces UTF-8 JSON. Any consumers that expected the binary `.NET` serialization format will receive JSON instead. | Review consumers of `FilesController.Get()` and `eShopLegacy.Utilities.Serializing` to ensure they handle JSON. |
| **Session data** | `Global.asax Session_Start` set `MachineName` and `SessionStartTime` in session. This was removed. `ISession` (ASP.NET Core) requires explicit opt-in via `services.AddSession()` + `app.UseSession()`. | Add session support in Program.cs if required by the application. |
| **EF Core Migrations** | The `eShopPorted` project has EF Core migrations in `Migrations/`. The migrated `eShopLegacyMVC` project uses `EnsureCreated()` which is suitable for dev/demo only. | Run `dotnet ef migrations add InitialCreate` for `eShopLegacyMVC` if a proper migration history is needed. |
