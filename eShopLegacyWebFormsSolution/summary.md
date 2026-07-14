# Migration Summary: eShopLegacyWebForms → net10.0

## Status
✅ **Build succeeded — 0 errors, 0 warnings**

---

## What Was Done

### 1. Project File Conversion
- Replaced legacy non-SDK `.csproj` (ToolsVersion=12.0) with `Microsoft.NET.Sdk.Web` SDK-style project
- Target framework: `net10.0`
- Enabled `<Nullable>enable</Nullable>` and `<ImplicitUsings>enable</ImplicitUsings>`
- Set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to prevent duplicate attribute conflicts
- Replaced `packages.config` + HintPath references with modern `<PackageReference>` elements

### 2. Packages Replaced
| Old Package | New Package |
|---|---|
| EntityFramework 6.2.0 | Microsoft.EntityFrameworkCore.SqlServer 9.0.6 |
| Autofac.Integration.Web | Autofac.Extensions.DependencyInjection 10.0.0 |
| Autofac 4.9.1 | Autofac 8.3.0 |
| All System.Web.* packages | Built into ASP.NET Core SDK |
| Microsoft.AspNet.Web.Optimization | Removed (static files middleware) |
| log4net | Microsoft.Extensions.Logging (built-in) |
| All ApplicationInsights packages | Removed |

### 3. WebForms → Razor Pages Migration
WebForms does not run on .NET 5+. All `.aspx` pages were rewritten as Razor Pages:

| Old WebForms | New Razor Page |
|---|---|
| `Default.aspx` + codebehind | `Pages/Index.cshtml` + `Index.cshtml.cs` |
| `Catalog/Create.aspx` + codebehind | `Pages/Catalog/Create.cshtml` + `.cs` |
| `Catalog/Edit.aspx` + codebehind | `Pages/Catalog/Edit.cshtml` + `.cs` |
| `Catalog/Details.aspx` + codebehind | `Pages/Catalog/Details.cshtml` + `.cs` |
| `Catalog/Delete.aspx` + codebehind | `Pages/Catalog/Delete.cshtml` + `.cs` |
| `About.aspx` | `Pages/About.cshtml` + `.cs` |
| `Site.Master` | `Pages/Shared/_Layout.cshtml` |

### 4. Global.asax → Program.cs
- `Application_Start` logic moved to `Program.cs`
- `RouteConfig` and `BundleConfig` replaced by ASP.NET Core routing + static files middleware
- `ConfigureContainer()` (Autofac WebForms DI) replaced by `builder.Services` built-in DI
- `Session_Start` logic moved to ASP.NET Core session middleware
- Database seeding moved to startup initialization block in `Program.cs`

### 5. Web.config → appsettings.json
- Connection strings migrated to `appsettings.json`
- AppSettings (`UseMockData`, `UseCustomizationData`) migrated to `appsettings.json`
- Environment-specific overrides in `appsettings.Development.json`

### 6. Entity Framework 6 → EF Core 9
- `CatalogDBContext` converted from EF6 (`System.Data.Entity.DbContext`) to EF Core (`Microsoft.EntityFrameworkCore.DbContext`)
- Constructor changed from `base("name=CatalogDBContext")` to `base(options)` with `DbContextOptions<T>` injection
- `EntityTypeConfiguration<T>` replaced with `EntityTypeBuilder<T>` in `OnModelCreating`
- `HasRequired<T>()` replaced with `HasOne()`
- `HasDatabaseGeneratedOption(None)` replaced with `ValueGeneratedNever()`
- EF6 `CreateDatabaseIfNotExists<T>` initializer rewritten as a `CatalogDBInitializer` service called at startup
- `Database.SqlQuery<T>()` replaced with raw ADO.NET `DbConnection` calls for sequence queries
- `Database.ExecuteSqlCommand()` replaced with `Database.ExecuteSqlRaw()`

### 7. Dependency Injection
- `Autofac.Integration.Web` (WebForms property injection) removed
- Replaced with ASP.NET Core built-in DI in `Program.cs`
- `ICatalogService` registered as Singleton (mock) or Scoped (real) based on `UseMockData` config
- Constructor injection used in all Razor Page models

### 8. Static Files
- Static assets (CSS, JS, images, fonts, Pics) copied to `wwwroot/` for `UseStaticFiles()` middleware

### 9. Files Excluded/Replaced
All legacy files excluded via `<Compile Remove>` in csproj:
- All `.aspx.cs`, `.aspx.designer.cs` files
- `Site.Master.cs`, `Site.Mobile.Master.cs`
- `ViewSwitcher.ascx.cs`
- `Global.asax.cs`
- `App_Start/BundleConfig.cs`, `App_Start/RouteConfig.cs`
- `Modules/ApplicationModule.cs`
- `Properties/AssemblyInfo.cs`

---

## Next Steps

1. **SQL Server connection** – The default connection string targets `(localdb)\MSSQLLocalDB`. Update for production environments via environment variables or a secrets manager.
2. **EF Core Migrations** – No migrations have been generated. If upgrading from an existing database, run `dotnet ef migrations add InitialCreate` and compare against the existing schema.
3. **Session middleware** – Currently uses in-memory distributed cache. For production/multi-instance deployments, replace with Redis or SQL Server session provider.
4. **HiLo sequences** – The `CatalogItemHiLoGenerator` requires `catalog_hilo`, `catalog_brand_hilo`, and `catalog_type_hilo` SQL Server sequences to exist. These are created by the SQL scripts in `Models/Infrastructure/` during seeding.
5. **Static assets** – Assets were copied to `wwwroot/`. Consider using a proper asset pipeline (Vite, webpack) for production builds.
6. **Autofac** – The Autofac packages were added but the built-in DI is used instead. If complex DI features are needed, wire in `AutofacServiceProviderFactory` in `Program.cs`.
7. **log4net** – Replaced by `Microsoft.Extensions.Logging`. If log4net-specific sinks are required, add `log4net` + `Microsoft.Extensions.Logging.Log4Net.AspNetCore` packages.
