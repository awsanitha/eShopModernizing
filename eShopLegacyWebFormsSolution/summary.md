# eShopLegacyWebForms – .NET Framework 4.7.2 → .NET 10 Migration Summary

## Final build result
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## What was migrated

### 1. Project file (`eShopLegacyWebForms.csproj`)
- Replaced the legacy MSBuild-style `.csproj` (with hundreds of explicit `<Reference>` and `<Compile>` items) with a minimal SDK-style project targeting `net10.0` / `Microsoft.NET.Sdk.Web`.
- Removed all framework-bound NuGet packages (EF6, Autofac, System.Web.Optimization, WebGrease, Antlr, ApplicationInsights, etc.).
- Added modern replacements: `Microsoft.EntityFrameworkCore.SqlServer 10.0.0`, `Microsoft.EntityFrameworkCore.Design 10.0.0`, `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation 10.0.0`, `Newtonsoft.Json 13.0.3`.
- Removed log4net (had a `NU1902` known vulnerability) and replaced with built-in `Microsoft.Extensions.Logging`.

### 2. WebForms → Razor Pages (25-webforms-to-razor-migration.md)
All `.aspx` / `.ascx` / `.master` files were re-authored as Razor Pages (no automated converter):

| Legacy file | Razor Pages replacement |
|---|---|
| `Default.aspx` + `Default.aspx.cs` | `Pages/Index.cshtml` + `IndexModel` |
| `Catalog/Create.aspx` + `.cs` | `Pages/Catalog/Create.cshtml` + `CreateModel` |
| `Catalog/Edit.aspx` + `.cs` | `Pages/Catalog/Edit.cshtml` + `EditModel` |
| `Catalog/Delete.aspx` + `.cs` | `Pages/Catalog/Delete.cshtml` + `DeleteModel` |
| `Catalog/Details.aspx` + `.cs` | `Pages/Catalog/Details.cshtml` + `DetailsModel` |
| `About.aspx` + `.cs` | `Pages/About.cshtml` + `AboutModel` |
| `Contact.aspx` + `.cs` | `Pages/Contact.cshtml` + `ContactModel` |
| `Site.Master` + `.cs` | `Pages/Shared/_Layout.cshtml` |
| `ViewSwitcher.ascx` | Removed (mobile-view switching not needed) |
| `Site.Mobile.Master` | Removed |

- `Page_Load` → `OnGet` handler methods
- `Button_Click` → `OnPost` handler methods
- `<asp:TextBox>` / `<asp:DropDownList>` → `<input asp-for>` / `<select asp-for asp-items>` tag helpers
- `<asp:ListView>` / `<asp:Repeater>` → `@foreach` loops
- `<asp:HyperLink>` with route URLs → `<a asp-page asp-route-*>` tag helpers
- `<asp:RequiredFieldValidator>` / `<asp:RangeValidator>` → DataAnnotations + `<span asp-validation-for>`
- ViewState → explicit model binding via `[BindProperty]`

### 3. EF6 → EF Core 10 (16-ef6-to-efcore-migration.md)
- `CatalogDBContext` constructor changed to `DbContextOptions<CatalogDBContext>` injection pattern.
- `DbModelBuilder` (EF6) → `ModelBuilder` (EF Core).
- `EntityTypeConfiguration<T>` fluent API → `modelBuilder.Entity<T>(entity => { ... })` lambdas.
- `DatabaseGeneratedOption.None` → `ValueGeneratedNever()`.
- `HasRequired<T>().WithMany().HasForeignKey()` → `HasOne().WithMany().HasForeignKey()`.
- `Database.SqlQuery<T>()` → `Database.SqlQueryRaw<T>()`.
- `Database.ExecuteSqlCommand()` → `Database.ExecuteSqlRaw()`.
- `Database.SetInitializer<T>()` → custom `CatalogDBInitializer.Seed()` called at startup.
- Lazy loading not used (explicit `Include()` already in place).
- `EntityState.Modified` → `Microsoft.EntityFrameworkCore.EntityState.Modified`.

### 4. Global.asax → Program.cs (10-global-asax-migration.md)
- `Application_Start` → `Program.cs` startup.
- `RouteConfig.RegisterRoutes` → `MapRazorPages()`.
- `BundleConfig.RegisterBundles` → static files served from `wwwroot/` via `UseStaticFiles()`.
- Autofac DI → built-in `builder.Services` DI registration.
- `Session_Start` machine name tracking → first-request population in `IndexModel.OnGet`.

### 5. System.Web → ASP.NET Core (02-systemweb-migration.md)
- All `using System.Web.*` removed from every source file.
- `HttpContext.Current.Session` → `HttpContext.Session` (with `ISession`).
- `ConfigurationManager.AppSettings` → `IConfiguration` / `builder.Configuration`.
- `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`.
- `AppDomain.CurrentDomain.BaseDirectory` → `IWebHostEnvironment.ContentRootPath`.

### 6. Web.config → appsettings.json (11-configuration-migration.md)
- `<connectionStrings>` → `appsettings.json` `ConnectionStrings` section.
- `<appSettings>` → top-level `UseMockData`, `UseCustomizationData` keys.
- `appsettings.Development.json` created for dev overrides.

### 7. Static assets → wwwroot (15-views-static-files-migration.md)
- `Content/` (CSS) → `wwwroot/css/`
- `Scripts/` key files (jQuery, Bootstrap bundle) → `wwwroot/js/`
- `images/` → `wwwroot/images/`
- `fonts/` → `wwwroot/fonts/`
- `Pics/` (product images) → `wwwroot/Pics/`
- `favicon.ico` → `wwwroot/`

### 8. Dependency injection (13-dependency-injection-migration.md)
- `Autofac` + `Autofac.Integration.Web` removed entirely.
- `ApplicationModule` removed; service registrations inlined in `Program.cs`.
- `ICatalogService` / `CatalogService` / `CatalogServiceMock` registered via `builder.Services`.
- `CatalogDBContext` registered via `builder.Services.AddDbContext<T>`.

### 9. Logging (12-logging-migration.md)
- `log4net` removed (NU1902 known vulnerability).
- `ILog` / `LogManager.GetLogger()` replaced with `ILogger<T>` injected via constructor.
- `log4net.xml` configuration no longer needed.
- Console and Debug providers registered via `builder.Logging.AddConsole(); AddDebug()`.

### 10. Nullable reference type annotations
- All entity classes (`CatalogItem`, `CatalogBrand`, `CatalogType`) updated with proper nullable annotations.
- Navigation properties marked `null!` (populated by EF Core).
- `PictureUri` made `string?` (legitimately optional).
- `ICatalogService.FindCatalogItem` returns `CatalogItem?`.

---

## Files excluded from compilation (legacy WebForms, kept for reference)
- `Global.asax` / `Global.asax.cs`
- `App_Start/BundleConfig.cs`, `App_Start/RouteConfig.cs`
- All `.aspx.cs`, `.aspx.designer.cs` files
- `Site.Master.cs`, `Site.Mobile.Master.cs`
- `ViewSwitcher.ascx.cs`, `ViewSwitcher.ascx.designer.cs`
- `Modules/ApplicationModule.cs`
- `Properties/AssemblyInfo.cs` (SDK auto-generates assembly attributes)

---

## Next steps / known limitations

1. **CatalogItemHiLoGenerator SQL sequences** — `SqlQueryRaw` for `SELECT NEXT VALUE FOR` works on SQL Server but not SQLite. If a SQLite dev environment is needed, the HiLo generator would need a pure-LINQ fallback. EF1002 warning suppressed with `#pragma warning disable EF1002` since the sequence name is an internal constant, not user input.

2. **DB initializer runs at startup** — `CatalogDBInitializer.Seed()` calls `EnsureCreated()` and seeds only if the table is empty. For production, prefer EF Core migrations (`dotnet ef migrations add InitialCreate`) over `EnsureCreated()`.

3. **No antiforgery validation on Delete form** — The delete confirmation form currently posts to `/Catalog/Delete/{id}` without an explicit AntiForgery token beyond what the Razor form tag helper adds automatically. This is fine since the default Razor Pages antiforgery middleware is enabled by default with `AddRazorPages()`.

4. **Session-based machine name display** — The layout reads session keys `MachineName` and `SessionStartTime` which are set on first request in `IndexModel.OnGet`. If users navigate directly to catalog pages first, these values won't be set until they visit `/`. Consider moving this to a middleware or `_Layout.cshtml` fallback.

5. **Pics served from wwwroot/Pics** — Product images are served as static files. When running with `UseCustomizationData=true`, the DB initializer extracts images to `ContentRootPath/Pics`, but the static files middleware serves from `WebRootPath` (wwwroot). A `StaticFileOptions` with `FileProvider` pointing at `ContentRootPath/Pics` would be needed for that scenario.
