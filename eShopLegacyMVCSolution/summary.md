# eShopLegacyMVC — .NET Framework → net10.0 Migration Summary

## Migration Status: ✅ COMPLETE

`dotnet build eShopLegacyMVC.sln` exits with code **0**, **0 Warnings**, **0 Errors**.

---

## What Was Migrated

### Projects in Solution

| Project | Old TFM | New TFM | SDK |
|---|---|---|---|
| `src/eShopLegacyMVC` | `v4.7` (.NET Framework) | `net10.0` | `Microsoft.NET.Sdk.Web` |
| `eShopPorted` | (new project) | `net10.0` | `Microsoft.NET.Sdk.Web` |
| `eShopLegacy.Utilities` | (new project) | `net10.0` | `Microsoft.NET.Sdk` |

### Project File Migration (eShopLegacyMVC)

- Replaced legacy XML `.csproj` with SDK-style `<Project Sdk="Microsoft.NET.Sdk.Web">`.
- Set `<TargetFramework>net10.0</TargetFramework>`.
- Set `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to avoid CS0579 duplicate attribute errors from the legacy `Properties/AssemblyInfo.cs`.
- Removed all legacy `packages.config` / `<Reference><HintPath>` blocks.
- Excluded legacy infrastructure files not compatible with ASP.NET Core (`Global.asax.cs`, `App_Start/*.cs`, `Views/Web.config`).

### Package Migrations

| Legacy Package | Modern Equivalent |
|---|---|
| `System.Web.Mvc` | `Microsoft.AspNetCore.Mvc` (via SDK) |
| `Microsoft.AspNet.WebApi.*` | `Microsoft.AspNetCore.Mvc` (via SDK) |
| `EntityFramework` (EF6) | `Microsoft.EntityFrameworkCore` 10.0.0 + `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0 |
| `Autofac.Mvc5` | `Autofac` 8.3.0 + `Autofac.Extensions.DependencyInjection` 10.0.0 |
| `Newtonsoft.Json` | `Newtonsoft.Json` 13.0.3 (kept for compatibility) |
| `log4net` 2.0.17 (CVE) | `log4net` 3.3.2 (vulnerability resolved) |

### Code Migrations

#### Global.asax → Program.cs
`Global.asax` / `Global.asax.cs` replaced by `src/eShopLegacyMVC/Program.cs`:
- ASP.NET Core `WebApplication.CreateBuilder` host.
- Autofac registered via `AutofacServiceProviderFactory`.
- EF Core `CatalogDBContext` registered via `AddDbContext`.
- Session and `IHttpContextAccessor` registered.
- `MapControllerRoute` for default and `GetPicRouteTemplate` routes.
- Database initialisation at startup using `CatalogDBInitializer`.

#### App_Start/* → Inline in Program.cs
- `BundleConfig.cs` — removed; static files served directly via `UseStaticFiles()`.
- `RouteConfig.cs` — replaced by `MapControllerRoute` in `Program.cs`.
- `WebApiConfig.cs` — replaced; API controllers use `[ApiController]` + `[Route]`.
- `FilterConfig.cs` — removed; global filters not required.

#### Web.config → appsettings.json
Connection strings and feature flags moved to `appsettings.json`:
```json
{
  "ConnectionStrings": { "CatalogDBContext": "..." },
  "UseMockData": "true",
  "UseCustomizationData": "false"
}
```

#### System.Web / ASP.NET MVC 5 → ASP.NET Core
- All `using System.Web.*` removed.
- `Controller` base class is now `Microsoft.AspNetCore.Mvc.Controller`.
- `ApiController` base class replaced by `ControllerBase` with `[ApiController]`.
- `HttpNotFound()` → `NotFound()`, `HttpBadRequest()` → `BadRequest()`.
- `HttpContext.Current` → injected `IHttpContextAccessor`.
- `IWebHostEnvironment` injected for content-root and environment checks.

#### Entity Framework 6 → EF Core 10
- `using System.Data.Entity` → `using Microsoft.EntityFrameworkCore`.
- `DbContext` constructors updated to accept `DbContextOptions<T>`.
- `OnModelCreating` uses `ModelBuilder` / `EntityTypeBuilder<T>` Fluent API.
- `[Index]` attribute (EF6) removed; replaced by Fluent API calls where needed.
- `Database.SqlQueryRaw<long>` used for HiLo sequence queries (EF Core raw SQL).
- `Database.ExecuteSqlRaw` used for DDL script execution.
- SQL injection advisory (EF1002) on constant-string HiLo query suppressed with `#pragma warning disable EF1002`.

#### Dependency Injection
- Autofac `ApplicationModule` (`Autofac.Module`) retained — registers `CatalogService`, `CatalogServiceMock`, `CatalogItemHiLoGenerator`, `CatalogDBInitializer`.
- `AutofacServiceProviderFactory` wires Autofac into the ASP.NET Core DI pipeline.

#### Views
- `_ViewImports.cshtml` includes `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers`.
- `@Html.Partial(...)` replaced with `<partial name="..." model="..." />` tag helper to resolve MVC1000 deadlock advisory.
- `@Scripts.Render` / `@Styles.Render` (BundleConfig) removed; static CDN links used in `_Layout.cshtml`.

#### Utilities Library (eShopLegacy.Utilities)
- `Serializing.cs` updated: replaced `BinaryFormatter` (removed in .NET 5+) with `System.Text.Json.JsonSerializer`.

#### WebApi Controllers
- `FilesController` and `BrandsController` migrated to `ControllerBase` with `[ApiController]` and attribute routing.
- `BrandDTO` serialization uses `System.Text.Json` (via stream-based `JsonSerializer.Serialize`).

---

## Final Build Output

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Next Steps / Notes

- **log4net XML config** (`log4Net.xml`) is loaded at startup via `XmlConfigurator.Configure`; ensure the file is present in the output directory when deploying (it is referenced relative to the working directory).
- **SQL Server dependency**: `CatalogDBInitializer` calls `Database.MigrateAsync()` and executes HiLo sequence SQL on startup when `UseMockData=false`. A SQL Server instance with the correct connection string is required for non-mock mode.
- **EF Core Migrations**: No EF Core migrations exist in `src/eShopLegacyMVC`. Running with `UseMockData=false` in a fresh environment will require `dotnet ef migrations add InitialCreate` and `dotnet ef database update` before first start.
- **Pics folder**: `PicController` serves images from `{ContentRootPath}/Pics`. In production / Docker deployments ensure this folder is mounted or pre-populated.
- **`[Serializable]` on `BrandDTO`**: Retained for compatibility; no longer required for `System.Text.Json` serialization and can be removed in a follow-up cleanup.
- **Newtonsoft.Json**: Still referenced in both `eShopLegacyMVC` and `eShopPorted`. Migrating to `System.Text.Json` throughout is a future hardening task.
