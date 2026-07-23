# Migration Summary: eShopLegacyWebForms → .NET 10

## Status
✅ **Build succeeded: 0 errors, 0 warnings**

## Migration Overview

The eShopLegacyWebForms solution was migrated from ASP.NET Web Forms on .NET Framework 4.7.2 to ASP.NET Core Razor Pages targeting **net10.0**.

---

## Changes Made

### 1. Project File (`eShopLegacyWebForms.csproj`)
- **Replaced** the legacy MSBuild XML project with an SDK-style project using `Microsoft.NET.Sdk.Web`
- **Target framework** changed from `v4.7.2` to `net10.0`
- All legacy framework `<Reference>` items, `<HintPath>` package references, and `packages.config` NuGet references replaced with modern `<PackageReference>` items
- Old Web Forms code-behind files excluded from compilation via `<Compile Remove="..." />` items

### 2. NuGet Packages (Updated)

| Old Package | New Package | Notes |
|---|---|---|
| `EntityFramework 6.2.0` | `Microsoft.EntityFrameworkCore 10.0.10` | EF Core 10 |
| `EntityFramework.SqlServer 6.2.0` | `Microsoft.EntityFrameworkCore.SqlServer 10.0.10` | |
| `Autofac 4.9.1` | `Autofac 9.3.1` | |
| `Autofac.Integration.Web 4.0.0` | `Autofac.Extensions.DependencyInjection 11.0.2` | Web Forms integration removed |
| `log4net 2.0.10` | `log4net 3.3.2` | Fixed known vulnerability (GHSA-4f7c-pmjv-c25w) |
| (new) | `Microsoft.Extensions.Logging.Log4Net.AspNetCore 10.0.0` | ASP.NET Core log4net integration |
| `Newtonsoft.Json 12.0.1` | `Newtonsoft.Json 13.0.3` | |
| `Microsoft.AspNet.Web.Optimization`, `WebGrease`, `Antlr` | **Removed** | Bundling not used in ASP.NET Core |
| `AspNet.ScriptManager.*`, `Microsoft.ScriptManager.*` | **Removed** | Web Forms only |
| `Microsoft.ApplicationInsights.*` | **Removed** | Can be re-added via modern AI SDK if needed |
| `Microsoft.AspNet.FriendlyUrls.*` | **Removed** | Web Forms routing no longer needed |
| `Microsoft.AspNet.SessionState.SessionStateModule` | **Removed** | Replaced by built-in ASP.NET Core session |

### 3. Application Startup (`Global.asax.cs` → `Program.cs`)
- Created `Program.cs` with `WebApplication.CreateBuilder` pattern
- Autofac registered via `UseServiceProviderFactory(new AutofacServiceProviderFactory())`
- EF Core registered via `AddDbContext<CatalogDBContext>` with SQL Server provider
- Session middleware configured via `AddDistributedMemoryCache()` + `AddSession()`
- log4net configured via `builder.Logging.AddLog4Net("log4net.xml")`
- Database initialization runs on startup (when `UseMockData=false`)

### 4. Configuration (`Web.config` → `appsettings.json`)
- Connection string moved to `ConnectionStrings:CatalogDBContext`
- App settings moved to `AppSettings:UseMockData` and `AppSettings:UseCustomizationData`
- Environment-specific override file `appsettings.Development.json` created

### 5. Entity Framework 6 → EF Core 10

**`CatalogDBContext`:**
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- Constructor changed from `base("name=CatalogDBContext")` to `base(DbContextOptions<CatalogDBContext> options)`
- `DbModelBuilder` → `ModelBuilder`
- `EntityTypeConfiguration<T>` → `EntityTypeBuilder<T>`
- `HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)` → `ValueGeneratedNever()`
- `HasRequired<T>(...).WithMany().HasForeignKey(...)` → `HasOne(...).WithMany().HasForeignKey(...)`

**`CatalogItemHiLoGenerator`:**
- `db.Database.SqlQuery<Int64>(sql)` → `db.Database.SqlQueryRaw<long>(sql)`

**`CatalogDBInitializer`:**
- `CreateDatabaseIfNotExists<T>` → custom `InitializeAsync` method using `EnsureCreatedAsync`
- `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath` (injected)
- `context.Database.ExecuteSqlCommand` → `context.Database.ExecuteSqlRaw`
- `ConfigurationManager.AppSettings` → `IConfiguration` (injected)
- Seeding methods made `async Task`

**`CatalogService`:**
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`
- `using System.Data.SqlClient` removed (not needed)

### 6. Web Forms → Razor Pages

All Web Forms pages were re-authored as Razor Pages in the `Pages/` folder:

| Web Forms (Old) | Razor Pages (New) |
|---|---|
| `Default.aspx` + `Default.aspx.cs` | `Pages/Index.cshtml` + `Pages/Index.cshtml.cs` |
| `About.aspx` + `About.aspx.cs` | `Pages/About.cshtml` + `Pages/About.cshtml.cs` |
| `Contact.aspx` + `Contact.aspx.cs` | `Pages/Contact.cshtml` + `Pages/Contact.cshtml.cs` |
| `Catalog/Create.aspx` + `.cs` | `Pages/Catalog/Create.cshtml` + `.cshtml.cs` |
| `Catalog/Edit.aspx` + `.cs` | `Pages/Catalog/Edit.cshtml` + `.cshtml.cs` |
| `Catalog/Details.aspx` + `.cs` | `Pages/Catalog/Details.cshtml` + `.cshtml.cs` |
| `Catalog/Delete.aspx` + `.cs` | `Pages/Catalog/Delete.cshtml` + `.cshtml.cs` |
| `Site.Master` + `.cs` | `Pages/Shared/_Layout.cshtml` |
| `ViewSwitcher.ascx` | Removed (mobile view switching not needed in modern browsers) |
| `Site.Mobile.Master` | Removed (responsive CSS handles mobile layout) |

**Key mapping patterns applied:**
- `System.Web.UI.Page` → `Microsoft.AspNetCore.Mvc.RazorPages.PageModel`
- `Page_Load(IsPostBack=false)` → `OnGet()` handler
- Button click events (`OnClick="Save_Click"`) → `OnPost()` / `OnPostSave()` handlers
- `<asp:TextBox>`, `<asp:DropDownList>` etc. → `<input asp-for>`, `<select asp-for asp-items>` tag helpers
- `<asp:ListView>` / `<asp:Repeater>` → `@foreach` over model collection
- `Response.Redirect("~")` → `return RedirectToPage("/Index")`
- `Page.RouteData.Values["id"]` → method parameter `int id` via model binding
- `CatalogService` property injection → constructor injection

### 7. Models Updated
- `CatalogBrand.cs`, `CatalogType.cs`, `CatalogItem.cs`: Removed `System.Web` usings, added nullable annotations
- `PreconfiguredData.cs`: Removed `System.Web` usings

### 8. Static Files
Static assets moved to `wwwroot/`:
- `Content/` → `wwwroot/Content/` (CSS)
- `Scripts/` (key files: jQuery, Bootstrap) → `wwwroot/Scripts/`
- `images/` → `wwwroot/images/`
- `fonts/` → `wwwroot/fonts/`
- `Pics/` → `wwwroot/Pics/`
- `favicon.ico` → `wwwroot/favicon.ico`

### 9. App_Start Files
- `BundleConfig.cs` → Replaced with placeholder comment (bundling not needed)
- `RouteConfig.cs` → Replaced with placeholder comment (routing via Razor Pages conventions)

---

## Files Preserved (Not Compiled)

The following legacy files remain on disk but are excluded from compilation. They are kept for reference/history:
- All `.aspx`, `.ascx`, `.master` files
- All `.aspx.cs`, `.aspx.designer.cs`, `.ascx.cs`, `.ascx.designer.cs` files
- `Global.asax` and `Global.asax.cs`
- `Web.config`, `Web.Debug.config`, `Web.Release.config`
- `Bundle.config`, `ApplicationInsights.config`

---

## Next Steps

1. **ApplicationInsights**: If Application Insights telemetry is needed, install `Microsoft.ApplicationInsights.AspNetCore` and configure via `builder.Services.AddApplicationInsightsTelemetry()`.

2. **Database migrations**: The current `CatalogDBInitializer` uses `EnsureCreatedAsync()` which creates the schema from the model. For production, consider using proper EF Core migrations (`dotnet ef migrations add`).

3. **HiLo sequence scripts**: The SQL scripts in `Models/Infrastructure/*.sql` assume the SQL sequences already exist or can be created. Verify these scripts are correct for the target SQL Server version.

4. **log4net.xml**: The `log4net.xml` configuration file is read from the application root. Ensure it's present and the log4net 3.x XML schema is compatible (the existing `log4Net.xml` uses log4net 2.x schema which should still work).

5. **ViewSwitcher / Mobile Master**: The mobile view switcher (`ViewSwitcher.ascx`) and mobile master page have been removed. Modern responsive CSS (Bootstrap 4/5) handles mobile layouts without server-side view switching.

6. **HTTPS redirect**: The `app.UseHttpsRedirection()` is enabled. Ensure a valid HTTPS certificate is configured for production.

7. **Antiforgery tokens**: The Delete page uses an inline form for the delete action. The `asp-page` tag helper automatically includes antiforgery tokens.
