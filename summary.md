# .NET Framework → .NET 10 Migration Summary

## Migration Status: COMPLETE ✅

All 11 targeted projects have been successfully migrated from .NET Framework 4.x to .NET 10.0.
**Build result: 0 errors, 0 warnings** across all projects.

---

## Projects Migrated

| Project | From | To | Status |
|---------|------|----|--------|
| eShopModernizedMVC | .NET Framework 4.7.2 (old-style csproj, MVC 5 + EF6) | net10.0 (SDK-style, ASP.NET Core MVC, EF Core 10) | ✅ |
| eShopLegacyWebForms | .NET Framework 4.7.2 (old-style csproj, Web Forms + EF6) | net10.0 (SDK-style, ASP.NET Core MVC, EF Core 10) | ✅ |
| eShopLegacyMVC | .NET Framework 4.7.2 (old-style csproj, MVC 5 + EF6) | net10.0 (SDK-style, ASP.NET Core MVC, EF Core 10) | ✅ |
| eShopLegacy.Utilities | netstandard2.0 (BinaryFormatter, CS0579) | netstandard2.0 (DataContractSerializer, fixes) | ✅ |
| eShopPorted | net10.0 (old patterns, System.Web) | net10.0 (modern patterns, null-safe) | ✅ |
| eShopModernizedNTier/eShopWCFService | .NET Framework 4.6.1 (old-style csproj, WCF + EF6) | net10.0 (SDK-style, CoreWCF 1.9.1, EF Core 10) | ✅ |
| eShopModernizedNTier/eShopWinForms | net10.0-windows (CoreWCF client issue) | net10.0-windows (System.ServiceModel client) | ✅ |
| eShopModernizedNTier/eShopWinForms.fx | net10.0-windows (CoreWCF client issue) | net10.0-windows (System.ServiceModel client) | ✅ |
| eShopModernizedWebForms | .NET Framework 4.7.2 (old-style csproj, Web Forms + EF6) | net10.0 (SDK-style, ASP.NET Core MVC, EF Core 10) | ✅ |
| eShopLegacyNTier/eShopWCFService | .NET Framework 4.6.1 (old-style csproj, WCF + EF6) | net10.0 (SDK-style, CoreWCF 1.9.1, EF Core 10) | ✅ |
| eShopLegacyNTier/eShopWinForms | net10.0-windows (CoreWCF client issue) | net10.0-windows (System.ServiceModel client) | ✅ |

---

## Key Transformations Applied

### Project File Migration
- All old-style (non-SDK) `.csproj` files converted to `Microsoft.NET.Sdk.Web` or `Microsoft.NET.Sdk` SDK-style format
- `packages.config` approach replaced with `<PackageReference>` inline in `.csproj`
- All legacy framework hints (`HintPath`, `ToolsVersion`, GAC references) removed

### Package Migrations
| Old Package | New Package | Notes |
|---|---|---|
| `EntityFramework` 6.x | `Microsoft.EntityFrameworkCore` 10.0.11 | EF6 → EF Core 10 |
| `System.Web.Mvc` (MVC5) | `Microsoft.AspNetCore.Mvc` (built-in) | ASP.NET Core MVC |
| `Microsoft.Owin.*` | `Microsoft.AspNetCore.Authentication.*` | ASP.NET Core auth |
| `System.Web.Http.*` | `Microsoft.AspNetCore.Mvc` | Unified MVC+API |
| `Autofac.Mvc5` | `Autofac.Extensions.DependencyInjection` | ASP.NET Core DI integration |
| `Microsoft.WindowsAzure.Storage` | `Azure.Storage.Blobs` 12.23.0 | Modern Azure SDK |
| `Microsoft.Azure.Services.AppAuthentication` | `Azure.Identity` 1.17.1 | Modern Azure Identity |
| `System.ServiceModel.*` (WCF server) | `CoreWCF.*` 1.9.1 | CoreWCF for server hosting |
| `System.ServiceModel.*` (WCF client) | `System.ServiceModel.Http` 10.0.652802 | Modern WCF client |
| `log4net` 2.0.17 | `log4net` 3.3.2 | Fixed vulnerability CVE |
| `Newtonsoft.Json` 12.x | `Newtonsoft.Json` 13.0.4 | Fixed vulnerability |
| `System.Security.Cryptography.Xml` 10.0.0 | 10.0.11 | Fixed NU1903 vulnerability |
| `BinaryFormatter` | `DataContractSerializer` (XML binary) | Removed removed/obsolete API |

### Code Migrations

#### System.Web → ASP.NET Core
- `System.Web.Mvc.Controller` → `Microsoft.AspNetCore.Mvc.Controller`
- `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
- `HttpNotFound()` → `NotFound()`
- `ActionResult` → `IActionResult`
- `[Bind(Include = "...")]` → `[Bind("...")]`
- `Request.IsAuthenticated` → `User.Identity?.IsAuthenticated`
- `HttpContext.Current.Session[...]` → removed (replaced with static footer text)
- `HttpPostedFile` → `IFormFile`
- `Server.MapPath("~/Pics")` → `IWebHostEnvironment.ContentRootPath`
- `HostingEnvironment.ApplicationPhysicalPath` → `IWebHostEnvironment.ContentRootPath`

#### EF6 → EF Core
- `DbContext(string)` constructor → `DbContext(DbContextOptions<T>)`
- `DbModelBuilder` → `ModelBuilder`
- `EntityTypeConfiguration<T>` (fluent) → `ModelBuilder.Entity<T>().Configure()`
- `DatabaseGeneratedOption.None` → `.ValueGeneratedNever()`
- `HasRequired<T>` → `HasOne<T>` + `.IsRequired()`
- `CreateDatabaseIfNotExists<T>` → `EnsureCreated()` + explicit seeding
- `Database.SetInitializer` → service startup seeding
- `Database.SqlQuery<T>` → `Database.SqlQueryRaw<T>` (EF Core 7+)
- `Database.ExecuteSqlCommand` → `Database.ExecuteSqlRaw`
- `EntityState.Modified` namespace → `Microsoft.EntityFrameworkCore`

#### WCF → CoreWCF (Server)
- `[ServiceContract]`, `[OperationContract]` → `CoreWCF.*` attributes
- Old `.svc` file hosting → CoreWCF `Program.cs` with `UseServiceModel()`
- `Database.SetInitializer` pattern → EF Core seeding on startup
- Removed `CatalogServiceClient.cs` from server projects (used CoreWCF; clients use System.ServiceModel)

#### WCF Client (WinForms)
- Replaced `CoreWCF.*` packages with `System.ServiceModel.Http` (client-side)
- Updated auto-generated `Reference.cs` constructors to remove string endpoint config overloads (not supported in .NET Core)
- Added `new` keyword to `CloseAsync()` override
- Fixed default constructor to use explicit `BasicHttpBinding` + `EndpointAddress`

#### Web Forms → ASP.NET Core MVC
- All `.aspx` / `.aspx.cs` / `.ascx` / designer files excluded from compilation
- New `CatalogController.cs` created for each Web Forms project replacing page code-behind logic
- Razor `.cshtml` views created from scratch (adapted from existing MVC projects)
- `@Styles.Render()` / `@Scripts.Render()` → direct `<link>` and `<script>` tags
- `@Html.Partial()` → `<partial>` tag helper (MVC1000 fix)

#### Global.asax + Web.config → Program.cs + appsettings.json
- `HttpApplication.Application_Start` → `WebApplication.CreateBuilder()` + service registration
- `ConfigurationManager.AppSettings` → `IConfiguration`
- Autofac `ContainerBuilder.Populate(services)` → `AutofacServiceProviderFactory`
- Static file serving for legacy `Content/`, `Scripts/`, `Images/` directories configured via `PhysicalFileProvider`

#### OWIN → ASP.NET Core Auth
- `OwinMiddleware` → `IMiddleware`
- `OpenIdConnectAuthenticationOptions` → `OpenIdConnectOptions`
- `CookieAuthenticationOptions` → `CookieAuthenticationDefaults`
- `HttpContext.GetOwinContext().Authentication` → `HttpContext.SignInAsync/SignOutAsync/Challenge`

#### Azure Storage SDK Migration
- `CloudStorageAccount.Parse()` → `BlobServiceClient(connectionString)`
- `CloudBlobClient` → `BlobServiceClient`
- `CloudBlobContainer` → `BlobContainerClient`
- `CloudBlockBlob` → `BlobClient`
- `container.CreateIfNotExists()` → `containerClient.CreateIfNotExists(PublicAccessType.Blob)`
- `blockBlob.UploadFromStream()` → `blobClient.Upload(stream, BlobUploadOptions)`
- `blockBlob.StartCopy()` → `blobClient.StartCopyFromUri()`

#### Azure Identity Migration
- `AzureServiceTokenProvider` → `DefaultAzureCredential`
- `GetAccessTokenAsync("https://database.windows.net/")` → `GetToken(TokenRequestContext(...))`

#### Nullable Reference Types
- Added `= string.Empty` defaults to string properties
- Added `= null!` defaults to non-nullable navigation properties
- Added `!` null-forgiving operator to EF Core `FirstOrDefault()` returns
- Changed `GetCurrentMethod().DeclaringType` → `typeof(ClassName)` for log4net logger initialization
- Changed `Description`, `PictureUri` etc. to `string?` where appropriate

#### Log4net Usage
- Updated from 2.x to 3.3.2 (fixed CVE vulnerability)
- Logger initialization changed from reflection to `typeof()` pattern
- `LogicalThreadContext.Properties` still works in log4net 3.x

---

## Architecture Notes

### Web Forms Projects (eShopLegacyWebForms, eShopModernizedWebForms)
Web Forms does not run on .NET 10. These projects were migrated to **ASP.NET Core MVC** with:
- New MVC `CatalogController` replacing Web Forms page logic
- Razor `.cshtml` views adapted from the existing MVC projects
- All Web Forms-specific code (`.aspx`, `.ascx`, `.designer.cs`) excluded from compilation
- The original Web Forms files are preserved on disk but excluded from the build

### WinForms Projects
- Remain as `net10.0-windows` targeting
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` for cross-platform builds
- Changed from CoreWCF (server packages) to `System.ServiceModel.Http` (client packages)

### BinaryFormatter Replacement
`BinaryFormatter` was removed in .NET 8. Replaced with `DataContractSerializer` (XML binary encoding) which preserves the binary stream API contract. Files serialized by the old implementation will not be compatible with the new one, but the interface is the same.

---

## Next Steps (recorded from migration cycle)

1. **Managed Identity SQL Connection**: The `ManagedIdentitySqlConnectionFactory` uses `Azure.Identity.DefaultAzureCredential`. This requires proper Azure AD setup and managed identity assignment in the target environment.

2. **Web.config → appsettings.json for WinForms**: The WinForms app.config's `<system.serviceModel>` section is ignored. The WCF client endpoint URL is now hardcoded to `http://localhost:62314/CatalogService.svc`. This should be made configurable via app.config or environment variable.

3. **EF Core Migrations**: The WCF service and MVC projects use `Database.EnsureCreated()` on startup. For production, EF Core migrations should be created:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Static Files in legacy MVC projects**: The CSS, JS, and image files are served from the content root (not `wwwroot`). Consider consolidating them into a `wwwroot` directory to follow ASP.NET Core conventions.

5. **Application Insights**: The `Microsoft.ApplicationInsights.AspNetCore` package is included but App Insights is only activated when `AppInsightsInstrumentationKey` is configured in appsettings.json.

6. **Session Support**: The legacy MVC projects used `Session["MachineName"]` and `Session["SessionStartTime"]` in the footer. Session support was removed from the migrated `_Layout.cshtml`. To re-enable, add `builder.Services.AddSession()` and `app.UseSession()` in `Program.cs`.

7. **HSTS / HTTPS Redirect**: Not configured in this migration. Add `app.UseHttpsRedirection()` and `app.UseHsts()` for production.

8. **Unit Tests**: No test projects were found or migrated. After validating the migration manually, unit tests should be added.
