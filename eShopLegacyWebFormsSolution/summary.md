# eShopLegacyWebForms Migration Summary

## Migration: .NET Framework 4.7.2 → .NET 10.0

**Build Status:** ✅ SUCCESS — 0 errors, 0 warnings

---

## What Was Done

### 1. Project File (eShopLegacyWebForms.csproj)
- Replaced legacy verbose MSBuild-XML `.csproj` with SDK-style `Microsoft.NET.Sdk.Web` project
- Target framework changed from `net472` to `net10.0`
- Removed all `packages.config` references and `<Reference>` elements
- Added `<PackageReference>` entries for modern packages:
  - `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` 10.0.0
  - `Microsoft.EntityFrameworkCore` / `SqlServer` / `Design` / `Tools` 10.0.0
  - `Autofac` 8.3.0 + `Autofac.Extensions.DependencyInjection` 10.0.0
  - `Newtonsoft.Json` 13.0.3
- Excluded all legacy Web Forms `.aspx.cs`, `.aspx.designer.cs`, `.master.cs` files from compilation
- Set `GenerateAssemblyInfo=false` to keep existing `Properties/AssemblyInfo.cs`

### 2. Web Forms → Razor Pages (KB 25)
All Web Forms pages re-authored as ASP.NET Core Razor Pages:

| Web Forms File | Razor Pages File |
|---|---|
| `Default.aspx` + `Default.aspx.cs` | `Pages/Index.cshtml` + `Index.cshtml.cs` |
| `Catalog/Create.aspx` + `.cs` | `Pages/Catalog/Create.cshtml` + `.cshtml.cs` |
| `Catalog/Edit.aspx` + `.cs` | `Pages/Catalog/Edit.cshtml` + `.cshtml.cs` |
| `Catalog/Details.aspx` + `.cs` | `Pages/Catalog/Details.cshtml` + `.cshtml.cs` |
| `Catalog/Delete.aspx` + `.cs` | `Pages/Catalog/Delete.cshtml` + `.cshtml.cs` |
| `About.aspx` + `.cs` | `Pages/About.cshtml` + `.cshtml.cs` |
| `Contact.aspx` + `.cs` | `Pages/Contact.cshtml` + `.cshtml.cs` |
| `Site.Master` + `.cs` | `Pages/Shared/_Layout.cshtml` |

- Server controls (`<asp:ListView>`, `<asp:TextBox>`, `<asp:DropDownList>`, etc.) replaced with Razor `@foreach`, tag helpers (`asp-for`, `asp-items`, `asp-validation-for`)
- `Page_Load`/`IsPostBack` lifecycle → `OnGet`/`OnPost` Razor Page handler methods
- `Response.Redirect("~")` → `RedirectToPage("/Index")`
- Route URLs (e.g. `GetRouteUrl("EditProductRoute", ...)`) → plain `/Catalog/Edit/@item.Id` href links
- `MasterPageFile="~/Site.Master"` → `Pages/_ViewStart.cshtml` with `Layout = "_Layout"`

### 3. Global.asax → Program.cs (KB 10)
- `Application_Start` setup migrated to `WebApplication.CreateBuilder` pattern
- Autofac DI wired via `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
- `ContainerBuilder.RegisterModule(new ApplicationModule(useMockData))` preserved
- Database seeding moved to post-`app.Run()` block using `IServiceScope`

### 4. Web.config → appsettings.json (KB 11)
- Connection strings moved to `ConnectionStrings` section
- App settings moved to `AppSettings` section
- Logging configuration added for `Microsoft.Extensions.Logging`

### 5. Entity Framework 6 → EF Core 10 (KB 16)
- `System.Data.Entity` → `Microsoft.EntityFrameworkCore`
- `CatalogDBContext` constructor updated to use `DbContextOptions<CatalogDBContext>` injection
- `OnModelCreating(DbModelBuilder)` → `OnModelCreating(ModelBuilder)`
- `EntityTypeConfiguration<T>` → `EntityTypeBuilder<T>` (inline in `OnModelCreating`)
- `HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)` → `.ValueGeneratedNever()`
- `HasRequired<T>(...).WithMany().HasForeignKey(...)` → `HasOne<T>(...).WithMany().HasForeignKey(...)`
- `CatalogDBInitializer : CreateDatabaseIfNotExists<T>` → plain service class with `context.Database.EnsureCreated()`
- `Database.SqlQuery<T>(...)` → `db.Database.GetDbConnection()` + ADO.NET command (avoids EF1002 SQL injection warning)
- `context.Database.ExecuteSqlCommand(...)` → `context.Database.ExecuteSqlRaw(...)`
- `db.Entry(entity).State = EntityState.Modified` preserved (same API in EF Core)

### 6. Static Assets
- Content, scripts, images, fonts, and Pics moved to `wwwroot/` for ASP.NET Core static file serving
- `useStaticFiles()` middleware in `Program.cs`

### 7. Autofac DI Module (KB 13)
- Removed `Autofac.Integration.Web` (Web Forms specific) dependency
- `ApplicationModule` now registers services compatible with ASP.NET Core DI scope
- `CatalogDBContext` and `CatalogDBInitializer` registration moved to `Program.cs` via Autofac container builder

### 8. Models / Services
- Removed `System.Web` using directives from all model and service files
- Updated nullable annotations: `string` properties marked `= string.Empty` or `null!` appropriately
- `ICatalogService.FindCatalogItem` return type updated to `CatalogItem?` (nullable)

---

## Files Excluded (not compiled, left as-is for reference)
- `Global.asax` / `Global.asax.cs`
- `App_Start/BundleConfig.cs`, `App_Start/RouteConfig.cs`
- All `*.aspx`, `*.aspx.cs`, `*.aspx.designer.cs` files
- `Site.Master`, `Site.Master.cs`, `Site.Master.designer.cs`
- `Site.Mobile.Master`, `Site.Mobile.Master.cs`, `Site.Mobile.Master.designer.cs`
- `ViewSwitcher.ascx`, `ViewSwitcher.ascx.cs`, `ViewSwitcher.ascx.designer.cs`
- `Web.config`, `Web.Debug.config`, `Web.Release.config`
- `ApplicationInsights.config`, `Bundle.config`
- `packages.config`

---

## Next Steps

1. **Database setup**: Run `dotnet ef migrations add InitialCreate` and `dotnet ef database update` to create the schema if connecting to SQL Server. The current mock data mode doesn't require a database.

2. **Client-side validation**: Consider adding `jquery-validation` and `jquery-validation-unobtrusive` NuGet packages for full browser-side form validation on the Catalog pages.

3. **ApplicationInsights**: The legacy `ApplicationInsights.config` and NuGet packages were removed. If telemetry is required, add `Microsoft.ApplicationInsights.AspNetCore` and configure in `Program.cs`.

4. **log4net removal**: The original codebase used `log4net`. It was replaced with `Microsoft.Extensions.Logging` (built-in). The `log4Net.xml` file is no longer used.

5. **Session state**: The original `Session_Start` storing `MachineName` and `SessionStartTime` in the layout was removed since Razor Pages handles session differently. Re-add via `builder.Services.AddSession()` and `app.UseSession()` if needed.

6. **Mobile views**: The `Site.Mobile.Master` and `ViewSwitcher` mobile view-switching feature has no equivalent in Razor Pages. If needed, implement responsive design or a separate mobile layout.

7. **Autofac scope**: The `ApplicationModule` no longer registers `CatalogDBContext` (now handled by `AddDbContext` in Program.cs for the real DB path). When `useMockData=false`, verify EF Core DbContext lifetime is correctly scoped.
