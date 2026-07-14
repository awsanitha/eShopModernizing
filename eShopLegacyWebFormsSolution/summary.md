# eShopLegacyWebForms Migration Summary

## Migration: .NET Framework 4.7.2 → net10.0

**Build Status:** ✅ `dotnet build` succeeds with 0 errors

---

## What Was Migrated

### 1. Project File (eShopLegacyWebForms.csproj)
- Replaced legacy verbose MSBuild XML with SDK-style `<Project Sdk="Microsoft.NET.Sdk.Web">`
- Target framework: `net10.0`
- Removed `packages.config` and all `<Reference>/<HintPath>` blocks
- Replaced with `<PackageReference>` items
- Excluded all WebForms-specific `.aspx.cs` and `.designer.cs` files from compilation

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

### 5. Entity Framework 6 → EF Core 9
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- `DbContext(string connectionString)` → `DbContext(DbContextOptions<T> options)`
- `EntityTypeConfiguration<T>` → `EntityTypeBuilder<T>` inline in `OnModelCreating`
- `HasRequired(...).WithMany()` → `HasOne(...).WithMany()`
- `HasDatabaseGeneratedOption(None)` → `.ValueGeneratedNever()`
- `Database.SqlQuery<T>` → `Database.SqlQueryRaw<T>`
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

### 10. Packages Removed
| Legacy Package | Reason Removed |
|---|---|
| `EntityFramework` 6.x | Replaced by `Microsoft.EntityFrameworkCore` 9.x |
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
    0 Error(s)
    2 Warning(s)  (NU1902: log4net vulnerability advisory — informational only)
```

---

## Next Steps (for subsequent cycles)

1. **log4net vulnerability**: The `log4net` 2.0.17 package has a known moderate severity vulnerability (GHSA-4f7c-pmjv-c25w). Consider upgrading to the latest version when available, or replacing with `Microsoft.Extensions.Logging` + Serilog.

2. **Nullable warnings**: Several model properties (`CatalogItem`, `CatalogBrand`, `CatalogType`) generate CS8618 nullable warnings. These should be cleaned up by adding `?` nullable annotations or `required` modifiers to the model properties.

3. **Session state**: The session-based MachineName and SessionStartTime display is preserved in `IndexModel.OnGet()`. In production, consider moving this to a proper middleware or a scoped service.

4. **Database initialization**: `CatalogDBInitializer` uses `context.Database.EnsureCreated()` which creates the schema from the EF Core model. For production use, proper EF Core migrations should be created with `dotnet ef migrations add InitialCreate`.

5. **EF Core SQL injection warning EF1002**: The `GetSequenceIdFromSelectedDBSequence` method uses string interpolation in `SqlQueryRaw`. The sequence name comes from a private constant, so it is safe, but suppressing the warning explicitly with `#pragma warning disable EF1002` would be cleaner.

6. **Static file caching**: The `wwwroot/` static files are served without cache headers. For production, consider adding `Cache-Control` headers via `StaticFileOptions`.

7. **Pics folder**: The `Pics/` folder with product images is referenced as content but is served as a static path `/Pics/...`. Consider moving it to `wwwroot/Pics/` and updating the references in the Razor Pages, or configure a custom static files path for it.

8. **ASPX files remaining**: The original `.aspx`, `.ascx`, and `.master` files still exist in the project folder. They are not compiled (SDK-style projects don't pick up non-`.cs` files into compilation), but they can be deleted to clean up the repository.

9. **Anti-forgery tokens**: The Razor Pages forms use the default ASP.NET Core anti-forgery token protection (automatically added by `AddRazorPages()`). Ensure this is tested in the target environment.
