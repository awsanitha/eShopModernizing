# .NET Framework → .NET 10 Migration Summary

## Build Status: ALL PROJECTS PASSING (0 errors)

| Project | Target | Status |
|---------|--------|--------|
| eShopModernizedMVC | net10.0 | ✅ Build succeeded, 0 errors |
| eShopLegacyWebForms | net10.0 | ✅ Build succeeded, 0 errors |
| eShopLegacyMVC | net10.0 | ✅ Build succeeded, 0 errors |
| eShopLegacy.Utilities | net10.0 | ✅ Build succeeded, 0 errors |
| eShopPorted | net10.0 | ✅ Build succeeded, 0 errors |
| eShopModernizedNTier/eShopWCFService | net10.0 | ✅ Build succeeded, 0 errors |
| eShopModernizedNTier/eShopWinForms | net10.0-windows | ✅ Build succeeded, 0 errors |
| eShopModernizedNTier/eShopWinForms.fx | net10.0-windows | ✅ Build succeeded, 0 errors |
| eShopModernizedWebForms | net10.0 | ✅ Build succeeded, 0 errors |
| eShopLegacyNTier/eShopWCFService | net10.0 | ✅ Build succeeded, 0 errors |
| eShopLegacyNTier/eShopWinForms | net10.0-windows | ✅ Build succeeded, 0 errors |

## Key Transformations

### eShopLegacy.Utilities
- Upgraded from netstandard2.0 → net10.0
- Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>` to resolve duplicate attribute errors
- Replaced `BinaryFormatter` (obsolete SYSLIB0011) with `System.Text.Json` serialization

### eShopPorted
- Replaced incompatible packages (Autofac.Mvc5, WebGrease, Antlr4)
- Updated to modern `WebApplication.CreateBuilder` pattern (Program.cs)
- Removed old Startup.cs (OWIN-style)
- Fixed PicController: `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`
- Fixed Views/_Layout.cshtml: removed reference to deleted Startup class

### eShopModernizedMVC & eShopLegacyMVC
- Replaced old-style .csproj with SDK-style targeting net10.0
- Deleted Global.asax / BundleConfig / FilterConfig / RouteConfig / Startup.cs (OWIN)
- Created Program.cs with WebApplication.CreateBuilder and Autofac integration
- All controllers: `System.Web.Mvc` → `Microsoft.AspNetCore.Mvc`
- CatalogDBContext: EF6 → EF Core 10 (DbContextOptions, ModelBuilder, ValueGeneratedNever, HasOne)
- ImageAzureStorage: `WindowsAzure.Storage` → `Azure.Storage.Blobs`
- IImageService: `HttpPostedFile` → `IFormFile`
- AuthenticationMiddleware: OWIN → ASP.NET Core middleware
- WebApi controllers: `System.Web.Http.ApiController` → `[ApiController] ControllerBase`
- Added Views/_ViewImports.cshtml with tag helpers
- Updated all Razor views (removed @Scripts.Render/@Styles.Render)
- Created appsettings.json replacing Web.config

### eShopLegacyWebForms & eShopModernizedWebForms (WebForms → ASP.NET Core MVC)
- **Full rewrite**: All .aspx/.ascx/.master files deleted, replaced with Razor MVC views
- Created Program.cs, Controllers/CatalogController.cs, full Razor view set
- Same EF6→EF Core, System.Web→ASP.NET Core transformations as MVC projects
- Preserved all CRUD functionality, pagination, and service layer
- eShopModernizedWebForms additionally: Azure Blob storage, Application Insights, Key Vault config

### eShopModernizedNTier/eShopWCFService & eShopLegacyNTier/eShopWCFService
- Replaced old-style .csproj with SDK-style `Microsoft.NET.Sdk.Web` targeting net10.0
- Service contract: `using System.ServiceModel` → `using CoreWCF`
- Created Program.cs with CoreWCF hosting (`AddServiceModelServices`, `AddServiceEndpoint`)
- Deleted .svc files (routing via Program.cs)
- EF6 → EF Core 10 for EntityModel/DbContext
- Removed `<system.serviceModel>` web.config sections

### WinForms Projects (eShopModernizedNTier, eShopLegacyNTier)
- Already SDK-style net10.0-windows from previous cycle
- Added `<EnableWindowsTargeting>true</EnableWindowsTargeting>` for Linux build compatibility
- WCF client: `System.ServiceModel.Http` and `System.ServiceModel.Primitives` packages added
- eShopLegacyNTier WinForms: excluded UWP helper files, removed incompatible string-based ClientBase constructors

## Remaining Warnings (non-blocking)
- NU1902: log4net 2.0.17 has a known moderate severity vulnerability (advisory only)
- CS8632: Nullable annotation context warnings in some files
- CS0114: Method hiding warnings (virtual/override mismatches)
- NU1510: Microsoft.CSharp auto-included package reference (redundant but harmless)

## Next Steps
- Consider upgrading log4net to a patched version or switch to Microsoft.Extensions.Logging
- Add `<Nullable>enable</Nullable>` to project files and fix nullable warnings
- Static files (Content/, Scripts/, Images/, Pics/) should be moved to wwwroot/ for proper serving in production; current setup works for build but may need UseStaticFiles path configuration at runtime
- EF Core migrations: run `dotnet ef migrations add InitialCreate` for each web project before first deployment
- Azure Key Vault configuration (SqlAccessTokenProvider, OptionalKeyVaultConfigurationBuilder) needs Azure credentials configured at runtime
- Application Insights instrumentation keys need to be set in appsettings.json or environment variables
