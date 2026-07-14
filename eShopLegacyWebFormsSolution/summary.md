# eShopLegacyWebForms Migration Summary

## Migration: .NET Framework 4.7.2 → net10.0

**Build Status:** ✅ `dotnet build` succeeds with **0 errors, 0 warnings**

---

## What Was Migrated

### 1. Project File (eShopLegacyWebForms.csproj)
- Replaced legacy verbose MSBuild XML with SDK-style `<Project Sdk="Microsoft.NET.Sdk.Web">`
- Target framework: `net10.0`
- Removed `packages.config` and all `<Reference>/<HintPath>` blocks
- Replaced with `<PackageReference>` items (all at versions compatible with net10.0)
- Excluded all WebForms-specific `.aspx.cs` and `.designer.cs` files from compilation

**Final package versions:**
| Package | Version |
|---|---|
| Microsoft.EntityFrameworkCore | 10.0.0 |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.0 |
| Microsoft.EntityFrameworkCore.Design | 10.0.0 |
| Autofac | 8.1.0 |
| Autofac.Extensions.DependencyInjection | 9.0.0 |
| log4net | 3.3.2 |
| Microsoft.Extensions.Logging.Log4Net.AspNetCore | 10.0.0 |
| Newtonsoft.Json | 13.0.3 |

### 2. WebForms → ASP.NET Core Razor Pages
WebForms does not exist in .NET Core/5+. Each `.aspx` page was rewritten as a Razor Page:

| Legacy (WebForms) | Modern (Razor Pages) |
|---|---|
| `Default.aspx` / `Default.aspx.cs` | `Pages/Index.cshtml` / `Pages/Index.cshtml.cs` |
| `Catalog/Create.aspx` / `.cs` | `Pages/Catalog/Create.cshtml` / `.cshtml.cs` |
| `Catalog/Edit.aspx` / `.cs` | `Pages/Catalog/Edit.cshtml` / `.cshtml.cs` |
| `Catalog/Delete.aspx` / `.cs` | `Pages/Catalog/Delete.cshtml` / `.cshtml.cs` |
| `Catalog/Details.aspx` / `.cs` | `Pages/Catalog/Details.cshtml` / `.cshtml.cs` |
| `About.aspx` | `Pages/About.cshtml` |
| `Contact.aspx` | `Pages/Contact.cshtml` |
| `Site.Master` | `Pages/Shared/_Layout.cshtml` |

- `asp:ListView` → Razor `@foreach` loop
- `asp:HyperLink` / `asp:Button` → HTML `<a>` / `<button>` with Tag Helpers
- `asp:TextBox` / `asp:DropDownList` → `<input asp-for>` / `<select asp-for>` Tag Helpers
- `asp:RequiredFieldValidator` / `asp:RangeValidator` → `<span asp-validation-for>` Tag Helpers
- Postback model → HTTP POST handlers (`OnPost`)
- Property injection (Autofac.Integration.Web) → Constructor injection (standard ASP.NET Core DI)

### 3. Global.asax → Program.cs
- `Application_Start` → `Program.cs` minimal hosting model
- `ConfigureContainer()` (Autofac) → `builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())`
- `ConfigDataBase()` → startup initialization block in `Program.cs`
- `Session_Start` → session initialization in `IndexModel.OnGet()`
- `Application_BeginRequest` (log4net) → log4net configured via `XmlConfigurator.Configure()`

### 4. App_Start/ → Program.cs
- `BundleConfig.cs` removed — static files served directly from `wwwroot/`
- `RouteConfig.cs` removed — Razor Pages routing handles this automatically

### 5. Entity Framework 6 → EF Core 10
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- `DbContext(string connectionString)` → `DbContext(DbContextOptions<T> options)`
- `EntityTypeConfiguration<T>` → `EntityTypeBuilder<T>` inline in `OnModelCreating`
- `HasRequired(...).WithMany()` → `HasOne(...).WithMany()`
- `HasDatabaseGeneratedOption(None)` → `.ValueGeneratedNever()`
- `Database.SqlQuery<T>` → `Database.SqlQueryRaw<T>` (with `#pragma warning disable EF1002` where sequence name is a private constant)
- `Database.ExecuteSqlCommand` → `Database.ExecuteSqlRaw`
- `CreateDatabaseIfNotExists<T>` → `context.Database.EnsureCreated()` + manual seed check
- `Database.SetInitializer` → startup initialization in `Program.cs`

### 6. Configuration Migration (Web.config → appsettings.json)
- `<connectionStrings>` → `ConnectionStrings` section in `appsettings.json`
- `<appSettings>` → `AppSettings` section in `appsettings.json`
- `ConfigurationManager.AppSettings["key"]` → `IConfiguration["AppSettings:key"]`
- `ConfigurationManager.ConnectionStrings["name"]` → `IConfiguration.GetConnectionString("name")`

### 7. Dependency Injection
- `Autofac.Integration.Web` (WebForms-specific) → `Autofac.Extensions.DependencyInjection`
- `Autofac.Web` module → `ApplicationModule` with standard Autofac registrations
- `CatalogDBContext` registration removed from Autofac module (registered via `services.AddDbContext`)
- `CatalogDBInitializer` — now receives `IConfiguration` and `IWebHostEnvironment` via constructor

### 8. System.Web Removal
- `System.Web.HttpApplication` → removed (replaced by `Program.cs`)
- `System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`
- `HttpContext.Current` → `IHttpContextAccessor` / direct `HttpContext` in PageModel
- `AppDomain.CurrentDomain.BaseDirectory` → `IWebHostEnvironment.ContentRootPath`
- `System.Web.UI.Page` → `PageModel`
- `System.Web.UI.WebControls.*` → removed (WebForms server controls replaced with HTML + Tag Helpers)

### 9. Static Files
- Static assets (`Content/`, `Scripts/`, `images/`, `fonts/`) → copied to `wwwroot/`
- `Microsoft.AspNet.Web.Optimization` → removed; files referenced directly in `_Layout.cshtml`
- `app.UseStaticFiles()` serves from `wwwroot/` automatically

### 10. Nullable Reference Type Fixes (CS8618)
- `CatalogBrand.Brand` → initialized via `required` modifier
- `CatalogType.Type` → initialized via `required` modifier
- `CatalogItem.Name`, `.Description`, `.PictureUri` → initialized to `string.Empty` in constructor
- `CatalogItem.CatalogType`, `.CatalogBrand` → marked as `CatalogType?` / `CatalogBrand?` (EF navigation properties are optional at init time)

### 11. log4net Logger Pattern Fix (CS8604)
- All `LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType)` calls → replaced with `LogManager.GetLogger(typeof(ClassName))` to avoid nullable `DeclaringType` dereference

### 12. Packages Removed
| Legacy Package | Reason Removed |
|---|---|
| `EntityFramework` 6.x | Replaced by `Microsoft.EntityFrameworkCore` 10.x |
| `Autofac.Web` / `Autofac.Integration.Web` | WebForms-specific; replaced by `Autofac.Extensions.DependencyInjection` |
| `Microsoft.AspNet.Web.Optimization` | Bundling; static files served directly |
| `Microsoft.AspNet.Web.Optimization.WebForms` | WebForms-specific |
| `Microsoft.AspNet.FriendlyUrls*` | WebForms routing |
| `Microsoft.AspNet.ScriptManager.*` | WebForms ScriptManager |
| `Microsoft.AspNet.SessionState.SessionStateModule` | WebForms-specific |
| `Microsoft.AspNet.TelemetryCorrelation` | Legacy telemetry |
| `Microsoft.ApplicationInsights.*` | Legacy AI SDK |
| `Microsoft.CodeDom.Providers.DotNetCompilerPlatform` | Legacy Roslyn provider |
| `Microsoft.Net.Compilers` | Legacy compiler |
| `AspNet.ScriptManager.*` | WebForms ScriptManager |
| `WebGrease`, `Antlr` | Bundling pipeline |

---

## Build Output
```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.76
```

---

## Next Steps (Optional Improvements)

1. **Database initialization**: `CatalogDBInitializer` uses `context.Database.EnsureCreated()` which creates the schema from the EF Core model. For production use, proper EF Core migrations should be created with `dotnet ef migrations add InitialCreate`.

2. **Session state**: The session-based MachineName and SessionStartTime display is preserved in `IndexModel.OnGet()`. In production, consider moving this to a proper middleware or a scoped service.

3. **Anti-forgery tokens**: The Razor Pages forms use the default ASP.NET Core anti-forgery token protection (automatically added by `AddRazorPages()`). Ensure this is tested in the target environment.

4. **Static file caching**: The `wwwroot/` static files are served without cache headers. For production, consider adding `Cache-Control` headers via `StaticFileOptions`.

5. **Pics folder**: The `Pics/` folder with product images is referenced as content and served at `/Pics/...`. Consider moving it to `wwwroot/Pics/` and removing the custom content path, or configure a dedicated static files middleware path.

6. **ASPX files remaining**: The original `.aspx`, `.ascx`, and `.master` files still exist in the project folder. They are excluded from compilation and do not affect the build, but can be deleted to clean up the repository.
